import "server-only";

import {
  crmApiClient,
  type ActivityTimelineFeed,
  type CrmListQuery,
  type CustomerListItemDto,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { handleCrmApiPageError } from "@/lib/crm-auth/handle-crm-api-page-error";

export async function getCustomersData(query: CrmListQuery, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listCustomers(query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerDetailData(customerId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getCustomerById(customerId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerDuplicateWarnings(customerId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.findCustomerDuplicates(customerId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerContactsData(
  customerId: string,
  query: CrmListQuery,
  returnPath: string,
) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listCustomerContacts(customerId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomer360Data(customerId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getCustomer360(customerId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerConsentsData(customerId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getCustomerConsents(customerId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerHierarchyData(customerId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getCustomerHierarchy(customerId, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerAuditTimelineData(customerId: string, returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.getCustomerAuditTimeline(
      customerId,
      { page: 1, pageSize: 50 },
      options,
    );
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerActivitiesTimelineData(
  customerId: string,
  returnPath: string,
  query: {
    from?: string;
    to?: string;
    page?: number;
    pageSize?: number;
  } = {},
): Promise<ActivityTimelineFeed> {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listRelatedActivities("customer", customerId, query, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export async function getCustomerImportBatchesData(returnPath: string) {
  try {
    const options = await getCrmApiRequestOptions();

    return await crmApiClient.listCustomerImportBatches(25, options);
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}

export type CustomerMetricsPeriod = "daily" | "weekly" | "monthly";

export type CustomerMetricsSnapshot = {
  corporateCount: number;
  individualCount: number;
  activeCount: number;
  corporateGrowthPercent: number;
  individualGrowthPercent: number;
};

export type CustomerMetricsByPeriod = Record<CustomerMetricsPeriod, CustomerMetricsSnapshot>;

function isCorporate(customerType: string | number): boolean {
  const normalized = String(customerType).toLowerCase();
  return normalized === "1" || normalized === "corporate";
}

function periodDurationMs(period: CustomerMetricsPeriod): number {
  if (period === "daily") return 24 * 60 * 60 * 1000;
  if (period === "weekly") return 7 * 24 * 60 * 60 * 1000;
  return 30 * 24 * 60 * 60 * 1000;
}

function calculateGrowth(current: number, previous: number): number {
  if (previous <= 0) {
    return current > 0 ? 100 : 0;
  }

  return ((current - previous) / previous) * 100;
}

function countCreatedWithin(
  items: CustomerListItemDto[],
  fromTime: number,
  toTime: number,
): number {
  return items.filter((item) => {
    const createdTime = new Date(item.createdAt).getTime();
    return Number.isFinite(createdTime) && createdTime >= fromTime && createdTime < toTime;
  }).length;
}

function buildMetricsForPeriod(
  period: CustomerMetricsPeriod,
  allCustomers: CustomerListItemDto[],
): CustomerMetricsSnapshot {
  const corporateCustomers = allCustomers.filter((item) => isCorporate(item.customerType));
  const individualCustomers = allCustomers.filter((item) => !isCorporate(item.customerType));
  const now = Date.now();
  const duration = periodDurationMs(period);
  const currentStart = now - duration;
  const previousStart = currentStart - duration;

  const currentCorporate = countCreatedWithin(corporateCustomers, currentStart, now);
  const previousCorporate = countCreatedWithin(corporateCustomers, previousStart, currentStart);
  const currentIndividual = countCreatedWithin(individualCustomers, currentStart, now);
  const previousIndividual = countCreatedWithin(individualCustomers, previousStart, currentStart);

  return {
    corporateCount: corporateCustomers.length,
    individualCount: individualCustomers.length,
    activeCount: allCustomers.filter((item) => item.isActive).length,
    corporateGrowthPercent: calculateGrowth(currentCorporate, previousCorporate),
    individualGrowthPercent: calculateGrowth(currentIndividual, previousIndividual),
  };
}

async function getAllCustomersForMetrics(
  options: Awaited<ReturnType<typeof getCrmApiRequestOptions>>,
) {
  const pageSize = 200;
  let page = 1;
  let totalPages = 1;
  const aggregated: CustomerListItemDto[] = [];

  while (page <= totalPages) {
    const result = await crmApiClient.listCustomers(
      { page, pageSize, sortBy: "createdAt", sortDirection: "desc" },
      options,
    );
    aggregated.push(...result.items);
    totalPages = result.totalPages;
    page += 1;
  }

  return aggregated;
}

export async function getCustomerMetricsData(returnPath: string): Promise<CustomerMetricsByPeriod> {
  try {
    const options = await getCrmApiRequestOptions();
    const allCustomers = await getAllCustomersForMetrics(options);

    return {
      daily: buildMetricsForPeriod("daily", allCustomers),
      weekly: buildMetricsForPeriod("weekly", allCustomers),
      monthly: buildMetricsForPeriod("monthly", allCustomers),
    };
  } catch (error) {
    handleCrmApiPageError(error, returnPath);
  }
}
