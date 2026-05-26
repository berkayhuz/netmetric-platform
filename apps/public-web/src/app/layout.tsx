import type { Metadata } from "next";
import { cookies } from "next/headers";
import { Inter } from "next/font/google";
import { getThemeInitScript } from "@netmetric/ui";
import { ThemeProvider } from "@netmetric/ui/client";
import { resolveUiPreferences, UI_LOCALE_COOKIE_NAME, UI_THEME_COOKIE_NAME } from "@netmetric/i18n";

import { PublicFooter } from "@/features/public/components/public-footer";
import { PublicHeader } from "@/features/public/components/public-header";
import {
  getCompanyLinks,
  getLegalLinks,
  getPrimaryNavLinks,
} from "@/features/public/content/navigation";
import { tPublic } from "@/lib/i18n/public-i18n";
import { PublicErrorMonitoring } from "@/lib/error-monitoring";
import { getCurrentPublicSession } from "@/lib/auth/session";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { defaultSiteDescription, siteTitle } from "@/lib/metadata";
import { publicEnv } from "@/lib/public-env";

import "./globals.css";

const inter = Inter({
  subsets: ["latin"],
  display: "swap",
  variable: "--font-sans",
});

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();
  const description = defaultSiteDescription(locale);

  return {
    metadataBase: new URL(publicEnv.siteUrl),
    title: {
      default: siteTitle,
      template: `%s | ${siteTitle}`,
    },
    description,
    alternates: {
      canonical: publicEnv.siteUrl,
    },
    openGraph: {
      type: "website",
      url: publicEnv.siteUrl,
      title: siteTitle,
      description,
      siteName: siteTitle,
    },
    twitter: {
      card: "summary_large_image",
      title: siteTitle,
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
  const session = await getCurrentPublicSession();
  const headerLinks = [...getPrimaryNavLinks(locale), ...getCompanyLinks(locale)];
  const legalLinks = getLegalLinks(locale);

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
          <PublicErrorMonitoring />
          <div className="flex min-h-screen flex-col">
            <a
              href="#main-content"
              className="sr-only left-4 top-4 z-modal rounded-sm bg-background px-3 py-2 focus:not-sr-only focus:absolute"
            >
              {tPublic("public.a11y.skipToContent", locale)}
            </a>
            <PublicHeader
              links={headerLinks}
              session={{
                isAuthenticated: session.isAuthenticated,
              }}
              copy={{
                primaryAria: tPublic("public.a11y.primaryNavigation", locale),
                signIn: tPublic("public.actions.signIn", locale),
                account: tPublic("public.actions.account", locale),
                dashboard: tPublic("public.actions.dashboard", locale),
                signOut: tPublic("public.actions.signOut", locale),
                openCrm: tPublic("public.actions.openCrm", locale),
                openMenu: tPublic("public.a11y.openNavigationMenu", locale),
                navigationTitle: tPublic("public.nav.navigation", locale),
              }}
            />
            <main id="main-content" className="flex-1">
              {children}
            </main>
            <PublicFooter
              legalLinks={legalLinks}
              copy={{
                description: tPublic("public.footer.description", locale),
                platform: tPublic("public.footer.platform", locale),
                legal: tPublic("public.footer.legal", locale),
                apiEndpoint: tPublic("public.footer.apiEndpoint", locale),
              }}
            />
          </div>
        </ThemeProvider>
      </body>
    </html>
  );
}
