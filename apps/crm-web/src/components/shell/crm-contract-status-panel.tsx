import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import type { CrmModuleRegistryItem } from "@/features/modules/module-registry";
import { getCrmStatusLabel, tCrm } from "@/lib/i18n/crm-i18n";

export function CrmContractStatusPanel({
  moduleItem,
  locale,
}: Readonly<{ moduleItem: CrmModuleRegistryItem; locale?: string | null | undefined }>) {
  return (
    <Alert>
      <AlertTitle>
        {tCrm("crm.modules.common.implementationStatus", locale)}:{" "}
        {getCrmStatusLabel(moduleItem.status, locale)}
      </AlertTitle>
      <AlertDescription>
        {moduleItem.status === "contract_pending"
          ? tCrm("crm.modules.common.contractPendingDescription", locale)
          : tCrm("crm.modules.common.shellOnlyDescription", locale)}
      </AlertDescription>
    </Alert>
  );
}
