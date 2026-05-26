"use client";

import type React from "react";
import { useActionState, useEffect, useMemo, useState } from "react";
import { useFormStatus } from "react-dom";
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
  Text,
  cn,
} from "@netmetric/ui";
import {
  Checkbox,
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
  toast,
} from "@netmetric/ui/client";
import {
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  Bell,
  CalendarClock,
  CheckCircle2,
  CircleDot,
  Filter,
  MoreHorizontal,
  Shield,
  Tag,
  Trash2,
  X,
} from "lucide-react";

import type { AccountNotificationsResponse } from "@/lib/account-api";
import type { AccountNotificationResponse } from "@/lib/account-api/account-api-types";
import type { AccountDateSettings } from "@/lib/account-date";
import { formatAccountDateTime } from "@/lib/account-date";
import { tAccountClient } from "@/lib/i18n/account-i18n";

import {
  deleteNotificationAction,
  markNotificationAsReadAction,
} from "../actions/notification-actions";
import { initialMutationState, type MutationState } from "../actions/mutation-state";

type NotificationListProps = {
  notifications: AccountNotificationsResponse;
  dateSettings: AccountDateSettings;
  options?: React.ReactNode;
};

type SortKey = "title" | "category" | "severity" | "occurredAt" | "status";
type SortDirection = "asc" | "desc";

type SortState = {
  key: SortKey;
  direction: SortDirection;
};

type FilterState = {
  title: string;
  category: string;
  severity: string;
  occurred: string;
  status: "all" | "read" | "unread";
};

const defaultFilters: FilterState = {
  title: "",
  category: "all",
  severity: "all",
  occurred: "",
  status: "all",
};

function normalizeFilterValue(value: string): string {
  return value.trim().toLocaleLowerCase();
}

function uniqueSorted(values: string[]): string[] {
  return Array.from(new Set(values.filter(Boolean))).sort((a, b) => a.localeCompare(b));
}

function rowAccent(item: AccountNotificationResponse): string {
  const source = `${item.category}-${item.severity}-${item.title}`.toLowerCase();
  if (source.includes("security") || source.includes("critical") || source.includes("high")) {
    return "bg-rose-500/15 text-rose-600 dark:text-rose-300";
  }
  if (source.includes("billing") || source.includes("payment")) {
    return "bg-amber-500/15 text-amber-700 dark:text-amber-300";
  }
  if (source.includes("profile") || source.includes("account")) {
    return "bg-sky-500/15 text-sky-700 dark:text-sky-300";
  }
  if (source.includes("system") || source.includes("info")) {
    return "bg-emerald-500/15 text-emerald-700 dark:text-emerald-300";
  }
  return "bg-violet-500/15 text-violet-700 dark:text-violet-300";
}

function statusLabel(item: AccountNotificationResponse): string {
  return item.isRead
    ? tAccountClient("account.notifications.readLabel")
    : tAccountClient("account.notifications.unreadLabel");
}

function compareStrings(left: string, right: string, direction: SortDirection): number {
  const result = left.localeCompare(right, undefined, { sensitivity: "base" });
  return direction === "asc" ? result : -result;
}

function compareNotifications(
  left: AccountNotificationResponse,
  right: AccountNotificationResponse,
  sort: SortState,
): number {
  if (sort.key === "occurredAt") {
    const leftTime = new Date(left.occurredAt).getTime();
    const rightTime = new Date(right.occurredAt).getTime();
    const result = leftTime - rightTime;
    return sort.direction === "asc" ? result : -result;
  }

  if (sort.key === "status") {
    return compareStrings(statusLabel(left), statusLabel(right), sort.direction);
  }

  return compareStrings(left[sort.key], right[sort.key], sort.direction);
}

function useMutationToast(
  state: MutationState,
  titles: {
    success: string;
    error: string;
  },
): void {
  useEffect(() => {
    if (!state.message || state.status === "idle") {
      return;
    }

    if (state.status === "success") {
      toast.success(titles.success, { description: state.message });
      return;
    }

    toast.error(titles.error, { description: state.message });
  }, [state, titles.error, titles.success]);
}

function RowActionButton({
  idleLabel,
  pendingLabel,
  variant = "ghost",
  icon,
}: {
  idleLabel: string;
  pendingLabel: string;
  variant?: "ghost" | "destructive";
  icon?: React.ReactNode;
}) {
  const { pending } = useFormStatus();
  return (
    <Button
      type="submit"
      size="xs"
      variant={variant}
      disabled={pending}
      className="w-full justify-start gap-1.5"
    >
      {icon}
      {pending ? pendingLabel : idleLabel}
    </Button>
  );
}

function SortableHead({
  sortKey,
  activeSort,
  children,
  className,
  onSort,
}: {
  sortKey: SortKey;
  activeSort: SortState;
  children: React.ReactNode;
  className?: string;
  onSort: (key: SortKey) => void;
}) {
  const isActive = activeSort.key === sortKey;
  const Icon = isActive ? (activeSort.direction === "asc" ? ArrowUp : ArrowDown) : ArrowUpDown;

  return (
    <TableHead className={cn("text-muted-foreground", className)}>
      <button
        type="button"
        className="inline-flex h-8 items-center gap-1.5 rounded-sm px-1 text-left text-sm font-medium hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        onClick={() => onSort(sortKey)}
      >
        {children}
        <Icon aria-hidden="true" className="size-3.5" />
      </button>
    </TableHead>
  );
}

function NotificationTableRow({
  item,
  dateSettings,
  selected,
  onSelectedChange,
}: {
  item: AccountNotificationResponse;
  dateSettings: AccountDateSettings;
  selected: boolean;
  onSelectedChange: (checked: boolean) => void;
}) {
  const [readState, readAction] = useActionState(
    markNotificationAsReadAction,
    initialMutationState,
  );
  const [deleteState, deleteAction] = useActionState(
    deleteNotificationAction,
    initialMutationState,
  );
  const accent = rowAccent(item);

  useMutationToast(readState, {
    success: tAccountClient("account.notifications.updatedOne"),
    error: tAccountClient("account.notifications.readUpdateFailed"),
  });
  useMutationToast(deleteState, {
    success: tAccountClient("account.notifications.removed"),
    error: tAccountClient("account.common.removeFailed"),
  });

  return (
    <TableRow data-state={selected ? "selected" : undefined} className="h-10 hover:bg-muted/30">
      <TableCell className="w-10 px-4">
        <Checkbox
          aria-label={`Select ${item.title}`}
          checked={selected}
          onCheckedChange={(value) => onSelectedChange(Boolean(value))}
        />
      </TableCell>
      <TableCell className="min-w-[260px] border-l border-border/50">
        <div className="flex min-w-0 items-center gap-2">
          <span
            className={`grid size-5 shrink-0 place-items-center rounded-sm text-[11px] font-semibold ${accent}`}
          >
            {item.title.charAt(0).toUpperCase()}
          </span>
          <div className="min-w-0">
            <Text className="truncate text-sm font-medium leading-5">{item.title}</Text>
            <Text className="truncate text-xs text-muted-foreground">{item.description}</Text>
          </div>
        </div>
      </TableCell>
      <TableCell className="border-l border-border/50">
        <Badge variant="outline" className="gap-1 border-transparent bg-muted px-1.5">
          <Tag aria-hidden="true" className="size-3" />
          {item.category}
        </Badge>
      </TableCell>
      <TableCell className="border-l border-border/50">
        <Badge variant="outline" className="gap-1 border-transparent bg-muted px-1.5">
          <Shield aria-hidden="true" className="size-3" />
          {item.severity}
        </Badge>
      </TableCell>
      <TableCell className="min-w-[190px] border-l border-border/50 font-medium">
        {formatAccountDateTime(item.occurredAt, dateSettings)}
      </TableCell>
      <TableCell className="border-l border-border/50">
        <Badge
          variant={item.isRead ? "outline" : "secondary"}
          className="gap-1 border-transparent px-1.5"
        >
          {item.isRead ? (
            <CheckCircle2 aria-hidden="true" className="size-3" />
          ) : (
            <CircleDot aria-hidden="true" className="size-3" />
          )}
          {statusLabel(item)}
        </Badge>
      </TableCell>
      <TableCell className="min-w-[80px] border-l border-border/50">
        <DropdownMenu>
          <DropdownMenuTrigger
            className="inline-grid size-7 place-items-center rounded-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            aria-label={`${item.title} actions`}
            title={`${item.title} actions`}
          >
            <MoreHorizontal aria-hidden="true" className="size-4" />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-44">
            {!item.isRead ? (
              <div className="p-1">
                <form action={readAction}>
                  <input type="hidden" name="notificationId" value={item.id} />
                  <RowActionButton
                    idleLabel={tAccountClient("account.notifications.markRead")}
                    pendingLabel={tAccountClient("account.common.marking")}
                    icon={<CheckCircle2 aria-hidden="true" className="size-3.5" />}
                  />
                </form>
              </div>
            ) : (
              <DropdownMenuItem disabled>
                <CheckCircle2 aria-hidden="true" className="size-3.5" />
                {tAccountClient("account.notifications.readLabel")}
              </DropdownMenuItem>
            )}
            <div className="p-1">
              <form action={deleteAction}>
                <input type="hidden" name="notificationId" value={item.id} />
                <input type="hidden" name="confirm" value="delete-notification" />
                <RowActionButton
                  idleLabel={tAccountClient("account.common.remove")}
                  pendingLabel={tAccountClient("account.common.removing")}
                  variant="destructive"
                  icon={<Trash2 aria-hidden="true" className="size-3.5" />}
                />
              </form>
            </div>
          </DropdownMenuContent>
        </DropdownMenu>
      </TableCell>
    </TableRow>
  );
}

export function NotificationList({ notifications, dateSettings, options }: NotificationListProps) {
  const [filters, setFilters] = useState<FilterState>(defaultFilters);
  const [sort, setSort] = useState<SortState>({ key: "occurredAt", direction: "desc" });
  const [selectedIds, setSelectedIds] = useState<Set<string>>(() => new Set());

  const categoryOptions = useMemo(
    () => uniqueSorted(notifications.items.map((item) => item.category)),
    [notifications.items],
  );
  const severityOptions = useMemo(
    () => uniqueSorted(notifications.items.map((item) => item.severity)),
    [notifications.items],
  );

  const filteredItems = useMemo(() => {
    const titleFilter = normalizeFilterValue(filters.title);
    const occurredFilter = normalizeFilterValue(filters.occurred);

    return notifications.items
      .filter((item) => {
        const formattedDate = formatAccountDateTime(item.occurredAt, dateSettings);
        const matchesTitle =
          titleFilter.length === 0 ||
          normalizeFilterValue(`${item.title} ${item.description}`).includes(titleFilter);
        const matchesCategory = filters.category === "all" || item.category === filters.category;
        const matchesSeverity = filters.severity === "all" || item.severity === filters.severity;
        const matchesOccurred =
          occurredFilter.length === 0 ||
          normalizeFilterValue(formattedDate).includes(occurredFilter);
        const matchesStatus =
          filters.status === "all" ||
          (filters.status === "read" && item.isRead) ||
          (filters.status === "unread" && !item.isRead);

        return (
          matchesTitle && matchesCategory && matchesSeverity && matchesOccurred && matchesStatus
        );
      })
      .sort((left, right) => compareNotifications(left, right, sort));
  }, [dateSettings, filters, notifications.items, sort]);

  const filteredIds = useMemo(() => filteredItems.map((item) => item.id), [filteredItems]);
  const allVisibleSelected =
    filteredIds.length > 0 && filteredIds.every((id) => selectedIds.has(id));
  const someVisibleSelected = filteredIds.some((id) => selectedIds.has(id));
  const selectedVisibleCount = filteredIds.filter((id) => selectedIds.has(id)).length;

  function updateFilter<TKey extends keyof FilterState>(key: TKey, value: FilterState[TKey]) {
    setFilters((current) => ({ ...current, [key]: value }));
  }

  function handleSort(key: SortKey) {
    setSort((current) => {
      if (current.key !== key) {
        return { key, direction: "asc" };
      }

      return { key, direction: current.direction === "asc" ? "desc" : "asc" };
    });
  }

  function setRowSelected(id: string, checked: boolean) {
    setSelectedIds((current) => {
      const next = new Set(current);
      if (checked) {
        next.add(id);
      } else {
        next.delete(id);
      }
      return next;
    });
  }

  function setAllVisibleSelected(checked: boolean) {
    setSelectedIds((current) => {
      const next = new Set(current);
      for (const id of filteredIds) {
        if (checked) {
          next.add(id);
        } else {
          next.delete(id);
        }
      }
      return next;
    });
  }

  if (notifications.items.length === 0) {
    return (
      <Empty className="border-none py-12" role="status" aria-live="polite">
        <EmptyHeader>
          <EmptyTitle>{tAccountClient("account.notifications.emptyTitle")}</EmptyTitle>
          <EmptyDescription>
            {tAccountClient("account.notifications.managementDescription")}
          </EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <div className="min-w-0">
      <div className="grid gap-2 border-b border-border/70 bg-background/40 p-3 md:grid-cols-[minmax(180px,1.4fr)_minmax(130px,0.8fr)_minmax(130px,0.8fr)_minmax(150px,0.9fr)_minmax(120px,0.7fr)_auto_auto]">
        <Input
          value={filters.title}
          placeholder="Filter by notification"
          aria-label="Filter by notification"
          onChange={(event) => updateFilter("title", event.target.value)}
        />
        <select
          value={filters.category}
          aria-label="Filter by category"
          className="h-8 rounded-sm border border-input bg-background px-2 text-sm text-foreground"
          onChange={(event) => updateFilter("category", event.target.value)}
        >
          <option value="all">All categories</option>
          {categoryOptions.map((category) => (
            <option key={category} value={category}>
              {category}
            </option>
          ))}
        </select>
        <select
          value={filters.severity}
          aria-label="Filter by severity"
          className="h-8 rounded-sm border border-input bg-background px-2 text-sm text-foreground"
          onChange={(event) => updateFilter("severity", event.target.value)}
        >
          <option value="all">All severities</option>
          {severityOptions.map((severity) => (
            <option key={severity} value={severity}>
              {severity}
            </option>
          ))}
        </select>
        <Input
          value={filters.occurred}
          placeholder="Filter by occurred"
          aria-label="Filter by occurred date"
          onChange={(event) => updateFilter("occurred", event.target.value)}
        />
        <select
          value={filters.status}
          aria-label="Filter by status"
          className="h-8 rounded-sm border border-input bg-background px-2 text-sm text-foreground"
          onChange={(event) => updateFilter("status", event.target.value as FilterState["status"])}
        >
          <option value="all">All statuses</option>
          <option value="unread">{tAccountClient("account.notifications.unreadLabel")}</option>
          <option value="read">{tAccountClient("account.notifications.readLabel")}</option>
        </select>
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
        {options}
      </div>

      <div className="flex items-center justify-between border-b border-border/70 px-4 py-2 text-xs text-muted-foreground">
        <span className="inline-flex items-center gap-1.5">
          <Filter aria-hidden="true" className="size-3.5" />
          {filteredItems.length} shown
        </span>
        <span>{selectedVisibleCount} selected</span>
      </div>

      <Table>
        <TableHeader className="bg-muted/10">
          <TableRow className="hover:bg-transparent">
            <TableHead className="w-10 px-4">
              <Checkbox
                aria-label="Select all visible notifications"
                checked={allVisibleSelected || someVisibleSelected}
                onCheckedChange={(value) => setAllVisibleSelected(Boolean(value))}
              />
            </TableHead>
            <SortableHead
              sortKey="title"
              activeSort={sort}
              onSort={handleSort}
              className="min-w-[260px] border-l border-border/50"
            >
              <Bell aria-hidden="true" className="size-4" />
              Notification
            </SortableHead>
            <SortableHead
              sortKey="category"
              activeSort={sort}
              onSort={handleSort}
              className="border-l border-border/50"
            >
              <Tag aria-hidden="true" className="size-4" />
              Category
            </SortableHead>
            <SortableHead
              sortKey="severity"
              activeSort={sort}
              onSort={handleSort}
              className="border-l border-border/50"
            >
              <Shield aria-hidden="true" className="size-4" />
              Severity
            </SortableHead>
            <SortableHead
              sortKey="occurredAt"
              activeSort={sort}
              onSort={handleSort}
              className="min-w-[190px] border-l border-border/50"
            >
              <CalendarClock aria-hidden="true" className="size-4" />
              Occurred
            </SortableHead>
            <SortableHead
              sortKey="status"
              activeSort={sort}
              onSort={handleSort}
              className="border-l border-border/50"
            >
              <CircleDot aria-hidden="true" className="size-4" />
              Status
            </SortableHead>
            <TableHead className="border-l border-border/50 text-muted-foreground">
              Actions
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filteredItems.length === 0 ? (
            <TableRow className="hover:bg-transparent">
              <TableCell colSpan={7}>
                <Empty className="border-none py-10" role="status" aria-live="polite">
                  <EmptyHeader>
                    <EmptyTitle>No matching notifications</EmptyTitle>
                    <EmptyDescription>
                      Try changing filters or clearing the current view.
                    </EmptyDescription>
                  </EmptyHeader>
                </Empty>
              </TableCell>
            </TableRow>
          ) : (
            filteredItems.map((item) => (
              <NotificationTableRow
                key={item.id}
                item={item}
                dateSettings={dateSettings}
                selected={selectedIds.has(item.id)}
                onSelectedChange={(checked) => setRowSelected(item.id, checked)}
              />
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
