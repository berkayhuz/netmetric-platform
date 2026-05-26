"use client";

import { useState, useTransition } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@netmetric/ui";
import { Trash2 } from "lucide-react";
import type { DataTableStateContent } from "@netmetric/ui/client";

import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import {
  CrmRecordsTable,
  type CrmRecordsTableColumn,
  type CrmRecordsTableFilter,
  type CrmRecordsTableLabels,
  type CrmRecordsTableRow,
} from "./crm-records-table";
import { CrmBulkDeleteConfirmDialog } from "./crm-bulk-delete-confirm-dialog";

export function CrmEntityListRecordsTable({
  caption,
  columns,
  rows,
  filters,
  initialSearch,
  labels,
  emptyState,
  canDelete = false,
  bulkDeleteAction,
}: Readonly<{
  caption: string;
  columns: CrmRecordsTableColumn[];
  rows: CrmRecordsTableRow[];
  filters: CrmRecordsTableFilter[];
  initialSearch: string;
  labels: CrmRecordsTableLabels;
  emptyState: DataTableStateContent;
  canDelete?: boolean;
  bulkDeleteAction?: (ids: string[]) => Promise<CrmMutationState>;
}>) {
  const router = useRouter();
  const [selectedIds, setSelectedIds] = useState<string[]>([]);
  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);
  const [actionMessage, setActionMessage] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  const deleteEnabled = canDelete && Boolean(bulkDeleteAction);

  return (
    <>
      {actionMessage ? <p className="text-sm text-muted-foreground">{actionMessage}</p> : null}
      <CrmRecordsTable
        caption={caption}
        columns={columns}
        rows={rows}
        filters={filters}
        initialSearch={initialSearch}
        labels={labels}
        emptyState={emptyState}
        {...(deleteEnabled ? { onSelectionChange: setSelectedIds } : {})}
        selectionActions={({ selectedCount }) =>
          deleteEnabled && selectedCount > 0 ? (
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
          ) : null
        }
      />
      {deleteEnabled ? (
        <CrmBulkDeleteConfirmDialog
          open={confirmDeleteOpen}
          pending={isPending}
          title="Delete selected records?"
          description={
            selectedIds.length > 1
              ? `${selectedIds.length} selected records will be deleted.`
              : "The selected record will be deleted."
          }
          onOpenChange={(open) => setConfirmDeleteOpen(open)}
          onConfirm={() =>
            startTransition(async () => {
              if (!bulkDeleteAction) {
                return;
              }

              const result = await bulkDeleteAction(selectedIds);
              setActionMessage(result.message ?? null);
              if (result.status === "success") {
                setConfirmDeleteOpen(false);
                setSelectedIds([]);
                router.refresh();
              }
            })
          }
        />
      ) : null}
    </>
  );
}
