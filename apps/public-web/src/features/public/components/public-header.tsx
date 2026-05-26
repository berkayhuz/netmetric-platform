"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

import { Button, Heading, cn } from "@netmetric/ui";
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from "@netmetric/ui/client";
import { Menu } from "lucide-react";

import { publicEnv } from "@/lib/public-env";

type PublicNavLink = {
  href: string;
  label: string;
};

type PublicHeaderCopy = {
  primaryAria: string;
  signIn: string;
  account: string;
  dashboard: string;
  signOut: string;
  openCrm: string;
  openMenu: string;
  navigationTitle: string;
};

type PublicHeaderSession = {
  isAuthenticated: boolean;
};

function PublicNavLinks({
  links,
  primaryAria,
  mobile = false,
}: {
  links: readonly PublicNavLink[];
  primaryAria: string;
  mobile?: boolean;
}) {
  const pathname = usePathname();

  return (
    <nav
      aria-label={primaryAria}
      className={mobile ? "grid gap-1.5" : "hidden items-center gap-1 lg:flex"}
    >
      {links.map((link) => {
        const active = pathname === link.href;
        return (
          <Link
            key={link.href}
            href={link.href}
            className={cn(
              "text-sm transition-all duration-200 px-3 py-1.5 rounded-md",
              active
                ? "text-foreground font-semibold"
                : "text-muted-foreground hover:text-foreground",
            )}
          >
            {link.label}
          </Link>
        );
      })}
    </nav>
  );
}

export function PublicHeader({
  links,
  copy,
  session,
}: {
  links: readonly PublicNavLink[];
  copy: PublicHeaderCopy;
  session: PublicHeaderSession;
}) {
  const isAuthenticated = session.isAuthenticated;
  const pathname = usePathname();

  return (
    <header className="sticky top-0 z-50 w-full border-b border-border/40 bg-background/80 backdrop-blur-md supports-[backdrop-filter]:bg-background/60">
      <div className="mx-auto flex h-16 w-full max-w-screen-xl items-center justify-between px-4 sm:px-6 lg:px-8">
        {/* Left: Brand Logo & Title */}
        <div className="flex items-center">
          <Link
            href="/"
            className="group flex items-center gap-2.5 transition-opacity hover:opacity-90"
          >
            <span className="inline-flex h-8 w-8 items-center justify-center text-white transition-all duration-300 dark:text-white">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                role="img"
                focusable="false"
                className="shrink-0 fill-current"
              >
                <path d="M4 0h16a4 4 0 014 4v16a4 4 0 01-4 4h-16a4 4 0 01-4-4v-16a4 4 0 014-4m1 18a1 1 0 002 0l0-4a1 1 0 00-2 0m4 4a1 1 0 002 0l0-9a1 1 0 00-2 0m4 9a1 1 0 002 0l0-12a1 1 0 00-2 0m4 12a1 1 0 002 0l0-7a1 1 0 00-2 0"></path>
              </svg>
            </span>
            <Heading level={3} className="tracking-tight">
              NetMetric
            </Heading>
          </Link>
        </div>

        {/* Center: Desktop Navigation */}
        <div className="hidden lg:block">
          <PublicNavLinks links={links} primaryAria={copy.primaryAria} />
        </div>

        {/* Right: Action Buttons / Mobile Menu Trigger */}
        <div className="flex items-center gap-3">
          {/* Desktop Authentication / Dashboard Actions */}
          <div className="hidden items-center gap-3 lg:flex">
            {isAuthenticated ? (
              <>
                <Button
                  asChild
                  variant="outline"
                  size="sm"
                  className="rounded-full px-4 h-9 font-medium transition-transform active:scale-[0.98]"
                >
                  <a href={publicEnv.accountUrl}>{copy.account}</a>
                </Button>
                <Button
                  asChild
                  size="sm"
                  className="rounded-full px-4 h-9 font-medium shadow-sm transition-transform active:scale-[0.98]"
                >
                  <a href={publicEnv.crmUrl}>{copy.dashboard}</a>
                </Button>
                <form action="/api/auth/logout" method="post">
                  <Button
                    type="submit"
                    variant="ghost"
                    size="sm"
                    className="rounded-full px-4 h-9 font-medium transition-colors text-muted-foreground hover:text-foreground"
                  >
                    {copy.signOut}
                  </Button>
                </form>
              </>
            ) : (
              <>
                <Button
                  asChild
                  size="sm"
                  className="rounded-full px-4 h-9 font-medium shadow-sm bg-neutral-900 text-white hover:bg-neutral-800 dark:bg-white dark:text-black dark:hover:bg-neutral-100 transition-transform active:scale-[0.98]"
                >
                  <a href={publicEnv.authUrl}>{copy.signIn}</a>
                </Button>
              </>
            )}
          </div>

          {/* Mobile Drawer Trigger (Sheet) */}
          <Sheet>
            <SheetTrigger asChild>
              <Button
                variant="ghost"
                size="icon"
                className="lg:hidden rounded-full hover:bg-neutral-100 dark:hover:bg-neutral-900 transition-all active:scale-[0.95]"
                aria-label={copy.openMenu}
              >
                <Menu className={cn("size-5")} />
              </Button>
            </SheetTrigger>
            <SheetContent
              side="right"
              className="w-[300px] border-l border-border/40 bg-background/95 backdrop-blur-md p-6 flex flex-col justify-between"
            >
              <div>
                <SheetHeader className="mb-6">
                  <SheetTitle className="text-left font-bold text-xl tracking-tight bg-gradient-to-r from-foreground to-foreground/80 bg-clip-text text-transparent">
                    {copy.navigationTitle}
                  </SheetTitle>
                </SheetHeader>

                {/* Mobile Nav Links list */}
                <div className="grid gap-1 py-2">
                  {links.map((link) => {
                    const active = pathname === link.href;
                    return (
                      <Link
                        key={link.href}
                        href={link.href}
                        className={cn(
                          "text-base font-medium py-2.5 px-3 rounded-lg transition-all duration-200 flex items-center justify-between",
                          active
                            ? "bg-neutral-100 text-foreground dark:bg-neutral-900 font-semibold"
                            : "text-muted-foreground hover:text-foreground hover:bg-neutral-100/50 dark:hover:bg-neutral-900/50",
                        )}
                      >
                        <span>{link.label}</span>
                        {active && <span className="h-1.5 w-1.5 rounded-full bg-foreground" />}
                      </Link>
                    );
                  })}
                </div>
              </div>

              {/* Mobile Actions Block */}
              <div className="grid gap-3 border-t border-border/40 pt-6">
                {isAuthenticated ? (
                  <>
                    <Button
                      asChild
                      variant="outline"
                      className="w-full rounded-xl py-5 font-medium transition-transform active:scale-[0.98]"
                    >
                      <a href={publicEnv.accountUrl}>{copy.account}</a>
                    </Button>
                    <Button
                      asChild
                      className="w-full rounded-xl py-5 font-medium transition-transform active:scale-[0.98]"
                    >
                      <a href={publicEnv.crmUrl}>{copy.dashboard}</a>
                    </Button>
                    <form action="/api/auth/logout" method="post" className="w-full">
                      <Button
                        type="submit"
                        variant="ghost"
                        className="w-full rounded-xl py-5 font-medium text-muted-foreground hover:text-foreground"
                      >
                        {copy.signOut}
                      </Button>
                    </form>
                  </>
                ) : (
                  <>
                    <Button
                      asChild
                      variant="outline"
                      className="w-full rounded-xl py-5 font-medium transition-transform active:scale-[0.98]"
                    >
                      <a href={publicEnv.authUrl}>{copy.signIn}</a>
                    </Button>
                    <Button
                      asChild
                      className="w-full rounded-xl py-5 font-medium bg-neutral-900 text-white hover:bg-neutral-800 dark:bg-white dark:text-black dark:hover:bg-neutral-100 transition-transform active:scale-[0.98]"
                    >
                      <a href={publicEnv.crmUrl}>{copy.openCrm}</a>
                    </Button>
                  </>
                )}
              </div>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    </header>
  );
}
