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

import {
  bulkAssignOpportunitiesOwnerFormAction,
  bulkChangeOpportunitiesStageFormAction,
} from "@/features/opportunities/actions/opportunity-lifecycle-actions";
import { opportunityStageOptions } from "@/features/shared/forms/options";
import { tCrm } from "@/lib/i18n/crm-i18n";

type OpportunityBulkActionsPanelProps = {
  canManage: boolean;
  locale?: string | null | undefined;
};

export function OpportunityBulkActionsPanel({
  canManage,
  locale,
}: Readonly<OpportunityBulkActionsPanelProps>) {
  if (!canManage) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.opportunities.pages.detail.bulkActionsTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.opportunities.pages.detail.bulkActionsDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="grid gap-5 xl:grid-cols-2">
        <form action={bulkAssignOpportunitiesOwnerFormAction} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="opportunity-bulk-owner-ids">
              {tCrm("crm.opportunities.fields.opportunityIds", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="opportunity-bulk-owner-ids" name="opportunityIds" rows={4} />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-bulk-owner-id">
              {tCrm("crm.opportunities.fields.ownerUserId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                id="opportunity-bulk-owner-id"
                name="ownerUserId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.opportunities.actions.bulkAssignOwner", locale)}</Button>
        </form>

        <form action={bulkChangeOpportunitiesStageFormAction} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="opportunity-bulk-stage-ids">
              {tCrm("crm.opportunities.fields.opportunityIds", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="opportunity-bulk-stage-ids" name="opportunityIds" rows={4} />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-bulk-stage">
              {tCrm("crm.opportunities.fields.stage", locale)}
            </FieldLabel>
            <FieldContent>
              <NativeSelect className="w-full" id="opportunity-bulk-stage" name="newStage">
                {opportunityStageOptions.map((option) => (
                  <NativeSelectOption key={option.value} value={option.value}>
                    {tCrm(`crm.opportunities.stage.${option.value}`, locale)}
                  </NativeSelectOption>
                ))}
              </NativeSelect>
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="opportunity-bulk-stage-note">
              {tCrm("crm.opportunities.fields.note", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="opportunity-bulk-stage-note" name="note" rows={3} />
            </FieldContent>
          </Field>
          <Button type="submit" variant="outline">
            {tCrm("crm.opportunities.actions.bulkChangeStage", locale)}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}
