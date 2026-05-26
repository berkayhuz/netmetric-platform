import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function CustomerIntelligenceOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/customer-intelligence", resolved.operationId);
}
