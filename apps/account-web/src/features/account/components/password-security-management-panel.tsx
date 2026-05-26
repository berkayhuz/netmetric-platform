import type { MfaStatusResponse, UserSessionsResponse } from "@/lib/account-api";

import type { MutationState } from "../actions/mutation-state";
import { EmailChangeConfirmPanel } from "./email-change-confirm-panel";
import { EmailChangeRequestForm } from "./email-change-request-form";
import { PasswordChangeForm } from "./password-change-form";
import { AccountPagePanel } from "./account-page-panel";
import { AccountField, AccountSection } from "./account-section";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type PasswordSecurityManagementPanelProps = {
  mfaStatus: MfaStatusResponse;
  sessions: UserSessionsResponse;
  passwordAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
  emailRequestAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
  emailConfirmAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
  confirmToken?: string;
};

export function PasswordSecurityManagementPanel({
  mfaStatus,
  sessions,
  passwordAction,
  emailRequestAction,
  emailConfirmAction,
  confirmToken,
}: PasswordSecurityManagementPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.security.passwordEmailTitle")}
      description={tAccountClient("account.security.passwordEmailDescription")}
      contentClassName="w-full lg:w-96"
    >
      <AccountSection
        title={tAccountClient("account.security.currentContext")}
        description={tAccountClient("account.security.currentContextDescription")}
        contentClassName="grid gap-3 sm:grid-cols-2"
      >
        <AccountField
          label={tAccountClient("account.mfa.statusTitle")}
          value={
            mfaStatus.isEnabled
              ? tAccountClient("account.common.enabled")
              : tAccountClient("account.common.disabled")
          }
        />
        <AccountField
          label={tAccountClient("account.sessions.activeTitle")}
          value={sessions.items.filter((item) => item.isActive).length}
        />
      </AccountSection>

      <PasswordChangeForm action={passwordAction} />
      <EmailChangeRequestForm action={emailRequestAction} />
      <EmailChangeConfirmPanel action={emailConfirmAction} tokenFromQuery={confirmToken} />
    </AccountPagePanel>
  );
}
