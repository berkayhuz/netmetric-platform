import { Text } from "@netmetric/ui";

export function CrmReadonlyField({
  label,
  value,
}: Readonly<{
  label: string;
  value: string | number | boolean | null | undefined;
}>) {
  return (
    <div className="space-y-1">
      <Text className="text-xs uppercase tracking-wide text-muted-foreground">{label}</Text>
      <Text>{value === null || value === undefined || value === "" ? "-" : String(value)}</Text>
    </div>
  );
}
