"use client";

import { useEffect, useState } from "react";
import { Pencil, Share2, Trash2, Mail, UserPlus, Users } from "lucide-react";
import { Badge, Button, Input, cn } from "@netmetric/ui";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import { getWidgetCatalogItem, widgetCatalog } from "@/features/widgets/registry/widget-catalog";
import type {
  DashboardShare,
  DashboardShareRole,
  WidgetCategory,
  WidgetType,
} from "@/features/widgets/types";

const DASHBOARD_NAME_EVENT = "crm-dashboard-name-changed";
export const DASHBOARD_EDIT_TOGGLE_EVENT = "crm-dashboard-toggle-edit";
export const DASHBOARD_MOVE_TO_TRASH_EVENT = "crm-dashboard-move-to-trash";
export const DASHBOARD_META_EVENT = "crm-dashboard-meta-changed";
export const DASHBOARD_EDIT_MODE_EVENT = "crm-dashboard-edit-mode-changed";
export const DASHBOARD_CREATE_EVENT = "crm-dashboard-create";
export const DASHBOARD_ADD_WIDGET_EVENT = "crm-dashboard-add-widget";
export const DASHBOARD_SELECT_EVENT = "crm-dashboard-select";
export const DASHBOARD_SET_DEFAULT_EVENT = "crm-dashboard-set-default";
export const DASHBOARD_SET_SHARE_EVENT = "crm-dashboard-set-share";
export const DASHBOARD_REMOVE_SHARE_EVENT = "crm-dashboard-remove-share";

type DashboardNameChangedDetail = {
  title: string;
};

type DashboardMetaChangedDetail = {
  activeCount: number;
  maxCount: number;
  canMoveToTrash: boolean;
  selectedDashboardId: string;
  dashboards: Array<{ id: string; name: string; shares: DashboardShare[] }>;
};

const WIDGET_CATEGORY_LABELS: Record<WidgetCategory, string> = {
  administration: "Administration",
  core: "Core CRM",
  custom: "Custom",
  intelligence_ai: "Intelligence / AI",
  marketing: "Marketing",
  operations: "Operations",
  sales: "Sales",
  service_support: "Service / Support",
};

const WIDGET_CATEGORIES: WidgetCategory[] = [
  "core",
  "sales",
  "service_support",
  "marketing",
  "operations",
  "intelligence_ai",
  "administration",
  "custom",
];

export function DashboardTitleControl({ initialTitle }: Readonly<{ initialTitle: string }>) {
  const [title, setTitle] = useState(initialTitle);

  useEffect(() => {
    function handleNameChanged(event: Event) {
      const customEvent = event as CustomEvent<DashboardNameChangedDetail>;
      if (customEvent.detail?.title) {
        setTitle(customEvent.detail.title);
      }
    }

    window.addEventListener(DASHBOARD_NAME_EVENT, handleNameChanged as EventListener);
    return () => {
      window.removeEventListener(DASHBOARD_NAME_EVENT, handleNameChanged as EventListener);
    };
  }, []);

  return <span>{title}</span>;
}

export function DashboardHeaderActions() {
  const [activeCount, setActiveCount] = useState(1);
  const [maxCount, setMaxCount] = useState(10);
  const [canMoveToTrash, setCanMoveToTrash] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);
  const [newDashboardName, setNewDashboardName] = useState("");
  const [selectedWidgetType, setSelectedWidgetType] = useState<WidgetType>("tasks_due_today");
  const [selectedDashboardId, setSelectedDashboardId] = useState("");
  const [dashboards, setDashboards] = useState<
    Array<{ id: string; name: string; shares: DashboardShare[] }>
  >([]);
  const [isShareDialogOpen, setIsShareDialogOpen] = useState(false);
  const [sharePrincipal, setSharePrincipal] = useState("");
  const [shareRole, setShareRole] = useState<DashboardShareRole>("viewer");

  useEffect(() => {
    function handleMetaChanged(event: Event) {
      const customEvent = event as CustomEvent<DashboardMetaChangedDetail>;
      if (!customEvent.detail) return;
      setActiveCount(customEvent.detail.activeCount);
      setMaxCount(customEvent.detail.maxCount);
      setCanMoveToTrash(customEvent.detail.canMoveToTrash);
      setSelectedDashboardId(customEvent.detail.selectedDashboardId);
      setDashboards(customEvent.detail.dashboards ?? []);
    }

    window.addEventListener(DASHBOARD_META_EVENT, handleMetaChanged as EventListener);
    return () => {
      window.removeEventListener(DASHBOARD_META_EVENT, handleMetaChanged as EventListener);
    };
  }, []);

  const selectedDashboard = dashboards.find((item) => item.id === selectedDashboardId);
  const shares = selectedDashboard?.shares ?? [];

  useEffect(() => {
    function handleEditModeChanged(event: Event) {
      const customEvent = event as CustomEvent<{ isEditMode: boolean }>;
      setIsEditMode(customEvent.detail?.isEditMode === true);
    }

    window.addEventListener(DASHBOARD_EDIT_MODE_EVENT, handleEditModeChanged as EventListener);
    return () => {
      window.removeEventListener(DASHBOARD_EDIT_MODE_EVENT, handleEditModeChanged as EventListener);
    };
  }, []);

  return (
    <div className="inline-flex items-center gap-2">
      {isEditMode ? (
        <>
          <div className="hidden items-center gap-2 xl:inline-flex">
            <Select
              value={selectedDashboardId}
              onValueChange={(value) => {
                if (!value) return;
                window.dispatchEvent(
                  new CustomEvent(DASHBOARD_SELECT_EVENT, { detail: { dashboardId: value } }),
                );
              }}
            >
              <SelectTrigger className="h-8 w-44">
                <SelectValue>
                  {dashboards.find((item) => item.id === selectedDashboardId)?.name ?? "Dashboard"}
                </SelectValue>
              </SelectTrigger>
              <SelectContent>
                {dashboards.map((dashboard) => (
                  <SelectItem key={dashboard.id} value={dashboard.id}>
                    {dashboard.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button
              type="button"
              size="sm"
              variant="outline"
              onClick={() => {
                if (!selectedDashboardId) return;
                window.dispatchEvent(
                  new CustomEvent(DASHBOARD_SET_DEFAULT_EVENT, {
                    detail: { dashboardId: selectedDashboardId },
                  }),
                );
              }}
            >
              Set Default
            </Button>
          </div>

          <div className="hidden items-center gap-2 xl:inline-flex">
            <Input
              className="h-8 w-44"
              placeholder="New dashboard name"
              value={newDashboardName}
              onChange={(event) => setNewDashboardName(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter") {
                  const name = newDashboardName.trim();
                  if (!name) return;
                  window.dispatchEvent(
                    new CustomEvent(DASHBOARD_CREATE_EVENT, { detail: { name } }),
                  );
                  setNewDashboardName("");
                }
              }}
            />
            <Button
              type="button"
              size="sm"
              onClick={() => {
                const name = newDashboardName.trim();
                if (!name) return;
                window.dispatchEvent(new CustomEvent(DASHBOARD_CREATE_EVENT, { detail: { name } }));
                setNewDashboardName("");
              }}
            >
              Create
            </Button>
          </div>

          <div className="hidden items-center gap-2 xl:inline-flex">
            <Select
              value={selectedWidgetType}
              onValueChange={(value) => setSelectedWidgetType(value as WidgetType)}
            >
              <SelectTrigger className="h-8 w-44">
                <SelectValue>{getWidgetCatalogItem(selectedWidgetType).title}</SelectValue>
              </SelectTrigger>
              <SelectContent>
                {WIDGET_CATEGORIES.map((category) => {
                  const widgets = widgetCatalog.filter((widget) => widget.category === category);
                  if (!widgets.length) return null;
                  return (
                    <SelectGroup key={category}>
                      <SelectLabel>{WIDGET_CATEGORY_LABELS[category]}</SelectLabel>
                      {widgets.map((widget) => (
                        <SelectItem key={widget.type} value={widget.type}>
                          {widget.title}
                        </SelectItem>
                      ))}
                    </SelectGroup>
                  );
                })}
              </SelectContent>
            </Select>
            <Button
              type="button"
              size="sm"
              variant="secondary"
              onClick={() => {
                window.dispatchEvent(
                  new CustomEvent(DASHBOARD_ADD_WIDGET_EVENT, {
                    detail: { widgetType: selectedWidgetType },
                  }),
                );
              }}
            >
              Add Widget
            </Button>
          </div>

          <span className="min-w-12 text-center text-xs font-medium text-muted-foreground">
            {activeCount}/{maxCount}
          </span>
        </>
      ) : null}
      <Button
        type="button"
        size="icon-sm"
        variant="outline"
        onClick={() => {
          window.dispatchEvent(new CustomEvent(DASHBOARD_EDIT_TOGGLE_EVENT));
        }}
        aria-label="Edit dashboard"
      >
        <Pencil aria-hidden="true" className="size-3.5" />
      </Button>

      {isEditMode ? (
        <>
          <Button
            type="button"
            size="icon-sm"
            variant="outline"
            disabled={!canMoveToTrash}
            onClick={() => {
              window.dispatchEvent(new CustomEvent(DASHBOARD_MOVE_TO_TRASH_EVENT));
            }}
            aria-label="Move dashboard to trash"
          >
            <Trash2 aria-hidden="true" className="size-3.5" />
          </Button>

          <Button
            type="button"
            size="icon-sm"
            variant="outline"
            aria-label="Share dashboard"
            onClick={() => setIsShareDialogOpen(true)}
          >
            <Share2 aria-hidden="true" className="size-3.5" />
          </Button>
        </>
      ) : null}

      <Dialog open={isShareDialogOpen} onOpenChange={setIsShareDialogOpen}>
        <DialogContent className="max-h-[min(760px,86vh)] w-full max-w-[calc(100%-2rem)] sm:max-w-[540px] md:max-w-[680px] overflow-hidden p-0 sm:max-w-[min(680px,calc(100%-2rem))]">
          <DialogHeader className="border-b border-border/45 bg-muted/25 px-5 py-4 pr-14">
            <DialogTitle className="flex items-center gap-2 text-base font-semibold">
              <Share2 className="size-4.5 text-primary" />
              <span>Share Dashboard</span>
            </DialogTitle>
            <DialogDescription>
              Grant view or edit access to other members of your team.
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-5 p-5">
            {/* Dashboard Selector Section */}
            <div className="bg-muted/30 border border-border/50 rounded-lg p-3.5 flex items-center justify-between gap-4">
              <div className="space-y-0.5">
                <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Select Dashboard
                </h4>
                <p className="text-[11px] text-muted-foreground/80">
                  Configure permissions for this specific dashboard
                </p>
              </div>
              <Select
                value={selectedDashboardId}
                onValueChange={(value) => {
                  if (!value) return;
                  window.dispatchEvent(
                    new CustomEvent(DASHBOARD_SELECT_EVENT, { detail: { dashboardId: value } }),
                  );
                }}
              >
                <SelectTrigger className="w-[180px] sm:w-[220px] h-9 bg-background">
                  <SelectValue>
                    {dashboards.find((item) => item.id === selectedDashboardId)?.name ??
                      "Dashboard"}
                  </SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {dashboards.map((dashboard) => (
                    <SelectItem key={dashboard.id} value={dashboard.id}>
                      {dashboard.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Invite Section */}
            <div className="space-y-2">
              <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                Invite Members
              </h4>
              <div className="flex flex-col sm:flex-row gap-2">
                <div className="relative flex-1">
                  <Input
                    value={sharePrincipal}
                    placeholder="Enter email or username (e.g. name@company.com)"
                    className="h-9 pl-9 pr-3 text-sm"
                    onChange={(event) => setSharePrincipal(event.target.value)}
                  />
                  <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3 text-muted-foreground/75">
                    <Mail className="size-4" />
                  </div>
                </div>
                <div className="flex gap-2">
                  <Select
                    value={shareRole}
                    onValueChange={(value) => setShareRole(value as DashboardShareRole)}
                  >
                    <SelectTrigger className="w-[110px] h-9 bg-background">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="viewer">Viewer</SelectItem>
                      <SelectItem value="editor">Editor</SelectItem>
                    </SelectContent>
                  </Select>
                  <Button
                    type="button"
                    className="h-9 px-4.5 font-semibold shrink-0"
                    onClick={() => {
                      if (!selectedDashboardId || !sharePrincipal.trim()) return;
                      window.dispatchEvent(
                        new CustomEvent(DASHBOARD_SET_SHARE_EVENT, {
                          detail: {
                            dashboardId: selectedDashboardId,
                            principal: sharePrincipal,
                            role: shareRole,
                          },
                        }),
                      );
                      setSharePrincipal("");
                    }}
                  >
                    <UserPlus className="size-4 mr-1.5" />
                    Share
                  </Button>
                </div>
              </div>
            </div>

            {/* Shared Members List */}
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <h4 className="text-xs font-semibold text-muted-foreground uppercase tracking-wider">
                  Members with access
                </h4>
                <Badge
                  variant="secondary"
                  className="px-2 py-0.5 rounded-full text-[10px] font-semibold bg-muted/60"
                >
                  {shares.length} {shares.length === 1 ? "member" : "members"}
                </Badge>
              </div>

              <div className="overflow-hidden rounded-lg border border-border/50 bg-background max-h-[220px] overflow-y-auto">
                {shares.length === 0 ? (
                  <div className="flex flex-col items-center justify-center p-8 text-center">
                    <div className="size-9 rounded-full bg-muted/40 flex items-center justify-center mb-2.5">
                      <Users className="size-4.5 text-muted-foreground" />
                    </div>
                    <p className="text-sm font-semibold text-foreground">No shared members</p>
                    <p className="text-xs text-muted-foreground mt-1 max-w-[320px]">
                      This dashboard is currently private. Invite team members to collaborate.
                    </p>
                  </div>
                ) : (
                  <div className="divide-y divide-border/40">
                    {shares.map((share) => (
                      <div
                        key={`${selectedDashboardId}-${share.principal}`}
                        className="flex items-center justify-between p-3 hover:bg-muted/15 transition-colors"
                      >
                        <div className="flex items-center gap-3 min-w-0">
                          <div className="size-8 rounded-full bg-primary/10 flex items-center justify-center text-xs font-semibold text-primary uppercase shrink-0">
                            {share.principal.charAt(0)}
                          </div>
                          <div className="flex flex-col min-w-0">
                            <span className="text-sm font-semibold truncate text-foreground">
                              {share.principal}
                            </span>
                            <span className="text-[11px] text-muted-foreground">
                              {share.role === "editor"
                                ? "Can edit dashboard"
                                : "Can view dashboard"}
                            </span>
                          </div>
                        </div>
                        <div className="flex items-center gap-2">
                          <Badge
                            variant={share.role === "editor" ? "default" : "outline"}
                            className={cn(
                              "capitalize text-[10px] px-2 py-0.5 rounded-full font-semibold border-transparent",
                              share.role === "editor"
                                ? "bg-indigo-500/10 text-indigo-500 dark:bg-indigo-400/25 dark:text-indigo-300"
                                : "bg-muted text-muted-foreground",
                            )}
                          >
                            {share.role}
                          </Badge>
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon-sm"
                            className="text-muted-foreground hover:text-destructive hover:bg-destructive/10 shrink-0"
                            onClick={() => {
                              window.dispatchEvent(
                                new CustomEvent(DASHBOARD_REMOVE_SHARE_EVENT, {
                                  detail: {
                                    dashboardId: selectedDashboardId,
                                    principal: share.principal,
                                  },
                                }),
                              );
                            }}
                            aria-label={`Remove access for ${share.principal}`}
                          >
                            <Trash2 className="size-4" />
                          </Button>
                        </div>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
