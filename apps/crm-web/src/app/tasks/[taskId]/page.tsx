import Link from "next/link";
import { Button } from "@netmetric/ui";

import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { getTaskByIdData } from "@/features/tasks/data/tasks-data";
import { TaskLifecyclePanel } from "@/features/tasks/components/task-lifecycle-panel";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDateTime } from "@/lib/date-time/crm-date-time";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

export default async function TaskDetailPage({ params }: { params: Promise<{ taskId: string }> }) {
  const { taskId } = await params;
  const session = await requireCrmSession(`/tasks/${taskId}`);
  const dateSettings = await getRequestDateSettings();
  const task = await getTaskByIdData(taskId, `/tasks/${taskId}`);
  const canEditTasks = crmCapabilityAllows(session.capabilities, "tasks.edit");
  const canManageTasks = crmCapabilityAllows(session.capabilities, "tasks.manage");
  const canDeleteTasks = crmCapabilityAllows(session.capabilities, "tasks.delete");

  return (
    <CrmPageShell title="Task detail" description="Review task lifecycle and schedule fields.">
      <div className="space-y-6">
        <section className="space-y-3 rounded-lg border border-border/60 p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h1 className="text-xl font-semibold">{task.title}</h1>
              <p className="text-sm text-muted-foreground">{task.description || "-"}</p>
            </div>
            {canEditTasks ? (
              <Button asChild size="sm" variant="outline" className="h-8">
                <Link href={`/tasks/${task.id}/edit`}>Edit task</Link>
              </Button>
            ) : null}
          </div>

          <dl className="grid gap-3 text-sm sm:grid-cols-2 lg:grid-cols-3">
            <div>
              <dt className="text-muted-foreground">Status</dt>
              <dd className="font-medium">{task.status}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Priority</dt>
              <dd className="font-medium">{task.priority}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Owner</dt>
              <dd className="font-medium">{task.ownerUserId || "-"}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Due date</dt>
              <dd className="font-medium">{formatCrmDateTime(task.dueAtUtc, dateSettings)}</dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Reminder</dt>
              <dd className="font-medium">
                {task.reminderAtUtc ? formatCrmDateTime(task.reminderAtUtc, dateSettings) : "-"}
              </dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Completed at</dt>
              <dd className="font-medium">
                {task.completedAtUtc ? formatCrmDateTime(task.completedAtUtc, dateSettings) : "-"}
              </dd>
            </div>
            <div>
              <dt className="text-muted-foreground">Completed by</dt>
              <dd className="font-medium">{task.completedByUserId || "-"}</dd>
            </div>
            <div className="sm:col-span-2">
              <dt className="text-muted-foreground">Completion note</dt>
              <dd className="font-medium">{task.completionNote || "-"}</dd>
            </div>
          </dl>
        </section>

        <TaskLifecyclePanel
          taskId={task.id}
          status={task.status}
          dueAtUtc={task.dueAtUtc}
          reminderAtUtc={task.reminderAtUtc ?? null}
          canEdit={canEditTasks}
          canManage={canManageTasks}
          canDelete={canDeleteTasks}
        />
      </div>
    </CrmPageShell>
  );
}
