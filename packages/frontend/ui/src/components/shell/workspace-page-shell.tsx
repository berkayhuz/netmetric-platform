import { PageShell, type PageShellProps } from "./page-shell";

import type { ReactNode } from "react";

export type WorkspacePageShellVariant = "crm" | "account";

export type WorkspacePageShellProps = Omit<PageShellProps, "variant"> & {
  variant: WorkspacePageShellVariant;
  children?: ReactNode;
};

export function WorkspacePageShell({ variant, ...props }: Readonly<WorkspacePageShellProps>) {
  return (
    <PageShell
      {...props}
      variant={variant === "crm" || variant === "account" ? "crm" : "default"}
    />
  );
}
