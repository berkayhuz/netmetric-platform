import "server-only";

import {
  crmApiClient,
  type CompanyListItemDto,
  type ContactListItemDto,
  type CustomerListItemDto,
  type DealListItemDto,
  type LeadListItemDto,
  type OpportunityListItemDto,
  type ProductCatalogItemDto,
} from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";

export type CrmReferenceOption = {
  value: string;
  label: string;
};

export type CrmProductReferenceOption = CrmReferenceOption & {
  description?: string;
  unitPrice?: number;
  currencyCode?: string;
  defaultDiscountRate?: number;
  defaultTaxRate?: number;
};

export type CrmFormReferenceData = {
  customers: CrmReferenceOption[];
  companies: CrmReferenceOption[];
  contacts: CrmReferenceOption[];
  leads: CrmReferenceOption[];
  opportunities: CrmReferenceOption[];
  ownerUsers: CrmReferenceOption[];
  products: CrmProductReferenceOption[];
};

function uniqueByValue(options: CrmReferenceOption[]): CrmReferenceOption[] {
  const map = new Map<string, CrmReferenceOption>();
  for (const option of options) {
    if (!map.has(option.value)) {
      map.set(option.value, option);
    }
  }
  return [...map.values()];
}

function formatIdLabel(prefix: string, value: string): string {
  return `${prefix} (${value.slice(0, 8)})`;
}

function customerOption(item: CustomerListItemDto): CrmReferenceOption {
  const email = item.email ? ` - ${item.email}` : "";
  return { value: item.id, label: `${item.fullName}${email}` };
}

function companyOption(item: CompanyListItemDto): CrmReferenceOption {
  const sector = item.sector ? ` - ${item.sector}` : "";
  return { value: item.id, label: `${item.name}${sector}` };
}

function leadOption(item: LeadListItemDto): CrmReferenceOption {
  const company = item.companyName ? ` - ${item.companyName}` : "";
  return { value: item.id, label: `${item.fullName}${company}` };
}

function contactOption(item: ContactListItemDto): CrmReferenceOption {
  const company = item.companyName ? ` - ${item.companyName}` : "";
  return { value: item.id, label: `${item.fullName}${company}` };
}

function opportunityOption(item: OpportunityListItemDto): CrmReferenceOption {
  return { value: item.id, label: `${item.opportunityCode} - ${item.name}` };
}

function productOption(item: ProductCatalogItemDto): CrmProductReferenceOption {
  const codeName = `${item.code} - ${item.name}`;
  const price = item.unitPrice != null ? ` (${item.unitPrice} ${item.currencyCode})` : "";
  const option: CrmProductReferenceOption = {
    value: item.id,
    label: `${codeName}${price}`,
    currencyCode: item.currencyCode,
    defaultDiscountRate: item.defaultDiscountRate,
    defaultTaxRate: item.defaultTaxRate,
  };

  if (item.description != null) {
    option.description = item.description;
  }
  if (item.unitPrice != null) {
    option.unitPrice = item.unitPrice;
  }

  return option;
}

function ownerIdOptions(
  leads: LeadListItemDto[],
  opportunities: OpportunityListItemDto[],
  deals: DealListItemDto[],
): CrmReferenceOption[] {
  const allOwnerIds = [
    ...leads.map((item) => item.ownerUserId ?? ""),
    ...opportunities.map((item) => item.ownerUserId ?? ""),
    ...deals.map((item) => item.ownerUserId ?? ""),
  ].filter((value): value is string => Boolean(value));

  return uniqueByValue(
    allOwnerIds.map((id) => ({
      value: id,
      label: formatIdLabel("Owner", id),
    })),
  );
}

async function safeList<T>(loader: () => Promise<T>): Promise<T | null> {
  try {
    return await loader();
  } catch {
    return null;
  }
}

export async function getCrmFormReferenceData(): Promise<CrmFormReferenceData> {
  const options = await getCrmApiRequestOptions();

  const [
    customersResult,
    companiesResult,
    contactsResult,
    leadsResult,
    opportunitiesResult,
    dealsResult,
    productsResult,
  ] = await Promise.all([
    safeList(() => crmApiClient.listCustomers({ page: 1, pageSize: 100 }, options)),
    safeList(() => crmApiClient.listCompanies({ page: 1, pageSize: 100 }, options)),
    safeList(() => crmApiClient.listContacts({ page: 1, pageSize: 100 }, options)),
    safeList(() => crmApiClient.listLeads({ page: 1, pageSize: 100 }, options)),
    safeList(() => crmApiClient.listOpportunities({ page: 1, pageSize: 100 }, options)),
    safeList(() => crmApiClient.listDeals({ page: 1, pageSize: 100 }, options)),
    safeList(() => crmApiClient.listProductCatalogItems({ page: 1, pageSize: 200 }, options)),
  ]);

  const customers = customersResult?.items ?? [];
  const companies = companiesResult?.items ?? [];
  const contacts = contactsResult?.items ?? [];
  const leads = leadsResult?.items ?? [];
  const opportunities = opportunitiesResult?.items ?? [];
  const deals = dealsResult?.items ?? [];
  const products = (productsResult?.items ?? []).filter((item) => item.isActive);

  return {
    customers: uniqueByValue(customers.map(customerOption)),
    companies: uniqueByValue(companies.map(companyOption)),
    contacts: uniqueByValue(contacts.map(contactOption)),
    leads: uniqueByValue(leads.map(leadOption)),
    opportunities: uniqueByValue(opportunities.map(opportunityOption)),
    ownerUsers: ownerIdOptions(leads, opportunities, deals),
    products: uniqueByValue(products.map(productOption)),
  };
}
