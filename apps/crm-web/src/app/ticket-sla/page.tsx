import Link from "next/link";
import {
  CrmMetricGrid,
  CrmSectionCard,
  CrmToolbarSurface,
} from "@/components/shell/crm-content-primitives";
import { CrmEmptyState } from "@/components/shell/crm-empty-state";
import { CrmPageShell } from "@/components/shell/crm-page-shell";
import { CrmRecordsTable, type CrmRecordsTableRow } from "@/components/shell/crm-records-table";
import { isGuid } from "@/features/shared/data/guid";
import { TicketSlaReadFiltersForm } from "@/features/ticket-sla/components/ticket-sla-read-filters-form";
import { getTicketSlaData } from "@/features/ticket-sla/data/ticket-sla-data";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import { formatCrmDateTime } from "@/lib/date-time/crm-date-time";
import { tCrm, tCrmWithFallback } from "@/lib/i18n/crm-i18n";
import { getRequestDateSettings } from "@/lib/i18n/request-date-settings";
import type {
  TicketEscalationRunDto,
  TicketSlaEscalationRuleDto,
  TicketSlaPolicyDto,
  TicketSlaWorkspaceDto,
} from "@/lib/crm-api";
import { Badge, Button, Text } from "@netmetric/ui";
import { AlarmClock, ClipboardList, ListChecks, TimerReset } from "lucide-react";

function toSingleValue(value: string | string[] | undefined): string | undefined {
  if (typeof value === "string") return value;
  if (Array.isArray(value)) return value[0];
  return undefined;
}

function enumLabel(namespace: string, value: string, locale: string): string {
  return tCrmWithFallback(`${namespace}.${value}`, value, locale);
}

export default async function TicketSlaPage({
  searchParams,
}: {
  searchParams: Promise<Record<string, string | string[] | undefined>>;
}) {
  await requireCrmSession("/ticket-sla");
  const dateSettings = await getRequestDateSettings();
  const locale = dateSettings.locale;
  const params = await searchParams;
  const policyIdRaw = toSingleValue(params.policyId);
  const ticketIdRaw = toSingleValue(params.ticketId);
  const policyId = policyIdRaw && isGuid(policyIdRaw) ? policyIdRaw : undefined;
  const ticketId = ticketIdRaw && isGuid(ticketIdRaw) ? ticketIdRaw : undefined;

  const { policies, escalationRules, workspace, escalationRuns } = await getTicketSlaData(
    {
      ...(policyId ? { policyId } : {}),
      ...(ticketId ? { ticketId } : {}),
    },
    "/ticket-sla",
  );
  const selectedPolicy = policyId ? policies.find((policy) => policy.id === policyId) : undefined;
  const enabledRuleCount = escalationRules?.filter((rule) => rule.isEnabled).length ?? 0;
  const breachCount = workspace
    ? Number(workspace.isFirstResponseBreached) + Number(workspace.isResolutionBreached)
    : 0;

  return (
    <CrmPageShell
      routePath="/ticket-sla"
      locale={locale}
      actions={
        <div className="flex flex-wrap gap-2">
          <Button asChild variant="outline">
            <Link href="/ticket-sla/policies">
              <ClipboardList aria-hidden="true" />
              {tCrm("crm.ticketSla.policies.title", locale)}
            </Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/ticket-sla/ticket-workspace">
              <AlarmClock aria-hidden="true" />
              {tCrm("crm.ticketSla.workspace.title", locale)}
            </Link>
          </Button>
        </div>
      }
    >
      <CrmMetricGrid
        columns="four"
        items={[
          {
            label: tCrm("crm.ticketSla.policies.title", locale),
            value: policies.length,
            description: tCrm("crm.ticketSla.policies.description", locale),
            icon: <ClipboardList aria-hidden="true" className="size-4" />,
          },
          {
            label: tCrm("crm.ticketSla.rules.title", locale),
            value: escalationRules?.length ?? 0,
            description:
              selectedPolicy?.name ?? tCrm("crm.ticketSla.rules.selectPolicyTitle", locale),
            icon: <ListChecks aria-hidden="true" className="size-4" />,
            tone: policyId ? "info" : "neutral",
          },
          {
            label: tCrm("crm.ticketSla.workspace.title", locale),
            value: ticketId
              ? workspace
                ? tCrm("crm.modules.workspace.current", locale)
                : tCrm("crm.modules.workspace.noDataYet", locale)
              : tCrm("crm.ticketSla.workspace.selectTicketTitle", locale),
            description: tCrm("crm.ticketSla.workspace.description", locale),
            icon: <AlarmClock aria-hidden="true" className="size-4" />,
            tone: breachCount > 0 ? "danger" : ticketId ? "success" : "neutral",
          },
          {
            label: tCrm("crm.ticketSla.runs.title", locale),
            value: escalationRuns?.length ?? 0,
            description: tCrm("crm.ticketSla.runs.description", locale),
            icon: <TimerReset aria-hidden="true" className="size-4" />,
          },
        ]}
      />

      <CrmToolbarSurface
        title={tCrm("crm.ticketSla.readFilters.title", locale)}
        description={tCrm("crm.ticketSla.readFilters.description", locale)}
      >
        <TicketSlaReadFiltersForm
          policies={policies}
          policyId={policyId}
          ticketIdValue={ticketId ?? ticketIdRaw ?? ""}
        />
        {ticketIdRaw && !ticketId ? (
          <p className="mt-3 text-sm text-muted-foreground">
            {tCrm("crm.ticketSla.validation.invalidTicketId", locale)}
          </p>
        ) : null}
      </CrmToolbarSurface>

      <div className="grid gap-4 xl:grid-cols-[minmax(0,1.05fr)_minmax(22rem,0.95fr)]">
        <CrmSectionCard
          title={tCrm("crm.ticketSla.policies.title", locale)}
          description={tCrm("crm.ticketSla.policies.description", locale)}
          actions={<Badge variant="secondary">{policies.length}</Badge>}
          contentClassName="p-0"
        >
          <PoliciesTable policies={policies} locale={locale} />
        </CrmSectionCard>

        <CrmSectionCard
          title={tCrm("crm.ticketSla.rules.title", locale)}
          description={tCrm("crm.ticketSla.rules.description", locale)}
          actions={
            policyId ? (
              <Badge variant="outline">
                {enabledRuleCount} {tCrm("crm.ticketSla.fields.enabled", locale)}
              </Badge>
            ) : null
          }
          contentClassName={
            !policyId || !escalationRules || escalationRules.length === 0 ? undefined : "p-0"
          }
        >
          <RulesContent
            escalationRules={escalationRules}
            hasPolicy={Boolean(policyId)}
            locale={locale}
          />
        </CrmSectionCard>
      </div>

      <div className="grid gap-4 xl:grid-cols-[minmax(0,0.95fr)_minmax(22rem,1.05fr)]">
        <CrmSectionCard
          title={tCrm("crm.ticketSla.workspace.title", locale)}
          description={tCrm("crm.ticketSla.workspace.description", locale)}
          actions={
            workspace ? (
              <Badge variant={breachCount > 0 ? "destructive" : "secondary"}>
                {breachCount > 0
                  ? tCrm("crm.modules.workspace.needsReview", locale)
                  : tCrm("crm.modules.workspace.current", locale)}
              </Badge>
            ) : null
          }
        >
          <WorkspaceContent
            dateSettings={dateSettings}
            hasTicket={Boolean(ticketId)}
            locale={locale}
            workspace={workspace}
          />
        </CrmSectionCard>

        <CrmSectionCard
          title={tCrm("crm.ticketSla.runs.title", locale)}
          description={tCrm("crm.ticketSla.runs.description", locale)}
          actions={<Badge variant="outline">{escalationRuns?.length ?? 0}</Badge>}
          contentClassName={
            !ticketId || !escalationRuns || escalationRuns.length === 0 ? undefined : "p-0"
          }
        >
          <RunsContent
            dateSettings={dateSettings}
            escalationRuns={escalationRuns}
            hasTicket={Boolean(ticketId)}
            locale={locale}
          />
        </CrmSectionCard>
      </div>
    </CrmPageShell>
  );
}

function PoliciesTable({
  policies,
  locale,
}: Readonly<{ policies: TicketSlaPolicyDto[]; locale: string }>) {
  const rows: CrmRecordsTableRow[] = policies.map((policy) => {
    const priority = enumLabel("crm.common.priority", String(policy.priority), locale);
    const defaultState = policy.isDefault
      ? tCrm("crm.ticketSla.labels.default", locale)
      : tCrm("crm.common.boolean.false", locale);

    return {
      id: policy.id,
      cells: {
        name: policy.name,
        priority,
        firstResponseTargetMinutes: `${policy.firstResponseTargetMinutes} min`,
        resolutionTargetMinutes: `${policy.resolutionTargetMinutes} min`,
        default: defaultState,
      },
      descriptions: {
        name: policy.ticketCategoryId ?? undefined,
      },
      searchText: [
        policy.name,
        policy.ticketCategoryId,
        priority,
        policy.firstResponseTargetMinutes,
        policy.resolutionTargetMinutes,
        defaultState,
      ]
        .filter(Boolean)
        .join(" "),
      filterValues: {
        priority,
        default: defaultState,
      },
    };
  });

  return (
    <CrmRecordsTable
      caption={tCrm("crm.ticketSla.policies.caption", locale)}
      columns={[
        { key: "name", header: tCrm("crm.ticketSla.fields.name", locale) },
        { key: "priority", header: tCrm("crm.ticketSla.fields.priority", locale), badge: true },
        {
          key: "firstResponseTargetMinutes",
          header: tCrm("crm.ticketSla.fields.firstResponseTargetMinutes", locale),
        },
        {
          key: "resolutionTargetMinutes",
          header: tCrm("crm.ticketSla.fields.resolutionTargetMinutes", locale),
        },
        { key: "default", header: tCrm("crm.ticketSla.fields.default", locale), badge: true },
      ]}
      rows={rows}
      filters={[
        {
          key: "priority",
          label: tCrm("crm.ticketSla.fields.priority", locale),
          allLabel: `All ${tCrm("crm.ticketSla.fields.priority", locale).toLocaleLowerCase()}`,
          options: [...new Set(rows.map((row) => row.cells.priority))]
            .filter((value): value is string => Boolean(value))
            .map((value) => ({
              value,
              label: value,
            })),
        },
        {
          key: "default",
          label: tCrm("crm.ticketSla.fields.default", locale),
          allLabel: `All ${tCrm("crm.ticketSla.fields.default", locale).toLocaleLowerCase()}`,
          options: [...new Set(rows.map((row) => row.cells.default))]
            .filter((value): value is string => Boolean(value))
            .map((value) => ({
              value,
              label: value,
            })),
        },
      ]}
      labels={{
        searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
        emptyTitle: tCrm("crm.ticketSla.policies.emptyTitle", locale),
        emptyDescription: tCrm("crm.ticketSla.policies.emptyDescription", locale),
      }}
      minWidthClassName="min-w-[760px]"
    />
  );
}

function RulesContent({
  escalationRules,
  hasPolicy,
  locale,
}: Readonly<{
  escalationRules: TicketSlaEscalationRuleDto[] | null;
  hasPolicy: boolean;
  locale: string;
}>) {
  if (!hasPolicy) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketSla.rules.selectPolicyTitle", locale)}
        description={tCrm("crm.ticketSla.rules.selectPolicyDescription", locale)}
        icon={<ListChecks aria-hidden="true" />}
      />
    );
  }

  if (!escalationRules || escalationRules.length === 0) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketSla.rules.emptyTitle", locale)}
        description={tCrm("crm.ticketSla.rules.emptyDescription", locale)}
        icon={<ListChecks aria-hidden="true" />}
      />
    );
  }

  const rows: CrmRecordsTableRow[] = escalationRules.map((rule) => {
    const metric = enumLabel("crm.ticketSla.metric", rule.metricType, locale);
    const action = enumLabel("crm.ticketSla.action", rule.actionType, locale);
    const enabled = rule.isEnabled
      ? tCrm("crm.common.boolean.true", locale)
      : tCrm("crm.common.boolean.false", locale);

    return {
      id: rule.id,
      cells: {
        metric,
        action,
        triggerMinutes: `${rule.triggerBeforeOrAfterMinutes} min`,
        enabled,
      },
      descriptions: {
        action: rule.targetTeamId ?? rule.targetUserId ?? undefined,
      },
      searchText: [
        metric,
        action,
        rule.targetTeamId,
        rule.targetUserId,
        rule.triggerBeforeOrAfterMinutes,
        enabled,
      ]
        .filter(Boolean)
        .join(" "),
      filterValues: {
        metric,
        enabled,
      },
    };
  });

  return (
    <CrmRecordsTable
      caption={tCrm("crm.ticketSla.rules.caption", locale)}
      columns={[
        { key: "metric", header: tCrm("crm.ticketSla.fields.metric", locale), badge: true },
        { key: "action", header: tCrm("crm.ticketSla.fields.action", locale) },
        { key: "triggerMinutes", header: tCrm("crm.ticketSla.fields.triggerMinutes", locale) },
        { key: "enabled", header: tCrm("crm.ticketSla.fields.enabled", locale), badge: true },
      ]}
      rows={rows}
      filters={[
        {
          key: "metric",
          label: tCrm("crm.ticketSla.fields.metric", locale),
          allLabel: `All ${tCrm("crm.ticketSla.fields.metric", locale).toLocaleLowerCase()}`,
          options: [...new Set(rows.map((row) => row.cells.metric))]
            .filter((value): value is string => Boolean(value))
            .map((value) => ({
              value,
              label: value,
            })),
        },
        {
          key: "enabled",
          label: tCrm("crm.ticketSla.fields.enabled", locale),
          allLabel: `All ${tCrm("crm.ticketSla.fields.enabled", locale).toLocaleLowerCase()}`,
          options: [...new Set(rows.map((row) => row.cells.enabled))]
            .filter((value): value is string => Boolean(value))
            .map((value) => ({
              value,
              label: value,
            })),
        },
      ]}
      labels={{
        searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
        emptyTitle: tCrm("crm.ticketSla.rules.emptyTitle", locale),
        emptyDescription: tCrm("crm.ticketSla.rules.emptyDescription", locale),
      }}
      minWidthClassName="min-w-[680px]"
    />
  );
}

function WorkspaceContent({
  dateSettings,
  hasTicket,
  locale,
  workspace,
}: Readonly<{
  dateSettings: Awaited<ReturnType<typeof getRequestDateSettings>>;
  hasTicket: boolean;
  locale: string;
  workspace: TicketSlaWorkspaceDto | null;
}>) {
  if (!hasTicket) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketSla.workspace.selectTicketTitle", locale)}
        description={tCrm("crm.ticketSla.workspace.selectTicketDescription", locale)}
        icon={<AlarmClock aria-hidden="true" />}
      />
    );
  }

  if (!workspace) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketSla.workspace.emptyTitle", locale)}
        description={tCrm("crm.ticketSla.workspace.emptyDescription", locale)}
        icon={<AlarmClock aria-hidden="true" />}
      />
    );
  }

  return (
    <div className="grid gap-3 md:grid-cols-2">
      <SlaCheckpoint
        breached={workspace.isFirstResponseBreached}
        dueAt={formatCrmDateTime(workspace.firstResponseDueAtUtc, dateSettings)}
        label={tCrm("crm.ticketSla.fields.firstResponseDue", locale)}
        completedAt={
          workspace.firstRespondedAtUtc
            ? formatCrmDateTime(workspace.firstRespondedAtUtc, dateSettings)
            : "-"
        }
        locale={locale}
      />
      <SlaCheckpoint
        breached={workspace.isResolutionBreached}
        dueAt={formatCrmDateTime(workspace.resolutionDueAtUtc, dateSettings)}
        label={tCrm("crm.ticketSla.fields.resolutionDue", locale)}
        completedAt={
          workspace.resolvedAtUtc ? formatCrmDateTime(workspace.resolvedAtUtc, dateSettings) : "-"
        }
        locale={locale}
      />
      <div className="rounded-lg border bg-muted/15 p-4 md:col-span-2">
        <Text className="text-xs font-medium uppercase text-muted-foreground">
          {tCrm("crm.ticketSla.fields.policyId", locale)}
        </Text>
        <Text className="mt-1 break-all text-sm font-medium">{workspace.slaPolicyId}</Text>
      </div>
    </div>
  );
}

function SlaCheckpoint({
  breached,
  completedAt,
  dueAt,
  label,
  locale,
}: Readonly<{
  breached: boolean;
  completedAt: string;
  dueAt: string;
  label: string;
  locale: string;
}>) {
  return (
    <div className="rounded-lg border bg-muted/15 p-4">
      <div className="flex items-center justify-between gap-3">
        <Text className="text-sm font-medium">{label}</Text>
        <Badge variant={breached ? "destructive" : "secondary"}>
          {breached
            ? tCrm("crm.modules.workspace.needsReview", locale)
            : tCrm("crm.modules.workspace.current", locale)}
        </Badge>
      </div>
      <div className="mt-4 grid gap-3 text-sm">
        <div>
          <Text className="text-xs uppercase text-muted-foreground">
            {tCrm("crm.ticketSla.fields.dueAt", locale)}
          </Text>
          <Text className="mt-1">{dueAt}</Text>
        </div>
        <div>
          <Text className="text-xs uppercase text-muted-foreground">
            {tCrm("crm.ticketSla.fields.completedAt", locale)}
          </Text>
          <Text className="mt-1">{completedAt}</Text>
        </div>
      </div>
    </div>
  );
}

function RunsContent({
  dateSettings,
  escalationRuns,
  hasTicket,
  locale,
}: Readonly<{
  dateSettings: Awaited<ReturnType<typeof getRequestDateSettings>>;
  escalationRuns: TicketEscalationRunDto[] | null;
  hasTicket: boolean;
  locale: string;
}>) {
  if (!hasTicket) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketSla.runs.selectTicketTitle", locale)}
        description={tCrm("crm.ticketSla.runs.selectTicketDescription", locale)}
        icon={<TimerReset aria-hidden="true" />}
      />
    );
  }

  if (!escalationRuns || escalationRuns.length === 0) {
    return (
      <CrmEmptyState
        title={tCrm("crm.ticketSla.runs.emptyTitle", locale)}
        description={tCrm("crm.ticketSla.runs.emptyDescription", locale)}
        icon={<TimerReset aria-hidden="true" />}
      />
    );
  }

  const rows: CrmRecordsTableRow[] = escalationRuns.map((run) => {
    const metric = enumLabel("crm.ticketSla.metric", run.metricType, locale);
    const executedAt = formatCrmDateTime(run.executedAtUtc, dateSettings);

    return {
      id: run.id,
      cells: {
        executedAt,
        metric,
        note: run.note,
      },
      searchText: [executedAt, metric, run.note].filter(Boolean).join(" "),
      filterValues: {
        metric,
      },
    };
  });

  return (
    <CrmRecordsTable
      caption={tCrm("crm.ticketSla.runs.caption", locale)}
      columns={[
        { key: "executedAt", header: tCrm("crm.ticketSla.fields.executedAt", locale) },
        { key: "metric", header: tCrm("crm.ticketSla.fields.metric", locale), badge: true },
        { key: "note", header: tCrm("crm.ticketSla.fields.note", locale) },
      ]}
      rows={rows}
      filters={[
        {
          key: "metric",
          label: tCrm("crm.ticketSla.fields.metric", locale),
          allLabel: `All ${tCrm("crm.ticketSla.fields.metric", locale).toLocaleLowerCase()}`,
          options: [...new Set(rows.map((row) => row.cells.metric))]
            .filter((value): value is string => Boolean(value))
            .map((value) => ({
              value,
              label: value,
            })),
        },
      ]}
      labels={{
        searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
        emptyTitle: tCrm("crm.ticketSla.runs.emptyTitle", locale),
        emptyDescription: tCrm("crm.ticketSla.runs.emptyDescription", locale),
      }}
      minWidthClassName="min-w-[720px]"
    />
  );
}
