================================================================================
ORDER UPLOAD API - IMPLEMENTATION SUMMARY
================================================================================
Author: Hassan
Date: 2025-12-01
Description: Complete implementation of Order Data Upload API with PdfPig PDF extraction

================================================================================
OVERVIEW
================================================================================

This implementation provides a complete API for uploading PDF files containing
TMMI Daily One-Way Kanban Order Summary Reports, extracting order data using
PdfPig, and storing the data in the database.

================================================================================
FEATURES
================================================================================

1. PDF File Upload
   - Accepts PDF files up to 10MB
   - Validates file type and size
   - Stores files in wwwroot/uploads/orders/

2. PDF Parsing with PdfPig
   - Extracts order header information (Supplier, Dock Code, Order Series, etc.)
   - Parses line items (Part Numbers, Descriptions, Quantities)
   - Handles multiple orders per PDF page
   - Supports multiple pages per PDF

3. Database Storage
   - Creates Order records with extracted header data
   - Creates PlannedItem records for each line item
   - Generates unique OWK Numbers: SupplierCode-DockCode-OrderSeries-OrderNumber
   - Tracks upload history with status

4. Error Handling
   - Comprehensive validation
   - Graceful error recovery
   - Detailed error logging
   - Status tracking (pending, processing, success, error)

================================================================================
FILES CREATED
================================================================================

DTOs (Models/DTOs/)
-------------------
1. OrderUploadRequestDto.cs          - Upload request model
2. OrderUploadResponseDto.cs         - Upload response with summary
3. ExtractedOrderDto.cs              - Parsed order header data
4. ExtractedOrderItemDto.cs          - Parsed line item data

Services (Services/)
--------------------
1. PdfParserService.cs               - PDF parsing using PdfPig
2. OrderUploadService.cs             - Upload and processing logic

Repositories (Repositories/)
----------------------------
1. OrderUploadRepository.cs          - OrderUpload data access
2. OrderRepository.cs                - Order and PlannedItem data access

Controllers (Controllers/)
--------------------------
1. OrderUploadController.cs          - REST API endpoints

Configuration
-------------
- Program.cs updated with DI registrations
- EF Core migration created: AddOrderUploadFeature

================================================================================
API ENDPOINTS
================================================================================

Base URL: http://localhost:5000/api/v1/orders

1. Upload Order PDF
   POST /upload
   - Headers: Authorization: Bearer {JWT_TOKEN}
   - Body: multipart/form-data with "file" field
   - Returns: OrderUploadResponseDto with extracted orders

2. Get Upload History
   GET /uploads
   - Headers: Authorization: Bearer {JWT_TOKEN}
   - Returns: List of all uploads

3. Get Upload by ID
   GET /uploads/{id}
   - Headers: Authorization: Bearer {JWT_TOKEN}
   - Returns: Specific upload details

4. Delete Upload
   DELETE /uploads/{id}
   - Headers: Authorization: Bearer {JWT_TOKEN}
   - Returns: Success/failure status

================================================================================
USAGE EXAMPLE
================================================================================

1. Start the API:
   cd D:\VUTEQ\FromHassan\Codes\Backend
   dotnet run

2. Login to get JWT token:
   POST http://localhost:5000/api/v1/auth/login
   Body: {"username":"cisg","password":"cisg1234"}

3. Upload PDF file:
   POST http://localhost:5000/api/v1/orders/upload
   Headers: Authorization: Bearer {YOUR_JWT_TOKEN}
   Body: multipart/form-data with file field

4. View upload history:
   GET http://localhost:5000/api/v1/orders/uploads
   Headers: Authorization: Bearer {YOUR_JWT_TOKEN}

================================================================================
SAMPLE REQUEST (cURL)
================================================================================

# Login
curl -X POST "http://localhost:5000/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"cisg","password":"cisg1234"}'

# Upload PDF
curl -X POST "http://localhost:5000/api/v1/orders/upload" \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}" \
  -F "file=@/path/to/order.pdf"

# Get uploads
curl -X GET "http://localhost:5000/api/v1/orders/uploads" \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}"

================================================================================
SAMPLE RESPONSE
================================================================================

{
  "success": true,
  "message": "Successfully uploaded and processed order_20251117.pdf. Created 3 orders with 15 items.",
  "data": {
    "uploadId": "550e8400-e29b-41d4-a716-446655440000",
    "fileName": "order_20251117.pdf",
    "fileSize": 2048576,
    "uploadDate": "2025-12-01T10:30:00Z",
    "status": "success",
    "ordersCreated": 3,
    "totalItemsCreated": 15,
    "extractedOrders": [
      {
        "owkNumber": "02806-FL-20251117-001",
        "customerName": "AGC Automotive",
        "supplierCode": "02806",
        "dockCode": "FL",
        "orderSeries": "20251117",
        "orderNumber": "001",
        "orderDate": "2025-11-17T00:00:00Z",
        "itemCount": 5,
        "items": [
          {
            "partNumber": "68101-0E120-00",
            "description": "GLASS SUB-ASSY FR",
            "lotQty": 45,
            "kanbanNumber": "FA99",
            "plannedQty": 90,
            "rawKanbanValue": "FA99"
          }
        ]
      }
    ]
  },
  "errors": []
}

================================================================================
PDF FORMAT REQUIREMENTS
================================================================================

The PDF parser expects "TMMI Daily One-Way Kanban Order Summary Report" format:

Header Information:
- Transmit Date: YYYY/MM/DD format
- Supplier Name: Text
- Supplier Code: 5-digit code (e.g., 02806)
- NAMC Dock Code: 1-3 character code (e.g., FL, H8, HL, ML, T6)
- Order Series: 8-digit number (e.g., 20251117)
- Order Numbers: 3-digit numbers (e.g., 001, 002, 003)

Line Items Table:
- Part Number: Format XXXXX-XXXXX-XX
- Part Description: Text
- Lot Qty: 5-digit quantity
- Kanban Number: Alphanumeric (e.g., FA99, FAA0)
- Lots Ordered: Integer quantities per order number column

================================================================================
DATABASE SCHEMA
================================================================================

Tables Used:
------------

1. tblOrderUploads
   - Id (Guid, PK)
   - FileName (nvarchar(500))
   - FileSize (bigint)
   - FilePath (nvarchar(1000))
   - Status (nvarchar(20))
   - UploadedBy (Guid, FK to UserMasters)
   - UploadDate (datetime2)
   - ErrorMessage (nvarchar(max))

2. tblOrders
   - OrderId (Guid, PK)
   - OwkNumber (nvarchar(50), Unique)
   - CustomerName (nvarchar(200))
   - SupplierCode (nvarchar(20))
   - DockCode (nvarchar(20))
   - OrderDate (datetime2)
   - Status (nvarchar(50))

3. tblPlannedItems
   - PlannedItemId (Guid, PK)
   - OwkNumber (nvarchar(50), FK to Orders)
   - PartNumber (nvarchar(100))
   - Description (nvarchar(500))
   - PlannedQty (int)
   - RawKanbanValue (nvarchar(max))
   - SupplierCode (nvarchar(20))

================================================================================
LOGGING
================================================================================

All operations are logged using Serilog:
- Upload attempts and results
- PDF parsing progress
- Database operations
- Errors and warnings

Log files location: D:\VUTEQ\FromHassan\Codes\Backend\logs\

================================================================================
TESTING
================================================================================

1. Use Swagger UI:
   - Navigate to http://localhost:5000
   - Use "Authorize" button to set JWT token
   - Test endpoints interactively

2. Use Postman:
   - Import endpoints from Swagger JSON
   - Configure Bearer token authentication
   - Upload test PDF files

3. Use test script:
   - Run: D:\VUTEQ\FromHassan\scripts\test-order-upload-api.bat
   - Update JWT_TOKEN variable after login
   - Place test PDF in script directory

================================================================================
TROUBLESHOOTING
================================================================================

Issue: "Only PDF files are allowed"
Solution: Ensure file has .pdf extension and Content-Type is application/pdf

Issue: "File size must be less than 10MB"
Solution: Compress or split large PDF files

Issue: "Failed to parse PDF file"
Solution: Ensure PDF follows expected format (TMMI Daily One-Way Kanban Order Summary Report)

Issue: "Order already exists"
Solution: Orders with duplicate OWK numbers are skipped (logged as warning)

Issue: "Authentication failed"
Solution: Ensure valid JWT token in Authorization header

================================================================================
DEPENDENCIES
================================================================================

NuGet Packages:
- UglyToad.PdfPig 1.7.0-custom-5 (PDF parsing)
- Microsoft.EntityFrameworkCore (Database)
- Microsoft.AspNetCore.Authentication.JwtBearer (Authentication)
- Serilog (Logging)

================================================================================
SECURITY
================================================================================

1. Authentication: All endpoints require JWT authentication
2. Authorization: Only SUPERVISOR and ADMIN roles can upload (frontend enforced)
3. File Validation: Type and size validation before processing
4. Path Security: Files stored in controlled directory with unique names
5. SQL Injection: EF Core parameterized queries prevent injection

================================================================================
NEXT STEPS
================================================================================

1. Frontend Integration:
   - Update upload-order page to call real API endpoints
   - Remove localStorage mock data
   - Display extracted order data in UI

2. Enhancements:
   - Add background processing for large files
   - Implement file preview before upload
   - Add order deduplication options
   - Export extracted data to Excel/CSV

3. Testing:
   - Create unit tests for PDF parser
   - Add integration tests for upload flow
   - Test with various PDF formats

================================================================================
SUPPORT
================================================================================

For issues or questions:
- Check logs in D:\VUTEQ\FromHassan\Codes\Backend\logs\
- Review Swagger documentation at http://localhost:5000
- Contact: Hassan

================================================================================
END OF DOCUMENTATION
================================================================================
