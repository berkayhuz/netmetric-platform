import { Badge, Text } from "@netmetric/ui";

import { tAccountClient } from "@/lib/i18n/account-i18n";
import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";

type ScaffoldPageProps = {
  title: string;
  description: string;
};

export function ScaffoldPage({ title, description }: ScaffoldPageProps) {
  return (
    <AccountPagePanel title={title} description={description} contentClassName="max-w-xl">
      <div>
        <Badge variant="secondary">{tAccountClient("account.scaffold.badge")}</Badge>
      </div>
      <AccountSection
        title={tAccountClient("account.scaffold.statusTitle")}
        description={tAccountClient("account.scaffold.statusDescription")}
      >
        <Text className="text-sm text-muted-foreground">
          {tAccountClient("account.scaffold.integrationDeferred")}
        </Text>
      </AccountSection>
    </AccountPagePanel>
  );
}
