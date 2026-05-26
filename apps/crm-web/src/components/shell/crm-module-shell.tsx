import { Badge, Text } from "@netmetric/ui";
import { Compass, LockKeyhole } from "lucide-react";

import { getCrmModuleByPath } from "@/features/modules/module-registry";
import {
  getCrmGroupLabel,
  getCrmModuleDescription,
  getCrmModuleTitle,
  tCrm,
} from "@/lib/i18n/crm-i18n";

import { CrmSectionCard } from "./crm-content-primitives";
import { CrmEmptyState } from "./crm-empty-state";
import { CrmPageShell } from "./crm-page-shell";

export function CrmModuleShell({
  path,
  locale,
}: Readonly<{ path: string; locale?: string | null }>) {
  const moduleItem = getCrmModuleByPath(path);

  if (!moduleItem) {
    return (
      <CrmPageShell
        title={tCrm("crm.modules.common.module", locale)}
        description={tCrm("crm.modules.common.notFound", locale)}
      >
        <CrmEmptyState
          title={tCrm("crm.modules.common.notFound", locale)}
          description={tCrm("crm.modules.common.notFoundDetail", locale)}
          icon={<Compass aria-hidden="true" />}
        />
      </CrmPageShell>
    );
  }

  return (
    <CrmPageShell
      title={getCrmModuleTitle(moduleItem, locale)}
      description={getCrmModuleDescription(moduleItem, locale)}
    >
      <CrmSectionCard
        title={tCrm("crm.modules.common.workspaceReadyTitle", locale)}
        description={tCrm("crm.modules.common.workspaceReadyDescription", locale)}
        actions={<Badge variant="outline">{getCrmGroupLabel(moduleItem.group, locale)}</Badge>}
      >
        <div className="flex items-start gap-3 rounded-lg border bg-muted/15 p-4">
          <div className="rounded-md border bg-background p-2 text-muted-foreground">
            <LockKeyhole aria-hidden="true" className="size-4" />
          </div>
          <Text className="text-sm leading-6 text-muted-foreground">
            {tCrm("crm.modules.common.workspaceUnavailable", locale)}
          </Text>
        </div>
      </CrmSectionCard>
    </CrmPageShell>
  );
}
