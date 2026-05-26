type SelectDisplayValue = string | number | boolean | null | undefined;

type SelectDisplayOption = {
  value: SelectDisplayValue;
  label: string;
};

export function getSelectDisplayLabel(
  value: SelectDisplayValue,
  options: readonly SelectDisplayOption[],
  emptyLabel = "-",
): string {
  if (value == null) {
    return emptyLabel;
  }

  const normalizedValue = String(value);
  if (normalizedValue.length === 0 || normalizedValue === "__none__") {
    return emptyLabel;
  }

  const matchedOption = options.find((option) => String(option.value) === normalizedValue);
  return matchedOption?.label ?? normalizedValue;
}
