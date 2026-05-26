"use client";

import Link from "next/link";
import type { ChangeEvent, CSSProperties } from "react";
import { useEffect, useMemo, useRef, useState } from "react";
import {
  Responsive,
  WidthProvider,
  type Layout,
  type LayoutItem,
  type ResponsiveLayouts,
} from "react-grid-layout/legacy";
import { Badge, Button, Card, CardContent, CardHeader, CardTitle, Input } from "@netmetric/ui";
import { toast } from "@netmetric/ui/client";
import {
  DropdownMenu,
  DropdownMenuCheckboxItem,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";
import {
  MoreHorizontal,
  TrendingUp,
  Clock,
  Users,
  Building2,
  Contact,
  Activity,
  CheckSquare,
  DollarSign,
  Target,
  UserPlus,
  ChevronRight,
  ShieldAlert,
  Sparkles,
  Layers,
  Briefcase,
  CheckCircle2,
  Plus,
  RotateCcw,
  Settings2,
  Trash2,
  type LucideIcon,
} from "lucide-react";

import {
  createDefaultDashboardProfile,
  getWidgetCatalogItem,
} from "@/features/widgets/registry/widget-catalog";
import {
  addWidget,
  createDashboard,
  getMaxDashboardLimit,
  hydrateClientCollection,
  moveToTrash,
  removeDashboardShare,
  removeWidget,
  saveDashboardCollection,
  setDashboardShares,
  setDashboardDefault,
  setSelectedDashboard,
  updateDashboardWidgets,
} from "@/features/widgets/persistence/dashboard-storage";
import type {
  DashboardCollection,
  DashboardGenericWidgetData,
  DashboardWidgetInstance,
  WidgetType,
} from "@/features/widgets/types";

const DASHBOARD_NAME_EVENT = "crm-dashboard-name-changed";
const DASHBOARD_EDIT_TOGGLE_EVENT = "crm-dashboard-toggle-edit";
const DASHBOARD_MOVE_TO_TRASH_EVENT = "crm-dashboard-move-to-trash";
const DASHBOARD_META_EVENT = "crm-dashboard-meta-changed";
const DASHBOARD_EDIT_MODE_EVENT = "crm-dashboard-edit-mode-changed";
const DASHBOARD_CREATE_EVENT = "crm-dashboard-create";
const DASHBOARD_ADD_WIDGET_EVENT = "crm-dashboard-add-widget";
const DASHBOARD_SELECT_EVENT = "crm-dashboard-select";
const DASHBOARD_SET_DEFAULT_EVENT = "crm-dashboard-set-default";
const DASHBOARD_SET_SHARE_EVENT = "crm-dashboard-set-share";
const DASHBOARD_REMOVE_SHARE_EVENT = "crm-dashboard-remove-share";

type DashboardBuilderProps = {
  userId: string;
  initialCollection: DashboardCollection;
  seed: {
    customerTotal: number;
    companyTotal: number;
    contactTotal: number;
    recentCustomers: Array<{ id: string; name: string; subtitle: string }>;
    recentCompanies: Array<{ id: string; name: string; subtitle: string }>;
    recentContacts: Array<{ id: string; name: string; subtitle: string }>;
    dealsPipeline: {
      totalValue: number;
      weightedValue: number;
      totalDeals: number;
      weeklyDeltaPct: number;
      stages: Array<{
        name: "Lead" | "Contact" | "Proposal" | "Negotiate" | "Won";
        count: number;
        value: number;
      }>;
    };
    opportunities: {
      openValue: number;
      items: Array<{ id: string; name: string; value: number; stage: string; probability: number }>;
    };
    tasksDueToday: {
      total: number;
      completed: number;
      items: Array<{
        id: string;
        title: string;
        dueAtUtc: string;
        priority: number;
        done: boolean;
      }>;
    };
    slaRiskAlerts: {
      atRiskCount: number;
      items: Array<{ id: string; subject: string; severity: string; timeLeftLabel: string }>;
    };
    salesForecast: {
      quota: number;
      closedValue: number;
      pipelineCoverage: number;
      winProbabilityPct: number;
    };
    crmWidgets: Record<string, DashboardGenericWidgetData>;
  };
};

const ResponsiveGridLayout = WidthProvider(Responsive);
const GRID_BREAKPOINTS = { lg: 1280, md: 1024, sm: 768, xs: 480, xxs: 0 };
const GRID_COLS = { lg: 12, md: 10, sm: 6, xs: 4, xxs: 2 };
const ROW_HEIGHT = 32;
const WIDGET_MIN_H = 3;
const WIDGET_MAX_H = 8;
type KpiTrendRange = "7d" | "1m" | "6m";

const KPI_RANGE_OPTIONS: Array<{ value: KpiTrendRange; label: string }> = [
  { value: "7d", label: "Last 7 Days" },
  { value: "1m", label: "Last 1 Month" },
  { value: "6m", label: "Last 6 Months" },
];

function clampWidgetHeight(value: number, min = WIDGET_MIN_H, max = WIDGET_MAX_H): number {
  return Math.max(min, Math.min(max, value));
}

function fitToCols(value: number, cols: number): number {
  return Math.max(1, Math.min(cols, value));
}

function adaptLayoutForCols(widgets: DashboardWidgetInstance[], cols: number): LayoutItem[] {
  return widgets.map<LayoutItem>((widget) => {
    const catalog = getWidgetCatalogItem(widget.widgetType);
    const minW = fitToCols(catalog.minSize.w, cols);
    const maxW = fitToCols(catalog.maxSize.w, cols);
    const requestedW = fitToCols(widget.layout.w, cols);
    const w = Math.max(minW, Math.min(maxW, requestedW));
    const x = Math.max(0, Math.min(cols - w, widget.layout.x));
    const minH = clampWidgetHeight(catalog.minSize.h, WIDGET_MIN_H, catalog.maxSize.h);
    const maxH = Math.max(minH, Math.min(WIDGET_MAX_H, catalog.maxSize.h));

    return {
      i: widget.id,
      x,
      y: widget.layout.y,
      w,
      h: clampWidgetHeight(widget.layout.h, minH, maxH),
      minW,
      minH,
      maxW,
      maxH,
    };
  });
}

function overlaps(
  a: { x: number; y: number; w: number; h: number },
  b: { x: number; y: number; w: number; h: number },
): boolean {
  return a.x < b.x + b.w && a.x + a.w > b.x && a.y < b.y + b.h && a.y + a.h > b.y;
}

function packWidgetsLeftToRight(
  widgets: DashboardWidgetInstance[],
  cols: number,
): DashboardWidgetInstance[] {
  const placed: Array<{ id: string; x: number; y: number; w: number; h: number }> = [];

  return widgets.map((widget) => {
    const w = Math.max(1, Math.min(cols, widget.layout.w));
    const h = Math.max(1, widget.layout.h);
    let y = 0;
    let found = false;
    let x = 0;

    while (!found) {
      for (let candidateX = 0; candidateX <= cols - w; candidateX += 1) {
        const candidate = { x: candidateX, y, w, h };
        const collides = placed.some((item) => overlaps(candidate, item));
        if (!collides) {
          x = candidateX;
          found = true;
          break;
        }
      }
      if (!found) {
        y += 1;
      }
    }

    placed.push({ id: widget.id, x, y, w, h });

    return {
      ...widget,
      layout: {
        ...widget.layout,
        x,
        y,
      },
    };
  });
}

function packLayoutItemsLeftToRight(items: LayoutItem[], cols: number): LayoutItem[] {
  const placed: Array<{ x: number; y: number; w: number; h: number }> = [];

  return items.map((item) => {
    const w = Math.max(1, Math.min(cols, item.w));
    const h = Math.max(1, item.h);
    let y = 0;
    let found = false;
    let x = 0;

    while (!found) {
      for (let candidateX = 0; candidateX <= cols - w; candidateX += 1) {
        const candidate = { x: candidateX, y, w, h };
        const collides = placed.some((placedItem) => overlaps(candidate, placedItem));
        if (!collides) {
          x = candidateX;
          found = true;
          break;
        }
      }
      if (!found) {
        y += 1;
      }
    }

    const packed = { ...item, x, y, w, h };
    placed.push({ x, y, w, h });
    return packed;
  });
}

function buildPackedLayoutForCols(widgets: DashboardWidgetInstance[], cols: number): LayoutItem[] {
  const ordered = [...widgets].sort((a, b) => {
    if (a.layout.y !== b.layout.y) return a.layout.y - b.layout.y;
    if (a.layout.x !== b.layout.x) return a.layout.x - b.layout.x;
    return a.id.localeCompare(b.id);
  });
  const adapted = adaptLayoutForCols(ordered, cols);
  return packLayoutItemsLeftToRight(adapted, cols);
}

function toGridLayouts(widgets: DashboardWidgetInstance[]): ResponsiveLayouts {
  return {
    lg: buildPackedLayoutForCols(widgets, GRID_COLS.lg),
    md: buildPackedLayoutForCols(widgets, GRID_COLS.md),
    sm: buildPackedLayoutForCols(widgets, GRID_COLS.sm),
    xs: buildPackedLayoutForCols(widgets, GRID_COLS.xs),
    xxs: buildPackedLayoutForCols(widgets, GRID_COLS.xxs),
  };
}

function getWidgetTheme(type: WidgetType) {
  switch (type) {
    case "customers_kpi":
      return {
        icon: Users,
        color: "text-blue-500",
        bg: "bg-blue-500/10",
        border: "border-blue-500/20",
      };
    case "companies_kpi":
      return {
        icon: Building2,
        color: "text-amber-500",
        bg: "bg-amber-500/10",
        border: "border-amber-500/20",
      };
    case "contacts_kpi":
      return {
        icon: Contact,
        color: "text-purple-500",
        bg: "bg-purple-500/10",
        border: "border-purple-500/20",
      };
    case "deals_pipeline_summary":
      return {
        icon: Layers,
        color: "text-indigo-500",
        bg: "bg-indigo-500/10",
        border: "border-indigo-500/20",
      };
    case "opportunities_summary":
      return {
        icon: Briefcase,
        color: "text-emerald-500",
        bg: "bg-emerald-500/10",
        border: "border-emerald-500/20",
      };
    case "tasks_due_today":
      return {
        icon: CheckSquare,
        color: "text-violet-500",
        bg: "bg-violet-500/10",
        border: "border-violet-500/20",
      };
    case "ticket_sla_risk":
      return {
        icon: ShieldAlert,
        color: "text-rose-500",
        bg: "bg-rose-500/10",
        border: "border-rose-500/20",
      };
    case "recent_activities_feed":
      return {
        icon: Activity,
        color: "text-cyan-500",
        bg: "bg-cyan-500/10",
        border: "border-cyan-500/20",
      };
    case "forecast_snapshot":
      return {
        icon: Target,
        color: "text-orange-500",
        bg: "bg-orange-500/10",
        border: "border-orange-500/20",
      };
    case "quick_actions":
      return {
        icon: Sparkles,
        color: "text-violet-500",
        bg: "bg-violet-500/10",
        border: "border-violet-500/20",
      };
    default:
      return {
        icon: Activity,
        color: "text-muted-foreground",
        bg: "bg-muted/10",
        border: "border-muted/20",
      };
  }
}

function isKpiTrendWidget(type: WidgetType): boolean {
  return type === "customers_kpi" || type === "companies_kpi" || type === "contacts_kpi";
}

function getKpiTrendRange(widget: DashboardWidgetInstance): KpiTrendRange {
  const value = widget.config.kpiTrendRange;
  return value === "1m" || value === "6m" ? value : "7d";
}

function buildKpiSeries(total: number, range: KpiTrendRange, widgetType: WidgetType): number[] {
  const seedByType: Record<string, number> = {
    customers_kpi: 17,
    companies_kpi: 31,
    contacts_kpi: 47,
    deals_pipeline_summary: 59,
    opportunities_summary: 67,
    tasks_due_today: 73,
    ticket_sla_risk: 79,
    recent_activities_feed: 83,
    forecast_snapshot: 89,
    quick_actions: 97,
  };
  const count = range === "7d" ? 7 : range === "1m" ? 10 : 12;
  const base = Math.max(5, total);
  const seed = seedByType[widgetType] ?? 101;
  const points: number[] = [];

  for (let index = 0; index < count; index += 1) {
    const progress = index / (count - 1);
    const wave = Math.sin(progress * Math.PI * 2 * (range === "6m" ? 1.5 : 2) + seed * 0.09) * 0.06;
    const trend = (progress - 0.5) * (range === "7d" ? 0.06 : range === "1m" ? 0.1 : 0.16);
    const value = base * (1 + wave + trend);
    points.push(Math.max(1, Math.round(value)));
  }

  points[points.length - 1] = total;
  return points;
}

function buildSparklinePath(points: number[]): { areaPath: string; linePath: string } {
  const width = 100;
  const height = 30;
  const min = Math.min(...points);
  const max = Math.max(...points);
  const span = Math.max(1, max - min);
  const step = points.length > 1 ? width / (points.length - 1) : width;

  const normalized = points.map((point, index) => {
    const x = index * step;
    const y = height - ((point - min) / span) * (height - 4) - 2;
    return { x: Number(x.toFixed(2)), y: Number(y.toFixed(2)) };
  });

  const linePath = normalized
    .map((point, index) => `${index === 0 ? "M" : "L"}${point.x},${point.y}`)
    .join(" ");
  const last = normalized[normalized.length - 1];
  const areaPath = `${linePath} L${last?.x ?? width},${height} L0,${height} Z`;

  return { areaPath, linePath };
}

const QUICK_ACTION_ICON_OPTIONS = [
  { value: "customer", label: "Customer", icon: UserPlus },
  { value: "company", label: "Company", icon: Building2 },
  { value: "deal", label: "Deal", icon: DollarSign },
  { value: "task", label: "Task", icon: CheckSquare },
  { value: "contact", label: "Contact", icon: Contact },
  { value: "ticket", label: "Ticket", icon: ShieldAlert },
  { value: "activity", label: "Activity", icon: Activity },
  { value: "forecast", label: "Forecast", icon: Target },
] as const satisfies ReadonlyArray<{ value: string; label: string; icon: LucideIcon }>;

const QUICK_ACTION_TONES = {
  blue: "text-blue-500 bg-blue-500/10 border-blue-500/20",
  amber: "text-amber-500 bg-amber-500/10 border-amber-500/20",
  emerald: "text-emerald-500 bg-emerald-500/10 border-emerald-500/20",
  violet: "text-violet-500 bg-violet-500/10 border-violet-500/20",
  cyan: "text-cyan-500 bg-cyan-500/10 border-cyan-500/20",
  rose: "text-rose-500 bg-rose-500/10 border-rose-500/20",
  orange: "text-orange-500 bg-orange-500/10 border-orange-500/20",
} as const;

const QUICK_ACTION_TONE_TEXT = {
  blue: "text-blue-500",
  amber: "text-amber-500",
  emerald: "text-emerald-500",
  violet: "text-violet-500",
  cyan: "text-cyan-500",
  rose: "text-rose-500",
  orange: "text-orange-500",
} as const;

const MAX_QUICK_ACTIONS = 8;

type QuickActionIconKey = (typeof QUICK_ACTION_ICON_OPTIONS)[number]["value"];
type QuickActionTone = keyof typeof QUICK_ACTION_TONES;
type QuickActionConfig = {
  [key: string]: string;
  id: string;
  label: string;
  description: string;
  href: string;
  iconKey: QuickActionIconKey;
  tone: QuickActionTone;
};

const DEFAULT_QUICK_ACTIONS: QuickActionConfig[] = [
  {
    id: "new-customer",
    label: "New Customer",
    description: "Create customer profile",
    href: "/customers/new",
    iconKey: "customer",
    tone: "blue",
  },
  {
    id: "new-company",
    label: "New Company",
    description: "Add business account",
    href: "/companies/new",
    iconKey: "company",
    tone: "amber",
  },
  {
    id: "new-deal",
    label: "New Deal",
    description: "Open pipeline record",
    href: "/deals/new",
    iconKey: "deal",
    tone: "emerald",
  },
  {
    id: "new-task",
    label: "New Task",
    description: "Assign scheduled task",
    href: "/work-management",
    iconKey: "task",
    tone: "violet",
  },
];

function cloneDefaultQuickActions(): QuickActionConfig[] {
  return DEFAULT_QUICK_ACTIONS.map((action) => ({ ...action }));
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function readConfigString(record: Record<string, unknown>, key: string, fallback: string): string {
  const value = record[key];
  return typeof value === "string" && value.trim() ? value : fallback;
}

function isQuickActionIconKey(value: string): value is QuickActionIconKey {
  return QUICK_ACTION_ICON_OPTIONS.some((option) => option.value === value);
}

function isQuickActionTone(value: string): value is QuickActionTone {
  return value in QUICK_ACTION_TONES;
}

function normalizeQuickAction(value: unknown, index: number): QuickActionConfig | null {
  if (!isRecord(value)) {
    return null;
  }

  const fallback =
    DEFAULT_QUICK_ACTIONS[index % DEFAULT_QUICK_ACTIONS.length] ?? DEFAULT_QUICK_ACTIONS[0];
  if (!fallback) {
    return null;
  }
  const label = readConfigString(value, "label", fallback.label).slice(0, 48);
  const href = readConfigString(value, "href", fallback.href).slice(0, 160);
  const iconKeyCandidate = readConfigString(value, "iconKey", fallback.iconKey);
  const toneCandidate = readConfigString(value, "tone", fallback.tone);

  return {
    id: readConfigString(value, "id", `quick-action-${index + 1}`),
    label,
    description: readConfigString(value, "description", fallback.description).slice(0, 96),
    href,
    iconKey: isQuickActionIconKey(iconKeyCandidate) ? iconKeyCandidate : fallback.iconKey,
    tone: isQuickActionTone(toneCandidate) ? toneCandidate : fallback.tone,
  };
}

function getQuickActions(widget: DashboardWidgetInstance): QuickActionConfig[] {
  const rawActions = widget.config.quickActions;
  if (!Array.isArray(rawActions)) {
    return cloneDefaultQuickActions();
  }

  const actions = rawActions
    .map((action, index) => normalizeQuickAction(action, index))
    .filter((action): action is QuickActionConfig => action !== null);

  return actions.length ? actions : cloneDefaultQuickActions();
}

function createQuickAction(): QuickActionConfig {
  return {
    id: `quick-action-${crypto.randomUUID()}`,
    label: "New Link",
    description: "Describe the shortcut",
    href: "/dashboard",
    iconKey: "activity",
    tone: "cyan",
  };
}

function getQuickActionIcon(iconKey: QuickActionIconKey): LucideIcon {
  return QUICK_ACTION_ICON_OPTIONS.find((option) => option.value === iconKey)?.icon ?? Activity;
}

type QuickActionScenario = {
  columns: 2 | 3 | 4 | 5 | 6 | 8;
  gapClass: "gap-1" | "gap-1.5" | "gap-2";
  iconOnly: boolean;
  iconScale: "sm" | "md" | "lg" | "xl";
  linkDensity: "icon" | "compact" | "regular";
  contentPaddingClass?: "p-1" | "p-2" | "px-2 py-1.5" | "px-3 py-2.5";
  iconLabelGapClass?: "gap-2" | "gap-3";
  labelDescGapClass?: "mt-0.5" | "mt-1";
  showChevron: boolean;
  showDescriptions: boolean;
  showLabels: boolean;
  iconSurface: boolean;
};

const QUICK_ACTION_SCENARIOS: Record<
  `${2 | 3 | 4 | 5 | 6}x${4 | 5 | 6 | 7 | 8}`,
  QuickActionScenario
> = {
  "2x4": {
    columns: 4,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "2x5": {
    columns: 4,
    gapClass: "gap-1.5",
    iconOnly: true,
    iconScale: "md",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "2x6": {
    columns: 4,
    gapClass: "gap-1.5",
    iconOnly: true,
    iconScale: "md",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "2x7": {
    columns: 2,
    gapClass: "gap-1.5",
    iconOnly: true,
    iconScale: "md",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "2x8": {
    columns: 2,
    gapClass: "gap-1.5",
    iconOnly: true,
    iconScale: "md",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "3x4": {
    columns: 4,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "3x5": {
    columns: 4,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "3x6": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: false,
  },
  "3x7": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: false,
  },
  "3x8": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "md",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: true,
    showLabels: true,
    iconSurface: false,
  },

  "4x4": {
    columns: 8,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "4x5": {
    columns: 4,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "lg",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "4x6": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: false,
  },
  "4x7": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-2",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: true,
  },
  "4x8": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-2",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: true,
    showLabels: true,
    iconSurface: true,
  },

  "5x4": {
    columns: 8,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "lg",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: true,
  },
  "5x5": {
    columns: 4,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "p-2",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: true,
  },
  "5x6": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "px-2 py-1.5",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: false,
  },
  "5x7": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "px-2 py-1.5",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: true,
  },
  "5x8": {
    columns: 2,
    gapClass: "gap-1",
    iconOnly: false,
    iconScale: "sm",
    linkDensity: "icon",
    contentPaddingClass: "px-2 py-1.5",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: true,
    showLabels: true,
    iconSurface: true,
  },

  "6x4": {
    columns: 8,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "lg",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "6x5": {
    columns: 4,
    gapClass: "gap-1",
    iconOnly: true,
    iconScale: "lg",
    linkDensity: "icon",
    contentPaddingClass: "p-1",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: false,
    showDescriptions: false,
    showLabels: false,
    iconSurface: false,
  },
  "6x6": {
    columns: 2,
    gapClass: "gap-1.5",
    iconOnly: false,
    iconScale: "md",
    linkDensity: "compact",
    contentPaddingClass: "px-2 py-1.5",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: false,
  },
  "6x7": {
    columns: 2,
    gapClass: "gap-1.5",
    iconOnly: false,
    iconScale: "md",
    linkDensity: "compact",
    contentPaddingClass: "px-2 py-1.5",
    iconLabelGapClass: "gap-2",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: false,
    showLabels: true,
    iconSurface: true,
  },
  "6x8": {
    columns: 2,
    gapClass: "gap-1.5",
    iconOnly: false,
    iconScale: "md",
    linkDensity: "regular",
    contentPaddingClass: "px-3 py-2.5",
    iconLabelGapClass: "gap-3",
    labelDescGapClass: "mt-0.5",
    showChevron: true,
    showDescriptions: true,
    showLabels: true,
    iconSurface: true,
  },
};

function getQuickActionGridModel(widget: DashboardWidgetInstance) {
  const width = Math.max(2, Math.min(6, widget.layout.w)) as 2 | 3 | 4 | 5 | 6;
  const height = Math.max(4, Math.min(8, widget.layout.h)) as 4 | 5 | 6 | 7 | 8;
  const scenario = QUICK_ACTION_SCENARIOS[`${width}x${height}`];
  const isIconMatrix = scenario.iconOnly && scenario.columns >= 4;
  const columnClass = scenario.columns === 8 ? "grid-cols-8" : `grid-cols-${scenario.columns}`;
  const contentPaddingClass =
    scenario.contentPaddingClass ??
    (scenario.linkDensity === "icon"
      ? "p-1"
      : scenario.linkDensity === "compact"
        ? "px-2 py-1.5"
        : "px-3 py-2.5");
  const iconLabelGapClass =
    scenario.iconLabelGapClass ?? (scenario.linkDensity === "regular" ? "gap-3" : "gap-2");
  const labelDescGapClass = scenario.labelDescGapClass ?? "mt-0.5";
  const iconClass =
    scenario.iconScale === "sm"
      ? "size-3.5"
      : scenario.iconScale === "md"
        ? "size-4"
        : scenario.iconScale === "lg"
          ? "size-5"
          : "size-6";

  return {
    columnClass,
    gapClass: scenario.gapClass,
    iconClass,
    iconWrapClass: scenario.iconOnly
      ? "size-full min-h-0 border-0 bg-transparent"
      : scenario.linkDensity === "compact"
        ? "size-7"
        : "size-8",
    iconSurface: scenario.iconSurface,
    isIconMatrix,
    isIconOnly: scenario.iconOnly,
    linkClass:
      scenario.linkDensity === "icon"
        ? scenario.showLabels
          ? `min-h-0 items-center ${iconLabelGapClass} ${contentPaddingClass}`
          : `min-h-0 justify-center ${contentPaddingClass}`
        : scenario.linkDensity === "compact"
          ? `min-h-8 ${iconLabelGapClass} ${contentPaddingClass}`
          : `min-h-10 ${iconLabelGapClass} ${contentPaddingClass}`,
    showChevron: scenario.showChevron,
    showDescriptions: scenario.showDescriptions,
    showLabels: scenario.showLabels,
    labelDescGapClass,
    textClass: scenario.linkDensity === "regular" ? "text-xs" : "text-[11px]",
  };
}

type DealsPipelineScenario = {
  topContentPaddingClass: "pt-0" | "pt-0.5" | "pt-1" | "pt-2" | "pb-2";
  showLead: boolean;
  showContact: boolean;
  showProposal: boolean;
  showNegotiate: boolean;
  showWon: boolean;
  showWeightedPipeline: boolean;
  showTotalDeals: boolean;
  showWeeklyDelta: boolean;
};

type DealsPipelineWidth = 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12;
type DealsPipelineHeight = 3 | 4 | 5 | 6;
type DealsPipelineKey = `${DealsPipelineWidth}x${DealsPipelineHeight}`;

const DEALS_PIPELINE_SCENARIOS: Record<DealsPipelineKey, DealsPipelineScenario> = {
  "2x3": {
    topContentPaddingClass: "pt-0",
    showLead: false,
    showContact: false,
    showProposal: false,
    showNegotiate: false,
    showWon: false,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: false,
  },
  "2x4": {
    topContentPaddingClass: "pt-2",
    showLead: false,
    showContact: false,
    showProposal: false,
    showNegotiate: false,
    showWon: false,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: false,
  },
  "2x5": {
    topContentPaddingClass: "pt-2",
    showLead: false,
    showContact: false,
    showProposal: false,
    showNegotiate: false,
    showWon: false,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: false,
  },
  "2x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "3x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: false,
    showProposal: false,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: false,
  },
  "3x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: false,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: false,
    showWeeklyDelta: false,
  },
  "3x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: false,
  },
  "3x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "4x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: false,
    showProposal: false,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: false,
  },
  "4x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: false,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: false,
    showWeeklyDelta: false,
  },
  "4x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: false,
  },
  "4x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "5x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: false,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: false,
  },
  "5x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "5x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "5x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "6x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "6x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "6x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "6x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "7x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: false,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "7x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "7x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "7x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "8x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "8x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "8x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "8x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "9x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "9x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "9x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "9x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "10x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "10x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "10x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "10x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "11x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "11x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "11x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "11x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "12x3": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: false,
    showTotalDeals: false,
    showWeeklyDelta: true,
  },
  "12x4": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "12x5": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
  "12x6": {
    topContentPaddingClass: "pt-0",
    showLead: true,
    showContact: true,
    showProposal: true,
    showNegotiate: true,
    showWon: true,
    showWeightedPipeline: true,
    showTotalDeals: true,
    showWeeklyDelta: true,
  },
};

function getDealsPipelineScenario(widget: DashboardWidgetInstance): DealsPipelineScenario {
  const width = Math.max(2, Math.min(12, widget.layout.w)) as DealsPipelineWidth;
  const height = Math.max(3, Math.min(6, widget.layout.h)) as DealsPipelineHeight;
  return DEALS_PIPELINE_SCENARIOS[`${width}x${height}`];
}

const GENERIC_WIDGET_TONES: Record<NonNullable<DashboardGenericWidgetData["tone"]>, string> = {
  default: "text-foreground bg-muted/20 border-border/35",
  blue: "text-blue-500 bg-blue-500/10 border-blue-500/20",
  amber: "text-amber-500 bg-amber-500/10 border-amber-500/20",
  emerald: "text-emerald-500 bg-emerald-500/10 border-emerald-500/20",
  violet: "text-violet-500 bg-violet-500/10 border-violet-500/20",
  cyan: "text-cyan-500 bg-cyan-500/10 border-cyan-500/20",
  rose: "text-rose-500 bg-rose-500/10 border-rose-500/20",
  orange: "text-orange-500 bg-orange-500/10 border-orange-500/20",
};

function renderGenericWidgetBody(
  widget: DashboardWidgetInstance,
  seed: DashboardBuilderProps["seed"],
) {
  const catalog = getWidgetCatalogItem(widget.widgetType);
  const data = seed.crmWidgets[catalog.dataKey ?? widget.widgetType];

  if (!data) {
    return (
      <div className="flex h-full items-center justify-center rounded-lg border border-dashed border-border/45 text-xs text-muted-foreground">
        No live CRM data
      </div>
    );
  }

  if (data.kind === "metric") {
    const toneClass = GENERIC_WIDGET_TONES[data.tone ?? "default"];
    return (
      <div className="flex h-full flex-col justify-between">
        <div className="space-y-1">
          <p className="text-[10px] font-medium uppercase tracking-wide text-muted-foreground">
            {data.label ?? catalog.description}
          </p>
          <p className="text-3xl font-extrabold tracking-tight text-foreground">
            {data.value ?? 0}
          </p>
        </div>
        <div className="flex items-center justify-between gap-2">
          <span
            className={`rounded-full border px-2 py-0.5 text-[10px] font-semibold ${toneClass}`}
          >
            Live CRM
          </span>
          {data.href ? (
            <Link
              href={data.href}
              className="inline-flex items-center text-[10px] font-semibold text-primary hover:underline"
            >
              Open <ChevronRight className="size-3" />
            </Link>
          ) : null}
        </div>
      </div>
    );
  }

  const items = data.items ?? [];
  return (
    <div className="flex h-full min-h-0 flex-col justify-between gap-2">
      <div className="min-h-0 space-y-1.5 overflow-hidden">
        {items.length ? (
          items.slice(0, widget.layout.h >= 5 ? 5 : 3).map((item, index) => (
            <div
              key={`${item.label}-${index}`}
              className="flex items-center justify-between gap-2 rounded-lg border border-border/35 bg-background/30 px-2 py-1.5"
            >
              <div className="min-w-0">
                <p className="truncate text-xs font-semibold text-foreground">{item.label}</p>
                {item.detail ? (
                  <p className="truncate text-[10px] text-muted-foreground">{item.detail}</p>
                ) : null}
              </div>
              <span className="shrink-0 text-[11px] font-bold text-foreground">{item.value}</span>
            </div>
          ))
        ) : (
          <div className="flex h-full items-center justify-center rounded-lg border border-dashed border-border/45 text-xs text-muted-foreground">
            No live records
          </div>
        )}
      </div>
      {data.href ? (
        <Link
          href={data.href}
          className="ml-auto inline-flex items-center text-[10px] font-semibold text-primary hover:underline"
        >
          Open <ChevronRight className="size-3" />
        </Link>
      ) : null}
    </div>
  );
}

function renderWidgetBody(widget: DashboardWidgetInstance, seed: DashboardBuilderProps["seed"]) {
  const formatCurrency = (value: number) => `$${Math.round(value).toLocaleString()}`;
  switch (widget.widgetType) {
    case "customers_kpi": {
      const showSparkline = widget.layout.h > 3;
      const isNarrow = widget.layout.w <= 2;
      const range = getKpiTrendRange(widget);
      const points = buildKpiSeries(seed.customerTotal, range, widget.widgetType);
      const first = points[0] ?? seed.customerTotal;
      const last = points[points.length - 1] ?? seed.customerTotal;
      const trendPct = ((last - first) / Math.max(1, first)) * 100;
      const sparkline = buildSparklinePath(points);
      return (
        <div className="flex flex-col h-full justify-between">
          <div className="flex items-baseline justify-between mt-1">
            <div>
              <p
                className={`${isNarrow ? "text-2xl" : "text-3xl"} font-extrabold font-sans tracking-tight text-foreground`}
              >
                {seed.customerTotal}
              </p>
              <p className="text-[10px] text-muted-foreground mt-0.5">Active customers</p>
            </div>
            {widget.layout.w > 2 && (
              <span className="text-[10px] text-emerald-500 font-bold bg-emerald-500/10 px-2 py-0.5 rounded-full flex items-center gap-0.5 shrink-0 border border-emerald-500/15">
                <TrendingUp className="size-3" /> {trendPct >= 0 ? "+" : ""}
                {trendPct.toFixed(1)}%
              </span>
            )}
          </div>
          {showSparkline && (
            <div className="mt-auto">
              <svg
                className="w-full h-8 overflow-visible"
                viewBox="0 0 100 30"
                preserveAspectRatio="none"
              >
                <defs>
                  <linearGradient id="customer-kpi-grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="rgb(59, 130, 246)" stopOpacity="0.25" />
                    <stop offset="100%" stopColor="rgb(59, 130, 246)" stopOpacity="0" />
                  </linearGradient>
                </defs>
                <path d={sparkline.areaPath} fill="url(#customer-kpi-grad)" />
                <path
                  d={sparkline.linePath}
                  fill="none"
                  stroke="rgb(59, 130, 246)"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          )}
        </div>
      );
    }
    case "companies_kpi": {
      const showSparkline = widget.layout.h > 3;
      const isNarrow = widget.layout.w <= 2;
      const range = getKpiTrendRange(widget);
      const points = buildKpiSeries(seed.companyTotal, range, widget.widgetType);
      const first = points[0] ?? seed.companyTotal;
      const last = points[points.length - 1] ?? seed.companyTotal;
      const trendPct = ((last - first) / Math.max(1, first)) * 100;
      const sparkline = buildSparklinePath(points);
      return (
        <div className="flex flex-col h-full justify-between">
          <div className="flex items-baseline justify-between mt-1">
            <div>
              <p
                className={`${isNarrow ? "text-2xl" : "text-3xl"} font-extrabold font-sans tracking-tight text-foreground`}
              >
                {seed.companyTotal}
              </p>
              <p className="text-[10px] text-muted-foreground mt-0.5">Companies tracked</p>
            </div>
            {widget.layout.w > 2 && (
              <span className="text-[10px] text-emerald-500 font-bold bg-emerald-500/10 px-2 py-0.5 rounded-full flex items-center gap-0.5 shrink-0 border border-emerald-500/15">
                <TrendingUp className="size-3" /> {trendPct >= 0 ? "+" : ""}
                {trendPct.toFixed(1)}%
              </span>
            )}
          </div>
          {showSparkline && (
            <div className="mt-auto">
              <svg
                className="w-full h-8 overflow-visible"
                viewBox="0 0 100 30"
                preserveAspectRatio="none"
              >
                <defs>
                  <linearGradient id="company-kpi-grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="rgb(245, 158, 11)" stopOpacity="0.25" />
                    <stop offset="100%" stopColor="rgb(245, 158, 11)" stopOpacity="0" />
                  </linearGradient>
                </defs>
                <path d={sparkline.areaPath} fill="url(#company-kpi-grad)" />
                <path
                  d={sparkline.linePath}
                  fill="none"
                  stroke="rgb(245, 158, 11)"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          )}
        </div>
      );
    }
    case "contacts_kpi": {
      const showSparkline = widget.layout.h > 3;
      const isNarrow = widget.layout.w <= 2;
      const range = getKpiTrendRange(widget);
      const points = buildKpiSeries(seed.contactTotal, range, widget.widgetType);
      const first = points[0] ?? seed.contactTotal;
      const last = points[points.length - 1] ?? seed.contactTotal;
      const trendPct = ((last - first) / Math.max(1, first)) * 100;
      const sparkline = buildSparklinePath(points);
      return (
        <div className="flex flex-col h-full justify-between">
          <div className="flex items-baseline justify-between mt-1">
            <div>
              <p
                className={`${isNarrow ? "text-2xl" : "text-3xl"} font-extrabold font-sans tracking-tight text-foreground`}
              >
                {seed.contactTotal}
              </p>
              <p className="text-[10px] text-muted-foreground mt-0.5">Contacts profile</p>
            </div>
            {widget.layout.w > 2 && (
              <span className="text-[10px] text-emerald-500 font-bold bg-emerald-500/10 px-2 py-0.5 rounded-full flex items-center gap-0.5 shrink-0 border border-emerald-500/15">
                <TrendingUp className="size-3" /> {trendPct >= 0 ? "+" : ""}
                {trendPct.toFixed(1)}%
              </span>
            )}
          </div>
          {showSparkline && (
            <div className="mt-auto">
              <svg
                className="w-full h-8 overflow-visible"
                viewBox="0 0 100 30"
                preserveAspectRatio="none"
              >
                <defs>
                  <linearGradient id="contact-kpi-grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stopColor="rgb(139, 92, 246)" stopOpacity="0.25" />
                    <stop offset="100%" stopColor="rgb(139, 92, 246)" stopOpacity="0" />
                  </linearGradient>
                </defs>
                <path d={sparkline.areaPath} fill="url(#contact-kpi-grad)" />
                <path
                  d={sparkline.linePath}
                  fill="none"
                  stroke="rgb(139, 92, 246)"
                  strokeWidth="1.5"
                  strokeLinecap="round"
                />
              </svg>
            </div>
          )}
        </div>
      );
    }
    case "deals_pipeline_summary": {
      const scenario = getDealsPipelineScenario(widget);
      const stages = seed.dealsPipeline.stages.map((stage) => ({
        ...stage,
        valueLabel: formatCurrency(stage.value),
        color:
          stage.name === "Lead"
            ? "bg-blue-500"
            : stage.name === "Contact"
              ? "bg-amber-500"
              : stage.name === "Proposal"
                ? "bg-indigo-500"
                : stage.name === "Negotiate"
                  ? "bg-violet-500"
                  : "bg-emerald-500",
      }));
      const visibleStages = stages.filter((stage) => {
        if (stage.name === "Lead") return scenario.showLead;
        if (stage.name === "Contact") return scenario.showContact;
        if (stage.name === "Proposal") return scenario.showProposal;
        if (stage.name === "Negotiate") return scenario.showNegotiate;
        if (stage.name === "Won") return scenario.showWon;
        return true;
      });

      return (
        <div
          className={`flex flex-col h-full justify-between space-y-2 ${scenario.topContentPaddingClass}`}
        >
          <div className="flex justify-between items-baseline border-b border-border/10 pb-1.5">
            <div>
              <p className="text-xs text-muted-foreground">Active Deals Pipeline</p>
              <p className="text-lg font-bold font-sans tracking-tight text-foreground">
                {formatCurrency(seed.dealsPipeline.totalValue)}
              </p>
            </div>
            {scenario.showWeeklyDelta && (
              <span className="text-[10px] text-emerald-500 font-medium bg-emerald-500/10 px-2 py-0.5 rounded-full flex items-center gap-1 border border-emerald-500/15">
                <TrendingUp className="size-3" />{" "}
                {seed.dealsPipeline.weeklyDeltaPct >= 0 ? "+" : ""}
                {seed.dealsPipeline.weeklyDeltaPct.toFixed(1)}% vs last week
              </span>
            )}
          </div>

          <div
            className={`grid gap-1.5 ${visibleStages.length >= 5 ? "grid-cols-5" : visibleStages.length === 4 ? "grid-cols-4" : visibleStages.length === 3 ? "grid-cols-3" : "grid-cols-2"}`}
          >
            {visibleStages.map((stage) => (
              <div key={stage.name} className="flex flex-col space-y-0.5">
                <div className="h-1 w-full rounded-full bg-secondary/50 overflow-hidden">
                  <div
                    className={`h-full ${stage.color} rounded-full`}
                    style={{ width: `${Math.max(8, Math.min(100, stage.count * 10))}%` }}
                  />
                </div>
                <div className="text-[9px] truncate font-medium text-foreground">{stage.name}</div>
                <div className="text-[10px] font-semibold text-foreground font-mono">
                  {stage.valueLabel}
                </div>
                {(scenario.showWeightedPipeline || scenario.showTotalDeals) && (
                  <div className="text-[8px] text-muted-foreground">{stage.count} deals</div>
                )}
              </div>
            ))}
          </div>

          {(scenario.showWeightedPipeline || scenario.showTotalDeals) && (
            <div className="flex items-center justify-between text-[10px] text-muted-foreground border-t border-border/10 pt-1.5">
              {scenario.showWeightedPipeline ? (
                <span>
                  Weighted pipeline: <b>{formatCurrency(seed.dealsPipeline.weightedValue)}</b>
                </span>
              ) : (
                <span />
              )}
              {scenario.showTotalDeals ? (
                <span>
                  Total deals: <b>{seed.dealsPipeline.totalDeals}</b>
                </span>
              ) : (
                <span />
              )}
            </div>
          )}
        </div>
      );
    }
    case "opportunities_summary": {
      const opps = seed.opportunities.items.map((item) => ({
        id: item.id,
        name: item.name,
        value: formatCurrency(item.value),
        stage: item.stage,
        prob: `${item.probability.toFixed(0)}%`,
        color:
          item.probability >= 70
            ? "bg-emerald-500/10 text-emerald-500 border-emerald-500/20"
            : item.probability >= 40
              ? "bg-indigo-500/10 text-indigo-500 border-indigo-500/20"
              : "bg-blue-500/10 text-blue-500 border-blue-500/20",
      }));

      let displayLimit = 1;
      if (widget.layout.h === 4) displayLimit = 2;
      else if (widget.layout.h >= 5) displayLimit = 3;

      const oppsToRender = opps.slice(0, displayLimit);
      const isShort = widget.layout.h <= 3;

      return (
        <div className="flex flex-col h-full justify-between space-y-2">
          <div className="space-y-1.5">
            {oppsToRender.map((opp) => (
              <div
                key={opp.id}
                className="flex items-center justify-between p-2 rounded-lg border border-border/40 bg-card/30 hover:bg-card/50 transition-colors duration-200"
              >
                <div className="min-w-0 pr-2">
                  <p className="text-xs font-semibold text-foreground truncate">{opp.name}</p>
                  <div className="flex items-center gap-1.5 mt-0.5">
                    <span
                      className={`text-[9px] font-medium px-1.5 py-0.5 rounded border ${opp.color}`}
                    >
                      {opp.stage}
                    </span>
                    {widget.layout.w > 3 && (
                      <span className="text-[9px] text-muted-foreground font-mono">
                        Prob: {opp.prob}
                      </span>
                    )}
                  </div>
                </div>
                <div className="text-right shrink-0">
                  <p className="text-xs font-bold font-mono text-foreground">{opp.value}</p>
                </div>
              </div>
            ))}
          </div>

          {!isShort && (
            <div className="flex items-center justify-between text-[10px] text-muted-foreground border-t border-border/10 pt-1.5 mt-auto">
              <span>
                Open value: <b>{formatCurrency(seed.opportunities.openValue)}</b>
              </span>
              <Link
                href="/opportunities"
                className="text-primary hover:underline flex items-center"
              >
                View all <ChevronRight className="size-3" />
              </Link>
            </div>
          )}
        </div>
      );
    }
    case "tasks_due_today": {
      const tasks = seed.tasksDueToday.items.map((task) => {
        const due = new Date(task.dueAtUtc);
        const hours = due.getHours().toString().padStart(2, "0");
        const mins = due.getMinutes().toString().padStart(2, "0");
        return {
          id: task.id,
          title: task.title,
          time: `${hours}:${mins}`,
          priority: task.priority >= 3 ? "High" : task.priority === 2 ? "Medium" : "Low",
          color:
            task.priority >= 3
              ? "bg-rose-500"
              : task.priority === 2
                ? "bg-amber-500"
                : "bg-blue-500",
          done: task.done,
        };
      });

      let displayLimit = 1;
      if (widget.layout.h === 4) displayLimit = 2;
      else if (widget.layout.h >= 5) displayLimit = 3;

      const isShort = widget.layout.h <= 3;
      const tasksToRender = tasks.slice(0, isShort ? 1 : displayLimit);

      return (
        <div className="flex flex-col h-full justify-between space-y-2">
          <div className="space-y-1.5">
            {tasksToRender.map((task) => (
              <div
                key={task.id}
                className="flex items-start gap-2 p-2 rounded-lg border border-border/40 bg-card/30 hover:bg-card/50 transition-colors duration-200"
              >
                <div className="mt-0.5 shrink-0 cursor-pointer">
                  {task.done ? (
                    <CheckCircle2 className="size-4 text-emerald-500 fill-emerald-500/10" />
                  ) : (
                    <div className="size-4 rounded-full border-2 border-muted-foreground/30 hover:border-primary transition-colors" />
                  )}
                </div>
                <div className="min-w-0 flex-1">
                  <p
                    className={`text-xs font-medium leading-tight ${task.done ? "line-through text-muted-foreground/60" : "text-foreground"}`}
                  >
                    {task.title}
                  </p>
                  <div className="flex items-center gap-2 mt-1">
                    <span className="text-[9px] text-muted-foreground flex items-center gap-1">
                      <Clock className="size-3" /> {task.time}
                    </span>
                    <span className="text-[9px] font-medium flex items-center gap-1">
                      <span className={`size-1.5 rounded-full ${task.color} animate-pulse`} />
                      {task.priority}
                    </span>
                  </div>
                </div>
              </div>
            ))}
          </div>

          {!isShort && (
            <div className="flex items-center justify-between text-[10px] text-muted-foreground border-t border-border/10 pt-1.5 mt-auto">
              <span>
                Completed:{" "}
                <b>
                  {seed.tasksDueToday.completed} / {seed.tasksDueToday.total}
                </b>
              </span>
              <Link href="/tasks" className="text-primary hover:underline flex items-center">
                Task list <ChevronRight className="size-3" />
              </Link>
            </div>
          )}
        </div>
      );
    }
    case "ticket_sla_risk": {
      const tickets = seed.slaRiskAlerts.items.map((ticket) => ({
        id: ticket.id,
        subject: ticket.subject,
        time: ticket.timeLeftLabel,
        severity: ticket.severity,
        avatar: ticket.subject.slice(0, 2).toUpperCase(),
        avatarColor:
          ticket.severity.toLowerCase() === "high"
            ? "bg-rose-500/10 text-rose-500 border-rose-500/20"
            : "bg-amber-500/10 text-amber-500 border-amber-500/20",
      }));

      let displayLimit = 1;
      if (widget.layout.h === 4) displayLimit = 2;
      else if (widget.layout.h >= 5) displayLimit = 3;

      const ticketsToRender = tickets.slice(0, displayLimit);
      const isShort = widget.layout.h <= 3;

      return (
        <div className="flex flex-col h-full justify-between space-y-2">
          <div className="space-y-1.5">
            {ticketsToRender.map((ticket) => (
              <div
                key={ticket.id}
                className="flex items-center justify-between p-2 rounded-lg border border-border/40 bg-card/30 hover:bg-card/50 transition-colors duration-200"
              >
                <div className="flex items-center gap-2 min-w-0 pr-2">
                  <div
                    className={`size-7 rounded-full border text-[10px] font-bold flex items-center justify-center shrink-0 ${ticket.avatarColor}`}
                  >
                    {ticket.avatar}
                  </div>
                  <div className="min-w-0">
                    <p className="text-xs font-semibold text-foreground truncate">
                      {ticket.subject}
                    </p>
                    <span className="text-[9px] text-muted-foreground">
                      {ticket.severity} Priority
                    </span>
                  </div>
                </div>
                <div className="text-right shrink-0">
                  <span className="text-[10px] font-bold font-mono text-rose-500 bg-rose-500/10 border border-rose-500/20 px-2 py-0.5 rounded-full animate-pulse">
                    {ticket.time}
                  </span>
                </div>
              </div>
            ))}
          </div>

          {!isShort && (
            <div className="flex items-center justify-between text-[10px] text-muted-foreground border-t border-border/10 pt-1.5 mt-auto">
              <span>
                SLA breach risk:{" "}
                <b className="text-rose-500">{seed.slaRiskAlerts.atRiskCount} Tickets</b>
              </span>
              <Link href="/tickets" className="text-primary hover:underline flex items-center">
                Open Queue <ChevronRight className="size-3" />
              </Link>
            </div>
          )}
        </div>
      );
    }
    case "recent_activities_feed": {
      const activities = [];
      if (seed.recentCustomers?.[0]) {
        activities.push({
          id: `act-1-${seed.recentCustomers[0].id}`,
          title: "Customer Onboarded",
          desc: `Added ${seed.recentCustomers[0].name} to workspace`,
          time: "4m ago",
          icon: UserPlus,
          color: "bg-blue-500/10 text-blue-500 border-blue-500/20",
        });
      }
      if (seed.recentCompanies?.[0]) {
        activities.push({
          id: `act-2-${seed.recentCompanies[0].id}`,
          title: "Account Synced",
          desc: `Updated details for ${seed.recentCompanies[0].name}`,
          time: "1h ago",
          icon: Building2,
          color: "bg-amber-500/10 text-amber-500 border-amber-500/20",
        });
      }
      if (seed.recentContacts?.[0]) {
        activities.push({
          id: `act-3-${seed.recentContacts[0].id}`,
          title: "Contact Created",
          desc: `Linked contact ${seed.recentContacts[0].name}`,
          time: "3h ago",
          icon: Contact,
          color: "bg-purple-500/10 text-purple-500 border-purple-500/20",
        });
      }
      if (seed.recentCustomers?.[1]) {
        activities.push({
          id: `act-4-${seed.recentCustomers[1].id}`,
          title: "Deal Initialized",
          desc: `Created opportunity with ${seed.recentCustomers[1].name}`,
          time: "5h ago",
          icon: DollarSign,
          color: "bg-emerald-500/10 text-emerald-500 border-emerald-500/20",
        });
      }

      // Fallback if empty seed:
      if (activities.length === 0) {
        activities.push(
          {
            id: "f1",
            title: "Lead Ingested",
            desc: "Incoming webhook lead saved",
            time: "10m ago",
            icon: Sparkles,
            color: "bg-cyan-500/10 text-cyan-500 border-cyan-500/20",
          },
          {
            id: "f2",
            title: "Campaign Sent",
            desc: "Sent newsletter to 250 targets",
            time: "2h ago",
            icon: Activity,
            color: "bg-pink-500/10 text-pink-500 border-pink-500/20",
          },
        );
      }

      let displayLimit = 1;
      if (widget.layout.h === 4) displayLimit = 2;
      else if (widget.layout.h === 5) displayLimit = 3;
      else if (widget.layout.h >= 6) displayLimit = 4;

      const itemsToRender = activities.slice(0, displayLimit);

      return (
        <div className="relative pl-4 space-y-3.5 before:absolute before:left-2 before:top-1.5 before:bottom-1.5 before:w-0.5 before:bg-border/30">
          {itemsToRender.map((item) => {
            const Icon = item.icon;
            return (
              <div key={item.id} className="relative group">
                <div
                  className={`absolute -left-[23px] top-0.5 rounded-full border bg-card p-1 z-10 transition-transform duration-300 group-hover:scale-110 ${item.color}`}
                >
                  <Icon className="size-3" />
                </div>
                <div className="flex flex-col space-y-0.5 pl-2">
                  <div className="flex items-center justify-between">
                    <p className="text-xs font-semibold text-foreground">{item.title}</p>
                    <span className="text-[10px] text-muted-foreground">{item.time}</span>
                  </div>
                  <p className="text-[11px] text-muted-foreground leading-normal line-clamp-1">
                    {item.desc}
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      );
    }
    case "forecast_snapshot": {
      const quota = Math.max(1, seed.salesForecast.quota);
      const closed = Math.max(0, seed.salesForecast.closedValue);
      const progress = (closed / quota) * 100;
      const isShort = widget.layout.h <= 3;

      return (
        <div className="flex flex-col h-full justify-between space-y-3">
          {!isShort && (
            <div className="space-y-1">
              <div className="flex items-center justify-between">
                <span className="text-xs text-muted-foreground">Quota progress (Q2)</span>
                <span className="text-xs font-bold text-foreground">{progress.toFixed(0)}%</span>
              </div>
              <div className="h-2 w-full rounded-full bg-secondary/50 overflow-hidden">
                <div
                  className="h-full bg-gradient-to-r from-orange-500 to-amber-500 rounded-full"
                  style={{ width: `${progress}%` }}
                />
              </div>
              <div className="flex items-center justify-between text-[10px] text-muted-foreground">
                <span>
                  Closed: <b>{formatCurrency(closed)}</b>
                </span>
                <span>
                  Target: <b>{formatCurrency(quota)}</b>
                </span>
              </div>
            </div>
          )}

          <div
            className={`grid grid-cols-2 gap-2 ${!isShort ? "border-t border-b border-border/10 py-2.5" : ""}`}
          >
            <div className="space-y-0.5">
              <p className="text-[10px] text-muted-foreground">Pipeline coverage</p>
              <p className="text-sm font-bold font-sans tracking-tight text-foreground">
                {formatCurrency(seed.salesForecast.pipelineCoverage)}
              </p>
            </div>
            <div className="space-y-0.5">
              <p className="text-[10px] text-muted-foreground">Win Probability</p>
              <p className="text-sm font-bold font-sans tracking-tight text-foreground">
                {seed.salesForecast.winProbabilityPct.toFixed(1)}%
              </p>
            </div>
          </div>

          {!isShort && (
            <div className="flex items-center justify-between text-[10px] text-muted-foreground mt-auto">
              <span>
                Forecast gap:{" "}
                <b className="text-amber-500">-{formatCurrency(Math.max(0, quota - closed))}</b>
              </span>
            </div>
          )}
        </div>
      );
    }
    case "quick_actions": {
      const actions = getQuickActions(widget);
      const grid = getQuickActionGridModel(widget);

      return (
        <div className="h-full min-h-0 overflow-hidden">
          <div className={`grid h-full min-h-0 auto-rows-fr ${grid.columnClass} ${grid.gapClass}`}>
            {actions.map((action) => {
              const Icon = getQuickActionIcon(action.iconKey);
              return (
                <Link
                  key={action.id}
                  href={action.href}
                  title={grid.isIconOnly ? action.label : undefined}
                  className={`group flex h-full min-w-0 items-center rounded-xl border border-border/45 bg-background/35 text-left shadow-[inset_0_1px_0_rgb(255_255_255/0.04)] transition-colors duration-150 hover:border-primary/35 hover:bg-primary/5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring/40 ${grid.linkClass}`}
                >
                  <span
                    className={`flex shrink-0 items-center justify-center rounded-lg ${
                      grid.iconSurface ? "border" : "border-0 bg-transparent"
                    } ${grid.iconWrapClass} ${grid.iconSurface ? QUICK_ACTION_TONES[action.tone] : QUICK_ACTION_TONE_TEXT[action.tone]}`}
                  >
                    <Icon className={grid.iconClass} />
                  </span>
                  {grid.showLabels ? (
                    <span className="min-w-0 flex-1">
                      <span
                        className={`${grid.textClass} block truncate font-semibold text-foreground group-hover:text-primary`}
                      >
                        {action.label}
                      </span>
                      {grid.showDescriptions ? (
                        <span
                          className={`${grid.labelDescGapClass} block truncate text-[10px] leading-tight text-muted-foreground`}
                        >
                          {action.description}
                        </span>
                      ) : null}
                    </span>
                  ) : (
                    <span className="sr-only">{action.label}</span>
                  )}
                  {grid.showChevron ? (
                    <ChevronRight className="size-3.5 shrink-0 text-muted-foreground opacity-50 transition-opacity group-hover:opacity-100" />
                  ) : null}
                </Link>
              );
            })}
          </div>
        </div>
      );
    }
    default:
      if (getWidgetCatalogItem(widget.widgetType).renderKind !== "custom") {
        return renderGenericWidgetBody(widget, seed);
      }
      return (
        <p className="text-sm text-muted-foreground">
          {getWidgetCatalogItem(widget.widgetType).description}
        </p>
      );
  }
}

type QuickActionsConfiguratorProps = {
  widget: DashboardWidgetInstance;
  onAdd: (widgetId: string) => void;
  onChange: (widgetId: string, actionId: string, patch: Partial<QuickActionConfig>) => void;
  onRemove: (widgetId: string, actionId: string) => void;
  onReset: (widgetId: string) => void;
};

function QuickActionsConfigurator({
  widget,
  onAdd,
  onChange,
  onRemove,
  onReset,
}: QuickActionsConfiguratorProps) {
  const actions = getQuickActions(widget);
  const isAtActionLimit = actions.length >= MAX_QUICK_ACTIONS;

  return (
    <div className="flex min-h-0 flex-1 flex-col gap-3">
      <div className="min-h-0 flex-1 space-y-2 overflow-auto pr-1">
        {actions.map((action, index) => (
          <div
            key={action.id}
            className="rounded-xl border border-border/45 bg-background/55 p-3 shadow-sm"
          >
            <div className="mb-2 flex items-center justify-between gap-2">
              <Badge variant="outline" className="text-[10px]">
                Link {index + 1}
              </Badge>
              <Button
                type="button"
                variant="ghost"
                size="icon-xs"
                disabled={actions.length === 1}
                onClick={() => onRemove(widget.id, action.id)}
              >
                <Trash2 className="size-3.5" />
                <span className="sr-only">Remove action</span>
              </Button>
            </div>

            <div className="grid gap-2 md:grid-cols-2">
              <Input
                value={action.label}
                placeholder="Label"
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  onChange(widget.id, action.id, { label: event.target.value })
                }
              />
              <Input
                value={action.href}
                placeholder="/customers/new"
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  onChange(widget.id, action.id, { href: event.target.value })
                }
              />
              <Input
                className="md:col-span-2"
                value={action.description}
                placeholder="Short description"
                onChange={(event: ChangeEvent<HTMLInputElement>) =>
                  onChange(widget.id, action.id, { description: event.target.value })
                }
              />
              <Select
                value={action.iconKey}
                onValueChange={(value) =>
                  onChange(widget.id, action.id, { iconKey: value as QuickActionIconKey })
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {QUICK_ACTION_ICON_OPTIONS.map((option) => (
                    <SelectItem key={option.value} value={option.value}>
                      {option.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <Select
                value={action.tone}
                onValueChange={(value) =>
                  onChange(widget.id, action.id, { tone: value as QuickActionTone })
                }
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {(Object.keys(QUICK_ACTION_TONES) as QuickActionTone[]).map((tone) => (
                    <SelectItem key={tone} value={tone}>
                      {tone}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>
        ))}
      </div>

      <div className="grid gap-2 border-t border-border/40 pt-3 sm:grid-cols-2">
        <Button
          type="button"
          variant="secondary"
          size="sm"
          disabled={isAtActionLimit}
          onClick={() => onAdd(widget.id)}
        >
          <Plus className="size-3.5" />
          Add link ({actions.length}/{MAX_QUICK_ACTIONS})
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={() => onReset(widget.id)}>
          <RotateCcw className="size-3.5" />
          Reset defaults
        </Button>
      </div>
      {isAtActionLimit ? (
        <p className="text-center text-[11px] text-muted-foreground">
          Maximum {MAX_QUICK_ACTIONS} quick action links can be added.
        </p>
      ) : null}
    </div>
  );
}

function toDashboardTitle(name: string): string {
  if (name.trim().toLowerCase() === "dashboard") {
    return "Dashboard";
  }

  return name;
}

export function DashboardBuilder({ userId, initialCollection, seed }: DashboardBuilderProps) {
  const [collection, setCollection] = useState<DashboardCollection>(initialCollection);
  const syncTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [isEditMode, setIsEditMode] = useState(false);
  const [configuringQuickActionsWidgetId, setConfiguringQuickActionsWidgetId] = useState<
    string | null
  >(null);
  const [currentGridCols, setCurrentGridCols] = useState(GRID_COLS.lg);
  const [isClientMounted, setIsClientMounted] = useState(false);
  const [gridViewportHeight, setGridViewportHeight] = useState(0);
  const gridViewportRef = useRef<HTMLDivElement | null>(null);

  const selectedDashboard =
    collection.active.find((dashboard) => dashboard.id === collection.selectedDashboardId) ??
    collection.active[0] ??
    createDefaultDashboardProfile();
  const configuringQuickActionsWidget =
    selectedDashboard.widgets.find(
      (widget) =>
        widget.id === configuringQuickActionsWidgetId && widget.widgetType === "quick_actions",
    ) ?? null;

  const layouts = useMemo(
    () => toGridLayouts(selectedDashboard.widgets),
    [selectedDashboard.widgets],
  );
  const packedCurrentColsLayout = useMemo(() => {
    return buildPackedLayoutForCols(selectedDashboard.widgets, currentGridCols);
  }, [selectedDashboard.widgets, currentGridCols]);

  const maxGridRows = useMemo(() => {
    return Math.max(1, ...packedCurrentColsLayout.map((item) => Math.max(1, item.y + item.h)));
  }, [packedCurrentColsLayout]);
  const effectiveRowHeight = useMemo(() => {
    if (gridViewportHeight <= 0) {
      return ROW_HEIGHT;
    }
    const totalGap = Math.max(0, (maxGridRows - 1) * 12);
    const fitted = Math.floor((gridViewportHeight - totalGap) / maxGridRows);
    return Math.max(10, Math.min(ROW_HEIGHT, fitted));
  }, [gridViewportHeight, maxGridRows]);

  function persist(nextCollection: typeof collection) {
    setCollection(nextCollection);
    saveDashboardCollection(userId, nextCollection);
  }

  function removeWidgetFromDashboard(dashboardId: string, widgetId: string) {
    setCollection((previous) => {
      const next = removeWidget(previous, dashboardId, widgetId);
      saveDashboardCollection(userId, next);
      return next;
    });
  }

  useEffect(() => {
    let isCancelled = false;

    queueMicrotask(() => {
      if (isCancelled) {
        return;
      }

      const mergedCollection = hydrateClientCollection(userId, initialCollection);
      setCollection(mergedCollection);
      saveDashboardCollection(userId, mergedCollection);
      setIsClientMounted(true);
    });

    return () => {
      isCancelled = true;
    };
  }, [initialCollection, userId]);

  useEffect(() => {
    window.dispatchEvent(
      new CustomEvent(DASHBOARD_NAME_EVENT, {
        detail: { title: toDashboardTitle(selectedDashboard.name) },
      }),
    );
  }, [selectedDashboard.name]);

  useEffect(() => {
    window.dispatchEvent(
      new CustomEvent(DASHBOARD_META_EVENT, {
        detail: {
          activeCount: collection.active.length,
          maxCount: getMaxDashboardLimit(),
          canMoveToTrash: collection.active.length > 1,
          selectedDashboardId: selectedDashboard.id,
          dashboards: collection.active.map((dashboard) => ({
            id: dashboard.id,
            name: dashboard.name,
            shares: dashboard.shares,
          })),
        },
      }),
    );
  }, [collection.active, selectedDashboard.id]);

  useEffect(() => {
    function toggleEdit() {
      setConfiguringQuickActionsWidgetId(null);
      setIsEditMode((previous) => !previous);
    }

    window.addEventListener(DASHBOARD_EDIT_TOGGLE_EVENT, toggleEdit);
    return () => {
      window.removeEventListener(DASHBOARD_EDIT_TOGGLE_EVENT, toggleEdit);
    };
  }, []);

  useEffect(() => {
    window.dispatchEvent(
      new CustomEvent(DASHBOARD_EDIT_MODE_EVENT, {
        detail: { isEditMode },
      }),
    );
  }, [isEditMode]);

  useEffect(() => {
    function moveDashboardToTrash() {
      try {
        const next = moveToTrash(collection, selectedDashboard.id);
        setCollection(next);
        saveDashboardCollection(userId, next);
      } catch (error) {
        if (error instanceof Error && error.message === "AT_LEAST_ONE_DASHBOARD_REQUIRED") {
          toast.error("Cannot delete last dashboard", {
            description: "At least one dashboard must remain active.",
          });
        }
      }
    }

    window.addEventListener(DASHBOARD_MOVE_TO_TRASH_EVENT, moveDashboardToTrash);
    return () => {
      window.removeEventListener(DASHBOARD_MOVE_TO_TRASH_EVENT, moveDashboardToTrash);
    };
  }, [collection, selectedDashboard.id, userId]);

  useEffect(() => {
    function handleCreateDashboardEvent(event: Event) {
      const customEvent = event as CustomEvent<{ name?: string }>;
      const trimmedName = (customEvent.detail?.name ?? "").trim();
      if (!trimmedName) return;
      try {
        const next = createDashboard(collection, trimmedName);
        setCollection(next);
        saveDashboardCollection(userId, next);
      } catch (error) {
        if (error instanceof Error && error.message === "MAX_DASHBOARD_LIMIT_REACHED") {
          toast.error("Dashboard limit reached", {
            description: "A maximum of 10 dashboards is allowed per user.",
          });
        }
      }
    }

    function handleAddWidgetEvent(event: Event) {
      const customEvent = event as CustomEvent<{ widgetType?: WidgetType }>;
      const widgetType = customEvent.detail?.widgetType;
      if (!widgetType) return;

      const catalogItem = getWidgetCatalogItem(widgetType);
      const widget: DashboardWidgetInstance = {
        id: `${widgetType}_${crypto.randomUUID()}`,
        widgetType,
        title: catalogItem.title,
        layout: {
          x: 0,
          y: 23,
          w: catalogItem.defaultSize.w,
          h: catalogItem.defaultSize.h,
          minW: catalogItem.minSize.w,
          minH: catalogItem.minSize.h,
          maxW: catalogItem.maxSize.w,
          maxH: catalogItem.maxSize.h,
        },
        config: {},
        refreshPolicy: {
          enabled: catalogItem.defaultRefresh.enabled,
          intervalSec: catalogItem.defaultRefresh.intervalSec,
        },
      };

      const next = addWidget(collection, selectedDashboard.id, widget);
      setCollection(next);
      saveDashboardCollection(userId, next);
    }

    function handleSelectDashboardEvent(event: Event) {
      const customEvent = event as CustomEvent<{ dashboardId?: string }>;
      const dashboardId = customEvent.detail?.dashboardId;
      if (!dashboardId) return;
      const next = setSelectedDashboard(collection, dashboardId);
      setCollection(next);
      saveDashboardCollection(userId, next);
    }

    function handleSetDefaultDashboardEvent(event: Event) {
      const customEvent = event as CustomEvent<{ dashboardId?: string }>;
      const dashboardId = customEvent.detail?.dashboardId ?? selectedDashboard.id;
      const next = setDashboardDefault(collection, dashboardId);
      setCollection(next);
      saveDashboardCollection(userId, next);
    }

    function handleSetShareEvent(event: Event) {
      const customEvent = event as CustomEvent<{
        dashboardId?: string;
        principal?: string;
        role?: "viewer" | "editor";
      }>;
      const dashboardId = customEvent.detail?.dashboardId;
      const principal = customEvent.detail?.principal;
      const role = customEvent.detail?.role;
      if (!dashboardId || !principal || (role !== "viewer" && role !== "editor")) return;
      const next = setDashboardShares(collection, dashboardId, principal, role);
      setCollection(next);
      saveDashboardCollection(userId, next);
    }

    function handleRemoveShareEvent(event: Event) {
      const customEvent = event as CustomEvent<{ dashboardId?: string; principal?: string }>;
      const dashboardId = customEvent.detail?.dashboardId;
      const principal = customEvent.detail?.principal;
      if (!dashboardId || !principal) return;
      const next = removeDashboardShare(collection, dashboardId, principal);
      setCollection(next);
      saveDashboardCollection(userId, next);
    }

    window.addEventListener(DASHBOARD_CREATE_EVENT, handleCreateDashboardEvent);
    window.addEventListener(DASHBOARD_ADD_WIDGET_EVENT, handleAddWidgetEvent);
    window.addEventListener(DASHBOARD_SELECT_EVENT, handleSelectDashboardEvent);
    window.addEventListener(DASHBOARD_SET_DEFAULT_EVENT, handleSetDefaultDashboardEvent);
    window.addEventListener(DASHBOARD_SET_SHARE_EVENT, handleSetShareEvent);
    window.addEventListener(DASHBOARD_REMOVE_SHARE_EVENT, handleRemoveShareEvent);

    return () => {
      window.removeEventListener(DASHBOARD_CREATE_EVENT, handleCreateDashboardEvent);
      window.removeEventListener(DASHBOARD_ADD_WIDGET_EVENT, handleAddWidgetEvent);
      window.removeEventListener(DASHBOARD_SELECT_EVENT, handleSelectDashboardEvent);
      window.removeEventListener(DASHBOARD_SET_DEFAULT_EVENT, handleSetDefaultDashboardEvent);
      window.removeEventListener(DASHBOARD_SET_SHARE_EVENT, handleSetShareEvent);
      window.removeEventListener(DASHBOARD_REMOVE_SHARE_EVENT, handleRemoveShareEvent);
    };
  }, [collection, selectedDashboard.id, userId]);

  useEffect(() => {
    const viewportNode = gridViewportRef.current;
    if (!viewportNode) return;

    const observer = new ResizeObserver((entries) => {
      const entry = entries[0];
      if (!entry) return;
      const nextHeight = Math.floor(entry.contentRect.height);
      setGridViewportHeight(nextHeight);
    });
    observer.observe(viewportNode);

    return () => observer.disconnect();
  }, []);

  useEffect(() => {
    if (!isClientMounted) {
      return;
    }

    if (syncTimerRef.current) {
      clearTimeout(syncTimerRef.current);
    }

    syncTimerRef.current = setTimeout(() => {
      void fetch("/api/dashboard-preferences", {
        method: "PUT",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ collection }),
      }).then((response) => {
        if (!response.ok) {
          toast.error("Dashboard sync failed", {
            description: "Your changes were kept locally but could not be synced to server.",
          });
        }
      });
    }, 700);

    return () => {
      if (syncTimerRef.current) {
        clearTimeout(syncTimerRef.current);
      }
    };
  }, [collection, isClientMounted]);

  function handleLayoutChange(nextLayout: Layout) {
    if (!isEditMode) return;

    const widgets = selectedDashboard.widgets.map((widget) => {
      const catalog = getWidgetCatalogItem(widget.widgetType);
      const matched = nextLayout.find((layoutItem: LayoutItem) => layoutItem.i === widget.id);
      if (!matched) return widget;
      const minH = clampWidgetHeight(catalog.minSize.h, WIDGET_MIN_H, catalog.maxSize.h);
      const maxH = Math.max(minH, Math.min(WIDGET_MAX_H, catalog.maxSize.h));

      return {
        ...widget,
        layout: {
          ...widget.layout,
          x: matched.x,
          y: matched.y,
          w: Math.max(catalog.minSize.w, Math.min(catalog.maxSize.w, matched.w)),
          h: clampWidgetHeight(matched.h, minH, maxH),
          minW: catalog.minSize.w,
          maxW: catalog.maxSize.w,
          minH,
          maxH,
        },
      };
    });

    const widgetOrderByGrid = [...widgets].sort((a, b) => {
      if (a.layout.y !== b.layout.y) return a.layout.y - b.layout.y;
      if (a.layout.x !== b.layout.x) return a.layout.x - b.layout.x;
      return a.id.localeCompare(b.id);
    });
    const packedWidgets = packWidgetsLeftToRight(widgetOrderByGrid, GRID_COLS.lg);

    persist(updateDashboardWidgets(collection, selectedDashboard.id, packedWidgets));
  }

  function handleToggleWidgetRefresh(widgetId: string, checked: boolean) {
    const widgets = selectedDashboard.widgets.map((widget) => {
      if (widget.id !== widgetId) return widget;
      const catalog = getWidgetCatalogItem(widget.widgetType);
      return {
        ...widget,
        refreshPolicy: {
          enabled: checked,
          intervalSec: checked
            ? (widget.refreshPolicy.intervalSec ?? catalog.defaultRefresh.intervalSec ?? 30)
            : null,
        },
      };
    });

    persist(updateDashboardWidgets(collection, selectedDashboard.id, widgets));
  }

  function handleSetKpiTrendRange(widgetId: string, range: KpiTrendRange) {
    const widgets = selectedDashboard.widgets.map((widget) => {
      if (widget.id !== widgetId || !isKpiTrendWidget(widget.widgetType)) return widget;
      return {
        ...widget,
        config: {
          ...widget.config,
          kpiTrendRange: range,
        },
      };
    });
    persist(updateDashboardWidgets(collection, selectedDashboard.id, widgets));
  }

  function handleUpdateQuickAction(
    widgetId: string,
    actionId: string,
    patch: Partial<QuickActionConfig>,
  ) {
    const widgets = selectedDashboard.widgets.map((widget) => {
      if (widget.id !== widgetId || widget.widgetType !== "quick_actions") return widget;
      const quickActions = getQuickActions(widget).map((action) =>
        action.id === actionId ? { ...action, ...patch } : action,
      );

      return {
        ...widget,
        config: {
          ...widget.config,
          quickActions,
        },
      };
    });

    persist(updateDashboardWidgets(collection, selectedDashboard.id, widgets));
  }

  function handleAddQuickAction(widgetId: string) {
    const widgets = selectedDashboard.widgets.map((widget) => {
      if (widget.id !== widgetId || widget.widgetType !== "quick_actions") return widget;
      const quickActions = getQuickActions(widget);
      if (quickActions.length >= MAX_QUICK_ACTIONS) {
        return widget;
      }

      return {
        ...widget,
        config: {
          ...widget.config,
          quickActions: [...quickActions, createQuickAction()],
        },
      };
    });

    persist(updateDashboardWidgets(collection, selectedDashboard.id, widgets));
  }

  function handleRemoveQuickAction(widgetId: string, actionId: string) {
    const widgets = selectedDashboard.widgets.map((widget) => {
      if (widget.id !== widgetId || widget.widgetType !== "quick_actions") return widget;
      const quickActions = getQuickActions(widget).filter((action) => action.id !== actionId);

      return {
        ...widget,
        config: {
          ...widget.config,
          quickActions: quickActions.length ? quickActions : getQuickActions(widget),
        },
      };
    });

    persist(updateDashboardWidgets(collection, selectedDashboard.id, widgets));
  }

  function handleResetQuickActions(widgetId: string) {
    const widgets = selectedDashboard.widgets.map((widget) => {
      if (widget.id !== widgetId || widget.widgetType !== "quick_actions") return widget;

      return {
        ...widget,
        config: {
          ...widget.config,
          quickActions: cloneDefaultQuickActions(),
        },
      };
    });

    persist(updateDashboardWidgets(collection, selectedDashboard.id, widgets));
  }

  return (
    <div>
      {isClientMounted ? (
        <div ref={gridViewportRef} className="h-[calc(100vh-12rem)] overflow-hidden">
          <ResponsiveGridLayout
            className={`crm-dashboard-grid-canvas layout h-full overflow-hidden ${isEditMode ? "is-editing" : ""}`}
            style={
              {
                "--crm-dashboard-cols": currentGridCols,
                "--crm-dashboard-gap": "12px",
                "--crm-dashboard-row-height": `${effectiveRowHeight}px`,
                "--crm-dashboard-cell-step-x": `${100 / currentGridCols}%`,
                "--crm-dashboard-cell-step-y": `${effectiveRowHeight + 12}px`,
              } as CSSProperties
            }
            layouts={layouts}
            breakpoints={GRID_BREAKPOINTS}
            cols={GRID_COLS}
            rowHeight={effectiveRowHeight}
            isDraggable={isEditMode}
            isResizable={isEditMode}
            resizeHandles={["e", "s", "se"]}
            compactType="vertical"
            preventCollision={false}
            margin={[12, 12]}
            useCSSTransforms
            onBreakpointChange={(_breakpoint: string, cols: number) => setCurrentGridCols(cols)}
            onLayoutChange={(layout: Layout) => handleLayoutChange(layout)}
            draggableHandle=".widget-drag-handle"
            draggableCancel=".widget-menu, .widget-menu *"
          >
            {selectedDashboard.widgets.map((widget) => {
              const theme = getWidgetTheme(widget.widgetType);
              const WidgetIcon = theme.icon;

              return (
                <Card
                  key={widget.id}
                  className="relative flex h-full py-0 min-h-0 flex-col overflow-hidden border border-border/40 bg-card/30 backdrop-blur-md hover:bg-card/45 hover:border-primary/20 transition-all duration-300 shadow-md hover:shadow-lg"
                >
                  <CardHeader
                    className={`widget-drag-handle flex flex-row items-center justify-between space-y-0 border-b border-border/10 px-4 py-3 hover:bg-muted/10 transition-colors ${
                      isEditMode ? "cursor-grab active:cursor-grabbing" : "cursor-default"
                    }`}
                  >
                    <div className="flex items-center gap-2.5 min-w-0">
                      <div
                        className={`flex items-center justify-center p-1.5 rounded-md border ${theme.bg} ${theme.border} shrink-0`}
                      >
                        <WidgetIcon className={`size-3.5 ${theme.color}`} />
                      </div>
                      <div className="min-w-0">
                        <CardTitle className="truncate text-xs font-bold tracking-wider uppercase text-muted-foreground/80">
                          {widget.title}
                        </CardTitle>
                        <div className="mt-0.5 flex items-center gap-1 text-[9px] text-muted-foreground/60">
                          <Clock className="size-2.5" />
                          {widget.refreshPolicy.enabled
                            ? `Auto ${widget.refreshPolicy.intervalSec ?? "-"}s`
                            : "Manual"}
                        </div>
                      </div>
                    </div>
                    {isEditMode ? (
                      <div
                        className="widget-menu"
                        onMouseDown={(event) => event.stopPropagation()}
                        onTouchStart={(event) => event.stopPropagation()}
                      >
                        <DropdownMenu>
                          <DropdownMenuTrigger>
                            <span className="inline-flex h-8 w-8 shrink-0 items-center justify-center rounded-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground">
                              <MoreHorizontal className="size-4" aria-hidden="true" />
                              <span className="sr-only">Widget options</span>
                            </span>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end" className="w-56">
                            {isKpiTrendWidget(widget.widgetType) ? (
                              <>
                                {KPI_RANGE_OPTIONS.map((option) => (
                                  <DropdownMenuCheckboxItem
                                    key={`${widget.id}-${option.value}`}
                                    checked={getKpiTrendRange(widget) === option.value}
                                    onCheckedChange={(checked) => {
                                      if (checked === true) {
                                        handleSetKpiTrendRange(widget.id, option.value);
                                      }
                                    }}
                                  >
                                    {option.label}
                                  </DropdownMenuCheckboxItem>
                                ))}
                                <DropdownMenuSeparator />
                              </>
                            ) : null}
                            {widget.widgetType === "quick_actions" ? (
                              <>
                                <DropdownMenuItem
                                  onClick={() => setConfiguringQuickActionsWidgetId(widget.id)}
                                >
                                  <Settings2 className="size-4" />
                                  Configure actions
                                </DropdownMenuItem>
                                <DropdownMenuSeparator />
                              </>
                            ) : null}
                            <DropdownMenuCheckboxItem
                              checked={widget.refreshPolicy.enabled}
                              onCheckedChange={(checked) =>
                                handleToggleWidgetRefresh(widget.id, checked === true)
                              }
                            >
                              Auto-refresh
                            </DropdownMenuCheckboxItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              className="text-destructive focus:text-destructive"
                              onClick={() =>
                                removeWidgetFromDashboard(selectedDashboard.id, widget.id)
                              }
                            >
                              Remove
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </div>
                    ) : null}
                  </CardHeader>
                  <CardContent
                    className={`min-h-0 flex-1 overflow-hidden ${
                      widget.widgetType === "quick_actions" ? "p-3 pt-0" : "p-4 pt-0"
                    }`}
                  >
                    {renderWidgetBody(widget, seed)}
                  </CardContent>
                </Card>
              );
            })}
          </ResponsiveGridLayout>
        </div>
      ) : (
        <div className="min-h-[68vh] rounded-xl border border-border/40 bg-card/20" />
      )}

      <Dialog
        open={Boolean(configuringQuickActionsWidget)}
        onOpenChange={(open) => {
          if (!open) {
            setConfiguringQuickActionsWidgetId(null);
          }
        }}
      >
        <DialogContent className="h-[min(760px,86vh)] max-w-[min(920px,calc(100%-2rem))] grid-rows-[auto_minmax(0,1fr)] overflow-hidden p-0 sm:max-w-[min(920px,calc(100%-2rem))]">
          <DialogHeader className="border-b border-border/45 bg-muted/25 px-5 py-4 pr-14">
            <DialogTitle>Configure Quick Actions</DialogTitle>
            <DialogDescription>
              Edit labels, destinations, icons, and visual tone. The widget will automatically
              simplify these links as it gets smaller.
            </DialogDescription>
          </DialogHeader>
          <div className="flex min-h-0 overflow-hidden p-4">
            {configuringQuickActionsWidget ? (
              <QuickActionsConfigurator
                widget={configuringQuickActionsWidget}
                onAdd={handleAddQuickAction}
                onChange={handleUpdateQuickAction}
                onRemove={handleRemoveQuickAction}
                onReset={handleResetQuickActions}
              />
            ) : null}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
