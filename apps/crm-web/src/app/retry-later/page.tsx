import { Alert, AlertDescription, AlertTitle, Heading, Text } from "@netmetric/ui";

import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tCrm } from "@/lib/i18n/crm-i18n";

export default async function RetryLaterPage() {
  const locale = await getRequestLocale();

  return (
    <section className="space-y-4">
      <Heading level={2}>{tCrm("crm.statusPages.retryLater.title", locale)}</Heading>
      <Text className="text-muted-foreground">
        {tCrm("crm.statusPages.retryLater.description", locale)}
      </Text>
      <Alert>
        <AlertTitle>{tCrm("crm.statusPages.retryLater.alertTitle", locale)}</AlertTitle>
        <AlertDescription>
          {tCrm("crm.statusPages.retryLater.alertDescription", locale)}
        </AlertDescription>
      </Alert>
    </section>
  );
}
