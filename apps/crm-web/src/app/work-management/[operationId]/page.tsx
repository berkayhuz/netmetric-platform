import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function WorkManagementOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/work-management", resolved.operationId);
}
