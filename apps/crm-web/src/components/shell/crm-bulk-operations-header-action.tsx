import { Layers3 } from "lucide-react";

import { tCrm } from "@/lib/i18n/crm-i18n";

import { CrmPageHeaderActionLink } from "./crm-page-header-actions";

export function CrmBulkOperationsHeaderAction({
  basePath,
  locale,
}: Readonly<{
  basePath: "/leads" | "/deals" | "/opportunities";
  locale?: string | null;
}>) {
  return (
    <CrmPageHeaderActionLink
      href={`${basePath}/bulk-operations`}
      icon={<Layers3 aria-hidden="true" />}
      label={tCrm("crm.common.bulkOperations", locale)}
      variant="outline"
    />
  );
}
