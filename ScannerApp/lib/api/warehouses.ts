/**
 * Warehouses API
 * Author: Hassan
 * Date: 2025-11-24
 *
 * CRUD operations for Warehouse management
 */

import apiClient, { getErrorMessage } from './client';

// Warehouse interface matching backend DTO
export interface Warehouse {
  warehouseId: string; // GUID
  code: string;
  name: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  office: string; // Office code (FK)
  phone?: string;
  contactName?: string;
  contactEmail?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

// Create/Update Warehouse DTO
export interface WarehouseDto {
  code: string;
  name: string;
  address: string;
  city: string;
  state: string;
  zip: string;
  office: string; // Office code (FK)
  phone?: string;
  contactName?: string;
  contactEmail?: string;
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
 * Get all warehouses (active only)
 */
export async function getWarehouses(): Promise<{
  success: boolean;
  data?: Warehouse[];
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<Warehouse[]>>(
      '/api/v1/admin/warehouses'
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch warehouses',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get all warehouses (including inactive)
 */
export async function getAllWarehouses(): Promise<{
  success: boolean;
  data?: Warehouse[];
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<Warehouse[]>>(
      '/api/v1/admin/warehouses/all'
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch all warehouses',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get warehouse by ID
 */
export async function getWarehouse(id: string): Promise<{
  success: boolean;
  data?: Warehouse;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<Warehouse>>(
      `/api/v1/admin/warehouses/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch warehouse',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Create new warehouse
 */
export async function createWarehouse(data: WarehouseDto): Promise<{
  success: boolean;
  data?: Warehouse;
  error?: string;
}> {
  try {
    const response = await apiClient.post<ApiResponse<Warehouse>>(
      '/api/v1/admin/warehouses',
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
      error: response.data.message || 'Failed to create warehouse',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Update existing warehouse
 */
export async function updateWarehouse(
  id: string,
  data: WarehouseDto
): Promise<{
  success: boolean;
  data?: Warehouse;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<Warehouse>>(
      `/api/v1/admin/warehouses/${id}`,
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
      error: response.data.message || 'Failed to update warehouse',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Delete warehouse
 */
export async function deleteWarehouse(id: string): Promise<{
  success: boolean;
  error?: string;
}> {
  try {
    const response = await apiClient.delete<ApiResponse<null>>(
      `/api/v1/admin/warehouses/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to delete warehouse',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}
