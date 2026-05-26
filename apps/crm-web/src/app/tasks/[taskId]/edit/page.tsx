import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { getTaskByIdData } from "@/features/tasks/data/tasks-data";
import { TaskForm } from "@/features/tasks/forms/task-form";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

function toLocalDateTimeInput(value: string): string {
  const date = new Date(value);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  const hour = String(date.getHours()).padStart(2, "0");
  const minute = String(date.getMinutes()).padStart(2, "0");
  return `${year}-${month}-${day}T${hour}:${minute}`;
}

export default async function EditTaskPage({ params }: { params: Promise<{ taskId: string }> }) {
  const { taskId } = await params;
  await requireCrmSession(`/tasks/${taskId}/edit`);
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();
  const task = await getTaskByIdData(taskId, `/tasks/${taskId}/edit`);

  return (
    <CrmEntityFormShell
      locale={locale}
      title="Edit task"
      description="Update task details and scheduling information."
    >
      <TaskForm
        mode="edit"
        taskId={taskId}
        ownerUserOptions={references.ownerUsers}
        initialValues={{
          title: task.title,
          description: task.description,
          ownerUserId: task.ownerUserId ?? "",
          dueAtUtc: toLocalDateTimeInput(task.dueAtUtc),
          priority: task.priority,
        }}
      />
    </CrmEntityFormShell>
  );
}
