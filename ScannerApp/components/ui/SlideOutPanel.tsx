/**
 * SlideOutPanel Component
 * Author: Hassan
 * Date: 2025-10-27
 * Updated: 2025-10-29 - Replaced Lucide icon with Font Awesome, added smooth animations
 * Updated: 2025-11-05 - Added VUTEQ Navy background to header with white text
 * Updated: 2025-11-05 - Added smooth closing animation with state management
 *
 * Azure-style right-side slide-out panel for forms with smooth open/close animations
 */

'use client';

import { useEffect, useState } from 'react';

interface SlideOutPanelProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  width?: 'sm' | 'md' | 'lg' | 'xl';
}

const ANIMATION_DURATION = 300; // milliseconds

export default function SlideOutPanel({
  isOpen,
  onClose,
  title,
  children,
  width = 'lg'
}: SlideOutPanelProps) {
  // Track closing animation state
  const [isClosing, setIsClosing] = useState(false);

  // Prevent body scroll when panel is open
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => {
      document.body.style.overflow = 'unset';
    };
  }, [isOpen]);

  // Handle panel closing with animation
  const handleClose = () => {
    setIsClosing(true);
    // Wait for animation to complete before calling onClose
    setTimeout(() => {
      onClose();
      setIsClosing(false);
    }, ANIMATION_DURATION);
  };

  // ESC key to close panel
  useEffect(() => {
    const handleEscape = (e: KeyboardEvent) => {
      if (e.key === 'Escape' && isOpen && !isClosing) {
        handleClose();
      }
    };

    document.addEventListener('keydown', handleEscape);
    return () => document.removeEventListener('keydown', handleEscape);
  }, [isOpen, isClosing]);

  // Width classes
  const widthClasses = {
    sm: 'max-w-md',
    md: 'max-w-lg',
    lg: 'max-w-2xl',
    xl: 'max-w-4xl'
  };

  if (!isOpen && !isClosing) return null;

  return (
    <>
      {/* Backdrop with fade-in/fade-out animation */}
      <div
        className="fixed inset-0 bg-black bg-opacity-50 z-40"
        onClick={handleClose}
        style={{
          animation: isClosing
            ? `fadeOut ${ANIMATION_DURATION}ms ease-in-out forwards`
            : `fadeIn ${ANIMATION_DURATION}ms ease-in-out`
        }}
      />

      {/* Slide-out Panel with slide-in/slide-out animations */}
      <div
        className={`fixed top-0 right-0 h-full ${widthClasses[width]} w-full bg-white shadow-2xl z-50 overflow-y-auto`}
        style={{
          animation: isClosing
            ? `slideOutRight ${ANIMATION_DURATION}ms ease-in-out forwards`
            : `slideInRight ${ANIMATION_DURATION}ms ease-in-out`
        }}
      >
        {/* Header */}
        <div className="sticky top-0 px-6 py-4 flex items-center justify-between z-10" style={{ backgroundColor: '#253262' }}>
          <h2 className="text-2xl font-bold text-white">{title}</h2>
          <button
            onClick={handleClose}
            className="p-2 hover:bg-white/10 rounded-full transition-colors"
            aria-label="Close panel"
            disabled={isClosing}
          >
            <i className="fa fa-xmark text-xl text-white"></i>
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {children}
        </div>
      </div>
    </>
  );
}
