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
  Textarea,
} from "@netmetric/ui";

import {
  bulkAssignLeadsOwnerFormAction,
  bulkDeleteLeadsFormAction,
} from "@/features/leads/actions/lead-mutation-actions";
import { tCrm } from "@/lib/i18n/crm-i18n";

type LeadBulkActionsPanelProps = {
  canAssign: boolean;
  canDelete: boolean;
  locale?: string | null | undefined;
};

export function LeadBulkActionsPanel({
  canAssign,
  canDelete,
  locale,
}: Readonly<LeadBulkActionsPanelProps>) {
  if (!canAssign && !canDelete) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.leads.pages.detail.bulkActionsTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.leads.pages.detail.bulkActionsDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent className="grid gap-5 xl:grid-cols-2">
        {canAssign ? (
          <form action={bulkAssignLeadsOwnerFormAction} className="space-y-3">
            <Field>
              <FieldLabel htmlFor="lead-bulk-assign-ids">
                {tCrm("crm.leads.fields.leadIds", locale)}
              </FieldLabel>
              <FieldContent>
                <Textarea id="lead-bulk-assign-ids" name="leadIds" rows={4} />
              </FieldContent>
            </Field>
            <Field>
              <FieldLabel htmlFor="lead-bulk-owner-id">
                {tCrm("crm.leads.fields.ownerUserId", locale)}
              </FieldLabel>
              <FieldContent>
                <Input
                  id="lead-bulk-owner-id"
                  name="ownerUserId"
                  placeholder="00000000-0000-0000-0000-000000000000"
                />
              </FieldContent>
            </Field>
            <Button type="submit">{tCrm("crm.leads.actions.bulkAssignOwner", locale)}</Button>
          </form>
        ) : null}

        {canDelete ? (
          <form action={bulkDeleteLeadsFormAction} className="space-y-3">
            <input name="confirm" type="hidden" value="bulk-delete-leads" />
            <Field>
              <FieldLabel htmlFor="lead-bulk-delete-ids">
                {tCrm("crm.leads.fields.leadIds", locale)}
              </FieldLabel>
              <FieldContent>
                <Textarea id="lead-bulk-delete-ids" name="leadIds" rows={4} />
              </FieldContent>
            </Field>
            <Button type="submit" variant="destructive">
              {tCrm("crm.leads.actions.bulkDelete", locale)}
            </Button>
          </form>
        ) : null}
      </CardContent>
    </Card>
  );
}
