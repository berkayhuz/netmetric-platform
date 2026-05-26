import "server-only";

import {
  accountApiClient,
  type AccountInvitationSummaryResponse,
  type AccountMemberResponse,
  type AccountRoleCatalogResponse,
} from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";

export type TeamReadData = {
  members: AccountMemberResponse[];
  rolesCatalog: AccountRoleCatalogResponse[];
  invitations: AccountInvitationSummaryResponse[];
};

export async function getTeamReadDataForPage(): Promise<TeamReadData> {
  const requestOptions = await getAccountApiRequestOptions();
  const [members, rolesCatalog, invitations] = await Promise.all([
    accountApiClient.listMembers(requestOptions),
    accountApiClient.listRoleCatalog(requestOptions),
    accountApiClient.listInvitations(requestOptions),
  ]);

  return { members, rolesCatalog, invitations };
}
