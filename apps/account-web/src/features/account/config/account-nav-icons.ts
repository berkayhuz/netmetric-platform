import {
  Bell,
  Building2,
  ClipboardList,
  KeyRound,
  Lock,
  ShieldCheck,
  SlidersHorizontal,
  User,
  UserCog,
  UserRoundCheck,
  Users,
} from "lucide-react";
import type { AppSidebarNavIcon } from "@netmetric/ui/client";

import { accountRoutes } from "./account-routes";

export type AccountRouteHref = (typeof accountRoutes)[number]["href"];

export const accountNavIcons: Record<AccountRouteHref, AppSidebarNavIcon> = {
  "/profile": User,
  "/preferences": SlidersHorizontal,
  "/workspaces": Building2,
  "/security": ShieldCheck,
  "/security/sessions": UserRoundCheck,
  "/security/mfa": Lock,
  "/security/password": KeyRound,
  "/notifications": Bell,
  "/audit": ClipboardList,
  "/settings/team": Users,
  "/privacy": UserCog,
};

export const accountNavIconColors: Record<AccountRouteHref, string> = {
  "/profile": "text-sky-500",
  "/preferences": "text-violet-500",
  "/workspaces": "text-emerald-500",
  "/security": "text-red-500",
  "/security/sessions": "text-cyan-500",
  "/security/mfa": "text-amber-500",
  "/security/password": "text-orange-500",
  "/notifications": "text-blue-500",
  "/audit": "text-purple-500",
  "/settings/team": "text-pink-500",
  "/privacy": "text-green-500",
};
