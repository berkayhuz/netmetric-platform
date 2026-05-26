import type { ReactNode } from "react";

import { WorkspacePageShell } from "@netmetric/ui";

import { tCrm } from "@/lib/i18n/crm-i18n";

import { getCrmPageMetadata, type CrmPagePath } from "./crm-page-metadata";
import { CrmPageHeaderActionScope } from "./crm-page-header-actions";

type CrmPageShellProps = {
  children?: ReactNode;
  actions?: ReactNode;
  bodyClassName?: string | undefined;
  bodyPadding?: "default" | "compact" | "none";
  className?: string | undefined;
  description?: string | undefined;
  locale?: string | null | undefined;
  routePath?: CrmPagePath | undefined;
  title?: ReactNode | undefined;
};

function resolveRouteCopy(routePath: CrmPagePath | undefined, locale: string | null | undefined) {
  if (!routePath) {
    return null;
  }

  const metadata = getCrmPageMetadata(routePath);
  return {
    title: tCrm(metadata.titleKey, locale),
    description: tCrm(metadata.descriptionKey, locale),
  };
}

export function CrmPageShell({
  children,
  actions,
  bodyClassName,
  bodyPadding = "compact",
  className,
  description,
  locale,
  routePath,
  title,
}: Readonly<CrmPageShellProps>) {
  const routeCopy = resolveRouteCopy(routePath, locale);
  const resolvedTitle = title ?? routeCopy?.title;
  const resolvedDescription = description ?? routeCopy?.description;
  const resolvedActions = actions ? (
    <CrmPageHeaderActionScope>{actions}</CrmPageHeaderActionScope>
  ) : undefined;

  if (!resolvedTitle) {
    throw new Error("CrmPageShell requires either title or routePath.");
  }

  return (
    <WorkspacePageShell
      variant="crm"
      surface="plain"
      title={resolvedTitle}
      description={resolvedDescription}
      actions={resolvedActions}
      bodyPadding={bodyPadding}
      className={className}
      bodyClassName={bodyClassName}
    >
      {children}
    </WorkspacePageShell>
  );
}
