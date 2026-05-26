import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function ContractsOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/contracts", resolved.operationId);
}
