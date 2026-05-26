import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function MarketingOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/marketing", resolved.operationId);
}
