import Link from "next/link";
import { CalendarClock, Plus } from "lucide-react";

import {
  CrmPageHeaderActionLink,
  CrmPageHeaderActions,
} from "@/components/shell/crm-page-header-actions";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CrmPagination } from "@/components/shell/crm-pagination";
import { toListQuery } from "@/features/shared/data/query";
import { TasksWorkspace } from "@/features/tasks/components/tasks-workspace";
import { getTasksData } from "@/features/tasks/data/tasks-data";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function TasksPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/tasks");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const capabilities = session.capabilities;
  const canCreateTasks = crmCapabilityAllows(capabilities, "tasks.create");
  const canEditTasks = crmCapabilityAllows(capabilities, "tasks.edit");
  const canManageTasks = crmCapabilityAllows(capabilities, "tasks.manage");
  const canDeleteTasks = crmCapabilityAllows(capabilities, "tasks.delete");

  const params = await searchParams;
  const queueParam = Array.isArray(params.queue) ? params.queue[0] : params.queue;
  const activeQueue = queueParam === "meetings" ? "meetings" : "tasks";
  const query = toListQuery(params);
  const { paged, meetings, lifecycleApiAvailable } = await getTasksData(query, "/tasks");

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }
  const queueTasksQuery = new URLSearchParams(currentQuery);
  queueTasksQuery.set("queue", "tasks");
  const queueMeetingsQuery = new URLSearchParams(currentQuery);
  queueMeetingsQuery.set("queue", "meetings");

  const queueSwitcher = (
    <div className="inline-flex rounded-lg border border-border/70 bg-muted/20 p-1">
      <Link
        href={`/tasks?${queueTasksQuery.toString()}`}
        className={`rounded-md px-3 py-1.5 text-sm transition ${
          activeQueue === "tasks"
            ? "bg-background font-medium text-foreground shadow-xs"
            : "text-muted-foreground hover:text-foreground"
        }`}
      >
        {tCrm("crm.tasks.table.caption", locale)}
      </Link>
      <Link
        href={`/tasks?${queueMeetingsQuery.toString()}`}
        className={`rounded-md px-3 py-1.5 text-sm transition ${
          activeQueue === "meetings"
            ? "bg-background font-medium text-foreground shadow-xs"
            : "text-muted-foreground hover:text-foreground"
        }`}
      >
        {tCrm("crm.tasks.meetings.title", locale)}
      </Link>
    </div>
  );
  const titleCounter =
    activeQueue === "tasks"
      ? `${paged.items.length} / ${paged.totalCount}`
      : String(meetings.length);
  return (
    <CrmPageShell
      routePath="/tasks"
      title={
        <span className="inline-flex items-center gap-2">
          <span>{tCrm("crm.tasks.page.title", locale)}</span>
          <span className="text-sm font-normal text-muted-foreground">{titleCounter}</span>
        </span>
      }
      locale={locale}
      actions={
        <CrmPageHeaderActions>
          {queueSwitcher}
          {canCreateTasks ? (
            <CrmPageHeaderActionLink
              href="/tasks/new"
              icon={<Plus aria-hidden="true" />}
              label={tCrm("crm.tasks.actions.create", locale)}
            />
          ) : null}
          {canCreateTasks ? (
            <CrmPageHeaderActionLink
              href="/tasks/meetings/new"
              icon={<CalendarClock aria-hidden="true" />}
              label={tCrm("crm.meetings.actions.schedule", locale)}
              variant="secondary"
            />
          ) : null}
        </CrmPageHeaderActions>
      }
    >
      <TasksWorkspace
        initialTab={activeQueue}
        dateSettings={dateSettings}
        initialSearch={query.search ?? ""}
        locale={locale}
        tasks={paged.items}
        meetings={meetings}
        canEditTasks={canEditTasks && lifecycleApiAvailable}
        canManageTasks={canManageTasks && lifecycleApiAvailable}
        canDeleteTasks={canDeleteTasks && lifecycleApiAvailable}
        lifecycleApiAvailable={lifecycleApiAvailable}
      />

      <CrmPagination
        currentPage={paged.pageNumber}
        totalPages={paged.totalPages}
        basePath="/tasks"
        currentQuery={currentQuery}
      />
    </CrmPageShell>
  );
}
