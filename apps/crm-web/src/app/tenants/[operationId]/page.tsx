import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function TenantsOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/tenants", resolved.operationId);
}
