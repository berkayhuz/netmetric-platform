import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function FinanceOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/finance", resolved.operationId);
}
