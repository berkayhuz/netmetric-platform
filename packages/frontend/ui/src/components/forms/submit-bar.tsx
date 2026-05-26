import { Button } from "../primitives/button";

export function SubmitBar({
  isPending,
  submitLabel,
  pendingLabel,
  cancelLabel,
  onCancel,
  align = "end",
}: Readonly<{
  isPending: boolean;
  submitLabel: string;
  pendingLabel?: string;
  cancelLabel: string;
  onCancel: () => void;
  align?: "end" | "between";
}>) {
  return (
    <div
      className={
        align === "between"
          ? "flex flex-col-reverse gap-3 sm:flex-row sm:items-center sm:justify-between"
          : "flex flex-col-reverse gap-3 sm:flex-row sm:justify-end"
      }
    >
      <Button type="button" variant="outline" onClick={onCancel}>
        {cancelLabel}
      </Button>
      <Button type="submit" disabled={isPending} aria-busy={isPending}>
        {isPending ? (pendingLabel ?? submitLabel) : submitLabel}
      </Button>
    </div>
  );
}
