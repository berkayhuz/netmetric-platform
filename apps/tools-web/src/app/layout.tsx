import type { Metadata } from "next";
import { cookies } from "next/headers";
import { Inter } from "next/font/google";
import { getThemeInitScript } from "@netmetric/ui";
import { ThemeProvider } from "@netmetric/ui/client";
import { resolveUiPreferences, UI_LOCALE_COOKIE_NAME, UI_THEME_COOKIE_NAME } from "@netmetric/i18n";

import { ToolsFooter } from "@/features/tools/components/tools-footer";
import { ToolsHeader } from "@/features/tools/components/tools-header";
import { ToolsErrorMonitoring } from "@/lib/error-monitoring";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { getCurrentToolsSession } from "@/lib/tools-auth/tools-auth-headers";
import { toolsEnv } from "@/lib/tools-env";

import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-sans",
});

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();
  const description = tTools("tools.meta.description", locale);

  return {
    metadataBase: new URL(toolsEnv.siteUrl),
    title: {
      default: tTools("tools.meta.title", locale),
      template: `%s | ${tTools("tools.meta.title", locale)}`,
    },
    description,
    alternates: {
      canonical: toolsEnv.siteUrl,
    },
    openGraph: {
      type: "website",
      url: toolsEnv.siteUrl,
      title: tTools("tools.meta.title", locale),
      description,
      siteName: tTools("tools.meta.title", locale),
    },
    twitter: {
      card: "summary_large_image",
      title: tTools("tools.meta.title", locale),
      description,
    },
  };
}

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const cookieStore = await cookies();
  const resolved = resolveUiPreferences({
    theme: cookieStore.get(UI_THEME_COOKIE_NAME)?.value,
    locale: cookieStore.get(UI_LOCALE_COOKIE_NAME)?.value,
  });
  const locale = resolved.locale;
  const session = await getCurrentToolsSession();

  return (
    <html lang={locale} className={inter.variable} suppressHydrationWarning>
      <head>
        <script
          id="netmetric-theme-init"
          dangerouslySetInnerHTML={{ __html: getThemeInitScript(resolved.theme) }}
        />
      </head>
      <body className="min-h-screen bg-background text-foreground">
        <ThemeProvider defaultTheme={resolved.theme}>
          <ToolsErrorMonitoring />
          <div className="flex min-h-screen flex-col">
            <a
              href="#main-content"
              className="sr-only left-4 top-4 z-modal rounded-sm bg-background px-3 py-2 focus:not-sr-only focus:absolute"
            >
              {tTools("tools.a11y.skipToContent", locale)}
            </a>
            <ToolsHeader locale={locale} isAuthenticated={session.isAuthenticated} />
            <main id="main-content" className="flex-1">
              {children}
            </main>
            <ToolsFooter locale={locale} />
          </div>
        </ThemeProvider>
      </body>
    </html>
  );
}
