import { translate, type MessageKey } from "@netmetric/i18n";

import type {
  CrmModuleGroup,
  CrmModuleRegistryItem,
  CrmModuleStatus,
} from "@/features/modules/module-registry";

const crmGroupKeyByValue: Record<CrmModuleGroup, string> = {
  core: "crm.modules.groups.core",
  sales: "crm.modules.groups.sales",
  service_support: "crm.modules.groups.serviceSupport",
  marketing: "crm.modules.groups.marketing",
  operations: "crm.modules.groups.operations",
  intelligence_ai: "crm.modules.groups.intelligenceAi",
  administration: "crm.modules.groups.administration",
};

const crmStatusKeyByValue: Record<CrmModuleStatus, string> = {
  active: "crm.modules.status.active",
  read_only: "crm.modules.status.readOnly",
  contract_pending: "crm.modules.status.contractPending",
  coming_soon: "crm.modules.status.comingSoon",
};

const endpointDiscoveryKeyByValue: Record<
  CrmModuleRegistryItem["endpointDiscoveryStatus"],
  string
> = {
  ready: "crm.modules.endpointDiscovery.ready",
  source_visible: "crm.modules.endpointDiscovery.sourceVisible",
  contract_pending: "crm.modules.endpointDiscovery.contractPending",
  disabled: "crm.modules.endpointDiscovery.disabled",
};

function toTitleCaseFromSlug(value: string): string {
  return value
    .split(/[-_]/g)
    .filter(Boolean)
    .map((token) => token.charAt(0).toUpperCase() + token.slice(1))
    .join(" ");
}

export function tCrm(
  key: string,
  localeInput?: string | null,
  params?: Record<string, string | number>,
): string {
  if (params) {
    return translate(key as MessageKey, { locale: localeInput, params });
  }

  return translate(key as MessageKey, { locale: localeInput });
}

function getClientLocale(): string | null {
  if (typeof document === "undefined") {
    return null;
  }

  return document.documentElement.lang || null;
}

export function tCrmClient(key: string, params?: Record<string, string | number>): string {
  return tCrm(key, getClientLocale(), params);
}

export function tCrmWithFallback(
  key: string,
  fallback: string,
  localeInput?: string | null,
  params?: Record<string, string | number>,
): string {
  const translated = tCrm(key, localeInput, params);
  return translated === key ? fallback : translated;
}

export function getCrmModuleTitle(
  moduleItem: CrmModuleRegistryItem,
  localeInput?: string | null,
): string {
  return tCrmWithFallback(moduleItem.titleKey, toTitleCaseFromSlug(moduleItem.id), localeInput);
}

export function getCrmModuleDescription(
  moduleItem: CrmModuleRegistryItem,
  localeInput?: string | null,
): string {
  return tCrmWithFallback(moduleItem.descriptionKey, moduleItem.id, localeInput);
}

export function getCrmModuleImplementationPhase(
  moduleItem: CrmModuleRegistryItem,
  localeInput?: string | null,
): string {
  return tCrmWithFallback(moduleItem.implementationPhaseKey, moduleItem.id, localeInput);
}

export function getCrmGroupLabel(group: CrmModuleGroup, localeInput?: string | null): string {
  return tCrmWithFallback(crmGroupKeyByValue[group], group, localeInput);
}

export function getCrmStatusLabel(status: CrmModuleStatus, localeInput?: string | null): string {
  return tCrmWithFallback(crmStatusKeyByValue[status], status.replace("_", " "), localeInput);
}

export function getCrmEndpointDiscoveryLabel(
  status: CrmModuleRegistryItem["endpointDiscoveryStatus"],
  localeInput?: string | null,
): string {
  return tCrmWithFallback(
    endpointDiscoveryKeyByValue[status],
    status.replace("_", " "),
    localeInput,
  );
}
