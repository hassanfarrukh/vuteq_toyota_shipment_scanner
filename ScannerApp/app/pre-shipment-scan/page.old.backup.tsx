/**
 * Pre-shipment Scan Page - 5-Step Workflow with Save/Resume
 * Author: Hassan
 * Date: 2025-10-29
 *
 * WORKFLOW:
 * Step 1: Toyota Manifest Scan First
 * Step 2: Load Skid (scan skids)
 * Step 3: Driver Checklist
 * Step 4: Trailer Info
 * Step 5: Send to Toyota API
 *
 * FEATURES:
 * - Save Progress button on steps 1-4
 * - Auto-save to localStorage using Toyota Manifest ID as key
 * - Resume dialog when returning to same manifest
 * - VUTEQ branding with duotone icons
 */

'use client';

import { useState, useEffect } from 'react';
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

type PreShipmentStep = 'toyota-manifest' | 'load-skids' | 'driver-check' | 'trailer-info' | 'complete';

// Step configuration for the stepper (5 steps)
const PRE_SHIPMENT_STEPS = [
  { label: 'Toyota Manifest', description: 'Scan Toyota label' },
  { label: 'Load Skids', description: 'Scan skids' },
  { label: 'Driver Check', description: 'Scan check sheet' },
  { label: 'Trailer Info', description: 'Enter details' },
  { label: 'Complete', description: 'Submit to Toyota' },
];

// LocalStorage key prefix
const STORAGE_KEY_PREFIX = 'pre_shipment_';

// Saved state interface
interface SavedState {
  toyotaManifestId: string;
  scannedSkids: string[];
  driverCheck: DriverCheckSheet | null;
  trailerInfo: TrailerInfo;
  currentStep: PreShipmentStep;
  savedAt: string;
}

export default function PreShipmentScanPage() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState<PreShipmentStep>('toyota-manifest');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showResumeDialog, setShowResumeDialog] = useState(false);
  const [savedState, setSavedState] = useState<SavedState | null>(null);

  // Helper function to convert step name to step number (1-indexed)
  const getStepNumber = (step: PreShipmentStep): number => {
    const stepMap: Record<PreShipmentStep, number> = {
      'toyota-manifest': 1,
      'load-skids': 2,
      'driver-check': 3,
      'trailer-info': 4,
      'complete': 5,
    };
    return stepMap[step];
  };

  // Step data
  const [toyotaManifestId, setToyotaManifestId] = useState<string | null>(null);
  const [scannedSkids, setScannedSkids] = useState<string[]>([]);
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

  // Save current state to localStorage
  const saveProgress = () => {
    if (!toyotaManifestId) {
      setError('Cannot save: No Toyota Manifest scanned');
      return;
    }

    const state: SavedState = {
      toyotaManifestId,
      scannedSkids,
      driverCheck,
      trailerInfo,
      currentStep,
      savedAt: new Date().toISOString(),
    };

    localStorage.setItem(`${STORAGE_KEY_PREFIX}${toyotaManifestId}`, JSON.stringify(state));

    // Show success feedback
    const originalError = error;
    setError(null);
    setTimeout(() => {
      setError('Progress saved successfully!');
      setTimeout(() => setError(originalError), 2000);
    }, 100);
  };

  // Load saved state from localStorage
  const loadSavedState = (manifest: string): SavedState | null => {
    const saved = localStorage.getItem(`${STORAGE_KEY_PREFIX}${manifest}`);
    if (saved) {
      try {
        return JSON.parse(saved) as SavedState;
      } catch (e) {
        console.error('Failed to parse saved state:', e);
        return null;
      }
    }
    return null;
  };

  // Resume from saved state
  const resumeSavedState = () => {
    if (savedState) {
      setToyotaManifestId(savedState.toyotaManifestId);
      setScannedSkids(savedState.scannedSkids);
      setDriverCheck(savedState.driverCheck);
      setTrailerInfo(savedState.trailerInfo);
      setCurrentStep(savedState.currentStep);
      setShowResumeDialog(false);
      setSavedState(null);
    }
  };

  // Start fresh (ignore saved state)
  const startFresh = () => {
    if (savedState && toyotaManifestId) {
      // Clear saved state from localStorage
      localStorage.removeItem(`${STORAGE_KEY_PREFIX}${toyotaManifestId}`);
    }
    setShowResumeDialog(false);
    setSavedState(null);
    // Keep toyotaManifestId but reset to step 2
    setCurrentStep('load-skids');
  };

  // Step 1: Handle Toyota Manifest scan
  const handleToyotaManifestScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    setLoading(true);
    setError(null);

    // Check if there's saved progress for this manifest
    const saved = loadSavedState(result.scannedValue);

    if (saved) {
      // Found saved progress - show resume dialog
      setSavedState(saved);
      setToyotaManifestId(result.scannedValue);
      setShowResumeDialog(true);
    } else {
      // No saved progress - proceed to step 2
      setToyotaManifestId(result.scannedValue);
      setCurrentStep('load-skids');
    }

    setLoading(false);
  };

  // Step 2: Handle skid scanning
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

  // Step 2 to Step 3: Proceed to Driver Check
  const proceedToDriverCheck = () => {
    if (scannedSkids.length === 0) {
      setError('At least one skid must be scanned');
      return;
    }
    setError(null);
    setCurrentStep('driver-check');
  };

  // Step 3: Handle Driver Check Sheet scan
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

  // Step 4: Handle trailer info form submission
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

    // Create shipment with all collected data
    const response = await createShipment(trailerInfo, driverCheck.id);

    if (response.success && response.data) {
      setShipment(response.data);
      setCurrentStep('complete');
    } else {
      setError(response.error || 'Failed to create shipment');
    }

    setLoading(false);
  };

  // Step 5: Submit shipment to Toyota API
  const handleSubmitToToyota = async () => {
    if (!shipment) {
      setError('No shipment data available');
      return;
    }

    setLoading(true);
    setError(null);

    const response = await submitToToyota(shipment.id);

    if (response.success) {
      // Clear saved state from localStorage
      if (toyotaManifestId) {
        localStorage.removeItem(`${STORAGE_KEY_PREFIX}${toyotaManifestId}`);
      }
      // Success - stay on complete step
    } else {
      setError(response.error || 'Failed to submit to Toyota API');
    }

    setLoading(false);
  };

  // Reset and start over
  const handleReset = () => {
    // Clear saved state if exists
    if (toyotaManifestId) {
      localStorage.removeItem(`${STORAGE_KEY_PREFIX}${toyotaManifestId}`);
    }

    setCurrentStep('toyota-manifest');
    setToyotaManifestId(null);
    setScannedSkids([]);
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
    setError(null);
    setSavedState(null);
    setShowResumeDialog(false);
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-4 pt-24 max-w-3xl mx-auto space-y-3">
          {/* Header with Stepper */}
          <Card style={{ backgroundColor: '#FCFCFC' }}>
            <CardContent className="p-3">
              <ResponsiveStepper
                steps={PRE_SHIPMENT_STEPS}
                currentStep={getStepNumber(currentStep)}
              />
            </CardContent>
          </Card>

          {/* Resume Dialog */}
          {showResumeDialog && savedState && (
            <Card style={{ backgroundColor: '#FCFCFC' }}>
              <CardContent className="p-4 space-y-3">
                <div className="flex items-center gap-3">
                  <i className="fa-light fa-clock-rotate-left text-3xl" style={{
                    color: '#253262'
                  } as React.CSSProperties}></i>
                  <div>
                    <h3 className="text-lg font-semibold" style={{ color: '#253262' }}>
                      Resume Previous Scan?
                    </h3>
                    <p className="text-sm text-gray-600">
                      Found saved progress from {new Date(savedState.savedAt).toLocaleString()}
                    </p>
                  </div>
                </div>

                <div className="p-3 bg-gray-50 border border-gray-200 rounded-lg text-sm space-y-1">
                  <p><strong>Toyota Manifest:</strong> {savedState.toyotaManifestId}</p>
                  <p><strong>Skids Scanned:</strong> {savedState.scannedSkids.length}</p>
                  <p><strong>Last Step:</strong> {PRE_SHIPMENT_STEPS[getStepNumber(savedState.currentStep) - 1].label}</p>
                </div>

                <div className="flex gap-2">
                  <Button
                    onClick={resumeSavedState}
                    variant="success-light"
                    fullWidth
                  >
                    <i className="fa-light fa-play mr-2"></i>
                    Resume
                  </Button>
                  <Button
                    onClick={startFresh}
                    variant="error"
                    fullWidth
                  >
                    <i className="fa-light fa-xmark mr-2"></i>
                    Start Fresh
                  </Button>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Error Alert */}
          {error && !error.includes('saved successfully') && (
            <Alert variant="error" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          {/* Success Alert for Save */}
          {error && error.includes('saved successfully') && (
            <Alert variant="success" onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          {/* Step 1: Scan Toyota Manifest */}
          {currentStep === 'toyota-manifest' && !showResumeDialog && (
            <Card style={{ backgroundColor: '#FCFCFC' }}>
              <CardContent className="p-3 space-y-3">
                <Alert variant="info" title="Start Pre-shipment Scan">
                  Scan the Toyota Manifest label to begin
                </Alert>
                <Scanner
                  onScan={handleToyotaManifestScan}
                  expectedType="SKID_MANIFEST"
                  label="Scan Toyota Manifest"
                  placeholder="TL-12345678"
                  disabled={loading}
                />
              </CardContent>
            </Card>
          )}

          {/* Step 2: Load Skids */}
          {currentStep === 'load-skids' && !showResumeDialog && (
            <Card style={{ backgroundColor: '#FCFCFC' }}>
              <CardContent className="space-y-3 p-3">
                <div className="p-4 bg-primary-50 border border-primary-200 rounded-lg">
                  <p className="text-sm font-medium" style={{ color: '#253262' }}>
                    Toyota Manifest: {toyotaManifestId}
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
                  <div className="flex gap-2">
                    <Button
                      onClick={saveProgress}
                      variant="success-light"
                      fullWidth
                    >
                      <i className="fa-light fa-floppy-disk mr-2"></i>
                      Save Progress
                    </Button>
                    <Button
                      onClick={proceedToDriverCheck}
                      variant="success-light"
                      fullWidth
                    >
                      <i className="fa-light fa-arrow-right mr-2"></i>
                      Continue ({scannedSkids.length} skids)
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* Step 3: Scan Driver Check Sheet */}
          {currentStep === 'driver-check' && !showResumeDialog && (
            <Card style={{ backgroundColor: '#FCFCFC' }}>
              <CardContent className="space-y-3 p-3">
                <Alert variant="success">
                  <div className="text-sm">
                    <strong>Skids Loaded:</strong> {scannedSkids.length}
                  </div>
                </Alert>

                <Alert variant="warning" title="Driver Check Required">
                  Scan the Driver Check Sheet before entering trailer info
                </Alert>

                <Scanner
                  onScan={handleDriverCheckScan}
                  expectedType="DRIVER_CHECK"
                  label="Scan Driver Check Sheet"
                  placeholder="DCS-123456"
                  disabled={loading}
                />

                <Button
                  onClick={saveProgress}
                  variant="success-light"
                  fullWidth
                >
                  <i className="fa-light fa-floppy-disk mr-2"></i>
                  Save Progress
                </Button>
              </CardContent>
            </Card>
          )}

          {/* Step 4: Enter Trailer Information */}
          {currentStep === 'trailer-info' && driverCheck && !showResumeDialog && (
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

                  <div className="flex gap-2">
                    <Button
                      onClick={saveProgress}
                      type="button"
                      variant="success-light"
                      fullWidth
                    >
                      <i className="fa-light fa-floppy-disk mr-2"></i>
                      Save
                    </Button>
                    <Button
                      type="submit"
                      variant="success"
                      fullWidth
                      loading={loading}
                    >
                      <i className="fa-light fa-check-circle mr-2"></i>
                      Complete Scan
                    </Button>
                  </div>
                </form>
              </CardContent>
            </Card>
          )}

          {/* Step 5: Complete - Submit to Toyota */}
          {currentStep === 'complete' && shipment && (
            <Card style={{ backgroundColor: '#FCFCFC' }}>
              <CardContent className="space-y-3 p-3">
                <Alert variant="success" title="Ready to Submit">
                  Pre-shipment scan completed. Submit to Toyota API.
                </Alert>

                <div className="p-4 bg-gray-50 border border-gray-200 rounded-lg space-y-2">
                  <p className="font-semibold" style={{ color: '#253262' }}>Shipment Summary:</p>
                  <p className="text-sm">Shipment Number: {shipment.shipmentNumber}</p>
                  <p className="text-sm">Toyota Manifest: {toyotaManifestId}</p>
                  <p className="text-sm">Trailer: {shipment.trailerInfo.trailerNumber}</p>
                  <p className="text-sm">Total Skids: {scannedSkids.length}</p>
                  <p className="text-sm">Driver: {driverCheck?.driverName}</p>
                </div>

                <Button
                  onClick={handleSubmitToToyota}
                  variant="success"
                  fullWidth
                  loading={loading}
                >
                  <i className="fa-light fa-paper-plane mr-2"></i>
                  Submit to Toyota API
                </Button>

                {!loading && (
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
                      New Scan
                    </Button>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          {/* Action Buttons */}
          {currentStep !== 'complete' && !showResumeDialog && (
            <div className="flex gap-2">
              <Button
                onClick={() => router.push('/')}
                variant="error"
                fullWidth
              >
                <i className="fa-light fa-xmark mr-2"></i>
                Cancel
              </Button>
              {currentStep !== 'toyota-manifest' && (
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
