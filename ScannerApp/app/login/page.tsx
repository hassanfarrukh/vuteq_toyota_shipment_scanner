/**
 * Login Page - Professional Design matching reference
 * Author: Hassan
 * Date: 2025-10-21
 * Updated: 2025-10-21 - Cleaned up UI, removed demo credentials, removed unnecessary icons, added mobile logo
 * Updated: 2025-10-21 - Added overflow hidden to prevent scrolling on login page only
 * Updated: 2025-10-21 - Added animated gradient blob background inspired by reference design
 * Updated: 2025-10-21 - Updated gradient colors to match VUTEQ brand colors (navy blue #253262 and red #D2312E)
 * Updated: 2025-10-21 - Enhanced red visibility (#D2312E) to be clearly prominent at 10-15% while navy blue dominates at 70-75%
 * Updated: 2025-10-21 - Updated login form panel colors to use VUTEQ brand colors (off-white #FCFCFC base with navy/red accents)
 * Updated: 2025-10-21 - Updated heading to "Track, Scan, and Manage" to better reflect application purpose
 * Updated: 2025-10-28 - Fixed Font Awesome icon classes: changed fa-solid to fa for user and lock icons
 * Updated: 2025-10-29 - Changed fa to fa-solid for all icons (solid/bold style) by Hassan
 * Updated: 2025-10-29 - Applied full VUTEQ duotone colors to login icons matching dashboard styling by Hassan
 * Updated: 2025-11-25 - Added comprehensive file logging for debugging login flow
 * Entry point with authentication for different user roles
 */

'use client';

import { useState, FormEvent, useEffect } from 'react';
import Image from 'next/image';
import { useAuth } from '@/contexts/AuthContext';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import Alert from '@/components/ui/Alert';
import { clientLogger } from '@/lib/logger';

export default function LoginPage() {
  const { login, loading: authLoading } = useAuth();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [rememberMe, setRememberMe] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Prevent scrolling on login page only
  useEffect(() => {
    clientLogger.info('LoginPage', 'Login page mounted');

    // Apply overflow hidden to body when login page mounts
    document.body.style.overflow = 'hidden';

    // Restore overflow when login page unmounts (user navigates away)
    return () => {
      clientLogger.info('LoginPage', 'Login page unmounting');
      document.body.style.overflow = 'auto';
    };
  }, []);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError(null);

    clientLogger.info('LoginPage', '========== FORM SUBMISSION STARTED ==========');
    clientLogger.debug('LoginPage', 'Form submit event triggered', {
      username,
      passwordLength: password.length,
      rememberMe,
    });

    // Validation
    if (!username.trim()) {
      clientLogger.warn('LoginPage', 'Validation failed: Username is empty');
      setError('Please enter your username');
      return;
    }

    if (!password.trim()) {
      clientLogger.warn('LoginPage', 'Validation failed: Password is empty');
      setError('Please enter your password');
      return;
    }

    clientLogger.info('LoginPage', 'Form validation passed - submitting login', { username });
    setLoading(true);

    try {
      clientLogger.debug('LoginPage', 'Calling AuthContext login function');
      const result = await login(username, password);

      clientLogger.info('LoginPage', 'Login function returned', {
        success: result.success,
        hasError: !!result.error,
        error: result.error,
      });

      if (!result.success) {
        clientLogger.error('LoginPage', 'Login failed - displaying error to user', {
          error: result.error,
        });
        setError(result.error || 'Login failed');
      } else {
        clientLogger.info('LoginPage', 'Login successful - user should be redirected by AuthContext');
      }
      // If successful, user will be redirected by AuthContext
    } catch (err) {
      clientLogger.error('LoginPage', 'Unexpected error caught in handleSubmit', {
        error: err instanceof Error ? err.message : String(err),
        errorStack: err instanceof Error ? err.stack : undefined,
      });
      setError('An unexpected error occurred. Please try again.');
    } finally {
      clientLogger.debug('LoginPage', 'Setting loading state to false');
      setLoading(false);
      clientLogger.info('LoginPage', '========== FORM SUBMISSION COMPLETED ==========');
    }
  };

  if (authLoading) {
    return (
      <div className="fixed inset-0 flex items-center justify-center" style={{ backgroundColor: '#1e3a5f' }}>
        <div className="text-center">
          <svg
            className="animate-spin h-10 w-10 text-white mx-auto mb-3"
            xmlns="http://www.w3.org/2000/svg"
            fill="none"
            viewBox="0 0 24 24"
          >
            <circle
              className="opacity-25"
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="4"
            />
            <path
              className="opacity-75"
              fill="currentColor"
              d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
            />
          </svg>
          <p className="text-white text-sm">Loading...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="fixed inset-0 flex items-center justify-center overflow-hidden" style={{ background: 'linear-gradient(135deg, #253262 0%, #1a4d7a 50%, #253262 100%)' }}>
      {/* Animated Gradient Blobs Background - VUTEQ Brand Colors */}
      <div className="absolute inset-0 overflow-hidden">
        {/* Large Navy Blue Blob - Top Left (Dominant) */}
        <div
          className="absolute w-[500px] h-[500px] rounded-full mix-blend-multiply filter blur-3xl opacity-80 animate-blob"
          style={{ top: '-15%', left: '-10%', animationDelay: '0s', backgroundColor: '#253262' }}
        ></div>

        {/* Large Navy Blue Blob - Top Right (Dominant) */}
        <div
          className="absolute w-[450px] h-[450px] rounded-full mix-blend-multiply filter blur-3xl opacity-75 animate-blob"
          style={{ top: '10%', right: '-8%', animationDelay: '2s', backgroundColor: '#253262' }}
        ></div>

        {/* PROMINENT Red Accent Blob - Center Left (CLEARLY VISIBLE) */}
        <div
          className="absolute w-[380px] h-[380px] rounded-full mix-blend-normal filter blur-2xl opacity-85 animate-blob"
          style={{ top: '35%', left: '15%', animationDelay: '4s', backgroundColor: '#D2312E' }}
        ></div>

        {/* Secondary Red Accent Blob - Bottom Right (VISIBLE) */}
        <div
          className="absolute w-[320px] h-[320px] rounded-full mix-blend-normal filter blur-2xl opacity-75 animate-blob"
          style={{ bottom: '5%', right: '10%', animationDelay: '1s', backgroundColor: '#D2312E' }}
        ></div>

        {/* Medium Navy Blue Blob - Bottom Center */}
        <div
          className="absolute w-[400px] h-[400px] rounded-full mix-blend-multiply filter blur-3xl opacity-70 animate-blob"
          style={{ bottom: '-5%', left: '30%', animationDelay: '6s', backgroundColor: '#253262' }}
        ></div>

        {/* Light Blue-Gray Accent Blob - For Smooth Blending */}
        <div
          className="absolute w-[280px] h-[280px] rounded-full mix-blend-overlay filter blur-2xl opacity-50 animate-blob"
          style={{ top: '50%', right: '25%', animationDelay: '3s', backgroundColor: '#EEF0F4' }}
        ></div>

        {/* Small Light Blue-Gray Blob - Subtle Highlight */}
        <div
          className="absolute w-[200px] h-[200px] rounded-full mix-blend-overlay filter blur-xl opacity-40 animate-blob"
          style={{ top: '25%', left: '40%', animationDelay: '5s', backgroundColor: '#F5F7F9' }}
        ></div>
      </div>

      {/* Main Card Container - NO padding, centered, fits in viewport */}
      <div className="relative z-10 w-full max-w-6xl h-[85vh] max-h-[700px] flex flex-col md:flex-row rounded-2xl shadow-2xl overflow-hidden">

        {/* Left Side - VUTEQ Branded Panel with Login Form */}
        <div className="w-full md:w-1/2 flex items-center justify-center px-6 md:px-12 py-8" style={{ backgroundColor: '#FCFCFC' }}>
          <div className="w-full max-w-md">
            {/* VUTEQ Logo - Only visible on mobile */}
            <div className="md:hidden flex justify-center mb-8">
              <Image
                src="/images/logos/logo01.svg"
                alt="VUTEQ Logo"
                width={140}
                height={50}
                className="object-contain"
              />
            </div>

            {/* Heading */}
            <h1 className="text-3xl font-bold mb-2" style={{ color: '#253262' }}>Log in to your Account</h1>
            {/* Updated: 2025-10-29 - Changed login page icons to fa-regular (single color) by Hassan */}
            <p className="text-sm mb-6" style={{ color: '#64748b' }}>Welcome back! Select method to log in:</p>

            {/* Login Form */}
            <form onSubmit={handleSubmit} className="space-y-4">
              {/* Error Alert */}
              {error && (
                <Alert variant="error" onClose={() => setError(null)}>
                  {error}
                </Alert>
              )}

              {/* Username Field */}
              <div>
                <label htmlFor="username" className="flex items-center gap-2 text-sm font-medium mb-1.5" style={{ color: '#253262' }}>
                  <i className="fa-regular fa-user" style={{ fontSize: '16px', color: '#253262', WebkitFontSmoothing: 'antialiased' }}></i>
                  Username
                </label>
                <Input
                  id="username"
                  type="text"
                  value={username}
                  onChange={(e) => setUsername(e.target.value)}
                  placeholder="Enter your username"
                  autoComplete="username"
                  disabled={loading}
                  autoFocus
                  className="h-11 border-gray-300 focus:border-[#253262] focus:ring-[#253262]"
                  style={{ backgroundColor: '#F5F7F9' }}
                />
              </div>

              {/* Password Field */}
              <div>
                <label htmlFor="password" className="flex items-center gap-2 text-sm font-medium mb-1.5" style={{ color: '#253262' }}>
                  <i className="fa-regular fa-lock" style={{ fontSize: '16px', color: '#253262', WebkitFontSmoothing: 'antialiased' }}></i>
                  Password
                </label>
                <Input
                  id="password"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Enter your password"
                  autoComplete="current-password"
                  disabled={loading}
                  className="h-11 border-gray-300 focus:border-[#253262] focus:ring-[#253262]"
                  style={{ backgroundColor: '#F5F7F9' }}
                />
              </div>

              {/* Remember Me */}
              <div className="flex items-center">
                <input
                  id="remember-me"
                  type="checkbox"
                  checked={rememberMe}
                  onChange={(e) => setRememberMe(e.target.checked)}
                  disabled={loading}
                  className="h-4 w-4 rounded border-gray-300 focus:ring-[#253262]"
                  style={{ color: '#253262', accentColor: '#253262' }}
                />
                <label htmlFor="remember-me" className="ml-2 block text-sm" style={{ color: '#64748b' }}>
                  Remember me
                </label>
              </div>

              {/* Submit Button - Success Green with Icon */}
              <Button
                type="submit"
                variant="success-light"
                fullWidth
                disabled={loading}
                className="h-12 font-semibold rounded-lg transition-all duration-200 hover:shadow-lg flex items-center justify-center gap-2"
              >
                {loading ? (
                  <>
                    <svg
                      className="animate-spin h-5 w-5"
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                    >
                      <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                      />
                      <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      />
                    </svg>
                    <span>Signing in...</span>
                  </>
                ) : (
                  <>
                    <i className="fa-light fa-right-to-bracket text-base"></i>
                    <span>Log In</span>
                  </>
                )}
              </Button>
            </form>
          </div>
        </div>

        {/* Right Side - VUTEQ Navy Blue Panel - Hidden on mobile */}
        <div className="hidden md:block md:w-1/2 relative overflow-hidden" style={{ backgroundColor: '#253262' }}>
          {/* Company Logos at Top - Using actual SVG files */}
          <div className="absolute top-8 left-8 right-8 flex justify-between items-center gap-4">
            <div className="rounded-xl px-6 py-4 shadow-lg flex items-center justify-center h-16" style={{ backgroundColor: '#FCFCFC' }}>
              <Image
                src="/images/logos/logo01.svg"
                alt="VUTEQ Logo"
                width={100}
                height={40}
                className="object-contain"
              />
            </div>
            <div className="rounded-xl px-6 py-4 shadow-lg flex items-center justify-center h-16" style={{ backgroundColor: '#FCFCFC' }}>
              <Image
                src="/images/logos/VIS_toyota_logo_horiz_black_RGB_2023.svg"
                alt="Toyota Logo"
                width={120}
                height={40}
                className="object-contain"
              />
            </div>
          </div>

          {/* Content - Bottom Center */}
          <div className="absolute bottom-0 left-0 right-0 px-12 pb-16 text-center" style={{ color: '#FCFCFC' }}>
            <h2 className="text-4xl font-bold mb-3 leading-tight" style={{ color: '#FCFCFC' }}>
              Track, Scan, and Manage
            </h2>
            <p className="text-base mb-8" style={{ color: '#EEF0F4', opacity: 0.95 }}>
              Your complete warehouse management solution<br />for seamless operations and real-time visibility.
            </p>

            {/* Features List */}
            <div className="space-y-3 text-left max-w-sm mx-auto">
              <div className="flex items-center gap-3">
                <div className="w-6 h-6 rounded flex items-center justify-center flex-shrink-0" style={{ backgroundColor: '#D2312E' }}>
                  <svg className="w-4 h-4" fill="none" stroke="#FCFCFC" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                </div>
                <span className="text-sm font-medium" style={{ color: '#FCFCFC' }}>Real-time dock monitoring</span>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-6 h-6 rounded flex items-center justify-center flex-shrink-0" style={{ backgroundColor: '#D2312E' }}>
                  <svg className="w-4 h-4" fill="none" stroke="#FCFCFC" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                </div>
                <span className="text-sm font-medium" style={{ color: '#FCFCFC' }}>Skid build tracking</span>
              </div>
              <div className="flex items-center gap-3">
                <div className="w-6 h-6 rounded flex items-center justify-center flex-shrink-0" style={{ backgroundColor: '#D2312E' }}>
                  <svg className="w-4 h-4" fill="none" stroke="#FCFCFC" viewBox="0 0 24 24">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                  </svg>
                </div>
                <span className="text-sm font-medium" style={{ color: '#FCFCFC' }}>Shipment load management</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
