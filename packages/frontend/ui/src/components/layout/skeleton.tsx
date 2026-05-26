import { cva } from "class-variance-authority";

import { cn } from "../../lib/utils";

const skeletonVariants = cva("animate-pulse rounded-sm bg-muted");

function Skeleton({ className, ...props }: React.ComponentProps<"div">) {
  return <div data-slot="skeleton" className={cn(skeletonVariants(), className)} {...props} />;
}

export { Skeleton, skeletonVariants };
