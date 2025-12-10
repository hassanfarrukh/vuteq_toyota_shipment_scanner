/**
 * Pre-Shipment Scan Page - 5-Screen Workflow with Multiple Manifests per Shipment
 * Author: Hassan
 * Date: 2025-11-05
 * Updated: 2025-11-05 - Replaced Screen 2 with Pickup Route QR scan from shipment-load Screen 1
 * Updated: 2025-11-05 - Added Screen 4 (Loading/Scanning) from shipment-load Screen 3
 * Updated: 2025-11-05 - CRITICAL BUG FIX: Fixed skid validation in Screen 4
 *                       parseToyotaManifest now extracts last 8 characters to match planned skid IDs
 * Updated: 2025-11-06 - CRITICAL BUG FIX: Fixed Parts count showing 0 in Screen 4
 *                       - ScannedSkids now properly match with plannedSkids to get correct partCount
 *                       - Added Parts count display in progress indicator (collapsed and expanded views)
 *                       - Fixed both initialization paths (resume and Screen 3->4 transition)
 * Updated: 2025-11-06 - CRITICAL BUG FIX: "Planned Skids" displaying wrong count (5/5 instead of 3/5)
 *                       - Root cause: Pre-scanned manifests from Screen 1 were not marked as isScanned
 *                       - Fixed in both Screen 3->4 transition and handleResumeShipment
 *                       - Now correctly marks plannedSkids as isScanned when manifests are pre-loaded
 *
 * SCREEN FLOW:
 * Screen 1: Pre-scan Manifests - Create shipments and scan multiple manifests
 * Screen 2: Pickup Route QR Scan - Shows Route, Plant, Supplier, Dock, Estimated Skids
 * Screen 3: Trailer Information - Copied EXACTLY from shipment-load Screen 2
 * Screen 4: Loading/Scanning - Copied EXACTLY from shipment-load Screen 3 (Planned/Scanned/Exceptions)
 * Screen 5: Success Screen - Copied from shipment-load Screen 4
 *
 * FEATURES:
 * - Multiple manifests per shipment support
 * - Extracts last 8 characters from barcode as manifest ID
 * - localStorage persistence using shipment_{shipmentId} and preShipmentScan_{manifestNumber} as keys
 * - Active shipment indicator showing current working shipment
 * - "New Shipment" button to create new shipments
 * - Table showing shipments (not individual manifests) with manifest count
 * - Play/Resume button to continue working on in-progress shipments
 * - Delete button for incomplete shipments
 * - Progress indicator showing "Screen X of 5"
 * - Font Awesome icons only
 * - VUTEQ colors (#253262 navy, #D2312E red, #FCFCFC off-white)
 * - Mobile-first responsive design
 * - Screen 4: Planned/Scanned items tracking with exceptions
 */

'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Scanner from '@/components/ui/Scanner';
import Button from '@/components/ui/Button';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Alert from '@/components/ui/Alert';
import Badge from '@/components/ui/Badge';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import type { ScanResult } from '@/types';

// Screen types
type Screen = 1 | 2 | 3 | 4 | 5;

// Status types for manifests
type ManifestStatus = 'scanned' | 'in-progress' | 'completed';

// Data interfaces
interface PickupRouteData {
  routeNumber: string;
  plant: string;
  supplierCode: string;
  dockCode: string;
  estimatedSkids: number;
  rawQRValue: string;
}

// Screen 4 interfaces - Added for Loading/Scanning screen
interface PlannedSkid {
  skidId: string;
  orderNumber: string;
  partCount: number;
  destination: string;
  isScanned: boolean;
}

interface ScannedSkid {
  id: string;
  skidId: string;
  orderNumber: string;
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

interface ManifestData {
  manifestNumber: string;
  scannedAt: string;
  status: ManifestStatus;
  driverCheckSheet?: string;
  trailerNumber?: string;
  sealNumber?: string;
  carrierName?: string;
  driverName?: string;
  driverLicense?: string;
  departureDate?: string;
  confirmationNumber?: string;
}

// Shipment interface - groups multiple manifests together
interface Shipment {
  shipmentId: string;
  manifests: string[];
  createdAt: string;
  status: 'in-progress' | 'completed';
  currentScreen: number;
  driverInfo?: {
    driverCheckSheet: string;
    driverName: string;
    driverLicense: string;
  };
  trailerInfo?: {
    trailerNumber: string;
    sealNumber: string;
    carrierName: string;
    departureDate: string;
  };
  confirmationNumber?: string;
}

// Toyota-specific exception types (matching shipment-load)
// Author: Hassan, 2025-11-05
const EXCEPTION_TYPES = [
  'Revised Quantity (Toyota Quantity Reduction)',
  'Modified Quantity per Box',
  'Supplier Revised Shortage (Short Shipment)',
  'Non-Standard Packaging (Expendable)',
];

// LocalStorage helpers
const STORAGE_PREFIX = 'preShipmentScan_';
const SHIPMENT_STORAGE_PREFIX = 'shipment_';

const saveManifestToStorage = (data: ManifestData) => {
  localStorage.setItem(`${STORAGE_PREFIX}${data.manifestNumber}`, JSON.stringify(data));
};

const loadManifestFromStorage = (manifestNumber: string): ManifestData | null => {
  const stored = localStorage.getItem(`${STORAGE_PREFIX}${manifestNumber}`);
  if (stored) {
    try {
      return JSON.parse(stored) as ManifestData;
    } catch (e) {
      console.error('Failed to parse manifest data:', e);
      return null;
    }
  }
  return null;
};

const getAllManifestsFromStorage = (): ManifestData[] => {
  const manifests: ManifestData[] = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (key && key.startsWith(STORAGE_PREFIX)) {
      const stored = localStorage.getItem(key);
      if (stored) {
        try {
          manifests.push(JSON.parse(stored) as ManifestData);
        } catch (e) {
          console.error('Failed to parse manifest:', e);
        }
      }
    }
  }
  // Sort by date (newest first)
  return manifests.sort((a, b) => new Date(b.scannedAt).getTime() - new Date(a.scannedAt).getTime());
};

// Shipment storage helpers
const saveShipmentToStorage = (shipment: Shipment) => {
  localStorage.setItem(`${SHIPMENT_STORAGE_PREFIX}${shipment.shipmentId}`, JSON.stringify(shipment));
};

const loadShipmentFromStorage = (shipmentId: string): Shipment | null => {
  const stored = localStorage.getItem(`${SHIPMENT_STORAGE_PREFIX}${shipmentId}`);
  if (stored) {
    try {
      return JSON.parse(stored) as Shipment;
    } catch (e) {
      console.error('Failed to parse shipment data:', e);
      return null;
    }
  }
  return null;
};

const getAllShipmentsFromStorage = (): Shipment[] => {
  const shipments: Shipment[] = [];
  for (let i = 0; i < localStorage.length; i++) {
    const key = localStorage.key(i);
    if (key && key.startsWith(SHIPMENT_STORAGE_PREFIX)) {
      const stored = localStorage.getItem(key);
      if (stored) {
        try {
          shipments.push(JSON.parse(stored) as Shipment);
        } catch (e) {
          console.error('Failed to parse shipment:', e);
        }
      }
    }
  }
  // Sort by date (newest first)
  return shipments.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
};

const deleteShipmentFromStorage = (shipmentId: string) => {
  localStorage.removeItem(`${SHIPMENT_STORAGE_PREFIX}${shipmentId}`);
};

export default function PreShipmentScanPage() {
  const router = useRouter();
  const [currentScreen, setCurrentScreen] = useState<Screen>(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Resume dialog state
  const [showResumeDialog, setShowResumeDialog] = useState(false);
  const [shipmentToResume, setShipmentToResume] = useState<Shipment | null>(null);
  const [pendingManifestId, setPendingManifestId] = useState<string | null>(null);

  // Shipment management
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [activeShipmentId, setActiveShipmentId] = useState<string | null>(null);

  // Current manifest being worked on (legacy - kept for backward compatibility)
  const [currentManifest, setCurrentManifest] = useState<ManifestData | null>(null);

  // All saved manifests (for Screen 1) - legacy
  const [savedManifests, setSavedManifests] = useState<ManifestData[]>([]);

  // Screen 2: Pickup Route Data (from shipment-load Screen 1)
  const [pickupRouteData, setPickupRouteData] = useState<PickupRouteData | null>(null);
  const [driverCheckSheet, setDriverCheckSheet] = useState<string>('');

  // Screen 3: Trailer Information Form
  const [trailerNumber, setTrailerNumber] = useState<string>('');
  const [sealNumber, setSealNumber] = useState<string>('');
  const [carrierName, setCarrierName] = useState<string>('');
  const [driverName, setDriverName] = useState<string>('');
  const [driverLicense, setDriverLicense] = useState<string>('');
  const [departureDate, setDepartureDate] = useState<string>('');
  const [notes, setNotes] = useState<string>('');

  // Screen 4: Loading/Scanning state (from shipment-load Screen 3)
  const [plannedSkids, setPlannedSkids] = useState<PlannedSkid[]>([]);
  const [scannedSkids, setScannedSkids] = useState<ScannedSkid[]>([]);
  const [expandedSection, setExpandedSection] = useState<'order' | 'planned' | 'scanned' | 'exceptions' | 'progress' | null>(null);

  // Exceptions Data (Screen 4)
  const [exceptions, setExceptions] = useState<Exception[]>([]);
  const [showExceptionModal, setShowExceptionModal] = useState(false);
  const [selectedExceptionType, setSelectedExceptionType] = useState('');
  const [exceptionComments, setExceptionComments] = useState('');
  const [selectedSkidForException, setSelectedSkidForException] = useState('');

  // Load all shipments and manifests on mount
  useEffect(() => {
    setShipments(getAllShipmentsFromStorage());
    setSavedManifests(getAllManifestsFromStorage());
  }, []);

  // Auto-save active shipment whenever it changes
  useEffect(() => {
    if (activeShipmentId) {
      const activeShipment = shipments.find(s => s.shipmentId === activeShipmentId);
      if (activeShipment) {
        saveShipmentToStorage(activeShipment);
      }
    }
  }, [shipments, activeShipmentId]);

  // Auto-save current manifest whenever it changes (legacy)
  useEffect(() => {
    if (currentManifest) {
      saveManifestToStorage(currentManifest);
      setSavedManifests(getAllManifestsFromStorage());
    }
  }, [currentManifest]);

  // SCREEN 1: Create new shipment
  const handleNewShipment = () => {
    const newShipmentId = `SHP${Date.now()}`;
    const newShipment: Shipment = {
      shipmentId: newShipmentId,
      manifests: [],
      createdAt: new Date().toISOString(),
      status: 'in-progress',
      currentScreen: 1,
    };

    setShipments(prev => [newShipment, ...prev]);
    setActiveShipmentId(newShipmentId);
    saveShipmentToStorage(newShipment);
    setError(null);
  };

  // SCREEN 1: Handle Manifest scan (now adds to active shipment)
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

    setLoading(true);
    setError(null);

    try {
      // Extract last 8 characters as manifest ID
      const manifestId = result.scannedValue.slice(-8);

      // Check if manifest already exists in any shipment
      const existingShipment = shipments.find(s => s.manifests.includes(manifestId));
      if (existingShipment) {
        // Show resume dialog instead of error
        setShowResumeDialog(true);
        setShipmentToResume(existingShipment);
        setPendingManifestId(manifestId);
        setLoading(false);
        return;
      }

      // If no active shipment, create one automatically
      let currentShipmentId = activeShipmentId;
      if (!currentShipmentId) {
        const newShipmentId = `SHP${Date.now()}`;
        const newShipment: Shipment = {
          shipmentId: newShipmentId,
          manifests: [],
          createdAt: new Date().toISOString(),
          status: 'in-progress',
          currentScreen: 1,
        };
        setShipments(prev => [newShipment, ...prev]);
        setActiveShipmentId(newShipmentId);
        currentShipmentId = newShipmentId;
      }

      // Add manifest to active shipment
      setShipments(prev => prev.map(shipment => {
        if (shipment.shipmentId === currentShipmentId) {
          const updatedShipment = {
            ...shipment,
            manifests: [...shipment.manifests, manifestId],
          };
          saveShipmentToStorage(updatedShipment);
          return updatedShipment;
        }
        return shipment;
      }));

      setLoading(false);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save manifest');
      setLoading(false);
    }
  };

  // SCREEN 1: Handle Resume Dialog - Resume existing shipment
  const handleResumeExisting = () => {
    if (shipmentToResume) {
      setShowResumeDialog(false);
      handleResumeShipment(shipmentToResume);
      setShipmentToResume(null);
      setPendingManifestId(null);
    }
  };

  // SCREEN 1: Handle Resume Dialog - Start new shipment
  const handleStartNew = () => {
    setShowResumeDialog(false);
    setShipmentToResume(null);
    // Create a new shipment and add the pending manifest
    const newShipmentId = `SHP${Date.now()}`;
    const newShipment: Shipment = {
      shipmentId: newShipmentId,
      manifests: pendingManifestId ? [pendingManifestId] : [],
      createdAt: new Date().toISOString(),
      status: 'in-progress',
      currentScreen: 1,
    };
    setShipments(prev => [newShipment, ...prev]);
    setActiveShipmentId(newShipmentId);
    saveShipmentToStorage(newShipment);
    setPendingManifestId(null);
  };

  // SCREEN 1: Resume existing shipment (Play button)
  const handleResumeShipment = (shipment: Shipment) => {
    setActiveShipmentId(shipment.shipmentId);

    // Restore form data if exists
    if (shipment.driverInfo) {
      setDriverCheckSheet(shipment.driverInfo.driverCheckSheet);
      setDriverName(shipment.driverInfo.driverName);
      setDriverLicense(shipment.driverInfo.driverLicense);
    }
    if (shipment.trailerInfo) {
      setTrailerNumber(shipment.trailerInfo.trailerNumber);
      setSealNumber(shipment.trailerInfo.sealNumber);
      setCarrierName(shipment.trailerInfo.carrierName);
      setDepartureDate(shipment.trailerInfo.departureDate);
    }

    // If resuming to Screen 4 or beyond, initialize scannedSkids with manifests
    // Fixed: Match scanned manifests with planned skids to get correct part counts
    // Author: Hassan, 2025-11-06
    // Updated: 2025-11-06 - CRITICAL BUG FIX: Mark pre-scanned manifests as isScanned in plannedSkids
    if (shipment.currentScreen && shipment.currentScreen >= 4) {
      if (shipment.manifests && shipment.manifests.length > 0) {
        const preScannedSkids: ScannedSkid[] = shipment.manifests.map((manifestId, index) => {
          // Find matching planned skid to get the correct part count
          const matchingPlannedSkid = plannedSkids.find(ps => ps.skidId === manifestId);

          return {
            id: `pre-scanned-${Date.now()}-${index}`,
            skidId: manifestId,
            orderNumber: matchingPlannedSkid?.orderNumber || `PRE-${shipment.shipmentId.slice(-6)}`,
            partCount: matchingPlannedSkid?.partCount || 0,
            destination: matchingPlannedSkid?.destination || 'Pre-scanned',
            timestamp: new Date().toISOString(),
          };
        });
        setScannedSkids(preScannedSkids);

        // CRITICAL FIX: Mark the pre-scanned skids as isScanned in plannedSkids
        setPlannedSkids(prevPlanned =>
          prevPlanned.map(skid => ({
            ...skid,
            isScanned: shipment.manifests.includes(skid.skidId) ? true : skid.isScanned
          }))
        );

        console.log('Initialized scannedSkids on resume:', preScannedSkids);
      }
    }

    // Determine which screen to go to based on progress
    if (shipment.status === 'completed') {
      setCurrentScreen(5);
    } else if (shipment.currentScreen) {
      setCurrentScreen(shipment.currentScreen as Screen);
    } else if (shipment.trailerInfo) {
      setCurrentScreen(3);
    } else if (shipment.driverInfo) {
      setCurrentScreen(3);
    } else {
      setCurrentScreen(2);
    }
  };

  // SCREEN 1: Delete shipment
  const handleDeleteShipment = (shipmentId: string) => {
    const shipment = shipments.find(s => s.shipmentId === shipmentId);
    if (!shipment) return;

    const confirmed = window.confirm(
      `Are you sure you want to delete this shipment?\n\nShipment ID: ${shipmentId}\nManifests: ${shipment.manifests.length}`
    );

    if (confirmed) {
      deleteShipmentFromStorage(shipmentId);
      setShipments(prev => prev.filter(s => s.shipmentId !== shipmentId));

      // Clear active shipment if it was the deleted one
      if (activeShipmentId === shipmentId) {
        setActiveShipmentId(null);
      }
    }
  };

  // SCREEN 1: Resume existing manifest (Play button) - LEGACY
  const handleResumeManifest = (manifest: ManifestData) => {
    setCurrentManifest(manifest);

    // Restore form data
    if (manifest.driverCheckSheet) {
      setDriverCheckSheet(manifest.driverCheckSheet);
    }
    if (manifest.trailerNumber) {
      setTrailerNumber(manifest.trailerNumber);
    }
    if (manifest.sealNumber) {
      setSealNumber(manifest.sealNumber);
    }
    if (manifest.carrierName) {
      setCarrierName(manifest.carrierName);
    }
    if (manifest.driverName) {
      setDriverName(manifest.driverName);
    }
    if (manifest.driverLicense) {
      setDriverLicense(manifest.driverLicense);
    }
    if (manifest.departureDate) {
      setDepartureDate(manifest.departureDate);
    }

    // Determine which screen to go to based on progress
    if (manifest.status === 'completed') {
      setCurrentScreen(5);
    } else if (manifest.trailerNumber) {
      setCurrentScreen(3);
    } else if (manifest.driverCheckSheet) {
      setCurrentScreen(3);
    } else {
      setCurrentScreen(2);
    }
  };

  // SCREEN 1: Delete manifest - LEGACY
  const handleDeleteManifest = (manifestNumber: string) => {
    const confirmed = window.confirm(
      `Are you sure you want to delete this manifest?\n\nManifest: ${manifestNumber}`
    );

    if (confirmed) {
      localStorage.removeItem(`${STORAGE_PREFIX}${manifestNumber}`);
      setSavedManifests(prevManifests =>
        prevManifests.filter(m => m.manifestNumber !== manifestNumber)
      );
    }
  };

  /**
   * Parse Pickup Route QR Code
   * Fixed-Position Format (50 characters):
   * Example: "02TMIHL56408   2024021301     IDZE06Load202402121411"
   *
   * Position Map:
   * - Pos 2-7: Plant Code (TMIHL)
   * - Pos 5-7: Dock Code (HL)
   * - Pos 7-12: Supplier (56408)
   * - Pos 15-23: Order Date (20240213)
   * - Pos 23-25: Sequence (01)
   * - Pos 30-36: Route (IDZE06)
   * - Pos 36-40: Load Type (Load)
   * - Pos 40-48: Pickup Date (20240212)
   * - Pos 48-52: Pickup Time (1411)
   *
   * Author: Hassan, 2025-11-05
   */
  const parsePickupRouteQR = (qrValue: string): PickupRouteData | null => {
    try {
      // Expected length: 50 characters
      if (qrValue.length < 50) {
        console.error('Invalid pickup route QR format. Expected 50-character format.');
        return null;
      }

      const plantCode = qrValue.substring(2, 7).trim();      // Pos 2-7: TMIHL
      const dockCode = qrValue.substring(5, 7).trim();       // Pos 5-7: HL
      const supplierCode = qrValue.substring(7, 12).trim();  // Pos 7-12: 56408
      const orderDate = qrValue.substring(15, 23).trim();    // Pos 15-23: 20240213
      const sequence = qrValue.substring(23, 25).trim();     // Pos 23-25: 01
      const route = qrValue.substring(30, 36).trim();        // Pos 30-36: IDZE06
      const loadType = qrValue.substring(36, 40).trim();     // Pos 36-40: Load
      const pickupDate = qrValue.substring(40, 48).trim();   // Pos 40-48: 20240212
      const pickupTime = qrValue.substring(48, 52).trim();   // Pos 48-52: 1411

      const fullOrderNumber = orderDate + sequence;          // 2024021301

      // For estimatedSkids, we'll use a default value since it's not in the QR
      // This will be replaced with actual planned skid data from the system
      const estimatedSkids = 5; // Default value

      if (!plantCode || !dockCode || !supplierCode || !route) {
        console.error('Invalid pickup route data - missing required fields');
        return null;
      }

      return {
        routeNumber: route,
        plant: plantCode,
        supplierCode,
        dockCode,
        estimatedSkids,
        rawQRValue: qrValue,
      };
    } catch (error) {
      console.error('Error parsing pickup route QR:', error);
      return null;
    }
  };

  /**
   * Parse Toyota Manifest QR Code (44 characters)
   * Copied from shipment-load
   * Author: Hassan, 2025-11-05
   * Updated: 2025-11-05 - Fixed to extract last 8 characters as skid ID
   */
  const parseToyotaManifest = (qr: string): string | null => {
    if (qr.length < 44) {
      console.error('Invalid Toyota Manifest - expected 44 characters, got:', qr.length);
      return null;
    }

    try {
      // Extract the last 8 characters as the skid ID (e.g., "LB05001D")
      const skidId = qr.substring(qr.length - 8).trim();

      console.log('Parsed Skid ID:', skidId);
      return skidId;
    } catch (error) {
      console.error('Error parsing Toyota Manifest:', error);
      return null;
    }
  };

  /**
   * Screen 4: Handle Skid Scan
   * Author: Hassan, 2025-11-05
   */
  const handleSkidScan = (result: ScanResult) => {
    console.log('Skid scan result:', result);

    if (!result.success) {
      setError(result.error || 'Scan failed');
      return;
    }

    const scannedValue = result.scannedValue;
    const parsedSkidId = parseToyotaManifest(scannedValue);

    if (!parsedSkidId) {
      setError('Invalid Toyota Manifest. Please scan the correct label.');
      return;
    }

    // Check if already scanned
    const alreadyScanned = scannedSkids.some(item => item.skidId === parsedSkidId);
    if (alreadyScanned) {
      setError(`Skid ${parsedSkidId} has already been scanned.`);
      return;
    }

    // Find in planned list
    const plannedSkidIndex = plannedSkids.findIndex(
      skid => skid.skidId === parsedSkidId && !skid.isScanned
    );

    if (plannedSkidIndex === -1) {
      setError(`Skid ${parsedSkidId} not found in planned list or already scanned.`);
      return;
    }

    const plannedSkid = plannedSkids[plannedSkidIndex];

    // Create scanned item
    const newScannedSkid: ScannedSkid = {
      id: `${Date.now()}-${Math.random()}`,
      skidId: plannedSkid.skidId,
      orderNumber: plannedSkid.orderNumber,
      partCount: plannedSkid.partCount,
      destination: plannedSkid.destination,
      timestamp: new Date().toISOString(),
    };

    // Update state
    setPlannedSkids(plannedSkids.map((skid, idx) =>
      idx === plannedSkidIndex ? { ...skid, isScanned: true } : skid
    ));
    setScannedSkids([...scannedSkids, newScannedSkid]);
    setError(null);
  };

  /**
   * Screen 4: Add Exception Handler
   * Author: Hassan, 2025-11-05
   */
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

  /**
   * Screen 4: Remove Exception Handler
   * Author: Hassan, 2025-11-05
   */
  const handleRemoveException = (index: number) => {
    console.log('Removing exception at index:', index);
    setExceptions(exceptions.filter((_, i) => i !== index));
  };

  /**
   * Screen 4: Final Submit with Validation
   * Author: Hassan, 2025-11-05
   */
  const handleFinalSubmit = () => {
    console.log('=== SUBMIT VALIDATION ===');
    console.log('Planned Skids:', plannedSkids);
    console.log('Scanned Skids:', scannedSkids);
    console.log('Exceptions:', exceptions);

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

    // Generate confirmation number
    const confirmationNumber = `PS${Date.now()}`;

    // Update active shipment as completed
    if (activeShipmentId) {
      setShipments(prev => prev.map(shipment => {
        if (shipment.shipmentId === activeShipmentId) {
          const updatedShipment: Shipment = {
            ...shipment,
            status: 'completed',
            currentScreen: 5,
            driverInfo: {
              driverCheckSheet,
              driverName,
              driverLicense,
            },
            trailerInfo: {
              trailerNumber,
              sealNumber,
              carrierName,
              departureDate: new Date().toISOString(),
            },
            confirmationNumber,
          };
          saveShipmentToStorage(updatedShipment);
          return updatedShipment;
        }
        return shipment;
      }));
    }

    setLoading(false);
    setCurrentScreen(5);
  };

  /**
   * Screen 2: Handle Pickup Route QR Scan
   * Author: Hassan, 2025-11-05
   */
  const handlePickupRouteScan = (result: ScanResult) => {
    console.log('Pickup route scan result:', result);

    const parsedData = parsePickupRouteQR(result.scannedValue);

    if (!parsedData) {
      setError('Invalid Pickup Route QR Code. Please scan the correct barcode.');
      return;
    }

    // Successfully parsed
    setPickupRouteData(parsedData);
    setError(null);
  };

  // SCREEN 2: Handle Driver Check Sheet scan
  const handleDriverCheckScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    setError(null);
    setDriverCheckSheet(result.scannedValue);

    // Update active shipment with driver info
    if (activeShipmentId) {
      setShipments(prev => prev.map(shipment => {
        if (shipment.shipmentId === activeShipmentId) {
          const updatedShipment = {
            ...shipment,
            currentScreen: 2,
            driverInfo: {
              driverCheckSheet: result.scannedValue,
              driverName: driverName || '',
              driverLicense: driverLicense || '',
            },
          };
          saveShipmentToStorage(updatedShipment);
          return updatedShipment;
        }
        return shipment;
      }));
    }

    // Update current manifest (legacy)
    if (currentManifest) {
      setCurrentManifest({
        ...currentManifest,
        driverCheckSheet: result.scannedValue,
        status: 'in-progress',
      });
    }
  };



  // SCREEN 5: Continue (return to Screen 1)
  const handleContinue = () => {
    // Reset form
    setCurrentManifest(null);
    setActiveShipmentId(null);
    setDriverCheckSheet('');
    setTrailerNumber('');
    setSealNumber('');
    setCarrierName('');
    setDriverName('');
    setDriverLicense('');
    setDepartureDate('');
    setNotes('');
    setPickupRouteData(null);
    setPlannedSkids([]);
    setScannedSkids([]);
    setExceptions([]);
    setError(null);
    setCurrentScreen(1);
    setSavedManifests(getAllManifestsFromStorage());
    setShipments(getAllShipmentsFromStorage());
  };

  // Cancel and return to dashboard
  const handleCancel = () => {
    router.push('/');
  };

  // Get status badge variant
  const getStatusBadgeVariant = (status: ManifestStatus): 'success' | 'warning' | 'default' => {
    switch (status) {
      case 'completed':
        return 'success';
      case 'in-progress':
        return 'warning';
      default:
        return 'default';
    }
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-4 pt-24 max-w-3xl mx-auto space-y-3">
          {/* Progress Indicator - Simple "Screen X of 5" format */}
          {currentScreen !== 5 && (
            <Card>
              <CardContent className="p-2">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-medium" style={{ color: '#253262' }}>
                    Screen {currentScreen} of 5
                  </span>
                  <div className="flex gap-1">
                    {[1, 2, 3, 4, 5].map((step) => (
                      <div
                        key={step}
                        className="w-6 h-1 rounded-full"
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

          {/* SCREEN 1: Pre-scan Manifests - Multiple Shipments Support */}
          {currentScreen === 1 && (
            <>
              {/* Active Shipment Indicator */}
              {activeShipmentId && (
                <Card style={{ backgroundColor: '#E8F5E9' }}>
                  <CardContent className="p-3 space-y-3">
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <i className="fa fa-truck text-lg" style={{ color: '#2E7D32' }}></i>
                        <div>
                          <p className="text-xs text-gray-600">Active Shipment</p>
                          <p className="font-mono font-bold text-sm" style={{ color: '#2E7D32' }}>
                            {activeShipmentId}
                          </p>
                        </div>
                      </div>
                      <div className="text-right">
                        <p className="text-xs text-gray-600">Manifests</p>
                        <p className="font-bold text-lg" style={{ color: '#2E7D32' }}>
                          {shipments.find(s => s.shipmentId === activeShipmentId)?.manifests.length || 0}
                        </p>
                      </div>
                    </div>

                    {/* Show manifests list if any */}
                    {shipments.find(s => s.shipmentId === activeShipmentId)?.manifests &&
                     shipments.find(s => s.shipmentId === activeShipmentId)!.manifests.length > 0 && (
                      <div>
                        <p className="text-xs text-gray-600 mb-1">Scanned Manifests:</p>
                        <div className="flex flex-wrap gap-1">
                          {shipments
                            .find(s => s.shipmentId === activeShipmentId)!
                            .manifests.map((manifestId, idx) => (
                              <span
                                key={idx}
                                className="px-2 py-1 bg-white rounded text-xs font-mono"
                                style={{ color: '#2E7D32', border: '1px solid #2E7D32' }}
                              >
                                {manifestId}
                              </span>
                            ))}
                        </div>
                      </div>
                    )}

                    {/* Proceed Button - Show if at least one manifest is scanned */}
                    {shipments.find(s => s.shipmentId === activeShipmentId)?.manifests.length! > 0 && (
                      <Button
                        onClick={() => setCurrentScreen(2)}
                        variant="success"
                        fullWidth
                        style={{ backgroundColor: '#2E7D32' }}
                      >
                        <i className="fa fa-arrow-right mr-2"></i>
                        Proceed with {shipments.find(s => s.shipmentId === activeShipmentId)?.manifests.length} Manifest(s)
                      </Button>
                    )}
                  </CardContent>
                </Card>
              )}

              {/* Scan Input with Header */}
              <Card style={{ backgroundColor: '#FCFCFC' }}>
                <CardContent className="p-3 space-y-3">
                  {/* Header */}
                  <div className="flex items-center gap-3 pb-2 border-b border-gray-200">
                    <i className="fa fa-barcode text-2xl" style={{ color: '#253262' }}></i>
                    <div>
                      <h2 className="text-lg font-bold" style={{ color: '#253262' }}>
                        Scan Manifest
                      </h2>
                      <p className="text-xs text-gray-600">
                        {activeShipmentId
                          ? 'Scan manifests to add to active shipment'
                          : 'Create a new shipment or scan a manifest to start'}
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

              {/* Shipments Table */}
              {shipments.length > 0 && (
                <Card style={{ backgroundColor: '#FCFCFC' }}>
                  <CardHeader>
                    <CardTitle>Shipments</CardTitle>
                  </CardHeader>
                  <CardContent className="p-0">
                    <div className="overflow-x-auto">
                      <table className="w-full text-sm">
                        <thead className="bg-gray-100 border-b border-gray-200">
                          <tr>
                            <th className="px-3 py-2 text-left font-semibold" style={{ color: '#253262' }}>
                              Shipment ID
                            </th>
                            <th className="px-3 py-2 text-left font-semibold hidden sm:table-cell" style={{ color: '#253262' }}>
                              Manifests
                            </th>
                            <th className="px-3 py-2 text-left font-semibold hidden sm:table-cell" style={{ color: '#253262' }}>
                              Date
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
                          {shipments.map((shipment) => (
                            <tr
                              key={shipment.shipmentId}
                              className={`hover:bg-gray-50 ${shipment.shipmentId === activeShipmentId ? 'bg-blue-50' : ''}`}
                            >
                              <td className="px-3 py-2 font-mono text-xs">
                                {shipment.shipmentId}
                              </td>
                              <td className="px-3 py-2 hidden sm:table-cell">
                                <div className="flex items-center gap-1">
                                  <i className="fa fa-cube text-xs" style={{ color: '#253262' }}></i>
                                  <span className="font-semibold">{shipment.manifests.length}</span>
                                </div>
                              </td>
                              <td className="px-3 py-2 text-xs text-gray-600 hidden sm:table-cell">
                                {new Date(shipment.createdAt).toLocaleDateString()}
                              </td>
                              <td className="px-3 py-2">
                                <Badge variant={shipment.status === 'completed' ? 'success' : 'warning'}>
                                  {shipment.status}
                                </Badge>
                              </td>
                              <td className="px-3 py-2">
                                <div className="flex items-center gap-2">
                                  {/* Play/Resume Button - Only for in-progress shipments */}
                                  {shipment.status !== 'completed' && (
                                    <button
                                      onClick={() => handleResumeShipment(shipment)}
                                      className="p-2 rounded-md hover:bg-gray-100 transition-colors"
                                      style={{ color: '#253262' }}
                                      title="Resume shipment"
                                      aria-label="Resume shipment"
                                    >
                                      <i className="fa fa-play text-lg"></i>
                                    </button>
                                  )}

                                  {/* Delete Button - Available for ALL shipments (both in-progress and completed) */}
                                  <button
                                    onClick={() => handleDeleteShipment(shipment.shipmentId)}
                                    className="p-2 rounded-md hover:bg-red-50 transition-colors"
                                    style={{ color: '#D2312E' }}
                                    title="Delete shipment"
                                    aria-label="Delete shipment"
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

              {/* No shipments message */}
              {shipments.length === 0 && (
                <Card style={{ backgroundColor: '#FCFCFC' }}>
                  <CardContent className="p-6 text-center text-gray-500">
                    <i className="fa fa-inbox text-4xl mb-2" style={{ color: '#253262', opacity: 0.3 }}></i>
                    <p>No shipments found</p>
                    <p className="text-sm mt-1">Click "New Shipment" to begin</p>
                  </CardContent>
                </Card>
              )}
            </>
          )}

          {/* SCREEN 2: Scan Pickup Route QR (Copied EXACTLY from shipment-load Screen 1) */}
          {currentScreen === 2 && (activeShipmentId || currentManifest) && (
            <Card style={{ backgroundColor: '#FCFCFC' }}>
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
                    {activeShipmentId && (
                      <p className="text-xs font-mono mt-1" style={{ color: '#253262' }}>
                        Shipment: {activeShipmentId}
                      </p>
                    )}
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
                          <span className="text-gray-600">Route Number:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.routeNumber}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Plant:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.plant}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Supplier Code:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.supplierCode}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Dock Code:</span>
                          <p className="font-mono font-bold text-gray-900">{pickupRouteData.dockCode}</p>
                        </div>
                        <div className="col-span-2">
                          <span className="text-gray-600">Estimated Skids:</span>
                          <p className="font-bold text-lg text-gray-900">{pickupRouteData.estimatedSkids}</p>
                        </div>
                      </div>

                      {/* Continue Button */}
                      <div className="mt-3">
                        <Button
                          onClick={() => {
                            setError(null);

                            // Use real manifest IDs from Pickup Route for planned skids
                            // Author: Hassan, 2025-11-05
                            const plannedSkidsData: PlannedSkid[] = [
                              { skidId: 'LB05001A', orderNumber: '681010E250', partCount: 45, destination: `Dock ${pickupRouteData.dockCode}`, isScanned: false },
                              { skidId: 'LB05001B', orderNumber: '681020F150', partCount: 30, destination: `Dock ${pickupRouteData.dockCode}`, isScanned: false },
                              { skidId: 'LB05001C', orderNumber: '692050G200', partCount: 60, destination: `Dock ${pickupRouteData.dockCode}`, isScanned: false },
                              { skidId: 'LB05001D', orderNumber: '693060H300', partCount: 35, destination: `Dock ${pickupRouteData.dockCode}`, isScanned: false },
                              { skidId: 'LB05001E', orderNumber: '694070J400', partCount: 50, destination: `Dock ${pickupRouteData.dockCode}`, isScanned: false },
                            ];

                            setPlannedSkids(plannedSkidsData);
                            setCurrentScreen(3);

                            // Update active shipment with pickup route data
                            if (activeShipmentId) {
                              setShipments(prev => prev.map(shipment => {
                                if (shipment.shipmentId === activeShipmentId) {
                                  const updatedShipment = {
                                    ...shipment,
                                    currentScreen: 3,
                                  };
                                  saveShipmentToStorage(updatedShipment);
                                  return updatedShipment;
                                }
                                return shipment;
                              }));
                            }
                          }}
                          variant="success-light"
                          fullWidth
                        >
                          <i className="fa fa-arrow-right mr-2"></i>
                          Continue to Trailer Information
                        </Button>
                      </div>
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* SCREEN 3: Trailer Information Form (EXACTLY from shipment-load Screen 2) */}
          {currentScreen === 3 && (activeShipmentId || currentManifest) && (
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
                    Seal Number *
                  </label>
                  <input
                    type="text"
                    value={sealNumber}
                    onChange={(e) => setSealNumber(e.target.value)}
                    placeholder="Enter seal number"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Carrier Name */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Carrier Name
                  </label>
                  <input
                    type="text"
                    value={carrierName}
                    onChange={(e) => setCarrierName(e.target.value)}
                    placeholder="Enter carrier name (optional)"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Driver Name */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Driver Name
                  </label>
                  <input
                    type="text"
                    value={driverName}
                    onChange={(e) => setDriverName(e.target.value)}
                    placeholder="Enter driver name (optional)"
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Notes */}
                <div>
                  <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                    Notes
                  </label>
                  <textarea
                    value={notes}
                    onChange={(e) => setNotes(e.target.value)}
                    placeholder="Additional notes (optional)"
                    rows={3}
                    className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 resize-none"
                    style={{ backgroundColor: '#FCFCFC' }}
                  />
                </div>

                {/* Continue Button */}
                <div className="flex gap-2 pt-2">
                  <Button
                    onClick={() => setCurrentScreen(2)}
                    variant="secondary"
                    fullWidth
                  >
                    <i className="fa fa-arrow-left mr-2"></i>
                    Back
                  </Button>
                  <Button
                    onClick={() => {
                      // Validate required fields
                      if (!trailerNumber.trim() || !sealNumber.trim()) {
                        setError('Trailer Number and Seal Number are required');
                        return;
                      }

                      // Initialize scannedSkids with manifests from Screen 1
                      // Fixed: Match with plannedSkids to get correct part counts
                      // Author: Hassan, 2025-11-06
                      // Updated: 2025-11-06 - CRITICAL BUG FIX: Mark pre-scanned manifests as isScanned in plannedSkids
                      if (activeShipmentId) {
                        const activeShip = shipments.find(s => s.shipmentId === activeShipmentId);
                        if (activeShip && activeShip.manifests && activeShip.manifests.length > 0) {
                          // Convert manifests from Screen 1 into scannedSkids format with correct part counts
                          const preScannedSkids: ScannedSkid[] = activeShip.manifests.map((manifestId, index) => {
                            // Find matching planned skid to get the correct part count, order number, and destination
                            const matchingPlannedSkid = plannedSkids.find(ps => ps.skidId === manifestId);

                            return {
                              id: `pre-scanned-${Date.now()}-${index}`,
                              skidId: manifestId,
                              orderNumber: matchingPlannedSkid?.orderNumber || `PRE-${activeShip.shipmentId.slice(-6)}`,
                              partCount: matchingPlannedSkid?.partCount || 0,
                              destination: matchingPlannedSkid?.destination || (pickupRouteData ? `Dock ${pickupRouteData.dockCode}` : 'Pre-scanned'),
                              timestamp: new Date().toISOString(),
                            };
                          });

                          setScannedSkids(preScannedSkids);

                          // CRITICAL FIX: Mark the pre-scanned skids as isScanned in plannedSkids
                          setPlannedSkids(prevPlanned =>
                            prevPlanned.map(skid => ({
                              ...skid,
                              isScanned: activeShip.manifests.includes(skid.skidId) ? true : skid.isScanned
                            }))
                          );

                          console.log('Initialized Screen 4 with pre-scanned manifests:', preScannedSkids);
                        }
                      }

                      setCurrentScreen(4);
                      setError(null);

                      // Update active shipment
                      if (activeShipmentId) {
                        setShipments(prev => prev.map(shipment => {
                          if (shipment.shipmentId === activeShipmentId) {
                            const updatedShipment = {
                              ...shipment,
                              currentScreen: 4,
                              trailerInfo: {
                                trailerNumber,
                                sealNumber,
                                carrierName,
                                departureDate: new Date().toISOString(),
                              }
                            };
                            saveShipmentToStorage(updatedShipment);
                            return updatedShipment;
                          }
                          return shipment;
                        }));
                      }
                    }}
                    variant="success"
                    fullWidth
                    disabled={!trailerNumber.trim() || !sealNumber.trim()}
                  >
                    <i className="fa fa-arrow-right mr-2"></i>
                    Continue to Skid Scanning
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {/* SCREEN 4: Skid Scanning (PLANNED/SCANNED Split) - Copied from shipment-load Screen 3 */}
          {currentScreen === 4 && (
            <>
              {/* Loading Progress - Collapsible */}
              {scannedSkids.length > 0 && (
                <Card style={{ backgroundColor: '#E8F5E9' }}>
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
                            Scanned: {scannedSkids.length}/{pickupRouteData?.estimatedSkids || plannedSkids.length} | Parts: {scannedSkids.reduce((total, skid) => total + skid.partCount, 0)}
                          </p>
                        </div>
                      </div>
                      <i className={`fa fa-chevron-${expandedSection === 'progress' ? 'up' : 'down'}`} style={{ color: '#2E7D32' }}></i>
                    </div>
                  </CardHeader>
                  {expandedSection === 'progress' && (
                    <CardContent className="p-3">
                      <div className="grid grid-cols-3 gap-3 pb-3 border-b border-green-200">
                        <div>
                          <p className="text-xs text-gray-600">Scanned</p>
                          <p className="font-bold text-lg" style={{ color: '#2E7D32' }}>
                            {scannedSkids.length}/{pickupRouteData?.estimatedSkids || plannedSkids.length}
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
                          <p className="font-bold text-lg" style={{ color: scannedSkids.length >= (pickupRouteData?.estimatedSkids || plannedSkids.length) ? '#2E7D32' : '#253262' }}>
                            {Math.max(0, (pickupRouteData?.estimatedSkids || plannedSkids.length) - scannedSkids.length)}
                          </p>
                        </div>
                      </div>
                      {/* Show pre-scanned manifest IDs */}
                      {activeShipmentId && (
                        <div className="mt-3 pt-3 border-t border-green-200">
                          <p className="text-xs text-gray-600 mb-2">Pre-scanned from Screen 1:</p>
                          <div className="flex flex-wrap gap-1">
                            {shipments.find(s => s.shipmentId === activeShipmentId)?.manifests.map((manifestId, idx) => (
                              <span
                                key={idx}
                                className="px-2 py-0.5 bg-white rounded text-xs font-mono"
                                style={{ color: '#2E7D32', border: '1px solid #2E7D32' }}
                              >
                                {manifestId}
                              </span>
                            ))}
                          </div>
                        </div>
                      )}
                    </CardContent>
                  )}
                </Card>
              )}

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
                        <span className="font-medium">{trailerNumber}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Seal:</span>
                        <span className="font-medium">{sealNumber}</span>
                      </div>
                    </div>
                  </CardContent>
                )}
              </Card>

              {/* Planned Skids - Collapsible */}
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
                      <p className="text-sm text-gray-500 text-center py-4">
                        All skids scanned!
                      </p>
                    ) : (
                      plannedSkids
                        .filter(skid => !skid.isScanned)
                        .map((skid, idx) => (
                          <div
                            key={idx}
                            className="p-3 border border-gray-200 rounded-lg"
                            style={{ backgroundColor: '#FFFFFF' }}
                          >
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

              {/* Scanned Skids - Collapsible */}
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
                      <p className="text-sm text-gray-500 text-center py-4">
                        No skids scanned yet
                      </p>
                    ) : (
                      scannedSkids.map((skid, idx) => (
                        <div
                          key={skid.id}
                          className="p-3 border-2 border-success-500 rounded-lg relative"
                          style={{ backgroundColor: '#FFFFFF' }}
                        >
                          <div className="flex items-center justify-between">
                            <div className="flex-1">
                              <p className="font-mono text-sm font-bold" style={{ color: '#253262' }}>
                                {skid.skidId}
                              </p>
                              <p className="text-xs text-gray-600">Order: {skid.orderNumber}</p>
                              <p className="text-xs text-gray-600">Parts: {skid.partCount}</p>
                              <p className="text-xs text-gray-500 mt-1">
                                <i className="fa fa-clock mr-1"></i>
                                {new Date(skid.timestamp).toLocaleTimeString()}
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

          {/* SCREEN 5: Success Screen (Copied from shipment-load Screen 4) */}
          {currentScreen === 5 && (
            <div className="flex items-center justify-center min-h-[calc(100vh-140px)]">
              {/* Compact Success Card - Modal Style */}
              <Card className="max-w-md w-full shadow-2xl">
                <CardContent className="p-8 text-center space-y-6">
                  {/* Large Success Icon */}
                  <div className="flex justify-center">
                    <div className="inline-flex items-center justify-center w-24 h-24 rounded-full bg-success-100">
                      <i className="fa fa-circle-check text-6xl text-success-600"></i>
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
                        {activeShipmentId
                          ? shipments.find(s => s.shipmentId === activeShipmentId)?.confirmationNumber
                          : currentManifest?.confirmationNumber}
                      </span>
                    </div>
                  </div>

                  {/* Action Buttons - Stacked Vertically */}
                  <div className="pt-4 space-y-3">
                    {/* Primary Action: Continue - VUTEQ Navy */}
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

          {/* Cancel Button (not shown on success screen) */}
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

      {/* Resume Dialog - Modal Overlay */}
      {showResumeDialog && shipmentToResume && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center p-4 z-50">
          <Card className="max-w-sm w-full shadow-2xl">
            <CardContent className="p-6 space-y-4">
              {/* Icon and Title */}
              <div className="flex items-center gap-3">
                <div className="flex-shrink-0">
                  <i className="fa fa-circle-check text-3xl" style={{ color: '#2E7D32' }}></i>
                </div>
                <div>
                  <h2 className="text-xl font-bold" style={{ color: '#253262' }}>
                    Shipment Found
                  </h2>
                </div>
              </div>

              {/* Message */}
              <div className="space-y-2 py-2">
                <p className="text-sm text-gray-700">
                  This manifest belongs to an existing shipment. Would you like to resume it?
                </p>
                <div className="p-3 bg-blue-50 rounded-lg border border-blue-200">
                  <p className="text-xs text-gray-600">Shipment ID</p>
                  <p className="font-mono font-bold text-sm" style={{ color: '#253262' }}>
                    {shipmentToResume.shipmentId}
                  </p>
                  <p className="text-xs text-gray-600 mt-2">Manifests</p>
                  <p className="font-bold text-sm" style={{ color: '#253262' }}>
                    {shipmentToResume.manifests.length} manifest{shipmentToResume.manifests.length !== 1 ? 's' : ''}
                  </p>
                </div>
              </div>

              {/* Action Buttons */}
              <div className="flex gap-2 pt-2">
                <Button
                  onClick={handleStartNew}
                  variant="secondary"
                  fullWidth
                >
                  <i className="fa fa-plus mr-2"></i>
                  Start New
                </Button>
                <Button
                  onClick={handleResumeExisting}
                  variant="success"
                  fullWidth
                  style={{ backgroundColor: '#2E7D32' }}
                >
                  <i className="fa fa-play mr-2"></i>
                  Resume
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}

      {/* Exception Modal - Popup Dialog (Screen 4) */}
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
            {/* Modal Header */}
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
                aria-label="Close modal"
              >
                <i className="fa fa-xmark text-2xl"></i>
              </button>
            </div>

            {/* Modal Content */}
            <div className="p-6 space-y-4">
              {/* Exception Type Dropdown */}
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

              {/* Which Skid Dropdown - Show unscanned planned items only */}
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
