import { Badge, Text } from "@netmetric/ui";
import type { ReactNode } from "react";

import type { MfaStatusResponse } from "@/lib/account-api";
import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";

type MfaReadOnlyPanelProps = {
  mfaStatus: MfaStatusResponse;
};

export function MfaReadOnlyPanel({ mfaStatus }: MfaReadOnlyPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.mfa.title")}
      description={tAccountClient("account.mfa.readOnlyDescription")}
    >
      <AccountSection
        title={tAccountClient("account.mfa.statusTitle")}
        description={tAccountClient("account.mfa.statusDescription")}
        contentClassName="space-y-3"
      >
        <Row
          label={tAccountClient("account.mfa.enabledLabel")}
          value={
            mfaStatus.isEnabled ? (
              <Badge variant="secondary">{tAccountClient("account.common.enabled")}</Badge>
            ) : (
              <Badge variant="outline">{tAccountClient("account.common.disabled")}</Badge>
            )
          }
        />
        <Row
          label={tAccountClient("account.mfa.authenticatorConfigured")}
          value={
            mfaStatus.hasAuthenticator ? (
              <Badge variant="secondary">{tAccountClient("account.common.configured")}</Badge>
            ) : (
              <Badge variant="outline">{tAccountClient("account.common.notConfigured")}</Badge>
            )
          }
        />
        <Row
          label={tAccountClient("account.mfa.recoveryCodesRemaining")}
          value={<Text>{String(mfaStatus.recoveryCodesRemaining)}</Text>}
        />
      </AccountSection>
    </AccountPagePanel>
  );
}

function Row({ label, value }: { label: string; value: ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-3">
      <Text className="text-sm text-muted-foreground">{label}</Text>
      {value}
    </div>
  );
}
