/**
 * VUTEQ Brand Theme Configuration
 * Author: Hassan
 * Date: 2025-10-21
 * Centralized color palette and design tokens for VUTEQ-branded Scanner Application
 */

export const VUTEQTheme = {
  colors: {
    // Brand Primary Colors
    navy: {
      DEFAULT: '#253262',
      dark: '#1e2850',
      darker: '#192040',
      light: '#2d3d7a',
    },
    red: {
      DEFAULT: '#D2312E',
      dark: '#B82925',
      darker: '#A02420',
      light: '#E04845',
    },

    // Neutral Backgrounds
    offWhite: '#FCFCFC',
    lightGray: '#F5F7F9',
    blueGray: '#EEF0F4',
  },

  // Animated Gradient Blob Configuration
  gradients: {
    animatedBlobs: [
      {
        color: '#253262',
        size: '500px',
        position: { top: '-15%', left: '-10%' },
        delay: '0s',
        opacity: 0.8,
        blur: 'blur-3xl',
        mixBlend: 'mix-blend-multiply',
      },
      {
        color: '#253262',
        size: '450px',
        position: { top: '10%', right: '-8%' },
        delay: '2s',
        opacity: 0.75,
        blur: 'blur-3xl',
        mixBlend: 'mix-blend-multiply',
      },
      {
        color: '#D2312E',
        size: '380px',
        position: { top: '35%', left: '15%' },
        delay: '4s',
        opacity: 0.85,
        blur: 'blur-2xl',
        mixBlend: 'mix-blend-normal',
      },
      {
        color: '#D2312E',
        size: '320px',
        position: { bottom: '5%', right: '10%' },
        delay: '1s',
        opacity: 0.75,
        blur: 'blur-2xl',
        mixBlend: 'mix-blend-normal',
      },
      {
        color: '#253262',
        size: '400px',
        position: { bottom: '-5%', left: '30%' },
        delay: '6s',
        opacity: 0.7,
        blur: 'blur-3xl',
        mixBlend: 'mix-blend-multiply',
      },
      {
        color: '#EEF0F4',
        size: '280px',
        position: { top: '50%', right: '25%' },
        delay: '3s',
        opacity: 0.5,
        blur: 'blur-2xl',
        mixBlend: 'mix-blend-overlay',
      },
      {
        color: '#F5F7F9',
        size: '200px',
        position: { top: '25%', left: '40%' },
        delay: '5s',
        opacity: 0.4,
        blur: 'blur-xl',
        mixBlend: 'mix-blend-overlay',
      },
    ],

    // Static gradient for data-heavy pages
    staticGradient: `
      radial-gradient(circle at 20% 30%, rgba(37, 50, 98, 0.15) 0%, transparent 50%),
      radial-gradient(circle at 80% 70%, rgba(210, 49, 46, 0.12) 0%, transparent 50%),
      radial-gradient(circle at 50% 50%, rgba(37, 50, 98, 0.08) 0%, transparent 60%),
      linear-gradient(135deg, #F5F7F9 0%, #FCFCFC 50%, #EEF0F4 100%)
    `,
  },

  // VUTEQ-tinted box shadows
  shadows: {
    card: '0 10px 15px -3px rgba(37, 50, 98, 0.08), 0 4px 6px -4px rgba(210, 49, 46, 0.04)',
    cardHover: '0 20px 25px -5px rgba(37, 50, 98, 0.12), 0 10px 10px -5px rgba(210, 49, 46, 0.06)',
  },
} as const;

export type VUTEQThemeType = typeof VUTEQTheme;
