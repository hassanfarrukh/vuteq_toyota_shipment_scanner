/**
 * Users API
 * Author: Hassan
 * Date: 2025-11-24
 * Updated: 2025-11-25 - Added file logging for debugging user creation
 * Updated: 2025-11-25 - Modified getUsers to fetch ALL users (active + inactive) for admin management
 * Updated: 2025-11-25 - Updated CreateUserDto: username is now REQUIRED, name is OPTIONAL (for display name)
 *
 * CRUD operations for User management
 */

import apiClient, { getErrorMessage } from './client';
import { fileLogger } from '@/lib/logger';

// User interface matching backend DTO
export interface User {
  userId: string; // String identifier
  username: string;
  name: string; // Note: backend uses "name" not "userName"
  email?: string;
  role: string;
  menuLevel?: string;
  operation?: string;
  code?: string; // Single code field - can be Office or Warehouse code
  isSupervisor: boolean;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
  updatedAt?: string;
}

// Create User DTO - matches backend CreateUserRequest exactly
export interface CreateUserDto {
  username: string; // Required - Login username
  password: string; // Required - Plain password (backend will hash it)
  name?: string; // Optional - User display name (if not provided, uses username)
  nickName?: string; // Optional - User nickname
  email?: string; // Optional - User email address
  notificationName?: string; // Optional - Notification recipient name
  notificationEmail?: string; // Optional - Notification email address
  supervisor?: boolean; // Optional - Indicates if user is a supervisor
  menuLevel?: string; // Optional - Menu access level (default: Scanner)
  operation?: string; // Optional - User operation type
  code?: string; // Optional - Single code (Office or Warehouse)
}

// Update User DTO
export interface UpdateUserDto {
  username?: string;
  password?: string; // Plain password - backend will hash it
  name?: string;
  email?: string;
  role?: string;
  menuLevel?: string;
  operation?: string;
  code?: string; // Single code (Office or Warehouse)
  isSupervisor?: boolean;
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
 * Get all users (both active and inactive)
 */
export async function getUsers(): Promise<{
  success: boolean;
  data?: User[];
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<User[]>>('/api/v1/admin/users/all');

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch users',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Get user by ID
 */
export async function getUser(id: string): Promise<{
  success: boolean;
  data?: User;
  error?: string;
}> {
  try {
    const response = await apiClient.get<ApiResponse<User>>(
      `/api/v1/admin/users/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
        data: response.data.data,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to fetch user',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Create new user
 */
export async function createUser(data: CreateUserDto): Promise<{
  success: boolean;
  data?: User;
  error?: string;
}> {
  try {
    // Log the request data (redact password for security)
    fileLogger.info('API-Users', 'Sending POST request to create user', {
      endpoint: '/api/v1/admin/users',
      data: {
        ...data,
        password: '[REDACTED]'
      }
    });

    const response = await apiClient.post<ApiResponse<User>>(
      '/api/v1/admin/users',
      data
    );

    // Log the raw response
    fileLogger.info('API-Users', 'Received response from server', {
      status: response.status,
      statusText: response.statusText,
      success: response.data.success,
      message: response.data.message,
      errors: response.data.errors,
      hasData: !!response.data.data
    });

    if (response.data.success) {
      fileLogger.info('API-Users', 'User created successfully');
      return {
        success: true,
        data: response.data.data,
      };
    }

    // Log failed response
    fileLogger.warn('API-Users', 'User creation failed', {
      message: response.data.message,
      errors: response.data.errors
    });

    return {
      success: false,
      error: response.data.message || 'Failed to create user',
    };
  } catch (error) {
    // Log the error details
    const errorMessage = getErrorMessage(error);

    fileLogger.error('API-Users', 'Exception during user creation', {
      error: errorMessage,
      errorType: error instanceof Error ? error.constructor.name : typeof error,
      // Log axios error details if available
      ...(error && typeof error === 'object' && 'response' in error ? {
        responseStatus: (error as any).response?.status,
        responseData: (error as any).response?.data,
        responseHeaders: (error as any).response?.headers,
      } : {}),
      ...(error && typeof error === 'object' && 'request' in error ? {
        hasRequest: true,
        requestUrl: (error as any).config?.url,
        requestMethod: (error as any).config?.method,
      } : {}),
    });

    return {
      success: false,
      error: errorMessage,
    };
  }
}

/**
 * Update existing user
 */
export async function updateUser(
  id: string,
  data: UpdateUserDto
): Promise<{
  success: boolean;
  data?: User;
  error?: string;
}> {
  try {
    const response = await apiClient.put<ApiResponse<User>>(
      `/api/v1/admin/users/${id}`,
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
      error: response.data.message || 'Failed to update user',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}

/**
 * Delete user
 */
export async function deleteUser(id: string): Promise<{
  success: boolean;
  error?: string;
}> {
  try {
    const response = await apiClient.delete<ApiResponse<null>>(
      `/api/v1/admin/users/${id}`
    );

    if (response.data.success) {
      return {
        success: true,
      };
    }

    return {
      success: false,
      error: response.data.message || 'Failed to delete user',
    };
  } catch (error) {
    return {
      success: false,
      error: getErrorMessage(error),
    };
  }
}
