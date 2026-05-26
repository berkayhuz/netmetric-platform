import { renderCrmOperationShell } from "@/features/modules/render-module-shell";

export default async function SalesForecastingOperationPage({
  params,
}: {
  params: Promise<{ operationId: string }>;
}) {
  const resolved = await params;
  return renderCrmOperationShell("/sales-forecasting", resolved.operationId);
}
