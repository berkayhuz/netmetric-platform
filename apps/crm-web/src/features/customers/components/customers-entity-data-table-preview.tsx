"use client";

import { type FormEvent, useCallback, useMemo, useState, useTransition } from "react";
import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Badge, Button, Input } from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  Building2,
  Filter,
  Search,
  Trash2,
  X,
} from "lucide-react";

import type { DataTableColumnDef, DataTableRowSelectionState } from "@netmetric/ui/client";
import {
  deleteCustomerFromListAction,
  deleteCustomersBulkFromListAction,
} from "@/features/customers/actions/customer-mutation-actions";
import { CrmBulkDeleteConfirmDialog } from "@/components/shell/crm-bulk-delete-confirm-dialog";
import { CrmEntityDataTable } from "@/components/shell/crm-entity-data-table";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import type { CustomerListItemDto, SortDirection } from "@/lib/crm-api";
import { tCrm } from "@/lib/i18n/crm-i18n";

type SortKey = "fullName" | "email" | "mobilePhone" | "companyName" | "customerType" | "isActive";

const PAGE_SIZE_OPTIONS = ["20", "50", "100", "200", "300", "400", "500"] as const;
const SORT_KEY_TO_API: Record<SortKey, string | null> = {
  fullName: "name",
  email: "email",
  mobilePhone: null,
  companyName: "company",
  customerType: null,
  isActive: null,
};
type FilterState = {
  customerType: "all" | "corporate" | "individual";
  status: "all" | "active" | "inactive";
};

const defaultFilters: FilterState = {
  customerType: "all",
  status: "all",
};

export function getCustomersPreviewSelectedIds(
  rowSelection: DataTableRowSelectionState,
  filteredCustomers: CustomerListItemDto[],
): string[] {
  return Object.entries(rowSelection)
    .filter(([, selected]) => Boolean(selected))
    .map(([rowId]) => rowId)
    .filter((rowId) => filteredCustomers.some((customer) => customer.id === rowId));
}

export function shouldShowCustomersPreviewBulkDelete(
  canDelete: boolean,
  selectedCount: number,
): boolean {
  return canDelete && selectedCount > 0;
}

export function getCustomersPreviewDeleteDescription(deleteTargetCount: number): string {
  return deleteTargetCount > 1
    ? `${deleteTargetCount} selected customers will be deleted permanently.`
    : "This customer will be deleted permanently.";
}

export async function executeCustomersPreviewDelete(
  deleteTargetIds: string[],
  callbacks: {
    deleteSingle: (id: string) => Promise<CrmMutationState>;
    deleteBulk: (ids: string[]) => Promise<CrmMutationState>;
    onSuccess: () => void;
    onFailure: () => void;
  },
): Promise<boolean> {
  if (deleteTargetIds.length === 0) {
    callbacks.onFailure();
    return false;
  }

  const mutationResult =
    deleteTargetIds.length === 1
      ? await callbacks.deleteSingle(deleteTargetIds[0]!)
      : await callbacks.deleteBulk(deleteTargetIds);

  if (mutationResult.status === "success") {
    callbacks.onSuccess();
    return true;
  }

  callbacks.onFailure();
  return false;
}

function isCorporate(customerType: string | number): boolean {
  const normalized = String(customerType).toLowerCase();
  return normalized === "1" || normalized === "corporate";
}

function customerTypeLabel(customerType: string | number, locale: string): string {
  return isCorporate(customerType)
    ? tCrm("crm.customers.options.customerType.corporate", locale)
    : tCrm("crm.customers.options.customerType.individual", locale);
}

export function CustomersEntityDataTablePreview({
  customers,
  detailBasePath,
  locale,
  canCreate = false,
  canDelete = false,
  createHref,
  createLabel,
  currentSearch = "",
  currentSortBy,
  currentSortDirection,
  currentPageSize = 20,
}: Readonly<{
  customers: CustomerListItemDto[];
  detailBasePath: string;
  locale: string;
  canCreate?: boolean;
  canDelete?: boolean;
  createHref?: string;
  createLabel?: string;
  currentSearch?: string;
  currentSortBy?: string;
  currentSortDirection?: SortDirection;
  currentPageSize?: number;
}>) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [searchValue, setSearchValue] = useState(currentSearch);
  const [filters, setFilters] = useState<FilterState>(defaultFilters);
  const [rowSelection, setRowSelection] = useState<DataTableRowSelectionState>({});
  const [isPending, startTransition] = useTransition();
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deleteTargetIds, setDeleteTargetIds] = useState<string[]>([]);

  const updateQuery = useCallback(
    (next: Record<string, string | null>) => {
      const params = new URLSearchParams(searchParams.toString());
      for (const [key, value] of Object.entries(next)) {
        if (!value) params.delete(key);
        else params.set(key, value);
      }
      const query = params.toString();
      router.replace(query ? `${pathname}?${query}` : pathname);
    },
    [pathname, router, searchParams],
  );

  function onSearchSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    updateQuery({ search: searchValue.trim() || null, page: "1" });
  }

  const onSort = useCallback(
    (sortKey: SortKey) => {
      const apiSortBy = SORT_KEY_TO_API[sortKey];
      if (!apiSortBy) {
        return;
      }
      const nextDirection: SortDirection =
        currentSortBy === apiSortBy && currentSortDirection === "asc" ? "desc" : "asc";
      updateQuery({ sortBy: apiSortBy, sortDirection: nextDirection, page: "1" });
    },
    [currentSortBy, currentSortDirection, updateQuery],
  );

  const onChangePageSize = useCallback(
    (value: string) => {
      updateQuery({ pageSize: value, page: "1" });
    },
    [updateQuery],
  );

  const columns = useMemo<DataTableColumnDef<CustomerListItemDto>[]>(() => {
    const baseColumns: DataTableColumnDef<CustomerListItemDto>[] = [
      {
        id: "fullName",
        accessorFn: (item) => item.fullName,
        header: () => (
          <SortHeader
            label={tCrm("crm.customers.fields.name", locale)}
            active={currentSortBy === SORT_KEY_TO_API.fullName}
            {...(currentSortDirection ? { direction: currentSortDirection } : {})}
            onClick={() => onSort("fullName")}
          />
        ),
        cell: ({ row }) => (
          <Link
            href={`${detailBasePath}/${row.original.id}`}
            className="font-medium hover:underline"
          >
            {row.original.fullName}
          </Link>
        ),
      },
      {
        id: "email",
        accessorFn: (item) => item.email ?? "-",
        header: () => (
          <SortHeader
            label={tCrm("crm.customers.fields.email", locale)}
            active={currentSortBy === SORT_KEY_TO_API.email}
            {...(currentSortDirection ? { direction: currentSortDirection } : {})}
            onClick={() => onSort("email")}
          />
        ),
      },
      {
        id: "mobilePhone",
        accessorFn: (item) => item.mobilePhone ?? "-",
        header: tCrm("crm.customers.fields.mobilePhoneShort", locale),
      },
      {
        id: "companyName",
        accessorFn: (item) => item.companyName ?? "-",
        header: () => (
          <SortHeader
            label={tCrm("crm.customers.fields.company", locale)}
            active={currentSortBy === SORT_KEY_TO_API.companyName}
            {...(currentSortDirection ? { direction: currentSortDirection } : {})}
            onClick={() => onSort("companyName")}
          />
        ),
      },
      {
        id: "customerType",
        accessorFn: (item) => customerTypeLabel(item.customerType, locale),
        header: tCrm("crm.customers.fields.customerType", locale),
      },
      {
        id: "isActive",
        accessorFn: (item) => (item.isActive ? "active" : "inactive"),
        header: tCrm("crm.customers.fields.status", locale),
        cell: ({ row }) => (
          <Badge
            variant="secondary"
            className={row.original.isActive ? "bg-emerald-500/15 text-emerald-700" : undefined}
          >
            {row.original.isActive
              ? tCrm("crm.common.active", locale)
              : tCrm("crm.common.inactive", locale)}
          </Badge>
        ),
      },
    ];
    return baseColumns;
  }, [currentSortBy, currentSortDirection, detailBasePath, locale, onSort]);
  const filteredCustomers = useMemo(
    () =>
      customers.filter((item) => {
        const type = isCorporate(item.customerType) ? "corporate" : "individual";
        const matchesType = filters.customerType === "all" || filters.customerType === type;
        const matchesStatus =
          filters.status === "all" ||
          (filters.status === "active" && item.isActive) ||
          (filters.status === "inactive" && !item.isActive);

        return matchesType && matchesStatus;
      }),
    [customers, filters],
  );

  const selectedCustomerIds = useMemo(
    () => getCustomersPreviewSelectedIds(rowSelection, filteredCustomers),
    [filteredCustomers, rowSelection],
  );

  async function confirmDelete() {
    if (isPending || deleteTargetIds.length === 0) return;

    startTransition(async () => {
      void (await executeCustomersPreviewDelete(deleteTargetIds, {
        deleteSingle: deleteCustomerFromListAction,
        deleteBulk: deleteCustomersBulkFromListAction,
        onSuccess: () => {
          setConfirmOpen(false);
          setDeleteTargetIds([]);
          setRowSelection({});
          router.refresh();
        },
        onFailure: () => {
          // Keep the dialog/selection intact on failure to avoid masking mutation errors.
          setConfirmOpen(true);
        },
      }));
    });
  }

  return (
    <>
      <CrmEntityDataTable
        data={filteredCustomers}
        columns={columns}
        enableSearch={false}
        enableFilters={false}
        enableColumnVisibility
        enableColumnReorder
        enablePagination={false}
        enableRowSelection
        enableSorting={false}
        getRowId={(row) => row.id}
        density="compact"
        rowSelection={rowSelection}
        onRowSelectionChange={setRowSelection}
        labels={{
          selectedRows: "{count} selected",
          emptyTitle: "No matching customers",
          emptyDescription: "Try changing filters, search, or sorting query parameters.",
        }}
        emptyState={{
          title: "No matching customers",
          description: "Try changing filters or clearing the current view.",
          icon: (
            <div className="mx-auto inline-flex h-10 w-10 items-center justify-center rounded-lg border border-white/12 bg-white/[0.04] text-muted-foreground">
              <Building2 aria-hidden="true" className="size-5" />
            </div>
          ),
          ...(canCreate && createHref && createLabel
            ? {
                action: (
                  <Button asChild size="sm" variant="secondary" className="h-8">
                    <Link href={createHref}>{createLabel}</Link>
                  </Button>
                ),
              }
            : {}),
        }}
        toolbarActions={
          <div className="flex flex-wrap items-center gap-2">
            <form onSubmit={onSearchSubmit} className="relative">
              <Search
                aria-hidden="true"
                className="pointer-events-none absolute top-1/2 left-2.5 size-3.5 -translate-y-1/2 text-muted-foreground"
              />
              <Input
                value={searchValue}
                placeholder={tCrm("crm.shell.searchPlaceholder", locale)}
                aria-label={tCrm("crm.shell.globalSearchAria", locale)}
                className="h-8 w-64 pl-8"
                onChange={(event) => setSearchValue(event.target.value)}
              />
            </form>
            <Select
              value={filters.customerType}
              onValueChange={(value) =>
                setFilters((current) => ({
                  ...current,
                  customerType: value as FilterState["customerType"],
                }))
              }
            >
              <SelectTrigger size="sm" className="h-8 w-44">
                <SelectValue placeholder="All customer types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All customer types</SelectItem>
                <SelectItem value="corporate">
                  {tCrm("crm.customers.options.customerType.corporate", locale)}
                </SelectItem>
                <SelectItem value="individual">
                  {tCrm("crm.customers.options.customerType.individual", locale)}
                </SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={filters.status}
              onValueChange={(value) =>
                setFilters((current) => ({ ...current, status: value as FilterState["status"] }))
              }
            >
              <SelectTrigger size="sm" className="h-8 w-36">
                <SelectValue placeholder="All statuses" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                <SelectItem value="active">{tCrm("crm.common.active", locale)}</SelectItem>
                <SelectItem value="inactive">{tCrm("crm.common.inactive", locale)}</SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={String(currentPageSize)}
              onValueChange={(value) => value && onChangePageSize(value)}
            >
              <SelectTrigger size="sm" className="h-8 w-20">
                <SelectValue placeholder="20" />
              </SelectTrigger>
              <SelectContent>
                {PAGE_SIZE_OPTIONS.map((option) => (
                  <SelectItem key={option} value={option}>
                    {option}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button
              type="button"
              size="sm"
              variant="ghost"
              className="h-8"
              onClick={() => {
                setFilters(defaultFilters);
                setSearchValue("");
                updateQuery({ search: null, page: "1" });
              }}
            >
              <X aria-hidden="true" className="size-3.5" />
              Clear
            </Button>
          </div>
        }
        selectionActions={() =>
          shouldShowCustomersPreviewBulkDelete(canDelete, selectedCustomerIds.length) ? (
            <Button
              type="button"
              size="sm"
              variant="destructive"
              disabled={isPending}
              onClick={() => {
                if (isPending) return;
                setDeleteTargetIds(selectedCustomerIds);
                setConfirmOpen(true);
              }}
            >
              <Trash2 className="size-4" />
              Delete selected
            </Button>
          ) : null
        }
        rowActions={(row) =>
          canDelete ? (
            <div className="flex justify-end">
              <Button
                type="button"
                variant="ghost"
                size="icon"
                aria-label={`Delete ${row.fullName}`}
                disabled={isPending}
                onClick={() => {
                  if (isPending) return;
                  setDeleteTargetIds([row.id]);
                  setConfirmOpen(true);
                }}
              >
                <Trash2 className="size-4" />
              </Button>
            </div>
          ) : null
        }
        renderToolbar={({ table }) => (
          <>
            <span className="inline-flex items-center gap-1.5">
              <Filter aria-hidden="true" className="size-3.5" />
              {table.getRowModel().rows.length} shown
            </span>
            <span>{table.getSelectedRowModel().rows.length} selected</span>
          </>
        )}
      />

      <CrmBulkDeleteConfirmDialog
        open={confirmOpen}
        pending={isPending}
        title="Are you sure?"
        description={getCustomersPreviewDeleteDescription(deleteTargetIds.length)}
        onOpenChange={(open) => {
          setConfirmOpen(open);
          if (!open) {
            setDeleteTargetIds([]);
          }
        }}
        onConfirm={() => {
          void confirmDelete();
        }}
      />
    </>
  );
}

function SortHeader({
  label,
  active,
  direction,
  onClick,
}: Readonly<{
  label: string;
  active: boolean;
  direction?: SortDirection;
  onClick: () => void;
}>) {
  const Icon = active ? (direction === "asc" ? ArrowUp : ArrowDown) : ArrowUpDown;

  return (
    <button
      type="button"
      className="inline-flex h-8 items-center gap-1.5 rounded-sm px-1 text-left text-sm font-medium hover:text-foreground"
      onClick={onClick}
    >
      {label}
      <Icon aria-hidden="true" className="size-3.5" />
    </button>
  );
}
