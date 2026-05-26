import { Badge } from "@netmetric/ui";

import type { CrmModuleStatus } from "@/features/modules/module-registry";
import { getCrmStatusLabel } from "@/lib/i18n/crm-i18n";

const statusConfig: Record<CrmModuleStatus, { variant: "default" | "secondary" | "outline" }> = {
  active: { variant: "default" },
  read_only: { variant: "secondary" },
  contract_pending: { variant: "outline" },
  coming_soon: { variant: "outline" },
};

export function CrmModuleStatusBadge({
  status,
  locale,
}: Readonly<{ status: CrmModuleStatus; locale?: string | null | undefined }>) {
  const config = statusConfig[status];
  return <Badge variant={config.variant}>{getCrmStatusLabel(status, locale)}</Badge>;
}
