import { useCallback, useState } from "react";

type UseControllableStateOptions<TValue> = {
  value?: TValue;
  defaultValue: TValue;
  onChange?: (value: TValue) => void;
};

export function useControllableState<TValue>({
  value,
  defaultValue,
  onChange,
}: UseControllableStateOptions<TValue>) {
  const [internalValue, setInternalValue] = useState(defaultValue);

  const isControlled = value !== undefined;
  const currentValue = isControlled ? value : internalValue;

  const setValue = useCallback(
    (nextValue: TValue) => {
      if (!isControlled) {
        setInternalValue(nextValue);
      }

      onChange?.(nextValue);
    },
    [isControlled, onChange],
  );

  return [currentValue, setValue] as const;
}
