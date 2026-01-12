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
// NOTE: Backend uses camelCase for JSON properties (ASP.NET Core default)
export interface SiteSettings {
  // Tab 1: Site Settings
  plantLocation: string;
  plantOpeningTime: string; // HH:mm format
  plantClosingTime: string; // HH:mm format
  enablePreShipmentScan: boolean;
  orderArchiveDays: number; // Days before orders are moved to archive

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
 * Helper function to format time for backend TimeOnly deserialization
 * Ensures time is in "HH:mm:ss" format
 * - If time is already "HH:mm:ss", returns as-is
 * - If time is "HH:mm", appends ":00"
 * - If null/undefined, returns null
 */
function formatTimeForBackend(time: string | null | undefined): string | null {
  if (!time) return null;

  const parts = time.split(':');

  // If already has seconds (HH:mm:ss), return as-is
  if (parts.length === 3) {
    return time;
  }

  // If only HH:mm format, append :00
  if (parts.length === 2) {
    return `${time}:00`;
  }

  // Invalid format, return as-is and let backend validation handle it
  return time;
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
      plantOpeningTime: formatTimeForBackend(settings.plantOpeningTime),
      plantClosingTime: formatTimeForBackend(settings.plantClosingTime),
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
