import type { ReactNode } from "react";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";

import { Button } from "@netmetric/ui";

import type { CrmPagePath } from "@/components/shell/crm-page-metadata";
import { getCrmPageMetadata } from "@/components/shell/crm-page-metadata";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { tCrm } from "@/lib/i18n/crm-i18n";

export function CrmEntityFormShell({
  title,
  description,
  children,
  locale,
  routePath,
}: Readonly<{
  title?: string;
  description?: string;
  children: ReactNode;
  locale?: string | null;
  routePath?: CrmPagePath;
}>) {
  const routeCopy = routePath ? getCrmPageMetadata(routePath) : null;
  const resolvedTitle = title ?? (routeCopy ? tCrm(routeCopy.titleKey, locale) : undefined);
  const resolvedDescription =
    description ?? (routeCopy ? tCrm(routeCopy.descriptionKey, locale) : undefined);

  if (!resolvedTitle || !resolvedDescription) {
    throw new Error("CrmEntityFormShell requires title/description or routePath.");
  }

  const backToListPath =
    routePath && routePath.endsWith("/new")
      ? routePath === "/tasks/meetings/new"
        ? "/tasks"
        : routePath.replace(/\/new$/, "")
      : null;

  return (
    <CrmPageShell
      title={resolvedTitle}
      description={resolvedDescription}
      actions={
        backToListPath ? (
          <Button asChild variant="outline" size="sm">
            <Link href={backToListPath}>
              <ArrowLeft aria-hidden="true" className="size-4" />
              {tCrm("crm.common.backToList", locale)}
            </Link>
          </Button>
        ) : null
      }
    >
      <div className="mx-auto w-full pt-6 pb-16 px-4 md:px-6">
        <div className="max-w-4xl">{children}</div>
      </div>
    </CrmPageShell>
  );
}
