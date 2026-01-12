/**
 * Dock Monitor Page
 * Author: Hassan
 * Date: 2025-10-21
 * Updated: 2025-10-21 - Made compact layout: removed info alert, shrunk header and legend
 * Updated: 2025-10-21 - Added VUTEQ static gradient background for optimal performance
 * Updated: 2025-10-21 - Replaced old static background with new VUTEQ static blob background
 * Updated: 2025-10-22 - Applied new mobile-optimized responsive background, removed Phase 1 notice
 * Updated: 2025-10-22 - Moved header and legend inside cards, reverted to VUTEQStaticBackground
 * Updated: 2025-10-28 - Removed heading card, integrated dynamic subtitle with Header via PageContext
 * Updated: 2025-10-28 - Fixed scroll position for desktop: fixed background, scrollable content (Hassan)
 * Updated: 2025-10-28 - Reduced card spacing from space-y-3 to space-y-2 for more compact layout (Hassan)
 * Updated: 2025-10-28 - Fixed button styling to match administration page: responsive width, proper container (Hassan)
 * Updated: 2025-10-29 - REVERTED: Removed "Refresh Now" button, "Update Status" action buttons, and "Actions" column (Hassan)
 * Updated: 2025-10-29 - Fixed "Back to Dashboard" button to use standardized primary style with fa-light fa-home icon (Hassan)
 * Updated: 2025-10-30 - Added datetime display format (date + time) and table sorting functionality (Hassan)
 * Updated: 2025-12-24 - Integrated with real backend API (Hassan)
 * Updated: 2026-01-12 - Added TSCN (Toyota Confirmation Number) columns for Skid Build and Shipment (Hassan)
 * Updated: 2026-01-12 - Fixed full-width layout: removed container constraints, maximized table width (Hassan)
 * Real-time dock status monitoring with 5-minute auto-refresh
 * Displays 12 columns in table format showing 1.5 days of information
 */

'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Button from '@/components/ui/Button';
import Alert from '@/components/ui/Alert';
import { TIMING } from '@/lib/constants';
import { getDockMonitorData, type DockMonitorOrder } from '@/lib/api/dock-monitor';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import { usePageContext } from '@/contexts/PageContext';

type SortColumn = 'order' | 'route' | 'destination' | 'supplier' | 'plannedSkidBuild' | 'completedSkidBuild' | 'tscnSkidBuild' | 'plannedShipmentLoad' | 'completedShipmentLoad' | 'tscnShipment' | 'status';
type SortDirection = 'asc' | 'desc';

// Type for flattened order (includes route from shipment)
interface FlattenedOrder extends DockMonitorOrder {
  route: string;
}

export default function DockMonitorPage() {
  const router = useRouter();
  const { setSubtitle } = usePageContext();
  const [orders, setOrders] = useState<FlattenedOrder[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [countdown, setCountdown] = useState(300); // 5 minutes = 300 seconds
  const [lastRefreshed, setLastRefreshed] = useState(new Date());
  const [sortColumn, setSortColumn] = useState<SortColumn | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const [currentPage, setCurrentPage] = useState(1);
  const [rowsPerPage] = useState(15); // Rows per page
  const intervalRef = useRef<NodeJS.Timeout | null>(null);
  const countdownRef = useRef<NodeJS.Timeout | null>(null);

  // Load dock orders from API
  const loadDockOrders = async () => {
    setLoading(true);
    setError(null);

    const result = await getDockMonitorData();

    if (result.success && result.data) {
      // Flatten shipments into orders with route information
      const flattenedOrders: FlattenedOrder[] = [];

      result.data.shipments.forEach((shipment) => {
        shipment.orders.forEach((order) => {
          flattenedOrders.push({
            ...order,
            route: shipment.routeNumber,
          });
        });
      });

      setOrders(flattenedOrders);
      setLastRefreshed(new Date());
      setCountdown(300); // Reset countdown
    } else {
      setError(result.error || 'Failed to load dock monitor data');
    }

    setLoading(false);
  };

  // Initial load
  useEffect(() => {
    loadDockOrders();
  }, []);

  // Update subtitle dynamically for Header
  useEffect(() => {
    const updateSubtitle = () => {
      const timeStr = lastRefreshed.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit', hour12: false });
      const countdownStr = formatCountdown(countdown);
      setSubtitle(`Last updated: ${timeStr} • Next: ${countdownStr}`);
    };

    updateSubtitle();

    // Cleanup: clear subtitle when component unmounts
    return () => {
      
      setSubtitle(null);
    };
  }, [lastRefreshed, countdown, setSubtitle]);

  // Setup auto-refresh (5-minute interval)
  useEffect(() => {
    if (autoRefresh) {
      // Set up 5-minute interval for data refresh
      intervalRef.current = setInterval(() => {
        loadDockOrders();
      }, TIMING.DOCK_REFRESH_INTERVAL);

      // Set up 1-second countdown
      countdownRef.current = setInterval(() => {
        setCountdown((prev) => {
          if (prev <= 1) return 300;
          return prev - 1;
        });
      }, 1000);
    }

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
      if (countdownRef.current) clearInterval(countdownRef.current);
    };
  }, [autoRefresh]);

  const handleManualRefresh = () => {
    loadDockOrders();
  };

  const formatCountdown = (seconds: number): string => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };

  /**
   * Format ISO datetime to display format: "YYYY-MM-DD HH:MM"
   */
  const formatTime = (isoString: string | null): string => {
    if (!isoString) return '-';

    try {
      const date = new Date(isoString);

      // Format: "YYYY-MM-DD HH:MM"
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      const hours = String(date.getHours()).padStart(2, '0');
      const minutes = String(date.getMinutes()).padStart(2, '0');

      return `${year}-${month}-${day} ${hours}:${minutes}`;
    } catch (error) {
      return '-';
    }
  };

  /**
   * Get display order number (adds EX- prefix for supplement orders)
   */
  const getDisplayOrderNumber = (order: FlattenedOrder): string => {
    return order.isSupplementOrder ? `EX-${order.orderNumber}` : order.orderNumber;
  };

  /**
   * Get status color class for table rows (light background)
   */
  const getStatusRowClass = (status: string): string => {
    switch (status) {
      case 'COMPLETED':
        return 'bg-green-50';
      case 'ON_TIME':
        return 'bg-blue-50';
      case 'BEHIND':
        return 'bg-orange-50';
      case 'CRITICAL':
        return 'bg-red-50';
      case 'PROJECT_SHORT':
        return 'bg-yellow-50';
      case 'SHORT_SHIPPED':
        return 'bg-purple-50';
      default:
        return 'bg-gray-50';
    }
  };

  /**
   * Get status bubble/badge color class (matches legend colors)
   */
  const getStatusBubbleClass = (status: string): string => {
    switch (status) {
      case 'COMPLETED':
        return 'bg-green-500 text-white';
      case 'ON_TIME':
        return 'bg-blue-400 text-white';
      case 'BEHIND':
        return 'bg-orange-500 text-white';
      case 'CRITICAL':
        return 'bg-red-500 text-white';
      case 'PROJECT_SHORT':
        return 'bg-yellow-500 text-gray-900';
      case 'SHORT_SHIPPED':
        return 'bg-purple-500 text-white';
      default:
        return 'bg-gray-400 text-white';
    }
  };

  /**
   * Get status badge display text
   */
  const getStatusText = (status: string): string => {
    switch (status) {
      case 'COMPLETED':
        return 'Completed';
      case 'ON_TIME':
        return 'On Time';
      case 'BEHIND':
        return 'Behind';
      case 'CRITICAL':
        return 'Critical';
      case 'PROJECT_SHORT':
        return 'Project Short';
      case 'SHORT_SHIPPED':
        return 'Short Shipped';
      default:
        return 'Unknown';
    }
  };

  const handleSort = (column: SortColumn) => {
    if (sortColumn === column) {
      // Toggle direction if clicking the same column
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      // New column, default to ascending
      setSortColumn(column);
      setSortDirection('asc');
    }
  };

  const getSortIcon = (column: SortColumn) => {
    if (sortColumn !== column) {
      return null; // No icon for unsorted columns
    }
    return sortDirection === 'asc'
      ? <i className="fa-light fa-sort-up ml-1 text-xs"></i>
      : <i className="fa-light fa-sort-down ml-1 text-xs"></i>;
  };

  // Sort orders based on current sort state
  const sortedOrders = [...orders].sort((a, b) => {
    if (!sortColumn) return 0;

    let aValue: any;
    let bValue: any;

    switch (sortColumn) {
      case 'order':
        aValue = getDisplayOrderNumber(a);
        bValue = getDisplayOrderNumber(b);
        break;
      case 'route':
        aValue = a.route;
        bValue = b.route;
        break;
      case 'destination':
        aValue = a.destination || a.dockCode || '';
        bValue = b.destination || b.dockCode || '';
        break;
      case 'supplier':
        aValue = a.supplierCode || '';
        bValue = b.supplierCode || '';
        break;
      case 'plannedSkidBuild':
        aValue = a.plannedSkidBuild || '';
        bValue = b.plannedSkidBuild || '';
        break;
      case 'completedSkidBuild':
        aValue = a.completedSkidBuild || '';
        bValue = b.completedSkidBuild || '';
        break;
      case 'plannedShipmentLoad':
        aValue = a.plannedShipmentLoad || '';
        bValue = b.plannedShipmentLoad || '';
        break;
      case 'completedShipmentLoad':
        aValue = a.completedShipmentLoad || '';
        bValue = b.completedShipmentLoad || '';
        break;
      case 'tscnSkidBuild':
        aValue = a.toyotaSkidBuildConfirmationNumber || '';
        bValue = b.toyotaSkidBuildConfirmationNumber || '';
        break;
      case 'tscnShipment':
        aValue = a.toyotaShipmentConfirmationNumber || '';
        bValue = b.toyotaShipmentConfirmationNumber || '';
        break;
      case 'status':
        aValue = getStatusText(a.status);
        bValue = getStatusText(b.status);
        break;
      default:
        return 0;
    }

    // Handle null/empty values
    if (!aValue && !bValue) return 0;
    if (!aValue) return 1;
    if (!bValue) return -1;

    // String comparison
    const comparison = String(aValue).localeCompare(String(bValue));
    return sortDirection === 'asc' ? comparison : -comparison;
  });

  // Pagination calculations
  const totalPages = Math.ceil(sortedOrders.length / rowsPerPage);
  const startIndex = (currentPage - 1) * rowsPerPage;
  const endIndex = startIndex + rowsPerPage;
  const paginatedOrders = sortedOrders.slice(startIndex, endIndex);

  // Reset to page 1 when data changes
  const handlePageChange = (page: number) => {
    if (page >= 1 && page <= totalPages) {
      setCurrentPage(page);
    }
  };

  return (
    <div className="relative">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content */}
      <div className="relative">
        {/* Status Legend - Compact inline */}
        <div className="flex flex-wrap items-center gap-2 text-xs pb-1 bg-white/80 rounded px-2 mb-1">
          <span className="font-bold text-gray-900">STATUS:</span>
          <div className="flex items-center gap-1">
            <div className="w-3 h-3 bg-green-500 rounded"></div>
            <span className="text-gray-700">Completed</span>
          </div>
          <div className="flex items-center gap-1">
            <div className="w-3 h-3 bg-blue-400 rounded"></div>
            <span className="text-gray-700">On Time</span>
          </div>
          <div className="flex items-center gap-1">
            <div className="w-3 h-3 bg-orange-500 rounded"></div>
            <span className="text-gray-700">Behind</span>
          </div>
          <div className="flex items-center gap-1">
            <div className="w-3 h-3 bg-red-500 rounded"></div>
            <span className="text-gray-700">Critical</span>
          </div>
          <div className="flex items-center gap-1">
            <div className="w-3 h-3 bg-yellow-500 rounded"></div>
            <span className="text-gray-700">Project Short</span>
          </div>
          <div className="flex items-center gap-1">
            <div className="w-3 h-3 bg-purple-500 rounded"></div>
            <span className="text-gray-700">Short Shipped</span>
          </div>
        </div>

        {/* Error Alert */}
        {error && (
          <Alert variant="error" className="mb-1">
            <div className="flex items-center justify-between">
              <span>{error}</span>
              <Button onClick={handleManualRefresh} variant="secondary" size="sm">
                Retry
              </Button>
            </div>
          </Alert>
        )}

        {/* Orders Table - Full width */}
        <div className="bg-white rounded-lg shadow overflow-hidden w-full">
          <div className="bg-gradient-to-r from-gray-800 to-gray-900 px-2 py-1.5">
            <h2 className="text-sm font-bold text-white">ORDER STATUS ({orders.length} ORDERS)</h2>
          </div>
          <div>
            {/* Loading State */}
            {loading && orders.length === 0 ? (
              <div className="flex justify-center items-center py-12">
                <div className="text-center">
                  <i className="fa fa-spinner fa-spin text-4xl text-gray-600 mb-4"></i>
                  <p className="text-gray-600">Loading dock monitor data...</p>
                </div>
              </div>
            ) : orders.length === 0 ? (
              <div className="flex justify-center items-center py-12">
                <div className="text-center">
                  <i className="fa fa-box text-4xl text-gray-400 mb-4"></i>
                  <p className="text-gray-600">No orders found</p>
                </div>
              </div>
            ) : (
              <>
                {/* Table View */}
                <div className="overflow-x-auto">
                  <table className="w-full border-collapse">
                <thead className="sticky top-0 z-10">
                  <tr className="bg-gray-700 text-white">
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('order')}
                    >
                      <div className="flex items-center justify-center">
                        Order{getSortIcon('order')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('route')}
                    >
                      <div className="flex items-center justify-center">
                        Route{getSortIcon('route')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('destination')}
                    >
                      <div className="flex items-center justify-center">
                        Destination{getSortIcon('destination')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('supplier')}
                    >
                      <div className="flex items-center justify-center">
                        Supplier{getSortIcon('supplier')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('plannedSkidBuild')}
                    >
                      <div className="flex items-center justify-center">
                        Planned<br/>Skid Build{getSortIcon('plannedSkidBuild')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('completedSkidBuild')}
                    >
                      <div className="flex items-center justify-center">
                        Completed<br/>Skid Build{getSortIcon('completedSkidBuild')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('tscnSkidBuild')}
                    >
                      <div className="flex items-center justify-center">
                        TSCN<br/>Skid Build{getSortIcon('tscnSkidBuild')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('plannedShipmentLoad')}
                    >
                      <div className="flex items-center justify-center">
                        Planned<br/>Shipment{getSortIcon('plannedShipmentLoad')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('completedShipmentLoad')}
                    >
                      <div className="flex items-center justify-center">
                        Completed<br/>Shipment{getSortIcon('completedShipmentLoad')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('tscnShipment')}
                    >
                      <div className="flex items-center justify-center">
                        TSCN<br/>Shipment{getSortIcon('tscnShipment')}
                      </div>
                    </th>
                    <th
                      className="px-2 py-2 text-center text-xs font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('status')}
                    >
                      <div className="flex items-center justify-center">
                        Status{getSortIcon('status')}
                      </div>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {paginatedOrders.map((order, index) => (
                    <tr
                      key={index}
                      className={`${getStatusRowClass(order.status)} transition-colors hover:opacity-80`}
                    >
                      <td className="px-2 py-1.5 text-xs font-mono font-bold text-gray-900 border border-gray-300 text-center">
                        {getDisplayOrderNumber(order)}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-semibold text-gray-900 border border-gray-300 text-center">
                        {order.route}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-semibold text-gray-900 border border-gray-300 text-center">
                        {order.destination || order.dockCode || '-'}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-medium text-gray-900 border border-gray-300 text-center">
                        {order.supplierCode || '-'}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-mono text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.plannedSkidBuild)}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-mono text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.completedSkidBuild)}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-medium text-gray-900 border border-gray-300 text-center">
                        {order.toyotaSkidBuildConfirmationNumber || '-'}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-mono text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.plannedShipmentLoad)}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-mono text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.completedShipmentLoad)}
                      </td>
                      <td className="px-2 py-1.5 text-xs font-medium text-gray-900 border border-gray-300 text-center">
                        {order.toyotaShipmentConfirmationNumber || '-'}
                      </td>
                      <td className="px-2 py-1.5 text-xs border border-gray-300 text-center">
                        <span className={`inline-block px-2 py-1 rounded-full text-xs font-bold ${getStatusBubbleClass(order.status)}`}>
                          {getStatusText(order.status)}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
              </>
            )}
          </div>

          {/* Pagination Controls */}
          {orders.length > 0 && (
            <div className="flex items-center justify-between px-2 py-1.5 bg-gray-100 border-t">
              <div className="text-xs text-gray-600">
                Showing {startIndex + 1}-{Math.min(endIndex, orders.length)} of {orders.length} orders
              </div>
              <div className="flex items-center gap-2">
                <button
                  onClick={() => handlePageChange(currentPage - 1)}
                  disabled={currentPage === 1}
                  className="px-2 py-1 text-xs font-medium bg-white border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  <i className="fa-light fa-chevron-left mr-1"></i> Prev
                </button>
                <span className="text-xs text-gray-700 font-medium">
                  Page {currentPage} of {totalPages}
                </span>
                <button
                  onClick={() => handlePageChange(currentPage + 1)}
                  disabled={currentPage === totalPages}
                  className="px-2 py-1 text-xs font-medium bg-white border border-gray-300 rounded hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                  Next <i className="fa-light fa-chevron-right ml-1"></i>
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
