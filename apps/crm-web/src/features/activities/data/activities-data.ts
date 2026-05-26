import "server-only";

import { crmApiClient, type ActivityTimelineFeed, type ActivityTimelineItem } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export type ActivitiesListQuery = {
  page: number;
  pageSize: number;
  type?: string;
  sourceModule?: string;
  ownerUserId?: string;
  from?: string;
  to?: string;
};

export function parseActivitiesListQuery(
  searchParams: Record<string, string | string[] | undefined>,
): ActivitiesListQuery {
  const pageRaw = Array.isArray(searchParams.page) ? searchParams.page[0] : searchParams.page;
  const pageSizeRaw = Array.isArray(searchParams.pageSize)
    ? searchParams.pageSize[0]
    : searchParams.pageSize;
  const typeRaw = Array.isArray(searchParams.type) ? searchParams.type[0] : searchParams.type;
  const sourceModuleRaw = Array.isArray(searchParams.sourceModule)
    ? searchParams.sourceModule[0]
    : searchParams.sourceModule;
  const ownerUserIdRaw = Array.isArray(searchParams.ownerUserId)
    ? searchParams.ownerUserId[0]
    : searchParams.ownerUserId;
  const fromRaw = Array.isArray(searchParams.from) ? searchParams.from[0] : searchParams.from;
  const toRaw = Array.isArray(searchParams.to) ? searchParams.to[0] : searchParams.to;

  const pageValue = pageRaw ? Number(pageRaw) : 1;
  const pageSizeValue = pageSizeRaw ? Number(pageSizeRaw) : 20;

  const normalizedType = typeRaw?.trim() || undefined;
  const normalizedSourceModule = sourceModuleRaw?.trim() || undefined;
  const normalizedOwnerUserId = ownerUserIdRaw?.trim() || undefined;
  const normalizedFrom = fromRaw?.trim() || undefined;
  const normalizedTo = toRaw?.trim() || undefined;

  return {
    page: Number.isFinite(pageValue) && pageValue > 0 ? pageValue : 1,
    pageSize:
      Number.isFinite(pageSizeValue) && pageSizeValue > 0 ? Math.min(100, pageSizeValue) : 20,
    ...(normalizedType ? { type: normalizedType } : {}),
    ...(normalizedSourceModule ? { sourceModule: normalizedSourceModule } : {}),
    ...(normalizedOwnerUserId ? { ownerUserId: normalizedOwnerUserId } : {}),
    ...(normalizedFrom ? { from: normalizedFrom } : {}),
    ...(normalizedTo ? { to: normalizedTo } : {}),
  };
}

export async function getActivitiesData(
  query: ActivitiesListQuery,
  returnPath: string,
): Promise<ActivityTimelineFeed> {
  try {
    const options = await getCrmApiRequestOptions();
    return await crmApiClient.listActivities(
      {
        page: query.page,
        pageSize: query.pageSize,
        ...(query.type ? { type: query.type } : {}),
        ...(query.sourceModule ? { sourceModule: query.sourceModule } : {}),
        ...(query.ownerUserId ? { ownerUserId: query.ownerUserId } : {}),
        ...(query.from ? { from: query.from } : {}),
        ...(query.to ? { to: query.to } : {}),
      },
      options,
    );
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getActivityByIdData(
  activityId: string,
  returnPath: string,
): Promise<ActivityTimelineItem> {
  try {
    const options = await getCrmApiRequestOptions();
    return await crmApiClient.getActivity(activityId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
