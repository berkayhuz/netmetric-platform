import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeAll, describe, expect, it, vi } from "vitest";

import {
  AppHeader,
  AppPagePanel,
  AppShell,
  AppSidebar,
  AppSidebarNav,
  AppSidebarUserHeader,
  AppSidebarUserMenu,
  AppWorkspaceHeader,
  AppWorkspaceShell,
  ThemeProvider,
} from "../client";

import type { CSSProperties, SVGProps } from "react";

describe("app shell components", () => {
  beforeAll(() => {
    Object.defineProperty(window, "matchMedia", {
      writable: true,
      value: vi.fn().mockImplementation(() => ({
        matches: false,
        media: "(max-width: 767px)",
        onchange: null,
        addListener: vi.fn(),
        removeListener: vi.fn(),
        addEventListener: vi.fn(),
        removeEventListener: vi.fn(),
        dispatchEvent: vi.fn(),
      })),
    });
  });

  const DashboardIcon = (props: SVGProps<SVGSVGElement>) => (
    <svg data-testid="dashboard-icon" viewBox="0 0 24 24" {...props}>
      <path d="M3 3h8v8H3zM13 3h8v5h-8zM13 10h8v11h-8zM3 13h8v8H3z" />
    </svg>
  );

  const UserIcon = (props: SVGProps<SVGSVGElement>) => (
    <svg viewBox="0 0 24 24" {...props}>
      <circle cx="12" cy="8" r="4" />
      <path d="M4 20a8 8 0 0 1 16 0" />
    </svg>
  );

  function renderShell(collapsed = false) {
    return render(
      <AppShell
        skipToContentLabel="Skip to content"
        mobileSidebarAriaLabel="Main navigation"
        defaultDesktopSidebarCollapsed={collapsed}
        sidebar={
          <AppSidebar
            nav={
              <AppSidebarNav
                pathname="/reports/current"
                collapsed={collapsed}
                ariaLabel="Main navigation"
                groups={[
                  {
                    id: "core",
                    label: "Core",
                    items: [
                      {
                        href: "/dashboard",
                        label: "Dashboard",
                        title: "Open dashboard panel",
                        icon: DashboardIcon,
                        iconClassName: "text-red-500",
                        match: "exact",
                      },
                      { href: "/reports", label: "Reports", match: "prefix" },
                      { label: "Disabled item", disabled: true },
                    ],
                  },
                ]}
                renderLink={({ href, className, children, ...linkProps }) => (
                  <a href={href} className={className} {...linkProps}>
                    {children}
                  </a>
                )}
              />
            }
          />
        }
        header={
          <AppHeader
            mobileMenuAriaLabel="Open navigation"
            desktopSidebarToggle={{
              expandedAriaLabel: "Collapse sidebar",
              collapsedAriaLabel: "Expand sidebar",
            }}
          />
        }
      >
        <div>Page body</div>
      </AppShell>,
    );
  }

  it("renders skip link, active nav state, disabled nav items, and collapse toggle", () => {
    const onDesktopSidebarCollapsedChange = vi.fn();

    render(
      <AppShell
        skipToContentLabel="Skip to content"
        mobileSidebarAriaLabel="Main navigation"
        onDesktopSidebarCollapsedChange={onDesktopSidebarCollapsedChange}
        sidebar={
          <AppSidebar
            nav={
              <AppSidebarNav
                pathname="/reports/current"
                ariaLabel="Main navigation"
                groups={[
                  {
                    id: "core",
                    items: [
                      {
                        href: "/dashboard",
                        label: "Dashboard",
                        icon: DashboardIcon,
                        iconClassName: "text-red-500",
                        match: "exact",
                      },
                      { href: "/reports", label: "Reports", match: "prefix" },
                      { label: "Disabled item", disabled: true },
                    ],
                  },
                ]}
              />
            }
          />
        }
        header={
          <AppHeader
            mobileMenuAriaLabel="Open navigation"
            desktopSidebarToggle={{
              expandedAriaLabel: "Collapse sidebar",
              collapsedAriaLabel: "Expand sidebar",
            }}
          />
        }
      >
        <div>Page body</div>
      </AppShell>,
    );

    expect(screen.getByRole("link", { name: "Skip to content" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Reports" })).toHaveAttribute("aria-current", "page");
    expect(screen.getByRole("button", { name: "Disabled item" })).toBeDisabled();
    expect(screen.getByTestId("dashboard-icon")).toHaveAttribute("aria-hidden", "true");
    expect(screen.getByTestId("dashboard-icon")).toHaveClass("text-red-500");
    expect(screen.getByRole("button", { name: "Open navigation" })).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Collapse sidebar" }));
    expect(onDesktopSidebarCollapsedChange).toHaveBeenCalledWith(true);
    expect(screen.getByRole("button", { name: "Expand sidebar" })).toBeInTheDocument();
  });

  it("renders nav in icon-only mode when collapsed", () => {
    renderShell(true);

    const dashboardLink = screen.getByRole("link", { name: "Dashboard" });
    expect(dashboardLink).toHaveClass("md:justify-center");
    expect(screen.getByText("Core")).toHaveClass("md:sr-only");
    expect(screen.getByText("Dashboard")).toHaveClass("md:sr-only");
  });

  it("shows sidebar tooltips in collapsed mode", async () => {
    renderShell(true);

    const dashboardLink = screen.getByRole("link", { name: "Dashboard" });
    fireEvent.mouseEnter(dashboardLink);
    fireEvent.focus(dashboardLink);

    expect(await screen.findByText("Open dashboard panel")).toBeInTheDocument();
  });

  it("renders user dropdown actions and overflow menu actions", async () => {
    const inviteAction = vi.fn();
    const createWorkspaceAction = vi.fn();

    render(
      <AppSidebarUserMenu
        user={{
          name: "Ada Lovelace",
          secondaryText: "ada@netmetric.net",
          avatar: (
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-xs">
              AL
            </div>
          ),
        }}
        triggerAriaLabel="Open profile menu"
        overflowAriaLabel="Open overflow menu"
        actions={[
          { label: "Invite user", icon: UserIcon, onSelect: inviteAction },
          { label: "Settings", icon: UserIcon, onSelect: () => {} },
          { label: "Support", icon: UserIcon, onSelect: () => {} },
        ]}
        overflowActions={[
          { label: "Create workspace", icon: UserIcon, onSelect: createWorkspaceAction },
          { label: "Sign out", icon: UserIcon, onSelect: () => {} },
        ]}
      />,
    );

    const profileMenuTrigger = screen.getByRole("button", { name: "Open profile menu" });
    fireEvent.pointerDown(profileMenuTrigger);
    fireEvent.pointerUp(profileMenuTrigger);
    fireEvent.mouseDown(profileMenuTrigger);
    fireEvent.click(profileMenuTrigger);
    fireEvent.keyDown(profileMenuTrigger, { key: "ArrowDown" });

    expect(await screen.findByText("Invite user")).toBeInTheDocument();
    expect(screen.getByText("Settings")).toBeInTheDocument();
    expect(screen.getByText("Support")).toBeInTheDocument();

    const overflowTrigger = screen.getByLabelText("Open overflow menu");
    fireEvent.pointerDown(overflowTrigger);
    fireEvent.click(overflowTrigger);
    expect(await screen.findByText("Create workspace")).toBeInTheDocument();
    expect(screen.getByText("Sign out")).toBeInTheDocument();

    fireEvent.click(screen.getByText("Create workspace"));
    expect(createWorkspaceAction).toHaveBeenCalledTimes(1);

    fireEvent.pointerDown(profileMenuTrigger);
    fireEvent.click(profileMenuTrigger);
    fireEvent.click(screen.getByText("Invite user"));
    expect(inviteAction).toHaveBeenCalledTimes(1);
  });

  it("renders the shared user footer with avatar fallback initials", () => {
    render(
      <AppSidebarUserHeader
        user={{ displayName: "Ada Lovelace", email: "ada@netmetric.net", avatarUrl: null }}
        labels={{
          triggerAriaLabel: "Open profile menu",
          overflowAriaLabel: "Open overflow menu",
        }}
        actions={[{ label: "Settings", icon: UserIcon, onSelect: () => {} }]}
      />,
    );

    expect(screen.getByRole("button", { name: "Open profile menu" })).toBeInTheDocument();
    expect(screen.getByText("Ada Lovelace")).toBeInTheDocument();
    expect(screen.getByText("ada@netmetric.net")).toBeInTheDocument();
    expect(screen.getByText("AL")).toBeInTheDocument();
  });

  it("renders the shared page panel title, description, actions, and body", () => {
    render(
      <AppPagePanel
        title="Profile"
        description="Manage profile details."
        actions={<button>Save</button>}
      >
        <div>Panel body</div>
      </AppPagePanel>,
    );

    expect(screen.getByRole("heading", { name: "Profile" })).toBeInTheDocument();
    expect(screen.getByText("Manage profile details.")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Save" })).toBeInTheDocument();
    expect(screen.getByText("Panel body")).toBeInTheDocument();
  });

  it("renders the shared workspace header notification dropdown states", async () => {
    render(
      <ThemeProvider>
        <AppShell
          skipToContentLabel="Skip to content"
          mobileSidebarAriaLabel="Main navigation"
          sidebar={<AppSidebar />}
          header={
            <AppWorkspaceHeader
              mobileMenuAriaLabel="Open navigation"
              collapseSidebarAriaLabel="Collapse sidebar"
              expandSidebarAriaLabel="Expand sidebar"
              searchPlaceholder="Search records"
              primaryActionLabel="New customer"
              notifications={{
                label: "Notifications",
                triggerAriaLabel: "Open notifications",
                emptyText: "No notifications yet.",
                unreadLabel: "1 unread",
                viewAllLabel: "View all",
                viewAllHref: "/notifications",
                unreadCount: 1,
                items: [
                  {
                    id: "notification-1",
                    title: "Password changed",
                    description: "Recorded as auth.password.changed.",
                    isRead: false,
                  },
                ],
              }}
            />
          }
        >
          <div>Page body</div>
        </AppShell>
      </ThemeProvider>,
    );

    const notifications = screen.getByRole("button", { name: "Open notifications" });
    fireEvent.pointerDown(notifications);
    fireEvent.click(notifications);
    fireEvent.keyDown(notifications, { key: "ArrowDown" });

    expect(await screen.findByText("Password changed")).toBeInTheDocument();
    expect(screen.getByText("Recorded as auth.password.changed.")).toBeInTheDocument();
    expect(screen.getByText("View all")).toBeInTheDocument();
  });

  it("fires workspace header search trigger callbacks", async () => {
    const onSearchClick = vi.fn();
    const onSearchFocus = vi.fn();

    render(
      <ThemeProvider>
        <AppShell
          skipToContentLabel="Skip to content"
          mobileSidebarAriaLabel="Main navigation"
          sidebar={<AppSidebar />}
          header={
            <AppWorkspaceHeader
              mobileMenuAriaLabel="Open navigation"
              collapseSidebarAriaLabel="Collapse sidebar"
              expandSidebarAriaLabel="Expand sidebar"
              searchPlaceholder="Search records"
              searchReadOnly
              onSearchClick={onSearchClick}
              onSearchFocus={onSearchFocus}
            />
          }
        >
          <div>Page body</div>
        </AppShell>
      </ThemeProvider>,
    );

    const search = screen.getByTestId("global-search-trigger");
    await waitFor(() => expect(search).toBeEnabled());

    fireEvent.focus(search);
    fireEvent.click(search);

    expect(onSearchFocus).toHaveBeenCalledTimes(1);
    expect(onSearchClick).toHaveBeenCalledTimes(1);
  });

  it("does not fire search focus callback on pointer-origin focus", async () => {
    const onSearchClick = vi.fn();
    const onSearchFocus = vi.fn();

    render(
      <ThemeProvider>
        <AppShell
          skipToContentLabel="Skip to content"
          mobileSidebarAriaLabel="Main navigation"
          sidebar={<AppSidebar />}
          header={
            <AppWorkspaceHeader
              mobileMenuAriaLabel="Open navigation"
              collapseSidebarAriaLabel="Collapse sidebar"
              expandSidebarAriaLabel="Expand sidebar"
              searchPlaceholder="Search records"
              searchReadOnly
              onSearchClick={onSearchClick}
              onSearchFocus={onSearchFocus}
            />
          }
        >
          <div>Page body</div>
        </AppShell>
      </ThemeProvider>,
    );

    const search = screen.getByTestId("global-search-trigger");
    await waitFor(() => expect(search).toBeEnabled());

    fireEvent.pointerDown(search);
    fireEvent.focus(search);
    fireEvent.click(search);
    fireEvent.pointerUp(search);

    expect(onSearchFocus).toHaveBeenCalledTimes(0);
    expect(onSearchClick).toHaveBeenCalledTimes(1);
  });

  it("renders the shared workspace shell with background variables", () => {
    render(
      <ThemeProvider>
        <AppWorkspaceShell
          labels={{
            skipToContent: "Skip to content",
            mobileSidebarAriaLabel: "Main navigation",
            mobileSidebarTitle: "Navigation",
            appName: "NetMetric Account",
            workspace: "Workspace",
          }}
          pathname="/dashboard"
          navGroups={[{ items: [{ href: "/dashboard", label: "Dashboard", icon: DashboardIcon }] }]}
          user={{ displayName: "Ada Lovelace", email: "ada@netmetric.net", avatarUrl: null }}
          userLabels={{
            triggerAriaLabel: "Open profile menu",
            overflowAriaLabel: "Open overflow menu",
          }}
          userActions={[{ label: "Settings", icon: UserIcon, onSelect: () => {} }]}
          header={{
            mobileMenuAriaLabel: "Open navigation",
            collapseSidebarAriaLabel: "Collapse sidebar",
            expandSidebarAriaLabel: "Expand sidebar",
            searchPlaceholder: "Search",
          }}
          contentFrameClassName="test-content-frame"
          backgroundStyle={
            {
              "--netmetric-app-background-image-light": 'url("data:image/png;base64,AAAA")',
              "--netmetric-app-background-image-dark": 'url("data:image/jpeg;base64,BBBB")',
            } as CSSProperties
          }
        >
          <div>Workspace content</div>
        </AppWorkspaceShell>
      </ThemeProvider>,
    );

    expect(screen.getByText("Workspace content")).toBeInTheDocument();
    expect(screen.getByText("Ada Lovelace")).toBeInTheDocument();
    expect(screen.getByText("ada@netmetric.net")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Dashboard" })).toHaveAttribute("aria-current", "page");
    expect(screen.getByText("AL")).toBeInTheDocument();
    expect(
      screen.getByText("Workspace content").closest("[data-netmetric-app-background]"),
    ).toHaveStyle({
      "--netmetric-app-background-image-light": 'url("data:image/png;base64,AAAA")',
      "--netmetric-app-background-image-dark": 'url("data:image/jpeg;base64,BBBB")',
    });
    expect(screen.getByText("Workspace content").closest(".test-content-frame")).not.toBeNull();
  });
});
