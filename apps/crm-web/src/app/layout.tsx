import type { Metadata } from "next";
import { cookies, headers } from "next/headers";
import { Manrope } from "next/font/google";
import { resolveUiPreferences, UI_LOCALE_COOKIE_NAME, UI_THEME_COOKIE_NAME } from "@netmetric/i18n";
import { createServerPerformanceLogger } from "@netmetric/observability/server";
import { getThemeInitScript } from "@netmetric/ui";
import { loadAppBackgroundStyle } from "@netmetric/ui/app-background";
import { ThemeProvider, Toaster } from "@netmetric/ui/client";

import { CrmShell } from "@/components/shell/crm-shell";
import { crmEnv } from "@/lib/crm-env";
import { getOptionalCrmShellSession, isPublicCrmPath } from "@/lib/crm-auth/crm-session";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { CrmErrorMonitoring } from "@/lib/error-monitoring";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

import "./globals.css";

const manrope = Manrope({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-sans",
});

const crmLayoutPerformance = createServerPerformanceLogger({
  app: "crm-web",
  component: "root-layout",
  enabled: process.env.NETMETRIC_PERF_LOG === "1",
});

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return {
    metadataBase: new URL(crmEnv.crmUrl),
    title: {
      default: tCrm("crm.shell.appTitle", locale),
      template: `%s | ${tCrm("crm.shell.appTitle", locale)}`,
    },
    description: tCrm("crm.shell.workspace", locale),
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

export default async function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  const cookieStore = await cookies();
  const headerStore = await headers();
  const pathname = headerStore.get("x-netmetric-pathname") ?? "/";
  const session = isPublicCrmPath(pathname)
    ? await crmLayoutPerformance.measure("layout.optionalSession", () =>
        getOptionalCrmShellSession(),
      )
    : await crmLayoutPerformance.measure("layout.requiredSession", () =>
        requireCrmSession(pathname),
      );
  const resolved = resolveUiPreferences({
    theme: cookieStore.get(UI_THEME_COOKIE_NAME)?.value,
    locale: cookieStore.get(UI_LOCALE_COOKIE_NAME)?.value,
  });
  const locale = resolved.locale;
  const backgroundStyle = loadAppBackgroundStyle();

  return (
    <html lang={locale} className={manrope.variable} suppressHydrationWarning>
      <head>
        {session?.faviconUrl ? <link rel="icon" href={session.faviconUrl} /> : null}
        <script
          id="netmetric-theme-init"
          dangerouslySetInnerHTML={{ __html: getThemeInitScript(resolved.theme) }}
        />
      </head>
      <body>
        <ThemeProvider defaultTheme={resolved.theme}>
          <CrmErrorMonitoring />
          <Toaster />
          <CrmShell
            locale={locale}
            user={
              session?.shellUser ?? {
                displayName: "",
                email: null,
                avatarUrl: null,
                workspaceName: null,
                sessionStatus: "authenticated",
              }
            }
            notifications={
              session?.shellNotifications ?? {
                items: [],
                unreadCount: 0,
                unavailable: false,
              }
            }
            authUrl={crmEnv.authUrl}
            accountUrl={crmEnv.accountUrl}
            toolsUrl={crmEnv.toolsUrl}
            publicUrl={crmEnv.publicUrl}
            backgroundStyle={backgroundStyle}
            {...(session ? { capabilities: session.capabilities } : {})}
          >
            {children}
          </CrmShell>
        </ThemeProvider>
      </body>
    </html>
  );
}
