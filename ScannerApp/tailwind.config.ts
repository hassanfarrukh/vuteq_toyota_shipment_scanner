/**
 * Tailwind CSS Configuration
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-21 - Added safelist for dock monitor status colors
 * Updated: 2025-10-21 - Added blob animation for login page background
 * Updated: 2025-10-21 - Added VUTEQ brand colors and design tokens
 * Mobile-first configuration for Symbol/Zebra scanners
 */

import type { Config } from 'tailwindcss'
import { VUTEQTheme } from './lib/theme'

const config: Config = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  safelist: [
    // Dock Monitor status colors - ensure these are always included in build
    'bg-green-500/30',
    'border-green-500',
    'bg-blue-400/30',
    'border-blue-400',
    'bg-orange-500/30',
    'border-orange-500',
    'bg-red-500/30',
    'border-red-500',
    'bg-yellow-500/30',
    'border-yellow-500',
    'bg-purple-500/30',
    'border-purple-500',
    'border-l-4',
  ],
  theme: {
    extend: {
      screens: {
        'xs': '480px', // Extra small devices (mobile landscape)
      },
      colors: {
        border: 'rgb(var(--color-border))',
        background: 'rgb(var(--color-background))',
        foreground: 'rgb(var(--color-foreground))',

        // VUTEQ Brand Colors
        'vuteq-navy': {
          DEFAULT: VUTEQTheme.colors.navy.DEFAULT,
          dark: VUTEQTheme.colors.navy.dark,
          darker: VUTEQTheme.colors.navy.darker,
          light: VUTEQTheme.colors.navy.light,
        },
        'vuteq-red': {
          DEFAULT: VUTEQTheme.colors.red.DEFAULT,
          dark: VUTEQTheme.colors.red.dark,
          darker: VUTEQTheme.colors.red.darker,
          light: VUTEQTheme.colors.red.light,
        },
        'vuteq-offwhite': VUTEQTheme.colors.offWhite,
        'vuteq-light-gray': VUTEQTheme.colors.lightGray,
        'vuteq-blue-gray': VUTEQTheme.colors.blueGray,

        // Update primary to use VUTEQ Navy
        primary: {
          50: '#f0f4fb',
          100: '#e0e9f7',
          200: '#c1d3ef',
          300: '#92b3e3',
          400: '#5d8fd4',
          500: '#3a6fc2',
          600: VUTEQTheme.colors.navy.DEFAULT, // VUTEQ Navy
          700: VUTEQTheme.colors.navy.dark,
          800: VUTEQTheme.colors.navy.darker,
          900: '#0f1729',
        },

        // Keep existing semantic colors
        success: {
          50: '#f0fdf4',
          100: '#dcfce7',
          500: '#22c55e',
          600: '#16a34a',
          700: '#15803d',
        },
        warning: {
          50: '#fffbeb',
          100: '#fef3c7',
          500: '#f59e0b',
          600: '#d97706',
        },
        error: {
          50: '#fef2f2',
          100: '#fee2e2',
          500: '#ef4444',
          600: '#dc2626',
          700: '#b91c1c',
        },
      },
      spacing: {
        '18': '4.5rem',
        '88': '22rem',
        '128': '32rem',
      },
      minHeight: {
        'touch': '44px', // Minimum touch target size
      },
      minWidth: {
        'touch': '44px',
      },
      animation: {
        'blob': 'blob 7s infinite',
      },
      keyframes: {
        blob: {
          '0%': {
            transform: 'translate(0px, 0px) scale(1)',
          },
          '33%': {
            transform: 'translate(30px, -50px) scale(1.1)',
          },
          '66%': {
            transform: 'translate(-20px, 20px) scale(0.9)',
          },
          '100%': {
            transform: 'translate(0px, 0px) scale(1)',
          },
        },
      },
      boxShadow: {
        'vuteq-card': VUTEQTheme.shadows.card,
        'vuteq-card-hover': VUTEQTheme.shadows.cardHover,
      },
    },
  },
  plugins: [],
}

export default config
