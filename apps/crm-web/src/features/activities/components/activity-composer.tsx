"use client";

import { useActionState, useMemo, useState } from "react";
import { useFormStatus } from "react-dom";
import {
  Button,
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
  Field,
  FieldContent,
  FieldLabel,
  Input,
  NativeSelect,
  NativeSelectOption,
  Textarea,
} from "@netmetric/ui";

import { CrmMutationResult } from "@/components/forms/crm-mutation-result";
import { createActivityAction } from "@/features/activities/actions/create-activity-action";
import {
  initialCrmMutationState,
  type CrmMutationState,
} from "@/features/shared/actions/mutation-state";
import { tCrm, tCrmClient } from "@/lib/i18n/crm-i18n";

type ActivityComposerProps = {
  primaryRecord: {
    entityType:
      | "lead"
      | "opportunity"
      | "deal"
      | "quote"
      | "ticket"
      | "customer"
      | "company"
      | "contact";
    entityId: string;
  };
  locale?: string | null | undefined;
};

type ActivityComposerActionInput = Parameters<typeof createActivityAction>[0];

function SubmitButton() {
  const { pending } = useFormStatus();
  return (
    <Button type="submit" size="sm" className="h-8" disabled={pending} aria-busy={pending}>
      {pending
        ? tCrmClient("crm.forms.actions.processing")
        : tCrmClient("crm.activities.actions.saveActivity")}
    </Button>
  );
}

export function ActivityComposer({ primaryRecord, locale }: Readonly<ActivityComposerProps>) {
  const [activeTab, setActiveTab] = useState<"note" | "call" | "email">("note");
  const [state, action] = useActionState(
    async (_previous: CrmMutationState, formData: FormData): Promise<CrmMutationState> => {
      const typeValue = formData.get("type");
      const type = typeValue === "call" || typeValue === "email" ? typeValue : "note";

      const toList = String(formData.get("emailTo") ?? "")
        .split(/[;,]+/)
        .map((item) => item.trim())
        .filter(Boolean);
      const ccList = String(formData.get("emailCc") ?? "")
        .split(/[;,]+/)
        .map((item) => item.trim())
        .filter(Boolean);
      const durationRaw = String(formData.get("callDurationSeconds") ?? "").trim();
      const duration = durationRaw.length > 0 ? Number(durationRaw) : null;

      const input: ActivityComposerActionInput = {
        primaryRecord,
        type,
        title: String(formData.get("title") ?? ""),
        description: String(formData.get("description") ?? ""),
        noteBody: String(formData.get("noteBody") ?? ""),
        callDirection: (formData.get("callDirection") as "inbound" | "outbound" | null) ?? null,
        callOutcome:
          (formData.get("callOutcome") as
            | "connected"
            | "no_answer"
            | "voicemail"
            | "other"
            | null) ?? null,
        callDurationSeconds: duration !== null && Number.isFinite(duration) ? duration : null,
        callSummary: String(formData.get("callSummary") ?? ""),
        emailSubject: String(formData.get("emailSubject") ?? ""),
        emailBodySummary: String(formData.get("emailBodySummary") ?? ""),
        emailDirection: (formData.get("emailDirection") as "inbound" | "outbound" | null) ?? null,
        emailTo: toList,
        emailCc: ccList,
      };

      return createActivityAction(input);
    },
    initialCrmMutationState,
  );

  const title = useMemo(() => tCrm("crm.activities.actions.addActivity", locale), [locale]);
  const translate = (key: string) => tCrm(key, locale);

  return (
    <Card>
      <CardHeader>
        <CardTitle>{title}</CardTitle>
        <CardDescription>
          {translate("crm.activities.sections.unifiedTimelinePreviewDescription")}
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        <CrmMutationResult state={state} />
        <div className="flex gap-2">
          <Button
            type="button"
            variant={activeTab === "note" ? "default" : "outline"}
            size="sm"
            className="h-8"
            onClick={() => setActiveTab("note")}
          >
            {translate("crm.activities.tabs.note")}
          </Button>
          <Button
            type="button"
            variant={activeTab === "call" ? "default" : "outline"}
            size="sm"
            className="h-8"
            onClick={() => setActiveTab("call")}
          >
            {translate("crm.activities.tabs.call")}
          </Button>
          <Button
            type="button"
            variant={activeTab === "email" ? "default" : "outline"}
            size="sm"
            className="h-8"
            onClick={() => setActiveTab("email")}
          >
            {translate("crm.activities.tabs.email")}
          </Button>
        </div>

        {activeTab === "note" ? (
          <div className="pt-2">
            <form action={action} className="space-y-3">
              <input type="hidden" name="type" value="note" />
              <Field>
                <FieldLabel htmlFor="activity-note-title">
                  {translate("crm.activities.fields.subject")}
                </FieldLabel>
                <FieldContent>
                  <Input id="activity-note-title" name="title" />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="activity-note-body">
                  {translate("crm.activities.tabs.note")}
                </FieldLabel>
                <FieldContent>
                  <Textarea id="activity-note-body" name="noteBody" rows={4} required />
                </FieldContent>
              </Field>
              <SubmitButton />
            </form>
          </div>
        ) : null}

        {activeTab === "call" ? (
          <div className="pt-2">
            <form action={action} className="space-y-3">
              <input type="hidden" name="type" value="call" />
              <Field>
                <FieldLabel htmlFor="activity-call-title">
                  {translate("crm.activities.fields.subject")}
                </FieldLabel>
                <FieldContent>
                  <Input id="activity-call-title" name="title" />
                </FieldContent>
              </Field>
              <div className="grid gap-3 md:grid-cols-2">
                <Field>
                  <FieldLabel htmlFor="activity-call-direction">
                    {translate("crm.activities.fields.direction")}
                  </FieldLabel>
                  <FieldContent>
                    <NativeSelect
                      id="activity-call-direction"
                      name="callDirection"
                      className="w-full"
                      required
                    >
                      <NativeSelectOption value="outbound">
                        {translate("crm.activities.direction.outbound")}
                      </NativeSelectOption>
                      <NativeSelectOption value="inbound">
                        {translate("crm.activities.direction.inbound")}
                      </NativeSelectOption>
                    </NativeSelect>
                  </FieldContent>
                </Field>
                <Field>
                  <FieldLabel htmlFor="activity-call-outcome">
                    {translate("crm.activities.fields.outcome")}
                  </FieldLabel>
                  <FieldContent>
                    <NativeSelect
                      id="activity-call-outcome"
                      name="callOutcome"
                      className="w-full"
                      required
                    >
                      <NativeSelectOption value="connected">
                        {translate("crm.activities.outcome.connected")}
                      </NativeSelectOption>
                      <NativeSelectOption value="no_answer">
                        {translate("crm.activities.outcome.noAnswer")}
                      </NativeSelectOption>
                      <NativeSelectOption value="voicemail">
                        {translate("crm.activities.outcome.voicemail")}
                      </NativeSelectOption>
                      <NativeSelectOption value="other">
                        {translate("crm.activities.outcome.other")}
                      </NativeSelectOption>
                    </NativeSelect>
                  </FieldContent>
                </Field>
              </div>
              <Field>
                <FieldLabel htmlFor="activity-call-duration">
                  {translate("crm.activities.fields.duration")}
                </FieldLabel>
                <FieldContent>
                  <Input
                    id="activity-call-duration"
                    name="callDurationSeconds"
                    inputMode="numeric"
                    type="number"
                    min={0}
                  />
                </FieldContent>
              </Field>
              <Field>
                <FieldLabel htmlFor="activity-call-summary">
                  {translate("crm.activities.fields.summary")}
                </FieldLabel>
                <FieldContent>
                  <Textarea id="activity-call-summary" name="callSummary" rows={3} />
                </FieldContent>
              </Field>
              <SubmitButton />
            </form>
          </div>
        ) : null}

        {activeTab === "email" ? (
          <div className="pt-2">
            <form action={action} className="space-y-3">
              <input type="hidden" name="type" value="email" />
              <div className="grid gap-3 md:grid-cols-2">
                <Field>
                  <FieldLabel htmlFor="activity-email-subject">
                    {translate("crm.activities.fields.subject")}
                  </FieldLabel>
                  <FieldContent>
                    <Input id="activity-email-subject" name="emailSubject" required />
                  </FieldContent>
                </Field>
                <Field>
                  <FieldLabel htmlFor="activity-email-direction">
                    {translate("crm.activities.fields.direction")}
                  </FieldLabel>
                  <FieldContent>
                    <NativeSelect
                      id="activity-email-direction"
                      name="emailDirection"
                      className="w-full"
                      required
                    >
                      <NativeSelectOption value="outbound">
                        {translate("crm.activities.direction.outbound")}
                      </NativeSelectOption>
                      <NativeSelectOption value="inbound">
                        {translate("crm.activities.direction.inbound")}
                      </NativeSelectOption>
                    </NativeSelect>
                  </FieldContent>
                </Field>
              </div>
              <Field>
                <FieldLabel htmlFor="activity-email-body-summary">
                  {translate("crm.activities.fields.bodySummary")}
                </FieldLabel>
                <FieldContent>
                  <Textarea
                    id="activity-email-body-summary"
                    name="emailBodySummary"
                    rows={4}
                    required
                  />
                </FieldContent>
              </Field>
              <div className="grid gap-3 md:grid-cols-2">
                <Field>
                  <FieldLabel htmlFor="activity-email-to">To</FieldLabel>
                  <FieldContent>
                    <Input
                      id="activity-email-to"
                      name="emailTo"
                      placeholder="a@company.com; b@company.com"
                    />
                  </FieldContent>
                </Field>
                <Field>
                  <FieldLabel htmlFor="activity-email-cc">Cc</FieldLabel>
                  <FieldContent>
                    <Input
                      id="activity-email-cc"
                      name="emailCc"
                      placeholder="c@company.com; d@company.com"
                    />
                  </FieldContent>
                </Field>
              </div>
              <SubmitButton />
            </form>
          </div>
        ) : null}
      </CardContent>
    </Card>
  );
}
