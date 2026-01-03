/**
 * Pre-Shipment Scan Page - Single Screen Workflow with API Integration
 * Author: Hassan
 * Date: 2025-11-05
 * Updated: 2025-12-31 - Integrated with backend API (replaced localStorage with API calls)
 * Updated: 2025-12-31 - Added sessionId-based state management
 * Updated: 2025-12-31 - plannedSkids now come from create-from-manifest API response
 * Updated: 2025-12-31 - FIXED Screen Flow per requirements (removed Driver Checksheet from Pre-Shipment)
 * Updated: 2025-12-31 - CRITICAL FIX: Match skids EXACTLY like shipment-load (orderNumber + dockCode + palletizationCode + skidId)
 * Updated: 2026-01-03 - BUG FIX: First manifest now auto-scans when creating new session (was showing 0/13 instead of 1/13)
 * Updated: 2026-01-03 - BUG FIX: Resumed sessions now restore ALL previously scanned skids from API response
 * Updated: 2026-01-03 - UI REDESIGN: Single screen design with summary as modal popup (removed stepper)
 *
 * SCREEN FLOW (REDESIGNED):
 * Screen 1: Main Scanning Screen - Scan manifests, view sessions, resume/delete sessions
 *   - When resuming a completed session (all skids scanned), shows summary as MODAL instead of navigating
 *   - Summary modal shows route, supplier, orders, skid counts with "Close" and "Continue" buttons
 * Screen 2: Summary & Save - Show all orders/skids, option to "Save & Exit" or "Enter Trailer Info"
 * Screen 3: Trailer Information (OPTIONAL) - Can skip if driver not arrived yet
 * Screen 4: Additional Scans (if needed) - For any remaining skids
 * Screen 5: Success Screen - Shows Toyota confirmation number
 *
 * API Integration:
 * - POST /api/v1/pre-shipment/create-from-manifest - Creates session from FIRST manifest scan
 * - POST /api/v1/pre-shipment/{sessionId}/scan-skid - Scan SUBSEQUENT manifests (not first)
 * - GET /api/v1/pre-shipment/list - List all sessions
 * - GET /api/v1/pre-shipment/{sessionId} - Get session details (for resume)
 * - PUT /api/v1/pre-shipment/{sessionId}/trailer-info - Update trailer/driver info (OPTIONAL)
 * - POST /api/v1/pre-shipment/{sessionId}/complete - Complete and submit to Toyota
 * - DELETE /api/v1/pre-shipment/{sessionId} - Delete incomplete session
 *
 * CRITICAL FIX (2025-12-31):
 * - UNIQUE KEY for each QR: orderNumber + dockCode + palletizationCode + skidId
 * - The combination of palletizationCode + skidId makes each manifest unique within an order
 * - Example: A4+001A is different from A8+001A even though both have skidId "001A"
 * - Matching logic now uses: orderNumber + dockCode + skidNumber (first 3 chars) + palletizationCode
 * - This EXACTLY matches the shipment-load implementation (3rd screen manifest scanning)
 */

'use client';

import { useState, useEffect } from 'react';
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
  createPreShipmentFromManifest,
  getPreShipmentList,
  getPreShipmentSession,
  scanPreShipmentSkid,
  updatePreShipmentTrailerInfo,
  completePreShipment,
  deletePreShipment,
  type PreShipmentListItem,
  type PreShipmentPlannedSkid,
  type PreShipmentOrder,
} from '@/lib/api';

// Screen types
type Screen = 1 | 2 | 3 | 4 | 5;

// Interfaces
interface PlannedSkid extends PreShipmentPlannedSkid {
  // API provides: skidId, orderNumber, dockCode, palletizationCode, skidNumber, skidSide, partCount, isScanned
}

interface ScannedSkid {
  id: string;
  skidId: string;
  orderNumber: string;
  dockCode: string;
  palletizationCode: string;
  partCount: number;
  destination: string;
  timestamp: string;
}

interface Exception {
  type: string;
  comments: string;
  relatedSkidId: string;
  timestamp: string;
}

// Session data interface
interface SessionData {
  sessionId: string;
  routeNumber: string;
  supplierCode: string;
  dockCode: string;
  orders: PreShipmentOrder[];
  plannedSkids: PlannedSkid[];
}

// Toyota-specific exception types
const EXCEPTION_TYPES = [
  'Revised Quantity (Toyota Quantity Reduction)',
  'Modified Quantity per Box',
  'Supplier Revised Shortage (Short Shipment)',
  'Non-Standard Packaging (Expendable)',
];

export default function PreShipmentScanPage() {
  const router = useRouter();
  const { user } = useAuth();
  const [currentScreen, setCurrentScreen] = useState<Screen>(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Session management (API-based)
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [sessionData, setSessionData] = useState<SessionData | null>(null);
  const [sessions, setSessions] = useState<PreShipmentListItem[]>([]);
  const [confirmationNumber, setConfirmationNumber] = useState<string | null>(null);

  // Screen 2: Trailer Information Form
  const [trailerNumber, setTrailerNumber] = useState<string>('');
  const [sealNumber, setSealNumber] = useState<string>('');
  const [driverFirstName, setDriverFirstName] = useState<string>('');
  const [driverLastName, setDriverLastName] = useState<string>('');
  const [supplierFirstName, setSupplierFirstName] = useState<string>('');
  const [supplierLastName, setSupplierLastName] = useState<string>('');

  // Screen 4: Loading/Scanning state
  const [plannedSkids, setPlannedSkids] = useState<PlannedSkid[]>([]);
  const [scannedSkids, setScannedSkids] = useState<ScannedSkid[]>([]);
  const [expandedSection, setExpandedSection] = useState<'planned' | 'scanned' | 'exceptions' | 'progress' | null>(null);

  // Exceptions Data
  const [exceptions, setExceptions] = useState<Exception[]>([]);
  const [showExceptionModal, setShowExceptionModal] = useState(false);
  const [selectedExceptionType, setSelectedExceptionType] = useState('');
  const [exceptionComments, setExceptionComments] = useState('');
  const [selectedSkidForException, setSelectedSkidForException] = useState('');

  // Summary Modal State
  const [showSummaryModal, setShowSummaryModal] = useState(false);

  // Load all sessions on mount
  useEffect(() => {
    loadSessions();
  }, []);

  const loadSessions = async () => {
    const result = await getPreShipmentList();
    if (result.success && result.data) {
      setSessions(result.data);
    } else {
      setError(result.error || 'Failed to load sessions');
    }
  };


  // SCREEN 1: Handle Manifest Scan - Creates session from FIRST manifest, scans subsequent ones
  // Updated: 2025-12-31 - FIXED to match shipment-load exactly: orderNumber + dockCode + palletizationCode + skidId
  const handleManifestScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    // Accept both SKID_MANIFEST and TOYOTA_KANBAN scan types
    if (result.validatedType !== 'SKID_MANIFEST' && result.validatedType !== 'TOYOTA_KANBAN') {
      setError(`Invalid scan type. Expected SKID_MANIFEST or TOYOTA_KANBAN, but got ${result.validatedType}`);
      return;
    }

    if (!user) {
      setError('User not authenticated');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // Parse manifest to get ALL fields (not just skidId)
      const manifestBarcode = result.scannedValue;
      const manifest = parseToyotaManifest(manifestBarcode);

      if (!manifest) {
        setError('Invalid Toyota Manifest. Could not parse barcode.');
        setLoading(false);
        return;
      }

      // Check if this is the first scan (no session yet)
      if (!sessionId) {
        // FIRST SCAN: Create session from manifest
        const apiResult = await createPreShipmentFromManifest(manifestBarcode, user.id);

        if (!apiResult.success || !apiResult.data) {
          setError(apiResult.error || 'Failed to create session from manifest');
          setLoading(false);
          return;
        }

        const data = apiResult.data;

        // Set session data
        setSessionId(data.sessionId);
        setSessionData({
          sessionId: data.sessionId,
          routeNumber: data.routeNumber,
          supplierCode: data.supplierCode,
          dockCode: data.dockCode,
          orders: data.orders,
          plannedSkids: data.plannedSkids,
        });
        setPlannedSkids(data.plannedSkids);

        // Restore previously scanned skids from API response
        const previouslyScanned = data.plannedSkids
          .filter(s => s.isScanned)
          .map((s, idx) => ({
            id: `restored-${idx}-${Date.now()}`,
            skidId: s.skidId,
            orderNumber: s.orderNumber,
            dockCode: s.dockCode,
            palletizationCode: s.palletizationCode,
            partCount: s.partCount,
            destination: `Dock ${s.dockCode}`,
            timestamp: new Date().toISOString(),
          }));

        // If any skids already scanned, this is a resumed session
        if (previouslyScanned.length > 0) {
          console.log('=== RESUMED SESSION ===');
          console.log('Restoring', previouslyScanned.length, 'previously scanned skids');
          setScannedSkids(previouslyScanned);

          // Now scan the newly scanned manifest
          const scannedSkidNumber = manifest.skidId.substring(0, 3);

          console.log('=== SCANNING NEW MANIFEST IN RESUMED SESSION ===');
          console.log('New manifest:');
          console.log('  orderNumber:', manifest.orderNumber);
          console.log('  dockCode:', manifest.dockCode);
          console.log('  skidId:', manifest.skidId);
          console.log('  skidNumber:', scannedSkidNumber);
          console.log('  palletizationCode:', manifest.palletizationCode);

          // Find the matching planned skid
          const plannedSkid = data.plannedSkids.find(
            skid => skid.orderNumber === manifest.orderNumber &&
                    skid.dockCode === manifest.dockCode &&
                    skid.skidNumber === scannedSkidNumber &&
                    skid.palletizationCode === manifest.palletizationCode
          );

          if (!plannedSkid) {
            setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} not found in planned list.`);
            setLoading(false);
            return;
          }

          // Check if already scanned
          const alreadyScanned = previouslyScanned.some(
            item => {
              const itemSkidNumber = item.skidId.substring(0, 3);
              return item.orderNumber === manifest.orderNumber &&
                     item.dockCode === manifest.dockCode &&
                     itemSkidNumber === scannedSkidNumber &&
                     item.palletizationCode === manifest.palletizationCode;
            }
          );

          if (alreadyScanned) {
            setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} has already been scanned.`);
            setLoading(false);
            return;
          }

          // Call scan-skid API to mark it as scanned
          const scanResult = await scanPreShipmentSkid({
            sessionId: data.sessionId,
            skidId: plannedSkid.skidId,
            palletizationCode: manifest.palletizationCode,
            orderNumber: manifest.orderNumber,
            dockCode: manifest.dockCode,
            scannedBy: user.id,
          });

          if (scanResult.success) {
            // Add to UI state
            const newScannedSkid: ScannedSkid = {
              id: `${Date.now()}-${Math.random()}`,
              skidId: plannedSkid.skidId,
              orderNumber: plannedSkid.orderNumber,
              dockCode: plannedSkid.dockCode,
              palletizationCode: plannedSkid.palletizationCode,
              partCount: plannedSkid.partCount,
              destination: `Dock ${plannedSkid.dockCode}`,
              timestamp: new Date().toISOString(),
            };

            setPlannedSkids(data.plannedSkids.map(skid =>
              skid.skidId === plannedSkid.skidId ? { ...skid, isScanned: true } : skid
            ));
            setScannedSkids([...previouslyScanned, newScannedSkid]);

            console.log('New manifest scanned successfully in resumed session!');
          } else {
            console.error('Failed to scan new manifest:', scanResult.error);
            setError(scanResult.error || 'Failed to record manifest scan');
          }

          console.log('======================');
        } else {
          // NEW SESSION: Scan the first manifest that created the session
          // Extract skidNumber from skidId (first 3 chars: "001A" → "001")
          const scannedSkidNumber = manifest.skidId.substring(0, 3);

          console.log('=== AUTO-SCANNING FIRST MANIFEST ===');
          console.log('First manifest:');
          console.log('  orderNumber:', manifest.orderNumber);
          console.log('  dockCode:', manifest.dockCode);
          console.log('  skidId:', manifest.skidId);
          console.log('  skidNumber:', scannedSkidNumber);
          console.log('  palletizationCode:', manifest.palletizationCode);

          // Find the matching planned skid (same logic as subsequent scans)
          const plannedSkid = data.plannedSkids.find(
            skid => skid.orderNumber === manifest.orderNumber &&
                    skid.dockCode === manifest.dockCode &&
                    skid.skidNumber === scannedSkidNumber &&
                    skid.palletizationCode === manifest.palletizationCode
          );

          if (plannedSkid) {
            console.log('Found matching planned skid:', plannedSkid);

            // Call scan-skid API to mark it as scanned
            const scanResult = await scanPreShipmentSkid({
              sessionId: data.sessionId,
              skidId: plannedSkid.skidId,
              palletizationCode: manifest.palletizationCode,
              orderNumber: manifest.orderNumber,
              dockCode: manifest.dockCode,
              scannedBy: user.id,
            });

            if (scanResult.success) {
              // Update UI state
              const firstScannedSkid: ScannedSkid = {
                id: `${Date.now()}-${Math.random()}`,
                skidId: plannedSkid.skidId,
                orderNumber: plannedSkid.orderNumber,
                dockCode: plannedSkid.dockCode,
                palletizationCode: plannedSkid.palletizationCode,
                partCount: plannedSkid.partCount,
                destination: `Dock ${plannedSkid.dockCode}`,
                timestamp: new Date().toISOString(),
              };

              setPlannedSkids(data.plannedSkids.map(skid =>
                skid.skidId === plannedSkid.skidId ? { ...skid, isScanned: true } : skid
              ));
              setScannedSkids([firstScannedSkid]);

              console.log('First manifest scanned successfully!');
            } else {
              console.error('Failed to scan first manifest:', scanResult.error);
              setError(scanResult.error || 'Failed to record first manifest scan');
            }
          } else {
            console.error('Could not find matching planned skid for first manifest');
          }

          console.log('======================');
        }

        // STAY ON SCREEN 1 - User can scan more manifests
        setLoading(false);
      } else {
        // SUBSEQUENT SCANS: Use scan-skid API
        // CRITICAL: Extract skidNumber from skidId (first 3 chars: "001A" → "001")
        const scannedSkidNumber = manifest.skidId.substring(0, 3);

        console.log('=== MANIFEST SCAN MATCHING ===');
        console.log('Scanned manifest:');
        console.log('  orderNumber:', manifest.orderNumber);
        console.log('  dockCode:', manifest.dockCode);
        console.log('  skidId:', manifest.skidId);
        console.log('  skidNumber:', scannedSkidNumber);
        console.log('  palletizationCode:', manifest.palletizationCode);

        // EXACT MATCH from shipment-load: orderNumber + dockCode + skidNumber + palletizationCode
        const plannedSkid = plannedSkids.find(
          skid => skid.orderNumber === manifest.orderNumber &&
                  skid.dockCode === manifest.dockCode &&
                  skid.skidNumber === scannedSkidNumber &&
                  skid.palletizationCode === manifest.palletizationCode
        );

        if (!plannedSkid) {
          setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} not found in planned list.`);
          setLoading(false);
          return;
        }

        // Check if already scanned - EXACT MATCH from shipment-load
        const alreadyScanned = scannedSkids.some(
          item => {
            const itemSkidNumber = item.skidId.substring(0, 3);
            return item.orderNumber === manifest.orderNumber &&
                   item.dockCode === manifest.dockCode &&
                   itemSkidNumber === scannedSkidNumber &&
                   item.palletizationCode === manifest.palletizationCode;
          }
        );

        if (alreadyScanned) {
          setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} has already been scanned.`);
          setLoading(false);
          return;
        }

        console.log('Found planned skid:', plannedSkid);
        console.log('======================');

        // Call scan-skid API with individual fields
        const scanResult = await scanPreShipmentSkid({
          sessionId,
          skidId: plannedSkid.skidId,
          palletizationCode: manifest.palletizationCode,
          orderNumber: manifest.orderNumber,
          dockCode: manifest.dockCode,
          scannedBy: user.id,
        });

        if (!scanResult.success) {
          setError(scanResult.error || 'Failed to record scan');
          setLoading(false);
          return;
        }

        // Update UI state
        const newScannedSkid: ScannedSkid = {
          id: `${Date.now()}-${Math.random()}`,
          skidId: plannedSkid.skidId,
          orderNumber: plannedSkid.orderNumber,
          dockCode: plannedSkid.dockCode,
          palletizationCode: plannedSkid.palletizationCode,
          partCount: plannedSkid.partCount,
          destination: `Dock ${plannedSkid.dockCode}`,
          timestamp: new Date().toISOString(),
        };

        setPlannedSkids(plannedSkids.map(skid =>
          skid.skidId === plannedSkid.skidId ? { ...skid, isScanned: true } : skid
        ));
        setScannedSkids([...scannedSkids, newScannedSkid]);
        setLoading(false);
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to process scan');
      setLoading(false);
    }
  };

  // SCREEN 1: Resume session
  const handleResumeSession = async (session: PreShipmentListItem) => {
    setLoading(true);
    setError(null);

    try {
      const result = await getPreShipmentSession(session.sessionId);

      if (!result.success || !result.data) {
        setError(result.error || 'Failed to load session');
        setLoading(false);
        return;
      }

      const data = result.data;

      setSessionId(data.sessionId);
      setSessionData({
        sessionId: data.sessionId,
        routeNumber: data.routeNumber,
        supplierCode: data.supplierCode,
        dockCode: data.dockCode,
        orders: data.orders,
        plannedSkids: data.plannedSkids,
      });
      setPlannedSkids(data.plannedSkids);

      // Restore scanned skids
      const alreadyScanned = data.plannedSkids.filter(s => s.isScanned);
      const scanned: ScannedSkid[] = alreadyScanned.map((s, idx) => ({
        id: `resumed-${idx}`,
        skidId: s.skidId,
        orderNumber: s.orderNumber,
        dockCode: s.dockCode,
        palletizationCode: s.palletizationCode,
        partCount: s.partCount,
        destination: `Dock ${s.dockCode}`,
        timestamp: new Date().toISOString(),
      }));
      setScannedSkids(scanned);

      // Check if ALL skids are scanned
      const allSkidsScanned = data.plannedSkids.every(s => s.isScanned);

      if (allSkidsScanned) {
        // Show summary modal instead of navigating
        setShowSummaryModal(true);
      }
      // Otherwise, stay on Screen 1 (already on Screen 1)

      setLoading(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to resume session');
      setLoading(false);
    }
  };

  // SCREEN 1: Delete session
  const handleDeleteSession = async (sessionIdToDelete: string) => {
    const confirmed = window.confirm('Are you sure you want to delete this session?');
    if (!confirmed) return;

    setLoading(true);
    const result = await deletePreShipment(sessionIdToDelete);

    if (result.success) {
      await loadSessions();
      if (sessionId === sessionIdToDelete) {
        setSessionId(null);
        setSessionData(null);
      }
    } else {
      setError(result.error || 'Failed to delete session');
    }

    setLoading(false);
  };

  // SCREEN 2: Save session and exit (for driver to complete later via Shipment Load)
  const handleSaveAndExit = () => {
    // Session is already saved in the database
    // Just return to home
    router.push('/');
  };

  // SCREEN 2: Continue to Screen 3 (Trailer Info - OPTIONAL)
  const handleContinueToTrailerInfo = () => {
    if (!sessionData) {
      setError('Session not initialized');
      return;
    }
    setCurrentScreen(3);
  };

  // SCREEN 3: Save trailer info and continue to Screen 4 (trailer info is OPTIONAL now)
  const handleContinueToScanning = async () => {
    if (!sessionId || !user) {
      setError('Session not initialized');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // Only save trailer info if user entered it
      if (trailerNumber.trim() && driverFirstName.trim() && driverLastName.trim()) {
        const result = await updatePreShipmentTrailerInfo({
          sessionId,
          trailerNumber,
          sealNumber,
          driverFirstName,
          driverLastName,
          supplierFirstName,
          supplierLastName,
        });

        if (!result.success) {
          setError(result.error || 'Failed to save trailer info');
          setLoading(false);
          return;
        }
      }

      // Move to Screen 4 (additional scans if needed)
      setCurrentScreen(4);
      setLoading(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save trailer info');
      setLoading(false);
    }
  };

  // SCREEN 3: Skip trailer info and go directly to Screen 4
  const handleSkipTrailerInfo = () => {
    setCurrentScreen(4);
  };

  /**
   * Parsed Manifest Interface - Matches shipment-load implementation
   * Author: Hassan, 2025-12-31
   */
  interface ParsedManifest {
    plantCode: string;
    supplierCode: string;
    dockCode: string;
    orderNumber: string;
    loadId: string;
    palletizationCode: string;  // "A4", "A8", "D1" - positions 36-38
    mros: string;               // "34" - positions 38-40
    skidId: string;             // "001A" - positions 40-44
  }

  /**
   * Parse Toyota Manifest QR Code (44 characters)
   * Extract ALL individual fields (not just skidId)
   *
   * Toyota Manifest Structure (44 chars):
   * - 0-5: Plant Code
   * - 5-10: Supplier Code
   * - 10-12: Dock Code
   * - 12-24: Order Number
   * - 24-36: Load ID
   * - 36-38: Palletization Code (CRITICAL - used for matching!)
   * - 38-40: MROS
   * - 40-44: Skid ID (4 chars, e.g., "001A")
   *
   * Updated: 2025-12-31 - Return full ParsedManifest object instead of just skidId
   */
  const parseToyotaManifest = (qr: string): ParsedManifest | null => {
    if (qr.length < 44) {
      console.error('Invalid Toyota Manifest - expected 44 characters, got:', qr.length);
      return null;
    }

    try {
      const plantCode = qr.substring(0, 5).trim();           // Positions 0-5
      const supplierCode = qr.substring(5, 10).trim();       // Positions 5-10
      const dockCode = qr.substring(10, 12).trim();          // Positions 10-12
      const orderNumber = qr.substring(12, 24).trim();       // Positions 12-24
      const loadId = qr.substring(24, 36).trim();            // Positions 24-36
      const palletizationCode = qr.substring(36, 38);        // Positions 36-38: "A4", "A8", "D1"
      const mros = qr.substring(38, 40);                     // Positions 38-40
      const skidId = qr.substring(40, 44);                   // Positions 40-44: "001A"

      console.log('=== PRE-SHIPMENT MANIFEST PARSING ===');
      console.log('Raw input:', qr);
      console.log('Length:', qr.length);
      console.log('Extracted fields:');
      console.log('  plantCode (0-5):', `"${plantCode}"`);
      console.log('  supplierCode (5-10):', `"${supplierCode}"`);
      console.log('  dockCode (10-12):', `"${dockCode}"`);
      console.log('  orderNumber (12-24):', `"${orderNumber}"`);
      console.log('  loadId (24-36):', `"${loadId}"`);
      console.log('  palletizationCode (36-38):', `"${palletizationCode}"`);
      console.log('  mros (38-40):', `"${mros}"`);
      console.log('  skidId (40-44):', `"${skidId}"`);
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

  // SCREEN 4: Handle Skid Scan
  // Updated: 2025-12-31 - FIXED to match shipment-load exactly: orderNumber + dockCode + palletizationCode + skidId
  const handleSkidScan = async (result: ScanResult) => {
    console.log('Skid scan result:', result);

    if (!result.success) {
      setError(result.error || 'Scan failed');
      return;
    }

    if (!sessionId || !user || !sessionData) {
      setError('Session not initialized');
      return;
    }

    const scannedValue = result.scannedValue;
    const manifest = parseToyotaManifest(scannedValue);

    if (!manifest) {
      setError('Invalid Toyota Manifest. Please scan the correct label.');
      return;
    }

    // CRITICAL: Extract skidNumber from skidId (first 3 chars: "001A" → "001")
    const scannedSkidNumber = manifest.skidId.substring(0, 3);

    console.log('=== SKID SCAN MATCHING (Screen 4) ===');
    console.log('Scanned manifest:');
    console.log('  orderNumber:', manifest.orderNumber);
    console.log('  dockCode:', manifest.dockCode);
    console.log('  skidId:', manifest.skidId);
    console.log('  skidNumber:', scannedSkidNumber);
    console.log('  palletizationCode:', manifest.palletizationCode);

    // EXACT MATCH from shipment-load: orderNumber + dockCode + skidNumber + palletizationCode
    const plannedSkid = plannedSkids.find(
      skid => skid.orderNumber === manifest.orderNumber &&
              skid.dockCode === manifest.dockCode &&
              skid.skidNumber === scannedSkidNumber &&
              skid.palletizationCode === manifest.palletizationCode
    );

    if (!plannedSkid) {
      setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} not found in planned list.`);
      return;
    }

    // Check if already scanned - EXACT MATCH from shipment-load
    const alreadyScanned = scannedSkids.some(
      item => {
        const itemSkidNumber = item.skidId.substring(0, 3);
        return item.orderNumber === manifest.orderNumber &&
               item.dockCode === manifest.dockCode &&
               itemSkidNumber === scannedSkidNumber &&
               item.palletizationCode === manifest.palletizationCode;
      }
    );

    if (alreadyScanned) {
      setError(`Order ${manifest.orderNumber}-${manifest.dockCode} - Skid ${scannedSkidNumber} with palletization ${manifest.palletizationCode} has already been scanned.`);
      return;
    }

    console.log('Found planned skid:', plannedSkid);
    console.log('======================');

    setLoading(true);
    setError(null);

    try {
      // Call API to record scan with individual fields
      const apiResult = await scanPreShipmentSkid({
        sessionId,
        skidId: plannedSkid.skidId,
        palletizationCode: manifest.palletizationCode,
        orderNumber: manifest.orderNumber,
        dockCode: manifest.dockCode,
        scannedBy: user.id,
      });

      if (!apiResult.success) {
        setError(apiResult.error || 'Failed to record scan');
        setLoading(false);
        return;
      }

      // Update UI state
      const newScannedSkid: ScannedSkid = {
        id: `${Date.now()}-${Math.random()}`,
        skidId: plannedSkid.skidId,
        orderNumber: plannedSkid.orderNumber,
        dockCode: plannedSkid.dockCode,
        palletizationCode: plannedSkid.palletizationCode,
        partCount: plannedSkid.partCount,
        destination: `Dock ${plannedSkid.dockCode}`,
        timestamp: new Date().toISOString(),
      };

      setPlannedSkids(plannedSkids.map(skid =>
        skid.skidId === plannedSkid.skidId ? { ...skid, isScanned: true } : skid
      ));
      setScannedSkids([...scannedSkids, newScannedSkid]);
      setLoading(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to record scan');
      setLoading(false);
    }
  };

  // SCREEN 4: Add Exception
  const handleAddException = () => {
    if (!selectedExceptionType || !exceptionComments.trim()) {
      setError('Please select exception type and add comments');
      return;
    }

    if (!selectedSkidForException) {
      setError('Please select which skid this exception is for');
      return;
    }

    const newException: Exception = {
      type: selectedExceptionType,
      comments: exceptionComments.trim(),
      relatedSkidId: selectedSkidForException,
      timestamp: new Date().toISOString(),
    };

    console.log('Adding exception:', newException);
    setExceptions([...exceptions, newException]);
    setSelectedExceptionType('');
    setExceptionComments('');
    setSelectedSkidForException('');
    setShowExceptionModal(false);
    setError(null);
  };

  // SCREEN 4: Remove Exception
  const handleRemoveException = (index: number) => {
    console.log('Removing exception at index:', index);
    setExceptions(exceptions.filter((_, i) => i !== index));
  };

  // SCREEN 4: Final Submit
  const handleFinalSubmit = async () => {
    console.log('=== SUBMIT VALIDATION ===');
    console.log('Planned Skids:', plannedSkids);
    console.log('Scanned Skids:', scannedSkids);
    console.log('Exceptions:', exceptions);

    if (!sessionId || !user) {
      setError('Session not initialized');
      return;
    }

    // Check if all planned items are loaded
    const areAllItemsLoaded = plannedSkids.every(skid => skid.isScanned);
    const unloadedSkids = plannedSkids.filter(skid => !skid.isScanned);
    const hasExceptionForAllUnloaded = unloadedSkids.every(skid =>
      exceptions.some(e => e.relatedSkidId === skid.skidId)
    );

    const canSubmit = areAllItemsLoaded || (scannedSkids.length > 0 && hasExceptionForAllUnloaded);

    if (!canSubmit) {
      if (scannedSkids.length === 0) {
        setError('Cannot submit: No skids have been scanned yet.');
      } else if (!hasExceptionForAllUnloaded) {
        setError(`Cannot submit: ${unloadedSkids.length} unloaded skid(s) require exceptions.`);
      }
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // Call API to complete session
      const result = await completePreShipment({
        sessionId,
        completedBy: user.id,
      });

      if (!result.success || !result.data) {
        setError(result.error || 'Failed to complete shipment');
        setLoading(false);
        return;
      }

      // Store confirmation number
      setConfirmationNumber(result.data.confirmationNumber);
      setLoading(false);
      setCurrentScreen(5);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to complete shipment');
      setLoading(false);
    }
  };

  // SCREEN 5: Continue (return to Screen 1)
  const handleContinue = () => {
    // Reset all state
    setSessionId(null);
    setSessionData(null);
    setTrailerNumber('');
    setSealNumber('');
    setDriverFirstName('');
    setDriverLastName('');
    setSupplierFirstName('');
    setSupplierLastName('');
    setPlannedSkids([]);
    setScannedSkids([]);
    setExceptions([]);
    setConfirmationNumber(null);
    setError(null);
    setCurrentScreen(1);
    loadSessions(); // Refresh session list
  };

  // Cancel and return to dashboard
  const handleCancel = () => {
    router.push('/');
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-4 pt-24 max-w-3xl mx-auto space-y-3">
          {/* Error Alert */}
          {error && (
            <Alert variant="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          {/* SCREEN 1: Scan Multiple Manifests */}
          {currentScreen === 1 && (
            <>
              {/* Scan Input */}
              <Card className="bg-[#FCFCFC]">
                <CardContent className="p-3 space-y-3">
                  <div className="flex items-center gap-3 pb-2 border-b border-gray-200">
                    <i className="fa fa-barcode text-2xl" style={{ color: '#253262' }}></i>
                    <div>
                      <h2 className="text-lg font-bold" style={{ color: '#253262' }}>
                        Scan Manifests
                      </h2>
                      <p className="text-xs text-gray-600">
                        {sessionData
                          ? 'Scan multiple manifests. Click "Continue" when ready.'
                          : 'Scan first manifest to begin Pre-Shipment session'}
                      </p>
                    </div>
                  </div>

                  <Scanner
                    onScan={handleManifestScan}
                    label="Scan Manifest"
                    placeholder="Scan Manifest barcode"
                    disabled={loading}
                  />
                </CardContent>
              </Card>

              {/* Session Info (after first scan) */}
              {sessionData && (
                <>
                  <Card className="bg-[#E8F5E9]">
                    <CardHeader>
                      <CardTitle className="flex items-center gap-2 text-sm">
                        <i className="fa fa-circle-check text-success-600"></i>
                        Session Active
                      </CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-2">
                      <div className="grid grid-cols-2 gap-2 text-sm">
                        <div>
                          <p className="text-xs text-gray-600">Route Number</p>
                          <p className="font-semibold" style={{ color: '#2E7D32' }}>
                            {sessionData.routeNumber}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600">Supplier Code</p>
                          <p className="font-semibold" style={{ color: '#2E7D32' }}>
                            {sessionData.supplierCode}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600">Orders</p>
                          <p className="font-semibold" style={{ color: '#2E7D32' }}>
                            {sessionData.orders.length}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600">Total Skids</p>
                          <p className="font-semibold" style={{ color: '#2E7D32' }}>
                            {plannedSkids.length}
                          </p>
                        </div>
                      </div>
                    </CardContent>
                  </Card>

                  {/* Scanned Skids Summary */}
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-sm">Scanned Skids ({scannedSkids.length}/{plannedSkids.length})</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-2">
                      {scannedSkids.length === 0 ? (
                        <p className="text-sm text-gray-500 text-center py-4">No skids scanned yet</p>
                      ) : (
                        <div className="space-y-2 max-h-60 overflow-y-auto">
                          {scannedSkids.map((skid, idx) => (
                            <div key={skid.id} className="p-2 border border-success-200 rounded-lg bg-success-50">
                              <div className="flex items-center justify-between">
                                <div className="flex-1">
                                  <p className="text-sm text-gray-700">
                                    <span className="font-normal">Order: </span>
                                    <span className="font-bold">{skid.orderNumber}</span>
                                    <span className="font-normal"> | Skid: </span>
                                    <span className="font-bold">{skid.skidId}</span>
                                    <span className="font-normal"> | Pallet: </span>
                                    <span className="font-bold">{skid.palletizationCode}</span>
                                  </p>
                                </div>
                                <i className="fa fa-circle-check text-success-600"></i>
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                    </CardContent>
                  </Card>

                  {/* Back Button */}
                  <Button
                    onClick={handleCancel}
                    variant="secondary"
                    fullWidth
                  >
                    <i className="fa fa-arrow-left mr-2"></i>
                    Back
                  </Button>
                </>
              )}

              {/* Sessions Table */}
              {sessions.length > 0 && (
                <Card className="bg-[#FCFCFC]">
                  <CardHeader>
                    <CardTitle>Pre-Shipment Sessions</CardTitle>
                  </CardHeader>
                  <CardContent className="p-0">
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm">
                        <thead className="bg-gray-100 border-b border-gray-200">
                          <tr>
                            <th className="px-3 py-2 text-left font-semibold" style={{ color: '#253262' }}>
                              Route
                            </th>
                            <th className="px-3 py-2 text-left font-semibold hidden sm:table-cell" style={{ color: '#253262' }}>
                              Skids
                            </th>
                            <th className="px-3 py-2 text-left font-semibold" style={{ color: '#253262' }}>
                              Status
                            </th>
                            <th className="px-3 py-2 text-left font-semibold" style={{ color: '#253262' }}>
                              Action
                            </th>
                          </tr>
                        </thead>
                        <tbody className="divide-y divide-gray-200">
                          {sessions.map((session) => (
                            <tr key={session.sessionId} className="hover:bg-gray-50">
                              <td className="px-3 py-2 font-mono text-xs">
                                {session.routeNumber}
                              </td>
                              <td className="px-3 py-2 hidden sm:table-cell">
                                <div className="flex items-center gap-1">
                                  <span className="font-semibold">{session.scannedSkidCount}/{session.totalSkidCount}</span>
                                </div>
                              </td>
                              <td className="px-3 py-2">
                                <Badge variant={session.status === 'Completed' ? 'success' : 'warning'}>
                                  {session.status}
                                </Badge>
                              </td>
                              <td className="px-3 py-2">
                                <div className="flex items-center gap-2">
                                  {session.status !== 'Completed' && (
                                    <button
                                      onClick={() => handleResumeSession(session)}
                                      className="p-2 rounded-md hover:bg-gray-100 transition-colors"
                                      style={{ color: '#253262' }}
                                      title="Resume session"
                                    >
                                      <i className="fa fa-play text-lg"></i>
                                    </button>
                                  )}
                                  <button
                                    onClick={() => handleDeleteSession(session.sessionId)}
                                    className="p-2 rounded-md hover:bg-red-50 transition-colors"
                                    style={{ color: '#D2312E' }}
                                    title="Delete session"
                                  >
                                    <i className="fa fa-trash text-lg"></i>
                                  </button>
                                </div>
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </CardContent>
                </Card>
              )}

              {sessions.length === 0 && (
                <Card className="bg-[#FCFCFC]">
                  <CardContent className="p-6 text-center text-gray-500">
                    <i className="fa fa-inbox text-4xl mb-2" style={{ color: '#253262', opacity: 0.3 }}></i>
                    <p>No sessions found</p>
                    <p className="text-sm mt-1">Scan a manifest to begin</p>
                  </CardContent>
                </Card>
              )}
            </>
          )}

          {/* SCREEN 2: Summary & Save */}
          {currentScreen === 2 && sessionData && (
            <>
              {/* Summary Card */}
              <Card className="bg-[#FCFCFC]">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <i className="fa fa-clipboard-list text-xl" style={{ color: '#253262' }}></i>
                    Pre-Shipment Summary
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  {/* Route Info */}
                  <div className="grid grid-cols-2 gap-2 text-sm pb-3 border-b border-gray-200">
                    <div>
                      <p className="text-xs text-gray-600">Route Number</p>
                      <p className="font-semibold" style={{ color: '#253262' }}>
                        {sessionData.routeNumber}
                      </p>
                    </div>
                    <div>
                      <p className="text-xs text-gray-600">Supplier Code</p>
                      <p className="font-semibold" style={{ color: '#253262' }}>
                        {sessionData.supplierCode}
                      </p>
                    </div>
                  </div>

                  {/* Orders Summary */}
                  <div className="space-y-2">
                    <p className="text-xs font-medium text-gray-600">Orders ({sessionData.orders.length})</p>
                    {sessionData.orders.map((order, idx) => {
                      const orderSkids = plannedSkids.filter(s => s.orderNumber === order.orderNumber);
                      const scannedCount = orderSkids.filter(s => s.isScanned).length;
                      return (
                        <div key={idx} className="p-2 bg-gray-50 rounded border border-gray-200">
                          <div className="flex items-center justify-between">
                            <div>
                              <p className="text-xs font-mono font-semibold" style={{ color: '#253262' }}>
                                {order.orderNumber}
                              </p>
                              <p className="text-xs text-gray-600">
                                Dock: {order.dockCode} | Skids: {scannedCount}/{orderSkids.length}
                              </p>
                            </div>
                            <Badge variant={scannedCount === orderSkids.length ? 'success' : 'warning'}>
                              {scannedCount}/{orderSkids.length}
                            </Badge>
                          </div>
                        </div>
                      );
                    })}
                  </div>

                  {/* Total Skids */}
                  <div className="pt-3 border-t border-gray-200">
                    <div className="grid grid-cols-2 gap-2 text-sm">
                      <div>
                        <p className="text-xs text-gray-600">Total Skids Scanned</p>
                        <p className="font-bold text-lg" style={{ color: '#253262' }}>
                          {scannedSkids.length}/{plannedSkids.length}
                        </p>
                      </div>
                      <div>
                        <p className="text-xs text-gray-600">Total Parts</p>
                        <p className="font-bold text-lg" style={{ color: '#253262' }}>
                          {scannedSkids.reduce((total, skid) => total + skid.partCount, 0)}
                        </p>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Action Buttons */}
              <div className="space-y-2">
                <div className="flex gap-2">
                  <Button onClick={() => setCurrentScreen(1)} variant="secondary" fullWidth>
                    <i className="fa fa-arrow-left mr-2"></i>
                    Back
                  </Button>
                  <Button
                    onClick={handleContinueToTrailerInfo}
                    variant="success"
                    fullWidth
                  >
                    <i className="fa fa-truck mr-2"></i>
                    Enter Trailer Info
                  </Button>
                </div>
                <Button
                  onClick={handleSaveAndExit}
                  variant="primary"
                  fullWidth
                  style={{ backgroundColor: '#253262' }}
                >
                  <i className="fa fa-floppy-disk mr-2"></i>
                  Save & Exit (Complete Later)
                </Button>
              </div>

              {/* Info Note */}
              <Card className="bg-blue-50 border-blue-200">
                <CardContent className="p-3">
                  <div className="flex items-start gap-2">
                    <i className="fa fa-circle-info text-blue-600 mt-1"></i>
                    <div className="text-xs text-blue-800">
                      <p className="font-semibold mb-1">Save & Exit Option</p>
                      <p>You can save this session now and complete it later via Shipment Load when the driver arrives with the trailer.</p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </>
          )}

          {/* SCREEN 3: Trailer Information (OPTIONAL) */}
          {currentScreen === 3 && sessionData && (
            <>
              <Card>
                <CardHeader>
                  <CardTitle>Trailer Information (Optional)</CardTitle>
                </CardHeader>
                <CardContent className="space-y-3">
                  <p className="text-sm text-gray-600">
                    Route: <strong>{sessionData.routeNumber}</strong> | Supplier: <strong>{sessionData.supplierCode}</strong>
                  </p>

                  {/* Trailer Number */}
                  <div>
                    <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                      Trailer Number
                    </label>
                    <input
                      type="text"
                      value={trailerNumber}
                      onChange={(e) => setTrailerNumber(e.target.value)}
                      placeholder="Enter trailer number"
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
                      value={sealNumber}
                      onChange={(e) => setSealNumber(e.target.value)}
                      placeholder="Enter seal number (optional)"
                      className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                      style={{ backgroundColor: '#FCFCFC' }}
                    />
                  </div>

                  {/* Driver First Name */}
                  <div>
                    <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                      Driver First Name
                    </label>
                    <input
                      type="text"
                      value={driverFirstName}
                      onChange={(e) => setDriverFirstName(e.target.value)}
                      placeholder="Enter driver first name"
                      className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                      style={{ backgroundColor: '#FCFCFC' }}
                    />
                  </div>

                  {/* Driver Last Name */}
                  <div>
                    <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                      Driver Last Name
                    </label>
                    <input
                      type="text"
                      value={driverLastName}
                      onChange={(e) => setDriverLastName(e.target.value)}
                      placeholder="Enter driver last name"
                      className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                      style={{ backgroundColor: '#FCFCFC' }}
                    />
                  </div>

                  {/* Supplier First Name */}
                  <div>
                    <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                      Supplier First Name
                    </label>
                    <input
                      type="text"
                      value={supplierFirstName}
                      onChange={(e) => setSupplierFirstName(e.target.value)}
                      placeholder="Enter supplier first name (optional)"
                      className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                      style={{ backgroundColor: '#FCFCFC' }}
                    />
                  </div>

                  {/* Supplier Last Name */}
                  <div>
                    <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                      Supplier Last Name
                    </label>
                    <input
                      type="text"
                      value={supplierLastName}
                      onChange={(e) => setSupplierLastName(e.target.value)}
                      placeholder="Enter supplier last name (optional)"
                      className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                      style={{ backgroundColor: '#FCFCFC' }}
                    />
                  </div>
                </CardContent>
              </Card>

              {/* Info Note */}
              <Card className="bg-blue-50 border-blue-200">
                <CardContent className="p-3">
                  <div className="flex items-start gap-2">
                    <i className="fa fa-circle-info text-blue-600 mt-1"></i>
                    <div className="text-xs text-blue-800">
                      <p className="font-semibold mb-1">Trailer Info is Optional</p>
                      <p>You can skip this step if the driver has not arrived yet. Trailer information can be added later during Shipment Load.</p>
                    </div>
                  </div>
                </CardContent>
              </Card>

              {/* Continue Buttons */}
              <div className="space-y-2">
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
                    onClick={handleContinueToScanning}
                    variant="success"
                    fullWidth
                    loading={loading}
                  >
                    <i className="fa fa-arrow-right mr-2"></i>
                    {trailerNumber.trim() ? 'Save & Continue' : 'Continue'}
                  </Button>
                </div>
                <Button
                  onClick={handleSkipTrailerInfo}
                  variant="secondary"
                  fullWidth
                >
                  <i className="fa fa-forward mr-2"></i>
                  Skip for Now
                </Button>
              </div>
            </>
          )}

          {/* SCREEN 4: Skid Scanning */}
          {currentScreen === 4 && (
            <>
              {/* Loading Progress */}
              {scannedSkids.length > 0 && (
                <Card className="bg-[#E8F5E9]">
                  <CardHeader className="p-0">
                    <div
                      className={`flex items-center justify-between cursor-pointer hover:bg-green-100 transition-colors ${
                        expandedSection === 'progress' ? 'p-4 rounded-t-lg' : 'p-2 rounded-lg'
                      }`}
                      onClick={() => setExpandedSection(expandedSection === 'progress' ? null : 'progress')}
                    >
                      <div className="flex items-center gap-2">
                        <i className={`fa fa-circle-check ${expandedSection === 'progress' ? 'text-lg' : 'text-sm'}`} style={{ color: '#2E7D32' }}></i>
                        <div className="min-w-0 flex-1">
                          <p className={`text-gray-600 ${expandedSection === 'progress' ? 'text-xs' : 'hidden'}`}>Loading Progress</p>
                          <p className={`font-semibold ${expandedSection === 'progress' ? 'text-sm' : 'text-xs'}`} style={{ color: '#2E7D32' }}>
                            Scanned: {scannedSkids.length}/{plannedSkids.length} | Parts: {scannedSkids.reduce((total, skid) => total + skid.partCount, 0)}
                          </p>
                        </div>
                      </div>
                      <i className={`fa fa-chevron-${expandedSection === 'progress' ? 'up' : 'down'}`} style={{ color: '#2E7D32' }}></i>
                    </div>
                  </CardHeader>
                  {expandedSection === 'progress' && (
                    <CardContent className="p-3">
                      <div className="grid grid-cols-3 gap-3">
                        <div>
                          <p className="text-xs text-gray-600">Scanned</p>
                          <p className="font-bold text-lg" style={{ color: '#2E7D32' }}>
                            {scannedSkids.length}/{plannedSkids.length}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600">Parts</p>
                          <p className="font-bold text-lg" style={{ color: '#2E7D32' }}>
                            {scannedSkids.reduce((total, skid) => total + skid.partCount, 0)}
                          </p>
                        </div>
                        <div>
                          <p className="text-xs text-gray-600">Remaining</p>
                          <p className="font-bold text-lg" style={{ color: scannedSkids.length >= plannedSkids.length ? '#2E7D32' : '#253262' }}>
                            {Math.max(0, plannedSkids.length - scannedSkids.length)}
                          </p>
                        </div>
                      </div>
                    </CardContent>
                  )}
                </Card>
              )}

              {/* Planned Skids */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className={`flex items-center justify-between cursor-pointer hover:bg-gray-50 transition-colors ${
                      expandedSection === 'planned' ? 'p-4 rounded-t-lg' : 'p-2 rounded-lg'
                    }`}
                    onClick={() => setExpandedSection(expandedSection === 'planned' ? null : 'planned')}
                  >
                    <CardTitle className={expandedSection === 'planned' ? 'text-sm' : 'text-xs'}>
                      Planned Skids ({plannedSkids.filter(s => !s.isScanned).length}/{plannedSkids.length})
                    </CardTitle>
                    <i className={`fa fa-chevron-${expandedSection === 'planned' ? 'up' : 'down'}`}></i>
                  </div>
                </CardHeader>
                {expandedSection === 'planned' && (
                  <CardContent className="space-y-2">
                    {plannedSkids.filter(skid => !skid.isScanned).length === 0 ? (
                      <p className="text-sm text-gray-500 text-center py-4">All skids scanned!</p>
                    ) : (
                      plannedSkids
                        .filter(skid => !skid.isScanned)
                        .map((skid, idx) => (
                          <div key={idx} className="p-3 border border-gray-200 rounded-lg" style={{ backgroundColor: '#FFFFFF' }}>
                            <div className="flex items-center justify-between">
                              <div className="flex-1">
                                <p className="font-mono text-sm font-bold" style={{ color: '#253262' }}>
                                  {skid.skidId}
                                </p>
                                <p className="text-xs text-gray-600">Order: {skid.orderNumber}</p>
                                <p className="text-xs text-gray-600">Parts: {skid.partCount}</p>
                              </div>
                              <Badge variant="warning">Pending</Badge>
                            </div>
                          </div>
                        ))
                    )}
                  </CardContent>
                )}
              </Card>

              {/* Scanned Skids */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className={`flex items-center justify-between cursor-pointer hover:bg-gray-50 transition-colors ${
                      expandedSection === 'scanned' ? 'p-4 rounded-t-lg' : 'p-2 rounded-lg'
                    }`}
                    onClick={() => setExpandedSection(expandedSection === 'scanned' ? null : 'scanned')}
                  >
                    <CardTitle className={expandedSection === 'scanned' ? 'text-sm' : 'text-xs'}>
                      Scanned Skids ({scannedSkids.length}/{plannedSkids.length})
                    </CardTitle>
                    <i className={`fa fa-chevron-${expandedSection === 'scanned' ? 'up' : 'down'}`}></i>
                  </div>
                </CardHeader>
                {expandedSection === 'scanned' && (
                  <CardContent className="space-y-2">
                    {scannedSkids.length === 0 ? (
                      <p className="text-sm text-gray-500 text-center py-4">No skids scanned yet</p>
                    ) : (
                      scannedSkids.map((skid, idx) => (
                        <div key={skid.id} className="p-3 border-2 border-success-500 rounded-lg relative" style={{ backgroundColor: '#FFFFFF' }}>
                          <div className="flex items-center justify-between">
                            <div className="flex-1">
                              <p className="font-mono text-sm font-bold" style={{ color: '#253262' }}>
                                {skid.skidId}
                              </p>
                              <p className="text-xs text-gray-600">Order: {skid.orderNumber}</p>
                              <p className="text-xs text-gray-600">Parts: {skid.partCount}</p>
                              <p className="text-xs text-gray-500 mt-1">
                                <i className="fa fa-clock mr-1"></i>
                                {new Date(skid.timestamp).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })}
                              </p>
                            </div>
                            <div className="flex flex-col items-end gap-1">
                              <i className="fa fa-circle-check text-success-600 text-xl"></i>
                              <span className="text-xs font-medium text-success-700">#{idx + 1}</span>
                            </div>
                          </div>
                        </div>
                      ))
                    )}
                  </CardContent>
                )}
              </Card>

              {/* Exceptions */}
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
                      <p className="text-sm text-gray-500 text-center py-4">No exceptions added</p>
                    ) : (
                      exceptions.map((exception, idx) => (
                        <div key={idx} className="p-3 bg-warning-50 border border-warning-200 rounded-lg">
                          <div className="flex items-start justify-between gap-2">
                            <div className="flex-1">
                              <div className="flex items-center gap-2 mb-1">
                                <Badge variant="warning">{exception.type}</Badge>
                                <span className="text-xs text-gray-500">
                                  {new Date(exception.timestamp).toLocaleString()}
                                </span>
                              </div>
                              <p className="text-xs text-gray-600 mb-1">
                                <span className="font-semibold">Skid:</span> {exception.relatedSkidId}
                              </p>
                              <p className="text-sm text-gray-700">{exception.comments}</p>
                            </div>
                            <button
                              onClick={() => handleRemoveException(idx)}
                              className="text-error-600 hover:text-error-700 p-1"
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

              {/* Scanner */}
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
                  onClick={() => setCurrentScreen(3)}
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
                        plannedSkids.filter(skid => !skid.isScanned).every(skid =>
                          exceptions.some(e => e.relatedSkidId === skid.skidId)
                        )))
                  }
                >
                  <i className="fa fa-paper-plane mr-2"></i>
                  Submit Shipment
                </Button>
              </div>
            </>
          )}

          {/* SCREEN 5: Success Screen */}
          {currentScreen === 5 && (
            <div className="flex items-center justify-center min-h-[calc(100vh-140px)]">
              <Card className="max-w-md w-full shadow-2xl">
                <CardContent className="p-8 text-center space-y-6">
                  <div className="flex justify-center">
                    <div className="inline-flex items-center justify-center w-24 h-24 rounded-full bg-success-100">
                      <i className="fa fa-circle-check text-6xl text-success-600"></i>
                    </div>
                  </div>

                  <div>
                    <h1 className="text-2xl font-bold mb-2" style={{ color: '#253262' }}>
                      Submitted Successfully
                    </h1>
                  </div>

                  <div className="py-4">
                    <p className="text-sm text-gray-600 mb-3">Confirmation Number</p>
                    <div className="bg-gray-50 px-6 py-4 rounded-lg border-2 border-gray-300">
                      <span className="font-mono text-2xl font-bold select-all" style={{ color: '#253262' }}>
                        {confirmationNumber || 'N/A'}
                      </span>
                    </div>
                  </div>

                  <div className="pt-4 space-y-3">
                    <Button
                      onClick={handleContinue}
                      variant="primary"
                      fullWidth
                      className="py-3 text-base font-semibold"
                      style={{ backgroundColor: '#253262', color: 'white' }}
                    >
                      <i className="fa fa-plus mr-2"></i>
                      CONTINUE
                    </Button>

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

          {/* Cancel Button */}
          {currentScreen !== 5 && (
            <Button
              onClick={handleCancel}
              variant="error"
              fullWidth
            >
              <i className="fa fa-xmark mr-2"></i>
              Cancel
            </Button>
          )}
        </div>
      </div>

      {/* Summary Modal - Shows when all skids are scanned */}
      {showSummaryModal && sessionData && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          onClick={() => setShowSummaryModal(false)}
        >
          <div
            className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4 max-h-[90vh] overflow-y-auto"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between p-4 border-b border-gray-200 sticky top-0 bg-white z-10">
              <h3 className="text-lg font-bold" style={{ color: '#253262' }}>
                Pre-Shipment Summary
              </h3>
              <button
                onClick={() => setShowSummaryModal(false)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                <i className="fa fa-xmark text-2xl"></i>
              </button>
            </div>

            <div className="p-6 space-y-4">
              {/* Route Info */}
              <div className="grid grid-cols-2 gap-3 pb-3 border-b border-gray-200">
                <div>
                  <p className="text-xs text-gray-600">Route Number</p>
                  <p className="font-semibold text-base" style={{ color: '#253262' }}>
                    {sessionData.routeNumber}
                  </p>
                </div>
                <div>
                  <p className="text-xs text-gray-600">Supplier Code</p>
                  <p className="font-semibold text-base" style={{ color: '#253262' }}>
                    {sessionData.supplierCode}
                  </p>
                </div>
              </div>

              {/* Orders Summary */}
              <div className="space-y-2">
                <p className="text-sm font-semibold text-gray-700">Orders ({sessionData.orders.length})</p>
                {sessionData.orders.map((order, idx) => {
                  const orderSkids = plannedSkids.filter(s => s.orderNumber === order.orderNumber);
                  const scannedCount = orderSkids.filter(s => s.isScanned).length;
                  return (
                    <div key={idx} className="p-3 bg-gray-50 rounded border border-gray-200">
                      <div className="flex items-center justify-between">
                        <div className="flex-1">
                          <p className="text-sm font-mono font-semibold" style={{ color: '#253262' }}>
                            {order.orderNumber}
                          </p>
                          <p className="text-xs text-gray-600">
                            Dock: {order.dockCode} | Skids: {scannedCount}/{orderSkids.length}
                          </p>
                        </div>
                        <Badge variant={scannedCount === orderSkids.length ? 'success' : 'warning'}>
                          {scannedCount}/{orderSkids.length}
                        </Badge>
                      </div>
                    </div>
                  );
                })}
              </div>

              {/* Total Skids */}
              <div className="pt-3 border-t border-gray-200">
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <p className="text-xs text-gray-600">Total Skids Scanned</p>
                    <p className="font-bold text-xl" style={{ color: '#2E7D32' }}>
                      {scannedSkids.length}/{plannedSkids.length}
                    </p>
                  </div>
                  <div>
                    <p className="text-xs text-gray-600">Total Parts</p>
                    <p className="font-bold text-xl" style={{ color: '#2E7D32' }}>
                      {scannedSkids.reduce((total, skid) => total + skid.partCount, 0)}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            <div className="p-4 border-t border-gray-200">
              <Button
                onClick={() => setShowSummaryModal(false)}
                variant="secondary"
                fullWidth
              >
                <i className="fa fa-xmark mr-2"></i>
                Close
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Exception Modal */}
      {showExceptionModal && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          onClick={() => {
            setShowExceptionModal(false);
            setSelectedExceptionType('');
            setExceptionComments('');
            setSelectedSkidForException('');
          }}
        >
          <div
            className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex items-center justify-between p-4 border-b border-gray-200">
              <h3 className="text-lg font-bold" style={{ color: '#253262' }}>
                Add Exception
              </h3>
              <button
                onClick={() => {
                  setShowExceptionModal(false);
                  setSelectedExceptionType('');
                  setExceptionComments('');
                  setSelectedSkidForException('');
                }}
                className="text-gray-400 hover:text-gray-600 transition-colors"
              >
                <i className="fa fa-xmark text-2xl"></i>
              </button>
            </div>

            <div className="p-6 space-y-4">
              <div>
                <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                  Exception Type *
                </label>
                <select
                  value={selectedExceptionType}
                  onChange={(e) => setSelectedExceptionType(e.target.value)}
                  className="w-full px-3 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 text-sm"
                  style={{ backgroundColor: '#FCFCFC' }}
                >
                  <option value="">Select exception type...</option>
                  {EXCEPTION_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </div>

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
                      <option key={skid.skidId} value={skid.skidId}>
                        {skid.skidId} - {skid.orderNumber}
                      </option>
                    ))}
                </select>
              </div>

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

            <div className="flex gap-2 p-4 border-t border-gray-200">
              <Button
                onClick={() => {
                  setShowExceptionModal(false);
                  setSelectedExceptionType('');
                  setExceptionComments('');
                  setSelectedSkidForException('');
                }}
                variant="secondary"
                fullWidth
              >
                <i className="fa fa-xmark mr-2"></i>
                Cancel
              </Button>
              <Button
                onClick={handleAddException}
                variant="warning"
                fullWidth
                disabled={!selectedExceptionType || !exceptionComments.trim() || !selectedSkidForException}
              >
                <i className="fa fa-plus mr-2"></i>
                Add Exception
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
