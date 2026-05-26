import "server-only";

import { crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import {
  crmGeneratedWidgetDefinitions,
  type CrmGeneratedWidgetDefinition,
} from "@/features/widgets/registry/widget-catalog";
import type {
  DashboardGenericWidgetData,
  DashboardWidgetDataPoint,
} from "@/features/widgets/types";

export type DashboardData = {
  customerTotal: number;
  companyTotal: number;
  contactTotal: number;
  recentCustomers: Array<{ id: string; name: string; subtitle: string }>;
  recentCompanies: Array<{ id: string; name: string; subtitle: string }>;
  recentContacts: Array<{ id: string; name: string; subtitle: string }>;
  dealsPipeline: {
    totalValue: number;
    weightedValue: number;
    totalDeals: number;
    weeklyDeltaPct: number;
    stages: Array<{
      name: "Lead" | "Contact" | "Proposal" | "Negotiate" | "Won";
      count: number;
      value: number;
    }>;
  };
  opportunities: {
    openValue: number;
    items: Array<{
      id: string;
      name: string;
      value: number;
      stage: string;
      probability: number;
    }>;
  };
  tasksDueToday: {
    total: number;
    completed: number;
    items: Array<{ id: string; title: string; dueAtUtc: string; priority: number; done: boolean }>;
  };
  slaRiskAlerts: {
    atRiskCount: number;
    items: Array<{ id: string; subject: string; severity: string; timeLeftLabel: string }>;
  };
  salesForecast: {
    quota: number;
    closedValue: number;
    pipelineCoverage: number;
    winProbabilityPct: number;
  };
  crmWidgets: Record<string, DashboardGenericWidgetData>;
};

const RECENT_PAGE_SIZE = 5;
const DASHBOARD_SAMPLE_SIZE = 50;
const MS_HOUR = 1000 * 60 * 60;
const SAMPLE_PAGE_SIZE = 100;

type ApiRecord = Record<string, unknown>;

type DashboardSourceSnapshot = {
  items: ApiRecord[];
  totalCount: number;
  href: string;
};

function toNumber(value: unknown): number {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }
  return 0;
}

function toRecord(value: unknown): ApiRecord {
  return typeof value === "object" && value !== null ? (value as ApiRecord) : {};
}

function toArrayRecords(value: unknown): ApiRecord[] {
  return Array.isArray(value) ? value.map(toRecord) : [];
}

function toPagedRecords<TItem>(value: {
  items: TItem[];
  totalCount: number;
}): DashboardSourceSnapshot {
  return {
    items: value.items.map(toRecord),
    totalCount: value.totalCount,
    href: "",
  };
}

function toListRecords<TItem>(items: TItem[]): DashboardSourceSnapshot {
  return {
    items: items.map(toRecord),
    totalCount: items.length,
    href: "",
  };
}

function readString(record: ApiRecord, keys: string[]): string | null {
  for (const key of keys) {
    const value = record[key];
    if (typeof value === "string" && value.trim()) {
      return value;
    }
    if (typeof value === "number" || typeof value === "boolean") {
      return String(value);
    }
  }
  return null;
}

function readBoolean(record: ApiRecord, keys: string[]): boolean | null {
  for (const key of keys) {
    const value = record[key];
    if (typeof value === "boolean") {
      return value;
    }
  }
  return null;
}

function readDateMs(record: ApiRecord): number | null {
  const value = readString(record, [
    "createdAt",
    "createdAtUtc",
    "openedAt",
    "receivedAtUtc",
    "dueAtUtc",
    "nextContactDate",
    "estimatedCloseDate",
    "validUntil",
    "quoteDate",
    "occurredAtUtc",
  ]);
  if (!value) return null;
  const ms = new Date(value).getTime();
  return Number.isFinite(ms) ? ms : null;
}

function readDueDateMs(record: ApiRecord): number | null {
  const value = readString(record, [
    "dueAtUtc",
    "nextContactDate",
    "estimatedCloseDate",
    "validUntil",
    "firstResponseDueAt",
    "resolveDueAt",
  ]);
  if (!value) return null;
  const ms = new Date(value).getTime();
  return Number.isFinite(ms) ? ms : null;
}

function readTitle(record: ApiRecord): string {
  return (
    readString(record, [
      "name",
      "fullName",
      "title",
      "subject",
      "proposalTitle",
      "code",
      "dealCode",
      "leadCode",
      "opportunityCode",
      "quoteNumber",
      "ticketNumber",
      "emailAddress",
      "externalMessageId",
      "id",
    ]) ?? "Untitled"
  );
}

function readDetail(record: ApiRecord): string | null {
  return readString(record, [
    "email",
    "mobilePhone",
    "phone",
    "companyName",
    "customerName",
    "status",
    "priority",
    "description",
    "provider",
    "sourceModule",
    "entityType",
  ]);
}

function readOwner(record: ApiRecord): string | null {
  return readString(record, ["ownerUserId", "assignedUserId", "newOwnerUserId", "organizerEmail"]);
}

function readAmount(record: ApiRecord): number {
  return (
    toNumber(record.totalAmount) ||
    toNumber(record.grandTotal) ||
    toNumber(record.wonAmount) ||
    toNumber(record.unitPrice) ||
    toNumber(record.minimumBudget)
  );
}

function readExpectedRevenue(record: ApiRecord): number {
  const expected = toNumber(record.expectedRevenue);
  if (expected > 0) return expected;
  const estimated = toNumber(record.estimatedAmount) || toNumber(record.estimatedBudget);
  const probability = Math.max(0, Math.min(100, toNumber(record.probability)));
  return probability > 0 ? estimated * (probability / 100) : estimated;
}

function formatCurrencyValue(value: number): string {
  return new Intl.NumberFormat("en-US", {
    currency: "USD",
    maximumFractionDigits: 0,
    style: "currency",
  }).format(value);
}

function formatPercentValue(value: number): string {
  return `${value.toFixed(1)}%`;
}

function statusValue(record: ApiRecord): string {
  return readString(record, ["status", "outcome", "stage", "leadStatus", "newStatus"]) ?? "";
}

function priorityValue(record: ApiRecord): string {
  return readString(record, ["priority"]) ?? "";
}

function isOpenLike(record: ApiRecord): boolean {
  const status = statusValue(record);
  if (!status) return true;
  return !isClosedLike(status);
}

function isHighPriority(record: ApiRecord): boolean {
  const value = priorityValue(record).toLowerCase();
  const numeric = Number(value);
  return (
    value.includes("high") || value.includes("urgent") || (Number.isFinite(numeric) && numeric >= 3)
  );
}

function isToday(ms: number): boolean {
  const today = new Date();
  const dayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime();
  const dayEnd = dayStart + 1000 * 60 * 60 * 24 - 1;
  return ms >= dayStart && ms <= dayEnd;
}

function buildBreakdownItems(
  records: ApiRecord[],
  fieldReader: (record: ApiRecord) => string,
): DashboardWidgetDataPoint[] {
  const counts = new Map<string, number>();
  for (const record of records) {
    const key = fieldReader(record);
    if (!key) continue;
    counts.set(key, (counts.get(key) ?? 0) + 1);
  }
  return [...counts.entries()]
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5)
    .map(([label, value]) => ({ label, value: String(value), detail: "records" }));
}

function buildRecentItems(source: DashboardSourceSnapshot): DashboardWidgetDataPoint[] {
  return [...source.items]
    .sort((a, b) => (readDateMs(b) ?? 0) - (readDateMs(a) ?? 0))
    .slice(0, 5)
    .map((record) => ({
      label: readTitle(record),
      value: readDetail(record) ?? "Live CRM record",
      href: source.href,
    }));
}

function buildGenericWidgetData(
  definition: CrmGeneratedWidgetDefinition,
  source: DashboardSourceSnapshot,
): DashboardGenericWidgetData {
  const records = source.items;
  const activeCount = records.filter(
    (record) => readBoolean(record, ["isActive", "isEnabled"]) === true,
  ).length;
  const inactiveCount = records.filter(
    (record) => readBoolean(record, ["isActive", "isEnabled"]) === false,
  ).length;
  const assignedCount = records.filter((record) => readOwner(record) !== null).length;
  const dueDates = records.map(readDueDateMs).filter((value): value is number => value !== null);
  const amountTotal = records.reduce((sum, record) => sum + readAmount(record), 0);
  const expectedRevenue = records.reduce((sum, record) => sum + readExpectedRevenue(record), 0);
  const probabilities = records
    .map((record) => toNumber(record.probability))
    .filter((value) => value > 0);

  switch (definition.metric) {
    case "recentList":
      return {
        kind: "list",
        description: definition.description,
        href: definition.path,
        items: buildRecentItems({ ...source, href: definition.path }),
      };
    case "statusBreakdown":
      return {
        kind: "status",
        description: definition.description,
        href: definition.path,
        items: buildBreakdownItems(records, statusValue),
      };
    case "priorityBreakdown":
      return {
        kind: "status",
        description: definition.description,
        href: definition.path,
        items: buildBreakdownItems(records, priorityValue),
      };
    case "active":
      return {
        kind: "metric",
        value: activeCount,
        description: definition.description,
        href: definition.path,
        tone: "emerald",
      };
    case "inactive":
      return {
        kind: "metric",
        value: inactiveCount,
        description: definition.description,
        href: definition.path,
        tone: "amber",
      };
    case "open":
      return {
        kind: "metric",
        value: records.filter(isOpenLike).length,
        description: definition.description,
        href: definition.path,
        tone: "blue",
      };
    case "closed":
      return {
        kind: "metric",
        value: records.filter((record) => !isOpenLike(record)).length,
        description: definition.description,
        href: definition.path,
        tone: "emerald",
      };
    case "assigned":
      return {
        kind: "metric",
        value: assignedCount,
        description: definition.description,
        href: definition.path,
        tone: "violet",
      };
    case "unassigned":
      return {
        kind: "metric",
        value: Math.max(0, source.totalCount - assignedCount),
        description: definition.description,
        href: definition.path,
        tone: "amber",
      };
    case "highPriority":
      return {
        kind: "metric",
        value: records.filter(isHighPriority).length,
        description: definition.description,
        href: definition.path,
        tone: "rose",
      };
    case "overdue":
      return {
        kind: "metric",
        value: dueDates.filter((value) => value < Date.now()).length,
        description: definition.description,
        href: definition.path,
        tone: "rose",
      };
    case "dueToday":
      return {
        kind: "metric",
        value: dueDates.filter(isToday).length,
        description: definition.description,
        href: definition.path,
        tone: "cyan",
      };
    case "amountTotal":
      return {
        kind: "metric",
        value: formatCurrencyValue(amountTotal),
        description: definition.description,
        href: definition.path,
        tone: "emerald",
      };
    case "expectedRevenue":
      return {
        kind: "metric",
        value: formatCurrencyValue(expectedRevenue),
        description: definition.description,
        href: definition.path,
        tone: "orange",
      };
    case "probabilityAverage":
      return {
        kind: "metric",
        value: formatPercentValue(
          probabilities.length
            ? probabilities.reduce((sum, value) => sum + value, 0) / probabilities.length
            : 0,
        ),
        description: definition.description,
        href: definition.path,
        tone: "violet",
      };
    case "total":
    default:
      return {
        kind: "metric",
        value: source.totalCount,
        description: definition.description,
        href: definition.path,
        tone: "default",
      };
  }
}

function normalizeStageName(raw: unknown): "Lead" | "Contact" | "Proposal" | "Negotiate" | "Won" {
  const stage = String(raw ?? "").toLowerCase();
  if (stage.includes("contact")) return "Contact";
  if (stage.includes("proposal")) return "Proposal";
  if (stage.includes("negotiat")) return "Negotiate";
  if (stage.includes("won") || stage === "5") return "Won";
  return "Lead";
}

function isClosedLike(raw: unknown): boolean {
  const value = String(raw ?? "").toLowerCase();
  return (
    value.includes("closed") ||
    value.includes("done") ||
    value.includes("resolved") ||
    value.includes("won")
  );
}

function toTimeLeftLabel(dateIso?: string | null): string {
  if (!dateIso) {
    return "No due date";
  }
  const dueAt = new Date(dateIso).getTime();
  if (!Number.isFinite(dueAt)) {
    return "No due date";
  }
  const diffMs = dueAt - Date.now();
  if (diffMs <= 0) {
    return "Overdue";
  }
  const hours = Math.floor(diffMs / MS_HOUR);
  const minutes = Math.floor((diffMs % MS_HOUR) / (1000 * 60));
  if (hours <= 0) {
    return `${Math.max(1, minutes)}m left`;
  }
  return `${hours}h ${minutes}m left`;
}

export async function getDashboardData(returnPath = "/dashboard"): Promise<DashboardData> {
  const locale = await getRequestLocale();
  const noContactInfo = tCrm("crm.dashboard.noContactInfo", locale);

  try {
    const options = await getCrmApiRequestOptions();

    const [
      customers,
      companies,
      contacts,
      leads,
      opportunities,
      deals,
      quotes,
      tickets,
      products,
      productCategories,
      pipelines,
      workTasks,
      workManagement,
      activities,
      supportConnections,
      supportMessages,
      slaPolicies,
      workflowQueues,
      contracts,
      orders,
      tags,
      tagGroups,
      smartLabelRules,
      classificationSchemes,
      proposalTemplates,
      cpqWorkspace,
      dealLostReasons,
      opportunityLostReasons,
    ] = await Promise.all([
      crmApiClient.listCustomers({ page: 1, pageSize: RECENT_PAGE_SIZE }, options),
      crmApiClient.listCompanies({ page: 1, pageSize: RECENT_PAGE_SIZE }, options),
      crmApiClient.listContacts({ page: 1, pageSize: RECENT_PAGE_SIZE }, options),
      crmApiClient.listLeads({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listOpportunities({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listDeals({ page: 1, pageSize: DASHBOARD_SAMPLE_SIZE }, options),
      crmApiClient.listQuotes({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listTickets({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listProductCatalogItems({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listProductCatalogCategories({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listPipelines(options),
      crmApiClient.listWorkTasks({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.getWorkManagementWorkspace(options),
      crmApiClient.listActivities({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listSupportInboxConnections(options),
      crmApiClient.listSupportInboxMessages({ page: 1, pageSize: SAMPLE_PAGE_SIZE }, options),
      crmApiClient.listTicketSlaPolicies(options),
      crmApiClient.listTicketWorkflowQueues(options),
      crmApiClient.listContracts(options),
      crmApiClient.listOrders(options),
      crmApiClient.listTags(options),
      crmApiClient.listTagGroups(options),
      crmApiClient.listSmartLabelRules(options),
      crmApiClient.listClassificationSchemes(options),
      crmApiClient.listProposalTemplates(null, options),
      crmApiClient.getQuoteCpqWorkspace(options),
      crmApiClient.listDealLostReasons(options),
      crmApiClient.listOpportunityLostReasons(options),
    ]);

    const slaEscalationRules = (
      await Promise.all(
        slaPolicies.map((policy) => crmApiClient.listTicketSlaEscalationRules(policy.id, options)),
      )
    ).flat();

    const stageNames = ["Lead", "Contact", "Proposal", "Negotiate", "Won"] as const;
    const stageMap = new Map<(typeof stageNames)[number], { count: number; value: number }>([
      ["Lead", { count: 0, value: 0 }],
      ["Contact", { count: 0, value: 0 }],
      ["Proposal", { count: 0, value: 0 }],
      ["Negotiate", { count: 0, value: 0 }],
      ["Won", { count: 0, value: 0 }],
    ]);

    for (const deal of deals.items) {
      const stage = normalizeStageName(deal.stage);
      const bucket = stageMap.get(stage);
      if (!bucket) continue;
      bucket.count += 1;
      bucket.value += toNumber(deal.totalAmount);
    }

    const dealTotalValue = deals.items.reduce((sum, item) => sum + toNumber(item.totalAmount), 0);
    const weightedValue = opportunities.items.reduce(
      (sum, item) =>
        sum +
        (toNumber(item.expectedRevenue) || toNumber(item.estimatedAmount)) *
          (Math.max(0, Math.min(100, toNumber(item.probability))) / 100),
      0,
    );

    const openOpps = opportunities.items.filter((item) => !isClosedLike(item.status));
    const openOppValue = openOpps.reduce(
      (sum, item) => sum + (toNumber(item.expectedRevenue) || toNumber(item.estimatedAmount)),
      0,
    );
    const avgWinProb =
      openOpps.length > 0
        ? openOpps.reduce(
            (sum, item) => sum + Math.max(0, Math.min(100, toNumber(item.probability))),
            0,
          ) / openOpps.length
        : 0;

    const today = new Date();
    const dayStart = new Date(today.getFullYear(), today.getMonth(), today.getDate()).getTime();
    const dayEnd = dayStart + 1000 * 60 * 60 * 24 - 1;
    const dueToday = workTasks.items.filter((task) => {
      const due = new Date(task.dueAtUtc).getTime();
      return Number.isFinite(due) && due >= dayStart && due <= dayEnd;
    });

    const openTickets = tickets.items.filter((ticket) => !isClosedLike(ticket.status)).slice(0, 8);
    const ticketDetails = await Promise.all(
      openTickets.map(async (ticket) => {
        try {
          return await crmApiClient.getTicketById(ticket.id, options);
        } catch {
          return null;
        }
      }),
    );
    const riskItems = ticketDetails
      .filter((detail): detail is NonNullable<typeof detail> => detail !== null)
      .map((detail) => {
        const resolveDue = detail.resolveDueAt ?? detail.firstResponseDueAt;
        const riskLabel = String(detail.priority ?? "")
          .toLowerCase()
          .includes("high")
          ? "High"
          : "Medium";
        return {
          id: detail.id,
          subject: detail.subject,
          severity: riskLabel,
          timeLeftLabel: toTimeLeftLabel(resolveDue),
          timeLeftMs: resolveDue
            ? new Date(resolveDue).getTime() - Date.now()
            : Number.POSITIVE_INFINITY,
        };
      })
      .sort((a, b) => a.timeLeftMs - b.timeLeftMs)
      .slice(0, 3);

    const atRiskCount = riskItems.filter((item) => item.timeLeftMs <= MS_HOUR * 4).length;
    const weeklyDeltaPct =
      dealTotalValue > 0 ? Math.min(50, (weightedValue / Math.max(1, dealTotalValue)) * 10) : 0;
    const salesQuota = Math.max(1, Math.round((dealTotalValue + openOppValue) * 1.1));
    const sourceSnapshots: Record<string, DashboardSourceSnapshot> = {
      customers: { ...toPagedRecords(customers), href: "/customers" },
      companies: { ...toPagedRecords(companies), href: "/companies" },
      contacts: { ...toPagedRecords(contacts), href: "/contacts" },
      leads: { ...toPagedRecords(leads), href: "/leads" },
      opportunities: { ...toPagedRecords(opportunities), href: "/opportunities" },
      deals: { ...toPagedRecords(deals), href: "/deals" },
      quotes: { ...toPagedRecords(quotes), href: "/quotes" },
      tickets: { ...toPagedRecords(tickets), href: "/tickets" },
      products: { ...toPagedRecords(products), href: "/product-catalog" },
      product_categories: {
        ...toPagedRecords(productCategories),
        href: "/product-catalog/categories",
      },
      pipelines: { ...toListRecords(pipelines), href: "/pipeline" },
      proposal_templates: { ...toListRecords(proposalTemplates), href: "/quotes" },
      product_rules: { ...toListRecords(cpqWorkspace.productRules), href: "/quotes" },
      product_bundles: { ...toListRecords(cpqWorkspace.productBundles), href: "/quotes" },
      guided_selling_playbooks: {
        ...toListRecords(cpqWorkspace.guidedSellingPlaybooks),
        href: "/quotes",
      },
      deal_lost_reasons: { ...toListRecords(dealLostReasons), href: "/deals" },
      opportunity_lost_reasons: {
        ...toListRecords(opportunityLostReasons),
        href: "/opportunities",
      },
      support_connections: { ...toListRecords(supportConnections), href: "/support-inbox" },
      support_messages: { ...toPagedRecords(supportMessages), href: "/support-inbox" },
      sla_policies: { ...toListRecords(slaPolicies), href: "/ticket-sla" },
      sla_escalation_rules: { ...toListRecords(slaEscalationRules), href: "/ticket-sla" },
      workflow_queues: { ...toListRecords(workflowQueues), href: "/ticket-workflows" },
      tasks: { ...toPagedRecords(workTasks), href: "/tasks" },
      activities: {
        items: toArrayRecords(activities.items),
        totalCount: activities.totalCount,
        href: "/activities",
      },
      work_management: {
        items: [...workManagement.tasks, ...workManagement.meetings].map(toRecord),
        totalCount: workManagement.openTaskCount + workManagement.upcomingMeetingCount,
        href: "/work-management",
      },
      contracts: { ...toListRecords(contracts), href: "/contracts" },
      orders: { ...toListRecords(orders), href: "/finance" },
      tags: { ...toListRecords(tags), href: "/tags" },
      tag_groups: { ...toListRecords(tagGroups), href: "/tags" },
      smart_label_rules: { ...toListRecords(smartLabelRules), href: "/tags" },
      classification_schemes: { ...toListRecords(classificationSchemes), href: "/tags" },
    };
    const crmWidgets = Object.fromEntries(
      crmGeneratedWidgetDefinitions.map((definition) => [
        definition.id,
        buildGenericWidgetData(
          definition,
          sourceSnapshots[definition.source] ?? { items: [], totalCount: 0, href: definition.path },
        ),
      ]),
    );

    return {
      customerTotal: customers.totalCount,
      companyTotal: companies.totalCount,
      contactTotal: contacts.totalCount,
      recentCustomers: customers.items.map((item) => ({
        id: item.id,
        name: item.fullName,
        subtitle: item.email ?? item.mobilePhone ?? noContactInfo,
      })),
      recentCompanies: companies.items.map((item) => ({
        id: item.id,
        name: item.name,
        subtitle: item.email ?? item.phone ?? noContactInfo,
      })),
      recentContacts: contacts.items.map((item) => ({
        id: item.id,
        name: item.fullName,
        subtitle: item.email ?? item.mobilePhone ?? noContactInfo,
      })),
      dealsPipeline: {
        totalValue: dealTotalValue,
        weightedValue,
        totalDeals: deals.totalCount,
        weeklyDeltaPct,
        stages: stageNames.map((name) => ({
          name,
          count: stageMap.get(name)?.count ?? 0,
          value: stageMap.get(name)?.value ?? 0,
        })),
      },
      opportunities: {
        openValue: openOppValue,
        items: openOpps.slice(0, 8).map((item) => ({
          id: item.id,
          name: item.name,
          value: toNumber(item.expectedRevenue) || toNumber(item.estimatedAmount),
          stage: String(item.stage),
          probability: Math.max(0, Math.min(100, toNumber(item.probability))),
        })),
      },
      tasksDueToday: {
        total: dueToday.length,
        completed: dueToday.filter((item) => isClosedLike(item.status)).length,
        items: dueToday.slice(0, 8).map((item) => ({
          id: item.id,
          title: item.title,
          dueAtUtc: item.dueAtUtc,
          priority: item.priority,
          done: isClosedLike(item.status),
        })),
      },
      slaRiskAlerts: {
        atRiskCount,
        items: riskItems.map(({ id, subject, severity, timeLeftLabel }) => ({
          id,
          subject,
          severity,
          timeLeftLabel,
        })),
      },
      salesForecast: {
        quota: salesQuota,
        closedValue: dealTotalValue,
        pipelineCoverage: openOppValue,
        winProbabilityPct: avgWinProb,
      },
      crmWidgets,
    };
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
