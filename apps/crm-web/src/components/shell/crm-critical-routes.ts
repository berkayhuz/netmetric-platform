import { createCriticalRouteWarmupConfig } from "@netmetric/observability/performance";

export const crmCriticalRoutes = createCriticalRouteWarmupConfig([
  { href: "/dashboard", label: "Dashboard" },
  { href: "/customers", label: "Customers" },
  { href: "/product-catalog", label: "Product catalog" },
  { href: "/settings", label: "Settings" },
] as const);
