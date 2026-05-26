import { Badge, Card, CardContent, CardDescription, CardHeader, CardTitle } from "@netmetric/ui";

export function CrmModuleCard({
  title,
  description,
  status,
}: Readonly<{
  title: string;
  description: string;
  status: string;
}>) {
  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between gap-3">
          <CardTitle>{title}</CardTitle>
          <Badge variant="secondary">{status}</Badge>
        </div>
      </CardHeader>
      <CardContent>
        <CardDescription>{description}</CardDescription>
      </CardContent>
    </Card>
  );
}
