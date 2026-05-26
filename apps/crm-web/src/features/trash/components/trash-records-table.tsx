"use client";

import { useState, useTransition } from "react";
import type { ReactNode } from "react";
import { useRouter } from "next/navigation";
import { Button } from "@netmetric/ui";

import { CrmBulkDeleteConfirmDialog } from "@/components/shell/crm-bulk-delete-confirm-dialog";
import {
  CrmRecordsTable,
  type CrmRecordsTableColumn,
  type CrmRecordsTableFilter,
  type CrmRecordsTableLabels,
  type CrmRecordsTableRow,
} from "@/components/shell/crm-records-table";
import { restoreTrashItemAction } from "@/features/trash/actions/trash-mutation-actions";

export function TrashRecordsTable({
  caption,
  columns,
  rows,
  filters,
  initialSearch,
  labels,
  infoContent,
  emptyState,
  restorableEntityTypes,
}: Readonly<{
  caption: string;
  columns: CrmRecordsTableColumn[];
  rows: CrmRecordsTableRow[];
  filters: CrmRecordsTableFilter[];
  initialSearch: string;
  labels: CrmRecordsTableLabels;
  infoContent: ReactNode;
  emptyState: {
    title: string;
    description: string;
    icon: ReactNode;
  };
  restorableEntityTypes: string[];
}>) {
  const router = useRouter();
  const [openItemId, setOpenItemId] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();

  return (
    <>
      {message ? <p className="text-sm text-muted-foreground">{message}</p> : null}
      <CrmRecordsTable
        caption={caption}
        columns={columns}
        rows={rows}
        filters={filters}
        initialSearch={initialSearch}
        labels={labels}
        infoContent={infoContent}
        emptyState={emptyState}
        rowActions={(row) =>
          restorableEntityTypes.includes((row.cells.entityType ?? "").toLowerCase()) ? (
            <Button
              type="button"
              size="sm"
              variant="outline"
              disabled={isPending}
              onClick={() => setOpenItemId(row.id)}
            >
              Restore
            </Button>
          ) : null
        }
      />
      <CrmBulkDeleteConfirmDialog
        open={openItemId !== null}
        pending={isPending}
        title="Restore item?"
        description="This item will be restored to active records."
        confirmLabel="Restore"
        confirmVariant="default"
        onOpenChange={(open) => {
          if (!open) {
            setOpenItemId(null);
          }
        }}
        onConfirm={() =>
          startTransition(async () => {
            if (!openItemId) return;
            const row = rows.find((candidate) => candidate.id === openItemId);
            const entityType = (row?.cells.entityType ?? "").toLowerCase();

            const result = await restoreTrashItemAction(openItemId, entityType);
            setMessage(result.message ?? null);
            setOpenItemId(null);

            if (result.status === "success") {
              router.refresh();
            }
          })
        }
      />
    </>
  );
}
