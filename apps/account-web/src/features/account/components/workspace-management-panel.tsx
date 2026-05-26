"use client";

import { useEffect, useMemo, useState, useTransition } from "react";
import { Building2, Check, Plus, RefreshCw, Trash2 } from "lucide-react";
import {
  Alert,
  AlertDescription,
  Button,
  Field,
  FieldContent,
  FieldError,
  FieldLabel,
  Input,
  Text,
} from "@netmetric/ui";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  toast,
} from "@netmetric/ui/client";

import type { OrganizationMembershipSummaryResponse } from "@/lib/account-api";

type WorkspaceManagementPanelProps = {
  organizations: OrganizationMembershipSummaryResponse[];
  showHeading?: boolean;
};

type WorkspaceSummary = {
  tenantId: string;
  name: string;
  slug?: string | null;
  role?: string | null;
  isDefault?: boolean;
};

const maxWorkspaceCount = 5;

async function readPayload(response: Response): Promise<unknown> {
  const text = await response.text();
  if (!text) {
    return null;
  }

  try {
    return JSON.parse(text) as unknown;
  } catch {
    return text;
  }
}

function mapOrganizationsToWorkspaces(
  organizations: OrganizationMembershipSummaryResponse[],
): WorkspaceSummary[] {
  return organizations.map((organization) => {
    const workspace: WorkspaceSummary = {
      tenantId: organization.tenantId,
      name: organization.organizationName,
      isDefault: organization.isDefault,
    };

    if (organization.organizationSlug) {
      workspace.slug = organization.organizationSlug;
    }

    const role = organization.roles[0];
    if (role) {
      workspace.role = role;
    }

    return workspace;
  });
}

export function WorkspaceManagementPanel({
  organizations,
  showHeading = true,
}: WorkspaceManagementPanelProps) {
  const [workspaces, setWorkspaces] = useState<WorkspaceSummary[]>(() =>
    mapOrganizationsToWorkspaces(organizations),
  );
  const [workspaceName, setWorkspaceName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [fieldError, setFieldError] = useState<string | null>(null);
  const [workspaceToDelete, setWorkspaceToDelete] = useState<WorkspaceSummary | null>(null);
  const [isPending, startTransition] = useTransition();
  const activeWorkspace = useMemo(
    () => workspaces.find((workspace) => workspace.isDefault) ?? workspaces[0],
    [workspaces],
  );
  const workspaceLimitReached = workspaces.length >= maxWorkspaceCount;

  useEffect(() => {
    let active = true;

    fetch("/api/auth/workspaces", {
      credentials: "include",
      cache: "no-store",
    })
      .then(async (response) => {
        const payload = await readPayload(response);
        if (!response.ok || !Array.isArray(payload)) {
          throw new Error("Workspaces could not be loaded.");
        }

        if (active) {
          setWorkspaces(
            payload.flatMap((item): WorkspaceSummary[] => {
              if (!item || typeof item !== "object") {
                return [];
              }

              const candidate = item as Record<string, unknown>;
              const tenantId = candidate.tenantId;
              const name = candidate.name;
              if (typeof tenantId !== "string" || typeof name !== "string") {
                return [];
              }

              return [
                {
                  tenantId,
                  name,
                  slug: typeof candidate.slug === "string" ? candidate.slug : null,
                  role: typeof candidate.role === "string" ? candidate.role : null,
                  isDefault: candidate.isDefault === true,
                },
              ];
            }),
          );
        }
      })
      .catch(() => {
        if (active) {
          setWorkspaces(mapOrganizationsToWorkspaces(organizations));
        }
      });

    return () => {
      active = false;
    };
  }, [organizations]);

  function createWorkspace(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const name = workspaceName.trim();
    setError(null);
    setFieldError(null);

    if (name.length < 2 || name.length > 200) {
      setFieldError("Workspace name must be between 2 and 200 characters.");
      return;
    }

    if (workspaceLimitReached) {
      setFieldError("You can create up to 5 workspaces.");
      return;
    }

    startTransition(async () => {
      try {
        const response = await fetch("/api/auth/workspaces", {
          method: "POST",
          credentials: "include",
          headers: {
            "content-type": "application/json",
          },
          body: JSON.stringify({ name }),
        });

        if (!response.ok) {
          throw new Error("Workspace could not be created.");
        }

        toast.success("Workspace created.");
        window.location.reload();
      } catch {
        setError("Workspace could not be created.");
      }
    });
  }

  function deleteWorkspace(workspace: WorkspaceSummary) {
    setError(null);
    startTransition(async () => {
      try {
        const response = await fetch("/api/auth/workspaces", {
          method: "POST",
          credentials: "include",
          headers: {
            "content-type": "application/json",
          },
          body: JSON.stringify({ intent: "delete", tenantId: workspace.tenantId }),
        });

        if (!response.ok) {
          throw new Error("Workspace could not be deleted.");
        }

        toast.success("Workspace deleted.");
        setWorkspaceToDelete(null);
        window.location.reload();
      } catch {
        setError("Workspace could not be deleted.");
      }
    });
  }

  function switchWorkspace(tenantId: string) {
    setError(null);
    startTransition(async () => {
      try {
        const response = await fetch("/api/auth/workspaces/switch", {
          method: "POST",
          credentials: "include",
          headers: {
            "content-type": "application/json",
          },
          body: JSON.stringify({ tenantId }),
        });

        if (!response.ok) {
          throw new Error("Workspace could not be switched.");
        }

        toast.success("Workspace switched.");
        window.location.reload();
      } catch {
        setError("Workspace could not be switched.");
      }
    });
  }

  return (
    <section className={showHeading ? "space-y-4 pb-5" : "space-y-4"}>
      <div className="flex items-start justify-between gap-3">
        {showHeading ? (
          <div className="space-y-1">
            <Text className="text-base font-semibold text-foreground">Workspaces</Text>
            <Text className="text-sm text-muted-foreground">
              Create workspaces and switch the active company context.
            </Text>
          </div>
        ) : (
          <Text className="text-sm font-medium text-foreground">Available workspaces</Text>
        )}
        <div className="shrink-0 rounded-sm border border-border/70 px-2 py-1 text-xs font-medium text-muted-foreground">
          {workspaces.length}/{maxWorkspaceCount}
        </div>
      </div>

      {error ? (
        <Alert variant="destructive">
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      ) : null}

      <div className="space-y-2">
        {workspaces.map((workspace) => {
          const isActive = workspace.tenantId === activeWorkspace?.tenantId;
          const canDelete = workspace.role?.toLowerCase() === "tenant-owner" && !isActive;
          return (
            <div
              key={workspace.tenantId}
              className="flex min-w-0 items-center justify-between gap-3 rounded-md border border-border/70 px-3 py-2"
            >
              <div className="flex min-w-0 items-center gap-3">
                <Building2 className="size-4 shrink-0 text-muted-foreground" />
                <div className="min-w-0">
                  <Text className="truncate text-sm font-medium text-foreground">
                    {workspace.name}
                  </Text>
                  <Text className="truncate text-xs text-muted-foreground">
                    {workspace.role ?? "Member"}
                  </Text>
                </div>
              </div>
              <div className="flex shrink-0 items-center gap-2">
                {canDelete ? (
                  <Button
                    type="button"
                    size="icon-xs"
                    variant="ghost"
                    disabled={isPending}
                    aria-label={`Delete ${workspace.name}`}
                    onClick={() => setWorkspaceToDelete(workspace)}
                  >
                    <Trash2 />
                  </Button>
                ) : null}
                <Button
                  type="button"
                  size="xs"
                  variant={isActive ? "secondary" : "outline"}
                  disabled={isPending || isActive}
                  onClick={() => switchWorkspace(workspace.tenantId)}
                >
                  {isActive ? <Check /> : <RefreshCw />}
                  {isActive ? "Active" : "Switch"}
                </Button>
              </div>
            </div>
          );
        })}
      </div>

      <form onSubmit={createWorkspace} className="space-y-2">
        <Field>
          <FieldLabel htmlFor="workspaceName">New workspace</FieldLabel>
          <FieldContent>
            <Input
              id="workspaceName"
              value={workspaceName}
              onChange={(event) => setWorkspaceName(event.currentTarget.value)}
              placeholder="Company or team name"
              aria-invalid={Boolean(fieldError)}
              disabled={isPending || workspaceLimitReached}
            />
            <FieldError>
              {fieldError ?? (workspaceLimitReached ? "Workspace limit reached." : undefined)}
            </FieldError>
          </FieldContent>
        </Field>
        <Button type="submit" size="xs" disabled={isPending || workspaceLimitReached}>
          <Plus />
          Create workspace
        </Button>
      </form>

      <AlertDialog
        open={workspaceToDelete !== null}
        onOpenChange={(open) => {
          if (!open && !isPending) {
            setWorkspaceToDelete(null);
          }
        }}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete workspace?</AlertDialogTitle>
            <AlertDialogDescription>
              {workspaceToDelete
                ? `${workspaceToDelete.name} will be removed for everyone in this workspace.`
                : "This workspace will be removed for everyone."}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel size="sm" disabled={isPending}>
              Cancel
            </AlertDialogCancel>
            <AlertDialogAction
              type="button"
              size="sm"
              variant="destructive"
              disabled={isPending || workspaceToDelete === null}
              onClick={() => {
                if (workspaceToDelete) {
                  deleteWorkspace(workspaceToDelete);
                }
              }}
            >
              Delete workspace
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </section>
  );
}
