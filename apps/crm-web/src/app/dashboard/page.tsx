import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { getDashboardData } from "@/features/dashboard/data/dashboard-data";
import { DashboardBuilder } from "@/features/widgets/components/dashboard-builder";
import {
  DashboardHeaderActions,
  DashboardTitleControl,
} from "@/features/widgets/components/dashboard-title-control";
import { getServerDashboardCollection } from "@/features/widgets/data/dashboard-preferences";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export default async function DashboardPage() {
  const session = await requireCrmSession("/dashboard");
  const locale = await getRequestLocale();
  const [data, initialCollection] = await Promise.all([
    getDashboardData("/dashboard"),
    getServerDashboardCollection(),
  ]);
  const selectedDashboard =
    initialCollection.active.find((item) => item.id === initialCollection.selectedDashboardId) ??
    initialCollection.active[0];
  const initialTitle =
    selectedDashboard?.name?.trim().toLowerCase() === "dashboard"
      ? "Dashboard"
      : (selectedDashboard?.name ?? "Dashboard");

  return (
    <CrmPageShell
      routePath="/dashboard"
      locale={locale}
      title={<DashboardTitleControl initialTitle={initialTitle} />}
      actions={<DashboardHeaderActions />}
    >
      <DashboardBuilder
        userId={session.profile.userId}
        initialCollection={initialCollection}
        seed={data}
      />
    </CrmPageShell>
  );
}
