/**
 * Administration Page
 * Author: Hassan
 * Date: 2025-10-27
 * Updated: 2025-10-27 - Desktop-optimized design for Supervisor/Admin users
 * Updated: 2025-10-27 - Updated all tabs to match Scott's UI fields exactly
 * Updated: 2025-10-27 - Added SlideOutPanel for add/edit operations
 * Updated: 2025-10-27 - Fixed scroll issues: background fixed, content scrolls, no horizontal scroll
 * Updated: 2025-10-28 - Updated Dock Monitor buttons layout for responsive design
 * Updated: 2025-10-28 - Removed heading card, subtitle now in Header
 * Updated: 2025-10-28 - Fixed Font Awesome icon classes: changed all fa-solid to fa for tabs and action buttons
 * Updated: 2025-10-29 - Changed fa to fa-solid for all icons (solid/bold style) by Hassan
 * Updated: 2025-10-29 - Changed table edit/delete icons to fa-duotone fa-light with VUTEQ colors (Hassan)
 * Updated: 2025-10-28 - Matched Scott's form control types:
 *   - Office Tab: Changed State field from text input to dropdown with all 50 US states
 *   - Warehouse Tab: Changed State field to dropdown, made W/H Name read-only when editing
 *   - User Tab: Added Supervisor checkbox, changed Menu Level to radio buttons (Admin/Scanner/Operation),
 *               changed Operation to radio buttons (Warehouse/Office/Administration), changed Code to dropdown
 * Updated: 2025-10-28 - Fixed action buttons in User table to display horizontally (inline) instead of stacked vertically
 * Updated: 2025-10-28 - Redesigned Dock Monitor settings with sleek card-based UI: radio buttons with visible descriptions,
 *               VUTEQ navy color (#253262), improved spacing and hover states (Hassan)
 * Updated: 2025-10-28 - Changed Dock Monitor Display Mode section to 2x2 grid layout (single column on mobile,
 *               2 columns on desktop) for better visual organization and space utilization (Hassan)
 * Updated: 2025-10-28 - Converted Dock Monitor Display Mode descriptions to hover tooltips for cleaner UI (Hassan)
 * Updated: 2025-10-28 - Updated Plant/Location section to 2x2 grid layout with hover tooltips (Hassan)
 * Updated: 2025-10-28 - Converted Time Thresholds to 2-column grid layout (only 2 thresholds: Behind and Critical),
 *               added blue info icons with hover tooltips to all three Dock Monitor section headers (Hassan)
 * Updated: 2025-10-28 - Merged three Dock Monitor cards into ONE card with horizontal dividers between sections (Hassan)
 * Updated: 2025-10-28 - Updated Time Thresholds input fields to use info icons with tooltips instead of visible description text (Hassan)
 * Updated: 2025-10-28 - Fixed blue info icons: replaced Font Awesome icons with simple text "i" for reliable visibility
 *               across all 5 locations (Time Thresholds section, Behind/Critical Threshold labels, Display Mode section,
 *               Plant/Location section) (Hassan)
 * Updated: 2025-10-28 - Reduced text sizes in Dock Monitor tab to eliminate scrolling: section headings from text-lg to text-base,
 *               radio option labels from text-base to text-sm, input labels from text-base to text-sm, card padding from p-6 to p-4,
 *               radio button card padding from p-4 to p-3, section spacing from mb-8/my-8 to mb-6/my-6, gap-4 to gap-3 (Hassan)
 * Updated: 2025-10-28 - Made delete buttons functional with confirmation dialogs for Office, Warehouse, and User tables (Hassan)
 * Updated: 2025-10-28 - Added Parts Maintenance tab between Warehouse and Users with table + slider panel pattern (Hassan)
 * Updated: 2025-10-28 - Changed Parts Maintenance tab icon from fa-cogs to fa-wrench for maintenance/tools theme (Hassan)
 * Updated: 2025-10-28 - Removed V2 fields from Parts Maintenance (packageQty, minOrderQty, maxOrderQty, properStockQty,
 *               unitSalesPrice, prevUnitSalesPrice, leadTimeDays, comment) - keeping only basic part information (Hassan)
 * Updated: 2025-10-29 - Added Internal Kanban tab with duplication rules settings (Hassan)
 * Updated: 2025-10-29 - Changed administration tab icons to keep duotone, changed action icons (edit/delete) to fa-solid (Hassan)
 * Updated: 2025-10-29 - Fixed Dock Monitor buttons layout: ALL THREE buttons (Save Settings, Reset Defaults, Back to Dashboard) now on same horizontal line on desktop, stack on mobile (Hassan)
 * Updated: 2025-10-30 - Added "Back to Dashboard" button to all administration tabs (Office, Warehouse, User, Internal Kanban)
 *               for consistent navigation across all tabs. Dock Monitor tab already had this button. (Hassan)
 * Updated: 2025-10-30 - FIXED button positioning: Moved "Back to Dashboard" button OUTSIDE the Card component for all tabs
 *               (Office, Warehouse, User, Internal Kanban) to match Dock Monitor's layout pattern. Buttons now use
 *               space-y-6 wrapper div for consistent spacing between card and buttons across all administration tabs. (Hassan)
 * Updated: 2025-11-25 - CONNECTED TO REAL APIs: Replaced dummy data with real API calls for Office, Warehouse, and User management.
 *               Added loading states (spinning loader), empty states (modern Font Awesome icons: fa-building, fa-warehouse, fa-users),
 *               error handling, and full CRUD operations (Create, Read, Update, Delete) using actual backend APIs. (Hassan)
 * Updated: 2025-11-25 - User Management UI Changes: Removed "User ID", "Notification Name", "Notification Email" columns from table.
 *               Added "IsActive" column after Menu Level (shows ✓ green for active, ✗ red for inactive). Modified API to fetch ALL users
 *               (active + inactive) for admin management. Reordered form fields: Username, Password, Email, Nickname, Supervisor checkbox,
 *               Menu Level, Operation, Code. Removed Notification Name/Email fields from Add/Edit forms. (Hassan)
 * Updated: 2025-11-25 - REMOVED NickName field completely from User Management (form state, Add form, Edit form, table column, API mapping).
 *               REORDERED table columns: Username, Name, Email, Supervisor, Menu Level, IsActive, Operation, Code, Actions. (Hassan)
 * Updated: 2025-11-25 - SEPARATED Username and Name fields: Added separate Username (login) and Name (display name) fields to user form.
 *               Username is REQUIRED for login, Name is OPTIONAL for display. Form order: Username, Name, Password, Email, Supervisor,
 *               Menu Level, Operation, Code. Updated CreateUserDto interface to make username required and name optional. (Hassan)
 * Updated: 2025-12-01 - CONNECTED SETTINGS TO REAL APIs: Integrated Internal Kanban and Dock Monitor settings with backend APIs.
 *               Added loading states, API calls for fetching/saving settings. Updated Internal Kanban UI to match API structure
 *               (allowDuplicates, duplicateWindowHours, alertOnDuplicate). (Hassan)
 * Updated: 2025-12-01 - FIXED DOCK MONITOR LOCATION SELECTION: Changed Plant/Location from checkboxes (multi-select) back to
 *               radio buttons (single select). Added selectedLocation state to track single selection. User can only select ONE
 *               location at a time (their current working office/location). This location will be displayed in app header and
 *               used to filter data. Backend API still receives selectedLocations as array with single item for compatibility. (Hassan)
 * Updated: 2025-12-01 - Integrated LocationContext: After saving Dock Monitor settings, calls refreshLocation() to update
 *               global location in Header immediately without page refresh. (Hassan)
 * Updated: 2025-12-14 - Added Toyota API Settings tab for managing Toyota API configurations (QA and PROD environments).
 *               Full CRUD operations: create, read, update, delete, and test connection functionality. (Hassan)
 *
 * Admin-only page with 7 tabs:
 * 1. Office Management - Manage office locations (CONNECTED TO API)
 * 2. Warehouse Management - Manage warehouse facilities (CONNECTED TO API)
 * 3. Internal Kanban - Configure kanban duplication rules (CONNECTED TO API)
 * 4. Parts Maintenance - Manage parts and inventory items (currently hidden)
 * 5. User Management - Manage user accounts and permissions (CONNECTED TO API)
 * 6. Dock Monitor Settings - Configure dock monitor thresholds and display (CONNECTED TO API)
 * 7. Toyota API Settings - Manage Toyota API configurations for QA and PROD (CONNECTED TO API)
 */

'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { useLocation } from '@/contexts/LocationContext';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Alert from '@/components/ui/Alert';
import { LOCATIONS } from '@/lib/constants';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import SlideOutPanel from '@/components/ui/SlideOutPanel';
import { getUsers, createUser as apiCreateUser, updateUser as apiUpdateUser, deleteUser as apiDeleteUser, User as ApiUser, CreateUserDto, UpdateUserDto } from '@/lib/api/users';
import { getOffices, createOffice as apiCreateOffice, updateOffice as apiUpdateOffice, deleteOffice as apiDeleteOffice, Office as ApiOffice, OfficeDto } from '@/lib/api/offices';
import { getWarehouses, createWarehouse as apiCreateWarehouse, updateWarehouse as apiUpdateWarehouse, deleteWarehouse as apiDeleteWarehouse, Warehouse as ApiWarehouse, WarehouseDto } from '@/lib/api/warehouses';
import { getInternalKanbanSettings, saveInternalKanbanSettings, getDockMonitorSettings, saveDockMonitorSettings, InternalKanbanSettings, DockMonitorSettings } from '@/lib/api/settings';
import { getAllToyotaConfigs, createToyotaConfig, updateToyotaConfig, deleteToyotaConfig, testToyotaConnection, ToyotaConfigResponse, ToyotaConfigCreate, ToyotaConfigUpdate, TOYOTA_CONFIG_DEFAULTS } from '@/lib/api/toyota-config';
import { fileLogger } from '@/lib/logger';

type Tab = 'office' | 'warehouse' | 'internal-kanban' | 'parts' | 'user' | 'dock-monitor' | 'toyota-api';
type DisplayMode = 'FULL' | 'SHIPMENT_ONLY' | 'SKID_ONLY' | 'COMPLETION_ONLY';

// Part interface
interface Part {
  id: string;
  partNo: string;
  description: string;
  unitOfMeasure: string;
  weightPerPiece: number;
  uomPerPiece: string;
  partType: 'Regular Parts' | 'Special Parts' | 'International Steel Parts';
  category: string;
  location?: string;
  packingStyle?: string;
  commonPart: boolean;
  discontinued: boolean;
}

// US States for dropdowns
const US_STATES = ['AL', 'AK', 'AZ', 'AR', 'CA', 'CO', 'CT', 'DE', 'FL', 'GA', 'HI', 'ID', 'IL', 'IN', 'IA', 'KS', 'KY', 'LA', 'ME', 'MD', 'MA', 'MI', 'MN', 'MS', 'MO', 'MT', 'NE', 'NV', 'NH', 'NJ', 'NM', 'NY', 'NC', 'ND', 'OH', 'OK', 'OR', 'PA', 'RI', 'SC', 'SD', 'TN', 'TX', 'UT', 'VT', 'VA', 'WA', 'WV', 'WI', 'WY'];

// Initial dummy data for Office tab
const INITIAL_OFFICES = [
  { id: 'off-001', code: 'IND', name: 'Indiana Office', address: '123 Main St', city: 'Indianapolis', state: 'IN', zip: '46225', phone: '317-555-0100', contact: 'John Doe', email: 'indiana@vuteq.com' },
  { id: 'off-002', code: 'MIC', name: 'Michigan Office', address: '456 Oak Ave', city: 'Detroit', state: 'MI', zip: '48226', phone: '313-555-0200', contact: 'Jane Smith', email: 'michigan@vuteq.com' },
  { id: 'off-003', code: 'OHI', name: 'Ohio Office', address: '789 Elm St', city: 'Columbus', state: 'OH', zip: '43215', phone: '614-555-0300', contact: 'Bob Johnson', email: 'ohio@vuteq.com' },
];

// Initial dummy data for Warehouse tab
const INITIAL_WAREHOUSES = [
  { id: 'wh-001', code: 'IND-WH1', name: 'Indiana Main Warehouse', address: '100 Industrial Blvd', city: 'Indianapolis', state: 'IN', zip: '46225', phone: '317-555-0150', contactName: 'Mike Johnson', contactEmail: 'mike.j@vuteq.com', office: 'IND' },
  { id: 'wh-002', code: 'MIC-WH1', name: 'Michigan Distribution Center', address: '200 Warehouse Rd', city: 'Detroit', state: 'MI', zip: '48226', phone: '313-555-0250', contactName: 'Sarah Lee', contactEmail: 'sarah.l@vuteq.com', office: 'MIC' },
  { id: 'wh-003', code: 'OHI-WH1', name: 'Ohio Storage Facility', address: '300 Logistics Way', city: 'Columbus', state: 'OH', zip: '43215', phone: '614-555-0350', contactName: 'Tom Davis', contactEmail: 'tom.d@vuteq.com', office: 'OHI' },
];

// Initial dummy data for User tab
const INITIAL_USERS = [
  { id: 'usr-001', userId: 'operator1', password: '********', userName: 'John Operator', email: 'john.operator@vuteq.com', notificationName: 'John Operator', notificationEmail: 'john.operator@vuteq.com', menuLevel: 'Scanner', operation: 'Warehouse', code: 'WH01' },
  { id: 'usr-002', userId: 'supervisor1', password: '********', userName: 'Jane Supervisor', email: 'jane.supervisor@vuteq.com', notificationName: 'Jane Supervisor', notificationEmail: 'jane.supervisor@vuteq.com', menuLevel: 'Scanner', operation: 'Warehouse', code: 'WH01' },
  { id: 'usr-003', userId: 'admin1', password: '********', userName: 'Bob Administrator', email: 'bob.admin@vuteq.com', notificationName: 'Bob Administrator', notificationEmail: 'bob.admin@vuteq.com', menuLevel: 'Admin', operation: 'Administration', code: 'ADM01' },
];

// Initial dummy data for Parts tab
const INITIAL_PARTS: Part[] = [
  { id: 'prt-001', partNo: 'RM-001', description: 'Steel Sheet 4x8', unitOfMeasure: 'LB', weightPerPiece: 120.5, uomPerPiece: 'LB', partType: 'Regular Parts', category: 'Metal', location: 'A-100', packingStyle: 'Bulk', commonPart: true, discontinued: false },
  { id: 'prt-002', partNo: 'FG-200', description: 'Door Panel Assembly', unitOfMeasure: 'EA', weightPerPiece: 15.2, uomPerPiece: 'LB', partType: 'Special Parts', category: 'Automotive', location: 'B-205', packingStyle: 'Boxed', commonPart: true, discontinued: false },
  { id: 'prt-003', partNo: 'CS-050', description: 'Welding Wire 0.035"', unitOfMeasure: 'LB', weightPerPiece: 25.0, uomPerPiece: 'LB', partType: 'International Steel Parts', category: 'Welding', location: 'C-010', packingStyle: 'Spool', commonPart: false, discontinued: false },
  { id: 'prt-004', partNo: 'FG-301', description: 'Trunk Lid Assembly', unitOfMeasure: 'EA', weightPerPiece: 22.8, uomPerPiece: 'LB', partType: 'Special Parts', category: 'Automotive', location: 'B-310', packingStyle: 'Crated', commonPart: true, discontinued: false },
  { id: 'prt-005', partNo: 'RM-045', description: 'Aluminum Bar 2" x 6"', unitOfMeasure: 'PC', weightPerPiece: 8.3, uomPerPiece: 'LB', partType: 'Regular Parts', category: 'Metal', location: 'A-150', packingStyle: 'Bundle', commonPart: false, discontinued: true },
];

export default function AdministrationPage() {
  const router = useRouter();
  const { user } = useAuth();
  const { refreshLocation } = useLocation();

  const [activeTab, setActiveTab] = useState<Tab>('office');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  // Loading states
  const [isLoadingOffices, setIsLoadingOffices] = useState(true);
  const [isLoadingWarehouses, setIsLoadingWarehouses] = useState(true);
  const [isLoadingUsers, setIsLoadingUsers] = useState(true);
  const [isLoadingKanbanSettings, setIsLoadingKanbanSettings] = useState(false);
  const [isLoadingDockSettings, setIsLoadingDockSettings] = useState(false);
  const [isLoadingToyotaConfigs, setIsLoadingToyotaConfigs] = useState(true);

  // State for data arrays
  const [offices, setOffices] = useState<any[]>([]);
  const [warehouses, setWarehouses] = useState<any[]>([]);
  const [parts, setParts] = useState<Part[]>(INITIAL_PARTS);
  const [users, setUsers] = useState<any[]>([]);
  const [toyotaConfigs, setToyotaConfigs] = useState<ToyotaConfigResponse[]>([]);

  // Slider panel states
  const [isAddOfficeOpen, setIsAddOfficeOpen] = useState(false);
  const [isEditOfficeOpen, setIsEditOfficeOpen] = useState(false);
  const [isAddWarehouseOpen, setIsAddWarehouseOpen] = useState(false);
  const [isEditWarehouseOpen, setIsEditWarehouseOpen] = useState(false);
  const [isAddPartOpen, setIsAddPartOpen] = useState(false);
  const [isEditPartOpen, setIsEditPartOpen] = useState(false);
  const [isAddUserOpen, setIsAddUserOpen] = useState(false);
  const [isEditUserOpen, setIsEditUserOpen] = useState(false);
  const [isAddToyotaConfigOpen, setIsAddToyotaConfigOpen] = useState(false);
  const [isEditToyotaConfigOpen, setIsEditToyotaConfigOpen] = useState(false);
  const [editingItem, setEditingItem] = useState<any>(null);

  // Form states for Office
  const [officeForm, setOfficeForm] = useState({
    code: '', name: '', address: '', city: '', state: '', zip: '', phone: '', contact: '', email: ''
  });

  // Form states for Warehouse
  const [warehouseForm, setWarehouseForm] = useState({
    code: '', name: '', address: '', city: '', state: '', zip: '', phone: '', contactName: '', contactEmail: '', office: ''
  });

  // Form states for Parts
  const [partForm, setPartForm] = useState<Omit<Part, 'id'>>({
    partNo: '',
    description: '',
    unitOfMeasure: '',
    weightPerPiece: 0,
    uomPerPiece: '',
    partType: 'Regular Parts',
    category: '',
    location: '',
    packingStyle: '',
    commonPart: false,
    discontinued: false
  });

  // Form states for User
  const [userForm, setUserForm] = useState({
    username: '', name: '', password: '', email: '', supervisor: false, menuLevel: 'Scanner', operation: '', code: ''
  });

  // Form states for Toyota Config
  const [toyotaConfigForm, setToyotaConfigForm] = useState({
    environment: 'QA',
    applicationName: '',
    clientId: '',
    clientSecret: '',
    tokenUrl: TOYOTA_CONFIG_DEFAULTS.QA.tokenUrl,
    apiBaseUrl: TOYOTA_CONFIG_DEFAULTS.QA.apiBaseUrl,
    resourceUrl: TOYOTA_CONFIG_DEFAULTS.QA.resourceUrl,
    xClientId: TOYOTA_CONFIG_DEFAULTS.QA.xClientId,
    isActive: true
  });
  const [isTestingConnection, setIsTestingConnection] = useState(false);

  // Internal Kanban Settings state
  const [kanbanSettings, setKanbanSettings] = useState<InternalKanbanSettings>({
    allowDuplicates: false,
    duplicateWindowHours: 24,
    alertOnDuplicate: true,
  });

  // Dock Monitor Settings state (from old settings page)
  const [dockSettings, setDockSettings] = useState<DockMonitorSettings>({
    behindThreshold: 15,
    criticalThreshold: 30,
    displayMode: 'FULL' as DisplayMode,
    selectedLocations: ['loc-001'], // Single location stored as array for API compatibility
  });
  // Track selected location as single value for UI (radio button)
  const [selectedLocation, setSelectedLocation] = useState<string>('loc-001');
  const [isSaving, setIsSaving] = useState(false);

  // Fetch data on component mount
  useEffect(() => {
    fetchOffices();
    fetchWarehouses();
    fetchUsers();
    fetchToyotaConfigs();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Fetch settings when Internal Kanban tab is activated
  useEffect(() => {
    if (activeTab === 'internal-kanban') {
      fetchInternalKanbanSettings();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]);

  // Fetch settings when Dock Monitor tab is activated
  useEffect(() => {
    if (activeTab === 'dock-monitor') {
      fetchDockMonitorSettings();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [activeTab]);

  // Redirect if not admin - moved after all hooks to comply with React Rules of Hooks
  if (user && user.role !== 'ADMIN') {
    router.push('/');
    return null;
  }

  // Fetch offices from API
  const fetchOffices = async () => {
    setIsLoadingOffices(true);
    setError(null);

    const result = await getOffices();

    if (result.success && result.data) {
      // Map API data to component format
      const mappedOffices = result.data.map((office: ApiOffice) => ({
        id: office.officeId,
        code: office.code,
        name: office.name,
        address: office.address,
        city: office.city,
        state: office.state,
        zip: office.zip,
        phone: office.phone || '',
        contact: office.contact || '',
        email: office.email || '',
        isActive: office.isActive,
      }));
      setOffices(mappedOffices);
    } else {
      setError(result.error || 'Failed to load offices');
    }

    setIsLoadingOffices(false);
  };

  // Fetch warehouses from API
  const fetchWarehouses = async () => {
    setIsLoadingWarehouses(true);
    setError(null);

    const result = await getWarehouses();

    if (result.success && result.data) {
      // Map API data to component format
      const mappedWarehouses = result.data.map((warehouse: ApiWarehouse) => ({
        id: warehouse.warehouseId,
        code: warehouse.code,
        name: warehouse.name,
        address: warehouse.address,
        city: warehouse.city,
        state: warehouse.state,
        zip: warehouse.zip,
        phone: warehouse.phone || '',
        contactName: warehouse.contactName || '',
        contactEmail: warehouse.contactEmail || '',
        office: warehouse.office,
        isActive: warehouse.isActive,
      }));
      setWarehouses(mappedWarehouses);
    } else {
      setError(result.error || 'Failed to load warehouses');
    }

    setIsLoadingWarehouses(false);
  };

  // Fetch users from API
  const fetchUsers = async () => {
    setIsLoadingUsers(true);
    setError(null);

    const result = await getUsers();

    if (result.success && result.data) {
      // Map API data to component format (including inactive users for admin management)
      const mappedUsers = result.data.map((user: ApiUser) => ({
        id: user.userId,
        password: '********', // Don't show actual password
        userName: user.name,
        username: user.username,
        email: user.email || '',
        supervisor: user.isSupervisor,
        menuLevel: user.menuLevel || 'Scanner',
        operation: user.operation || '',
        code: user.code || '',
        isActive: user.isActive,
      }));
      setUsers(mappedUsers);
    } else {
      setError(result.error || 'Failed to load users');
    }

    setIsLoadingUsers(false);
  };

  // Fetch Toyota Configs from API
  const fetchToyotaConfigs = async () => {
    setIsLoadingToyotaConfigs(true);
    setError(null);

    const result = await getAllToyotaConfigs();

    if (result.success && result.data) {
      setToyotaConfigs(result.data);
    } else {
      setError(result.error || 'Failed to load Toyota API configurations');
    }

    setIsLoadingToyotaConfigs(false);
  };

  // Fetch Internal Kanban settings from API
  const fetchInternalKanbanSettings = async () => {
    setIsLoadingKanbanSettings(true);
    setError(null);

    const result = await getInternalKanbanSettings();

    if (result.success && result.data) {
      setKanbanSettings(result.data);
    } else {
      setError(result.error || 'Failed to load Internal Kanban settings');
    }

    setIsLoadingKanbanSettings(false);
  };

  // Fetch Dock Monitor settings from API
  const fetchDockMonitorSettings = async () => {
    setIsLoadingDockSettings(true);
    setError(null);

    const result = await getDockMonitorSettings();

    if (result.success && result.data) {
      setDockSettings(result.data);
      // Sync selectedLocation with first item in array (single selection)
      if (result.data.selectedLocations && result.data.selectedLocations.length > 0) {
        setSelectedLocation(result.data.selectedLocations[0]);
      }
    } else {
      setError(result.error || 'Failed to load Dock Monitor settings');
    }

    setIsLoadingDockSettings(false);
  };

  const handleSaveKanbanSettings = async () => {
    setError(null);
    setSuccess(null);
    setIsSaving(true);

    const result = await saveInternalKanbanSettings(kanbanSettings);

    if (result.success) {
      setSuccess('Internal Kanban settings saved successfully!');
      setTimeout(() => setSuccess(null), 3000);
    } else {
      setError(result.error || 'Failed to save Internal Kanban settings');
    }

    setIsSaving(false);
  };

  const handleSaveDockSettings = async () => {
    setError(null);
    setSuccess(null);

    // Validation
    if (dockSettings.behindThreshold <= 0) {
      setError('Behind threshold must be greater than 0');
      return;
    }

    if (dockSettings.criticalThreshold <= dockSettings.behindThreshold) {
      setError('Critical threshold must be greater than behind threshold');
      return;
    }

    if (!selectedLocation) {
      setError('Please select a location');
      return;
    }

    setIsSaving(true);

    // Send selectedLocation as single-item array to match API expectations
    const settingsToSave = {
      ...dockSettings,
      selectedLocations: [selectedLocation],
    };

    const result = await saveDockMonitorSettings(settingsToSave);

    if (result.success) {
      setSuccess('Dock monitor settings saved successfully!');
      // Refresh global location in header
      await refreshLocation();
      setTimeout(() => setSuccess(null), 3000);
    } else {
      setError(result.error || 'Failed to save Dock Monitor settings');
    }

    setIsSaving(false);
  };

  const handleResetDockSettings = () => {
    setDockSettings({
      behindThreshold: 15,
      criticalThreshold: 30,
      displayMode: 'FULL',
      selectedLocations: ['loc-001'],
    });
    setSelectedLocation('loc-001');
    setError(null);
    setSuccess(null);
  };

  // CRUD handlers (Phase 1 - console.log only)
  const handleAddOffice = () => {
    setOfficeForm({ code: '', name: '', address: '', city: '', state: '', zip: '', phone: '', contact: '', email: '' });
    setIsAddOfficeOpen(true);
  };

  const handleEditOffice = (officeId: string) => {
    const office = offices.find(o => o.id === officeId);
    if (office) {
      setOfficeForm(office);
      setEditingItem(office);
      setIsEditOfficeOpen(true);
    }
  };

  const handleDeleteOffice = async (officeId: string) => {
    const office = offices.find(o => o.id === officeId);
    if (!office) return;

    const confirmMessage = `Are you sure you want to delete office "${office.name}" (${office.code})?`;
    if (window.confirm(confirmMessage)) {
      setError(null);
      setSuccess(null);

      const result = await apiDeleteOffice(officeId);

      if (result.success) {
        setSuccess('Office deleted successfully!');
        // Refresh data
        await fetchOffices();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to delete office');
      }
    }
  };

  const handleAddWarehouse = () => {
    setWarehouseForm({ code: '', name: '', address: '', city: '', state: '', zip: '', phone: '', contactName: '', contactEmail: '', office: '' });
    setIsAddWarehouseOpen(true);
  };

  const handleEditWarehouse = (warehouseId: string) => {
    const warehouse = warehouses.find(w => w.id === warehouseId);
    if (warehouse) {
      setWarehouseForm(warehouse);
      setEditingItem(warehouse);
      setIsEditWarehouseOpen(true);
    }
  };

  const handleDeleteWarehouse = async (warehouseId: string) => {
    const warehouse = warehouses.find(w => w.id === warehouseId);
    if (!warehouse) return;

    const confirmMessage = `Are you sure you want to delete warehouse "${warehouse.name}" (${warehouse.code})?`;
    if (window.confirm(confirmMessage)) {
      setError(null);
      setSuccess(null);

      const result = await apiDeleteWarehouse(warehouseId);

      if (result.success) {
        setSuccess('Warehouse deleted successfully!');
        // Refresh data
        await fetchWarehouses();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to delete warehouse');
      }
    }
  };

  const handleAddUser = () => {
    setUserForm({ username: '', name: '', password: '', email: '', supervisor: false, menuLevel: 'Scanner', operation: '', code: '' });
    setIsAddUserOpen(true);
  };

  const handleEditUser = (userId: string) => {
    const userToEdit = users.find(u => u.id === userId);
    if (userToEdit) {
      setUserForm({
        username: userToEdit.username || '',
        name: userToEdit.userName || '',
        password: userToEdit.password || '********',
        email: userToEdit.email || '',
        supervisor: userToEdit.supervisor || false,
        menuLevel: userToEdit.menuLevel || 'Scanner',
        operation: userToEdit.operation || '',
        code: userToEdit.code || ''
      });
      setEditingItem(userToEdit);
      setIsEditUserOpen(true);
    }
  };

  const handleDeleteUser = async (userId: string) => {
    const userToDelete = users.find(u => u.id === userId);
    if (!userToDelete) return;

    const confirmMessage = `Are you sure you want to delete user "${userToDelete.userName}" (${userToDelete.userId})?`;
    if (window.confirm(confirmMessage)) {
      setError(null);
      setSuccess(null);

      const result = await apiDeleteUser(userId);

      if (result.success) {
        setSuccess('User deleted successfully!');
        // Refresh data
        await fetchUsers();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to delete user');
      }
    }
  };

  // Parts CRUD handlers
  const handleAddPart = () => {
    setPartForm({
      partNo: '',
      description: '',
      unitOfMeasure: '',
      weightPerPiece: 0,
      uomPerPiece: '',
      partType: 'Regular Parts',
      category: '',
      location: '',
      packingStyle: '',
      commonPart: false,
      discontinued: false
    });
    setIsAddPartOpen(true);
  };

  const handleEditPart = (partId: string) => {
    const part = parts.find(p => p.id === partId);
    if (part) {
      setPartForm(part);
      setEditingItem(part);
      setIsEditPartOpen(true);
    }
  };

  const handleDeletePart = (partId: string) => {
    const part = parts.find(p => p.id === partId);
    if (!part) return;

    const confirmMessage = `Are you sure you want to delete part "${part.description}" (${part.partNo})?`;
    if (window.confirm(confirmMessage)) {
      setParts(parts.filter(p => p.id !== partId));
      setSuccess('Part deleted successfully!');
      setTimeout(() => setSuccess(null), 3000);
    }
  };

  // Save handlers
  const handleSaveOffice = async () => {
    setError(null);
    setSuccess(null);

    // Validate required fields
    if (!officeForm.code || !officeForm.name || !officeForm.address || !officeForm.city || !officeForm.state || !officeForm.zip) {
      setError('Please fill in all required fields');
      return;
    }

    try {
      // Build office data object with only required fields
      const officeData: any = {
        code: officeForm.code,
        name: officeForm.name,
        address: officeForm.address,
        city: officeForm.city,
        state: officeForm.state,
        zip: officeForm.zip,
      };

      // Only add optional fields if they have values (not empty strings)
      if (officeForm.email?.trim()) {
        officeData.email = officeForm.email.trim();
      }
      if (officeForm.phone?.trim()) {
        officeData.phone = officeForm.phone.trim();
      }
      if (officeForm.contact?.trim()) {
        officeData.contact = officeForm.contact.trim();
      }

      let result;
      if (isEditOfficeOpen && editingItem) {
        // Update existing office
        result = await apiUpdateOffice(editingItem.id, officeData as OfficeDto);
      } else {
        // Create new office
        result = await apiCreateOffice(officeData as OfficeDto);
      }

      if (result.success) {
        setSuccess(isEditOfficeOpen ? 'Office updated successfully!' : 'Office created successfully!');
        setIsAddOfficeOpen(false);
        setIsEditOfficeOpen(false);
        setEditingItem(null);
        // Refresh data
        await fetchOffices();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to save office');
      }
    } catch (err) {
      setError('An unexpected error occurred');
    }
  };

  const handleSaveWarehouse = async () => {
    setError(null);
    setSuccess(null);

    // Validate required fields
    if (!warehouseForm.code || !warehouseForm.name || !warehouseForm.address || !warehouseForm.city || !warehouseForm.state || !warehouseForm.zip || !warehouseForm.office) {
      setError('Please fill in all required fields');
      return;
    }

    try {
      // Build warehouse data object with only required fields
      const warehouseData: any = {
        code: warehouseForm.code,
        name: warehouseForm.name,
        address: warehouseForm.address,
        city: warehouseForm.city,
        state: warehouseForm.state,
        zip: warehouseForm.zip,
        office: warehouseForm.office,
      };

      // Only add optional fields if they have values (not empty strings)
      if (warehouseForm.contactName?.trim()) {
        warehouseData.contactName = warehouseForm.contactName.trim();
      }
      if (warehouseForm.contactEmail?.trim()) {
        warehouseData.contactEmail = warehouseForm.contactEmail.trim();
      }
      if (warehouseForm.phone?.trim()) {
        warehouseData.phone = warehouseForm.phone.trim();
      }

      let result;
      if (isEditWarehouseOpen && editingItem) {
        // Update existing warehouse
        result = await apiUpdateWarehouse(editingItem.id, warehouseData as WarehouseDto);
      } else {
        // Create new warehouse
        result = await apiCreateWarehouse(warehouseData as WarehouseDto);
      }

      if (result.success) {
        setSuccess(isEditWarehouseOpen ? 'Warehouse updated successfully!' : 'Warehouse created successfully!');
        setIsAddWarehouseOpen(false);
        setIsEditWarehouseOpen(false);
        setEditingItem(null);
        // Refresh data
        await fetchWarehouses();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to save warehouse');
      }
    } catch (err) {
      setError('An unexpected error occurred');
    }
  };

  const handleSaveUser = async () => {
    setError(null);
    setSuccess(null);

    // Validate required fields
    if (!userForm.username || !userForm.menuLevel) {
      setError('Please fill in all required fields (Username and Menu Level)');
      return;
    }

    // For new users, password is required
    if (!isEditUserOpen && !userForm.password) {
      setError('Password is required for new users');
      return;
    }

    try {
      let result;
      if (isEditUserOpen && editingItem) {
        // Update existing user
        const updateData: UpdateUserDto = {
          username: userForm.username,
          name: userForm.name,
          email: userForm.email,
          role: userForm.menuLevel,
          menuLevel: userForm.menuLevel,
          operation: userForm.operation,
          code: userForm.code,
          isSupervisor: userForm.supervisor,
        };

        // Only include password if it was changed (not the placeholder)
        if (userForm.password && userForm.password !== '********') {
          updateData.password = userForm.password;
        }

        result = await apiUpdateUser(editingItem.id, updateData);
      } else {
        // Create new user
        const createData: CreateUserDto = {
          username: userForm.username, // Required - login username
          password: userForm.password, // Required - password
        };

        // Add name field if provided (optional, defaults to username on backend)
        if (userForm.name) createData.name = userForm.name;

        // Only add optional fields if they have values (don't send empty strings)
        if (userForm.email) createData.email = userForm.email;
        if (userForm.code) createData.code = userForm.code;
        if (userForm.operation) createData.operation = userForm.operation;

        // Add boolean/enum fields with defaults
        createData.supervisor = userForm.supervisor;
        createData.menuLevel = userForm.menuLevel;

        result = await apiCreateUser(createData);
      }

      if (result.success) {
        setSuccess(isEditUserOpen ? 'User updated successfully!' : 'User created successfully!');
        setIsAddUserOpen(false);
        setIsEditUserOpen(false);
        setEditingItem(null);
        // Refresh data
        await fetchUsers();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to save user');
      }
    } catch (err) {
      // Log the exception
      fileLogger.error('UserCreation', 'Exception occurred during user save', {
        error: err instanceof Error ? err.message : String(err),
        stack: err instanceof Error ? err.stack : undefined
      });
      setError('An unexpected error occurred');
    }
  };

  const handleSavePart = () => {
    console.log('Save part:', partForm);
    setIsAddPartOpen(false);
    setIsEditPartOpen(false);
    setSuccess('Part saved successfully!');
    setTimeout(() => setSuccess(null), 3000);
  };

  // Toyota Config CRUD handlers
  const handleAddToyotaConfig = () => {
    setToyotaConfigForm({
      environment: 'QA',
      applicationName: '',
      clientId: '',
      clientSecret: '',
      tokenUrl: TOYOTA_CONFIG_DEFAULTS.QA.tokenUrl,
      apiBaseUrl: TOYOTA_CONFIG_DEFAULTS.QA.apiBaseUrl,
      resourceUrl: TOYOTA_CONFIG_DEFAULTS.QA.resourceUrl,
      xClientId: TOYOTA_CONFIG_DEFAULTS.QA.xClientId,
      isActive: true
    });
    setIsAddToyotaConfigOpen(true);
  };

  const handleEditToyotaConfig = (configId: string) => {
    const config = toyotaConfigs.find(c => c.configId === configId);
    if (config) {
      setToyotaConfigForm({
        environment: config.environment,
        applicationName: config.applicationName || '',
        clientId: config.clientId,
        clientSecret: '', // Don't populate secret (it's masked)
        tokenUrl: config.tokenUrl,
        apiBaseUrl: config.apiBaseUrl,
        resourceUrl: config.resourceUrl,
        xClientId: config.xClientId,
        isActive: config.isActive
      });
      setEditingItem(config);
      setIsEditToyotaConfigOpen(true);
    }
  };

  const handleDeleteToyotaConfig = async (configId: string) => {
    const config = toyotaConfigs.find(c => c.configId === configId);
    if (!config) return;

    const confirmMessage = `Are you sure you want to delete Toyota API configuration for ${config.environment}${config.applicationName ? ' (' + config.applicationName + ')' : ''}?`;
    if (window.confirm(confirmMessage)) {
      setError(null);
      setSuccess(null);

      const result = await deleteToyotaConfig(configId);

      if (result.success) {
        setSuccess('Toyota API configuration deleted successfully!');
        await fetchToyotaConfigs();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to delete Toyota API configuration');
      }
    }
  };

  const handleSaveToyotaConfig = async () => {
    setError(null);
    setSuccess(null);

    // Validate required fields
    if (!toyotaConfigForm.environment || !toyotaConfigForm.clientId || !toyotaConfigForm.tokenUrl || !toyotaConfigForm.apiBaseUrl || !toyotaConfigForm.resourceUrl || !toyotaConfigForm.xClientId) {
      setError('Please fill in all required fields (Environment, Client ID, Token URL, API Base URL, Resource URL, X-Client-ID)');
      return;
    }

    // For new configs, client secret is required
    if (!isEditToyotaConfigOpen && !toyotaConfigForm.clientSecret) {
      setError('Client Secret is required for new configurations');
      return;
    }

    try {
      let result;
      if (isEditToyotaConfigOpen && editingItem) {
        // Update existing config
        const updateData: ToyotaConfigUpdate = {
          environment: toyotaConfigForm.environment,
          applicationName: toyotaConfigForm.applicationName || undefined,
          clientId: toyotaConfigForm.clientId,
          tokenUrl: toyotaConfigForm.tokenUrl,
          apiBaseUrl: toyotaConfigForm.apiBaseUrl,
          resourceUrl: toyotaConfigForm.resourceUrl,
          xClientId: toyotaConfigForm.xClientId,
          isActive: toyotaConfigForm.isActive,
        };

        // Only include client secret if it was changed
        if (toyotaConfigForm.clientSecret) {
          updateData.clientSecret = toyotaConfigForm.clientSecret;
        }

        result = await updateToyotaConfig(editingItem.configId, updateData);
      } else {
        // Create new config
        const createData: ToyotaConfigCreate = {
          environment: toyotaConfigForm.environment,
          applicationName: toyotaConfigForm.applicationName || undefined,
          clientId: toyotaConfigForm.clientId,
          clientSecret: toyotaConfigForm.clientSecret,
          tokenUrl: toyotaConfigForm.tokenUrl,
          apiBaseUrl: toyotaConfigForm.apiBaseUrl,
          resourceUrl: toyotaConfigForm.resourceUrl,
          xClientId: toyotaConfigForm.xClientId,
          isActive: toyotaConfigForm.isActive,
        };

        result = await createToyotaConfig(createData);
      }

      if (result.success) {
        setSuccess(isEditToyotaConfigOpen ? 'Toyota API configuration updated successfully!' : 'Toyota API configuration created successfully!');
        setIsAddToyotaConfigOpen(false);
        setIsEditToyotaConfigOpen(false);
        setEditingItem(null);
        await fetchToyotaConfigs();
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.error || 'Failed to save Toyota API configuration');
      }
    } catch (err) {
      fileLogger.error('ToyotaConfigSave', 'Exception occurred during Toyota config save', {
        error: err instanceof Error ? err.message : String(err),
        stack: err instanceof Error ? err.stack : undefined
      });
      setError('An unexpected error occurred');
    }
  };

  const handleTestConnection = async (configId: string) => {
    setIsTestingConnection(true);
    setError(null);
    setSuccess(null);

    const result = await testToyotaConnection(configId);

    if (result.success && result.data) {
      const testResult = result.data;
      if (testResult.success) {
        setSuccess(`Connection successful! Token preview: ${testResult.tokenPreview || 'N/A'}. Expires in: ${testResult.expiresIn || 'N/A'} seconds.`);
        setTimeout(() => setSuccess(null), 5000);
      } else {
        setError(testResult.message || 'Connection test failed');
      }
    } else {
      setError(result.error || 'Failed to test connection');
    }

    setIsTestingConnection(false);
  };

  // Handle environment change to auto-fill URLs
  const handleToyotaEnvChange = (env: string) => {
    const defaults = env === 'QA' ? TOYOTA_CONFIG_DEFAULTS.QA : TOYOTA_CONFIG_DEFAULTS.PROD;
    setToyotaConfigForm(prev => ({
      ...prev,
      environment: env,
      tokenUrl: defaults.tokenUrl,
      apiBaseUrl: defaults.apiBaseUrl,
      resourceUrl: defaults.resourceUrl,
      xClientId: defaults.xClientId,
    }));
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-8 pt-24 w-full space-y-6">
        {/* Success Alert */}
        {success && (
          <Alert variant="success" onClose={() => setSuccess(null)}>
            {success}
          </Alert>
        )}

        {/* Error Alert - Only show on main page when no popups are open */}
        {error && !isAddUserOpen && !isEditUserOpen && (
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
                <i className="fa-light fa-building" style={{ fontSize: '20px', color: '#253262' }}></i>
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
                <i className="fa-light fa-warehouse" style={{ fontSize: '20px', color: '#253262' }}></i>
                Warehouse
              </button>
              <button
                onClick={() => setActiveTab('internal-kanban')}
                className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                  activeTab === 'internal-kanban'
                    ? 'border-[#253262] text-[#253262]'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <i className="fa-light fa-square-kanban" style={{ fontSize: '20px', color: '#253262' }}></i>
                Internal Kanban
              </button>
              <button
                onClick={() => setActiveTab('dock-monitor')}
                className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                  activeTab === 'dock-monitor'
                    ? 'border-[#253262] text-[#253262]'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <i className="fa-light fa-desktop" style={{ fontSize: '20px', color: '#253262' }}></i>
                Dock Monitor
              </button>
              <button
                onClick={() => setActiveTab('toyota-api')}
                className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                  activeTab === 'toyota-api'
                    ? 'border-[#253262] text-[#253262]'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <i className="fa-light fa-cloud-arrow-up" style={{ fontSize: '20px', color: '#253262' }}></i>
                Toyota API
              </button>
              {/* Parts Maintenance tab hidden - will be re-enabled when feature is complete */}
              {/* <button
                onClick={() => setActiveTab('parts')}
                className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                  activeTab === 'parts'
                    ? 'border-[#253262] text-[#253262]'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <i className="fa-duotone fa-regular fa-screwdriver-wrench" style={{ fontSize: '20px', '--fa-primary-color': '#D2312E', '--fa-secondary-color': '#253262', '--fa-primary-opacity': 1, '--fa-secondary-opacity': 1 } as React.CSSProperties}></i>
                Parts Maintenance
              </button> */}
              <button
                onClick={() => setActiveTab('user')}
                className={`flex items-center gap-2 px-6 py-3 text-base font-medium whitespace-nowrap border-b-2 transition-colors ${
                  activeTab === 'user'
                    ? 'border-[#253262] text-[#253262]'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                }`}
              >
                <i className="fa-light fa-users" style={{ fontSize: '20px', color: '#253262' }}></i>
                User
              </button>
            </nav>
          </div>
        </Card>

        {/* Tab Content */}
        {activeTab === 'office' && (
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <div className="flex justify-between items-center">
                  <CardTitle>Office Management</CardTitle>
                  <Button onClick={handleAddOffice} size="md" variant="primary">
                    <i className="fa-light fa-plus mr-2"></i>
                    Add Office
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                {isLoadingOffices ? (
                  <div className="flex justify-center items-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#253262]"></div>
                  </div>
                ) : offices.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-12 text-gray-500">
                    <i className="fa fa-building text-6xl text-gray-300 mb-4"></i>
                    <p className="text-lg">No offices found</p>
                  </div>
                ) : (
                  <div className="w-full">
                    <table className="w-full table-fixed divide-y divide-gray-200">
                      <colgroup>
                        <col style={{width: '80px'}} />
                        <col style={{width: '180px'}} />
                        <col style={{width: '200px'}} />
                        <col style={{width: '120px'}} />
                        <col style={{width: '60px'}} />
                        <col style={{width: '100px'}} />
                        <col style={{width: '140px'}} />
                        <col style={{width: '140px'}} />
                        <col style={{width: '200px'}} />
                        <col style={{width: '100px'}} />
                        <col style={{width: '120px'}} />
                      </colgroup>
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Code</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Name</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Address</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">City</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">State</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Zip</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Phone</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Contact Person</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Email</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">IsActive</th>
                          <th className="sticky right-0 bg-gray-50 px-6 py-4 text-right text-sm font-medium text-gray-500 uppercase tracking-wider shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white divide-y divide-gray-200">
                        {offices.map((office) => (
                          <tr key={office.id} className="hover:bg-gray-50">
                            <td className="px-6 py-4 text-base font-medium text-gray-900">{office.code}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={office.name}>{office.name}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={office.address}>{office.address}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{office.city}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{office.state}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{office.zip}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{office.phone}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={office.contact}>{office.contact}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={office.email}>{office.email}</td>
                            <td className="px-6 py-4 text-base text-center">
                              {office.isActive ? (
                                <i className="fa fa-check text-green-600 text-lg"></i>
                              ) : (
                                <i className="fa fa-xmark text-red-600 text-lg"></i>
                              )}
                            </td>
                            <td className="sticky right-0 bg-white px-6 py-4 text-right text-base shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">
                              <button onClick={() => handleEditOffice(office.id)} className="text-primary-600 hover:text-primary-900 mr-3">
                                <i className="fa-light fa-edit text-blue-600" style={{ fontSize: '20px' }}></i>
                              </button>
                              <button onClick={() => handleDeleteOffice(office.id)} className="text-error-600 hover:text-error-900">
                                <i className="fa-light fa-trash text-red-600" style={{ fontSize: '20px' }}></i>
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Office Action Buttons - OUTSIDE the card */}
            <div className="flex justify-end">
              <Button
                onClick={() => router.push('/')}
                variant="primary"
                className="w-full sm:w-auto"
              >
                <i className="fa-light fa-home mr-2"></i>
                Back to Dashboard
              </Button>
            </div>
          </div>
        )}

        {activeTab === 'warehouse' && (
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <div className="flex justify-between items-center">
                  <CardTitle>Warehouse Management</CardTitle>
                  <Button onClick={handleAddWarehouse} size="md" variant="primary">
                    <i className="fa-light fa-plus mr-2"></i>
                    Add Warehouse
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                {isLoadingWarehouses ? (
                  <div className="flex justify-center items-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#253262]"></div>
                  </div>
                ) : warehouses.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-12 text-gray-500">
                    <i className="fa fa-warehouse text-6xl text-gray-300 mb-4"></i>
                    <p className="text-lg">No warehouses found</p>
                  </div>
                ) : (
                  <div className="w-full">
                    <table className="w-full table-fixed divide-y divide-gray-200">
                      <colgroup>
                        <col style={{width: '100px'}} />
                        <col style={{width: '200px'}} />
                        <col style={{width: '180px'}} />
                        <col style={{width: '120px'}} />
                        <col style={{width: '60px'}} />
                        <col style={{width: '100px'}} />
                        <col style={{width: '140px'}} />
                        <col style={{width: '140px'}} />
                        <col style={{width: '180px'}} />
                        <col style={{width: '80px'}} />
                        <col style={{width: '100px'}} />
                        <col style={{width: '120px'}} />
                      </colgroup>
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Code</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Name</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Address</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">City</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">State</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Zip</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Phone</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Contact Name</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Contact Email</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Office</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">IsActive</th>
                          <th className="sticky right-0 bg-gray-50 px-6 py-4 text-right text-sm font-medium text-gray-500 uppercase tracking-wider shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white divide-y divide-gray-200">
                        {warehouses.map((warehouse) => (
                          <tr key={warehouse.id} className="hover:bg-gray-50">
                            <td className="px-6 py-4 text-base font-medium text-gray-900">{warehouse.code}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={warehouse.name}>{warehouse.name}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={warehouse.address}>{warehouse.address}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{warehouse.city}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{warehouse.state}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{warehouse.zip}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{warehouse.phone}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={warehouse.contactName}>{warehouse.contactName}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={warehouse.contactEmail}>{warehouse.contactEmail}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{warehouse.office}</td>
                            <td className="px-6 py-4 text-base text-center">
                              {warehouse.isActive ? (
                                <i className="fa fa-check text-green-600 text-lg"></i>
                              ) : (
                                <i className="fa fa-xmark text-red-600 text-lg"></i>
                              )}
                            </td>
                            <td className="sticky right-0 bg-white px-6 py-4 text-right text-base shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">
                              <button onClick={() => handleEditWarehouse(warehouse.id)} className="text-primary-600 hover:text-primary-900 mr-3">
                                <i className="fa-light fa-edit text-blue-600" style={{ fontSize: '20px' }}></i>
                              </button>
                              <button onClick={() => handleDeleteWarehouse(warehouse.id)} className="text-error-600 hover:text-error-900">
                                <i className="fa-light fa-trash text-red-600" style={{ fontSize: '20px' }}></i>
                              </button>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Warehouse Action Buttons - OUTSIDE the card */}
            <div className="flex justify-end">
              <Button
                onClick={() => router.push('/')}
                variant="primary"
                className="w-full sm:w-auto"
              >
                <i className="fa-light fa-home mr-2"></i>
                Back to Dashboard
              </Button>
            </div>
          </div>
        )}

        {activeTab === 'user' && (
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <div className="flex justify-between items-center">
                  <CardTitle>User Management</CardTitle>
                  <Button onClick={handleAddUser} size="md" variant="primary">
                    <i className="fa-light fa-plus mr-2"></i>
                    Add User
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                {isLoadingUsers ? (
                  <div className="flex justify-center items-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#253262]"></div>
                  </div>
                ) : users.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-12 text-gray-500">
                    <i className="fa fa-users text-6xl text-gray-300 mb-4"></i>
                    <p className="text-lg">No users found</p>
                  </div>
                ) : (
                  <div className="w-full">
                    <table className="w-full table-fixed divide-y divide-gray-200">
                      <colgroup>
                        <col style={{width: '120px'}} />
                        <col style={{width: '160px'}} />
                        <col style={{width: '180px'}} />
                        <col style={{width: '140px'}} />
                        <col style={{width: '160px'}} />
                        <col style={{width: '100px'}} />
                        <col style={{width: '180px'}} />
                        <col style={{width: '200px'}} />
                        <col style={{width: '120px'}} />
                      </colgroup>
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Username</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Name</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Email</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Supervisor</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Menu Level</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">IsActive</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Operation</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Code</th>
                          <th className="sticky right-0 bg-gray-50 px-6 py-4 text-right text-sm font-medium text-gray-500 uppercase tracking-wider shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white divide-y divide-gray-200">
                        {users.map((userRow) => (
                          <tr key={userRow.id} className="hover:bg-gray-50">
                            <td className="px-6 py-4 text-base text-gray-900">{userRow.username}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={userRow.userName}>{userRow.userName}</td>
                            <td className="px-6 py-4 text-base text-gray-900 truncate" title={userRow.email}>{userRow.email}</td>
                            <td className="px-6 py-4 text-base text-center">
                              {userRow.supervisor ? (
                                <i className="fa fa-check text-green-600 text-lg"></i>
                              ) : (
                                <i className="fa fa-xmark text-red-600 text-lg"></i>
                              )}
                            </td>
                            <td className="px-6 py-4 text-base">
                              <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                                userRow.menuLevel === 'Admin' ? 'bg-error-100 text-error-800' :
                                userRow.menuLevel === 'Scanner' ? 'bg-primary-100 text-primary-800' :
                                'bg-warning-100 text-warning-800'
                              }`}>
                                {userRow.menuLevel}
                              </span>
                            </td>
                            <td className="px-6 py-4 text-base text-center">
                              {userRow.isActive ? (
                                <i className="fa fa-check text-green-600 text-lg"></i>
                              ) : (
                                <i className="fa fa-xmark text-red-600 text-lg"></i>
                              )}
                            </td>
                            <td className="px-6 py-4 text-base text-gray-900">{userRow.operation}</td>
                            <td className="px-6 py-4 text-base text-gray-900">{userRow.code || '-'}</td>
                            <td className="sticky right-0 bg-white px-6 py-4 text-right text-base shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">
                              <div className="flex items-center justify-end gap-3">
                                <button onClick={() => handleEditUser(userRow.id)} className="text-primary-600 hover:text-primary-900">
                                  <i className="fa-light fa-edit text-blue-600" style={{ fontSize: '20px' }}></i>
                                </button>
                                <button onClick={() => handleDeleteUser(userRow.id)} className="text-error-600 hover:text-error-900">
                                  <i className="fa-light fa-trash text-red-600" style={{ fontSize: '20px' }}></i>
                                </button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* User Action Buttons - OUTSIDE the card */}
            <div className="flex justify-end">
              <Button
                onClick={() => router.push('/')}
                variant="primary"
                className="w-full sm:w-auto"
              >
                <i className="fa-light fa-home mr-2"></i>
                Back to Dashboard
              </Button>
            </div>
          </div>
        )}

        {activeTab === 'toyota-api' && (
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <div className="flex justify-between items-center">
                  <CardTitle>Toyota API Configuration</CardTitle>
                  <Button onClick={handleAddToyotaConfig} size="md" variant="primary">
                    <i className="fa-light fa-plus mr-2"></i>
                    Add Configuration
                  </Button>
                </div>
              </CardHeader>
              <CardContent>
                {isLoadingToyotaConfigs ? (
                  <div className="flex justify-center items-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#253262]"></div>
                  </div>
                ) : toyotaConfigs.length === 0 ? (
                  <div className="flex flex-col items-center justify-center py-12 text-gray-500">
                    <i className="fa fa-cloud-arrow-up text-6xl text-gray-300 mb-4"></i>
                    <p className="text-lg">No Toyota API configurations found</p>
                    <p className="text-sm mt-2">Add a configuration to connect to Toyota API</p>
                  </div>
                ) : (
                  <div className="w-full overflow-x-auto">
                    <table className="w-full divide-y divide-gray-200">
                      <thead className="bg-gray-50">
                        <tr>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Environment</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Application</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Client ID</th>
                          <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">API Base URL</th>
                          <th className="px-6 py-4 text-center text-sm font-medium text-gray-500 uppercase tracking-wider">Status</th>
                          <th className="px-6 py-4 text-right text-sm font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                        </tr>
                      </thead>
                      <tbody className="bg-white divide-y divide-gray-200">
                        {toyotaConfigs.map((config) => (
                          <tr key={config.configId} className="hover:bg-gray-50">
                            <td className="px-6 py-4 text-base">
                              <span className={`px-3 py-1 text-sm font-semibold rounded-full ${
                                config.environment === 'QA'
                                  ? 'bg-blue-100 text-blue-800'
                                  : 'bg-green-100 text-green-800'
                              }`}>
                                {config.environment}
                              </span>
                            </td>
                            <td className="px-6 py-4 text-base text-gray-900">{config.applicationName || '-'}</td>
                            <td className="px-6 py-4 text-base text-gray-900 font-mono text-sm">{config.clientId}</td>
                            <td className="px-6 py-4 text-base text-gray-600 text-sm truncate max-w-xs" title={config.apiBaseUrl}>
                              {config.apiBaseUrl}
                            </td>
                            <td className="px-6 py-4 text-center">
                              {config.isActive ? (
                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                                  <i className="fa fa-circle-check mr-1"></i>
                                  Active
                                </span>
                              ) : (
                                <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                                  <i className="fa fa-circle-xmark mr-1"></i>
                                  Inactive
                                </span>
                              )}
                            </td>
                            <td className="px-6 py-4 text-right">
                              <div className="flex items-center justify-end gap-3">
                                <button
                                  onClick={() => handleTestConnection(config.configId)}
                                  className="text-blue-600 hover:text-blue-900"
                                  disabled={isTestingConnection}
                                  title="Test Connection"
                                >
                                  <i className={`fa-light ${isTestingConnection ? 'fa-spinner fa-spin' : 'fa-plug-circle-check'} text-blue-600`} style={{ fontSize: '20px' }}></i>
                                </button>
                                <button
                                  onClick={() => handleEditToyotaConfig(config.configId)}
                                  className="text-primary-600 hover:text-primary-900"
                                  title="Edit"
                                >
                                  <i className="fa-light fa-edit text-blue-600" style={{ fontSize: '20px' }}></i>
                                </button>
                                <button
                                  onClick={() => handleDeleteToyotaConfig(config.configId)}
                                  className="text-error-600 hover:text-error-900"
                                  title="Delete"
                                >
                                  <i className="fa-light fa-trash text-red-600" style={{ fontSize: '20px' }}></i>
                                </button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </CardContent>
            </Card>

            {/* Toyota API Action Buttons */}
            <div className="flex justify-end">
              <Button
                onClick={() => router.push('/')}
                variant="primary"
                className="w-full sm:w-auto"
              >
                <i className="fa-light fa-home mr-2"></i>
                Back to Dashboard
              </Button>
            </div>
          </div>
        )}

        {activeTab === 'internal-kanban' && (
          <div className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Internal Kanban Settings</CardTitle>
              </CardHeader>
              <CardContent>
                {isLoadingKanbanSettings ? (
                  <div className="flex justify-center items-center py-12">
                    <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#253262]"></div>
                  </div>
                ) : (
                  <>
                    {/* Kanban Duplication Rules Section */}
                    <div className="space-y-6">
                      <div>
                        <h3 className="text-base font-semibold text-gray-900 mb-4 flex items-center gap-2">
                          Kanban Duplication Rules
                          <div className="group relative">
                            <span className="inline-flex items-center justify-center w-5 h-5 text-xs font-bold text-white bg-blue-500 rounded-full cursor-help">i</span>
                            <div className="absolute left-0 top-6 w-64 p-2 bg-gray-900 text-white text-xs rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all z-10">
                              Configure how the system handles kanban duplication for internal operations
                            </div>
                          </div>
                        </h3>

                        <div className="space-y-3">
                          {/* Allow Duplicates Checkbox */}
                          <label className="flex items-start p-3 border-2 rounded-lg cursor-pointer transition-all hover:border-gray-300 hover:bg-gray-50">
                            <input
                              type="checkbox"
                              checked={kanbanSettings.allowDuplicates}
                              onChange={(e) => setKanbanSettings({ ...kanbanSettings, allowDuplicates: e.target.checked })}
                              className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300 rounded mt-0.5"
                            />
                            <div className="ml-3 flex-1">
                              <div className="text-sm font-medium text-gray-900">Allow Duplicate Kanbans</div>
                              <div className="text-sm text-gray-600 mt-1">
                                Enable creation of duplicate kanbans for the same part within the specified time window.
                              </div>
                            </div>
                          </label>

                          {/* Duplicate Window Hours */}
                          <div className="p-3 border-2 rounded-lg border-gray-200">
                            <label className="block text-sm font-medium text-gray-900 mb-2">
                              Duplicate Window (Hours)
                            </label>
                            <div className="flex items-center gap-3">
                              <Input
                                type="number"
                                min="1"
                                value={kanbanSettings.duplicateWindowHours}
                                onChange={(e) => setKanbanSettings({ ...kanbanSettings, duplicateWindowHours: parseInt(e.target.value) || 1 })}
                                placeholder="Enter hours"
                                className="flex-1 text-sm"
                              />
                              <span className="text-sm text-gray-600 font-medium">hours</span>
                            </div>
                            <div className="text-sm text-gray-600 mt-2">
                              Time window to check for duplicate kanbans (e.g., 24 hours = check if duplicate exists within last 24 hours).
                            </div>
                          </div>

                          {/* Alert on Duplicate Checkbox */}
                          <label className="flex items-start p-3 border-2 rounded-lg cursor-pointer transition-all hover:border-gray-300 hover:bg-gray-50">
                            <input
                              type="checkbox"
                              checked={kanbanSettings.alertOnDuplicate}
                              onChange={(e) => setKanbanSettings({ ...kanbanSettings, alertOnDuplicate: e.target.checked })}
                              className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300 rounded mt-0.5"
                            />
                            <div className="ml-3 flex-1">
                              <div className="text-sm font-medium text-gray-900">Alert on Duplicate Detection</div>
                              <div className="text-sm text-gray-600 mt-1">
                                Show an alert/warning when a duplicate kanban is detected during creation.
                              </div>
                            </div>
                          </label>
                        </div>
                      </div>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>

            {/* Internal Kanban Action Buttons - OUTSIDE the card */}
            <div className="flex flex-col sm:flex-row gap-3 justify-end">
              <Button
                onClick={handleSaveKanbanSettings}
                disabled={isSaving}
                loading={isSaving}
                variant="success-light"
                className="w-full sm:w-auto flex-1 sm:flex-none"
              >
                <i className="fa-light fa-floppy-disk mr-2"></i>
                {isSaving ? 'Saving...' : 'Save Settings'}
              </Button>
              <Button
                onClick={() => router.push('/')}
                variant="primary"
                className="w-full sm:w-auto flex-1 sm:flex-none"
              >
                <i className="fa-light fa-home mr-2"></i>
                Back to Dashboard
              </Button>
            </div>
          </div>
        )}

        {activeTab === 'dock-monitor' && (
          <div className="space-y-6">
            {/* Single Card containing all three sections */}
            <div className="bg-white rounded-lg shadow-sm p-4">
              {isLoadingDockSettings ? (
                <div className="flex justify-center items-center py-12">
                  <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-[#253262]"></div>
                </div>
              ) : (
                <>
              {/* Time Thresholds Section */}
              <div className="mb-6">
                <div className="flex items-center gap-2 mb-3">
                  <h3 className="text-base font-semibold text-gray-900">Time Thresholds</h3>
                  <div className="relative group">
                    {/* Blue circled i icon */}
                    <div className="w-4 h-4 rounded-full bg-blue-500 text-white flex items-center justify-center cursor-help" style={{ fontSize: '10px', fontWeight: 'bold' }}>
                      i
                    </div>
                    {/* Tooltip */}
                    <div className="absolute left-full ml-2 top-1/2 -translate-y-1/2 px-3 py-2 bg-gray-900 text-white text-sm rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 whitespace-normal pointer-events-none z-10 max-w-xs">
                      Set warning times for skid build and shipment load operations
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  {/* Behind Threshold */}
                  <div className="p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors">
                    <div className="flex items-center gap-2 mb-2">
                      <label htmlFor="behind-threshold" className="text-sm font-medium text-gray-900">
                        Behind Threshold
                      </label>
                      <div className="relative group">
                        <div className="w-3.5 h-3.5 rounded-full bg-blue-500 text-white flex items-center justify-center cursor-help" style={{ fontSize: '9px', fontWeight: 'bold' }}>
                          i
                        </div>
                        <div className="absolute left-full ml-2 top-1/2 -translate-y-1/2 px-3 py-2 bg-gray-900 text-white text-sm rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 whitespace-normal pointer-events-none z-10 w-72 max-w-xs">
                          Set the time in minutes when orders should be marked as behind schedule
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <Input
                        id="behind-threshold"
                        type="number"
                        min="1"
                        value={dockSettings.behindThreshold}
                        onChange={(e) => setDockSettings({ ...dockSettings, behindThreshold: parseInt(e.target.value) || 0 })}
                        placeholder="Enter minutes"
                        className="flex-1 text-sm"
                      />
                      <span className="text-sm text-gray-600 font-medium">minutes</span>
                    </div>
                  </div>

                  {/* Critical Threshold */}
                  <div className="p-3 rounded-lg border border-gray-200 hover:bg-gray-50 transition-colors">
                    <div className="flex items-center gap-2 mb-2">
                      <label htmlFor="critical-threshold" className="text-sm font-medium text-gray-900">
                        Critical Threshold
                      </label>
                      <div className="relative group">
                        <div className="w-3.5 h-3.5 rounded-full bg-blue-500 text-white flex items-center justify-center cursor-help" style={{ fontSize: '9px', fontWeight: 'bold' }}>
                          i
                        </div>
                        <div className="absolute left-full ml-2 top-1/2 -translate-y-1/2 px-3 py-2 bg-gray-900 text-white text-sm rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 whitespace-normal pointer-events-none z-10 w-72 max-w-xs">
                          Set the time in minutes when orders should be marked as critically behind schedule
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <Input
                        id="critical-threshold"
                        type="number"
                        min="1"
                        value={dockSettings.criticalThreshold}
                        onChange={(e) => setDockSettings({ ...dockSettings, criticalThreshold: parseInt(e.target.value) || 0 })}
                        placeholder="Enter minutes"
                        className="flex-1 text-sm"
                      />
                      <span className="text-sm text-gray-600 font-medium">minutes</span>
                    </div>
                  </div>
                </div>
              </div>

              {/* Divider */}
              <hr className="my-6 border-gray-200" />

              {/* Display Mode Section */}
              <div className="mb-6">
                <div className="flex items-center gap-2 mb-3">
                  <h3 className="text-base font-semibold text-gray-900">Display Mode</h3>
                  <div className="relative group">
                    {/* Blue circled i icon */}
                    <div className="w-4 h-4 rounded-full bg-blue-500 text-white flex items-center justify-center cursor-help" style={{ fontSize: '10px', fontWeight: 'bold' }}>
                      i
                    </div>
                    {/* Tooltip */}
                    <div className="absolute left-full ml-2 top-1/2 -translate-y-1/2 px-3 py-2 bg-gray-900 text-white text-sm rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 pointer-events-none z-10 w-72 whitespace-normal">
                      Choose how you want to view order information on the dock monitor display
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  {/* Full Version */}
                  <label
                    className="flex items-center gap-3 cursor-pointer p-3 rounded-lg hover:bg-gray-50 transition-colors border border-gray-200"
                    title="Display all order information including skid build and shipment load times"
                  >
                    <input
                      type="radio"
                      name="display-mode"
                      value="FULL"
                      checked={dockSettings.displayMode === 'FULL'}
                      onChange={(e) => setDockSettings({ ...dockSettings, displayMode: e.target.value as DisplayMode })}
                      className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300"
                    />
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-900">Full Version</div>
                    </div>
                  </label>

                  {/* Pre-shipment Scan Only */}
                  <label
                    className="flex items-center gap-3 cursor-pointer p-3 rounded-lg hover:bg-gray-50 transition-colors border border-gray-200"
                    title="Display only pre-shipment scan information and status"
                  >
                    <input
                      type="radio"
                      name="display-mode"
                      value="SHIPMENT_ONLY"
                      checked={dockSettings.displayMode === 'SHIPMENT_ONLY'}
                      onChange={(e) => setDockSettings({ ...dockSettings, displayMode: e.target.value as DisplayMode })}
                      className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300"
                    />
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-900">Pre-shipment Scan Only</div>
                    </div>
                  </label>

                  {/* Skid Build Only */}
                  <label
                    className="flex items-center gap-3 cursor-pointer p-3 rounded-lg hover:bg-gray-50 transition-colors border border-gray-200"
                    title="Display only skid building information and status"
                  >
                    <input
                      type="radio"
                      name="display-mode"
                      value="SKID_ONLY"
                      checked={dockSettings.displayMode === 'SKID_ONLY'}
                      onChange={(e) => setDockSettings({ ...dockSettings, displayMode: e.target.value as DisplayMode })}
                      className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300"
                    />
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-900">Skid Build Only</div>
                    </div>
                  </label>

                  {/* Completion Only */}
                  <label
                    className="flex items-center gap-3 cursor-pointer p-3 rounded-lg hover:bg-gray-50 transition-colors border border-gray-200"
                    title="Display only completion times and final status"
                  >
                    <input
                      type="radio"
                      name="display-mode"
                      value="COMPLETION_ONLY"
                      checked={dockSettings.displayMode === 'COMPLETION_ONLY'}
                      onChange={(e) => setDockSettings({ ...dockSettings, displayMode: e.target.value as DisplayMode })}
                      className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300"
                    />
                    <div className="flex-1">
                      <div className="text-sm font-medium text-gray-900">Completion Only</div>
                    </div>
                  </label>
                </div>
              </div>

              {/* Divider */}
              <hr className="my-6 border-gray-200" />

              {/* Plant/Location Section */}
              <div>
                <div className="flex items-center gap-2 mb-3">
                  <h3 className="text-base font-semibold text-gray-900">Plant / Location</h3>
                  <div className="relative group">
                    {/* Blue circled i icon */}
                    <div className="w-4 h-4 rounded-full bg-blue-500 text-white flex items-center justify-center cursor-help" style={{ fontSize: '10px', fontWeight: 'bold' }}>
                      i
                    </div>
                    {/* Tooltip */}
                    <div className="absolute left-full ml-2 top-1/2 -translate-y-1/2 px-3 py-2 bg-gray-900 text-white text-sm rounded-lg opacity-0 invisible group-hover:opacity-100 group-hover:visible transition-all duration-200 pointer-events-none z-10 w-64 whitespace-normal">
                      Select your current working location (shown in app header)
                    </div>
                  </div>
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                  {LOCATIONS.map((location) => (
                    <label
                      key={location.id}
                      className="flex items-center gap-3 cursor-pointer p-3 rounded-lg hover:bg-gray-50 transition-colors border border-gray-200"
                      title={location.address}
                    >
                      <input
                        type="radio"
                        name="plant-location"
                        value={location.id}
                        checked={selectedLocation === location.id}
                        onChange={(e) => setSelectedLocation(e.target.value)}
                        className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300"
                      />
                      <div className="flex-1">
                        <div className="text-sm font-medium text-gray-900">{location.name}</div>
                      </div>
                    </label>
                  ))}
                </div>
              </div>
                </>
              )}
            </div>

            {/* Dock Monitor Action Buttons - All THREE on same line */}
            <div className="flex flex-col sm:flex-row gap-3 sm:gap-4 justify-end items-center">
              <Button
                onClick={handleSaveDockSettings}
                disabled={isSaving}
                loading={isSaving}
                variant="success-light"
                className="w-full sm:w-auto flex-1 sm:flex-none"
              >
                <i className="fa-light fa-floppy-disk mr-2"></i>
                {isSaving ? 'Saving...' : 'Save Settings'}
              </Button>
              <Button
                onClick={handleResetDockSettings}
                variant="primary"
                disabled={isSaving}
                className="w-full sm:w-auto flex-1 sm:flex-none"
              >
                <i className="fa-light fa-rotate-right mr-2"></i>
                Reset Defaults
              </Button>
              <Button
                onClick={() => router.push('/')}
                variant="primary"
                className="w-full sm:w-auto flex-1 sm:flex-none"
              >
                <i className="fa-light fa-home mr-2"></i>
                Back to Dashboard
              </Button>
            </div>
          </div>
        )}

        {/* Parts Maintenance tab hidden - will be re-enabled when feature is complete */}
        {/* {activeTab === 'parts' && (
          <Card>
            <CardHeader>
              <div className="flex justify-between items-center">
                <CardTitle>Parts Maintenance</CardTitle>
                <Button onClick={handleAddPart} size="md" variant="primary">
                  <i className="fa-light fa-plus mr-2"></i>
                  Add Part
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="w-full">
                <table className="w-full table-fixed divide-y divide-gray-200">
                  <colgroup>
                    <col style={{width: '120px'}} />
                    <col style={{width: '280px'}} />
                    <col style={{width: '100px'}} />
                    <col style={{width: '120px'}} />
                    <col style={{width: '140px'}} />
                    <col style={{width: '140px'}} />
                    <col style={{width: '120px'}} />
                  </colgroup>
                  <thead className="bg-gray-50">
                    <tr>
                      <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Part No</th>
                      <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Description</th>
                      <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Unit of Measure</th>
                      <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Weight/Piece</th>
                      <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Part Type</th>
                      <th className="px-6 py-4 text-left text-sm font-medium text-gray-500 uppercase tracking-wider">Category</th>
                      <th className="sticky right-0 bg-gray-50 px-6 py-4 text-right text-sm font-medium text-gray-500 uppercase tracking-wider shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">Actions</th>
                    </tr>
                  </thead>
                  <tbody className="bg-white divide-y divide-gray-200">
                    {parts.map((part) => (
                      <tr key={part.id} className="hover:bg-gray-50">
                        <td className="px-6 py-4 text-base font-medium text-gray-900">{part.partNo}</td>
                        <td className="px-6 py-4 text-base text-gray-900 truncate" title={part.description}>{part.description}</td>
                        <td className="px-6 py-4 text-base text-gray-900">{part.unitOfMeasure}</td>
                        <td className="px-6 py-4 text-base text-gray-900">{part.weightPerPiece} {part.uomPerPiece}</td>
                        <td className="px-6 py-4 text-base">
                          <span className={`px-2 py-1 text-xs font-semibold rounded-full ${
                            part.partType === 'Regular Parts' ? 'bg-blue-100 text-blue-800' :
                            part.partType === 'Special Parts' ? 'bg-green-100 text-green-800' :
                            'bg-orange-100 text-orange-800'
                          }`}>
                            {part.partType}
                          </span>
                        </td>
                        <td className="px-6 py-4 text-base text-gray-900">{part.category}</td>
                        <td className="sticky right-0 bg-white px-6 py-4 text-right text-base shadow-[-4px_0_4px_-4px_rgba(0,0,0,0.1)]">
                          <button onClick={() => handleEditPart(part.id)} className="text-primary-600 hover:text-primary-900 mr-3">
                            <i className="fa-light fa-edit text-blue-600" style={{ fontSize: '20px' }}></i>
                          </button>
                          <button onClick={() => handleDeletePart(part.id)} className="text-error-600 hover:text-error-900">
                            <i className="fa-light fa-trash text-red-600" style={{ fontSize: '20px' }}></i>
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>
        )} */}
        </div>
      </div>

      {/* Office Add Panel */}
      <SlideOutPanel
        isOpen={isAddOfficeOpen}
        onClose={() => setIsAddOfficeOpen(false)}
        title="Add New Office"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Office Code *</label>
              <Input value={officeForm.code} onChange={(e) => setOfficeForm({...officeForm, code: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Office Name *</label>
              <Input value={officeForm.name} onChange={(e) => setOfficeForm({...officeForm, name: e.target.value})} />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Address *</label>
            <Input value={officeForm.address} onChange={(e) => setOfficeForm({...officeForm, address: e.target.value})} />
          </div>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">City *</label>
              <Input value={officeForm.city} onChange={(e) => setOfficeForm({...officeForm, city: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">State *</label>
              <select
                value={officeForm.state}
                onChange={(e) => setOfficeForm({...officeForm, state: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select a state</option>
                {US_STATES.map((state) => (
                  <option key={state} value={state}>
                    {state}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Zip *</label>
              <Input value={officeForm.zip} onChange={(e) => setOfficeForm({...officeForm, zip: e.target.value})} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
              <Input value={officeForm.phone} onChange={(e) => setOfficeForm({...officeForm, phone: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Contact Person</label>
              <Input value={officeForm.contact} onChange={(e) => setOfficeForm({...officeForm, contact: e.target.value})} />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
            <Input type="email" value={officeForm.email} onChange={(e) => setOfficeForm({...officeForm, email: e.target.value})} />
          </div>
          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveOffice} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Office
            </Button>
            <Button onClick={() => setIsAddOfficeOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* Office Edit Panel */}
      <SlideOutPanel
        isOpen={isEditOfficeOpen}
        onClose={() => setIsEditOfficeOpen(false)}
        title="Edit Office"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Office Code *</label>
              <Input value={officeForm.code} onChange={(e) => setOfficeForm({...officeForm, code: e.target.value})} disabled />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Office Name *</label>
              <Input value={officeForm.name} onChange={(e) => setOfficeForm({...officeForm, name: e.target.value})} />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Address *</label>
            <Input value={officeForm.address} onChange={(e) => setOfficeForm({...officeForm, address: e.target.value})} />
          </div>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">City *</label>
              <Input value={officeForm.city} onChange={(e) => setOfficeForm({...officeForm, city: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">State *</label>
              <select
                value={officeForm.state}
                onChange={(e) => setOfficeForm({...officeForm, state: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select a state</option>
                {US_STATES.map((state) => (
                  <option key={state} value={state}>
                    {state}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Zip *</label>
              <Input value={officeForm.zip} onChange={(e) => setOfficeForm({...officeForm, zip: e.target.value})} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
              <Input value={officeForm.phone} onChange={(e) => setOfficeForm({...officeForm, phone: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Contact Person</label>
              <Input value={officeForm.contact} onChange={(e) => setOfficeForm({...officeForm, contact: e.target.value})} />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
            <Input type="email" value={officeForm.email} onChange={(e) => setOfficeForm({...officeForm, email: e.target.value})} />
          </div>
          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveOffice} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Changes
            </Button>
            <Button onClick={() => setIsEditOfficeOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* Warehouse Add Panel */}
      <SlideOutPanel
        isOpen={isAddWarehouseOpen}
        onClose={() => setIsAddWarehouseOpen(false)}
        title="Add New Warehouse"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Warehouse Code *</label>
              <Input value={warehouseForm.code} onChange={(e) => setWarehouseForm({...warehouseForm, code: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Warehouse Name *</label>
              <Input value={warehouseForm.name} onChange={(e) => setWarehouseForm({...warehouseForm, name: e.target.value})} />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Address *</label>
            <Input value={warehouseForm.address} onChange={(e) => setWarehouseForm({...warehouseForm, address: e.target.value})} />
          </div>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">City *</label>
              <Input value={warehouseForm.city} onChange={(e) => setWarehouseForm({...warehouseForm, city: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">State *</label>
              <select
                value={warehouseForm.state}
                onChange={(e) => setWarehouseForm({...warehouseForm, state: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select a state</option>
                {US_STATES.map((state) => (
                  <option key={state} value={state}>
                    {state}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Zip *</label>
              <Input value={warehouseForm.zip} onChange={(e) => setWarehouseForm({...warehouseForm, zip: e.target.value})} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
              <Input value={warehouseForm.phone} onChange={(e) => setWarehouseForm({...warehouseForm, phone: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Office *</label>
              <select
                value={warehouseForm.office}
                onChange={(e) => setWarehouseForm({...warehouseForm, office: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select an office</option>
                {offices.map((office) => (
                  <option key={office.id} value={office.code}>
                    {office.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Contact Name</label>
              <Input value={warehouseForm.contactName} onChange={(e) => setWarehouseForm({...warehouseForm, contactName: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Contact Email</label>
              <Input type="email" value={warehouseForm.contactEmail} onChange={(e) => setWarehouseForm({...warehouseForm, contactEmail: e.target.value})} />
            </div>
          </div>
          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveWarehouse} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Warehouse
            </Button>
            <Button onClick={() => setIsAddWarehouseOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* Warehouse Edit Panel */}
      <SlideOutPanel
        isOpen={isEditWarehouseOpen}
        onClose={() => setIsEditWarehouseOpen(false)}
        title="Edit Warehouse"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Warehouse Code *</label>
              <Input value={warehouseForm.code} onChange={(e) => setWarehouseForm({...warehouseForm, code: e.target.value})} disabled />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Warehouse Name *</label>
              <Input value={warehouseForm.name} onChange={(e) => setWarehouseForm({...warehouseForm, name: e.target.value})} disabled />
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Address *</label>
            <Input value={warehouseForm.address} onChange={(e) => setWarehouseForm({...warehouseForm, address: e.target.value})} />
          </div>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">City *</label>
              <Input value={warehouseForm.city} onChange={(e) => setWarehouseForm({...warehouseForm, city: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">State *</label>
              <select
                value={warehouseForm.state}
                onChange={(e) => setWarehouseForm({...warehouseForm, state: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select a state</option>
                {US_STATES.map((state) => (
                  <option key={state} value={state}>
                    {state}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Zip *</label>
              <Input value={warehouseForm.zip} onChange={(e) => setWarehouseForm({...warehouseForm, zip: e.target.value})} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Phone</label>
              <Input value={warehouseForm.phone} onChange={(e) => setWarehouseForm({...warehouseForm, phone: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Office *</label>
              <select
                value={warehouseForm.office}
                onChange={(e) => setWarehouseForm({...warehouseForm, office: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select an office</option>
                {offices.map((office) => (
                  <option key={office.id} value={office.code}>
                    {office.name}
                  </option>
                ))}
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Contact Name</label>
              <Input value={warehouseForm.contactName} onChange={(e) => setWarehouseForm({...warehouseForm, contactName: e.target.value})} />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Contact Email</label>
              <Input type="email" value={warehouseForm.contactEmail} onChange={(e) => setWarehouseForm({...warehouseForm, contactEmail: e.target.value})} />
            </div>
          </div>
          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveWarehouse} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Changes
            </Button>
            <Button onClick={() => setIsEditWarehouseOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* User Add Panel */}
      <SlideOutPanel
        isOpen={isAddUserOpen}
        onClose={() => setIsAddUserOpen(false)}
        title="Add New User"
        width="lg"
      >
        <div className="space-y-6">
          {/* Error Alert - Shows inside popup */}
          {error && isAddUserOpen && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Username (Login) *</label>
            <Input
              value={userForm.username}
              onChange={(e) => setUserForm({...userForm, username: e.target.value})}
              placeholder="e.g., jdoe"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Name (Display Name)</label>
            <Input
              value={userForm.name}
              onChange={(e) => setUserForm({...userForm, name: e.target.value})}
              placeholder="e.g., John Doe (optional)"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Password *</label>
            <Input type="password" value={userForm.password} onChange={(e) => setUserForm({...userForm, password: e.target.value})} />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
            <Input type="email" value={userForm.email} onChange={(e) => setUserForm({...userForm, email: e.target.value})} />
          </div>
          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={userForm.supervisor}
                onChange={(e) => setUserForm({...userForm, supervisor: e.target.checked})}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Supervisor</span>
            </label>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Menu Level *</label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="menuLevel"
                  value="Admin"
                  checked={userForm.menuLevel === 'Admin'}
                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Admin</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="menuLevel"
                  value="Scanner"
                  checked={userForm.menuLevel === 'Scanner'}
                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Scanner</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="menuLevel"
                  value="Operation"
                  checked={userForm.menuLevel === 'Operation'}
                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Operation</span>
              </label>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Operation</label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="operation"
                  value="Warehouse"
                  checked={userForm.operation === 'Warehouse'}
                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Warehouse</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="operation"
                  value="Office"
                  checked={userForm.operation === 'Office'}
                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Office</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="operation"
                  value="Administration"
                  checked={userForm.operation === 'Administration'}
                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Administration</span>
              </label>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Code</label>
            <select
              value={userForm.code}
              onChange={(e) => setUserForm({...userForm, code: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="">Select a code</option>
              <optgroup label="Offices">
                {offices.filter(office => office.isActive !== false).map((office) => (
                  <option key={`office-${office.id}`} value={office.code}>
                    {office.name} ({office.code})
                  </option>
                ))}
              </optgroup>
              <optgroup label="Warehouses">
                {warehouses.filter(warehouse => warehouse.isActive !== false).map((warehouse) => (
                  <option key={`warehouse-${warehouse.id}`} value={warehouse.code}>
                    {warehouse.name} ({warehouse.code})
                  </option>
                ))}
              </optgroup>
            </select>
          </div>
          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveUser} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save User
            </Button>
            <Button onClick={() => setIsAddUserOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* User Edit Panel */}
      <SlideOutPanel
        isOpen={isEditUserOpen}
        onClose={() => setIsEditUserOpen(false)}
        title="Edit User"
        width="lg"
      >
        <div className="space-y-6">
          {/* Error Alert - Shows inside popup */}
          {error && isEditUserOpen && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded text-red-700">
              {error}
            </div>
          )}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Username (Login) *</label>
            <Input
              value={userForm.username}
              onChange={(e) => setUserForm({...userForm, username: e.target.value})}
              placeholder="e.g., jdoe"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Name (Display Name)</label>
            <Input
              value={userForm.name}
              onChange={(e) => setUserForm({...userForm, name: e.target.value})}
              placeholder="e.g., John Doe (optional)"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Password</label>
            <Input
              type="password"
              value={userForm.password}
              onChange={(e) => setUserForm({...userForm, password: e.target.value})}
              placeholder="Leave blank to keep current password"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Email</label>
            <Input type="email" value={userForm.email} onChange={(e) => setUserForm({...userForm, email: e.target.value})} />
          </div>
          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={userForm.supervisor}
                onChange={(e) => setUserForm({...userForm, supervisor: e.target.checked})}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Supervisor</span>
            </label>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Menu Level *</label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="menuLevel-edit"
                  value="Admin"
                  checked={userForm.menuLevel === 'Admin'}
                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Admin</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="menuLevel-edit"
                  value="Scanner"
                  checked={userForm.menuLevel === 'Scanner'}
                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Scanner</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="menuLevel-edit"
                  value="Operation"
                  checked={userForm.menuLevel === 'Operation'}
                  onChange={(e) => setUserForm({...userForm, menuLevel: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Operation</span>
              </label>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Operation</label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="operation-edit"
                  value="Warehouse"
                  checked={userForm.operation === 'Warehouse'}
                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Warehouse</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="operation-edit"
                  value="Office"
                  checked={userForm.operation === 'Office'}
                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Office</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="operation-edit"
                  value="Administration"
                  checked={userForm.operation === 'Administration'}
                  onChange={(e) => setUserForm({...userForm, operation: e.target.value})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Administration</span>
              </label>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Code</label>
            <select
              value={userForm.code}
              onChange={(e) => setUserForm({...userForm, code: e.target.value})}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
            >
              <option value="">Select a code</option>
              <optgroup label="Offices">
                {offices.filter(office => office.isActive !== false).map((office) => (
                  <option key={`office-${office.id}`} value={office.code}>
                    {office.name} ({office.code})
                  </option>
                ))}
              </optgroup>
              <optgroup label="Warehouses">
                {warehouses.filter(warehouse => warehouse.isActive !== false).map((warehouse) => (
                  <option key={`warehouse-${warehouse.id}`} value={warehouse.code}>
                    {warehouse.name} ({warehouse.code})
                  </option>
                ))}
              </optgroup>
            </select>
          </div>
          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveUser} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Changes
            </Button>
            <Button onClick={() => setIsEditUserOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* Part Add Panel */}
      <SlideOutPanel
        isOpen={isAddPartOpen}
        onClose={() => setIsAddPartOpen(false)}
        title="Add New Part"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part No *</label>
              <Input
                value={partForm.partNo}
                onChange={(e) => setPartForm({...partForm, partNo: e.target.value})}
                placeholder="e.g., RM-001"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part Description *</label>
              <Input
                value={partForm.description}
                onChange={(e) => setPartForm({...partForm, description: e.target.value})}
                placeholder="e.g., Steel Sheet 4x8"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Unit of Measure *</label>
              <select
                value={partForm.unitOfMeasure}
                onChange={(e) => setPartForm({...partForm, unitOfMeasure: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select UOM</option>
                <option value="EA">EA - Each</option>
                <option value="LB">LB - Pounds</option>
                <option value="PC">PC - Piece</option>
                <option value="FT">FT - Feet</option>
                <option value="IN">IN - Inches</option>
                <option value="KG">KG - Kilograms</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Weight Per Piece *</label>
              <Input
                type="number"
                step="0.1"
                value={partForm.weightPerPiece}
                onChange={(e) => setPartForm({...partForm, weightPerPiece: parseFloat(e.target.value) || 0})}
                placeholder="e.g., 120.5"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">UOM Per Piece *</label>
              <select
                value={partForm.uomPerPiece}
                onChange={(e) => setPartForm({...partForm, uomPerPiece: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select UOM</option>
                <option value="EA">EA - Each</option>
                <option value="LB">LB - Pounds</option>
                <option value="PC">PC - Piece</option>
                <option value="FT">FT - Feet</option>
                <option value="IN">IN - Inches</option>
                <option value="KG">KG - Kilograms</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part Classification *</label>
              <select
                value={partForm.category}
                onChange={(e) => setPartForm({...partForm, category: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select Classification</option>
                <option value="Metal">Metal</option>
                <option value="Automotive">Automotive</option>
                <option value="Welding">Welding</option>
                <option value="Electronics">Electronics</option>
                <option value="Packaging">Packaging</option>
                <option value="Hardware">Hardware</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Part Type *</label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="partType-add"
                  value="Regular Parts"
                  checked={partForm.partType === 'Regular Parts'}
                  onChange={(e) => setPartForm({...partForm, partType: e.target.value as Part['partType']})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Regular Parts</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="partType-add"
                  value="Special Parts"
                  checked={partForm.partType === 'Special Parts'}
                  onChange={(e) => setPartForm({...partForm, partType: e.target.value as Part['partType']})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Special Parts</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="partType-add"
                  value="International Steel Parts"
                  checked={partForm.partType === 'International Steel Parts'}
                  onChange={(e) => setPartForm({...partForm, partType: e.target.value as Part['partType']})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">International Steel Parts</span>
              </label>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part Location</label>
              <select
                value={partForm.location}
                onChange={(e) => setPartForm({...partForm, location: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select Location</option>
                <option value="A-100">A-100</option>
                <option value="A-150">A-150</option>
                <option value="B-205">B-205</option>
                <option value="B-310">B-310</option>
                <option value="C-010">C-010</option>
                <option value="C-050">C-050</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Packing Style</label>
              <Input
                value={partForm.packingStyle}
                onChange={(e) => setPartForm({...partForm, packingStyle: e.target.value})}
                placeholder="e.g., Bulk, Boxed, Crated"
              />
            </div>
          </div>

          <div className="flex gap-6">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={partForm.commonPart}
                onChange={(e) => setPartForm({...partForm, commonPart: e.target.checked})}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Common Part</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={partForm.discontinued}
                onChange={(e) => setPartForm({...partForm, discontinued: e.target.checked})}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Discontinued Part</span>
            </label>
          </div>

          <div className="flex gap-3 pt-4">
            <Button onClick={handleSavePart} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Part
            </Button>
            <Button onClick={() => setIsAddPartOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* Part Edit Panel */}
      <SlideOutPanel
        isOpen={isEditPartOpen}
        onClose={() => setIsEditPartOpen(false)}
        title="Edit Part"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part No *</label>
              <Input
                value={partForm.partNo}
                onChange={(e) => setPartForm({...partForm, partNo: e.target.value})}
                placeholder="e.g., RM-001"
                disabled
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part Description *</label>
              <Input
                value={partForm.description}
                onChange={(e) => setPartForm({...partForm, description: e.target.value})}
                placeholder="e.g., Steel Sheet 4x8"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Unit of Measure *</label>
              <select
                value={partForm.unitOfMeasure}
                onChange={(e) => setPartForm({...partForm, unitOfMeasure: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select UOM</option>
                <option value="EA">EA - Each</option>
                <option value="LB">LB - Pounds</option>
                <option value="PC">PC - Piece</option>
                <option value="FT">FT - Feet</option>
                <option value="IN">IN - Inches</option>
                <option value="KG">KG - Kilograms</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Weight Per Piece *</label>
              <Input
                type="number"
                step="0.1"
                value={partForm.weightPerPiece}
                onChange={(e) => setPartForm({...partForm, weightPerPiece: parseFloat(e.target.value) || 0})}
                placeholder="e.g., 120.5"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">UOM Per Piece *</label>
              <select
                value={partForm.uomPerPiece}
                onChange={(e) => setPartForm({...partForm, uomPerPiece: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select UOM</option>
                <option value="EA">EA - Each</option>
                <option value="LB">LB - Pounds</option>
                <option value="PC">PC - Piece</option>
                <option value="FT">FT - Feet</option>
                <option value="IN">IN - Inches</option>
                <option value="KG">KG - Kilograms</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part Classification *</label>
              <select
                value={partForm.category}
                onChange={(e) => setPartForm({...partForm, category: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select Classification</option>
                <option value="Metal">Metal</option>
                <option value="Automotive">Automotive</option>
                <option value="Welding">Welding</option>
                <option value="Electronics">Electronics</option>
                <option value="Packaging">Packaging</option>
                <option value="Hardware">Hardware</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Part Type *</label>
            <div className="flex gap-4">
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="partType-edit"
                  value="Regular Parts"
                  checked={partForm.partType === 'Regular Parts'}
                  onChange={(e) => setPartForm({...partForm, partType: e.target.value as Part['partType']})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Regular Parts</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="partType-edit"
                  value="Special Parts"
                  checked={partForm.partType === 'Special Parts'}
                  onChange={(e) => setPartForm({...partForm, partType: e.target.value as Part['partType']})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">Special Parts</span>
              </label>
              <label className="flex items-center gap-2">
                <input
                  type="radio"
                  name="partType-edit"
                  value="International Steel Parts"
                  checked={partForm.partType === 'International Steel Parts'}
                  onChange={(e) => setPartForm({...partForm, partType: e.target.value as Part['partType']})}
                  className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300"
                />
                <span className="text-sm text-gray-700">International Steel Parts</span>
              </label>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Part Location</label>
              <select
                value={partForm.location}
                onChange={(e) => setPartForm({...partForm, location: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="">Select Location</option>
                <option value="A-100">A-100</option>
                <option value="A-150">A-150</option>
                <option value="B-205">B-205</option>
                <option value="B-310">B-310</option>
                <option value="C-010">C-010</option>
                <option value="C-050">C-050</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Packing Style</label>
              <Input
                value={partForm.packingStyle}
                onChange={(e) => setPartForm({...partForm, packingStyle: e.target.value})}
                placeholder="e.g., Bulk, Boxed, Crated"
              />
            </div>
          </div>

          <div className="flex gap-6">
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={partForm.commonPart}
                onChange={(e) => setPartForm({...partForm, commonPart: e.target.checked})}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Common Part</span>
            </label>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={partForm.discontinued}
                onChange={(e) => setPartForm({...partForm, discontinued: e.target.checked})}
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Discontinued Part</span>
            </label>
          </div>

          <div className="flex gap-3 pt-4">
            <Button onClick={handleSavePart} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Changes
            </Button>
            <Button onClick={() => setIsEditPartOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* Toyota Config Add Panel */}
      <SlideOutPanel
        isOpen={isAddToyotaConfigOpen}
        onClose={() => setIsAddToyotaConfigOpen(false)}
        title="Add Toyota API Configuration"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Environment *</label>
              <select
                value={toyotaConfigForm.environment}
                onChange={(e) => handleToyotaEnvChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
              >
                <option value="QA">QA</option>
                <option value="PROD">PROD</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Application Name</label>
              <Input
                value={toyotaConfigForm.applicationName}
                onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, applicationName: e.target.value})}
                placeholder="e.g., VUTEQ Scanner App"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Client ID *</label>
            <Input
              value={toyotaConfigForm.clientId}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, clientId: e.target.value})}
              placeholder="Enter Client ID from Azure AD"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Client Secret *</label>
            <Input
              type="password"
              value={toyotaConfigForm.clientSecret}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, clientSecret: e.target.value})}
              placeholder="Enter Client Secret"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Token URL *</label>
            <Input
              value={toyotaConfigForm.tokenUrl}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, tokenUrl: e.target.value})}
              placeholder="https://login.microsoftonline.com/..."
            />
            <p className="text-xs text-gray-500 mt-1">
              OAuth2 token endpoint URL
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">API Base URL *</label>
            <Input
              value={toyotaConfigForm.apiBaseUrl}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, apiBaseUrl: e.target.value})}
              placeholder="https://api.scs.toyota.com/..."
            />
            <p className="text-xs text-gray-500 mt-1">
              Base URL for Toyota API endpoints
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Resource URL *</label>
            <Input
              value={toyotaConfigForm.resourceUrl}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, resourceUrl: e.target.value})}
              placeholder="https://tmnatest.onmicrosoft.com/..."
            />
            <p className="text-xs text-gray-500 mt-1">
              OAuth2 resource identifier (V2.1)
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">X-Client-ID *</label>
            <Input
              value={toyotaConfigForm.xClientId}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, xClientId: e.target.value})}
              placeholder="a1012aed-c89a-49d3-a796-63a4345ecc98"
            />
            <p className="text-xs text-gray-500 mt-1">
              Client ID for API request headers (V2.1)
            </p>
          </div>

          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={toyotaConfigForm.isActive}
                onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, isActive: e.target.checked})}
                className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Set as Active Configuration</span>
            </label>
            <p className="text-xs text-gray-500 ml-6">
              Only one configuration per environment can be active at a time
            </p>
          </div>

          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveToyotaConfig} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Configuration
            </Button>
            <Button onClick={() => setIsAddToyotaConfigOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>

      {/* Toyota Config Edit Panel */}
      <SlideOutPanel
        isOpen={isEditToyotaConfigOpen}
        onClose={() => setIsEditToyotaConfigOpen(false)}
        title="Edit Toyota API Configuration"
        width="lg"
      >
        <div className="space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Environment *</label>
              <select
                value={toyotaConfigForm.environment}
                onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, environment: e.target.value})}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                disabled
              >
                <option value="QA">QA</option>
                <option value="PROD">PROD</option>
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">Application Name</label>
              <Input
                value={toyotaConfigForm.applicationName}
                onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, applicationName: e.target.value})}
                placeholder="e.g., VUTEQ Scanner App"
              />
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Client ID *</label>
            <Input
              value={toyotaConfigForm.clientId}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, clientId: e.target.value})}
              placeholder="Enter Client ID from Azure AD"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Client Secret</label>
            <Input
              type="password"
              value={toyotaConfigForm.clientSecret}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, clientSecret: e.target.value})}
              placeholder="Leave blank to keep current secret"
            />
            <p className="text-xs text-gray-500 mt-1">
              Leave blank if you don&apos;t want to update the secret
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Token URL *</label>
            <Input
              value={toyotaConfigForm.tokenUrl}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, tokenUrl: e.target.value})}
              placeholder="https://login.microsoftonline.com/..."
            />
            <p className="text-xs text-gray-500 mt-1">
              OAuth2 token endpoint URL
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">API Base URL *</label>
            <Input
              value={toyotaConfigForm.apiBaseUrl}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, apiBaseUrl: e.target.value})}
              placeholder="https://api.scs.toyota.com/..."
            />
            <p className="text-xs text-gray-500 mt-1">
              Base URL for Toyota API endpoints
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">Resource URL *</label>
            <Input
              value={toyotaConfigForm.resourceUrl}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, resourceUrl: e.target.value})}
              placeholder="https://tmnatest.onmicrosoft.com/..."
            />
            <p className="text-xs text-gray-500 mt-1">
              OAuth2 resource identifier (V2.1)
            </p>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">X-Client-ID *</label>
            <Input
              value={toyotaConfigForm.xClientId}
              onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, xClientId: e.target.value})}
              placeholder="a1012aed-c89a-49d3-a796-63a4345ecc98"
            />
            <p className="text-xs text-gray-500 mt-1">
              Client ID for API request headers (V2.1)
            </p>
          </div>

          <div>
            <label className="flex items-center gap-2">
              <input
                type="checkbox"
                checked={toyotaConfigForm.isActive}
                onChange={(e) => setToyotaConfigForm({...toyotaConfigForm, isActive: e.target.checked})}
                className="h-4 w-4 text-[#253262] focus:ring-[#253262] border-gray-300 rounded"
              />
              <span className="text-sm font-medium text-gray-700">Set as Active Configuration</span>
            </label>
            <p className="text-xs text-gray-500 ml-6">
              Only one configuration per environment can be active at a time
            </p>
          </div>

          <div className="flex gap-3 pt-4">
            <Button onClick={handleSaveToyotaConfig} variant="success-light" className="flex-1">
              <i className="fa-light fa-floppy-disk mr-2"></i>
              Save Changes
            </Button>
            <Button onClick={() => setIsEditToyotaConfigOpen(false)} variant="error" className="flex-1">
              <i className="fa-light fa-xmark mr-2"></i>
              Cancel
            </Button>
          </div>
        </div>
      </SlideOutPanel>
    </div>
  );
}
