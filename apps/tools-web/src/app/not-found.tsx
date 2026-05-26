import { NotFoundState } from "@netmetric/ui";

import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";

export default async function NotFoundPage() {
  const locale = await getRequestLocale();

  return (
    <main className="mx-auto flex min-h-[60vh] w-full max-w-3xl items-center justify-center px-6 py-10">
      <NotFoundState
        title={tTools("error.notFoundTitle", locale)}
        description={tTools("error.notFoundDescription", locale)}
        actionLabel={tTools("tools.actions.backToCatalog", locale)}
        actionHref="/"
      />
    </main>
  );
}
