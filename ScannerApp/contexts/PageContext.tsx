/**
 * Page Context
 * Author: Hassan
 * Date: 2025-10-28
 *
 * Context for managing dynamic page subtitles across the application.
 * Allows pages to set custom subtitles that are displayed in the Header component.
 */

'use client';

import { createContext, useContext, useState, ReactNode } from 'react';

interface PageContextType {
  subtitle: string | null;
  setSubtitle: (subtitle: string | null) => void;
}

const PageContext = createContext<PageContextType | undefined>(undefined);

export function PageProvider({ children }: { children: ReactNode }) {
  const [subtitle, setSubtitle] = useState<string | null>(null);

  return (
    <PageContext.Provider value={{ subtitle, setSubtitle }}>
      {children}
    </PageContext.Provider>
  );
}

export function usePageContext() {
  const context = useContext(PageContext);
  if (context === undefined) {
    throw new Error('usePageContext must be used within a PageProvider');
  }
  return context;
}
