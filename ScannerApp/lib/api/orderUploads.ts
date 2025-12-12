/**
 * Order Uploads API Module
 * Author: Hassan
 * Date: 2025-12-01
 *
 * API functions for order file uploads and processing
 */

import apiClient, { getErrorMessage } from './client';

// Types matching backend DTOs
export interface ExtractedOrderItemDto {
  partNumber: string;
  description?: string;
  lotQty: number;
  kanbanNumber?: string;
  lotsOrdered: number;
}

export interface ExtractedOrderDto {
  owkNumber: string;
  customerName?: string;
  supplierCode?: string;
  dockCode?: string;
  orderSeries?: string;
  orderNumber?: string;
  orderDate?: string;
  arriveDateTime?: string;
  departDateTime?: string;
  unloadDateTime?: string;
  itemCount: number;
  items: ExtractedOrderItemDto[];
}

export interface OrderUploadResponseDto {
  uploadId: string;
  fileName: string;
  fileSize: number;
  uploadDate: string;
  status: 'pending' | 'processing' | 'success' | 'error';
  ordersCreated: number;
  totalItemsCreated: number;
  totalManifestsCreated?: number;
  ordersSkipped?: number;
  skippedOrderNumbers?: string[];
  extractedOrders: ExtractedOrderDto[];
  errorMessage?: string;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
}

/**
 * Upload and process an order PDF file
 * @param file PDF file to upload
 * @returns Upload response with extracted order data
 */
export async function uploadOrderFile(file: File): Promise<ApiResponse<OrderUploadResponseDto>> {
  try {
    const formData = new FormData();
    formData.append('file', file);

    const response = await apiClient.post<ApiResponse<OrderUploadResponseDto>>(
      '/api/v1/orders/upload',
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        timeout: 60000, // 60 seconds for file upload
      }
    );

    return response.data;
  } catch (error) {
    return {
      success: false,
      message: getErrorMessage(error),
      errors: [getErrorMessage(error)],
    };
  }
}

/**
 * Get upload history
 * @returns List of all upload records
 */
export async function getUploadHistory(): Promise<ApiResponse<OrderUploadResponseDto[]>> {
  try {
    const response = await apiClient.get<ApiResponse<OrderUploadResponseDto[]>>(
      '/api/v1/orders/uploads'
    );
    return response.data;
  } catch (error) {
    return {
      success: false,
      message: getErrorMessage(error),
      errors: [getErrorMessage(error)],
    };
  }
}

/**
 * Get specific upload by ID
 * @param id Upload ID
 * @returns Upload record details
 */
export async function getUploadById(id: string): Promise<ApiResponse<OrderUploadResponseDto>> {
  try {
    const response = await apiClient.get<ApiResponse<OrderUploadResponseDto>>(
      `/api/v1/orders/uploads/${id}`
    );
    return response.data;
  } catch (error) {
    return {
      success: false,
      message: getErrorMessage(error),
      errors: [getErrorMessage(error)],
    };
  }
}

/**
 * Delete upload record
 * @param id Upload ID to delete
 * @returns Deletion result
 */
export async function deleteUpload(id: string): Promise<ApiResponse<boolean>> {
  try {
    const response = await apiClient.delete<ApiResponse<boolean>>(
      `/api/v1/orders/uploads/${id}`
    );
    return response.data;
  } catch (error) {
    return {
      success: false,
      message: getErrorMessage(error),
      errors: [getErrorMessage(error)],
    };
  }
}

/**
 * Order DTO from backend (tblOrders)
 */
export interface OrderDto {
  orderId: string;
  realOrderNumber: string;
  totalParts: number;
  dockCode: string;
  departureDate: string;
  orderDate: string;
  status: string;
  uploadId: string;
  uploadFileName?: string;
}

/**
 * Get orders - all or filtered by uploadId
 * @param uploadId Optional upload ID to filter by
 * @returns List of orders
 */
export async function getOrders(uploadId?: string): Promise<ApiResponse<OrderDto[]>> {
  try {
    const url = uploadId
      ? `/api/v1/orders?uploadId=${uploadId}`
      : '/api/v1/orders';

    const response = await apiClient.get<ApiResponse<OrderDto[]>>(url);
    return response.data;
  } catch (error) {
    return {
      success: false,
      message: getErrorMessage(error),
      errors: [getErrorMessage(error)],
    };
  }
}

/**
 * Planned item DTO from backend
 */
export interface PlannedItemDto {
  id: string;
  realOrderNumber: string;
  dockCode: string;
  partNumber: string;
  qpc: number;
  kanbanNumber: string;
  internalKanban: string;
  totalBoxPlanned: number;
  palletizationCode: string;
  externalOrderId: number;
  skidUid: string | null;
  manifestNo: number;
  shortOver: number | null;
  uploadId: string;
  orderId?: string;
  uploadFileName?: string;
}

/**
 * Get planned items - all or filtered by orderId
 * @param orderId Optional order ID to filter by
 * @returns List of planned items
 */
export async function getPlannedItems(orderId?: string): Promise<ApiResponse<PlannedItemDto[]>> {
  try {
    const url = orderId
      ? `/api/v1/orders/planned-items?orderId=${orderId}`
      : '/api/v1/orders/planned-items';

    const response = await apiClient.get<ApiResponse<PlannedItemDto[]>>(url);
    return response.data;
  } catch (error) {
    return {
      success: false,
      message: getErrorMessage(error),
      errors: [getErrorMessage(error)],
    };
  }
}
