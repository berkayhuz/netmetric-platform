import { Text } from "@netmetric/ui";

import type { UserPreferenceResponse } from "@/lib/account-api";

import { AccountPagePanel } from "./account-page-panel";
import { AccountSection } from "./account-section";
import { ReadOnlyValue } from "./read-only-value";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type PreferencesReadOnlyPanelProps = {
  preferences: UserPreferenceResponse;
};

export function PreferencesReadOnlyPanel({ preferences }: PreferencesReadOnlyPanelProps) {
  return (
    <AccountPagePanel
      title={tAccountClient("account.preferences.title")}
      description={tAccountClient("account.preferences.readOnlyDescription")}
    >
      <AccountSection
        title={tAccountClient("account.preferences.detailsTitle")}
        description={tAccountClient("account.common.editingNextPhase")}
        contentClassName="grid gap-3 sm:grid-cols-2"
      >
        <Field
          label={tAccountClient("account.profile.fields.language")}
          value={preferences.language}
        />
        <Field
          label={tAccountClient("account.profile.fields.timeZone")}
          value={preferences.timeZone}
        />
        <Field label={tAccountClient("account.preferences.theme")} value={preferences.theme} />
        <Field
          label={tAccountClient("account.preferences.dateFormat")}
          value={preferences.dateFormat}
        />
        <Field
          label={tAccountClient("account.preferences.postLoginDestination")}
          value={preferences.postLoginDestination}
        />
        <Field
          label={tAccountClient("account.preferences.defaultOrganization")}
          value={preferences.defaultOrganizationId}
        />
        <Field label={tAccountClient("account.fields.version")} value={preferences.version} />
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
