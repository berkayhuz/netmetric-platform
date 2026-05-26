"use client";

import { Building2, LifeBuoy, LogOut, Settings, UserPlus } from "lucide-react";

import type { AppSidebarUserMenuAction } from "./app-shell";

export type SharedWorkspaceUserMenuLabels = {
  inviteUser: string;
  settings: string;
  support: string;
  createWorkspace: string;
  signOut: string;
  unavailableAction: string;
};

export type SharedWorkspaceUserMenuOptions = {
  labels: SharedWorkspaceUserMenuLabels;
  onSignOut: () => void;
  inviteUserHref: string;
  settingsHref: string;
  createWorkspaceHref: string;
};

export type SharedWorkspaceUserMenuActions = {
  actions: readonly AppSidebarUserMenuAction[];
  overflowActions: readonly AppSidebarUserMenuAction[];
};

export function createSharedWorkspaceUserMenuActions({
  labels,
  onSignOut,
  inviteUserHref,
  settingsHref,
  createWorkspaceHref,
}: SharedWorkspaceUserMenuOptions): SharedWorkspaceUserMenuActions {
  return {
    actions: [
      {
        id: "invite-user",
        label: labels.inviteUser,
        icon: UserPlus,
        href: inviteUserHref,
      },
      {
        id: "settings",
        label: labels.settings,
        icon: Settings,
        href: settingsHref,
      },
      {
        id: "support",
        label: labels.support,
        icon: LifeBuoy,
        disabled: true,
        title: labels.unavailableAction,
      },
    ],
    overflowActions: [
      {
        id: "create-workspace",
        label: labels.createWorkspace,
        icon: Building2,
        href: createWorkspaceHref,
      },
      {
        id: "sign-out",
        label: labels.signOut,
        icon: LogOut,
        onSelect: onSignOut,
      },
    ],
  };
}
