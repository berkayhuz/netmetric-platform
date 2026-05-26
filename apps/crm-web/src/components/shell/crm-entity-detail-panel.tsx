import { Card, CardContent, CardHeader, CardTitle } from "@netmetric/ui";

import { CrmFieldList } from "./crm-field-list";

export function CrmEntityDetailPanel({
  title,
  fields,
}: Readonly<{
  title: string;
  fields: Array<{ label: string; value: string | number | boolean | null | undefined }>;
}>) {
  return (
    <Card className="border-none ring-0 min-h-0" radius="none">
      <CardHeader>
        <CardTitle>{title}</CardTitle>
      </CardHeader>
      <CardContent>
        <CrmFieldList fields={fields} />
      </CardContent>
    </Card>
  );
}
