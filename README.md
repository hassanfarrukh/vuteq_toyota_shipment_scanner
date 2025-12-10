# VUTEQ Toyota Shipment Scanner System

**Author:** Hassan
**Created:** 2025-11-23
**Last Updated:** 2025-12-11
**Repository:** `git@github.com:hassanfarrukh/vuteq_toyota_shipment_scanner.git`

---

## Overview

A **Toyota SCS (Shipping Confirmation System) API-compliant** barcode scanning application for VUTEQ warehouse operations. The system handles the complete Toyota parts shipment workflow from pallet building to trailer loading.

---

## System Components

| Component | Technology | Description |
|-----------|------------|-------------|
| **Backend** | ASP.NET Core 8 | REST API with EF Core, JWT auth, Toyota validation |
| **ScannerApp** | Next.js 14 | Mobile-first scanning UI for Zebra TC51/52/70/72 |
| **Docker** | Docker Compose | Containerized deployment for dev and production |

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    SCANNERAPP (Next.js 14)                  │
│         Mobile Scanner UI - Zebra TC51/52/70/72             │
│  ┌───────────┐  ┌───────────┐  ┌─────────────────┐         │
│  │Skid Build │  │ Shipment  │  │  Pre-Shipment   │         │
│  │  Module   │  │Load Module│  │   Scan Module   │         │
│  └─────┬─────┘  └─────┬─────┘  └───────┬─────────┘         │
└────────┼──────────────┼────────────────┼────────────────────┘
         │              │                │
         └──────────────┼────────────────┘
                        │ REST API
┌───────────────────────┼─────────────────────────────────────┐
│                       ▼                                     │
│              BACKEND (ASP.NET Core 8)                       │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────┐     │
│  │ Controllers │  │  Services   │  │ToyotaValidation │     │
│  └──────┬──────┘  └──────┬──────┘  │    Service      │     │
│         │                │         └─────────────────┘     │
│         └────────────────┼──────────────────────────────┘  │
│                          │                                  │
│                    ┌─────▼─────┐                            │
│                    │ EF Core 8 │                            │
│                    └─────┬─────┘                            │
└──────────────────────────┼──────────────────────────────────┘
                           │
                    ┌──────▼──────┐
                    │ SQL Server  │
                    └─────────────┘
```

---

## Key Features

### Skid Build (Pallet Build)
- Scan Manifest QR → Toyota Kanban → Internal Kanban
- Palletization code matching validation
- Exception handling (codes 10, 11, 12, 20)
- Toyota API V2.0 compliant validation

### Shipment Load
- Driver check sheet verification
- Skid-to-trailer loading workflow
- Route/Run validation
- Exception handling (codes 13-24, 99)

### Pre-Shipment Scan
- Final verification before departure
- Skid count validation

---

## Project Structure

```
Codes/
├── Backend/              # ASP.NET Core 8 Web API
│   ├── Controllers/      # API endpoints
│   ├── Services/         # Business logic + ToyotaValidationService
│   ├── Models/           # Entities & DTOs
│   ├── Repositories/     # Data access layer
│   └── README.md         # Backend documentation
│
├── ScannerApp/           # Next.js 14 Frontend
│   ├── app/              # Pages (skid-build, shipment-load, etc.)
│   ├── components/       # Reusable UI components
│   ├── lib/              # API client & utilities
│   └── README.md         # Frontend documentation
│
├── Docker/               # Container configurations
│   ├── development/      # Dev environment (docker-compose)
│   └── prod/             # Production deployment
│
└── README.md             # This file
```

---

## Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- SQL Server 2019+
- Docker (optional)

### Backend
```bash
cd Backend
dotnet restore
dotnet ef database update
dotnet run
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Frontend
```bash
cd ScannerApp
npm install
npm run dev
# App: http://localhost:3000
```

### Docker (Full Stack)
```bash
cd Docker/development
docker-compose up -d
```

---

## Implementation Status

| Phase | Description | Status |
|-------|-------------|--------|
| Phase 1 | Data Model & Backend | ✅ Complete |
| Phase 2 | Frontend Changes | ✅ Complete |
| Phase 3 | Toyota Validation Rules | ✅ Complete |
| Phase 4 | Toyota API Integration | 🔮 Future |

### Toyota Validation Rules Implemented
- SkidId: 3 numeric digits
- SkidSide: A or B
- Order Number: YYYYMMDD format (21TMC exception)
- Palletization code matching
- Exception code validation
- No special characters
- Uppercase alpha enforcement

---

## Documentation

| Document | Location |
|----------|----------|
| Backend API Details | `Backend/README.md` |
| Frontend Guide | `ScannerApp/README.md` |
| Implementation Plan | `FromHassan/skid-build-implementation-plan.md` |
| Toyota Business Rules | `FromHassan/toyota_business_rules.md` |

---

## API Endpoints Summary

### Skid Build
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/skid-build/order/{orderNumber}/{dockCode}` | Get order |
| POST | `/api/v1/skid-build/session/start` | Start session |
| POST | `/api/v1/skid-build/scan` | Record scan |
| POST | `/api/v1/skid-build/exception` | Record exception |
| POST | `/api/v1/skid-build/session/{id}/complete` | Complete |

### Shipment Load
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v1/shipment-load/start` | Start load |
| POST | `/api/v1/shipment-load/scan-skid` | Scan skid |
| POST | `/api/v1/shipment-load/complete` | Complete |

---

## Toyota API Integration (Future)

| Environment | API URL |
|-------------|---------|
| QA | `https://api.dev.scs.toyota.com/spbapi/rest/` |
| Production | `https://api.scs.toyota.com/spbapi/rest/` |

Authentication: OAuth 2.0 via Microsoft Azure AD

---

## License

Proprietary - VUTEQ Internal Use Only

---

## Contact

**Author:** Hassan
