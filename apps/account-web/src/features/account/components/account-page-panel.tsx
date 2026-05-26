import type React from "react";
import { WorkspacePageShell, cn } from "@netmetric/ui";

type AccountPagePanelProps = {
  title: string;
  description?: string;
  children: React.ReactNode;
  contentClassName?: string;
  bodyPadding?: "default" | "none";
};

export function AccountPagePanel({
  title,
  description,
  children,
  contentClassName,
  bodyPadding = "default",
}: AccountPagePanelProps) {
  return (
    <section className="min-h-0 min-w-0 flex-1 overflow-hidden">
      <WorkspacePageShell
        variant="account"
        title={title}
        description={description}
        bodyPadding={bodyPadding}
      >
        <div className={cn("min-w-0 space-y-6", contentClassName)}>{children}</div>
      </WorkspacePageShell>
    </section>
  );
}
