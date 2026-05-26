import { cn } from "../../lib/utils";

import type { HTMLAttributes } from "react";

export function Text({ className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn("leading-7 [&:not(:first-child)]:mt-6", className)} {...props} />;
}

export function TextTitle({ className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn("leading-5", className)} {...props} />;
}
