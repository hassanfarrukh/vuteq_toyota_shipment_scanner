/**
 * Input Component
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-21 - Added VUTEQ colors (navy focus, light gray background)
 * Accessible input field with proper labeling and error states
 */

'use client';

import { InputHTMLAttributes, forwardRef } from 'react';
import { cn } from '@/lib/utils';

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string;
  error?: string;
  helperText?: string;
  fullWidth?: boolean;
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ label, error, helperText, fullWidth = false, className, id, ...props }, ref) => {
    const inputId = id || `input-${Math.random().toString(36).substring(2, 9)}`;

    return (
      <div className={cn('flex flex-col gap-1', fullWidth && 'w-full')}>
        {label && (
          <label
            htmlFor={inputId}
            className="text-sm font-medium"
            style={{ color: '#253262' }} // VUTEQ Navy for labels
          >
            {label}
            {props.required && <span className="text-error-500 ml-1">*</span>}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={cn(
            'px-4 py-3 border rounded-lg text-base',
            'focus:outline-none focus:ring-2 focus:border-transparent',
            'disabled:bg-gray-100 disabled:cursor-not-allowed',
            'min-h-touch',
            error
              ? 'border-error-500 focus:ring-error-500'
              : 'border-gray-300 focus:ring-vuteq-navy focus:border-vuteq-navy', // VUTEQ Navy focus
            fullWidth && 'w-full',
            className
          )}
          style={{
            backgroundColor: error ? undefined : '#F5F7F9', // VUTEQ Light Gray background
          }}
          aria-invalid={error ? 'true' : 'false'}
          aria-describedby={
            error
              ? `${inputId}-error`
              : helperText
              ? `${inputId}-helper`
              : undefined
          }
          {...props}
        />
        {error && (
          <p
            id={`${inputId}-error`}
            className="text-sm text-error-600"
            role="alert"
          >
            {error}
          </p>
        )}
        {helperText && !error && (
          <p id={`${inputId}-helper`} className="text-sm text-gray-500">
            {helperText}
          </p>
        )}
      </div>
    );
  }
);

Input.displayName = 'Input';

export default Input;
