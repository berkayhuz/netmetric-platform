import { CrmEntityFormShell } from "@/components/forms/crm-entity-form-shell";
import { MeetingForm } from "@/features/tasks/forms/meeting-form";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function NewMeetingPage() {
  await requireCrmSession("/tasks/meetings/new");
  const locale = await getRequestLocale();

  return (
    <CrmEntityFormShell routePath="/tasks/meetings/new" locale={locale}>
      <MeetingForm />
    </CrmEntityFormShell>
  );
}
