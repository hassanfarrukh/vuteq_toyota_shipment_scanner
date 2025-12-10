/**
 * Scanner Input Component
 * Author: Hassan
 * Date: 2025-10-20
 * Specialized input for barcode scanning with validation feedback
 */

'use client';

import { useState, useEffect, useRef } from 'react';
import { cn } from '@/lib/utils';
import { validateScan, handleScanFeedback, debounce } from '@/lib/utils';
import { TIMING } from '@/lib/constants';
import type { ScanResult } from '@/types';

interface ScannerProps {
  onScan: (result: ScanResult) => void;
  expectedType?: ScanResult['validatedType'];
  placeholder?: string;
  label?: string;
  autoFocus?: boolean;
  disabled?: boolean;
}

export default function Scanner({
  onScan,
  expectedType,
  placeholder = 'Scan or enter barcode...',
  label,
  autoFocus = true,
  disabled = false,
}: ScannerProps) {
  const [value, setValue] = useState('');
  const [scanning, setScanning] = useState(false);
  const [lastScanResult, setLastScanResult] = useState<ScanResult | null>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  // Auto-focus on mount and after each scan
  useEffect(() => {
    if (autoFocus && !disabled) {
      inputRef.current?.focus();
    }
  }, [autoFocus, disabled, lastScanResult]);

  // Debounced scan handler
  const handleScan = debounce((scannedValue: string) => {
    if (!scannedValue.trim()) return;

    setScanning(true);
    const result = validateScan(scannedValue);

    // Check if scan matches expected type
    if (expectedType && result.validatedType !== expectedType && result.success) {
      const modifiedResult: ScanResult = {
        ...result,
        success: false,
        error: `Expected ${expectedType} scan, but got ${result.validatedType}`,
      };
      setLastScanResult(modifiedResult);
      handleScanFeedback(false);
      onScan(modifiedResult);
    } else {
      setLastScanResult(result);
      handleScanFeedback(result.success);
      onScan(result);
    }

    // Clear input and reset state
    setValue('');
    setScanning(false);
  }, TIMING.SCAN_DEBOUNCE);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setValue(e.target.value);
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter' && value.trim()) {
      e.preventDefault();
      handleScan(value);
    }
  };

  // Clear last result after 3 seconds
  useEffect(() => {
    if (lastScanResult) {
      const timer = setTimeout(() => setLastScanResult(null), 3000);
      return () => clearTimeout(timer);
    }
  }, [lastScanResult]);

  return (
    <div className="w-full space-y-2">
      {label && (
        <label className="block text-sm font-medium text-gray-700">
          {label}
        </label>
      )}

      <div className="relative">
        <input
          ref={inputRef}
          type="text"
          value={value}
          onChange={handleChange}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          disabled={disabled || scanning}
          className={cn(
            'w-full px-4 py-4 pr-12 border-2 rounded-lg text-base font-mono',
            'focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent',
            'disabled:bg-gray-100 disabled:cursor-not-allowed',
            'min-h-touch',
            lastScanResult?.success === false
              ? 'border-error-500'
              : lastScanResult?.success === true
              ? 'border-success-500'
              : 'border-gray-300'
          )}
          aria-label={label || 'Scanner input'}
          autoComplete="off"
          autoCorrect="off"
          autoCapitalize="off"
          spellCheck="false"
        />

        {/* Scan indicator icon */}
        <div className="absolute right-3 top-1/2 -translate-y-1/2">
          {scanning ? (
            <svg
              className="animate-spin h-6 w-6 text-primary-600"
              xmlns="http://www.w3.org/2000/svg"
              fill="none"
              viewBox="0 0 24 24"
              aria-hidden="true"
            >
              <circle
                className="opacity-25"
                cx="12"
                cy="12"
                r="10"
                stroke="currentColor"
                strokeWidth="4"
              />
              <path
                className="opacity-75"
                fill="currentColor"
                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
              />
            </svg>
          ) : lastScanResult?.success === true ? (
            <svg
              className="h-6 w-6 text-success-600"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden="true"
            >
              <path
                fillRule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                clipRule="evenodd"
              />
            </svg>
          ) : lastScanResult?.success === false ? (
            <svg
              className="h-6 w-6 text-error-600"
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 20 20"
              fill="currentColor"
              aria-hidden="true"
            >
              <path
                fillRule="evenodd"
                d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.28 7.22a.75.75 0 00-1.06 1.06L8.94 10l-1.72 1.72a.75.75 0 101.06 1.06L10 11.06l1.72 1.72a.75.75 0 101.06-1.06L11.06 10l1.72-1.72a.75.75 0 00-1.06-1.06L10 8.94 8.28 7.22z"
                clipRule="evenodd"
              />
            </svg>
          ) : (
            <i className="fa-light fa-qrcode" style={{
              fontSize: '20px',
              color: '#253262'
            } as React.CSSProperties} aria-hidden="true"></i>
          )}
        </div>
      </div>

      {/* Feedback message */}
      {lastScanResult && (
        <div
          className={cn(
            'p-3 rounded-lg text-sm',
            lastScanResult.success
              ? 'bg-success-50 text-success-800'
              : 'bg-error-50 text-error-800'
          )}
          role={lastScanResult.success ? 'status' : 'alert'}
        >
          {lastScanResult.success ? (
            <div className="flex items-center gap-2">
              <span className="font-medium">Valid {lastScanResult.validatedType}</span>
              <span className="text-xs opacity-75">{lastScanResult.scannedValue}</span>
            </div>
          ) : (
            <div className="font-medium">{lastScanResult.error}</div>
          )}
        </div>
      )}

      {/* Expected type hint */}
      {expectedType && !lastScanResult && (
        <p className="text-sm text-gray-500">
          Expected: <span className="font-medium">{expectedType}</span>
        </p>
      )}
    </div>
  );
}
