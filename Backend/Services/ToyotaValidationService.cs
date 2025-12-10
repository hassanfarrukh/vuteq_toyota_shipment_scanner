// Author: Hassan
// Date: 2025-12-10
// Description: Toyota API validation service - implements all Toyota SCS API validation rules

using System.Globalization;
using System.Text.RegularExpressions;

namespace Backend.Services;

/// <summary>
/// Validation result for Toyota API validations
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }

    public static ValidationResult Success() => new ValidationResult { IsValid = true };
    public static ValidationResult Error(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
}

/// <summary>
/// Interface for Toyota validation service operations
/// </summary>
public interface IToyotaValidationService
{
    ValidationResult ValidateOrderNumber(string orderNumber, string plantCode);
    ValidationResult ValidateSupplierCode(string supplierCode);
    ValidationResult ValidatePlantCode(string plantCode);
    ValidationResult ValidateDockCode(string dockCode);
    ValidationResult ValidateSkidId(string skidId);
    ValidationResult ValidatePartNumber(string partNumber);
    ValidationResult ValidateKanban(string kanban);
    ValidationResult ValidateQpc(int qpc);
    ValidationResult ValidateBoxNumber(int boxNumber);
    ValidationResult ValidateExceptionCode(string exceptionCode, string level);
    ValidationResult ValidatePalletizationCode(string manifestPalletization, string kanbanPalletization);
    ValidationResult ValidateNoSpecialCharacters(string value, string fieldName);
    ValidationResult ValidateUpperCase(string value, string fieldName);
}

/// <summary>
/// Service implementation for Toyota API validation rules
/// Based on Toyota API Specification V2.0 and toyota_business_rules.md
/// </summary>
public class ToyotaValidationService : IToyotaValidationService
{
    // Valid exception codes for different levels
    private static readonly HashSet<string> ValidSkidBuildExceptionCodes = new() { "10", "11", "12", "20" };
    private static readonly HashSet<string> ValidShipmentLoadTrailerExceptionCodes = new() { "13", "17", "24", "99" };
    private static readonly HashSet<string> ValidShipmentLoadSkidExceptionCodes = new() { "14", "15", "18", "19", "21", "22" };

    /// <summary>
    /// Validate Order Number format
    /// Rule: YYYYMMDD format for positions 1-8 (except 21TMC), positions 9-10 numeric, positions 11-12 alpha uppercase
    /// Source: toyota_business_rules.md - Section 7.2
    /// </summary>
    public ValidationResult ValidateOrderNumber(string orderNumber, string plantCode)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return ValidationResult.Error("Order number is required");

        // No special characters allowed
        var noSpecialCharsResult = ValidateNoSpecialCharacters(orderNumber, "Order number");
        if (!noSpecialCharsResult.IsValid)
            return noSpecialCharsResult;

        // 21TMC has different format - alphanumeric code (not date-based)
        if (plantCode == "21TMC")
            return ValidationResult.Success(); // Skip YYYYMMDD validation for TMMC

        // Order number must be 10 or 12 digits
        if (orderNumber.Length != 10 && orderNumber.Length != 12)
            return ValidationResult.Error("Order number must be 10 or 12 digits");

        // Rule: YYYYMMDD format for digits 1-8
        if (orderNumber.Length >= 8)
        {
            var datePart = orderNumber.Substring(0, 8);
            if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null, DateTimeStyles.None, out _))
                return ValidationResult.Error("Order number digits 1-8 must be YYYYMMDD format");
        }

        // Rule: 9th and 10th digit must be numeric
        if (orderNumber.Length >= 10)
        {
            var seqPart = orderNumber.Substring(8, 2);
            if (!int.TryParse(seqPart, out _))
                return ValidationResult.Error("Order number digits 9-10 must be numeric");
        }

        // Rule: 11th and 12th digit must be Alpha Upper Case (if present)
        if (orderNumber.Length == 12)
        {
            var suffix = orderNumber.Substring(10, 2);
            if (!Regex.IsMatch(suffix, @"^[A-Z]{2}$"))
                return ValidationResult.Error("Order number digits 11-12 must be uppercase alpha");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Supplier Code
    /// Rule: 5 digits alphanumeric
    /// Source: toyota_business_rules.md - Section 7.3
    /// </summary>
    public ValidationResult ValidateSupplierCode(string supplierCode)
    {
        if (string.IsNullOrWhiteSpace(supplierCode))
            return ValidationResult.Error("Supplier code is required");

        // No special characters allowed
        var noSpecialCharsResult = ValidateNoSpecialCharacters(supplierCode, "Supplier code");
        if (!noSpecialCharsResult.IsValid)
            return noSpecialCharsResult;

        // Must be 5 digits alphanumeric
        if (!Regex.IsMatch(supplierCode, @"^[a-zA-Z0-9]{5}$"))
            return ValidationResult.Error("Supplier code must be 5 alphanumeric characters");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Plant Code
    /// Rule: Format like 01TMK, 02TMI - 2 digits + 3 alpha uppercase
    /// Source: toyota_business_rules.md - Section 4.3
    /// </summary>
    public ValidationResult ValidatePlantCode(string plantCode)
    {
        if (string.IsNullOrWhiteSpace(plantCode))
            return ValidationResult.Error("Plant code is required");

        // No special characters allowed
        var noSpecialCharsResult = ValidateNoSpecialCharacters(plantCode, "Plant code");
        if (!noSpecialCharsResult.IsValid)
            return noSpecialCharsResult;

        // Format: 2 digits + 3 alpha uppercase
        if (!Regex.IsMatch(plantCode, @"^[0-9]{2}[A-Z]{3}$"))
            return ValidationResult.Error("Plant code must be in format: 2 digits + 3 uppercase alpha (e.g., 01TMK)");

        // Alpha characters must be uppercase
        var alphaResult = ValidateUpperCase(plantCode.Substring(2), "Plant code");
        if (!alphaResult.IsValid)
            return alphaResult;

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Dock Code
    /// Rule: Max 3 digits alphanumeric
    /// Source: toyota_business_rules.md - Section 4.2
    /// </summary>
    public ValidationResult ValidateDockCode(string dockCode)
    {
        if (string.IsNullOrWhiteSpace(dockCode))
            return ValidationResult.Error("Dock code is required");

        // No special characters allowed
        var noSpecialCharsResult = ValidateNoSpecialCharacters(dockCode, "Dock code");
        if (!noSpecialCharsResult.IsValid)
            return noSpecialCharsResult;

        // Max 3 digits alphanumeric
        if (!Regex.IsMatch(dockCode, @"^[a-zA-Z0-9]{1,3}$"))
            return ValidationResult.Error("Dock code must be 1-3 alphanumeric characters");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Skid ID
    /// Rule: Must be exactly 3 numeric digits
    /// Source: toyota_business_rules.md - Section 4.4, 4.5
    /// </summary>
    public ValidationResult ValidateSkidId(string skidId)
    {
        if (string.IsNullOrWhiteSpace(skidId))
            return ValidationResult.Error("Skid ID is required");

        // Must be exactly 3 numeric digits
        if (!Regex.IsMatch(skidId, @"^\d{3}$"))
            return ValidationResult.Error("Skid ID must be exactly 3 numeric digits (e.g., 001, 002)");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Part Number
    /// Rule: 10-12 digits alphanumeric, alpha uppercase
    /// Source: toyota_business_rules.md - Section 4.8
    /// </summary>
    public ValidationResult ValidatePartNumber(string partNumber)
    {
        if (string.IsNullOrWhiteSpace(partNumber))
            return ValidationResult.Error("Part number is required");

        // No special characters allowed
        var noSpecialCharsResult = ValidateNoSpecialCharacters(partNumber, "Part number");
        if (!noSpecialCharsResult.IsValid)
            return noSpecialCharsResult;

        // 10-12 digits alphanumeric
        if (!Regex.IsMatch(partNumber, @"^[A-Z0-9]{10,12}$"))
            return ValidationResult.Error("Part number must be 10-12 alphanumeric characters with uppercase alpha");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Kanban
    /// Rule: 4 digits alphanumeric
    /// Source: toyota_business_rules.md - Section 4.8
    /// </summary>
    public ValidationResult ValidateKanban(string kanban)
    {
        if (string.IsNullOrWhiteSpace(kanban))
            return ValidationResult.Error("Kanban is required");

        // No special characters allowed
        var noSpecialCharsResult = ValidateNoSpecialCharacters(kanban, "Kanban");
        if (!noSpecialCharsResult.IsValid)
            return noSpecialCharsResult;

        // 4 digits alphanumeric
        if (!Regex.IsMatch(kanban, @"^[a-zA-Z0-9]{4}$"))
            return ValidationResult.Error("Kanban must be 4 alphanumeric characters");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate QPC (Quantity Per Container)
    /// Rule: Numeric only, greater than 0
    /// Source: toyota_business_rules.md - Section 4.8
    /// </summary>
    public ValidationResult ValidateQpc(int qpc)
    {
        if (qpc <= 0)
            return ValidationResult.Error("QPC must be greater than 0");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Box Number
    /// Rule: 1-3 digits numeric (1-999)
    /// Source: toyota_business_rules.md - Section 4.8
    /// </summary>
    public ValidationResult ValidateBoxNumber(int boxNumber)
    {
        if (boxNumber < 1 || boxNumber > 999)
            return ValidationResult.Error("Box number must be between 1 and 999");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Exception Code
    /// Rule: Must be valid for the specified level
    /// Skid Build: 10, 11, 12, 20
    /// Shipment Load Trailer: 13, 17, 24, 99
    /// Shipment Load Skid: 14, 15, 18, 19, 21, 22
    /// Source: toyota_business_rules.md - Section 4.9, 5.8, 5.9
    /// </summary>
    public ValidationResult ValidateExceptionCode(string exceptionCode, string level)
    {
        if (string.IsNullOrWhiteSpace(exceptionCode))
            return ValidationResult.Error("Exception code is required");

        var validCodes = level.ToLower() switch
        {
            "skid_build_order" => ValidSkidBuildExceptionCodes,
            "shipment_load_trailer" => ValidShipmentLoadTrailerExceptionCodes,
            "shipment_load_skid" => ValidShipmentLoadSkidExceptionCodes,
            _ => throw new ArgumentException($"Unknown exception level: {level}")
        };

        if (!validCodes.Contains(exceptionCode))
        {
            return ValidationResult.Error(
                $"Invalid exception code '{exceptionCode}' for {level}. Valid codes: {string.Join(", ", validCodes)}");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Palletization Code Matching
    /// Rule: Manifest palletization code must match Kanban palletization code
    /// Source: toyota_business_rules.md - Section 2.6, BR-014
    /// </summary>
    public ValidationResult ValidatePalletizationCode(string manifestPalletization, string kanbanPalletization)
    {
        if (string.IsNullOrWhiteSpace(manifestPalletization) || string.IsNullOrWhiteSpace(kanbanPalletization))
            return ValidationResult.Success(); // Skip if either is null (optional fields)

        if (manifestPalletization != kanbanPalletization)
        {
            return ValidationResult.Error(
                $"Palletization code mismatch. Manifest: '{manifestPalletization}', Kanban: '{kanbanPalletization}'");
        }

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate No Special Characters
    /// Rule: No special characters allowed
    /// Source: toyota_business_rules.md - Section 7.1, VAL-001
    /// </summary>
    public ValidationResult ValidateNoSpecialCharacters(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Success();

        // Allow only alphanumeric and hyphen (for part numbers like 56089-08E90-00)
        if (!Regex.IsMatch(value, @"^[a-zA-Z0-9-]+$"))
            return ValidationResult.Error($"{fieldName} cannot contain special characters (except hyphen)");

        return ValidationResult.Success();
    }

    /// <summary>
    /// Validate Upper Case
    /// Rule: All alpha characters must be Upper Case
    /// Source: toyota_business_rules.md - Section 7.1, VAL-002
    /// </summary>
    public ValidationResult ValidateUpperCase(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ValidationResult.Success();

        // Extract only alpha characters
        var alphaChars = new string(value.Where(char.IsLetter).ToArray());

        if (alphaChars.Length > 0 && alphaChars != alphaChars.ToUpper())
            return ValidationResult.Error($"{fieldName} alpha characters must be uppercase");

        return ValidationResult.Success();
    }
}
