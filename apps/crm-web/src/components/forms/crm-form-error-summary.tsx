"use client";

import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

export function CrmFormErrorSummary({
  message,
  errors,
  title,
}: Readonly<{
  message?: string;
  errors?: Record<string, string[]>;
  title?: string;
}>) {
  if (!message && (!errors || Object.keys(errors).length === 0)) {
    return null;
  }

  return (
    <Alert variant="destructive" role="alert" aria-live="assertive">
      <AlertTitle>{title ?? tCrmClient("crm.forms.errors.reviewTitle")}</AlertTitle>
      <AlertDescription>
        <div className="space-y-1">
          {message ? <p>{message}</p> : null}
          {errors
            ? Object.entries(errors).map(([key, fieldErrors]) => <p key={key}>{fieldErrors[0]}</p>)
            : null}
        </div>
      </AlertDescription>
    </Alert>
  );
}
