"use client";

import * as React from "react";

import { Skeleton } from "../../layout/skeleton";
import { TableCell, TableRow } from "../table";

interface DataTableSkeletonProps {
  columnCount: number;
  rowCount?: number;
}

export function DataTableSkeleton({
  columnCount,
  rowCount = 5,
}: DataTableSkeletonProps): React.JSX.Element {
  return (
    <>
      {Array.from({ length: rowCount }).map((_, rowIndex) => (
        <TableRow key={`skeleton-row-${rowIndex}`}>
          {Array.from({ length: columnCount }).map((__, columnIndex) => (
            <TableCell key={`skeleton-cell-${rowIndex}-${columnIndex}`}>
              <Skeleton
                className={
                  columnIndex === 0 ? "h-4 w-36" : columnIndex % 3 === 0 ? "h-4 w-20" : "h-4 w-28"
                }
              />
            </TableCell>
          ))}
        </TableRow>
      ))}
    </>
  );
}
