import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function OmnichannelOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/omnichannel", resolved.operationId);
}
