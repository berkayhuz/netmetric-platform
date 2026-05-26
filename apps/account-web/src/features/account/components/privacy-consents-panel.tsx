import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from "@netmetric/ui";

import type { ConsentsResponse } from "@/lib/account-api";

import { AccountPagePanel } from "./account-page-panel";
import { AccountField, AccountSection } from "./account-section";
import { ConsentStatusCard } from "./consent-status-card";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type PrivacyConsentsPanelProps = {
  consents: ConsentsResponse;
};

export function PrivacyConsentsPanel({ consents }: PrivacyConsentsPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.privacy.title")}
      description={tAccountClient("account.privacy.description")}
    >
      {consents.items.length === 0 ? (
        <Empty className="border-none py-12" role="status" aria-live="polite">
          <EmptyHeader>
            <EmptyTitle>{tAccountClient("account.privacy.emptyTitle")}</EmptyTitle>
            <EmptyDescription>
              {tAccountClient("account.privacy.emptyDescription")}
            </EmptyDescription>
          </EmptyHeader>
        </Empty>
      ) : (
        <div className="space-y-4">
          <AccountSection
            title={tAccountClient("account.privacy.overviewTitle")}
            description={tAccountClient("account.privacy.overviewDescription")}
          >
            <AccountField
              label={tAccountClient("account.audit.returnedEntriesLabel")}
              value={consents.items.length}
            />
          </AccountSection>
          {consents.items.map((item) => (
            <ConsentStatusCard key={item.id} item={item} />
          ))}
        </div>
      )}
    </AccountPagePanel>
  );
}
