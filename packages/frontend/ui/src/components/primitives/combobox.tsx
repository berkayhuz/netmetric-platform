"use client";

import { Check, ChevronsUpDown } from "lucide-react";
import * as React from "react";

import { cn } from "../../lib/utils";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "../overlay/command";
import { Popover, PopoverContent, PopoverTrigger } from "../overlay/popover";

import { Button } from "./button";

type ComboboxOption = {
  value: string;
  label: string;
  disabled?: boolean;
};

type ComboboxProps = {
  options: ComboboxOption[];
  value?: string;
  onValueChange?: (value: string) => void;
  open?: boolean;
  defaultOpen?: boolean;
  onOpenChange?: (open: boolean) => void;
  placeholder?: string;
  searchPlaceholder?: string;
  emptyMessage?: string;
  disabled?: boolean;
  className?: string;
};

function Combobox({
  options,
  value,
  onValueChange,
  open,
  defaultOpen,
  onOpenChange,
  placeholder = "Select option...",
  searchPlaceholder = "Search...",
  emptyMessage = "No results found.",
  disabled,
  className,
}: ComboboxProps) {
  const [internalOpen, setInternalOpen] = React.useState(defaultOpen ?? false);
  const isControlled = open !== undefined;
  const currentOpen = isControlled ? open : internalOpen;

  const setOpen = React.useCallback(
    (nextOpen: boolean) => {
      if (!isControlled) {
        setInternalOpen(nextOpen);
      }

      onOpenChange?.(nextOpen);
    },
    [isControlled, onOpenChange],
  );

  const selectedOption = options.find((option) => option.value === value);

  return (
    <Popover open={currentOpen} onOpenChange={setOpen}>
      <PopoverTrigger
        render={
          <Button
            type="button"
            variant="outline"
            role="combobox"
            aria-expanded={currentOpen}
            disabled={disabled}
            className={cn("w-full justify-between", className)}
          />
        }
      >
        <span className="truncate">{selectedOption?.label ?? placeholder}</span>
        <ChevronsUpDown className="ml-2 size-4 shrink-0 opacity-50" />
      </PopoverTrigger>

      <PopoverContent className="w-(--radix-popover-trigger-width) p-0" align="start">
        <Command>
          <CommandInput placeholder={searchPlaceholder} />

          <CommandList>
            <CommandEmpty>{emptyMessage}</CommandEmpty>

            <CommandGroup>
              {options.map((option) => {
                const commandItemProps: React.ComponentProps<typeof CommandItem> = {
                  value: option.value,
                  onSelect: (currentValue) => {
                    onValueChange?.(currentValue === value ? "" : currentValue);

                    setOpen(false);
                  },
                };

                if (option.disabled !== undefined) {
                  commandItemProps.disabled = option.disabled;
                }

                return (
                  <CommandItem key={option.value} {...commandItemProps}>
                    <Check
                      className={cn(
                        "mr-2 size-4",
                        value === option.value ? "opacity-100" : "opacity-0",
                      )}
                    />

                    {option.label}
                  </CommandItem>
                );
              })}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}

export { Combobox };
export type { ComboboxOption };
