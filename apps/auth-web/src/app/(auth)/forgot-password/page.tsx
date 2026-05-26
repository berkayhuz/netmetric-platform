import Link from "next/link";

import { AuthCard } from "@/features/auth/components/auth-card";
import { ForgotPasswordForm } from "@/features/auth/components/forgot-password-form";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { authRoutes } from "@/features/auth/config/auth-routes";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { createAuthMetadata } from "@/features/auth/i18n/auth-metadata";
import { requireAnonymousAuthPage } from "@/features/auth/server/anonymous-only";

export async function generateMetadata() {
  return createAuthMetadata(await getRequestLocale(), "auth.forgot.metaTitle");
}

export default async function ForgotPasswordPage() {
  await requireAnonymousAuthPage();

  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <AuthPageShell
      title={t("auth.forgot.pageTitle")}
      description={t("auth.forgot.pageDescription")}
    >
      <AuthCard
        title={t("auth.forgot.cardTitle")}
        description={t("auth.forgot.cardDescription")}
        footer={
          <p className="w-full text-center text-sm text-muted-foreground">
            {t("auth.forgot.backToLoginPrefix")}{" "}
            <Link
              href={authRoutes.login}
              className="font-medium text-foreground underline-offset-4 hover:underline"
            >
              {t("link.login")}
            </Link>
          </p>
        }
      >
        <ForgotPasswordForm locale={locale} />
      </AuthCard>
    </AuthPageShell>
  );
}
