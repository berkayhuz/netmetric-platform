import { Text } from "@netmetric/ui";

import { tAccountClient } from "@/lib/i18n/account-i18n";

type ReadOnlyValueProps = {
  value: string | number | boolean | null | undefined;
  emptyLabel?: string;
};

export function ReadOnlyValue({ value, emptyLabel }: ReadOnlyValueProps) {
  const fallbackEmptyLabel = emptyLabel ?? tAccountClient("account.common.notAvailable");

  if (value === null || value === undefined || value === "") {
    return <Text className="text-muted-foreground">{fallbackEmptyLabel}</Text>;
  }

  if (typeof value === "boolean") {
    return (
      <Text>
        {value
          ? tAccountClient("account.common.boolean.yes")
          : tAccountClient("account.common.boolean.no")}
      </Text>
    );
  }

  return <Text>{String(value)}</Text>;
}
