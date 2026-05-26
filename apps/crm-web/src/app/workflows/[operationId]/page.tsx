import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function WorkflowsOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/workflows", resolved.operationId);
}
