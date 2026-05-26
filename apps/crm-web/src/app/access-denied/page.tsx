import { AccessDeniedState, Heading, Text } from "@netmetric/ui";

import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tCrm } from "@/lib/i18n/crm-i18n";

export default async function AccessDeniedPage() {
  const locale = await getRequestLocale();

  return (
    <section className="space-y-4">
      <Heading level={2}>{tCrm("crm.statusPages.accessDenied.title", locale)}</Heading>
      <Text className="text-muted-foreground">
        {tCrm("crm.statusPages.accessDenied.description", locale)}
      </Text>
      <AccessDeniedState
        title={tCrm("crm.statusPages.accessDenied.alertTitle", locale)}
        description={tCrm("crm.statusPages.accessDenied.alertDescription", locale)}
      />
    </section>
  );
}
