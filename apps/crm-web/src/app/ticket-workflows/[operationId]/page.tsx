import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function TicketWorkflowsOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/ticket-workflows", resolved.operationId);
}
