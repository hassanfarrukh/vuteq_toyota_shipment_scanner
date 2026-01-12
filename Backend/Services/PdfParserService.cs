// Author: Hassan
// Date: 2025-12-02
// Description: Service for parsing TMMI Daily One-Way Kanban Order Summary Report PDFs using PdfPig
// Updated: Fixed blank cell handling using word coordinates for proper column alignment

using Backend.Models.DTOs;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace Backend.Services;

/// <summary>
/// Interface for PDF parsing operations
/// </summary>
public interface IPdfParserService
{
    /// <summary>
    /// Parse TMMI Daily One-Way Kanban Order Summary Report PDF
    /// </summary>
    Task<List<ExtractedOrderDto>> ParseOrderPdfAsync(Stream pdfStream);
}

/// <summary>
/// Service implementation for parsing PDF files using PdfPig
/// </summary>
public class PdfParserService : IPdfParserService
{
    private readonly ILogger<PdfParserService> _logger;

    public PdfParserService(ILogger<PdfParserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parse TMMI Daily One-Way Kanban Order Summary Report PDF
    /// Each page may contain different dock codes and order numbers
    /// </summary>
    public async Task<List<ExtractedOrderDto>> ParseOrderPdfAsync(Stream pdfStream)
    {
        var extractedOrders = new List<ExtractedOrderDto>();

        try
        {
            // Open PDF document using PdfPig
            using (PdfDocument document = PdfDocument.Open(pdfStream))
            {
                _logger.LogInformation("PDF opened successfully. Total pages: {PageCount}", document.NumberOfPages);

                foreach (Page page in document.GetPages())
                {
                    _logger.LogInformation("Processing page {PageNumber}", page.Number);

                    // Extract text from page
                    string pageText = page.Text;
                    var words = page.GetWords().ToList();

                    // Parse page and extract orders
                    var pageOrders = ParsePage(pageText, words, page.Number);
                    extractedOrders.AddRange(pageOrders);

                    _logger.LogInformation("Extracted {OrderCount} orders from page {PageNumber}",
                        pageOrders.Count, page.Number);
                }
            }

            _logger.LogInformation("PDF parsing completed. Total orders extracted: {TotalOrders}",
                extractedOrders.Count);

            return await Task.FromResult(extractedOrders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing PDF");
            throw new InvalidOperationException("Failed to parse PDF file", ex);
        }
    }

    /// <summary>
    /// Parse a single page of the PDF
    /// </summary>
    private List<ExtractedOrderDto> ParsePage(string pageText, List<Word> words, int pageNumber)
    {
        var orders = new List<ExtractedOrderDto>();

        try
        {
            // DEBUG: Log the COMPLETE raw text from each page
            _logger.LogInformation("=== PAGE {PageNumber} RAW TEXT START (Length: {TextLength}) ===", pageNumber, pageText.Length);
            _logger.LogInformation("{PageText}", pageText);
            _logger.LogInformation("=== PAGE {PageNumber} RAW TEXT END ===", pageNumber);

            // *** CRITICAL FIX: Reconstruct text from words with proper spacing ***
            // Group words by Y coordinate (rows), then sort by X coordinate (left to right)
            var reconstructedText = ReconstructTextFromWords(words, pageNumber);

            _logger.LogInformation("=== PAGE {PageNumber} RECONSTRUCTED TEXT START (Length: {TextLength}) ===", pageNumber, reconstructedText.Length);
            _logger.LogInformation("{ReconstructedText}", reconstructedText);
            _logger.LogInformation("=== PAGE {PageNumber} RECONSTRUCTED TEXT END ===", pageNumber);

            // Use reconstructed text for parsing (better spacing)
            var textToParse = reconstructedText;

            // Extract header information
            var supplierName = ExtractSupplierName(textToParse);
            var supplierCode = ExtractSupplierCode(textToParse);
            var dockCode = ExtractDockCode(textToParse);
            var orderSeries = ExtractOrderSeries(textToParse);
            var transmitDate = ExtractTransmitDate(textToParse);
            var arriveDateTime = ExtractArriveDateTime(textToParse);
            var departDateTime = ExtractDepartDateTime(textToParse);
            var unloadDateTime = ExtractUnloadDateTime(textToParse);

            // Extract order numbers from header (e.g., 001, 002, 003)
            var orderNumbers = ExtractOrderNumbers(textToParse);

            // *** NEW: Extract order column positions using word coordinates ***
            var orderColumnPositions = ExtractOrderNumberPositions(words, orderNumbers);

            _logger.LogInformation("Page {PageNumber} - Supplier: {SupplierName}, Code: {SupplierCode}, " +
                "Dock: {DockCode}, Series: {OrderSeries}, Orders: {OrderCount}",
                pageNumber, supplierName, supplierCode, dockCode, orderSeries, orderNumbers.Count);

            // Extract line items (table rows) using word coordinates for proper column alignment
            var lineItems = ExtractLineItems(textToParse, orderNumbers.Count, words, orderColumnPositions);

            // NEW STRUCTURE V2: Create ONE order per OrderNumber (not one per page)
            // Page 1 has Order Numbers 001, 002, 003 → Create 3 separate orders
            foreach (var orderNumber in orderNumbers)
            {
                int orderIndex = orderNumbers.IndexOf(orderNumber);

                // Construct RealOrderNumber from OrderSeries + OrderNumber (e.g., "2025111701")
                string realOrderNumber = $"{orderSeries}{orderNumber}";

                var order = new ExtractedOrderDto
                {
                    OwkNumber = $"{supplierCode}-{dockCode}-{orderSeries}-{orderNumber}", // For display/tracking purposes
                    CustomerName = supplierName,
                    SupplierCode = supplierCode,
                    DockCode = dockCode,
                    RealOrderNumber = realOrderNumber, // Consolidated identifier
                    OrderDate = transmitDate,
                    UnloadDateTime = unloadDateTime,
                    Items = new List<ExtractedOrderItemDto>()
                };

                // Add line items for THIS specific order number
                foreach (var lineItem in lineItems)
                {
                    // Get the quantity for this specific order number
                    if (orderIndex < lineItem.QuantitiesByOrder.Count)
                    {
                        int qty = lineItem.QuantitiesByOrder[orderIndex];
                        if (qty > 0) // Only add items with quantity > 0
                        {
                            order.Items.Add(new ExtractedOrderItemDto
                            {
                                PartNumber = lineItem.PartNumber,
                                Description = lineItem.Description,
                                LotQty = lineItem.LotQty,
                                KanbanNumber = lineItem.KanbanNumber,
                                PlannedQty = qty,
                                RawKanbanValue = lineItem.KanbanNumber,
                                ManifestNo = 0 // PDFs don't have manifest numbers
                            });
                        }
                    }
                }

                order.ItemCount = order.Items.Count;

                if (order.Items.Count > 0) // Only add order if it has items
                {
                    orders.Add(order);
                    _logger.LogInformation("Created order {RealOrderNumber} (Series: {OrderSeries}, Number: {OrderNumber}) with {ItemCount} items",
                        realOrderNumber, orderSeries, orderNumber, order.Items.Count);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing page {PageNumber}", pageNumber);
        }

        return orders;
    }

    /// <summary>
    /// Reconstruct text from words with proper spacing
    /// Groups words by Y coordinate (rows) and sorts by X coordinate (left to right)
    /// </summary>
    private string ReconstructTextFromWords(List<Word> words, int pageNumber)
    {
        try
        {
            if (words == null || words.Count == 0)
            {
                _logger.LogWarning("No words found on page {PageNumber}", pageNumber);
                return string.Empty;
            }

            _logger.LogInformation("Reconstructing text from {WordCount} words on page {PageNumber}", words.Count, pageNumber);

            // Group words by Y coordinate (same row = similar Y position)
            // Tolerance of 5 pixels for Y coordinate grouping
            const double yTolerance = 5.0;

            // Round Y coordinates to nearest 5 pixels for grouping
            var wordsByRow = words
                .GroupBy(w => Math.Round(w.BoundingBox.Bottom / yTolerance) * yTolerance)
                .OrderByDescending(g => g.Key) // Top to bottom (higher Y = top of page)
                .ToList();

            _logger.LogDebug("Grouped words into {RowCount} rows", wordsByRow.Count);

            var reconstructedLines = new List<string>();

            foreach (var rowGroup in wordsByRow)
            {
                // Sort words in this row by X coordinate (left to right)
                var wordsInRow = rowGroup
                    .OrderBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text)
                    .ToList();

                // Join words with spaces
                var rowText = string.Join(" ", wordsInRow);
                reconstructedLines.Add(rowText);

                _logger.LogDebug("Row at Y={YPos}: {RowText}",
                    rowGroup.Key,
                    rowText.Length > 100 ? rowText.Substring(0, 100) + "..." : rowText);
            }

            // Join all rows with newlines
            var result = string.Join("\n", reconstructedLines);

            _logger.LogInformation("Successfully reconstructed text with {LineCount} lines, total length: {Length}",
                reconstructedLines.Count, result.Length);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconstructing text from words on page {PageNumber}", pageNumber);
            return string.Empty;
        }
    }

    /// <summary>
    /// Extract supplier name from PDF text
    /// </summary>
    private string? ExtractSupplierName(string text)
    {
        // Pattern 1: Look for "AGC Automotive" or other known supplier names with spaces
        // Since PDF text is concatenated, supplier name is one of few values with spaces
        var knownSuppliers = new[] { "AGC Automotive", "Toyota", "Denso", "Aisin", "Bridgestone" };

        foreach (var supplier in knownSuppliers)
        {
            if (text.Contains(supplier, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Extracted supplier name: '{SupplierName}'", supplier);
                return supplier;
            }
        }

        // Pattern 2: Extract after "Supplier Name:" label
        var match = Regex.Match(text, @"Supplier\s+Name\s*:?\s*([A-Za-z0-9\s&\-\.]+?)(?=Supplier\s+Code|\d{5})",
            RegexOptions.IgnoreCase);

        if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
        {
            var supplierName = match.Groups[1].Value.Trim();
            _logger.LogDebug("Extracted supplier name (pattern 2): '{SupplierName}'", supplierName);
            return supplierName;
        }

        _logger.LogWarning("Could not extract supplier name from text");
        return null;
    }

    /// <summary>
    /// Extract supplier code from PDF text
    /// </summary>
    private string? ExtractSupplierCode(string text)
    {
        // Pattern 1: PRIORITY - Explicit "Supplier Code:" label with space (clean format)
        // Example: "Supplier Code: 02806"
        var match = Regex.Match(text, @"Supplier\s+Code\s*:?\s*(\d{5})", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var supplierCode = match.Groups[1].Value.Trim();
            _logger.LogDebug("Extracted supplier code (labeled): '{SupplierCode}'", supplierCode);
            return supplierCode;
        }

        // Pattern 2: Look for 5-digit code immediately followed by 2-character dock code (concatenated - fallback)
        // Example: "02806FL" where 02806 is supplier code and FL is dock code (letter + letter/digit)
        var match2 = Regex.Match(text, @"(\d{5})([A-Z][A-Z0-9])");

        if (match2.Success)
        {
            var supplierCode = match2.Groups[1].Value;
            _logger.LogDebug("Extracted supplier code (concatenated): '{SupplierCode}'", supplierCode);
            return supplierCode;
        }

        // Pattern 3: Look for 5-digit code after supplier name
        var match3 = Regex.Match(text, @"(AGC Automotive|Toyota|Denso)\s*(\d{5})");

        if (match3.Success)
        {
            var supplierCode = match3.Groups[2].Value;
            _logger.LogDebug("Extracted supplier code (after name): '{SupplierCode}'", supplierCode);
            return supplierCode;
        }

        _logger.LogWarning("Could not extract supplier code from text");
        return null;
    }

    /// <summary>
    /// Extract dock code from PDF text
    /// </summary>
    private string? ExtractDockCode(string text)
    {
        // Pattern 1: PRIORITY - Explicit "NAMC Dock Code:" label with space (clean format)
        // Example: "NAMC Dock Code: T6" - 2-character dock code (letter + letter/digit)
        var match = Regex.Match(text, @"NAMC\s+Dock\s+Code\s*:?\s*([A-Z][A-Z0-9])\b",
            RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var dockCode = match.Groups[1].Value.ToUpper();
            _logger.LogDebug("Extracted dock code (labeled): '{DockCode}'", dockCode);
            return dockCode;
        }

        // Pattern 2: Look for 2-character dock code concatenated after 5-digit supplier code (fallback)
        // Example: "02806FL" where FL is the dock code (letter + letter/digit)
        var match2 = Regex.Match(text, @"\d{5}([A-Z][A-Z0-9])");

        if (match2.Success)
        {
            var dockCode = match2.Groups[1].Value.ToUpper();
            _logger.LogDebug("Extracted dock code (concatenated): '{DockCode}'", dockCode);
            return dockCode;
        }

        // Pattern 3: Look for 2-character dock codes in first 500 characters (header area)
        // Matches any 2-character code: letter + letter/digit
        var headerText = text.Length > 500 ? text.Substring(0, 500) : text;
        var match3 = Regex.Match(headerText, @"\b([A-Z][A-Z0-9])\b", RegexOptions.IgnoreCase);

        if (match3.Success)
        {
            var dockCode = match3.Groups[1].Value.ToUpper();
            _logger.LogDebug("Extracted dock code (header): '{DockCode}'", dockCode);
            return dockCode;
        }

        _logger.LogWarning("Could not extract dock code from text");
        return null;
    }

    /// <summary>
    /// Extract order series from PDF text
    /// </summary>
    private string? ExtractOrderSeries(string text)
    {
        // Pattern 1: Explicit "Order Series:" label followed by 8-digit number
        // Example: "Order Series 20251117" or just "20251117" on its own line
        var match = Regex.Match(text, @"Order\s+Series\s*:?\s*(\d{8})", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var orderSeries = match.Groups[1].Value.Trim();
            _logger.LogDebug("Extracted order series (labeled): '{OrderSeries}'", orderSeries);
            return orderSeries;
        }

        // Pattern 2: Look for 8-digit number starting with 202 on its own line or after certain keywords
        // Example: "20251117" standalone or after "Build Out" or near order data
        // This pattern looks for 8 consecutive digits NOT in date format (no slashes or dashes nearby)
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            // Look for 8-digit number starting with 202 on a line
            var lineMatch = Regex.Match(line, @"\b(202\d{5})\b");
            if (lineMatch.Success)
            {
                // Ensure it's not part of a date format (no slashes/dashes around it)
                if (!line.Contains('/') || line.IndexOf(lineMatch.Value) > line.LastIndexOf('/'))
                {
                    var orderSeries = lineMatch.Groups[1].Value;
                    _logger.LogDebug("Extracted order series (line scan): '{OrderSeries}'", orderSeries);
                    return orderSeries;
                }
            }
        }

        // Pattern 3: Look for 8-digit number starting with 202 (NOT in date format YYYY/MM/DD)
        // Match 8 consecutive digits starting with 202
        var match3 = Regex.Match(text, @"(?<!\d)(?<!/)(?<!-)(\d{8})(?!/)(?!-)");

        if (match3.Success)
        {
            var orderSeries = match3.Groups[1].Value;

            // Verify it starts with 202 (year 2020-2029)
            if (orderSeries.StartsWith("202"))
            {
                _logger.LogDebug("Extracted order series (8-digit): '{OrderSeries}'", orderSeries);
                return orderSeries;
            }
        }

        // Pattern 4: Look for 8-digit after 2-character dock code (concatenation pattern - fallback)
        // Matches: dock code (letter + letter/digit) + 8-digit order series
        var match4 = Regex.Match(text, @"([A-Z][A-Z0-9])(\d{8})");

        if (match4.Success)
        {
            var orderSeries = match4.Groups[2].Value;
            _logger.LogDebug("Extracted order series (after dock): '{OrderSeries}'", orderSeries);
            return orderSeries;
        }

        _logger.LogWarning("Could not extract order series from text");
        return null;
    }

    /// <summary>
    /// Extract transmit date from PDF text
    /// </summary>
    private DateTime? ExtractTransmitDate(string text)
    {
        // Look for pattern: "Transmit Date" followed by date (e.g., 2025/11/12)
        // Pattern 1: Explicit "Transmit Date:" with date in format YYYY/MM/DD
        var match = Regex.Match(text, @"Transmit\s+Date\s*:?\s*(\d{4})[/\-](\d{2})[/\-](\d{2})",
            RegexOptions.IgnoreCase);

        if (match.Success)
        {
            int year = int.Parse(match.Groups[1].Value);
            int month = int.Parse(match.Groups[2].Value);
            int day = int.Parse(match.Groups[3].Value);

            try
            {
                var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                _logger.LogDebug("Extracted transmit date: {Date}", date.ToString("yyyy-MM-dd"));
                return date;
            }
            catch
            {
                _logger.LogWarning("Invalid date extracted: {Year}/{Month}/{Day}", year, month, day);
            }
        }

        // Pattern 2: Look for date in format YYYY/MM/DD near the beginning
        var match2 = Regex.Match(text, @"\b(\d{4})[/\-](\d{1,2})[/\-](\d{1,2})\b");

        if (match2.Success)
        {
            int year = int.Parse(match2.Groups[1].Value);
            int month = int.Parse(match2.Groups[2].Value);
            int day = int.Parse(match2.Groups[3].Value);

            try
            {
                var date = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
                _logger.LogDebug("Extracted transmit date (pattern 2): {Date}", date.ToString("yyyy-MM-dd"));
                return date;
            }
            catch
            {
                _logger.LogWarning("Invalid date extracted (pattern 2): {Year}/{Month}/{Day}", year, month, day);
            }
        }

        _logger.LogWarning("Could not extract transmit date from text");
        return null;
    }

    /// <summary>
    /// Extract arrive date and time from PDF text
    /// Format: "Arrive Date 11/14" and "Arrive Time 13:01"
    /// </summary>
    private DateTime? ExtractArriveDateTime(string text)
    {
        try
        {
            // Extract arrive date
            var dateMatch = Regex.Match(text, @"Arrive\s+Date\s+(\d{1,2})/(\d{1,2})", RegexOptions.IgnoreCase);
            if (!dateMatch.Success)
            {
                _logger.LogWarning("Could not extract arrive date from text");
                return null;
            }

            int month = int.Parse(dateMatch.Groups[1].Value);
            int day = int.Parse(dateMatch.Groups[2].Value);

            // Extract arrive time
            var timeMatch = Regex.Match(text, @"Arrive\s+Time\s+(\d{1,2}):(\d{2})", RegexOptions.IgnoreCase);
            if (!timeMatch.Success)
            {
                _logger.LogWarning("Could not extract arrive time from text");
                return null;
            }

            int hour = int.Parse(timeMatch.Groups[1].Value);
            int minute = int.Parse(timeMatch.Groups[2].Value);

            // Use current year if not specified (or extract from transmit date if available)
            int year = DateTime.Now.Year;

            var arriveDateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Local);
            _logger.LogDebug("Extracted arrive date/time: {DateTime}", arriveDateTime.ToString("yyyy-MM-dd HH:mm"));
            return arriveDateTime;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting arrive date/time");
            return null;
        }
    }

    /// <summary>
    /// Extract depart date and time from PDF text
    /// Format: "Depart Date 11/14" and "Depart Time 13:01"
    /// </summary>
    private DateTime? ExtractDepartDateTime(string text)
    {
        try
        {
            // Extract depart date
            var dateMatch = Regex.Match(text, @"Depart\s+Date\s+(\d{1,2})/(\d{1,2})", RegexOptions.IgnoreCase);
            if (!dateMatch.Success)
            {
                _logger.LogWarning("Could not extract depart date from text");
                return null;
            }

            int month = int.Parse(dateMatch.Groups[1].Value);
            int day = int.Parse(dateMatch.Groups[2].Value);

            // Extract depart time
            var timeMatch = Regex.Match(text, @"Depart\s+Time\s+(\d{1,2}):(\d{2})", RegexOptions.IgnoreCase);
            if (!timeMatch.Success)
            {
                _logger.LogWarning("Could not extract depart time from text");
                return null;
            }

            int hour = int.Parse(timeMatch.Groups[1].Value);
            int minute = int.Parse(timeMatch.Groups[2].Value);

            // Use current year if not specified
            int year = DateTime.Now.Year;

            var departDateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Local);
            _logger.LogDebug("Extracted depart date/time: {DateTime}", departDateTime.ToString("yyyy-MM-dd HH:mm"));
            return departDateTime;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting depart date/time");
            return null;
        }
    }

    /// <summary>
    /// Extract unload date and time from PDF text
    /// Format: "Unload Date 11/17" and "Unload Time 14:51"
    /// </summary>
    private DateTime? ExtractUnloadDateTime(string text)
    {
        try
        {
            // Extract unload date
            var dateMatch = Regex.Match(text, @"Unload\s+Date\s+(\d{1,2})/(\d{1,2})", RegexOptions.IgnoreCase);
            if (!dateMatch.Success)
            {
                _logger.LogWarning("Could not extract unload date from text");
                return null;
            }

            int month = int.Parse(dateMatch.Groups[1].Value);
            int day = int.Parse(dateMatch.Groups[2].Value);

            // Extract unload time
            var timeMatch = Regex.Match(text, @"Unload\s+Time\s+(\d{1,2}):(\d{2})", RegexOptions.IgnoreCase);
            if (!timeMatch.Success)
            {
                _logger.LogWarning("Could not extract unload time from text");
                return null;
            }

            int hour = int.Parse(timeMatch.Groups[1].Value);
            int minute = int.Parse(timeMatch.Groups[2].Value);

            // Use current year if not specified
            int year = DateTime.Now.Year;

            var unloadDateTime = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Local);
            _logger.LogDebug("Extracted unload date/time: {DateTime}", unloadDateTime.ToString("yyyy-MM-dd HH:mm"));
            return unloadDateTime;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting unload date/time");
            return null;
        }
    }

    /// <summary>
    /// Extract order number column positions using word coordinates
    /// This returns the X position of each order number header (001, 002, 003, etc.)
    /// These X positions define the column boundaries for quantity alignment
    /// </summary>
    private Dictionary<string, double> ExtractOrderNumberPositions(List<Word> words, List<string> orderNumbers)
    {
        var positions = new Dictionary<string, double>();

        try
        {
            if (words == null || words.Count == 0 || orderNumbers == null || orderNumbers.Count == 0)
            {
                _logger.LogWarning("Cannot extract order positions: words or orderNumbers is null/empty");
                return positions;
            }

            _logger.LogInformation("=== EXTRACTING ORDER COLUMN POSITIONS ===");

            // Find words that contain "Order" and "Number" to identify the header row
            var orderHeaderWords = words
                .Where(w => w.Text.Contains("Order", StringComparison.OrdinalIgnoreCase) ||
                           w.Text.Contains("Number", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (orderHeaderWords.Count == 0)
            {
                _logger.LogWarning("Could not find 'Order Number' header words");
                return positions;
            }

            // Get the Y coordinate of the header row (average Y of header words)
            double headerY = orderHeaderWords.Average(w => w.BoundingBox.Bottom);
            _logger.LogDebug("Order Number header row Y position: {HeaderY}", headerY);

            // Y tolerance for finding words in the same row (within 10 pixels)
            const double yTolerance = 10.0;

            // Find all words in the header row (same Y coordinate within tolerance)
            var headerRowWords = words
                .Where(w => Math.Abs(w.BoundingBox.Bottom - headerY) < yTolerance)
                .OrderBy(w => w.BoundingBox.Left)
                .ToList();

            _logger.LogDebug("Found {Count} words in Order Number header row", headerRowWords.Count);

            // Find each order number and record its X position
            foreach (var orderNum in orderNumbers)
            {
                // Find the word that matches this order number (exact match)
                var orderWord = headerRowWords.FirstOrDefault(w => w.Text.Trim() == orderNum);

                if (orderWord != null)
                {
                    // Use the center X position of the word as the column position
                    double columnX = orderWord.BoundingBox.Left + (orderWord.BoundingBox.Width / 2);
                    positions[orderNum] = columnX;

                    _logger.LogInformation("Order {OrderNum} column position: X={ColumnX} (Word bounds: Left={Left}, Right={Right})",
                        orderNum, columnX, orderWord.BoundingBox.Left, orderWord.BoundingBox.Right);
                }
                else
                {
                    _logger.LogWarning("Could not find word for order number {OrderNum} in header row", orderNum);
                }
            }

            _logger.LogInformation("Extracted {Count} order column positions", positions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting order column positions");
        }

        return positions;
    }

    /// <summary>
    /// Extract order numbers from the header (e.g., 001, 002, 003)
    /// In clean format: "Order Number 001 002 003"
    /// </summary>
    private List<string> ExtractOrderNumbers(string text)
    {
        var orderNumbers = new List<string>();

        // Pattern 1: PRIORITY - Look for "Order Number" followed by multiple 3-digit numbers (clean format)
        // Example: "Order Number 001 002 003"
        var match = Regex.Match(text, @"Order\s+Number\s+((?:\d{3}\s*)+)", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            var numbersStr = match.Groups[1].Value;
            _logger.LogDebug("Found order numbers string: '{NumbersStr}'", numbersStr);

            // Extract all 3-digit numbers from the matched string
            var numberMatches = Regex.Matches(numbersStr, @"\d{3}");
            foreach (Match numMatch in numberMatches)
            {
                var num = numMatch.Value;
                if (!orderNumbers.Contains(num))
                {
                    orderNumbers.Add(num);
                    _logger.LogDebug("Extracted order number: {OrderNumber}", num);
                }
            }
        }

        // Pattern 2: Look for line containing "Order Number" and extract all 3-digit numbers on that line
        if (orderNumbers.Count == 0)
        {
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                if (line.Contains("Order Number", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug("Found Order Number line: '{Line}'", line);

                    // Extract all 3-digit numbers from this line
                    var numberMatches = Regex.Matches(line, @"\b(\d{3})\b");
                    foreach (Match numMatch in numberMatches)
                    {
                        var num = numMatch.Groups[1].Value;
                        if (int.TryParse(num, out int n) && n >= 1 && n <= 999 && !orderNumbers.Contains(num))
                        {
                            orderNumbers.Add(num);
                            _logger.LogDebug("Extracted order number from line: {OrderNumber}", num);
                        }
                    }
                    break; // Found the order number line, no need to continue
                }
            }
        }

        // Pattern 3: Look for 3-digit numbers after "Lots Ord'd" repeating pattern and before first part number
        // Text pattern: "20251117Lots Ord'dLots Ord'd...Kanban NumberLots Ord'dLots Ord'd00168101-0E120-00"
        // The order number (001) appears just before the first part number
        if (orderNumbers.Count == 0)
        {
            var match3 = Regex.Match(text, @"Lots\s+Ord['\u2019]d.*?(?<orderNum>\d{3})(?=\d{5}-[A-Z0-9]{5}-\d{2})");

            if (match3.Success)
            {
                var orderNum = match3.Groups["orderNum"].Value;
                if (!orderNumbers.Contains(orderNum))
                {
                    orderNumbers.Add(orderNum);
                    _logger.LogDebug("Found order number (before part): {OrderNumber}", orderNum);
                }
            }
        }

        // Pattern 4: Find 3-digit numbers between order series and first part number in concatenated text
        // Example: "FL11/17 14:5120251117...001...68101-0E120-00"
        if (orderNumbers.Count == 0)
        {
            // Find order series (8-digit starting with 202)
            var seriesMatch = Regex.Match(text, @"(202\d{5})");
            if (seriesMatch.Success)
            {
                int startPos = seriesMatch.Index + seriesMatch.Length;

                // Find first part number position
                var partMatch = Regex.Match(text.Substring(startPos), @"\d{5}-[A-Z0-9]{5}-\d{2}");
                if (partMatch.Success)
                {
                    int endPos = startPos + partMatch.Index;

                    // Extract section between series and first part number
                    string section = text.Substring(startPos, endPos - startPos);

                    _logger.LogDebug("Searching for order numbers in section: {Section}",
                        section.Length > 100 ? section.Substring(0, 100) + "..." : section);

                    // Look for 3-digit numbers that appear just before the part number
                    // The number should be at the end of the section
                    var orderMatch = Regex.Match(section, @"(\d{3})(?=\d{5}-|\s*$)");
                    if (orderMatch.Success)
                    {
                        var num = orderMatch.Groups[1].Value;
                        if (int.TryParse(num, out int n) && n >= 1 && n <= 999 && !orderNumbers.Contains(num))
                        {
                            orderNumbers.Add(num);
                            _logger.LogDebug("Found order number (between series and part): {OrderNumber}", num);
                        }
                    }
                }
            }
        }

        // Pattern 5: Extract from raw concatenated format - look backwards from first part number
        if (orderNumbers.Count == 0)
        {
            // Find first part number
            var partMatch = Regex.Match(text, @"\d{5}-[A-Z0-9]{5}-\d{2}");
            if (partMatch.Success && partMatch.Index >= 3)
            {
                // Look at the 3 characters immediately before the part number
                string beforePart = text.Substring(Math.Max(0, partMatch.Index - 10), Math.Min(10, partMatch.Index));

                _logger.LogDebug("Text before first part number: '{BeforePart}'", beforePart);

                // Find last 3-digit sequence before part number
                var orderMatch = Regex.Match(beforePart, @"(\d{3})(?!\d)");
                if (orderMatch.Success)
                {
                    var num = orderMatch.Groups[1].Value;
                    if (int.TryParse(num, out int n) && n >= 1 && n <= 999 && !orderNumbers.Contains(num))
                    {
                        orderNumbers.Add(num);
                        _logger.LogDebug("Found order number (before first part): {OrderNumber}", num);
                    }
                }
            }
        }

        // Default to "001" if no order numbers found
        if (orderNumbers.Count == 0)
        {
            _logger.LogWarning("No order numbers found, defaulting to '001'");
            orderNumbers.Add("001");
        }

        var result = orderNumbers.Distinct().OrderBy(x => x).ToList();
        _logger.LogInformation("Extracted {Count} order numbers: {OrderNumbers}",
            result.Count, string.Join(", ", result));

        return result;
    }

    /// <summary>
    /// Extract line items from the PDF table using word coordinates for proper column alignment
    /// Handles blank cells correctly by matching quantity words to order column positions
    /// </summary>
    private List<LineItemData> ExtractLineItems(string text, int orderCount, List<Word> words, Dictionary<string, double> orderColumnPositions)
    {
        var lineItems = new List<LineItemData>();

        try
        {
            // Split text into lines for clean format processing
            var lines = text.Split('\n');

            _logger.LogInformation("=== STARTING LINE ITEM EXTRACTION ===");
            _logger.LogInformation("Total lines to process: {LineCount}, Expected order count: {OrderCount}", lines.Length, orderCount);

            // PRIORITY PATTERN: Clean multi-line format (one item per line)
            // Format: "68105-0E131-00 GLASS SUB-ASSY BA 00012 TF63 2 1"
            // Part: 68105-0E131-00
            // Description: GLASS SUB-ASSY BA (uppercase letters, spaces, dashes)
            // Lot Qty: 00012 (5 digits)
            // Kanban: TF63 (2-4 uppercase letters with optional digits)
            // Quantities: 2 1 (space-separated, one per order)

            int lineNumber = 0;
            foreach (var line in lines)
            {
                lineNumber++;

                // Skip empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Trim line to remove any trailing/leading whitespace and newline characters
                var trimmedLine = line.Trim();

                // DEBUG: Log each line being checked
                _logger.LogInformation("Line {LineNum}: [{Line}]", lineNumber, trimmedLine.Length > 100 ? trimmedLine.Substring(0, 100) + "..." : trimmedLine);

                // Look for lines that start with part number pattern
                var lineMatch = Regex.Match(trimmedLine, @"^(\d{5}-[A-Z0-9]{5}-\d{2})\s+(.+)$");

                // DEBUG: Log if line starts with part number
                if (lineMatch.Success)
                {
                    var partNumber = lineMatch.Groups[1].Value;
                    var restOfLine = lineMatch.Groups[2].Value;

                    _logger.LogInformation("✓ Line {LineNum} MATCHES part number pattern: Part={PartNumber}", lineNumber, partNumber);
                    _logger.LogDebug("    Rest of line: [{Rest}]", restOfLine);

                    // Pattern for clean format with space-separated values
                    // Description (uppercase letters/spaces/dashes) + LotQty (5 digits) + Kanban (2-4 letters+digits) + Quantities (space-separated digits)
                    // STRATEGY: Try multiple patterns in order of specificity

                    bool matched = false;
                    string? description = null;
                    string? lotQtyStr = null;
                    string? kanbanNumber = null;
                    string? quantitiesStr = null;

                    // PATTERN 1: Full pattern with quantities (most common)
                    // Example: "GLASS SUB-ASSY BA 00012 TF63 2 1" or "GLASS SUB-ASSY FR 00045 FA99 4"
                    // Updated to handle single-digit quantities and trim whitespace
                    // Fixed kanban pattern to allow digits in middle: HN7X, HM55, FA99
                    var itemMatch1 = Regex.Match(restOfLine.Trim(),
                        @"^([A-Z][A-Z\s\-/,]+?)\s+(\d{5})\s+([A-Z0-9]{2,6})\s+([\d\s]+)\s*$",
                        RegexOptions.IgnoreCase);

                    if (itemMatch1.Success)
                    {
                        description = itemMatch1.Groups[1].Value.Trim();
                        lotQtyStr = itemMatch1.Groups[2].Value;
                        kanbanNumber = itemMatch1.Groups[3].Value;
                        quantitiesStr = itemMatch1.Groups[4].Value;
                        matched = true;
                        _logger.LogInformation("    ✓✓ PATTERN 1 MATCH (with quantities)");
                    }

                    // PATTERN 2: Without quantities at the end
                    // Example: "GLASS SUB-ASSY BA 00012 TF63"
                    // Fixed kanban pattern to allow digits in middle: HN7X, HM55, FA99
                    if (!matched)
                    {
                        var itemMatch2 = Regex.Match(restOfLine.Trim(),
                            @"^([A-Z][A-Z\s\-/,]+?)\s+(\d{5})\s+([A-Z0-9]{2,6})\s*$",
                            RegexOptions.IgnoreCase);

                        if (itemMatch2.Success)
                        {
                            description = itemMatch2.Groups[1].Value.Trim();
                            lotQtyStr = itemMatch2.Groups[2].Value;
                            kanbanNumber = itemMatch2.Groups[3].Value;
                            quantitiesStr = "";
                            matched = true;
                            _logger.LogInformation("    ✓✓ PATTERN 2 MATCH (without quantities)");
                        }
                    }

                    // PATTERN 3: Greedy description match (description contains more text)
                    // Example: "GLASS SUB-ASSY FR DOOR 00045 MH98 1 1 1"
                    // Fixed kanban pattern to allow digits in middle: HN7X, HM55, FA99
                    if (!matched)
                    {
                        var itemMatch3 = Regex.Match(restOfLine.Trim(),
                            @"^([A-Z][A-Z\s\-/,]+)\s+(\d{5})\s+([A-Z0-9]{2,6})\s+([\d\s]+)\s*$",
                            RegexOptions.IgnoreCase);

                        if (itemMatch3.Success)
                        {
                            description = itemMatch3.Groups[1].Value.Trim();
                            lotQtyStr = itemMatch3.Groups[2].Value;
                            kanbanNumber = itemMatch3.Groups[3].Value;
                            quantitiesStr = itemMatch3.Groups[4].Value;
                            matched = true;
                            _logger.LogInformation("    ✓✓ PATTERN 3 MATCH (greedy description with quantities)");
                        }
                    }

                    if (matched)
                    {
                        _logger.LogInformation("    Extracted: Desc=[{Desc}] LotQty=[{Lot}] Kanban=[{Kanban}] Quantities=[{Qty}]",
                            description, lotQtyStr, kanbanNumber, quantitiesStr ?? "");

                        int? lotQty = int.TryParse(lotQtyStr, out int lq) ? lq : null;

                        List<int> quantities;

                        // *** CRITICAL FIX: Use word coordinates to extract quantities ***
                        // This properly handles blank cells by matching word X positions to column positions
                        if (orderColumnPositions.Count > 0)
                        {
                            // Find the part number word to identify the row
                            var partWord = words.FirstOrDefault(w => w.Text.Trim() == partNumber);

                            if (partWord != null)
                            {
                                double rowY = partWord.BoundingBox.Bottom;
                                const double yTolerance = 5.0;

                                // Get all words in the same row (same Y coordinate)
                                var wordsInRow = words
                                    .Where(w => Math.Abs(w.BoundingBox.Bottom - rowY) < yTolerance)
                                    .ToList();

                                _logger.LogDebug("    Found {Count} words in part row at Y={RowY}", wordsInRow.Count, rowY);

                                // Extract order numbers list from the dictionary keys
                                var orderNumbers = orderColumnPositions.Keys.OrderBy(k => k).ToList();

                                // Use coordinate-based extraction
                                quantities = ExtractQuantitiesUsingCoordinates(wordsInRow, orderColumnPositions, orderNumbers);

                                _logger.LogInformation("    >>> COORDINATE-BASED EXTRACTION: Quantities=[{Quantities}]",
                                    string.Join(", ", quantities));
                            }
                            else
                            {
                                _logger.LogWarning("    Could not find part word '{PartNumber}' in words list - falling back to text parsing",
                                    partNumber);
                                quantities = ParseSpaceSeparatedQuantities(quantitiesStr ?? "", orderCount);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("    No order column positions available - falling back to text parsing");
                            quantities = ParseSpaceSeparatedQuantities(quantitiesStr ?? "", orderCount);
                        }

                        lineItems.Add(new LineItemData
                        {
                            PartNumber = partNumber,
                            Description = description,
                            LotQty = lotQty,
                            KanbanNumber = kanbanNumber,
                            QuantitiesByOrder = quantities
                        });

                        _logger.LogInformation("    >>> LINE ITEM ADDED: Part={PartNumber}, Desc={Description}, Lot={LotQty}, " +
                            "Kanban={KanbanNumber}, Quantities=[{Quantities}]",
                            partNumber, description, lotQty, kanbanNumber, string.Join(", ", quantities));

                        continue; // Move to next line
                    }
                    else
                    {
                        // DEBUG: Log why all patterns failed
                        _logger.LogWarning("    ✗ ALL PATTERNS FAILED for part {PartNumber}", partNumber);
                        _logger.LogDebug("    Rest of line was: [{Rest}]", restOfLine);

                        // Try to match individual components to see what's missing
                        var descMatch = Regex.Match(restOfLine, @"^([A-Z][A-Z\s\-/,]+)");
                        var lotMatch = Regex.Match(restOfLine, @"(\d{5})");
                        var kanbanMatch = Regex.Match(restOfLine, @"([A-Z]{2,4}\d*)");
                        var qtyMatch = Regex.Match(restOfLine, @"([\d\s]+)$");

                        _logger.LogDebug("    Component matches: Desc={DescMatch} Lot={LotMatch} Kanban={KanbanMatch} Qty={QtyMatch}",
                            descMatch.Success ? $"[{descMatch.Groups[1].Value}]" : "NO MATCH",
                            lotMatch.Success ? $"[{lotMatch.Groups[1].Value}]" : "NO MATCH",
                            kanbanMatch.Success ? $"[{kanbanMatch.Groups[1].Value}]" : "NO MATCH",
                            qtyMatch.Success ? $"[{qtyMatch.Groups[1].Value}]" : "NO MATCH");
                    }
                }
                else
                {
                    // DEBUG: Log lines that don't match part number pattern
                    if (line.Contains("68") || line.Contains("Part") || Regex.IsMatch(line, @"\d{5}-\d"))
                    {
                        _logger.LogDebug("✗ Line {LineNum} DOES NOT match part number pattern but contains suspicious content: [{Line}]",
                            lineNumber, line.Length > 150 ? line.Substring(0, 150) + "..." : line);
                    }
                }
            }

            _logger.LogInformation("=== CLEAN FORMAT EXTRACTION COMPLETE: Found {Count} items ===", lineItems.Count);

            // FALLBACK: If no items found using clean format, try concatenated format
            if (lineItems.Count == 0)
            {
                _logger.LogWarning("=== NO ITEMS FOUND IN CLEAN FORMAT - TRYING CONCATENATED FORMAT ===");

                var partMatches = Regex.Matches(text, @"(\d{5}-[A-Z0-9]{5}-\d{2})");

                _logger.LogInformation("Found {Count} part number matches in concatenated format", partMatches.Count);

                int partIndex = 0;
                foreach (Match partMatch in partMatches)
                {
                    partIndex++;
                    var partNumber = partMatch.Groups[1].Value;
                    int partStartPos = partMatch.Index;

                    // Extract text around this part number (next 200 chars should contain all data)
                    int extractLength = Math.Min(200, text.Length - partStartPos);
                    string lineContext = text.Substring(partStartPos, extractLength);

                    _logger.LogInformation("Part {Index}/{Total}: {PartNumber} at position {Pos}",
                        partIndex, partMatches.Count, partNumber, partStartPos);
                    _logger.LogDebug("    Context: [{Context}]",
                        lineContext.Length > 150 ? lineContext.Substring(0, 150) + "..." : lineContext);

                    // PRIMARY PATTERN for concatenated text:
                    // Part: 68101-0E120-00
                    // Space(s): " " (may have one or more spaces)
                    // Description: GLASS SUB-ASSY FR (uppercase letters, spaces, dashes - NO digits)
                    // Lot Qty: 00045 (exactly 5 digits, directly after description with NO space)
                    // Kanban: FA99 (2-6 alphanumeric chars to handle HN7X, HM55, FA99, etc.)
                    // Quantity: 4 (single digit or multiple digits for multiple orders)
                    var itemMatch = Regex.Match(lineContext,
                        @"^(\d{5}-[A-Z0-9]{5}-\d{2})\s+([A-Z][A-Z\s\-/,]*?)(\d{5})([A-Z0-9]{2,6})(\d+)",
                        RegexOptions.IgnoreCase);

                    if (itemMatch.Success)
                    {
                        var description = itemMatch.Groups[2].Value.Trim();
                        var lotQtyStr = itemMatch.Groups[3].Value;
                        var kanbanNumber = itemMatch.Groups[4].Value;
                        var quantitiesStr = itemMatch.Groups[5].Value;

                        _logger.LogInformation("    ✓✓ CONCATENATED PATTERN MATCH - Desc=[{Desc}] LotQty=[{Lot}] Kanban=[{Kanban}] Quantities=[{Qty}]",
                            description, lotQtyStr, kanbanNumber, quantitiesStr);

                        int? lotQty = int.TryParse(lotQtyStr, out int lq) ? lq : null;

                        // Parse quantities: could be single digit (4) or multiple concatenated (436)
                        var quantities = ParseQuantities(quantitiesStr, orderCount);

                        lineItems.Add(new LineItemData
                        {
                            PartNumber = partNumber,
                            Description = description,
                            LotQty = lotQty,
                            KanbanNumber = kanbanNumber,
                            QuantitiesByOrder = quantities
                        });

                        _logger.LogInformation("    >>> LINE ITEM ADDED (concatenated): Part={PartNumber}, Desc={Description}, Lot={LotQty}, " +
                            "Kanban={KanbanNumber}, Quantities=[{Quantities}]",
                            partNumber, description, lotQty, kanbanNumber, string.Join(", ", quantities));
                    }
                    else
                    {
                        // FALLBACK: Extract piece by piece when primary pattern fails
                        _logger.LogWarning("    ✗ CONCATENATED PATTERN FAILED for part {PartNumber} - trying fallback extraction", partNumber);

                        string? description = null;
                        int? lotQty = null;
                        string? kanbanNumber = null;
                        var quantities = new List<int>();

                        // Extract description: uppercase text after part number, before 5-digit lot qty
                        // Description ends when we encounter 5 consecutive digits
                        var descMatch = Regex.Match(lineContext,
                            @"^" + Regex.Escape(partNumber) + @"\s+([A-Z][A-Z\s\-/,]*?)(?=\d{5})",
                            RegexOptions.IgnoreCase);
                        if (descMatch.Success)
                        {
                            description = descMatch.Groups[1].Value.Trim();
                            _logger.LogDebug("        Fallback: Extracted description: '{Description}'", description);
                        }
                        else
                        {
                            _logger.LogDebug("        Fallback: Failed to extract description");
                        }

                        // Extract 5-digit lot quantity (must be followed by uppercase letters for kanban)
                        var lotMatch = Regex.Match(lineContext, @"(\d{5})(?=[A-Z])");
                        if (lotMatch.Success)
                        {
                            lotQty = int.Parse(lotMatch.Groups[1].Value);
                            _logger.LogDebug("        Fallback: Extracted lot qty: {LotQty}", lotQty);
                        }
                        else
                        {
                            _logger.LogDebug("        Fallback: Failed to extract lot qty");
                        }

                        // Extract kanban: 2-6 alphanumeric chars immediately after 5-digit lot qty
                        // Handles formats like FA99, FHC55, HN7X, HM55
                        var kanbanMatch = Regex.Match(lineContext, @"(?<=\d{5})([A-Z0-9]{2,6})");
                        if (kanbanMatch.Success)
                        {
                            kanbanNumber = kanbanMatch.Groups[1].Value;
                            _logger.LogDebug("        Fallback: Extracted kanban: '{KanbanNumber}'", kanbanNumber);

                            // Extract quantities after kanban
                            var qtyMatch = Regex.Match(lineContext,
                                Regex.Escape(kanbanNumber) + @"(\d+)");
                            if (qtyMatch.Success)
                            {
                                quantities = ParseQuantities(qtyMatch.Groups[1].Value, orderCount);
                                _logger.LogDebug("        Fallback: Extracted quantities: [{Quantities}]", string.Join(", ", quantities));
                            }
                            else
                            {
                                _logger.LogDebug("        Fallback: Failed to extract quantities after kanban");
                            }
                        }
                        else
                        {
                            _logger.LogDebug("        Fallback: Failed to extract kanban");
                        }

                        if (quantities.Count == 0)
                        {
                            // Default to zeros if we couldn't extract quantities
                            quantities = Enumerable.Repeat(0, orderCount).ToList();
                            _logger.LogDebug("        Fallback: Defaulting to zero quantities");
                        }

                        lineItems.Add(new LineItemData
                        {
                            PartNumber = partNumber,
                            Description = description,
                            LotQty = lotQty,
                            KanbanNumber = kanbanNumber,
                            QuantitiesByOrder = quantities
                        });

                        _logger.LogInformation("    >>> LINE ITEM ADDED (fallback): Part={PartNumber}, Desc={Description}, Lot={LotQty}, " +
                            "Kanban={KanbanNumber}, Quantities=[{Quantities}]",
                            partNumber, description ?? "NULL", lotQty?.ToString() ?? "NULL", kanbanNumber ?? "NULL", string.Join(", ", quantities));
                    }
                }

                _logger.LogInformation("=== CONCATENATED FORMAT EXTRACTION COMPLETE: Found {Count} items ===", lineItems.Count);
            }

            _logger.LogInformation("Extracted {ItemCount} line items from PDF", lineItems.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting line items");
        }

        return lineItems;
    }

    /// <summary>
    /// Extract quantities for a part row using word coordinates to match quantities to order columns
    /// This handles blank cells correctly by checking X positions
    /// </summary>
    private List<int> ExtractQuantitiesUsingCoordinates(
        List<Word> wordsInRow,
        Dictionary<string, double> orderColumnPositions,
        List<string> orderNumbers)
    {
        var quantities = new List<int>();

        try
        {
            // X tolerance for matching quantity to column (30 pixels)
            const double xTolerance = 30.0;

            _logger.LogDebug("Extracting quantities from {WordCount} words in row", wordsInRow.Count);

            // For each order column, find the quantity word at that X position
            foreach (var orderNum in orderNumbers)
            {
                int qty = 0; // Default to 0 (blank cell)

                if (orderColumnPositions.TryGetValue(orderNum, out double columnX))
                {
                    // Find word(s) in this row that are at this X position (within tolerance)
                    // Look for numeric words only
                    var quantityWord = wordsInRow
                        .Where(w => int.TryParse(w.Text.Trim(), out _)) // Must be numeric
                        .Where(w =>
                        {
                            double wordCenterX = w.BoundingBox.Left + (w.BoundingBox.Width / 2);
                            double distance = Math.Abs(wordCenterX - columnX);
                            return distance <= xTolerance;
                        })
                        .OrderBy(w => Math.Abs((w.BoundingBox.Left + w.BoundingBox.Width / 2) - columnX)) // Closest match
                        .FirstOrDefault();

                    if (quantityWord != null && int.TryParse(quantityWord.Text.Trim(), out int parsedQty))
                    {
                        qty = parsedQty;
                        _logger.LogDebug("Order {OrderNum} (X={ColumnX}): Found quantity {Qty} at X={WordX}",
                            orderNum, columnX, qty, quantityWord.BoundingBox.Left + quantityWord.BoundingBox.Width / 2);
                    }
                    else
                    {
                        _logger.LogDebug("Order {OrderNum} (X={ColumnX}): No quantity found (blank cell) - defaulting to 0",
                            orderNum, columnX);
                    }
                }
                else
                {
                    _logger.LogWarning("Order {OrderNum}: Column position not found - defaulting to 0", orderNum);
                }

                quantities.Add(qty);
            }

            _logger.LogDebug("Extracted quantities using coordinates: [{Quantities}]", string.Join(", ", quantities));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting quantities using coordinates");
            // Return all zeros if there's an error
            quantities = Enumerable.Repeat(0, orderNumbers.Count).ToList();
        }

        return quantities;
    }

    /// <summary>
    /// Parse space-separated quantities (clean format) - FALLBACK METHOD
    /// Example: "2 1" for 2 orders, "4 5 6" for 3 orders
    /// </summary>
    private List<int> ParseSpaceSeparatedQuantities(string quantitiesStr, int expectedCount)
    {
        var quantities = new List<int>();

        if (string.IsNullOrWhiteSpace(quantitiesStr))
        {
            return Enumerable.Repeat(0, expectedCount).ToList();
        }

        // Split by whitespace and parse each number
        var parts = quantitiesStr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (int.TryParse(part.Trim(), out int qty))
            {
                quantities.Add(qty);
            }
        }

        // Pad with zeros if needed
        while (quantities.Count < expectedCount)
        {
            quantities.Add(0);
        }

        // Truncate if we have too many
        return quantities.Take(expectedCount).ToList();
    }

    /// <summary>
    /// Parse quantity string which may have concatenated digits (legacy concatenated format)
    /// Example: "4" for 1 order, "436" for 3 orders (4, 3, 6)
    /// </summary>
    private List<int> ParseQuantities(string quantitiesStr, int expectedCount)
    {
        var quantities = new List<int>();

        if (string.IsNullOrEmpty(quantitiesStr))
        {
            return Enumerable.Repeat(0, expectedCount).ToList();
        }

        // If quantities string length matches expected count, split into single digits
        if (quantitiesStr.Length == expectedCount)
        {
            foreach (char c in quantitiesStr)
            {
                if (char.IsDigit(c))
                {
                    quantities.Add(c - '0');
                }
            }
        }
        // If only 1 quantity expected, parse the whole string as one number
        else if (expectedCount == 1)
        {
            if (int.TryParse(quantitiesStr, out int qty))
            {
                quantities.Add(qty);
            }
        }
        // Try to intelligently split (assume single digits if length > expectedCount)
        else if (quantitiesStr.Length > expectedCount)
        {
            // Take first N digits as individual quantities
            for (int i = 0; i < Math.Min(expectedCount, quantitiesStr.Length); i++)
            {
                if (char.IsDigit(quantitiesStr[i]))
                {
                    quantities.Add(quantitiesStr[i] - '0');
                }
            }
        }
        else
        {
            // Parse what we can and pad with zeros
            foreach (char c in quantitiesStr)
            {
                if (char.IsDigit(c))
                {
                    quantities.Add(c - '0');
                }
            }
        }

        // Pad with zeros if needed
        while (quantities.Count < expectedCount)
        {
            quantities.Add(0);
        }

        return quantities.Take(expectedCount).ToList();
    }

    /// <summary>
    /// Internal class to hold line item data during parsing
    /// </summary>
    private class LineItemData
    {
        public string PartNumber { get; set; } = null!;
        public string? Description { get; set; }
        public int? LotQty { get; set; }
        public string? KanbanNumber { get; set; }
        public List<int> QuantitiesByOrder { get; set; } = new();
    }
}
