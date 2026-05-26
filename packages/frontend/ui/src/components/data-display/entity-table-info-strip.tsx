import * as React from "react";

import { cn } from "../../lib/utils";

export function EntityTableInfoStrip({
  children,
  className,
}: Readonly<{ children: React.ReactNode; className?: string }>) {
  return (
    <div
      className={cn(
        "flex flex-wrap items-center gap-x-3 gap-y-1 px-3 pt-2 text-xs font-medium text-muted-foreground",
        className,
      )}
    >
      {children}
    </div>
  );
}
