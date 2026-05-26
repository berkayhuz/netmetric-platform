"use client";

import { Alert, AlertDescription, AlertTitle, Button, Card, CardContent } from "@netmetric/ui";

import { tClient } from "@/features/auth/i18n/auth-i18n.client";

type GlobalErrorProps = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function GlobalError({ reset }: GlobalErrorProps) {
  return (
    <html lang="en">
      <body>
        <main className="flex min-h-screen items-center justify-center px-6">
          <Card className="w-full max-w-md">
            <CardContent className="space-y-4 p-6 text-center">
              <Alert variant="destructive">
                <AlertTitle>{tClient("error.criticalTitle")}</AlertTitle>
                <AlertDescription>{tClient("error.criticalDescription")}</AlertDescription>
              </Alert>
              <Button type="button" onClick={reset} className="w-full">
                {tClient("common.retry")}
              </Button>
            </CardContent>
          </Card>
        </main>
      </body>
    </html>
  );
}
