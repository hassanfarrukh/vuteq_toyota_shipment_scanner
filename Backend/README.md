# VUTEQ Scanner API

**Author:** Hassan
**Date:** 2025-11-11
**Version:** 1.0.0

## Overview

ASP.NET Core 8.0 Web API for the VUTEQ Scanner Application - Toyota Manufacturing Scanning System. This API provides authentication, session management, and will support 63 total endpoints for order management, skid building, shipment loading, and administrative functions.

## Technology Stack

- **Framework:** ASP.NET Core 8.0
- **ORM (Primary):** Entity Framework Core 8.0 (Code-First)
- **Database Access (Secondary):** Dapper (Complex Queries when needed)
- **Database:** SQL Server (VUTEQ_Scanner)
- **Authentication:** JWT Bearer Tokens
- **API Documentation:** Swagger/OpenAPI
- **Database Provider:** Microsoft.EntityFrameworkCore.SqlServer

## Project Structure

```
Backend/
├── Controllers/          # API endpoints (thin controllers)
│   └── AuthController.cs
├── Services/            # Business logic layer
│   └── AuthService.cs
├── Data/                # EF Core Data Context
│   ├── VuteqDbContext.cs
│   └── Configurations/  # Entity configurations
├── Entities/            # Domain entities (30 total)
│   ├── User.cs
│   ├── UserSession.cs
│   ├── Order.cs
│   └── [27 more entities...]
├── Migrations/          # EF Core code-first migrations
├── Models/              # DTOs, request/response models
│   ├── ApiResponse.cs
│   ├── LoginRequest.cs
│   ├── LoginResponse.cs
│   └── SessionValidationResponse.cs
├── Middleware/          # Error handling, authentication
│   └── ErrorHandlingMiddleware.cs
├── Configuration/       # Settings classes
│   └── JwtConfiguration.cs
├── Program.cs          # Application entry point
├── Backend.csproj      # Project file
└── appsettings.json    # Configuration
```

## Database Connection

**Connection String:**
```
Server=localhost,1433;Database=VUTEQ_Scanner;User Id=sa;Password=VuteqDev2025!;TrustServerCertificate=True;
```

## Authentication APIs Implemented

### 1. POST /api/auth/login
- **Purpose:** User authentication with JWT token generation
- **Request Body:**
  ```json
  {
    "username": "string",
    "password": "string"
  }
  ```
- **Response (Success):**
  ```json
  {
    "success": true,
    "token": "eyJhbGc...",
    "user": {
      "id": "string",
      "username": "string",
      "name": "string",
      "role": "ADMIN | SUPERVISOR | OPERATOR",
      "locationId": "string"
    }
  }
  ```
- **Data Access:** EF Core LINQ query on Users entity

### 2. GET /api/auth/v1/auth/session/validate
- **Purpose:** Validate existing JWT token and session
- **Headers:** `Authorization: Bearer {token}`
- **Response (Valid):**
  ```json
  {
    "valid": true,
    "user": {
      "id": "string",
      "username": "string",
      "name": "string",
      "role": "string",
      "locationId": "string"
    }
  }
  ```
- **Data Access:** EF Core query on UserSessions entity

## Getting Started

### Prerequisites

1. .NET 8.0 SDK installed
2. SQL Server running on localhost:1433
3. VUTEQ_Scanner database created with schema from `F:\VUTEQ\FromHassan\Databases\vuteq_scanner.sql`

### Build and Run

```bash
# Navigate to project directory
cd F:\VUTEQ\FromHassan\Codes\Backend

# Restore NuGet packages
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

The API will start on:
- **HTTPS:** https://localhost:7001
- **HTTP:** http://localhost:5000
- **Swagger UI:** https://localhost:7001 (root path)

## Testing with Swagger

1. Navigate to https://localhost:7001
2. Expand `/api/auth/login` endpoint
3. Click "Try it out"
4. Enter test credentials:
   ```json
   {
     "username": "testuser",
     "password": "testpass"
   }
   ```
5. Copy the returned JWT token
6. Click "Authorize" button at top
7. Enter: `Bearer {your-token-here}`
8. Test `/api/auth/v1/auth/session/validate` endpoint

## CORS Configuration

Configured to allow requests from:
- `http://localhost:3000` (Next.js frontend)

## JWT Token Configuration

- **Regular Users:** 480 minutes (8 hours)
- **Supervisors:** 720 minutes (12 hours)
- **Algorithm:** HS256 (HMAC-SHA256)
- **Claims:** Sub, UniqueName, Name, Role, LocationId

## Security Notes

1. **Password Hashing:** Currently using SHA256 for compatibility. **IMPORTANT:** Upgrade to bcrypt or argon2 for production.
2. **HTTPS:** Always use HTTPS in production.
3. **JWT Secret:** Change the secret key in production to a cryptographically secure random value.
4. **Rate Limiting:** Not yet implemented - add before production deployment.

## Next Steps

The following API categories will be implemented next:

1. **Order Upload Management (3 APIs)**
2. **Skid Build Workflow (15 APIs)**
3. **Shipment Load Workflow (10 APIs)**
4. **Pre-Shipment Scan Workflow (10 APIs)**
5. **Dock Monitor (4 APIs)**
6. **Administration (19 APIs)**

**Total APIs:** 63 (2 completed, 61 remaining)

## API Requirements Reference

All API specifications are defined in:
`F:\VUTEQ\FromHassan\api-requirements-final.md`

## Database Schema Reference

Database schema and stored procedures:
`F:\VUTEQ\FromHassan\Databases\vuteq_scanner.sql`

## Dependencies

```xml
<PackageReference Include="Dapper" Version="2.1.28" />
<PackageReference Include="Microsoft.Data.SqlClient" Version="5.1.5" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.1.2" />
```

## Error Handling

Global error handling middleware (`ErrorHandlingMiddleware`) catches all unhandled exceptions and returns standardized error responses:

```json
{
  "success": false,
  "message": "An internal server error occurred.",
  "data": null,
  "errors": ["Error details"]
}
```

## Logging

Configured logging levels:
- **Development:** Debug level
- **Production:** Information level
- **ASP.NET Core:** Warning level

## Support

For questions or issues, contact Hassan.
