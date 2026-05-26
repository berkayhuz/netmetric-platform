import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import { tCrm } from "@/lib/i18n/crm-i18n";

export function CrmContractPending({ module }: Readonly<{ module: string }>) {
  return (
    <Alert>
      <AlertTitle>{tCrm("crm.contractPending.title")}</AlertTitle>
      <AlertDescription>
        {tCrm("crm.contractPending.description", undefined, { module })}
      </AlertDescription>
    </Alert>
  );
}
