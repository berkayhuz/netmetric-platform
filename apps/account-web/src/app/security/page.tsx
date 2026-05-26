import { SecurityOverviewPanel } from "@/features/account/components/security-overview-panel";
import { getSecurityOverviewForPage } from "@/features/account/data/security-read-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function SecurityPage() {
  await requireAccountSession("/security");

  let data;
  try {
    data = await getSecurityOverviewForPage();
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return (
    <SecurityOverviewPanel
      overview={data.overview}
      mfaStatus={data.mfaStatus}
      sessions={data.sessions}
      trustedDevices={data.trustedDevices}
    />
  );
}
