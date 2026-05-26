import { AuthCard } from "@/features/auth/components/auth-card";
import { RecoveryCodeForm } from "@/features/auth/components/recovery-code-form";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { createAuthMetadata } from "@/features/auth/i18n/auth-metadata";

type RecoveryCodePageProps = {
  searchParams: Promise<{
    identifier?: string;
    email?: string;
    challengeId?: string;
    returnUrl?: string;
  }>;
};

export async function generateMetadata() {
  return createAuthMetadata(await getRequestLocale(), "auth.recovery.metaTitle");
}

export default async function RecoveryCodePage({ searchParams }: RecoveryCodePageProps) {
  const params = await searchParams;
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <AuthPageShell
      title={t("auth.recovery.pageTitle")}
      description={t("auth.recovery.pageDescription")}
    >
      <AuthCard
        title={t("auth.recovery.cardTitle")}
        description={t("auth.recovery.cardDescription")}
      >
        <RecoveryCodeForm
          locale={locale}
          identifier={params.identifier ?? params.email ?? ""}
          challengeId={params.challengeId ?? ""}
          returnUrl={params.returnUrl ?? ""}
        />
      </AuthCard>
    </AuthPageShell>
  );
}
