import { AuthCard } from "@/features/auth/components/auth-card";
import { MfaForm } from "@/features/auth/components/mfa-form";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { createAuthMetadata } from "@/features/auth/i18n/auth-metadata";

type MfaPageProps = {
  searchParams: Promise<{
    identifier?: string;
    email?: string;
    challengeId?: string;
    returnUrl?: string;
  }>;
};

export async function generateMetadata() {
  return createAuthMetadata(await getRequestLocale(), "auth.mfa.metaTitle");
}

export default async function MfaPage({ searchParams }: MfaPageProps) {
  const params = await searchParams;
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <AuthPageShell title={t("auth.mfa.pageTitle")} description={t("auth.mfa.pageDescription")}>
      <AuthCard title={t("auth.mfa.cardTitle")} description={t("auth.mfa.cardDescription")}>
        <MfaForm
          locale={locale}
          identifier={params.identifier ?? params.email ?? ""}
          challengeId={params.challengeId ?? ""}
          returnUrl={params.returnUrl ?? ""}
        />
      </AuthCard>
    </AuthPageShell>
  );
}
