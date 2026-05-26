"use client";

import {
  BellIcon,
  ChevronDownIcon,
  ChevronRightIcon,
  MenuIcon,
  MoreHorizontalIcon,
  PanelLeftCloseIcon,
  PanelLeftOpenIcon,
  PlusIcon,
  SearchIcon,
} from "lucide-react";
import * as React from "react";

import { useIsMobile } from "../../hooks/use-mobile";
import { useMounted } from "../../hooks/use-mounted";
import { cn } from "../../lib/utils";
import { Avatar, AvatarFallback, AvatarImage } from "../data-display/avatar";
import { Empty, EmptyDescription, EmptyHeader } from "../data-display/empty";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuSub,
  DropdownMenuSubContent,
  DropdownMenuSubTrigger,
  DropdownMenuTrigger,
} from "../overlay/dropdown-menu";
import { Sheet, SheetContent, SheetDescription, SheetHeader, SheetTitle } from "../overlay/sheet";
import { Tooltip, TooltipContent, TooltipTrigger } from "../overlay/tooltip";
import { Button } from "../primitives/button";
import { Spinner } from "../primitives/spinner";
import { ThemeToggle } from "../theme/theme-toggle";
import { Heading } from "../typography/heading";

export type AppShellNavPathMatch =
  | "exact"
  | "prefix"
  | ((pathname: string, item: AppSidebarNavItem) => boolean);

export type AppSidebarNavIcon = React.ComponentType<React.SVGProps<SVGSVGElement>>;

export type AppSidebarNavItem = {
  id?: string;
  label: React.ReactNode;
  href?: string;
  icon?: AppSidebarNavIcon;
  iconClassName?: string;
  badge?: React.ReactNode;
  disabled?: boolean;
  ariaLabel?: string;
  title?: string;
  match?: AppShellNavPathMatch;
};

export type AppSidebarNavGroup = {
  id?: string;
  label?: React.ReactNode;
  collapsible?: boolean;
  defaultExpanded?: boolean;
  items: readonly AppSidebarNavItem[];
};

export type AppSidebarLinkRendererProps = {
  href: string;
  className: string;
  children: React.ReactNode;
  "aria-current"?: "page";
  "aria-label"?: string;
  title?: string;
};

export type AppSidebarLinkRenderer = (props: AppSidebarLinkRendererProps) => React.ReactElement;

export type AppHeaderActionSlots = {
  leading?: React.ReactNode;
  trailing?: React.ReactNode;
};

export type AppHeaderDesktopSidebarToggle = {
  collapsedAriaLabel: string;
  expandedAriaLabel: string;
  className?: string;
};

export type AppSidebarUserMenuAction = {
  id?: string;
  label: string;
  icon: AppSidebarNavIcon;
  href?: string;
  onSelect?: () => void;
  disabled?: boolean;
  ariaLabel?: string;
  title?: string;
};

export type AppSidebarUserMenuUser = {
  name: string;
  secondaryText?: string;
  avatar: React.ReactNode;
};

export type AppSidebarUserMenuProps = {
  user: AppSidebarUserMenuUser;
  actions: readonly AppSidebarUserMenuAction[];
  overflowActions?: readonly AppSidebarUserMenuAction[];
  triggerAriaLabel: string;
  overflowAriaLabel: string;
  menuLabel?: React.ReactNode;
  collapsed?: boolean;
  className?: string;
};

export type AppShellUserIdentity = {
  displayName: string;
  email?: string | null;
  avatarUrl?: string | null;
};

export type AppSidebarUserHeaderLabels = {
  triggerAriaLabel: string;
  overflowAriaLabel: string;
  fallbackName?: string;
  secondaryText?: string;
};

export type AppHeaderNotificationItem = {
  id: string;
  title: string;
  description?: string | null;
  occurredAt?: string | null;
  isRead?: boolean;
  href?: string;
};

export type AppHeaderNotifications = {
  items: readonly AppHeaderNotificationItem[];
  unreadCount?: number;
  loading?: boolean;
  error?: string | null;
  emptyText: string;
  label: string;
  triggerAriaLabel: string;
  unreadLabel?: string;
  viewAllLabel?: string;
  viewAllHref?: string;
};

export type AppWorkspaceHeaderProps = {
  mobileMenuAriaLabel: string;
  collapseSidebarAriaLabel: string;
  expandSidebarAriaLabel: string;
  searchPlaceholder: string;
  searchAriaLabel?: string;
  searchReadOnly?: boolean;
  onSearchClick?: () => void;
  onSearchFocus?: () => void;
  primaryActionLabel?: string;
  primaryActionHref?: string;
  primaryActionDisabled?: boolean;
  primaryActionTitle?: string;
  notifications?: AppHeaderNotifications;
  rightContent?: React.ReactNode;
  className?: string;
};

export type AppWorkspaceShellLabels = {
  skipToContent: string;
  mobileSidebarAriaLabel: string;
  mobileSidebarTitle: string;
  appName: string;
  workspace?: string;
};

export type AppWorkspaceShellHeaderConfig = Omit<
  AppWorkspaceHeaderProps,
  "mobileMenuAriaLabel" | "collapseSidebarAriaLabel" | "expandSidebarAriaLabel"
> & {
  mobileMenuAriaLabel: string;
  collapseSidebarAriaLabel: string;
  expandSidebarAriaLabel: string;
};

export type AppWorkspaceShellProps = {
  children: React.ReactNode;
  labels: AppWorkspaceShellLabels;
  pathname: string;
  navGroups: readonly AppSidebarNavGroup[];
  user: AppShellUserIdentity;
  userLabels: AppSidebarUserHeaderLabels;
  userActions: readonly AppSidebarUserMenuAction[];
  userOverflowActions?: readonly AppSidebarUserMenuAction[];
  header: AppWorkspaceShellHeaderConfig;
  sidebarFooter?: React.ReactNode;
  sidebarNavStateKey?: string;
  renderLink?: AppSidebarLinkRenderer;
  className?: string;
  bodyClassName?: string;
  contentClassName?: string;
  contentFrameClassName?: string;
  backgroundStyle?: React.CSSProperties;
};

type AppShellContextValue = {
  setMobileSidebarOpen: (value: boolean) => void;
  isDesktopSidebarCollapsed: boolean;
  toggleDesktopSidebarCollapsed: () => void;
};

const AppShellContext = React.createContext<AppShellContextValue | null>(null);

function useAppShellContext(): AppShellContextValue {
  const context = React.useContext(AppShellContext);
  if (!context) {
    throw new Error("AppHeader must be used within AppShell.");
  }

  return context;
}

function normalizePathname(pathname: string): string {
  const trimmed = pathname.split(/[?#]/, 1)[0]?.replace(/\/+$/, "") ?? "";
  return trimmed.length > 0 ? trimmed : "/";
}

export function isAppSidebarItemActive(pathname: string, item: AppSidebarNavItem): boolean {
  if (!item.href) {
    return false;
  }

  const normalizedPathname = normalizePathname(pathname);
  const normalizedHref = normalizePathname(item.href);

  if (typeof item.match === "function") {
    return item.match(normalizedPathname, item);
  }

  if (item.match === "exact") {
    return normalizedPathname === normalizedHref;
  }

  if (item.match === "prefix") {
    if (normalizedHref === "/") {
      return normalizedPathname === "/";
    }

    return (
      normalizedPathname === normalizedHref || normalizedPathname.startsWith(`${normalizedHref}/`)
    );
  }

  return normalizedPathname === normalizedHref;
}

function getTooltipContent(item: AppSidebarNavItem): string | undefined {
  if (item.title) {
    return item.title;
  }

  if (item.ariaLabel) {
    return item.ariaLabel;
  }

  return typeof item.label === "string" ? item.label : undefined;
}

function handleUserMenuAction(action: AppSidebarUserMenuAction): void {
  action.onSelect?.();

  if (!action.onSelect && action.href) {
    window.location.assign(action.href);
  }
}

function renderUserMenuAction(action: AppSidebarUserMenuAction): React.ReactElement {
  const Icon = action.icon;

  return (
    <DropdownMenuItem
      key={action.id ?? action.href ?? action.label}
      disabled={action.disabled}
      aria-label={action.ariaLabel}
      title={action.title}
      className="text-sm"
      onClick={() => handleUserMenuAction(action)}
    >
      <Icon aria-hidden="true" focusable="false" className="h-4 w-4" />
      <span>{action.label}</span>
    </DropdownMenuItem>
  );
}

function getUserInitials(displayName: string): string {
  const parts = displayName.trim().split(/\s+/).filter(Boolean);

  if (parts.length >= 2) {
    return `${parts[0]?.[0] ?? ""}${parts[1]?.[0] ?? ""}`.toUpperCase();
  }

  return displayName.slice(0, 2).toUpperCase();
}

function resolveUserName(user: AppShellUserIdentity, fallbackName: string): string {
  const displayName = user.displayName.trim();
  if (displayName) {
    return displayName;
  }

  const email = user.email?.trim();
  return email || fallbackName;
}

function AppHeaderNotificationsMenu({
  notifications,
}: Readonly<{
  notifications: AppHeaderNotifications;
}>) {
  const unreadCount =
    notifications.unreadCount ?? notifications.items.filter((item) => !item.isRead).length;

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        className="relative grid h-8 w-8 place-items-center rounded-sm border border-border bg-card text-muted-foreground transition-colors hover:bg-muted hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
        aria-label={notifications.triggerAriaLabel}
        title={notifications.triggerAriaLabel}
      >
        <BellIcon aria-hidden="true" className="h-4 w-4" />
        {unreadCount > 0 ? (
          <span
            className="absolute right-1 top-1 h-2 w-2 rounded-full bg-destructive"
            aria-hidden="true"
          />
        ) : null}
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end" sideOffset={8} className="w-80 min-w-80">
        <div className="flex items-center justify-between gap-3 px-1.5 py-1">
          <p className="text-sm font-medium text-foreground">{notifications.label}</p>
          {notifications.unreadLabel && unreadCount > 0 ? (
            <span className="text-sm text-muted-foreground">{notifications.unreadLabel}</span>
          ) : null}
        </div>
        <DropdownMenuSeparator />
        {notifications.loading ? (
          <Empty>
            <EmptyHeader>
              <Heading>
                <Spinner></Spinner>
              </Heading>
            </EmptyHeader>
          </Empty>
        ) : notifications.error ? (
          <Empty>
            <EmptyHeader>
              <EmptyDescription>{notifications.error}</EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : notifications.items.length === 0 ? (
          <Empty>
            <EmptyHeader>
              <EmptyDescription>{notifications.emptyText}</EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          notifications.items.map((item) => (
            <DropdownMenuItem
              key={item.id}
              className="block whitespace-normal py-2"
              onClick={() => {
                if (item.href) {
                  window.location.assign(item.href);
                }
              }}
            >
              <span className="flex items-start gap-2">
                <span
                  className={cn(
                    "mt-1 h-1.5 w-1.5 shrink-0 rounded-full",
                    item.isRead ? "bg-muted-foreground/30" : "bg-destructive",
                  )}
                  aria-hidden="true"
                />
                <span className="min-w-0">
                  <span className="block truncate text-sm font-medium text-foreground">
                    {item.title}
                  </span>
                  {item.description ? (
                    <span className="mt-0.5 line-clamp-2 block text-xs text-muted-foreground">
                      {item.description}
                    </span>
                  ) : null}
                  {item.occurredAt ? (
                    <span className="mt-1 block text-[11px] text-muted-foreground">
                      {item.occurredAt}
                    </span>
                  ) : null}
                </span>
              </span>
            </DropdownMenuItem>
          ))
        )}
        {notifications.viewAllHref && notifications.viewAllLabel ? (
          <>
            <DropdownMenuSeparator />
            <DropdownMenuItem
              className="text-sm font-medium text-foreground"
              onClick={() => window.location.assign(notifications.viewAllHref!)}
            >
              {notifications.viewAllLabel}
            </DropdownMenuItem>
          </>
        ) : null}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export function AppShell({
  children,
  skipToContentLabel,
  mobileSidebarAriaLabel,
  mobileSidebarTitle = "Navigation",
  mobileSidebarDescription = "Displays the application navigation links.",
  sidebar,
  header,
  className,
  desktopSidebarClassName,
  mobileSidebarClassName,
  mainClassName,
  bodyClassName,
  contentSectionClassName,
  contentSurfaceClassName,
  contentFrameClassName,
  defaultDesktopSidebarCollapsed = false,
  desktopSidebarCollapsed,
  onDesktopSidebarCollapsedChange,
  style,
}: Readonly<{
  children: React.ReactNode;
  skipToContentLabel: string;
  mobileSidebarAriaLabel: string;
  mobileSidebarTitle?: string;
  mobileSidebarDescription?: string;
  sidebar: React.ReactNode;
  header: React.ReactNode;
  className?: string;
  desktopSidebarClassName?: string;
  mobileSidebarClassName?: string;
  mainClassName?: string;
  bodyClassName?: string;
  contentSectionClassName?: string;
  contentSurfaceClassName?: string;
  contentFrameClassName?: string;
  defaultDesktopSidebarCollapsed?: boolean;
  desktopSidebarCollapsed?: boolean;
  onDesktopSidebarCollapsedChange?: (collapsed: boolean) => void;
  style?: React.CSSProperties;
}>) {
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = React.useState(false);
  const [isDesktopSidebarCollapsedUncontrolled, setIsDesktopSidebarCollapsedUncontrolled] =
    React.useState(defaultDesktopSidebarCollapsed);
  const isDesktopSidebarCollapsed =
    desktopSidebarCollapsed ?? isDesktopSidebarCollapsedUncontrolled;

  const setDesktopSidebarCollapsed = React.useCallback(
    (collapsed: boolean) => {
      if (desktopSidebarCollapsed === undefined) {
        setIsDesktopSidebarCollapsedUncontrolled(collapsed);
      }

      onDesktopSidebarCollapsedChange?.(collapsed);
    },
    [desktopSidebarCollapsed, onDesktopSidebarCollapsedChange],
  );

  const toggleDesktopSidebarCollapsed = React.useCallback(() => {
    setDesktopSidebarCollapsed(!isDesktopSidebarCollapsed);
  }, [isDesktopSidebarCollapsed, setDesktopSidebarCollapsed]);

  return (
    <AppShellContext.Provider
      value={{
        setMobileSidebarOpen: setIsMobileSidebarOpen,
        isDesktopSidebarCollapsed,
        toggleDesktopSidebarCollapsed,
      }}
    >
      <div
        className={cn("flex h-dvh max-h-dvh bg-background text-foreground", className)}
        style={style}
        data-netmetric-app-background=""
      >
        <a
          href="#main-content"
          className="sr-only left-4 top-4 z-50 rounded-sm bg-background px-3 py-2 focus:not-sr-only focus:absolute"
        >
          {skipToContentLabel}
        </a>

        <aside
          className={cn(
            "hidden shrink-0 bg-background/80 transition-[width] overflow-hidden duration-200 ease-linear md:block",
            isDesktopSidebarCollapsed ? "w-16 px-3" : "w-[250px]",
            desktopSidebarClassName,
          )}
        >
          {sidebar}
        </aside>

        <Sheet open={isMobileSidebarOpen} onOpenChange={setIsMobileSidebarOpen}>
          <SheetContent
            side="left"
            className={cn(
              "w-[250px] bg-background p-0 text-foreground sm:w-[250px]",
              mobileSidebarClassName,
            )}
            aria-label={mobileSidebarAriaLabel}
          >
            <SheetHeader className="sr-only">
              <SheetTitle>{mobileSidebarTitle}</SheetTitle>
              <SheetDescription>{mobileSidebarDescription}</SheetDescription>
            </SheetHeader>
            {sidebar}
          </SheetContent>
        </Sheet>

        <main
          id="main-content"
          className={cn("min-h-0 min-w-0 flex-1 overflow-hidden", mainClassName)}
        >
          <div className={cn("flex h-full min-h-0 flex-col", bodyClassName)}>
            {header}
            <section
              className={cn(
                "min-h-0 min-w-0 flex-1 overflow-hidden px-3 lg:px-0 lg:pr-2 pb-3 rounded-md",
                contentSectionClassName,
              )}
            >
              <div
                className={cn(
                  "flex h-full min-h-0 min-w-0 flex-col overflow-hidden rounded-md",
                  contentSurfaceClassName,
                )}
              >
                {contentFrameClassName ? (
                  <div
                    className={cn(
                      "flex h-full min-h-0 min-w-0 flex-col overflow-hidden",
                      contentFrameClassName,
                    )}
                  >
                    {children}
                  </div>
                ) : (
                  children
                )}
              </div>
            </section>
          </div>
        </main>
      </div>
    </AppShellContext.Provider>
  );
}

export function AppHeader({
  mobileMenuAriaLabel,
  showMobileMenuButton = true,
  desktopSidebarToggle,
  actionSlots,
  leftContent,
  centerContent,
  rightContent,
  className,
}: Readonly<{
  mobileMenuAriaLabel?: string;
  showMobileMenuButton?: boolean;
  desktopSidebarToggle?: AppHeaderDesktopSidebarToggle;
  actionSlots?: AppHeaderActionSlots;
  leftContent?: React.ReactNode;
  centerContent?: React.ReactNode;
  rightContent?: React.ReactNode;
  className?: string;
}>) {
  const { setMobileSidebarOpen, isDesktopSidebarCollapsed, toggleDesktopSidebarCollapsed } =
    useAppShellContext();
  const desktopSidebarToggleAriaLabel = isDesktopSidebarCollapsed
    ? desktopSidebarToggle?.collapsedAriaLabel
    : desktopSidebarToggle?.expandedAriaLabel;

  return (
    <header
      className={cn(
        "flex h-14 items-center justify-between bg-background/70 px-3 lg:px-0 lg:pr-2 backdrop-blur-sm",
        className,
      )}
    >
      <div className="flex min-w-0 items-center gap-3">
        {showMobileMenuButton ? (
          <Button
            type="button"
            size="icon-sm"
            variant="outline"
            className="md:hidden"
            aria-label={mobileMenuAriaLabel}
            onClick={() => setMobileSidebarOpen(true)}
          >
            <MenuIcon className="h-4 w-4" />
          </Button>
        ) : null}
        {desktopSidebarToggle ? (
          <Button
            type="button"
            size="icon-sm"
            variant="outline"
            className={cn("hidden md:inline-flex", desktopSidebarToggle.className)}
            aria-label={desktopSidebarToggleAriaLabel}
            title={desktopSidebarToggleAriaLabel}
            onClick={toggleDesktopSidebarCollapsed}
          >
            {isDesktopSidebarCollapsed ? (
              <PanelLeftOpenIcon aria-hidden="true" className="h-4 w-4" />
            ) : (
              <PanelLeftCloseIcon aria-hidden="true" className="h-4 w-4" />
            )}
          </Button>
        ) : null}
        {actionSlots?.leading}
        {leftContent}
      </div>
      {centerContent ? <div className="mx-3 min-w-0 flex-1">{centerContent}</div> : null}
      {rightContent || actionSlots?.trailing ? (
        <div className="flex items-center gap-2">
          {actionSlots?.trailing}
          {rightContent}
        </div>
      ) : null}
    </header>
  );
}

export function AppWorkspaceHeader({
  mobileMenuAriaLabel,
  collapseSidebarAriaLabel,
  expandSidebarAriaLabel,
  searchPlaceholder,
  searchAriaLabel,
  searchReadOnly,
  onSearchClick,
  onSearchFocus,
  primaryActionLabel,
  primaryActionHref,
  primaryActionDisabled = false,
  primaryActionTitle,
  notifications,
  rightContent,
  className,
}: Readonly<AppWorkspaceHeaderProps>) {
  const isMounted = useMounted();
  const isPointerFocusRef = React.useRef(false);
  const primaryAction = primaryActionLabel ? (
    <button
      type="button"
      className="top-chip hidden h-8 items-center gap-2 rounded-sm border border-border bg-card px-3 text-[12px] text-muted-foreground transition-colors hover:bg-muted hover:text-foreground disabled:pointer-events-none disabled:opacity-50 md:flex"
      disabled={primaryActionDisabled}
      aria-disabled={primaryActionDisabled}
      title={primaryActionTitle ?? primaryActionLabel}
      onClick={() => {
        if (primaryActionHref && !primaryActionDisabled) {
          window.location.assign(primaryActionHref);
        }
      }}
    >
      <PlusIcon aria-hidden="true" className="h-4 w-4" />
      {primaryActionLabel}
    </button>
  ) : null;

  return (
    <AppHeader
      mobileMenuAriaLabel={mobileMenuAriaLabel}
      desktopSidebarToggle={{
        expandedAriaLabel: collapseSidebarAriaLabel,
        collapsedAriaLabel: expandSidebarAriaLabel,
      }}
      {...(className ? { className } : {})}
      leftContent={
        <>
          <div className="hidden items-center gap-2 rounded-md border border-border bg-card px-3 md:flex">
            <SearchIcon aria-hidden="true" className="h-4 w-4 text-muted-foreground" />
            <button
              type="button"
              data-testid="global-search-trigger"
              aria-label={searchAriaLabel ?? searchPlaceholder}
              aria-readonly={searchReadOnly || undefined}
              title={searchAriaLabel ?? searchPlaceholder}
              disabled={!isMounted}
              onClick={onSearchClick}
              onPointerDown={() => {
                isPointerFocusRef.current = true;
              }}
              onPointerUp={() => {
                isPointerFocusRef.current = false;
              }}
              onPointerCancel={() => {
                isPointerFocusRef.current = false;
              }}
              onBlur={() => {
                isPointerFocusRef.current = false;
              }}
              onFocus={() => {
                if (isPointerFocusRef.current) {
                  return;
                }

                onSearchFocus?.();
              }}
              onKeyDown={(event) => {
                if (event.key === "Enter" || event.key === " ") {
                  event.preventDefault();
                  onSearchClick?.();
                }
              }}
              className={cn(
                "h-8 w-96 bg-transparent text-left text-[13px] text-foreground outline-none disabled:cursor-wait disabled:opacity-60",
                (onSearchClick || onSearchFocus) && "cursor-pointer",
              )}
            >
              <span className="text-muted-foreground">{searchPlaceholder}</span>
            </button>
          </div>
        </>
      }
      rightContent={
        <>
          {primaryAction}
          <ThemeToggle />
          {notifications ? <AppHeaderNotificationsMenu notifications={notifications} /> : null}
          {rightContent}
        </>
      }
    />
  );
}

export function AppWorkspaceShell({
  children,
  labels,
  pathname,
  navGroups,
  user,
  userLabels,
  userActions,
  userOverflowActions,
  header,
  sidebarFooter,
  sidebarNavStateKey,
  renderLink,
  className,
  bodyClassName,
  contentClassName,
  contentFrameClassName,
  backgroundStyle,
}: Readonly<AppWorkspaceShellProps>) {
  const [isSidebarCollapsed, setIsSidebarCollapsed] = React.useState(false);

  return (
    <AppShell
      skipToContentLabel={labels.skipToContent}
      mobileSidebarAriaLabel={labels.mobileSidebarAriaLabel}
      mobileSidebarTitle={labels.mobileSidebarTitle}
      desktopSidebarCollapsed={isSidebarCollapsed}
      onDesktopSidebarCollapsedChange={setIsSidebarCollapsed}
      className={cn(className)}
      {...(backgroundStyle ? { style: backgroundStyle } : {})}
      {...(bodyClassName ? { bodyClassName } : {})}
      {...(contentClassName ? { contentSurfaceClassName: contentClassName } : {})}
      {...(contentFrameClassName ? { contentFrameClassName } : {})}
      sidebar={
        <AppSidebar
          collapsed={isSidebarCollapsed}
          brand={
            <AppSidebarUserHeader
              user={user}
              labels={userLabels}
              collapsed={isSidebarCollapsed}
              actions={userActions}
              {...(userOverflowActions ? { overflowActions: userOverflowActions } : {})}
            />
          }
          nav={
            <AppSidebarNav
              groups={navGroups}
              pathname={pathname}
              collapsed={isSidebarCollapsed}
              ariaLabel={labels.mobileSidebarAriaLabel}
              {...(sidebarNavStateKey ? { stateStorageKey: sidebarNavStateKey } : {})}
              {...(renderLink ? { renderLink } : {})}
            />
          }
          footer={sidebarFooter}
        />
      }
      header={<AppWorkspaceHeader {...header} />}
    >
      {children}
    </AppShell>
  );
}

export function AppSidebar({
  brand,
  nav,
  footer,
  collapsed = false,
  className,
}: Readonly<{
  brand?: React.ReactNode;
  nav?: React.ReactNode;
  footer?: React.ReactNode;
  collapsed?: boolean;
  className?: string;
}>) {
  return (
    <div
      className={cn(
        "flex h-full flex-col p-2 space-y-2",
        collapsed && "md:items-center md:px-0 md:py-2",
        className,
      )}
    >
      {brand}
      {nav ? (
        <div
          className={cn(
            "min-h-0 flex-1 pt-4 overflow-y-auto [scrollbar-width:none] [-ms-overflow-style:none] [&::-webkit-scrollbar]:hidden",
            collapsed && "md:w-full",
          )}
        >
          {nav}
        </div>
      ) : null}
      {footer ? <div className={cn("mt-auto pt-3", collapsed && "md:hidden")}>{footer}</div> : null}
    </div>
  );
}

export function AppSidebarNav({
  groups,
  pathname,
  ariaLabel,
  collapsed,
  renderLink,
  stateStorageKey,
  className,
  groupLabelClassName,
}: Readonly<{
  groups: readonly AppSidebarNavGroup[];
  pathname: string;
  ariaLabel: string;
  collapsed?: boolean;
  renderLink?: AppSidebarLinkRenderer;
  stateStorageKey?: string;
  className?: string;
  groupLabelClassName?: string;
}>) {
  const isMobile = useIsMobile();
  const [expandedGroups, setExpandedGroups] = React.useState<Record<string, boolean>>(() =>
    Object.fromEntries(
      groups.map((group, groupIndex) => {
        const groupKey = group.id ?? `group-${groupIndex}`;
        return [groupKey, group.defaultExpanded ?? true];
      }),
    ),
  );

  React.useEffect(() => {
    setExpandedGroups((prev) => {
      const next: Record<string, boolean> = {};
      groups.forEach((group, groupIndex) => {
        const groupKey = group.id ?? `group-${groupIndex}`;
        next[groupKey] = prev[groupKey] ?? group.defaultExpanded ?? true;
      });
      return next;
    });
  }, [groups]);

  React.useEffect(() => {
    if (!stateStorageKey) {
      return;
    }

    try {
      const raw = window.localStorage.getItem(stateStorageKey);
      if (!raw) {
        return;
      }

      const parsed = JSON.parse(raw) as Record<string, boolean>;
      if (!parsed || typeof parsed !== "object") {
        return;
      }

      setExpandedGroups((prev) => {
        const next = { ...prev };
        for (const [groupKey, value] of Object.entries(parsed)) {
          if (typeof value === "boolean" && groupKey in next) {
            next[groupKey] = value;
          }
        }
        return next;
      });
    } catch {
      // ignore invalid/missing persisted nav group state
    }
  }, [stateStorageKey, groups]);

  React.useEffect(() => {
    if (!stateStorageKey) {
      return;
    }

    try {
      window.localStorage.setItem(stateStorageKey, JSON.stringify(expandedGroups));
    } catch {
      // ignore write failures
    }
  }, [expandedGroups, stateStorageKey]);

  return (
    <nav className={cn("space-y-3", collapsed && "md:w-full", className)} aria-label={ariaLabel}>
      {groups.map((group, groupIndex) => {
        const groupKey = group.id ?? `group-${groupIndex}`;
        const isCollapsible = Boolean(group.collapsible && !collapsed);
        const isExpanded = expandedGroups[groupKey] ?? group.defaultExpanded ?? true;

        return (
          <div key={groupKey}>
            {group.label ? (
              isCollapsible ? (
                <button
                  type="button"
                  className={cn(
                    "mb-2 flex w-full items-center gap-1.5 rounded-sm px-1 py-1 text-left text-[11px] font-medium tracking-wide text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground",
                    groupLabelClassName,
                  )}
                  aria-expanded={isExpanded}
                  onClick={() =>
                    setExpandedGroups((prev) => ({ ...prev, [groupKey]: !isExpanded }))
                  }
                >
                  {isExpanded ? (
                    <ChevronDownIcon aria-hidden="true" className="h-3.5 w-3.5 shrink-0" />
                  ) : (
                    <ChevronRightIcon aria-hidden="true" className="h-3.5 w-3.5 shrink-0" />
                  )}
                  <span>{group.label}</span>
                </button>
              ) : (
                <div
                  className={cn(
                    "mb-2 px-1 text-[11px] font-medium tracking-wide text-muted-foreground",
                    collapsed && "md:sr-only",
                    groupLabelClassName,
                  )}
                >
                  {group.label}
                </div>
              )
            ) : null}
            {isCollapsible && !isExpanded ? null : (
              <ul className={cn("space-y-0.5", collapsed && "md:flex md:flex-col md:items-center")}>
                {group.items.map((item, itemIndex) => (
                  <AppSidebarItem
                    key={item.id ?? item.href ?? `item-${groupIndex}-${itemIndex}`}
                    item={item}
                    pathname={pathname}
                    isMobile={isMobile}
                    {...(collapsed !== undefined ? { collapsed } : {})}
                    {...(renderLink ? { renderLink } : {})}
                  />
                ))}
              </ul>
            )}
          </div>
        );
      })}
    </nav>
  );
}

export function AppSidebarItem({
  item,
  pathname,
  collapsed,
  isMobile,
  renderLink,
  className,
}: Readonly<{
  item: AppSidebarNavItem;
  pathname: string;
  collapsed?: boolean;
  isMobile?: boolean;
  renderLink?: AppSidebarLinkRenderer;
  className?: string;
}>) {
  const isActive = isAppSidebarItemActive(pathname, item);
  const isDisabled = item.disabled || !item.href;
  const Icon = item.icon;
  const tooltipContent = getTooltipContent(item);
  const linkClassName = cn(
    "flex h-8 w-full items-center gap-2 rounded-sm pl-1 pr-2.5 text-left text-[14px] text-muted-foreground transition-colors hover:bg-accent hover:text-accent-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
    isActive && "bg-accent text-accent-foreground",
    collapsed && "md:size-10 md:w-10 md:justify-center md:px-0",
    isDisabled && "pointer-events-none opacity-60",
    className,
  );
  const content = (
    <>
      {Icon ? (
        <Icon
          aria-hidden="true"
          focusable="false"
          className={cn("h-4 w-4 shrink-0", item.iconClassName)}
        />
      ) : null}
      <span className={cn("flex items-center pt-0.5 text-[13px]", collapsed && "md:sr-only")}>
        {item.label}
      </span>
      {item.badge ? (
        <span className={cn("ml-auto text-[11px]", collapsed && "md:hidden")}>{item.badge}</span>
      ) : null}
    </>
  );

  const itemElement =
    item.href && !isDisabled ? (
      renderLink ? (
        renderLink({
          href: item.href,
          className: linkClassName,
          children: content,
          ...(isActive ? { "aria-current": "page" as const } : {}),
          ...(item.ariaLabel ? { "aria-label": item.ariaLabel } : {}),
          ...(item.title ? { title: item.title } : {}),
        })
      ) : (
        <a
          href={item.href}
          className={linkClassName}
          aria-current={isActive ? "page" : undefined}
          aria-label={item.ariaLabel}
          title={item.title}
        >
          {content}
        </a>
      )
    ) : (
      <button
        type="button"
        className={linkClassName}
        aria-disabled="true"
        disabled
        aria-label={item.ariaLabel}
        title={item.title}
      >
        {content}
      </button>
    );

  return (
    <li>
      {collapsed && tooltipContent ? (
        <Tooltip>
          <TooltipTrigger render={itemElement} />
          <TooltipContent side="right" align="center" hidden={isMobile}>
            {tooltipContent}
          </TooltipContent>
        </Tooltip>
      ) : (
        itemElement
      )}
    </li>
  );
}

export function AppSidebarUserMenu({
  user,
  actions,
  overflowActions,
  triggerAriaLabel,
  overflowAriaLabel,
  menuLabel,
  collapsed = false,
  className,
}: Readonly<AppSidebarUserMenuProps>) {
  const hasOverflowActions = Boolean(overflowActions?.length);

  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        className={cn(
          "flex w-full items-center gap-2 rounded-sm px-2 py-1.5 text-left transition-colors hover:bg-muted/60 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring",
          collapsed && "md:size-10 md:w-10 md:justify-center md:p-0",
          className,
        )}
        aria-label={triggerAriaLabel}
      >
        <div className={cn("flex shrink-0 items-center justify-center", collapsed && "md:size-6")}>
          {user.avatar}
        </div>
        <div className={cn("min-w-0", collapsed && "md:sr-only")}>
          <div className="truncate text-[13px] text-foreground">{user.name}</div>
          {user.secondaryText ? (
            <div className="truncate text-[12px] text-muted-foreground">{user.secondaryText}</div>
          ) : null}
        </div>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="start" side="top" sideOffset={6} className="w-56 min-w-56">
        <div className="flex items-start justify-between gap-2 px-1.5 py-1">
          <div className="min-w-0">
            {menuLabel ? (
              <DropdownMenuLabel className="px-0 py-0 text-foreground">
                {menuLabel}
              </DropdownMenuLabel>
            ) : null}
            <p className="truncate text-sm font-medium text-foreground">{user.name}</p>
            {user.secondaryText ? (
              <p className="truncate text-xs text-muted-foreground">{user.secondaryText}</p>
            ) : null}
          </div>
          {hasOverflowActions ? (
            <DropdownMenuSub>
              <DropdownMenuSubTrigger
                aria-label={overflowAriaLabel}
                className="h-8 w-7 justify-center p-0"
                hideChevron
              >
                <MoreHorizontalIcon aria-hidden="true" className="h-4 w-4" />
              </DropdownMenuSubTrigger>
              <DropdownMenuSubContent side="right" align="start" className="w-48 min-w-48">
                {overflowActions?.map((action) => renderUserMenuAction(action))}
              </DropdownMenuSubContent>
            </DropdownMenuSub>
          ) : null}
        </div>
        <DropdownMenuSeparator />
        {actions.map((action) => renderUserMenuAction(action))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

export function AppSidebarUserHeader({
  user,
  labels,
  actions,
  overflowActions,
  collapsed = false,
  className,
}: Readonly<{
  user: AppShellUserIdentity;
  labels: AppSidebarUserHeaderLabels;
  actions: readonly AppSidebarUserMenuAction[];
  overflowActions?: readonly AppSidebarUserMenuAction[];
  collapsed?: boolean;
  className?: string;
}>) {
  const name = resolveUserName(user, labels.fallbackName ?? "User");
  const secondaryText = labels.secondaryText ?? user.email ?? undefined;
  const initials = getUserInitials(name);

  return (
    <AppSidebarUserMenu
      user={{
        name,
        avatar: (
          <Avatar size="sm" className="overflow-hidden">
            <AvatarImage src={user.avatarUrl ?? undefined} alt={name} className="object-center" />
            <AvatarFallback>{initials}</AvatarFallback>
          </Avatar>
        ),
        ...(secondaryText ? { secondaryText } : {}),
      }}
      collapsed={collapsed}
      triggerAriaLabel={labels.triggerAriaLabel}
      overflowAriaLabel={labels.overflowAriaLabel}
      actions={actions}
      {...(overflowActions ? { overflowActions } : {})}
      {...(className ? { className } : {})}
    />
  );
}
