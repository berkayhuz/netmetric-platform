import { Empty, EmptyDescription, EmptyHeader, EmptyTitle } from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";
import type { ToolHistoryPageResponse } from "@/lib/tools-api";

import { ToolHistoryItem } from "./tool-history-item";

type ToolHistoryListProps = {
  response: ToolHistoryPageResponse;
  locale?: string | null | undefined;
};

export function ToolHistoryList({ response, locale }: ToolHistoryListProps) {
  if (response.items.length === 0) {
    return (
      <Empty>
        <EmptyHeader>
          <EmptyTitle>{tTools("tools.history.emptyTitle", locale)}</EmptyTitle>
          <EmptyDescription>{tTools("tools.history.emptyDescription", locale)}</EmptyDescription>
        </EmptyHeader>
      </Empty>
    );
  }

  return (
    <div className="space-y-4">
      {response.items.map((item) => (
        <ToolHistoryItem key={item.runId} item={item} locale={locale} />
      ))}
    </div>
  );
}
