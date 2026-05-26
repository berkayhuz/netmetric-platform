import { ErrorState } from "@netmetric/ui";

import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tCrm } from "@/lib/i18n/crm-i18n";

export default async function ServiceUnavailablePage() {
  const locale = await getRequestLocale();

  return (
    <ErrorState
      title={tCrm("crm.statusPages.serviceUnavailable.title", locale)}
      description={tCrm("crm.statusPages.serviceUnavailable.description", locale)}
    />
  );
}
