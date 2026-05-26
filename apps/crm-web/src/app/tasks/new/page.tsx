import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { getCrmFormReferenceData } from "@/features/shared/data/form-reference-data";
import { TaskForm } from "@/features/tasks/forms/task-form";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewTaskPage() {
  await requireCrmSession("/tasks/new");
  const locale = await getRequestLocale();
  const references = await getCrmFormReferenceData();

  return (
    <CrmEntityFormShell routePath="/tasks/new" locale={locale}>
      <TaskForm ownerUserOptions={references.ownerUsers} />
    </CrmEntityFormShell>
  );
}
