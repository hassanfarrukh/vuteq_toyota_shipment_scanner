/**
 * Application Constants
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-29 - Added "Operator Pre Shipment Scan" dashboard tile with forklift icon (Hassan)
 * Updated: 2025-11-05 - Added "Upload Order Data" dashboard tile (tile-007) for SUPERVISOR/ADMIN users (Hassan)
 * Updated: 2025-11-05 - Changed "Upload Order Data" icon from 'upload' to 'file-arrow-up' (Hassan)
 * Updated: 2025-12-01 - Changed "Upload Order Data" to "Order Data" with clipboard-list icon (Hassan)
 * Centralized configuration and constants
 */

import type { Location, DashboardTile } from '@/types';

// ===== LOCATION CONFIGURATION =====
export const LOCATIONS: Location[] = [
  {
    id: 'loc-001',
    name: 'INDIANA',
    requiresSerialScanning: true,
    address: 'Indiana Facility',
    timezone: 'America/Indiana/Indianapolis',
  },
  {
    id: 'loc-002',
    name: 'MICHIGAN',
    requiresSerialScanning: false,
    address: 'Michigan Facility',
    timezone: 'America/Detroit',
  },
  {
    id: 'loc-003',
    name: 'OHIO',
    requiresSerialScanning: false,
    address: 'Ohio Facility',
    timezone: 'America/New_York',
  },
  {
    id: 'loc-004',
    name: 'KENTUCKY',
    requiresSerialScanning: false,
    address: 'Kentucky Facility',
    timezone: 'America/Kentucky/Louisville',
  },
  {
    id: 'loc-005',
    name: 'TENNESSEE',
    requiresSerialScanning: false,
    address: 'Tennessee Facility',
    timezone: 'America/Chicago',
  },
  {
    id: 'loc-006',
    name: 'ALABAMA',
    requiresSerialScanning: false,
    address: 'Alabama Facility',
    timezone: 'America/Chicago',
  },
];

// ===== DASHBOARD TILES =====
// Author: Hassan
// Date: 2025-10-22
// Role-based access control: OPERATOR (Skid Build, Shipment Load), SUPERVISOR (dashboards, monitor), ADMIN (all + settings)
export const DASHBOARD_TILES: DashboardTile[] = [
  {
    id: 'tile-001',
    title: 'Skid Build',
    icon: 'box',
    route: '/skid-build',
    requiresRole: 'OPERATOR', // Accessible by OPERATOR, SUPERVISOR, ADMIN
  },
  {
    id: 'tile-002',
    title: 'Shipment Load',
    icon: 'truck',
    route: '/shipment-load',
    requiresRole: 'OPERATOR', // Accessible by OPERATOR, SUPERVISOR, ADMIN
  },
  {
    id: 'tile-004',
    title: 'Pre-shipment Scan',
    icon: 'forklift',
    route: '/pre-shipment-scan',
    requiresRole: 'OPERATOR', // Accessible by OPERATOR, SUPERVISOR, ADMIN
  },
  {
    id: 'tile-003',
    title: 'Download Scanned Data',
    icon: 'download',
    route: '/download-scanned-data',
    requiresRole: 'SUPERVISOR', // Accessible by SUPERVISOR, ADMIN only
  },
  {
    id: 'tile-007',
    title: 'Order Data',
    icon: 'clipboard-list',
    route: '/orders',
    requiresRole: 'SUPERVISOR', // Accessible by SUPERVISOR, ADMIN only
  },
  {
    id: 'tile-005',
    title: 'Dock Monitor',
    icon: 'monitor',
    route: '/dock-monitor',
    requiresRole: 'SUPERVISOR', // Accessible by SUPERVISOR, ADMIN only
  },
  {
    id: 'tile-006',
    title: 'Administration',
    icon: 'gear',
    route: '/administration',
    requiresRole: 'ADMIN', // Accessible by ADMIN only
  },
  // Hidden - kept for reference (Hassan, 2025-10-28)
  // {
  //   id: 'tile-007',
  //   title: "Scott's Administration",
  //   icon: 'user-shield',
  //   route: '/scott-admin',
  //   requiresRole: 'ADMIN', // Accessible by ADMIN only
  // },
];

// ===== TIMING CONFIGURATION =====
export const TIMING = {
  DOCK_REFRESH_INTERVAL: 300000, // 5 minutes (300 seconds) - Updated 2025-10-21
  API_TIMEOUT: 5000, // 5 seconds max response time
  SCAN_DEBOUNCE: 300, // Debounce scan input
  DUPLICATE_SCAN_WINDOW: 24 * 60 * 60 * 1000, // 24 hours in ms (BR-006)
} as const;

// ===== VALIDATION PATTERNS =====
// Updated: 2025-12-06 - Fixed TOYOTA_KANBAN regex to accept hyphens and parentheses (Hassan)
export const VALIDATION_PATTERNS = {
  OWK_ORDER: /^OWK-\d{6,10}$/i,
  TOYOTA_LABEL: /^TL-\d{8}$/i,
  PICKUP_ROUTE: /^[A-Z0-9]{8}\d{8}[A-Z0-9\s]{34}$/, // 50-char Pickup Route format: "02TMI02806V82023080205  IDVV01      LB05001A"
  TOYOTA_KANBAN: /^[A-Z0-9\s\-()]+$/i, // Accepts alphanumeric, spaces, hyphens, and parentheses
  INTERNAL_KANBAN: /^[A-Z0-9]{7,13}$/i, // BR-003
  SERIAL_NUMBER: /^SN-[A-Z0-9]{8,16}$/i,
  DRIVER_CHECK_SHEET: /^DCS-\d{6}$/i,
  TRAILER_NUMBER: /^TR-[A-Z0-9]{4,8}$/i,
} as const;

// ===== API ENDPOINTS =====
export const API_ENDPOINTS = {
  // Orders
  UPLOAD_ORDER: '/api/v1/orders/upload',
  GET_ORDER: (orderId: string) => `/api/v1/orders/${orderId}`,
  LIST_ORDERS: '/api/v1/orders',

  // Skid Build
  SCAN_SKID: '/api/v1/skid-build/scan',
  GET_SKID_SESSION: (sessionId: string) => `/api/v1/skid-build/session/${sessionId}`,
  COMPLETE_SKID: '/api/v1/skid-build/complete',

  // Shipment
  SCAN_DRIVER_CHECK: '/api/v1/shipment/driver-check',
  CREATE_SHIPMENT: '/api/v1/shipment/load',
  SUBMIT_TO_TOYOTA: '/api/v1/shipment/submit-toyota',

  // Dashboard
  DOCK_STATUS: '/api/v1/dashboard/dock-status',
  SUPPLIER_ROUTES: '/api/v1/dashboard/supplier-routes',

  // Reports
  COMPLIANCE_REPORT: '/api/v1/reports/compliance',
  PERFORMANCE_METRICS: '/api/v1/reports/performance',
} as const;

// ===== ERROR MESSAGES =====
export const ERROR_MESSAGES = {
  // BR-001
  DRIVER_CHECK_REQUIRED: 'Driver Check Sheet must be scanned first before loading',

  // BR-006
  DUPLICATE_SCAN: 'This item was already scanned within the last 24 hours',

  // General
  INVALID_SCAN: 'Invalid scan format. Please try again.',
  NETWORK_ERROR: 'Network error. Please check connection and retry.',
  TIMEOUT_ERROR: 'Request timeout. Operation took longer than 5 seconds.',
  SERVER_ERROR: 'Server error. Please contact support.',
  ORDER_NOT_FOUND: 'Order not found. Please verify the order number.',
  SKID_NOT_FOUND: 'Skid manifest not found.',
  INVALID_STEP: 'Cannot perform this action at current step.',
  // BR-004
  SERIAL_REQUIRED: 'Serial number scanning is required at this location',
} as const;

// ===== SUCCESS MESSAGES =====
export const SUCCESS_MESSAGES = {
  ORDER_SCANNED: 'Order successfully linked',
  SKID_SCANNED: 'Skid manifest scanned successfully',
  KANBAN_SCANNED: 'Kanban scanned successfully',
  SERIAL_SCANNED: 'Serial number recorded',
  DRIVER_CHECK_COMPLETE: 'Driver check sheet verified',
  SHIPMENT_LOADED: 'Shipment loaded successfully',
  SHIPMENT_SUBMITTED: 'Shipment submitted to Toyota API',
} as const;

// ===== MOBILE SCANNER CONFIGURATION =====
export const SCANNER_CONFIG = {
  SUPPORTED_DEVICES: ['TC51', 'TC52', 'TC70', 'TC72'],
  SCAN_MODE: 'continuous',
  BEEP_ON_SCAN: true,
  VIBRATE_ON_SCAN: true,
  AUTO_FOCUS: true,
} as const;

// ===== PRIORITY COLORS =====
export const PRIORITY_COLORS = {
  CRITICAL: 'bg-error-500 text-white',
  HIGH: 'bg-warning-500 text-white',
  MEDIUM: 'bg-primary-500 text-white',
  LOW: 'bg-gray-400 text-white',
} as const;

// ===== STATUS COLORS =====
export const STATUS_COLORS = {
  PENDING: 'bg-gray-200 text-gray-800',
  IN_PROGRESS: 'bg-primary-100 text-primary-800',
  COMPLETED: 'bg-success-100 text-success-800',
  SHIPPED: 'bg-success-500 text-white',
  DELAYED: 'bg-error-100 text-error-800',
  BLOCKED: 'bg-error-500 text-white',
} as const;
