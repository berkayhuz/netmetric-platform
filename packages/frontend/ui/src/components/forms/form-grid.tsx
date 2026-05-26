import { cn } from "../../lib/utils";

import { FieldSet } from "./field";

type FormGridProps = React.ComponentProps<typeof FieldSet> & {
  columns?: 1 | 2;
};

export function FormGrid({ className, columns = 1, ...props }: FormGridProps) {
  return (
    <FieldSet
      className={cn(columns === 2 ? "grid gap-4 sm:grid-cols-2" : "grid gap-4", className)}
      {...props}
    />
  );
}
