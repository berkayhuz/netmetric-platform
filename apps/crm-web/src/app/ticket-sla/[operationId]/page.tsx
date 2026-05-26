import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function TicketSlaOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/ticket-sla", resolved.operationId);
}
