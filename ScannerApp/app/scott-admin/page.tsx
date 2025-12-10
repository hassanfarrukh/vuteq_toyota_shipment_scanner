/**
 * Scott's Administration Page
 * Author: Hassan
 * Date: 2025-10-28
 * Updated: 2025-10-29 - Changed fa to fa-solid for all icons (solid/bold style) by Hassan
 * Updated: 2025-10-29 - Changed delete icons to fa-duotone fa-light with VUTEQ colors (Hassan)
 *
 * EXACT Scott's approach:
 * - Dropdown selector at top with [ADD] option
 * - Form below (NO tables)
 * - Save/Delete buttons
 *
 * 3 Tabs:
 * 1. Office: 9 fields (Office Code dropdown, State dropdown, form)
 * 2. Warehouse: 9 fields (Office dropdown, Warehouse dropdown, State dropdown, form)
 * 3. User: 11 fields (Office dropdown, User dropdown, radio buttons for Menu Level/Operation, dynamic Code dropdown)
 */

'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Alert from '@/components/ui/Alert';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';

type Tab = 'office' | 'warehouse' | 'user';

// Dummy data
const DUMMY_OFFICES = [
  { code: 'IND', name: 'Indiana Office', address: '123 Main St', city: 'Indianapolis', state: 'IN', zip: '46225', phone: '317-555-0100', contact: 'John Doe', email: 'indiana@vuteq.com' },
  { code: 'MIC', name: 'Michigan Office', address: '456 Oak Ave', city: 'Detroit', state: 'MI', zip: '48226', phone: '313-555-0200', contact: 'Jane Smith', email: 'michigan@vuteq.com' },
  { code: 'OHI', name: 'Ohio Office', address: '789 Elm St', city: 'Columbus', state: 'OH', zip: '43215', phone: '614-555-0300', contact: 'Bob Johnson', email: 'ohio@vuteq.com' },
];

const DUMMY_WAREHOUSES = [
  { code: 'IND-WH1', name: 'Indiana Main Warehouse', office: 'IND', address: '100 Industrial Blvd', city: 'Indianapolis', state: 'IN', zip: '46225', phone: '317-555-0150', contactName: 'Mike Johnson', contactEmail: 'mike.j@vuteq.com' },
  { code: 'MIC-WH1', name: 'Michigan Distribution Center', office: 'MIC', address: '200 Warehouse Rd', city: 'Detroit', state: 'MI', zip: '48226', phone: '313-555-0250', contactName: 'Sarah Lee', contactEmail: 'sarah.l@vuteq.com' },
  { code: 'OHI-WH1', name: 'Ohio Storage Facility', office: 'OHI', address: '300 Logistics Way', city: 'Columbus', state: 'OH', zip: '43215', phone: '614-555-0350', contactName: 'Tom Davis', contactEmail: 'tom.d@vuteq.com' },
];

const DUMMY_USERS = [
  { userId: 'operator1', password: '********', userName: 'John Operator', nickName: 'JohnO', email: 'john.operator@vuteq.com', notificationName: 'John Operator', notificationEmail: 'john.operator@vuteq.com', office: 'IND', menuLevel: 'Scanner', operation: 'Warehouse', code: 'WH01' },
  { userId: 'supervisor1', password: '********', userName: 'Jane Supervisor', nickName: 'JaneS', email: 'jane.supervisor@vuteq.com', notificationName: 'Jane Supervisor', notificationEmail: 'jane.supervisor@vuteq.com', office: 'MIC', menuLevel: 'Scanner', operation: 'Warehouse', code: 'WH01' },
  { userId: 'admin1', password: '********', userName: 'Bob Administrator', nickName: 'BobA', email: 'bob.admin@vuteq.com', notificationName: 'Bob Administrator', notificationEmail: 'bob.admin@vuteq.com', office: 'OHI', menuLevel: 'Admin', operation: 'Administration', code: 'ADM01' },
];

const US_STATES = [
  'AL', 'AK', 'AZ', 'AR', 'CA', 'CO', 'CT', 'DE', 'FL', 'GA',
  'HI', 'ID', 'IL', 'IN', 'IA', 'KS', 'KY', 'LA', 'ME', 'MD',
  'MA', 'MI', 'MN', 'MS', 'MO', 'MT', 'NE', 'NV', 'NH', 'NJ',
  'NM', 'NY', 'NC', 'ND', 'OH', 'OK', 'OR', 'PA', 'RI', 'SC',
  'SD', 'TN', 'TX', 'UT', 'VT', 'VA', 'WA', 'WV', 'WI', 'WY'
];

export default function ScottAdminPage() {
  const router = useRouter();
  const { user } = useAuth();

  // Redirect if not admin
  if (user && user.role !== 'ADMIN') {
    router.push('/');
    return null;
  }

  const [activeTab, setActiveTab] = useState<Tab>('office');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Office Tab States
  const [selectedOffice, setSelectedOffice] = useState<string>('');
  const [officeForm, setOfficeForm] = useState({
    code: '', name: '', address: '', city: '', state: '', zip: '', phone: '', contact: '', email: ''
  });

  // Warehouse Tab States
  const [selectedWarehouse, setSelectedWarehouse] = useState<string>('');
  const [warehouseForm, setWarehouseForm] = useState({
    code: '', name: '', office: '', address: '', city: '', state: '', zip: '', phone: '', contactName: '', contactEmail: ''
  });

  // User Tab States
  const [selectedUser, setSelectedUser] = useState<string>('');
  const [userForm, setUserForm] = useState({
    userId: '', password: '', userName: '', nickName: '', email: '', notificationName: '', notificationEmail: '', office: '', menuLevel: 'Scanner', operation: 'Warehouse', code: ''
  });

  // Handle Office selection
  const handleOfficeSelect = (code: string) => {
    if (code === '[ADD]') {
      setSelectedOffice('[ADD]');
      setOfficeForm({ code: '', name: '', address: '', city: '', state: '', zip: '', phone: '', contact: '', email: '' });
    } else {
      setSelectedOffice(code);
      const office = DUMMY_OFFICES.find(o => o.code === code);
      if (office) {
        setOfficeForm(office);
      }
    }
  };

  // Handle Warehouse selection
  const handleWarehouseSelect = (code: string) => {
    if (code === '[ADD]') {
      setSelectedWarehouse('[ADD]');
      setWarehouseForm({ code: '', name: '', office: '', address: '', city: '', state: '', zip: '', phone: '', contactName: '', contactEmail: '' });
    } else {
      setSelectedWarehouse(code);
      const warehouse = DUMMY_WAREHOUSES.find(w => w.code === code);
      if (warehouse) {
        setWarehouseForm(warehouse);
      }
    }
  };

  // Handle User selection
  const handleUserSelect = (userId: string) => {
    if (userId === '[ADD]') {
      setSelectedUser('[ADD]');
      setUserForm({ userId: '', password: '', userName: '', nickName: '', email: '', notificationName: '', notificationEmail: '', office: '', menuLevel: 'Scanner', operation: 'Warehouse', code: '' });
    } else {
      setSelectedUser(userId);
      const userRecord = DUMMY_USERS.find(u => u.userId === userId);
      if (userRecord) {
        setUserForm(userRecord);
      }
    }
  };

  // Save handlers
  const handleSaveOffice = () => {
    console.log('Save office:', officeForm);
    setSuccess('Office saved successfully! (Phase 1: Console only)');
    setTimeout(() => setSuccess(null), 3000);
  };

  const handleDeleteOffice = () => {
    console.log('Delete office:', selectedOffice);
    setSuccess('Office deleted successfully! (Phase 1: Console only)');
    setSelectedOffice('');
    setOfficeForm({ code: '', name: '', address: '', city: '', state: '', zip: '', phone: '', contact: '', email: '' });
    setTimeout(() => setSuccess(null), 3000);
  };

  const handleSaveWarehouse = () => {
    console.log('Save warehouse:', warehouseForm);
    setSuccess('Warehouse saved successfully! (Phase 1: Console only)');
    setTimeout(() => setSuccess(null), 3000);
  };

  const handleDeleteWarehouse = () => {
    console.log('Delete warehouse:', selectedWarehouse);
    setSuccess('Warehouse deleted successfully! (Phase 1: Console only)');
    setSelectedWarehouse('');
    setWarehouseForm({ code: '', name: '', office: '', address: '', city: '', state: '', zip: '', phone: '', contactName: '', contactEmail: '' });
    setTimeout(() => setSuccess(null), 3000);
  };

  const handleSaveUser = () => {
    console.log('Save user:', userForm);
    setSuccess('User saved successfully! (Phase 1: Console only)');
    setTimeout(() => setSuccess(null), 3000);
  };

  const handleDeleteUser = () => {
    console.log('Delete user:', selectedUser);
    setSuccess('User deleted successfully! (Phase 1: Console only)');
    setSelectedUser('');
    setUserForm({ userId: '', password: '', userName: '', nickName: '', email: '', notificationName: '', notificationEmail: '', office: '', menuLevel: 'Scanner', operation: 'Warehouse', code: '' });
    setTimeout(() => setSuccess(null), 3000);
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-8 pt-24 w-full space-y-6 max-w-7xl mx-auto">
          {/* Success Alert */}
          {success && (
            <Alert variant="success" onClose={() => setSuccess(null)}>
              {success}
            </Alert>
          )}

          {/* Error Alert */}
          {error && (
            <Alert variant="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          {/* Tab Navigation */}
          <Card>
            <div className="border-b border-gray-200">
              <nav className="flex -mb-px overflow-x-auto">
                <button
                  onClick={() => setActiveTab('office')}
                  className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                    activeTab === 'office'
                      ? 'border-[#253262] text-[#253262]'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <i className="fa-solid fa-building" style={{ fontSize: '20px' }}></i>
                  Office
                </button>
                <button
                  onClick={() => setActiveTab('warehouse')}
                  className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                    activeTab === 'warehouse'
                      ? 'border-[#253262] text-[#253262]'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <i className="fa-solid fa-warehouse" style={{ fontSize: '20px' }}></i>
                  Warehouse
                </button>
                <button
                  onClick={() => setActiveTab('user')}
                  className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                    activeTab === 'user'
                      ? 'border-[#253262] text-[#253262]'
                      : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
                >
                  <i className="fa-solid fa-users" style={{ fontSize: '20px' }}></i>
                  User
                </button>
              </nav>
            </div>
          </Card>

          {/* OFFICE TAB */}
          {activeTab === 'office' && (
            <Card>
              <CardHeader>
                <CardTitle>Office Management</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {/* Office Selector Dropdown */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Select Office</label>
                    <select
                      value={selectedOffice}
                      onChange={(e) => handleOfficeSelect(e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                    >
                      <option value="">-- Select an Office --</option>
                      <option value="[ADD]" style={{ fontWeight: 'bold', color: '#253262' }}>[ADD NEW OFFICE]</option>
                      {DUMMY_OFFICES.map((office) => (
                        <option key={office.code} value={office.code}>
                          {office.code} - {office.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Office Form - Appears when office is selected */}
                  {selectedOffice && (
                    <>
                      <div className="border-t border-gray-200 pt-6">
                        <h3 className="text-lg font-medium text-gray-900 mb-4">
                          {selectedOffice === '[ADD]' ? 'Add New Office' : 'Edit Office'}
                        </h3>

                        <div className="space-y-4">
                          {/* Office Code */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Office Code *</label>
                            <Input
                              value={officeForm.code}
                              onChange={(e) => setOfficeForm({...officeForm, code: e.target.value})}
                              disabled={selectedOffice !== '[ADD]'}
                            />
                          </div>

                          {/* Office Name */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Office Name *</label>
                            <Input
                              value={officeForm.name}
                              onChange={(e) => setOfficeForm({...officeForm, name: e.target.value})}
                            />
                          </div>

                          {/* Address */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Address *</label>
                            <Input
                              value={officeForm.address}
                              onChange={(e) => setOfficeForm({...officeForm, address: e.target.value})}
                            />
                          </div>

                          {/* City */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">City *</label>
                            <Input
                              value={officeForm.city}
                              onChange={(e) => setOfficeForm({...officeForm, city: e.target.value})}
                            />
                          </div>

                          {/* State Dropdown */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">State *</label>
                            <select
                              value={officeForm.state}
                              onChange={(e) => setOfficeForm({...officeForm, state: e.target.value})}
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                            >
                              <option value="">-- Select State --</option>
                              {US_STATES.map((state) => (
                                <option key={state} value={state}>{state}</option>
                              ))}
                            </select>
                          </div>

                          {/* Zip */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Zip Code *</label>
                            <Input
                              value={officeForm.zip}
                              onChange={(e) => setOfficeForm({...officeForm, zip: e.target.value})}
                            />
                          </div>

                          {/* Phone */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
                            <Input
                              value={officeForm.phone}
                              onChange={(e) => setOfficeForm({...officeForm, phone: e.target.value})}
                            />
                          </div>

                          {/* Contact Person */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Contact Person</label>
                            <Input
                              value={officeForm.contact}
                              onChange={(e) => setOfficeForm({...officeForm, contact: e.target.value})}
                            />
                          </div>

                          {/* Email */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                            <Input
                              type="email"
                              value={officeForm.email}
                              onChange={(e) => setOfficeForm({...officeForm, email: e.target.value})}
                            />
                          </div>
                        </div>
                      </div>

                      {/* Save/Delete Buttons */}
                      <div className="flex gap-3 pt-4 border-t border-gray-200">
                        <Button onClick={handleSaveOffice} className="flex-1">
                          <i className="fa-solid fa-save mr-2"></i>
                          Save Office
                        </Button>
                        {selectedOffice !== '[ADD]' && (
                          <Button onClick={handleDeleteOffice} variant="tertiary" className="flex-1">
                            <i className="fa-duotone fa-regular fa-trash mr-2" style={{ '--fa-primary-color': '#D2312E', '--fa-secondary-color': '#253262', '--fa-primary-opacity': 1, '--fa-secondary-opacity': 1 } as React.CSSProperties}></i>
                            Delete Office
                          </Button>
                        )}
                      </div>
                    </>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          {/* WAREHOUSE TAB */}
          {activeTab === 'warehouse' && (
            <Card>
              <CardHeader>
                <CardTitle>Warehouse Management</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {/* Warehouse Selector Dropdown */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Select Warehouse</label>
                    <select
                      value={selectedWarehouse}
                      onChange={(e) => handleWarehouseSelect(e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                    >
                      <option value="">-- Select a Warehouse --</option>
                      <option value="[ADD]" style={{ fontWeight: 'bold', color: '#253262' }}>[ADD NEW WAREHOUSE]</option>
                      {DUMMY_WAREHOUSES.map((warehouse) => (
                        <option key={warehouse.code} value={warehouse.code}>
                          {warehouse.code} - {warehouse.name}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* Warehouse Form - Appears when warehouse is selected */}
                  {selectedWarehouse && (
                    <>
                      <div className="border-t border-gray-200 pt-6">
                        <h3 className="text-lg font-medium text-gray-900 mb-4">
                          {selectedWarehouse === '[ADD]' ? 'Add New Warehouse' : 'Edit Warehouse'}
                        </h3>

                        <div className="space-y-4">
                          {/* Office Dropdown */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Office *</label>
                            <select
                              value={warehouseForm.office}
                              onChange={(e) => setWarehouseForm({...warehouseForm, office: e.target.value})}
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                            >
                              <option value="">-- Select Office --</option>
                              {DUMMY_OFFICES.map((office) => (
                                <option key={office.code} value={office.code}>{office.name}</option>
                              ))}
                            </select>
                          </div>

                          {/* Warehouse Code */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Warehouse Code *</label>
                            <Input
                              value={warehouseForm.code}
                              onChange={(e) => setWarehouseForm({...warehouseForm, code: e.target.value})}
                              disabled={selectedWarehouse !== '[ADD]'}
                            />
                          </div>

                          {/* Warehouse Name */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Warehouse Name *</label>
                            <Input
                              value={warehouseForm.name}
                              onChange={(e) => setWarehouseForm({...warehouseForm, name: e.target.value})}
                            />
                          </div>

                          {/* Address */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Address *</label>
                            <Input
                              value={warehouseForm.address}
                              onChange={(e) => setWarehouseForm({...warehouseForm, address: e.target.value})}
                            />
                          </div>

                          {/* City */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">City *</label>
                            <Input
                              value={warehouseForm.city}
                              onChange={(e) => setWarehouseForm({...warehouseForm, city: e.target.value})}
                            />
                          </div>

                          {/* State Dropdown */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">State *</label>
                            <select
                              value={warehouseForm.state}
                              onChange={(e) => setWarehouseForm({...warehouseForm, state: e.target.value})}
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                            >
                              <option value="">-- Select State --</option>
                              {US_STATES.map((state) => (
                                <option key={state} value={state}>{state}</option>
                              ))}
                            </select>
                          </div>

                          {/* Zip */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Zip Code *</label>
                            <Input
                              value={warehouseForm.zip}
                              onChange={(e) => setWarehouseForm({...warehouseForm, zip: e.target.value})}
                            />
                          </div>

                          {/* Phone */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
                            <Input
                              value={warehouseForm.phone}
                              onChange={(e) => setWarehouseForm({...warehouseForm, phone: e.target.value})}
                            />
                          </div>

                          {/* Contact Name */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Contact Name</label>
                            <Input
                              value={warehouseForm.contactName}
                              onChange={(e) => setWarehouseForm({...warehouseForm, contactName: e.target.value})}
                            />
                          </div>

                          {/* Contact Email */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Contact Email</label>
                            <Input
                              type="email"
                              value={warehouseForm.contactEmail}
                              onChange={(e) => setWarehouseForm({...warehouseForm, contactEmail: e.target.value})}
                            />
                          </div>
                        </div>
                      </div>

                      {/* Save/Delete Buttons */}
                      <div className="flex gap-3 pt-4 border-t border-gray-200">
                        <Button onClick={handleSaveWarehouse} className="flex-1">
                          <i className="fa-solid fa-save mr-2"></i>
                          Save Warehouse
                        </Button>
                        {selectedWarehouse !== '[ADD]' && (
                          <Button onClick={handleDeleteWarehouse} variant="tertiary" className="flex-1">
                            <i className="fa-duotone fa-regular fa-trash mr-2" style={{ '--fa-primary-color': '#D2312E', '--fa-secondary-color': '#253262', '--fa-primary-opacity': 1, '--fa-secondary-opacity': 1 } as React.CSSProperties}></i>
                            Delete Warehouse
                          </Button>
                        )}
                      </div>
                    </>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          {/* USER TAB */}
          {activeTab === 'user' && (
            <Card>
              <CardHeader>
                <CardTitle>User Management</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {/* User Selector Dropdown */}
                  <div>
                    <label className="block text-sm font-medium text-gray-700 mb-2">Select User</label>
                    <select
                      value={selectedUser}
                      onChange={(e) => handleUserSelect(e.target.value)}
                      className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                    >
                      <option value="">-- Select a User --</option>
                      <option value="[ADD]" style={{ fontWeight: 'bold', color: '#253262' }}>[ADD NEW USER]</option>
                      {DUMMY_USERS.map((user) => (
                        <option key={user.userId} value={user.userId}>
                          {user.userId} - {user.userName}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* User Form - Appears when user is selected */}
                  {selectedUser && (
                    <>
                      <div className="border-t border-gray-200 pt-6">
                        <h3 className="text-lg font-medium text-gray-900 mb-4">
                          {selectedUser === '[ADD]' ? 'Add New User' : 'Edit User'}
                        </h3>

                        <div className="space-y-4">
                          {/* Office Dropdown */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Office *</label>
                            <select
                              value={userForm.office}
                              onChange={(e) => setUserForm({...userForm, office: e.target.value})}
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                            >
                              <option value="">-- Select Office --</option>
                              {DUMMY_OFFICES.map((office) => (
                                <option key={office.code} value={office.code}>{office.name}</option>
                              ))}
                            </select>
                          </div>

                          {/* User ID */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">User ID *</label>
                            <Input
                              value={userForm.userId}
                              onChange={(e) => setUserForm({...userForm, userId: e.target.value})}
                              disabled={selectedUser !== '[ADD]'}
                            />
                          </div>

                          {/* Password */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Password *</label>
                            <Input
                              type="password"
                              value={userForm.password}
                              onChange={(e) => setUserForm({...userForm, password: e.target.value})}
                            />
                          </div>

                          {/* User Name */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">User Name *</label>
                            <Input
                              value={userForm.userName}
                              onChange={(e) => setUserForm({...userForm, userName: e.target.value})}
                            />
                          </div>

                          {/* Nick Name */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Nick Name</label>
                            <Input
                              value={userForm.nickName}
                              onChange={(e) => setUserForm({...userForm, nickName: e.target.value})}
                            />
                          </div>

                          {/* Email */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
                            <Input
                              type="email"
                              value={userForm.email}
                              onChange={(e) => setUserForm({...userForm, email: e.target.value})}
                            />
                          </div>

                          {/* Notification Name */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Notification Name</label>
                            <Input
                              value={userForm.notificationName}
                              onChange={(e) => setUserForm({...userForm, notificationName: e.target.value})}
                            />
                          </div>

                          {/* Notification Email */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Notification Email</label>
                            <Input
                              type="email"
                              value={userForm.notificationEmail}
                              onChange={(e) => setUserForm({...userForm, notificationEmail: e.target.value})}
                            />
                          </div>

                          {/* Menu Level - Radio Buttons */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Menu Level *</label>
                            <div className="flex gap-6">
                              <label className="flex items-center cursor-pointer">
                                <input
                                  type="radio"
                                  name="menuLevel"
                                  value="Scanner"
                                  checked={userForm.menuLevel === 'Scanner'}
                                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                                />
                                <span className="ml-2 text-base text-gray-700">Scanner</span>
                              </label>
                              <label className="flex items-center cursor-pointer">
                                <input
                                  type="radio"
                                  name="menuLevel"
                                  value="Admin"
                                  checked={userForm.menuLevel === 'Admin'}
                                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                                />
                                <span className="ml-2 text-base text-gray-700">Admin</span>
                              </label>
                              <label className="flex items-center cursor-pointer">
                                <input
                                  type="radio"
                                  name="menuLevel"
                                  value="Supervisor"
                                  checked={userForm.menuLevel === 'Supervisor'}
                                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                                />
                                <span className="ml-2 text-base text-gray-700">Supervisor</span>
                              </label>
                            </div>
                          </div>

                          {/* Operation - Radio Buttons */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">Operation *</label>
                            <div className="flex gap-6">
                              <label className="flex items-center cursor-pointer">
                                <input
                                  type="radio"
                                  name="operation"
                                  value="Warehouse"
                                  checked={userForm.operation === 'Warehouse'}
                                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                                />
                                <span className="ml-2 text-base text-gray-700">Warehouse</span>
                              </label>
                              <label className="flex items-center cursor-pointer">
                                <input
                                  type="radio"
                                  name="operation"
                                  value="Administration"
                                  checked={userForm.operation === 'Administration'}
                                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                                />
                                <span className="ml-2 text-base text-gray-700">Administration</span>
                              </label>
                            </div>
                          </div>

                          {/* Code Dropdown (Dynamic based on Operation) */}
                          <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                              {userForm.operation === 'Warehouse' ? 'Warehouse Code *' : 'Admin Code *'}
                            </label>
                            <select
                              value={userForm.code}
                              onChange={(e) => setUserForm({...userForm, code: e.target.value})}
                              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent text-base"
                            >
                              <option value="">-- Select Code --</option>
                              {userForm.operation === 'Warehouse' ? (
                                DUMMY_WAREHOUSES.map((warehouse) => (
                                  <option key={warehouse.code} value={warehouse.code}>{warehouse.code} - {warehouse.name}</option>
                                ))
                              ) : (
                                <>
                                  <option value="ADM01">ADM01 - Administration</option>
                                  <option value="ADM02">ADM02 - System Admin</option>
                                </>
                              )}
                            </select>
                          </div>
                        </div>
                      </div>

                      {/* Save/Delete Buttons */}
                      <div className="flex gap-3 pt-4 border-t border-gray-200">
                        <Button onClick={handleSaveUser} className="flex-1">
                          <i className="fa-solid fa-save mr-2"></i>
                          Save User
                        </Button>
                        {selectedUser !== '[ADD]' && (
                          <Button onClick={handleDeleteUser} variant="tertiary" className="flex-1">
                            <i className="fa-duotone fa-regular fa-trash mr-2" style={{ '--fa-primary-color': '#D2312E', '--fa-secondary-color': '#253262', '--fa-primary-opacity': 1, '--fa-secondary-opacity': 1 } as React.CSSProperties}></i>
                            Delete User
                          </Button>
                        )}
                      </div>
                    </>
                  )}
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
