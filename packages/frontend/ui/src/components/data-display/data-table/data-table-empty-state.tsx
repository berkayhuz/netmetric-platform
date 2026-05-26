"use client";

import * as React from "react";

import { cn } from "../../../lib/utils";
import {
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
} from "../empty";

interface DataTableEmptyStateProps {
  title: string;
  description?: string | undefined;
  action?: React.ReactNode | undefined;
  icon?: React.ReactNode | undefined;
  className?: string | undefined;
  role?: "status" | "alert";
}

export function DataTableEmptyState({
  title,
  description,
  action,
  icon,
  className,
  role = "status",
}: DataTableEmptyStateProps): React.JSX.Element {
  return (
    <Empty
      className={cn(
        "min-h-[clamp(14rem,34vh,22rem)] w-full border-none px-6 py-8 text-center",
        "flex items-center justify-center",
        className,
      )}
      role={role}
      aria-live={role === "status" ? "polite" : undefined}
    >
      {icon ? <EmptyMedia>{icon}</EmptyMedia> : null}
      <EmptyHeader>
        <EmptyTitle>{title}</EmptyTitle>
        {description ? <EmptyDescription>{description}</EmptyDescription> : null}
      </EmptyHeader>
      {action ? <EmptyContent>{action}</EmptyContent> : null}
    </Empty>
  );
}
