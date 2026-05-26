import Link from "next/link";
import { ArrowUpRight } from "lucide-react";
import {
  Badge,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Text,
} from "@netmetric/ui";

export function DashboardSummaryCard({
  title,
  total,
  href,
  description,
  summaryAriaLabel,
  readOnlyLabel,
  viewAllLabel,
}: Readonly<{
  title: string;
  total: number;
  href: string;
  description: string;
  summaryAriaLabel: string;
  readOnlyLabel: string;
  viewAllLabel: string;
}>) {
  return (
    <Card aria-label={summaryAriaLabel} className="shadow-xs transition-colors hover:bg-muted/10">
      <CardHeader>
        <div className="flex items-center justify-between gap-3">
          <CardTitle>{title}</CardTitle>
          <Badge variant="secondary">{readOnlyLabel}</Badge>
        </div>
        <CardDescription>{description}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        <Text className="text-3xl font-semibold">{total}</Text>
        <Link
          className="inline-flex items-center gap-1 text-sm font-medium text-primary underline-offset-4 hover:underline focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
          href={href}
        >
          {viewAllLabel}
          <ArrowUpRight aria-hidden="true" className="size-3.5" />
        </Link>
      </CardContent>
    </Card>
  );
}
