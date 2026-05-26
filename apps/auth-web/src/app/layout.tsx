import type { Metadata } from "next";
import { cookies } from "next/headers";
import { Inter } from "next/font/google";
import { resolveUiPreferences, UI_LOCALE_COOKIE_NAME } from "@netmetric/i18n";
import { getThemeInitScript } from "@netmetric/ui";
import { ThemeProvider, Toaster } from "@netmetric/ui/client";

import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { AuthErrorMonitoring } from "@/lib/error-monitoring";

import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-sans",
});

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return {
    title: {
      default: t("meta.appTitle"),
      template: `%s | ${t("meta.appTitle")}`,
    },
    description: t("meta.appDescription"),
    metadataBase: new URL("http://localhost:7002"),
    robots: {
      index: false,
      follow: false,
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
    theme: "light",
    locale: cookieStore.get(UI_LOCALE_COOKIE_NAME)?.value,
  });
  const locale = resolved.locale;
  const t = getTranslator(locale);

  return (
    <html lang={locale} className={inter.variable} suppressHydrationWarning>
      <head>
        <script
          id="netmetric-theme-init"
          dangerouslySetInnerHTML={{ __html: getThemeInitScript(resolved.theme) }}
        />
      </head>
      <body>
        <ThemeProvider defaultTheme={resolved.theme} forcedTheme="light">
          <AuthErrorMonitoring />
          <a
            href="#main-content"
            className="sr-only left-4 top-4 z-modal rounded-sm bg-background px-3 py-2 focus:not-sr-only focus:absolute"
          >
            {t("common.skipToContent")}
          </a>
          <main id="main-content">{children}</main>
          <Toaster />
        </ThemeProvider>
      </body>
    </html>
  );
}
