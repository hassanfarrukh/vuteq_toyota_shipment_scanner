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

// Site Settings interface - matches backend UpdateSiteSettingsRequest
export interface SiteSettings {
  // Tab 1: Site Settings
  plantLocation: string;
  plantOpeningTime: string; // HH:mm format
  plantClosingTime: string; // HH:mm format
  enablePreShipmentScan: boolean;

  // Tab 2: Dock Monitor (with "dock" prefix to match backend)
  dockBehindThreshold: number; // minutes
  dockCriticalThreshold: number; // minutes
  dockDisplayMode: 'FULL' | 'COMPACT';
  dockRefreshInterval: number; // milliseconds
  dockOrderLookbackHours: number; // hours

  // Tab 3: Internal Kanban (with "kanban" prefix to match backend)
  kanbanAllowDuplicates: boolean;
  kanbanDuplicateWindowHours: number;
  kanbanAlertOnDuplicate: boolean;
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
 * Note: Backend requires ALL fields, not partial updates
 */
export async function updateSiteSettings(
  settings: SiteSettings
): Promise<{
  success: boolean;
  data?: SiteSettings;
  error?: string;
}> {
  try {
    // Format time values for backend TimeOnly deserialization
    // Convert "HH:mm" to "HH:mm:ss" format which ASP.NET Core can parse as TimeOnly
    const payload = {
      ...settings,
      plantOpeningTime: settings.plantOpeningTime ? `${settings.plantOpeningTime}:00` : null,
      plantClosingTime: settings.plantClosingTime ? `${settings.plantClosingTime}:00` : null,
    };

    const response = await apiClient.put<ApiResponse<SiteSettings>>(
      '/api/v1/site-settings',
      payload
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
