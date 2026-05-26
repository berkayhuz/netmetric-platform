import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function KnowledgeBaseOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/knowledge-base", resolved.operationId);
}
