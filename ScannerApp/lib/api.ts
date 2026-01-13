/**
 * API Client and Mock Functions
 * Author: Hassan
 * Date: 2025-10-20
 * Mock API implementation for future backend integration
 * Updated: 2025-12-06 - Added Skid Build V2 APIs with apiClient for JWT auth
 */

import { API_ENDPOINTS, TIMING, ERROR_MESSAGES } from './constants';
import apiClient, { getErrorMessage } from './api/client';
import type {
  ApiResponse,
  Order,
  DriverCheckSheet,
  ShipmentLoad,
  DockStatus,
  SupplierRoute,
  ComplianceReport,
  ScanResult,
  ToyotaKanban,
  InternalKanban,
  SerialNumber,
  TrailerInfo,
} from '@/types';

/**
 * Base API fetch with timeout
 */
async function apiFetch<T>(
  url: string,
  options: RequestInit = {}
): Promise<ApiResponse<T>> {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), TIMING.API_TIMEOUT);

  try {
    const response = await fetch(url, {
      ...options,
      signal: controller.signal,
      headers: {
        'Content-Type': 'application/json',
        ...options.headers,
      },
    });

    clearTimeout(timeout);

    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }

    const data = await response.json();
    return {
      success: true,
      data: data as T,
      error: null,
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    clearTimeout(timeout);

    if (error instanceof Error && error.name === 'AbortError') {
      return {
        success: false,
        data: null,
        error: ERROR_MESSAGES.TIMEOUT_ERROR,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: error instanceof Error ? error.message : ERROR_MESSAGES.SERVER_ERROR,
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== MOCK DATA GENERATORS =====

/**
 * Mock: Generate sample order
 */
function generateMockOrder(orderNumber: string): Order {
  return {
    id: `order-${Date.now()}`,
    owkOrderNumber: orderNumber,
    customerName: 'Toyota Motor Manufacturing',
    destination: 'Georgetown, KY',
    totalSkids: 5,
    completedSkids: 0,
    status: 'PENDING',
    deliveryDate: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString(),
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString(),
  };
}

/**
 * Mock: Generate Toyota Kanban
 */
function generateMockToyotaKanban(kanbanNumber: string): ToyotaKanban {
  return {
    id: `tk-${Date.now()}`,
    kanbanNumber,
    partNumber: `PART-${Math.floor(Math.random() * 10000)}`,
    partDescription: 'Sample Part Description',
    quantity: Math.floor(Math.random() * 50) + 10,
    supplierCode: 'SUP-001',
    dockCode: `DOCK-${Math.floor(Math.random() * 10) + 1}`,
  };
}

/**
 * Mock: Generate Internal Kanban
 */
function generateMockInternalKanban(kanbanNumber: string): InternalKanban {
  return {
    id: `ik-${Date.now()}`,
    kanbanNumber,
    partNumber: `PART-${Math.floor(Math.random() * 10000)}`,
    binLocation: `BIN-${String.fromCharCode(65 + Math.floor(Math.random() * 26))}${Math.floor(Math.random() * 100)}`,
    lastScannedAt: null,
  };
}

/**
 * Mock: Generate Driver Check Sheet
 */
function generateMockDriverCheck(checkSheetNumber: string): DriverCheckSheet {
  return {
    id: `dcs-${Date.now()}`,
    checkSheetNumber,
    driverName: 'John Driver',
    driverLicense: `DL-${Math.random().toString(36).substring(2, 10).toUpperCase()}`,
    trailerNumber: `TR-${Math.random().toString(36).substring(2, 6).toUpperCase()}`,
    scannedAt: new Date().toISOString(),
    scannedBy: 'user-001',
  };
}

// ===== ORDER APIs =====

export async function uploadOrder(file: File): Promise<ApiResponse<Order>> {
  // Mock implementation - in production, this would upload to backend
  await new Promise(resolve => setTimeout(resolve, 1000));

  const mockOrder = generateMockOrder(`OWK-${Date.now()}`);

  return {
    success: true,
    data: mockOrder,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function getOrder(orderId: string): Promise<ApiResponse<Order>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 500));

  const mockOrder = generateMockOrder(orderId);

  return {
    success: true,
    data: mockOrder,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function searchOrder(orderNumber: string): Promise<ApiResponse<Order>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 800));

  if (!orderNumber.startsWith('OWK-')) {
    return {
      success: false,
      data: null,
      error: ERROR_MESSAGES.ORDER_NOT_FOUND,
      timestamp: new Date().toISOString(),
    };
  }

  const mockOrder = generateMockOrder(orderNumber);

  return {
    success: true,
    data: mockOrder,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

// ===== SKID BUILD APIs =====

export async function scanSkidManifest(
  toyotaLabel: string,
  orderId: string
): Promise<ApiResponse<{ manifestId: string }>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 600));

  return {
    success: true,
    data: { manifestId: `manifest-${Date.now()}` },
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function scanToyotaKanban(
  kanbanNumber: string,
  sessionId: string
): Promise<ApiResponse<ToyotaKanban>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 500));

  const mockKanban = generateMockToyotaKanban(kanbanNumber);

  return {
    success: true,
    data: mockKanban,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function scanInternalKanban(
  kanbanNumber: string,
  sessionId: string
): Promise<ApiResponse<InternalKanban>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 500));

  const mockKanban = generateMockInternalKanban(kanbanNumber);

  return {
    success: true,
    data: mockKanban,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function scanSerialNumber(
  serialNumber: string,
  kanbanId: string
): Promise<ApiResponse<SerialNumber>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 500));

  const mockSerial: SerialNumber = {
    id: `sn-${Date.now()}`,
    serialNumber,
    partNumber: `PART-${Math.floor(Math.random() * 10000)}`,
    internalKanbanId: kanbanId,
    scannedAt: new Date().toISOString(),
    scannedBy: 'user-001',
    locationId: 'loc-001',
  };

  return {
    success: true,
    data: mockSerial,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function completeSkidBuild(sessionId: string): Promise<ApiResponse<{ success: boolean }>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 1000));

  return {
    success: true,
    data: { success: true },
    error: null,
    timestamp: new Date().toISOString(),
  };
}

// ===== SHIPMENT APIs =====

export async function scanDriverCheckSheet(
  checkSheetNumber: string
): Promise<ApiResponse<DriverCheckSheet>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 600));

  const mockCheck = generateMockDriverCheck(checkSheetNumber);

  return {
    success: true,
    data: mockCheck,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function createShipment(
  trailerInfo: TrailerInfo,
  driverCheckId: string
): Promise<ApiResponse<ShipmentLoad>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 800));

  const mockShipment: ShipmentLoad = {
    id: `shipment-${Date.now()}`,
    shipmentNumber: `SHIP-${Date.now()}`,
    driverCheckSheetId: driverCheckId,
    trailerInfo,
    skidIds: [],
    totalSkids: 0,
    status: 'LOADING',
    loadedBy: 'user-001',
    loadedAt: new Date().toISOString(),
    submittedToToyotaAt: null,
  };

  return {
    success: true,
    data: mockShipment,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function submitToToyota(shipmentId: string): Promise<ApiResponse<{ success: boolean }>> {
  // Mock implementation - simulates Toyota API submission
  await new Promise(resolve => setTimeout(resolve, 2000));

  return {
    success: true,
    data: { success: true },
    error: null,
    timestamp: new Date().toISOString(),
  };
}

// ===== DASHBOARD APIs =====

export async function getDockStatus(location: string): Promise<ApiResponse<DockStatus>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 300));

  const mockStatus: DockStatus = {
    location: location as DockStatus['location'],
    doors: Array.from({ length: 10 }, (_, i) => ({
      dockNumber: `DOCK-${i + 1}`,
      status: ['AVAILABLE', 'OCCUPIED', 'LOADING'][Math.floor(Math.random() * 3)] as 'AVAILABLE' | 'OCCUPIED' | 'LOADING',
      currentShipment: Math.random() > 0.5 ? `SHIP-${Date.now() + i}` : null,
      trailerNumber: Math.random() > 0.5 ? `TR-${Math.random().toString(36).substring(2, 6).toUpperCase()}` : null,
      estimatedCompletion: Math.random() > 0.5 ? new Date(Date.now() + Math.random() * 3600000).toISOString() : null,
      lastUpdated: new Date().toISOString(),
    })),
    lastRefreshed: new Date().toISOString(),
  };

  return {
    success: true,
    data: mockStatus,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function getSupplierRoutes(): Promise<ApiResponse<SupplierRoute[]>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 500));

  const mockRoutes: SupplierRoute[] = Array.from({ length: 5 }, (_, i) => ({
    id: `route-${i + 1}`,
    routeNumber: `RT-${String(i + 1).padStart(3, '0')}`,
    supplierId: `sup-${i + 1}`,
    supplier: {
      id: `sup-${i + 1}`,
      code: `SUP-${String(i + 1).padStart(3, '0')}`,
      name: `Supplier ${i + 1}`,
      contactPerson: `Contact ${i + 1}`,
      phone: '555-0100',
      email: `supplier${i + 1}@example.com`,
    },
    pickupTime: new Date(Date.now() - Math.random() * 7200000).toISOString(),
    expectedArrival: new Date(Date.now() + Math.random() * 7200000).toISOString(),
    status: ['SCHEDULED', 'IN_TRANSIT', 'ARRIVED'][Math.floor(Math.random() * 3)] as 'SCHEDULED' | 'IN_TRANSIT' | 'ARRIVED',
    ordersCount: Math.floor(Math.random() * 10) + 1,
  }));

  return {
    success: true,
    data: mockRoutes,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

export async function getComplianceReport(): Promise<ApiResponse<ComplianceReport>> {
  // Mock implementation
  await new Promise(resolve => setTimeout(resolve, 700));

  const mockReport: ComplianceReport = {
    location: 'VOSC',
    reportDate: new Date().toISOString(),
    metrics: [
      {
        id: 'metric-1',
        metricName: 'Scan Accuracy',
        metricType: 'ACCURACY',
        targetValue: 99.5,
        actualValue: 99.8,
        unit: '%',
        period: 'Daily',
        status: 'PASS',
        lastUpdated: new Date().toISOString(),
      },
      {
        id: 'metric-2',
        metricName: 'On-Time Shipments',
        metricType: 'TIMELINESS',
        targetValue: 95,
        actualValue: 97.2,
        unit: '%',
        period: 'Daily',
        status: 'PASS',
        lastUpdated: new Date().toISOString(),
      },
      {
        id: 'metric-3',
        metricName: 'Quality Check Pass Rate',
        metricType: 'QUALITY',
        targetValue: 98,
        actualValue: 96.5,
        unit: '%',
        period: 'Daily',
        status: 'WARNING',
        lastUpdated: new Date().toISOString(),
      },
    ],
    overallScore: 97.8,
    issues: [],
  };

  return {
    success: true,
    data: mockReport,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

// ===== SHIPMENT LOAD V2 APIs =====
// Author: Hassan, Date: 2025-11-05

export interface PlannedSkid {
  skidId: string;
  partNumber: string;
  description: string;
  quantity: number;
  supplierCode: string;
  dockCode: string;
  status: 'PENDING' | 'LOADED' | 'VERIFIED';
}

export async function getPlannedSkidsForShipment(
  shipmentNumber: string
): Promise<ApiResponse<PlannedSkid[]>> {
  // Mock implementation - in production, fetch from backend
  await new Promise(resolve => setTimeout(resolve, 800));

  // Generate 5 mock planned skids with IDs matching real scan format (last 8 chars like "LB05001A")
  // Author: Hassan, Date: 2025-11-05 - Updated to match real scan format
  const mockPlannedSkids: PlannedSkid[] = [
    {
      skidId: 'LB05001A',
      partNumber: '681010E250',
      description: 'TEM RH WEST',
      quantity: 45,
      supplierCode: '02806',
      dockCode: 'V8',
      status: 'PENDING',
    },
    {
      skidId: 'LB05001B',
      partNumber: '681020F150',
      description: 'TEM LH EAST',
      quantity: 30,
      supplierCode: '02806',
      dockCode: 'V8',
      status: 'PENDING',
    },
    {
      skidId: 'LB05001C',
      partNumber: '692050G200',
      description: 'BRACKET ASSY',
      quantity: 60,
      supplierCode: '02807',
      dockCode: 'V9',
      status: 'PENDING',
    },
    {
      skidId: 'LB05001D',
      partNumber: '681030H175',
      description: 'WIRING HARNESS',
      quantity: 25,
      supplierCode: '02808',
      dockCode: 'V8',
      status: 'PENDING',
    },
    {
      skidId: 'LB05001E',
      partNumber: '681040I225',
      description: 'CONNECTOR ASSY',
      quantity: 50,
      supplierCode: '02809',
      dockCode: 'V10',
      status: 'PENDING',
    },
  ];

  return {
    success: true,
    data: mockPlannedSkids,
    error: null,
    timestamp: new Date().toISOString(),
  };
}

// ===== SKID BUILD V2 APIs =====
// Author: Hassan, Date: 2025-12-06
// Updated: 2025-12-06 - Use apiClient with JWT auth instead of fetch
// Updated: 2025-12-13 - Added ScanDetailDto for detailed scan information

export interface ScanDetailDto {
  skidNumber: string;
  boxNumber: number;
  internalKanban: string | null;
  palletizationCode: string | null;
}

export interface SkidBuildPlannedItem {
  plannedItemId: string;
  partNumber: string;
  kanbanNumber: string;
  qpc: number;
  totalBoxPlanned: number;
  manifestNo: number;
  palletizationCode: string;
  externalOrderId: number;
  scannedCount: number;
  scanDetails: ScanDetailDto[];
  internalKanbanRequired?: boolean; // false if part is excluded, true/undefined otherwise
}

export interface SkidBuildOrder {
  orderId: string;
  orderNumber: string;
  dockCode: string;
  supplierCode: string;
  plantCode: string;
  transmitDate: string;
  status: string;
  plannedItems: SkidBuildPlannedItem[];
}

export interface SkidBuildSession {
  sessionId: string;
  orderId: string;
  skidNumber: number;
  status: string;
  startedAt: string;
  completedAt?: string;
  confirmationNumber?: string;
  totalScans: number;
  totalExceptions: number;
}

export interface SkidBuildCompleteResponse {
  confirmationNumber: string;
  sessionId: string;
  totalScanned: number;
  totalExceptions: number;
  toyotaConfirmationNumber?: string;
  toyotaError?: string;
}

// Get order by number and dock code
export async function getSkidBuildOrder(
  orderNumber: string,
  dockCode: string
): Promise<ApiResponse<SkidBuildOrder>> {
  try {
    const response = await apiClient.get(
      `/api/v1/skid-build/order/${encodeURIComponent(orderNumber)}?dockCode=${encodeURIComponent(dockCode)}`
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to fetch order',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Start a skid build session
export async function startSkidBuildSession(
  orderId: string,
  skidNumber: number,
  userId?: string
): Promise<ApiResponse<SkidBuildSession>> {
  try {
    const response = await apiClient.post('/api/v1/skid-build/session/start', {
      orderId,
      skidNumber,
      userId: userId || 'unknown',
    });

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to start session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Record a scan
// Updated: 2025-12-10 by Hassan - Toyota API Specification V2.0 alignment
// - skidNumber changed from number to string (3 numeric digits)
// - Added skidSide (A or B)
// - Added rawSkidId (original 4-char value for reference)
// - Added palletizationCode
export async function recordSkidBuildScan(
  sessionId: string,
  plannedItemId: string,
  skidNumber: string,        // Changed from number to string - "001" (3 digits)
  skidSide: string,           // NEW - "A" or "B"
  rawSkidId: string,          // NEW - "001B" (original 4 chars)
  boxNumber: number,          // From Toyota Kanban, SEPARATE from skidNumber
  lineSideAddress: string,
  palletizationCode: string,  // NEW - For validation
  internalKanban: string,
  userId?: string
): Promise<ApiResponse<{ scanId: string }>> {
  try {
    const response = await apiClient.post('/api/v1/skid-build/scan', {
      sessionId,
      plannedItemId,
      skidNumber,               // Now string "001"
      skidSide,                 // "A" or "B"
      rawSkidId,                // "001B"
      boxNumber,                // From Toyota Kanban
      lineSideAddress,
      palletizationCode,        // For backend validation
      internalKanban,
      userId: userId || 'unknown',
    });

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to record scan',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Record an exception
export async function recordSkidBuildException(
  sessionId: string,
  orderId: string,
  exceptionCode: string,
  comments: string,
  skidNumber: number,
  userId?: string
): Promise<ApiResponse<{ exceptionId: string }>> {
  try {
    const response = await apiClient.post('/api/v1/skid-build/exception', {
      sessionId,
      orderId,
      exceptionCode,
      comments,
      skidNumber,
      userId: userId || 'unknown',
    });

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to record exception',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Complete session
export async function completeSkidBuildSession(
  sessionId: string,
  userId?: string
): Promise<ApiResponse<SkidBuildCompleteResponse>> {
  try {
    const response = await apiClient.post('/api/v1/skid-build/session/complete', {
      sessionId,
      userId: userId || 'unknown',
    });

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to complete session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Add exception
// Author: Hassan, Date: 2025-12-14
export async function addSkidBuildException(
  sessionId: string,
  orderId: string,
  exceptionCode: string,
  comments: string,
  skidNumber?: number,
  userId?: string
): Promise<ApiResponse<{ exceptionId: string }>> {
  try {
    const response = await apiClient.post('/api/v1/skid-build/exception', {
      sessionId,
      orderId,
      exceptionCode,
      comments,
      skidNumber,
      userId: userId || 'unknown',
    });

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to add exception',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Delete exception
// Author: Hassan, Date: 2025-12-14
export async function deleteSkidBuildException(
  exceptionId: string
): Promise<ApiResponse<boolean>> {
  try {
    const response = await apiClient.delete(`/api/v1/skid-build/exception/${exceptionId}`);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: true,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: false,
      error: result.message || 'Failed to delete exception',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: false,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== SKID BUILD V2 GROUPED API =====
// Author: Hassan, Date: 2025-12-12
// Get order with items grouped by skid (for multi-skid workflow)

export interface SkidGroupDto {
  skidId: string;
  manifestNo: number;
  palletizationCode: string;
  plannedKanbans: SkidBuildPlannedItem[];
}

export interface SkidBuildOrderGrouped {
  orderId: string;
  orderNumber: string;
  dockCode: string;
  supplierCode: string;
  plantCode: string;
  status: string;
  skids: SkidGroupDto[];
  toyotaSkidBuildConfirmationNumber?: string;
  toyotaSkidBuildErrorMessage?: string;
}

export async function getSkidBuildOrderGrouped(
  orderNumber: string,
  dockCode: string
): Promise<ApiResponse<SkidBuildOrderGrouped>> {
  try {
    const response = await apiClient.get(
      `/api/v1/skid-build/order/${encodeURIComponent(orderNumber)}/grouped?dockCode=${encodeURIComponent(dockCode)}`
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to fetch grouped order',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== SHIPMENT LOAD APIs =====
// Author: Hassan, Date: 2025-12-08
// Updated: 2025-12-08 - Use apiClient with JWT auth

export interface ShipmentLoadRouteResponse {
  routeNumber: string;
  totalOrders: number;
  orders: PlannedOrder[];
}

export interface PlannedOrder {
  orderId: string;
  orderNumber: string;
  dockCode: string;
  status: string;
  plantCode: string;
  supplierCode: string;
  mros: number;
  skidCount: number;
  scannedCount: number;
  plannedItems: PlannedOrderItem[];
  isScanned?: boolean;  // Indicates if order was scanned in current session
}

export interface PlannedOrderItem {
  plannedItemId: string;
  partNumber: string;
  kanbanNumber: string;
  palletizationCode: string;
  manifestNo: number;
  totalBoxPlanned: number;
  scannedCount: number;
}

export interface ShipmentLoadScanRequest {
  sessionId: string;       // REQUIRED - Session ID from session/start
  routeNumber: string;
  orderNumber: string;
  dockCode: string;
  palletizationCode: string;
  mros: string;
  skidId: string;
  scannedBy?: string;
}

export interface ShipmentLoadScanResponse {
  orderId: string;
  orderNumber: string;
  status: string;
  isValid: boolean;
  validationDetails: {
    orderFound: boolean;
    skidBuilt: boolean;
    routeMatches: boolean;
    palletizationMatches: boolean;
  };
}

export interface ShipmentLoadCompleteRequest {
  routeNumber: string;
  trailerNumber: string;
  sealNumber: string;
  driverName?: string;
  carrierName?: string;
  notes?: string;
  orderIds: string[];
  completedBy?: string;
}

export interface ShipmentLoadCompleteResponse {
  confirmationNumber: string;
  routeNumber: string;
  trailerNumber: string;
  totalOrdersShipped: number;
  completedAt: string;
  orders: Array<{
    orderId: string;
    orderNumber: string;
    status: string;
  }>;
}

// Get orders by route number
export async function getShipmentLoadRoute(
  routeNumber: string
): Promise<ApiResponse<ShipmentLoadRouteResponse>> {
  try {
    const response = await apiClient.get(
      `/api/v1/shipment-load/route/${encodeURIComponent(routeNumber)}`
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to fetch route orders',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Scan and validate a skid
export async function scanShipmentLoadSkid(
  request: ShipmentLoadScanRequest
): Promise<ApiResponse<ShipmentLoadScanResponse>> {
  try {
    const response = await apiClient.post('/api/v1/shipment-load/scan', request);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to scan skid',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Complete shipment load
export async function completeShipmentLoad(
  request: ShipmentLoadCompleteRequest
): Promise<ApiResponse<ShipmentLoadCompleteResponse>> {
  try {
    const response = await apiClient.post('/api/v1/shipment-load/complete', request);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to complete shipment',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== SHIPMENT LOAD SESSION APIs (New Backend Integration) =====
// Author: Hassan, Date: 2025-12-17
// Session-based workflow matching Toyota API specification

export interface StartSessionRequest {
  routeNumber: string;
  supplierCode: string;
  pickupDateTime: string; // ISO 8601 format
  userId: string;
  orderNumber?: string;  // NEW - Order number from QR
  dockCode?: string;     // NEW - Dock code from QR
}

export interface StartSessionResponse {
  sessionId: string;
  routeNumber: string;
  route: string;  // Route without run (e.g., "YUAN")
  run: string;    // Last 2 chars (e.g., "03")
  status: string;
  orders: PlannedOrder[];
  isResumed: boolean;
  scannedOrderSkidCount?: number;  // NEW - Actual skid count for the scanned order
  // Trailer data fields (populated when session is resumed)
  trailerNumber?: string;
  sealNumber?: string;
  driverFirstName?: string;
  driverLastName?: string;
  supplierFirstName?: string;
  supplierLastName?: string;
}

export interface UpdateSessionRequest {
  sessionId: string;
  trailerNumber: string;      // Required
  sealNumber?: string;         // Optional
  lpCode?: string;             // Optional - Logistics Partner SCAC code
  driverFirstName: string;     // REQUIRED (dropHook=false)
  driverLastName: string;      // REQUIRED (dropHook=false)
  supplierFirstName?: string;  // Optional
  supplierLastName?: string;   // Optional
}

export interface SessionResponse {
  sessionId: string;
  routeNumber: string;
  route: string;
  run: string;
  supplierCode: string;
  pickupDateTime: string;
  trailerNumber?: string;
  sealNumber?: string;
  lpCode?: string;
  driverFirstName?: string;
  driverLastName?: string;
  supplierFirstName?: string;
  supplierLastName?: string;
  status: string;
  orders: PlannedOrder[];
  exceptions: ShipmentException[];
  toyotaConfirmationNumber?: string;
  createdAt: string;
  completedAt?: string;
}

export interface ScanManifestRequest {
  sessionId: string;
  orderNumber: string;
  dockCode: string;
  palletizationCode: string;
  mros: string;
  skidId: string;
}

export interface ScanManifestResponse {
  orderId: string;
  orderNumber: string;
  status: string;
  isValid: boolean;
  message?: string;
}

export interface CompleteSessionRequest {
  sessionId: string;
  userId: string;
}

export interface CompleteSessionResponse {
  confirmationNumber: string;  // Toyota API confirmation
  sessionId: string;
  routeNumber: string;
  trailerNumber: string;
  totalOrdersShipped: number;
  completedAt: string;
}

export interface AddExceptionRequest {
  sessionId: string;
  exceptionCode: string;
  comments: string;
  relatedSkidId?: string;  // NULL for trailer-level, skidId for skid-level
}

export interface ShipmentException {
  exceptionId: string;
  sessionId: string;
  exceptionCode: string;
  comments: string;
  relatedSkidId?: string;
  createdAt: string;
  createdBy: string;
}

// Start or resume shipment load session
export async function startShipmentLoadSession(
  request: StartSessionRequest
): Promise<ApiResponse<StartSessionResponse>> {
  try {
    const response = await apiClient.post('/api/v1/shipment-load/session/start', request);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    // Build error message including details from errors array
    let errorMessage = result.message || 'Failed to start session';
    if (result.errors && result.errors.length > 0) {
      // Use errors array which contains detailed info like order numbers
      errorMessage = result.errors.join(' ');
    }

    console.log('[startShipmentLoadSession] Error response:', { message: result.message, errors: result.errors, errorMessage });

    return {
      success: false,
      data: null,
      error: errorMessage,
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Update session with trailer information
export async function updateShipmentLoadSession(
  request: UpdateSessionRequest
): Promise<ApiResponse<SessionResponse>> {
  try {
    const { sessionId, ...payload } = request;
    const response = await apiClient.put(
      `/api/v1/shipment-load/session/${sessionId}`,
      payload
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to update session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Get session details
export async function getShipmentLoadSession(
  sessionId: string
): Promise<ApiResponse<SessionResponse>> {
  try {
    const response = await apiClient.get(
      `/api/v1/shipment-load/session/${sessionId}`
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to get session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Scan manifest and link order to session
export async function scanShipmentLoadManifest(
  request: ScanManifestRequest
): Promise<ApiResponse<ScanManifestResponse>> {
  try {
    const response = await apiClient.post('/api/v1/shipment-load/scan', request);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to scan manifest',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Complete shipment load session (submits to Toyota API)
export async function completeShipmentLoadSession(
  request: CompleteSessionRequest
): Promise<ApiResponse<CompleteSessionResponse>> {
  try {
    const response = await apiClient.post('/api/v1/shipment-load/complete', request);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to complete session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Add exception to session
export async function addShipmentLoadException(
  request: AddExceptionRequest
): Promise<ApiResponse<ShipmentException>> {
  try {
    const response = await apiClient.post('/api/v1/shipment-load/exception', request);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to add exception',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Delete exception from session
export async function deleteShipmentLoadException(
  exceptionId: string
): Promise<ApiResponse<boolean>> {
  try {
    const response = await apiClient.delete(
      `/api/v1/shipment-load/exception/${exceptionId}`
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: true,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: false,
      error: result.message || 'Failed to delete exception',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: false,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== SHIPMENT LOAD VALIDATE ORDER API =====
// Author: Hassan, Date: 2025-12-17
// Validate order without starting session (get skid count)

// Validate Order Response (for shipment load)
export interface ValidateOrderResponse {
  success: boolean;
  orderId: string;
  orderNumber: string;
  dockCode: string;
  plantCode: string;
  supplierCode: string;
  status: string;
  skidBuildComplete: boolean;
  skidCount: number;
  toyotaConfirmationNumber?: string;
  toyotaShipmentConfirmationNumber?: string;
}

// Validate order for shipment load (get skid count without starting session)
export async function validateShipmentLoadOrder(
  orderNumber: string,
  dockCode: string
): Promise<ApiResponse<ValidateOrderResponse>> {
  try {
    const response = await apiClient.get(
      `/api/v1/shipment-load/validate-order?orderNumber=${encodeURIComponent(orderNumber)}&dockCode=${encodeURIComponent(dockCode)}`
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to validate order',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== ORDER SKIDS API =====
// Author: Hassan, Date: 2025-12-17
// Get built skids for an order from tblSkidScans

export interface SkidDto {
  skidId: string;           // Combined: "001A"
  skidNumber: string;       // "001"
  skidSide: string | null;  // "A" or "B"
  palletizationCode: string | null;
  scannedAt: string | null;
}

export interface OrderSkidsResponse {
  orderNumber: string;
  dockCode: string;
  orderId: string;
  skids: SkidDto[];
  totalSkids: number;
}

export async function getOrderSkids(
  orderNumber: string,
  dockCode: string
): Promise<ApiResponse<OrderSkidsResponse>> {
  try {
    const response = await apiClient.get(
      `/api/v1/orders/${encodeURIComponent(orderNumber)}/skids?dockCode=${encodeURIComponent(dockCode)}`
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to fetch order skids',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== PRE-SHIPMENT SCAN APIs =====
// Author: Hassan, Date: 2025-12-31
// Pre-Shipment scanning workflow APIs

export interface PreShipmentOrder {
  orderNumber: string;
  dockCode: string;
  status: string;
  skidCount: number;
  scannedSkidCount: number;
}

export interface PreShipmentPlannedSkid {
  skidId: string;
  orderNumber: string;
  dockCode: string;
  palletizationCode: string;
  skidNumber: string;
  skidSide: string;
  partCount: number;
  isScanned: boolean;
}

export interface PreShipmentSessionResponse {
  sessionId: string;
  routeNumber: string;
  supplierCode: string;
  dockCode: string;
  isResumed: boolean;
  orders: PreShipmentOrder[];
  plannedSkids: PreShipmentPlannedSkid[];
}

export interface PreShipmentListItem {
  sessionId: string;
  routeNumber: string;
  supplierCode: string;
  status: string;
  totalSkidCount: number;
  scannedSkidCount: number;
  createdAt: string;
  trailerNumber: string | null;
}

export interface PreShipmentScanSkidRequest {
  sessionId: string;
  skidId: string;
  palletizationCode: string;
  orderNumber: string;
  dockCode: string;
  scannedBy: string;
}

export interface PreShipmentTrailerInfoRequest {
  sessionId: string;
  trailerNumber: string;
  sealNumber?: string;
  driverFirstName: string;
  driverLastName: string;
  supplierFirstName?: string;
  supplierLastName?: string;
}

export interface PreShipmentCompleteRequest {
  sessionId: string;
  completedBy: string;
}

export interface PreShipmentCompleteResponse {
  confirmationNumber: string;
  sessionId: string;
  routeNumber: string;
  totalSkidsShipped: number;
  completedAt: string;
}

// Create Pre-Shipment session from manifest scan
export async function createPreShipmentFromManifest(
  manifestBarcode: string,
  scannedBy: string
): Promise<ApiResponse<PreShipmentSessionResponse>> {
  try {
    const response = await apiClient.post('/api/v1/pre-shipment/create-from-manifest', {
      manifestBarcode,
      scannedBy,
    });

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to create Pre-Shipment session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// List all Pre-Shipment sessions
export async function getPreShipmentList(): Promise<ApiResponse<PreShipmentListItem[]>> {
  try {
    const response = await apiClient.get('/api/v1/pre-shipment/list');

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to fetch Pre-Shipment list',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Get Pre-Shipment session details
export async function getPreShipmentSession(
  sessionId: string
): Promise<ApiResponse<PreShipmentSessionResponse>> {
  try {
    const response = await apiClient.get(`/api/v1/pre-shipment/${sessionId}`);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to fetch Pre-Shipment session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Scan individual skid
export async function scanPreShipmentSkid(
  request: PreShipmentScanSkidRequest
): Promise<ApiResponse<{ success: boolean }>> {
  try {
    const { sessionId, ...payload } = request;
    const response = await apiClient.post(
      `/api/v1/pre-shipment/${sessionId}/scan-skid`,
      payload
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to scan skid',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Update trailer and driver info
export async function updatePreShipmentTrailerInfo(
  request: PreShipmentTrailerInfoRequest
): Promise<ApiResponse<{ success: boolean }>> {
  try {
    const { sessionId, ...payload } = request;
    const response = await apiClient.put(
      `/api/v1/pre-shipment/${sessionId}/trailer-info`,
      payload
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to update trailer info',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Complete and submit Pre-Shipment to Toyota
export async function completePreShipment(
  request: PreShipmentCompleteRequest
): Promise<ApiResponse<PreShipmentCompleteResponse>> {
  try {
    const { sessionId, ...payload } = request;
    const response = await apiClient.post(
      `/api/v1/pre-shipment/${sessionId}/complete`,
      payload
    );

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to complete Pre-Shipment',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Delete incomplete Pre-Shipment session
export async function deletePreShipment(
  sessionId: string
): Promise<ApiResponse<boolean>> {
  try {
    const response = await apiClient.delete(`/api/v1/pre-shipment/${sessionId}`);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: true,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: false,
      error: result.message || 'Failed to delete Pre-Shipment session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: false,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// ===== RESTART SESSION APIs =====
// Author: Hassan, Date: 2026-01-04
// Restart APIs for Skid Build and Shipment Load sessions

export interface RestartSessionResponse {
  success: boolean;
  message: string;
  newSessionId: string | null;
}

// Restart Skid Build session
export async function restartSkidBuildSession(
  sessionId: string
): Promise<ApiResponse<RestartSessionResponse>> {
  try {
    const response = await apiClient.post(`/api/v1/skid-build/session/${sessionId}/restart`);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to restart session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}

// Restart Shipment Load session
export async function restartShipmentLoadSession(
  sessionId: string
): Promise<ApiResponse<RestartSessionResponse>> {
  try {
    const response = await apiClient.post(`/api/v1/shipment-load/session/${sessionId}/restart`);

    const result = response.data;

    if (result.success) {
      return {
        success: true,
        data: result.data,
        error: null,
        timestamp: new Date().toISOString(),
      };
    }

    return {
      success: false,
      data: null,
      error: result.message || 'Failed to restart session',
      timestamp: new Date().toISOString(),
    };
  } catch (error) {
    return {
      success: false,
      data: null,
      error: getErrorMessage(error),
      timestamp: new Date().toISOString(),
    };
  }
}
