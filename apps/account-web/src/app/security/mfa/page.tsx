import { MfaManagementPanel } from "@/features/account/components/mfa-management-panel";
import { getMfaStatusForPage } from "@/features/account/data/security-read-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function SecurityMfaPage() {
  await requireAccountSession("/security/mfa");

  let mfaStatus;
  try {
    mfaStatus = await getMfaStatusForPage();
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return <MfaManagementPanel mfaStatus={mfaStatus} />;
}
