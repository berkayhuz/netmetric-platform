import { Alert, AlertDescription, AlertTitle } from "@netmetric/ui";

type ToolHistoryActionResultProps = {
  status: "idle" | "success" | "error";
  message: string;
};

export function ToolHistoryActionResult({ status, message }: ToolHistoryActionResultProps) {
  if (status === "idle" || !message) {
    return null;
  }

  return (
    <Alert>
      <AlertTitle>{status === "success" ? "Success" : "Action failed"}</AlertTitle>
      <AlertDescription>{message}</AlertDescription>
    </Alert>
  );
}
