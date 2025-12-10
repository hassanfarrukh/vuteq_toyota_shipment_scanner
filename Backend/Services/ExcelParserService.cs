// Author: Hassan
// Date: 2025-12-04
// Description: Service for parsing TSCS Compliance Dashboard Excel files using ClosedXML
// Extracts NAMC summary and pending shipment details

using Backend.Models.DTOs;
using ClosedXML.Excel;
using System.Globalization;

namespace Backend.Services;

/// <summary>
/// Service implementation for parsing Excel files using ClosedXML
/// </summary>
public class ExcelParserService : IExcelParserService
{
    private readonly ILogger<ExcelParserService> _logger;

    // Sheet names in TSCS Compliance Dashboard
    private const string NAMC_DETAIL_SHEET = "NAMC Detail";
    private const string SHIPMENT_DETAIL_SHEET = "Shipment Detail";

    // Filter criteria
    private const string PENDING_STATUS = "Pending";

    public ExcelParserService(ILogger<ExcelParserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse TSCS Compliance Dashboard Excel file
    /// Extracts summary from NAMC Detail sheet and pending shipments from Shipment Detail sheet
    /// </summary>
    public async Task<ExcelParseResult> ParseComplianceDashboardAsync(string filePath)
    {
        var result = new ExcelParseResult();

        try
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Excel file not found: {filePath}");
            }

            _logger.LogInformation("Starting Excel parsing: {FilePath}", filePath);

            // Open Excel workbook
            using (var workbook = new XLWorkbook(filePath))
            {
                _logger.LogInformation("Excel workbook opened successfully. Total sheets: {SheetCount}", workbook.Worksheets.Count);

                // Parse NAMC Detail sheet (summary)
                result.Summary = ParseNamcDetailSheet(workbook);

                // Parse Shipment Detail sheet (pending shipments only)
                result.Shipments = ParseShipmentDetailSheet(workbook);
            }

            _logger.LogInformation("Excel parsing completed. Summary: SupplierCode={SupplierCode}, Pending={Pending}, Shipments={ShipmentCount}",
                result.Summary.SupplierCode, result.Summary.TotalPending, result.Shipments.Count);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Excel file: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to parse Excel file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parse NAMC Detail sheet to extract summary data
    /// Expects one data row with columns: SUPPLIER, PLANT CODE, PLANNED, SHIPPED, SHORTED, LATE, PENDING
    /// </summary>
    private NamcSummary ParseNamcDetailSheet(XLWorkbook workbook)
    {
        var summary = new NamcSummary();

        try
        {
            _logger.LogInformation("Parsing NAMC Detail sheet...");

            // Get NAMC Detail worksheet
            var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == NAMC_DETAIL_SHEET);
            if (worksheet == null)
            {
                _logger.LogWarning("NAMC Detail sheet not found in workbook");
                return summary;
            }

            // Find header row (row containing "SUPPLIER")
            var headerRow = worksheet.RowsUsed().FirstOrDefault(row =>
                row.CellsUsed().Any(cell => cell.GetString().Trim().Equals("SUPPLIER", StringComparison.OrdinalIgnoreCase)));

            if (headerRow == null)
            {
                _logger.LogWarning("Header row not found in NAMC Detail sheet");
                return summary;
            }

            int headerRowNumber = headerRow.RowNumber();
            _logger.LogDebug("NAMC Detail header row found at row {RowNumber}", headerRowNumber);

            // Map column names to indices
            var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var columnName = cell.GetString().Trim();
                if (!string.IsNullOrEmpty(columnName))
                {
                    columnMap[columnName] = cell.Address.ColumnNumber;
                }
            }

            _logger.LogDebug("NAMC Detail columns found: {Columns}", string.Join(", ", columnMap.Keys));

            // Get data row (first row after header)
            var dataRow = worksheet.Row(headerRowNumber + 1);
            if (dataRow == null || !dataRow.CellsUsed().Any())
            {
                _logger.LogWarning("No data row found in NAMC Detail sheet");
                return summary;
            }

            // Extract values
            summary.SupplierCode = GetIntValue(dataRow, columnMap, "SUPPLIER");
            summary.PlantCode = GetStringValue(dataRow, columnMap, "PLANT CODE");
            summary.TotalPlanned = GetIntValue(dataRow, columnMap, "PLANNED");
            summary.TotalShipped = GetIntValue(dataRow, columnMap, "SHIPPED");
            summary.TotalShorted = GetIntValue(dataRow, columnMap, "SHORTED");
            summary.TotalLate = GetIntValue(dataRow, columnMap, "LATE");
            summary.TotalPending = GetIntValue(dataRow, columnMap, "PENDING");

            _logger.LogInformation("NAMC Summary parsed: Supplier={SupplierCode}, Plant={PlantCode}, Planned={Planned}, Pending={Pending}",
                summary.SupplierCode, summary.PlantCode, summary.TotalPlanned, summary.TotalPending);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing NAMC Detail sheet");
            throw;
        }

        return summary;
    }

    /// <summary>
    /// Parse Shipment Detail sheet to extract pending shipments
    /// Filters by SHIPMENT STATUS = 'Pending'
    /// </summary>
    private List<ParsedShipment> ParseShipmentDetailSheet(XLWorkbook workbook)
    {
        var shipments = new List<ParsedShipment>();

        try
        {
            _logger.LogInformation("Parsing Shipment Detail sheet...");

            // Get Shipment Detail worksheet
            var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == SHIPMENT_DETAIL_SHEET);
            if (worksheet == null)
            {
                _logger.LogWarning("Shipment Detail sheet not found in workbook");
                return shipments;
            }

            // Find header row (row containing "MANIFEST_NO" or "SHIPMENT STATUS")
            var headerRow = worksheet.RowsUsed().FirstOrDefault(row =>
                row.CellsUsed().Any(cell =>
                {
                    var value = cell.GetString().Trim();
                    return value.Equals("MANIFEST_NO", StringComparison.OrdinalIgnoreCase) ||
                           value.Equals("MANIFEST NO", StringComparison.OrdinalIgnoreCase) ||
                           value.Equals("SHIPMENT STATUS", StringComparison.OrdinalIgnoreCase);
                }));

            if (headerRow == null)
            {
                _logger.LogWarning("Header row not found in Shipment Detail sheet");
                return shipments;
            }

            int headerRowNumber = headerRow.RowNumber();
            _logger.LogDebug("Shipment Detail header row found at row {RowNumber}", headerRowNumber);

            // Map column names to indices (handle both underscore and space variants)
            var columnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var columnName = cell.GetString().Trim().Replace(" ", "_");
                if (!string.IsNullOrEmpty(columnName))
                {
                    columnMap[columnName] = cell.Address.ColumnNumber;
                    // Also store original name for fallback
                    columnMap[cell.GetString().Trim()] = cell.Address.ColumnNumber;
                }
            }

            _logger.LogDebug("Shipment Detail columns found: {Count} columns", columnMap.Count);

            // Find SHIPMENT_STATUS column index
            int statusColumnIndex = GetColumnIndex(columnMap, "SHIPMENT_STATUS", "SHIPMENT STATUS");
            if (statusColumnIndex == -1)
            {
                _logger.LogWarning("SHIPMENT STATUS column not found - will process all rows");
            }

            // Process data rows (skip header)
            int totalRows = 0;
            int pendingRows = 0;
            int skippedRows = 0;

            foreach (var row in worksheet.RowsUsed().Skip(headerRowNumber))
            {
                totalRows++;

                try
                {
                    // Check if row is pending (or process all if status column not found)
                    if (statusColumnIndex != -1)
                    {
                        var status = row.Cell(statusColumnIndex).GetString().Trim();
                        if (!status.Equals(PENDING_STATUS, StringComparison.OrdinalIgnoreCase))
                        {
                            skippedRows++;
                            continue; // Skip non-pending rows
                        }
                    }

                    // Extract shipment data
                    var shipment = new ParsedShipment
                    {
                        // Order-level fields
                        ManifestNo = GetLongValue(row, columnMap, "MANIFEST_NO", "MANIFEST NO"),
                        SupplierCode = GetStringValue(row, columnMap, "SUPPLIER"),
                        DockCode = GetStringValue(row, columnMap, "DOCK"),
                        RealOrderNumber = GetStringValue(row, columnMap, "ORDER_NUMBER", "ORDER NUMBER"),
                        TransmitDate = GetDateTimeValue(row, columnMap, "ORDER_DATE", "ORDER DATE"),
                        PlantCode = GetStringValue(row, columnMap, "PLANT_CODE", "PLANT CODE"),
                        PlannedRoute = GetStringValue(row, columnMap, "PLANNED_ROUTE", "PLANNED ROUTE"),
                        MainRoute = GetStringValue(row, columnMap, "MAIN_ROUTE", "MAIN ROUTE"),
                        SpecialistCode = GetNullableIntValue(row, columnMap, "SPECIALIST_CODE", "SPECIALIST CODE"),
                        Mros = GetNullableIntValue(row, columnMap, "MROS"),

                        // Item-level fields
                        PartNumber = GetStringValue(row, columnMap, "PART"),
                        KanbanNumber = GetStringValue(row, columnMap, "KANBAN"),
                        Qpc = GetIntValue(row, columnMap, "QPC"),
                        TotalBoxPlanned = GetIntValue(row, columnMap, "TOTAL_BOX_PLANNED", "TOTAL BOX PLANNED"),
                        PalletizationCode = GetStringValue(row, columnMap, "PALLETIZATION_CODE", "PALLETIZATION CODE"),
                        ExternalOrderId = GetLongValue(row, columnMap, "ORDER_ID", "ORDER ID"),

                        // New fields - Added 2025-12-09
                        PlannedPickup = GetDateTimeValue(row, columnMap, "PLANNED_PICKUP", "PLANNED PICKUP"),
                        ShortOver = GetIntValue(row, columnMap, "SHORT_OVER", "SHORT/OVER"),
                        Pieces = GetIntValue(row, columnMap, "PIECES")
                    };

                    // Parse UNLOAD_DATE into DateOnly and TimeOnly
                    var unloadDateTime = GetDateTimeValue(row, columnMap, "UNLOAD_DATE", "UNLOAD DATE");
                    if (unloadDateTime.HasValue)
                    {
                        shipment.UnloadDate = DateOnly.FromDateTime(unloadDateTime.Value);
                        shipment.UnloadTime = TimeOnly.FromDateTime(unloadDateTime.Value);
                    }

                    shipments.Add(shipment);
                    pendingRows++;

                    if (pendingRows <= 3) // Log first 3 shipments for debugging
                    {
                        _logger.LogDebug("Parsed shipment: ManifestNo={ManifestNo}, Order={OrderNumber}, Part={PartNumber}, Qpc={Qpc}",
                            shipment.ManifestNo, shipment.RealOrderNumber, shipment.PartNumber, shipment.Qpc);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing shipment row {RowNumber} - skipping", row.RowNumber());
                    skippedRows++;
                }
            }

            _logger.LogInformation("Shipment Detail parsing completed: Total rows={Total}, Pending={Pending}, Skipped={Skipped}",
                totalRows, pendingRows, skippedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Shipment Detail sheet");
            throw;
        }

        return shipments;
    }

    // ========== Helper Methods for Data Extraction ==========

    /// <summary>
    /// Get column index by name (supports multiple name variants)
    /// </summary>
    private int GetColumnIndex(Dictionary<string, int> columnMap, params string[] columnNames)
    {
        foreach (var name in columnNames)
        {
            if (columnMap.TryGetValue(name, out int index))
            {
                return index;
            }
        }
        return -1; // Not found
    }

    /// <summary>
    /// Get string value from row by column name
    /// </summary>
    private string GetStringValue(IXLRow row, Dictionary<string, int> columnMap, params string[] columnNames)
    {
        int columnIndex = GetColumnIndex(columnMap, columnNames);
        if (columnIndex == -1)
        {
            _logger.LogDebug("Column not found: {Columns}", string.Join(" or ", columnNames));
            return string.Empty;
        }

        try
        {
            return row.Cell(columnIndex).GetString().Trim();
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Get int value from row by column name
    /// </summary>
    private int GetIntValue(IXLRow row, Dictionary<string, int> columnMap, params string[] columnNames)
    {
        int columnIndex = GetColumnIndex(columnMap, columnNames);
        if (columnIndex == -1)
        {
            _logger.LogDebug("Column not found: {Columns}", string.Join(" or ", columnNames));
            return 0;
        }

        try
        {
            var cell = row.Cell(columnIndex);

            // Try to get as number first
            if (cell.TryGetValue(out double doubleValue))
            {
                return (int)doubleValue;
            }

            // Try to parse as string
            var stringValue = cell.GetString().Trim();
            if (int.TryParse(stringValue, out int intValue))
            {
                return intValue;
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get nullable int value from row by column name
    /// </summary>
    private int? GetNullableIntValue(IXLRow row, Dictionary<string, int> columnMap, params string[] columnNames)
    {
        int columnIndex = GetColumnIndex(columnMap, columnNames);
        if (columnIndex == -1)
        {
            return null;
        }

        try
        {
            var cell = row.Cell(columnIndex);

            // Check if cell is empty
            if (cell.IsEmpty())
            {
                return null;
            }

            // Try to get as number first
            if (cell.TryGetValue(out double doubleValue))
            {
                return (int)doubleValue;
            }

            // Try to parse as string
            var stringValue = cell.GetString().Trim();
            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            if (int.TryParse(stringValue, out int intValue))
            {
                return intValue;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Get long value from row by column name
    /// </summary>
    private long GetLongValue(IXLRow row, Dictionary<string, int> columnMap, params string[] columnNames)
    {
        int columnIndex = GetColumnIndex(columnMap, columnNames);
        if (columnIndex == -1)
        {
            _logger.LogDebug("Column not found: {Columns}", string.Join(" or ", columnNames));
            return 0;
        }

        try
        {
            var cell = row.Cell(columnIndex);

            // Try to get as number first
            if (cell.TryGetValue(out double doubleValue))
            {
                return (long)doubleValue;
            }

            // Try to parse as string
            var stringValue = cell.GetString().Trim();
            if (long.TryParse(stringValue, out long longValue))
            {
                return longValue;
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Get DateTime value from row by column name
    /// </summary>
    private DateTime? GetDateTimeValue(IXLRow row, Dictionary<string, int> columnMap, params string[] columnNames)
    {
        int columnIndex = GetColumnIndex(columnMap, columnNames);
        if (columnIndex == -1)
        {
            return null;
        }

        try
        {
            var cell = row.Cell(columnIndex);

            // Check if cell is empty
            if (cell.IsEmpty())
            {
                return null;
            }

            // Try to get as DateTime directly
            if (cell.TryGetValue(out DateTime dateValue))
            {
                return DateTime.SpecifyKind(dateValue, DateTimeKind.Utc);
            }

            // Try to parse as string
            var stringValue = cell.GetString().Trim();
            if (string.IsNullOrEmpty(stringValue))
            {
                return null;
            }

            // Try multiple date formats
            var dateFormats = new[]
            {
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-dd HH:mm",
                "yyyy-MM-dd",
                "MM/dd/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm",
                "MM/dd/yyyy",
                "M/d/yyyy HH:mm:ss",
                "M/d/yyyy HH:mm",
                "M/d/yyyy"
            };

            foreach (var format in dateFormats)
            {
                if (DateTime.TryParseExact(stringValue, format, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime parsedDate))
                {
                    return DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                }
            }

            // Try general parse as last resort
            if (DateTime.TryParse(stringValue, out DateTime generalDate))
            {
                return DateTime.SpecifyKind(generalDate, DateTimeKind.Utc);
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing DateTime for columns: {Columns}", string.Join(" or ", columnNames));
            return null;
        }
    }
}
