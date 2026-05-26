"use client";

import { ArrowDown, ArrowUp, ChevronsUpDown } from "lucide-react";
import * as React from "react";

import { cn } from "../../../lib/utils";
import { Button } from "../../primitives/button";

import type { Column } from "@tanstack/react-table";

interface DataTableColumnHeaderProps<TData, TValue> {
  column: Column<TData, TValue>;
  title: string;
  className?: string;
}

function SortIcon({ sorted }: { sorted: false | "asc" | "desc" }): React.JSX.Element {
  if (sorted === "asc") {
    return <ArrowUp aria-hidden="true" className="size-3.5" />;
  }

  if (sorted === "desc") {
    return <ArrowDown aria-hidden="true" className="size-3.5" />;
  }

  return <ChevronsUpDown aria-hidden="true" className="size-3.5 text-muted-foreground" />;
}

export function DataTableColumnHeader<TData, TValue>({
  column,
  title,
  className,
}: DataTableColumnHeaderProps<TData, TValue>): React.JSX.Element {
  if (!column.getCanSort()) {
    return <span className={className}>{title}</span>;
  }

  const sorted = column.getIsSorted();

  return (
    <Button
      type="button"
      variant="ghost"
      size="sm"
      className={cn("-ml-2 h-8 px-2 font-medium", className)}
      aria-label={`Sort by ${title}`}
      onClick={() => column.toggleSorting(sorted === "asc")}
    >
      <span>{title}</span>
      <SortIcon sorted={sorted} />
    </Button>
  );
}
