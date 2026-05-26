import { Button, Separator } from "@netmetric/ui";

import { PageSection } from "@/features/public/components/page-section";
import { type PublicPageContent } from "@/features/public/content/pages";
import { tPublic } from "@/lib/i18n/public-i18n";
import { publicEnv } from "@/lib/public-env";

export function StandardPage({
  content,
  locale,
}: {
  content: PublicPageContent;
  locale?: string | null;
}) {
  const sectionProps = content.badge ? { badge: content.badge } : {};

  return (
    <>
      <PageSection
        {...sectionProps}
        title={content.title}
        lead={content.lead}
        governanceText={tPublic("public.pages.shared.governanceText", locale)}
        highlights={content.highlights}
      />
      <Separator />
      <section className="mx-auto w-full max-w-7xl px-4 py-12 sm:px-6">
        <div className="flex flex-col gap-3 sm:flex-row">
          <Button asChild>
            <a href={publicEnv.authUrl}>{tPublic("public.actions.signInAuthDomain", locale)}</a>
          </Button>
          <Button asChild variant="outline">
            <a href={publicEnv.apiUrl}>{tPublic("public.actions.exploreApi", locale)}</a>
          </Button>
        </div>
      </section>
    </>
  );
}
