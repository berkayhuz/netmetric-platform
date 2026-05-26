import { CrmModuleShell } from "@/components/shell/crm-module-shell";
import { OperationalModuleOperationPage } from "@/features/modules/components/operational-module-workspace";
import { OperationalModuleWorkspace } from "@/features/modules/components/operational-module-workspace";
import { getOperationalModuleData } from "@/features/modules/data/operational-module-data";
import { getCrmModuleByPath } from "@/features/modules/module-registry";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export async function renderCrmModuleShell(path: string) {
  const locale = await getRequestLocale();
  const moduleItem = getCrmModuleByPath(path);
  const operationalData = await getOperationalModuleData(path);

  if (moduleItem && operationalData) {
    return (
      <OperationalModuleWorkspace moduleItem={moduleItem} data={operationalData} locale={locale} />
    );
  }

  await requireCrmSession(path);
  return <CrmModuleShell path={path} locale={locale} />;
}

export async function renderCrmOperationShell(path: string, operationId: string) {
  const locale = await getRequestLocale();
  const moduleItem = getCrmModuleByPath(path);
  const operationalData = await getOperationalModuleData(path);

  if (moduleItem && operationalData) {
    return (
      <OperationalModuleOperationPage
        moduleItem={moduleItem}
        data={operationalData}
        operationId={operationId}
        locale={locale}
      />
    );
  }

  await requireCrmSession(path);
  return <CrmModuleShell path={path} locale={locale} />;
}
