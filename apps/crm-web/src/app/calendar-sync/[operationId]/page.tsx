import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function CalendarSyncOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/calendar-sync", resolved.operationId);
}
