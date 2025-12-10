// Author: Hassan
// Date: 2025-12-04
// Description: Interface for Excel parsing operations - TSCS Compliance Dashboard

using Backend.Models.DTOs;

namespace Backend.Services;

/// <summary>
/// Interface for Excel parsing operations
/// </summary>
public interface IExcelParserService
{
    /// <summary>
    /// Parse TSCS Compliance Dashboard Excel file
    /// Extracts NAMC summary and pending shipment details
    /// </summary>
    /// <param name="filePath">Full path to the Excel file</param>
    /// <returns>Parsed data containing summary and shipment details</returns>
    Task<ExcelParseResult> ParseComplianceDashboardAsync(string filePath);
}
