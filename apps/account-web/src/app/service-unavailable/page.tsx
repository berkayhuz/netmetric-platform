import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";
import { AccountPagePanel } from "@/features/account/components/account-page-panel";
import { tAccountClient } from "@/lib/i18n/account-i18n";

export default function ServiceUnavailablePage() {
  return (
    <AccountPagePanel
      title={tAccountClient("account.statusPages.serviceUnavailable.title")}
      description={tAccountClient("account.statusPages.serviceUnavailable.description")}
      contentClassName="max-w-xl"
    >
      <Alert>
        <AlertTitle>
          {tAccountClient("account.statusPages.serviceUnavailable.alertTitle")}
        </AlertTitle>
        <AlertDescription>
          {tAccountClient("account.statusPages.serviceUnavailable.alertDescription")}
        </AlertDescription>
      </Alert>
    </AccountPagePanel>
  );
}
