import { AuthCard } from "@/features/auth/components/auth-card";
import { ConfirmEmailPanel } from "@/features/auth/components/confirm-email-panel";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { createAuthMetadata } from "@/features/auth/i18n/auth-metadata";

type ConfirmEmailPageProps = {
  searchParams: Promise<{ tenantId?: string; userId?: string; token?: string; email?: string }>;
};

export async function generateMetadata() {
  return createAuthMetadata(await getRequestLocale(), "auth.confirm.metaTitle");
}

export default async function ConfirmEmailPage({ searchParams }: ConfirmEmailPageProps) {
  const params = await searchParams;
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <AuthPageShell
      title={t("auth.confirm.pageTitle")}
      description={t("auth.confirm.pageDescription")}
    >
      <AuthCard title={t("auth.confirm.cardTitle")} description={t("auth.confirm.cardDescription")}>
        <ConfirmEmailPanel
          locale={locale}
          tenantId={params.tenantId ?? ""}
          userId={params.userId ?? ""}
          token={params.token ?? ""}
          email={params.email ?? ""}
        />
      </AuthCard>
    </AuthPageShell>
  );
}
