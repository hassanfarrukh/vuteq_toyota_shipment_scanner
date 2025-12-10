/**
 * Skid Build Page - 5-Step Workflow
 * Author: Hassan
 * Date: 2025-10-21 (Updated with new Stepper component)
 * Updated: 2025-10-21 - Applied VUTEQ brand colors (Navy #253262, Red #D2312E, Off-white #FCFCFC)
 * Updated: 2025-10-21 - Added VUTEQ static gradient background
 * Updated: 2025-10-22 - Mobile-only: Removed page headings and step titles
 * Updated: 2025-10-27 - Mobile no-scroll optimization: compact spacing, flex layout, overflow management
 * Updated: 2025-10-27 - Fixed button colors: "Start Over" now uses Navy Blue (primary), "Cancel" uses Red (error)
 * Updated: 2025-10-27 - Removed Indiana-only restriction: Serial scanning now required for ALL locations
 * Updated: 2025-10-28 - CRITICAL FIX: Implemented 1:1 Toyota-Internal Kanban pairing workflow matching Scott's VB program
 * Updated: 2025-10-28 - Reverted desktop styling optimizations to simpler mobile-focused design (Hassan)
 * Updated: 2025-10-28 - Fixed header visibility: Changed pt-20 to pt-24 to prevent header overlap with fixed layout (Hassan)
 * Updated: 2025-10-30 - Added exception handling: Validates planned vs actual quantity, requires exceptions if mismatch (Hassan)
 *
 * Step 1: Search/Scan Order
 * Step 2: Scan Skid Manifest (Toyota Label)
 * Step 3: Scan Toyota Kanban #1 → Step 4: Internal Kanban #1 → Step 5: Serial #1
 * Loop: "Add Another Item?" → Repeat for next Toyota-Internal-Serial triplet
 *
 * WORKFLOW: Toyota Kanban → Internal Kanban → Serial → "Add Another?" (1:1:1 relationship)
 */

'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Scanner from '@/components/ui/Scanner';
import Button from '@/components/ui/Button';
import Card, { CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import Alert from '@/components/ui/Alert';
import Badge from '@/components/ui/Badge';
import { ResponsiveStepper } from '@/components/ui/Stepper';
import VUTEQStaticBackground from '@/components/layout/VUTEQStaticBackground';
import {
  searchOrder,
  scanSkidManifest,
  scanToyotaKanban,
  scanInternalKanban,
  scanSerialNumber,
  completeSkidBuild,
} from '@/lib/api';
import { generateSessionId } from '@/lib/utils';
import { SUCCESS_MESSAGES, ERROR_MESSAGES } from '@/lib/constants';
import type { ScanResult, Order, ToyotaKanban, InternalKanban, SerialNumber } from '@/types';

type Step = 1 | 2 | 3 | 4 | 5;

// Exception types configuration
const EXCEPTION_TYPES = [
  'Material Shortage',
  'Quality Issue',
  'Wrong Part Delivered',
  'Damaged Parts',
  'Production Delay',
  'System Error',
  'Other',
];

interface Exception {
  type: string;
  comments: string;
  timestamp: string;
}

// Step configuration for the stepper
const SKID_BUILD_STEPS = [
  { label: 'Search Order', description: 'Scan OWK order' },
  { label: 'Skid Manifest', description: 'Scan Toyota label' },
  { label: 'Toyota Kanban', description: 'Scan kanban cards' },
  { label: 'Internal Kanban', description: 'Scan plastic card' },
  { label: 'Serial Numbers', description: 'Scan serials (required)' },
];

export default function SkidBuildPage() {
  const router = useRouter();
  const [currentStep, setCurrentStep] = useState<Step>(1);
  const [sessionId] = useState(() => generateSessionId());
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Step data
  const [order, setOrder] = useState<Order | null>(null);
  const [manifestId, setManifestId] = useState<string | null>(null);

  // Parallel arrays for 1:1:1 pairing (Scott's implementation pattern)
  const [toyotaKanbans, setToyotaKanbans] = useState<ToyotaKanban[]>([]);
  const [internalKanbans, setInternalKanbans] = useState<InternalKanban[]>([]);
  const [serialNumbers, setSerialNumbers] = useState<SerialNumber[]>([]);

  // Current item being scanned (for the current triplet)
  const [currentToyotaKanban, setCurrentToyotaKanban] = useState<ToyotaKanban | null>(null);
  const [currentInternalKanban, setCurrentInternalKanban] = useState<InternalKanban | null>(null);
  const [currentSerial, setCurrentSerial] = useState<SerialNumber | null>(null);

  // Exception handling state
  const [exceptions, setExceptions] = useState<Exception[]>([]);
  const [showExceptionModal, setShowExceptionModal] = useState(false);
  const [selectedExceptionType, setSelectedExceptionType] = useState('');
  const [exceptionComments, setExceptionComments] = useState('');

  // Step 1: Search/Scan Order
  const handleOrderScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    setLoading(true);
    setError(null);

    const response = await searchOrder(result.scannedValue);

    if (response.success && response.data) {
      setOrder(response.data);
      setCurrentStep(2);
    } else {
      setError(response.error || ERROR_MESSAGES.ORDER_NOT_FOUND);
    }

    setLoading(false);
  };

  // Step 2: Scan Skid Manifest
  const handleManifestScan = async (result: ScanResult) => {
    if (!result.success || !order) {
      setError(result.error || 'Order not found');
      return;
    }

    setLoading(true);
    setError(null);

    const response = await scanSkidManifest(result.scannedValue, order.id);

    if (response.success && response.data) {
      setManifestId(response.data.manifestId);
      setCurrentStep(3);
    } else {
      setError(response.error || ERROR_MESSAGES.SKID_NOT_FOUND);
    }

    setLoading(false);
  };

  // Step 3: Scan Toyota Kanban (1:1 pairing - immediately advance to Internal Kanban)
  const handleToyotaKanbanScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    setLoading(true);
    setError(null);

    const response = await scanToyotaKanban(result.scannedValue, sessionId);

    if (response.success && response.data) {
      // Store current Toyota Kanban for pairing
      setCurrentToyotaKanban(response.data);
      // Immediately advance to Internal Kanban scan (1:1 pairing)
      setCurrentStep(4);
    } else {
      setError(response.error || 'Failed to scan Toyota Kanban');
    }

    setLoading(false);
  };

  // Step 4: Scan Internal Kanban (paired with current Toyota Kanban)
  const handleInternalKanbanScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    if (!currentToyotaKanban) {
      setError('No Toyota Kanban scanned yet');
      return;
    }

    setLoading(true);
    setError(null);

    const response = await scanInternalKanban(result.scannedValue, sessionId);

    if (response.success && response.data) {
      // Store current Internal Kanban for pairing
      setCurrentInternalKanban(response.data);
      // Always move to step 5 for serial scanning (completes the triplet)
      setCurrentStep(5);
    } else {
      setError(response.error || 'Failed to scan Internal Kanban');
    }

    setLoading(false);
  };

  // Step 5: Scan Serial Numbers (Completes the Toyota-Internal-Serial triplet)
  const handleSerialScan = async (result: ScanResult) => {
    if (!result.success) {
      setError(result.error);
      return;
    }

    if (!currentToyotaKanban || !currentInternalKanban) {
      setError('Toyota Kanban and Internal Kanban must be scanned first');
      return;
    }

    setLoading(true);
    setError(null);

    const response = await scanSerialNumber(result.scannedValue, currentInternalKanban.id);

    if (response.success && response.data) {
      // Complete the triplet: Add all three to parallel arrays at same index
      setToyotaKanbans((prev) => [...prev, currentToyotaKanban]);
      setInternalKanbans((prev) => [...prev, currentInternalKanban]);
      setSerialNumbers((prev) => [...prev, response.data]);

      // Store the current serial for display
      setCurrentSerial(response.data);

      // Clear current items (triplet is complete)
      // Don't clear yet - we'll show the "Add Another Item?" prompt
    } else {
      setError(response.error || 'Failed to scan Serial Number');
    }

    setLoading(false);
  };

  // Add another item (restart from Toyota Kanban scan)
  const handleAddAnotherItem = () => {
    // Clear current triplet items
    setCurrentToyotaKanban(null);
    setCurrentInternalKanban(null);
    setCurrentSerial(null);
    setError(null);
    // Go back to step 3 to scan next Toyota Kanban
    setCurrentStep(3);
  };

  // Calculate planned vs actual quantities
  const getPlannedQuantity = () => {
    // Sum all Toyota Kanban quantities (planned)
    return toyotaKanbans.reduce((sum, kanban) => sum + kanban.quantity, 0);
  };

  const getActualQuantity = () => {
    // Actual quantity is the number of serials scanned
    return serialNumbers.length;
  };

  // Check if exception is required
  const isExceptionRequired = () => {
    return getPlannedQuantity() !== getActualQuantity();
  };

  // Handle adding an exception
  const handleAddException = () => {
    if (!selectedExceptionType || !exceptionComments.trim()) {
      setError('Please select exception type and add comments');
      return;
    }

    const newException: Exception = {
      type: selectedExceptionType,
      comments: exceptionComments.trim(),
      timestamp: new Date().toISOString(),
    };

    setExceptions([...exceptions, newException]);
    setSelectedExceptionType('');
    setExceptionComments('');
    setError(null);
  };

  // Handle removing an exception
  const handleRemoveException = (index: number) => {
    setExceptions(exceptions.filter((_, i) => i !== index));
  };

  // Complete workflow with validation
  const handleComplete = async () => {
    // Validation: Check if exception is required
    if (isExceptionRequired() && exceptions.length === 0) {
      setShowExceptionModal(true);
      setError('Planned quantity does not match actual quantity. Please add at least one exception.');
      return;
    }

    setLoading(true);
    setError(null);

    const response = await completeSkidBuild(sessionId);

    if (response.success) {
      alert('Skid build completed successfully!');
      router.push('/');
    } else {
      setError(response.error || 'Failed to complete skid build');
    }

    setLoading(false);
  };

  // Final submission from exception modal
  const handleFinalSubmit = async () => {
    if (isExceptionRequired() && exceptions.length === 0) {
      setError('At least one exception is required when quantities do not match');
      return;
    }

    setShowExceptionModal(false);
    await handleComplete();
  };

  // Reset to start
  const handleReset = () => {
    setCurrentStep(1);
    setOrder(null);
    setManifestId(null);
    // Clear parallel arrays
    setToyotaKanbans([]);
    setInternalKanbans([]);
    setSerialNumbers([]);
    // Clear current items
    setCurrentToyotaKanban(null);
    setCurrentInternalKanban(null);
    setCurrentSerial(null);
    // Clear exceptions
    setExceptions([]);
    setShowExceptionModal(false);
    setSelectedExceptionType('');
    setExceptionComments('');
    setError(null);
  };

  return (
    <div className="fixed inset-0 flex flex-col">
      {/* Background - Fixed, doesn't scroll */}
      <VUTEQStaticBackground />

      {/* Content - Scrolls on top of fixed background */}
      <div className="relative flex-1 overflow-y-auto">
        <div className="p-4 pt-24 max-w-3xl mx-auto space-y-3">
        {/* Progress Header with New Stepper */}
        <Card style={{ backgroundColor: '#FCFCFC' }}>
        <CardContent className="p-3">
          <ResponsiveStepper steps={SKID_BUILD_STEPS} currentStep={currentStep} />
        </CardContent>
      </Card>

      {/* Error Alert */}
      {error && (
        <Alert variant="error" onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      {/* Step 1: Search/Scan Order */}
      {currentStep === 1 && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="p-3">
            <Scanner
              onScan={handleOrderScan}
              expectedType="ORDER"
              label="Scan OWK Order Number"
              placeholder="OWK-123456"
              disabled={loading}
            />
          </CardContent>
        </Card>
      )}

      {/* Step 2: Scan Skid Manifest */}
      {currentStep === 2 && order && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="space-y-3 p-3">
            <Alert variant="success">
              <div>
                <strong>Order Found:</strong> {order.owkOrderNumber}
              </div>
              <div className="text-sm mt-1">
                Customer: {order.customerName} | Destination: {order.destination}
              </div>
            </Alert>
            <Scanner
              onScan={handleManifestScan}
              expectedType="SKID_MANIFEST"
              label="Scan Toyota Label"
              placeholder="TL-12345678"
              disabled={loading}
            />
          </CardContent>
        </Card>
      )}

      {/* Step 3: Scan Toyota Kanban (1:1 pairing - auto-advances to Internal Kanban) */}
      {currentStep === 3 && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="space-y-3 p-3">
            {/* Show previously scanned triplets */}
            {toyotaKanbans.length > 0 && (
              <div className="space-y-2">
                <p className="text-sm font-medium" style={{ color: '#253262' }}>
                  Previously Scanned Items ({toyotaKanbans.length}):
                </p>
                {toyotaKanbans.map((kanban, idx) => (
                  <div
                    key={idx}
                    className="p-3 bg-success-50 border border-success-200 rounded-lg"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex-1">
                        <div className="flex items-center gap-2">
                          <span className="font-mono text-sm font-bold">#{idx + 1}</span>
                          <Badge variant="success">✓</Badge>
                        </div>
                        <div className="text-xs text-gray-600 mt-1">
                          <div><strong>Toyota:</strong> {kanban.kanbanNumber}</div>
                          <div><strong>Internal:</strong> {internalKanbans[idx]?.kanbanNumber}</div>
                          <div><strong>Serial:</strong> {serialNumbers[idx]?.serialNumber}</div>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}

            <Alert variant="info">
              After scanning Toyota Kanban, you will immediately scan its paired Internal Kanban
            </Alert>

            <Scanner
              onScan={handleToyotaKanbanScan}
              expectedType="TOYOTA_KANBAN"
              label="Scan Toyota Kanban"
              placeholder="TK-ABC123"
              disabled={loading}
            />
          </CardContent>
        </Card>
      )}

      {/* Step 4: Scan Internal Kanban (paired with current Toyota Kanban) */}
      {currentStep === 4 && currentToyotaKanban && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="space-y-3 p-3">
            {/* Show current Toyota Kanban being paired */}
            <div className="p-3 bg-primary-50 border border-primary-200 rounded-lg">
              <p className="text-sm font-medium" style={{ color: '#253262' }}>
                Pairing with Toyota Kanban:
              </p>
              <p className="font-mono text-sm">{currentToyotaKanban.kanbanNumber}</p>
              <p className="text-xs text-gray-600 mt-1">
                Part: {currentToyotaKanban.partNumber} | Qty: {currentToyotaKanban.quantity}
              </p>
            </div>

            {/* BR-003 */}
            <Alert variant="info">
              Internal Kanbans are 7-13 character reusable plastic cards (1:1 pairing)
            </Alert>

            <Scanner
              onScan={handleInternalKanbanScan}
              expectedType="INTERNAL_KANBAN"
              label="Scan Internal Kanban"
              placeholder="ABC1234"
              disabled={loading}
            />
          </CardContent>
        </Card>
      )}

      {/* Step 5: Scan Serial Numbers (Completes the triplet) */}
      {currentStep === 5 && currentToyotaKanban && currentInternalKanban && (
        <Card style={{ backgroundColor: '#FCFCFC' }}>
          <CardContent className="space-y-3 p-3">
            {/* Show current paired items */}
            <div className="p-3 bg-primary-50 border border-primary-200 rounded-lg">
              <p className="text-sm font-medium" style={{ color: '#253262' }}>Current Pair:</p>
              <div className="text-xs text-gray-600 mt-1 space-y-1">
                <div><strong>Toyota:</strong> {currentToyotaKanban.kanbanNumber}</div>
                <div><strong>Internal:</strong> {currentInternalKanban.kanbanNumber}</div>
                <div><strong>Part:</strong> {currentInternalKanban.partNumber}</div>
              </div>
            </div>

            {/* Show previously scanned triplets */}
            {serialNumbers.length > 0 && (
              <div className="space-y-2">
                <p className="text-sm font-medium" style={{ color: '#253262' }}>
                  Previously Scanned Items ({serialNumbers.length}):
                </p>
                {serialNumbers.map((serial, idx) => (
                  <div
                    key={idx}
                    className="p-3 bg-success-50 border border-success-200 rounded-lg"
                  >
                    <div className="flex items-center gap-2">
                      <span className="font-mono text-sm font-bold">#{idx + 1}</span>
                      <Badge variant="success">✓</Badge>
                    </div>
                    <div className="text-xs text-gray-600 mt-1">
                      <div><strong>Toyota:</strong> {toyotaKanbans[idx]?.kanbanNumber}</div>
                      <div><strong>Internal:</strong> {internalKanbans[idx]?.kanbanNumber}</div>
                      <div><strong>Serial:</strong> {serial.serialNumber}</div>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* If serial just scanned, show "Add Another?" or "Complete" options */}
            {currentSerial ? (
              <div className="space-y-3">
                <Alert variant="success">
                  Serial scanned successfully! Item #{serialNumbers.length} complete.
                </Alert>

                {/* Quantity Summary */}
                <div className="p-3 border rounded-lg" style={{
                  backgroundColor: isExceptionRequired() ? '#FEF3C7' : '#DCFCE7',
                  borderColor: isExceptionRequired() ? '#F59E0B' : '#10B981'
                }}>
                  <p className="text-sm font-semibold mb-2" style={{ color: '#253262' }}>
                    Quantity Summary:
                  </p>
                  <div className="space-y-1 text-sm">
                    <div className="flex justify-between">
                      <span>Planned (Total Kanban Qty):</span>
                      <span className="font-bold">{getPlannedQuantity()}</span>
                    </div>
                    <div className="flex justify-between">
                      <span>Actual (Serials Scanned):</span>
                      <span className="font-bold">{getActualQuantity()}</span>
                    </div>
                    {isExceptionRequired() && (
                      <div className="pt-2 border-t" style={{ borderColor: '#F59E0B' }}>
                        <p className="text-xs text-warning-700 font-medium">
                          <i className="fa-light fa-triangle-exclamation mr-1"></i>
                          Exception will be required before completion
                        </p>
                      </div>
                    )}
                  </div>
                </div>

                <div className="flex gap-2">
                  <Button
                    onClick={handleAddAnotherItem}
                    variant="success-light"
                    fullWidth
                  >
                    <i className="fa-light fa-plus mr-2"></i>
                    Add Another Item
                  </Button>
                  <Button
                    onClick={handleComplete}
                    variant="success"
                    fullWidth
                    loading={loading}
                  >
                    <i className="fa-light fa-check-circle mr-2"></i>
                    Complete Skid Build
                  </Button>
                </div>
              </div>
            ) : (
              <Scanner
                onScan={handleSerialScan}
                expectedType="SERIAL"
                label="Scan Serial Number"
                placeholder="SN-ABC12345"
                disabled={loading}
              />
            )}
          </CardContent>
        </Card>
      )}

      {/* Action Buttons */}
      <div className="flex gap-2">
        <Button
          onClick={() => router.push('/')}
          variant="error"
          fullWidth
        >
          <i className="fa-light fa-xmark mr-2"></i>
          Cancel
        </Button>
        {currentStep > 1 && (
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
        </div>
      </div>

      {/* Exception Modal */}
      {showExceptionModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 p-4">
          <Card style={{ backgroundColor: '#FCFCFC', maxWidth: '600px', width: '100%', maxHeight: '90vh', overflow: 'auto' }}>
            <CardHeader>
              <CardTitle>Exception Required</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              {/* Quantity Mismatch Alert */}
              <Alert variant="warning">
                <div className="space-y-2">
                  <div className="flex justify-between items-center">
                    <span className="font-semibold">Planned Quantity:</span>
                    <span className="text-lg font-bold">{getPlannedQuantity()}</span>
                  </div>
                  <div className="flex justify-between items-center">
                    <span className="font-semibold">Actual Quantity:</span>
                    <span className="text-lg font-bold">{getActualQuantity()}</span>
                  </div>
                  <div className="pt-2 border-t border-warning-300">
                    <p className="text-sm">
                      Quantities do not match. Please add at least one exception to explain the discrepancy.
                    </p>
                  </div>
                </div>
              </Alert>

              {/* Exception Type Dropdown */}
              <div>
                <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                  Exception Type *
                </label>
                <select
                  value={selectedExceptionType}
                  onChange={(e) => setSelectedExceptionType(e.target.value)}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                  style={{ backgroundColor: '#FCFCFC' }}
                >
                  <option value="">Select exception type...</option>
                  {EXCEPTION_TYPES.map((type) => (
                    <option key={type} value={type}>
                      {type}
                    </option>
                  ))}
                </select>
              </div>

              {/* Comments Textarea */}
              <div>
                <label className="block text-sm font-medium mb-2" style={{ color: '#253262' }}>
                  Comments *
                </label>
                <textarea
                  value={exceptionComments}
                  onChange={(e) => setExceptionComments(e.target.value)}
                  placeholder="Describe the reason for the exception..."
                  rows={3}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500 resize-none"
                  style={{ backgroundColor: '#FCFCFC' }}
                />
              </div>

              {/* Add Exception Button */}
              <Button
                onClick={handleAddException}
                variant="primary"
                fullWidth
                disabled={!selectedExceptionType || !exceptionComments.trim()}
              >
                <i className="fa-light fa-plus mr-2"></i>
                Add Exception
              </Button>

              {/* Exceptions List */}
              {exceptions.length > 0 && (
                <div className="space-y-2">
                  <p className="text-sm font-medium" style={{ color: '#253262' }}>
                    Added Exceptions ({exceptions.length}):
                  </p>
                  {exceptions.map((exception, idx) => (
                    <div
                      key={idx}
                      className="p-3 bg-warning-50 border border-warning-200 rounded-lg"
                    >
                      <div className="flex items-start justify-between gap-2">
                        <div className="flex-1">
                          <div className="flex items-center gap-2 mb-1">
                            <Badge variant="warning">{exception.type}</Badge>
                            <span className="text-xs text-gray-500">
                              {new Date(exception.timestamp).toLocaleString()}
                            </span>
                          </div>
                          <p className="text-sm text-gray-700">{exception.comments}</p>
                        </div>
                        <button
                          onClick={() => handleRemoveException(idx)}
                          className="text-error-600 hover:text-error-700 p-1"
                          aria-label="Remove exception"
                        >
                          <i className="fa-light fa-trash"></i>
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Modal Actions */}
              <div className="flex gap-2 pt-4 border-t border-gray-200">
                <Button
                  onClick={() => {
                    setShowExceptionModal(false);
                    setError(null);
                  }}
                  variant="error"
                  fullWidth
                >
                  <i className="fa-light fa-xmark mr-2"></i>
                  Cancel
                </Button>
                <Button
                  onClick={handleFinalSubmit}
                  variant="success"
                  fullWidth
                  disabled={exceptions.length === 0}
                  loading={loading}
                >
                  <i className="fa-light fa-paper-plane mr-2"></i>
                  Submit with Exceptions
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  );
}
