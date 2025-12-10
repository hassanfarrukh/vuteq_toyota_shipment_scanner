/**
 * Static Gradient Background Component
 * Author: Hassan
 * Date: 2025-10-21
 *
 * VUTEQ-branded static gradient background for data-heavy pages
 * Uses subtle brand colors with reduced opacity for maximum readability
 *
 * Usage:
 * - Best for data-heavy pages: Dock Monitor, Supplier Dashboard, Compliance Dashboard
 * - Zero performance impact (CSS-only, no animations)
 * - Maintains brand identity without distracting from content
 *
 * Performance Notes:
 * - Static CSS gradient (no JavaScript, no animations)
 * - Ideal for mobile scanners (TC51/52/70/72)
 * - No battery drain or performance concerns
 */

'use client';

import { VUTEQTheme } from '@/lib/theme';

export default function StaticGradientBackground() {
  return (
    <div
      className="fixed inset-0 -z-10"
      style={{
        background: VUTEQTheme.gradients.staticGradient,
      }}
      aria-hidden="true"
    />
  );
}
