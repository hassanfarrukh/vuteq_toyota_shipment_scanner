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
  SkidBuildSession,
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
    location: 'INDIANA',
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
