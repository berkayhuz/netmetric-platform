"use client";

import * as React from "react";

type Theme = "light" | "dark" | "system";
type ResolvedTheme = "light" | "dark";

type ThemeContextValue = {
  theme: Theme;
  resolvedTheme: ResolvedTheme;
  setTheme: (theme: Theme) => void;
};

const THEME_STORAGE_KEY = "netmetric-theme";
const THEME_COOKIE_KEY = "netmetric-theme";

const ThemeContext = React.createContext<ThemeContextValue | undefined>(undefined);

function getSystemTheme(): ResolvedTheme {
  if (typeof window === "undefined") {
    return "light";
  }

  return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

function resolveTheme(theme: Theme): ResolvedTheme {
  return theme === "system" ? getSystemTheme() : theme;
}

function normalizeTheme(value: string | null | undefined): Theme | null {
  if (!value) {
    return null;
  }

  const normalized = value.trim().toLowerCase();
  if (normalized === "light" || normalized === "dark" || normalized === "system") {
    return normalized;
  }

  if (normalized === "default") {
    return "system";
  }

  return null;
}

function applyThemeToDocument(theme: Theme): ResolvedTheme {
  const resolved = resolveTheme(theme);
  const root = document.documentElement;
  root.classList.toggle("dark", resolved === "dark");
  root.style.colorScheme = resolved;
  return resolved;
}

type ThemeProviderProps = {
  children: React.ReactNode;
  defaultTheme?: Theme;
  forcedTheme?: Theme;
};

export function ThemeProvider({
  children,
  defaultTheme = "system",
  forcedTheme,
}: ThemeProviderProps) {
  const [theme, setThemeState] = React.useState<Theme>(defaultTheme);
  const [resolvedTheme, setResolvedTheme] = React.useState<ResolvedTheme>("light");

  React.useEffect(() => {
    if (forcedTheme) {
      setThemeState(forcedTheme);
      setResolvedTheme(applyThemeToDocument(forcedTheme));
      return;
    }

    const cookieThemeMatch = document.cookie.match(
      new RegExp(`(?:^|; )${THEME_COOKIE_KEY}=([^;]*)`),
    )?.[1];
    const cookieTheme = normalizeTheme(
      cookieThemeMatch ? decodeURIComponent(cookieThemeMatch) : null,
    );
    const stored = window.localStorage.getItem(THEME_STORAGE_KEY);
    const storedTheme = normalizeTheme(stored);
    const initialTheme: Theme =
      cookieTheme ?? (defaultTheme !== "system" ? defaultTheme : storedTheme) ?? defaultTheme;

    if (stored !== initialTheme) {
      window.localStorage.setItem(THEME_STORAGE_KEY, initialTheme);
    }

    setThemeState(initialTheme);
    setResolvedTheme(applyThemeToDocument(initialTheme));
  }, [defaultTheme, forcedTheme]);

  React.useEffect(() => {
    if (forcedTheme) {
      return;
    }

    if (theme !== "system") {
      return;
    }

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    const onChange = () => {
      setResolvedTheme(applyThemeToDocument("system"));
    };

    mediaQuery.addEventListener("change", onChange);
    return () => mediaQuery.removeEventListener("change", onChange);
  }, [forcedTheme, theme]);

  const setTheme = React.useCallback(
    (nextTheme: Theme) => {
      if (forcedTheme) {
        return;
      }

      setThemeState(nextTheme);
      window.localStorage.setItem(THEME_STORAGE_KEY, nextTheme);
      setResolvedTheme(applyThemeToDocument(nextTheme));
    },
    [forcedTheme],
  );

  const value = React.useMemo<ThemeContextValue>(
    () => ({
      theme,
      resolvedTheme,
      setTheme,
    }),
    [theme, resolvedTheme, setTheme],
  );

  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>;
}

export function useTheme(): ThemeContextValue {
  const context = React.useContext(ThemeContext);

  if (!context) {
    throw new Error("useTheme must be used within a ThemeProvider.");
  }

  return context;
}

export type { Theme, ResolvedTheme };
