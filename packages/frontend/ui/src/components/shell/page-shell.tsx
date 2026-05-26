import { cn } from "../../lib/utils";
import { Heading } from "../typography/heading";
import { TextTitle } from "../typography/text";

import type { ReactNode } from "react";

type PageShellSurface = "panel" | "plain";
type PageShellBodyPadding = "default" | "compact" | "none";
type PageShellVariant = "default" | "crm";

export type PageHeaderProps = {
  title: ReactNode;
  description?: ReactNode;
  actions?: ReactNode;
  className?: string | undefined;
};

export type PageShellProps = Omit<PageHeaderProps, "className"> & {
  children?: ReactNode;
  bodyClassName?: string | undefined;
  bodyPadding?: PageShellBodyPadding;
  className?: string | undefined;
  headerClassName?: string | undefined;
  surface?: PageShellSurface;
  variant?: PageShellVariant;
};

const surfaceClasses: Record<PageShellSurface, string> = {
  panel: "rounded-md border border-border bg-[image:var(--netmetric-app-background-image)]",
  plain: "",
};

const bodyPaddingClasses: Record<PageShellBodyPadding, string> = {
  default: "",
  compact: "",
  none: "",
};

const variantClasses: Record<PageShellVariant, { root: string; header: string; body: string }> = {
  default: { root: "", header: "", body: "" },
  crm: {
    root: "bg-[image:var(--netmetric-app-background-image)]",
    header:
      "relative h-auto min-h-[4rem] items-start overflow-hidden border-b border-border/60 bg-[image:var(--crm-hero-glow)] px-5 py-5 before:pointer-events-none before:absolute before:inset-0 before:bg-[image:var(--crm-page-grid)] before:bg-[size:28px_28px] before:opacity-70 sm:px-6",
    body: "space-y-6 bg-[linear-gradient(180deg,rgb(248_250_252_/_0.5),transparent_16rem)] dark:bg-[linear-gradient(180deg,rgb(15_23_42_/_0.28),transparent_16rem)]",
  },
};

export function PageHeader({ title, description, actions, className }: Readonly<PageHeaderProps>) {
  return (
    <div
      className={cn(
        "flex h-16 shrink-0 items-center justify-between border-b border-border px-4",
        className,
      )}
    >
      <div className="min-w-0 space-y-0.5">
        <Heading level={4}>{title}</Heading>
        {description ? (
          <TextTitle className="truncate text-xs text-muted-foreground">{description}</TextTitle>
        ) : null}
      </div>
      {actions ? <div className="flex shrink-0 items-center gap-2">{actions}</div> : null}
    </div>
  );
}

export function PageShell({
  title,
  description,
  actions,
  children,
  className,
  headerClassName,
  bodyClassName,
  bodyPadding = "default",
  surface = "panel",
  variant = "default",
}: Readonly<PageShellProps>) {
  const variantClassSet = variantClasses[variant];

  return (
    <div
      className={cn(
        "flex h-full min-h-0 w-full min-w-0 flex-col overflow-hidden",
        surfaceClasses[surface],
        variantClassSet.root,
        className,
      )}
    >
      <PageHeader
        title={title}
        description={description}
        actions={actions}
        className={cn(variantClassSet.header, headerClassName)}
      />
      {children ? (
        <div
          className={cn(
            "min-h-0 flex-1 overflow-x-hidden overflow-y-auto",
            bodyPaddingClasses[bodyPadding],
            variantClassSet.body,
            bodyClassName,
          )}
        >
          {children}
        </div>
      ) : null}
    </div>
  );
}

export const AppPagePanel = PageShell;
