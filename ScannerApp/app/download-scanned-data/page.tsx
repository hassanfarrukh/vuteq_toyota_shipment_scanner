/**
 * Download Scanned Data Page
 * Author: Hassan
 * Date: 2025-10-28
 * Updated: 2025-10-28 - Fixed Font Awesome icon classes: changed fa-solid to fa for download button
 * Updated: 2025-10-29 - Changed fa to fa-solid for all icons (solid/bold style) by Hassan
 * Updated: 2025-10-29 - Applied standardized button designs with Font Awesome 6 Light icons by Hassan
 *   - Download Button: success variant (Dark Green #10B981) with fa-light fa-download
 *   - Reset Filters Button: primary variant (Navy Blue #253262) with fa-light fa-rotate-right
 *   - Back to Dashboard Button: primary variant (Navy Blue #253262) with fa-light fa-home
 * Updated: 2025-11-05 - Changed description text to "Filter options" by Hassan
 * Updated: 2025-11-05 - Updated page header to match standard style with icon and description by Hassan
 *   - Added fa-download icon in navy (#253262) at text-2xl
 *   - Added title "Filter options" in text-lg font-semibold
 *   - Added description "Export ItemConfirmationHistory with following custom filters"
 *   - Added border-bottom separator with proper spacing
 *   - Changed Card background to #FCFCFC to match other pages
 *
 * Desktop-optimized design for Supervisor/Admin users
 * Allows supervisors and admins to download ItemConfirmationHistory.xls
 * with filters for date range, company, and warehouse.
 * Phase 1: Using dummy data, console.log download action
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

// Dummy data for dropdowns
const COMPANIES = [
  { id: 'comp-001', name: 'VUTEQ USA' },
  { id: 'comp-002', name: 'Toyota Motor Manufacturing' },
  { id: 'comp-003', name: 'Supplier Inc.' },
];

const WAREHOUSES = [
  { id: 'wh-001', name: 'Indiana Warehouse' },
  { id: 'wh-002', name: 'Michigan Warehouse' },
  { id: 'wh-003', name: 'Ohio Warehouse' },
  { id: 'wh-004', name: 'Kentucky Warehouse' },
];

export default function DownloadScannedDataPage() {
  const router = useRouter();
  const { user } = useAuth();

  // State for filters
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [selectedCompany, setSelectedCompany] = useState('');
  const [selectedWarehouse, setSelectedWarehouse] = useState('');
  const [isDownloading, setIsDownloading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  // Redirect if not supervisor or admin - moved after all hooks to comply with React Rules of Hooks
  if (user && user.role !== 'SUPERVISOR' && user.role !== 'ADMIN') {
    router.push('/');
    return null;
  }

  const handleDownload = async () => {
    setError(null);
    setSuccess(false);

    // Validation
    if (!startDate) {
      setError('Please select a start date');
      return;
    }

    if (!endDate) {
      setError('Please select an end date');
      return;
    }

    if (new Date(startDate) > new Date(endDate)) {
      setError('Start date must be before end date');
      return;
    }

    if (!selectedCompany) {
      setError('Please select a company');
      return;
    }

    if (!selectedWarehouse) {
      setError('Please select a warehouse');
      return;
    }

    setIsDownloading(true);

    // Phase 1: Console log the download parameters
    console.log('Download ItemConfirmationHistory.xls with filters:', {
      startDate,
      endDate,
      company: selectedCompany,
      warehouse: selectedWarehouse,
      user: user?.username,
    });

    // Simulate API call
    await new Promise(resolve => setTimeout(resolve, 1500));

    // Phase 2 will implement actual file download
    // For now, just show success message
    setIsDownloading(false);
    setSuccess(true);

    // Clear success message after 3 seconds
    setTimeout(() => setSuccess(false), 3000);
  };

  const handleReset = () => {
    setStartDate('');
    setEndDate('');
    setSelectedCompany('');
    setSelectedWarehouse('');
    setError(null);
    setSuccess(false);
  };

  return (
    <div className="relative min-h-screen">
      {/* VUTEQ Static Background */}
      <VUTEQStaticBackground />

      <div className="relative">
        <div className="p-8 max-w-6xl mx-auto space-y-6">

        {/* Success Alert */}
        {success && (
          <Alert variant="success" onClose={() => setSuccess(false)}>
            Download initiated successfully! Check console for details.
          </Alert>
        )}

        {/* Error Alert */}
        {error && (
          <Alert variant="error" onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

        {/* Filters Card */}
        <Card className="bg-[#FCFCFC]">
          <CardContent className="p-3 space-y-4">
            {/* Header with Icon */}
            <div className="flex items-center gap-3 pb-3 border-b border-gray-200">
              <i className="fa fa-download text-2xl" style={{ color: '#253262' }}></i>
              <div>
                <h1 className="text-lg font-semibold" style={{ color: '#253262' }}>
                  Filter options
                </h1>
                <p className="text-sm text-gray-600">
                  Export ItemConfirmationHistory with following custom filters
                </p>
              </div>
            </div>

            {/* Filters Section */}
            <div className="space-y-4">
              {/* Date Range */}
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label htmlFor="start-date" className="block text-base font-medium text-gray-700 mb-2">
                    Start Date
                  </label>
                  <Input
                    id="start-date"
                    type="date"
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    placeholder="Select start date"
                    className="text-base py-3"
                  />
                </div>

                <div>
                  <label htmlFor="end-date" className="block text-base font-medium text-gray-700 mb-2">
                    End Date
                  </label>
                  <Input
                    id="end-date"
                    type="date"
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
                    placeholder="Select end date"
                    className="text-base py-3"
                  />
                </div>
              </div>

              {/* Company Dropdown */}
              <div>
                <label htmlFor="company-select" className="block text-base font-medium text-gray-700 mb-2">
                  Company
                </label>
                <select
                  id="company-select"
                  value={selectedCompany}
                  onChange={(e) => setSelectedCompany(e.target.value)}
                  className="w-full px-4 py-3 text-base border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                >
                  <option value="">Select a company</option>
                  {COMPANIES.map((company) => (
                    <option key={company.id} value={company.id}>
                      {company.name}
                    </option>
                  ))}
                </select>
              </div>

              {/* Warehouse Dropdown */}
              <div>
                <label htmlFor="warehouse-select" className="block text-base font-medium text-gray-700 mb-2">
                  Warehouse
                </label>
                <select
                  id="warehouse-select"
                  value={selectedWarehouse}
                  onChange={(e) => setSelectedWarehouse(e.target.value)}
                  className="w-full px-4 py-3 text-base border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-transparent"
                >
                  <option value="">Select a warehouse</option>
                  {WAREHOUSES.map((warehouse) => (
                    <option key={warehouse.id} value={warehouse.id}>
                      {warehouse.name}
                    </option>
                  ))}
                </select>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Action Buttons */}
        <div className="flex flex-col sm:flex-row gap-3">
          {/* Download Button (Green - Primary Action) */}
          <Button
            onClick={handleDownload}
            disabled={isDownloading}
            loading={isDownloading}
            variant="success"
            size="lg"
            className="w-full sm:flex-1"
          >
            <i className="fa-light fa-download mr-2" style={{ fontSize: '20px' }}></i>
            {isDownloading ? 'Preparing Download...' : 'Download'}
          </Button>

          {/* Reset Filters Button (Navy Blue - Refresh/Reset) */}
          <Button
            onClick={handleReset}
            variant="primary"
            size="lg"
            disabled={isDownloading}
            className="w-full sm:w-auto"
          >
            <i className="fa-light fa-rotate-right mr-2" style={{ fontSize: '18px' }}></i>
            Reset Filters
          </Button>

          {/* Back to Dashboard Button (Navy Blue - Navigation) */}
          <Button
            onClick={() => router.push('/')}
            variant="primary"
            size="lg"
            disabled={isDownloading}
            className="w-full sm:w-auto"
          >
            <i className="fa-light fa-home mr-2" style={{ fontSize: '18px' }}></i>
            Return to Dashboard
          </Button>
        </div>
        </div>
      </div>
    </div>
  );
}
