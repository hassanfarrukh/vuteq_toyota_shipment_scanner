/**
 * Home Dashboard Page
 * Author: Hassan
 * Date: 2025-10-22
 * Updated: 2025-10-22 - Added left-aligned welcome card and restored greeting
 * Updated: 2025-10-28 - Fixed Font Awesome icon classes: changed fa-solid to fa for all dashboard icons
 * Updated: 2025-10-28 - CRITICAL FIX: Changed icon rendering from <i> tags to <span> with unicode and inline font-family for local Font Awesome Pro fonts (Hassan)
 * Updated: 2025-10-28 - Upgraded dashboard icons to Duotone with VUTEQ brand colors (Navy #253262 primary, Red #D2312E secondary)
 * Updated: 2025-10-29 - Properly implemented Font Awesome 7 duotone icons with CSS variables for dual-color rendering (Hassan)
 * Updated: 2025-10-29 - Added forklift icon mapping and "Operator Pre Shipment Scan" tile support (Hassan)
 * Updated: 2025-10-29 - Converted dashboard icons from fa-duotone to fa-light with navy blue color (Hassan)
 * Updated: 2025-11-05 - Added upload and file-arrow-up icon mappings for Upload Order Data tile (Hassan)
 * Updated: 2025-11-05 - Reduced vertical spacing to eliminate scrolling: space-y reduced, gap-y separated, py and min-h reduced (Hassan)
 * Updated: 2026-01-03 - Added Site Settings check to conditionally hide Pre-shipment Scan tile based on enablePreShipmentScan flag (Hassan)
 *
 * Main dashboard with feature tiles - simple and clean design
 */

'use client';

import { useRouter } from 'next/navigation';
import Card from '@/components/ui/Card';
import { DASHBOARD_TILES } from '@/lib/constants';
import { useAuth } from '@/contexts/AuthContext';
import { useLocation } from '@/contexts/LocationContext';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
export default function HomePage() {
  const router = useRouter();
  const { user } = useAuth();
  const { enablePreShipmentScan } = useLocation(); // Get from context - no extra API call needed

  // Filter tiles based on user role with proper hierarchy
  // Author: Hassan
  // Date: 2025-10-22
  // Updated: 2026-01-03 - Added feature flag filtering for Pre-shipment Scan (Hassan)
  // Role Hierarchy: OPERATOR < SUPERVISOR < ADMIN
  // OPERATOR: Can access OPERATOR-level features only (Skid Build, Shipment Load)
  // SUPERVISOR: Can access OPERATOR + SUPERVISOR features (all dashboards, monitor)
  // ADMIN: Can access all features including Settings
  const getFilteredTiles = () => {
    if (!user) return [];

    return DASHBOARD_TILES.filter(tile => {
      // All tiles now require a role (no undefined check needed)
      if (!tile.requiresRole) return false;

      // Check feature flags - hide Pre-shipment Scan if disabled
      if (tile.route === '/pre-shipment-scan' && !enablePreShipmentScan) {
        return false;
      }

      // ADMIN can access everything
      if (user.role === 'ADMIN') return true;

      // SUPERVISOR can access OPERATOR and SUPERVISOR level tiles (but not ADMIN)
      if (user.role === 'SUPERVISOR') {
        return tile.requiresRole === 'OPERATOR' || tile.requiresRole === 'SUPERVISOR';
      }

      // OPERATOR can only access OPERATOR level tiles
      if (user.role === 'OPERATOR') {
        return tile.requiresRole === 'OPERATOR';
      }

      return false;
    });
  };

  const filteredTiles = getFilteredTiles();

  const getTileIcon = (icon: string) => {
    // Font Awesome 7 Light icons with VUTEQ navy blue color
    // Author: Hassan, Date: 2025-10-29
    // Using light weight icons with single navy blue color for clean, professional look
    const iconStyle = {
      fontSize: '80px',
      color: '#253262', // VUTEQ navy blue
    } as React.CSSProperties;

    // Icon class mappings - using fa-light for clean, professional appearance
    // Author: Hassan, Date: 2025-10-29
    // Updated: Hassan, Date: 2025-11-05 - Added file-arrow-up and upload icons for Upload Order Data tile
    // Updated: Hassan, Date: 2025-12-01 - Added clipboard-list icon for Order Data tile
    const iconMap: Record<string, JSX.Element> = {
      box: <i className="fa-light fa-clipboard-list-check" style={iconStyle}></i>,
      truck: <i className="fa-light fa-truck-container" style={iconStyle}></i>,
      forklift: <i className="fa-light fa-forklift" style={iconStyle}></i>,
      download: <i className="fa-light fa-file-arrow-down" style={iconStyle}></i>,
      upload: <i className="fa-light fa-upload" style={iconStyle}></i>,
      'file-arrow-up': <i className="fa-light fa-file-arrow-up" style={iconStyle}></i>,
      'clipboard-list': <i className="fa-light fa-clipboard-list" style={iconStyle}></i>,
      monitor: <i className="fa-light fa-desktop" style={iconStyle}></i>,
      gear: <i className="fa-light fa-folder-gear" style={iconStyle}></i>,
      'user-shield': <i className="fa-light fa-user-shield" style={iconStyle}></i>,
    };

    return iconMap[icon] || iconMap.box;
  };

  return (
    <>
      {/* VUTEQ Static Background */}
      <VUTEQStaticBackground />

      <div className="space-y-2 sm:space-y-3">
        {/* Welcome Card - Left Aligned */}
        <Card>
          <div className="p-3">
            <h1 className="text-xl font-bold tracking-tight" style={{ color: '#253262' }}>
              WELCOME TO VUTEQ SCANNER
            </h1>
            <p className="text-xs text-gray-600 font-medium">
              Welcome back, {user?.name || 'User'}
            </p>
          </div>
        </Card>

        {/* Dashboard Tiles Grid - Premium Mobile-First Design */}
        {/* Optimized touch targets for scanner devices */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-x-3 gap-y-2 sm:gap-x-4 sm:gap-y-3">
          {filteredTiles.map((tile) => (
            <Card
              key={tile.id}
              className="group hover:shadow-xl transition-all duration-300 cursor-pointer bg-white border border-gray-200 hover:border-[#253262]/30"
              onClick={() => router.push(tile.route)}
            >
              {/* Enhanced Tile Content with better visual hierarchy */}
              <div className="relative overflow-hidden">
                {/* Hover effect overlay */}
                <div className="absolute inset-0 bg-gradient-to-br from-[#253262]/0 to-[#3d4d7f]/0 group-hover:from-[#253262]/5 group-hover:to-[#3d4d7f]/5 transition-all duration-300"></div>

                {/* Content */}
                <div className="relative flex flex-col items-center justify-center text-center py-5 px-4 min-h-[120px] sm:min-h-[140px]">
                  {/* Icon - Clean outlined style without background */}
                  <div className="mb-4 transition-transform duration-300 group-hover:scale-110">
                    {getTileIcon(tile.icon)}
                  </div>

                  {/* Title - Large enough for easy reading on mobile */}
                  <h2 className="text-base sm:text-lg font-semibold text-gray-900 group-hover:text-[#253262] transition-colors">
                    {tile.title}
                  </h2>
                </div>

                {/* Bottom accent indicator */}
                <div className="absolute bottom-0 left-0 right-0 h-1 bg-gradient-to-r from-transparent via-[#253262] to-transparent opacity-0 group-hover:opacity-100 transition-opacity duration-300"></div>
              </div>
            </Card>
          ))}
        </div>
      </div>
    </>
  );
}
