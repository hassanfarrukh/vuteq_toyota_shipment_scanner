/**
 * Administration Helpers
 * Author: Hassan
 * Date: 2025-11-24
 *
 * Helper functions for administration page API operations
 */

import { officesApi, warehousesApi, usersApi } from '@/lib/api';
import type { Office, OfficeDto, Warehouse, WarehouseDto, User, CreateUserDto, UpdateUserDto } from '@/lib/api';

// Office Operations
export async function fetchOffices() {
  const result = await officesApi.getOffices();
  return result;
}

export async function addOffice(data: OfficeDto) {
  const result = await officesApi.createOffice(data);
  return result;
}

export async function updateOffice(id: string, data: OfficeDto) {
  const result = await officesApi.updateOffice(id, data);
  return result;
}

export async function removeOffice(id: string) {
  const result = await officesApi.deleteOffice(id);
  return result;
}

// Warehouse Operations
export async function fetchWarehouses() {
  const result = await warehousesApi.getWarehouses();
  return result;
}

export async function addWarehouse(data: WarehouseDto) {
  const result = await warehousesApi.createWarehouse(data);
  return result;
}

export async function updateWarehouse(id: string, data: WarehouseDto) {
  const result = await warehousesApi.updateWarehouse(id, data);
  return result;
}

export async function removeWarehouse(id: string) {
  const result = await warehousesApi.deleteWarehouse(id);
  return result;
}

// User Operations
export async function fetchUsers() {
  const result = await usersApi.getUsers();
  return result;
}

export async function addUser(data: CreateUserDto) {
  const result = await usersApi.createUser(data);
  return result;
}

export async function updateUser(id: string, data: UpdateUserDto) {
  const result = await usersApi.updateUser(id, data);
  return result;
}

export async function removeUser(id: string) {
  const result = await usersApi.deleteUser(id);
  return result;
}
