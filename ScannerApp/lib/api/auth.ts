/**
 * Authentication API
 * Author: Hassan
 * Date: 2025-11-24
 * Updated: 2025-11-24 - Enhanced error handling with user-friendly messages
 * Updated: 2025-11-25 - Added comprehensive file logging for debugging authentication
 *
 * Handles user authentication with backend API
 */

import apiClient, { getErrorMessage } from './client';
import { clientLogger } from '@/lib/logger';

// Login request payload
interface LoginRequest {
  username: string;
  password: string;
}

// Login response from backend
interface LoginResponse {
  token: string;
  user: {
    userId: string;
    username: string;
    name: string;
    email?: string;
    role: string;
  };
}

// API Response wrapper - Backend returns flat structure
interface ApiResponse {
  success: boolean;
  message?: string;
  user?: {
    userId: string;
    username: string;
    name: string;
    email?: string;
    role: string;
  };
  token?: string;
  errors?: string[];
}

/**
 * Login user with username and password
 * @param username - User's username
 * @param password - User's password
 * @returns Promise with JWT token and user info
 */
export async function login(username: string, password: string): Promise<{
  success: boolean;
  token?: string;
  user?: {
    userId: string;
    username: string;
    name: string;
    email?: string;
    role: string;
  };
  error?: string;
}> {
  try {
    const payload: LoginRequest = { username, password };

    clientLogger.info('Auth API', '========== API LOGIN FUNCTION CALLED ==========');
    clientLogger.info('Auth API', 'Attempting login', {
      username,
      endpoint: '/api/Auth/login',
      baseURL: apiClient.defaults.baseURL,
      passwordProvided: !!password,
    });

    clientLogger.debug('Auth API', 'Making POST request to backend');
    const response = await apiClient.post<ApiResponse>(
      '/api/Auth/login',
      payload
    );

    clientLogger.info('Auth API', 'Backend response received', {
      status: response.status,
      success: response.data.success,
      hasToken: !!response.data.token,
      hasUser: !!response.data.user,
      message: response.data.message,
    });

    if (response.data.success && response.data.token && response.data.user) {
      const { token, user } = response.data;

      clientLogger.debug('Auth API', 'Login successful - storing JWT token', {
        tokenLength: token.length,
        userId: user.userId,
        username: user.username,
        role: user.role,
      });

      // Store JWT token in localStorage
      if (typeof window !== 'undefined') {
        localStorage.setItem('jwt_token', token);
        clientLogger.info('Auth API', 'JWT token stored in localStorage');
      }

      clientLogger.info('Auth API', '========== API LOGIN SUCCESS ==========');
      return {
        success: true,
        token,
        user,
      };
    }

    clientLogger.warn('Auth API', 'Login failed - no token or user in response', {
      message: response.data.message,
      errors: response.data.errors,
    });
    return {
      success: false,
      error: response.data.message || 'Login failed',
    };
  } catch (error) {
    clientLogger.error('Auth API', 'Login request failed with exception', {
      error: error instanceof Error ? error.message : String(error),
      errorStack: error instanceof Error ? error.stack : undefined,
    });

    const errorMsg = getErrorMessage(error);
    clientLogger.debug('Auth API', 'Extracted error message', { errorMsg });

    // Provide more helpful error messages for common issues
    let userFriendlyError = errorMsg;

    if (errorMsg.includes('Network Error') || errorMsg.includes('ECONNREFUSED')) {
      userFriendlyError = 'Unable to connect to the server. Please ensure the backend API is running on port 5000.';
      clientLogger.error('Auth API', 'Network connectivity issue detected');
    } else if (errorMsg.includes('timeout')) {
      userFriendlyError = 'Connection timeout. The server is taking too long to respond.';
      clientLogger.error('Auth API', 'Request timeout detected');
    } else if (errorMsg.includes('401') || errorMsg.includes('Unauthorized')) {
      userFriendlyError = 'Invalid username or password. Please try again.';
      clientLogger.error('Auth API', '401 Unauthorized - invalid credentials');
    } else if (errorMsg.includes('403') || errorMsg.includes('Forbidden')) {
      userFriendlyError = 'Access denied. You do not have permission to log in.';
      clientLogger.error('Auth API', '403 Forbidden - access denied');
    } else if (errorMsg.includes('500')) {
      userFriendlyError = 'Server error. Please contact the administrator or try again later.';
      clientLogger.error('Auth API', '500 Internal Server Error');
    }

    clientLogger.info('Auth API', '========== API LOGIN FAILED ==========', {
      userFriendlyError,
    });

    return {
      success: false,
      error: userFriendlyError,
    };
  }
}

/**
 * Logout user (clear token from localStorage)
 */
export function logout(): void {
  clientLogger.info('Auth API', 'Logout function called');
  if (typeof window !== 'undefined') {
    localStorage.removeItem('jwt_token');
    sessionStorage.removeItem('auth_user');
    clientLogger.info('Auth API', 'Cleared JWT token and user data from storage');
  }
}

/**
 * Check if user is authenticated (has valid token)
 */
export function isAuthenticated(): boolean {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('jwt_token');
    return !!token;
  }
  return false;
}

/**
 * Get stored JWT token
 */
export function getToken(): string | null {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('jwt_token');
  }
  return null;
}
