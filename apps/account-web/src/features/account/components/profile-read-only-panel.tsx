import { Text } from "@netmetric/ui";

import type { MyProfileResponse } from "@/lib/account-api";

import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";
import { ReadOnlyValue } from "./read-only-value";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type ProfileReadOnlyPanelProps = {
  profile: MyProfileResponse;
};

export function ProfileReadOnlyPanel({ profile }: ProfileReadOnlyPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.profile.title")}
      description={tAccountClient("account.profile.readOnlyDescription")}
    >
      <AccountSection
        title={tAccountClient("account.profile.detailsTitle")}
        description={tAccountClient("account.common.editingNextPhase")}
        contentClassName="grid gap-3 sm:grid-cols-2"
      >
        <Field label={tAccountClient("account.fields.displayName")} value={profile.displayName} />
        <Field
          label={tAccountClient("account.profile.fields.firstName")}
          value={profile.firstName}
        />
        <Field label={tAccountClient("account.profile.fields.lastName")} value={profile.lastName} />
        <Field label={tAccountClient("account.profile.fields.phone")} value={profile.phoneNumber} />
        <Field label={tAccountClient("account.profile.fields.jobTitle")} value={profile.jobTitle} />
        <Field
          label={tAccountClient("account.profile.fields.department")}
          value={profile.department}
        />
        <Field label={tAccountClient("account.profile.fields.timeZone")} value={profile.timeZone} />
        <Field label={tAccountClient("account.profile.fields.culture")} value={profile.culture} />
        <Field label={tAccountClient("account.fields.avatarUrl")} value={profile.avatarUrl} />
        <Field label={tAccountClient("account.fields.version")} value={profile.version} />
      </AccountSection>
    </AccountPagePanel>
  );
}

function Field({ label, value }: { label: string; value: string | null | undefined }) {
  return (
    <div className="space-y-1">
      <Text className="text-sm text-muted-foreground">{label}</Text>
      <ReadOnlyValue value={value} />
    </div>
  );
}
