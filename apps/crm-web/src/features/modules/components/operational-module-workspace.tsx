import { Alert, AlertDescription, AlertTitle, Badge, Button, Text } from "@netmetric/ui";
import Link from "next/link";
import {
  Activity,
  AlertTriangle,
  CheckCircle2,
  Database,
  Layers3,
  LockKeyhole,
} from "lucide-react";

import { CrmMetricGrid, CrmSectionCard } from "@/components/shell/crm-content-primitives";
import { CrmEmptyState } from "@/components/shell/crm-empty-state";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CrmRecordsTable, type CrmRecordsTableRow } from "@/components/shell/crm-records-table";
import { OperationalModuleQuickCreate } from "@/features/modules/components/operational-module-quick-create";
import type { CrmModuleRegistryItem } from "@/features/modules/module-registry";
import type {
  CrmOperationalEndpointResult,
  CrmOperationalModuleData,
  CrmOperationalPreview,
} from "@/features/modules/data/operational-module-data";
import {
  getCrmGroupLabel,
  getCrmModuleDescription,
  getCrmModuleTitle,
  tCrm,
} from "@/lib/i18n/crm-i18n";

type InsightItem = {
  label: string;
  value: string;
};

export function OperationalModuleWorkspace({
  moduleItem,
  data,
  locale,
}: Readonly<{
  moduleItem: CrmModuleRegistryItem;
  data: CrmOperationalModuleData;
  locale?: string | null;
}>) {
  const readableSections = data.endpoints.filter(isReadableBusinessSection);
  const visibleSections = readableSections.filter(
    (endpoint) => endpoint.status === "loaded" || endpoint.status === "empty",
  );
  const failedSections = readableSections.filter((endpoint) => endpoint.status === "failed");
  const pendingSelectionSections = readableSections.filter(
    (endpoint) => endpoint.status === "skipped" && endpoint.requiresSelection,
  );
  const primarySection = getPrimarySection(visibleSections);
  const insightItems = createInsightItems(primarySection);
  const operationSections = readableSections.filter((endpoint) => endpoint.kind !== "mutation");
  const recordCount = visibleSections.reduce(
    (total, endpoint) => total + (endpoint.payload?.count ?? 0),
    0,
  );
  const healthTone = failedSections.length > 0 ? "danger" : "success";

  return (
    <CrmPageShell
      title={getCrmModuleTitle(moduleItem, locale)}
      description={getCrmModuleDescription(moduleItem, locale)}
    >
      <section
        className="relative overflow-hidden rounded-2xl border border-border/70 bg-[image:var(--crm-hero-glow)] p-5 shadow-[0_24px_70px_rgb(15_23_42_/_0.10)]"
        aria-label={data.config.title}
      >
        <div className="pointer-events-none absolute inset-0 bg-[image:var(--crm-page-grid)] bg-[size:28px_28px] opacity-55" />
        <div className="relative">
          <div className="max-w-3xl space-y-3">
            <div className="flex flex-wrap items-center gap-2">
              <Badge variant="outline" className="rounded-sm">
                <Layers3 aria-hidden="true" />
                {getCrmGroupLabel(moduleItem.group, locale)}
              </Badge>
              <Badge variant={failedSections.length > 0 ? "destructive" : "secondary"}>
                {failedSections.length > 0 ? (
                  <AlertTriangle aria-hidden="true" />
                ) : (
                  <CheckCircle2 aria-hidden="true" />
                )}
                {failedSections.length > 0
                  ? tCrm("crm.modules.workspace.needsReview", locale)
                  : tCrm("crm.modules.workspace.live", locale)}
              </Badge>
            </div>
            <div className="space-y-1">
              <h2 className="text-lg font-semibold tracking-tight">
                {tCrm("crm.modules.workspace.overviewTitle", locale)}
              </h2>
              <p className="text-sm leading-6 text-muted-foreground">{data.config.description}</p>
            </div>
          </div>

          <div className="mt-4">
            <CrmMetricGrid
              columns="three"
              items={[
                {
                  label: tCrm("crm.modules.workspace.businessRecords", locale),
                  value: recordCount,
                  description: tCrm("crm.modules.workspace.businessRecordsDescription", locale),
                  icon: <Database aria-hidden="true" className="size-4" />,
                },
                {
                  label: tCrm("crm.modules.workspace.workAreas", locale),
                  value: visibleSections.length,
                  description: tCrm("crm.modules.workspace.workAreasDescription", locale),
                  icon: <Layers3 aria-hidden="true" className="size-4" />,
                },
                {
                  label: tCrm("crm.modules.workspace.needsReview", locale),
                  value: failedSections.length,
                  description: tCrm("crm.modules.workspace.needsReviewDescription", locale),
                  icon: <AlertTriangle aria-hidden="true" className="size-4" />,
                  tone: healthTone,
                },
              ]}
            />
          </div>
        </div>
      </section>

      {failedSections.length > 0 ? (
        <Alert variant="destructive">
          <AlertTitle>{tCrm("crm.modules.workspace.unavailableTitle", locale)}</AlertTitle>
          <AlertDescription>
            {tCrm("crm.modules.workspace.unavailableDescription", locale)}
          </AlertDescription>
        </Alert>
      ) : null}

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1fr)_minmax(21rem,0.42fr)]">
        <CrmSectionCard
          title={primarySection?.label ?? tCrm("crm.modules.workspace.primaryView", locale)}
          description={tCrm("crm.modules.workspace.primaryViewDescription", locale)}
          actions={
            primarySection ? (
              <EndpointBadge endpoint={primarySection} locale={locale} />
            ) : (
              <Badge variant="secondary">{tCrm("crm.modules.workspace.noDataYet", locale)}</Badge>
            )
          }
          contentClassName="space-y-4"
        >
          {insightItems.length > 0 ? (
            <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
              {insightItems.map((item) => (
                <div className="rounded-lg border bg-muted/20 p-3" key={item.label}>
                  <Text className="text-xs font-medium uppercase text-muted-foreground">
                    {item.label}
                  </Text>
                  <Text className="mt-1 text-sm font-medium leading-6">{item.value}</Text>
                </div>
              ))}
            </div>
          ) : null}

          {primarySection?.payload?.preview &&
          primarySection.payload.preview.columns.length > 0 &&
          primarySection.payload.preview.rows.length > 0 ? (
            <PreviewTable preview={primarySection.payload.preview} />
          ) : insightItems.length === 0 ? (
            <EmptyBusinessState locale={locale} />
          ) : null}
        </CrmSectionCard>

        <aside className="space-y-4">
          <OperationalModuleQuickCreate locale={locale} modulePath={data.config.path} />

          <CrmSectionCard
            title={tCrm("crm.modules.workspace.operationalFocus", locale)}
            description={tCrm("crm.modules.workspace.operationalFocusDescription", locale)}
          >
            <div className="space-y-3">
              {visibleSections.slice(0, 6).map((endpoint) => (
                <EndpointFocusItem endpoint={endpoint} key={endpoint.id} locale={locale} />
              ))}
              {visibleSections.length === 0 ? <EmptyBusinessState locale={locale} compact /> : null}
            </div>
          </CrmSectionCard>
        </aside>
      </div>

      {operationSections.length > 0 ? (
        <CrmSectionCard
          title={tCrm("crm.modules.workspace.workAreas", locale)}
          description={tCrm("crm.modules.workspace.workAreasDescription", locale)}
          contentClassName="grid gap-3 md:grid-cols-2 xl:grid-cols-3"
        >
          {operationSections.map((endpoint) => (
            <OperationDirectoryItem
              endpoint={endpoint}
              href={`${data.config.path}/${endpoint.id}`}
              key={endpoint.id}
              locale={locale}
            />
          ))}
        </CrmSectionCard>
      ) : null}

      {pendingSelectionSections.length > 0 ? (
        <CrmSectionCard
          title={tCrm("crm.modules.workspace.recordWorkspaces", locale)}
          description={tCrm("crm.modules.workspace.recordWorkspacesDescription", locale)}
          contentClassName="grid gap-3 md:grid-cols-2 xl:grid-cols-3"
        >
          {pendingSelectionSections.map((endpoint) => (
            <div className="rounded-lg border bg-muted/15 p-4" key={endpoint.id}>
              <div className="flex items-start gap-3">
                <div className="rounded-md border bg-background p-2 text-muted-foreground">
                  <LockKeyhole aria-hidden="true" className="size-4" />
                </div>
                <div className="min-w-0 space-y-1">
                  <Text className="text-sm font-medium">{endpoint.label}</Text>
                  <Text className="text-xs leading-5 text-muted-foreground">
                    {tCrm("crm.modules.workspace.selectedRecordRequired", locale)}
                  </Text>
                </div>
              </div>
            </div>
          ))}
        </CrmSectionCard>
      ) : null}
    </CrmPageShell>
  );
}

export function OperationalModuleOperationPage({
  moduleItem,
  data,
  operationId,
  locale,
}: Readonly<{
  moduleItem: CrmModuleRegistryItem;
  data: CrmOperationalModuleData;
  operationId: string;
  locale?: string | null;
}>) {
  const endpoint = data.endpoints.find((item) => item.id === operationId);
  const fallbackEndpoint = data.endpoints.find(isReadableBusinessSection);
  const selected = endpoint ?? fallbackEndpoint;
  const siblingEndpoints = data.endpoints.filter(
    (item) => isReadableBusinessSection(item) && item.kind !== "mutation",
  );

  return (
    <CrmPageShell
      title={selected?.label ?? getCrmModuleTitle(moduleItem, locale)}
      description={getCrmModuleDescription(moduleItem, locale)}
      actions={
        <Button asChild variant="outline">
          <Link href={data.config.path}>{getCrmModuleTitle(moduleItem, locale)}</Link>
        </Button>
      }
    >
      <div className="grid gap-4 xl:grid-cols-[18rem_minmax(0,1fr)]">
        <aside className="space-y-2 rounded-lg border bg-card p-3 shadow-xs">
          {siblingEndpoints.map((item) => (
            <Link
              key={item.id}
              href={`${data.config.path}/${item.id}`}
              className={`block rounded-md px-3 py-2 text-sm transition-colors hover:bg-muted ${
                item.id === selected?.id
                  ? "bg-muted font-medium text-foreground"
                  : "text-muted-foreground"
              }`}
            >
              {item.label}
            </Link>
          ))}
        </aside>

        <CrmSectionCard
          title={selected?.label ?? tCrm("crm.modules.workspace.primaryView", locale)}
          description={formatOperationDescription(selected, locale)}
          actions={selected ? <EndpointBadge endpoint={selected} locale={locale} /> : null}
          contentClassName="space-y-4"
        >
          {selected?.payload?.preview &&
          selected.payload.preview.columns.length > 0 &&
          selected.payload.preview.rows.length > 0 ? (
            <PreviewTable preview={selected.payload.preview} />
          ) : selected ? (
            <EndpointEmptyState endpoint={selected} locale={locale} />
          ) : (
            <EmptyBusinessState locale={locale} />
          )}
        </CrmSectionCard>
      </div>
    </CrmPageShell>
  );
}

function EndpointFocusItem({
  endpoint,
  locale,
}: Readonly<{ endpoint: CrmOperationalEndpointResult; locale?: string | null | undefined }>) {
  return (
    <div className="flex items-center justify-between gap-3 rounded-lg border bg-muted/10 p-3">
      <div className="flex min-w-0 items-start gap-3">
        <div className="rounded-md border bg-background p-2 text-muted-foreground">
          <Activity aria-hidden="true" className="size-4" />
        </div>
        <div className="min-w-0">
          <Text className="truncate text-sm font-medium">{endpoint.label}</Text>
          <Text className="text-xs leading-5 text-muted-foreground">
            {formatPayloadSummary(endpoint, locale)}
          </Text>
        </div>
      </div>
      <EndpointBadge endpoint={endpoint} locale={locale} />
    </div>
  );
}

function OperationDirectoryItem({
  endpoint,
  href,
  locale,
}: Readonly<{
  endpoint: CrmOperationalEndpointResult;
  href: string;
  locale?: string | null | undefined;
}>) {
  return (
    <Link
      href={href}
      className="group rounded-lg border bg-background p-4 transition-colors hover:border-primary/40 hover:bg-muted/20"
    >
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0 space-y-1">
          <h3 className="truncate text-sm font-semibold">{endpoint.label}</h3>
          <p className="text-sm leading-5 text-muted-foreground">
            {formatPayloadSummary(endpoint, locale)}
          </p>
        </div>
        <EndpointBadge endpoint={endpoint} locale={locale} />
      </div>
    </Link>
  );
}

function EndpointBadge({
  endpoint,
  locale,
}: Readonly<{ endpoint: CrmOperationalEndpointResult; locale?: string | null | undefined }>) {
  if (endpoint.status === "empty") {
    return <Badge variant="secondary">{tCrm("crm.modules.workspace.noDataYet", locale)}</Badge>;
  }

  if (endpoint.status === "failed") {
    return <Badge variant="destructive">{tCrm("crm.modules.workspace.failed", locale)}</Badge>;
  }

  return <Badge variant="outline">{tCrm("crm.modules.workspace.current", locale)}</Badge>;
}

function PreviewTable({ preview }: Readonly<{ preview: CrmOperationalPreview }>) {
  const rows: CrmRecordsTableRow[] = preview.rows.map((row, index) => {
    const cells = Object.fromEntries(
      preview.columns.map((column) => [column, formatPreviewValue(column, row[column])]),
    );

    return {
      id: `preview-${index}`,
      cells,
      searchText: Object.values(cells).join(" "),
    };
  });

  return (
    <CrmRecordsTable
      caption="Records"
      columns={preview.columns.map((column) => ({
        key: column,
        header: formatColumnLabel(column),
        badge: /status|state|active|priority|enabled|default/i.test(column),
      }))}
      rows={rows}
      labels={{
        searchPlaceholder: "Search",
        emptyTitle: "No matching records",
        emptyDescription: "Try changing filters or clearing the current view.",
      }}
      minWidthClassName="min-w-[760px]"
    />
  );
}

function EmptyBusinessState({
  locale,
  compact = false,
}: Readonly<{ locale?: string | null | undefined; compact?: boolean }>) {
  return (
    <CrmEmptyState
      compact={compact}
      title={tCrm("crm.modules.workspace.emptyStateTitle", locale)}
      description={tCrm("crm.modules.workspace.emptyStateDescription", locale)}
      icon={<Database aria-hidden="true" />}
    />
  );
}

function EndpointEmptyState({
  endpoint,
  locale,
}: Readonly<{ endpoint: CrmOperationalEndpointResult; locale?: string | null | undefined }>) {
  if (endpoint.status === "skipped" && endpoint.requiresSelection) {
    return (
      <CrmEmptyState
        title={tCrm("crm.modules.workspace.selectedRecordRequired", locale)}
        description={endpoint.requiresSelection}
        icon={<LockKeyhole aria-hidden="true" />}
      />
    );
  }

  return <EmptyBusinessState locale={locale} />;
}

function getPrimarySection(
  endpoints: CrmOperationalEndpointResult[],
): CrmOperationalEndpointResult | undefined {
  return (
    endpoints.find((endpoint) => endpoint.kind === "workspace" && endpoint.payload) ??
    endpoints.find((endpoint) => endpoint.kind === "analytics" && endpoint.payload) ??
    endpoints.find((endpoint) => endpoint.payload)
  );
}

function createInsightItems(endpoint: CrmOperationalEndpointResult | undefined): InsightItem[] {
  const preview = endpoint?.payload?.preview;
  if (!preview || preview.rows.length === 0) {
    return [];
  }

  if (preview.columns.includes("field") && preview.columns.includes("value")) {
    return preview.rows
      .filter((row) => row.field && row.value)
      .slice(0, 6)
      .map((row) => ({
        label: formatColumnLabel(row.field ?? ""),
        value: formatPreviewValue("value", row.value),
      }));
  }

  const firstRow = preview.rows[0];
  if (!firstRow) {
    return [];
  }

  return preview.columns.slice(0, 6).map((column) => ({
    label: formatColumnLabel(column),
    value: formatPreviewValue(column, firstRow[column]),
  }));
}

function isReadableBusinessSection(endpoint: CrmOperationalEndpointResult): boolean {
  return endpoint.method === "GET" && endpoint.fetch !== false;
}

function formatPayloadSummary(
  endpoint: CrmOperationalEndpointResult,
  locale?: string | null | undefined,
): string {
  if (!endpoint.payload || endpoint.payload.count === 0) {
    return tCrm("crm.modules.workspace.emptyStateTitle", locale);
  }

  if (endpoint.payload.count === null) {
    return endpoint.payload.summary;
  }

  return `${endpoint.payload.count} ${tCrm("crm.modules.workspace.recordsLabel", locale)}`;
}

function formatOperationDescription(
  endpoint: CrmOperationalEndpointResult | undefined,
  locale?: string | null | undefined,
): string {
  if (!endpoint) {
    return tCrm("crm.modules.workspace.primaryViewDescription", locale);
  }

  return `Review ${endpoint.label.toLocaleLowerCase()} in a focused workspace, then move to the next customer action without cluttering the module home.`;
}

function formatColumnLabel(value: string): string {
  const spaced = value
    .replace(/([a-z0-9])([A-Z])/g, "$1 $2")
    .replace(/[_-]+/g, " ")
    .trim();

  return spaced.length === 0
    ? "-"
    : `${spaced.charAt(0).toUpperCase()}${spaced.slice(1).toLowerCase()}`;
}

function formatPreviewValue(column: string, value: string | undefined): string {
  if (!value) {
    return "-";
  }

  if (column.toLowerCase() === "field") {
    return formatColumnLabel(value);
  }

  if (value.toLowerCase() === "true") {
    return "Yes";
  }

  if (value.toLowerCase() === "false") {
    return "No";
  }

  const dateTimePattern = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+\-]\d{2}:\d{2})$/;
  const dateOnlyPattern = /^\d{4}-\d{2}-\d{2}$/;
  if (dateTimePattern.test(value)) {
    const parsed = new Date(value);
    if (!Number.isNaN(parsed.getTime())) {
      return new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "short",
      }).format(parsed);
    }
  }
  if (dateOnlyPattern.test(value)) {
    const parsed = new Date(`${value}T00:00:00`);
    if (!Number.isNaN(parsed.getTime())) {
      return new Intl.DateTimeFormat(undefined, { dateStyle: "medium" }).format(parsed);
    }
  }

  return value;
}
