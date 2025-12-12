/**
 * TypeScript Type Definitions
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-11-24 - Added backend API types for Office, Warehouse, and User management
 * All type definitions for the VUTEQ Scanner Application
 */

// ===== LOCATION TYPES =====
export type LocationType = 'INDIANA' | 'MICHIGAN' | 'OHIO' | 'KENTUCKY' | 'TENNESSEE' | 'ALABAMA';

export interface Location {
  id: string;
  name: LocationType;
  requiresSerialScanning: boolean;
  address: string;
  timezone: string;
}

// ===== ADMINISTRATION TYPES (Backend API) =====
// These types match the backend DTOs for administration functionality

export interface AdminOffice {
  officeId: string; // GUID
  code: string;
  name: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  phone?: string;
  contact?: string;
  email?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface AdminWarehouse {
  warehouseId: string; // GUID
  code: string;
  name: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  office: string; // Office code (FK)
  phone?: string;
  contactName?: string;
  contactEmail?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface AdminUser {
  userId: string; // String identifier
  username: string;
  name: string; // Backend uses "name" not "userName"
  email?: string;
  role: string;
  menuLevel?: string;
  operation?: string;
  code?: string;
  isSupervisor: boolean;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt?: string;
}

// ===== USER & AUTH TYPES =====
export interface User {
  id: string;
  username: string;
  email: string;
  role: 'ADMIN' | 'SUPERVISOR' | 'OPERATOR';
  locationId: string;
  createdAt: string;
}

export interface AuthToken {
  token: string;
  expiresAt: string;
  user: User;
}

// ===== ORDER TYPES =====
export interface Order {
  id: string;
  owkOrderNumber: string;
  customerName: string;
  destination: string;
  totalSkids: number;
  completedSkids: number;
  status: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED' | 'SHIPPED';
  deliveryDate: string;
  createdAt: string;
  updatedAt: string;
}

// ===== SKID TYPES =====
export interface SkidManifest {
  id: string;
  toyotaLabelNumber: string;
  orderId: string;
  skidNumber: number;
  totalParts: number;
  scannedParts: number;
  status: 'PENDING' | 'IN_PROGRESS' | 'COMPLETED';
  createdAt: string;
  updatedAt: string;
}

export interface ToyotaKanban {
  id: string;
  kanbanNumber: string;
  partNumber: string;
  partDescription: string;
  quantity: number;
  supplierCode: string;
  dockCode: string;
}

export interface InternalKanban {
  id: string;
  kanbanNumber: string; // 7-13 characters, reusable plastic card
  partNumber: string;
  binLocation: string;
  lastScannedAt: string | null;
}

export interface SerialNumber {
  id: string;
  serialNumber: string;
  partNumber: string;
  internalKanbanId: string;
  scannedAt: string;
  scannedBy: string;
  locationId: string;
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

// ===== SHIPMENT TYPES =====
export interface DriverCheckSheet {
  id: string;
  checkSheetNumber: string;
  driverName: string;
  driverLicense: string;
  trailerNumber: string;
  scannedAt: string;
  scannedBy: string;
}

export interface TrailerInfo {
  trailerNumber: string;
  sealNumber: string;
  carrierName: string;
  driverName: string;
  driverLicense: string;
  expectedDeparture: string;
}

export interface ShipmentLoad {
  id: string;
  shipmentNumber: string;
  driverCheckSheetId: string;
  trailerInfo: TrailerInfo;
  skidIds: string[];
  totalSkids: number;
  status: 'PENDING' | 'LOADING' | 'COMPLETED' | 'SHIPPED';
  loadedBy: string;
  loadedAt: string;
  submittedToToyotaAt: string | null;
}

// ===== DOCK TYPES =====
export interface DockDoor {
  dockNumber: string;
  status: 'AVAILABLE' | 'OCCUPIED' | 'LOADING' | 'UNLOADING' | 'BLOCKED';
  currentShipment: string | null;
  trailerNumber: string | null;
  estimatedCompletion: string | null;
  lastUpdated: string;
}

export interface DockStatus {
  location: LocationType;
  doors: DockDoor[];
  lastRefreshed: string;
}

// ===== SUPPLIER TYPES =====
export interface Supplier {
  id: string;
  code: string;
  name: string;
  contactPerson: string;
  phone: string;
  email: string;
}

export interface SupplierRoute {
  id: string;
  routeNumber: string;
  supplierId: string;
  supplier: Supplier;
  pickupTime: string;
  expectedArrival: string;
  status: 'SCHEDULED' | 'IN_TRANSIT' | 'ARRIVED' | 'DELAYED';
  ordersCount: number;
}

// ===== COMPLIANCE TYPES =====
export interface ComplianceMetric {
  id: string;
  metricName: string;
  metricType: 'ACCURACY' | 'TIMELINESS' | 'QUALITY' | 'SAFETY';
  targetValue: number;
  actualValue: number;
  unit: string;
  period: string;
  status: 'PASS' | 'FAIL' | 'WARNING';
  lastUpdated: string;
}

export interface ComplianceReport {
  location: LocationType;
  reportDate: string;
  metrics: ComplianceMetric[];
  overallScore: number;
  issues: ComplianceIssue[];
}

export interface ComplianceIssue {
  id: string;
  severity: 'CRITICAL' | 'HIGH' | 'MEDIUM' | 'LOW';
  category: string;
  description: string;
  detectedAt: string;
  resolvedAt: string | null;
  assignedTo: string | null;
}

// ===== API TYPES =====
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

export interface ApiError {
  code: string;
  message: string;
  details?: Record<string, unknown>;
}

export interface PaginatedResponse<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
  hasMore: boolean;
}

// ===== SCAN TYPES =====
export interface ScanResult {
  success: boolean;
  scannedValue: string;
  validatedType: 'ORDER' | 'SKID_MANIFEST' | 'TOYOTA_KANBAN' | 'INTERNAL_KANBAN' | 'SERIAL' | 'DRIVER_CHECK' | 'UNKNOWN';
  data: unknown;
  error: string | null;
  timestamp: string;
}

export interface ScanValidation {
  isValid: boolean;
  errorMessage: string | null;
  isDuplicate: boolean;
  lastScannedAt: string | null;
}

// ===== DASHBOARD TYPES =====
export interface DashboardTile {
  id: string;
  title: string;
  icon: string;
  route: string;
  badge?: number;
  disabled?: boolean;
  requiresRole?: 'ADMIN' | 'SUPERVISOR' | 'OPERATOR'; // Role-based visibility
}

// ===== BUSINESS RULES =====
export interface BusinessRule {
  code: string;
  description: string;
  isActive: boolean;
  validationFn?: (data: unknown) => boolean;
}

// Business Rules Constants
export const BUSINESS_RULES = {
  BR_001: 'Driver Check Sheet MUST be scanned FIRST before loading',
  BR_002: 'Skid manifest must link to valid OWK order',
  BR_003: 'Internal Kanbans are 7-13 character reusable plastic cards',
  BR_004: 'Serial number scanning required at Indiana location',
  BR_005: 'Dock Monitor refresh every 10 seconds',
  BR_006: 'Prevent duplicate scans within 24 hours',
} as const;

export type BusinessRuleCode = keyof typeof BUSINESS_RULES;
