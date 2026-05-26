import Link from "next/link";

import { Button } from "@netmetric/ui";

import { authHeaderLinks } from "@/features/auth/config/auth-links";
import { authRoutes } from "@/features/auth/config/auth-routes";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { AppLogo } from "@/shared/components/app-logo";

export async function AuthHeader() {
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <header className="border-b bg-background/80 backdrop-blur">
      <div className="mx-auto flex h-16 w-full max-w-6xl items-center justify-between px-6">
        <AppLogo />

        <nav aria-label={t("nav.auth")} className="flex items-center gap-2">
          {authHeaderLinks.map((link) => {
            const isPrimary = link.href === authRoutes.register;
            return (
              <Button key={link.href} asChild variant={isPrimary ? "default" : "ghost"}>
                <Link href={link.href}>{t(link.labelKey)}</Link>
              </Button>
            );
          })}
        </nav>
      </div>
    </header>
  );
}
