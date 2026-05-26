import { TeamManagementPanel } from "@/features/account/components/team-management-panel";
import { getTeamReadDataForPage } from "@/features/account/data/team-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function TeamSettingsPage() {
  await requireAccountSession("/settings/team");

  let teamData;
  try {
    teamData = await getTeamReadDataForPage();
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return <TeamManagementPanel teamData={teamData} />;
}
