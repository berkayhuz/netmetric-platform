"use client";

import { useMemo, useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { Button, EntityTableInfoStrip } from "@netmetric/ui";
import { Trash2 } from "lucide-react";

import {
  CrmRecordsTable,
  type CrmRecordsTableFilter,
  type CrmRecordsTableRow,
} from "@/components/shell/crm-records-table";
import { CrmBulkDeleteConfirmDialog } from "@/components/shell/crm-bulk-delete-confirm-dialog";
import type {
  CrmPagedResult,
  ProductCatalogItemDto,
  ProductCatalogLookupItemDto,
} from "@/lib/crm-api";
import {
  bulkDeleteProductCatalogItemsAction,
  bulkSetProductCatalogItemsActiveStateAction,
  downloadProductCatalogExportAction,
  downloadProductCatalogTemplateAction,
} from "../actions/product-catalog-mutation-actions";
import {
  buildActiveInactiveFilter,
  buildCatalogRecordsTableLabels,
  CatalogRefreshButton,
} from "./product-catalog-table-helpers";
import { buildProductCatalogColumns, mapProductCatalogRows } from "./product-catalog-table-mapping";
import { tCrm } from "@/lib/i18n/crm-i18n";

export type ProductCatalogTableLabels = {
  actions: string;
  active: string;
  addProduct: string;
  category: string;
  code: string;
  columns: string;
  description: string;
  edit: string;
  emptyDescription: string;
  emptyTitle: string;
  errorDescription: string;
  errorTitle: string;
  firstPage: string;
  inactive: string;
  lastPage: string;
  manageCategories: string;
  name: string;
  nextPage: string;
  noCategory: string;
  noPrice: string;
  page: string;
  previousPage: string;
  price: string;
  refresh: string;
  reset: string;
  rowsPerPage: string;
  search: string;
  searchPlaceholder: string;
  selectedRows: string;
  status: string;
  view: string;
  viewColumns: string;
};

function triggerDownload(url: string, fileName: string) {
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  link.click();
}

export function ProductCatalogDataTable({
  paged,
  categories,
  locale,
  labels,
  unavailable,
  canManage,
  activeProductCount,
  categoryCount,
  version,
}: Readonly<{
  paged: CrmPagedResult<ProductCatalogItemDto>;
  categories: ProductCatalogLookupItemDto[];
  locale: string | null;
  labels: ProductCatalogTableLabels;
  unavailable: boolean;
  canManage: boolean;
  activeProductCount?: number | undefined;
  categoryCount?: number | undefined;
  version?: string | undefined;
}>) {
  const router = useRouter();
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);

  const rows = useMemo<CrmRecordsTableRow[]>(
    () => mapProductCatalogRows({ items: paged.items, labels, locale }),
    [labels, locale, paged.items],
  );
  const columns = useMemo(() => buildProductCatalogColumns(labels), [labels]);
  const filters = useMemo<CrmRecordsTableFilter[]>(
    () => [
      {
        key: "categoryName",
        label: labels.category,
        allLabel: `${labels.category}: ${labels.reset}`,
        options: categories.map((category) => ({
          label: `${category.code} - ${category.name}`,
          value: category.name,
        })),
      },
      buildActiveInactiveFilter(labels),
    ],
    [categories, labels],
  );

  return (
    <div className="space-y-3">
      {actionMessage ? <p className="text-sm text-muted-foreground">{actionMessage}</p> : null}
      <CrmRecordsTable
        caption={labels.description}
        columns={columns}
        rows={unavailable ? [] : rows}
        filters={filters}
        labels={buildCatalogRecordsTableLabels(labels, unavailable)}
        onSelectionChange={setSelectedIds}
        infoContent={
          !unavailable ? (
            <EntityTableInfoStrip>
              <div className="flex items-center gap-1">
                <span>{tCrm("crm.productCatalog.stats.products", locale)}:</span>
                <span className="font-semibold text-foreground">{paged.totalCount}</span>
              </div>
              <span className="text-border/60 select-none">|</span>
              <div className="flex items-center gap-1">
                <span>{tCrm("crm.productCatalog.stats.activeProducts", locale)}:</span>
                <span className="font-semibold text-foreground">{activeProductCount ?? "-"}</span>
              </div>
              <span className="text-border/60 select-none">|</span>
              <div className="flex items-center gap-1">
                <span>{tCrm("crm.productCatalog.stats.categories", locale)}:</span>
                <span className="font-semibold text-foreground">{categoryCount ?? "-"}</span>
              </div>
              <span className="text-border/60 select-none">|</span>
              <div className="flex items-center gap-1">
                <span>{tCrm("crm.productCatalog.stats.moduleVersion", locale)}:</span>
                <span className="font-semibold text-foreground">{version ?? "-"}</span>
              </div>
            </EntityTableInfoStrip>
          ) : null
        }
        toolbarContent={
          <div className="flex flex-wrap items-center gap-2">
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="h-8"
              disabled={isPending}
              onClick={() =>
                startTransition(async () => {
                  const result = await downloadProductCatalogExportAction({});
                  if (result.status !== "success" || !result.downloadUrl) {
                    setActionMessage(result.message ?? "Export failed.");
                    return;
                  }

                  triggerDownload(result.downloadUrl, result.fileName ?? "product-catalog-export");
                  setActionMessage(null);
                })
              }
            >
              Export
            </Button>
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="h-8"
              disabled={isPending}
              onClick={() =>
                startTransition(async () => {
                  const result = await downloadProductCatalogTemplateAction();
                  if (result.status !== "success" || !result.downloadUrl) {
                    setActionMessage(result.message ?? "Template download failed.");
                    return;
                  }

                  triggerDownload(
                    result.downloadUrl,
                    result.fileName ?? "product-catalog-template",
                  );
                  setActionMessage(null);
                })
              }
            >
              Template
            </Button>
            <CatalogRefreshButton
              label={labels.refresh}
              size="xs"
              onRefresh={() => router.refresh()}
            />
          </div>
        }
        selectionActions={({ selectedCount }) =>
          canManage && selectedCount > 0 ? (
            <>
              <Button
                type="button"
                variant="outline"
                size="sm"
                className="h-8"
                disabled={isPending}
                onClick={() =>
                  startTransition(async () => {
                    const result = await bulkSetProductCatalogItemsActiveStateAction(
                      selectedIds,
                      true,
                    );
                    setActionMessage(result.message ?? null);
                    if (result.status === "success") {
                      router.refresh();
                    }
                  })
                }
              >
                Activate selected
              </Button>
              <Button
                type="button"
                variant="outline"
                size="sm"
                className="h-8"
                disabled={isPending}
                onClick={() =>
                  startTransition(async () => {
                    const result = await bulkSetProductCatalogItemsActiveStateAction(
                      selectedIds,
                      false,
                    );
                    setActionMessage(result.message ?? null);
                    if (result.status === "success") {
                      router.refresh();
                    }
                  })
                }
              >
                Deactivate selected
              </Button>
              <Button
                type="button"
                variant="destructive"
                size="sm"
                className="h-8"
                disabled={isPending}
                onClick={() => setConfirmDeleteOpen(true)}
              >
                <Trash2 aria-hidden="true" className="size-4" />
                Delete selected
              </Button>
            </>
          ) : null
        }
      />
      <CrmBulkDeleteConfirmDialog
        open={confirmDeleteOpen}
        pending={isPending}
        title="Delete selected products?"
        description={
          selectedIds.length > 1
            ? `${selectedIds.length} selected products will be deleted.`
            : "The selected product will be deleted."
        }
        onOpenChange={(open) => setConfirmDeleteOpen(open)}
        onConfirm={() =>
          startTransition(async () => {
            const result = await bulkDeleteProductCatalogItemsAction(selectedIds);
            setActionMessage(result.message ?? null);
            if (result.status === "success") {
              setConfirmDeleteOpen(false);
              setSelectedIds([]);
              router.refresh();
            }
          })
        }
      />
    </div>
  );
}
