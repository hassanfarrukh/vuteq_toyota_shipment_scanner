/**
 * Root Layout
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-21 - Added favicon configuration
 * Updated: 2025-10-21 - Added AuthProvider for authentication
 * Updated: 2025-10-27 - Mobile optimization: reduced padding, added overflow management
 * Updated: 2025-10-28 - Added PageProvider for dynamic page subtitles
 * Updated: 2025-10-29 - Removed local Font Awesome config, using public/fontawesome/css/all.css
 * Updated: 2025-10-29 - Removed duotone.css (already included in all.css)
 * Updated: 2025-10-29 - Emergency fix: Switched to Font Awesome CDN for immediate presentation (Hassan)
 * Updated: 2025-12-01 - Added LocationProvider for global location management (Hassan)
 * Main application layout
 */

import type { Metadata, Viewport } from 'next';
import './globals.css';
import Header from '@/components/layout/Header';
import { AuthProvider } from '@/contexts/AuthContext';
import { PageProvider } from '@/contexts/PageContext';
import { LocationProvider } from '@/contexts/LocationContext';

export const metadata: Metadata = {
  title: 'VUTEQ Scanner App',
  description: 'Warehouse Scanning Application for Symbol/Zebra TC51/52/70/72 Scanners',
  icons: {
    icon: '/favicon.ico',
    shortcut: '/favicon.ico',
    apple: '/favicon.ico',
  },
  manifest: '/manifest.json',
  appleWebApp: {
    capable: true,
    statusBarStyle: 'default',
    title: 'VUTEQ Scanner',
  },
};

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  maximumScale: 1,
  userScalable: false,
  viewportFit: 'cover',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <head>
        {/* Font Awesome Pro Local */}
        <link rel="stylesheet" href="/fontawesome/css/all.css" />
      </head>
      <body className="min-h-screen flex flex-col">
        <AuthProvider>
          <LocationProvider>
            <PageProvider>
              <Header />
              <main className="flex-1 container py-2 md:py-6 overflow-auto">
                {children}
              </main>
            </PageProvider>
          </LocationProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
