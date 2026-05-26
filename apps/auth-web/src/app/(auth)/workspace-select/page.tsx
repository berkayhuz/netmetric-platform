import { AuthCard } from "@/features/auth/components/auth-card";
import { WorkspaceSelectPanel } from "@/features/auth/components/workspace-select-panel";
import { AuthPageShell } from "@/features/auth/components/auth-page-shell";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";
import { createAuthMetadata } from "@/features/auth/i18n/auth-metadata";

type WorkspaceSelectPageProps = {
  searchParams: Promise<{ returnUrl?: string }>;
};

export async function generateMetadata() {
  return createAuthMetadata(await getRequestLocale(), "auth.workspace.metaTitle");
}

export default async function WorkspaceSelectPage({ searchParams }: WorkspaceSelectPageProps) {
  const params = await searchParams;
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <AuthPageShell
      title={t("auth.workspace.pageTitle")}
      description={t("auth.workspace.pageDescription")}
    >
      <AuthCard
        title={t("auth.workspace.cardTitle")}
        description={t("auth.workspace.cardDescription")}
      >
        <WorkspaceSelectPanel locale={locale} returnUrl={params.returnUrl ?? ""} />
      </AuthCard>
    </AuthPageShell>
  );
}
