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
  CalendarClock,
  CheckCircle2,
  Laptop,
  MapPin,
  MoreHorizontal,
  Network,
  ShieldCheck,
  Trash2,
  X,
} from "lucide-react";

import type {
  TrustedDeviceResponse,
  TrustedDevicesResponse,
  UserSessionResponse,
  UserSessionsResponse,
} from "@/lib/account-api";
import type { AccountDateSettings } from "@/lib/account-date";
import { formatAccountDateTime } from "@/lib/account-date";

import { initialMutationState, type MutationState } from "../actions/mutation-state";
import { AccountPagePanel } from "./account-page-panel";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type SessionsManagementPanelProps = {
  sessions: UserSessionsResponse;
  trustedDevices: TrustedDevicesResponse;
  dateSettings: AccountDateSettings;
  revokeSessionAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
  revokeOtherSessionsAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
  revokeTrustedDeviceAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
  revokeOtherTrustedDevicesAction: (
    state: MutationState,
    formData: FormData,
  ) => Promise<MutationState>;
};

type SortDirection = "asc" | "desc";
type SessionSortKey = "device" | "status" | "ip" | "location" | "lastSeenAt";
type DeviceSortKey = "name" | "status" | "ip" | "trustedAt" | "expiresAt";
type StatusFilter = "all" | "current" | "active" | "inactive";
type DeviceStatusFilter = "all" | "current" | "trusted" | "expired";

type SessionFilters = {
  query: string;
  status: StatusFilter;
};

type DeviceFilters = {
  query: string;
  status: DeviceStatusFilter;
};

const defaultSessionFilters: SessionFilters = {
  query: "",
  status: "all",
};

const defaultDeviceFilters: DeviceFilters = {
  query: "",
  status: "all",
};

function normalize(value: string | null | undefined): string {
  return (value ?? "").trim().toLocaleLowerCase();
}

function compareStrings(left: string, right: string, direction: SortDirection): number {
  const result = left.localeCompare(right, undefined, { sensitivity: "base" });
  return direction === "asc" ? result : -result;
}

function compareDates(left: string, right: string, direction: SortDirection): number {
  const result = new Date(left).getTime() - new Date(right).getTime();
  return direction === "asc" ? result : -result;
}

function sessionStatus(session: UserSessionResponse): string {
  if (session.isCurrent) {
    return tAccountClient("account.sessions.currentSession");
  }

  return session.isActive
    ? tAccountClient("account.common.active")
    : tAccountClient("account.common.inactive");
}

function deviceStatus(device: TrustedDeviceResponse): string {
  if (device.isCurrent) {
    return tAccountClient("account.sessions.currentDevice");
  }

  return device.isActive
    ? tAccountClient("account.sessions.trusted")
    : tAccountClient("account.sessions.expired");
}

function deviceLabel(session: UserSessionResponse): string {
  return session.deviceName || tAccountClient("account.sessions.unknownDevice");
}

function useMutationToast(state: MutationState): void {
  useEffect(() => {
    if (!state.message || state.status === "idle") {
      return;
    }

    if (state.status === "success") {
      toast.success(tAccountClient("account.common.success"), { description: state.message });
      return;
    }

    toast.error(tAccountClient("account.common.actionFailed"), { description: state.message });
  }, [state]);
}

function DestructiveSubmitButton({
  label,
  pendingLabel,
  icon,
}: {
  label: string;
  pendingLabel: string;
  icon: React.ReactNode;
}) {
  const { pending } = useFormStatus();
  return (
    <Button
      size="xs"
      type="submit"
      variant="destructive"
      disabled={pending}
      className="w-full justify-start gap-1.5"
    >
      {icon}
      {pending ? pendingLabel : label}
    </Button>
  );
}

function SortableHead<TKey extends string>({
  sortKey,
  activeKey,
  direction,
  className,
  children,
  onSort,
}: {
  sortKey: TKey;
  activeKey: TKey;
  direction: SortDirection;
  className?: string;
  children: React.ReactNode;
  onSort: (key: TKey) => void;
}) {
  const isActive = activeKey === sortKey;
  const Icon = isActive ? (direction === "asc" ? ArrowUp : ArrowDown) : ArrowUpDown;

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

function SessionRow({
  session,
  dateSettings,
  selected,
  onSelectedChange,
  revokeSessionAction,
}: {
  session: UserSessionResponse;
  dateSettings: AccountDateSettings;
  selected: boolean;
  onSelectedChange: (checked: boolean) => void;
  revokeSessionAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
}) {
  const [state, formAction] = useActionState(revokeSessionAction, initialMutationState);
  useMutationToast(state);

  return (
    <TableRow data-state={selected ? "selected" : undefined} className="h-10 hover:bg-muted/30">
      <TableCell className="w-10 px-4">
        <Checkbox
          aria-label={`Select ${deviceLabel(session)}`}
          checked={selected}
          onCheckedChange={(value) => onSelectedChange(Boolean(value))}
        />
      </TableCell>
      <TableCell className="min-w-[240px] border-l border-border/50">
        <div className="flex min-w-0 items-center gap-2">
          <span className="grid size-5 shrink-0 place-items-center rounded-sm bg-cyan-500/15 text-cyan-700 dark:text-cyan-300">
            <Laptop aria-hidden="true" className="size-3.5" />
          </span>
          <div className="min-w-0">
            <Text className="truncate text-sm font-medium leading-5">{deviceLabel(session)}</Text>
            <Text className="truncate text-xs text-muted-foreground">{session.userAgent}</Text>
          </div>
        </div>
      </TableCell>
      <TableCell className="border-l border-border/50">
        <Badge
          variant={session.isCurrent || session.isActive ? "secondary" : "outline"}
          className="gap-1 border-transparent px-1.5"
        >
          <CheckCircle2 aria-hidden="true" className="size-3" />
          {sessionStatus(session)}
        </Badge>
      </TableCell>
      <TableCell className="border-l border-border/50">
        {session.ipAddress || tAccountClient("account.common.notAvailable")}
      </TableCell>
      <TableCell className="min-w-[160px] border-l border-border/50">
        {session.approximateLocation || tAccountClient("account.common.notAvailable")}
      </TableCell>
      <TableCell className="min-w-[190px] border-l border-border/50 font-medium">
        {formatAccountDateTime(session.lastSeenAt, dateSettings)}
      </TableCell>
      <TableCell className="min-w-[80px] border-l border-border/50">
        <DropdownMenu>
          <DropdownMenuTrigger
            className="inline-grid size-7 place-items-center rounded-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            aria-label={`${deviceLabel(session)} actions`}
            title={`${deviceLabel(session)} actions`}
          >
            <MoreHorizontal aria-hidden="true" className="size-4" />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            {session.isCurrent ? (
              <DropdownMenuItem disabled>
                <ShieldCheck aria-hidden="true" className="size-3.5" />
                {tAccountClient("account.sessions.currentCannotRevoke")}
              </DropdownMenuItem>
            ) : (
              <div className="p-1">
                <form action={formAction}>
                  <input type="hidden" name="sessionId" value={session.id} />
                  <DestructiveSubmitButton
                    label={tAccountClient("account.sessions.confirmRevokeSession")}
                    pendingLabel={tAccountClient("account.sessions.revokingSession")}
                    icon={<Trash2 aria-hidden="true" className="size-3.5" />}
                  />
                </form>
              </div>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </TableCell>
    </TableRow>
  );
}

function DeviceRow({
  device,
  dateSettings,
  selected,
  onSelectedChange,
  revokeTrustedDeviceAction,
}: {
  device: TrustedDeviceResponse;
  dateSettings: AccountDateSettings;
  selected: boolean;
  onSelectedChange: (checked: boolean) => void;
  revokeTrustedDeviceAction: (state: MutationState, formData: FormData) => Promise<MutationState>;
}) {
  const [state, formAction] = useActionState(revokeTrustedDeviceAction, initialMutationState);
  useMutationToast(state);

  return (
    <TableRow data-state={selected ? "selected" : undefined} className="h-10 hover:bg-muted/30">
      <TableCell className="w-10 px-4">
        <Checkbox
          aria-label={`Select ${device.name}`}
          checked={selected}
          onCheckedChange={(value) => onSelectedChange(Boolean(value))}
        />
      </TableCell>
      <TableCell className="min-w-[240px] border-l border-border/50">
        <div className="flex min-w-0 items-center gap-2">
          <span className="grid size-5 shrink-0 place-items-center rounded-sm bg-emerald-500/15 text-emerald-700 dark:text-emerald-300">
            <ShieldCheck aria-hidden="true" className="size-3.5" />
          </span>
          <div className="min-w-0">
            <Text className="truncate text-sm font-medium leading-5">{device.name}</Text>
            <Text className="truncate text-xs text-muted-foreground">{device.userAgent}</Text>
          </div>
        </div>
      </TableCell>
      <TableCell className="border-l border-border/50">
        <Badge
          variant={device.isCurrent || device.isActive ? "secondary" : "outline"}
          className="gap-1 border-transparent px-1.5"
        >
          <CheckCircle2 aria-hidden="true" className="size-3" />
          {deviceStatus(device)}
        </Badge>
      </TableCell>
      <TableCell className="border-l border-border/50">
        {device.ipAddress || tAccountClient("account.common.notAvailable")}
      </TableCell>
      <TableCell className="min-w-[190px] border-l border-border/50 font-medium">
        {formatAccountDateTime(device.trustedAt, dateSettings)}
      </TableCell>
      <TableCell className="min-w-[190px] border-l border-border/50 font-medium">
        {formatAccountDateTime(device.expiresAt, dateSettings)}
      </TableCell>
      <TableCell className="min-w-[80px] border-l border-border/50">
        <DropdownMenu>
          <DropdownMenuTrigger
            className="inline-grid size-7 place-items-center rounded-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
            aria-label={`${device.name} actions`}
            title={`${device.name} actions`}
          >
            <MoreHorizontal aria-hidden="true" className="size-4" />
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-56">
            <div className="p-1">
              <form action={formAction}>
                <input type="hidden" name="deviceId" value={device.id} />
                <input type="hidden" name="confirm" value="revoke-device" />
                <DestructiveSubmitButton
                  label={tAccountClient("account.sessions.confirmRevokeDevice")}
                  pendingLabel={tAccountClient("account.sessions.revokingDevice")}
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

function BulkOptions({
  canRevoke,
  action,
  confirmName,
  confirmValue,
  label,
  pendingLabel,
}: {
  canRevoke: boolean;
  action: (formData: FormData) => void;
  confirmName: string;
  confirmValue: string;
  label: string;
  pendingLabel: string;
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger className="inline-flex h-8 items-center gap-1.5 rounded-sm px-2 text-xs font-medium transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
        <MoreHorizontal aria-hidden="true" className="size-3.5" />
        Options
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" className="w-56">
        <div className="p-1">
          <form action={action}>
            <input type="hidden" name={confirmName} value={confirmValue} />
            <DestructiveSubmitButton
              label={label}
              pendingLabel={pendingLabel}
              icon={<Trash2 aria-hidden="true" className="size-3.5" />}
            />
          </form>
        </div>
        {!canRevoke ? (
          <DropdownMenuItem disabled>
            <ShieldCheck aria-hidden="true" className="size-3.5" />
            {tAccountClient("account.common.notAvailable")}
          </DropdownMenuItem>
        ) : null}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export function SessionsManagementPanel({
  sessions,
  trustedDevices,
  revokeSessionAction,
  revokeOtherSessionsAction,
  revokeTrustedDeviceAction,
  revokeOtherTrustedDevicesAction,
  dateSettings,
}: SessionsManagementPanelProps) {
  const [revokeOthersState, revokeOthersFormAction] = useActionState(
    revokeOtherSessionsAction,
    initialMutationState,
  );
  const [revokeOtherDevicesState, revokeOtherDevicesFormAction] = useActionState(
    revokeOtherTrustedDevicesAction,
    initialMutationState,
  );

  useMutationToast(revokeOthersState);
  useMutationToast(revokeOtherDevicesState);

  const [sessionFilters, setSessionFilters] = useState<SessionFilters>(defaultSessionFilters);
  const [deviceFilters, setDeviceFilters] = useState<DeviceFilters>(defaultDeviceFilters);
  const [sessionSort, setSessionSort] = useState<{
    key: SessionSortKey;
    direction: SortDirection;
  }>({ key: "lastSeenAt", direction: "desc" });
  const [deviceSort, setDeviceSort] = useState<{
    key: DeviceSortKey;
    direction: SortDirection;
  }>({ key: "trustedAt", direction: "desc" });
  const [selectedSessionIds, setSelectedSessionIds] = useState<Set<string>>(() => new Set());
  const [selectedDeviceIds, setSelectedDeviceIds] = useState<Set<string>>(() => new Set());

  const filteredSessions = useMemo(() => {
    const query = normalize(sessionFilters.query);
    return sessions.items
      .filter((session) => {
        const matchesQuery =
          query.length === 0 ||
          normalize(
            `${deviceLabel(session)} ${session.userAgent} ${session.ipAddress} ${session.approximateLocation}`,
          ).includes(query);
        const matchesStatus =
          sessionFilters.status === "all" ||
          (sessionFilters.status === "current" && session.isCurrent) ||
          (sessionFilters.status === "active" && session.isActive && !session.isCurrent) ||
          (sessionFilters.status === "inactive" && !session.isActive);

        return matchesQuery && matchesStatus;
      })
      .sort((left, right) => {
        switch (sessionSort.key) {
          case "device":
            return compareStrings(deviceLabel(left), deviceLabel(right), sessionSort.direction);
          case "status":
            return compareStrings(sessionStatus(left), sessionStatus(right), sessionSort.direction);
          case "ip":
            return compareStrings(
              left.ipAddress ?? "",
              right.ipAddress ?? "",
              sessionSort.direction,
            );
          case "location":
            return compareStrings(
              left.approximateLocation ?? "",
              right.approximateLocation ?? "",
              sessionSort.direction,
            );
          case "lastSeenAt":
            return compareDates(left.lastSeenAt, right.lastSeenAt, sessionSort.direction);
          default:
            return 0;
        }
      });
  }, [sessionFilters, sessionSort, sessions.items]);

  const filteredDevices = useMemo(() => {
    const query = normalize(deviceFilters.query);
    return trustedDevices.items
      .filter((device) => {
        const matchesQuery =
          query.length === 0 ||
          normalize(`${device.name} ${device.userAgent} ${device.ipAddress}`).includes(query);
        const matchesStatus =
          deviceFilters.status === "all" ||
          (deviceFilters.status === "current" && device.isCurrent) ||
          (deviceFilters.status === "trusted" && device.isActive && !device.isCurrent) ||
          (deviceFilters.status === "expired" && !device.isActive);

        return matchesQuery && matchesStatus;
      })
      .sort((left, right) => {
        switch (deviceSort.key) {
          case "name":
            return compareStrings(left.name, right.name, deviceSort.direction);
          case "status":
            return compareStrings(deviceStatus(left), deviceStatus(right), deviceSort.direction);
          case "ip":
            return compareStrings(
              left.ipAddress ?? "",
              right.ipAddress ?? "",
              deviceSort.direction,
            );
          case "trustedAt":
            return compareDates(left.trustedAt, right.trustedAt, deviceSort.direction);
          case "expiresAt":
            return compareDates(left.expiresAt, right.expiresAt, deviceSort.direction);
          default:
            return 0;
        }
      });
  }, [deviceFilters, deviceSort, trustedDevices.items]);

  const filteredSessionIds = filteredSessions.map((session) => session.id);
  const filteredDeviceIds = filteredDevices.map((device) => device.id);
  const allSessionsSelected =
    filteredSessionIds.length > 0 && filteredSessionIds.every((id) => selectedSessionIds.has(id));
  const someSessionsSelected = filteredSessionIds.some((id) => selectedSessionIds.has(id));
  const allDevicesSelected =
    filteredDeviceIds.length > 0 && filteredDeviceIds.every((id) => selectedDeviceIds.has(id));
  const someDevicesSelected = filteredDeviceIds.some((id) => selectedDeviceIds.has(id));

  function toggleSessionSort(key: SessionSortKey) {
    setSessionSort((current) =>
      current.key === key
        ? { key, direction: current.direction === "asc" ? "desc" : "asc" }
        : { key, direction: "asc" },
    );
  }

  function toggleDeviceSort(key: DeviceSortKey) {
    setDeviceSort((current) =>
      current.key === key
        ? { key, direction: current.direction === "asc" ? "desc" : "asc" }
        : { key, direction: "asc" },
    );
  }

  function setAllSelected(
    ids: string[],
    checked: boolean,
    setter: React.Dispatch<React.SetStateAction<Set<string>>>,
  ) {
    setter((current) => {
      const next = new Set(current);
      for (const id of ids) {
        if (checked) {
          next.add(id);
        } else {
          next.delete(id);
        }
      }
      return next;
    });
  }

  function setSelected(
    id: string,
    checked: boolean,
    setter: React.Dispatch<React.SetStateAction<Set<string>>>,
  ) {
    setter((current) => {
      const next = new Set(current);
      if (checked) {
        next.add(id);
      } else {
        next.delete(id);
      }
      return next;
    });
  }

  return (
    <AccountPagePanel
      title={tAccountClient("account.sessions.title")}
      description={tAccountClient("account.sessions.manageDescription")}
      contentClassName="space-y-0"
      bodyPadding="none"
    >
      <section className="overflow-hidden">
        <div className="grid gap-2 border-b border-border/70 bg-background/40 p-3 md:grid-cols-[minmax(220px,1fr)_minmax(150px,220px)_auto_auto]">
          <Input
            value={sessionFilters.query}
            placeholder="Filter sessions"
            aria-label="Filter sessions"
            onChange={(event) =>
              setSessionFilters((current) => ({ ...current, query: event.target.value }))
            }
          />
          <select
            value={sessionFilters.status}
            aria-label="Filter sessions by status"
            className="h-8 rounded-sm border border-input bg-background px-2 text-sm text-foreground"
            onChange={(event) =>
              setSessionFilters((current) => ({
                ...current,
                status: event.target.value as StatusFilter,
              }))
            }
          >
            <option value="all">All statuses</option>
            <option value="current">{tAccountClient("account.sessions.currentSession")}</option>
            <option value="active">{tAccountClient("account.common.active")}</option>
            <option value="inactive">{tAccountClient("account.common.inactive")}</option>
          </select>
          <Button
            type="button"
            size="xs"
            variant="ghost"
            className="gap-1.5"
            onClick={() => setSessionFilters(defaultSessionFilters)}
          >
            <X aria-hidden="true" className="size-3.5" />
            Clear
          </Button>
          <BulkOptions
            canRevoke={sessions.items.some((session) => !session.isCurrent)}
            action={revokeOthersFormAction}
            confirmName="confirm"
            confirmValue="revoke-others"
            label={tAccountClient("account.sessions.confirmRevokeOtherSessions")}
            pendingLabel={tAccountClient("account.sessions.revokingOthers")}
          />
        </div>

        <div className="flex items-center justify-between border-b border-border/70 px-4 py-2 text-xs text-muted-foreground">
          <span>{filteredSessions.length} shown</span>
          <span>
            {filteredSessionIds.filter((id) => selectedSessionIds.has(id)).length} selected
          </span>
        </div>

        <Table>
          <TableHeader className="bg-muted/10">
            <TableRow className="hover:bg-transparent">
              <TableHead className="w-10 px-4">
                <Checkbox
                  aria-label="Select all visible sessions"
                  checked={allSessionsSelected || someSessionsSelected}
                  onCheckedChange={(value) =>
                    setAllSelected(filteredSessionIds, Boolean(value), setSelectedSessionIds)
                  }
                />
              </TableHead>
              <SortableHead
                sortKey="device"
                activeKey={sessionSort.key}
                direction={sessionSort.direction}
                onSort={toggleSessionSort}
                className="min-w-[240px] border-l border-border/50"
              >
                <Laptop aria-hidden="true" className="size-4" />
                Device
              </SortableHead>
              <SortableHead
                sortKey="status"
                activeKey={sessionSort.key}
                direction={sessionSort.direction}
                onSort={toggleSessionSort}
                className="border-l border-border/50"
              >
                <ShieldCheck aria-hidden="true" className="size-4" />
                Status
              </SortableHead>
              <SortableHead
                sortKey="ip"
                activeKey={sessionSort.key}
                direction={sessionSort.direction}
                onSort={toggleSessionSort}
                className="border-l border-border/50"
              >
                <Network aria-hidden="true" className="size-4" />
                {tAccountClient("account.sessions.ipLabel")}
              </SortableHead>
              <SortableHead
                sortKey="location"
                activeKey={sessionSort.key}
                direction={sessionSort.direction}
                onSort={toggleSessionSort}
                className="min-w-[160px] border-l border-border/50"
              >
                <MapPin aria-hidden="true" className="size-4" />
                {tAccountClient("account.sessions.locationLabel")}
              </SortableHead>
              <SortableHead
                sortKey="lastSeenAt"
                activeKey={sessionSort.key}
                direction={sessionSort.direction}
                onSort={toggleSessionSort}
                className="min-w-[190px] border-l border-border/50"
              >
                <CalendarClock aria-hidden="true" className="size-4" />
                {tAccountClient("account.sessions.lastSeenLabel")}
              </SortableHead>
              <TableHead className="border-l border-border/50 text-muted-foreground">
                Actions
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredSessions.length === 0 ? (
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={7}>
                  <Empty className="border-none py-10" role="status" aria-live="polite">
                    <EmptyHeader>
                      <EmptyTitle>{tAccountClient("account.sessions.emptySessions")}</EmptyTitle>
                      <EmptyDescription>
                        {tAccountClient("account.sessions.activeDescription")}
                      </EmptyDescription>
                    </EmptyHeader>
                  </Empty>
                </TableCell>
              </TableRow>
            ) : (
              filteredSessions.map((session) => (
                <SessionRow
                  key={session.id}
                  session={session}
                  dateSettings={dateSettings}
                  selected={selectedSessionIds.has(session.id)}
                  onSelectedChange={(checked) =>
                    setSelected(session.id, checked, setSelectedSessionIds)
                  }
                  revokeSessionAction={revokeSessionAction}
                />
              ))
            )}
          </TableBody>
        </Table>

        <div className="grid border-t border-border/70 bg-muted/10 px-4 py-3 text-sm text-muted-foreground sm:grid-cols-[180px_1fr_1fr_1fr]">
          <span>Calculate</span>
          <span>
            Total <strong className="font-semibold text-foreground">{sessions.items.length}</strong>
          </span>
          <span>
            Active{" "}
            <strong className="font-semibold text-foreground">
              {sessions.items.filter((session) => session.isActive).length}
            </strong>
          </span>
          <span>
            Current{" "}
            <strong className="font-semibold text-foreground">
              {sessions.items.filter((session) => session.isCurrent).length}
            </strong>
          </span>
        </div>
      </section>

      <section className="overflow-hidden">
        <div className="grid gap-2 border-b border-border/70 bg-background/40 p-3 md:grid-cols-[minmax(220px,1fr)_minmax(150px,220px)_auto_auto]">
          <Input
            value={deviceFilters.query}
            placeholder="Filter trusted devices"
            aria-label="Filter trusted devices"
            onChange={(event) =>
              setDeviceFilters((current) => ({ ...current, query: event.target.value }))
            }
          />
          <select
            value={deviceFilters.status}
            aria-label="Filter trusted devices by status"
            className="h-8 rounded-sm border border-input bg-background px-2 text-sm text-foreground"
            onChange={(event) =>
              setDeviceFilters((current) => ({
                ...current,
                status: event.target.value as DeviceStatusFilter,
              }))
            }
          >
            <option value="all">All statuses</option>
            <option value="current">{tAccountClient("account.sessions.currentDevice")}</option>
            <option value="trusted">{tAccountClient("account.sessions.trusted")}</option>
            <option value="expired">{tAccountClient("account.sessions.expired")}</option>
          </select>
          <Button
            type="button"
            size="xs"
            variant="ghost"
            className="gap-1.5"
            onClick={() => setDeviceFilters(defaultDeviceFilters)}
          >
            <X aria-hidden="true" className="size-3.5" />
            Clear
          </Button>
          <BulkOptions
            canRevoke={trustedDevices.items.some((device) => !device.isCurrent && device.isActive)}
            action={revokeOtherDevicesFormAction}
            confirmName="confirm"
            confirmValue="revoke-other-devices"
            label={tAccountClient("account.sessions.confirmRevokeOtherDevices")}
            pendingLabel={tAccountClient("account.sessions.revokingOtherDevices")}
          />
        </div>

        <div className="flex items-center justify-between border-b border-border/70 px-4 py-2 text-xs text-muted-foreground">
          <span>{filteredDevices.length} shown</span>
          <span>{filteredDeviceIds.filter((id) => selectedDeviceIds.has(id)).length} selected</span>
        </div>

        <Table>
          <TableHeader className="bg-muted/10">
            <TableRow className="hover:bg-transparent">
              <TableHead className="w-10 px-4">
                <Checkbox
                  aria-label="Select all visible trusted devices"
                  checked={allDevicesSelected || someDevicesSelected}
                  onCheckedChange={(value) =>
                    setAllSelected(filteredDeviceIds, Boolean(value), setSelectedDeviceIds)
                  }
                />
              </TableHead>
              <SortableHead
                sortKey="name"
                activeKey={deviceSort.key}
                direction={deviceSort.direction}
                onSort={toggleDeviceSort}
                className="min-w-[240px] border-l border-border/50"
              >
                <ShieldCheck aria-hidden="true" className="size-4" />
                Device
              </SortableHead>
              <SortableHead
                sortKey="status"
                activeKey={deviceSort.key}
                direction={deviceSort.direction}
                onSort={toggleDeviceSort}
                className="border-l border-border/50"
              >
                <CheckCircle2 aria-hidden="true" className="size-4" />
                Status
              </SortableHead>
              <SortableHead
                sortKey="ip"
                activeKey={deviceSort.key}
                direction={deviceSort.direction}
                onSort={toggleDeviceSort}
                className="border-l border-border/50"
              >
                <Network aria-hidden="true" className="size-4" />
                {tAccountClient("account.sessions.ipLabel")}
              </SortableHead>
              <SortableHead
                sortKey="trustedAt"
                activeKey={deviceSort.key}
                direction={deviceSort.direction}
                onSort={toggleDeviceSort}
                className="min-w-[190px] border-l border-border/50"
              >
                <CalendarClock aria-hidden="true" className="size-4" />
                {tAccountClient("account.sessions.trustedAtLabel")}
              </SortableHead>
              <SortableHead
                sortKey="expiresAt"
                activeKey={deviceSort.key}
                direction={deviceSort.direction}
                onSort={toggleDeviceSort}
                className="min-w-[190px] border-l border-border/50"
              >
                <CalendarClock aria-hidden="true" className="size-4" />
                {tAccountClient("account.sessions.expiresAtLabel")}
              </SortableHead>
              <TableHead className="border-l border-border/50 text-muted-foreground">
                Actions
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredDevices.length === 0 ? (
              <TableRow className="hover:bg-transparent">
                <TableCell colSpan={7}>
                  <Empty className="border-none py-10" role="status" aria-live="polite">
                    <EmptyHeader>
                      <EmptyTitle>{tAccountClient("account.sessions.emptyDevices")}</EmptyTitle>
                      <EmptyDescription>
                        {tAccountClient("account.sessions.trustedDescription")}
                      </EmptyDescription>
                    </EmptyHeader>
                  </Empty>
                </TableCell>
              </TableRow>
            ) : (
              filteredDevices.map((device) => (
                <DeviceRow
                  key={device.id}
                  device={device}
                  dateSettings={dateSettings}
                  selected={selectedDeviceIds.has(device.id)}
                  onSelectedChange={(checked) =>
                    setSelected(device.id, checked, setSelectedDeviceIds)
                  }
                  revokeTrustedDeviceAction={revokeTrustedDeviceAction}
                />
              ))
            )}
          </TableBody>
        </Table>

        <div className="grid border-t border-border/70 bg-muted/10 px-4 py-3 text-sm text-muted-foreground sm:grid-cols-[180px_1fr_1fr_1fr]">
          <span>Calculate</span>
          <span>
            Total{" "}
            <strong className="font-semibold text-foreground">{trustedDevices.items.length}</strong>
          </span>
          <span>
            Trusted{" "}
            <strong className="font-semibold text-foreground">
              {trustedDevices.items.filter((device) => device.isActive).length}
            </strong>
          </span>
          <span>
            Current{" "}
            <strong className="font-semibold text-foreground">
              {trustedDevices.items.filter((device) => device.isCurrent).length}
            </strong>
          </span>
        </div>
      </section>
    </AccountPagePanel>
  );
}
