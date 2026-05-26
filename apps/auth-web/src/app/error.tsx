"use client";

import { Alert, AlertDescription, AlertTitle, Button } from "@netmetric/ui";

import { tClient } from "@/features/auth/i18n/auth-i18n.client";

type ErrorPageProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function ErrorPage({ reset }: ErrorPageProps) {
  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-6 text-foreground">
      <div className="w-full max-w-md space-y-4">
        <Alert variant="destructive">
          <AlertTitle>{tClient("error.unexpectedTitle")}</AlertTitle>
          <AlertDescription>{tClient("error.unexpectedDescription")}</AlertDescription>
        </Alert>
        <Button type="button" onClick={reset} variant="outline" className="w-full">
          {tClient("common.retry")}
        </Button>
      </div>
    </main>
  );
}
