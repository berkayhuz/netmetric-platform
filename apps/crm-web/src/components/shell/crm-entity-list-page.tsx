import type { CrmPagedResult } from "@/lib/crm-api";
import { tCrm } from "@/lib/i18n/crm-i18n";
import {
  Building2,
  CheckSquare,
  FileText,
  HandCoins,
  Lightbulb,
  Plus,
  ReceiptText,
  Ticket,
  Users,
} from "lucide-react";
import { isValidElement, type ReactNode } from "react";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";

import type { CrmEntityTableColumn } from "./crm-table.types";
import type { CrmPagePath } from "./crm-page-metadata";
import { CrmPageShell } from "./crm-page-shell";
import { CrmPagination } from "./crm-pagination";
import { CrmPageHeaderActionLink, CrmPageHeaderActions } from "./crm-page-header-actions";
import {
  CrmRecordsTable,
  type CrmRecordsTableFilter,
  type CrmRecordsTableRow,
} from "./crm-records-table";
import { CrmEntityListRecordsTable } from "./crm-entity-list-records-table";

export function CrmEntityListPage<TItem extends { id: string }>({
  title,
  description,
  routePath,
  actionPath,
  createPath,
  createLabel,
  canCreate = true,
  createDisabledMessage,
  secondaryActions,
  search,
  caption,
  columns,
  paged,
  detailBasePath,
  currentQuery,
  locale,
  emptyTitle,
  emptyDescription,
  canDelete = false,
  bulkDeleteAction,
  children,
}: Readonly<{
  title?: string;
  description?: string;
  routePath?: CrmPagePath;
  actionPath: string;
  createPath?: string;
  createLabel?: string;
  canCreate?: boolean;
  createDisabledMessage?: string;
  secondaryActions?: ReactNode;
  search?: string;
  caption: string;
  columns: CrmEntityTableColumn<TItem>[];
  paged: CrmPagedResult<TItem>;
  detailBasePath: string;
  currentQuery: URLSearchParams;
  locale?: string | null;
  emptyTitle?: string;
  emptyDescription?: string;
  canDelete?: boolean;
  bulkDeleteAction?: (ids: string[]) => Promise<CrmMutationState>;
  children?: ReactNode;
}>) {
  const resolvedEmptyTitle = emptyTitle ?? tCrm("crm.lists.states.empty", locale);
  const resolvedEmptyDescription =
    emptyDescription ?? tCrm("crm.lists.states.emptyDescription", locale);
  const recordRows = createRecordRows(columns, paged.items, detailBasePath);
  const tableFilters = createTableFilters(columns, recordRows);

  const createAction =
    createPath && canCreate && createLabel ? (
      <CrmPageHeaderActionLink
        href={createPath}
        icon={<Plus aria-hidden="true" />}
        label={createLabel}
      />
    ) : null;
  const headerActions =
    secondaryActions || createAction ? (
      <CrmPageHeaderActions>
        {secondaryActions}
        {createAction}
      </CrmPageHeaderActions>
    ) : null;

  return (
    <CrmPageShell
      routePath={routePath}
      title={title}
      description={description}
      locale={locale}
      actions={headerActions}
      bodyClassName="flex min-h-full flex-col gap-4"
    >
      {children}

      <div className="min-h-0 flex-1">
        {canDelete && bulkDeleteAction ? (
          <CrmEntityListRecordsTable
            caption={caption}
            columns={columns.map((column) => ({
              key: column.key,
              header: column.header,
              badge: isBadgeColumn(column.key),
            }))}
            rows={recordRows}
            filters={tableFilters}
            initialSearch={search ?? ""}
            labels={{
              searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
              searchAriaLabel: tCrm("crm.shell.globalSearchAria", locale),
              emptyTitle: resolvedEmptyTitle,
              emptyDescription: resolvedEmptyDescription,
            }}
            emptyState={{
              title: resolvedEmptyTitle,
              description: resolvedEmptyDescription,
              icon: resolveEmptyStateIcon(actionPath),
              action:
                createPath && canCreate && createLabel ? (
                  <CrmPageHeaderActionLink
                    href={createPath}
                    icon={<Plus aria-hidden="true" />}
                    label={createLabel}
                  />
                ) : null,
            }}
            canDelete={canDelete}
            bulkDeleteAction={bulkDeleteAction}
          />
        ) : (
          <CrmRecordsTable
            caption={caption}
            columns={columns.map((column) => ({
              key: column.key,
              header: column.header,
              badge: isBadgeColumn(column.key),
            }))}
            rows={recordRows}
            filters={tableFilters}
            initialSearch={search ?? ""}
            labels={{
              searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
              searchAriaLabel: tCrm("crm.shell.globalSearchAria", locale),
              emptyTitle: resolvedEmptyTitle,
              emptyDescription: resolvedEmptyDescription,
            }}
            emptyState={{
              title: resolvedEmptyTitle,
              description: resolvedEmptyDescription,
              icon: resolveEmptyStateIcon(actionPath),
              action:
                createPath && canCreate && createLabel ? (
                  <CrmPageHeaderActionLink
                    href={createPath}
                    icon={<Plus aria-hidden="true" />}
                    label={createLabel}
                  />
                ) : null,
            }}
          />
        )}
      </div>

      <CrmPagination
        currentPage={paged.pageNumber}
        totalPages={paged.totalPages}
        basePath={actionPath}
        currentQuery={currentQuery}
      />
    </CrmPageShell>
  );
}

function resolveEmptyStateIcon(actionPath: string): ReactNode {
  if (actionPath.includes("/customers")) return <Users aria-hidden="true" className="size-5" />;
  if (actionPath.includes("/contacts")) return <Users aria-hidden="true" className="size-5" />;
  if (actionPath.includes("/companies")) return <Building2 aria-hidden="true" className="size-5" />;
  if (actionPath.includes("/tasks")) return <CheckSquare aria-hidden="true" className="size-5" />;
  if (actionPath.includes("/deals")) return <HandCoins aria-hidden="true" className="size-5" />;
  if (actionPath.includes("/opportunities"))
    return <Lightbulb aria-hidden="true" className="size-5" />;
  if (actionPath.includes("/quotes")) return <ReceiptText aria-hidden="true" className="size-5" />;
  if (actionPath.includes("/tickets")) return <Ticket aria-hidden="true" className="size-5" />;
  return <FileText aria-hidden="true" className="size-5" />;
}

function createRecordRows<TItem extends { id: string }>(
  columns: CrmEntityTableColumn<TItem>[],
  items: TItem[],
  detailBasePath: string,
): CrmRecordsTableRow[] {
  return items.map((item) => {
    const cells = Object.fromEntries(
      columns.map((column) => [column.key, stringifyCell(column.render(item))]),
    );

    return {
      id: item.id,
      href: `${detailBasePath}/${item.id}`,
      cells,
      searchText: Object.values(cells).join(" "),
    };
  });
}

function createTableFilters<TItem>(
  columns: CrmEntityTableColumn<TItem>[],
  rows: CrmRecordsTableRow[],
): CrmRecordsTableFilter[] {
  return columns
    .filter((column) => isFilterColumn(column.key))
    .map((column) => {
      const options = [...new Set(rows.map((row) => row.cells[column.key]).filter(Boolean))]
        .filter((value): value is string => Boolean(value) && value !== "-")
        .slice(0, 12)
        .map((value) => ({ value, label: value }));

      return {
        key: column.key,
        label: column.header,
        allLabel: `All ${column.header.toLocaleLowerCase()}`,
        options,
      };
    })
    .filter((filter) => filter.options.length > 1)
    .slice(0, 2);
}

function stringifyCell(value: ReactNode): string {
  if (value === null || value === undefined || value === false) {
    return "-";
  }

  if (typeof value === "string" || typeof value === "number" || typeof value === "bigint") {
    const text = String(value).trim();
    return text.length > 0 ? text : "-";
  }

  if (typeof value === "boolean") {
    return value ? "Yes" : "No";
  }

  if (Array.isArray(value)) {
    const text = value.map(stringifyCell).join(" ").trim();
    return text.length > 0 ? text : "-";
  }

  if (isValidElement<{ children?: ReactNode }>(value)) {
    return stringifyCell(value.props.children);
  }

  return "-";
}

function isFilterColumn(key: string): boolean {
  return /status|state|active|priority|stage|source|outcome|type/i.test(key);
}

function isBadgeColumn(key: string): boolean {
  return /status|state|active|priority|stage|outcome|type/i.test(key);
}
