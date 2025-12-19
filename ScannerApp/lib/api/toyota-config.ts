/**
 * Toyota Configuration API
 * Author: Hassan
 * Date: 2025-12-14
 *
 * API client for managing Toyota API configurations (QA and PROD environments)
 */

import apiClient, { getErrorMessage } from './client';

// Toyota Config Response DTO
export interface ToyotaConfigResponse {
  configId: string; // Guid
  environment: string; // "QA" or "PROD"
  applicationName?: string;
  clientId: string;
  clientSecretMasked: string; // Always "********"
  tokenUrl: string;
  apiBaseUrl: string;
  resourceUrl: string; // V2.1 - Resource URL for OAuth
  xClientId: string; // V2.1 - X-Client-ID header
  isActive: boolean;
  createdBy?: string;
  createdAt: string;
  updatedBy?: string;
  updatedAt?: string;
}

// Toyota Config Create DTO
export interface ToyotaConfigCreate {
  environment: string; // "QA" or "PROD"
  applicationName?: string;
  clientId: string;
  clientSecret: string;
  tokenUrl: string;
  apiBaseUrl: string;
  resourceUrl: string; // V2.1 - Resource URL for OAuth (required)
  xClientId: string; // V2.1 - X-Client-ID header (required)
  isActive: boolean;
}

// Toyota Config Update DTO
export interface ToyotaConfigUpdate {
  environment?: string;
  applicationName?: string;
  clientId?: string;
  clientSecret?: string; // Only send if changing
  tokenUrl?: string;
  apiBaseUrl?: string;
  resourceUrl?: string; // V2.1 - Resource URL for OAuth (optional for updates)
  xClientId?: string; // V2.1 - X-Client-ID header (optional for updates)
  isActive?: boolean;
}

// Toyota Connection Test Response DTO
export interface ToyotaConnectionTest {
  success: boolean;
  message: string;
  tokenPreview?: string;
  expiresIn?: number;
  testedAt?: string;
}

// API Response wrapper
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}

/**
 * Get all Toyota configurations
 */
export async function getAllToyotaConfigs(): Promise<{
  success: boolean;
  data?: ToyotaConfigResponse[];
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<ToyotaConfigResponse[]>>(
      '/api/v1/toyota-config'
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch Toyota configurations',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get Toyota configuration by ID
 */
export async function getToyotaConfigById(configId: string): Promise<{
  success: boolean;
  data?: ToyotaConfigResponse;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<ToyotaConfigResponse>>(
      `/api/v1/toyota-config/${configId}`
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch Toyota configuration',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get active Toyota configuration by environment
 */
export async function getActiveToyotaConfig(environment: string): Promise<{
  success: boolean;
  data?: ToyotaConfigResponse;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<ToyotaConfigResponse>>(
      `/api/v1/toyota-config/environment/${environment}`
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch active Toyota configuration',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Create new Toyota configuration
 */
export async function createToyotaConfig(
  config: ToyotaConfigCreate
): Promise<{
  success: boolean;
  data?: ToyotaConfigResponse;
  error?: string;
}> {
  try {
    const response = await apiClient.post<ApiResponse<ToyotaConfigResponse>>(
      '/api/v1/toyota-config',
      config
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to create Toyota configuration',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Update Toyota configuration
 */
export async function updateToyotaConfig(
  configId: string,
  config: ToyotaConfigUpdate
): Promise<{
  success: boolean;
  data?: ToyotaConfigResponse;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<ToyotaConfigResponse>>(
      `/api/v1/toyota-config/${configId}`,
      config
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to update Toyota configuration',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Delete Toyota configuration
 */
export async function deleteToyotaConfig(configId: string): Promise<{
  success: boolean;
  error?: string;
}> {
  try {
    const response = await apiClient.delete<ApiResponse<void>>(
      `/api/v1/toyota-config/${configId}`
    );

    if (response.data.success) {
      return {
        success: true,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to delete Toyota configuration',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Test Toyota API connection (get OAuth token)
 */
export async function testToyotaConnection(configId: string): Promise<{
  success: boolean;
  data?: ToyotaConnectionTest;
  error?: string;
}> {
  try {
    const response = await apiClient.post<ApiResponse<ToyotaConnectionTest>>(
      `/api/v1/toyota-config/${configId}/test`
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to test Toyota connection',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

// Default values for new configurations (V2.1)
export const TOYOTA_CONFIG_DEFAULTS = {
  QA: {
    tokenUrl: 'https://login.microsoftonline.com/9107b728-2166-4e5d-8d13-d1ffdf0351ef/oauth2/token',
    apiBaseUrl: 'https://api.qa.toyota.com/tmna/qa/Logistics/v/SupplierOrderInformation/',
    resourceUrl: 'https://tmnatest.onmicrosoft.com/supplier-order-information-oauth-lios-product',
    xClientId: 'a1012aed-c89a-49d3-a796-63a4345ecc98',
  },
  PROD: {
    tokenUrl: 'https://login.microsoftonline.com/8c642d1d-d709-47b0-ab10-080af10798fb/oauth2/token',
    apiBaseUrl: 'https://api.toyota.com/tmna/prod/Logistics/v/SupplierOrderInformation/',
    resourceUrl: 'https://toyota1.onmicrosoft.com/supplier-order-information-oauth-lios-product',
    xClientId: 'a1012aed-c89a-49d3-a796-63a4345ecc98',
  },
};
