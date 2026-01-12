/**
 * Authentication Context
 * Author: Hassan
 * Date: 2025-10-21
 * Updated: 2025-11-24 - Integrated real backend authentication API
 * Updated: 2025-11-24 - Fixed import issue: Changed from namespace import to direct function import
 * Updated: 2025-11-24 - Enhanced error handling and debugging for login failures
 * Updated: 2025-11-24 - Fixed login redirect loop by adding isLoggingIn state to prevent race condition
 * Updated: 2025-11-24 - CRITICAL FIX: Removed isLoggingIn from useEffect dependencies to prevent page refresh on login error
 * Updated: 2025-11-25 - Added comprehensive file logging for debugging login flow
 * Updated: 2025-01-12 - Fixed role mapping to give ADMIN priority over SUPERVISOR flag
 * Manages user authentication state and provides login/logout functionality
 */

'use client';

import { createContext, useContext, useState, useEffect, ReactNode } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { login as apiLogin, logout as apiLogout } from '@/lib/api/auth';
import { clientLogger } from '@/lib/logger';

export type UserRole = 'ADMIN' | 'SUPERVISOR' | 'OPERATOR';

export interface AuthUser {
  id: string;
  username: string;
  name: string;
  role: UserRole;
  locationId: string;
}

interface AuthContextType {
  user: AuthUser | null;
  isAuthenticated: boolean;
  login: (username: string, password: string) => Promise<{ success: boolean; error?: string }>;
  logout: () => void;
  loading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Helper function to map backend role to frontend UserRole
// ADMIN takes priority over everything, then supervisor flag/role
function mapBackendRoleToUserRole(backendRole: string, supervisor?: boolean): UserRole {
  const roleUpper = backendRole.toUpperCase();

  // ADMIN takes priority over everything
  if (roleUpper.includes('ADMIN')) return 'ADMIN';

  // If supervisor flag is true OR role contains SUPERVISOR
  if (supervisor === true || roleUpper.includes('SUPERVISOR')) return 'SUPERVISOR';

  return 'OPERATOR';
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [loading, setLoading] = useState(true);
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const router = useRouter();
  const pathname = usePathname();

  // Load user from session storage on mount
  useEffect(() => {
    clientLogger.info('AuthContext', 'Initializing AuthContext - checking for stored user');

    const storedUser = sessionStorage.getItem('auth_user');
    const token = localStorage.getItem('jwt_token');

    clientLogger.debug('AuthContext', 'Storage check', {
      hasStoredUser: !!storedUser,
      hasToken: !!token,
    });

    if (storedUser && token) {
      try {
        const parsedUser = JSON.parse(storedUser);
        setUser(parsedUser);
        clientLogger.info('AuthContext', 'Restored user from storage', {
          userId: parsedUser.id,
          username: parsedUser.username,
          role: parsedUser.role,
        });
      } catch (error) {
        clientLogger.error('AuthContext', 'Failed to parse stored user', { error });
        sessionStorage.removeItem('auth_user');
        localStorage.removeItem('jwt_token');
      }
    } else {
      clientLogger.info('AuthContext', 'No stored user found - user needs to login');
    }
    setLoading(false);
  }, []);

  // Redirect to login if not authenticated (except on login page)
  useEffect(() => {
    // Only redirect if:
    // 1. Not loading initial state
    // 2. No user is logged in
    // 3. Not currently in the login process
    // 4. Not already on the login page
    // 5. User is not on a public page
    if (!loading && !user && !isLoggingIn && pathname !== '/login') {
      clientLogger.warn('AuthContext', 'Redirecting to login - user not authenticated', {
        pathname,
        loading,
        hasUser: !!user,
        isLoggingIn,
      });
      router.push('/login');
    }
  }, [loading, user, pathname, router]); // REMOVED isLoggingIn from dependencies to prevent refresh on login failure

  const login = async (username: string, password: string): Promise<{ success: boolean; error?: string }> => {
    try {
      // Set logging in state to prevent redirect during login
      setIsLoggingIn(true);

      clientLogger.info('AuthContext', '========== LOGIN PROCESS STARTED ==========');
      clientLogger.info('AuthContext', 'Starting login process', {
        username,
        apiUrl: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000',
        timestamp: new Date().toISOString(),
      });

      // Call real backend API
      clientLogger.debug('AuthContext', 'Calling backend API login function');
      const result = await apiLogin(username, password);

      clientLogger.info('AuthContext', 'Backend API login response received', {
        success: result.success,
        hasToken: !!result.token,
        hasUser: !!result.user,
        error: result.error,
      });

      if (!result.success || !result.user) {
        clientLogger.warn('AuthContext', 'Login failed - invalid credentials or missing user data', {
          error: result.error,
          hasUser: !!result.user,
        });
        return {
          success: false,
          error: result.error || 'Login failed',
        };
      }

      clientLogger.debug('AuthContext', 'Mapping backend user to AuthUser format', {
        backendUserId: result.user.id,
        backendUsername: result.user.username,
        backendRole: result.user.role,
        supervisor: result.user.supervisor,
      });

      // Map backend user to AuthUser format
      const authUser: AuthUser = {
        id: result.user.id,
        username: result.user.username,
        name: result.user.name,
        role: mapBackendRoleToUserRole(result.user.role, result.user.supervisor),
        locationId: 'loc-001', // Default location for now
      };

      clientLogger.info('AuthContext', 'Mapped user to AuthUser format', {
        userId: authUser.id,
        username: authUser.username,
        role: authUser.role,
        locationId: authUser.locationId,
      });

      // Store user in session storage
      clientLogger.debug('AuthContext', 'Storing user in sessionStorage');
      sessionStorage.setItem('auth_user', JSON.stringify(authUser));

      clientLogger.debug('AuthContext', 'Updating React state with user');
      setUser(authUser);

      clientLogger.info('AuthContext', 'Login successful! Initiating redirect to dashboard', {
        userId: authUser.id,
        username: authUser.username,
        role: authUser.role,
        redirectTo: '/',
      });

      // Redirect to dashboard
      clientLogger.debug('AuthContext', 'Calling router.push("/")');
      router.push('/');

      clientLogger.info('AuthContext', '========== LOGIN PROCESS COMPLETED SUCCESSFULLY ==========');
      return { success: true };
    } catch (error) {
      clientLogger.error('AuthContext', 'Unexpected error during login process', {
        error: error instanceof Error ? error.message : String(error),
        errorStack: error instanceof Error ? error.stack : undefined,
      });

      // Extract meaningful error message
      let errorMessage = 'An unexpected error occurred. Please try again.';

      if (error instanceof Error) {
        errorMessage = error.message;
      }

      // If error contains network issues, provide better guidance
      if (errorMessage.includes('Network Error') || errorMessage.includes('ECONNREFUSED')) {
        errorMessage = 'Cannot connect to the server. Please ensure the backend API is running on port 5000.';
        clientLogger.error('AuthContext', 'Network connection error detected', {
          originalError: errorMessage,
        });
      }

      clientLogger.info('AuthContext', '========== LOGIN PROCESS FAILED ==========');
      return {
        success: false,
        error: errorMessage,
      };
    } finally {
      // Always reset logging in state
      clientLogger.debug('AuthContext', 'Resetting isLoggingIn state to false');
      setIsLoggingIn(false);
    }
  };

  const logout = () => {
    clientLogger.info('AuthContext', 'Logout initiated', {
      userId: user?.id,
      username: user?.username,
    });

    // Call apiLogout to clear tokens
    apiLogout();
    setUser(null);

    clientLogger.info('AuthContext', 'Logout completed - redirecting to login');
    router.push('/login');
  };

  const value = {
    user,
    isAuthenticated: !!user,
    login,
    logout,
    loading,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
