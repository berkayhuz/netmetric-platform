import { notFound } from "next/navigation";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { getCustomersData, getCustomerDetailData } from "@/features/customers/data/customers-data";
import { crmApiClient } from "@/lib/crm-api";
import { CustomerIntelligenceDashboard } from "@/features/customer-intelligence/components/customer-intelligence-dashboard";
import { CustomerIntelligenceMutationPanels } from "@/features/customer-intelligence/components/customer-intelligence-mutation-panels";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

type PageProps = {
  searchParams: Promise<{ customerId?: string }>;
};

function isGuid(value: string | undefined): boolean {
  if (!value) return false;
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
}

export default async function CustomerIntelligencePage({ searchParams }: PageProps) {
  const params = await searchParams;
  const session = await requireCrmSession("/customer-intelligence");
  const locale = await getRequestLocale();
  const canManageDuplicates = crmCapabilityAllows(
    session.capabilities,
    "customers.duplicates.review",
  );
  const canManageIntelligence = crmCapabilityAllows(
    session.capabilities,
    "customerIntelligence.read",
  );

  // Fetch first page of customers (up to 100) to populate the selector dropdown
  const customersResult = await getCustomersData(
    { page: 1, pageSize: 100 },
    "/customer-intelligence",
  );
  const customersList = customersResult?.items ?? [];

  if (customersList.length === 0) {
    return (
      <CrmPageShell
        title={tCrm("crm.modules.customer-intelligence.title", locale)}
        description="Unified behavioral telemetry, identity resolution, relationship graph, and custom health scoring."
      >
        <div className="flex flex-col items-center justify-center py-16 text-center border border-dashed border-border/40 rounded-2xl bg-muted/5">
          <p className="text-sm font-medium text-muted-foreground">
            No customers found in this CRM workspace. Create a customer to enable intelligence
            insights.
          </p>
        </div>
      </CrmPageShell>
    );
  }

  // Determine active customer ID
  let activeCustomerId = params.customerId;
  if (!isGuid(activeCustomerId)) {
    activeCustomerId = customersList[0]?.id;
  }

  if (!activeCustomerId || !isGuid(activeCustomerId)) {
    notFound();
  }

  // Fetch full details of the active customer
  const customerDetail = await getCustomerDetailData(activeCustomerId, "/customer-intelligence");
  if (!customerDetail) {
    notFound();
  }

  // Fetch CRM Customer Intelligence Workspace and Portal Summary payloads
  const options = await getCrmApiRequestOptions();

  let workspace: any = {
    customerId: activeCustomerId,
    activityStream: [],
    relationshipGraph: [],
    recentBehavioralEvents: [],
    linkedIdentities: [],
  };

  let portalSummary: any = {
    customerId: activeCustomerId,
    displayName: customerDetail.fullName,
    healthScore: 0,
    openTickets: 0,
    openOpportunities: 0,
    openInvoices: 0,
  };

  try {
    const wsResult = await crmApiClient.fetchOperationalEndpoint(
      `/api/customer-intelligence/customers/${activeCustomerId}/workspace`,
      {},
      options,
    );
    if (wsResult && typeof wsResult === "object") {
      workspace = wsResult;
    }
  } catch (error) {
    console.error("Failed to load customer intelligence workspace:", error);
  }

  try {
    const portalResult = await crmApiClient.fetchOperationalEndpoint(
      `/api/customer-intelligence/customers/${activeCustomerId}/portal-summary`,
      {},
      options,
    );
    if (portalResult && typeof portalResult === "object") {
      portalSummary = portalResult;
    }
  } catch (error) {
    console.error("Failed to load customer intelligence portal summary:", error);
  }

  return (
    <CrmPageShell
      title={tCrm("crm.modules.customer-intelligence.title", locale)}
      description="Unified behavioral telemetry, identity resolution, relationship graph, and custom health scoring."
    >
      <div className="mx-auto w-full pt-4 pb-16 px-4 md:px-6">
        <CustomerIntelligenceDashboard
          selectedCustomer={customerDetail}
          customersList={customersList}
          workspace={workspace}
          portalSummary={portalSummary}
          locale={locale}
        />
        <CustomerIntelligenceMutationPanels
          customerId={activeCustomerId}
          canManageDuplicates={canManageDuplicates}
          canManageIntelligence={canManageIntelligence}
        />
      </div>
    </CrmPageShell>
  );
}
