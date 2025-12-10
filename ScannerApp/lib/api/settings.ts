/**
 * Settings API
 * Author: Hassan
 * Date: 2025-12-01
 *
 * API client for Internal Kanban and Dock Monitor settings
 */

import apiClient, { getErrorMessage } from './client';

// Internal Kanban Settings interface
export interface InternalKanbanSettings {
  allowDuplicates: boolean;
  duplicateWindowHours: number;
  alertOnDuplicate: boolean;
}

// Dock Monitor Settings interface
export interface DockMonitorSettings {
  behindThreshold: number;
  criticalThreshold: number;
  displayMode: 'FULL' | 'SHIPMENT_ONLY' | 'SKID_ONLY' | 'COMPLETION_ONLY';
  selectedLocations: string[];
}

// API Response wrapper
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

/**
 * Get Internal Kanban settings
 */
export async function getInternalKanbanSettings(): Promise<{
  success: boolean;
  data?: InternalKanbanSettings;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<InternalKanbanSettings>>(
      '/api/v1/settings/internal-kanban'
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch Internal Kanban settings',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Save Internal Kanban settings
 */
export async function saveInternalKanbanSettings(
  settings: InternalKanbanSettings
): Promise<{
  success: boolean;
  data?: InternalKanbanSettings;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<InternalKanbanSettings>>(
      '/api/v1/settings/internal-kanban',
      settings
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to save Internal Kanban settings',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get Dock Monitor settings
 */
export async function getDockMonitorSettings(): Promise<{
  success: boolean;
  data?: DockMonitorSettings;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<DockMonitorSettings>>(
      '/api/v1/settings/dock-monitor'
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch Dock Monitor settings',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Save Dock Monitor settings
 */
export async function saveDockMonitorSettings(
  settings: DockMonitorSettings
): Promise<{
  success: boolean;
  data?: DockMonitorSettings;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<DockMonitorSettings>>(
      '/api/v1/settings/dock-monitor',
      settings
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to save Dock Monitor settings',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}
