"use client";

import { Check, ListFilter, X } from "lucide-react";
import * as React from "react";

import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "../../overlay/dropdown-menu";
import { Button } from "../../primitives/button";
import { Badge } from "../badge";

import type { DataTableFacetedFilterOption } from "./data-table-types";
import type { Table as TanstackTable } from "@tanstack/react-table";

interface DataTableFacetedFilterProps<TData> {
  table: TanstackTable<TData>;
  columnId: string;
  title: string;
  options: DataTableFacetedFilterOption[];
  multiple?: boolean | undefined;
  emptyLabel?: string | undefined;
  clearLabel?: string | undefined;
}

function toSelectedValues(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.filter((item): item is string => typeof item === "string" && item.length > 0);
  }

  if (typeof value === "string" && value.length > 0) {
    return [value];
  }

  if (typeof value === "number" || typeof value === "boolean") {
    return [String(value)];
  }

  return [];
}

export function DataTableFacetedFilter<TData>({
  table,
  columnId,
  title,
  options,
  multiple = true,
  emptyLabel = "No options",
  clearLabel = "Clear",
}: DataTableFacetedFilterProps<TData>): React.JSX.Element | null {
  const column = table.getColumn(columnId);

  if (!column) {
    return null;
  }

  const selectedValues = new Set(toSelectedValues(column.getFilterValue()));
  const selectedCount = selectedValues.size;
  const facets = column.getFacetedUniqueValues();

  function commit(nextValues: string[]): void {
    column?.setFilterValue(nextValues.length > 0 ? nextValues : undefined);
  }

  function toggleValue(value: string): void {
    const nextValues = new Set(selectedValues);

    if (!multiple) {
      commit(selectedValues.has(value) ? [] : [value]);
      return;
    }

    if (nextValues.has(value)) {
      nextValues.delete(value);
    } else {
      nextValues.add(value);
    }

    commit(Array.from(nextValues));
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button type="button" variant="outline" size="sm">
            <ListFilter aria-hidden="true" />
            {title}
            {selectedCount > 0 ? <Badge variant="secondary">{selectedCount}</Badge> : null}
          </Button>
        }
      />
      <DropdownMenuContent align="start" className="min-w-56">
        <DropdownMenuGroup>
          <DropdownMenuLabel>{title}</DropdownMenuLabel>
          <DropdownMenuSeparator />
          {options.length === 0 ? (
            <DropdownMenuItem disabled>{emptyLabel}</DropdownMenuItem>
          ) : (
            options.map((option) => {
              const Icon = option.icon;
              const isSelected = selectedValues.has(option.value);
              const count = option.count ?? facets.get(option.value);

              return (
                <DropdownMenuCheckboxItem
                  key={option.value}
                  checked={isSelected}
                  disabled={option.disabled}
                  onCheckedChange={() => toggleValue(option.value)}
                >
                  {Icon ? <Icon className="size-3.5 text-muted-foreground" /> : null}
                  <span className="min-w-0 flex-1 truncate">{option.label}</span>
                  {typeof count === "number" ? (
                    <span className="ml-auto text-xs tabular-nums text-muted-foreground">
                      {count}
                    </span>
                  ) : null}
                  {isSelected ? <Check aria-hidden="true" className="sr-only" /> : null}
                </DropdownMenuCheckboxItem>
              );
            })
          )}
        </DropdownMenuGroup>
        {selectedCount > 0 ? (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuItem onClick={() => commit([])}>
              <X aria-hidden="true" />
              {clearLabel}
            </DropdownMenuItem>
          </>
        ) : null}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
