import { cn } from "../../lib/utils";
import { Textarea } from "../primitives/textarea";

import { Field, FieldContent, FieldError, FieldLabel } from "./field";

type TextareaFieldProps = Omit<React.ComponentProps<typeof Textarea>, "children"> & {
  label: React.ReactNode;
  error?: React.ReactNode;
  labelProps?: Omit<React.ComponentProps<typeof FieldLabel>, "children" | "htmlFor">;
  fieldProps?: Omit<React.ComponentProps<typeof Field>, "children">;
  contentProps?: Omit<React.ComponentProps<typeof FieldContent>, "children">;
  errorProps?: Omit<React.ComponentProps<typeof FieldError>, "children">;
};

export function TextareaField({
  label,
  error,
  id,
  className,
  labelProps,
  fieldProps,
  contentProps,
  errorProps,
  ...textareaProps
}: TextareaFieldProps) {
  return (
    <Field {...fieldProps}>
      <FieldLabel htmlFor={id} {...labelProps}>
        {label}
      </FieldLabel>
      <FieldContent {...contentProps}>
        <Textarea id={id} className={cn(className)} {...textareaProps} />
        <FieldError {...errorProps}>{error}</FieldError>
      </FieldContent>
    </Field>
  );
}
