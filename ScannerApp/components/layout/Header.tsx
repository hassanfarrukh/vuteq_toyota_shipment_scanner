/**
 * Header Component
 * Author: Hassan
 * Date: 2025-10-20
 * Updated: 2025-10-21 - Added VUTEQ and Toyota logos
 * Updated: 2025-10-21 - Added user info and logout functionality
 * Updated: 2025-10-21 - Removed Toyota logo, added user dropdown menu
 * Updated: 2025-10-21 - Redesigned user dropdown with sexy circular Font Awesome icon
 * Updated: 2025-10-21 - CRITICAL FIX: Moved all hooks before conditional return to fix React Hooks error
 * Updated: 2025-10-21 - Modern enterprise redesign: Removed redundant home button, enhanced logo clickability,
 *                        added page title/breadcrumb display, improved visual hierarchy
 * Updated: 2025-10-21 - Simplified user area: Moved user info into dropdown, clean icon-only button in header
 * Updated: 2025-10-28 - Added location display (INDIANA) in user dropdown menu
 * Updated: 2025-10-28 - Added getPageSubtitle() function for custom page subtitles;
 *                        download-scanned-data page now shows "Export ItemConfirmationHistory with custom filters"
 * Updated: 2025-10-28 - Integrated PageContext for dynamic subtitles (dock-monitor, administration)
 * Updated: 2025-10-28 - Redesigned header with Option D layout:
 *                        Desktop: [Logo] [Heading/Subheading] ... [Home Icon] [User Dropdown]
 *                        Mobile: [☰ Menu] [Heading] ... [User Avatar] with subheading on second line
 * Updated: 2025-10-28 - Fixed Font Awesome icons: added antialiasing to home icon
 * Updated: 2025-10-28 - CRITICAL FIX: Made location icon conditional - only shows when subtitle equals location (Hassan)
 * Updated: 2025-10-29 - Changed fa to fa-solid for all icons (solid/bold style) by Hassan
 * Updated: 2025-10-29 - Changed home icon to fa-duotone fa-light with VUTEQ colors (Hassan)
 * Updated: 2025-10-29 - Updated user avatar to duotone fa-circle-user with VUTEQ colors (Hassan)
 * Updated: 2025-10-29 - Changed location icons to fa-duotone fa-light with VUTEQ colors (Hassan)
 * Updated: 2025-10-29 - Changed header icons to fa-light (single color) by Hassan
 * Updated: 2025-10-29 - Hidden location display on pre-shipment-scan page (Hassan)
 * Updated: 2025-10-28 - Updated page subtitles: skid-build shows "Item confirmation for the skid build",
 *                        shipment-load shows empty subtitle (Hassan)
 * Updated: 2025-11-05 - Added upload-order page support: displays last order upload timestamp instead of location (Hassan)
 * Updated: 2025-12-01 - Integrated LocationContext for global location display from Dock Monitor settings (Hassan)
 * Updated: 2025-12-01 - Changed upload-order subtitle to static text: "Manage imported files and view planned order items" (Hassan)
 * Updated: 2026-01-16 - Integrated formatLocation helper to display location as "Code | City" format (Hassan)
 * Application header with navigation, brand logos, and user session
 */

'use client';

import { usePathname, useRouter } from 'next/navigation';
import Image from 'next/image';
import { useState, useRef, useEffect } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { usePageContext } from '@/contexts/PageContext';
import { useLocation } from '@/contexts/LocationContext';
import { formatLocation } from '@/lib/constants';

export default function Header() {
  const pathname = usePathname();
  const router = useRouter();
  const { location } = useLocation(); // Get global location from context
  const { user, logout, isAuthenticated } = useAuth();
  const { subtitle } = usePageContext();

  const [isDropdownOpen, setIsDropdownOpen] = useState(false);
  const [lastUploadTime, setLastUploadTime] = useState<string>('Loading...');
  const [hasMounted, setHasMounted] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const isHomePage = pathname === '/';
  const isLoginPage = pathname === '/login';
  const isOrdersPage = pathname === '/orders';

  // Close dropdown when clicking outside
  // IMPORTANT: This hook MUST be called before any conditional returns
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsDropdownOpen(false);
      }
    };

    if (isDropdownOpen) {
      document.addEventListener('mousedown', handleClickOutside);
    }

    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, [isDropdownOpen]);

  // Mark component as mounted (for hydration safety)
  useEffect(() => {
    setHasMounted(true);
  }, []);

  // Fetch last upload time from API when on orders page
  useEffect(() => {
    if (!hasMounted || !isOrdersPage) return;

    const fetchLastUploadTime = async () => {
      try {
        const { getUploadHistory } = await import('@/lib/api/orderUploads');
        const response = await getUploadHistory();

        if (response.success && response.data && response.data.length > 0) {
          // Sort by uploadDate descending and get the first (most recent)
          const sortedFiles = [...response.data].sort((a, b) => {
            return new Date(b.uploadDate).getTime() - new Date(a.uploadDate).getTime();
          });
          const lastFile = sortedFiles[0];

          // Format the date
          const date = new Date(lastFile.uploadDate);
          setLastUploadTime(date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
          }));
        } else {
          setLastUploadTime('No orders uploaded yet');
        }
      } catch (e) {
        console.error('Error fetching last upload time:', e);
        setLastUploadTime('No orders uploaded yet');
      }
    };

    fetchLastUploadTime();
  }, [hasMounted, isOrdersPage]);

  // Don't show header on login page
  // This return is AFTER all hooks, which is correct per Rules of Hooks
  if (isLoginPage) {
    return null;
  }

  const handleLogout = () => {
    setIsDropdownOpen(false);
    logout();
  };

  const getRoleBadgeColor = (role: string) => {
    switch (role) {
      case 'ADMIN':
        return 'bg-red-100 text-red-800';
      case 'SUPERVISOR':
        return 'bg-blue-100 text-blue-800';
      case 'OPERATOR':
        return 'bg-green-100 text-green-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  };

  // Get current page title based on pathname
  const getPageTitle = () => {
    if (isHomePage) return 'Dashboard';

    // Custom title for orders page
    if (pathname === '/orders') return 'Order Data';

    const pathSegments = pathname.split('/').filter(Boolean);
    if (pathSegments.length === 0) return 'Dashboard';

    // Capitalize and format the page name
    const pageName = pathSegments[pathSegments.length - 1]
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');

    return pageName;
  };

  // Get custom subtitle for specific pages, otherwise return location
  // Now supports dynamic subtitles via PageContext
  const getPageSubtitle = () => {
    // If a page has set a dynamic subtitle via PageContext, use that
    if (subtitle) {
      return subtitle;
    }

    // Static subtitles for specific pages
    if (pathname === '/skid-build') {
      return 'Item confirmation for the skid build';
    }
    if (pathname === '/shipment-load') {
      return '';
    }
    if (pathname === '/pre-shipment-scan') {
      return '';
    }
    if (pathname === '/download-scanned-data') {
      return 'Export ItemConfirmationHistory with custom filters';
    }
    if (pathname === '/administration') {
      return 'Manage offices, warehouses, users, and dock monitor';
    }
    if (pathname === '/orders') {
      return 'Manage imported files and view planned order items';
    }

    // Default to formatted location (Code | City)
    return location ? formatLocation(location) : location;
  };

  // Get last upload time - returns static text on server, dynamic on client
  const getLastUploadTime = (): string => {
    if (!hasMounted) return 'Loading...';
    return lastUploadTime;
  };

  return (
    <header className="sticky top-0 z-50 bg-white shadow-sm border-b-2 border-vuteq-navy">
      <div className="container mx-auto px-3 py-2.5 sm:px-6 sm:py-3.5">
        <div className="flex items-center justify-between gap-3 sm:gap-6">
          {/* Left Section: Hamburger (Mobile) / Logo (Desktop) + Page Title */}
          <div className="flex items-center gap-3 sm:gap-6 flex-1 min-w-0">
            {/* Home Icon - Mobile Only */}
            <button
              onClick={() => router.push('/')}
              className="sm:hidden shrink-0 p-2 hover:bg-gray-100 rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500"
              aria-label="Go to home"
            >
              <i className="fa-light fa-home text-gray-700 text-2xl" style={{ WebkitFontSmoothing: 'antialiased' }}></i>
            </button>

            {/* VUTEQ Logo - Desktop Only */}
            <button
              onClick={() => router.push('/')}
              className="hidden sm:flex group items-center gap-2 hover:opacity-90 transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 rounded-lg p-1.5 shrink-0"
              aria-label="VUTEQ Home"
            >
              <div className="relative h-11 w-28 shrink-0">
                <Image
                  src="/images/logos/logo01.svg"
                  alt="VUTEQ Logo"
                  fill
                  className="object-contain group-hover:scale-105 transition-transform duration-200"
                  priority
                />
              </div>
            </button>

            {/* Vertical Divider - Desktop Only */}
            <div className="hidden sm:block h-8 w-px bg-gray-300" aria-hidden="true"></div>

            {/* Page Title Section */}
            <div className="min-w-0 flex-1">
              <h1 className="text-base sm:text-lg font-semibold text-gray-900 truncate">
                {getPageTitle()}
              </h1>
              {/* Subheading - Desktop: inline, Mobile: second line */}
              <p className="text-xs text-gray-500 truncate mt-0.5 sm:mt-0">
                {getPageSubtitle() === location && (
                  <i className="fa-regular fa-location-dot mr-1.5" style={{ color: '#64748b' }}></i>
                )}
                {getPageSubtitle()}
              </p>
            </div>
          </div>

          {/* Right Section: Home Icon (Desktop) + User Dropdown */}
          <div className="flex items-center gap-2 sm:gap-3 shrink-0">
            {/* Home Icon - Desktop Only */}
            <button
              onClick={() => router.push('/')}
              className="hidden sm:flex items-center justify-center p-2.5 hover:bg-gray-100 rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-blue-500"
              aria-label="Go to home"
            >
              <i className="fa-regular fa-home text-gray-700 text-2xl" style={{ WebkitFontSmoothing: 'antialiased' }}></i>
            </button>

            {/* User Dropdown - Show if authenticated */}
            {isAuthenticated && user && (
              <div className="relative" ref={dropdownRef}>
                {/* User Avatar Button - Desktop: with chevron, Mobile: avatar only */}
                <button
                  onClick={() => setIsDropdownOpen(!isDropdownOpen)}
                  className="flex items-center gap-2 px-3 py-2 bg-white hover:bg-gray-50 rounded-full border border-gray-300 shadow-sm transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
                  aria-label="User menu"
                  aria-expanded={isDropdownOpen}
                >
                  {/* Circular User Avatar with Regular Font Awesome Icon */}
                  <i className="fa-regular fa-circle-user text-2xl" style={{ color: '#253262', WebkitFontSmoothing: 'antialiased' }}></i>

                  {/* Chevron Down Icon - Desktop Only */}
                  <i className={`hidden sm:block fa-solid fa-chevron-down text-gray-500 text-xs transition-transform duration-200 ${isDropdownOpen ? 'rotate-180' : ''}`}></i>
                </button>

                {/* Dropdown Menu */}
                {isDropdownOpen && (
                  <div className="absolute right-0 mt-2 w-64 bg-white rounded-lg shadow-xl border border-gray-200 py-2 z-50 animate-in fade-in slide-in-from-top-2 duration-200">
                    {/* User Info Section at Top of Dropdown */}
                    <div className="px-4 py-3 border-b border-gray-200">
                      {/* User Name */}
                      <p className="text-base font-semibold text-gray-900 leading-tight">{user.name}</p>

                      {/* Username */}
                      <p className="text-sm text-gray-600 mt-1">@{user.username}</p>

                      {/* Role Badge */}
                      <div className="mt-2">
                        <span className={`inline-block text-xs font-medium px-2.5 py-1 rounded-full ${getRoleBadgeColor(user.role)}`}>
                          {user.role}
                        </span>
                      </div>

                      {/* Location Display */}
                      <div className="mt-2 flex items-center gap-2">
                        <i className="fa-regular fa-location-dot text-sm" style={{ color: '#64748b' }}></i>
                        <span className="text-sm font-medium text-gray-700">{location ? formatLocation(location) : location}</span>
                      </div>
                    </div>

                    {/* Logout Option */}
                    <button
                      onClick={handleLogout}
                      className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-600 hover:bg-red-50 transition-colors"
                    >
                      <i className="fa-light fa-right-from-bracket text-base"></i>
                      <span className="font-light">Logout</span>
                    </button>
                  </div>
                )}
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}
