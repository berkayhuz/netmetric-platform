import { cn } from "../../lib/utils";

import type { ComponentProps } from "react";

function ButtonGroup({ className, ...props }: ComponentProps<"div">) {
  return (
    <div
      data-slot="button-group"
      role="group"
      className={cn(
        "inline-flex w-fit items-center rounded-sm shadow-xs [&>button:not(:first-child)]:rounded-l-none [&>button:not(:last-child)]:rounded-r-none [&>button:not(:first-child)]:border-l-0",
        className,
      )}
      {...props}
    />
  );
}

export { ButtonGroup };
