/**
 * Card Component
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-21 - Added red-to-blue gradient border for dashboard tiles
 * Updated: 2025-10-21 - Made gradient borders darker and smaller for subtle effect (v2)
 * Updated: 2025-10-21 - Added VUTEQ off-white backgrounds and branded shadows
 * Updated: 2025-10-22 - Fixed mobile gradient border bleeding issue
 * Container card component for consistent layout
 */

import { ReactNode } from 'react';
import { cn } from '@/lib/utils';

interface CardProps {
  children: ReactNode;
  className?: string;
  padding?: 'none' | 'sm' | 'md' | 'lg';
  onClick?: () => void;
  gradientBorder?: boolean; // Enable red-to-blue gradient border
  vuteqStyle?: boolean; // Use VUTEQ off-white background (default true)
}

export default function Card({
  children,
  className,
  padding = 'md',
  onClick,
  gradientBorder = false,
  vuteqStyle = true, // Default to VUTEQ styling
}: CardProps) {
  const paddingStyles = {
    none: 'p-0',
    sm: 'p-3',
    md: 'p-4',
    lg: 'p-6',
  };

  // VUTEQ off-white background color
  const backgroundColor = vuteqStyle ? '#FCFCFC' : 'white';

  // For gradient border: use wrapper with gradient background and inner container
  // Subtle gradient with darker shades (red-800 to blue-800) and 1px border width
  // Mobile fix: Ensure proper border rendering without bleeding
  if (gradientBorder) {
    return (
      <div
        className={cn(
          'p-[1.5px] rounded-lg bg-gradient-to-r from-red-800 to-blue-800',
          'overflow-hidden', // Prevent gradient bleeding on mobile
          onClick && 'cursor-pointer transition-all duration-200 active:scale-98',
          onClick && 'hover:shadow-vuteq-card-hover',
          className
        )}
        onClick={onClick}
        role={onClick ? 'button' : undefined}
        tabIndex={onClick ? 0 : undefined}
        onKeyDown={
          onClick
            ? (e) => {
                if (e.key === 'Enter' || e.key === ' ') {
                  e.preventDefault();
                  onClick();
                }
              }
            : undefined
        }
      >
        <div
          className={cn(
            'rounded-[calc(0.5rem-1.5px)] h-full w-full',
            'relative', // Ensure proper stacking context
            paddingStyles[padding]
          )}
          style={{
            backgroundColor,
            // Add subtle white overlay to prevent gradient show-through
            backgroundClip: 'padding-box'
          }}
        >
          {children}
        </div>
      </div>
    );
  }

  // Standard card with VUTEQ-tinted shadow
  return (
    <div
      className={cn(
        'rounded-lg border border-gray-200',
        onClick && 'cursor-pointer transition-shadow active:scale-98',
        onClick ? 'shadow-vuteq-card hover:shadow-vuteq-card-hover' : 'shadow-vuteq-card',
        paddingStyles[padding],
        className
      )}
      style={{ backgroundColor }}
      onClick={onClick}
      role={onClick ? 'button' : undefined}
      tabIndex={onClick ? 0 : undefined}
      onKeyDown={
        onClick
          ? (e) => {
              if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                onClick();
              }
            }
          : undefined
      }
    >
      {children}
    </div>
  );
}

export function CardHeader({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <div className={cn('mb-4', className)}>
      {children}
    </div>
  );
}

export function CardTitle({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  return (
    <h3
      className={cn('text-lg font-semibold', className)}
      style={{ color: '#253262' }} // VUTEQ Navy for headings
    >
      {children}
    </h3>
  );
}

export function CardContent({
  children,
  className,
}: {
  children: ReactNode;
  className?: string;
}) {
  return <div className={cn(className)}>{children}</div>;
}
