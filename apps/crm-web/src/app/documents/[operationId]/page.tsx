import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function DocumentsOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/documents", resolved.operationId);
}
