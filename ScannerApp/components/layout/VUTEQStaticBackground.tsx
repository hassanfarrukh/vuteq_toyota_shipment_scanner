/**
 * VUTEQ Static Gradient Background Component
 * Author: Hassan
 * Date: 2025-10-21
 *
 * VUTEQ-branded static background with gradient blobs in FIXED positions
 * Uses the SAME visual concept as AnimatedGradientBackground but NO animations
 * Perfect for all working pages (Dashboard, Settings, Dock Monitor, etc.)
 *
 * Features:
 * - Same blob sizes, colors, and blur effects as animated version
 * - Fixed positions for zero performance impact
 * - Uses VUTEQ brand colors: Navy (#253262), Red (#D2312E), light grays
 * - Professional look matching animated version but completely static
 *
 * Usage:
 * - Best for all pages EXCEPT login (login keeps animated version)
 * - Zero performance impact (CSS-only, no animations, no JavaScript)
 * - Ideal for mobile scanners and data-heavy pages
 *
 * Performance Notes:
 * - Static CSS only (no JavaScript, no animations, no will-change)
 * - No battery drain or performance concerns
 * - Perfect for TC51/52/70/72 mobile scanners
 */

'use client';

import { VUTEQTheme } from '@/lib/theme';

export default function VUTEQStaticBackground() {
  const blobs = VUTEQTheme.gradients.animatedBlobs;

  return (
    <div
      className="fixed inset-0 -z-10 overflow-hidden bg-gradient-to-br from-vuteq-light-gray via-white to-vuteq-blue-gray"
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
  );
}
