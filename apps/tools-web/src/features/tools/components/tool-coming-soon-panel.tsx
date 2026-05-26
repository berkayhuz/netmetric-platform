import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

import { tTools } from "@/lib/i18n/tools-i18n";

export function ToolComingSoonPanel({ locale }: { locale?: string | null | undefined }) {
  return (
    <Alert>
      <AlertTitle>{tTools("tools.status.comingSoon", locale)}</AlertTitle>
      <AlertDescription>{tTools("tools.comingSoon.description", locale)}</AlertDescription>
    </Alert>
  );
}
