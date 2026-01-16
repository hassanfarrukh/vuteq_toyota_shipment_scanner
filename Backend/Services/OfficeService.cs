// Author: Hassan
// Date: 2025-11-24
// Updated: 2026-01-16 - Added audit field assignments (Hassan)
// Description: Service for Office management - handles business logic

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for Office service operations
/// </summary>
public interface IOfficeService
{
    Task<ApiResponse<IEnumerable<OfficeDto>>> GetAllOfficesAsync();
    Task<ApiResponse<OfficeDto>> GetOfficeByIdAsync(Guid officeId);
    Task<ApiResponse<OfficeDto>> CreateOfficeAsync(CreateOfficeRequest request, Guid userId);
    Task<ApiResponse<OfficeDto>> UpdateOfficeAsync(Guid officeId, UpdateOfficeRequest request, Guid userId);
    Task<ApiResponse<bool>> DeleteOfficeAsync(Guid officeId);
}

/// <summary>
/// Service implementation for Office management
/// </summary>
public class OfficeService : IOfficeService
{
    private readonly IOfficeRepository _officeRepository;
    private readonly ILogger<OfficeService> _logger;

    public OfficeService(IOfficeRepository officeRepository, ILogger<OfficeService> logger)
    {
        _officeRepository = officeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all offices (active and inactive)
    /// </summary>
    public async Task<ApiResponse<IEnumerable<OfficeDto>>> GetAllOfficesAsync()
    {
        try
        {
            var offices = await _officeRepository.GetAllAsync();
            var officeDtos = offices.Select(MapToDto);

            return ApiResponse<IEnumerable<OfficeDto>>.SuccessResponse(
                officeDtos,
                "Offices retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving offices");
            return ApiResponse<IEnumerable<OfficeDto>>.ErrorResponse(
                "Failed to retrieve offices",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Get office by ID
    /// </summary>
    public async Task<ApiResponse<OfficeDto>> GetOfficeByIdAsync(Guid officeId)
    {
        try
        {
            var office = await _officeRepository.GetByIdAsync(officeId);
            if (office == null)
            {
                return ApiResponse<OfficeDto>.ErrorResponse(
                    "Office not found",
                    $"Office with ID {officeId} does not exist"
                );
            }

            return ApiResponse<OfficeDto>.SuccessResponse(
                MapToDto(office),
                "Office retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving office: {OfficeId}", officeId);
            return ApiResponse<OfficeDto>.ErrorResponse(
                "Failed to retrieve office",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Create a new office
    /// </summary>
    public async Task<ApiResponse<OfficeDto>> CreateOfficeAsync(CreateOfficeRequest request, Guid userId)
    {
        try
        {
            // Check if office code already exists
            if (await _officeRepository.CodeExistsAsync(request.Code))
            {
                return ApiResponse<OfficeDto>.ErrorResponse(
                    "Office creation failed",
                    $"Office code '{request.Code}' already exists"
                );
            }

            var office = new OfficeMaster
            {
                OfficeId = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Zip = request.Zip,
                Phone = request.Phone,
                Contact = request.Contact,
                Email = request.Email,
                IsActive = true,
                CreatedBy = userId.ToString(),
                CreatedAt = DateTime.Now
            };

            var createdOffice = await _officeRepository.CreateAsync(office);

            _logger.LogInformation("Office created successfully: {Code}", createdOffice.Code);

            return ApiResponse<OfficeDto>.SuccessResponse(
                MapToDto(createdOffice),
                "Office created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating office: {Code}", request.Code);
            return ApiResponse<OfficeDto>.ErrorResponse(
                "Failed to create office",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Update an existing office
    /// </summary>
    public async Task<ApiResponse<OfficeDto>> UpdateOfficeAsync(Guid officeId, UpdateOfficeRequest request, Guid userId)
    {
        try
        {
            var office = await _officeRepository.GetByIdAsync(officeId);
            if (office == null)
            {
                return ApiResponse<OfficeDto>.ErrorResponse(
                    "Office update failed",
                    $"Office with ID {officeId} does not exist"
                );
            }

            // Update office properties
            office.Name = request.Name;
            office.Address = request.Address;
            office.City = request.City;
            office.State = request.State;
            office.Zip = request.Zip;
            office.Phone = request.Phone;
            office.Contact = request.Contact;
            office.Email = request.Email;
            office.UpdatedBy = userId.ToString();
            office.UpdatedAt = DateTime.Now;

            var updatedOffice = await _officeRepository.UpdateAsync(office);

            _logger.LogInformation("Office updated successfully: {OfficeId}", officeId);

            return ApiResponse<OfficeDto>.SuccessResponse(
                MapToDto(updatedOffice),
                "Office updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating office: {OfficeId}", officeId);
            return ApiResponse<OfficeDto>.ErrorResponse(
                "Failed to update office",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Delete an office (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteOfficeAsync(Guid officeId)
    {
        try
        {
            var office = await _officeRepository.GetByIdAsync(officeId);
            if (office == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Office deletion failed",
                    $"Office with ID {officeId} does not exist"
                );
            }

            // Check for dependent warehouses
            if (await _officeRepository.HasDependentWarehousesAsync(office.Code))
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Office deletion failed",
                    "Cannot delete office with active warehouses. Please deactivate or reassign warehouses first."
                );
            }

            var deleted = await _officeRepository.DeleteAsync(officeId);

            if (deleted)
            {
                _logger.LogInformation("Office deleted successfully: {OfficeId}", officeId);
                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Office deleted successfully"
                );
            }

            return ApiResponse<bool>.ErrorResponse(
                "Office deletion failed",
                "Failed to delete office"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting office: {OfficeId}", officeId);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete office",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Map OfficeMaster entity to OfficeDto
    /// </summary>
    private static OfficeDto MapToDto(OfficeMaster office)
    {
        return new OfficeDto
        {
            OfficeId = office.OfficeId,
            Code = office.Code,
            Name = office.Name,
            Address = office.Address,
            City = office.City,
            State = office.State,
            Zip = office.Zip,
            Phone = office.Phone,
            Contact = office.Contact,
            Email = office.Email,
            IsActive = office.IsActive,
            CreatedAt = office.CreatedAt,
            UpdatedAt = office.UpdatedAt
        };
    }
}
