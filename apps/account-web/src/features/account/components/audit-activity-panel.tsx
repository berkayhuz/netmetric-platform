"use client";

import { useMemo, useState } from "react";
import type React from "react";
import {
  Badge,
  Button,
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
  Input,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@netmetric/ui";
import { ArrowDownUp, CalendarClock, Fingerprint, Search, ShieldAlert, X } from "lucide-react";

import type { AccountAuditEntriesResponse } from "@/lib/account-api";
import type { AccountAuditEntryResponse } from "@/lib/account-api/account-api-types";
import type { AccountDateSettings } from "@/lib/account-date";
import { formatAccountDateTime } from "@/lib/account-date";
import { tAccountClient } from "@/lib/i18n/account-i18n";

import { AccountPagePanel } from "./account-page-panel";

type AuditActivityPanelProps = {
  audit: AccountAuditEntriesResponse;
  activeEventType: string | undefined;
  activeLimit: number;
  dateSettings: AccountDateSettings;
};

type AuditSortKey = "eventType" | "severity" | "occurredAt" | "correlationId";
type SortDirection = "asc" | "desc";

const defaultFilters = {
  query: "",
  eventType: "all",
  severity: "all",
  occurred: "",
};

export function AuditActivityPanel({
  audit,
  activeEventType,
  activeLimit,
  dateSettings,
}: AuditActivityPanelProps) {
  const [filters, setFilters] = useState({
    ...defaultFilters,
    eventType: activeEventType?.trim() || "all",
  });
  const [sort, setSort] = useState<{ key: AuditSortKey; direction: SortDirection }>({
    key: "occurredAt",
    direction: "desc",
  });

  const eventTypes = useMemo(
    () => Array.from(new Set(audit.items.map((item) => item.eventType).filter(Boolean))).sort(),
    [audit.items],
  );
  const severities = useMemo(
    () => Array.from(new Set(audit.items.map((item) => item.severity).filter(Boolean))).sort(),
    [audit.items],
  );

  const filteredItems = useMemo(() => {
    const query = filters.query.trim().toLowerCase();
    const occurredQuery = filters.occurred.trim().toLowerCase();

    return audit.items
      .filter((item) => {
        const eventMatches = filters.eventType === "all" || item.eventType === filters.eventType;
        const severityMatches = filters.severity === "all" || item.severity === filters.severity;
        const occurredText = formatAccountDateTime(item.occurredAt, dateSettings).toLowerCase();
        const occurredMatches = !occurredQuery || occurredText.includes(occurredQuery);
        const queryMatches =
          !query ||
          [item.eventType, item.severity, item.correlationId, item.userId, item.tenantId]
            .filter(Boolean)
            .some((value) => String(value).toLowerCase().includes(query));

        return eventMatches && severityMatches && occurredMatches && queryMatches;
      })
      .sort((left, right) => compareAuditEntry(left, right, sort.key, sort.direction));
  }, [audit.items, dateSettings, filters, sort]);

  function toggleSort(key: AuditSortKey) {
    setSort((current) =>
      current.key === key
        ? { key, direction: current.direction === "asc" ? "desc" : "asc" }
        : { key, direction: "asc" },
    );
  }

  return (
    <AccountPagePanel
      title={tAccountClient("account.audit.title")}
      description={tAccountClient("account.audit.description")}
      contentClassName="space-y-0"
      bodyPadding="none"
    >
      <section className="overflow-hidden">
        <div className="grid gap-2 border-b border-border/70 bg-background/40 p-3 md:grid-cols-[minmax(220px,1fr)_minmax(150px,220px)_minmax(150px,220px)_minmax(180px,240px)_auto]">
          <div className="relative">
            <Search
              aria-hidden="true"
              className="pointer-events-none absolute left-2 top-1/2 size-3.5 -translate-y-1/2 text-muted-foreground"
            />
            <Input
              value={filters.query}
              placeholder="Filter audit events"
              aria-label="Filter audit events"
              className="pl-7"
              onChange={(event) =>
                setFilters((current) => ({ ...current, query: event.target.value }))
              }
            />
          </div>
          <select
            value={filters.eventType}
            aria-label={tAccountClient("account.audit.eventTypeLabel")}
            className="h-8 rounded-sm border border-input bg-background px-2 text-sm text-foreground"
            onChange={(event) =>
              setFilters((current) => ({ ...current, eventType: event.target.value }))
            }
          >
            <option value="all">{tAccountClient("account.common.all")}</option>
            {eventTypes.map((eventType) => (
              <option key={eventType} value={eventType}>
                {eventType}
              </option>
            ))}
          </select>
          <select
            value={filters.severity}
            aria-label="Filter audit severity"
            className="h-8 rounded-sm border border-input bg-background px-2 text-sm text-foreground"
            onChange={(event) =>
              setFilters((current) => ({ ...current, severity: event.target.value }))
            }
          >
            <option value="all">All severities</option>
            {severities.map((severity) => (
              <option key={severity} value={severity}>
                {severity}
              </option>
            ))}
          </select>
          <Input
            value={filters.occurred}
            placeholder={tAccountClient("account.audit.occurredLabel")}
            aria-label={tAccountClient("account.audit.occurredLabel")}
            onChange={(event) =>
              setFilters((current) => ({ ...current, occurred: event.target.value }))
            }
          />
          <Button
            type="button"
            size="xs"
            variant="ghost"
            className="gap-1.5"
            onClick={() => setFilters(defaultFilters)}
          >
            <X aria-hidden="true" className="size-3.5" />
            Clear
          </Button>
        </div>

        <Table>
          <TableHeader className="bg-muted/10">
            <TableRow className="hover:bg-transparent">
              <SortableHead
                sortKey="eventType"
                activeKey={sort.key}
                direction={sort.direction}
                onSort={toggleSort}
                className="min-w-[220px]"
              >
                <Fingerprint aria-hidden="true" className="size-4" />
                {tAccountClient("account.audit.eventTypeLabel")}
              </SortableHead>
              <SortableHead
                sortKey="severity"
                activeKey={sort.key}
                direction={sort.direction}
                onSort={toggleSort}
                className="border-l border-border/50"
              >
                <ShieldAlert aria-hidden="true" className="size-4" />
                Severity
              </SortableHead>
              <SortableHead
                sortKey="occurredAt"
                activeKey={sort.key}
                direction={sort.direction}
                onSort={toggleSort}
                className="min-w-[200px] border-l border-border/50"
              >
                <CalendarClock aria-hidden="true" className="size-4" />
                {tAccountClient("account.audit.occurredLabel")}
              </SortableHead>
              <SortableHead
                sortKey="correlationId"
                activeKey={sort.key}
                direction={sort.direction}
                onSort={toggleSort}
                className="min-w-[240px] border-l border-border/50"
              >
                Correlation
              </SortableHead>
              <TableHead className="min-w-[220px] border-l border-border/50 text-muted-foreground">
                User
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredItems.length === 0 ? (
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={5}>
                  <Empty className="border-none py-10" role="status" aria-live="polite">
                    <EmptyHeader>
                      <EmptyTitle>{tAccountClient("account.audit.emptyTitle")}</EmptyTitle>
                      <EmptyDescription>
                        {tAccountClient("account.audit.emptyDescription")}
                      </EmptyDescription>
                    </EmptyHeader>
                  </Empty>
                </TableCell>
              </TableRow>
            ) : (
              filteredItems.map((item) => (
                <TableRow key={item.id} className="h-10 hover:bg-muted/30">
                  <TableCell className="font-medium">{item.eventType}</TableCell>
                  <TableCell className="border-l border-border/50">
                    <Badge variant="outline" className="border-transparent bg-muted px-1.5">
                      {item.severity}
                    </Badge>
                  </TableCell>
                  <TableCell className="border-l border-border/50 font-medium">
                    {formatAccountDateTime(item.occurredAt, dateSettings)}
                  </TableCell>
                  <TableCell className="border-l border-border/50 font-mono text-xs">
                    {item.correlationId?.trim()
                      ? item.correlationId
                      : tAccountClient("account.common.notAvailable")}
                  </TableCell>
                  <TableCell className="border-l border-border/50 font-mono text-xs">
                    {item.userId}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>

        <div className="grid border-t border-border/70 bg-muted/10 px-4 py-3 text-sm text-muted-foreground sm:grid-cols-[180px_1fr_1fr_1fr]">
          <span>Calculate</span>
          <span>
            {tAccountClient("account.audit.returnedEntriesLabel")}{" "}
            <strong className="font-semibold text-foreground">{audit.count}</strong>
          </span>
          <span>
            Shown <strong className="font-semibold text-foreground">{filteredItems.length}</strong>
          </span>
          <span>
            Limit <strong className="font-semibold text-foreground">{activeLimit}</strong>
          </span>
        </div>
      </section>
    </AccountPagePanel>
  );
}

function SortableHead({
  sortKey,
  activeKey,
  direction,
  onSort,
  className,
  children,
}: {
  sortKey: AuditSortKey;
  activeKey: AuditSortKey;
  direction: SortDirection;
  onSort: (key: AuditSortKey) => void;
  className?: string;
  children: React.ReactNode;
}) {
  return (
    <TableHead className={className}>
      <button
        type="button"
        className="inline-flex items-center gap-1.5 text-muted-foreground transition-colors hover:text-foreground"
        onClick={() => onSort(sortKey)}
      >
        {children}
        <ArrowDownUp
          aria-hidden="true"
          className={`size-3.5 ${activeKey === sortKey ? "text-foreground" : ""}`}
        />
        <span className="sr-only">{activeKey === sortKey ? direction : "unsorted"}</span>
      </button>
    </TableHead>
  );
}

function compareAuditEntry(
  left: AccountAuditEntryResponse,
  right: AccountAuditEntryResponse,
  key: AuditSortKey,
  direction: SortDirection,
): number {
  const multiplier = direction === "asc" ? 1 : -1;
  const leftValue = key === "occurredAt" ? Date.parse(left.occurredAt) : String(left[key] ?? "");
  const rightValue = key === "occurredAt" ? Date.parse(right.occurredAt) : String(right[key] ?? "");

  if (typeof leftValue === "number" && typeof rightValue === "number") {
    return (leftValue - rightValue) * multiplier;
  }

  return String(leftValue).localeCompare(String(rightValue)) * multiplier;
}
