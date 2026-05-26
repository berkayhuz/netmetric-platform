import { Spinner } from "@netmetric/ui";

import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";

export default async function Loading() {
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <main className="flex min-h-screen items-center justify-center bg-background text-foreground">
      <div
        role="status"
        aria-live="polite"
        className="flex items-center gap-2 text-sm text-muted-foreground"
      >
        <Spinner />
        <span>{t("common.loading")}</span>
      </div>
    </main>
  );
}
