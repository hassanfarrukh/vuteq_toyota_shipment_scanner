/**
 * Internal Kanban Exclusions API
 * Author: Hassan
 * Date: 2025-01-13
 *
 * CRUD operations for Internal Kanban Exclusions (Excluded Parts)
 */

import apiClient, { getErrorMessage } from './client';

// InternalKanbanExclusion interface matching backend DTO
export interface InternalKanbanExclusion {
  exclusionId: string; // GUID
  partNumber: string;
  isExcluded: boolean;
  mode: string; // 'single' or 'bulk'
  createdBy: string;
  createdByUsername: string | null;
  createdAt: string;
  updatedBy: string | null;
  updatedByUsername: string | null;
  updatedAt: string | null;
}

// Create/Update Exclusion DTO
export interface ExclusionDto {
  partNumber: string;
  isExcluded: boolean;
  // mode is NOT sent - backend auto-detects
}

// Bulk Upload Response
export interface BulkUploadResponse {
  totalProcessed: number;
  successCount: number;
  failedCount: number;
  errors: string[];
  createdExclusions: InternalKanbanExclusion[];
}

// API Response wrapper
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

/**
 * Get all exclusions
 */
export async function getExclusions(): Promise<{
  success: boolean;
  data?: InternalKanbanExclusion[];
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<InternalKanbanExclusion[]>>('/api/internal-kanban-exclusions');

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch exclusions',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get exclusion by ID
 */
export async function getExclusion(id: string): Promise<{
  success: boolean;
  data?: InternalKanbanExclusion;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<InternalKanbanExclusion>>(
      `/api/internal-kanban-exclusions/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch exclusion',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Create new exclusion (single)
 */
export async function createExclusion(data: ExclusionDto): Promise<{
  success: boolean;
  data?: InternalKanbanExclusion;
  error?: string;
}> {
  try {
    const response = await apiClient.post<ApiResponse<InternalKanbanExclusion>>(
      '/api/internal-kanban-exclusions',
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
      error: response.data.message || 'Failed to create exclusion',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Update existing exclusion
 */
export async function updateExclusion(
  id: string,
  data: ExclusionDto
): Promise<{
  success: boolean;
  data?: InternalKanbanExclusion;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<InternalKanbanExclusion>>(
      `/api/internal-kanban-exclusions/${id}`,
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
      error: response.data.message || 'Failed to update exclusion',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Delete exclusion
 */
export async function deleteExclusion(id: string): Promise<{
  success: boolean;
  error?: string;
}> {
  try {
    const response = await apiClient.delete<ApiResponse<null>>(
      `/api/internal-kanban-exclusions/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to delete exclusion',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Bulk upload exclusions from Excel file
 */
export async function bulkUploadExclusions(file: File): Promise<{
  success: boolean;
  data?: BulkUploadResponse;
  error?: string;
}> {
  try {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<ApiResponse<BulkUploadResponse>>(
      '/api/internal-kanban-exclusions/bulk-upload',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to upload file',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}
