"use client";

import { Columns3 } from "lucide-react";
import * as React from "react";

import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "../../overlay/dropdown-menu";
import { Button } from "../../primitives/button";

import type { DataTableColumnMeta } from "./data-table-types";
import type { Column, Table as TanstackTable } from "@tanstack/react-table";

interface DataTableViewOptionsProps<TData> {
  table: TanstackTable<TData>;
  label?: string;
  title?: string;
}

function getColumnLabel<TData>(column: Column<TData, unknown>): string {
  const meta = column.columnDef.meta as DataTableColumnMeta | undefined;
  if (meta?.label) {
    return meta.label;
  }

  if (typeof column.columnDef.header === "string") {
    return column.columnDef.header;
  }

  return column.id;
}

export function DataTableViewOptions<TData>({
  table,
  label = "View columns",
  title = "Columns",
}: DataTableViewOptionsProps<TData>): React.JSX.Element | null {
  const columns = table
    .getAllLeafColumns()
    .filter((column) => column.getCanHide() && column.id !== "__select");

  if (columns.length === 0) {
    return null;
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button type="button" variant="outline" size="sm">
            <Columns3 aria-hidden="true" />
            {label}
          </Button>
        }
      />
      <DropdownMenuContent align="end" className="min-w-44">
        <DropdownMenuGroup>
          <DropdownMenuLabel>{title}</DropdownMenuLabel>
          <DropdownMenuSeparator />
          {columns.map((column) => (
            <DropdownMenuCheckboxItem
              key={column.id}
              checked={column.getIsVisible()}
              onCheckedChange={(checked) => column.toggleVisibility(Boolean(checked))}
            >
              {getColumnLabel(column)}
            </DropdownMenuCheckboxItem>
          ))}
        </DropdownMenuGroup>
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
