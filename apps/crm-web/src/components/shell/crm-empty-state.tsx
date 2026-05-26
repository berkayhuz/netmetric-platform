import type { ReactNode } from "react";

import {
  EmptyState,
  Empty,
  EmptyContent,
  EmptyDescription,
  EmptyHeader,
  EmptyMedia,
  EmptyTitle,
  cn,
} from "@netmetric/ui";

export function CrmEmptyState({
  title,
  description,
  icon,
  actions,
  compact = false,
  className,
}: Readonly<{
  title: string;
  description: string;
  icon?: ReactNode | undefined;
  actions?: ReactNode | undefined;
  compact?: boolean | undefined;
  className?: string | undefined;
}>) {
  if (!icon && !actions && !compact && !className) {
    return <EmptyState title={title} description={description} />;
  }

  return (
    <Empty
      className={cn(
        "w-full border border-dashed bg-muted/10 text-center",
        "flex items-center justify-center",
        compact ? "min-h-40 p-4" : "min-h-[clamp(14rem,34vh,22rem)] p-8",
        className,
      )}
    >
      <EmptyHeader>
        {icon ? (
          <EmptyMedia variant="icon" className="size-10 rounded-lg">
            {icon}
          </EmptyMedia>
        ) : null}
        <EmptyTitle className="text-base">{title}</EmptyTitle>
        <EmptyDescription>{description}</EmptyDescription>
      </EmptyHeader>
      {actions ? <EmptyContent>{actions}</EmptyContent> : null}
    </Empty>
  );
}
