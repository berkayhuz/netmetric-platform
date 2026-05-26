import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const previewPath = path.resolve(__dirname, "./customers-entity-data-table-preview.tsx");

describe("customers entity data-table preview source contract", () => {
  it("uses read-only CrmEntityDataTable composition and keeps unsupported workflows out", () => {
    const source = fs.readFileSync(previewPath, "utf8");

    expect(source).toContain("<CrmEntityDataTable");
    expect(source).toContain("enablePagination={false}");
    expect(source).toContain("enableRowSelection");
    expect(source).toContain("enableColumnVisibility");
    expect(source).toContain("enableColumnReorder");
    expect(source).toContain("setFilters");
    expect(source).toContain("customerType");
    expect(source).toContain("status");
    expect(source).toContain("emptyState={{");
    expect(source).toContain("Building2");
    expect(source).toContain("Filter");
    expect(source).toContain("canCreate && createHref && createLabel");
    expect(source).toContain("selected");
    expect(source).toContain("shown");
    expect(source).toContain("getSelectedRowModel().rows.length");
    expect(source).toContain("getRowModel().rows.length");
    expect(source).toContain("onSort(");
    expect(source).toContain("onChangePageSize(");
    expect(source).toContain("router.replace");
    expect(source).not.toContain("updateQuery({ customerType");
    expect(source).not.toContain("updateQuery({ status");
    expect(source).toContain("deleteCustomerFromListAction");
    expect(source).toContain("deleteCustomersBulkFromListAction");
    expect(source).toContain("canDelete");
    expect(source).toContain("shouldShowCustomersPreviewBulkDelete(");
    expect(source).toContain("Delete selected");
    expect(source).toContain("setDeleteTargetIds(selectedCustomerIds)");
    expect(source).toContain("getCustomersPreviewDeleteDescription(deleteTargetIds.length)");
    expect(source).toContain("setConfirmOpen");
    expect(source).toContain("deleteTargetIds");
    expect(source).toContain("isPending");
    expect(source).toContain("disabled={isPending}");
    expect(source).toContain("if (isPending) return;");
    expect(source).toContain("CrmBulkDeleteConfirmDialog");
    expect(source).toContain("confirmDelete");
    expect(source).toContain('mutationResult.status === "success"');
    expect(source).toContain("if (!open) {");
    expect(source).toContain("setDeleteTargetIds([]);");
    expect(source).toContain("router.refresh()");
    expect(source).toContain("setRowSelection({})");
    expect(source).toContain("customer-mutation-actions");
    expect(source).toContain("Trash2");
    expect(source).toContain("rowActions={(row)");
    expect(source).toContain("getRowId={(row) => row.id}");
    expect(source).not.toContain("createCustomerImportBatchFormAction");
    expect(source).not.toContain("previewCustomerImportBatchFormAction");
    expect(source).not.toContain("validateCustomerImportBatchFormAction");
    expect(source).not.toContain("commitCustomerImportBatchFormAction");
    expect(source).not.toContain("cancelCustomerImportBatchFormAction");
    expect(source).not.toContain("setColumnOrder");
    expect(source).not.toContain("setDragKey");
    expect(source).not.toContain("draggable");
  });
});
