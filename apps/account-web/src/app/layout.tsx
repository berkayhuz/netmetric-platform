import type { Metadata } from "next";
import { cookies, headers } from "next/headers";
import { Inter } from "next/font/google";
import {
  resolveUiPreferences as resolveSharedUiPreferences,
  UI_LOCALE_COOKIE_NAME,
  UI_THEME_COOKIE_NAME,
} from "@netmetric/i18n";
import { getThemeInitScript } from "@netmetric/ui";
import { ThemeProvider, Toaster } from "@netmetric/ui/client";

import { AccountShell } from "@/features/account/components/account-shell";
import { appEnv } from "@/lib/app-env";
import { AccountErrorMonitoring } from "@/lib/error-monitoring";
import { isPublicAccountPath } from "@/lib/auth/account-session";

import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-sans",
});

export const dynamic = "force-dynamic";

const accountPageTitles: Array<{ prefix: string; title: string }> = [
  { prefix: "/settings/team", title: "Team" },
  { prefix: "/security/sessions", title: "Sessions" },
  { prefix: "/security/mfa", title: "MFA" },
  { prefix: "/security/password", title: "Password" },
  { prefix: "/notifications", title: "Notifications" },
  { prefix: "/preferences", title: "Preferences" },
  { prefix: "/workspaces", title: "Workspaces" },
  { prefix: "/security", title: "Security" },
  { prefix: "/profile", title: "Profile" },
  { prefix: "/privacy", title: "Privacy" },
  { prefix: "/audit", title: "Audit" },
];

function resolveAccountPageTitle(pathname: string): string | null {
  const match = accountPageTitles.find(
    (item) => pathname === item.prefix || pathname.startsWith(`${item.prefix}/`),
  );
  return match?.title ?? null;
}

export async function generateMetadata(): Promise<Metadata> {
  const headerStore = await headers();
  const pathname = headerStore.get("x-netmetric-pathname") ?? "/";
  const appTitle = "NetMetric Account";
  const pageTitle = resolveAccountPageTitle(pathname);

  return {
    metadataBase: new URL(appEnv.accountUrl),
    title: pageTitle ? `${appTitle} | ${pageTitle}` : appTitle,
    description: "Authenticated account portal.",
    robots: {
      index: false,
      follow: false,
      nocache: true,
      googleBot: {
        index: false,
        follow: false,
        noimageindex: true,
        noarchive: true,
        nosnippet: true,
      },
    },
  };
}

async function resolveUiPreferences(): Promise<{
  theme: "system" | "light" | "dark";
  lang: string;
  faviconUrl: string | null;
}> {
  const cookieStore = await cookies();
  const cookieResolved = resolveSharedUiPreferences({
    theme: cookieStore.get(UI_THEME_COOKIE_NAME)?.value,
    locale: cookieStore.get(UI_LOCALE_COOKIE_NAME)?.value,
  });
  return {
    theme: cookieResolved.theme,
    lang: cookieResolved.locale,
    faviconUrl: null,
  };
}

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const uiPreferences = await resolveUiPreferences();
  const isServerResolvedDark = uiPreferences.theme === "dark";
  return (
    <html
      lang={uiPreferences.lang}
      className={`${inter.variable}${isServerResolvedDark ? " dark" : ""}`}
      style={isServerResolvedDark ? { colorScheme: "dark" } : undefined}
      suppressHydrationWarning
    >
      <head>
        {uiPreferences.faviconUrl ? <link rel="icon" href={uiPreferences.faviconUrl} /> : null}
        <script
          id="netmetric-theme-init"
          dangerouslySetInnerHTML={{ __html: getThemeInitScript(uiPreferences.theme) }}
        />
      </head>
      <body className="overflow-hidden">
        <ThemeProvider defaultTheme={uiPreferences.theme}>
          <AccountErrorMonitoring />
          <AccountShell locale={uiPreferences.lang}>{children}</AccountShell>
          <Toaster />
        </ThemeProvider>
      </body>
    </html>
  );
}
