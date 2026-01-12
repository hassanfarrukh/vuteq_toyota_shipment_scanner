// Author: Hassan
// Date: 2025-11-24
// Description: Service for Warehouse management - handles business logic

using Backend.Models;
using Backend.Models.DTOs;
using Backend.Models.Entities;
using Backend.Repositories;

namespace Backend.Services;

/// <summary>
/// Interface for Warehouse service operations
/// </summary>
public interface IWarehouseService
{
    Task<ApiResponse<IEnumerable<WarehouseDto>>> GetAllWarehousesAsync();
    Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid warehouseId);
    Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseRequest request);
    Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequest request);
    Task<ApiResponse<bool>> DeleteWarehouseAsync(Guid warehouseId);
}

/// <summary>
/// Service implementation for Warehouse management
/// </summary>
public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IOfficeRepository _officeRepository;
    private readonly ILogger<WarehouseService> _logger;

    public WarehouseService(
        IWarehouseRepository warehouseRepository,
        IOfficeRepository officeRepository,
        ILogger<WarehouseService> logger)
    {
        _warehouseRepository = warehouseRepository;
        _officeRepository = officeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Get all warehouses (active and inactive)
    /// </summary>
    public async Task<ApiResponse<IEnumerable<WarehouseDto>>> GetAllWarehousesAsync()
    {
        try
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            var warehouseDtos = warehouses.Select(MapToDto);

            return ApiResponse<IEnumerable<WarehouseDto>>.SuccessResponse(
                warehouseDtos,
                "Warehouses retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouses");
            return ApiResponse<IEnumerable<WarehouseDto>>.ErrorResponse(
                "Failed to retrieve warehouses",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Get warehouse by ID
    /// </summary>
    public async Task<ApiResponse<WarehouseDto>> GetWarehouseByIdAsync(Guid warehouseId)
    {
        try
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                return ApiResponse<WarehouseDto>.ErrorResponse(
                    "Warehouse not found",
                    $"Warehouse with ID {warehouseId} does not exist"
                );
            }

            return ApiResponse<WarehouseDto>.SuccessResponse(
                MapToDto(warehouse),
                "Warehouse retrieved successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving warehouse: {WarehouseId}", warehouseId);
            return ApiResponse<WarehouseDto>.ErrorResponse(
                "Failed to retrieve warehouse",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Create a new warehouse
    /// </summary>
    public async Task<ApiResponse<WarehouseDto>> CreateWarehouseAsync(CreateWarehouseRequest request)
    {
        try
        {
            // Check if warehouse code already exists
            if (await _warehouseRepository.CodeExistsAsync(request.Code))
            {
                return ApiResponse<WarehouseDto>.ErrorResponse(
                    "Warehouse creation failed",
                    $"Warehouse code '{request.Code}' already exists"
                );
            }

            // Validate that the office exists
            var office = await _officeRepository.GetByCodeAsync(request.Office);
            if (office == null)
            {
                return ApiResponse<WarehouseDto>.ErrorResponse(
                    "Warehouse creation failed",
                    $"Office with code '{request.Office}' does not exist"
                );
            }

            var warehouse = new WarehouseMaster
            {
                WarehouseId = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Zip = request.Zip,
                Phone = request.Phone,
                ContactName = request.ContactName,
                ContactEmail = request.ContactEmail,
                OfficeCode = request.Office,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var createdWarehouse = await _warehouseRepository.CreateAsync(warehouse);

            _logger.LogInformation("Warehouse created successfully: {Code}", createdWarehouse.Code);

            return ApiResponse<WarehouseDto>.SuccessResponse(
                MapToDto(createdWarehouse),
                "Warehouse created successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warehouse: {Code}", request.Code);
            return ApiResponse<WarehouseDto>.ErrorResponse(
                "Failed to create warehouse",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Update an existing warehouse
    /// </summary>
    public async Task<ApiResponse<WarehouseDto>> UpdateWarehouseAsync(Guid warehouseId, UpdateWarehouseRequest request)
    {
        try
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                return ApiResponse<WarehouseDto>.ErrorResponse(
                    "Warehouse update failed",
                    $"Warehouse with ID {warehouseId} does not exist"
                );
            }

            // Validate that the office exists
            var office = await _officeRepository.GetByCodeAsync(request.Office);
            if (office == null)
            {
                return ApiResponse<WarehouseDto>.ErrorResponse(
                    "Warehouse update failed",
                    $"Office with code '{request.Office}' does not exist"
                );
            }

            // Update warehouse properties (Code and Name are read-only in edit mode as per requirements)
            warehouse.Address = request.Address;
            warehouse.City = request.City;
            warehouse.State = request.State;
            warehouse.Zip = request.Zip;
            warehouse.Phone = request.Phone;
            warehouse.ContactName = request.ContactName;
            warehouse.ContactEmail = request.ContactEmail;
            warehouse.OfficeCode = request.Office;
            warehouse.UpdatedAt = DateTime.Now;

            var updatedWarehouse = await _warehouseRepository.UpdateAsync(warehouse);

            _logger.LogInformation("Warehouse updated successfully: {WarehouseId}", warehouseId);

            return ApiResponse<WarehouseDto>.SuccessResponse(
                MapToDto(updatedWarehouse),
                "Warehouse updated successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warehouse: {WarehouseId}", warehouseId);
            return ApiResponse<WarehouseDto>.ErrorResponse(
                "Failed to update warehouse",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Delete a warehouse (soft delete)
    /// </summary>
    public async Task<ApiResponse<bool>> DeleteWarehouseAsync(Guid warehouseId)
    {
        try
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
            {
                return ApiResponse<bool>.ErrorResponse(
                    "Warehouse deletion failed",
                    $"Warehouse with ID {warehouseId} does not exist"
                );
            }

            var deleted = await _warehouseRepository.DeleteAsync(warehouseId);

            if (deleted)
            {
                _logger.LogInformation("Warehouse deleted successfully: {WarehouseId}", warehouseId);
                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Warehouse deleted successfully"
                );
            }

            return ApiResponse<bool>.ErrorResponse(
                "Warehouse deletion failed",
                "Failed to delete warehouse"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warehouse: {WarehouseId}", warehouseId);
            return ApiResponse<bool>.ErrorResponse(
                "Failed to delete warehouse",
                ex.Message
            );
        }
    }

    /// <summary>
    /// Map WarehouseMaster entity to WarehouseDto
    /// </summary>
    private static WarehouseDto MapToDto(WarehouseMaster warehouse)
    {
        return new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            Code = warehouse.Code,
            Name = warehouse.Name,
            Address = warehouse.Address,
            City = warehouse.City,
            State = warehouse.State,
            Zip = warehouse.Zip,
            Phone = warehouse.Phone,
            ContactName = warehouse.ContactName,
            ContactEmail = warehouse.ContactEmail,
            Office = warehouse.OfficeCode,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt,
            UpdatedAt = warehouse.UpdatedAt
        };
    }
}
