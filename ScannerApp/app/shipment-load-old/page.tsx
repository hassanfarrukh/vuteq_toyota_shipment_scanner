/**
 * Pre-shipment Scan Page
 * Author: Hassan
 * Date: 2025-10-29 (Removed Toyota Manifest scan - changed from 5-step to 4-step workflow)
 * Previous Author: Hassan
 * Previous Date: 2025-10-21 (Updated with new Stepper component)
 * Updated: 2025-10-21 - Applied VUTEQ brand colors (Navy #253262, Red #D2312E, Off-white #FCFCFC)
 * Updated: 2025-10-21 - Added VUTEQ static gradient background
 * Updated: 2025-10-22 - Mobile-only: Removed page headings, step titles, and scroll behavior
 * Updated: 2025-10-27 - Mobile no-scroll optimization: compact spacing, flex layout, overflow management
 * Updated: 2025-10-27 - Fixed button colors: "Start Over"/"New Shipment" now use Navy Blue (primary), "Cancel" uses Red (error)
 * Updated: 2025-10-28 - Reverted desktop styling optimizations to simpler mobile-focused design (Hassan)
 * Updated: 2025-10-28 - Fixed header visibility: Changed pt-20 to pt-24 to prevent header overlap with fixed layout (Hassan)
 * Updated: 2025-10-28 - Fixed Font Awesome icon classes: changed fa-solid to fa for remove (xmark) icon
 * Updated: 2025-10-29 - Changed fa to fa-solid for all icons (solid/bold style) by Hassan
 * Updated: 2025-10-29 - Updated xmark icon to fa-duotone fa-light with VUTEQ colors (Navy #253262, Red #D2312E) by Hassan
 * Updated: 2025-10-29 - Fixed Step 2 scrolling issue: Reduced text sizes (text-xs), spacing (space-y-2), and margins in Alert and form (Hassan)
 * Updated: 2025-10-29 - Reduced text input sizes in step 2: h-9 height, text-sm size, p-2 padding, space-y-1.5 between fields (Hassan)
 * Updated: 2025-10-29 - NEW 4-STEP WORKFLOW: Step 1: Driver Check, Step 2: Trailer Info, Step 3: Load Skids, Step 4: Submit to Toyota (Hassan)
 *
 * MANDATORY: Scan Driver Check Sheet FIRST (BR-001)
 * Then enter trailer info and scan skids
 */

'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Scanner from '@/components/ui/Scanner';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Alert from '@/components/ui/Alert';
import Badge from '@/components/ui/Badge';
import { ResponsiveStepper } from '@/components/ui/Stepper';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import {
  scanDriverCheckSheet,
  createShipment,
  submitToToyota,
} from '@/lib/api';
import { ERROR_MESSAGES, SUCCESS_MESSAGES } from '@/lib/constants';
import { formatDateTime } from '@/lib/utils';
import type { ScanResult, DriverCheckSheet, TrailerInfo, ShipmentLoad } from '@/types';

type LoadStep = 'driver-check' | 'trailer-info' | 'loading' | 'complete';

// Step configuration for the stepper (4 steps)
const SHIPMENT_LOAD_STEPS = [
  { label: 'Driver Check', description: 'Scan check sheet' },
  { label: 'Trailer Info', description: 'Enter details' },
  { label: 'Load Skids', description: 'Scan skids' },
  { label: 'Complete', description: 'Submit to Toyota' },
];

export default function ShipmentLoadPage() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState<LoadStep>('driver-check');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Helper function to convert step name to step number (1-indexed)
  const getStepNumber = (step: LoadStep): number => {
    const stepMap: Record<LoadStep, number> = {
      'driver-check': 1,
      'trailer-info': 2,
      'loading': 3,
      'complete': 4,
    };
    return stepMap[step];
  };

  // Step data
  const [driverCheck, setDriverCheck] = useState<DriverCheckSheet | null>(null);
  const [trailerInfo, setTrailerInfo] = useState<TrailerInfo>({
    trailerNumber: '',
    sealNumber: '',
    carrierName: '',
    driverName: '',
    driverLicense: '',
    expectedDeparture: '',
  });
  const [shipment, setShipment] = useState<ShipmentLoad | null>(null);
  const [scannedSkids, setScannedSkids] = useState<string[]>([]);

  // BR-001: Driver Check Sheet MUST be scanned FIRST
  const handleDriverCheckScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    setLoading(true);
    setError(null);

    const response = await scanDriverCheckSheet(result.scannedValue);

    if (response.success && response.data) {
      setDriverCheck(response.data);
      // Pre-fill trailer info from driver check
      setTrailerInfo((prev) => ({
        ...prev,
        trailerNumber: response.data.trailerNumber,
        driverName: response.data.driverName,
        driverLicense: response.data.driverLicense,
      }));
      setCurrentStep('trailer-info');
    } else {
      setError(response.error || 'Failed to scan Driver Check Sheet');
    }

    setLoading(false);
  };

  // Handle trailer info form submission
  const handleTrailerInfoSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!driverCheck) {
      setError(ERROR_MESSAGES.DRIVER_CHECK_REQUIRED);
      return;
    }

    // Validate required fields
    if (
      !trailerInfo.trailerNumber ||
      !trailerInfo.sealNumber ||
      !trailerInfo.carrierName ||
      !trailerInfo.expectedDeparture
    ) {
      setError('All trailer information fields are required');
      return;
    }

    setLoading(true);
    setError(null);

    // Create shipment and move to loading step
    const response = await createShipment(trailerInfo, driverCheck.id);

    if (response.success && response.data) {
      setShipment(response.data);
      setCurrentStep('loading');
    } else {
      setError(response.error || 'Failed to create shipment');
    }

    setLoading(false);
  };

  // Handle skid scanning
  const handleSkidScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    // Check for duplicate
    if (scannedSkids.includes(result.scannedValue)) {
      setError('This skid has already been scanned');
      return;
    }

    setScannedSkids((prev) => [...prev, result.scannedValue]);
    setError(null);
  };

  // Remove scanned skid
  const handleRemoveSkid = (skidToRemove: string) => {
    setScannedSkids((prev) => prev.filter((skid) => skid !== skidToRemove));
  };

  // Submit shipment to Toyota API
  const handleSubmitToToyota = async () => {
    if (!shipment || scannedSkids.length === 0) {
      setError('No skids have been scanned');
      return;
    }

    setLoading(true);
    setError(null);

    const response = await submitToToyota(shipment.id);

    if (response.success) {
      setCurrentStep('complete');
    } else {
      setError(response.error || 'Failed to submit to Toyota API');
    }

    setLoading(false);
  };

  // Reset and start over
  const handleReset = () => {
    setCurrentStep('driver-check');
    setDriverCheck(null);
    setTrailerInfo({
      trailerNumber: '',
      sealNumber: '',
      carrierName: '',
      driverName: '',
      driverLicense: '',
      expectedDeparture: '',
    });
    setShipment(null);
    setScannedSkids([]);
    setError(null);
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-4 pt-24 max-w-3xl mx-auto space-y-3">
        {/* Header with New Stepper */}
        <Card style={{ backgroundColor: '#FCFCFC' }}>
        <CardContent className="p-3">
          <ResponsiveStepper
            steps={SHIPMENT_LOAD_STEPS}
            currentStep={getStepNumber(currentStep)}
          />
        </CardContent>
      </Card>

      {/* BR-001 Warning */}
      {currentStep === 'driver-check' && (
        <Alert variant="warning" title="MANDATORY REQUIREMENT">
          Driver Check Sheet MUST be scanned FIRST before loading
        </Alert>
      )}

      {/* Error Alert */}
      {error && (
        <Alert variant="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Step 1: Scan Driver Check Sheet */}
      {currentStep === 'driver-check' && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="p-3">
            <Scanner
              onScan={handleDriverCheckScan}
              expectedType="DRIVER_CHECK"
              label="Scan Driver Check Sheet"
              placeholder="DCS-123456"
              disabled={loading}
            />
          </CardContent>
        </Card>
      )}

      {/* Step 2: Enter Trailer Information */}
      {currentStep === 'trailer-info' && driverCheck && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="space-y-2 p-3">
            <Alert variant="success">
              <div className="text-xs font-medium">
                <strong>Driver Check Verified:</strong> {driverCheck.checkSheetNumber}
              </div>
              <div className="text-xs text-gray-600 mt-0.5">
                Driver: {driverCheck.driverName} | License: {driverCheck.driverLicense}
              </div>
            </Alert>

            <form onSubmit={handleTrailerInfoSubmit} className="space-y-2">
              <div className="space-y-1.5">
                <Input
                  label="Trailer Number"
                  value={trailerInfo.trailerNumber}
                  onChange={(e) =>
                    setTrailerInfo((prev) => ({
                      ...prev,
                      trailerNumber: e.target.value,
                    }))
                  }
                  required
                  fullWidth
                  style={{ backgroundColor: '#F5F7F9' }}
                  className="h-9 text-sm p-2"
                />
              </div>

              <div className="space-y-1.5">
                <Input
                  label="Seal Number"
                  value={trailerInfo.sealNumber}
                  onChange={(e) =>
                    setTrailerInfo((prev) => ({
                      ...prev,
                      sealNumber: e.target.value,
                    }))
                  }
                  required
                  fullWidth
                  style={{ backgroundColor: '#F5F7F9' }}
                  className="h-9 text-sm p-2"
                />
              </div>

              <div className="space-y-1.5">
                <Input
                  label="Carrier Name"
                  value={trailerInfo.carrierName}
                  onChange={(e) =>
                    setTrailerInfo((prev) => ({
                      ...prev,
                      carrierName: e.target.value,
                    }))
                  }
                  required
                  fullWidth
                  style={{ backgroundColor: '#F5F7F9' }}
                  className="h-9 text-sm p-2"
                />
              </div>

              <div className="space-y-1.5">
                <Input
                  label="Driver Name"
                  value={trailerInfo.driverName}
                  onChange={(e) =>
                    setTrailerInfo((prev) => ({
                      ...prev,
                      driverName: e.target.value,
                    }))
                  }
                  required
                  fullWidth
                  style={{ backgroundColor: '#F5F7F9' }}
                  className="h-9 text-sm p-2"
                />
              </div>

              <div className="space-y-1.5">
                <Input
                  label="Driver License"
                  value={trailerInfo.driverLicense}
                  onChange={(e) =>
                    setTrailerInfo((prev) => ({
                      ...prev,
                      driverLicense: e.target.value,
                    }))
                  }
                  required
                  fullWidth
                  style={{ backgroundColor: '#F5F7F9' }}
                  className="h-9 text-sm p-2"
                />
              </div>

              <div className="space-y-1.5">
                <Input
                  label="Expected Departure"
                  type="datetime-local"
                  value={trailerInfo.expectedDeparture}
                  onChange={(e) =>
                    setTrailerInfo((prev) => ({
                      ...prev,
                      expectedDeparture: e.target.value,
                    }))
                  }
                  required
                  fullWidth
                  style={{ backgroundColor: '#F5F7F9' }}
                  className="h-9 text-sm p-2"
                />
              </div>

              <Button
                type="submit"
                variant="success-light"
                fullWidth
                loading={loading}
              >
                <i className="fa-light fa-arrow-right mr-2"></i>
                Continue to Loading
              </Button>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Step 3: Load Skids */}
      {currentStep === 'loading' && shipment && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="space-y-3 p-3">
            <div className="p-4 bg-primary-50 border border-primary-200 rounded-lg">
              <p className="text-sm font-medium" style={{ color: '#253262' }}>Shipment: {shipment.shipmentNumber}</p>
              <p className="text-sm text-gray-600">
                Trailer: {shipment.trailerInfo.trailerNumber} | Seal:{' '}
                {shipment.trailerInfo.sealNumber}
              </p>
              <p className="text-sm text-gray-600">
                Carrier: {shipment.trailerInfo.carrierName}
              </p>
            </div>

            {scannedSkids.length > 0 && (
              <div className="space-y-2">
                <div className="flex items-center justify-between">
                  <p className="text-sm font-medium" style={{ color: '#253262' }}>
                    Loaded Skids ({scannedSkids.length}):
                  </p>
                </div>
                <div className="space-y-2">
                  {scannedSkids.map((skid, idx) => (
                    <div
                      key={idx}
                      className="p-3 bg-success-50 border border-success-200 rounded-lg flex items-center justify-between"
                    >
                      <div>
                        <span className="font-mono text-sm">{skid}</span>
                        <p className="text-xs text-gray-600">#{idx + 1}</p>
                      </div>
                      <button
                        onClick={() => handleRemoveSkid(skid)}
                        className="text-error-600 hover:text-error-800 p-2"
                        aria-label="Remove skid"
                      >
                        <i className="fa-light fa-xmark" style={{
                          fontSize: '20px',
                          color: '#253262'
                        } as React.CSSProperties}></i>
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            )}

            <Scanner
              onScan={handleSkidScan}
              expectedType="SKID_MANIFEST"
              label="Scan Skid"
              placeholder="TL-12345678"
              disabled={loading}
            />

            {scannedSkids.length > 0 && (
              <Button
                onClick={handleSubmitToToyota}
                variant="success"
                fullWidth
                loading={loading}
              >
                <i className="fa-light fa-paper-plane mr-2"></i>
                Submit to Toyota ({scannedSkids.length} skids)
              </Button>
            )}
          </CardContent>
        </Card>
      )}

      {/* Step 4: Complete - Submit to Toyota */}
      {currentStep === 'complete' && shipment && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="space-y-3 p-3">
            <Alert variant="success" title="Success!">
              Shipment has been submitted to Toyota API successfully.
            </Alert>

            <div className="p-4 bg-gray-50 border border-gray-200 rounded-lg space-y-2">
              <p className="font-semibold" style={{ color: '#253262' }}>Shipment Summary:</p>
              <p className="text-sm">Shipment Number: {shipment.shipmentNumber}</p>
              <p className="text-sm">
                Trailer: {shipment.trailerInfo.trailerNumber}
              </p>
              <p className="text-sm">Total Skids: {scannedSkids.length}</p>
              <p className="text-sm">Submitted: {formatDateTime(new Date())}</p>
            </div>

            <div className="flex gap-2">
              <Button
                onClick={() => router.push('/')}
                variant="primary"
                fullWidth
              >
                <i className="fa-light fa-home mr-2"></i>
                Return to Dashboard
              </Button>
              <Button
                onClick={handleReset}
                variant="primary"
                fullWidth
              >
                <i className="fa-light fa-plus mr-2"></i>
                New Shipment
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      {/* Action Buttons */}
      {currentStep !== 'complete' && (
        <div className="flex gap-2">
          <Button
            onClick={() => router.push('/')}
            variant="error"
            fullWidth
          >
            <i className="fa-light fa-xmark mr-2"></i>
            Cancel
          </Button>
          {currentStep !== 'driver-check' && (
            <Button
              onClick={handleReset}
              variant="primary"
              fullWidth
            >
              <i className="fa-light fa-rotate-right mr-2"></i>
              Start Over
            </Button>
          )}
        </div>
      )}
        </div>
      </div>
    </div>
  );
}
