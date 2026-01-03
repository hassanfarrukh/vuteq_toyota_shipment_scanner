/**
 * Site Settings API
 * Author: Hassan
 * Date: 2025-01-03
 *
 * API client for Site Settings including:
 * - Plant/Site configuration
 * - Dock Monitor settings
 * - Internal Kanban settings
 */

import apiClient, { getErrorMessage } from './client';

// Site Settings interface - matches all three tabs
export interface SiteSettings {
  // Tab 1: Site Settings
  plantLocation: string;
  plantOpeningTime: string; // HH:mm format
  plantClosingTime: string; // HH:mm format
  enablePreShipmentScan: boolean;

  // Tab 2: Dock Monitor
  behindThreshold: number; // minutes
  criticalThreshold: number; // minutes
  displayMode: 'FULL' | 'COMPACT';
  refreshInterval: number; // milliseconds
  orderLookbackHours: number; // hours

  // Tab 3: Internal Kanban
  allowDuplicates: boolean;
  duplicateWindowHours: number;
  alertOnDuplicate: boolean;
}

// API Response wrapper
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

/**
 * Get Site Settings
 */
export async function getSiteSettings(): Promise<{
  success: boolean;
  data?: SiteSettings;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<SiteSettings>>(
      '/api/v1/site-settings'
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch Site Settings',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Update Site Settings
 */
export async function updateSiteSettings(
  settings: Partial<SiteSettings>
): Promise<{
  success: boolean;
  data?: SiteSettings;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<SiteSettings>>(
      '/api/v1/site-settings',
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
      error: response.data.message || 'Failed to update Site Settings',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}
