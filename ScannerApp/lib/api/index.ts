/**
 * API Index - Central export for all API modules
 * Author: Hassan
 * Date: 2025-11-24
 */

// Export client and utilities
export { default as apiClient, getErrorMessage } from './client';

// Export auth API
export * as authApi from './auth';

// Export offices API
export * as officesApi from './offices';
export type { Office, OfficeDto } from './offices';

// Export warehouses API
export * as warehousesApi from './warehouses';
export type { Warehouse, WarehouseDto } from './warehouses';

// Export users API
export * as usersApi from './users';
export type { User, CreateUserDto, UpdateUserDto } from './users';

// Export settings API
export * as settingsApi from './settings';
export type { InternalKanbanSettings, DockMonitorSettings } from './settings';

// Export order uploads API
export * as orderUploadsApi from './orderUploads';
export type { OrderUploadResponseDto, ExtractedOrderDto, ExtractedOrderItemDto } from './orderUploads';

// Export dock monitor API
export * as dockMonitorApi from './dock-monitor';
export type { DockMonitorOrder, DockMonitorShipment, DockMonitorResponse } from './dock-monitor';
export type { DockMonitorSettings as DockMonitorDataSettings } from './dock-monitor';
