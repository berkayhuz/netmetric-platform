import {
  changePasswordAction,
  confirmEmailChangeAction,
  requestEmailChangeAction,
} from "@/features/account/actions/security-credential-actions";
import { PasswordSecurityManagementPanel } from "@/features/account/components/password-security-management-panel";
import {
  getMfaStatusForPage,
  getSessionsAndDevicesForPage,
} from "@/features/account/data/security-read-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function SecurityPasswordPage() {
  await requireAccountSession("/security/password");

  let mfaStatus;
  let sessions;
  try {
    [mfaStatus, sessions] = await Promise.all([
      getMfaStatusForPage(),
      getSessionsAndDevicesForPage().then((result) => result.sessions),
    ]);
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return (
    <PasswordSecurityManagementPanel
      mfaStatus={mfaStatus}
      sessions={sessions}
      passwordAction={changePasswordAction}
      emailRequestAction={requestEmailChangeAction}
      emailConfirmAction={confirmEmailChangeAction}
    />
  );
}
