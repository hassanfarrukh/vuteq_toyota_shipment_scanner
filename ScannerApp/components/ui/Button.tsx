/**
 * Button Component
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-21 - Added VUTEQ brand colors (Red primary, Navy secondary)
 * Updated: 2025-10-27 - Changed primary color to Navy Blue, Red only for error/cancel actions
 * Updated: 2025-10-29 - Added new standardized button variants (Hassan)
 * Accessible, touch-optimized button component for mobile scanners
 */

'use client';

import { ButtonHTMLAttributes, ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'tertiary' | 'success' | 'success-light' | 'warning' | 'error' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  fullWidth?: boolean;
  loading?: boolean;
  children: ReactNode;
}

export default function Button({
  variant = 'primary',
  size = 'md',
  fullWidth = false,
  loading = false,
  disabled,
  className,
  children,
  ...props
}: ButtonProps) {
  const baseStyles = 'inline-flex items-center justify-center font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed min-h-touch min-w-touch';

  const variantStyles = {
    // PRIMARY: VUTEQ Navy Blue (#253262) - Navigation buttons (Home, Return to Dashboard, Add, Continue, Start Over)
    primary: 'bg-vuteq-navy text-white hover:bg-vuteq-navy-dark focus:ring-vuteq-navy/50 active:bg-vuteq-navy-darker',

    // SECONDARY: Neutral gray - Alternative actions
    secondary: 'bg-gray-200 text-gray-900 hover:bg-gray-300 focus:ring-gray-500 active:bg-gray-400',

    // TERTIARY: Even lighter - Low emphasis actions
    tertiary: 'bg-gray-100 text-gray-700 hover:bg-gray-200 focus:ring-gray-400 active:bg-gray-300',

    // SUCCESS: Dark Green (#10B981) - Complete, Submit actions
    success: 'bg-success-600 text-white hover:bg-success-700 focus:ring-success-500 active:bg-success-800',

    // SUCCESS-LIGHT: Vibrant Green (#22C55E) - Continue, Proceed, Save actions
    'success-light': 'bg-[#22C55E] text-white hover:bg-[#16A34A] focus:ring-[#22C55E]/50 active:bg-[#15803D]',

    // WARNING: Yellow - Special states
    warning: 'bg-warning-500 text-white hover:bg-warning-600 focus:ring-warning-500 active:bg-warning-700',

    // ERROR: Red (#EF4444) - Cancel, Start Fresh, Abort actions
    error: 'bg-vuteq-red text-white hover:bg-vuteq-red-dark focus:ring-vuteq-red/50 active:bg-vuteq-red-darker',

    // GHOST: Transparent - Low emphasis
    ghost: 'bg-transparent text-gray-700 hover:bg-gray-100 focus:ring-gray-500 active:bg-gray-200',
  };

  const sizeStyles = {
    sm: 'px-3 py-2 text-sm',
    md: 'px-4 py-3 text-base',
    lg: 'px-6 py-4 text-lg',
  };

  return (
    <button
      className={cn(
        baseStyles,
        variantStyles[variant],
        sizeStyles[size],
        fullWidth && 'w-full',
        className
      )}
      disabled={disabled || loading}
      {...props}
    >
      {loading && (
        <svg
          className="animate-spin -ml-1 mr-2 h-4 w-4"
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
      )}
      {children}
    </button>
  );
}
