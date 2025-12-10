/**
 * Stepper Component - Professional step indicator for multi-step workflows
 * Author: Hassan
 * Date: 2025-10-21
 * Updated: 2025-10-22 - Fixed mobile responsive logic for step labels
 * Updated: 2025-10-22 - Changed to HORIZONTAL layout on mobile for compact display
 * Updated: 2025-10-22 - Mobile-only: Removed ALL text labels and step counter (circles only)
 * Updated: 2025-10-29 - Changed active step styling: Red circle (#D2312E) with blue number (#253262) (Hassan)
 * Updated: 2025-10-29 - Changed active step styling: Blue circle (#253262) with white number (#FFFFFF) (Hassan)
 *
 * Features:
 * - Clean, modern design with circular step indicators
 * - Smooth connecting lines between steps
 * - Mobile (<640px): Circles and lines ONLY (no text labels, no step counter)
 * - Desktop (>=640px): Full display with labels, descriptions, and step counter
 * - Active, completed, and pending states
 * - Fully responsive and mobile-optimized
 * - Accessible with proper ARIA attributes
 */

'use client';

import React from 'react';

export interface StepConfig {
  label: string;
  description?: string;
}

export interface StepperProps {
  steps: StepConfig[];
  currentStep: number; // 1-indexed
  orientation?: 'horizontal' | 'vertical';
  className?: string;
}

export default function Stepper({
  steps,
  currentStep,
  orientation = 'horizontal',
  className = '',
}: StepperProps) {
  const isHorizontal = orientation === 'horizontal';

  return (
    <div
      className={`w-full ${className}`}
      role="navigation"
      aria-label="Progress indicator"
    >
      <div
        className={`flex ${
          isHorizontal ? 'flex-row items-center' : 'flex-col items-start'
        } w-full`}
      >
        {steps.map((step, index) => {
          const stepNumber = index + 1;
          const isCompleted = stepNumber < currentStep;
          const isActive = stepNumber === currentStep;
          const isPending = stepNumber > currentStep;
          const isLast = index === steps.length - 1;

          return (
            <React.Fragment key={stepNumber}>
              {/* Step Item */}
              <div
                className={`flex ${
                  isHorizontal ? 'flex-col items-center' : 'flex-row items-start'
                } ${isHorizontal ? 'flex-1' : 'w-full'}`}
              >
                {/* Step Circle and Label Container */}
                <div
                  className={`flex ${
                    isHorizontal ? 'flex-col items-center' : 'flex-row items-center gap-4'
                  } ${isHorizontal ? 'w-full' : ''}`}
                >
                  {/* Step Circle */}
                  <div
                    className={`
                      relative z-10 flex items-center justify-center
                      w-10 h-10 rounded-full font-bold text-sm
                      transition-all duration-300 ease-in-out
                      ${
                        isCompleted
                          ? 'bg-success-600 text-white shadow-lg shadow-success-200'
                          : isActive
                          ? 'shadow-lg shadow-blue-200 ring-4 ring-blue-100'
                          : 'bg-gray-200 text-gray-500'
                      }
                      ${isActive ? 'scale-110' : 'scale-100'}
                      ${isActive ? 'bg-blue-900' : ''}
                    `}
                    style={isActive ? { backgroundColor: '#253262', color: '#FFFFFF' } : undefined}
                    aria-current={isActive ? 'step' : undefined}
                    aria-label={`Step ${stepNumber}: ${step.label}`}
                  >
                    {isCompleted ? (
                      <svg
                        className="w-6 h-6"
                        fill="none"
                        stroke="currentColor"
                        viewBox="0 0 24 24"
                        aria-hidden="true"
                      >
                        <path
                          strokeLinecap="round"
                          strokeLinejoin="round"
                          strokeWidth={3}
                          d="M5 13l4 4L19 7"
                        />
                      </svg>
                    ) : (
                      stepNumber
                    )}
                  </div>

                  {/* Step Label - Hidden on mobile (<640px), visible on desktop (>=640px) */}
                  <div
                    className={`
                      ${isHorizontal ? 'mt-3' : 'ml-0'}
                      text-center ${isHorizontal ? 'w-full' : 'flex-1'}
                      hidden sm:block
                    `}
                  >
                    <div>
                      <div
                        className={`
                          text-sm font-semibold transition-colors duration-200
                          ${
                            isCompleted || isActive
                              ? 'text-gray-900'
                              : 'text-gray-500'
                          }
                        `}
                      >
                        {step.label}
                      </div>
                      {step.description && (
                        <div className="text-xs text-gray-500 mt-1">
                          {step.description}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              {/* Connecting Line */}
              {!isLast && (
                <div
                  className={`
                    ${isHorizontal ? 'flex-1 h-0.5' : 'w-0.5 h-12 ml-5'}
                    ${isHorizontal ? 'mx-2' : 'my-2'}
                    transition-all duration-300 ease-in-out
                    ${
                      stepNumber < currentStep
                        ? 'bg-success-600'
                        : 'bg-gray-300'
                    }
                    ${isHorizontal ? '' : ''}
                  `}
                  aria-hidden="true"
                />
              )}
            </React.Fragment>
          );
        })}
      </div>

      {/* Progress Text - Hidden on mobile (<640px), visible on desktop (>=640px) */}
      <div className="text-sm text-gray-600 text-center mt-4 hidden sm:block">
        Step {currentStep} of {steps.length}
      </div>
    </div>
  );
}

/**
 * Mobile-optimized Stepper that uses horizontal layout on all screen sizes
 * Mobile: Compact horizontal with smaller circles and minimal spacing
 * Desktop: Full horizontal with all labels visible
 */
export function ResponsiveStepper({
  steps,
  currentStep,
  className = '',
}: Omit<StepperProps, 'orientation'>) {
  return (
    <Stepper
      steps={steps}
      currentStep={currentStep}
      orientation="horizontal"
      className={className}
    />
  );
}
