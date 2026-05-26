import Link from "next/link";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Empty,
  EmptyDescription,
  EmptyHeader,
  EmptyTitle,
  Item,
  ItemContent,
  ItemDescription,
  ItemGroup,
  ItemTitle,
} from "@netmetric/ui";

export function DashboardRecentList({
  title,
  description,
  items,
  detailBasePath,
  emptyTitle,
  emptyDescription,
}: Readonly<{
  title: string;
  description: string;
  items: Array<{ id: string; name: string; subtitle: string }>;
  detailBasePath: string;
  emptyTitle: string;
  emptyDescription: string;
}>) {
  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent>
        {items.length === 0 ? (
          <Empty>
            <EmptyHeader>
              <EmptyTitle>{emptyTitle}</EmptyTitle>
              <EmptyDescription>{emptyDescription}</EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <ItemGroup>
            {items.map((item) => (
              <Item key={item.id}>
                <ItemContent>
                  <ItemTitle>
                    <Link
                      href={`${detailBasePath}/${item.id}`}
                      className="underline-offset-4 hover:underline"
                    >
                      {item.name}
                    </Link>
                  </ItemTitle>
                  <ItemDescription>{item.subtitle}</ItemDescription>
                </ItemContent>
              </Item>
            ))}
          </ItemGroup>
        )}
      </CardContent>
    </Card>
  );
}
