/**
 * Animated Gradient Background Component
 * Author: Hassan
 * Date: 2025-10-21
 *
 * VUTEQ-branded animated background with 7 gradient blobs
 * Uses brand colors: Navy (#253262) and Red (#D2312E)
 *
 * Usage:
 * - Best for light pages: Dashboard, Settings
 * - Provides visual interest and brand reinforcement
 * - Automatically respects prefers-reduced-motion accessibility
 *
 * Performance Notes:
 * - Uses CSS animations (GPU-accelerated)
 * - Will-change property for optimization
 * - 7-second animation cycle
 */

'use client';

import { VUTEQTheme } from '@/lib/theme';

export default function AnimatedGradientBackground() {
  const blobs = VUTEQTheme.gradients.animatedBlobs;

  return (
    <div
      className="fixed inset-0 -z-10 overflow-hidden bg-gradient-to-br from-vuteq-light-gray via-white to-vuteq-blue-gray"
      aria-hidden="true"
    >
      {blobs.map((blob, index) => (
        <div
          key={index}
          className={`absolute rounded-full ${blob.blur} ${blob.mixBlend} animate-blob`}
          style={{
            width: blob.size,
            height: blob.size,
            backgroundColor: blob.color,
            opacity: blob.opacity,
            ...blob.position,
            animationDelay: blob.delay,
            willChange: 'transform',
          }}
        />
      ))}
    </div>
  );
}
