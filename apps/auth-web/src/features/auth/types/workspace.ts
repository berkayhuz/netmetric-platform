export type WorkspaceRole = "Owner" | "Admin" | "Member" | "Viewer" | string;

export type WorkspaceSummary = {
  tenantId: string;
  name: string;
  slug?: string;
  role?: WorkspaceRole;
  isDefault?: boolean;
  lastUsedAt?: string;
};

export type WorkspaceSwitchResult = {
  activeTenantId: string;
  redirectUrl?: string;
};
