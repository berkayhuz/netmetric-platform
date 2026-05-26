import { createCriticalRouteWarmupConfig } from "@netmetric/observability/performance";

export const accountCriticalRoutes = createCriticalRouteWarmupConfig([
  { href: "/profile", label: "Account overview" },
  { href: "/preferences", label: "Settings" },
  { href: "/security", label: "Security" },
  { href: "/settings/team", label: "Team settings" },
] as const);
