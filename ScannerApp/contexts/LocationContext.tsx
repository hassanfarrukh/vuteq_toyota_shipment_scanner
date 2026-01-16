/**
 * Location Context
 * Author: Hassan
 * Date: 2025-12-01
 * Updated: 2026-01-03 - Changed to use getSiteSettings API instead of getDockMonitorSettings (Hassan)
 * Updated: 2026-01-16 - Made auth-aware, clears state on logout (Hassan)
 *
 * Manages the global location/plant setting for the application.
 * This location is determined by the Site Settings (plantLocation).
 * It is displayed in the header and used throughout the app.
 */

'use client';

import { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';
import { getSiteSettings } from '@/lib/api/siteSettings';
import { clientLogger } from '@/lib/logger';
import { useAuth } from './AuthContext';

interface LocationContextType {
  location: string;
  enablePreShipmentScan: boolean | null;
  isLoading: boolean;
  error: string | null;
  refreshLocation: () => Promise<void>;
}

const LocationContext = createContext<LocationContextType | undefined>(undefined);

export function LocationProvider({ children }: { children: ReactNode }) {
  const [location, setLocation] = useState<string>('VOSC'); // Default fallback - Issue #5 update
  const [enablePreShipmentScan, setEnablePreShipmentScan] = useState<boolean | null>(null); // null = not loaded
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Get auth state to watch for login/logout
  const auth = useAuth();

  // Function to fetch and update location
  const refreshLocation = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      clientLogger.info('LocationContext', 'Fetching global location from Site Settings');

      const result = await getSiteSettings();

      if (result.success && result.data) {
        const plantLocation = result.data.plantLocation;

        if (plantLocation) {
          // plantLocation is already the location name string (e.g., "MICHIGAN")
          setLocation(plantLocation);
          clientLogger.info('LocationContext', 'Global location updated', {
            plantLocation
          });
        } else {
          // No location set, use default
          setLocation('NO LOCATION');
          clientLogger.warn('LocationContext', 'No plant location set in Site Settings');
        }

        // Also set the PreShipment scan flag from the same API call
        setEnablePreShipmentScan(result.data.enablePreShipmentScan);
        clientLogger.info('LocationContext', 'PreShipment scan flag updated', {
          enablePreShipmentScan: result.data.enablePreShipmentScan
        });
      } else {
        setError(result.error || 'Failed to fetch location');
        clientLogger.error('LocationContext', 'Failed to fetch location', { error: result.error });
      }
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Unknown error';
      setError(errorMsg);
      clientLogger.error('LocationContext', 'Error fetching location', { error: errorMsg });
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Auth-aware effect: Fetch data when user logs in, clear when user logs out
  useEffect(() => {
    // Handle SSR - auth might not be available
    if (typeof window === 'undefined') return;

    const user = auth?.user;

    // User logged out or session expired - clear all state
    if (!user) {
      clientLogger.info('LocationContext', 'User logged out - clearing location state');
      setLocation('VOSC'); // Reset to default
      setEnablePreShipmentScan(null); // Reset to not-loaded state
      setError(null);
      setIsLoading(false);
      return;
    }

    // User is authenticated AND data hasn't been loaded yet - fetch fresh data
    if (user && enablePreShipmentScan === null) {
      clientLogger.info('LocationContext', 'User authenticated - fetching fresh location data');
      refreshLocation();
    }
  }, [auth?.user, enablePreShipmentScan, refreshLocation]);

  const value = {
    location,
    enablePreShipmentScan,
    isLoading,
    error,
    refreshLocation,
  };

  return <LocationContext.Provider value={value}>{children}</LocationContext.Provider>;
}

export function useLocation() {
  const context = useContext(LocationContext);
  if (context === undefined) {
    throw new Error('useLocation must be used within a LocationProvider');
  }
  return context;
}
