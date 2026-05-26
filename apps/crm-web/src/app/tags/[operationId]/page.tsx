import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function TagsOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/tags", resolved.operationId);
}
