import Link from "next/link";

import { cn } from "@netmetric/ui";

import { authRoutes } from "@/features/auth/config/auth-routes";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";

type AppLogoProps = {
  className?: string;
};

export async function AppLogo({ className }: AppLogoProps) {
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <Link
      href={authRoutes.home}
      className={cn(
        "inline-flex items-center gap-3 rounded-sm text-sm font-semibold outline-none transition focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2",
        className,
      )}
      aria-label={t("nav.logoHomeAria")}
    >
      <span className="flex size-9 items-center justify-center rounded-xl bg-foreground text-background">
        N
      </span>
      <span className="tracking-tight">{t("common.brand")}</span>
    </Link>
  );
}
