/**
 * Dummy Dock Monitor Data
 * Author: Hassan
 * Date: 2025-10-21
 * Phase 1: Dummy data showing all status colors and features
 */

export type DockOrderStatus =
  | 'COMPLETED'      // Green
  | 'ON_TIME'        // Light Blue
  | 'BEHIND'         // Orange
  | 'CRITICAL'       // Red
  | 'PROJECT_SHORT'  // Yellow
  | 'SHORT_SHIPPED'; // Purple/Pink

export interface DockOrderData {
  orderNumber: string;
  route: string;
  destination: string;
  supplier: string;
  plannedSkidBuild: string; // Time format
  completedSkidBuild: string | null; // Time format or null if not completed
  plannedShipmentLoad: string; // Time format
  completedShipmentLoad: string | null; // Time format or null if not completed
  status: DockOrderStatus;
  isSupplementOrder: boolean; // If true, display with EX- prefix
}

// Helper to generate time strings
const getTime = (hoursAgo: number, minutesAgo: number = 0): string => {
  const date = new Date();
  date.setHours(date.getHours() - hoursAgo);
  date.setMinutes(date.getMinutes() - minutesAgo);
  return date.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false });
};

const getDate = (daysAgo: number = 0): string => {
  const date = new Date();
  date.setDate(date.getDate() - daysAgo);
  return date.toLocaleDateString('en-US', { month: '2-digit', day: '2-digit', year: 'numeric' });
};

/**
 * Dummy data demonstrating all status colors and features
 * - Shows 1.5 days of information
 * - Includes supplement orders with EX- prefix
 * - Includes unfinished/short-shipped items
 */
export const DUMMY_DOCK_ORDERS: DockOrderData[] = [
  // COMPLETED (Green)
  {
    orderNumber: 'OWK-123456',
    route: 'R-001',
    destination: 'Georgetown, KY',
    supplier: 'ABC Manufacturing',
    plannedSkidBuild: getTime(4, 30),
    completedSkidBuild: getTime(4, 15),
    plannedShipmentLoad: getTime(3, 0),
    completedShipmentLoad: getTime(2, 45),
    status: 'COMPLETED',
    isSupplementOrder: false,
  },
  {
    orderNumber: 'OWK-123457',
    route: 'R-002',
    destination: 'Princeton, IN',
    supplier: 'XYZ Parts Co',
    plannedSkidBuild: getTime(6, 0),
    completedSkidBuild: getTime(5, 50),
    plannedShipmentLoad: getTime(4, 30),
    completedShipmentLoad: getTime(4, 20),
    status: 'COMPLETED',
    isSupplementOrder: false,
  },

  // ON TIME (Light Blue)
  {
    orderNumber: 'OWK-123458',
    route: 'R-003',
    destination: 'Buffalo, WV',
    supplier: 'Delta Components',
    plannedSkidBuild: getTime(2, 0),
    completedSkidBuild: getTime(1, 55),
    plannedShipmentLoad: getTime(1, 0),
    completedShipmentLoad: null, // In progress
    status: 'ON_TIME',
    isSupplementOrder: false,
  },
  {
    orderNumber: 'OWK-123459',
    route: 'R-004',
    destination: 'Blue Springs, MS',
    supplier: 'Gamma Industries',
    plannedSkidBuild: getTime(1, 30),
    completedSkidBuild: getTime(1, 25),
    plannedShipmentLoad: getTime(0, 45),
    completedShipmentLoad: null,
    status: 'ON_TIME',
    isSupplementOrder: false,
  },

  // BEHIND (Orange)
  {
    orderNumber: 'OWK-123460',
    route: 'R-005',
    destination: 'Huntsville, AL',
    supplier: 'Epsilon Parts',
    plannedSkidBuild: getTime(3, 0),
    completedSkidBuild: getTime(2, 30), // 30 min behind
    plannedShipmentLoad: getTime(1, 30),
    completedShipmentLoad: null,
    status: 'BEHIND',
    isSupplementOrder: false,
  },
  {
    orderNumber: 'OWK-123461',
    route: 'R-006',
    destination: 'Georgetown, KY',
    supplier: 'Theta Manufacturing',
    plannedSkidBuild: getTime(5, 0),
    completedSkidBuild: getTime(4, 30),
    plannedShipmentLoad: getTime(3, 30),
    completedShipmentLoad: null, // 20 min behind
    status: 'BEHIND',
    isSupplementOrder: false,
  },

  // CRITICAL (Red)
  {
    orderNumber: 'OWK-123462',
    route: 'R-007',
    destination: 'Princeton, IN',
    supplier: 'Sigma Logistics',
    plannedSkidBuild: getTime(7, 0),
    completedSkidBuild: getTime(5, 30), // 1.5 hours behind
    plannedShipmentLoad: getTime(5, 0),
    completedShipmentLoad: null,
    status: 'CRITICAL',
    isSupplementOrder: false,
  },
  {
    orderNumber: 'OWK-123463',
    route: 'R-008',
    destination: 'Buffalo, WV',
    supplier: 'Omega Parts',
    plannedSkidBuild: getTime(8, 30),
    completedSkidBuild: null, // Still not completed
    plannedShipmentLoad: getTime(6, 30),
    completedShipmentLoad: null,
    status: 'CRITICAL',
    isSupplementOrder: false,
  },

  // PROJECT SHORT SHIP (Yellow)
  {
    orderNumber: 'OWK-123464',
    route: 'R-009',
    destination: 'Blue Springs, MS',
    supplier: 'Kappa Industries',
    plannedSkidBuild: getTime(10, 0),
    completedSkidBuild: getTime(9, 45),
    plannedShipmentLoad: getTime(8, 30),
    completedShipmentLoad: getTime(8, 15), // Completed but short
    status: 'PROJECT_SHORT',
    isSupplementOrder: false,
  },

  // SHORT SHIPPED (Purple/Pink)
  {
    orderNumber: 'OWK-123465',
    route: 'R-010',
    destination: 'Huntsville, AL',
    supplier: 'Lambda Components',
    plannedSkidBuild: getTime(12, 0),
    completedSkidBuild: getTime(11, 50),
    plannedShipmentLoad: getTime(10, 30),
    completedShipmentLoad: getTime(10, 15),
    status: 'SHORT_SHIPPED',
    isSupplementOrder: false,
  },

  // SUPPLEMENT ORDERS with EX- prefix
  {
    orderNumber: 'IDTT-01', // Will display as EX-IDTT-01
    route: 'R-011',
    destination: 'Georgetown, KY',
    supplier: 'ABC Manufacturing',
    plannedSkidBuild: getTime(1, 0),
    completedSkidBuild: getTime(0, 55),
    plannedShipmentLoad: getTime(0, 30),
    completedShipmentLoad: null,
    status: 'ON_TIME',
    isSupplementOrder: true,
  },
  {
    orderNumber: 'IDTT-02', // Will display as EX-IDTT-02
    route: 'R-012',
    destination: 'Princeton, IN',
    supplier: 'XYZ Parts Co',
    plannedSkidBuild: getTime(2, 30),
    completedSkidBuild: getTime(2, 15),
    plannedShipmentLoad: getTime(1, 30),
    completedShipmentLoad: null,
    status: 'BEHIND',
    isSupplementOrder: true,
  },

  // Older orders (showing 1.5 days window)
  {
    orderNumber: 'OWK-123450',
    route: 'R-013',
    destination: 'Buffalo, WV',
    supplier: 'Mu Manufacturing',
    plannedSkidBuild: getTime(28, 0), // Over 1 day old
    completedSkidBuild: getTime(27, 45),
    plannedShipmentLoad: getTime(26, 0),
    completedShipmentLoad: getTime(25, 50),
    status: 'COMPLETED',
    isSupplementOrder: false,
  },
  {
    orderNumber: 'OWK-123451',
    route: 'R-014',
    destination: 'Blue Springs, MS',
    supplier: 'Nu Components',
    plannedSkidBuild: getTime(30, 0), // ~1.25 days old
    completedSkidBuild: getTime(29, 30),
    plannedShipmentLoad: getTime(28, 0),
    completedShipmentLoad: null, // Still not shipped - kept until caught up
    status: 'CRITICAL',
    isSupplementOrder: false,
  },
  {
    orderNumber: 'OWK-123452',
    route: 'R-015',
    destination: 'Huntsville, AL',
    supplier: 'Xi Industries',
    plannedSkidBuild: getTime(32, 0), // ~1.3 days old
    completedSkidBuild: getTime(31, 45),
    plannedShipmentLoad: getTime(30, 30),
    completedShipmentLoad: getTime(30, 0),
    status: 'SHORT_SHIPPED', // Kept until caught up
    isSupplementOrder: false,
  },
];

/**
 * Get display order number (adds EX- prefix for supplement orders)
 */
export const getDisplayOrderNumber = (order: DockOrderData): string => {
  return order.isSupplementOrder ? `EX-${order.orderNumber}` : order.orderNumber;
};

/**
 * Get status color class for table rows
 * Updated: 2025-10-21 by Hassan - Fixed to match legend colors exactly
 * Row backgrounds now use same color intensity as legend for visual consistency
 */
export const getStatusColorClass = (status: DockOrderStatus): string => {
  switch (status) {
    case 'COMPLETED':
      return 'bg-green-500/30 border-l-4 border-green-500';
    case 'ON_TIME':
      return 'bg-blue-400/30 border-l-4 border-blue-400';
    case 'BEHIND':
      return 'bg-orange-500/30 border-l-4 border-orange-500';
    case 'CRITICAL':
      return 'bg-red-500/30 border-l-4 border-red-500';
    case 'PROJECT_SHORT':
      return 'bg-yellow-500/30 border-l-4 border-yellow-500';
    case 'SHORT_SHIPPED':
      return 'bg-purple-500/30 border-l-4 border-purple-500';
    default:
      return 'bg-gray-100';
  }
};

/**
 * Get status badge display text
 */
export const getStatusText = (status: DockOrderStatus): string => {
  switch (status) {
    case 'COMPLETED':
      return 'COMPLETED';
    case 'ON_TIME':
      return 'ON TIME';
    case 'BEHIND':
      return 'BEHIND';
    case 'CRITICAL':
      return 'CRITICAL';
    case 'PROJECT_SHORT':
      return 'PROJECT SHORT SHIP';
    case 'SHORT_SHIPPED':
      return 'SHORT SHIPPED';
    default:
      return 'UNKNOWN';
  }
};
