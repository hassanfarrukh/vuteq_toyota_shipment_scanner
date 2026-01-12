/**
 * Order Data Page
 * Author: Hassan
 * Date: 2025-11-05
 * Last Updated: 2025-12-12 by Hassan - Added column sorting, renamed route to /orders
 *
 * Manages order data with three main views:
 *
 * Tab 1 - Imported Files:
 * - Excel file upload with drag & drop support via side panel
 * - Real backend API integration for Excel processing
 * - Upload results displayed INSIDE modal popup (success/warning/error)
 * - Handles skipped orders response (ordersSkipped, skippedOrderNumbers)
 * - File metadata tracking (name, size, upload date, status)
 * - Delete functionality for uploaded files
 * - Click on file row to switch to Tab 2 with uploadId filter
 * - Pagination with rows per page options (10, 25, 50, 100)
 * - Column sorting on all columns
 *
 * Tab 2 - Planned Orders (tblOrders):
 * - Table showing orders with summary information
 * - Filter by specific upload file (when clicked from Tab 1)
 * - Clear filter to show all orders
 * - Columns: RealOrderNumber, Total Parts, DockCode, Departure Date, Order Date, Status
 * - Click on order row to switch to Tab 3 with orderId filter
 * - Pagination with rows per page options (10, 25, 50, 100)
 * - Column sorting on all columns
 *
 * Tab 3 - Planned Parts:
 * - Table showing planned order items with full details
 * - Filter by specific order (when clicked from Tab 2)
 * - Clear filter to show all planned items
 * - Columns: OrderNumber, PartNumber, Kanban, QPC, TotalBoxPlanned, ManifestNo, PalletizationCode, Short/Over
 * - Pagination with rows per page options (10, 25, 50, 100)
 * - Column sorting on all columns
 *
 * Features:
 * - Mobile-responsive design
 * - Role restriction: SUPERVISOR and ADMIN only
 * - Uses Font Awesome Pro icons
 * - VUTEQ Navy (#253262) for primary actions and headers
 * - Side panel slides in from right with backdrop overlay
 * - Universal search bar works across all tabs (searches all visible columns)
 * - Real-time filtering as user types
 * - Clear button (X) in search input
 * - Shows filtered count vs total (e.g., "Showing 5 of 25 results")
 * - Empty state with clear search button when no results found
 * - Each tab maintains its own pagination state
 */

'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Button from '@/components/ui/Button';
import Alert from '@/components/ui/Alert';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import SlideOutPanel from '@/components/ui/SlideOutPanel';
import * as orderUploadsApi from '@/lib/api/orderUploads';
import type { OrderUploadResponseDto } from '@/lib/api/orderUploads';
import { getSiteSettings } from '@/lib/api/siteSettings';

// Type definitions
interface UploadedFile {
  id: string;
  fileName: string;
  fileSize: number;
  uploadDate: string;
  uploadedByUsername: string | null;
  status: 'success' | 'pending' | 'error' | 'warning';
  ordersCreated?: number;
  totalItemsCreated?: number;
  totalManifestsCreated?: number;
  ordersSkipped?: number;
  skippedOrderNumbers?: string[];
}

interface UploadResult {
  type: 'success' | 'warning' | 'error';
  message: string;
  ordersCreated?: number;
  ordersSkipped?: number;
  skippedOrderNumbers?: string[];
}

interface Order {
  id: string;
  realOrderNumber: string;
  totalParts: number;
  dockCode: string;
  departureDate: string;
  orderDate: string;
  status: string;
  uploadId: string;
  uploadFileName?: string;
  plannedRoute?: string;
  mainRoute?: string;
}

interface PlannedItem {
  id: string;
  realOrderNumber: string;
  dockCode: string;
  partNumber: string;
  qpc: number;
  kanbanNumber: string;
  internalKanban: string;
  totalBoxPlanned: number;
  palletizationCode: string;
  externalOrderId: number;
  skidUid: string | null;
  manifestNo: number;
  shortOver: number | null;
  uploadId: string;
  orderId?: string;
  uploadFileName?: string;
}

type TabType = 'imported-files' | 'planned-orders' | 'planned-parts';

// Sort types for each table
type FilesSortColumn = 'fileName' | 'uploadDate' | 'uploadedByUsername' | 'ordersCreated' | 'status';
type OrdersSortColumn = 'realOrderNumber' | 'totalParts' | 'dockCode' | 'departureDate' | 'orderDate' | 'status' | 'mainRoute' | 'plannedRoute';
type PartsSortColumn = 'realOrderNumber' | 'partNumber' | 'kanbanNumber' | 'internalKanban' | 'qpc' | 'totalBoxPlanned' | 'manifestNo' | 'palletizationCode' | 'shortOver';
type SortDirection = 'asc' | 'desc';

export default function OrdersPage() {
  const router = useRouter();
  const { user } = useAuth();
  const fileInputRef = useRef<HTMLInputElement>(null);

  // State
  const [activeTab, setActiveTab] = useState<TabType>('planned-orders');
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFile[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [plannedItems, setPlannedItems] = useState<PlannedItem[]>([]);
  const [filteredUploadId, setFilteredUploadId] = useState<string | null>(null);
  const [filteredOrderId, setFilteredOrderId] = useState<string | null>(null);
  const [filteredOrderNumber, setFilteredOrderNumber] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [isLoadingOrders, setIsLoadingOrders] = useState(false);
  const [isLoadingPlanned, setIsLoadingPlanned] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [isTableDragging, setIsTableDragging] = useState(false);
  const [isUploadPanelOpen, setIsUploadPanelOpen] = useState(false);
  const [uploadResult, setUploadResult] = useState<UploadResult | null>(null);
  const [showSkippedDetails, setShowSkippedDetails] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  // Archive view state
  const [isArchivedView, setIsArchivedView] = useState(false);
  const [orderArchiveDays, setOrderArchiveDays] = useState(14);
  const [latestUploadDate, setLatestUploadDate] = useState<string | null>(null);

  // Pagination state for each tab
  const [filesPage, setFilesPage] = useState(1);
  const [filesPerPage, setFilesPerPage] = useState(10);
  const [ordersPage, setOrdersPage] = useState(1);
  const [ordersPerPage, setOrdersPerPage] = useState(10);
  const [partsPage, setPartsPage] = useState(1);
  const [partsPerPage, setPartsPerPage] = useState(10);

  // Sort state for each tab
  const [filesSortColumn, setFilesSortColumn] = useState<FilesSortColumn | null>(null);
  const [filesSortDirection, setFilesSortDirection] = useState<SortDirection>('asc');
  const [ordersSortColumn, setOrdersSortColumn] = useState<OrdersSortColumn | null>(null);
  const [ordersSortDirection, setOrdersSortDirection] = useState<SortDirection>('asc');
  const [partsSortColumn, setPartsSortColumn] = useState<PartsSortColumn | null>(null);
  const [partsSortDirection, setPartsSortDirection] = useState<SortDirection>('asc');

  // Load site settings and data on mount
  useEffect(() => {
    loadSiteSettings();
    loadOrders();
    loadLatestUploadDate();
  }, []);

  // Fetch the latest upload date (unfiltered) - for display purposes only
  const loadLatestUploadDate = async () => {
    try {
      // Get all uploads without date filtering to find the latest
      const response = await orderUploadsApi.getUploadHistory();
      if (response.success && response.data && response.data.length > 0) {
        // Sort by uploadDate descending and get the first (most recent)
        const sortedUploads = [...response.data].sort((a, b) => {
          return new Date(b.uploadDate).getTime() - new Date(a.uploadDate).getTime();
        });
        setLatestUploadDate(sortedUploads[0].uploadDate);
      }
    } catch (e) {
      console.error('Error loading latest upload date:', e);
    }
  };

  // Load upload history when archived view changes
  useEffect(() => {
    console.log('[Archive Tab Change] isArchivedView changed to:', isArchivedView);
    loadUploadHistory();
    // Reload orders and planned items when archive view changes
    // Clear any filters first
    setFilteredUploadId(null);
    setFilteredOrderId(null);
    setFilteredOrderNumber(null);
    loadOrders();
    loadPlannedItems();
  }, [isArchivedView, orderArchiveDays]);

  // Fetch site settings to get orderArchiveDays
  const loadSiteSettings = async () => {
    try {
      const response = await getSiteSettings();
      if (response.success && response.data) {
        setOrderArchiveDays(response.data.orderArchiveDays || 14);
      }
    } catch (e) {
      console.error('Error loading site settings:', e);
      // Use default value of 14 days if settings fail to load
      setOrderArchiveDays(14);
    }
  };

  // Fetch upload history from API with date filtering
  const loadUploadHistory = async () => {
    setIsLoading(true);
    try {
      // Calculate date filters based on archive view
      const today = new Date();
      const archiveCutoffDate = new Date();
      archiveCutoffDate.setDate(today.getDate() - orderArchiveDays);

      const formatDate = (date: Date): string => {
        return date.toISOString().split('T')[0]; // YYYY-MM-DD format
      };

      let fromDate: string | undefined;
      let toDate: string | undefined;

      if (isArchivedView) {
        // Archived: show uploads older than X days (toDate = cutoff date)
        toDate = formatDate(archiveCutoffDate);
        console.log('[API Call] Loading ARCHIVED uploads (toDate):', toDate);
      } else {
        // Current: show uploads from last X days (fromDate = cutoff date)
        fromDate = formatDate(archiveCutoffDate);
        console.log('[API Call] Loading CURRENT uploads (fromDate):', fromDate);
      }

      const response = await orderUploadsApi.getUploadHistory(fromDate, toDate);
      if (response.success && response.data) {
        const files: UploadedFile[] = response.data.map((upload: OrderUploadResponseDto) => ({
          id: upload.uploadId,
          fileName: upload.fileName,
          fileSize: upload.fileSize,
          uploadDate: upload.uploadDate,
          uploadedByUsername: upload.uploadedByUsername,
          status: upload.status as 'success' | 'pending' | 'error' | 'warning',
          ordersCreated: upload.ordersCreated,
          totalItemsCreated: upload.totalItemsCreated,
          totalManifestsCreated: upload.totalManifestsCreated,
          ordersSkipped: upload.ordersSkipped,
          skippedOrderNumbers: upload.skippedOrderNumbers,
        }));
        console.log('[API Response] Loaded', files.length, 'upload files');
        setUploadedFiles(files);
      }
    } catch (e) {
      console.error('Error loading upload history:', e);
    } finally {
      setIsLoading(false);
    }
  };

  // Fetch orders from API
  const loadOrders = async (uploadId?: string) => {
    setIsLoadingOrders(true);
    console.log('[API Call] Loading orders with uploadId:', uploadId || 'ALL');
    try {
      // Calculate date filters based on archive view
      const today = new Date();
      const archiveCutoffDate = new Date();
      archiveCutoffDate.setDate(today.getDate() - orderArchiveDays);

      const formatDate = (date: Date): string => {
        return date.toISOString().split('T')[0]; // YYYY-MM-DD format
      };

      let fromDate: string | undefined;
      let toDate: string | undefined;

      if (isArchivedView) {
        // Archived: show orders from uploads older than X days (toDate = cutoff date)
        toDate = formatDate(archiveCutoffDate);
        console.log('[API Call] Loading ARCHIVED orders (toDate):', toDate);
      } else {
        // Current: show orders from uploads within last X days (fromDate = cutoff date)
        fromDate = formatDate(archiveCutoffDate);
        console.log('[API Call] Loading CURRENT orders (fromDate):', fromDate);
      }

      const response = await orderUploadsApi.getOrders(uploadId, fromDate, toDate);
      if (response.success && response.data) {
        const fetchedOrders: Order[] = response.data.map((order: orderUploadsApi.OrderDto) => ({
          id: order.orderId,
          realOrderNumber: order.realOrderNumber,
          totalParts: order.totalParts,
          dockCode: order.dockCode,
          departureDate: order.departureDate,
          orderDate: order.orderDate,
          status: order.status,
          uploadId: order.uploadId,
          uploadFileName: order.uploadFileName,
          plannedRoute: order.plannedRoute,
          mainRoute: order.mainRoute,
        }));
        console.log('[API Response] Loaded', fetchedOrders.length, 'orders');
        setOrders(fetchedOrders);
      }
    } catch (e) {
      console.error('Error loading orders:', e);
    } finally {
      setIsLoadingOrders(false);
    }
  };

  // Fetch planned items from API
  const loadPlannedItems = async (orderId?: string) => {
    setIsLoadingPlanned(true);
    console.log('[API Call] Loading planned items with orderId:', orderId || 'ALL');
    try {
      // Calculate date filters based on archive view
      const today = new Date();
      const archiveCutoffDate = new Date();
      archiveCutoffDate.setDate(today.getDate() - orderArchiveDays);

      const formatDate = (date: Date): string => {
        return date.toISOString().split('T')[0]; // YYYY-MM-DD format
      };

      let fromDate: string | undefined;
      let toDate: string | undefined;

      if (isArchivedView) {
        // Archived: show planned items from uploads older than X days (toDate = cutoff date)
        toDate = formatDate(archiveCutoffDate);
        console.log('[API Call] Loading ARCHIVED planned items (toDate):', toDate);
      } else {
        // Current: show planned items from uploads within last X days (fromDate = cutoff date)
        fromDate = formatDate(archiveCutoffDate);
        console.log('[API Call] Loading CURRENT planned items (fromDate):', fromDate);
      }

      const response = await orderUploadsApi.getPlannedItems(orderId, fromDate, toDate);
      if (response.success && response.data) {
        const items: PlannedItem[] = response.data.map((item: orderUploadsApi.PlannedItemDto) => ({
          id: item.id,
          realOrderNumber: item.realOrderNumber,
          dockCode: item.dockCode,
          partNumber: item.partNumber,
          qpc: item.qpc,
          kanbanNumber: item.kanbanNumber,
          internalKanban: item.internalKanban,
          totalBoxPlanned: item.totalBoxPlanned,
          palletizationCode: item.palletizationCode,
          externalOrderId: item.externalOrderId,
          skidUid: item.skidUid,
          manifestNo: item.manifestNo,
          shortOver: item.shortOver,
          uploadId: item.uploadId,
          orderId: item.orderId,
          uploadFileName: item.uploadFileName,
        }));
        console.log('[API Response] Loaded', items.length, 'planned items');
        setPlannedItems(items);
      }
    } catch (e) {
      console.error('Error loading planned items:', e);
    } finally {
      setIsLoadingPlanned(false);
    }
  };

  // Handle file row click - switch to orders tab with filter
  const handleFileRowClick = (fileId: string) => {
    setFilteredUploadId(fileId);
    setFilteredOrderId(null);
    setFilteredOrderNumber(null);
    setActiveTab('planned-orders');
    loadOrders(fileId);
  };

  // Handle order row click - switch to parts tab with filter
  const handleOrderRowClick = (orderId: string, orderNumber: string) => {
    setFilteredOrderId(orderId);
    setFilteredOrderNumber(orderNumber);
    setSearchQuery(''); // Clear search query when viewing parts for specific order
    setActiveTab('planned-parts');
    loadPlannedItems(orderId);
  };

  // Handle clear upload filter (Tab 2)
  const handleClearUploadFilter = () => {
    setFilteredUploadId(null);
    loadOrders();
  };

  // Handle clear order filter (Tab 3)
  const handleClearOrderFilter = () => {
    setFilteredOrderId(null);
    setFilteredOrderNumber(null);
    loadPlannedItems();
  };

  // Format file size
  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  };

  // Format date
  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  // Get most recent upload timestamp
  const getLastUploadTime = (): string => {
    if (!latestUploadDate) {
      return 'No orders uploaded yet';
    }
    return formatDate(latestUploadDate);
  };

  // Handle file selection - upload to API
  const handleFileSelect = async (file: File) => {
    setUploadResult(null);
    setShowSkippedDetails(false);

    // Validate file type (Excel only)
    const allowedTypes = [
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    ];
    if (!allowedTypes.includes(file.type)) {
      setUploadResult({
        type: 'error',
        message: 'Only Excel (.xlsx) files are allowed',
      });
      return;
    }

    // Validate file size (max 10MB)
    const maxSize = 10 * 1024 * 1024; // 10MB
    if (file.size > maxSize) {
      setUploadResult({
        type: 'error',
        message: 'File size must be less than 10MB',
      });
      return;
    }

    setIsUploading(true);

    try {
      // Upload file to API
      const response = await orderUploadsApi.uploadOrderFile(file);

      if (response.success && response.data) {
        // Create new file entry from response
        const newFile: UploadedFile = {
          id: response.data.uploadId,
          fileName: response.data.fileName,
          fileSize: response.data.fileSize,
          uploadDate: response.data.uploadDate,
          uploadedByUsername: response.data.uploadedByUsername,
          status: response.data.status as 'success' | 'pending' | 'error' | 'warning',
          ordersCreated: response.data.ordersCreated,
          totalItemsCreated: response.data.totalItemsCreated,
          totalManifestsCreated: response.data.totalManifestsCreated,
          ordersSkipped: response.data.ordersSkipped,
          skippedOrderNumbers: response.data.skippedOrderNumbers,
        };

        // Add to files list
        setUploadedFiles(prev => [newFile, ...prev]);

        // Reload latest upload date to update "Last order uploaded at" display
        loadLatestUploadDate();

        // Switch to Imported Files tab after successful upload
        setActiveTab('imported-files');

        // Determine result type and message based on response
        const ordersCreated = response.data.ordersCreated || 0;
        const ordersSkipped = response.data.ordersSkipped || 0;

        if (ordersSkipped > 0 && ordersCreated === 0) {
          // All orders were skipped - show warning
          setUploadResult({
            type: 'warning',
            message: `All orders already exist in the system. No new orders were created.`,
            ordersCreated,
            ordersSkipped,
            skippedOrderNumbers: response.data.skippedOrderNumbers,
          });
        } else if (ordersSkipped > 0 && ordersCreated > 0) {
          // Some orders created, some skipped - show success with info
          setUploadResult({
            type: 'success',
            message: `Upload completed successfully!`,
            ordersCreated,
            ordersSkipped,
            skippedOrderNumbers: response.data.skippedOrderNumbers,
          });
        } else if (ordersCreated > 0) {
          // All orders created successfully
          setUploadResult({
            type: 'success',
            message: `Successfully uploaded ${file.name}!`,
            ordersCreated,
            ordersSkipped: 0,
          });
        } else {
          // No orders created or skipped - show error
          setUploadResult({
            type: 'error',
            message: `No orders were processed from the file.`,
          });
        }
      } else {
        setUploadResult({
          type: 'error',
          message: response.message || 'Failed to upload file',
        });
      }
    } catch (e) {
      console.error('Upload error:', e);
      setUploadResult({
        type: 'error',
        message: 'An error occurred while uploading the file',
      });
    } finally {
      setIsUploading(false);

      // Reset file input
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  // Handle file input change
  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  // Handle drag events
  const handleDragEnter = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragging(false);

    const file = e.dataTransfer.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  // Handle upload button click
  const handleUploadClick = () => {
    fileInputRef.current?.click();
  };

  // Handle close upload panel
  const handleClosePanel = () => {
    setIsUploadPanelOpen(false);
    setUploadResult(null);
    setShowSkippedDetails(false);
  };

  // Handle "Done" button after successful upload
  const handleDone = () => {
    handleClosePanel();
    // Show brief success notification on page
    setSuccess(uploadResult?.message || 'Upload completed');
    setTimeout(() => setSuccess(null), 3000);
  };

  // Handle delete file - call API
  const handleDeleteFile = async (fileId: string) => {
    if (confirm('Are you sure you want to delete this file from history?')) {
      try {
        const response = await orderUploadsApi.deleteUpload(fileId);
        if (response.success) {
          setUploadedFiles(prev => prev.filter(f => f.id !== fileId));
          setSuccess('File removed from history');
          setTimeout(() => setSuccess(null), 3000);
        } else {
          setError(response.message || 'Failed to delete file');
        }
      } catch (e) {
        console.error('Delete error:', e);
        setError('An error occurred while deleting the file');
      }
    }
  };

  // Handle table drag events
  const handleTableDragEnter = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsTableDragging(true);
  };

  const handleTableDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsTableDragging(false);
  };

  const handleTableDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const handleTableDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsTableDragging(false);

    const file = e.dataTransfer.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  // Get filtered file name for display
  const getFilteredFileName = (): string | null => {
    if (!filteredUploadId) return null;
    const file = uploadedFiles.find(f => f.id === filteredUploadId);
    return file ? file.fileName : null;
  };

  // Filter function for search - works on any data type
  const filterData = <T extends Record<string, any>>(data: T[], query: string): T[] => {
    if (!query.trim()) return data;
    const lowerQuery = query.toLowerCase();

    return data.filter(item =>
      Object.values(item).some(value => {
        if (value === null || value === undefined) return false;
        return value.toString().toLowerCase().includes(lowerQuery);
      })
    );
  };

  // Apply search filter to current tab data
  const filteredFiles = filterData(uploadedFiles, searchQuery);
  const filteredOrders = filterData(orders, searchQuery);
  const filteredPlannedItems = filterData(plannedItems, searchQuery);

  // Sort handlers for each table
  const handleFilesSort = (column: FilesSortColumn) => {
    if (filesSortColumn === column) {
      setFilesSortDirection(filesSortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setFilesSortColumn(column);
      setFilesSortDirection('asc');
    }
  };

  const handleOrdersSort = (column: OrdersSortColumn) => {
    if (ordersSortColumn === column) {
      setOrdersSortDirection(ordersSortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setOrdersSortColumn(column);
      setOrdersSortDirection('asc');
    }
  };

  const handlePartsSort = (column: PartsSortColumn) => {
    if (partsSortColumn === column) {
      setPartsSortDirection(partsSortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setPartsSortColumn(column);
      setPartsSortDirection('asc');
    }
  };

  // Sort icon helper
  const getSortIcon = (isActive: boolean, direction: SortDirection) => {
    if (!isActive) return <i className="fa-light fa-sort ml-1 text-xs opacity-30"></i>;
    return direction === 'asc'
      ? <i className="fa-light fa-sort-up ml-1 text-xs"></i>
      : <i className="fa-light fa-sort-down ml-1 text-xs"></i>;
  };

  // Generic sort function
  const sortData = <T extends Record<string, any>>(
    data: T[],
    column: string | null,
    direction: SortDirection
  ): T[] => {
    if (!column) return data;
    return [...data].sort((a, b) => {
      let aValue = a[column];
      let bValue = b[column];

      // Handle null/undefined
      if (aValue === null || aValue === undefined) aValue = '';
      if (bValue === null || bValue === undefined) bValue = '';

      // Handle numbers
      if (typeof aValue === 'number' && typeof bValue === 'number') {
        return direction === 'asc' ? aValue - bValue : bValue - aValue;
      }

      // String comparison
      const comparison = String(aValue).localeCompare(String(bValue));
      return direction === 'asc' ? comparison : -comparison;
    });
  };

  // Apply sorting to filtered data
  const sortedFiles = sortData(filteredFiles, filesSortColumn, filesSortDirection);
  const sortedOrders = sortData(filteredOrders, ordersSortColumn, ordersSortDirection);
  const sortedPlannedItems = sortData(filteredPlannedItems, partsSortColumn, partsSortDirection);

  // Reset to page 1 when search query or archived view changes
  useEffect(() => {
    setFilesPage(1);
    setOrdersPage(1);
    setPartsPage(1);
  }, [searchQuery, isArchivedView]);

  // Redirect if not supervisor or admin (after all hooks)
  if (user && user.role !== 'SUPERVISOR' && user.role !== 'ADMIN') {
    router.push('/');
    return null;
  }

  // Pagination helper function
  const paginateData = <T,>(data: T[], page: number, perPage: number): T[] => {
    const startIndex = (page - 1) * perPage;
    const endIndex = startIndex + perPage;
    return data.slice(startIndex, endIndex);
  };

  // Apply pagination to sorted data
  const paginatedFiles = paginateData(sortedFiles, filesPage, filesPerPage);
  const paginatedOrders = paginateData(sortedOrders, ordersPage, ordersPerPage);
  const paginatedPlannedItems = paginateData(sortedPlannedItems, partsPage, partsPerPage);

  // Calculate total pages
  const filesTotalPages = Math.ceil(filteredFiles.length / filesPerPage);
  const ordersTotalPages = Math.ceil(filteredOrders.length / ordersPerPage);
  const partsTotalPages = Math.ceil(filteredPlannedItems.length / partsPerPage);

  // Get current tab filtered results count
  const getFilteredCount = () => {
    if (activeTab === 'imported-files') {
      return { filtered: filteredFiles.length, total: uploadedFiles.length };
    } else if (activeTab === 'planned-orders') {
      return { filtered: filteredOrders.length, total: orders.length };
    } else {
      return { filtered: filteredPlannedItems.length, total: plannedItems.length };
    }
  };

  // Pagination Component
  interface PaginationProps {
    currentPage: number;
    totalPages: number;
    totalItems: number;
    itemsPerPage: number;
    onPageChange: (page: number) => void;
    onItemsPerPageChange: (perPage: number) => void;
  }

  const Pagination: React.FC<PaginationProps> = ({
    currentPage,
    totalPages,
    totalItems,
    itemsPerPage,
    onPageChange,
    onItemsPerPageChange,
  }) => {
    // Calculate start and end item numbers
    const startItem = totalItems === 0 ? 0 : (currentPage - 1) * itemsPerPage + 1;
    const endItem = Math.min(currentPage * itemsPerPage, totalItems);

    // Generate page numbers to show
    const getPageNumbers = () => {
      const pages: (number | string)[] = [];

      if (totalPages <= 7) {
        // Show all pages if 7 or fewer
        for (let i = 1; i <= totalPages; i++) {
          pages.push(i);
        }
      } else {
        // Always show first page
        pages.push(1);

        if (currentPage > 3) {
          pages.push('...');
        }

        // Show current page and neighbors
        const start = Math.max(2, currentPage - 1);
        const end = Math.min(totalPages - 1, currentPage + 1);

        for (let i = start; i <= end; i++) {
          pages.push(i);
        }

        if (currentPage < totalPages - 2) {
          pages.push('...');
        }

        // Always show last page
        pages.push(totalPages);
      }

      return pages;
    };

    const handlePerPageChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
      const newPerPage = parseInt(e.target.value, 10);
      onItemsPerPageChange(newPerPage);
      onPageChange(1); // Reset to first page when changing items per page
    };

    return (
      <div className="flex flex-col sm:flex-row justify-between items-center gap-4 px-4 py-3 border-t border-gray-200 bg-gray-50">
        {/* Left: Rows per page */}
        <div className="flex items-center gap-2">
          <span className="text-sm text-gray-700">Rows per page:</span>
          <select
            value={itemsPerPage}
            onChange={handlePerPageChange}
            className="border border-gray-300 rounded px-2 py-1 text-sm focus:outline-none focus:ring-2 focus:ring-[#253262] focus:border-transparent"
          >
            <option value={10}>10</option>
            <option value={25}>25</option>
            <option value={50}>50</option>
            <option value={100}>100</option>
          </select>
        </div>

        {/* Center: Showing X-Y of Z */}
        <div className="text-sm text-gray-700">
          Showing {startItem}-{endItem} of {totalItems}
        </div>

        {/* Right: Page navigation */}
        <div className="flex items-center gap-1">
          {/* Previous button */}
          <button
            onClick={() => onPageChange(currentPage - 1)}
            disabled={currentPage === 1}
            className="px-3 py-1 rounded text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-200 disabled:hover:bg-transparent"
            title="Previous page"
          >
            <i className="fa fa-chevron-left"></i>
          </button>

          {/* Page numbers */}
          {getPageNumbers().map((page, index) => {
            if (page === '...') {
              return (
                <span key={`ellipsis-${index}`} className="px-2 text-gray-500">
                  ...
                </span>
              );
            }

            const pageNum = page as number;
            return (
              <button
                key={pageNum}
                onClick={() => onPageChange(pageNum)}
                className={`px-3 py-1 rounded text-sm font-medium transition-colors ${
                  currentPage === pageNum
                    ? 'bg-[#253262] text-white'
                    : 'hover:bg-gray-200 text-gray-700'
                }`}
              >
                {pageNum}
              </button>
            );
          })}

          {/* Next button */}
          <button
            onClick={() => onPageChange(currentPage + 1)}
            disabled={currentPage === totalPages || totalPages === 0}
            className="px-3 py-1 rounded text-sm font-medium transition-colors disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-200 disabled:hover:bg-transparent"
            title="Next page"
          >
            <i className="fa fa-chevron-right"></i>
          </button>
        </div>
      </div>
    );
  };

  return (
    <div className="relative min-h-screen">
      {/* VUTEQ Static Background */}
      <VUTEQStaticBackground />

      {/* Content */}
      <div className="relative">
        <div className="pt-1 px-8 pb-8 w-full space-y-6">
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

          {/* Main Content Card with Tabs */}
          <Card>
            <CardContent className="p-0">
              {/* Last Upload Time - Top row inside card */}
              <div className="px-6 pt-6 pb-4">
                {/* Last Upload Time */}
                <div className="flex items-center gap-2 text-gray-700">
                  <i className="fa fa-clock text-lg"></i>
                  <span className="text-sm">
                    Last order uploaded at: {getLastUploadTime()}
                  </span>
                </div>
              </div>

              {/* Current/Archived Toggle Tabs + Upload Button */}
              <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 px-6 pb-4">
                {/* Left: Current/Archived Tabs */}
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => setIsArchivedView(false)}
                    className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                      !isArchivedView
                        ? 'bg-[#253262] text-white shadow-md'
                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    }`}
                  >
                    <i className="fa fa-clock mr-2"></i>
                    Current (Last {orderArchiveDays} Days)
                  </button>
                  <button
                    onClick={() => setIsArchivedView(true)}
                    className={`px-4 py-2 rounded-lg text-sm font-medium transition-all ${
                      isArchivedView
                        ? 'bg-[#253262] text-white shadow-md'
                        : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                    }`}
                  >
                    <i className="fa fa-archive mr-2"></i>
                    Archived (Older than {orderArchiveDays} Days)
                  </button>
                </div>

                {/* Right: Upload New File Button */}
                <Button
                  onClick={() => setIsUploadPanelOpen(true)}
                  size="md"
                  variant="primary"
                  className="w-full sm:w-auto"
                  style={{ backgroundColor: '#253262', color: 'white' }}
                >
                  <i className="fa-light fa-upload mr-2"></i>
                  Upload New File
                </Button>
              </div>

              {/* Tab Navigation + Search Bar */}
              <div className="flex items-center justify-between border-b border-gray-700 px-6">
                {/* Tabs on the left */}
                <div className="flex">
                  <button
                    onClick={() => {
                      setActiveTab('planned-orders');
                      setFilteredUploadId(null);
                      if (orders.length === 0) {
                        loadOrders();
                      }
                    }}
                    className={`px-6 py-3 text-sm font-medium transition-colors relative ${
                      activeTab === 'planned-orders'
                        ? 'text-[#253262] border-b-2 border-[#253262]'
                        : 'text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    <i className="fa fa-clipboard-list mr-2"></i>
                    Planned Orders
                  </button>
                  <button
                    onClick={() => {
                      setActiveTab('planned-parts');
                      setFilteredOrderId(null);
                      setFilteredOrderNumber(null);
                      if (plannedItems.length === 0) {
                        loadPlannedItems();
                      }
                    }}
                    className={`px-6 py-3 text-sm font-medium transition-colors relative ${
                      activeTab === 'planned-parts'
                        ? 'text-[#253262] border-b-2 border-[#253262]'
                        : 'text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    <i className="fa fa-table mr-2"></i>
                    Planned Parts
                  </button>
                  <button
                    onClick={() => setActiveTab('imported-files')}
                    className={`px-6 py-3 text-sm font-medium transition-colors relative ${
                      activeTab === 'imported-files'
                        ? 'text-[#253262] border-b-2 border-[#253262]'
                        : 'text-gray-500 hover:text-gray-700'
                    }`}
                  >
                    <i className="fa fa-file-import mr-2"></i>
                    Imported Files
                  </button>
                </div>

                {/* Search on the right */}
                <div className="py-2">
                  <div className="relative" style={{ width: '280px' }}>
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <i className="fa fa-magnifying-glass text-gray-400 text-sm"></i>
                    </div>
                    <input
                      type="text"
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      placeholder="Search..."
                      className="block w-full pl-9 pr-9 py-2 border border-gray-300 rounded-lg text-sm placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-[#253262] focus:border-transparent transition-all"
                    />
                    {searchQuery && (
                      <button
                        onClick={() => setSearchQuery('')}
                        className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600 transition-colors"
                        title="Clear search"
                      >
                        <i className="fa fa-xmark text-base"></i>
                      </button>
                    )}
                  </div>
                </div>
              </div>

              {/* Search Results Count - Below tabs */}
              {searchQuery && (
                <div className="px-6 pt-3 pb-2">
                  <div className="text-sm text-gray-600">
                    <i className="fa fa-filter text-[#253262] mr-2"></i>
                    Showing {getFilteredCount().filtered} of {getFilteredCount().total} results
                  </div>
                </div>
              )}

              {/* Tab Content */}
              <div className="p-4">
                {activeTab === 'imported-files' && (
                  <div className="space-y-4">
                    {/* Upload History Table */}
                    {isLoading ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-spinner-third fa-spin text-gray-400"
                          style={{ fontSize: '48px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          Loading upload history...
                        </p>
                      </div>
                    ) : uploadedFiles.length === 0 ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-folder-open text-gray-300"
                          style={{ fontSize: '64px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          No order data files uploaded yet
                        </p>
                      </div>
                    ) : filteredFiles.length === 0 ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-magnifying-glass text-gray-300"
                          style={{ fontSize: '64px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          No files found matching &quot;{searchQuery}&quot;
                        </p>
                        <button
                          onClick={() => setSearchQuery('')}
                          className="mt-4 text-sm text-[#253262] hover:text-[#1a2449] font-medium transition-colors"
                        >
                          Clear search
                        </button>
                      </div>
                    ) : (
                      <>
                        <div
                          onDragEnter={handleTableDragEnter}
                          onDragOver={handleTableDragOver}
                          onDragLeave={handleTableDragLeave}
                          onDrop={handleTableDrop}
                          className={`overflow-x-auto rounded-lg border border-gray-200 transition-all ${
                            isTableDragging
                              ? 'border-2 border-primary-500 border-dashed bg-primary-50'
                              : ''
                          }`}
                        >
                          <table className="w-full text-left">
                            <thead className="bg-gray-50">
                              <tr className="border-b border-gray-200">
                                <th
                                  className="px-4 py-3 text-sm font-semibold text-gray-700 cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleFilesSort('fileName')}
                                >
                                  <div className="flex items-center">
                                    File Name{getSortIcon(filesSortColumn === 'fileName', filesSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 text-sm font-semibold text-gray-700 hidden sm:table-cell cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleFilesSort('uploadDate')}
                                >
                                  <div className="flex items-center">
                                    Upload Date{getSortIcon(filesSortColumn === 'uploadDate', filesSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 text-sm font-semibold text-gray-700 hidden md:table-cell cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleFilesSort('uploadedByUsername')}
                                >
                                  <div className="flex items-center">
                                    Uploaded By{getSortIcon(filesSortColumn === 'uploadedByUsername', filesSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 text-sm font-semibold text-gray-700 hidden lg:table-cell cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleFilesSort('ordersCreated')}
                                >
                                  <div className="flex items-center">
                                    Orders{getSortIcon(filesSortColumn === 'ordersCreated', filesSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 text-sm font-semibold text-gray-700 hidden lg:table-cell cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleFilesSort('status')}
                                >
                                  <div className="flex items-center">
                                    Status{getSortIcon(filesSortColumn === 'status', filesSortDirection)}
                                  </div>
                                </th>
                                <th className="px-4 py-3 text-sm font-semibold text-gray-700">
                                  Actions
                                </th>
                              </tr>
                            </thead>
                            <tbody>
                              {paginatedFiles.map((file) => (
                                <tr
                                  key={file.id}
                                  onClick={() => handleFileRowClick(file.id)}
                                  className="border-b border-gray-100 hover:bg-blue-50 cursor-pointer transition-colors"
                                >
                                  <td className="px-4 py-4 text-sm text-gray-900">
                                    <div className="flex items-center space-x-2">
                                      <i className="fa-light fa-file-excel text-success-600"></i>
                                      <span className="truncate max-w-[150px] sm:max-w-none">
                                        {file.fileName}
                                      </span>
                                    </div>
                                  </td>
                                  <td className="px-4 py-4 text-sm text-gray-600 hidden sm:table-cell">
                                    {formatDate(file.uploadDate)}
                                  </td>
                                  <td className="px-4 py-4 text-sm text-gray-600 hidden md:table-cell">
                                    {file.uploadedByUsername || <span className="text-gray-400">-</span>}
                                  </td>
                                  <td className="px-4 py-4 text-sm text-gray-600 hidden lg:table-cell">
                                    {file.ordersCreated !== undefined ? (
                                      <span className="text-xs">
                                        {file.ordersCreated} orders / {file.totalManifestsCreated || 0} manifests / {file.totalItemsCreated} items
                                      </span>
                                    ) : (
                                      <span className="text-gray-400">-</span>
                                    )}
                                  </td>
                                  <td className="px-4 py-4 text-sm hidden lg:table-cell">
                                    <span
                                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                                        file.status === 'success'
                                          ? 'bg-success-100 text-success-800'
                                          : file.status === 'warning'
                                          ? 'bg-yellow-100 text-yellow-800'
                                          : file.status === 'error'
                                          ? 'bg-error-100 text-error-800'
                                          : 'bg-gray-100 text-gray-800'
                                      }`}
                                    >
                                      {file.status === 'success' ? (
                                        <>
                                          <i className="fa-light fa-circle-check mr-1"></i>
                                          Success
                                        </>
                                      ) : file.status === 'warning' ? (
                                        <>
                                          <i className="fa-light fa-triangle-exclamation mr-1"></i>
                                          Warning
                                        </>
                                      ) : file.status === 'error' ? (
                                        <>
                                          <i className="fa-light fa-circle-xmark mr-1"></i>
                                          Error
                                        </>
                                      ) : (
                                        <>
                                          <i className="fa-light fa-clock mr-1"></i>
                                          Pending
                                        </>
                                      )}
                                    </span>
                                  </td>
                                  <td className="px-4 py-4" onClick={(e) => e.stopPropagation()}>
                                    <button
                                      onClick={() => handleDeleteFile(file.id)}
                                      className="text-error-600 hover:text-error-800 transition-colors"
                                      title="Delete file"
                                    >
                                      <i className="fa-light fa-trash" style={{ fontSize: '18px' }}></i>
                                    </button>
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>

                        {/* Pagination for Imported Files */}
                        <Pagination
                          currentPage={filesPage}
                          totalPages={filesTotalPages}
                          totalItems={filteredFiles.length}
                          itemsPerPage={filesPerPage}
                          onPageChange={setFilesPage}
                          onItemsPerPageChange={setFilesPerPage}
                        />
                      </>
                    )}
                  </div>
                )}

                {activeTab === 'planned-orders' && (
                  <div className="space-y-4">
                    {/* Filter Indicator and Actions */}
                    <div className="flex items-center justify-between">
                      {filteredUploadId ? (
                        <div className="flex items-center gap-2">
                          <i className="fa fa-filter text-[#253262]"></i>
                          <span className="text-sm text-gray-700">
                            Showing orders from: <span className="font-semibold">{getFilteredFileName()}</span>
                          </span>
                        </div>
                      ) : (
                        <span className="text-sm text-gray-600">Showing all orders</span>
                      )}

                      {filteredUploadId && (
                        <Button
                          onClick={handleClearUploadFilter}
                          size="sm"
                          variant="secondary"
                        >
                          <i className="fa fa-xmark mr-2"></i>
                          Clear Filter
                        </Button>
                      )}
                    </div>

                    {/* Orders Table */}
                    {isLoadingOrders ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-spinner-third fa-spin text-gray-400"
                          style={{ fontSize: '48px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          Loading orders...
                        </p>
                      </div>
                    ) : orders.length === 0 ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-clipboard-list text-gray-300"
                          style={{ fontSize: '64px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          No orders found
                        </p>
                      </div>
                    ) : filteredOrders.length === 0 ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-magnifying-glass text-gray-300"
                          style={{ fontSize: '64px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          No orders found matching &quot;{searchQuery}&quot;
                        </p>
                        <button
                          onClick={() => setSearchQuery('')}
                          className="mt-4 text-sm text-[#253262] hover:text-[#1a2449] font-medium transition-colors"
                        >
                          Clear search
                        </button>
                      </div>
                    ) : (
                      <>
                        <div className="overflow-x-auto rounded-lg border border-gray-200">
                          <table className="w-full text-left text-sm">
                            <thead className="bg-gray-50">
                              <tr className="border-b border-gray-200">
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('realOrderNumber')}
                                >
                                  <div className="flex items-center">
                                    Order Number{getSortIcon(ordersSortColumn === 'realOrderNumber', ordersSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('totalParts')}
                                >
                                  <div className="flex items-center justify-center">
                                    Total Parts{getSortIcon(ordersSortColumn === 'totalParts', ordersSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('dockCode')}
                                >
                                  <div className="flex items-center justify-center">
                                    Dock Code{getSortIcon(ordersSortColumn === 'dockCode', ordersSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('mainRoute')}
                                >
                                  <div className="flex items-center justify-center">
                                    Main Route{getSortIcon(ordersSortColumn === 'mainRoute', ordersSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('plannedRoute')}
                                >
                                  <div className="flex items-center justify-center">
                                    Planned Route{getSortIcon(ordersSortColumn === 'plannedRoute', ordersSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('departureDate')}
                                >
                                  <div className="flex items-center justify-center">
                                    Planned Pickup{getSortIcon(ordersSortColumn === 'departureDate', ordersSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('orderDate')}
                                >
                                  <div className="flex items-center justify-center">
                                    Order Date{getSortIcon(ordersSortColumn === 'orderDate', ordersSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handleOrdersSort('status')}
                                >
                                  <div className="flex items-center justify-center">
                                    Status{getSortIcon(ordersSortColumn === 'status', ordersSortDirection)}
                                  </div>
                                </th>
                              </tr>
                            </thead>
                            <tbody>
                              {paginatedOrders.map((order) => (
                                <tr
                                  key={order.id}
                                  onClick={() => handleOrderRowClick(order.id, order.realOrderNumber)}
                                  className="border-b border-gray-100 hover:bg-blue-50 cursor-pointer transition-colors"
                                >
                                  <td className="px-4 py-3 text-gray-900 whitespace-nowrap font-medium">
                                    {order.realOrderNumber}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center">
                                    {order.totalParts}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {order.dockCode}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {order.mainRoute || <span className="text-gray-400">-</span>}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {order.plannedRoute || <span className="text-gray-400">-</span>}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {formatDate(order.departureDate)}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {formatDate(order.orderDate)}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                                      order.status === 'Planned'
                                        ? 'bg-gray-100 text-gray-800'
                                        : order.status === 'SkidBuilding'
                                        ? 'bg-blue-100 text-blue-800'
                                        : order.status === 'SkidBuilt'
                                        ? 'bg-cyan-100 text-cyan-800'
                                        : order.status === 'ReadyToShip'
                                        ? 'bg-purple-100 text-purple-800'
                                        : order.status === 'ShipmentLoading'
                                        ? 'bg-orange-100 text-orange-800'
                                        : order.status === 'Shipped'
                                        ? 'bg-green-100 text-green-800'
                                        : order.status === 'SkidBuildError'
                                        ? 'bg-red-100 text-red-800'
                                        : order.status === 'ShipmentError'
                                        ? 'bg-red-100 text-red-800'
                                        : 'bg-gray-100 text-gray-800'
                                    }`}>
                                      {order.status}
                                    </span>
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>

                        {/* Pagination for Planned Orders */}
                        <Pagination
                          currentPage={ordersPage}
                          totalPages={ordersTotalPages}
                          totalItems={filteredOrders.length}
                          itemsPerPage={ordersPerPage}
                          onPageChange={setOrdersPage}
                          onItemsPerPageChange={setOrdersPerPage}
                        />
                      </>
                    )}
                  </div>
                )}

                {activeTab === 'planned-parts' && (
                  <div className="space-y-4">
                    {/* Filter Indicator and Actions */}
                    <div className="flex items-center justify-between">
                      {filteredOrderId ? (
                        <div className="flex items-center gap-2">
                          <i className="fa fa-filter text-[#253262]"></i>
                          <span className="text-sm text-gray-700">
                            Showing parts for order: <span className="font-semibold">{filteredOrderNumber}</span>
                          </span>
                        </div>
                      ) : (
                        <span className="text-sm text-gray-600">Showing all planned parts</span>
                      )}

                      {filteredOrderId && (
                        <Button
                          onClick={handleClearOrderFilter}
                          size="sm"
                          variant="secondary"
                        >
                          <i className="fa fa-xmark mr-2"></i>
                          Clear Filter
                        </Button>
                      )}
                    </div>

                    {/* Planned Items Table */}
                    {isLoadingPlanned ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-spinner-third fa-spin text-gray-400"
                          style={{ fontSize: '48px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          Loading planned parts...
                        </p>
                      </div>
                    ) : plannedItems.length === 0 ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-table text-gray-300"
                          style={{ fontSize: '64px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          No planned parts found
                        </p>
                      </div>
                    ) : filteredPlannedItems.length === 0 ? (
                      <div className="text-center py-12">
                        <i
                          className="fa-light fa-magnifying-glass text-gray-300"
                          style={{ fontSize: '64px' }}
                        ></i>
                        <p className="text-gray-500 mt-4 text-base">
                          No parts found matching &quot;{searchQuery}&quot;
                        </p>
                        <button
                          onClick={() => setSearchQuery('')}
                          className="mt-4 text-sm text-[#253262] hover:text-[#1a2449] font-medium transition-colors"
                        >
                          Clear search
                        </button>
                      </div>
                    ) : (
                      <>
                        <div className="overflow-x-auto rounded-lg border border-gray-200">
                          <table className="w-full text-left text-sm">
                            <thead className="bg-gray-50">
                              <tr className="border-b border-gray-200">
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('realOrderNumber')}
                                >
                                  <div className="flex items-center">
                                    Order Number{getSortIcon(partsSortColumn === 'realOrderNumber', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('partNumber')}
                                >
                                  <div className="flex items-center">
                                    Part Number{getSortIcon(partsSortColumn === 'partNumber', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('kanbanNumber')}
                                >
                                  <div className="flex items-center justify-center">
                                    Kanban{getSortIcon(partsSortColumn === 'kanbanNumber', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('internalKanban')}
                                >
                                  <div className="flex items-center justify-center">
                                    Internal Kanban{getSortIcon(partsSortColumn === 'internalKanban', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('qpc')}
                                >
                                  <div className="flex items-center justify-center">
                                    QPC{getSortIcon(partsSortColumn === 'qpc', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('totalBoxPlanned')}
                                >
                                  <div className="flex items-center justify-center">
                                    Total Box Planned{getSortIcon(partsSortColumn === 'totalBoxPlanned', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('manifestNo')}
                                >
                                  <div className="flex items-center justify-center">
                                    Manifest No{getSortIcon(partsSortColumn === 'manifestNo', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('palletizationCode')}
                                >
                                  <div className="flex items-center justify-center">
                                    Palletization Code{getSortIcon(partsSortColumn === 'palletizationCode', partsSortDirection)}
                                  </div>
                                </th>
                                <th
                                  className="px-4 py-3 font-semibold text-gray-700 text-center whitespace-nowrap cursor-pointer hover:bg-gray-100 transition-colors"
                                  onClick={() => handlePartsSort('shortOver')}
                                >
                                  <div className="flex items-center justify-center">
                                    Short/Over{getSortIcon(partsSortColumn === 'shortOver', partsSortDirection)}
                                  </div>
                                </th>
                              </tr>
                            </thead>
                            <tbody>
                              {paginatedPlannedItems.map((item) => (
                                <tr key={item.id} className="border-b border-gray-100 hover:bg-gray-50">
                                  <td className="px-4 py-3 text-gray-700 whitespace-nowrap">
                                    {item.realOrderNumber}
                                  </td>
                                  <td className="px-4 py-3 text-gray-900 whitespace-nowrap font-medium">
                                    {item.partNumber}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {item.kanbanNumber}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {item.internalKanban || <span className="text-gray-400">-</span>}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center">
                                    {item.qpc}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center">
                                    {item.totalBoxPlanned}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center">
                                    {item.manifestNo}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center whitespace-nowrap">
                                    {item.palletizationCode}
                                  </td>
                                  <td className="px-4 py-3 text-gray-700 text-center">
                                    {item.shortOver !== null ? (
                                      <span className={item.shortOver < 0 ? 'text-red-600 font-medium' : item.shortOver > 0 ? 'text-green-600 font-medium' : 'text-gray-700'}>
                                        {item.shortOver > 0 ? '+' : ''}{item.shortOver}
                                      </span>
                                    ) : (
                                      <span className="text-gray-400">-</span>
                                    )}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        </div>

                        {/* Pagination for Planned Parts */}
                        <Pagination
                          currentPage={partsPage}
                          totalPages={partsTotalPages}
                          totalItems={filteredPlannedItems.length}
                          itemsPerPage={partsPerPage}
                          onPageChange={setPartsPage}
                          onItemsPerPageChange={setPartsPerPage}
                        />
                      </>
                    )}
                  </div>
                )}
              </div>
            </CardContent>
          </Card>

        </div>
      </div>

      {/* Upload File Side Panel */}
      <SlideOutPanel
        isOpen={isUploadPanelOpen}
        onClose={handleClosePanel}
        title="Upload New File"
        width="md"
      >
        <div className="space-y-6">
          {/* Hidden file input */}
          <input
            ref={fileInputRef}
            type="file"
            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            onChange={handleFileInputChange}
            className="hidden"
          />

          {/* Upload Result Display - Shows inside modal after upload */}
          {uploadResult && (
            <div className={`rounded-lg p-4 border-2 ${
              uploadResult.type === 'success'
                ? 'bg-green-50 border-green-500'
                : uploadResult.type === 'warning'
                ? 'bg-yellow-50 border-yellow-500'
                : 'bg-red-50 border-red-500'
            }`}>
              <div className="flex items-start gap-3">
                <i className={`fa ${
                  uploadResult.type === 'success'
                    ? 'fa-circle-check text-green-600'
                    : uploadResult.type === 'warning'
                    ? 'fa-triangle-exclamation text-yellow-600'
                    : 'fa-circle-xmark text-red-600'
                } text-2xl mt-0.5`}></i>

                <div className="flex-1">
                  <h3 className={`font-semibold mb-2 ${
                    uploadResult.type === 'success'
                      ? 'text-green-900'
                      : uploadResult.type === 'warning'
                      ? 'text-yellow-900'
                      : 'text-red-900'
                  }`}>
                    {uploadResult.type === 'success' ? 'Success!' : uploadResult.type === 'warning' ? 'Warning' : 'Error'}
                  </h3>

                  <p className={`text-sm mb-3 ${
                    uploadResult.type === 'success'
                      ? 'text-green-800'
                      : uploadResult.type === 'warning'
                      ? 'text-yellow-800'
                      : 'text-red-800'
                  }`}>
                    {uploadResult.message}
                  </p>

                  {/* Order Statistics */}
                  {(uploadResult.ordersCreated !== undefined || uploadResult.ordersSkipped !== undefined) && (
                    <div className="space-y-2 mb-3">
                      {uploadResult.ordersCreated !== undefined && uploadResult.ordersCreated > 0 && (
                        <div className="flex items-center gap-2 text-sm">
                          <i className="fa fa-check-circle text-green-600"></i>
                          <span className="font-medium text-gray-900">
                            {uploadResult.ordersCreated} {uploadResult.ordersCreated === 1 ? 'order' : 'orders'} created
                          </span>
                        </div>
                      )}

                      {uploadResult.ordersSkipped !== undefined && uploadResult.ordersSkipped > 0 && (
                        <div className="flex items-center gap-2 text-sm">
                          <i className="fa fa-info-circle text-yellow-600"></i>
                          <span className="font-medium text-gray-900">
                            {uploadResult.ordersSkipped} {uploadResult.ordersSkipped === 1 ? 'order' : 'orders'} skipped (already exists)
                          </span>
                        </div>
                      )}
                    </div>
                  )}

                  {/* Skipped Order Numbers - Collapsible */}
                  {uploadResult.skippedOrderNumbers && uploadResult.skippedOrderNumbers.length > 0 && (
                    <div className="mt-3">
                      <button
                        onClick={() => setShowSkippedDetails(!showSkippedDetails)}
                        className="flex items-center gap-2 text-sm font-medium text-gray-700 hover:text-gray-900 transition-colors"
                      >
                        <i className={`fa fa-chevron-${showSkippedDetails ? 'down' : 'right'} text-xs`}></i>
                        View skipped order numbers
                      </button>

                      {showSkippedDetails && (
                        <div className="mt-2 p-3 bg-white rounded border border-gray-200 max-h-40 overflow-y-auto">
                          <div className="text-xs text-gray-600 space-y-1">
                            {uploadResult.skippedOrderNumbers.map((orderNumber, index) => (
                              <div key={index} className="flex items-center gap-2">
                                <i className="fa fa-file text-gray-400 text-xs"></i>
                                <span className="font-mono">{orderNumber}</span>
                              </div>
                            ))}
                          </div>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}

          {/* Upload drop zone - Only show if no result yet */}
          {!uploadResult && (
            <div
              onDragEnter={handleDragEnter}
              onDragOver={handleDragOver}
              onDragLeave={handleDragLeave}
              onDrop={handleDrop}
              className={`rounded-lg p-8 text-center transition-all duration-200 ${
                isDragging
                  ? 'bg-blue-50 border-2 border-[#253262] shadow-lg'
                  : 'bg-white border-2 border-gray-200 hover:border-gray-300'
              }`}
            >
              <div className="flex flex-col items-center space-y-4">
                <div className={`w-20 h-20 rounded-full flex items-center justify-center transition-colors ${
                  isDragging ? 'bg-[#253262]' : 'bg-gray-100'
                }`}>
                  <i
                    className={`fa-light fa-upload ${isDragging ? 'text-white' : 'text-gray-500'}`}
                    style={{ fontSize: '32px' }}
                  ></i>
                </div>
                <div>
                  <p className="text-base font-semibold text-gray-800 mb-1">
                    {isDragging ? 'Drop file to upload' : 'Drag and drop Excel file here'}
                  </p>
                  {!isDragging && <p className="text-sm text-gray-500 mt-2">or</p>}
                </div>

                {/* Upload Button */}
                <Button
                  onClick={handleUploadClick}
                  disabled={isUploading}
                  loading={isUploading}
                  size="lg"
                  className="w-full"
                  style={{ backgroundColor: '#253262', color: 'white' }}
                >
                  <i className="fa-light fa-upload mr-2" style={{ fontSize: '16px' }}></i>
                  {isUploading ? 'Uploading & Processing...' : 'Choose File'}
                </Button>

                <p className="text-xs text-gray-500 mt-2">
                  Excel (.xlsx) files only - Max 10MB - TMMI Order Summary Reports
                </p>
              </div>
            </div>
          )}

          {/* Panel Actions */}
          <div className="flex justify-end gap-3 pt-4 border-t border-gray-200">
            {uploadResult ? (
              // After upload result is shown
              <>
                <Button
                  onClick={() => setUploadResult(null)}
                  variant="secondary"
                  size="md"
                >
                  Upload Another
                </Button>
                <Button
                  onClick={handleDone}
                  variant="primary"
                  size="md"
                  style={{ backgroundColor: '#253262', color: 'white' }}
                >
                  Done
                </Button>
              </>
            ) : (
              // Before upload
              <Button
                onClick={handleClosePanel}
                variant="secondary"
                size="md"
                disabled={isUploading}
              >
                Cancel
              </Button>
            )}
          </div>
        </div>
      </SlideOutPanel>
    </div>
  );
}
