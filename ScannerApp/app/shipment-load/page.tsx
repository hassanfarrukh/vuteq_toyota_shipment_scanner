/**
 * Shipment Load V2 Page - 4-Screen Workflow
 * Author: Hassan
 * Date: 2025-11-05
 * Updated: 2025-11-05 - Added Screen 4 (Success/Confirmation)
 * Updated: 2025-11-05 - Added Exception Modal, FAB Menu, Submit Validation
 * Updated: 2025-11-05 - Added Rack Exception Toggle (FAB Menu)
 * Updated: 2025-11-05 - Simplified Screen 4 to match TSCS popup-style confirmation
 * Updated: 2025-11-05 - Changed Screen 4 button colors and added Back to Dashboard
 * Updated: 2025-12-08 - CRITICAL: Fixed Toyota Manifest parsing to use INDIVIDUAL fields
 * Updated: 2025-12-08 - Integrated Shipment Load APIs (getShipmentLoadRoute, scanShipmentLoadSkid, completeShipmentLoad)
 * Updated: 2025-12-08 - Changed from mock data to real API calls with JWT authentication
 * Updated: 2025-12-17 - Integrated validate-order API on QR scan to get actual skid count
 * Updated: 2025-12-17 - FIXED: Replaced hardcoded 'user-001' with actual authenticated user from AuthContext
 * Updated: 2025-12-17 - SCREEN 2 FIELDS UPDATED: Replaced carrierName/driverName/notes with separate first/last names for driver and supplier
 * Updated: 2025-12-17 - SCREEN 3 SKIDS: Integrated getOrderSkids API to show actual built skids from tblSkidScans (001A, 001B, etc.)
 * Updated: 2025-12-17 - FIXED: Changed grouping to match Toyota API - SkidNumber + PalletizationCode (not SkidSide)
 * Updated: 2025-12-17 - CRITICAL FIX: Added sessionId to scanShipmentLoadSkid API call to resolve "session not found" error
 * Updated: 2025-12-20 - SCREEN 3 UI REDESIGN: Grouped skids BY ORDER with nested collapsibles (matches TSCS Mobile workflow)
 * Updated: 2025-12-20 - Added "Planned Orders" section with order-level grouping and skid build status validation
 * Updated: 2025-12-20 - Added "Scanned Orders" section mirroring planned structure with order grouping
 * Updated: 2025-12-22 - CRITICAL FIX: Auto-populate trailer data when session is resumed (isResumed=true)
 * Updated: 2025-12-22 - CRITICAL FIX: Restore scanned items when session is resumed (check order.isScanned flag)
 * Updated: 2026-01-04 - BUG FIX: Block already-shipped orders on Screen 1 with Toyota confirmation number
 *
 * SCREEN FLOW:
 * 1. Scan Pickup Route QR → Parse and store → Continue to Screen 2
 * 2. Trailer Information Form → Fill details → Continue to Screen 3
 * 3. Skid Manifest Scanning (PLANNED/SCANNED split) → Scan moves items from planned to scanned → Submit
 * 4. Success/Confirmation → Navy CONTINUE button + Back to Dashboard button
 *
 * FEATURES:
 * - Full API integration with backend (/api/v1/shipment-load)
 * - JWT authentication via apiClient
 * - Fetches planned orders from API based on route
 * - Toyota Manifest QR scanning (44-char format)
 * - Parses: Plant, Supplier, Dock, Order, Load ID, Palletization (INDIVIDUAL), MROS (INDIVIDUAL), Skid ID (INDIVIDUAL)
 * - Individual fields sent to API (NOT combined string)
 * - Collapsible sections (Order Details, Planned Skids, Scanned Skids, Exceptions)
 * - QR scanning with auto-parsing
 * - Planned → Scanned workflow (items move upon scan)
 * - Exception handling for unplanned items
 * - FAB Menu: Save Draft, Unpick All, Rack Exception Toggle (Screen 3 only)
 * - Draft session recovery from localStorage
 * - Submit validation (all items loaded or exceptions documented)
 * - Confirmation number generation (format: SL{timestamp}{random})
 * - Screen 4: Minimal popup-style success confirmation (matches TSCS pattern)
 * - Theme colors: Navy #253262, Red #D2312E, Off-white #FCFCFC
 * - Font Awesome icons only (no Lucide)
 */

'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import Scanner from '@/components/ui/Scanner';
import Button from '@/components/ui/Button';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Alert from '@/components/ui/Alert';
import Badge from '@/components/ui/Badge';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import type { ScanResult } from '@/types';
import {
  getShipmentLoadRoute,
  scanShipmentLoadSkid,
  completeShipmentLoadSession,
  startShipmentLoadSession,
  updateShipmentLoadSession,
  validateShipmentLoadOrder,
  getOrderSkids,
  restartShipmentLoadSession,
  type PlannedOrder as ApiPlannedOrder,
  type StartSessionResponse,
  type ValidateOrderResponse,
  type OrderSkidsResponse,
  type SkidDto,
} from '@/lib/api';

// Screen types
type Screen = 1 | 2 | 3 | 4;

// Toyota-specific exception types for Shipment Load
// Author: Hassan, 2025-12-22
// F-GAP-002 FIX: Replaced Skid Build exception types with correct Shipment Load types
// Reference: Toyota Spec Page 24 (Trailer Level) and Page 26 (Skid Level)

// TRAILER LEVEL EXCEPTIONS (Toyota Spec Page 24)
const TRAILER_EXCEPTION_TYPES = [
  { label: 'Blowout - Space / Weight', code: '13' },
  { label: 'Freight Pulled Ahead Already', code: '16' },
  { label: 'Freight Damage', code: '17' },
  { label: 'Supplier Revised Shortage (Short Shipment)', code: '24' },
  { label: 'Toyota Instructed Delay', code: '25' },
  { label: 'Others', code: '27' },
  { label: 'Unplanned Expedite', code: '99' }, // Special rules apply!
];

// SKID LEVEL EXCEPTIONS (Toyota Spec Page 26)
const SKID_EXCEPTION_TYPES = [
  { label: 'Blowout Recovery - Normal Route', code: '14' },
  { label: 'Buildout Recovery - Normal Route', code: '15' },
  { label: 'Expedite – Supplement', code: '18' },
  { label: 'Expedite – Blowout', code: '19' },
  { label: 'Continuous Load', code: '21' },
  { label: 'Expedite - Buildout', code: '22' },
  { label: 'Freight Pull Ahead (At Pallet Level)', code: '23' },
];

// Data interfaces
interface PickupRouteData {
  orderNumber: string;
  routeNumber: string;
  plant: string;
  supplierCode: string;
  dockCode: string;
  pickupDateTime: string; // ISO 8601 format: YYYY-MM-DDTHH:MM:SS (for API use)
  pickupDate: string;     // YYYY-MM-DD format
  pickupTime: string;     // HH:MM format
  estimatedSkids: number;
  rawQRValue: string;
}

interface TrailerData {
  trailerNumber: string;
  driverFirstName: string;
  driverLastName: string;
  sealNumber: string;
  supplierFirstName: string;
  supplierLastName: string;
}

interface PlannedSkid {
  skidId: string;           // "001A"
  skidNumber: string;       // "001"
  skidSide: string | null;  // "A" or "B"
  palletizationCode: string | null;
  scannedAt: string | null;
  orderNumber: string;
  dockCode: string;
  isScanned: boolean;
}

interface ScannedSkid {
  id: string;
  skidId: string;           // "001", "002", etc.
  manifestNo: number;
  palletizationCode: string;
  mros: string;
  orderNumber: string;
  dockCode: string;
  timestamp: string;
}

interface Exception {
  type: string;
  code: string; // Exception code (13-27, 99 for trailer; 14-23 for skid)
  comments: string;
  relatedSkidId: string | null; // null for trailer-level, SkidNumber-PalletizationCode for skid-level
  timestamp: string;
  level: 'trailer' | 'skid'; // F-GAP-002: Differentiate trailer vs skid level exceptions
}

export default function ShipmentLoadV2Page() {
  const router = useRouter();
  const { user } = useAuth();

  // Screen state
  const [currentScreen, setCurrentScreen] = useState<Screen>(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Screen 1: Pickup Route Data
  const [pickupRouteData, setPickupRouteData] = useState<PickupRouteData | null>(null);

  // Session data from API
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [actualSkidCount, setActualSkidCount] = useState<number>(0);
  const [orderValidationData, setOrderValidationData] = useState<{
    toyotaConfirmationNumber?: string;
    toyotaShipmentConfirmationNumber?: string;
    status?: string;
  } | null>(null);

  // Screen 2: Trailer Information
  const [trailerData, setTrailerData] = useState<TrailerData>({
    trailerNumber: '',
    driverFirstName: '',
    driverLastName: '',
    sealNumber: '',
    supplierFirstName: '',
    supplierLastName: '',
  });

  // Screen 3: Planned and Scanned Skids
  const [plannedSkids, setPlannedSkids] = useState<PlannedSkid[]>([]);
  const [scannedSkids, setScannedSkids] = useState<ScannedSkid[]>([]);
  const [expandedSection, setExpandedSection] = useState<string | null>(null);
  const [expandedOrders, setExpandedOrders] = useState<Set<string>>(new Set()); // Track expanded orders separately

  // Exceptions Data (Screen 3)
  // Author: Hassan, 2025-11-05
  // Updated: 2025-12-22 - F-GAP-002: Added exception level state
  const [exceptions, setExceptions] = useState<Exception[]>([]);
  const [showExceptionModal, setShowExceptionModal] = useState(false);
  const [selectedExceptionType, setSelectedExceptionType] = useState('');
  const [selectedExceptionCode, setSelectedExceptionCode] = useState('');
  const [exceptionComments, setExceptionComments] = useState('');
  const [selectedSkidForException, setSelectedSkidForException] = useState('');
  const [exceptionLevel, setExceptionLevel] = useState<'trailer' | 'skid'>('trailer'); // F-GAP-002: Track exception level

  // FAB Menu State
  // Author: Hassan, 2025-11-05
  const [fabMenuOpen, setFabMenuOpen] = useState(false);
  const [showSuccessAlert, setShowSuccessAlert] = useState(false);

  // Rack Exception Toggle (FAB Menu - Screen 3 only)
  // Author: Hassan, 2025-11-05
  const [rackExceptionEnabled, setRackExceptionEnabled] = useState(false);

  // Confirmation number - generated at final submit
  const [confirmationNumber, setConfirmationNumber] = useState<string>('');

  /**
   * Parse Pickup Route QR Code
   * Fixed-Position Format (54-byte Driver Checksheet - Toyota spec):
   * Example: "26MTMFB05474   2025121134     JAAJ17Load20251211181000"
   *
   * Position Map (1-indexed in spec, 0-indexed in JavaScript):
   * - Pos 0-5: Plant Code (26MTM)
   * - Pos 5-7: Dock Code (FB)
   * - Pos 7-12: Supplier Code (05474)
   * - Pos 12-15: Supplier Ship Dock (spaces)
   * - Pos 15-27: Order Number (2025121134  ) - 12 chars with trailing spaces
   * - Pos 27-36: Route Code (JAAJ17   ) - 9 chars with trailing spaces
   * - Pos 36-40: Filler ("Load")
   * - Pos 40-54: Pickup DateTime (20251211181000) - YYYYMMDDHHMMSS (14 chars)
   *
   * Author: Hassan, 2025-11-05
   * Updated: 2025-12-17 - Fixed to use correct 54-byte Driver Checksheet format
   */
  const parsePickupRouteQR = (qrValue: string): PickupRouteData | null => {
    try {
      // Expected length: 54 characters
      if (qrValue.length < 54) {
        console.error(`Invalid pickup route QR format. Expected 54-character format, got ${qrValue.length} characters.`);
        return null;
      }

      // Extract fields using 0-indexed substring (Toyota spec is 1-indexed)
      const plantCode = qrValue.substring(0, 5).trim();           // Pos 0-5: 26MTM
      const dockCode = qrValue.substring(5, 7).trim();            // Pos 5-7: FB
      const supplierCode = qrValue.substring(7, 12).trim();       // Pos 7-12: 05474
      const supplierShipDock = qrValue.substring(12, 15).trim();  // Pos 12-15: (spaces/unused)
      const orderNumber = qrValue.substring(15, 27).trim();       // Pos 15-27: 2025121134 (trim spaces)
      const routeCode = qrValue.substring(27, 36).trim();         // Pos 27-36: JAAJ17 (trim spaces)
      const filler = qrValue.substring(36, 40);                   // Pos 36-40: "Load"
      const pickupDateTime = qrValue.substring(40, 54);           // Pos 40-54: 20251211181000 (YYYYMMDDHHMMSS)

      console.log('=== PICKUP ROUTE QR PARSING (54-BYTE FORMAT) ===');
      console.log('Raw input:', qrValue);
      console.log('Length:', qrValue.length);
      console.log('Extracted fields:');
      console.log('  plantCode (0-5):', `"${plantCode}"`);
      console.log('  dockCode (5-7):', `"${dockCode}"`);
      console.log('  supplierCode (7-12):', `"${supplierCode}"`);
      console.log('  supplierShipDock (12-15):', `"${supplierShipDock}"`);
      console.log('  orderNumber (15-27):', `"${orderNumber}"`);
      console.log('  routeCode (27-36):', `"${routeCode}"`);
      console.log('  filler (36-40):', `"${filler}"`);
      console.log('  pickupDateTime (40-54):', `"${pickupDateTime}"`);

      // Convert pickup datetime (YYYYMMDDHHMMSS) to ISO 8601 format and separate date/time
      let pickupDateTimeISO = '';
      let pickupDate = '';
      let pickupTime = '';
      if (pickupDateTime.length === 14) {
        const year = pickupDateTime.substring(0, 4);
        const month = pickupDateTime.substring(4, 6);
        const day = pickupDateTime.substring(6, 8);
        const hour = pickupDateTime.substring(8, 10);
        const minute = pickupDateTime.substring(10, 12);
        const second = pickupDateTime.substring(12, 14);
        pickupDateTimeISO = `${year}-${month}-${day}T${hour}:${minute}:${second}`;
        pickupDate = `${year}-${month}-${day}`;
        pickupTime = `${hour}:${minute}`;
        console.log('  Parsed Pickup DateTime (ISO):', pickupDateTimeISO);
        console.log('  Parsed Pickup Date:', pickupDate);
        console.log('  Parsed Pickup Time:', pickupTime);
      }

      // Validate required fields
      if (!plantCode || !dockCode || !supplierCode || !routeCode) {
        console.error('Invalid pickup route data - missing required fields');
        return null;
      }

      console.log('✓ Pickup Route QR Parsed successfully!');
      console.log('======================');

      // For estimatedSkids, we'll use a default value since it's not in the QR
      // This will be replaced with actual planned skid data from the system
      const estimatedSkids = 5; // Default value

      return {
        orderNumber,
        routeNumber: routeCode,
        plant: plantCode,
        supplierCode,
        dockCode,
        pickupDateTime: pickupDateTimeISO,
        pickupDate,
        pickupTime,
        estimatedSkids,
        rawQRValue: qrValue,
      };
    } catch (error) {
      console.error('Error parsing pickup route QR:', error);
      return null;
    }
  };

  /**
   * Screen 1: Handle Pickup Route QR Scan
   * Updated: 2025-11-05 - Added draft session recovery
   * Updated: 2025-12-17 - Call validate-order API to get actual skid count
   */
  const handlePickupRouteScan = async (result: ScanResult) => {
    console.log('Pickup route scan result:', result);

    const parsedData = parsePickupRouteQR(result.scannedValue);

    if (!parsedData) {
      setError('Invalid Pickup Route QR Code. Please scan the correct barcode.');
      return;
    }

    // Check for existing draft in localStorage
    const savedDraft = localStorage.getItem('shipment-load-v2-draft');
    if (savedDraft) {
      try {
        const draft = JSON.parse(savedDraft);
        // Check if draft is for the same route
        if (draft.pickupRouteData?.routeNumber === parsedData.routeNumber) {
          const shouldResume = confirm(
            `Found a saved session for route ${parsedData.routeNumber} from ${new Date(draft.savedAt).toLocaleString()}.\n\nDo you want to resume this session?`
          );

          if (shouldResume) {
            console.log('Resuming saved session:', draft);
            // Restore all state from draft
            setPickupRouteData(draft.pickupRouteData);
            setTrailerData(draft.trailerData);
            setPlannedSkids(draft.plannedSkids);
            setScannedSkids(draft.scannedSkids);
            setExceptions(draft.exceptions);
            setCurrentScreen(draft.currentScreen);
            setConfirmationNumber(draft.confirmationNumber || '');
            setRackExceptionEnabled(draft.rackExceptionEnabled || false);
            setError(null);
            return;
          } else {
            // Clear old draft if user chooses not to resume
            localStorage.removeItem('shipment-load-v2-draft');
          }
        }
      } catch (e) {
        console.error('Error parsing saved draft:', e);
        localStorage.removeItem('shipment-load-v2-draft');
      }
    }

    // Call validate-order API to get actual skid count
    setLoading(true);
    setError(null);

    try {
      console.log('Validating order:', parsedData.orderNumber, 'Dock:', parsedData.dockCode);

      const validateResponse = await validateShipmentLoadOrder(
        parsedData.orderNumber,
        parsedData.dockCode
      );

      if (!validateResponse.success || !validateResponse.data) {
        setError(validateResponse.message || 'Order not found or skid-build not complete.');
        setLoading(false);
        return;
      }

      const orderData: ValidateOrderResponse = validateResponse.data;

      // Check if skid build is complete OR skid count is zero
      if (orderData.skidCount === 0 || !orderData.skidBuildComplete) {
        setError(`Skid for order number ${parsedData.orderNumber} is not built. Please build the skid and then continue!`);
        setLoading(false);
        return;
      }

      // Check if order is already shipped
      // Author: Hassan, Date: 2026-01-04
      // FIX: Show the hidden view with info but prevent continuation
      if (orderData.status === 'Shipped') {
        setError(`Order ${parsedData.orderNumber} has already been shipped. Cannot load again.`);
        // Don't return - continue to set pickupRouteData so the view shows
      }

      console.log('Order validated successfully!');
      console.log('Actual skid count:', orderData.skidCount);

      // Store actual skid count
      setActualSkidCount(orderData.skidCount);

      // Store order validation data (Toyota confirmation numbers)
      setOrderValidationData({
        toyotaConfirmationNumber: orderData.toyotaConfirmationNumber,
        toyotaShipmentConfirmationNumber: orderData.toyotaShipmentConfirmationNumber,
        status: orderData.status,
      });

      // Successfully parsed and validated - new session
      setPickupRouteData(parsedData);

      // Only clear error if not shipped (shipped orders need to keep their error message)
      if (orderData.status !== 'Shipped') {
        setError(null);
      }
    } catch (err) {
      console.error('Error validating order:', err);
      setError('Failed to validate order. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Screen 1: Continue to Trailer Information
   * Updated: 2025-12-08 - Call API to get planned orders for route
   * Updated: 2025-12-17 - Use session/start API with orderNumber and dockCode
   * Updated: 2025-12-17 - Fixed to use actual authenticated user instead of hardcoded ID
   * Updated: 2025-12-17 - Integrated getOrderSkids to fetch actual skids from tblSkidScans
   * Updated: 2025-12-22 - Auto-populate trailer data when session is resumed
   */
  const handlePickupRouteContinue = async () => {
    if (!pickupRouteData) {
      setError('Please scan a valid pickup route QR code first');
      return;
    }

    // Validate user authentication
    if (!user?.id) {
      setError('User not authenticated. Please log in again.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // Call session/start API with orderNumber and dockCode
      const sessionResponse = await startShipmentLoadSession({
        routeNumber: pickupRouteData.routeNumber,
        supplierCode: pickupRouteData.supplierCode,
        pickupDateTime: pickupRouteData.pickupDateTime,
        userId: user.id,
        orderNumber: pickupRouteData.orderNumber,  // NEW - from QR
        dockCode: pickupRouteData.dockCode,         // NEW - from QR
      });

      if (!sessionResponse.success || !sessionResponse.data) {
        setError(sessionResponse.message || 'Failed to start session');
        setLoading(false);
        return;
      }

      const sessionData: StartSessionResponse = sessionResponse.data;

      // Store session ID and actual skid count
      setSessionId(sessionData.sessionId);
      setActualSkidCount(sessionData.scannedOrderSkidCount || 0);

      // If session was resumed, populate trailer data from session
      if (sessionData.isResumed) {
        setTrailerData({
          trailerNumber: sessionData.trailerNumber || '',
          driverFirstName: sessionData.driverFirstName || '',
          driverLastName: sessionData.driverLastName || '',
          sealNumber: sessionData.sealNumber || '',
          supplierFirstName: sessionData.supplierFirstName || '',
          supplierLastName: sessionData.supplierLastName || '',
        });
        console.log('Session resumed - trailer data restored:', {
          trailerNumber: sessionData.trailerNumber,
          driverFirstName: sessionData.driverFirstName,
          driverLastName: sessionData.driverLastName,
        });
      }

      console.log('Session started successfully!');
      console.log('Session ID:', sessionData.sessionId);
      console.log('Is Resumed:', sessionData.isResumed);
      console.log('Orders from session:', sessionData.orders?.length || 0);
      console.log('Orders:', sessionData.orders?.map(o => `${o.orderNumber}-${o.dockCode}`).join(', '));

      // Fetch skids for ALL orders in the session (not just the scanned one)
      const allPlannedSkids: PlannedSkid[] = [];
      const allScannedSkids: ScannedSkid[] = [];

      // Build a map of which orders are already scanned
      const scannedOrdersMap = new Map<string, boolean>();
      if (sessionData.orders) {
        sessionData.orders.forEach(order => {
          const key = `${order.orderNumber}-${order.dockCode}`;
          scannedOrdersMap.set(key, order.isScanned || false);
        });
      }

      // Get all unique orders from session (or use scanned order if no orders returned)
      const ordersToFetch = sessionData.orders && sessionData.orders.length > 0
        ? sessionData.orders.map(o => ({ orderNumber: o.orderNumber, dockCode: o.dockCode }))
        : [{ orderNumber: pickupRouteData.orderNumber, dockCode: pickupRouteData.dockCode }];

      console.log('Fetching skids for orders:', ordersToFetch.map(o => `${o.orderNumber}-${o.dockCode}`).join(', '));
      console.log('Scanned orders map:', Object.fromEntries(scannedOrdersMap));

      // Fetch skids for each order
      for (const orderInfo of ordersToFetch) {
        try {
          const skidsResponse = await getOrderSkids(orderInfo.orderNumber, orderInfo.dockCode);

          if (skidsResponse.success && skidsResponse.data) {
            const skidsData: OrderSkidsResponse = skidsResponse.data;
            const orderKey = `${orderInfo.orderNumber}-${orderInfo.dockCode}`;
            const isOrderScanned = scannedOrdersMap.get(orderKey) || false;

            skidsData.skids.forEach((skid: SkidDto) => {
              if (isOrderScanned) {
                // Order was already scanned - add to scannedSkids
                allScannedSkids.push({
                  id: skid.skidId,
                  skidId: skid.skidNumber,
                  manifestNo: 0, // Not available from skid data
                  palletizationCode: skid.palletizationCode || '',
                  mros: '',
                  orderNumber: skidsData.orderNumber,
                  dockCode: skidsData.dockCode,
                  timestamp: skid.scannedAt || new Date().toISOString(),
                });
              } else {
                // Order not scanned yet - add to plannedSkids
                allPlannedSkids.push({
                  skidId: skid.skidId,
                  skidNumber: skid.skidNumber,
                  skidSide: skid.skidSide,
                  palletizationCode: skid.palletizationCode,
                  scannedAt: skid.scannedAt,
                  orderNumber: skidsData.orderNumber,
                  dockCode: skidsData.dockCode,
                  isScanned: false,
                });
              }
            });

            console.log(`Loaded skids for order ${orderInfo.orderNumber}-${orderInfo.dockCode}: isScanned=${isOrderScanned}, count=${skidsData.skids.length}`);
          } else {
            console.warn(`Failed to fetch skids for order ${orderInfo.orderNumber}-${orderInfo.dockCode}:`, skidsResponse.message);
          }
        } catch (err) {
          console.error(`Error fetching skids for order ${orderInfo.orderNumber}-${orderInfo.dockCode}:`, err);
        }
      }

      if (allPlannedSkids.length === 0 && allScannedSkids.length === 0) {
        setError('No skids found for any orders in this route');
        setLoading(false);
        return;
      }

      console.log('Total planned skids:', allPlannedSkids.length);
      console.log('Total scanned skids:', allScannedSkids.length);

      setPlannedSkids(allPlannedSkids);
      setScannedSkids(allScannedSkids);
      setCurrentScreen(2);
      setError(null);
    } catch (err) {
      console.error('Error starting session:', err);
      setError('Failed to start session. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Screen 2: Validate and Continue to Skid Scanning
   * Updated: 2025-12-17 - Integrated updateShipmentLoadSession API
   * Updated: 2025-12-17 - Changed to use separate first/last name fields
   */
  const handleTrailerContinue = async () => {
    // Validate required fields
    if (!trailerData.trailerNumber.trim()) {
      setError('Trailer Number is required');
      return;
    }
    if (!trailerData.driverFirstName.trim()) {
      setError('Driver First Name is required');
      return;
    }
    if (!trailerData.driverLastName.trim()) {
      setError('Driver Last Name is required');
      return;
    }

    // Check user authentication
    if (!user?.id) {
      setError('User not authenticated. Please log in again.');
      return;
    }

    // Check sessionId exists
    if (!sessionId) {
      setError('Session not found. Please scan the pickup route QR again.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await updateShipmentLoadSession({
        sessionId: sessionId,
        trailerNumber: trailerData.trailerNumber.trim(),
        sealNumber: trailerData.sealNumber.trim() || undefined,
        driverFirstName: trailerData.driverFirstName.trim(),
        driverLastName: trailerData.driverLastName.trim(),
        supplierFirstName: trailerData.supplierFirstName.trim() || undefined,
        supplierLastName: trailerData.supplierLastName.trim() || undefined,
        // NOTE: NOT sending lpCode (SCAC Code) per business requirement
      });

      if (!response.success) {
        setError(response.error || 'Failed to save trailer information');
        setLoading(false);
        return;
      }

      setCurrentScreen(3);
    } catch (err) {
      setError('Failed to save trailer information');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Parse Toyota Manifest QR Code (44 characters)
   *
   * Example: "02TMI02806V82023080205  IDVV01      LB05001A"
   *
   * Fixed-Position Format (MUST MATCH skid-build/page.tsx EXACTLY):
   * - Pos 0-5: Plant Code (02TMI)
   * - Pos 5-10: Supplier Code (02806)
   * - Pos 10-12: Dock Code (V8)
   * - Pos 12-24: Order Number (2023080205  ) - 12 chars with trailing spaces
   * - Pos 24-36: Load/Transport ID (IDVV01      ) - 12 chars with trailing spaces
   * - Pos 36-38: Palletization (LB)
   * - Pos 38-40: MROS (05)
   * - Pos 40-44: SKID ID (001A)
   *
   * Returns INDIVIDUAL FIELDS (NOT combined string)
   *
   * Author: Hassan, 2025-11-05
   * Updated: 2025-12-08 - Return individual fields instead of combined skidId string
   */
  interface ParsedManifest {
    plantCode: string;
    supplierCode: string;
    dockCode: string;
    orderNumber: string;
    loadId: string;
    palletizationCode: string;  // "LB" - INDIVIDUAL
    mros: string;               // "05" - INDIVIDUAL
    skidId: string;             // "001A" - INDIVIDUAL
  }

  const parseToyotaManifest = (qr: string): ParsedManifest | null => {
    if (qr.length < 44) {
      console.error('Invalid Toyota Manifest - expected 44 characters, got:', qr.length);
      return null;
    }

    try {
      // FIXED POSITION EXTRACTION (0-indexed for JavaScript substring)
      // CRITICAL: MUST MATCH skid-build/page.tsx positions EXACTLY
      const plantCode = qr.substring(0, 5).trim();           // Positions 0-5: "02TMI"
      const supplierCode = qr.substring(5, 10).trim();       // Positions 5-10: "02806"
      const dockCode = qr.substring(10, 12).trim();          // Positions 10-12: "V8"
      const orderNumber = qr.substring(12, 24).trim();       // Positions 12-24: "2023080205  " (trim spaces)
      const loadId = qr.substring(24, 36).trim();            // Positions 24-36: "IDVV01      " (12 chars)
      const palletizationCode = qr.substring(36, 38);        // Positions 36-38: "LB" - INDIVIDUAL
      const mros = qr.substring(38, 40);                     // Positions 38-40: "05" - INDIVIDUAL
      const skidId = qr.substring(40, 44);                   // Positions 40-44: "001A" - INDIVIDUAL

      console.log('=== MANIFEST SCAN PARSING (INDIVIDUAL FIELDS) ===');
      console.log('Raw input:', qr);
      console.log('Length:', qr.length);
      console.log('Extracted fields (INDIVIDUAL - NOT COMBINED):');
      console.log('  plantCode (0-5):', `"${plantCode}"`);
      console.log('  supplierCode (5-10):', `"${supplierCode}"`);
      console.log('  dockCode (10-12):', `"${dockCode}"`);
      console.log('  orderNumber (12-24):', `"${orderNumber}"`);
      console.log('  loadId (24-36):', `"${loadId}"`);
      console.log('  palletizationCode (36-38):', `"${palletizationCode}"` + ' (INDIVIDUAL)');
      console.log('  mros (38-40):', `"${mros}"` + ' (INDIVIDUAL)');
      console.log('  skidId (40-44):', `"${skidId}"` + ' (INDIVIDUAL)');
      console.log('✓ Manifest Scan Parsed successfully!');
      console.log('======================');

      return {
        plantCode,
        supplierCode,
        dockCode,
        orderNumber,
        loadId,
        palletizationCode,
        mros,
        skidId,
      };
    } catch (error) {
      console.error('Error parsing Toyota Manifest:', error);
      return null;
    }
  };

  /**
   * Screen 3: Handle Skid Scan
   * Parse scanned Toyota Manifest (44 chars) and validate via API
   * Author: Hassan, 2025-11-05
   * Updated: 2025-12-08 - Call API with individual fields instead of combined skidId
   * Updated: 2025-12-17 - Fixed to use actual authenticated user instead of hardcoded ID
   * Updated: 2025-12-17 - Match skids by skidId AND palletizationCode (SkidNumber + PalletizationCode grouping)
   * Updated: 2025-12-17 - CRITICAL FIX: Added sessionId to API call and validation check
   */
  const handleSkidScan = async (result: ScanResult) => {
    console.log('Skid scan result:', result);

    if (!result.success) {
      setError(result.error || 'Scan failed');
      return;
    }

    const scannedValue = result.scannedValue;

    // Parse the Toyota Manifest QR to extract INDIVIDUAL fields
    const manifest = parseToyotaManifest(scannedValue);

    if (!manifest) {
      setError('Invalid Toyota Manifest. Please scan the correct label.');
      return;
    }

    if (!pickupRouteData) {
      setError('Route data not found. Please start over.');
      return;
    }

    // Validate user authentication
    if (!user?.id) {
      setError('User not authenticated. Please log in again.');
      return;
    }

    // Validate sessionId exists
    if (!sessionId) {
      setError('Session not found. Please scan the pickup route QR again.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      console.log('=== SKID SCAN API CALL DEBUG ===');
      console.log('Session ID:', sessionId);
      console.log('Route Number:', pickupRouteData.routeNumber);
      console.log('Order Number:', manifest.orderNumber);
      console.log('Dock Code:', manifest.dockCode);
      console.log('Palletization Code:', manifest.palletizationCode);
      console.log('MROS:', manifest.mros);
      console.log('Skid ID:', manifest.skidId);
      console.log('Scanned By:', user.id);

      // Call API to validate and scan skid with INDIVIDUAL fields
      const response = await scanShipmentLoadSkid({
        sessionId: sessionId,                           // CRITICAL - Session ID required by backend
        routeNumber: pickupRouteData.routeNumber,
        orderNumber: manifest.orderNumber,
        dockCode: manifest.dockCode,
        palletizationCode: manifest.palletizationCode,  // Individual field
        mros: manifest.mros,                            // Individual field
        skidId: manifest.skidId,                        // Individual field (4 chars: "001A")
        scannedBy: user.id,
      });

      console.log('API Response:', response);
      console.log('======================');

      if (!response.success || !response.data) {
        setError(response.error || 'Failed to validate skid');
        setLoading(false);
        return;
      }

      // CRITICAL: Extract SkidNumber from scanned skidId (e.g., "001A" → "001")
      // Match by SkidNumber + PalletizationCode (same as Toyota API grouping)
      const scannedSkidNumber = manifest.skidId.substring(0, 3); // "001A" → "001"

      console.log('=== SKID MATCHING (SkidNumber + PalletizationCode) ===');
      console.log('Scanned skidId:', manifest.skidId);
      console.log('Extracted skidNumber:', scannedSkidNumber);
      console.log('Scanned palletizationCode:', manifest.palletizationCode);
      console.log('Scanned orderNumber:', manifest.orderNumber);
      console.log('Scanned dockCode:', manifest.dockCode);

      // Find matching planned skid by Order Number + Dock Code + Skid Number + Palletization Code
      const plannedSkidIndex = plannedSkids.findIndex(
        skid => skid.orderNumber === manifest.orderNumber &&
                skid.dockCode === manifest.dockCode &&
                skid.skidNumber === scannedSkidNumber &&
                skid.palletizationCode === manifest.palletizationCode
      );

      if (plannedSkidIndex === -1) {
        setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} not found in planned list.`);
        setLoading(false);
        return;
      }

      const plannedSkid = plannedSkids[plannedSkidIndex];

      // Check if already scanned by Order Number + Dock Code + Skid Number + Palletization Code
      const scannedSkidNumber_from_scanned = manifest.skidId.substring(0, 3);
      const alreadyScanned = scannedSkids.some(
        item => {
          const itemSkidNumber = item.skidId.substring(0, 3);
          return item.orderNumber === manifest.orderNumber &&
                 item.dockCode === manifest.dockCode &&
                 itemSkidNumber === scannedSkidNumber_from_scanned &&
                 item.palletizationCode === manifest.palletizationCode;
        }
      );

      if (alreadyScanned) {
        setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} has already been scanned.`);
        setLoading(false);
        return;
      }

      // Create scanned item with individual fields
      const newScannedSkid: ScannedSkid = {
        id: `${Date.now()}-${Math.random()}`,
        skidId: manifest.skidId,
        manifestNo: 0, // Not available from new API
        palletizationCode: manifest.palletizationCode,
        mros: manifest.mros,
        orderNumber: manifest.orderNumber,
        dockCode: manifest.dockCode,
        timestamp: new Date().toISOString(),
      };

      // Update state: mark planned as scanned and add to scanned list
      setPlannedSkids(plannedSkids.map((skid, idx) =>
        idx === plannedSkidIndex ? { ...skid, isScanned: true } : skid
      ));
      setScannedSkids([...scannedSkids, newScannedSkid]);
      setError(null);
      console.log('Skid scanned successfully:', newScannedSkid);
      console.log('Matched planned skid:', plannedSkid);
      console.log('======================');
    } catch (err) {
      console.error('Error scanning skid:', err);
      setError('Failed to scan skid. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Screen 3: Add Exception Handler
   * Author: Hassan, 2025-11-05
   * Updated: 2025-12-17 - Use composite key (SkidNumber-PalletizationCode) for relatedSkidId
   */
  const handleAddException = () => {
    // F-GAP-002: Updated validation for trailer/skid level exceptions
    if (!selectedExceptionType || !selectedExceptionCode || !exceptionComments.trim()) {
      setError('Please select exception type and add comments');
      return;
    }

    // For skid-level exceptions, require skid selection
    if (exceptionLevel === 'skid' && !selectedSkidForException) {
      setError('Please select which skid this exception is for');
      return;
    }

    const newException: Exception = {
      type: selectedExceptionType,
      code: selectedExceptionCode,
      comments: exceptionComments.trim(),
      relatedSkidId: exceptionLevel === 'trailer' ? null : selectedSkidForException, // null for trailer, SkidNumber-PalletizationCode for skid
      timestamp: new Date().toISOString(),
      level: exceptionLevel,
    };

    console.log('Adding exception:', newException);
    setExceptions([...exceptions, newException]);
    setSelectedExceptionType('');
    setSelectedExceptionCode('');
    setExceptionComments('');
    setSelectedSkidForException('');
    setExceptionLevel('trailer'); // Reset to default
    setShowExceptionModal(false);
    setError(null);
  };

  /**
   * Screen 3: Remove Exception Handler
   * Author: Hassan, 2025-11-05
   */
  const handleRemoveException = (index: number) => {
    console.log('Removing exception at index:', index);
    setExceptions(exceptions.filter((_, i) => i !== index));
  };

  /**
   * FAB Menu: Save Draft Handler
   * Author: Hassan, 2025-11-05
   * Updated: 2025-11-05 - Added rackExceptionEnabled to draft
   */
  const handleSaveDraft = () => {
    const draft = {
      pickupRouteData,
      trailerData,
      plannedSkids,
      scannedSkids,
      exceptions,
      currentScreen,
      confirmationNumber,
      rackExceptionEnabled,
      savedAt: new Date().toISOString(),
    };

    localStorage.setItem('shipment-load-v2-draft', JSON.stringify(draft));
    console.log('Draft saved to localStorage:', draft);
    setFabMenuOpen(false);
    setShowSuccessAlert(true);
    setTimeout(() => setShowSuccessAlert(false), 3000);
  };

  /**
   * FAB Menu: Unpick All Handler
   * Author: Hassan, 2025-11-05
   */
  const handleUnpickAll = () => {
    if (confirm('Are you sure you want to unpick all scanned items?')) {
      console.log('Unpicking all scanned items');
      setScannedSkids([]);
      setPlannedSkids(plannedSkids.map(skid => ({ ...skid, isScanned: false })));
      setFabMenuOpen(false);
    }
  };

  /**
   * Screen 3: Final Submit with Validation
   * Updated: 2025-11-05 - Added exception validation logic
   * Updated: 2025-12-08 - Call complete shipment API
   * Updated: 2025-12-17 - Fixed to use actual authenticated user instead of hardcoded ID
   * Updated: 2025-12-17 - Match exceptions using composite key (SkidNumber-PalletizationCode)
   */
  const handleFinalSubmit = async () => {
    console.log('=== SUBMIT VALIDATION ===');
    console.log('Planned Skids:', plannedSkids);
    console.log('Scanned Skids:', scannedSkids);
    console.log('Exceptions:', exceptions);

    // F-GAP-004: Code 99 (Unplanned Expedite) Special Validation
    // Toyota Spec Page 25: Three conditions must be present when code 99 is used
    const hasCode99TrailerException = exceptions.some(
      e => e.code === '99' && e.level === 'trailer'
    );

    if (hasCode99TrailerException) {
      console.log('=== CODE 99 VALIDATION ===');

      // Condition 1: Route must have "EX-" prefix
      if (!pickupRouteData?.routeNumber.startsWith('EX-')) {
        setError('Code 99 (Unplanned Expedite) requires route to start with "EX-" prefix. Current route: ' + pickupRouteData?.routeNumber);
        return;
      }
      console.log('✓ Route has EX- prefix');

      // Condition 3: Every scanned skid must have a skid-level exception
      const skidIdsWithExceptions = new Set(
        exceptions
          .filter(e => e.level === 'skid' && e.relatedSkidId)
          .map(e => e.relatedSkidId)
      );

      // Check all scanned skids have exceptions
      const scannedSkidKeys = scannedSkids.map(s => {
        const skidNumber = s.skidId.substring(0, 3);
        return `${skidNumber}-${s.palletizationCode}`;
      });

      const skidsWithoutExceptions = scannedSkidKeys.filter(
        key => !skidIdsWithExceptions.has(key)
      );

      if (skidsWithoutExceptions.length > 0) {
        setError(`Code 99 (Unplanned Expedite) requires ALL skids to have exceptions. Missing exceptions for: ${skidsWithoutExceptions.join(', ')}`);
        return;
      }
      console.log('✓ All skids have exceptions');
      console.log('======================');
    }

    // Check if all planned items are loaded
    const areAllItemsLoaded = plannedSkids.every(skid => skid.isScanned);
    console.log('All items loaded:', areAllItemsLoaded);

    // Get unloaded skids
    const unloadedSkids = plannedSkids.filter(skid => !skid.isScanned);
    console.log('Unloaded skids:', unloadedSkids);

    // Check if unloaded items have exceptions (using composite key: SkidNumber-PalletizationCode)
    const hasExceptionForAllUnloaded = unloadedSkids.every(skid => {
      const compositeKey = `${skid.skidNumber}-${skid.palletizationCode}`;
      return exceptions.some(e => e.relatedSkidId === compositeKey);
    });
    console.log('All unloaded have exceptions:', hasExceptionForAllUnloaded);

    // Enable submit only if all loaded OR (some loaded AND all unloaded have exceptions)
    const canSubmit = areAllItemsLoaded || (scannedSkids.length > 0 && hasExceptionForAllUnloaded);
    console.log('Can submit:', canSubmit);

    if (!canSubmit) {
      if (scannedSkids.length === 0) {
        setError('Cannot submit: No skids have been scanned yet.');
      } else if (!hasExceptionForAllUnloaded) {
        setError(`Cannot submit: ${unloadedSkids.length} unloaded skid(s) require exceptions.`);
      }
      return;
    }

    if (!pickupRouteData) {
      setError('Route data not found. Please start over.');
      return;
    }

    // Validate user authentication
    if (!user?.id) {
      setError('User not authenticated. Please log in again.');
      return;
    }

    setLoading(true);
    setError(null);

    // Check sessionId exists
    if (!sessionId) {
      setError('Session not found. Please start over.');
      return;
    }

    try {
      // Call complete shipment API with sessionId and userId
      const response = await completeShipmentLoadSession({
        sessionId: sessionId,
        userId: user.id,
      });

      if (!response.success || !response.data) {
        setError(response.error || 'Failed to complete shipment');
        setLoading(false);
        return;
      }

      // Set confirmation number from API response
      setConfirmationNumber(response.data.confirmationNumber);
      console.log('Shipment completed successfully:', response.data);

      // Clear localStorage draft on successful submission
      localStorage.removeItem('shipment-load-v2-draft');

      // Move to success screen
      setCurrentScreen(4);
      setError(null);
    } catch (err) {
      console.error('Error completing shipment:', err);
      setError('Failed to complete shipment. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Reset everything
   * Updated: 2025-11-05 - Clear localStorage draft on reset
   * Updated: 2025-11-05 - Added rackExceptionEnabled reset
   * Updated: 2025-12-17 - Updated trailerData to use new field structure
   * Updated: 2026-01-04 - Integrated restart session API
   */
  const handleReset = async () => {
    if (!confirm('Are you sure you want to restart? All scanned data will be deleted. This action cannot be undone.')) {
      return;
    }

    // Clear local draft
    localStorage.removeItem('shipment-load-v2-draft');

    if (!sessionId) {
      // No session, just reset local state
      resetLocalState();
      return;
    }

    try {
      const response = await restartShipmentLoadSession(sessionId);
      if (response.success) {
        // Reset local state and go back to screen 1
        resetLocalState();
      } else {
        alert(response.error || 'Failed to restart session');
        // Still reset local state even if API fails
        resetLocalState();
      }
    } catch (error: any) {
      alert(error.message || 'Failed to restart session. It may have already been confirmed by Toyota.');
      // Still reset local state
      resetLocalState();
    }
  };

  /**
   * Reset local state
   * Extracted from handleReset for reusability
   */
  const resetLocalState = () => {
    setCurrentScreen(1);
    setPickupRouteData(null);
    setTrailerData({
      trailerNumber: '',
      driverFirstName: '',
      driverLastName: '',
      sealNumber: '',
      supplierFirstName: '',
      supplierLastName: '',
    });
    setPlannedSkids([]);
    setScannedSkids([]);
    setExceptions([]);
    setConfirmationNumber('');
    setRackExceptionEnabled(false);
    setOrderValidationData(null);
    setError(null);
    setSessionId(null); // Also clear sessionId
  };

  /**
   * Start New Shipment Handler (Screen 4)
   * Author: Hassan, 2025-11-05
   */
  const handleNewShipment = () => {
    handleReset();
  };

  return (
    <div className="relative min-h-screen">
      {/* Background - Fixed */}
      <VUTEQStaticBackground />

      {/* Content */}
      <div className="relative">
        <div className="p-3 max-w-3xl mx-auto space-y-2 pb-20">
          {/* Progress Indicator - Hide on Success Screen */}
          {currentScreen !== 4 && (
            <Card>
              <CardContent className="p-2">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-medium" style={{ color: '#253262' }}>
                    Screen {currentScreen} of 3
                  </span>
                  <div className="flex gap-1">
                    {[1, 2, 3].map((step) => (
                      <div
                        key={step}
                        className="w-10 h-1 rounded-full"
                        style={{
                          backgroundColor: step <= currentScreen ? '#253262' : '#E5E7EB',
                        }}
                      />
                    ))}
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Error Alert */}
          {error && (
            <Alert variant="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          {/* Success Alert for Draft Save */}
          {showSuccessAlert && (
            <Alert variant="success" onClose={() => setShowSuccessAlert(false)}>
              Draft saved successfully!
            </Alert>
          )}

          {/* SCREEN 1: Scan Pickup Route QR */}
          {currentScreen === 1 && (
            <Card className="bg-[#FCFCFC]">
              <CardContent className="p-3 space-y-3">
                {/* Header with Icon */}
                <div className="flex items-center gap-3 pb-2 border-b border-gray-200">
                  <i className="fa fa-truck text-2xl" style={{ color: '#253262' }}></i>
                  <div>
                    <h1 className="text-xl font-bold" style={{ color: '#253262' }}>
                      Shipment Load
                    </h1>
                    <p className="text-sm text-gray-600">
                      Scan the Pickup Route QR code to begin
                    </p>
                  </div>
                </div>

                {/* Scanner */}
                {!pickupRouteData && (
                  <Scanner
                    onScan={handlePickupRouteScan}
                    label="Scan Pickup Route QR Code"
                    placeholder="Scan Pickup Route QR Code"
                    disabled={loading}
                  />
                )}

                {/* Display Scanned Route Information */}
                {pickupRouteData && (
                  <div className="space-y-2 pt-2">
                    <div className="p-3 bg-success-50 border-2 border-success-200 rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <i className="fa fa-circle-check text-success-600 text-lg"></i>
                        <h3 className="font-semibold text-sm text-success-700">Pickup Route Scanned</h3>
                      </div>

                      <div className="grid grid-cols-2 gap-x-3 gap-y-1.5 text-xs">
                        <div>
                          <span className="text-gray-600">Order Number:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.orderNumber}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Route Number:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.routeNumber}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Scanned Skids:</span>
                          <p className="font-mono font-bold text-gray-900">{actualSkidCount > 0 ? actualSkidCount : pickupRouteData.estimatedSkids}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Dock Code:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.dockCode}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Pickup Date:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.pickupDate}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Pickup Time:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.pickupTime}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Plant Code:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.plant}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Supplier Code:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.supplierCode}</p>
                        </div>

                        {orderValidationData?.toyotaConfirmationNumber && (
                          <div>
                            <span className="text-gray-600">Toyota Confirmation:</span>
                            <p className="font-mono font-bold text-gray-900">{orderValidationData.toyotaConfirmationNumber}</p>
                          </div>
                        )}
                        {orderValidationData?.status === 'Shipped' && orderValidationData?.toyotaShipmentConfirmationNumber && (
                          <div>
                            <span className="text-gray-600">Toyota Shipment:</span>
                            <p className="font-mono font-bold text-gray-900">{orderValidationData.toyotaShipmentConfirmationNumber}</p>
                          </div>
                        )}
                      </div>

                      {/* Continue Button - hide if already shipped */}
                      {orderValidationData?.status !== 'Shipped' && (
                        <div className="mt-3">
                          <Button
                            onClick={handlePickupRouteContinue}
                            variant="success-light"
                            fullWidth
                            loading={loading}
                            disabled={loading}
                          >
                            <i className="fa fa-arrow-right mr-2"></i>
                            Continue to Trailer Information
                          </Button>
                        </div>
                      )}
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* SCREEN 2: Trailer Information Form */}
          {currentScreen === 2 && (
            <Card>
              <CardHeader>
                <CardTitle>Trailer Information</CardTitle>
              </CardHeader>
              <CardContent className="space-y-3">
                <p className="text-sm text-gray-600">
                  Enter trailer and driver details for this shipment.
                </p>

                {/* Trailer Number */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Trailer Number *
                  </label>
                  <input
                    type="text"
                    value={trailerData.trailerNumber}
                    onChange={(e) => setTrailerData({ ...trailerData, trailerNumber: e.target.value })}
                    placeholder="Enter trailer number (max 20 chars)"
                    maxLength={20}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Driver First Name */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Driver First Name *
                  </label>
                  <input
                    type="text"
                    value={trailerData.driverFirstName}
                    onChange={(e) => setTrailerData({ ...trailerData, driverFirstName: e.target.value })}
                    placeholder="Enter driver first name (max 9 chars)"
                    maxLength={9}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Driver Last Name */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Driver Last Name *
                  </label>
                  <input
                    type="text"
                    value={trailerData.driverLastName}
                    onChange={(e) => setTrailerData({ ...trailerData, driverLastName: e.target.value })}
                    placeholder="Enter driver last name (max 12 chars)"
                    maxLength={12}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Seal Number */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Seal Number
                  </label>
                  <input
                    type="text"
                    value={trailerData.sealNumber}
                    onChange={(e) => setTrailerData({ ...trailerData, sealNumber: e.target.value })}
                    placeholder="Enter seal number (max 20 chars, optional)"
                    maxLength={20}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Shipping TM First Name */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Shipping TM First Name
                  </label>
                  <input
                    type="text"
                    value={trailerData.supplierFirstName}
                    onChange={(e) => setTrailerData({ ...trailerData, supplierFirstName: e.target.value })}
                    placeholder="Enter shipping TM first name (max 9 chars, optional)"
                    maxLength={9}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Shipping TM Last Name */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Shipping TM Last Name
                  </label>
                  <input
                    type="text"
                    value={trailerData.supplierLastName}
                    onChange={(e) => setTrailerData({ ...trailerData, supplierLastName: e.target.value })}
                    placeholder="Enter shipping TM last name (max 12 chars, optional)"
                    maxLength={12}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Continue Button */}
                <div className="flex gap-2 pt-2">
                  <Button
                    onClick={() => setCurrentScreen(1)}
                    variant="secondary"
                    fullWidth
                  >
                    <i className="fa fa-arrow-left mr-2"></i>
                    Back
                  </Button>
                  <Button
                    onClick={handleTrailerContinue}
                    variant="success"
                    fullWidth
                    disabled={!trailerData.trailerNumber.trim() || !trailerData.driverFirstName.trim() || !trailerData.driverLastName.trim()}
                  >
                    <i className="fa fa-arrow-right mr-2"></i>
                    Continue to Skid Scanning
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {/* SCREEN 3: Skid Scanning (PLANNED/SCANNED Split) */}
          {currentScreen === 3 && (
            <>
              {/* Order Details - Collapsible */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className={`flex items-center justify-between cursor-pointer hover:bg-gray-50 transition-colors ${
                      expandedSection === 'order' ? 'p-4 rounded-t-lg' : 'p-2 rounded-lg'
                    }`}
                    onClick={() => setExpandedSection(expandedSection === 'order' ? null : 'order')}
                  >
                    <CardTitle className={expandedSection === 'order' ? 'text-sm' : 'text-xs'}>Shipment Details</CardTitle>
                    <i className={`fa fa-chevron-${expandedSection === 'order' ? 'up' : 'down'}`}></i>
                  </div>
                </CardHeader>
                {expandedSection === 'order' && (
                  <CardContent>
                    <div className="space-y-2 text-sm">
                      <div className="flex justify-between">
                        <span className="text-gray-600">Route Number:</span>
                        <span className="font-mono font-bold">{pickupRouteData?.routeNumber}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Plant:</span>
                        <span className="font-medium">{pickupRouteData?.plant}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Supplier:</span>
                        <span className="font-medium">{pickupRouteData?.supplierCode}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Dock:</span>
                        <span className="font-medium">{pickupRouteData?.dockCode}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Trailer:</span>
                        <span className="font-medium">{trailerData.trailerNumber}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Seal:</span>
                        <span className="font-medium">{trailerData.sealNumber}</span>
                      </div>
                    </div>
                  </CardContent>
                )}
              </Card>

              {/* Planned Orders - Collapsible (Grouped by Order) */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className={`flex items-center justify-between cursor-pointer hover:bg-gray-50 transition-colors ${
                      expandedSection === 'planned' ? 'p-4 rounded-t-lg' : 'p-2 rounded-lg'
                    }`}
                    onClick={() => setExpandedSection(expandedSection === 'planned' ? null : 'planned')}
                  >
                    <CardTitle className={expandedSection === 'planned' ? 'text-sm' : 'text-xs'}>
                      Planned Orders ({Object.keys(plannedSkids.reduce((acc, s) => ({ ...acc, [s.orderNumber]: true }), {})).length})
                    </CardTitle>
                    <i className={`fa fa-chevron-${expandedSection === 'planned' ? 'up' : 'down'}`}></i>
                  </div>
                </CardHeader>
                {expandedSection === 'planned' && (
                  <CardContent className="space-y-2">
                    {(() => {
                      // Group skids by orderNumber
                      const orderGroups = plannedSkids.reduce((acc, skid) => {
                        if (!acc[skid.orderNumber]) {
                          acc[skid.orderNumber] = [];
                        }
                        acc[skid.orderNumber].push(skid);
                        return acc;
                      }, {} as Record<string, PlannedSkid[]>);

                      // Helper function to toggle order expansion
                      const toggleOrder = (orderNum: string, e: React.MouseEvent) => {
                        e.stopPropagation(); // Prevent parent collapse
                        setExpandedOrders(prev => {
                          const newSet = new Set(prev);
                          if (newSet.has(`planned-${orderNum}`)) {
                            newSet.delete(`planned-${orderNum}`);
                          } else {
                            newSet.add(`planned-${orderNum}`);
                          }
                          return newSet;
                        });
                      };

                      return Object.entries(orderGroups).map(([orderNumber, skids]) => {
                        const dockCode = skids[0]?.dockCode || 'N/A';
                        const unscannedSkids = skids.filter(s => !s.isScanned);
                        const isOrderExpanded = expandedOrders.has(`planned-${orderNumber}`);

                        // Check if order has toyotaSkidBuildConfirmationNumber (dummy check - you need real data)
                        // For now, we assume all orders are ready unless explicitly marked
                        const isOrderReady = true; // TODO: Check actual confirmation number from backend

                        return (
                          <div key={orderNumber} className="border border-gray-200 rounded-lg overflow-hidden">
                            {/* Order Header - Collapsible */}
                            <div
                              className={`flex items-center justify-between p-3 cursor-pointer hover:bg-gray-50 transition-colors ${
                                !isOrderReady ? 'bg-red-50 border-red-300' : 'bg-white'
                              }`}
                              onClick={(e) => toggleOrder(orderNumber, e)}
                            >
                              <div className="flex items-center gap-2 flex-1">
                                <i className={`fa fa-chevron-${isOrderExpanded ? 'down' : 'right'} text-gray-500`}></i>
                                <div>
                                  <p className="text-sm font-bold" style={{ color: isOrderReady ? '#253262' : '#DC2626' }}>
                                    Order {orderNumber}
                                  </p>
                                  <p className="text-xs text-gray-600">
                                    Dock: <span className="font-semibold">{dockCode}</span> | Skids: {unscannedSkids.length}/{skids.length}
                                  </p>
                                  {!isOrderReady && (
                                    <p className="text-xs font-semibold text-red-600 mt-1">
                                      SKID BUILD NOT COMPLETE
                                    </p>
                                  )}
                                </div>
                              </div>
                              {!isOrderReady && (
                                <Badge variant="error">Blocked</Badge>
                              )}
                            </div>

                            {/* Order Skids - Nested Collapsible */}
                            {isOrderExpanded && isOrderReady && (
                              <div className="p-2 bg-gray-50 space-y-1">
                                {unscannedSkids.length === 0 ? (
                                  <p className="text-sm text-gray-500 text-center py-2">
                                    All skids scanned for this order
                                  </p>
                                ) : (
                                  unscannedSkids.map((skid) => (
                                    <div
                                      key={`${skid.skidNumber}-${skid.palletizationCode}`}
                                      className="p-2 bg-white border border-gray-200 rounded"
                                    >
                                      <p className="text-xs" style={{ color: '#253262' }}>
                                        {skid.palletizationCode && (
                                          <span>Pallet: <span className="font-semibold">{skid.palletizationCode}</span> | </span>
                                        )}
                                        Skid: <span className="font-semibold">{skid.skidNumber}</span>
                                      </p>
                                    </div>
                                  ))
                                )}
                              </div>
                            )}

                            {/* Blocked Order Message */}
                            {isOrderExpanded && !isOrderReady && (
                              <div className="p-3 bg-red-50 border-t border-red-200">
                                <p className="text-sm text-red-700">
                                  This order cannot be scanned. Please complete skid build first.
                                </p>
                              </div>
                            )}
                          </div>
                        );
                      });
                    })()}
                  </CardContent>
                )}
              </Card>

              {/* Scanned Orders - Collapsible (Grouped by Order) */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className={`flex items-center justify-between cursor-pointer hover:bg-gray-50 transition-colors ${
                      expandedSection === 'scanned' ? 'p-4 rounded-t-lg' : 'p-2 rounded-lg'
                    }`}
                    onClick={() => setExpandedSection(expandedSection === 'scanned' ? null : 'scanned')}
                  >
                    <CardTitle className={expandedSection === 'scanned' ? 'text-sm' : 'text-xs'}>
                      Scanned Orders ({Object.keys(scannedSkids.reduce((acc, s) => ({ ...acc, [s.orderNumber]: true }), {})).length})
                    </CardTitle>
                    <i className={`fa fa-chevron-${expandedSection === 'scanned' ? 'up' : 'down'}`}></i>
                  </div>
                </CardHeader>
                {expandedSection === 'scanned' && (
                  <CardContent className="space-y-2">
                    {scannedSkids.length === 0 ? (
                      <p className="text-sm text-gray-500 text-center py-4">
                        No skids scanned yet
                      </p>
                    ) : (
                      (() => {
                        // Group scanned skids by orderNumber
                        const scannedOrderGroups = scannedSkids.reduce((acc, skid) => {
                          if (!acc[skid.orderNumber]) {
                            acc[skid.orderNumber] = [];
                          }
                          acc[skid.orderNumber].push(skid);
                          return acc;
                        }, {} as Record<string, ScannedSkid[]>);

                        // Helper function to toggle scanned order expansion
                        const toggleScannedOrder = (orderNum: string, e: React.MouseEvent) => {
                          e.stopPropagation(); // Prevent parent collapse
                          setExpandedOrders(prev => {
                            const newSet = new Set(prev);
                            if (newSet.has(`scanned-${orderNum}`)) {
                              newSet.delete(`scanned-${orderNum}`);
                            } else {
                              newSet.add(`scanned-${orderNum}`);
                            }
                            return newSet;
                          });
                        };

                        return Object.entries(scannedOrderGroups).map(([orderNumber, skids]) => {
                          const dockCode = skids[0]?.dockCode || 'N/A';
                          const isOrderExpanded = expandedOrders.has(`scanned-${orderNumber}`);

                          return (
                            <div key={orderNumber} className="border-2 border-success-500 rounded-lg overflow-hidden bg-success-50">
                              {/* Order Header - Collapsible */}
                              <div
                                className="flex items-center justify-between p-3 cursor-pointer hover:bg-success-100 transition-colors bg-white"
                                onClick={(e) => toggleScannedOrder(orderNumber, e)}
                              >
                                <div className="flex items-center gap-2 flex-1">
                                  <i className={`fa fa-chevron-${isOrderExpanded ? 'down' : 'right'} text-success-600`}></i>
                                  <div>
                                    <p className="text-sm font-bold text-success-700">
                                      Order {orderNumber}
                                    </p>
                                    <p className="text-xs text-gray-600">
                                      Dock: <span className="font-semibold">{dockCode}</span> | Scanned: {skids.length}
                                    </p>
                                  </div>
                                </div>
                                <i className="fa fa-circle-check text-success-600 text-xl"></i>
                              </div>

                              {/* Order Scanned Skids - Nested Collapsible */}
                              {isOrderExpanded && (
                                <div className="p-2 bg-success-50 space-y-1">
                                  {skids.map((skid, idx) => (
                                    <div
                                      key={skid.id}
                                      className="p-2 bg-white border border-success-300 rounded"
                                    >
                                      <div className="flex items-center justify-between">
                                        <div className="flex-1">
                                          <p className="text-xs" style={{ color: '#253262' }}>
                                            {skid.palletizationCode && (
                                              <span>Pallet: <span className="font-semibold">{skid.palletizationCode}</span> | </span>
                                            )}
                                            Skid: <span className="font-semibold">{skid.skidId}</span>
                                          </p>
                                          <p className="text-xs text-gray-500 mt-1">
                                            <i className="fa fa-clock mr-1"></i>
                                            {new Date(skid.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })}
                                          </p>
                                        </div>
                                        <span className="text-xs font-medium text-success-700">#{idx + 1}</span>
                                      </div>
                                    </div>
                                  ))}
                                </div>
                              )}
                            </div>
                          );
                        });
                      })()
                    )}
                  </CardContent>
                )}
              </Card>

              {/* Exceptions List - Collapsible */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className={`flex items-center justify-between cursor-pointer hover:bg-gray-50 transition-colors ${
                      expandedSection === 'exceptions' ? 'p-4 rounded-t-lg' : 'p-2 rounded-lg'
                    }`}
                    onClick={() => setExpandedSection(expandedSection === 'exceptions' ? null : 'exceptions')}
                  >
                    <CardTitle className={expandedSection === 'exceptions' ? 'text-sm' : 'text-xs'}>
                      Exceptions ({exceptions.length})
                    </CardTitle>
                    <i className={`fa fa-chevron-${expandedSection === 'exceptions' ? 'up' : 'down'}`}></i>
                  </div>
                </CardHeader>
                {expandedSection === 'exceptions' && (
                  <CardContent className="space-y-2">
                    {exceptions.length === 0 ? (
                      <p className="text-sm text-gray-500 text-center py-4">
                        No exceptions added
                      </p>
                    ) : (
                      exceptions.map((exception, idx) => (
                        <div
                          key={idx}
                          className="p-3 bg-warning-50 border border-warning-200 rounded-lg"
                        >
                          <div className="flex items-start justify-between gap-2">
                            <div className="flex-1">
                              <div className="flex items-center gap-2 mb-1">
                                <Badge variant="warning">
                                  {exception.level === 'trailer' ? 'Trailer' : 'Skid'} - Code {exception.code}
                                </Badge>
                                <span className="text-xs text-gray-500">
                                  {new Date(exception.timestamp).toLocaleString()}
                                </span>
                              </div>
                              <p className="text-xs text-gray-600 mb-1">
                                <span className="font-semibold">Type:</span> {exception.type}
                              </p>
                              {exception.level === 'skid' && exception.relatedSkidId && (
                                <p className="text-xs text-gray-600 mb-1">
                                  <span className="font-semibold">Skid:</span> {exception.relatedSkidId}
                                </p>
                              )}
                              <p className="text-sm text-gray-700">{exception.comments}</p>
                            </div>
                            <button
                              onClick={() => handleRemoveException(idx)}
                              className="text-error-600 hover:text-error-700 p-1"
                              aria-label="Remove exception"
                            >
                              <i className="fa fa-trash"></i>
                            </button>
                          </div>
                        </div>
                      ))
                    )}
                  </CardContent>
                )}
              </Card>

              {/* Scan Skid Manifest - ALWAYS VISIBLE */}
              <div className="space-y-3">
                <Scanner
                  onScan={handleSkidScan}
                  label="Scan Toyota Manifest"
                  placeholder="Scan Toyota Manifest Label"
                />
              </div>

              {/* Action Buttons */}
              <div className="flex gap-2">
                <Button
                  onClick={() => setShowExceptionModal(true)}
                  variant="warning"
                  fullWidth
                >
                  <i className="fa fa-exclamation-triangle mr-2"></i>
                  Add Exception
                </Button>
              </div>

              {/* Submit Button */}
              <div className="flex gap-2">
                <Button
                  onClick={() => setCurrentScreen(2)}
                  variant="secondary"
                  fullWidth
                >
                  <i className="fa fa-arrow-left mr-2"></i>
                  Back
                </Button>
                <Button
                  onClick={handleFinalSubmit}
                  variant="success"
                  fullWidth
                  loading={loading}
                  disabled={
                    !(plannedSkids.every(skid => skid.isScanned) ||
                      (scannedSkids.length > 0 &&
                        plannedSkids.filter(skid => !skid.isScanned).every(skid => {
                          const compositeKey = `${skid.skidNumber}-${skid.palletizationCode}`;
                          return exceptions.some(e => e.relatedSkidId === compositeKey);
                        })
                      ))
                  }
                >
                  <i className="fa fa-paper-plane mr-2"></i>
                  Submit Shipment
                </Button>
              </div>
            </>
          )}

          {/* SCREEN 4: Success/Confirmation Display - SIMPLIFIED */}
          {/* Author: Hassan, 2025-11-05 */}
          {/* Updated: 2025-11-05 - Simplified to match TSCS popup-style confirmation */}
          {/* Updated: 2025-11-05 - Changed button colors and added Back to Dashboard */}
          {currentScreen === 4 && (
            <div className="flex items-center justify-center min-h-[calc(100vh-140px)]">
              {/* Compact Success Card - Modal Style */}
              <Card className="max-w-md w-full shadow-2xl">
                <CardContent className="p-8 text-center space-y-6">
                  {/* Large Success Icon */}
                  <div className="flex justify-center">
                    <div className="inline-flex items-center justify-center w-24 h-24 rounded-full bg-success-100">
                      <i className="fa-light fa-circle-check text-6xl text-success-600"></i>
                    </div>
                  </div>

                  {/* Success Message */}
                  <div>
                    <h1 className="text-2xl font-bold mb-2" style={{ color: '#253262' }}>
                      Submitted Successfully
                    </h1>
                  </div>

                  {/* Confirmation Number Display */}
                  <div className="py-4">
                    <p className="text-sm text-gray-600 mb-3">Confirmation Number</p>
                    <div className="bg-gray-50 px-6 py-4 rounded-lg border-2 border-gray-300">
                      <span className="font-mono text-2xl font-bold select-all" style={{ color: '#253262' }}>
                        {confirmationNumber}
                      </span>
                    </div>
                  </div>

                  {/* Action Buttons - Stacked Vertically */}
                  <div className="pt-4 space-y-3">
                    {/* Primary Action: Continue - VUTEQ Navy */}
                    <Button
                      onClick={handleNewShipment}
                      variant="primary"
                      fullWidth
                      className="py-3 text-base font-semibold"
                      style={{ backgroundColor: '#253262', color: 'white' }}
                    >
                      <i className="fa fa-plus mr-2"></i>
                      CONTINUE
                    </Button>

                    {/* Secondary Action: Back to Dashboard */}
                    <Button
                      onClick={() => router.push('/')}
                      variant="secondary"
                      fullWidth
                      className="py-3 text-base font-semibold"
                    >
                      <i className="fa fa-home mr-2"></i>
                      BACK TO DASHBOARD
                    </Button>
                  </div>
                </CardContent>
              </Card>
            </div>
          )}

          {/* Global Action Buttons - Hide on Success Screen */}
          {currentScreen !== 4 && (
            <div className="flex gap-2">
              <Button
                onClick={() => router.push('/')}
                variant="error"
                fullWidth
              >
                <i className="fa fa-xmark mr-2"></i>
                Cancel
              </Button>
              {currentScreen > 1 && (
                <Button
                  onClick={handleReset}
                  variant="primary"
                  fullWidth
                >
                  <i className="fa fa-rotate-right mr-2"></i>
                  Start Over
                </Button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* FAB Menu (Bottom Left) - Visible on Screens 2-3 */}
      {currentScreen >= 2 && currentScreen <= 3 && (
        <div className="fixed bottom-20 left-4 z-50">
          {/* FAB Toggle Button */}
          <button
            onClick={() => setFabMenuOpen(!fabMenuOpen)}
            className="w-14 h-14 rounded-full bg-[#253262] text-white shadow-lg hover:bg-[#1a2347] transition-colors flex items-center justify-center"
          >
            <i className={`fa-light ${fabMenuOpen ? 'fa-xmark' : 'fa-ellipsis-vertical'} text-xl`}></i>
          </button>

          {/* Popup Menu Card */}
          {fabMenuOpen && (
            <div className="absolute bottom-16 left-0 bg-white rounded-lg shadow-xl py-1 min-w-[240px] border border-gray-200">
              {/* Save Draft - ALL SCREENS */}
              <button
                onClick={() => {
                  handleSaveDraft();
                  setFabMenuOpen(false);
                }}
                className="flex items-center gap-3 px-4 py-3 w-full hover:bg-gray-50 transition-colors text-left"
              >
                <i className="fa-light fa-cloud-arrow-up text-lg text-blue-500"></i>
                <span className="text-sm font-medium text-gray-700">Save Draft</span>
              </button>

              {/* Unpick All - Screen 3 Only */}
              <button
                onClick={() => {
                  handleUnpickAll();
                  setFabMenuOpen(false);
                }}
                disabled={currentScreen !== 3 || scannedSkids.length === 0}
                className={`flex items-center gap-3 px-4 py-3 w-full transition-colors text-left ${
                  currentScreen === 3 && scannedSkids.length > 0
                    ? 'hover:bg-gray-50 cursor-pointer'
                    : 'opacity-50 cursor-not-allowed bg-gray-50'
                }`}
              >
                <i className={`fa-light fa-trash text-lg ${
                  currentScreen === 3 && scannedSkids.length > 0 ? 'text-red-500' : 'text-gray-400'
                }`}></i>
                <span className="text-sm font-medium text-gray-700">Unpick All</span>
              </button>

              {/* Rack Exception Toggle - Screen 3 Only */}
              {/* Author: Hassan, 2025-11-05 */}
              <button
                onClick={() => {
                  if (currentScreen === 3) {
                    setRackExceptionEnabled(!rackExceptionEnabled);
                  }
                }}
                disabled={currentScreen !== 3}
                className={`flex items-center justify-between gap-3 px-4 py-3 w-full transition-colors text-left ${
                  currentScreen === 3
                    ? 'hover:bg-gray-50 cursor-pointer'
                    : 'opacity-50 cursor-not-allowed bg-gray-50'
                }`}
              >
                <div className="flex items-center gap-3">
                  <i className={`fa-light fa-warehouse text-lg ${
                    currentScreen === 3 ? 'text-purple-500' : 'text-gray-400'
                  }`}></i>
                  <span className="text-sm font-medium text-gray-700">Rack Exception</span>
                </div>
                {currentScreen === 3 && (
                  <Badge variant={rackExceptionEnabled ? 'success' : 'default'}>
                    {rackExceptionEnabled ? 'ON' : 'OFF'}
                  </Badge>
                )}
              </button>
            </div>
          )}
        </div>
      )}

      {/* Exception Modal - Popup Dialog */}
      {showExceptionModal && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          onClick={() => {
            setShowExceptionModal(false);
            setSelectedExceptionType('');
            setSelectedExceptionCode('');
            setExceptionComments('');
            setSelectedSkidForException('');
            setExceptionLevel('trailer');
          }}
        >
          <div
            className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Modal Header */}
            <div className="flex items-center justify-between p-4 border-b border-gray-200">
              <h3 className="text-lg font-bold" style={{ color: '#253262' }}>
                Add Exception
              </h3>
              <button
                onClick={() => {
                  setShowExceptionModal(false);
                  setSelectedExceptionType('');
                  setSelectedExceptionCode('');
                  setExceptionComments('');
                  setSelectedSkidForException('');
                  setExceptionLevel('trailer');
                }}
                className="text-gray-400 hover:text-gray-600 transition-colors"
                aria-label="Close modal"
              >
                <i className="fa-light fa-xmark text-2xl"></i>
              </button>
            </div>

            {/* Modal Content */}
            <div className="p-6 space-y-4">
              {/* Exception Level Selection */}
              <div>
                <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                  Exception Level *
                </label>
                <div className="flex gap-4">
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="exceptionLevel"
                      value="trailer"
                      checked={exceptionLevel === 'trailer'}
                      onChange={(e) => {
                        setExceptionLevel('trailer');
                        setSelectedExceptionType('');
                        setSelectedExceptionCode('');
                        setSelectedSkidForException('');
                      }}
                      className="w-4 h-4 text-primary-600"
                    />
                    <span className="text-sm">Trailer Level</span>
                  </label>
                  <label className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="radio"
                      name="exceptionLevel"
                      value="skid"
                      checked={exceptionLevel === 'skid'}
                      onChange={(e) => {
                        setExceptionLevel('skid');
                        setSelectedExceptionType('');
                        setSelectedExceptionCode('');
                      }}
                      className="w-4 h-4 text-primary-600"
                    />
                    <span className="text-sm">Skid Level</span>
                  </label>
                </div>
              </div>

              {/* Exception Type Dropdown */}
              <div>
                <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                  Exception Type *
                </label>
                <select
                  value={selectedExceptionType}
                  onChange={(e) => {
                    const selected = e.target.value;
                    setSelectedExceptionType(selected);
                    // Find and set the exception code
                    const exceptionList = exceptionLevel === 'trailer' ? TRAILER_EXCEPTION_TYPES : SKID_EXCEPTION_TYPES;
                    const found = exceptionList.find(ex => ex.label === selected);
                    setSelectedExceptionCode(found?.code || '');
                  }}
                  className="w-full px-3 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
                  style={{ backgroundColor: '#FCFCFC' }}
                >
                  <option value="">Select exception type...</option>
                  {(exceptionLevel === 'trailer' ? TRAILER_EXCEPTION_TYPES : SKID_EXCEPTION_TYPES).map((ex) => (
                    <option key={ex.code} value={ex.label}>
                      {ex.label} (Code: {ex.code})
                    </option>
                  ))}
                </select>
              </div>

              {/* Which Skid Dropdown - ONLY for Skid Level Exceptions */}
              {exceptionLevel === 'skid' && (
                <div>
                  <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                    Which Skid? *
                  </label>
                  <select
                    value={selectedSkidForException}
                    onChange={(e) => setSelectedSkidForException(e.target.value)}
                    className="w-full px-3 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
                    style={{ backgroundColor: '#FCFCFC' }}
                  >
                    <option value="">Select unscanned skid...</option>
                    {plannedSkids
                      .filter(skid => !skid.isScanned)
                      .map((skid) => (
                        <option key={`${skid.skidNumber}-${skid.palletizationCode}`} value={`${skid.skidNumber}-${skid.palletizationCode}`}>
                          Skid {skid.skidId} (Pallet: {skid.palletizationCode || 'N/A'}) - Order {skid.orderNumber}
                        </option>
                      ))}
                  </select>
                </div>
              )}

              {/* Comments Textarea */}
              <div>
                <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                  Comments *
                </label>
                <div className="space-y-1">
                  <textarea
                    value={exceptionComments}
                    onChange={(e) => setExceptionComments(e.target.value)}
                    placeholder="Describe the reason for the exception..."
                    rows={4}
                    maxLength={100}
                    className="w-full px-3 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 resize-none text-sm"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                  <div className="text-xs text-gray-500 text-right">
                    {exceptionComments.length} / 100 characters
                  </div>
                </div>
              </div>
            </div>

            {/* Modal Footer */}
            <div className="flex gap-2 p-4 border-t border-gray-200">
              <Button
                onClick={() => {
                  setShowExceptionModal(false);
                  setSelectedExceptionType('');
                  setSelectedExceptionCode('');
                  setExceptionComments('');
                  setSelectedSkidForException('');
                  setExceptionLevel('trailer');
                }}
                variant="secondary"
                fullWidth
              >
                <i className="fa-light fa-xmark mr-2"></i>
                Cancel
              </Button>
              <Button
                onClick={handleAddException}
                variant="warning"
                fullWidth
                disabled={
                  !selectedExceptionType ||
                  !selectedExceptionCode ||
                  !exceptionComments.trim() ||
                  (exceptionLevel === 'skid' && !selectedSkidForException)
                }
              >
                <i className="fa-light fa-plus mr-2"></i>
                Add Exception
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
