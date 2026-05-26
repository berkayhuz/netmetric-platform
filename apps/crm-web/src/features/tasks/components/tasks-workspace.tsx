"use client";

import { useMemo } from "react";
import { Badge } from "@netmetric/ui";

import { CrmRecordsTable, type CrmRecordsTableRow } from "@/components/shell/crm-records-table";
import type { MeetingScheduleDto, WorkTaskDto } from "@/lib/crm-api";
import {
  formatCrmDate,
  formatCrmDateTime,
  type CrmDateSettings,
} from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";
import { TaskListRowActions } from "./task-list-row-actions";

type TasksWorkspaceTab = "tasks" | "meetings";

export function TasksWorkspace({
  initialTab = "tasks",
  dateSettings,
  initialSearch,
  locale,
  tasks,
  meetings,
  canEditTasks,
  canManageTasks,
  canDeleteTasks,
  lifecycleApiAvailable,
}: Readonly<{
  initialTab?: TasksWorkspaceTab;
  dateSettings: CrmDateSettings;
  initialSearch: string;
  locale: string;
  tasks: WorkTaskDto[];
  meetings: MeetingScheduleDto[];
  canEditTasks: boolean;
  canManageTasks: boolean;
  canDeleteTasks: boolean;
  lifecycleApiAvailable: boolean;
}>) {
  const activeTab: TasksWorkspaceTab = initialTab;

  const taskRows: CrmRecordsTableRow[] = useMemo(
    () =>
      tasks.map((task) => {
        const status = tCrmWithFallback(
          `crm.tasks.status.${task.status}`,
          String(task.status),
          locale,
        );
        const priority = tCrmWithFallback(
          `crm.common.priority.${task.priority}`,
          String(task.priority),
          locale,
        );
        const dueDate = formatCrmDate(task.dueAtUtc, dateSettings);

        const row: CrmRecordsTableRow = {
          id: task.id,
          cells: {
            title: task.title,
            status,
            priority,
            dueDate,
            owner: task.ownerUserId || "-",
            ...(lifecycleApiAvailable ? { actions: "" } : {}),
          },
          descriptions: {
            title: task.description || undefined,
          },
          searchText: [task.title, task.description, status, priority, dueDate, task.ownerUserId]
            .filter(Boolean)
            .join(" "),
          filterValues: {
            status,
            priority,
          },
        };

        if (lifecycleApiAvailable) {
          row.href = `/tasks/${task.id}`;
        }

        return row;
      }),
    [dateSettings, locale, tasks],
  );

  const meetingRows: CrmRecordsTableRow[] = useMemo(
    () =>
      meetings.map((meeting) => {
        const externalSync = meeting.requiresExternalSync
          ? tCrm("crm.common.boolean.true", locale)
          : tCrm("crm.common.boolean.false", locale);

        return {
          id: meeting.id,
          cells: {
            title: meeting.title,
            startsAt: formatCrmDateTime(meeting.startsAtUtc, dateSettings),
            endsAt: formatCrmDateTime(meeting.endsAtUtc, dateSettings),
            organizer: meeting.organizerEmail || "-",
            externalSync,
          },
          searchText: [
            meeting.title,
            meeting.organizerEmail,
            formatCrmDateTime(meeting.startsAtUtc, dateSettings),
            externalSync,
          ]
            .filter(Boolean)
            .join(" "),
          filterValues: {
            externalSync,
          },
        };
      }),
    [dateSettings, locale, meetings],
  );

  return (
    <section>
      {activeTab === "tasks" ? (
        <CrmRecordsTable
          caption={tCrm("crm.tasks.table.caption", locale)}
          columns={[
            { key: "title", header: tCrm("crm.tasks.fields.title", locale) },
            { key: "status", header: tCrm("crm.tasks.fields.status", locale), badge: true },
            { key: "priority", header: tCrm("crm.tasks.fields.priority", locale), badge: true },
            { key: "dueDate", header: tCrm("crm.tasks.fields.dueDate", locale) },
            { key: "owner", header: tCrm("crm.tasks.fields.owner", locale) },
            ...(lifecycleApiAvailable
              ? [
                  {
                    key: "actions",
                    header: "Actions",
                    sortable: false,
                    render: (row: CrmRecordsTableRow) => {
                      const task = tasks.find((candidate) => candidate.id === row.id);
                      if (!task) {
                        return null;
                      }

                      return (
                        <TaskListRowActions
                          taskId={task.id}
                          taskStatus={task.status}
                          canEdit={canEditTasks}
                          canManage={canManageTasks}
                          canDelete={canDeleteTasks}
                        />
                      );
                    },
                  },
                ]
              : []),
          ]}
          rows={taskRows}
          filters={[
            {
              key: "status",
              label: tCrm("crm.tasks.fields.status", locale),
              allLabel: `All ${tCrm("crm.tasks.fields.status", locale).toLocaleLowerCase()}`,
              options: [...new Set(taskRows.map((row) => row.cells.status))]
                .filter((value): value is string => Boolean(value))
                .map((value) => ({
                  value,
                  label: value,
                })),
            },
            {
              key: "priority",
              label: tCrm("crm.tasks.fields.priority", locale),
              allLabel: `All ${tCrm("crm.tasks.fields.priority", locale).toLocaleLowerCase()}`,
              options: [...new Set(taskRows.map((row) => row.cells.priority))]
                .filter((value): value is string => Boolean(value))
                .map((value) => ({
                  value,
                  label: value,
                })),
            },
          ]}
          initialSearch={initialSearch}
          labels={{
            searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
            emptyTitle: tCrm("crm.tasks.empty.title", locale),
            emptyDescription: tCrm("crm.tasks.empty.description", locale),
          }}
          toolbarContent={
            <div className="flex items-center gap-2">
              {!lifecycleApiAvailable ? (
                <Badge variant="secondary">Task lifecycle API unavailable</Badge>
              ) : null}
            </div>
          }
        />
      ) : (
        <CrmRecordsTable
          caption={tCrm("crm.tasks.meetings.caption", locale)}
          columns={[
            { key: "title", header: tCrm("crm.tasks.fields.title", locale) },
            { key: "startsAt", header: tCrm("crm.tasks.fields.startsAt", locale) },
            { key: "endsAt", header: tCrm("crm.tasks.fields.endsAt", locale) },
            { key: "organizer", header: tCrm("crm.tasks.fields.organizer", locale) },
            {
              key: "externalSync",
              header: tCrm("crm.tasks.fields.externalSync", locale),
              badge: true,
            },
          ]}
          rows={meetingRows}
          filters={[
            {
              key: "externalSync",
              label: tCrm("crm.tasks.fields.externalSync", locale),
              allLabel: `All ${tCrm("crm.tasks.fields.externalSync", locale).toLocaleLowerCase()}`,
              options: [...new Set(meetingRows.map((row) => row.cells.externalSync))]
                .filter((value): value is string => Boolean(value))
                .map((value) => ({
                  value,
                  label: value,
                })),
            },
          ]}
          labels={{
            searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
            emptyTitle: tCrm("crm.tasks.meetings.emptyTitle", locale),
            emptyDescription: tCrm("crm.tasks.meetings.emptyDescription", locale),
          }}
        />
      )}
    </section>
  );
}
