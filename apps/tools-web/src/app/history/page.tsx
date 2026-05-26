import type { Metadata } from "next";
import { Heading, Text } from "@netmetric/ui";

import { ToolHistoryList } from "@/features/tools/history/tool-history-list";
import { getRequestLocale } from "@/lib/i18n/request-locale";
import { tTools } from "@/lib/i18n/tools-i18n";
import { createNoIndexMetadata } from "@/lib/seo";
import { handleToolsApiPageError } from "@/lib/tools-auth/handle-tools-api-page-error";
import { requireToolsSession } from "@/lib/tools-auth/require-tools-session";
import { toolsApiClient, type ToolHistoryPageResponse } from "@/lib/tools-api";
import { getToolsApiRequestOptions } from "@/lib/tools-api/tools-api-request-options";

export async function generateMetadata(): Promise<Metadata> {
  const locale = await getRequestLocale();

  return createNoIndexMetadata(
    tTools("tools.history.metaTitle", locale),
    tTools("tools.history.metaDescription", locale),
    "/history",
  );
}

export default async function HistoryPage() {
  const locale = await getRequestLocale();
  await requireToolsSession("/history");

  let response: ToolHistoryPageResponse;
  try {
    const requestOptions = await getToolsApiRequestOptions();
    response = await toolsApiClient.getHistory({ page: 1, pageSize: 20 }, requestOptions);
  } catch (error) {
    handleToolsApiPageError(error, "/history");
  }

  return (
    <section className="mx-auto w-full max-w-4xl space-y-6 px-4 py-10 sm:px-6 lg:px-8">
      <div className="space-y-2">
        <Heading level={1}>{tTools("tools.history.title", locale)}</Heading>
        <Text className="text-muted-foreground">{tTools("tools.history.description", locale)}</Text>
      </div>
      <ToolHistoryList response={response} locale={locale} />
    </section>
  );
}
