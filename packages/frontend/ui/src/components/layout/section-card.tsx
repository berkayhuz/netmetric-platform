import { cn } from "../../lib/utils";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./card";

import type { ReactNode } from "react";

export function SectionCard({
  title,
  description,
  actions,
  children,
  className,
  contentClassName,
}: Readonly<{
  title: string;
  description?: string | undefined;
  actions?: ReactNode | undefined;
  children: ReactNode;
  className?: string | undefined;
  contentClassName?: string | undefined;
}>) {
  return (
    <Card
      aria-label={title}
      className={cn(
        "overflow-hidden rounded-2xl border-border/70 bg-card/95 shadow-[0_18px_55px_rgb(15_23_42_/_0.08)]",
        className,
      )}
    >
      <CardHeader className="gap-3 border-b border-border/60 bg-muted/10">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
          <div className="min-w-0 space-y-1">
            <CardTitle>{title}</CardTitle>
            {description ? <CardDescription>{description}</CardDescription> : null}
          </div>
          {actions ? <div className="shrink-0">{actions}</div> : null}
        </div>
      </CardHeader>
      <CardContent className={cn("pt-4", contentClassName)}>{children}</CardContent>
    </Card>
  );
}
