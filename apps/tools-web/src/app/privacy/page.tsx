import type { Metadata } from "next";
import { Heading, Text } from "@netmetric/ui";

import { ToolsShell } from "@/features/tools/components/tools-shell";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { createPageMetadata } from "@/lib/seo";

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return createPageMetadata(
    tTools("tools.privacy.metaTitle", locale),
    tTools("tools.privacy.metaDescription", locale),
    "/privacy",
  );
}

export default async function PrivacyPage() {
  const locale = await getRequestLocale();

  return (
    <ToolsShell>
      <section aria-labelledby="privacy-heading" className="space-y-4">
        <Heading id="privacy-heading" level={1}>
          {tTools("tools.privacy.title", locale)}
        </Heading>
        <Text className="text-muted-foreground">{tTools("tools.privacy.description", locale)}</Text>
      </section>
    </ToolsShell>
  );
}
