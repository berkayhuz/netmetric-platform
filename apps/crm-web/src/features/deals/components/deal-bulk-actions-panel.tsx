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

import { bulkAssignDealsOwnerFormAction } from "@/features/deals/actions/deal-lifecycle-actions";
import { tCrm } from "@/lib/i18n/crm-i18n";

type DealBulkActionsPanelProps = {
  canAssign: boolean;
  locale?: string | null | undefined;
};

export function DealBulkActionsPanel({ canAssign, locale }: Readonly<DealBulkActionsPanelProps>) {
  if (!canAssign) {
    return null;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{tCrm("crm.deals.pages.detail.bulkActionsTitle", locale)}</CardTitle>
        <CardDescription>
          {tCrm("crm.deals.pages.detail.bulkActionsDescription", locale)}
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form action={bulkAssignDealsOwnerFormAction} className="space-y-3">
          <Field>
            <FieldLabel htmlFor="deal-bulk-assign-ids">
              {tCrm("crm.deals.fields.dealIds", locale)}
            </FieldLabel>
            <FieldContent>
              <Textarea id="deal-bulk-assign-ids" name="dealIds" rows={4} />
            </FieldContent>
          </Field>
          <Field>
            <FieldLabel htmlFor="deal-bulk-owner-id">
              {tCrm("crm.deals.fields.ownerUserId", locale)}
            </FieldLabel>
            <FieldContent>
              <Input
                id="deal-bulk-owner-id"
                name="ownerUserId"
                placeholder="00000000-0000-0000-0000-000000000000"
              />
            </FieldContent>
          </Field>
          <Button type="submit">{tCrm("crm.deals.actions.bulkAssignOwner", locale)}</Button>
        </form>
      </CardContent>
    </Card>
  );
}
