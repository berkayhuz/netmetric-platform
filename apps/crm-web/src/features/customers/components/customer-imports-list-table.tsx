"use client";

import { useMemo, useState } from "react";
import { Badge, Button, Input } from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import { Filter, X } from "lucide-react";
import { CrmEntityDataTable } from "@/components/shell/crm-entity-data-table";

import {
  cancelCustomerImportBatchFormAction,
  commitCustomerImportBatchFormAction,
  previewCustomerImportBatchFormAction,
  validateCustomerImportBatchFormAction,
} from "@/features/customers/actions/customer-mutation-actions";
import type { CustomerImportBatchDto } from "@/lib/crm-api";
import { tCrm } from "@/lib/i18n/crm-i18n";
import type { DataTableColumnDef, DataTableRowSelectionState } from "@netmetric/ui/client";

const duplicateStrategies = ["Skip", "Update existing", "Create new", "Merge"] as const;

export function CustomerImportsListTable({
  batches,
  locale,
}: Readonly<{
  batches: CustomerImportBatchDto[];
  locale: string;
}>) {
  const [rowSelection, setRowSelection] = useState<DataTableRowSelectionState>({});
  const [searchValue, setSearchValue] = useState("");
  const [statusFilter, setStatusFilter] = useState("all");
  const [duplicateStrategyByBatchId, setDuplicateStrategyByBatchId] = useState<
    Record<string, string>
  >({});

  const filtered = useMemo(() => {
    const search = searchValue.trim().toLocaleLowerCase();
    return batches.filter((batch) => {
      const matchesStatus = statusFilter === "all" || String(batch.status) === statusFilter;
      const matchesSearch =
        search.length === 0 ||
        [batch.fileName, batch.source, String(batch.status)]
          .join(" ")
          .toLocaleLowerCase()
          .includes(search);
      return matchesStatus && matchesSearch;
    });
  }, [batches, searchValue, statusFilter]);

  const filteredIds = filtered.map((batch) => batch.id);
  const selectedVisibleCount = filteredIds.filter((id) => rowSelection[id]).length;
  const statusOptions = [...new Set(batches.map((batch) => String(batch.status)))];

  const columns = useMemo<DataTableColumnDef<CustomerImportBatchDto>[]>(
    () => [
      {
        id: "fileName",
        accessorFn: (item) => item.fileName,
        header: tCrm("crm.customers.fields.fileName", locale),
        cell: ({ row }) => <span className="font-medium">{row.original.fileName}</span>,
      },
      {
        id: "status",
        accessorFn: (item) => String(item.status),
        header: tCrm("crm.modules.workspace.status", locale),
        cell: ({ row }) => <Badge variant="secondary">{String(row.original.status)}</Badge>,
      },
      {
        id: "totalRows",
        accessorFn: (item) => item.totalRows,
        header: tCrm("crm.customers.fields.totalRows", locale),
      },
      {
        id: "operations",
        accessorFn: (item) => item.id,
        header: tCrm("crm.customers.fields.operations", locale),
        meta: { disableReorder: true },
        enableHiding: false,
        cell: ({ row }) => {
          const batch = row.original;

          return (
            <div className="flex items-center flex-wrap gap-2">
              <BatchButton
                action={previewCustomerImportBatchFormAction}
                batchId={batch.id}
                label={tCrm("crm.customers.actions.previewImportBatch", locale)}
              />
              <BatchButton
                action={validateCustomerImportBatchFormAction}
                batchId={batch.id}
                label={tCrm("crm.customers.actions.validateImportBatch", locale)}
              />
              <form
                action={commitCustomerImportBatchFormAction}
                className="flex items-center gap-2"
              >
                <input name="batchId" type="hidden" value={batch.id} />
                <input
                  name="duplicateStrategy"
                  type="hidden"
                  value={duplicateStrategyByBatchId[batch.id] ?? "0"}
                />
                <Select
                  value={duplicateStrategyByBatchId[batch.id] ?? "0"}
                  onValueChange={(value) =>
                    setDuplicateStrategyByBatchId((current) => ({
                      ...current,
                      [batch.id]: value ?? "0",
                    }))
                  }
                >
                  <SelectTrigger size="sm" className="w-[160px]">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {duplicateStrategies.map((strategy, index) => (
                      <SelectItem key={strategy} value={String(index)}>
                        {strategy}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Button type="submit" size="sm" variant="outline">
                  {tCrm("crm.customers.actions.commitImportBatch", locale)}
                </Button>
              </form>
              <form
                action={cancelCustomerImportBatchFormAction}
                className="flex items-center gap-2"
              >
                <input name="batchId" type="hidden" value={batch.id} />
                <Input
                  className="h-8 max-w-36"
                  name="reason"
                  placeholder={tCrm("crm.customers.fields.reason", locale)}
                />
                <Button type="submit" size="sm" variant="outline">
                  {tCrm("crm.customers.actions.cancelImportBatch", locale)}
                </Button>
              </form>
            </div>
          );
        },
      },
    ],
    [duplicateStrategyByBatchId, locale],
  );

  return (
    <CrmEntityDataTable
      data={filtered}
      columns={columns}
      enableSearch={false}
      enableFilters={false}
      enablePagination={false}
      enableRowSelection
      enableSorting={false}
      density="compact"
      getRowId={(row) => row.id}
      rowSelection={rowSelection}
      onRowSelectionChange={setRowSelection}
      labels={{
        selectedRows: "{count} selected",
        emptyTitle: tCrm("crm.customers.states.noImportBatches", locale),
        emptyDescription: "Try changing filters or clearing the current view.",
      }}
      emptyState={{
        title: tCrm("crm.customers.states.noImportBatches", locale),
        description: "Try changing filters or clearing the current view.",
      }}
      toolbarActions={
        <>
          <Input
            value={searchValue}
            placeholder={tCrm("crm.shell.searchPlaceholder", locale)}
            aria-label={tCrm("crm.shell.globalSearchAria", locale)}
            className="h-8 w-64"
            onChange={(event) => setSearchValue(event.target.value)}
          />
          <Select value={statusFilter} onValueChange={(value) => setStatusFilter(value || "all")}>
            <SelectTrigger size="sm" className="w-44">
              <SelectValue placeholder={tCrm("crm.modules.workspace.status", locale)} />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All statuses</SelectItem>
              {statusOptions.map((status) => (
                <SelectItem key={status} value={status}>
                  {status}
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
              setSearchValue("");
              setStatusFilter("all");
            }}
          >
            <X aria-hidden="true" className="size-3.5" />
            Clear
          </Button>
        </>
      }
      renderToolbar={() => (
        <>
          <span className="inline-flex items-center gap-1.5">
            <Filter aria-hidden="true" className="size-3.5" />
            {filtered.length} shown
          </span>
          <span>{selectedVisibleCount} selected</span>
        </>
      )}
    />
  );
}

function BatchButton({
  action,
  batchId,
  label,
}: Readonly<{
  action: (formData: FormData) => Promise<void>;
  batchId: string;
  label: string;
}>) {
  return (
    <form action={action}>
      <input name="batchId" type="hidden" value={batchId} />
      <Button type="submit" size="sm" variant="outline">
        {label}
      </Button>
    </form>
  );
}
