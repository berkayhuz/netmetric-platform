import * as React from "react";

import { cn } from "../../lib/utils";

export const compactActionControlsClassName =
  "[&_[data-slot='button']]:h-8 [&_[data-slot='button']]:gap-1 [&_[data-slot='button']]:px-2.5 [&_[data-slot='button']]:text-[0.8rem] [&_[data-slot='button']]:[&_svg:not([class*='size-'])]:size-3.5";

export function CompactActionGroup({
  children,
  className,
}: Readonly<{
  children?: React.ReactNode;
  className?: string;
}>): React.JSX.Element {
  return (
    <div
      className={cn("flex flex-wrap items-center gap-2", compactActionControlsClassName, className)}
    >
      {children}
    </div>
  );
}
