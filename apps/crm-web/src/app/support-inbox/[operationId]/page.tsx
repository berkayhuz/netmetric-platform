import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function SupportInboxOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/support-inbox", resolved.operationId);
}
