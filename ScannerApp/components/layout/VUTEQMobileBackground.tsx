/**
 * VUTEQ Mobile-Optimized Static Background Component
 * Author: Hassan
 * Date: 2025-10-22
 *
 * Mobile-first responsive background that adapts to screen size:
 * - Mobile (<768px): Clean, minimal gradient with subtle accent
 * - Desktop (>=768px): Full blob background for visual richness
 *
 * Features:
 * - Clean, uncluttered design perfect for mobile scanners
 * - Uses VUTEQ brand colors: Navy (#253262), Red (#D2312E)
 * - Smaller, more subtle design elements for mobile
 * - Zero performance impact (static CSS only)
 * - Responsive - automatically switches based on viewport
 *
 * Performance Notes:
 * - Static CSS only (no JavaScript, no animations)
 * - Perfect for TC51/52/70/72 mobile scanners
 * - No battery drain or performance concerns
 */

'use client';

import { VUTEQTheme } from '@/lib/theme';

export default function VUTEQMobileBackground() {
  const blobs = VUTEQTheme.gradients.animatedBlobs;

  return (
    <>
      {/* Mobile Background (<768px): Clean minimal gradient */}
      <div
        className="fixed inset-0 -z-10 md:hidden"
        aria-hidden="true"
        style={{
          background: `
            radial-gradient(circle at 85% 15%, rgba(210, 49, 46, 0.08) 0%, transparent 40%),
            radial-gradient(circle at 15% 85%, rgba(37, 50, 98, 0.08) 0%, transparent 40%),
            linear-gradient(180deg, #F5F7F9 0%, #FCFCFC 50%, #EEF0F4 100%)
          `,
        }}
      >
        {/* Subtle top accent for mobile */}
        <div
          className="absolute rounded-full blur-3xl"
          style={{
            width: '200px',
            height: '200px',
            backgroundColor: '#253262',
            opacity: 0.05,
            top: '-5%',
            right: '-10%',
            pointerEvents: 'none',
          }}
        />

        {/* Subtle bottom accent for mobile */}
        <div
          className="absolute rounded-full blur-2xl"
          style={{
            width: '180px',
            height: '180px',
            backgroundColor: '#D2312E',
            opacity: 0.06,
            bottom: '-5%',
            left: '-8%',
            pointerEvents: 'none',
          }}
        />
      </div>

      {/* Desktop Background (>=768px): Full blob design */}
      <div
        className="fixed inset-0 -z-10 hidden md:block overflow-hidden bg-gradient-to-br from-vuteq-light-gray via-white to-vuteq-blue-gray"
        aria-hidden="true"
      >
        {blobs.map((blob, index) => (
          <div
            key={index}
            className={`absolute rounded-full ${blob.blur} ${blob.mixBlend}`}
            style={{
              width: blob.size,
              height: blob.size,
              backgroundColor: blob.color,
              opacity: blob.opacity,
              ...blob.position,
              // NO animation properties - completely static
              pointerEvents: 'none',
            }}
          />
        ))}
      </div>
    </>
  );
}
