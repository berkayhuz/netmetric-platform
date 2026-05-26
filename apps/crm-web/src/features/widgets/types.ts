export type WidgetType = string;

export type WidgetCategory =
  | "core"
  | "sales"
  | "service_support"
  | "marketing"
  | "operations"
  | "intelligence_ai"
  | "administration"
  | "custom";

export type DashboardWidgetDataPoint = {
  label: string;
  value: string;
  detail?: string | null;
  tone?: "default" | "blue" | "amber" | "emerald" | "violet" | "cyan" | "rose" | "orange";
  href?: string | null;
};

export type DashboardGenericWidgetData = {
  kind: "metric" | "list" | "status";
  value?: string | number | null;
  label?: string | null;
  description?: string | null;
  tone?: "default" | "blue" | "amber" | "emerald" | "violet" | "cyan" | "rose" | "orange";
  href?: string | null;
  items?: DashboardWidgetDataPoint[];
};

export type RefreshIntervalSec = 15 | 30 | 60;

export type WidgetRefreshPolicy = {
  enabled: boolean;
  intervalSec: RefreshIntervalSec | null;
};

export type WidgetLayout = {
  x: number;
  y: number;
  w: number;
  h: number;
  minW: number;
  minH: number;
  maxW: number;
  maxH: number;
};

export type DashboardWidgetInstance = {
  id: string;
  widgetType: WidgetType;
  title: string;
  layout: WidgetLayout;
  config: Record<string, unknown>;
  refreshPolicy: WidgetRefreshPolicy;
};

export type DashboardShareRole = "viewer" | "editor";

export type DashboardShare = {
  principal: string;
  role: DashboardShareRole;
};

export type DashboardProfile = {
  id: string;
  name: string;
  isDefault: boolean;
  layoutVersion: number;
  widgets: DashboardWidgetInstance[];
  shares: DashboardShare[];
  deletedAt: string | null;
  purgeAt: string | null;
};

export type DashboardCollection = {
  active: DashboardProfile[];
  trash: DashboardProfile[];
  selectedDashboardId: string | null;
  updatedAt: string;
};
