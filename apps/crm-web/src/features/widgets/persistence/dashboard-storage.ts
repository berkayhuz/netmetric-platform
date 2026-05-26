"use client";

import { createDefaultDashboardProfile } from "@/features/widgets/registry/widget-catalog";
import type {
  DashboardCollection,
  DashboardProfile,
  DashboardShareRole,
  DashboardWidgetInstance,
} from "@/features/widgets/types";

const MAX_DASHBOARDS = 10;
const TRASH_TTL_DAYS = 7;

function buildStorageKey(userId: string): string {
  return `crm.dashboard.preferences.v1:${userId}`;
}

function nowIso(): string {
  return new Date().toISOString();
}

function addDays(date: Date, days: number): Date {
  const copy = new Date(date);
  copy.setDate(copy.getDate() + days);
  return copy;
}

function createInitialCollection(): DashboardCollection {
  const profile = createDefaultDashboardProfile();
  return {
    active: [profile],
    trash: [],
    selectedDashboardId: profile.id,
    updatedAt: nowIso(),
  };
}

function purgeExpiredTrash(collection: DashboardCollection): DashboardCollection {
  const currentTime = Date.now();
  return {
    ...collection,
    trash: collection.trash.filter((profile) => {
      if (!profile.purgeAt) return false;
      return new Date(profile.purgeAt).getTime() > currentTime;
    }),
  };
}

export function loadDashboardCollection(userId: string): DashboardCollection {
  if (typeof window === "undefined") {
    return createInitialCollection();
  }

  const raw = window.localStorage.getItem(buildStorageKey(userId));
  if (!raw) {
    return createInitialCollection();
  }

  try {
    const parsed = JSON.parse(raw) as DashboardCollection;
    const hydrated = purgeExpiredTrash(parsed);

    if (!hydrated.active.length) {
      return createInitialCollection();
    }

    return hydrated;
  } catch {
    return createInitialCollection();
  }
}

export function hydrateClientCollection(
  userId: string,
  serverCollection: DashboardCollection,
): DashboardCollection {
  const clientCollection = loadDashboardCollection(userId);
  const serverTime = Date.parse(serverCollection.updatedAt);
  const clientTime = Date.parse(clientCollection.updatedAt);

  if (Number.isNaN(serverTime)) {
    return clientCollection;
  }

  if (Number.isNaN(clientTime)) {
    return serverCollection;
  }

  return serverTime >= clientTime ? serverCollection : clientCollection;
}

export function saveDashboardCollection(userId: string, collection: DashboardCollection): void {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(
    buildStorageKey(userId),
    JSON.stringify({ ...collection, updatedAt: nowIso() }),
  );
}

export function createDashboard(
  collection: DashboardCollection,
  name: string,
): DashboardCollection {
  if (collection.active.length >= MAX_DASHBOARDS) {
    throw new Error("MAX_DASHBOARD_LIMIT_REACHED");
  }

  const dashboard: DashboardProfile = {
    ...createDefaultDashboardProfile(),
    id: `dashboard_${crypto.randomUUID()}`,
    name,
    isDefault: false,
  };

  return {
    ...collection,
    active: [...collection.active, dashboard],
    selectedDashboardId: dashboard.id,
    updatedAt: nowIso(),
  };
}

export function updateDashboardWidgets(
  collection: DashboardCollection,
  dashboardId: string,
  widgets: DashboardWidgetInstance[],
): DashboardCollection {
  return {
    ...collection,
    active: collection.active.map((dashboard) =>
      dashboard.id === dashboardId
        ? { ...dashboard, widgets, layoutVersion: dashboard.layoutVersion + 1 }
        : dashboard,
    ),
    updatedAt: nowIso(),
  };
}

export function addWidget(
  collection: DashboardCollection,
  dashboardId: string,
  widget: DashboardWidgetInstance,
): DashboardCollection {
  return {
    ...collection,
    active: collection.active.map((dashboard) =>
      dashboard.id === dashboardId
        ? { ...dashboard, widgets: [...dashboard.widgets, widget] }
        : dashboard,
    ),
    updatedAt: nowIso(),
  };
}

export function removeWidget(
  collection: DashboardCollection,
  dashboardId: string,
  widgetId: string,
): DashboardCollection {
  return {
    ...collection,
    active: collection.active.map((dashboard) =>
      dashboard.id === dashboardId
        ? { ...dashboard, widgets: dashboard.widgets.filter((widget) => widget.id !== widgetId) }
        : dashboard,
    ),
    updatedAt: nowIso(),
  };
}

export function moveToTrash(
  collection: DashboardCollection,
  dashboardId: string,
): DashboardCollection {
  const target = collection.active.find((dashboard) => dashboard.id === dashboardId);
  if (!target) {
    return collection;
  }

  if (collection.active.length === 1) {
    throw new Error("AT_LEAST_ONE_DASHBOARD_REQUIRED");
  }

  const deletedAt = new Date();
  const trashed: DashboardProfile = {
    ...target,
    deletedAt: deletedAt.toISOString(),
    purgeAt: addDays(deletedAt, TRASH_TTL_DAYS).toISOString(),
  };

  const active = collection.active.filter((dashboard) => dashboard.id !== dashboardId);
  const selectedDashboardId =
    collection.selectedDashboardId === dashboardId
      ? (active[0]?.id ?? null)
      : collection.selectedDashboardId;

  return {
    ...collection,
    active,
    trash: [trashed, ...collection.trash],
    selectedDashboardId,
    updatedAt: nowIso(),
  };
}

export function restoreFromTrash(
  collection: DashboardCollection,
  dashboardId: string,
): DashboardCollection {
  if (collection.active.length >= MAX_DASHBOARDS) {
    throw new Error("MAX_DASHBOARD_LIMIT_REACHED");
  }

  const target = collection.trash.find((dashboard) => dashboard.id === dashboardId);
  if (!target) {
    return collection;
  }

  return {
    ...collection,
    active: [
      ...collection.active,
      {
        ...target,
        deletedAt: null,
        purgeAt: null,
      },
    ],
    trash: collection.trash.filter((dashboard) => dashboard.id !== dashboardId),
    selectedDashboardId: target.id,
    updatedAt: nowIso(),
  };
}

export function setSelectedDashboard(
  collection: DashboardCollection,
  dashboardId: string,
): DashboardCollection {
  return {
    ...collection,
    selectedDashboardId: dashboardId,
    updatedAt: nowIso(),
  };
}

export function setDashboardDefault(
  collection: DashboardCollection,
  dashboardId: string,
): DashboardCollection {
  return {
    ...collection,
    active: collection.active.map((dashboard) => ({
      ...dashboard,
      isDefault: dashboard.id === dashboardId,
    })),
    updatedAt: nowIso(),
  };
}

export function setDashboardShares(
  collection: DashboardCollection,
  dashboardId: string,
  principal: string,
  role: DashboardShareRole,
): DashboardCollection {
  const normalized = principal.trim().toLowerCase();
  if (!normalized) {
    return collection;
  }

  return {
    ...collection,
    active: collection.active.map((dashboard) => {
      if (dashboard.id !== dashboardId) {
        return dashboard;
      }

      const existingIndex = dashboard.shares.findIndex((share) => share.principal === normalized);
      if (existingIndex === -1) {
        return {
          ...dashboard,
          shares: [...dashboard.shares, { principal: normalized, role }],
        };
      }

      const shares = [...dashboard.shares];
      shares[existingIndex] = { principal: normalized, role };
      return { ...dashboard, shares };
    }),
    updatedAt: nowIso(),
  };
}

export function removeDashboardShare(
  collection: DashboardCollection,
  dashboardId: string,
  principal: string,
): DashboardCollection {
  const normalized = principal.trim().toLowerCase();
  if (!normalized) {
    return collection;
  }

  return {
    ...collection,
    active: collection.active.map((dashboard) =>
      dashboard.id === dashboardId
        ? {
            ...dashboard,
            shares: dashboard.shares.filter((share) => share.principal !== normalized),
          }
        : dashboard,
    ),
    updatedAt: nowIso(),
  };
}

export function getMaxDashboardLimit(): number {
  return MAX_DASHBOARDS;
}
