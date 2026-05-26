"use client";

import { useActionState, useEffect } from "react";
import { useFormStatus } from "react-dom";
import {
  Badge,
  Button,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
} from "@netmetric/ui";
import { toast } from "@netmetric/ui/client";
import { BellRing, CheckCircle2, Mail, MonitorSmartphone, Save, Tag } from "lucide-react";

import type { NotificationPreferencesResponse } from "@/lib/account-api";

import { updateNotificationPreferencesAction } from "../actions/notification-actions";
import { initialMutationState } from "../actions/mutation-state";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type NotificationPreferencesPanelProps = {
  preferences: NotificationPreferencesResponse;
};

function SaveButton() {
  const { pending } = useFormStatus();
  return (
    <Button size="xs" type="submit" disabled={pending} className="gap-1.5">
      <Save aria-hidden="true" className="size-3.5" />
      {pending
        ? tAccountClient("account.common.saving")
        : tAccountClient("account.preferences.save")}
    </Button>
  );
}

export function NotificationPreferencesPanel({ preferences }: NotificationPreferencesPanelProps) {
  const [state, formAction] = useActionState(
    updateNotificationPreferencesAction,
    initialMutationState,
  );

  useEffect(() => {
    if (!state.message || state.status === "idle") {
      return;
    }

    if (state.status === "success") {
      toast.success(tAccountClient("account.notifications.preferencesSaved"), {
        description: state.message,
      });
      return;
    }

    toast.error(tAccountClient("account.notifications.preferencesUpdateFailed"), {
      description: state.message,
    });
  }, [state]);

  if (preferences.items.length === 0) {
    return null;
  }

  return (
    <section className="overflow-hidden">
      <div className="flex min-h-12 flex-wrap items-center justify-between gap-3 border-b border-border/70 px-1 py-2.5">
        <div className="flex min-w-0 items-center gap-2">
          <BellRing aria-hidden="true" className="size-4 shrink-0 text-muted-foreground" />
          <Text className="truncate text-sm font-medium">
            {tAccountClient("account.notifications.preferenceSettingsTitle")}
          </Text>
          <span className="text-sm text-muted-foreground">/</span>
          <span className="text-sm text-muted-foreground">{preferences.items.length}</span>
        </div>
        <Text className="hidden text-sm text-muted-foreground md:block">
          {tAccountClient("account.notifications.deliveryDescription")}
        </Text>
      </div>

      <div className="space-y-4 py-4">
        <form action={formAction} className="space-y-4">
          <div className="overflow-x-auto">
            <Table>
              <TableHeader className="bg-muted/10">
                <TableRow className="hover:bg-transparent">
                  <TableHead className="min-w-[180px] text-muted-foreground">
                    <span className="inline-flex items-center gap-1.5">
                      <MonitorSmartphone aria-hidden="true" className="size-4" />
                      Channel
                    </span>
                  </TableHead>
                  <TableHead className="border-l border-border/50 text-muted-foreground">
                    <span className="inline-flex items-center gap-1.5">
                      <Tag aria-hidden="true" className="size-4" />
                      Category
                    </span>
                  </TableHead>
                  <TableHead className="w-28 border-l border-border/50 text-muted-foreground">
                    Enabled
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {preferences.items.map((item) => {
                  const key = `${item.channel}|${item.category}`;
                  return (
                    <TableRow key={item.id} className="h-10 hover:bg-muted/30">
                      <TableCell>
                        <Badge
                          variant="outline"
                          className="gap-1 border-transparent bg-muted px-1.5"
                        >
                          <Mail aria-hidden="true" className="size-3" />
                          {item.channel}
                        </Badge>
                      </TableCell>
                      <TableCell className="border-l border-border/50 font-medium">
                        {item.category}
                      </TableCell>
                      <TableCell className="border-l border-border/50">
                        <input type="hidden" name={`pref:${item.id}`} value={key} />
                        <label className="inline-flex items-center gap-2 text-sm">
                          <input
                            id={`enabled-${item.id}`}
                            name={`enabled:${key}`}
                            type="checkbox"
                            defaultChecked={item.isEnabled}
                            className="size-4 rounded border border-input accent-primary"
                            aria-label={tAccountClient("account.notifications.toggleAriaLabel", {
                              channel: item.channel,
                              category: item.category,
                            })}
                          />
                          <CheckCircle2
                            aria-hidden="true"
                            className="size-3.5 text-muted-foreground"
                          />
                        </label>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </div>
          <div className="flex justify-end">
            <SaveButton />
          </div>
        </form>
      </div>
    </section>
  );
}
