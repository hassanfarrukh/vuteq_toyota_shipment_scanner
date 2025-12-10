/**
 * Location Context
 * Author: Hassan
 * Date: 2025-12-01
 *
 * Manages the global location/plant setting for the application.
 * This location is determined by the Dock Monitor settings (single location selection).
 * It is displayed in the header and used throughout the app.
 */

'use client';

import { createContext, useContext, useState, useEffect, ReactNode, useCallback } from 'react';
import { getDockMonitorSettings, DockMonitorSettings } from '@/lib/api/settings';
import { clientLogger } from '@/lib/logger';
import { LOCATIONS } from '@/lib/constants';

interface LocationContextType {
  location: string;
  isLoading: boolean;
  error: string | null;
  refreshLocation: () => Promise<void>;
}

const LocationContext = createContext<LocationContextType | undefined>(undefined);

export function LocationProvider({ children }: { children: ReactNode }) {
  const [location, setLocation] = useState<string>('INDIANA'); // Default fallback
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Function to fetch and update location
  const refreshLocation = useCallback(async () => {
    try {
      setIsLoading(true);
      setError(null);

      clientLogger.info('LocationContext', 'Fetching global location from Dock Monitor settings');

      const result = await getDockMonitorSettings();

      if (result.success && result.data) {
        // Get the first selected location (should only be one)
        const selectedLocationId = result.data.selectedLocations?.[0];

        if (selectedLocationId) {
          // Convert location ID to location NAME (e.g., "loc-002" -> "MICHIGAN")
          const locationName = LOCATIONS.find(l => l.id === selectedLocationId)?.name || selectedLocationId;
          setLocation(locationName);
          clientLogger.info('LocationContext', 'Global location updated', {
            locationId: selectedLocationId,
            locationName
          });
        } else {
          // No location set, use default
          setLocation('NO LOCATION');
          clientLogger.warn('LocationContext', 'No location selected in Dock Monitor settings');
        }
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

  // Fetch location on mount
  useEffect(() => {
    refreshLocation();
  }, [refreshLocation]);

  const value = {
    location,
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
