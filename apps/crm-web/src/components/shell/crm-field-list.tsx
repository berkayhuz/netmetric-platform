import { CrmReadonlyField } from "./crm-readonly-field";

export function CrmFieldList({
  fields,
}: Readonly<{
  fields: Array<{ label: string; value: string | number | boolean | null | undefined }>;
}>) {
  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {fields.map((field) => (
        <CrmReadonlyField key={field.label} label={field.label} value={field.value} />
      ))}
    </div>
  );
}
