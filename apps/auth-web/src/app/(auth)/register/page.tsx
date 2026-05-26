import Link from "next/link";

import { AuthCard } from "@/features/auth/components/auth-card";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { RegisterForm } from "@/features/auth/components/register-form";
import { authRoutes } from "@/features/auth/config/auth-routes";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { createAuthMetadata } from "@/features/auth/i18n/auth-metadata";
import { requireAnonymousAuthPage } from "@/features/auth/server/anonymous-only";

type RegisterPageProps = {
  searchParams: Promise<{ returnUrl?: string }>;
};

export async function generateMetadata() {
  return createAuthMetadata(await getRequestLocale(), "auth.register.metaTitle");
}

export default async function RegisterPage({ searchParams }: RegisterPageProps) {
  const params = await searchParams;
  await requireAnonymousAuthPage(params.returnUrl);

  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <AuthPageShell
      title={t("auth.register.pageTitle")}
      description={t("auth.register.pageDescription")}
    >
      <AuthCard
        title={t("auth.register.cardTitle")}
        description={t("auth.register.cardDescription")}
        footer={
          <p className="w-full text-center text-sm text-muted-foreground">
            {t("auth.register.hasAccount")}{" "}
            <Link
              href={authRoutes.login}
              className="font-medium text-foreground underline-offset-4 hover:underline"
            >
              {t("auth.register.goLogin")}
            </Link>
          </p>
        }
      >
        <RegisterForm locale={locale} returnUrl={params.returnUrl ?? ""} />
      </AuthCard>
    </AuthPageShell>
  );
}
