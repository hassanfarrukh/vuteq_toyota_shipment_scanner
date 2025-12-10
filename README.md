# VUTEQ Toyota Shipment Scanner System

**Author:** Hassan
**Created:** 2025-11-23
**Last Updated:** 2025-12-11

---

## Overview

This system is a **Toyota SCS (Shipping Confirmation System) API-compliant** barcode scanning application for VUTEQ. It handles the complete workflow for:

1. **Skid Build (Pallet Build)** - Building pallets by scanning Toyota Kanbans and matching them with internal kanbans
2. **Shipment Load** - Loading skids onto trailers for shipment to Toyota plants
3. **Pre-Shipment Scan** - Verification scanning before shipment departure

The system is designed to integrate with **Toyota SCS API V2.0** for real-time shipment confirmation.

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        FRONTEND                                  │
│                   (Next.js 14 + React)                          │
│                                                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────────┐        │
│  │ Skid Build  │  │  Shipment   │  │  Pre-Shipment    │        │
│  │    Page     │  │  Load Page  │  │   Scan Page      │        │
│  └──────┬──────┘  └──────┬──────┘  └────────┬─────────┘        │
│         │                │                   │                  │
│         └────────────────┼───────────────────┘                  │
│                          │                                      │
│                    ┌─────▼─────┐                                │
│                    │  API Lib  │                                │
│                    └─────┬─────┘                                │
└──────────────────────────┼──────────────────────────────────────┘
                           │ HTTP/REST
┌──────────────────────────┼──────────────────────────────────────┐
│                          │                                      │
│                    ┌─────▼─────┐                                │
│                    │Controllers│                                │
│                    └─────┬─────┘                                │
│                          │                                      │
│  ┌───────────────────────┼───────────────────────────┐         │
│  │                 SERVICES                           │         │
│  │  ┌─────────────┐  ┌─────────────┐  ┌───────────┐ │         │
│  │  │ SkidBuild   │  │  Shipment   │  │  Toyota   │ │         │
│  │  │  Service    │  │  Service    │  │ Validation│ │         │
│  │  └─────────────┘  └─────────────┘  └───────────┘ │         │
│  └───────────────────────┼───────────────────────────┘         │
│                          │                                      │
│                    ┌─────▼─────┐                                │
│                    │Repository │                                │
│                    └─────┬─────┘                                │
│                          │                                      │
│                 BACKEND (ASP.NET Core 8)                        │
└──────────────────────────┼──────────────────────────────────────┘
                           │ EF Core
                    ┌──────▼──────┐
                    │  SQL Server │
                    │   Database  │
                    └─────────────┘
```

---

## Project Structure

```
FromHassan/Codes/
├── Backend/                    # ASP.NET Core 8 Web API
│   ├── Controllers/            # API endpoints
│   ├── Models/
│   │   ├── Entities/          # Database entities (EF Core)
│   │   └── DTOs/              # Data transfer objects
│   ├── Services/              # Business logic
│   ├── Repositories/          # Data access layer
│   └── Program.cs             # App configuration
│
├── ScannerApp/                 # Next.js 14 Frontend
│   ├── app/
│   │   ├── skid-build/        # Skid Build workflow
│   │   ├── shipment-load/     # Shipment Load workflow
│   │   └── pre-shipment-scan/ # Pre-shipment verification
│   ├── components/            # Reusable UI components
│   └── lib/
│       └── api.ts             # API client functions
│
└── README.md                   # This file
```

---

## Key Features

### Skid Build Module
- Scan **Manifest QR** to identify order, supplier, plant, dock
- Scan **Toyota Kanban** to capture part details (part number, kanban, QPC, box number)
- Scan **Internal Kanban** to match with Toyota kanban
- **Palletization code matching** - Validates manifest vs kanban codes
- **Exception handling** - Codes 10, 11, 12, 20 for shortage, qty changes, etc.
- Generates internal reference number (future: Toyota confirmation number)

### Shipment Load Module
- Load skids onto trailers
- Capture driver information
- Route and run validation
- **Exception handling** - Codes 13, 14, 15, 17, 18, 19, 21, 22, 24, 99

### Validation Rules (Toyota API V2.0 Compliant)
- **SkidId**: 3 numeric digits only (e.g., "001", "002")
- **SkidSide**: A or B (extracted from 4th char of manifest skid ID)
- **Order Number**: YYYYMMDD format (except 21TMC plant - alphanumeric)
- **BoxNumber**: 1-999
- **Exception Codes**: Level-specific validation
- **No special characters** in any field
- **Uppercase alpha** enforcement

---

## Technology Stack

| Layer | Technology |
|-------|------------|
| Frontend | Next.js 14, React 18, TypeScript, Tailwind CSS |
| Backend | ASP.NET Core 8, C# 12 |
| Database | SQL Server |
| ORM | Entity Framework Core 8 |
| Authentication | JWT (future: OAuth 2.0 for Toyota API) |

---

## API Endpoints

### Skid Build
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/skid-build/order/{orderNumber}/{dockCode}` | Get order with planned items |
| POST | `/api/v1/skid-build/session/start` | Start skid build session |
| POST | `/api/v1/skid-build/scan` | Record a scan |
| POST | `/api/v1/skid-build/exception` | Record an exception |
| POST | `/api/v1/skid-build/session/{sessionId}/complete` | Complete session |

### Shipment Load
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/shipment-load/start` | Start shipment load |
| POST | `/api/v1/shipment-load/scan-skid` | Scan skid onto trailer |
| POST | `/api/v1/shipment-load/complete` | Complete and submit |

---

## Database Schema (Key Tables)

| Table | Description |
|-------|-------------|
| `tblOrders` | Toyota orders with order number, plant, dock, supplier |
| `tblPlannedItems` | Kanban items planned for each order |
| `tblSkidBuildSessions` | Active/completed skid build sessions |
| `tblSkidScans` | Individual scan records |
| `tblSkidBuildExceptions` | Exception records (10, 11, 12, 20) |

---

## Implementation Status

| Phase | Description | Status |
|-------|-------------|--------|
| Phase 1 | Data Model & Backend | ✅ Complete |
| Phase 2 | Frontend Changes | ✅ Complete |
| Phase 3 | Validation Rules | ✅ Complete |
| Phase 4 | Toyota API Integration | 🔮 Future |

### Current State
- System generates **internal reference numbers** (`SKB-{timestamp}-{random}`)
- Ready for **internal testing**
- **Toyota API integration** pending (will provide real confirmation numbers)

---

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server 2019+

### Backend Setup
```bash
cd Backend
dotnet restore
dotnet ef database update
dotnet run
```

### Frontend Setup
```bash
cd ScannerApp
npm install
npm run dev
```

### Environment Variables

**Backend (appsettings.json)**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=VuteqScanner;Trusted_Connection=True;"
  }
}
```

**Frontend (.env.local)**
```
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## Toyota API Integration (Future - Phase 4)

### Endpoints
| Environment | Token URL | API URL |
|-------------|-----------|---------|
| QA | `https://login.microsoftonline.com/tmnatest.onmicrosoft.com/oauth2/token` | `https://api.dev.scs.toyota.com/spbapi/rest/` |
| Production | `https://login.microsoftonline.com/toyota1.onmicrosoft.com/oauth2/token` | `https://api.scs.toyota.com/spbapi/rest/` |

### Authentication
- OAuth 2.0 with Microsoft Azure AD
- Unique credentials per supplier

---

## Documentation

For detailed implementation plans and business rules, see:
- `FromHassan/skid-build-implementation-plan.md` - Detailed implementation plan
- `FromHassan/toyota_business_rules.md` - Complete Toyota API business rules
- `FromHassan/ORDER_NUMBER_RULES.md` - Order number format rules

---

## Contact

**Author:** Hassan
**Repository:** git@github.com:hassanfarrukh/vuteq_toyota_shipment_scanner.git

---

## License

Proprietary - VUTEQ Internal Use Only
