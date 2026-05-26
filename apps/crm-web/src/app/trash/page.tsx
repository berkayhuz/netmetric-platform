import { FileText } from "lucide-react";

import { CrmPagination } from "@/components/shell/crm-pagination";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import {
  type CrmRecordsTableColumn,
  type CrmRecordsTableFilter,
  type CrmRecordsTableRow,
} from "@/components/shell/crm-records-table";
import { toListQuery } from "@/features/shared/data/query";
import { getTrashData } from "@/features/trash/data/trash-data";
import { TrashRecordsTable } from "@/features/trash/components/trash-records-table";
import type { GlobalTrashListItemDto } from "@/lib/crm-api";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDateTime, type CrmDateSettings } from "@/lib/date-time/crm-date-time";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";

function resolveEntityTypeLabel(entityType: string): string {
  const normalized = entityType.toLowerCase();

  if (normalized === "contact") {
    return "Contact";
  }

  if (normalized === "customer") {
    return "Customer";
  }

  if (normalized === "company") {
    return "Company";
  }
  if (normalized === "lead") {
    return "Lead";
  }
  if (normalized === "deal") {
    return "Deal";
  }
  if (normalized === "opportunity") {
    return "Opportunity";
  }
  if (normalized === "quote") {
    return "Quote";
  }
  if (normalized === "ticket") {
    return "Ticket";
  }
  if (normalized === "productcatalogitem") {
    return "Catalog product";
  }

  return entityType;
}

function resolveExpiresIn(expiresAtUtc: string): string {
  const now = Date.now();
  const expiresAt = Date.parse(expiresAtUtc);
  if (Number.isNaN(expiresAt)) {
    return "-";
  }

  const deltaMs = expiresAt - now;
  if (deltaMs <= 0) {
    return "Expired";
  }

  const days = Math.ceil(deltaMs / (1000 * 60 * 60 * 24));
  return days === 1 ? "1 day" : `${days} days`;
}

function toRows(
  items: GlobalTrashListItemDto[],
  dateSettings: CrmDateSettings,
): CrmRecordsTableRow[] {
  return items.map((item) => ({
    id: item.id,
    cells: {
      item: item.displayName,
      type: resolveEntityTypeLabel(item.entityType),
      entityType: item.entityType,
      deletedBy: item.deletedByDisplayName ?? item.deletedByUserId ?? "-",
      deletedAt: formatCrmDateTime(item.deletedAtUtc, dateSettings),
      expiresIn: resolveExpiresIn(item.expiresAtUtc),
      status: item.status,
    },
    descriptions: {
      item: item.summary ?? undefined,
    },
    searchText: `${item.displayName} ${item.summary ?? ""} ${item.entityType}`.trim(),
  }));
}

function buildFilters(items: GlobalTrashListItemDto[]): CrmRecordsTableFilter[] {
  const types = [...new Set(items.map((item) => item.entityType).filter(Boolean))]
    .sort((left, right) => left.localeCompare(right))
    .map((value) => ({ value, label: resolveEntityTypeLabel(value) }));

  if (types.length <= 1) {
    return [];
  }

  return [
    {
      key: "type",
      label: "Type",
      allLabel: "All types",
      options: types,
    },
  ];
}

export default async function TrashPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  const session = await requireCrmSession("/trash");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const canRestoreContact = crmCapabilityAllows(session.capabilities, "contacts.delete");
  const canRestoreCustomer = crmCapabilityAllows(session.capabilities, "customers.delete");
  const canRestoreCompany = crmCapabilityAllows(session.capabilities, "companies.delete");
  const canRestoreLead = crmCapabilityAllows(session.capabilities, "leads.delete");
  const canRestoreDeal = crmCapabilityAllows(session.capabilities, "deals.delete");
  const canRestoreOpportunity = crmCapabilityAllows(session.capabilities, "opportunities.delete");
  const canRestoreQuote = crmCapabilityAllows(session.capabilities, "quotes.delete");
  const canRestoreTicket = crmCapabilityAllows(session.capabilities, "tickets.delete");
  const canRestoreProductCatalogItem = crmCapabilityAllows(
    session.capabilities,
    "productCatalog.manage",
  );

  const params = await searchParams;
  const query = toListQuery(params);
  const entityTypeRaw = Array.isArray(params.entityType) ? params.entityType[0] : params.entityType;
  const entityType = entityTypeRaw?.trim() ? entityTypeRaw.trim().toLowerCase() : undefined;

  const data = await getTrashData(
    {
      ...query,
      sortBy: query.sortBy ?? "deletedAt",
      sortDirection: query.sortDirection ?? "desc",
      filters: {
        ...(entityType ? { entityType } : {}),
      },
    },
    "/trash",
  );

  const currentQuery = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (typeof value === "string") currentQuery.set(key, value);
    if (Array.isArray(value) && value[0]) currentQuery.set(key, value[0]);
  }

  const columns: CrmRecordsTableColumn[] = [
    { key: "item", header: "Item" },
    { key: "type", header: "Type", badge: true },
    { key: "deletedBy", header: "Deleted by" },
    { key: "deletedAt", header: "Deleted at" },
    { key: "expiresIn", header: "Expires in" },
    { key: "status", header: "Status", badge: true },
  ];
  const restorableEntityTypes: string[] = [];
  if (canRestoreContact) restorableEntityTypes.push("contact");
  if (canRestoreCustomer) restorableEntityTypes.push("customer");
  if (canRestoreCompany) restorableEntityTypes.push("company");
  if (canRestoreLead) restorableEntityTypes.push("lead");
  if (canRestoreDeal) restorableEntityTypes.push("deal");
  if (canRestoreOpportunity) restorableEntityTypes.push("opportunity");
  if (canRestoreQuote) restorableEntityTypes.push("quote");
  if (canRestoreTicket) restorableEntityTypes.push("ticket");
  if (canRestoreProductCatalogItem) restorableEntityTypes.push("productcatalogitem");

  return (
    <CrmPageShell
      title="Trash"
      description="Deleted CRM records appear here for 7 days."
      locale={locale}
      bodyClassName="flex min-h-full flex-col gap-4"
    >
      <div className="min-h-0 flex-1">
        <TrashRecordsTable
          caption="Global trash"
          columns={columns}
          rows={toRows(data.items, dateSettings)}
          filters={buildFilters(data.items)}
          initialSearch={query.search ?? ""}
          labels={{
            searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
            searchAriaLabel: tCrm("crm.shell.globalSearchAria", locale),
            emptyTitle: "Trash is empty",
            emptyDescription: "Deleted items will appear here for 7 days.",
          }}
          infoContent={
            <p className="text-sm text-muted-foreground">
              Items stay in Trash for 7 days before permanent deletion.
            </p>
          }
          restorableEntityTypes={restorableEntityTypes}
          emptyState={{
            title: "Trash is empty",
            description: "Deleted items will appear here for 7 days.",
            icon: <FileText aria-hidden="true" className="size-5" />,
          }}
        />
      </div>

      <CrmPagination
        currentPage={data.pageNumber}
        totalPages={data.totalPages}
        basePath="/trash"
        currentQuery={currentQuery}
      />
    </CrmPageShell>
  );
}
