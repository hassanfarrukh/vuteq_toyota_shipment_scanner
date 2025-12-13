/**
 * Skid Build V2 Page - 5-Screen Workflow with FAB Menu
 * Author: Hassan
 * Date: 2025-11-03
 * Updated: 2025-12-12 - Fixed TypeScript errors (error→errors, skidId→rawSkidId, onClick wrapper, Card style)
 * Updated: 2025-11-04 - MAJOR RESTRUCTURE: Removed separate exceptions screen, moved exceptions inline to Screen 2
 * Updated: 2025-11-04 - Screen numbering: Screen 4 is now Review/Submit, Screen 5 is Confirmation Number Display
 * Updated: 2025-11-04 - Exceptions now handled via MODAL POPUP on Screen 2 (not collapsible section)
 * Updated: 2025-11-04 - New Screen 5: Shows confirmation number AFTER submission with Done button
 * Updated: 2025-11-04 - Removed copy to clipboard button from Screen 5 (confirmation number display only)
 * Updated: 2025-11-04 - Exceptions collapsible section ALWAYS VISIBLE on Screen 2 (shows "Exceptions (0)" when empty)
 * Updated: 2025-11-04 - CRITICAL FIX: Toyota Kanban regex patterns (Description, Dock, Supplier Name, Load ID, Plant)
 * Updated: 2025-11-04 - CRITICAL: Implemented FIXED-POSITION PARSING for both Manifest Scan and Toyota Kanban
 *                        - Manifest Scan: Uses exact substring positions (NO REGEX)
 *                        - Toyota Kanban: Uses substring + minimal indexOf for field extraction
 * Updated: 2025-11-05 - CRITICAL: Complete rewrite of parseToyotaKanban with EXACT FIXED-POSITION parsing
 *                        - QR string parsing (216 chars actual) with precise substring positions
 *                        - All fields extracted using 0-indexed substring (NO REGEX at all)
 *                        - Plant Code (159-164) extracts "02TMI" as ONE field (5 chars)
 *                        - Added: planUnloadDate, shipDate, shipTime, kanbanNumber, deliveryOrder, boxNumber, etc.
 *                        - Console logging for debugging field extraction
 *                        - Fixed: Changed length validation from 212 to 200 minimum (actual QR is 216 chars)
 * Updated: 2025-11-05 - CRITICAL: Fixed Part Description parsing to SKIP position 0 (the "C")
 *                        - Part Description now uses substring(1, 12) instead of substring(0, 12)
 *                        - Result: "TEM RH WEST" instead of "CTEM RH WEST"
 * Updated: 2025-11-05 - MAJOR: Screen 3 comprehensive Toyota Kanban display
 *                        - Added ALL 26 parsed fields to ParsedToyotaKanban interface
 *                        - Created comprehensive collapsible display in Screen 3
 *                        - Shows all fields numbered 1-26 in organized 2-column grid
 *                        - Collapsible section (hidden by default) with scrollable content
 *                        - Clean card layout with border-boxed fields for better readability
 * Updated: 2025-11-05 - CRITICAL FIX: Corrected substring positions for Container Type, Pallet Code, Control Field, Status, Zone/Area
 *                        - Container Type: 193-195 (was 192-194) - Now shows "55" correctly
 *                        - Pallet Code: 195-197 (was 194-196) - Now shows "LB" correctly
 *                        - Control Field: 197-202 (was 196-201) - Now shows "XAXXX" correctly
 *                        - Status: 207-208 (was 206-207) - Now shows "0" correctly
 *                        - Zone/Area: 209-211 (was 207-212) - Now shows "05" correctly
 * Updated: 2025-11-05 - DISPLAY CLEANUP: Removed duplicate fields from Screen 3 Toyota Kanban display
 *                        - Removed "Part Number (Repeated)" field - duplicate of Part Number
 *                        - Removed "Load ID #2" field - duplicate of Load ID #1
 *                        - Changed "Load ID #1" label to "Load/Transport ID" for clarity
 *                        - Renumbered all fields sequentially 1-24 (was 1-26)
 *                        - Display now shows only UNIQUE information
 * Updated: 2025-11-05 - DATE/TIME FORMATTING: Added helper functions for better readability
 *                        - formatDate(): Converts YYYYMMDD to YYYY/MM/DD (e.g., 20230802 → 2023/08/02)
 *                        - formatTime(): Converts HHMM to HH:MM (e.g., 1321 → 13:21)
 *                        - Applied to Plan Unload Date (field 11), Ship Date (field 12), Ship Time (field 13)
 *                        - Raw values preserved in state, formatting only applied to display
 *                        - Edge case handling: Returns original value if empty or wrong length
 * Updated: 2025-11-05 - TOYOTA KANBAN DISPLAY STYLING: Restyled to match Screen 1 Order Details
 *                        - Removed field numbering (1., 2., 3., etc.)
 *                        - Removed individual field borders and colored backgrounds
 *                        - Simple text layout: gray labels, medium weight values
 *                        - Matches Screen 1 order data display pattern exactly
 *                        - All 24 Toyota Kanban fields displayed in clean, consistent format
 * Updated: 2025-11-05 - TOYOTA KANBAN INLINE DISPLAY: Changed to inline "Label: Value" format
 *                        - All 24 fields now display as: <Label>: <Value> on same line
 *                        - Arranged in 2-column responsive grid (grid-cols-1 md:grid-cols-2)
 *                        - Label format: gray text (text-gray-600) with colon and space
 *                        - Value format: bold text (font-medium) immediately after label
 *                        - Author: Hassan, Date: 2025-11-05
 * Updated: 2025-11-05 - DUPLICATE INTERNAL KANBAN PREVENTION: Prevents reusing internal kanbans
 *                        - Tracks all scanned internal kanban numbers in state
 *                        - Validates against scanned list in Screen 3 before parsing
 *                        - Shows error message if duplicate detected
 *                        - Clears list when starting new order or resetting
 *                        - Example: 56089-08E90-00/MPE/001 can only be scanned once per order
 *                        - Author: Hassan, Date: 2025-11-05
 * Updated: 2025-11-05 - CRITICAL: SCREEN 1 MANIFEST-ONLY VALIDATION
 *                        - Screen 1 now ONLY accepts Toyota Manifest QR codes (44 chars)
 *                        - Rejects Toyota Kanban QR codes (200+ chars) with clear error message
 *                        - Length validation: Reject if scanned value > 100 characters
 *                        - Error message: "Please scan the Toyota Manifest QR code, not the Toyota Kanban label"
 *                        - Added instructional text in UI: "Do not scan Toyota Kanban labels here"
 *                        - Toyota Kanban scanning only allowed in Screen 2
 *                        - Author: Hassan, Date: 2025-11-05
 *
 * SCREEN FLOW:
 * 1. Order Search (Manual entry OR scan MANIFEST QR ONLY - auto-parse and manual continue)
 *    - CRITICAL: Only Toyota Manifest QR codes allowed (44 chars)
 *    - Toyota Kanban QR codes (200+ chars) are REJECTED with error message
 * 2. Main Skid Build (Collapsible sections: Order Details, Planned Items, Scanned Items + FAB menu + Exception Modal)
 *    - Toyota Kanban scanning happens HERE
 * 3. Internal Kanban (auto-parse PART/KANBAN/SERIAL, show fields, continue - no list) - Collapsible Toyota Kanban details
 * 4. Review/Submit (Previously Screen 5 - Review and submit the build)
 * 5. Confirmation Number Display (NEW - Shows after successful submission with Done button)
 *
 * FEATURES:
 * - FAB Menu (bottom left) on Screens 1-4: All 3 options visible on all screens (Unpick All/Rack Exception disabled when not on Screen 2)
 * - QR Code Parser: Extracts all fields (Plant, Supplier, Dock, Order, Load ID, Palletization, SKID ID)
 * - Toyota Kanban Parser: Extracts all fields from long QR format (Description, Part Number, Supplier, Dock, Quantity, etc.)
 * - Auto-parse Internal Kanban: PART/KANBAN/SERIAL format (read-only after parse, single scan workflow)
 * - Accordion sections on Screen 2: Only one section expanded at a time, initially all collapsed
 * - Exception Modal: Popup dialog for adding exceptions (not collapsible section)
 * - Scanner input always visible below collapsible sections
 * - Toyota-specific exception types
 * - Exact theme match with skid-build (Navy #253262, Red #D2312E, Off-white #FCFCFC)
 * - TSCS-aligned confirmation number handling: Field always visible, generated at final submit
 * - Screen 1: Two flows - Manual entry (fill all fields) OR Scan flow (auto-parse, display, manual continue)
 * - Screen 2: Planned → Scanned workflow - Items move from planned list to scanned list upon successful scan
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
import {
  getSkidBuildOrder,
  startSkidBuildSession,
  recordSkidBuildScan,
  recordSkidBuildException,
  completeSkidBuildSession,
  type SkidBuildPlannedItem,
} from '@/lib/api';

// Screen types
type Screen = 1 | 2 | 3 | 4 | 5;

// Toyota-specific exception types (TSCS)
const EXCEPTION_TYPES = [
  'Revised Quantity (Toyota Quantity Reduction)',
  'Modified Quantity per Box',
  'Supplier Revised Shortage (Short Shipment)',
  'Non-Standard Packaging (Expendable)',
];

// Data interfaces
interface OrderData {
  owkNumber: string;
  plant: string;
  destination: string;
  dock: string;
  kanbanCount: number;
  loadId?: string;
  palletization?: string;
  skidId?: string;
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

interface ScannedItem {
  id: string;
  toyotaKanban: string;
  internalKanban: string;
  serialNumber: string;
  partNumber: string;
  quantity: number;
  timestamp: string;
  skidId?: string; // Multi-skid support - tracks which skid this item belongs to
}

interface PlannedItem {
  partNumber: string;
  description: string;
  plannedQty: number;
  scannedQty: number;
  rawKanbanValue?: string; // Store original kanban QR for matching
  plannedItemId?: string; // API planned item ID - Author: Hassan, Date: 2025-12-06
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

interface Exception {
  type: string;
  comments: string;
  timestamp: string;
}

interface InternalKanbanParsed {
  toyotaKanban: string;
  internalKanban: string;
  serialNumber: string;
}

// Helper Functions for Date and Time Formatting
// Author: Hassan, Date: 2025-11-05

/**
 * Format date from YYYYMMDD to YYYY/MM/DD
 * @param dateStr - Date string in YYYYMMDD format
 * @returns Formatted date string YYYY/MM/DD or original if invalid
 */
const formatDate = (dateStr: string): string => {
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
const formatTime = (timeStr: string): string => {
  if (!timeStr || timeStr.length !== 4) return timeStr;
  const hours = timeStr.substring(0, 2);
  const minutes = timeStr.substring(2, 4);
  return `${hours}:${minutes}`;
};

export default function SkidBuildV2Page() {
  const router = useRouter();

  // Screen state
  const [currentScreen, setCurrentScreen] = useState<Screen>(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // API Integration State - Author: Hassan, Date: 2025-12-06
  const [sessionId, setSessionId] = useState<string>('');
  const [orderId, setOrderId] = useState<string>('');

  // Screen 1: Order Search Data
  const [orderData, setOrderData] = useState<OrderData>({
    owkNumber: '',
    plant: '',
    destination: '',
    dock: '',
    kanbanCount: 0,
  });

  // Supplier Code state
  const [supplierCode, setSupplierCode] = useState('');

  // Screen 1: Track input mode (manual or scan)
  const [inputMode, setInputMode] = useState<'manual' | 'scan' | null>(null);
  const [parsedQRData, setParsedQRData] = useState<ParsedQRData | null>(null);

  // Screen 2: Main Skid Build Data
  const [plannedItems, setPlannedItems] = useState<PlannedItem[]>([]);
  const [scannedItems, setScannedItems] = useState<ScannedItem[]>([]);
  const [rackExceptionEnabled, setRackExceptionEnabled] = useState(false);
  const [fabMenuOpen, setFabMenuOpen] = useState(false);
  const [expandedSection, setExpandedSection] = useState<'order' | 'planned' | 'scanned' | 'exceptions' | null>(null);
  const [showExceptionModal, setShowExceptionModal] = useState(false);

  // Screen 3: Internal Kanban Data
  const [currentToyotaKanban, setCurrentToyotaKanban] = useState('');
  const [internalKanbanScanned, setInternalKanbanScanned] = useState('');
  const [parsedInternalKanban, setParsedInternalKanban] = useState<InternalKanbanParsed | null>(null);
  const [showToyotaDetails, setShowToyotaDetails] = useState(false);

  // Track scanned internal kanbans to prevent duplicates
  // Author: Hassan, Date: 2025-11-05
  const [scannedInternalKanbans, setScannedInternalKanbans] = useState<string[]>([]);

  // Screen 2: Exceptions Data (moved from Screen 4 - now inline on Screen 2)
  const [exceptions, setExceptions] = useState<Exception[]>([]);
  const [selectedExceptionType, setSelectedExceptionType] = useState('');
  const [exceptionComments, setExceptionComments] = useState('');

  // Alert state for save draft success
  const [showSuccessAlert, setShowSuccessAlert] = useState(false);

  // Confirmation number - generated when reaching Screen 5
  const [confirmationNumber, setConfirmationNumber] = useState<string>('');

  // Multi-skid support - Author: Hassan, Date: 2025-12-12
  // Tracks the currently open skid (set when manifest is scanned)
  // Format: "palletizationCode-skidId" (e.g., "A4-001A") to ensure uniqueness
  // Same skidId can exist with different palletization codes!
  const [currentSkidId, setCurrentSkidId] = useState<string | null>(null);
  const [skidSwitchMessage, setSkidSwitchMessage] = useState<string | null>(null);

  // Helper: Get unique skid identifier from palletization + skidId
  const getSkidIdentifier = (palletization: string, skidId: string) => `${palletization}-${skidId}`;

  // Parse Internal Kanban format: PART/KANBAN/SERIAL
  const parseInternalKanban = (scanned: string): InternalKanbanParsed | null => {
    const parts = scanned.split('/');

    if (parts.length !== 3) {
      setError('Invalid format. Expected: PART/KANBAN/SERIAL');
      return null;
    }

    const toyotaKanban = parts[0]?.trim();
    const internalKanban = parts[1]?.trim();
    const serialNumber = parts[2]?.trim();

    if (!toyotaKanban || !internalKanban || !serialNumber) {
      setError('All fields are required');
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
   * Updated: 2025-12-10 by Hassan - Toyota API Specification V2.0 alignment
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

  const parseToyotaKanban = (qrValue: string): ParsedToyotaKanban | null => {
    try {
      // Updated: 2025-11-05 by Hassan
      // CRITICAL: Uses FIXED POSITION SUBSTRING EXTRACTION for all fields
      // QR String length: Actual string is 216 characters (changed from 212)

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
      const containerType = qrString.substring(193, 195).trim();       // 55 (FIXED: was 192-194)
      const palletCode = qrString.substring(195, 197).trim();          // LB (FIXED: was 194-196)
      const controlField = qrString.substring(197, 202).trim();        // XAXXX (FIXED: was 196-201)
      const status = qrString.substring(207, 208).trim();              // 0 (FIXED: was 206-207)
      const zoneArea = qrString.substring(209, 211).trim();            // 05 (FIXED: was 207-212)

      console.log('Extracted fields (fixed positions):');
      console.log('  partDescription (1-12):', `"${partDescription}"` + ' (SKIPPED position 0 - the "C")');
      console.log('  partNumber (12-22):', `"${partNumber}"`);
      console.log('  supplierCode (31-36):', `"${supplierCode}"`);
      console.log('  dockCode (36-38):', `"${dockCode}"`);
      console.log('  kanbanNumber (38-42):', `"${kanbanNumber}"`);
      console.log('  quantity (74-79):', `"${quantity}"`);
      console.log('  supplierName (79-99):', `"${supplierName}"`);
      console.log('  loadId1 (99-108):', `"${loadId1}"`);
      console.log('  loadId2 (108-117):', `"${loadId2}"`);
      console.log('  planUnloadDate (117-125):', `"${planUnloadDate}"`);
      console.log('  shipDate (125-133):', `"${shipDate}"`);
      console.log('  shipTime (133-137):', `"${shipTime}"`);
      console.log('  plantCode (159-164):', `"${plantCode}"`);
      console.log('  route (183-192):', `"${route}"`);
      console.log('  containerType (193-195):', `"${containerType}"`);
      console.log('  palletCode (195-197):', `"${palletCode}"`);
      console.log('  controlField (197-202):', `"${controlField}"`);
      console.log('  status (207-208):', `"${status}"`);
      console.log('  zoneArea (209-211):', `"${zoneArea}"`);

      // UPDATED 2025-12-12 by Hassan:
      // Use partNumberRepeat (positions 42-53) as PRIMARY part number source
      // This field is RELIABLE across all Toyota Kanban formats:
      // - Some formats have blank positions 1-30 (no part# at 12-22)
      // - Some formats have identifiers at 1-30
      // - Some formats have description at 1-30
      // - BUT position 42-53 ALWAYS has the 12-char part number (with color code)
      const effectivePartNumber = partNumberRepeat || partNumber;

      // Validate required fields - use effectivePartNumber instead of partNumber
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
        partNumber: effectivePartNumber, // Use the reliable part number
        description: partDescription,
        supplierCode,
        dockCode,
        quantity,
        supplierName,
        loadId: loadId1, // Using first Load ID
        date: shipDate, // Using Ship Date as primary date
        plant: plantCode,
        location: route,
        rawValue: qrValue,
        // All additional fields
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

  // Parse QR Code format: 02TMI02806V82023080205  IDVV01      LB05001B
  // Author: Hassan, Date: 2025-11-04
  // Updated: 2025-11-04 - CRITICAL FIX: Corrected Load ID (24-36), MROS (38-40), and skidId (40-44) positions
  // CRITICAL: Uses FIXED POSITION SUBSTRING EXTRACTION - NO REGEX
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
      // Updated: 2025-12-10 by Hassan - Toyota API Specification V2.0 alignment
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

  // Screen 1: Handle QR Code Scan
  // Updated: 2025-11-05 by Hassan - Added Toyota Kanban rejection for Screen 1
  const handleOrderScan = async (result: ScanResult) => {
    console.log('Order scan result:', result);

    const scannedValue = result.scannedValue;

    // CRITICAL: Reject Toyota Kanban QR codes (they are 200+ characters)
    // Manifest QR codes are ~44 characters
    if (scannedValue.length > 100) {
      setError('Please scan the Toyota Manifest QR code, not the Toyota Kanban label.');
      return;
    }

    // Try to parse QR code regardless of Scanner validation
    const parsedData = parseQRCode(scannedValue);

    if (parsedData) {
      // QR code parsed successfully!
      setInputMode('scan');
      setParsedQRData(parsedData);
      setOrderData({
        owkNumber: parsedData.orderNumber,
        plant: parsedData.plantPrefix,
        destination: parsedData.loadId,
        dock: parsedData.dockCode,
        kanbanCount: 0, // Default
        loadId: parsedData.loadId,
        palletization: parsedData.palletizationCode,
        skidId: parsedData.rawSkidId, // Use rawSkidId for display (e.g., "001B")
      });
      setSupplierCode(parsedData.supplierCode);

      // Multi-skid support: Set current skid from first manifest scan
      // Author: Hassan, Date: 2025-12-12
      // Use palletization + skidId for unique identifier (e.g., "A4-001A")
      setCurrentSkidId(getSkidIdentifier(parsedData.palletizationCode, parsedData.rawSkidId));

      return;
    }

    // If parseQRCode failed AND Scanner validation failed, show error
    if (!result.success) {
      setError(result.error);
      return;
    }

    // If Scanner validation passed but parseQRCode failed, show format error
    setError('Invalid manifest QR code format. Please scan a valid Toyota Manifest QR code.');
  };

  // Screen 1: Validate and proceed
  // Updated: 2025-12-06 by Hassan - Integrated real API calls
  const handleOrderSearchNext = async (isFromScan: boolean = false) => {
    // For scan mode, only validate essential fields (destination and kanbanCount set to defaults)
    if (inputMode === 'scan' || isFromScan) {
      if (!orderData.owkNumber || !orderData.plant || !orderData.dock || !supplierCode) {
        setError('Missing required fields from QR scan');
        return;
      }
      // Set defaults for fields not in QR
      setOrderData(prev => ({
        ...prev,
        destination: prev.destination || 'N/A',
        kanbanCount: prev.kanbanCount || 1,
      }));
    } else {
      // For manual mode, validate all required fields
      if (!orderData.owkNumber || !orderData.plant || !orderData.dock || !supplierCode) {
        setError('Please fill all required fields before proceeding');
        return;
      }
      // Set defaults
      setOrderData(prev => ({
        ...prev,
        destination: prev.destination || 'N/A',
        kanbanCount: prev.kanbanCount || 1,
      }));
    }

    // Check if there's a saved draft for THIS OWK
    const savedDraft = localStorage.getItem('skid-build-v2-draft');
    if (savedDraft) {
      try {
        const draft = JSON.parse(savedDraft);

        // If saved OWK matches searched OWK, ask to resume
        if (draft.orderData?.owkNumber === orderData.owkNumber) {
          const resume = confirm(
            `Found saved session for ${orderData.owkNumber}. Resume previous session?`
          );

          if (resume) {
            // Restore all saved data
            setOrderData(draft.orderData);
            setSupplierCode(draft.supplierCode || '');
            setRackExceptionEnabled(draft.rackExceptionEnabled || false);
            setPlannedItems(draft.plannedItems || []);
            setScannedItems(draft.scannedItems || []);
            setExceptions(draft.exceptions || []);
            setConfirmationNumber(draft.confirmationNumber || '');
            setSessionId(draft.sessionId || '');
            setOrderId(draft.orderId || '');
            setCurrentScreen(draft.currentScreen || 2);
            return;
          } else {
            // User chose not to resume, clear the old draft
            localStorage.removeItem('skid-build-v2-draft');
          }
        }
        // If different OWK, don't load draft (start fresh)
      } catch (e) {
        console.error('Failed to load draft:', e);
      }
    }

    // API Integration: Fetch order and start session
    // Author: Hassan, Date: 2025-12-06
    setLoading(true);
    setError(null);

    try {
      // Step 1: Get order by number and dock code
      const orderResponse = await getSkidBuildOrder(orderData.owkNumber, orderData.dock);

      if (!orderResponse.success || !orderResponse.data) {
        // Fallback to mock data if API fails
        console.warn('API failed, using mock data:', orderResponse.errors);
        setError(`API Error: ${orderResponse.errors}. Using mock data for development.`);

        // Mock planned items with 2 Toyota Kanbans for demo
        const mockPlanned: PlannedItem[] = [
          {
            partNumber: '681010E250',
            description: 'TEM RH WEST',
            plannedQty: 1,
            scannedQty: 0,
            rawKanbanValue: 'CTEM RH WEST681010E250         02806V8VH98681010E25000SA-FDG    TV-00A    00045AGC AUTOMOTIVE (VUTEIDVV01   IDVV01   20230802202308011321002023080205  0001000102TMI                   MROS 05   55LBXAXXX     0 05',
          },
          {
            partNumber: '681020F150',
            description: 'TEM LH EAST',
            plannedQty: 1,
            scannedQty: 0,
            rawKanbanValue: 'CTEM LH EAST681020F150         02806V8VH99681020F15000SA-FDG    TV-00B    00030AGC AUTOMOTIVE (VUTEIDVV01   IDVV01   20230802202308011325002023080205  0002000202TMI                   MROS 05   55LBXAXXX     0 05',
          },
        ];

        setPlannedItems(mockPlanned);
        setCurrentScreen(2);
        setScannedInternalKanbans([]);
        setLoading(false);
        return;
      }

      // Step 2: Store order ID
      const fetchedOrder = orderResponse.data;
      setOrderId(fetchedOrder.orderId);

      // Step 3: Transform API planned items to UI format
      const apiPlannedItems: PlannedItem[] = fetchedOrder.plannedItems.map(item => ({
        partNumber: item.partNumber,
        description: `Kanban: ${item.kanbanNumber}`, // Use kanban number as description
        plannedQty: item.totalBoxPlanned,
        scannedQty: item.scannedCount,
        plannedItemId: item.plannedItemId,
      }));

      setPlannedItems(apiPlannedItems);

      // Step 4: Start a new session
      const sessionResponse = await startSkidBuildSession(
        fetchedOrder.orderId,
        1, // Default skid number
        'user-001' // TODO: Replace with actual user ID
      );

      if (!sessionResponse.success || !sessionResponse.data) {
        setError(`Failed to start session: ${sessionResponse.errors}`);
        setLoading(false);
        return;
      }

      setSessionId(sessionResponse.data.sessionId);

      // Step 5: Move to Screen 2
      setCurrentScreen(2);
      setError(null);
      setScannedInternalKanbans([]);
      setLoading(false);

    } catch (err) {
      console.error('Error in handleOrderSearchNext:', err);
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
      setLoading(false);
    }
  };

  // Screen 2: Handle Toyota Kanban Scan
  // Updated: 2025-12-10 by Hassan - Added palletization code validation
  // Updated: 2025-12-12 by Hassan - Multi-skid support: detect manifest scan to switch skids
  const handleToyotaKanbanScan = (result: ScanResult) => {
    console.log('Toyota Kanban scan result:', result);

    // Multi-skid support: Check if this is a manifest scan (short QR ~44 chars)
    // If user scans a manifest on Screen 2, switch to that skid
    if (result.scannedValue.length < 100) {
      const manifestParsed = parseQRCode(result.scannedValue);
      if (manifestParsed && manifestParsed.rawSkidId) {
        const newSkidId = getSkidIdentifier(manifestParsed.palletizationCode, manifestParsed.rawSkidId);
        setCurrentSkidId(newSkidId);
        setParsedQRData(manifestParsed); // Update manifest data for palletization validation
        setError(null);
        console.log('Multi-skid: Switched to skid', newSkidId);
        // Show skid switch message with full identifier
        setSkidSwitchMessage(`Switched to Skid ${newSkidId}`);
        setTimeout(() => setSkidSwitchMessage(null), 2500);
        return;
      }
    }

    // Try to parse Toyota Kanban regardless of Scanner validation (bypass Scanner checks)
    const parsedKanban = parseToyotaKanban(result.scannedValue);

    if (!parsedKanban) {
      // If parsing failed, show error
      setError('Invalid Toyota Kanban QR format. Please scan a valid Toyota Kanban.');
      return;
    }

    // Validate palletization code match (Manifest QR vs Toyota Kanban)
    // Only validate if we have parsed QR data from Screen 1
    if (parsedQRData) {
      const palletizationMatch = validatePalletizationMatch(
        parsedQRData.palletizationCode,
        parsedKanban.palletCode
      );

      if (!palletizationMatch) {
        setError(
          `Palletization code mismatch. Manifest QR: "${parsedQRData.palletizationCode}", Toyota Kanban: "${parsedKanban.palletCode}". Please verify the correct items are being scanned.`
        );
        return;
      }
    }

    // Check for duplicate Toyota Kanban
    const isDuplicate = scannedItems.some(
      item => item.toyotaKanban === result.scannedValue
    );

    if (isDuplicate) {
      setError(`Kanban has already been scanned. Please scan a different barcode.`);
      return;
    }

    // Match against planned list
    // Part number structure: First 10 chars = Part Number, Last 2 chars = Color Code
    // e.g., "649930E26000" = Part "649930E260" + Color "00"
    // QR gives us 10-char part number, DB has 12-char (part + color)
    // Compare first 10 characters only (ignore color code for matching)
    // Author: Hassan, 2025-12-06
    const getBasePartNumber = (pn: string) => pn.substring(0, 10);
    const matchedPlannedIndex = plannedItems.findIndex(
      item => getBasePartNumber(item.partNumber) === getBasePartNumber(parsedKanban.partNumber) ||
              item.rawKanbanValue === result.scannedValue
    );

    if (matchedPlannedIndex === -1) {
      setError(`Part ${parsedKanban.partNumber} (${parsedKanban.description}) is not in the planned list.`);
      return;
    }

    const matchedPlanned = plannedItems[matchedPlannedIndex];

    // Check if already fully scanned
    if (matchedPlanned.scannedQty >= matchedPlanned.plannedQty) {
      setError(`Part ${parsedKanban.partNumber} has already been fully scanned (${matchedPlanned.scannedQty}/${matchedPlanned.plannedQty}).`);
      return;
    }

    // Success! Store the parsed kanban and move to Internal Kanban screen (Screen 3)
    setCurrentToyotaKanban(result.scannedValue);
    setCurrentScreen(3);
    setError(null);
  };

  // Screen 2: FAB Menu - Save Draft
  // Updated: 2025-12-06 by Hassan - Include sessionId and orderId
  const handleSaveDraft = () => {
    const draft = {
      orderData,
      supplierCode,
      rackExceptionEnabled,
      plannedItems,
      scannedItems,
      exceptions,
      currentScreen,
      confirmationNumber,
      sessionId,
      orderId,
      savedAt: new Date().toISOString(),
    };

    localStorage.setItem('skid-build-v2-draft', JSON.stringify(draft));
    setFabMenuOpen(false);
    setShowSuccessAlert(true);
    setTimeout(() => setShowSuccessAlert(false), 3000);
  };

  // Screen 2: FAB Menu - Unpick All
  const handleUnpickAll = () => {
    if (confirm('Are you sure you want to unpick all scanned items?')) {
      setScannedItems([]);
      setPlannedItems(plannedItems.map(item => ({ ...item, scannedQty: 0 })));
      setFabMenuOpen(false);
    }
  };

  // Screen 3: Handle Internal Kanban Scan (auto-parse)
  // Updated: 2025-11-05 by Hassan - Added duplicate internal kanban prevention
  const handleInternalKanbanScan = (scannedValue: string) => {
    setInternalKanbanScanned(scannedValue);
    const parsed = parseInternalKanban(scannedValue);
    if (parsed) {
      // Check if this internal kanban has already been scanned
      if (scannedInternalKanbans.includes(parsed.internalKanban)) {
        setError(`Duplicate internal kanban: ${parsed.internalKanban} has already been scanned.`);
        setParsedInternalKanban(null);
        return;
      }

      setParsedInternalKanban(parsed);
      setError(null);
    }
  };

  // Screen 2: Add Exception (via modal)
  // Updated: 2025-12-06 by Hassan - Call recordSkidBuildException API
  const handleAddException = async () => {
    if (!selectedExceptionType || !exceptionComments.trim()) {
      setError('Please select exception type and add comments');
      return;
    }

    // Map exception type to code
    // REVISED_QTY=10, MODIFIED_QTY=11, SHORT_SHIPMENT=12, NON_STANDARD=20
    let exceptionCode = '20'; // Default to NON_STANDARD
    if (selectedExceptionType.includes('Revised Quantity')) {
      exceptionCode = '10';
    } else if (selectedExceptionType.includes('Modified Quantity')) {
      exceptionCode = '11';
    } else if (selectedExceptionType.includes('Short Shipment')) {
      exceptionCode = '12';
    }

    setLoading(true);
    setError(null);

    try {
      // Call API to record exception
      const exceptionResponse = await recordSkidBuildException(
        sessionId,
        orderId,
        exceptionCode,
        exceptionComments.trim(),
        1, // TODO: Get actual skid number
        'user-001' // TODO: Replace with actual user ID
      );

      if (!exceptionResponse.success) {
        setError(`Failed to record exception: ${exceptionResponse.errors}`);
        setLoading(false);
        return;
      }

      const newException: Exception = {
        type: selectedExceptionType,
        comments: exceptionComments.trim(),
        timestamp: new Date().toISOString(),
      };

      setExceptions([...exceptions, newException]);
      setSelectedExceptionType('');
      setExceptionComments('');
      setShowExceptionModal(false);
      setError(null);
      setLoading(false);

    } catch (err) {
      console.error('Error recording exception:', err);
      setError(err instanceof Error ? err.message : 'Failed to record exception');
      setLoading(false);
    }
  };

  // Screen 2: Remove Exception
  const handleRemoveException = (index: number) => {
    setExceptions(exceptions.filter((_, i) => i !== index));
  };

  // Screen 2 to 4: Move to Review/Submit Screen (previously Screen 5)
  const handleComplete = () => {
    // Do NOT generate confirmation number here
    // Number will be generated at final submit (TSCS behavior)
    setCurrentScreen(4);
  };

  // Screen 4: Final Submit - Generate confirmation and move to Screen 5
  // Updated: 2025-12-06 by Hassan - Call completeSkidBuildSession API
  const handleFinalSubmit = async () => {
    setLoading(true);
    setError(null);

    try {
      // Call API to complete session
      const completeResponse = await completeSkidBuildSession(
        sessionId,
        'user-001' // TODO: Replace with actual user ID
      );

      if (!completeResponse.success || !completeResponse.data) {
        setError(`Failed to complete session: ${completeResponse.errors}`);
        setLoading(false);
        return;
      }

      // Use confirmation number from API response
      setConfirmationNumber(completeResponse.data.confirmationNumber);

      // Clear the saved draft
      localStorage.removeItem('skid-build-v2-draft');

      // Move to Screen 5 (Confirmation Number Display)
      setCurrentScreen(5);
      setLoading(false);

    } catch (err) {
      console.error('Error completing session:', err);
      setError(err instanceof Error ? err.message : 'Failed to complete session');
      setLoading(false);
    }
  };

  // Reset everything
  const handleReset = () => {
    setCurrentScreen(1);
    setOrderData({ owkNumber: '', plant: '', destination: '', dock: '', kanbanCount: 0 });
    setSupplierCode('');
    setInputMode(null);
    setParsedQRData(null);
    setPlannedItems([]);
    setScannedItems([]);
    setExceptions([]);
    setRackExceptionEnabled(false);
    setInternalKanbanScanned('');
    setParsedInternalKanban(null);
    setCurrentToyotaKanban('');
    setShowToyotaDetails(false);
    setConfirmationNumber('');
    setError(null);
    // Clear scanned internal kanbans list
    // Author: Hassan, Date: 2025-11-05
    setScannedInternalKanbans([]);
    // Multi-skid support: Clear current skid
    // Author: Hassan, Date: 2025-12-12
    setCurrentSkidId(null);
    setSkidSwitchMessage(null);
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-3 pt-20 max-w-3xl mx-auto space-y-2 pb-20">
          {/* Progress Indicator */}
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
                      className="w-7 h-1 rounded-full"
                      style={{
                        backgroundColor: step <= currentScreen ? '#253262' : '#E5E7EB',
                      }}
                    />
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Error Alert */}
          {error && (
            <Alert variant="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          {/* Success Alert for Save Draft */}
          {showSuccessAlert && (
            <Alert variant="success" onClose={() => setShowSuccessAlert(false)}>
              <i className="fa-light fa-check-circle mr-2"></i>
              Draft saved successfully!
            </Alert>
          )}

          {/* Multi-skid: Skid Switch Message - Author: Hassan, Date: 2025-12-12 */}
          {skidSwitchMessage && (
            <Alert variant="info" onClose={() => setSkidSwitchMessage(null)}>
              <i className="fa-light fa-exchange mr-2"></i>
              {skidSwitchMessage}
            </Alert>
          )}

          {/* SCREEN 1: Order Search */}
          {currentScreen === 1 && (
            <Card>
              <CardContent className="p-3 space-y-3">
                {/* Header with Icon - Professional Style */}
                <div className="flex items-center gap-3 pb-3 border-b border-gray-200">
                  <i className="fa fa-boxes-stacked text-2xl" style={{ color: '#253262' }}></i>
                  <div className="flex-1">
                    <h1 className="text-lg font-semibold" style={{ color: '#253262' }}>
                      Order Search
                    </h1>
                    <p className="text-sm text-gray-600 mt-0.5">
                      Item confirmation for the skid build
                    </p>
                  </div>
                </div>

                {/* Tab Selection */}
                <div className="flex gap-2 mb-2">
                  <button
                    onClick={() => setInputMode('scan')}
                    className={`flex-1 py-2 px-3 rounded-lg text-sm font-medium transition-colors ${
                      inputMode === 'scan' || inputMode === null
                        ? 'bg-[#253262] text-white'
                        : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                    }`}
                  >
                    <i className="fa fa-qrcode mr-2"></i>
                    Scan Toyota Manifest
                  </button>
                  <button
                    onClick={() => {
                      setInputMode('manual');
                      setParsedQRData(null);
                    }}
                    className={`flex-1 py-2 px-3 rounded-lg text-sm font-medium transition-colors ${
                      inputMode === 'manual'
                        ? 'bg-[#253262] text-white'
                        : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
                    }`}
                  >
                    <i className="fa fa-keyboard mr-2"></i>
                    Manual Entry
                  </button>
                </div>

                {/* SCAN FLOW */}
                {(inputMode === 'scan' || inputMode === null) && !parsedQRData && (
                  <div className="space-y-2">
                    <Scanner
                      onScan={handleOrderScan}
                      label="Scan Toyota Manifest QR Code"
                      placeholder="Scan or enter Toyota Manifest data"
                      disabled={loading}
                    />
                  </div>
                )}

                {/* Display Scanned Information */}
                {parsedQRData && inputMode === 'scan' && (
                  <div className="space-y-2 pt-2">
                    <div className="p-3 bg-success-50 border-2 border-success-200 rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <i className="fa fa-circle-check text-success-600 text-lg"></i>
                        <h3 className="font-semibold text-sm text-success-700">Toyota Manifest Scanned Successfully</h3>
                      </div>

                      <div className="grid grid-cols-2 gap-x-3 gap-y-1.5 text-xs">
                        <div>
                          <span className="text-gray-600">Order Number:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.orderNumber}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Supplier Code:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.supplierCode}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Plant Code:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.plantPrefix}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Dock Code:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.dockCode}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Palletization:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.palletizationCode}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">MROS:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.mros}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">SKID ID:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.rawSkidId}</p>
                        </div>
                        <div>
                          <span className="text-gray-600">Load/Transport ID:</span>
                          <p className="font-mono font-bold text-gray-900">{parsedQRData.loadId}</p>
                        </div>
                      </div>

                      {/* Continue Button */}
                      <Button
                        onClick={() => handleOrderSearchNext(true)}
                        variant="success-light"
                        fullWidth
                      >
                        <i className="fa fa-arrow-right mr-2"></i>
                        Continue to Skid Build
                      </Button>
                    </div>
                  </div>
                )}

                {/* MANUAL ENTRY FLOW */}
                {inputMode === 'manual' && (
                  <div className="space-y-2 pt-2">
                    {/* Order Number */}
                    <div>
                      <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                        Order Number *
                      </label>
                      <input
                        type="text"
                        value={orderData.owkNumber}
                        onChange={(e) => setOrderData({ ...orderData, owkNumber: e.target.value })}
                        placeholder="Enter order number"
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                        style={{ backgroundColor: '#FCFCFC' }}
                      />
                    </div>

                    {/* Supplier Code */}
                    <div>
                      <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                        Supplier Code *
                      </label>
                      <input
                        type="text"
                        value={supplierCode}
                        onChange={(e) => setSupplierCode(e.target.value)}
                        placeholder="Enter supplier code"
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                        style={{ backgroundColor: '#FCFCFC' }}
                      />
                    </div>

                    {/* Plant Code */}
                    <div>
                      <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                        Plant Code *
                      </label>
                      <input
                        type="text"
                        value={orderData.plant}
                        onChange={(e) => setOrderData({ ...orderData, plant: e.target.value })}
                        placeholder="Enter plant code"
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                        style={{ backgroundColor: '#FCFCFC' }}
                      />
                    </div>

                    {/* Dock Code */}
                    <div>
                      <label className="block text-xs font-medium mb-1" style={{ color: '#253262' }}>
                        Dock Code *
                      </label>
                      <input
                        type="text"
                        value={orderData.dock}
                        onChange={(e) => setOrderData({ ...orderData, dock: e.target.value })}
                        placeholder="Enter dock code"
                        className="w-full px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                        style={{ backgroundColor: '#FCFCFC' }}
                      />
                    </div>

                    {/* Continue Button */}
                    <Button
                      onClick={() => handleOrderSearchNext()}
                      variant="success-light"
                      fullWidth
                      disabled={!orderData.owkNumber || !supplierCode || !orderData.plant || !orderData.dock}
                    >
                      <i className="fa fa-arrow-right mr-2"></i>
                      Continue to Skid Build
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* SCREEN 2: Main Skid Build */}
          {currentScreen === 2 && (
            <>
              {/* Order Details - Collapsible */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className="flex items-center justify-between cursor-pointer hover:bg-gray-50 p-4 rounded-t-lg transition-colors"
                    onClick={() => setExpandedSection(expandedSection === 'order' ? null : 'order')}
                  >
                    <div className="flex items-center gap-2">
                      <CardTitle className="text-sm">Order Details</CardTitle>
                      <span className={`text-xs px-2.5 py-1 rounded-full font-medium ${
                        confirmationNumber
                          ? 'bg-green-100 text-green-700'
                          : 'bg-pink-100 text-pink-700'
                      }`}>
                        {confirmationNumber ? 'Confirmed' : 'Unconfirmed'}
                      </span>
                      {/* Multi-skid support: Show current skid indicator - Author: Hassan, Date: 2025-12-12 */}
                      {currentSkidId && (
                        <span className="text-xs px-2.5 py-1 rounded-full font-medium bg-blue-100 text-blue-700">
                          Skid: {currentSkidId}
                        </span>
                      )}
                    </div>
                    <i className={`fa-light fa-chevron-${expandedSection === 'order' ? 'up' : 'down'}`}></i>
                  </div>
                </CardHeader>
                {expandedSection === 'order' && (
                  <CardContent>
                    <div className="space-y-2 text-sm">
                      {/* Confirmation # - ALWAYS VISIBLE (TSCS-aligned behavior) */}
                      <div className="flex justify-between py-2 border-b">
                        <span className="text-gray-600">Confirmation #:</span>
                        <span className="font-semibold">
                          {confirmationNumber || '---'}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Supplier Code:</span>
                        <span className="font-semibold">{supplierCode || 'N/A'}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">OWK Order:</span>
                        <span className="font-mono font-bold">{orderData.owkNumber}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Plant:</span>
                        <span className="font-medium">{orderData.plant}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Destination:</span>
                        <span className="font-medium">{orderData.destination}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Dock Code:</span>
                        <span className="font-medium">{orderData.dock}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-gray-600">Kanban Count:</span>
                        <span className="font-medium">{orderData.kanbanCount}</span>
                      </div>
                    </div>
                  </CardContent>
                )}
              </Card>

              {/* Planned Items - Collapsible */}
              {/* Only show items NOT fully scanned (scannedQty < plannedQty) */}
              {/* Fully scanned items move to Scanned section */}
              {(() => {
                const remainingPlanned = plannedItems.filter(item => item.scannedQty < item.plannedQty);
                const completedCount = plannedItems.length - remainingPlanned.length;

                return (
                  <Card>
                    <CardHeader className="p-0">
                      <div
                        className="flex items-center justify-between cursor-pointer hover:bg-gray-50 p-4 rounded-t-lg transition-colors"
                        onClick={() => setExpandedSection(expandedSection === 'planned' ? null : 'planned')}
                      >
                        <CardTitle className="text-sm">
                          Planned Items ({remainingPlanned.length})
                          {completedCount > 0 && (
                            <span className="text-xs text-green-600 ml-2">
                              ({completedCount} complete)
                            </span>
                          )}
                        </CardTitle>
                        <i className={`fa-light fa-chevron-${expandedSection === 'planned' ? 'up' : 'down'}`}></i>
                      </div>
                    </CardHeader>
                    {expandedSection === 'planned' && (
                      <CardContent className="space-y-2">
                        {remainingPlanned.length === 0 ? (
                          <p className="text-sm text-green-600 text-center py-4">
                            <i className="fa fa-check-circle mr-2"></i>
                            All items scanned!
                          </p>
                        ) : (
                          remainingPlanned.map((item, idx) => (
                            <div
                              key={idx}
                              className={`p-3 border rounded-lg ${
                                item.scannedQty > 0
                                  ? 'border-yellow-400 bg-yellow-50'
                                  : 'border-gray-200 bg-white'
                              }`}
                            >
                              <div className="flex items-center justify-between">
                                <div className="flex-1">
                                  <p className="font-mono text-sm font-bold" style={{ color: '#253262' }}>
                                    {item.partNumber}
                                  </p>
                                  <p className="text-xs text-gray-600">{item.description}</p>
                                </div>
                                <div className="text-right">
                                  <p className="text-sm">
                                    <span className="font-bold" style={{ color: item.scannedQty > 0 ? '#F59E0B' : '#253262' }}>
                                      {item.scannedQty}
                                    </span>
                                    <span className="text-gray-500"> / {item.plannedQty}</span>
                                  </p>
                                  {item.scannedQty > 0 && (
                                    <Badge variant="warning">Partial</Badge>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))
                        )}
                      </CardContent>
                    )}
                  </Card>
                );
              })()}

              {/* Scanned Items - Collapsible */}
              {/* Computed from plannedItems.scannedQty (from database) - persists across refresh */}
              {(() => {
                // Items with scannedQty > 0 are "scanned"
                const scannedFromPlanned = plannedItems.filter(item => item.scannedQty > 0);
                const totalScannedCount = scannedFromPlanned.reduce((sum, item) => sum + item.scannedQty, 0);

                return (
                  <Card>
                    <CardHeader className="p-0">
                      <div
                        className="flex items-center justify-between cursor-pointer hover:bg-gray-50 p-4 rounded-t-lg transition-colors"
                        onClick={() => setExpandedSection(expandedSection === 'scanned' ? null : 'scanned')}
                      >
                        <CardTitle className="text-sm">
                          Scanned Items ({totalScannedCount})
                        </CardTitle>
                        <i className={`fa-light fa-chevron-${expandedSection === 'scanned' ? 'up' : 'down'}`}></i>
                      </div>
                    </CardHeader>
                    {expandedSection === 'scanned' && (
                      <CardContent className="space-y-2">
                        {scannedFromPlanned.length === 0 ? (
                          <p className="text-sm text-gray-500 text-center py-4">
                            No items scanned yet
                          </p>
                        ) : (
                          scannedFromPlanned.map((item, idx) => (
                            <div
                              key={idx}
                              className={`p-3 border-2 rounded-lg ${
                                item.scannedQty >= item.plannedQty
                                  ? 'border-success-500 bg-green-50'
                                  : 'border-yellow-400 bg-yellow-50'
                              }`}
                            >
                              <div className="flex items-center justify-between">
                                <div className="flex-1">
                                  <p className="font-mono text-sm font-bold" style={{ color: '#253262' }}>
                                    {item.partNumber}
                                  </p>
                                  <p className="text-xs text-gray-600">{item.description}</p>
                                </div>
                                <div className="flex flex-col items-end gap-1">
                                  <p className="text-sm font-bold">
                                    <span className={item.scannedQty >= item.plannedQty ? 'text-green-600' : 'text-yellow-600'}>
                                      {item.scannedQty}
                                    </span>
                                    <span className="text-gray-500"> / {item.plannedQty}</span>
                                  </p>
                                  {item.scannedQty >= item.plannedQty ? (
                                    <Badge variant="success">Complete</Badge>
                                  ) : (
                                    <Badge variant="warning">Partial</Badge>
                                  )}
                                </div>
                              </div>
                            </div>
                          ))
                        )}
                      </CardContent>
                    )}
                  </Card>
                );
              })()}

              {/* Exceptions List - Collapsible - ALWAYS VISIBLE */}
              <Card>
                <CardHeader className="p-0">
                  <div
                    className="flex items-center justify-between cursor-pointer hover:bg-gray-50 p-4 rounded-t-lg transition-colors"
                    onClick={() => setExpandedSection(expandedSection === 'exceptions' ? null : 'exceptions')}
                  >
                    <CardTitle className="text-sm">
                      Exceptions ({exceptions.length})
                    </CardTitle>
                    <i className={`fa-light fa-chevron-${expandedSection === 'exceptions' ? 'up' : 'down'}`}></i>
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
                              <p className="text-sm text-gray-700">{exception.comments}</p>
                            </div>
                            <button
                              onClick={() => handleRemoveException(idx)}
                              className="text-error-600 hover:text-error-700 p-1"
                              aria-label="Remove exception"
                            >
                              <i className="fa-light fa-trash"></i>
                            </button>
                          </div>
                        </div>
                      ))
                    )}
                  </CardContent>
                )}
              </Card>

              {/* Scan Toyota Kanban - ALWAYS VISIBLE (NOT COLLAPSIBLE) */}
              <div className="space-y-3">
                <Scanner
                  onScan={handleToyotaKanbanScan}
                  label="Scan Toyota Kanban"
                  placeholder="Scan Toyota Kanban QR Code"
                />
              </div>

              {/* Action Buttons */}
              <div className="flex gap-2">
                <Button
                  onClick={() => setShowExceptionModal(true)}
                  variant="warning"
                  fullWidth
                >
                  <i className="fa-light fa-exclamation-triangle mr-2"></i>
                  Add Exception
                </Button>
                <Button
                  onClick={handleComplete}
                  variant="success"
                  fullWidth
                >
                  <i className="fa-light fa-check-circle mr-2"></i>
                  Complete
                </Button>
              </div>
            </>
          )}

          {/* SCREEN 3: Internal Kanban */}
          {currentScreen === 3 && (
            <div className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle>Scan Internal Kanban</CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Collapsible Toyota Kanban Details */}
                  <div className="border rounded-lg p-3 bg-gray-50">
                    <button
                      onClick={() => setShowToyotaDetails(!showToyotaDetails)}
                      className="w-full flex items-center justify-between text-left hover:bg-gray-100 -m-3 p-3 rounded-lg transition-colors"
                    >
                      <span className="font-medium text-sm" style={{ color: '#253262' }}>
                        <i className={`fa fa-chevron-${showToyotaDetails ? 'down' : 'right'} mr-2 text-xs`}></i>
                        Toyota Kanban Details
                      </span>
                      <Badge variant="info">
                        {showToyotaDetails ? 'Hide' : 'Show'}
                      </Badge>
                    </button>

                    {showToyotaDetails && (() => {
                      // Only parse if currentToyotaKanban is not empty
                      const parsedToyota = currentToyotaKanban && currentToyotaKanban.trim()
                        ? parseToyotaKanban(currentToyotaKanban)
                        : null;
                      return parsedToyota ? (
                        <div className="mt-3 pt-3 border-t border-gray-200 max-h-96 overflow-y-auto">
                          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                            {/* Part Description */}
                            <div>
                              <span className="text-sm text-gray-600">Part Description: </span>
                              <span className="font-medium">{parsedToyota.description}</span>
                            </div>

                            {/* Part Number */}
                            <div>
                              <span className="text-sm text-gray-600">Part Number: </span>
                              <span className="font-medium">{parsedToyota.partNumber}</span>
                            </div>

                            {/* Supplier Code */}
                            <div>
                              <span className="text-sm text-gray-600">Supplier Code: </span>
                              <span className="font-medium">{parsedToyota.supplierCode}</span>
                            </div>

                            {/* Dock Code */}
                            <div>
                              <span className="text-sm text-gray-600">Dock Code: </span>
                              <span className="font-medium">{parsedToyota.dockCode}</span>
                            </div>

                            {/* Kanban Number */}
                            <div>
                              <span className="text-sm text-gray-600">Kanban Number: </span>
                              <span className="font-medium">{parsedToyota.kanbanNumber || 'N/A'}</span>
                            </div>

                            {/* Line Side Address */}
                            <div>
                              <span className="text-sm text-gray-600">Line Side Address: </span>
                              <span className="font-medium">{parsedToyota.lineSideAddress || 'N/A'}</span>
                            </div>

                            {/* Store Address */}
                            <div>
                              <span className="text-sm text-gray-600">Store Address: </span>
                              <span className="font-medium">{parsedToyota.storeAddress || 'N/A'}</span>
                            </div>

                            {/* Quantity */}
                            <div>
                              <span className="text-sm text-gray-600">Quantity: </span>
                              <span className="font-medium">{parsedToyota.quantity}</span>
                            </div>

                            {/* Supplier Name */}
                            <div>
                              <span className="text-sm text-gray-600">Supplier Name: </span>
                              <span className="font-medium">{parsedToyota.supplierName || 'N/A'}</span>
                            </div>

                            {/* Load/Transport ID */}
                            <div>
                              <span className="text-sm text-gray-600">Load/Transport ID: </span>
                              <span className="font-medium">{parsedToyota.loadId1 || 'N/A'}</span>
                            </div>

                            {/* Plan Unload Date */}
                            <div>
                              <span className="text-sm text-gray-600">Plan Unload Date: </span>
                              <span className="font-medium">{parsedToyota.planUnloadDate ? formatDate(parsedToyota.planUnloadDate) : 'N/A'}</span>
                            </div>

                            {/* Ship Date */}
                            <div>
                              <span className="text-sm text-gray-600">Ship Date: </span>
                              <span className="font-medium">{parsedToyota.shipDate ? formatDate(parsedToyota.shipDate) : 'N/A'}</span>
                            </div>

                            {/* Ship Time */}
                            <div>
                              <span className="text-sm text-gray-600">Ship Time: </span>
                              <span className="font-medium">{parsedToyota.shipTime ? formatTime(parsedToyota.shipTime) : 'N/A'}</span>
                            </div>

                            {/* Control Code */}
                            <div>
                              <span className="text-sm text-gray-600">Control Code: </span>
                              <span className="font-medium">{parsedToyota.controlCode || 'N/A'}</span>
                            </div>

                            {/* Delivery Order */}
                            <div>
                              <span className="text-sm text-gray-600">Delivery Order: </span>
                              <span className="font-medium">{parsedToyota.deliveryOrder || 'N/A'}</span>
                            </div>

                            {/* Box Number */}
                            <div>
                              <span className="text-sm text-gray-600">Box Number: </span>
                              <span className="font-medium">{parsedToyota.boxNumber || 'N/A'}</span>
                            </div>

                            {/* Total Boxes */}
                            <div>
                              <span className="text-sm text-gray-600">Total Boxes: </span>
                              <span className="font-medium">{parsedToyota.totalBoxes || 'N/A'}</span>
                            </div>

                            {/* Plant Code */}
                            <div>
                              <span className="text-sm text-gray-600">Plant Code: </span>
                              <span className="font-medium">{parsedToyota.plantCode || 'N/A'}</span>
                            </div>

                            {/* Route */}
                            <div>
                              <span className="text-sm text-gray-600">Route: </span>
                              <span className="font-medium">{parsedToyota.route || 'N/A'}</span>
                            </div>

                            {/* Container Type */}
                            <div>
                              <span className="text-sm text-gray-600">Container Type: </span>
                              <span className="font-medium">{parsedToyota.containerType || 'N/A'}</span>
                            </div>

                            {/* Pallet Code */}
                            <div>
                              <span className="text-sm text-gray-600">Pallet Code: </span>
                              <span className="font-medium">{parsedToyota.palletCode || 'N/A'}</span>
                            </div>

                            {/* Control Field */}
                            <div>
                              <span className="text-sm text-gray-600">Control Field: </span>
                              <span className="font-medium">{parsedToyota.controlField || 'N/A'}</span>
                            </div>

                            {/* Status */}
                            <div>
                              <span className="text-sm text-gray-600">Status: </span>
                              <span className="font-medium">{parsedToyota.status || 'N/A'}</span>
                            </div>

                            {/* Zone/Area */}
                            <div>
                              <span className="text-sm text-gray-600">Zone/Area: </span>
                              <span className="font-medium">{parsedToyota.zoneArea || 'N/A'}</span>
                            </div>
                          </div>
                        </div>
                      ) : (
                        <div className="mt-3 pt-3 border-t border-gray-200">
                          <p className="text-xs text-gray-500 italic">Unable to parse Toyota Kanban details</p>
                        </div>
                      );
                    })()}
                  </div>

                  {/* Alert showing current Toyota Kanban being paired */}
                  <Alert variant="info">
                    <i className="fa-light fa-info-circle mr-2"></i>
                    Pairing with Toyota Kanban: <strong className="font-mono text-xs">{currentToyotaKanban.substring(0, 50)}...</strong>
                  </Alert>

                  {/* Scanner for Internal Kanban */}
                  <div>
                    <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                      Scan Internal Kanban QR Code
                    </label>
                    <input
                      type="text"
                      value={internalKanbanScanned}
                      onChange={(e) => handleInternalKanbanScan(e.target.value)}
                      placeholder="Format: PART/KANBAN/SERIAL"
                      className="w-full px-4 py-3 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 font-mono"
                      style={{ backgroundColor: '#FCFCFC' }}
                    />
                    <p className="text-xs text-gray-500 mt-1">Example: 56089-08E90-00/MPE/001</p>
                  </div>

                  {/* Show parsed fields as READ-ONLY after scan */}
                  {internalKanbanScanned && parsedInternalKanban && (
                    <div className="space-y-3 p-4 bg-gray-50 rounded-lg">
                      <h3 className="font-semibold text-sm text-gray-700">Scanned:</h3>

                      <div>
                        <label className="text-xs text-gray-600">Toyota Kanban (Part Number)</label>
                        <div className="p-2 bg-white border rounded mt-1 text-gray-700 font-mono text-sm">
                          {parsedInternalKanban.toyotaKanban}
                        </div>
                      </div>

                      <div>
                        <label className="text-xs text-gray-600">Internal Kanban</label>
                        <div className="p-2 bg-white border rounded mt-1 text-gray-700 font-mono text-sm">
                          {parsedInternalKanban.internalKanban}
                        </div>
                      </div>

                      <div>
                        <label className="text-xs text-gray-600">Serial Number</label>
                        <div className="p-2 bg-white border rounded mt-1 text-gray-700 font-mono text-sm">
                          {parsedInternalKanban.serialNumber}
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Continue Button */}
                  {/* Updated: 2025-12-06 by Hassan - Call recordSkidBuildScan API */}
                  {parsedInternalKanban && (
                    <Button
                      variant="success"
                      onClick={async () => {
                        // Parse the current Toyota Kanban to get part number and box data
                        const parsedKanban = parseToyotaKanban(currentToyotaKanban);

                        if (!parsedKanban) {
                          setError('Failed to parse Toyota Kanban data');
                          return;
                        }

                        // Find matching planned item to get plannedItemId
                        // Part number structure: First 10 chars = Part Number, Last 2 chars = Color Code
                        // e.g., "649930E26000" = Part "649930E260" + Color "00"
                        // Compare first 10 characters only (ignore color code for matching)
                        // Author: Hassan, 2025-12-06
                        const getBasePartNumber = (pn: string) => pn.substring(0, 10);
                        const matchedPlannedItem = plannedItems.find(
                          item => getBasePartNumber(item.partNumber) === getBasePartNumber(parsedKanban.partNumber)
                        );

                        if (!matchedPlannedItem || !matchedPlannedItem.plannedItemId) {
                          setError('Could not find matching planned item for this scan');
                          return;
                        }

                        setLoading(true);
                        setError(null);

                        try {
                          // Call API to record the scan
                          // Updated: 2025-12-10 by Hassan - Toyota API Specification V2.0 alignment
                          // - skidNumber from Manifest QR (parsedQRData.skidNumber) - NOT from Toyota Kanban!
                          // - boxNumber from Toyota Kanban (parsedKanban.boxNumber)
                          // - These are now SEPARATE values
                          const scanResponse = await recordSkidBuildScan(
                            sessionId,
                            matchedPlannedItem.plannedItemId,
                            parsedQRData?.skidNumber || '001',                  // Skid number from Manifest QR - string "001"
                            parsedQRData?.skidSide || 'A',                      // Skid side from Manifest QR - "A" or "B"
                            parsedQRData?.rawSkidId || '001A',                  // Raw skid ID from Manifest QR - "001B"
                            parseInt(parsedKanban.boxNumber) || 1,              // Box number from Toyota Kanban - SEPARATE!
                            parsedKanban.lineSideAddress || 'N/A',              // Line side address from Toyota Kanban
                            parsedQRData?.palletizationCode || '',              // Palletization code from Manifest QR
                            parsedInternalKanban.internalKanban,
                            'user-001' // TODO: Replace with actual user ID
                          );

                          if (!scanResponse.success) {
                            setError(`Failed to record scan: ${scanResponse.errors}`);
                            setLoading(false);
                            return;
                          }

                          // Add scanned item to the list
                          // Multi-skid support: Include skidId - Author: Hassan, Date: 2025-12-12
                          const newScannedItem: ScannedItem = {
                            id: scanResponse.data?.scanId || `${Date.now()}-${Math.random()}`,
                            toyotaKanban: currentToyotaKanban,
                            internalKanban: parsedInternalKanban.internalKanban,
                            serialNumber: parsedInternalKanban.serialNumber,
                            partNumber: parsedKanban?.partNumber || parsedInternalKanban.toyotaKanban,
                            quantity: 1,
                            timestamp: new Date().toISOString(),
                            skidId: currentSkidId || undefined, // Track which skid this item belongs to
                          };

                          setScannedItems([...scannedItems, newScannedItem]);

                          // Update planned items scanned quantity
                          // Fix: Use same matching logic as API call (first 10 chars)
                          // Author: Hassan, Date: 2025-12-12
                          setPlannedItems(plannedItems.map(item => {
                            if (getBasePartNumber(item.partNumber) === getBasePartNumber(parsedKanban.partNumber)) {
                              return { ...item, scannedQty: item.scannedQty + 1 };
                            }
                            return item;
                          }));

                          // Add internal kanban to the scanned list to prevent duplicates
                          setScannedInternalKanbans([...scannedInternalKanbans, parsedInternalKanban.internalKanban]);

                          // Reset for next scan
                          setInternalKanbanScanned('');
                          setParsedInternalKanban(null);
                          setCurrentToyotaKanban('');
                          setShowToyotaDetails(false);
                          setCurrentScreen(2);
                          setLoading(false);

                        } catch (err) {
                          console.error('Error recording scan:', err);
                          setError(err instanceof Error ? err.message : 'Failed to record scan');
                          setLoading(false);
                        }
                      }}
                      fullWidth
                      loading={loading}
                    >
                      <i className="fa-light fa-arrow-right mr-2"></i>
                      Continue
                    </Button>
                  )}
                </CardContent>
              </Card>
            </div>
          )}

          {/* SCREEN 4: Review/Submit (previously Screen 5) */}
          {currentScreen === 4 && (
            <Card>
              <CardHeader>
                <CardTitle>Review and Submit</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">

                {/* Order Summary */}
                <div className="p-4 border border-gray-200 rounded-lg" style={{ backgroundColor: '#FFFFFF' }}>
                  <p className="text-sm font-semibold mb-3" style={{ color: '#253262' }}>
                    Order Summary:
                  </p>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">OWK Order:</span>
                      <span className="font-mono font-bold">{orderData.owkNumber}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Plant:</span>
                      <span className="font-medium">{orderData.plant}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Destination:</span>
                      <span className="font-medium">{orderData.destination}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Dock Code:</span>
                      <span className="font-medium">{orderData.dock}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Kanban Count:</span>
                      <span className="font-medium">{orderData.kanbanCount}</span>
                    </div>
                  </div>
                </div>

                {/* Scanned Items Summary */}
                <div className="p-4 border border-gray-200 rounded-lg" style={{ backgroundColor: '#FFFFFF' }}>
                  <p className="text-sm font-semibold mb-3" style={{ color: '#253262' }}>
                    Scanned Items Summary:
                  </p>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-gray-600">Total Items Scanned:</span>
                      <span className="font-bold text-lg" style={{ color: '#10B981' }}>
                        {scannedItems.length}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-gray-600">Exceptions Recorded:</span>
                      <span className="font-bold text-lg" style={{ color: exceptions.length > 0 ? '#F59E0B' : '#10B981' }}>
                        {exceptions.length}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Exceptions Summary */}
                {exceptions.length > 0 && (
                  <div className="p-4 bg-warning-50 border border-warning-200 rounded-lg">
                    <p className="text-sm font-semibold mb-2" style={{ color: '#253262' }}>
                      Exceptions:
                    </p>
                    {exceptions.map((exception, idx) => (
                      <div key={idx} className="text-xs text-gray-700 mb-2">
                        <Badge variant="warning">{exception.type}</Badge>
                        <p className="mt-1">{exception.comments}</p>
                      </div>
                    ))}
                  </div>
                )}

                {/* Final Actions */}
                <div className="flex gap-2 pt-3">
                  <Button
                    onClick={() => setCurrentScreen(2)}
                    variant="secondary"
                    fullWidth
                  >
                    <i className="fa-light fa-arrow-left mr-2"></i>
                    Back to Edit
                  </Button>
                  <Button
                    onClick={handleFinalSubmit}
                    variant="success"
                    fullWidth
                    loading={loading}
                  >
                    <i className="fa-light fa-paper-plane mr-2"></i>
                    Submit Skid Build
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {/* SCREEN 5: Confirmation Number Display (NEW - shows after submission) */}
          {currentScreen === 5 && (
            <Card>
              <CardHeader>
                <CardTitle>Build Submitted Successfully!</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {/* Success Icon */}
                <div className="text-center py-4">
                  <div className="inline-flex items-center justify-center w-20 h-20 rounded-full bg-success-100 mb-4">
                    <i className="fa fa-check-circle text-4xl text-success-600"></i>
                  </div>
                  <h2 className="text-xl font-bold text-gray-800 mb-2">Skid Build Complete!</h2>
                  <p className="text-sm text-gray-600">Your skid build has been successfully submitted.</p>
                </div>

                {/* Confirmation Number - Prominent Display */}
                <div className="p-6 border-2 border-success-500 rounded-lg bg-gradient-to-br from-success-50 to-white shadow-md">
                  <div className="text-center">
                    <p className="text-xs uppercase tracking-wide text-gray-600 font-semibold mb-2">
                      Confirmation Number
                    </p>
                    <div className="flex items-center justify-center gap-2 bg-white p-4 rounded-lg border border-success-200">
                      <span className="font-mono text-lg font-bold text-success-700 select-all">
                        {confirmationNumber}
                      </span>
                    </div>
                  </div>
                </div>

                {/* Order Summary - Compact */}
                <div className="p-4 border border-gray-200 rounded-lg bg-gray-50">
                  <p className="text-xs font-semibold mb-2 text-gray-700">Order Summary:</p>
                  <div className="grid grid-cols-2 gap-2 text-xs">
                    <div>
                      <span className="text-gray-600">OWK Order:</span>
                      <p className="font-mono font-bold text-gray-800">{orderData.owkNumber}</p>
                    </div>
                    <div>
                      <span className="text-gray-600">Plant:</span>
                      <p className="font-semibold text-gray-800">{orderData.plant}</p>
                    </div>
                    <div>
                      <span className="text-gray-600">Items Scanned:</span>
                      <p className="font-bold text-success-700">{scannedItems.length}</p>
                    </div>
                    <div>
                      <span className="text-gray-600">Exceptions:</span>
                      <p className="font-bold" style={{ color: exceptions.length > 0 ? '#F59E0B' : '#10B981' }}>
                        {exceptions.length}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Done Button */}
                <Button
                  onClick={() => {
                    handleReset();
                    router.push('/');
                  }}
                  variant="primary"
                  fullWidth
                >
                  <i className="fa-light fa-home mr-2"></i>
                  Done - Return to Home
                </Button>
              </CardContent>
            </Card>
          )}

          {/* Global Action Buttons - Hide on Screen 5 */}
          {currentScreen !== 5 && (
            <div className="flex gap-2">
              <Button
                onClick={() => router.push('/')}
                variant="error"
                fullWidth
              >
                <i className="fa-light fa-xmark mr-2"></i>
                Cancel
              </Button>
              {currentScreen > 1 && (
                <Button
                  onClick={handleReset}
                  variant="primary"
                  fullWidth
                >
                  <i className="fa-light fa-rotate-right mr-2"></i>
                  Start Over
                </Button>
              )}
            </div>
          )}
        </div>
      </div>

      {/* FAB Menu (Bottom Left) - Visible on Screens 1-4 */}
      {currentScreen >= 1 && currentScreen <= 4 && (
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
            <div className="absolute bottom-16 left-0 bg-white rounded-lg shadow-xl py-1 min-w-[220px] border border-gray-200">
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

              {/* Unpick All - ALL SCREENS (disabled when not on Screen 2) */}
              <button
                onClick={() => {
                  handleUnpickAll();
                  setFabMenuOpen(false);
                }}
                disabled={currentScreen !== 2 || scannedItems.length === 0}
                className={`flex items-center gap-3 px-4 py-3 w-full transition-colors text-left ${
                  currentScreen === 2 && scannedItems.length > 0
                    ? 'hover:bg-gray-50 cursor-pointer'
                    : 'opacity-50 cursor-not-allowed bg-gray-50'
                }`}
              >
                <i className={`fa-light fa-trash text-lg ${
                  currentScreen === 2 && scannedItems.length > 0 ? 'text-red-500' : 'text-gray-400'
                }`}></i>
                <span className="text-sm font-medium text-gray-700">Unpick All</span>
              </button>

              {/* Rack Exception - ALL SCREENS (disabled when not on Screen 2) */}
              <button
                onClick={() => {
                  setRackExceptionEnabled(!rackExceptionEnabled);
                  setFabMenuOpen(false);
                }}
                disabled={currentScreen !== 2}
                className={`flex items-center gap-3 px-4 py-3 w-full transition-colors text-left ${
                  currentScreen === 2
                    ? 'hover:bg-gray-50 cursor-pointer'
                    : 'opacity-50 cursor-not-allowed bg-gray-50'
                }`}
              >
                <i className={`fa-light fa-warehouse text-lg ${
                  currentScreen === 2
                    ? (rackExceptionEnabled ? 'text-green-500' : 'text-gray-400')
                    : 'text-gray-400'
                }`}></i>
                <span className="text-sm font-medium text-gray-700">Rack Exception</span>
                <Badge className={`ml-auto ${
                  currentScreen === 2 && rackExceptionEnabled ? 'bg-green-500' : 'bg-gray-400'
                } text-white text-xs px-2 py-0.5 rounded`}>
                  {rackExceptionEnabled ? 'ON' : 'OFF'}
                </Badge>
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
            setExceptionComments('');
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
                }}
                className="text-gray-400 hover:text-gray-600 transition-colors"
                aria-label="Close modal"
              >
                <i className="fa-light fa-xmark text-2xl"></i>
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
                disabled={!selectedExceptionType || !exceptionComments.trim()}
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
