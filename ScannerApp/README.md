# VUTEQ Warehouse Scanner Application

**Author:** Hassan
**Date:** 2025-10-20
**Version:** 1.0.0

## Overview

Complete Next.js 14 warehouse scanning application designed for Symbol/Zebra TC51/52/70/72 mobile scanners. Built with TypeScript, Tailwind CSS, and mobile-first responsive design principles.

## Features

### Core Functionality

1. **Skid Build (CRITICAL)** - 5-Step Workflow
   - Step 1: Search/Scan Order (OWK)
   - Step 2: Scan Skid Manifest (Toyota Label)
   - Step 3: Scan Toyota Kanban
   - Step 4: Scan Internal Kanban (7-13 char reusable cards)
   - Step 5: Scan Serial Numbers (Indiana location only - BR-004)

2. **Shipment Load (CRITICAL)** - 4-Step Process
   - **MANDATORY:** Driver Check Sheet scan FIRST (BR-001)
   - Enter trailer information
   - Scan skids for loading
   - Submit to Toyota API

3. **Supplier Dashboard (HIGH)**
   - View supplier routes and schedules
   - Monitor in-transit deliveries
   - Track order counts per supplier

4. **Compliance Dashboard (MEDIUM)**
   - Overall compliance score
   - Metrics by type (Accuracy, Timeliness, Quality, Safety)
   - Issue tracking and resolution

5. **Dock Monitor (HIGH)**
   - Real-time dock status
   - 10-second auto-refresh (BR-005)
   - Door availability tracking

## Technology Stack

- **Framework:** Next.js 14 (App Router)
- **Language:** TypeScript 5.5+
- **Styling:** Tailwind CSS 3.4+
- **State Management:** React Hooks
- **Mobile Target:** Symbol/Zebra TC51/52/70/72 scanners

## Business Rules

- **BR-001:** Driver Check Sheet MUST be scanned FIRST before loading
- **BR-002:** Skid manifest must link to valid OWK order
- **BR-003:** Internal Kanbans are 7-13 character reusable plastic cards
- **BR-004:** Serial number scanning required at Indiana location
- **BR-005:** Dock Monitor refresh every 10 seconds
- **BR-006:** Prevent duplicate scans within 24 hours

## Installation

### Prerequisites

- Node.js 18+ installed
- npm or yarn package manager

### Setup Instructions

1. **Navigate to the project directory:**
   ```bash
   cd F:\VUTEQ\FromClaude\Codes\ScannerApp
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Create environment file:**
   ```bash
   copy .env.example .env.local
   ```

4. **Configure environment variables:**
   Edit `.env.local` and set your configuration:
   ```env
   NEXT_PUBLIC_API_URL=http://localhost:3000/api/v1
   NEXT_PUBLIC_LOCATION=INDIANA
   NEXT_PUBLIC_ENABLE_SERIAL_SCANNING=true
   ```

5. **Start development server:**
   ```bash
   npm run dev
   ```

6. **Open browser:**
   Navigate to `http://localhost:3000`

## Project Structure

```
F:\VUTEQ\FromClaude\Codes\ScannerApp\
├── app/                          # Next.js App Router pages
│   ├── page.tsx                  # Home dashboard with 5 tiles
│   ├── skid-build/
│   │   └── page.tsx              # Skid Build 5-step workflow
│   ├── shipment-load/
│   │   └── page.tsx              # Shipment Load with BR-001
│   ├── supplier-dashboard/
│   │   └── page.tsx              # Supplier routes and orders
│   ├── compliance-dashboard/
│   │   └── page.tsx              # Compliance metrics
│   ├── dock-monitor/
│   │   └── page.tsx              # Real-time dock status
│   ├── layout.tsx                # Root layout with header
│   └── globals.css               # Global styles
├── components/
│   ├── ui/                       # Reusable UI components
│   │   ├── Button.tsx            # Touch-optimized button
│   │   ├── Card.tsx              # Container card
│   │   ├── Input.tsx             # Accessible input field
│   │   ├── Scanner.tsx           # Barcode scanner input
│   │   ├── Badge.tsx             # Status badge
│   │   └── Alert.tsx             # Alert/notification
│   └── layout/
│       └── Header.tsx            # Application header
├── lib/
│   ├── api.ts                    # Mock API functions
│   ├── constants.ts              # App constants and config
│   └── utils.ts                  # Utility functions
├── types/
│   └── index.ts                  # TypeScript type definitions
├── package.json                  # Dependencies
├── tsconfig.json                 # TypeScript configuration
├── tailwind.config.ts            # Tailwind CSS configuration
├── next.config.js                # Next.js configuration
├── postcss.config.js             # PostCSS configuration
├── .env.example                  # Environment variables template
└── README.md                     # This file
```

## Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm start` - Start production server
- `npm run lint` - Run ESLint
- `npm run type-check` - Check TypeScript types

## API Integration

### Mock API Structure

The application currently uses mock APIs in `lib/api.ts`. Replace with actual backend endpoints:

**Order Management:**
- `POST /api/v1/orders/upload` - Upload order
- `GET /api/v1/orders/{orderId}` - Get order details

**Skid Build:**
- `POST /api/v1/skid-build/scan` - Process scan
- `POST /api/v1/skid-build/complete` - Complete build

**Shipment:**
- `POST /api/v1/shipment/driver-check` - Verify driver
- `POST /api/v1/shipment/load` - Create shipment
- `POST /api/v1/shipment/submit-toyota` - Submit to Toyota

**Dashboard:**
- `GET /api/v1/dashboard/dock-status` - Get dock status
- `GET /api/v1/dashboard/supplier-routes` - Get supplier routes
- `GET /api/v1/reports/compliance` - Get compliance report

## Barcode Format Validation

The application validates the following barcode formats:

- **OWK Order:** `OWK-XXXXXX` (6-10 digits)
- **Toyota Label:** `TL-XXXXXXXX` (8 digits)
- **Toyota Kanban:** `TK-XXXXXX` (6-12 alphanumeric)
- **Internal Kanban:** `XXXXXXX` (7-13 alphanumeric, BR-003)
- **Serial Number:** `SN-XXXXXXXX` (8-16 alphanumeric)
- **Driver Check Sheet:** `DCS-XXXXXX` (6 digits)

## Mobile Scanner Configuration

### Zebra TC51/52/70/72 Setup

1. **Scanner Mode:** Continuous
2. **Beep on Scan:** Enabled
3. **Vibrate on Scan:** Enabled
4. **Auto-focus:** Enabled

### Browser Recommendations

- **Primary:** Chrome for Android
- **Alternative:** Firefox Mobile
- **Full-screen mode recommended**

## Performance Requirements

- **Scan Processing:** 3-5 seconds max
- **Dashboard Refresh:** <10 seconds
- **Dock Monitor Refresh:** 10 seconds (BR-005)
- **Lighthouse Score:** 90+ across all metrics

## Accessibility Features

- WCAG 2.1 AA compliant
- Keyboard navigation support
- Screen reader compatible
- Minimum touch target: 44x44px
- Color contrast ratio: 4.5:1+

## Multi-Location Support

The application supports 6 locations:

1. **INDIANA** - Serial scanning required (BR-004)
2. **MICHIGAN** - Standard scanning
3. **OHIO** - Standard scanning
4. **KENTUCKY** - Standard scanning
5. **TENNESSEE** - Standard scanning
6. **ALABAMA** - Standard scanning

Set location in `.env.local`:
```env
NEXT_PUBLIC_LOCATION=INDIANA
```

## Future Enhancements

- [ ] JWT authentication implementation
- [ ] Database integration (PostgreSQL)
- [ ] Toyota API integration
- [ ] Offline mode with sync
- [ ] Print label functionality
- [ ] Advanced reporting
- [ ] User management
- [ ] Audit logging

## Troubleshooting

### Scanner Not Working

1. Verify browser permissions for camera
2. Check scanner device configuration
3. Ensure HTTPS or localhost for WebRTC
4. Test with manual input first

### API Timeout Errors

1. Check network connection
2. Verify API endpoint URLs
3. Increase timeout in `.env.local`:
   ```env
   NEXT_PUBLIC_API_TIMEOUT=10000
   ```

### Serial Scanning Not Required

1. Verify location setting in `.env.local`
2. Only INDIANA location requires serials (BR-004)
3. Check `requiresSerialScanning()` in `lib/utils.ts`

## Support

For issues or questions, contact the development team.

## License

Proprietary - VUTEQ Corporation

---

**Built with care for warehouse efficiency** 📦
