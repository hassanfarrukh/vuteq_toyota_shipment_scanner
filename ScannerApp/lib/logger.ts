/**
 * Universal Logger Utility for Next.js
 * Author: Hassan
 * Date: 2025-11-25
 *
 * Provides logging for both server and client environments:
 * - Server: File-based logging with fs module
 * - Client: Console + localStorage logging
 *
 * Usage:
 * import { logger } from '@/lib/logger';
 * logger.info('Login', 'Login successful', { userId: '123' });
 * logger.error('Login', 'Login failed', { error: 'Invalid credentials' });
 */

// Log levels
export enum LogLevel {
  DEBUG = 'DEBUG',
  INFO = 'INFO',
  WARN = 'WARN',
  ERROR = 'ERROR',
}

// Log entry interface
interface LogEntry {
  timestamp: string;
  level: LogLevel;
  category: string;
  message: string;
  data?: any;
}

/**
 * Server-side File Logger (only works in Node.js environment)
 */
class FileLogger {
  private logDir: string | null = null;
  private logFile: string | null = null;
  private enabled: boolean = false;
  private fs: any = null;
  private path: any = null;

  constructor() {
    // Only initialize on server-side
    if (typeof window === 'undefined') {
      this.initializeServerLogger();
    }
  }

  /**
   * Initialize server-side logging (Node.js only)
   */
  private initializeServerLogger(): void {
    try {
      // Dynamically import Node.js modules (only available on server)
      this.fs = require('fs');
      this.path = require('path');

      // Use project root logs directory
      this.logDir = this.path.join(process.cwd(), 'logs');
      this.logFile = this.path.join(this.logDir, 'nextjs-app.log');
      this.enabled = true;

      // Create logs directory if it doesn't exist
      this.ensureLogDirectory();
    } catch (error) {
      console.error('[FileLogger] Failed to initialize:', error);
      this.enabled = false;
    }
  }

  /**
   * Ensure logs directory exists
   */
  private ensureLogDirectory(): void {
    if (!this.enabled || !this.fs || !this.logDir) return;

    try {
      if (!this.fs.existsSync(this.logDir)) {
        this.fs.mkdirSync(this.logDir, { recursive: true });
        console.log(`[Logger] Created logs directory: ${this.logDir}`);
      }
    } catch (error) {
      console.error('[Logger] Failed to create logs directory:', error);
      this.enabled = false;
    }
  }

  /**
   * Format log entry as string
   */
  private formatLogEntry(entry: LogEntry): string {
    const dataStr = entry.data ? ` | ${JSON.stringify(entry.data)}` : '';
    return `[${entry.timestamp}] [${entry.level}] [${entry.category}] ${entry.message}${dataStr}\n`;
  }

  /**
   * Write log entry to file
   */
  private writeToFile(entry: LogEntry): void {
    if (!this.enabled || !this.fs || !this.logFile) {
      // Fallback to console if file logging not available
      const logLine = this.formatLogEntry(entry).trim();
      console.log(logLine);
      return;
    }

    try {
      const logLine = this.formatLogEntry(entry);

      // Append to log file
      this.fs.appendFileSync(this.logFile, logLine, 'utf8');

      // Also log to console for immediate visibility
      console.log(logLine.trim());
    } catch (error) {
      console.error('[Logger] Failed to write to log file:', error);
    }
  }

  /**
   * Create log entry
   */
  private log(level: LogLevel, category: string, message: string, data?: any): void {
    const entry: LogEntry = {
      timestamp: new Date().toISOString(),
      level,
      category,
      message,
      data,
    };

    this.writeToFile(entry);
  }

  /**
   * Log debug message
   */
  debug(category: string, message: string, data?: any): void {
    this.log(LogLevel.DEBUG, category, message, data);
  }

  /**
   * Log info message
   */
  info(category: string, message: string, data?: any): void {
    this.log(LogLevel.INFO, category, message, data);
  }

  /**
   * Log warning message
   */
  warn(category: string, message: string, data?: any): void {
    this.log(LogLevel.WARN, category, message, data);
  }

  /**
   * Log error message
   */
  error(category: string, message: string, data?: any): void {
    this.log(LogLevel.ERROR, category, message, data);
  }

  /**
   * Clear log file (server-side only)
   */
  clear(): void {
    if (!this.enabled || !this.fs || !this.logFile) return;

    try {
      if (this.fs.existsSync(this.logFile)) {
        this.fs.writeFileSync(this.logFile, '', 'utf8');
        console.log('[Logger] Log file cleared');
      }
    } catch (error) {
      console.error('[Logger] Failed to clear log file:', error);
    }
  }

  /**
   * Get log file path
   */
  getLogFilePath(): string | null {
    return this.logFile;
  }
}

// Create singleton instance
export const fileLogger = new FileLogger();

/**
 * Client-side logger for browser environments
 * Uses console + localStorage for logging
 */
class ClientLogger {
  private readonly LOG_KEY = 'scanner_app_logs';
  private readonly MAX_LOGS = 1000; // Keep last 1000 log entries

  /**
   * Format log entry for display
   */
  private formatLogEntry(entry: LogEntry): string {
    const dataStr = entry.data ? ` | ${JSON.stringify(entry.data)}` : '';
    return `[${entry.timestamp}] [${entry.level}] [${entry.category}] ${entry.message}${dataStr}`;
  }

  /**
   * Save log to localStorage
   */
  private saveToLocalStorage(entry: LogEntry): void {
    if (typeof window === 'undefined' || !window.localStorage) return;

    try {
      // Get existing logs
      const existingLogs = this.getLogs();

      // Add new log
      existingLogs.push(entry);

      // Keep only the last MAX_LOGS entries
      const trimmedLogs = existingLogs.slice(-this.MAX_LOGS);

      // Save back to localStorage
      localStorage.setItem(this.LOG_KEY, JSON.stringify(trimmedLogs));
    } catch (error) {
      // localStorage might be full or disabled
      console.warn('[ClientLogger] Failed to save to localStorage:', error);
    }
  }

  /**
   * Get all logs from localStorage
   */
  getLogs(): LogEntry[] {
    if (typeof window === 'undefined' || !window.localStorage) return [];

    try {
      const logsJson = localStorage.getItem(this.LOG_KEY);
      return logsJson ? JSON.parse(logsJson) : [];
    } catch (error) {
      console.warn('[ClientLogger] Failed to read from localStorage:', error);
      return [];
    }
  }

  /**
   * Clear all logs from localStorage
   */
  clear(): void {
    if (typeof window === 'undefined' || !window.localStorage) return;

    try {
      localStorage.removeItem(this.LOG_KEY);
      console.log('[ClientLogger] Logs cleared from localStorage');
    } catch (error) {
      console.warn('[ClientLogger] Failed to clear localStorage:', error);
    }
  }

  /**
   * Create log entry
   */
  private log(level: LogLevel, category: string, message: string, data?: any): void {
    const entry: LogEntry = {
      timestamp: new Date().toISOString(),
      level,
      category,
      message,
      data,
    };

    // Format and display in console
    const logLine = this.formatLogEntry(entry);

    switch (level) {
      case LogLevel.DEBUG:
        console.log(`🔍 ${logLine}`, data || '');
        break;
      case LogLevel.INFO:
        console.log(`ℹ️ ${logLine}`, data || '');
        break;
      case LogLevel.WARN:
        console.warn(`⚠️ ${logLine}`, data || '');
        break;
      case LogLevel.ERROR:
        console.error(`❌ ${logLine}`, data || '');
        break;
    }

    // Save to localStorage
    this.saveToLocalStorage(entry);
  }

  /**
   * Log debug message
   */
  debug(category: string, message: string, data?: any): void {
    this.log(LogLevel.DEBUG, category, message, data);
  }

  /**
   * Log info message
   */
  info(category: string, message: string, data?: any): void {
    this.log(LogLevel.INFO, category, message, data);
  }

  /**
   * Log warning message
   */
  warn(category: string, message: string, data?: any): void {
    this.log(LogLevel.WARN, category, message, data);
  }

  /**
   * Log error message
   */
  error(category: string, message: string, data?: any): void {
    this.log(LogLevel.ERROR, category, message, data);
  }

  /**
   * Export logs as JSON string (useful for debugging)
   */
  exportLogs(): string {
    const logs = this.getLogs();
    return JSON.stringify(logs, null, 2);
  }

  /**
   * Get log file path (returns null on client)
   */
  getLogFilePath(): string | null {
    return null;
  }
}

// Create client logger instance
export const clientLogger = new ClientLogger();

// Export appropriate logger based on environment
export const logger = typeof window === 'undefined' ? fileLogger : clientLogger;
