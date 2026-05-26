import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function IntegrationsOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/integrations", resolved.operationId);
}
