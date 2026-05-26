import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";
import { AccountPagePanel } from "@/features/account/components/account-page-panel";
import { tAccountClient } from "@/lib/i18n/account-i18n";

export default function RetryLaterPage() {
  return (
    <AccountPagePanel
      title={tAccountClient("account.statusPages.retryLater.title")}
      description={tAccountClient("account.statusPages.retryLater.description")}
      contentClassName="max-w-xl"
    >
      <Alert>
        <AlertTitle>{tAccountClient("account.statusPages.retryLater.alertTitle")}</AlertTitle>
        <AlertDescription>
          {tAccountClient("account.statusPages.retryLater.alertDescription")}
        </AlertDescription>
      </Alert>
    </AccountPagePanel>
  );
}
