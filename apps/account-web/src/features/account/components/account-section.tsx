import type React from "react";
import { Text, cn } from "@netmetric/ui";

import { ReadOnlyValue } from "./read-only-value";

type AccountSectionProps = {
  title?: React.ReactNode;
  description?: React.ReactNode;
  children?: React.ReactNode;
  className?: string;
  contentClassName?: string;
};

export function AccountSection({
  title,
  description,
  children,
  className,
  contentClassName,
}: AccountSectionProps) {
  return (
    <section className={cn("min-w-0 space-y-4 border-b border-border/70 pb-5", className)}>
      {title || description ? (
        <div className="space-y-1">
          {title ? <Text className="text-base font-semibold text-foreground">{title}</Text> : null}
          {description ? (
            <Text className="text-sm text-muted-foreground">{description}</Text>
          ) : null}
        </div>
      ) : null}
      {children ? <div className={cn("min-w-0", contentClassName)}>{children}</div> : null}
    </section>
  );
}

export function AccountField({
  label,
  value,
  className,
}: {
  label: React.ReactNode;
  value: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={cn("space-y-1 rounded-md border border-border/70 px-4 py-3", className)}>
      <Text className="text-sm text-muted-foreground">{label}</Text>
      {typeof value === "string" ||
      typeof value === "number" ||
      value === null ||
      value === undefined ? (
        <ReadOnlyValue value={value} />
      ) : (
        <div className="text-sm font-medium text-foreground">{value}</div>
      )}
    </div>
  );
}

export function AccountActionLink({ children, className, ...props }: React.ComponentProps<"a">) {
  return (
    <a
      className={cn(
        "inline-flex h-8 items-center rounded-sm border border-border px-3 text-sm font-medium transition-colors hover:bg-muted focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
        className,
      )}
      {...props}
    >
      {children}
    </a>
  );
}
