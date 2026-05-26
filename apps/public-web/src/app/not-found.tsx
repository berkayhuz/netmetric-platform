import { NotFoundState } from "@netmetric/ui";

import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tPublic } from "@/lib/i18n/public-i18n";

export default async function NotFoundPage() {
  const locale = await getRequestLocale();

  return (
    <main className="mx-auto flex min-h-[60vh] w-full max-w-3xl items-center justify-center px-6 py-10">
      <NotFoundState
        title={tPublic("error.notFoundTitle", locale)}
        description={tPublic("error.notFoundDescription", locale)}
        actionLabel={tPublic("public.actions.backToHome", locale)}
        actionHref="/"
      />
    </main>
  );
}
