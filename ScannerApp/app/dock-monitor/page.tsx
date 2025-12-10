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
 * Real-time dock status monitoring with 5-minute auto-refresh
 * Displays 10 columns in table format showing 1.5 days of information
 * Phase 1: Using dummy data
 */

'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Button from '@/components/ui/Button';
import Alert from '@/components/ui/Alert';
import { TIMING } from '@/lib/constants';
import {
  DUMMY_DOCK_ORDERS,
  getDisplayOrderNumber,
  getStatusColorClass,
  getStatusText,
  type DockOrderData,
} from '@/lib/dummyDockData';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import { usePageContext } from '@/contexts/PageContext';

type SortColumn = 'order' | 'route' | 'destination' | 'supplier' | 'plannedSkidBuild' | 'completedSkidBuild' | 'plannedShipmentLoad' | 'completedShipmentLoad' | 'status';
type SortDirection = 'asc' | 'desc';

export default function DockMonitorPage() {
  const router = useRouter();
  const { setSubtitle } = usePageContext();
  const [orders, setOrders] = useState<DockOrderData[]>(DUMMY_DOCK_ORDERS);
  const [loading, setLoading] = useState(false);
  const [autoRefresh, setAutoRefresh] = useState(true);
  const [countdown, setCountdown] = useState(300); // 5 minutes = 300 seconds
  const [lastRefreshed, setLastRefreshed] = useState(new Date());
  const [sortColumn, setSortColumn] = useState<SortColumn | null>(null);
  const [sortDirection, setSortDirection] = useState<SortDirection>('asc');
  const intervalRef = useRef<NodeJS.Timeout | null>(null);
  const countdownRef = useRef<NodeJS.Timeout | null>(null);

  // Load dock orders
  const loadDockOrders = async () => {
    setLoading(true);

    // Simulate API delay
    await new Promise(resolve => setTimeout(resolve, 500));

    // In Phase 2, this will be an actual API call
    setOrders(DUMMY_DOCK_ORDERS);
    setLastRefreshed(new Date());
    setCountdown(300); // Reset countdown

    setLoading(false);
  };

  // Update subtitle dynamically for Header
  useEffect(() => {
    const updateSubtitle = () => {
      const timeStr = lastRefreshed.toLocaleTimeString();
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

  const formatTime = (timeString: string | null): string => {
    if (!timeString) return '-';

    // Parse the time string (format: "HH:MM AM/PM")
    // For now, we'll add today's date to it
    const now = new Date();
    const [time, period] = timeString.split(' ');
    const [hours, minutes] = time.split(':');

    let hour = parseInt(hours);
    if (period === 'PM' && hour !== 12) hour += 12;
    if (period === 'AM' && hour === 12) hour = 0;

    const dateTime = new Date(now.getFullYear(), now.getMonth(), now.getDate(), hour, parseInt(minutes));

    // Format: "2025-10-30 10:30 AM"
    const year = dateTime.getFullYear();
    const month = String(dateTime.getMonth() + 1).padStart(2, '0');
    const day = String(dateTime.getDate()).padStart(2, '0');

    return `${year}-${month}-${day} ${timeString}`;
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
        aValue = a.destination;
        bValue = b.destination;
        break;
      case 'supplier':
        aValue = a.supplier;
        bValue = b.supplier;
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

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-4 pt-24 max-w-[2000px] mx-auto space-y-2">
          {/* Status Legend Card - Compact */}
          <Card>
            <div className="p-2">
              <div className="flex flex-wrap items-center gap-3 text-xs">
                <span className="font-bold text-gray-900 text-sm">STATUS:</span>
                <div className="flex items-center gap-1.5">
                  <div className="w-4 h-4 bg-green-500 rounded shadow-sm"></div>
                  <span className="font-medium text-gray-700">Completed</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-4 h-4 bg-blue-400 rounded shadow-sm"></div>
                  <span className="font-medium text-gray-700">On Time</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-4 h-4 bg-orange-500 rounded shadow-sm"></div>
                  <span className="font-medium text-gray-700">Behind</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-4 h-4 bg-red-500 rounded shadow-sm"></div>
                  <span className="font-medium text-gray-700">Critical</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-4 h-4 bg-yellow-500 rounded shadow-sm"></div>
                  <span className="font-medium text-gray-700">Project Short Ship</span>
                </div>
                <div className="flex items-center gap-1.5">
                  <div className="w-4 h-4 bg-purple-500 rounded shadow-sm"></div>
                  <span className="font-medium text-gray-700">Short Shipped</span>
                </div>
              </div>
            </div>
          </Card>

          {/* Orders Table */}
          <Card>
            <div className="bg-gradient-to-r from-gray-800 to-gray-900 px-4 py-3 -m-[1px] rounded-t-xl">
              <h2 className="text-lg font-bold text-white">ORDER STATUS ({orders.length} ORDERS)</h2>
            </div>
          <div className="p-0">
            {/* Table View */}
            <div className="overflow-x-auto">
              <table className="w-full border-collapse">
                <thead>
                  <tr className="bg-gray-700 text-white">
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('order')}
                    >
                      <div className="flex items-center justify-center">
                        Order{getSortIcon('order')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('route')}
                    >
                      <div className="flex items-center justify-center">
                        Route{getSortIcon('route')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('destination')}
                    >
                      <div className="flex items-center justify-center">
                        Destination{getSortIcon('destination')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('supplier')}
                    >
                      <div className="flex items-center justify-center">
                        Supplier{getSortIcon('supplier')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('plannedSkidBuild')}
                    >
                      <div className="flex items-center justify-center">
                        Planned<br/>Skid Build{getSortIcon('plannedSkidBuild')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('completedSkidBuild')}
                    >
                      <div className="flex items-center justify-center">
                        Completed<br/>Skid Build{getSortIcon('completedSkidBuild')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('plannedShipmentLoad')}
                    >
                      <div className="flex items-center justify-center">
                        Planned<br/>Pre-shipment Scan{getSortIcon('plannedShipmentLoad')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('completedShipmentLoad')}
                    >
                      <div className="flex items-center justify-center">
                        Completed<br/>Pre-shipment Scan{getSortIcon('completedShipmentLoad')}
                      </div>
                    </th>
                    <th
                      className="px-3 py-2 text-center text-sm font-bold uppercase border border-gray-600 cursor-pointer hover:bg-gray-600 transition-colors"
                      onClick={() => handleSort('status')}
                    >
                      <div className="flex items-center justify-center">
                        Status{getSortIcon('status')}
                      </div>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {sortedOrders.map((order, index) => (
                    <tr
                      key={index}
                      className={`${getStatusColorClass(order.status)} transition-colors hover:opacity-90`}
                    >
                      <td className="px-3 py-2 text-sm font-mono font-bold text-gray-900 border border-gray-300 text-center">
                        {getDisplayOrderNumber(order)}
                      </td>
                      <td className="px-3 py-2 text-sm font-semibold text-gray-900 border border-gray-300 text-center">
                        {order.route}
                      </td>
                      <td className="px-3 py-2 text-sm font-semibold text-gray-900 border border-gray-300 text-center">
                        {order.destination}
                      </td>
                      <td className="px-3 py-2 text-sm font-medium text-gray-900 border border-gray-300">
                        {order.supplier}
                      </td>
                      <td className="px-3 py-2 text-sm font-mono font-semibold text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.plannedSkidBuild)}
                      </td>
                      <td className="px-3 py-2 text-sm font-mono font-semibold text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.completedSkidBuild)}
                      </td>
                      <td className="px-3 py-2 text-sm font-mono font-semibold text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.plannedShipmentLoad)}
                      </td>
                      <td className="px-3 py-2 text-sm font-mono font-semibold text-gray-900 border border-gray-300 text-center">
                        {formatTime(order.completedShipmentLoad)}
                      </td>
                      <td className="px-3 py-2 text-sm border border-gray-300 text-center">
                        <span className="font-bold text-gray-900">
                          {getStatusText(order.status)}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
          </Card>

          {/* Action Buttons */}
          <div className="flex justify-end">
            <Button
              onClick={() => router.push('/')}
              variant="primary"
            >
              <i className="fa-light fa-home mr-2"></i>
              Return to Dashboard
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}
