import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";
import { tCrm } from "@/lib/i18n/crm-i18n";

export function CrmDeleteZone({
  title,
  description,
  locale,
  dangerTitle,
  dangerDescription,
  children,
}: Readonly<{
  title: string;
  description: string;
  locale?: string | null;
  dangerTitle?: string;
  dangerDescription?: string;
  children: React.ReactNode;
}>) {
  return (
    <div className="space-y-4">
      <div className="space-y-1">
        <h3 className="text-sm font-semibold text-destructive">{title}</h3>
        <p className="text-xs text-muted-foreground">{description}</p>
      </div>
      <div className="space-y-4">
        <Alert variant="destructive">
          <AlertTitle>{dangerTitle ?? tCrm("crm.delete.dangerTitle", locale)}</AlertTitle>
          <AlertDescription>
            {dangerDescription ?? tCrm("crm.delete.cannotUndo", locale)}
          </AlertDescription>
        </Alert>
        {children}
      </div>
    </div>
  );
}
