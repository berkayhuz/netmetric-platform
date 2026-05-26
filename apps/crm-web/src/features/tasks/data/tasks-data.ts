import "server-only";

import {
  crmApiClient,
  CrmApiError,
  type CrmListQuery,
  type CrmPagedResult,
  type MeetingScheduleDto,
  type WorkManagementWorkspaceDto,
  type WorkTaskDto,
} from "@/lib/crm-api";
import { normalizeListQuery } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export type TasksDataResult = {
  paged: CrmPagedResult<WorkTaskDto>;
  workspaceSummary: Pick<WorkManagementWorkspaceDto, "openTaskCount" | "upcomingMeetingCount">;
  meetings: MeetingScheduleDto[];
  lifecycleApiAvailable: boolean;
};

function paginateWorkspaceTasks(
  tasks: WorkTaskDto[],
  query: CrmListQuery,
): CrmPagedResult<WorkTaskDto> {
  const normalized = normalizeListQuery(query);
  const searchLower = normalized.search?.toLowerCase();

  const filtered = searchLower
    ? tasks.filter((task) => {
        const title = task.title.toLowerCase();
        const description = task.description.toLowerCase();
        const status = task.status.toLowerCase();
        return (
          title.includes(searchLower) ||
          description.includes(searchLower) ||
          status.includes(searchLower)
        );
      })
    : tasks;

  const startIndex = (normalized.page - 1) * normalized.pageSize;
  const items = filtered.slice(startIndex, startIndex + normalized.pageSize);
  const totalCount = filtered.length;
  const totalPages = Math.ceil(totalCount / normalized.pageSize) || 0;

  return {
    items,
    totalCount,
    pageNumber: normalized.page,
    pageSize: normalized.pageSize,
    totalPages,
  };
}

export async function getTasksData(
  query: CrmListQuery,
  returnPath: string,
): Promise<TasksDataResult> {
  try {
    const options = await getCrmApiRequestOptions();

    const workspacePromise = crmApiClient.getWorkManagementWorkspace(options);
    const pagedPromise = crmApiClient.listWorkTasks(
      {
        ...(query.page !== undefined ? { page: query.page } : {}),
        ...(query.pageSize !== undefined ? { pageSize: query.pageSize } : {}),
        ...(query.search !== undefined ? { search: query.search } : {}),
        ...(query.sortBy !== undefined ? { sortBy: query.sortBy } : {}),
        ...(query.sortDirection !== undefined ? { sortDirection: query.sortDirection } : {}),
        ...(query.filters !== undefined ? { filters: query.filters } : {}),
      },
      options,
    );

    const [pagedResult, workspace] = await Promise.allSettled([pagedPromise, workspacePromise]);

    if (workspace.status !== "fulfilled") {
      throw workspace.reason;
    }

    const isFallbackList =
      pagedResult.status !== "fulfilled" &&
      pagedResult.reason instanceof CrmApiError &&
      [404, 405, 501].includes(pagedResult.reason.status);

    const paged =
      pagedResult.status === "fulfilled"
        ? pagedResult.value
        : isFallbackList
          ? paginateWorkspaceTasks(workspace.value.tasks, query)
          : (() => {
              throw pagedResult.reason;
            })();

    return {
      paged,
      workspaceSummary: {
        openTaskCount: workspace.value.openTaskCount,
        upcomingMeetingCount: workspace.value.upcomingMeetingCount,
      },
      meetings: workspace.value.meetings,
      lifecycleApiAvailable: !isFallbackList,
    };
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getTaskByIdData(taskId: string, returnPath: string): Promise<WorkTaskDto> {
  try {
    const options = await getCrmApiRequestOptions();
    return await crmApiClient.getWorkTask(taskId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
