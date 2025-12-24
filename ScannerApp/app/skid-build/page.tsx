/**
 * Skid Build V2 - Multi-Skid Scanner with Auto-Detection
 * Author: Hassan
 * Date: 2025-12-12
 * Updated: 2025-12-14 - USER AUTHENTICATION FIX:
 *                      - Replaced hardcoded 'user-001' with actual authenticated user ID from AuthContext
 *                      - Added useAuth hook to get current user
 *                      - Added validation to ensure user is authenticated before API calls
 *                      - Fixed 400 error caused by sending string userId instead of Guid
 * Updated: 2025-12-13 - Fixed PLANNED/SCANNED tab display issues
 * Updated: 2025-12-14 - PLACEHOLDER FIX & TOYOTA CONFIRMATION DISPLAY:
 *                      - Fixed placeholder text: "Scan Manifest QR, Toyota Kanban" (removed "or Internal Kanban")
 *                      - Added Toyota confirmation number display after successful submit
 *                      - Added prominent success card with copyable Toyota confirmation number
 *                      - Added warning card for Toyota submission errors (shows error + internal confirmation)
 *                      - Enhanced submit handler to handle Toyota API responses (success/error)
 * Updated: 2025-12-14 - ORDER DETAILS UI ENHANCEMENTS:
 *                      - Changed confirmation badge logic to use toyotaConfirmationNumber instead of confirmationNumber
 *                      - Added "Confirmed" badge with checkmark when Toyota confirmation exists
 *                      - Added separate "Conf# XXX" badge displaying Toyota confirmation number
 *                      - Moved skid information to top badges area (with Order Details header)
 *                      - Enhanced expanded order details to show Toyota confirmation number with copy button
 *                      - Improved layout: badges now wrap on small screens for better mobile UX
 * Updated: 2025-12-13 - Mobile optimization: Scanner instructions tooltip, collapsed sections
 * Updated: 2025-12-13 - CRITICAL FIXES: Collapsible sections now work properly (Order Details, Skid Items)
 *                      - Order Details: Fixed onClick handler, proper CardHeader structure, Confirmed/Unconfirmed badge
 *                      - Skid Items: Collapsible tabs section with scanned/planned badge (STARTS COLLAPSED)
 *                      - All sections now have consistent styling matching original skid-build page
 *                      - Success messages auto-dismiss after 3 seconds
 * Updated: 2025-12-13 - EXCEPTION HANDLING & 4-BUTTON LAYOUT:
 *                      - Added exception functionality (Add, Remove, List)
 *                      - Exception modal with type selection and comments
 *                      - Collapsible Exceptions section (count badge)
 *                      - 4-button layout: Add Exception | Submit | Reset | Cancel
 *                      - Matches skid-build screen 2 button pattern exactly
 * Updated: 2025-12-13 - KANBAN VALIDATION & INTERNAL KANBAN POPUP:
 *                      - Toyota Kanban now validated against currentSkid.planned
 *                      - Internal Kanban scan moved to popup modal
 *                      - Popup shows Toyota Kanban details and input for internal kanban
 *                      - Auto-focus on internal kanban input field
 * Updated: 2025-12-13 - BUG FIXES:
 *                      - BUG 1: Fixed double-counting issue (5 scans showing as 10)
 *                        * Removed scannedQty increment on line 1017
 *                        * scannedQty now ONLY represents "previously scanned from API"
 *                        * Session scans tracked separately in scanned[] array
 *                        * Count calculation: previouslyScanned (scannedQty) + sessionScanned (scanned.length)
 *                      - BUG 2: FIXED PALLETIZATION VALIDATION (BR-014)
 *                        * Now validates MANIFEST's palletization code vs KANBAN's pallet code
 *                        * Fixed multi-skid scenario where wrong manifest was used for validation
 *                        * Added parsedQRData state to track current manifest's parsed data
 *                        * Validates against current manifest (parsedQRData), not stored skid data
 *                        * Matches correct pattern from skid-build page
 * Updated: 2025-12-13 - INTERNAL KANBAN POPUP ERROR HANDLING:
 *                      - Added internalKanbanError state for popup-specific errors
 *                      - Validation errors now shown IN the popup (not on main page)
 *                      - Popup stays open on error, allowing immediate retry
 *                      - Error clears automatically when user types new input
 *                      - No need to re-scan Toyota Kanban on validation failures
 * Updated: 2025-12-13 - MULTI-PALLET SKID MATCHING FIX (lines 844-867):
 *                      - FIXED: Same skid ID with different pallet codes now create separate groups
 *                      - REMOVED: Fallback matching by palletization code in planned items
 *                      - REMOVED: Single-skid fallback matching
 *                      - NOW: Each palletCode-skidId combination gets its own SkidGroup
 *                      - Example: D1-001A, A4-001A, A8-001A are now three separate skid groups
 *                      - Items scanned after manifest go to the correct pallet-skid group
 * Updated: 2025-12-13 - CRITICAL BUG FIXES (lines 992-1004, 1510-1655):
 *                      - BUG 1 FIX: Removed local scannedQty increment in handleInternalKanbanScan
 *                        * scannedQty now ONLY comes from API (represents previously scanned items)
 *                        * Local scans only update scanned[] array
 *                        * Prevents double-counting bug where counts showed doubled values
 *                      - BUG 2 FIX: SCANNED tab now shows ALL items together (unified display)
 *                        * Combines previouslyScanned (from API via scannedQty) + sessionScanned (from scanned[])
 *                        * Generates ScannedItem objects from planned[].scannedQty and internalKanbans[]
 *                        * Displays everything in one unified list grouped by PalletizationCode
 *                        * Removed conditional logic that separated "session only" vs "previous only"
 *                        * Single source of truth for display - no more fragmented views
 *
 * FEATURES:
 * - Multi-skid state management (track multiple skids in one session)
 * - Smart scanner with auto-detection (manifest QR vs Toyota Kanban)
 * - TAB-based UI: PLANNED | SCANNED tabs (collapsible section, starts collapsed)
 * - Grouped by PalletizationCode for PLANNED
 * - Grouped by PalletizationCode | SkidNumber for SCANNED
 * - Individual box numbers visible in scanned items
 * - All parsers copied from existing skid-build page
 * - Expandable/collapsible groups in SCANNED tab
 * - Mobile-optimized: Scanner instructions in tooltip, collapsible sections
 *
 * PARSERS:
 * - parseQRCode: Parses Toyota Manifest QR (44 chars, fixed positions)
 * - parseToyotaKanban: Parses Toyota Kanban QR (200+ chars, fixed positions)
 * - parseInternalKanban: Parses internal kanban format (PART/KANBAN/SERIAL)
 *
 * UI LAYOUT:
 * ┌─────────────────────────────────────────┐
 * │  Order Details [Unconfirmed]        ∨   │  ← Collapsible (CLOSED by default) + badge
 * ├─────────────────────────────────────────┤
 * │  Skid Items [5/10]                  ∨   │  ← Collapsible tabs section (STARTS COLLAPSED)
 * │    ┌──────────┐  ┌──────────┐           │
 * │    │ PLANNED  │  │ SCANNED  │           │  ← TABS
 * │    └──────────┘  └──────────┘           │
 * │  (Tab Content - grouped by Pallet/Skid) │
 * └─────────────────────────────────────────┘
 *
 * FIXES (2025-12-13):
 * - PLANNED tab: Shows "X boxes scanned (Y items completed)" instead of just items
 * - SCANNED tab: Groups are now expandable/collapsible with chevron icons
 * - SCANNED tab: Previously scanned items are also expandable/collapsible
 * - Mobile: Scanner instructions moved to tooltip (info icon)
 * - Mobile: Order Details starts COLLAPSED by default, has Confirmed/Unconfirmed badge
 * - Mobile: Tabs section (Skid Items) starts COLLAPSED by default with scanned/planned badge
 * - Mobile: Success messages auto-dismiss after 3 seconds
 * - Removed: Exceptions section, Debug Info section, Exception modal
 * - Action buttons: Updated to match skid-build screen 2 (fullWidth, gap-2, success/error variants)
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
import { getSkidBuildOrderGrouped, startSkidBuildSession, recordSkidBuildScan, completeSkidBuildSession, addSkidBuildException, deleteSkidBuildException } from '@/lib/api';
import type { ScanResult } from '@/types';

// ===========================
// EXCEPTION TYPES
// ===========================

const EXCEPTION_TYPES = [
  'Revised Quantity (Toyota Quantity Reduction)',
  'Modified Quantity per Box',
  'Supplier Revised Shortage (Short Shipment)',
  'Non-Standard Packaging (Expendable)',
  'Others', // Code 26 - F-GAP-001 fix
];

// ===========================
// TYPE DEFINITIONS
// ===========================

interface Exception {
  id?: string; // Exception ID from backend (for deletion)
  type: string;
  comments: string;
  timestamp: string;
}

interface ParsedQRData {
  plantPrefix: string;
  supplierCode: string;
  dockCode: string;
  orderNumber: string;
  loadId: string;
  palletizationCode: string;
  mros: string;
  rawSkidId: string;        // "001B" - Original 4-char skid ID from QR
  skidNumber: string;       // "001" - First 3 chars (numeric only, for API)
  skidSide: string;         // "B" - 4th char (A or B, for internal tracking)
}

interface ParsedToyotaKanban {
  description: string;
  partNumber: string;
  supplierCode: string;
  dockCode: string;
  quantity: string;
  supplierName: string;
  loadId: string;
  date: string;
  plant: string;
  location: string;
  rawValue: string; // Keep original for matching
  // All additional fields for comprehensive display
  kanbanNumber: string;
  partNumberRepeat: string;
  lineSideAddress: string;
  storeAddress: string;
  loadId1: string;
  loadId2: string;
  planUnloadDate: string;
  shipDate: string;
  shipTime: string;
  controlCode: string;
  deliveryOrder: string;
  boxNumber: string;
  totalBoxes: string;
  plantCode: string;
  route: string;
  containerType: string;
  palletCode: string;
  controlField: string;
  status: string;
  zoneArea: string;
}

interface InternalKanbanParsed {
  toyotaKanban: string;
  internalKanban: string;
  serialNumber: string;
}

interface PlannedItem {
  partNumber: string;
  description: string;
  plannedQty: number;
  scannedQty: number;
  rawKanbanValue?: string; // Store original kanban QR for matching
  skidId: string; // "001A", "002B", etc.
  plannedItemId?: string;  // Add for API
  manifestNo?: number;     // Add for API
  palletizationCode?: string; // Add for API
  internalKanbans?: string[]; // Array of internal kanbans from API (kept for backward compat)
  scanDetails?: Array<{        // NEW - detailed scan information from API
    skidNumber: string;
    boxNumber: number;
    internalKanban: string | null;
    palletizationCode: string | null;
  }>;
}

interface ScannedItem {
  id: string;
  skidId: string; // "001A", "002B", etc.
  toyotaKanban: string;
  internalKanban: string;
  serialNumber: string;
  partNumber: string;
  quantity: number;
  timestamp: string;
  description: string;
  palletizationCode?: string; // Add for grouping
  skidNumber?: string; // "001" - numeric only
  boxNumber?: number; // Box sequence number
  kanbanNumber?: string; // Kanban identifier (FCJR, FCJT, etc.)
}

interface SkidGroup {
  skidId: string;
  manifestNo?: number;
  palletizationCode?: string;
  planned: PlannedItem[];
  scanned: ScannedItem[];
}

// ===========================
// HELPER FUNCTIONS
// ===========================

/**
 * Format date from YYYYMMDD to YYYY/MM/DD
 * @param dateStr - Date string in YYYYMMDD format
 * @returns Formatted date string YYYY/MM/DD or original if invalid
 */
const _formatDate = (dateStr: string): string => {
  if (!dateStr || dateStr.length !== 8) return dateStr;
  const year = dateStr.substring(0, 4);
  const month = dateStr.substring(4, 6);
  const day = dateStr.substring(6, 8);
  return `${year}/${month}/${day}`;
};

/**
 * Format time from HHMM to HH:MM
 * @param timeStr - Time string in HHMM format
 * @returns Formatted time string HH:MM or original if invalid
 */
const _formatTime = (timeStr: string): string => {
  if (!timeStr || timeStr.length !== 4) return timeStr;
  const hours = timeStr.substring(0, 2);
  const minutes = timeStr.substring(2, 4);
  return `${hours}:${minutes}`;
};

// ===========================
// PARSER FUNCTIONS
// ===========================

/**
 * Parse QR Code format: 02TMI02806V82023080205  IDVV01      LB05001B
 * Author: Hassan, Date: 2025-11-04
 * CRITICAL: Uses FIXED POSITION SUBSTRING EXTRACTION - NO REGEX
 */
const parseQRCode = (qrValue: string): ParsedQRData | null => {
  try {
    // Sample: "02TMI02806V82023080205  IDVV01      LB05001B"

    // DO NOT TRIM - Preserve exact character positions
    const scanValue = qrValue;

    console.log('=== MANIFEST SCAN PARSING (FIXED POSITIONS) ===');
    console.log('Raw input:', scanValue);
    console.log('Length:', scanValue.length);

    // Validate minimum length
    if (scanValue.length < 24) {
      console.error('Scan too short:', scanValue.length);
      return null;
    }

    // FIXED POSITION EXTRACTION (0-indexed for JavaScript)
    // Based on sample: 02TMI02806V82023080205  IDVV01      LB05001B
    const plantPrefix = scanValue.substring(0, 5).trim();           // Positions 1-5: "02TMI"
    const supplierCode = scanValue.substring(5, 10).trim();         // Positions 6-10: "02806"
    const dockCode = scanValue.substring(10, 12).trim();            // Positions 11-12: "V8"
    const orderNumber = scanValue.substring(12, 24).trim();         // Positions 13-24: "2023080205  " (trim spaces)
    const loadId = scanValue.substring(24, 36).trim();              // Positions 25-36: "IDVV01      " (12 chars)
    const palletizationCode = scanValue.substring(36, 38);          // Positions 37-38: "LB"
    const mros = scanValue.substring(38, 40);                       // Positions 39-40: "05" (MROS)
    const rawSkidId = scanValue.substring(40, 44);                  // Positions 41-44: "001B" (raw, 4 chars)

    // Split rawSkidId into skidNumber (first 3 chars) and skidSide (4th char)
    const skidNumber = rawSkidId.substring(0, 3);                   // "001" - 3 numeric digits
    const skidSide = rawSkidId.substring(3, 4);                     // "B" - Side indicator (A or B)

    console.log('Extracted fields (fixed positions):');
    console.log('  plantPrefix (0-5):', `"${plantPrefix}"`);
    console.log('  supplierCode (5-10):', `"${supplierCode}"`);
    console.log('  dockCode (10-12):', `"${dockCode}"`);
    console.log('  orderNumber (12-24):', `"${orderNumber}"`);
    console.log('  loadId (24-36):', `"${loadId}"`);
    console.log('  palletizationCode (36-38):', `"${palletizationCode}"`);
    console.log('  mros (38-40):', `"${mros}"`);
    console.log('  rawSkidId (40-44):', `"${rawSkidId}"`);
    console.log('  skidNumber (first 3):', `"${skidNumber}"`);
    console.log('  skidSide (4th char):', `"${skidSide}"`);

    // Validate required fields
    if (!plantPrefix || !supplierCode || !dockCode || !orderNumber) {
      console.error('VALIDATION FAILED - Missing required fields');
      return null;
    }

    // Validate skidNumber format (3 numeric digits)
    if (!/^\d{3}$/.test(skidNumber)) {
      console.error('VALIDATION FAILED - SkidNumber must be 3 numeric digits:', skidNumber);
      return null;
    }

    // Validate skidSide (must be A or B)
    if (skidSide && !/^[AB]$/i.test(skidSide)) {
      console.error('VALIDATION FAILED - SkidSide must be A or B:', skidSide);
      return null;
    }

    console.log('✓ Manifest Scan Parsed successfully!');
    console.log('======================');

    return {
      plantPrefix,
      supplierCode,
      dockCode,
      orderNumber,
      loadId,
      palletizationCode,
      mros,
      rawSkidId,
      skidNumber,
      skidSide,
    };
  } catch (e) {
    console.error('Manifest scan parse error:', e);
    return null;
  }
};

/**
 * Parse Toyota Kanban QR code (200+ chars)
 * Author: Hassan, Date: 2025-11-05
 * CRITICAL: Uses FIXED POSITION SUBSTRING EXTRACTION for all fields
 */
const parseToyotaKanban = (qrValue: string): ParsedToyotaKanban | null => {
  try {
    // QR String length: Actual string is 216 characters

    if (!qrValue || qrValue.length < 200) {
      console.error('Toyota Kanban QR too short:', qrValue.length, 'Expected at least 200');
      return null;
    }

    const qrString = qrValue; // NO TRIM - preserve exact positions

    console.log('=== TOYOTA KANBAN PARSING (FIXED POSITIONS) ===');
    console.log('Raw input:', qrString);
    console.log('Length:', qrString.length);

    // FIXED POSITION EXTRACTION (0-indexed for JavaScript substring)
    // CRITICAL: Skip position 0 (the "C"), start at position 1
    const partDescription = qrString.substring(1, 12).trim();        // TEM RH WEST (skip "C" at position 0)
    const partNumber = qrString.substring(12, 22).trim();            // 681010E250
    // skip spaces 22-31
    const supplierCode = qrString.substring(31, 36).trim();          // 02806
    const dockCode = qrString.substring(36, 38).trim();              // V8
    const kanbanNumber = qrString.substring(38, 42).trim();          // VH98
    const partNumberRepeat = qrString.substring(42, 54).trim();      // 681010E25000
    const lineSideAddress = qrString.substring(54, 64).trim();       // SA-FDG
    const storeAddress = qrString.substring(64, 74).trim();          // TV-00A
    const quantity = qrString.substring(74, 79).trim();              // 00045
    const supplierName = qrString.substring(79, 99).trim();          // AGC AUTOMOTIVE (VUTE
    const loadId1 = qrString.substring(99, 108).trim();              // IDVV01
    const loadId2 = qrString.substring(108, 117).trim();             // IDVV01
    const planUnloadDate = qrString.substring(117, 125).trim();      // 20230802
    const shipDate = qrString.substring(125, 133).trim();            // 20230801
    const shipTime = qrString.substring(133, 137).trim();            // 1321
    const controlCode = qrString.substring(137, 139).trim();         // 00
    const deliveryOrder = qrString.substring(139, 149).trim();       // 2023080205
    const boxNumber = qrString.substring(151, 155).trim();           // 0001
    const totalBoxes = qrString.substring(155, 159).trim();          // 0001
    const plantCode = qrString.substring(159, 164).trim();           // 02TMI (5 chars, ONE field)
    const route = qrString.substring(183, 192).trim();               // MROS 05
    const containerType = qrString.substring(193, 195).trim();       // 55
    const palletCode = qrString.substring(195, 197).trim();          // LB
    const controlField = qrString.substring(197, 202).trim();        // XAXXX
    const status = qrString.substring(207, 208).trim();              // 0
    const zoneArea = qrString.substring(209, 211).trim();            // 05

    console.log('Extracted fields (fixed positions):');
    console.log('  partDescription (1-12):', `"${partDescription}"` + ' (SKIPPED position 0 - the "C")');
    console.log('  partNumber (12-22):', `"${partNumber}"`);
    console.log('  supplierCode (31-36):', `"${supplierCode}"`);
    console.log('  dockCode (36-38):', `"${dockCode}"`);
    console.log('  kanbanNumber (38-42):', `"${kanbanNumber}"`);
    console.log('  quantity (74-79):', `"${quantity}"`);
    console.log('  supplierName (79-99):', `"${supplierName}"`);
    console.log('  loadId1 (99-108):', `"${loadId1}"`);
    console.log('  plantCode (159-164):', `"${plantCode}"`);
    console.log('  palletCode (195-197):', `"${palletCode}"`);

    // Use partNumberRepeat (positions 42-53) as PRIMARY part number source
    // This field is RELIABLE across all Toyota Kanban formats
    const effectivePartNumber = partNumberRepeat || partNumber;

    // Validate required fields
    if (!effectivePartNumber || !supplierCode || !dockCode) {
      console.error('VALIDATION FAILED - Missing required fields');
      console.error('  effectivePartNumber:', effectivePartNumber);
      console.error('  supplierCode:', supplierCode);
      console.error('  dockCode:', dockCode);
      return null;
    }

    console.log('✓ Toyota Kanban Parsed successfully!');
    console.log('  Using effectivePartNumber:', effectivePartNumber);
    console.log('======================');

    return {
      partNumber: effectivePartNumber,
      description: partDescription,
      supplierCode,
      dockCode,
      quantity,
      supplierName,
      loadId: loadId1,
      date: shipDate,
      plant: plantCode,
      location: route,
      rawValue: qrValue,
      kanbanNumber,
      partNumberRepeat,
      lineSideAddress,
      storeAddress,
      loadId1,
      loadId2,
      planUnloadDate,
      shipDate,
      shipTime,
      controlCode,
      deliveryOrder,
      boxNumber,
      totalBoxes,
      plantCode,
      route,
      containerType,
      palletCode,
      controlField,
      status,
      zoneArea,
    };
  } catch (error) {
    console.error('Error parsing Toyota Kanban:', error);
    return null;
  }
};

/**
 * Parse Internal Kanban format: PART/KANBAN/SERIAL
 * Author: Hassan
 */
const parseInternalKanban = (scanned: string): InternalKanbanParsed | null => {
  const parts = scanned.split('/');

  if (parts.length !== 3) {
    return null;
  }

  const toyotaKanban = parts[0]?.trim();
  const internalKanban = parts[1]?.trim();
  const serialNumber = parts[2]?.trim();

  if (!toyotaKanban || !internalKanban || !serialNumber) {
    return null;
  }

  return {
    toyotaKanban,
    internalKanban,
    serialNumber,
  };
};

/**
 * Validate that palletization codes match between Manifest QR and Toyota Kanban
 * Author: Hassan, Date: 2025-12-13
 * @param manifestPalletization - Palletization code from Manifest QR (pos 36-38, 2 chars)
 * @param kanbanPalletization - Palletization code from Toyota Kanban (pos 195-197, 2 chars)
 * @returns true if codes match, false otherwise
 */
const validatePalletizationMatch = (
  manifestPalletization: string,
  kanbanPalletization: string
): boolean => {
  if (!manifestPalletization || !kanbanPalletization) {
    console.warn('Palletization code validation skipped - one or both codes are empty');
    return true; // Skip validation if either is empty
  }

  const match = manifestPalletization.trim().toUpperCase() === kanbanPalletization.trim().toUpperCase();

  if (!match) {
    console.error(
      `Palletization code mismatch. Manifest: "${manifestPalletization}", Kanban: "${kanbanPalletization}"`
    );
  } else {
    console.log(`✓ Palletization codes match: "${manifestPalletization}"`);
  }

  return match;
};

// ===========================
// MAIN COMPONENT
// ===========================

export default function SkidBuildV2Page() {
  const router = useRouter();
  const { user } = useAuth();

  // State
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Order info
  const [orderNumber, setOrderNumber] = useState('');
  const [currentOrderData, setCurrentOrderData] = useState<ParsedQRData | null>(null);
  const [orderLoaded, setOrderLoaded] = useState(false); // Track if we've fetched order data

  // API session state - for persisting scans to database
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [_orderId, setOrderId] = useState<string | null>(null);

  // Multi-skid state
  const [skidGroups, setSkidGroups] = useState<SkidGroup[]>([]);
  const [currentSkidId, setCurrentSkidId] = useState<string | null>(null);

  // Pending Toyota Kanban (waiting for internal kanban scan)
  const [pendingToyotaKanban, setPendingToyotaKanban] = useState<ParsedToyotaKanban | null>(null);

  // Track scanned internal kanbans to prevent duplicates
  const [scannedInternalKanbans, setScannedInternalKanbans] = useState<string[]>([]);

  // Expandable sections
  const [_expandedSkids, setExpandedSkids] = useState<string[]>([]);

  // Tab state for PLANNED/SCANNED toggle
  type ActiveTab = 'planned' | 'scanned';
  const [activeTab, setActiveTab] = useState<ActiveTab>('planned');

  // Expandable groups within tabs (for scanned items)
  const [expandedGroups, setExpandedGroups] = useState<string[]>([]);

  // Collapsible sections - Mobile optimized: Order Details CLOSED by default
  const [orderDetailsExpanded, setOrderDetailsExpanded] = useState(false);
  const [tabsSectionExpanded, setTabsSectionExpanded] = useState(false); // PLANNED/SCANNED tabs - Start collapsed

  // Auto-dismiss success messages after 3 seconds
  useEffect(() => {
    if (success) {
      const timer = setTimeout(() => setSuccess(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [success]);

  // Scanner instructions tooltip
  const [showScannerInstructions, setShowScannerInstructions] = useState(false);

  // Confirmation number - set when session is completed
  const [confirmationNumber, setConfirmationNumber] = useState<string>('');
  const [toyotaConfirmationNumber, setToyotaConfirmationNumber] = useState<string>('');
  const [toyotaSubmissionError, setToyotaSubmissionError] = useState<string>('');
  const [showSuccessModal, setShowSuccessModal] = useState<boolean>(false);
  const [showWarningModal, setShowWarningModal] = useState<boolean>(false); // Warning modal for Toyota errors - Author: Hassan, Date: 2025-12-14

  // Track current manifest's parsed data for palletization validation
  const [parsedQRData, setParsedQRData] = useState<ParsedQRData | null>(null);

  // Exception handling - Author: Hassan, Date: 2025-12-13
  const [exceptions, setExceptions] = useState<Exception[]>([]);
  const [selectedExceptionType, setSelectedExceptionType] = useState('');
  const [exceptionComments, setExceptionComments] = useState('');
  const [showExceptionModal, setShowExceptionModal] = useState(false);
  const [exceptionsExpanded, setExceptionsExpanded] = useState(false);

  // Internal Kanban Modal - Author: Hassan, Date: 2025-12-13
  const [showInternalKanbanModal, setShowInternalKanbanModal] = useState(false);
  const [internalKanbanInput, setInternalKanbanInput] = useState('');
  const [internalKanbanError, setInternalKanbanError] = useState<string | null>(null);

  // ===========================
  // GROUPING HELPERS
  // ===========================

  /**
   * Group planned items by PalletizationCode only
   * Returns: Map<PalletizationCode, PlannedItem[]>
   */
  const groupPlannedByPalletization = (items: PlannedItem[]) => {
    const grouped = new Map<string, PlannedItem[]>();

    items.forEach(item => {
      const key = item.palletizationCode || 'UNKNOWN';
      if (!grouped.has(key)) {
        grouped.set(key, []);
      }
      grouped.get(key)!.push(item);
    });

    return grouped;
  };

  /**
   * Group scanned items by PalletizationCode + SkidNumber
   * Returns: Map<"PalletizationCode-SkidNumber", ScannedItem[]>
   */
  const groupScannedByPalletAndSkid = (items: ScannedItem[]) => {
    const grouped = new Map<string, ScannedItem[]>();

    items.forEach(item => {
      const key = `${item.palletizationCode || 'UNKNOWN'}-${item.skidNumber || '000'}`;
      if (!grouped.has(key)) {
        grouped.set(key, []);
      }
      grouped.get(key)!.push(item);
    });

    return grouped;
  };

  /**
   * Toggle group expansion in scanned tab
   */
  const toggleGroupExpansion = (groupKey: string) => {
    setExpandedGroups(prev => {
      if (prev.includes(groupKey)) {
        return prev.filter(k => k !== groupKey);
      } else {
        return [...prev, groupKey];
      }
    });
  };

  // ===========================
  // HANDLERS
  // ===========================

  /**
   * Smart scanner that auto-detects manifest QR vs Toyota Kanban
   * Author: Hassan, Date: 2025-12-13
   * Updated: Internal Kanban scanning now happens in popup modal only
   */
  const handleScan = async (result: ScanResult) => {
    setError(null);
    setSuccess(null);

    const scannedValue = result.scannedValue.trim();

    // If Internal Kanban modal is open, ignore main scanner (user should scan in modal)
    if (showInternalKanbanModal) {
      setError('Please scan Internal Kanban in the popup window');
      return;
    }

    // Auto-detect: Manifest QR (short) vs Toyota Kanban (long)
    // Internal Kanban scanning is now handled by the popup modal
    if (scannedValue.length < 100) {
      // Short scan = Manifest QR
      handleManifestScan(scannedValue);
    } else {
      // Long scan = Toyota Kanban (will validate and open popup)
      handleToyotaKanbanScan(scannedValue);
    }
  };

  /**
   * Handle Manifest QR scan
   * - FIRST scan: Fetch order data from API, load all planned items grouped by manifest
   * - Subsequent scans: Switch to that skid
   */
  const handleManifestScan = async (scannedValue: string) => {
    const parsed = parseQRCode(scannedValue);

    if (!parsed) {
      setError('Invalid Manifest QR code format');
      return;
    }

    // Store parsed manifest data for palletization validation
    setParsedQRData(parsed);

    const skidId = parsed.rawSkidId; // e.g., "001A"

    // FIRST MANIFEST SCAN OR NEW ORDER - Fetch order data from API
    const isNewOrder = orderNumber && orderNumber !== parsed.orderNumber;
    if (!orderLoaded || isNewOrder) {
      setLoading(true);
      setError(null);

      // Reset states when loading a new order
      if (isNewOrder) {
        setSkidGroups([]);
        setCurrentSkidId(null);
        setSessionId(null);
        setOrderLoaded(false);
        setCurrentOrderData(null);
        setOrderId(null);
      }

      try {
        const result = await getSkidBuildOrderGrouped(parsed.orderNumber, parsed.dockCode);

        console.log('=== API RESPONSE DEBUG ===');
        console.log('Requested:', { orderNumber: parsed.orderNumber, dockCode: parsed.dockCode });
        console.log('Result success:', result.success);
        console.log('Result error:', (result as any).error);
        console.log('Result data:', JSON.stringify(result.data, null, 2));

        if (!result.success || !result.data) {
          setError((result as any).error || 'Failed to fetch order');
          setLoading(false);
          return;
        }

        const orderData = result.data;

        // SIMPLE: Load ALL items into ONE skid - SAME AS SKID-BUILD
        const allPlannedItems: PlannedItem[] = [];
        orderData.skids.forEach(skid => {
          skid.plannedKanbans.forEach(item => {
            allPlannedItems.push({
              skidId: skid.skidId,
              partNumber: item.partNumber,
              description: item.kanbanNumber || 'N/A',
              plannedQty: item.totalBoxPlanned || 1,
              scannedQty: item.scannedCount || 0,
              plannedItemId: item.plannedItemId,
              manifestNo: item.manifestNo,
              palletizationCode: item.palletizationCode,
              internalKanbans: (item as any).internalKanbans || [],
              scanDetails: item.scanDetails || [],  // MAP scanDetails from API
            });
          });
        });

        // ONE skid with ALL items - like skid-build does
        const displaySkidId = `${parsed.palletizationCode}-${skidId}`;
        const singleSkid: SkidGroup = {
          skidId: displaySkidId,
          manifestNo: orderData.skids[0]?.manifestNo,
          palletizationCode: parsed.palletizationCode,
          planned: allPlannedItems,  // ALL ITEMS HERE
          scanned: [],
        };

        console.log('=== LOADED ALL ITEMS ===');
        console.log('Total items:', allPlannedItems.length);
        allPlannedItems.forEach(p => console.log(`  - ${p.partNumber} (${p.palletizationCode})`));

        setSkidGroups([singleSkid]);
        setOrderLoaded(true);
        setCurrentOrderData(parsed);
        setOrderNumber(parsed.orderNumber);
        setOrderId(orderData.orderId);

        // Extract Toyota fields from API response (if order was already submitted)
        // Author: Hassan, Date: 2025-12-14
        if (orderData.toyotaSkidBuildConfirmationNumber) {
          setToyotaConfirmationNumber(orderData.toyotaSkidBuildConfirmationNumber);
        }
        if (orderData.toyotaSkidBuildErrorMessage) {
          setToyotaSubmissionError(orderData.toyotaSkidBuildErrorMessage);
          setShowWarningModal(true); // Show warning modal
        }

        // Start session
        if (!user?.id) {
          setError('User not authenticated. Please log in again.');
          return;
        }

        const sessionResponse = await startSkidBuildSession(
          orderData.orderId,
          1,
          user.id
        );

        if (sessionResponse.success && sessionResponse.data) {
          setSessionId(sessionResponse.data.sessionId);
        }

        setCurrentSkidId(displaySkidId);
        setExpandedSkids([displaySkidId]);
        setParsedQRData(parsed);  // Store for palletization validation
        setSuccess(`Order loaded: ${allPlannedItems.length} items`);
      } catch (err) {
        setError('Error fetching order data');
        console.error(err);
      } finally {
        setLoading(false);
      }
    } else {
      // SUBSEQUENT MANIFEST SCANS - Just update current skid ID and parsedQRData
      // All items are already loaded in the single skid group
      const displaySkidId = `${parsed.palletizationCode}-${skidId}`;

      // Update the existing skid's ID to match scanned manifest
      setSkidGroups(prev => prev.map(s => ({
        ...s,
        skidId: displaySkidId,
        palletizationCode: parsed.palletizationCode,
      })));

      setCurrentSkidId(displaySkidId);
      setParsedQRData(parsed);  // Update for palletization validation
      setSuccess(`Switched to ${displaySkidId}`);
      console.log('Switched manifest to:', displaySkidId);
      setCurrentOrderData(parsed);
    }
  };

  /**
   * Handle Toyota Kanban scan - VALIDATES against planned items, then opens Internal Kanban popup
   * Author: Hassan, Date: 2025-12-13
   * Updated: Added validation to check if kanban exists in currentSkid.planned
   */
  const handleToyotaKanbanScan = (scannedValue: string) => {
    if (!currentSkidId) {
      setError('Please scan a Manifest QR first to open a skid');
      return;
    }

    const parsed = parseToyotaKanban(scannedValue);

    if (!parsed) {
      setError('Invalid Toyota Kanban QR code format');
      return;
    }

    // Verify current skid exists in our state
    const currentSkid = skidGroups.find(g => g.skidId === currentSkidId);
    if (!currentSkid) {
      setError(`Skid ${currentSkidId} not found. Please scan manifest first.`);
      return;
    }

    // CRITICAL VALIDATION: Check if this kanban exists in the planned items for current skid
    const matchingPlanned = currentSkid.planned.find(
      p => p.partNumber === parsed.partNumber ||
           p.partNumber === parsed.partNumberRepeat ||
           parsed.partNumber.includes(p.partNumber) ||
           p.partNumber.includes(parsed.partNumber)
    );

    if (!matchingPlanned) {
      // Kanban NOT found in manifest - REJECT
      setError(`This kanban doesn't belong to this manifest. Part: ${parsed.partNumber}`);
      console.log('Toyota Kanban REJECTED - not in planned items:', parsed.partNumber);
      return;
    }

    // Validate palletization code match (Manifest QR vs Toyota Kanban)
    // Only validate if we have parsed QR data from manifest scan
    if (parsedQRData) {
      const palletizationMatch = validatePalletizationMatch(
        parsedQRData.palletizationCode,
        parsed.palletCode
      );

      if (!palletizationMatch) {
        setError(
          `Palletization code mismatch. Manifest QR: "${parsedQRData.palletizationCode}", Toyota Kanban: "${parsed.palletCode}". Please verify the correct items are being scanned.`
        );
        return;
      }
    }

    // Kanban VALIDATED - Store and open Internal Kanban modal
    setPendingToyotaKanban(parsed);
    setShowInternalKanbanModal(true);
    setInternalKanbanInput(''); // Clear previous input
    setInternalKanbanError(null); // Clear any previous error
    setSuccess(`✓ Validated: ${parsed.partNumber} - ${parsed.description}`);
    console.log('Toyota Kanban validated:', parsed, 'Matched planned:', matchingPlanned);
  };

  /**
   * Handle Internal Kanban scan (completes the scan pair)
   * Adds the Toyota Kanban + Internal Kanban pair to the current skid
   * Updates planned item scannedQty if matched
   * Calls recordSkidBuildScan API to persist to database
   */
  const handleInternalKanbanScan = async (scannedValue: string) => {
    if (!pendingToyotaKanban) {
      setError('Please scan a Toyota Kanban first');
      return;
    }

    if (!currentSkidId) {
      setError('No current skid selected');
      return;
    }

    if (!user?.id) {
      setError('User not authenticated. Please log in again.');
      return;
    }

    const parsed = parseInternalKanban(scannedValue);

    if (!parsed) {
      setError('Invalid Internal Kanban format. Expected: PART/KANBAN/SERIAL');
      return;
    }

    // Check for duplicate internal kanban
    if (scannedInternalKanbans.includes(parsed.internalKanban)) {
      setError(`Internal Kanban ${parsed.internalKanban} has already been scanned`);
      return;
    }

    // Get current skid to find palletizationCode
    const currentSkid = skidGroups.find(g => g.skidId === currentSkidId);
    const palletizationCode = currentSkid?.palletizationCode || pendingToyotaKanban.palletCode || '';

    // Extract skidNumber and skidSide from currentSkidId format "D1-001A"
    // Format: "PalletCode-RawSkidId" where RawSkidId is "001A" (3 digits + side)
    const skidIdParts = currentSkidId.split('-');
    const rawSkidId = skidIdParts.length > 1 ? skidIdParts[1] : currentSkidId;
    const skidNumber = rawSkidId.substring(0, 3); // "001" from "001A"
    const skidSide = rawSkidId.length > 3 ? rawSkidId.substring(3, 4) : 'A'; // "A" from "001A"

    console.log('=== SKID ID PARSING ===');
    console.log('currentSkidId:', currentSkidId);
    console.log('rawSkidId:', rawSkidId);
    console.log('skidNumber:', skidNumber);
    console.log('skidSide:', skidSide);

    // Count boxes already scanned for this palletization + skid to determine box number
    const existingBoxes = currentSkid?.scanned.filter(
      s => s.palletizationCode === palletizationCode && s.skidNumber === skidNumber
    ) || [];
    // Use the parsed box number from Toyota kanban (positions 151-155 of AIAG barcode)
    const boxNumber = parseInt(pendingToyotaKanban.boxNumber) || 1;

    // Find matching planned item to get plannedItemId for API
    // Match by BOTH part number AND pallet code for precision
    const matchedPlannedItem = currentSkid?.planned.find(p => {
      const partMatches =
        p.partNumber === pendingToyotaKanban.partNumber ||
        p.partNumber === pendingToyotaKanban.partNumberRepeat ||
        pendingToyotaKanban.partNumber.includes(p.partNumber) ||
        p.partNumber.includes(pendingToyotaKanban.partNumber);

      const palletMatches = p.palletizationCode === palletizationCode;

      return partMatches && palletMatches;
    });

    // Call API to record the scan
    let scanId = `${currentSkidId}-${Date.now()}`;

    if (sessionId && matchedPlannedItem?.plannedItemId) {
      setLoading(true);
      console.log('=== CALLING RECORD SCAN API ===');
      console.log('sessionId:', sessionId);
      console.log('plannedItemId:', matchedPlannedItem.plannedItemId);
      console.log('skidNumber:', skidNumber);
      console.log('skidSide:', skidSide);
      console.log('rawSkidId:', rawSkidId);
      console.log('boxNumber:', boxNumber);
      console.log('palletizationCode:', palletizationCode);

      try {
        const scanResponse = await recordSkidBuildScan(
          sessionId,
          matchedPlannedItem.plannedItemId,
          skidNumber,                                    // "001" (3 numeric digits)
          skidSide,                                      // "A" or "B"
          rawSkidId,                                     // "001A" (original 4 chars)
          boxNumber,                                     // Box number from count
          pendingToyotaKanban.lineSideAddress || 'N/A', // Line side address from Toyota Kanban
          palletizationCode,                             // For backend validation
          parsed.internalKanban,
          user.id // User ID from authentication context (validated above)
        );

        if (scanResponse.success && scanResponse.data) {
          scanId = scanResponse.data.scanId;
          console.log('Scan recorded to API:', scanId);
        } else {
          console.warn('Failed to record scan to API:', (scanResponse as any).error);
          setError(`Warning: Scan not saved to database: ${(scanResponse as any).error}`);
        }
      } catch (err) {
        console.error('Error calling recordSkidBuildScan:', err);
        setError('Warning: Scan not saved to database');
      } finally {
        setLoading(false);
      }
    } else {
      console.warn('Skipping API call - missing sessionId or plannedItemId');
      if (!sessionId) console.warn('  - No sessionId');
      if (!matchedPlannedItem?.plannedItemId) console.warn('  - No matching planned item with plannedItemId');
    }

    // Create scanned item for UI
    const scannedItem: ScannedItem = {
      id: scanId,
      skidId: currentSkidId,
      toyotaKanban: pendingToyotaKanban.rawValue,
      internalKanban: parsed.internalKanban,
      serialNumber: parsed.serialNumber,
      partNumber: pendingToyotaKanban.partNumber,
      quantity: parseInt(pendingToyotaKanban.quantity) || 0,
      timestamp: new Date().toISOString(),
      description: pendingToyotaKanban.description,
      palletizationCode: palletizationCode,
      skidNumber: skidNumber,
      boxNumber: boxNumber,
      kanbanNumber: pendingToyotaKanban.kanbanNumber || '',
    };

    // Update skid groups - add scanned item to scanned[] array
    // DON'T modify scannedQty - it comes from API only
    setSkidGroups(prev => {
      return prev.map(skid => {
        if (skid.skidId === currentSkidId) {
          return {
            ...skid,
            scanned: [...skid.scanned, scannedItem],
          };
        }
        return skid;
      });
    });

    // Track internal kanban to prevent duplicates
    setScannedInternalKanbans(prev => [...prev, parsed.internalKanban]);

    // Clear pending kanban
    setPendingToyotaKanban(null);
    setSuccess(`✓ Added: ${pendingToyotaKanban.partNumber} - ${pendingToyotaKanban.description} to Skid ${currentSkidId}`);

    console.log('Internal Kanban scanned:', parsed);
    console.log('Scanned item created:', scannedItem);
  };

  /**
   * Toggle skid expansion
   */
  const _toggleSkidExpansion = (skidId: string) => {
    setExpandedSkids(prev => {
      if (prev.includes(skidId)) {
        return prev.filter(id => id !== skidId);
      } else {
        return [...prev, skidId];
      }
    });
  };

  /**
   * Submit all scanned items - calls completeSkidBuildSession API
   */
  const handleSubmit = async () => {
    // Count ALL scanned items: previouslyScanned (from API via scannedQty) + sessionScanned (from scanned[])
    const totalScanned = skidGroups.reduce((sum, skid) => {
      const previouslyScanned = skid.planned.reduce((pSum, p) => pSum + p.scannedQty, 0);
      const sessionScanned = skid.scanned.length;
      return sum + previouslyScanned + sessionScanned;
    }, 0);

    if (totalScanned === 0) {
      setError('No items scanned. Please scan at least one item before submitting.');
      return;
    }

    if (!sessionId) {
      setError('No active session. Please reload and try again.');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      console.log('=== COMPLETING SESSION ===');
      console.log('sessionId:', sessionId);
      console.log('totalScanned:', totalScanned);

      if (!user?.id) {
        setError('User not authenticated. Please log in again.');
        setLoading(false);
        return;
      }

      const completeResponse = await completeSkidBuildSession(
        sessionId,
        user.id
      );

      if (completeResponse.success && completeResponse.data) {
        const confNumber = completeResponse.data.confirmationNumber;
        const toyotaConfirmation = completeResponse.data.toyotaConfirmationNumber;
        const toyotaError = completeResponse.data.toyotaError;

        // Update state
        setConfirmationNumber(confNumber); // Update state for badge
        setToyotaConfirmationNumber(toyotaConfirmation || '');
        setToyotaSubmissionError(toyotaError || '');

        // Display appropriate message based on Toyota API result
        if (toyotaConfirmation) {
          // Success - both skid build saved and Toyota confirmed
          setSuccess(`✅ Skid Build Submitted Successfully!\nToyota Confirmation Number: ${toyotaConfirmation}\nInternal Confirmation: ${confNumber}`);
          setShowSuccessModal(true); // Show success modal
        } else if (toyotaError) {
          // Warning - skid build saved but Toyota submission failed
          setShowWarningModal(true); // Show warning modal instead of inline error
        } else {
          // Fallback - only internal confirmation
          setSuccess(`✓ Skid Build Completed! Confirmation: ${confNumber}`);
        }

        console.log('Session completed:', completeResponse.data);
        if (toyotaConfirmation) console.log('Toyota Confirmation Number:', toyotaConfirmation);
        if (toyotaError) console.warn('Toyota submission error:', toyotaError);

        // Clear session state
        setSessionId(null);
      } else {
        setError(`Failed to complete session: ${(completeResponse as any).error}`);
        console.error('Failed to complete session:', (completeResponse as any).error);
      }
    } catch (err) {
      console.error('Error completing session:', err);
      setError('Failed to complete session. Please try again.');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Reset all data
   */
  const handleReset = () => {
    if (confirm('Reset all data and start over?')) {
      window.location.reload();
    }
  };

  /**
   * Add Exception - Author: Hassan, Date: 2025-12-13
   * Updated: 2025-12-14 - Call backend API to persist exception
   */
  const handleAddException = async () => {
    if (!selectedExceptionType || !exceptionComments.trim()) {
      setError('Please select exception type and add comments');
      return;
    }

    // Validate required state
    if (!sessionId || !_orderId) {
      setError('No active session. Please scan a manifest first.');
      return;
    }

    if (!user?.id) {
      setError('User not authenticated');
      return;
    }

    // Map exception type to code
    const exceptionCodeMap: { [key: string]: string } = {
      'Revised Quantity (Toyota Quantity Reduction)': '10',
      'Modified Quantity per Box': '11',
      'Supplier Revised Shortage (Short Shipment)': '12',
      'Non-Standard Packaging (Expendable)': '20',
      'Others': '26', // F-GAP-001 fix
    };

    const exceptionCode = exceptionCodeMap[selectedExceptionType];
    if (!exceptionCode) {
      setError('Invalid exception type selected');
      return;
    }

    // Extract skid number from current skid ID (e.g., "001A" -> 1)
    const skidNumber = currentSkidId ? parseInt(currentSkidId.substring(0, 3), 10) : undefined;

    setLoading(true);
    setError(null);

    try {
      // Call backend API to add exception
      const response = await addSkidBuildException(
        sessionId,
        _orderId,
        exceptionCode,
        exceptionComments.trim(),
        skidNumber,
        user.id
      );

      if (!response.success || !response.data) {
        setError(response.message || 'Failed to add exception');
        setLoading(false);
        return;
      }

      // Success - add to local state with returned ID
      const newException: Exception = {
        id: response.data.exceptionId,
        type: selectedExceptionType,
        comments: exceptionComments.trim(),
        timestamp: new Date().toISOString(),
      };

      setExceptions(prev => [...prev, newException]);
      setShowExceptionModal(false);
      setSelectedExceptionType('');
      setExceptionComments('');
      setSuccess(`Exception added: ${selectedExceptionType}`);

      console.log('Exception added:', newException);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to add exception');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Remove Exception - Author: Hassan, Date: 2025-12-13
   * Updated: 2025-12-14 - Call backend API to delete exception
   */
  const handleRemoveException = async (index: number) => {
    const exception = exceptions[index];

    // If exception has no ID, just remove from local state (shouldn't happen with new flow)
    if (!exception.id) {
      setExceptions(exceptions.filter((_, i) => i !== index));
      setSuccess('Exception removed');
      return;
    }

    setLoading(true);
    setError(null);

    try {
      // Call backend API to delete exception
      const response = await deleteSkidBuildException(exception.id);

      if (!response.success) {
        setError(response.message || 'Failed to delete exception');
        setLoading(false);
        return;
      }

      // Success - remove from local state
      setExceptions(exceptions.filter((_, i) => i !== index));
      setSuccess('Exception removed');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to delete exception');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Confirm Internal Kanban from popup modal
   * Author: Hassan, Date: 2025-12-13
   * Updated: 2025-12-13 - Show validation errors IN popup instead of closing it
   */
  const handleConfirmInternalKanban = () => {
    if (!internalKanbanInput.trim()) {
      setInternalKanbanError('Please scan an internal kanban');
      return;
    }

    const parsed = parseInternalKanban(internalKanbanInput);
    if (!parsed) {
      setInternalKanbanError('Invalid internal kanban format. Expected: PART/KANBAN/SERIAL');
      setInternalKanbanInput(''); // Clear for retry
      return;
    }

    // Check for duplicate
    if (scannedInternalKanbans.includes(parsed.internalKanban)) {
      setInternalKanbanError(`Internal Kanban ${parsed.internalKanban} has already been scanned`);
      setInternalKanbanInput(''); // Clear for retry
      return;
    }

    // Success - clear error, close popup, process scan
    setInternalKanbanError(null);
    setShowInternalKanbanModal(false);
    handleInternalKanbanScan(internalKanbanInput.trim());
    setInternalKanbanInput('');
  };

  /**
   * Cancel Internal Kanban popup
   * Author: Hassan, Date: 2025-12-13
   * Updated: 2025-12-13 - Clear popup error when canceling
   */
  const handleCancelInternalKanban = () => {
    setShowInternalKanbanModal(false);
    setInternalKanbanInput('');
    setInternalKanbanError(null);
    setPendingToyotaKanban(null);
    setSuccess('Scan cancelled');
  };


  // ===========================
  // RENDER
  // ===========================

  return (
    <div className="fixed inset-0 flex flex-col overflow-auto">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      <div className="min-h-screen pb-20">
        {/* Header */}
        <div className="bg-[#253262] text-white p-4 shadow-md">
          <div className="max-w-7xl mx-auto flex items-center justify-between">
            <div className="flex items-center gap-3">
              <button
                onClick={() => router.push('/home')}
                className="hover:bg-white/10 p-2 rounded transition-colors"
              >
                <i className="fa fa-arrow-left text-xl"></i>
              </button>
              <h1 className="text-xl font-semibold">Skid Build V2 - Multi-Skid Scanner</h1>
            </div>
            <div className="flex items-center gap-2">
              <Badge variant="info" className="text-sm">
                {skidGroups.length} Skids
              </Badge>
            </div>
          </div>
        </div>

        <div className="max-w-7xl mx-auto p-4 space-y-4">
          {/* Alerts */}
          {error && (
            <Alert variant="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          {success && (
            <Alert variant="success" onClose={() => setSuccess(null)}>
              {success}
            </Alert>
          )}

          {/* Toyota Confirmation Number Display - Now shown as modal popup */}
          {/* Toyota Submission Error Display - Now shown as warning modal popup - Author: Hassan, Date: 2025-12-14 */}

          {/* Order Details - Collapsible */}
          {currentOrderData && (
            <Card>
              <CardHeader className="p-0">
                <div
                  className="cursor-pointer hover:bg-gray-50 p-4 rounded-t-lg transition-colors flex items-center justify-between"
                  onClick={() => setOrderDetailsExpanded(!orderDetailsExpanded)}
                >
                  <div className="flex items-center gap-2 flex-wrap">
                    <CardTitle className="text-base font-medium">Order Details</CardTitle>
                    {/* Confirmation Status Badge */}
                    <span className={`text-xs px-2.5 py-1 rounded-full font-medium ${
                      toyotaConfirmationNumber
                        ? 'bg-green-100 text-green-700'
                        : 'bg-pink-100 text-pink-700'
                    }`}>
                      {toyotaConfirmationNumber ? (
                        <>
                          <i className="fa fa-check mr-1"></i>
                          Confirmed
                        </>
                      ) : 'Unconfirmed'}
                    </span>
                    {/* Toyota Confirmation Number Badge */}
                    {toyotaConfirmationNumber && (
                      <span className="text-xs px-2.5 py-1 rounded-full font-medium bg-green-100 text-green-700 font-mono">
                        Conf# {toyotaConfirmationNumber}
                      </span>
                    )}
                    {/* Current Skid Badge - moved from expanded section */}
                    {currentSkidId && (
                      <span className="text-xs px-2.5 py-1 rounded-full font-medium bg-blue-100 text-blue-700">
                        Skid: {currentSkidId}
                      </span>
                    )}
                  </div>
                  <i className={`fa fa-chevron-${orderDetailsExpanded ? 'down' : 'right'} text-gray-400`}></i>
                </div>
              </CardHeader>
              {orderDetailsExpanded && (
                <CardContent className="p-4 border-t">
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-4 text-sm">
                    <div>
                      <span className="text-gray-600">Order:</span>
                      <span className="ml-2 font-medium">{currentOrderData.orderNumber}</span>
                    </div>
                    <div>
                      <span className="text-gray-600">Plant:</span>
                      <span className="ml-2 font-medium">{currentOrderData.plantPrefix}</span>
                    </div>
                    <div>
                      <span className="text-gray-600">Dock:</span>
                      <span className="ml-2 font-medium">{currentOrderData.dockCode}</span>
                    </div>
                    <div>
                      <span className="text-gray-600">Supplier:</span>
                      <span className="ml-2 font-medium">{currentOrderData.supplierCode}</span>
                    </div>
                    <div>
                      <span className="text-gray-600">Load ID:</span>
                      <span className="ml-2 font-medium">{currentOrderData.loadId}</span>
                    </div>
                    {/* Toyota Confirmation Number in expanded details */}
                    {toyotaConfirmationNumber && (
                      <div className="col-span-2 md:col-span-1">
                        <span className="text-gray-600">Toyota Conf#:</span>
                        <span className="ml-2 font-medium font-mono text-green-700">{toyotaConfirmationNumber}</span>
                        <button
                          onClick={(e) => {
                            e.stopPropagation();
                            navigator.clipboard.writeText(toyotaConfirmationNumber);
                            setSuccess('Toyota confirmation number copied!');
                          }}
                          className="ml-2 text-blue-600 hover:text-blue-700"
                          title="Copy confirmation number"
                        >
                          <i className="fa fa-copy text-xs"></i>
                        </button>
                      </div>
                    )}
                  </div>
                </CardContent>
              )}
            </Card>
          )}

          {/* Pending Toyota Kanban - Now shown in modal instead */}
          {pendingToyotaKanban && !showInternalKanbanModal && (
            <Card>
              <CardContent className="p-4 bg-yellow-50">
                <div className="flex items-center gap-2">
                  <i className="fa fa-barcode text-yellow-600 text-xl"></i>
                  <div>
                    <div className="font-medium text-yellow-900">
                      Pending: {pendingToyotaKanban.partNumber} - {pendingToyotaKanban.description}
                    </div>
                    <div className="text-sm text-yellow-700">
                      Use the popup to scan Internal Kanban
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Scanner */}
          <Card>
            <CardContent className="p-4">
              <div className="flex items-start gap-2">
                <div className="flex-1">
                  <Scanner
                    onScan={handleScan}
                    placeholder="Scan Manifest QR, Toyota Kanban"
                    disabled={loading}
                  />
                </div>
                <button
                  onClick={() => setShowScannerInstructions(!showScannerInstructions)}
                  className="flex-shrink-0 w-10 h-10 rounded-full bg-blue-100 text-blue-600 hover:bg-blue-200 transition-colors flex items-center justify-center mt-1"
                  aria-label="Scanner instructions"
                  title="Show scanner instructions"
                >
                  <i className="fa fa-info text-sm"></i>
                </button>
              </div>
              {showScannerInstructions && (
                <div className="mt-3 p-3 bg-blue-50 border border-blue-200 rounded-lg text-xs text-gray-700 space-y-1.5">
                  <div className="flex items-start gap-2">
                    <i className="fa fa-circle text-[6px] text-blue-600 mt-1.5"></i>
                    <span><strong>Manifest QR</strong> (44 chars) → Sets current skid</span>
                  </div>
                  <div className="flex items-start gap-2">
                    <i className="fa fa-circle text-[6px] text-blue-600 mt-1.5"></i>
                    <span><strong>Toyota Kanban</strong> (200+ chars) → Validates & opens popup for internal kanban</span>
                  </div>
                  <div className="flex items-start gap-2">
                    <i className="fa fa-circle text-[6px] text-blue-600 mt-1.5"></i>
                    <span><strong>Internal Kanban</strong> → Scanned in popup window (PART/KANBAN/SERIAL)</span>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>

          {/* Empty State */}
          {skidGroups.length === 0 && !loading && (
            <Card>
              <CardContent className="p-8 text-center">
                <i className="fa fa-box-open text-4xl text-gray-300 mb-4"></i>
                <div className="text-gray-500 font-medium">No order loaded</div>
                <div className="text-sm text-gray-400 mt-1">
                  Scan a Manifest QR code to load the order and all planned skids
                </div>
              </CardContent>
            </Card>
          )}

          {/* Loading State */}
          {loading && (
            <Card>
              <CardContent className="p-8 text-center">
                <i className="fa fa-spinner fa-spin text-4xl text-blue-500 mb-4"></i>
                <div className="text-gray-500 font-medium">Loading order...</div>
              </CardContent>
            </Card>
          )}

          {/* TAB-BASED INTERFACE - PLANNED / SCANNED - Collapsible */}
          {orderLoaded && currentSkidId && (
            <Card>
              {/* Collapsible Header */}
              <CardHeader className="p-0">
                <div
                  className="cursor-pointer hover:bg-gray-50 p-4 transition-colors flex items-center justify-between"
                  onClick={() => setTabsSectionExpanded(!tabsSectionExpanded)}
                >
                  <div className="flex items-center gap-2">
                    <CardTitle className="text-sm">Skid Items</CardTitle>
                    <Badge variant="info" className="text-xs">
                      {(() => {
                        const currentSkid = skidGroups.find(s => s.skidId === currentSkidId);
                        if (!currentSkid) return '0/0';
                        const totalScanned = currentSkid.planned.reduce((sum, p) => sum + p.scannedQty, 0) + currentSkid.scanned.length;
                        const totalPlanned = currentSkid.planned.reduce((sum, p) => sum + p.plannedQty, 0);
                        return `${totalScanned}/${totalPlanned}`;
                      })()}
                    </Badge>
                  </div>
                  <i className={`fa fa-chevron-${tabsSectionExpanded ? 'down' : 'right'} text-gray-400`}></i>
                </div>
              </CardHeader>

              {tabsSectionExpanded && (
                <>
                  {/* TAB NAVIGATION */}
                  <div className="flex border-t border-b">
                    <button
                      onClick={() => setActiveTab('planned')}
                      className={`flex-1 py-3 px-4 font-medium text-sm transition-all ${
                        activeTab === 'planned'
                          ? 'bg-[#253262] text-white border-b-2 border-[#D2312E]'
                          : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                      }`}
                    >
                      PLANNED ({(() => {
                        const currentSkid = skidGroups.find(s => s.skidId === currentSkidId);
                        if (!currentSkid) return 0;
                        // Count session scans per part+pallet
                        const sessionScans = new Map<string, number>();
                        currentSkid.scanned.forEach(scan => {
                          const key = `${scan.partNumber}|${scan.palletizationCode}`;
                          sessionScans.set(key, (sessionScans.get(key) || 0) + 1);
                        });
                        // Show REMAINING items to scan (plannedQty - scannedQty - sessionScans)
                        return currentSkid.planned.reduce((sum, p) => {
                          const key = `${p.partNumber}|${p.palletizationCode}`;
                          const sessScans = sessionScans.get(key) || 0;
                          return sum + Math.max(0, p.plannedQty - p.scannedQty - sessScans);
                        }, 0);
                      })()})
                    </button>
                    <button
                      onClick={() => setActiveTab('scanned')}
                      className={`flex-1 py-3 px-4 font-medium text-sm transition-all ${
                        activeTab === 'scanned'
                          ? 'bg-[#253262] text-white border-b-2 border-[#D2312E]'
                          : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                      }`}
                    >
                      SCANNED ({(() => {
                        const currentSkid = skidGroups.find(s => s.skidId === currentSkidId);
                        if (!currentSkid) return 0;
                        // Show TOTAL scanned (previous from API + this session)
                        const previouslyScanned = currentSkid.planned.reduce((sum, p) => sum + p.scannedQty, 0);
                        const sessionScanned = currentSkid.scanned.length;
                        return previouslyScanned + sessionScanned;
                      })()})
                    </button>
                  </div>

                  {/* TAB CONTENT */}
                  <CardContent className="p-4">
                {/* PLANNED TAB */}
                {activeTab === 'planned' && (
                  <div className="space-y-3">
                    {(() => {
                      const currentSkid = skidGroups.find(s => s.skidId === currentSkidId);

                      console.log('=== PLANNED TAB DEBUG ===');
                      console.log('currentSkidId:', currentSkidId);
                      console.log('currentSkid found:', !!currentSkid);
                      console.log('planned items count:', currentSkid?.planned?.length || 0);
                      if (currentSkid?.planned) {
                        currentSkid.planned.forEach(p => console.log('  -', p.partNumber, p.palletizationCode, `planned:${p.plannedQty}`, `scanned:${p.scannedQty}`, `remaining:${p.plannedQty - p.scannedQty}`));
                      }

                      if (!currentSkid || currentSkid.planned.length === 0) {
                        return (
                          <div className="p-8 text-center text-gray-400">
                            <i className="fa fa-clipboard-list text-4xl mb-2"></i>
                            <div>No planned items</div>
                          </div>
                        );
                      }

                      // Count session scans per part+pallet combo
                      const sessionScansMap = new Map<string, number>();
                      currentSkid.scanned.forEach(scan => {
                        const key = `${scan.partNumber}|${scan.palletizationCode}`;
                        sessionScansMap.set(key, (sessionScansMap.get(key) || 0) + 1);
                      });

                      // Filter to only items that still need scanning (remaining > 0)
                      // Must account for BOTH API scannedQty AND session scans
                      const itemsNeedingScanning = currentSkid.planned.filter(item => {
                        const key = `${item.partNumber}|${item.palletizationCode}`;
                        const sessionScans = sessionScansMap.get(key) || 0;
                        const totalScanned = item.scannedQty + sessionScans;
                        return (item.plannedQty - totalScanned) > 0;
                      });

                      // Also count fully scanned items (API + session)
                      const fullyScannedCount = currentSkid.planned.filter(item => {
                        const key = `${item.partNumber}|${item.palletizationCode}`;
                        const sessionScans = sessionScansMap.get(key) || 0;
                        return (item.scannedQty + sessionScans) >= item.plannedQty;
                      }).length;

                      if (itemsNeedingScanning.length === 0) {
                        // Calculate total BOXES scanned (sum of scannedQty across all items)
                        const totalBoxesScanned = currentSkid.planned.reduce((sum, p) => sum + p.scannedQty, 0);
                        return (
                          <div className="p-8 text-center">
                            <i className="fa fa-check-circle text-4xl mb-2 text-green-500"></i>
                            <div className="text-green-600 font-medium">All items scanned!</div>
                            <div className="text-sm text-gray-500 mt-1">
                              {totalBoxesScanned} box{totalBoxesScanned !== 1 ? 'es' : ''} scanned ({fullyScannedCount} item{fullyScannedCount !== 1 ? 's' : ''} completed)
                            </div>
                          </div>
                        );
                      }

                      // Group by PalletizationCode - only items needing scanning
                      const grouped = groupPlannedByPalletization(itemsNeedingScanning);

                      return (
                        <>
                          {fullyScannedCount > 0 && (
                            <div className="text-sm text-green-600 bg-green-50 p-2 rounded mb-2">
                              <i className="fa fa-check mr-1"></i>
                              {(() => {
                                // Calculate total BOXES scanned (sum of scannedQty across all items)
                                const totalBoxesScanned = currentSkid.planned.reduce((sum, p) => sum + p.scannedQty, 0);
                                return `${totalBoxesScanned} box${totalBoxesScanned !== 1 ? 'es' : ''} scanned (${fullyScannedCount} item${fullyScannedCount !== 1 ? 's' : ''} completed)`;
                              })()}
                            </div>
                          )}
                          {Array.from(grouped.entries()).map(([palletCode, items]) => {
                            // Calculate REMAINING boxes to scan (including session scans)
                            const remainingBoxes = items.reduce((sum, item) => {
                              const key = `${item.partNumber}|${item.palletizationCode}`;
                              const sessionScans = sessionScansMap.get(key) || 0;
                              return sum + Math.max(0, item.plannedQty - item.scannedQty - sessionScans);
                            }, 0);
                            const totalPlanned = items.reduce((sum, item) => sum + item.plannedQty, 0);
                            const totalScanned = items.reduce((sum, item) => {
                              const key = `${item.partNumber}|${item.palletizationCode}`;
                              const sessionScans = sessionScansMap.get(key) || 0;
                              return sum + item.scannedQty + sessionScans;
                            }, 0);
                            const kanbanNumber = items[0]?.description || 'N/A';

                            return (
                              <div key={palletCode} className="border border-gray-200 rounded-lg p-4 bg-white">
                                <div className="flex items-center justify-between mb-2">
                                  <div className="flex items-center gap-2">
                                    <span className="text-lg font-bold text-gray-800">{palletCode}</span>
                                    <Badge variant="warning" className="text-xs">
                                      {remainingBoxes} remaining
                                    </Badge>
                                    {totalScanned > 0 && (
                                      <span className="text-xs text-gray-400">
                                        ({totalScanned}/{totalPlanned} done)
                                      </span>
                                    )}
                                  </div>
                                </div>
                                <div className="text-sm text-gray-600 pl-2 border-l-2 border-gray-300">
                                  Kanban: {kanbanNumber}
                                </div>
                                {/* Show individual items */}
                                <div className="mt-2 space-y-1">
                                  {items.map((item, idx) => {
                                    const key = `${item.partNumber}|${item.palletizationCode}`;
                                    const sessionScans = sessionScansMap.get(key) || 0;
                                    const totalItemScanned = item.scannedQty + sessionScans;
                                    return (
                                      <div key={idx} className="text-xs text-gray-500 flex justify-between">
                                        <span>{item.partNumber}</span>
                                        <span>{totalItemScanned}/{item.plannedQty} scanned</span>
                                      </div>
                                    );
                                  })}
                                </div>
                              </div>
                            );
                          })}
                        </>
                      );
                    })()}
                  </div>
                )}

                {/* SCANNED TAB */}
                {activeTab === 'scanned' && (
                  <div className="space-y-3">
                    {(() => {
                      const currentSkid = skidGroups.find(s => s.skidId === currentSkidId);

                      if (!currentSkid) {
                        return (
                          <div className="p-8 text-center text-gray-400">
                            <i className="fa fa-check text-4xl mb-2"></i>
                            <div>No skid selected</div>
                          </div>
                        );
                      }

                      // Generate ALL scanned items (previous + session combined)
                      const allScannedItems: ScannedItem[] = [];

                      // 1. Generate items from API (using scanDetails which includes actual SkidNumber)
                      currentSkid.planned.forEach(plannedItem => {
                        // Use scanDetails from API - each scan has its own SkidNumber, BoxNumber, etc.
                        const scanDetails = plannedItem.scanDetails || [];

                        scanDetails.forEach((scan) => {
                          allScannedItems.push({
                            id: `previous-${plannedItem.partNumber}-${scan.skidNumber}-${scan.boxNumber}`,
                            skidId: `${scan.palletizationCode || plannedItem.palletizationCode}-${scan.skidNumber}`,
                            toyotaKanban: '',
                            internalKanban: scan.internalKanban || 'N/A',
                            serialNumber: '',
                            partNumber: plannedItem.partNumber,
                            quantity: 0,
                            timestamp: '',
                            description: plannedItem.description || 'N/A',
                            palletizationCode: scan.palletizationCode || plannedItem.palletizationCode,
                            skidNumber: scan.skidNumber,
                            boxNumber: scan.boxNumber,
                            kanbanNumber: plannedItem.description || 'N/A',
                          });
                        });
                      });

                      // 2. Add items from current session (scanned[] array)
                      allScannedItems.push(...currentSkid.scanned);

                      // Total count
                      const totalScanned = allScannedItems.length;

                      if (totalScanned === 0) {
                        return (
                          <div className="p-8 text-center text-gray-400">
                            <i className="fa fa-check text-4xl mb-2"></i>
                            <div>No items scanned yet</div>
                          </div>
                        );
                      }

                      // Group ALL items by PalletizationCode + SkidNumber
                      const grouped = groupScannedByPalletAndSkid(allScannedItems);

                      return (
                        <>
                          {/* Show total count */}
                          <div className="text-sm text-gray-700 bg-gray-50 p-2 rounded mb-3">
                            <i className="fa fa-check-circle mr-2 text-green-600"></i>
                            <strong>{totalScanned}</strong> total items scanned
                          </div>

                          {/* ALL Scanned Items - Grouped */}
                          {Array.from(grouped.entries()).map(([groupKey, items]) => {
                            const isExpanded = expandedGroups.includes(groupKey);
                            const [palletCode, skidNum] = groupKey.split('-');
                            // Get full skidId from first item (includes side like "001A")
                            const fullSkidId = items[0]?.skidId?.split('-')[1] || skidNum;
                            const displayHeader = `${palletCode} | ${fullSkidId}`;
                            // Get kanban and part number from first item
                            const kanbanNumber = items[0]?.kanbanNumber || 'N/A';
                            const partNumber = items[0]?.partNumber || 'N/A';

                            return (
                              <div key={groupKey} className="border border-green-200 rounded-lg bg-green-50">
                                {/* Group Header - Clickable */}
                                <div
                                  className="p-4 cursor-pointer hover:bg-green-100 transition-colors"
                                  onClick={() => toggleGroupExpansion(groupKey)}
                                >
                                  <div className="flex items-center justify-between">
                                    <div className="flex items-center gap-2">
                                      <i className={`fa fa-chevron-${isExpanded ? 'down' : 'right'} text-gray-600`}></i>
                                      <span className="text-base font-bold text-gray-800">{displayHeader}</span>
                                      <Badge variant="success" className="text-xs">
                                        {items.length} scanned
                                      </Badge>
                                    </div>
                                    <i className="fa fa-circle-check text-green-600 text-lg"></i>
                                  </div>
                                </div>

                                {/* Expandable Box List */}
                                {isExpanded && (
                                  <div className="border-t border-green-200 bg-white">
                                    {items.map((item) => (
                                      <div
                                        key={item.id}
                                        className="px-4 py-2 border-b border-gray-100 last:border-b-0 hover:bg-gray-50"
                                      >
                                        <div className="flex items-start justify-between">
                                          <div className="flex-1">
                                            <div className="text-sm font-medium text-gray-800">
                                              Kanban: {item.kanbanNumber || 'N/A'} &nbsp; Box# {item.boxNumber}
                                            </div>
                                            <div className="text-xs text-gray-500 mt-1">
                                              Internal Kanban: {item.internalKanban || 'N/A'}
                                            </div>
                                            <div className="text-xs text-gray-500 mt-1">
                                              Part# {item.partNumber || 'N/A'}
                                            </div>
                                          </div>
                                        </div>
                                      </div>
                                    ))}
                                  </div>
                                )}
                              </div>
                            );
                          })}

                          {/* Total Count at bottom */}
                          <div className="pt-2 border-t border-gray-200">
                            <div className="text-sm font-medium text-gray-700 text-center">
                              Total: {totalScanned} items scanned
                            </div>
                          </div>
                        </>
                      );
                    })()}
                  </div>
                )}
                  </CardContent>
                </>
              )}
            </Card>
          )}

          {/* Exceptions List - Collapsible - ALWAYS VISIBLE */}
          {orderLoaded && (
            <Card>
              <CardHeader className="p-0">
                <div
                  className="flex items-center justify-between cursor-pointer hover:bg-gray-50 p-4 rounded-t-lg transition-colors"
                  onClick={() => setExceptionsExpanded(!exceptionsExpanded)}
                >
                  <CardTitle className="text-sm">
                    Exceptions ({exceptions.length})
                  </CardTitle>
                  <i className={`fa fa-chevron-${exceptionsExpanded ? 'down' : 'right'} text-gray-400`}></i>
                </div>
              </CardHeader>
              {exceptionsExpanded && (
                <CardContent className="p-4 space-y-2">
                  {exceptions.length === 0 ? (
                    <p className="text-sm text-gray-500 text-center py-4">
                      No exceptions added
                    </p>
                  ) : (
                    exceptions.map((exception, idx) => (
                      <div
                        key={idx}
                        className="p-3 bg-yellow-50 border border-yellow-200 rounded-lg"
                      >
                        <div className="flex items-start justify-between gap-2">
                          <div className="flex-1">
                            <div className="flex items-center gap-2 mb-1">
                              <Badge variant="warning">{exception.type}</Badge>
                              <span className="text-xs text-gray-500">
                                {new Date(exception.timestamp).toLocaleString()}
                              </span>
                            </div>
                            <p className="text-sm text-gray-700">{exception.comments}</p>
                          </div>
                          <button
                            onClick={() => handleRemoveException(idx)}
                            className="text-red-600 hover:text-red-700 p-1"
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
          )}

          {/* Action Buttons - 4 buttons matching skid-build screen 2 */}
          {orderLoaded && (
            <div className="grid grid-cols-2 gap-2">
              <Button
                onClick={() => setShowExceptionModal(true)}
                variant="warning"
                fullWidth
                disabled={!!toyotaConfirmationNumber}
                className={!!toyotaConfirmationNumber ? 'opacity-50 cursor-not-allowed' : ''}
              >
                <i className="fa fa-exclamation-triangle mr-2"></i>
                Add Exception
              </Button>
              <Button
                onClick={handleSubmit}
                variant="success"
                fullWidth
                disabled={!!toyotaConfirmationNumber || (() => {
                  const currentSkid = skidGroups.find(s => s.skidId === currentSkidId);
                  if (!currentSkid) return true;
                  const totalPlanned = currentSkid.planned.reduce((sum, p) => sum + p.plannedQty, 0);
                  const totalScanned = currentSkid.planned.reduce((sum, p) => sum + p.scannedQty, 0) + currentSkid.scanned.length;
                  const allScanned = totalScanned >= totalPlanned;
                  const hasException = exceptions.length > 0;
                  return !(allScanned || hasException);
                })()}
                className={!!toyotaConfirmationNumber ? 'opacity-50 cursor-not-allowed' : ''}
              >
                <i className="fa fa-check-circle mr-2"></i>
                Submit
              </Button>
              <Button
                onClick={handleReset}
                variant="primary"
                fullWidth
              >
                <i className="fa fa-rotate-right mr-2"></i>
                Reset
              </Button>
              <Button
                onClick={() => router.push('/')}
                variant="error"
                fullWidth
              >
                <i className="fa fa-xmark mr-2"></i>
                Cancel
              </Button>
            </div>
          )}

        </div>
      </div>

      {/* Internal Kanban Modal - Author: Hassan, Date: 2025-12-13 */}
      {showInternalKanbanModal && pendingToyotaKanban && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          onClick={handleCancelInternalKanban}
        >
          <div
            className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Modal Header */}
            <div className="flex items-center justify-between p-4 border-b border-gray-200">
              <h3 className="text-lg font-bold text-[#253262]">
                Scan Internal Kanban
              </h3>
              <button
                onClick={handleCancelInternalKanban}
                className="text-gray-400 hover:text-gray-600 transition-colors"
                aria-label="Close modal"
              >
                <i className="fa fa-xmark text-2xl"></i>
              </button>
            </div>

            {/* Modal Content */}
            <div className="p-6 space-y-4">
              {/* Toyota Kanban Info */}
              <div className="p-3 bg-blue-50 border border-blue-200 rounded-lg">
                <div className="text-sm font-medium text-blue-900 mb-1">
                  Toyota Kanban Scanned:
                </div>
                <div className="text-xs text-blue-700 space-y-1">
                  <div><strong>Part:</strong> {pendingToyotaKanban.partNumber}</div>
                  <div><strong>Description:</strong> {pendingToyotaKanban.description}</div>
                  <div><strong>Quantity:</strong> {pendingToyotaKanban.quantity}</div>
                  <div><strong>Kanban:</strong> {pendingToyotaKanban.kanbanNumber}</div>
                </div>
              </div>

              {/* Error Display */}
              {internalKanbanError && (
                <div className="p-3 bg-red-100 border border-red-300 rounded-lg text-red-700 text-sm">
                  <i className="fa fa-exclamation-circle mr-2"></i>
                  {internalKanbanError}
                </div>
              )}

              {/* Internal Kanban Input */}
              <div>
                <label className="block text-sm font-medium mb-2 text-[#253262]">
                  Internal Kanban Barcode *
                </label>
                <input
                  type="text"
                  value={internalKanbanInput}
                  onChange={(e) => {
                    setInternalKanbanInput(e.target.value);
                    setInternalKanbanError(null); // Clear error on new input
                  }}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter' && internalKanbanInput.trim()) {
                      handleConfirmInternalKanban();
                    }
                  }}
                  placeholder="Format: PART/KANBAN/SERIAL"
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#253262] focus:border-transparent font-mono"
                  autoFocus
                />
                <p className="text-xs text-gray-500 mt-1">
                  Example: 681010E250/FCJR/001
                </p>
              </div>
            </div>

            {/* Modal Footer */}
            <div className="flex gap-2 p-4 border-t border-gray-200">
              <Button
                onClick={handleCancelInternalKanban}
                variant="error"
                fullWidth
              >
                <i className="fa fa-xmark mr-2"></i>
                Cancel
              </Button>
              <Button
                onClick={handleConfirmInternalKanban}
                variant="success"
                fullWidth
                disabled={!internalKanbanInput.trim()}
              >
                <i className="fa fa-check mr-2"></i>
                Confirm
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Exception Modal - Popup Dialog */}
      {showExceptionModal && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          onClick={() => {
            setShowExceptionModal(false);
            setSelectedExceptionType('');
            setExceptionComments('');
          }}
        >
          <div
            className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Modal Header */}
            <div className="flex items-center justify-between p-4 border-b border-gray-200">
              <h3 className="text-lg font-bold text-[#253262]">
                Add Exception
              </h3>
              <button
                onClick={() => {
                  setShowExceptionModal(false);
                  setSelectedExceptionType('');
                  setExceptionComments('');
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
                <label className="block text-sm font-medium mb-2 text-[#253262]">
                  Exception Type *
                </label>
                <select
                  value={selectedExceptionType}
                  onChange={(e) => setSelectedExceptionType(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#253262] focus:border-transparent"
                >
                  <option value="">Select exception type...</option>
                  {EXCEPTION_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </div>

              {/* Comments Textarea */}
              <div>
                <label className="block text-sm font-medium mb-2 text-[#253262]">
                  Comments *
                </label>
                <textarea
                  value={exceptionComments}
                  onChange={(e) => setExceptionComments(e.target.value)}
                  rows={4}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#253262] focus:border-transparent resize-none"
                  placeholder="Enter exception details..."
                />
              </div>
            </div>

            {/* Modal Footer */}
            <div className="flex gap-2 p-4 border-t border-gray-200">
              <Button
                onClick={() => {
                  setShowExceptionModal(false);
                  setSelectedExceptionType('');
                  setExceptionComments('');
                }}
                variant="error"
                fullWidth
              >
                <i className="fa fa-xmark mr-2"></i>
                Cancel
              </Button>
              <Button
                onClick={handleAddException}
                variant="warning"
                fullWidth
                disabled={!selectedExceptionType || !exceptionComments.trim()}
              >
                <i className="fa fa-plus mr-2"></i>
                Add Exception
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Success Modal - Toyota Confirmation Number - Author: Hassan, Date: 2025-12-14 */}
      {showSuccessModal && toyotaConfirmationNumber && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          onClick={() => setShowSuccessModal(false)}
        >
          <div
            className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Modal Header */}
            <div className="flex items-center justify-between p-4 border-b border-green-200 bg-green-50">
              <h3 className="text-lg font-bold text-green-900 flex items-center gap-2">
                <i className="fa fa-circle-check text-green-600"></i>
                Submission Successful
              </h3>
              <button
                onClick={() => setShowSuccessModal(false)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
                aria-label="Close modal"
              >
                <i className="fa fa-xmark text-2xl"></i>
              </button>
            </div>

            {/* Modal Content */}
            <div className="p-6 space-y-4">
              <div className="text-center space-y-3">
                <div className="flex items-center justify-center gap-2">
                  <i className="fa fa-circle-check text-5xl text-green-600"></i>
                </div>
                <div className="text-xl font-bold text-green-900">
                  Skid Build Submitted Successfully!
                </div>
                <div className="bg-green-50 rounded-lg p-4 border-2 border-green-200">
                  <div className="text-sm text-gray-600 mb-2">Toyota Confirmation Number</div>
                  <div className="text-2xl font-mono font-bold text-green-700 tracking-wider">
                    {toyotaConfirmationNumber}
                  </div>
                  <button
                    onClick={() => {
                      navigator.clipboard.writeText(toyotaConfirmationNumber);
                      setSuccess('Toyota confirmation number copied to clipboard!');
                    }}
                    className="mt-3 text-sm text-blue-600 hover:text-blue-700 flex items-center gap-1 mx-auto"
                  >
                    <i className="fa fa-copy"></i>
                    <span>Copy to clipboard</span>
                  </button>
                </div>
                {confirmationNumber && (
                  <div className="text-sm text-gray-600 pt-2 border-t border-gray-200">
                    Internal Confirmation: <span className="font-medium">{confirmationNumber}</span>
                  </div>
                )}
              </div>
            </div>

            {/* Modal Footer */}
            <div className="p-4 border-t border-gray-200">
              <Button
                onClick={() => setShowSuccessModal(false)}
                variant="primary"
                fullWidth
              >
                <i className="fa fa-check mr-2"></i>
                Close
              </Button>
            </div>
          </div>
        </div>
      )}

      {/* Warning Modal - Toyota Submission Error - Author: Hassan, Date: 2025-12-14 */}
      {showWarningModal && toyotaSubmissionError && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4"
          onClick={() => setShowWarningModal(false)}
        >
          <div
            className="bg-white rounded-lg shadow-xl max-w-md w-full mx-4"
            onClick={(e) => e.stopPropagation()}
          >
            {/* Modal Header */}
            <div className="flex items-center justify-between p-4 border-b border-yellow-200 bg-yellow-50">
              <h3 className="text-lg font-bold text-yellow-900 flex items-center gap-2">
                <i className="fa fa-exclamation-triangle text-yellow-600"></i>
                Toyota Submission Warning
              </h3>
              <button
                onClick={() => setShowWarningModal(false)}
                className="text-gray-400 hover:text-gray-600 transition-colors"
                aria-label="Close modal"
              >
                <i className="fa fa-xmark text-2xl"></i>
              </button>
            </div>

            {/* Modal Content */}
            <div className="p-6 space-y-4">
              <div className="text-center space-y-3">
                <div className="flex items-center justify-center gap-2">
                  <i className="fa fa-exclamation-triangle text-5xl text-yellow-600"></i>
                </div>
                <div className="text-xl font-bold text-yellow-900">
                  Skid Build Saved, but Toyota submission failed
                </div>
                <div className="bg-yellow-50 rounded-lg p-4 border-2 border-yellow-200">
                  <div className="text-sm text-gray-600 mb-2">Error Details</div>
                  <div className="text-base text-red-700 font-medium">
                    {toyotaSubmissionError}
                  </div>
                </div>
                {confirmationNumber && (
                  <div className="bg-green-50 rounded-lg p-4 border-2 border-green-200">
                    <div className="text-sm text-gray-600 mb-2">Internal Confirmation Number</div>
                    <div className="text-xl font-mono font-bold text-green-700 tracking-wider">
                      {confirmationNumber}
                    </div>
                    <div className="mt-2 text-xs text-gray-500">
                      Your skid build was saved successfully
                    </div>
                  </div>
                )}
              </div>
            </div>

            {/* Modal Footer */}
            <div className="p-4 border-t border-gray-200">
              <Button
                onClick={() => setShowWarningModal(false)}
                variant="primary"
                fullWidth
              >
                <i className="fa fa-check mr-2"></i>
                Close
              </Button>
            </div>
          </div>
        </div>
      )}

    </div>
  );
}
