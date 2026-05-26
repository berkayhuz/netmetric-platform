"use client";

import { useMemo, type ReactNode } from "react";
import { cn } from "@netmetric/ui";
import {
  EntityDataTable,
  type DataTableColumnDef,
  type EntityDataTableProps,
} from "@netmetric/ui/client";

type CrmToolbarContext<TData> = {
  table: Parameters<NonNullable<EntityDataTableProps<TData>["renderToolbar"]>>[0]["table"];
  selectedCount: number;
};

export type CrmEntityDataTableProps<TData> = EntityDataTableProps<TData> & {
  minWidthClassName?: string;
  infoContent?: ReactNode;
  selectionActions?: ReactNode | ((context: CrmToolbarContext<TData>) => ReactNode);
  rowActions?: (row: TData) => ReactNode;
};

export function CrmEntityDataTable<TData>({
  minWidthClassName,
  infoContent,
  toolbarActions,
  renderToolbar,
  selectionActions,
  rowActions,
  columns,
  ...props
}: CrmEntityDataTableProps<TData>) {
  const resolvedColumns = useMemo<DataTableColumnDef<TData>[]>(() => {
    if (!rowActions) {
      return columns;
    }

    const rowActionsColumn: DataTableColumnDef<TData> = {
      id: "__actions",
      header: "",
      meta: { disableReorder: true },
      enableHiding: false,
      cell: ({ row }) => rowActions(row.original),
    };

    return [...columns, rowActionsColumn];
  }, [columns, rowActions]);

  const resolvedToolbarActions =
    typeof toolbarActions === "function" ? (
      (context: Parameters<Exclude<typeof toolbarActions, ReactNode>>[0]) => (
        <div className="flex flex-wrap items-center gap-2">{toolbarActions(context)}</div>
      )
    ) : toolbarActions ? (
      <div className="flex flex-wrap items-center gap-2">{toolbarActions}</div>
    ) : undefined;

  const resolvedRenderToolbar = renderToolbar
    ? (context: Parameters<Exclude<typeof renderToolbar, undefined>>[0]) => (
        <div className="flex w-full items-center justify-between gap-2 border-b border-border/70 px-4 py-2 text-xs text-muted-foreground">
          <div className="flex min-w-0 flex-1 justify-between items-center gap-2">
            {renderToolbar(context)}
          </div>
          {selectionActions ? (
            <div className="inline-flex shrink-0 items-center gap-2">
              {typeof selectionActions === "function"
                ? selectionActions({
                    table: context.table,
                    selectedCount: context.table.getSelectedRowModel().rows.length,
                  })
                : selectionActions}
            </div>
          ) : null}
        </div>
      )
    : undefined;

  return (
    <div className="min-w-0">
      {infoContent ? <div>{infoContent}</div> : null}
      <div className={cn(minWidthClassName)}>
        <EntityDataTable
          {...props}
          columns={resolvedColumns}
          {...(resolvedToolbarActions ? { toolbarActions: resolvedToolbarActions } : {})}
          {...(resolvedRenderToolbar ? { renderToolbar: resolvedRenderToolbar } : {})}
        />
      </div>
    </div>
  );
}
