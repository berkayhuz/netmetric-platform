import Link from "next/link";
import { ArrowUpRight, LockKeyhole } from "lucide-react";
import { Badge, Text, cn } from "@netmetric/ui";

import { crmNavIconColors, crmNavIcons } from "@/components/shell/crm-nav-icons";
import { CrmModuleStatusBadge } from "@/components/shell/crm-module-status-badge";
import {
  canNavigateCrmModule,
  crmModuleGroups,
  type CrmModuleRegistryItem,
} from "@/features/modules/module-registry";
import type { CrmCapabilities } from "@/lib/crm-auth/crm-capabilities";
import {
  getCrmGroupLabel,
  getCrmModuleDescription,
  getCrmModuleTitle,
  tCrm,
} from "@/lib/i18n/crm-i18n";

export function DashboardModuleGrid({
  modules,
  locale,
  capabilities,
}: Readonly<{
  modules: CrmModuleRegistryItem[];
  locale?: string | null | undefined;
  capabilities?: CrmCapabilities;
}>) {
  const groupedModules = crmModuleGroups
    .map((group) => ({
      group,
      items: modules.filter((moduleItem) => moduleItem.group === group),
    }))
    .filter((group) => group.items.length > 0);

  return (
    <div className="space-y-4">
      {groupedModules.map(({ group, items }) => (
        <section className="rounded-lg border bg-card p-4 shadow-xs" key={group}>
          <div className="mb-4 flex items-center justify-between gap-3">
            <div>
              <h3 className="text-sm font-semibold">{getCrmGroupLabel(group, locale)}</h3>
              <p className="text-xs text-muted-foreground">
                {items.length} {tCrm("crm.dashboard.modules.countLabel", locale)}
              </p>
            </div>
            <Badge variant="outline">{getCrmGroupLabel(group, locale)}</Badge>
          </div>
          <div className="grid gap-3 md:grid-cols-2 xl:grid-cols-3">
            {items.map((moduleItem) => (
              <DashboardModuleTile
                capabilities={capabilities}
                key={moduleItem.id}
                locale={locale}
                moduleItem={moduleItem}
              />
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

function DashboardModuleTile({
  capabilities,
  locale,
  moduleItem,
}: Readonly<{
  capabilities?: CrmCapabilities | undefined;
  locale?: string | null | undefined;
  moduleItem: CrmModuleRegistryItem;
}>) {
  const canOpen = canNavigateCrmModule(moduleItem, capabilities);
  const Icon = crmNavIcons[moduleItem.iconKey];
  const content = (
    <>
      <div className="flex items-start justify-between gap-3">
        <div className="flex min-w-0 items-start gap-3">
          <div className="rounded-md border bg-background p-2">
            <Icon
              aria-hidden="true"
              className={cn("size-4", crmNavIconColors[moduleItem.iconKey])}
            />
          </div>
          <div className="min-w-0 space-y-1">
            <Text className="truncate text-sm font-semibold">
              {getCrmModuleTitle(moduleItem, locale)}
            </Text>
            <Text className="line-clamp-2 text-xs leading-5 text-muted-foreground">
              {getCrmModuleDescription(moduleItem, locale)}
            </Text>
          </div>
        </div>
        <CrmModuleStatusBadge locale={locale} status={moduleItem.status} />
      </div>
      <div className="mt-4 flex items-center justify-between gap-3 text-xs">
        <span className="text-muted-foreground">{moduleItem.backendModuleFolder}</span>
        <span
          className={cn(
            "inline-flex items-center gap-1 font-medium",
            canOpen ? "text-primary" : "text-muted-foreground",
          )}
        >
          {canOpen ? (
            <>
              {tCrm("crm.modules.common.openModule", locale)}
              <ArrowUpRight aria-hidden="true" className="size-3.5" />
            </>
          ) : (
            <>
              <LockKeyhole aria-hidden="true" className="size-3.5" />
              {tCrm("crm.modules.common.workspaceUnavailable", locale)}
            </>
          )}
        </span>
      </div>
    </>
  );

  if (!canOpen) {
    return (
      <div className="rounded-lg border bg-muted/10 p-4 opacity-80" aria-disabled="true">
        {content}
      </div>
    );
  }

  return (
    <Link
      className="block rounded-lg border bg-background p-4 shadow-xs transition-colors hover:bg-muted/20 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
      href={moduleItem.path}
    >
      {content}
    </Link>
  );
}
