// Author: Hassan
// Date: 2025-12-01
// Description: DTO for order file upload request

using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs;

/// <summary>
/// Request DTO for uploading order PDF files
/// </summary>
public class OrderUploadRequestDto
{
    /// <summary>
    /// PDF file to upload
    /// </summary>
    [Required(ErrorMessage = "File is required")]
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// User ID who is uploading the file
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
}
