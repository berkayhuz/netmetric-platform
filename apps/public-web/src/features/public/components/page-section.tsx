import {
  Badge,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Heading,
  Lead,
  Text,
} from "@netmetric/ui";

type Highlight = {
  title: string;
  description: string;
};

type PageSectionProps = {
  badge?: string;
  title: string;
  lead: string;
  governanceText: string;
  highlights: Highlight[];
};

export function PageSection({ badge, title, lead, governanceText, highlights }: PageSectionProps) {
  return (
    <section className="mx-auto w-full max-w-7xl space-y-8 px-4 py-16 sm:px-6">
      <div className="space-y-3">
        {badge ? <Badge variant="secondary">{badge}</Badge> : null}
        <Heading level={1}>{title}</Heading>
        <Lead>{lead}</Lead>
      </div>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {highlights.map((item) => (
          <Card key={item.title}>
            <CardHeader>
              <CardTitle>{item.title}</CardTitle>
              <CardDescription>{item.description}</CardDescription>
            </CardHeader>
            <CardContent>
              <Text className="text-sm text-muted-foreground">{governanceText}</Text>
            </CardContent>
          </Card>
        ))}
      </div>
    </section>
  );
}
