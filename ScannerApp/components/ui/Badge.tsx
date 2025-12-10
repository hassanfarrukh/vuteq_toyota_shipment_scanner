/**
 * Badge Component
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-21 - Added VUTEQ-branded badge variants (navy, red, light)
 * Small label component for status indicators
 */

import { ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface BadgeProps {
  children: ReactNode;
  variant?: 'default' | 'success' | 'warning' | 'error' | 'info' | 'vuteq-navy' | 'vuteq-red' | 'vuteq-light';
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export default function Badge({
  children,
  variant = 'default',
  size = 'md',
  className,
}: BadgeProps) {
  const variantStyles = {
    default: 'bg-gray-100 text-gray-800',
    success: 'bg-success-100 text-success-800',
    warning: 'bg-warning-100 text-warning-800',
    error: 'bg-error-100 text-error-800',
    info: 'bg-primary-100 text-primary-800',

    // VUTEQ-Branded Variants
    'vuteq-navy': 'bg-vuteq-navy text-white border border-vuteq-navy',
    'vuteq-red': 'bg-vuteq-red text-white border border-vuteq-red',
    'vuteq-light': 'bg-vuteq-light-gray border border-vuteq-navy/20',
  };

  const sizeStyles = {
    sm: 'px-2 py-0.5 text-xs',
    md: 'px-2.5 py-1 text-sm',
    lg: 'px-3 py-1.5 text-base',
  };

  return (
    <span
      className={cn(
        'inline-flex items-center font-medium rounded-full',
        variantStyles[variant],
        sizeStyles[size],
        className
      )}
      style={
        variant === 'vuteq-light'
          ? { color: '#253262' } // VUTEQ Navy text for light variant
          : undefined
      }
    >
      {children}
    </span>
  );
}
