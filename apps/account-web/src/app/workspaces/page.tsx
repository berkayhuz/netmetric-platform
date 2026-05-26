import { AccountPagePanel } from "@/features/account/components/account-page-panel";
import { WorkspaceManagementPanel } from "@/features/account/components/workspace-management-panel";
import { getOverviewForPage } from "@/features/account/data/account-read-data";
import { handleAccountApiPageError } from "@/lib/auth/handle-account-api-page-error";
import { requireAccountSession } from "@/lib/auth/require-account-session";

export default async function WorkspacesPage() {
  await requireAccountSession("/workspaces");

  let overview;
  try {
    overview = await getOverviewForPage();
  } catch (error) {
    handleAccountApiPageError(error);
  }

  return (
    <AccountPagePanel
      title="Workspaces"
      description="Create workspaces and switch the active company context."
      contentClassName="mr-auto w-full lg:w-96"
    >
      <WorkspaceManagementPanel organizations={overview.organizations} showHeading={false} />
    </AccountPagePanel>
  );
}
