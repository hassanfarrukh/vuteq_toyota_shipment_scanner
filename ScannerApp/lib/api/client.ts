/**
 * API Client Configuration
 * Author: Hassan
 * Date: 2025-11-24
 * Updated: 2025-11-24 - Enhanced error handling for ECONNREFUSED
 * Updated: 2025-11-25 - Added comprehensive file logging for debugging API requests
 *
 * Axios instance configured for backend API with:
 * - Base URL: http://localhost:5000 (configurable via NEXT_PUBLIC_API_URL)
 * - JWT token interceptor
 * - Error handling and response transformation
 */

import axios, { AxiosInstance, AxiosError, InternalAxiosRequestConfig } from 'axios';
import { clientLogger } from '@/lib/logger';

// API Base URL
// In Docker: NEXT_PUBLIC_API_URL should be http://api:5000 (using service name)
// Local dev: http://localhost:5000
// Note: Don't add /api suffix here - endpoints already have it
const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000, // 30 seconds
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor - Add JWT token to all requests
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Get JWT token from localStorage
    const token = typeof window !== 'undefined' ? localStorage.getItem('jwt_token') : null;

    clientLogger.debug('API Client', 'Outgoing HTTP request', {
      method: config.method?.toUpperCase(),
      url: config.url,
      baseURL: config.baseURL,
      fullURL: `${config.baseURL}${config.url}`,
      hasToken: !!token,
      hasData: !!config.data,
      dataPreview: config.data ? JSON.stringify(config.data).substring(0, 200) : null,
    });

    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
      clientLogger.debug('API Client', 'Added Authorization header to request');
    }

    return config;
  },
  (error: AxiosError) => {
    clientLogger.error('API Client', 'Request interceptor error', {
      error: error.message,
      code: error.code,
    });
    return Promise.reject(error);
  }
);

// Response interceptor - Handle errors globally
apiClient.interceptors.response.use(
  (response) => {
    clientLogger.info('API Client', 'HTTP response received', {
      status: response.status,
      statusText: response.statusText,
      url: response.config.url,
      method: response.config.method?.toUpperCase(),
      hasData: !!response.data,
      dataPreview: response.data ? JSON.stringify(response.data).substring(0, 200) : null,
    });
    return response;
  },
  (error: AxiosError) => {
    clientLogger.error('API Client', 'HTTP response error', {
      message: error.message,
      code: error.code,
      status: error.response?.status,
      statusText: error.response?.statusText,
      url: error.config?.url,
      method: error.config?.method?.toUpperCase(),
      responseData: error.response?.data,
    });

    // Handle 401 Unauthorized - redirect to login
    if (error.response?.status === 401) {
      clientLogger.warn('API Client', '401 Unauthorized - clearing tokens');
      // Clear token
      if (typeof window !== 'undefined') {
        localStorage.removeItem('jwt_token');
        sessionStorage.removeItem('auth_user');

        // Only redirect to login if we're not already on the login page
        // This allows login errors to be displayed properly
        if (window.location.pathname !== '/login') {
          clientLogger.warn('API Client', 'Redirecting to login page');
          window.location.href = '/login';
        } else {
          clientLogger.info('API Client', 'Already on login page - allowing error to propagate');
        }
      }
    }

    // Handle 403 Forbidden
    if (error.response?.status === 403) {
      clientLogger.error('API Client', '403 Forbidden - Access denied', {
        responseData: error.response.data,
      });
    }

    // Handle 404 Not Found
    if (error.response?.status === 404) {
      clientLogger.error('API Client', '404 Not Found - Resource not found', {
        responseData: error.response.data,
      });
    }

    // Handle 500 Internal Server Error
    if (error.response?.status === 500) {
      clientLogger.error('API Client', '500 Internal Server Error', {
        responseData: error.response.data,
      });
    }

    // Handle network errors
    if (error.code === 'ECONNREFUSED' || error.message === 'Network Error') {
      clientLogger.error('API Client', 'Network connection failed - cannot reach server', {
        code: error.code,
        message: error.message,
      });
    }

    return Promise.reject(error);
  }
);

// Helper function to extract error message
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<any>;

    // Backend error response
    if (axiosError.response?.data) {
      const data = axiosError.response.data;

      // Handle ApiResponse format
      if (data.message) {
        return data.message;
      }

      // Handle errors array
      if (data.errors && Array.isArray(data.errors) && data.errors.length > 0) {
        return data.errors.join(', ');
      }

      // Handle string error
      if (typeof data === 'string') {
        return data;
      }
    }

    // Network error or connection refused
    if (axiosError.message === 'Network Error' || axiosError.code === 'ECONNREFUSED') {
      return 'Unable to connect to server. Please check your internet connection or ensure the API is running.';
    }

    // Timeout error
    if (axiosError.code === 'ECONNABORTED') {
      return 'Request timeout. Please try again.';
    }

    // Generic axios error
    return axiosError.message || 'An unexpected error occurred';
  }

  // Unknown error
  if (error instanceof Error) {
    return error.message;
  }

  return 'An unexpected error occurred';
}

export default apiClient;
