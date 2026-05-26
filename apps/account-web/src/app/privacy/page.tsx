import { PrivacyConsentsPanel } from "@/features/account/components/privacy-consents-panel";
import { getConsentsForPage } from "@/features/account/data/privacy-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function PrivacyPage() {
  await requireAccountSession("/privacy");

  let consents;
  try {
    consents = await getConsentsForPage();
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return <PrivacyConsentsPanel consents={consents} />;
}
