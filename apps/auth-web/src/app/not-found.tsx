import { NotFoundState } from "@netmetric/ui";

import { authRoutes } from "@/features/auth/config/auth-routes";
import { getRequestLocale, getTranslator } from "@/features/auth/i18n/auth-i18n.server";

export default async function NotFoundPage() {
  const locale = await getRequestLocale();
  const t = getTranslator(locale);

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6 text-foreground">
      <div className="w-full max-w-md">
        <NotFoundState
          title={t("error.notFoundTitle")}
          description={t("error.notFoundDescription")}
          actionLabel={t("common.backToLogin")}
          actionHref={authRoutes.login}
        />
      </div>
    </main>
  );
}
