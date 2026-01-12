/**
 * Dock Monitor API
 * Author: Hassan
 * Date: 2025-12-24
 */

import apiClient, { getErrorMessage } from './client';

export interface DockMonitorOrder {
  orderId: string;
  orderNumber: string;
  dockCode: string;
  destination: string | null;
  supplierCode: string | null;
  plannedPickup: string | null;
  plannedSkidBuild: string | null;
  completedSkidBuild: string | null;
  plannedShipmentLoad: string | null;
  completedShipmentLoad: string | null;
  status: 'COMPLETED' | 'ON_TIME' | 'BEHIND' | 'CRITICAL' | 'PROJECT_SHORT' | 'SHORT_SHIPPED';
  isSupplementOrder: boolean;
  toyotaSkidBuildConfirmationNumber: string | null;
  toyotaShipmentConfirmationNumber: string | null;
}

export interface DockMonitorShipment {
  routeNumber: string;
  run: string | null;
  supplierCode: string | null;
  pickupDateTime: string | null;
  shipmentStatus: string;
  completedAt: string | null;
  orders: DockMonitorOrder[];
}

export interface DockMonitorSettings {
  behindThreshold: number;
  criticalThreshold: number;
  displayMode: string;
  selectedLocations: string[];
}

export interface DockMonitorResponse {
  shipments: DockMonitorShipment[];
  totalOrders: number;
  settings: DockMonitorSettings;
  refreshedAt: string;
}

export async function getDockMonitorData(): Promise<{
  success: boolean;
  data?: DockMonitorResponse;
  error?: string;
}> {
  try {
    const response = await apiClient.get<{
      success: boolean;
      message: string;
      data: DockMonitorResponse;
    }>('/api/v1/dock-monitor/data');

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch dock monitor data',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}
