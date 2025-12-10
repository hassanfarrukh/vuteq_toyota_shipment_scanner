/**
 * Offices API
 * Author: Hassan
 * Date: 2025-11-24
 *
 * CRUD operations for Office management
 */

import apiClient, { getErrorMessage } from './client';

// Office interface matching backend DTO
export interface Office {
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

// Create/Update Office DTO
export interface OfficeDto {
  code: string;
  name: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  phone?: string;
  contact?: string;
  email?: string;
  isActive?: boolean;
}

// API Response wrapper
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

/**
 * Get all offices (active only)
 */
export async function getOffices(): Promise<{
  success: boolean;
  data?: Office[];
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<Office[]>>('/api/v1/admin/offices');

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch offices',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get all offices (including inactive)
 */
export async function getAllOffices(): Promise<{
  success: boolean;
  data?: Office[];
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<Office[]>>('/api/v1/admin/offices/all');

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch all offices',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get office by ID
 */
export async function getOffice(id: string): Promise<{
  success: boolean;
  data?: Office;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<Office>>(
      `/api/v1/admin/offices/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch office',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Create new office
 */
export async function createOffice(data: OfficeDto): Promise<{
  success: boolean;
  data?: Office;
  error?: string;
}> {
  try {
    const response = await apiClient.post<ApiResponse<Office>>(
      '/api/v1/admin/offices',
      data
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to create office',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Update existing office
 */
export async function updateOffice(
  id: string,
  data: OfficeDto
): Promise<{
  success: boolean;
  data?: Office;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<Office>>(
      `/api/v1/admin/offices/${id}`,
      data
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to update office',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Delete office
 */
export async function deleteOffice(id: string): Promise<{
  success: boolean;
  error?: string;
}> {
  try {
    const response = await apiClient.delete<ApiResponse<null>>(
      `/api/v1/admin/offices/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to delete office',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}
