/**
 * Utility Functions
 * Author: Hassan
 * Date: 2025-10-20
 * Common utility functions used throughout the application
 */

import { type ClassValue, clsx } from 'clsx';
import { VALIDATION_PATTERNS, TIMING } from './constants';
import type { ScanResult, ScanValidation } from '@/types';

/**
 * Combine class names using clsx
 */
export function cn(...inputs: ClassValue[]) {
  return clsx(inputs);
}

/**
 * Format date to local timezone
 */
export function formatDate(date: string | Date, format: 'short' | 'long' | 'time' = 'short'): string {
  const d = typeof date === 'string' ? new Date(date) : date;

  switch (format) {
    case 'short':
      return d.toLocaleDateString('en-US', { month: '2-digit', day: '2-digit', year: 'numeric' });
    case 'long':
      return d.toLocaleDateString('en-US', { month: 'long', day: 'numeric', year: 'numeric' });
    case 'time':
      return d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' });
    default:
      return d.toISOString();
  }
}

/**
 * Format date and time together
 */
export function formatDateTime(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return `${formatDate(d, 'short')} ${formatDate(d, 'time')}`;
}

/**
 * Check if a timestamp is within the duplicate scan window (24 hours)
 */
export function isDuplicateScan(lastScannedAt: string | null): boolean {
  if (!lastScannedAt) return false;

  const lastScan = new Date(lastScannedAt).getTime();
  const now = Date.now();
  const diff = now - lastScan;

  return diff < TIMING.DUPLICATE_SCAN_WINDOW;
}

/**
 * Validate scanned barcode format and determine type
 */
export function validateScan(scannedValue: string): ScanResult {
  const trimmed = scannedValue.trim().toUpperCase();

  // Check each pattern
  if (VALIDATION_PATTERNS.OWK_ORDER.test(trimmed)) {
    return {
      success: true,
      scannedValue: trimmed,
      validatedType: 'ORDER',
      data: { orderNumber: trimmed },
      error: null,
      timestamp: new Date().toISOString(),
    };
  }

  if (VALIDATION_PATTERNS.TOYOTA_LABEL.test(trimmed)) {
    return {
      success: true,
      scannedValue: trimmed,
      validatedType: 'SKID_MANIFEST',
      data: { toyotaLabel: trimmed },
      error: null,
      timestamp: new Date().toISOString(),
    };
  }

  if (VALIDATION_PATTERNS.PICKUP_ROUTE.test(trimmed)) {
    return {
      success: true,
      scannedValue: trimmed,
      validatedType: 'SKID_MANIFEST',
      data: { pickupRoute: trimmed },
      error: null,
      timestamp: new Date().toISOString(),
    };
  }

  if (VALIDATION_PATTERNS.TOYOTA_KANBAN.test(trimmed)) {
    return {
      success: true,
      scannedValue: trimmed,
      validatedType: 'TOYOTA_KANBAN',
      data: { kanbanNumber: trimmed },
      error: null,
      timestamp: new Date().toISOString(),
    };
  }

  if (VALIDATION_PATTERNS.INTERNAL_KANBAN.test(trimmed)) {
    return {
      success: true,
      scannedValue: trimmed,
      validatedType: 'INTERNAL_KANBAN',
      data: { kanbanNumber: trimmed },
      error: null,
      timestamp: new Date().toISOString(),
    };
  }

  if (VALIDATION_PATTERNS.SERIAL_NUMBER.test(trimmed)) {
    return {
      success: true,
      scannedValue: trimmed,
      validatedType: 'SERIAL',
      data: { serialNumber: trimmed },
      error: null,
      timestamp: new Date().toISOString(),
    };
  }

  if (VALIDATION_PATTERNS.DRIVER_CHECK_SHEET.test(trimmed)) {
    return {
      success: true,
      scannedValue: trimmed,
      validatedType: 'DRIVER_CHECK',
      data: { checkSheetNumber: trimmed },
      error: null,
      timestamp: new Date().toISOString(),
    };
  }

  // No pattern matched
  return {
    success: false,
    scannedValue: trimmed,
    validatedType: 'UNKNOWN',
    data: null,
    error: 'Invalid scan format. Please scan a valid barcode.',
    timestamp: new Date().toISOString(),
  };
}

/**
 * Debounce function for scan input
 */
export function debounce<T extends (...args: any[]) => void>(
  func: T,
  wait: number
): (...args: Parameters<T>) => void {
  let timeout: NodeJS.Timeout | null = null;

  return function executedFunction(...args: Parameters<T>) {
    const later = () => {
      timeout = null;
      func(...args);
    };

    if (timeout) {
      clearTimeout(timeout);
    }
    timeout = setTimeout(later, wait);
  };
}

/**
 * Trigger scanner beep (for mobile devices)
 */
export function triggerScanBeep(success: boolean = true): void {
  if (typeof window === 'undefined') return;

  // For Zebra scanners, use the native API if available
  const zebra = (window as unknown as { Zebra?: { beep: (type: string) => void } }).Zebra;
  if (zebra && typeof zebra.beep === 'function') {
    zebra.beep(success ? 'success' : 'error');
    return;
  }

  // Fallback to Web Audio API
  try {
    const audioContext = new (window.AudioContext || (window as unknown as { webkitAudioContext: typeof AudioContext }).webkitAudioContext)();
    const oscillator = audioContext.createOscillator();
    const gainNode = audioContext.createGain();

    oscillator.connect(gainNode);
    gainNode.connect(audioContext.destination);

    oscillator.frequency.value = success ? 800 : 400;
    oscillator.type = 'sine';

    gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
    gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + 0.1);

    oscillator.start(audioContext.currentTime);
    oscillator.stop(audioContext.currentTime + 0.1);
  } catch (error) {
    console.warn('Audio not available:', error);
  }
}

/**
 * Trigger scanner vibration (for mobile devices)
 */
export function triggerScanVibration(success: boolean = true): void {
  if (typeof window === 'undefined' || !navigator.vibrate) return;

  if (success) {
    navigator.vibrate(50); // Short vibration for success
  } else {
    navigator.vibrate([100, 50, 100]); // Pattern for error
  }
}

/**
 * Handle scan feedback (beep + vibration)
 */
export function handleScanFeedback(success: boolean): void {
  triggerScanBeep(success);
  triggerScanVibration(success);
}

/**
 * Sleep utility for delays
 */
export function sleep(ms: number): Promise<void> {
  return new Promise(resolve => setTimeout(resolve, ms));
}

/**
 * Get current location from environment or default
 */
export function getCurrentLocation(): string {
  if (typeof window === 'undefined') return 'INDIANA';
  return process.env.NEXT_PUBLIC_LOCATION || 'INDIANA';
}

/**
 * Check if current location requires serial scanning (BR-004)
 */
export function requiresSerialScanning(): boolean {
  const location = getCurrentLocation();
  return location === 'INDIANA';
}

/**
 * Generate unique session ID
 */
export function generateSessionId(): string {
  return `session-${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
}

/**
 * Safe JSON parse with fallback
 */
export function safeJsonParse<T>(json: string, fallback: T): T {
  try {
    return JSON.parse(json) as T;
  } catch {
    return fallback;
  }
}

/**
 * Calculate percentage
 */
export function calculatePercentage(value: number, total: number): number {
  if (total === 0) return 0;
  return Math.round((value / total) * 100);
}

/**
 * Truncate text with ellipsis
 */
export function truncate(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength - 3) + '...';
}

/**
 * Check if value is empty
 */
export function isEmpty(value: unknown): boolean {
  if (value === null || value === undefined) return true;
  if (typeof value === 'string') return value.trim().length === 0;
  if (Array.isArray(value)) return value.length === 0;
  if (typeof value === 'object') return Object.keys(value).length === 0;
  return false;
}
