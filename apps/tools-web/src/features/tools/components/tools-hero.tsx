import {
  Badge,
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@netmetric/ui";
import Link from "next/link";

import { tTools } from "@/lib/i18n/tools-i18n";

export function ToolsHero({ locale }: { locale?: string | null | undefined }) {
  return (
    <section className="mx-auto w-full max-w-6xl px-4 py-12 sm:px-6 lg:px-8">
      <Card>
        <CardHeader>
          <Badge className="w-fit" variant="secondary">
            {tTools("tools.hero.badge", locale)}
          </Badge>
          <CardTitle className="text-3xl">{tTools("tools.hero.title", locale)}</CardTitle>
          <CardDescription>{tTools("tools.hero.description", locale)}</CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-3">
          <Button asChild>
            <Link href="/qr-generator">{tTools("tools.hero.tryQr", locale)}</Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/categories">{tTools("tools.hero.browseCategories", locale)}</Link>
          </Button>
        </CardContent>
      </Card>
    </section>
  );
}
