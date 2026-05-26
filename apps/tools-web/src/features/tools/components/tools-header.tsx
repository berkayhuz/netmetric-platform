import Link from "next/link";
import { Button } from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";
import { toolsEnv } from "@/lib/tools-env";

export function ToolsHeader({
  locale,
  isAuthenticated,
}: {
  locale?: string | null | undefined;
  isAuthenticated: boolean;
}) {
  return (
    <header className="border-b">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
        <div>
          <Link href="/" className="text-lg font-semibold tracking-tight">
            {toolsEnv.appName}
          </Link>
          <p className="text-sm text-muted-foreground">{tTools("tools.header.tagline", locale)}</p>
        </div>
        <nav
          aria-label={tTools("tools.a11y.primaryNavigation", locale)}
          className="flex items-center gap-2"
        >
          <Button asChild variant="ghost" size="sm">
            <Link href="/categories">{tTools("tools.nav.categories", locale)}</Link>
          </Button>
          <Button asChild variant="outline" size="sm">
            <Link href="/history">{tTools("tools.nav.history", locale)}</Link>
          </Button>
          {isAuthenticated ? (
            <>
              <Button asChild size="sm">
                <Link href="/history">{tTools("tools.actions.goToHistory", locale)}</Link>
              </Button>
              <form action="/api/auth/logout" method="post">
                <Button type="submit" variant="ghost" size="sm">
                  {tTools("tools.actions.signOut", locale)}
                </Button>
              </form>
            </>
          ) : (
            <Button asChild size="sm">
              <Link href={toolsEnv.authUrl}>{tTools("tools.actions.signIn", locale)}</Link>
            </Button>
          )}
        </nav>
      </div>
    </header>
  );
}
