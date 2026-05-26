import "server-only";

import {
  crmApiClient,
  type CrmPagedResult,
  type SupportInboxConnectionDto,
  type SupportInboxMessageDto,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";

export type SupportInboxReadQuery = {
  connectionId?: string;
  linkedToTicket?: boolean;
  page: number;
  pageSize: number;
};

export type SupportInboxDataResult = {
  connections: SupportInboxConnectionDto[];
  messages: CrmPagedResult<SupportInboxMessageDto>;
};

export async function getSupportInboxData(
  query: SupportInboxReadQuery,
  returnPath: string,
): Promise<SupportInboxDataResult> {
  await requireCrmSession("/");

  try {
    const options = await getCrmApiRequestOptions();

    const [connections, messages] = await Promise.all([
      crmApiClient.listSupportInboxConnections(options),
      crmApiClient.listSupportInboxMessages(
        {
          ...(query.connectionId ? { connectionId: query.connectionId } : {}),
          ...(query.linkedToTicket !== undefined ? { linkedToTicket: query.linkedToTicket } : {}),
          page: query.page,
          pageSize: query.pageSize,
        },
        options,
      ),
    ]);

    return { connections, messages };
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
