import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function AiOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/ai", resolved.operationId);
}
