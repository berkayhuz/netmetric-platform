import { AccessDeniedState } from "@netmetric/ui";
import { AccountPagePanel } from "@/features/account/components/account-page-panel";
import { tAccountClient } from "@/lib/i18n/account-i18n";

export default function AccessDeniedPage() {
  return (
    <AccountPagePanel
      title={tAccountClient("account.statusPages.accessDenied.title")}
      description={tAccountClient("account.statusPages.accessDenied.description")}
      contentClassName="max-w-xl"
    >
      <AccessDeniedState
        title={tAccountClient("account.statusPages.accessDenied.alertTitle")}
        description={tAccountClient("account.statusPages.accessDenied.alertDescription")}
      />
    </AccountPagePanel>
  );
}
