import Link from "next/link";

import { authFooterLinks } from "@/features/auth/config/auth-links";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";

export async function AuthFooter() {
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <footer className="absolute bottom-0 left-6 right-6 lg:left-8 lg:right-8 lg:bottom-4 z-20">
      <div className="mx-auto flex min-h-16 w-full max-w-6xl flex-col items-center justify-between gap-3 px-6 py-5 text-sm text-muted-foreground sm:flex-row">
        <p>{t("nav.copyright", { year: new Date().getFullYear() })}</p>
        <nav aria-label={t("nav.legal")} className="flex items-center gap-4">
          {authFooterLinks.map((link) => (
            <Link key={link.href} href={link.href} className="hover:text-foreground">
              {t(link.labelKey)}
            </Link>
          ))}
        </nav>
      </div>
    </footer>
  );
}
