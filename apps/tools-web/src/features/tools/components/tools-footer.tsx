import Link from "next/link";

import { tTools } from "@/lib/i18n/tools-i18n";

export function ToolsFooter({ locale }: { locale?: string | null | undefined }) {
  return (
    <footer className="border-t">
      <div className="mx-auto flex w-full max-w-6xl flex-col gap-3 px-4 py-6 text-sm text-muted-foreground sm:flex-row sm:items-center sm:justify-between sm:px-6 lg:px-8">
        <p>{tTools("tools.footer.description", locale)}</p>
        <nav aria-label={tTools("tools.a11y.footerNavigation", locale)} className="flex gap-4">
          <Link href="/privacy" className="hover:underline">
            {tTools("tools.nav.privacy", locale)}
          </Link>
          <Link href="/terms" className="hover:underline">
            {tTools("tools.nav.terms", locale)}
          </Link>
        </nav>
      </div>
    </footer>
  );
}
