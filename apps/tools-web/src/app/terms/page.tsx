import type { Metadata } from "next";
import { Heading, Text } from "@netmetric/ui";

import { ToolsShell } from "@/features/tools/components/tools-shell";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { createPageMetadata } from "@/lib/seo";

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return createPageMetadata(
    tTools("tools.terms.metaTitle", locale),
    tTools("tools.terms.metaDescription", locale),
    "/terms",
  );
}

export default async function TermsPage() {
  const locale = await getRequestLocale();

  return (
    <ToolsShell>
      <section aria-labelledby="terms-heading" className="space-y-4">
        <Heading id="terms-heading" level={1}>
          {tTools("tools.terms.title", locale)}
        </Heading>
        <Text className="text-muted-foreground">{tTools("tools.terms.description", locale)}</Text>
      </section>
    </ToolsShell>
  );
}
