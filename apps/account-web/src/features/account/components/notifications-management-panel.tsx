"use client";

import { useActionState, useEffect } from "react";
import { useFormStatus } from "react-dom";
import { Button } from "@netmetric/ui";
import {
  DropdownMenuContent,
  DropdownMenuTrigger,
  toast,
  DropdownMenu,
} from "@netmetric/ui/client";
import { MoreHorizontal } from "lucide-react";

import type {
  AccountNotificationsResponse,
  NotificationPreferencesResponse,
} from "@/lib/account-api";
import type { AccountDateSettings } from "@/lib/account-date";

import { markAllNotificationsAsReadAction } from "../actions/notification-actions";
import { initialMutationState } from "../actions/mutation-state";
import { AccountPagePanel } from "./account-page-panel";
import { NotificationList } from "./notification-list";
import { NotificationPreferencesPanel } from "./notification-preferences-panel";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type NotificationsManagementPanelProps = {
  notifications: AccountNotificationsResponse;
  preferences: NotificationPreferencesResponse;
  activeFilter: "all" | "unread" | "read";
  dateSettings: AccountDateSettings;
};

function MarkAllButton() {
  const { pending } = useFormStatus();
  return (
    <Button size="xs" type="submit" disabled={pending} variant="outline">
      {pending
        ? tAccountClient("account.common.updating")
        : tAccountClient("account.notifications.markAllRead")}
    </Button>
  );
}

export function NotificationsManagementPanel({
  notifications,
  preferences,
  dateSettings,
}: NotificationsManagementPanelProps) {
  const [markAllState, markAllAction] = useActionState(
    markAllNotificationsAsReadAction,
    initialMutationState,
  );

  useEffect(() => {
    if (!markAllState.message || markAllState.status === "idle") {
      return;
    }

    if (markAllState.status === "success") {
      toast.success(tAccountClient("account.notifications.updated"), {
        description: markAllState.message,
      });
      return;
    }

    toast.error(tAccountClient("account.notifications.bulkActionFailed"), {
      description: markAllState.message,
    });
  }, [markAllState]);

  return (
    <AccountPagePanel
      title={tAccountClient("account.notifications.title")}
      description={tAccountClient("account.notifications.managementDescription")}
      contentClassName="space-y-5"
      bodyPadding="none"
    >
      <section className="overflow-hidden">
        <NotificationList
          notifications={notifications}
          dateSettings={dateSettings}
          options={
            <DropdownMenu>
              <DropdownMenuTrigger className="inline-flex h-8 items-center gap-1.5 rounded-sm px-2 text-xs font-medium transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring">
                <MoreHorizontal aria-hidden="true" className="size-3.5" />
                Options
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end" className="w-48">
                <div className="p-1">
                  <form action={markAllAction}>
                    <input type="hidden" name="confirm" value="mark-all-read" />
                    <MarkAllButton />
                  </form>
                </div>
              </DropdownMenuContent>
            </DropdownMenu>
          }
        />

        <div className="grid border-t border-border/70 bg-muted/10 px-4 py-3 text-sm text-muted-foreground sm:grid-cols-[180px_1fr_1fr_1fr]">
          <span>Calculate</span>
          <span>
            {tAccountClient("account.notifications.totalLabel")}{" "}
            <strong className="font-semibold text-foreground">{notifications.totalCount}</strong>
          </span>
          <span>
            {tAccountClient("account.notifications.unreadLabel")}{" "}
            <strong className="font-semibold text-foreground">{notifications.unreadCount}</strong>
          </span>
          <span>
            {tAccountClient("account.notifications.readLabel")}{" "}
            <strong className="font-semibold text-foreground">{notifications.readCount}</strong>
          </span>
        </div>
      </section>

      <NotificationPreferencesPanel preferences={preferences} />
    </AccountPagePanel>
  );
}
