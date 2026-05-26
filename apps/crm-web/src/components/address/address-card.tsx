import { Button, Card, CardContent, CardDescription, CardHeader, CardTitle } from "@netmetric/ui";
import { tCrm } from "@/lib/i18n/crm-i18n";

import { AddressDeleteForm } from "@/components/address/address-delete-form";
import { AddressForm } from "@/components/address/address-form";
import {
  setDefaultAddressAction,
  updateAddressAction,
} from "@/features/addresses/actions/address-mutation-actions";
import type { CrmMutationState } from "@/features/shared/actions/mutation-state";
import type { AddressDto } from "@/lib/crm-api";

export function AddressCard({
  entityType,
  entityId,
  address,
  onDelete,
  locale,
  canManage = true,
  useCard = true,
}: Readonly<{
  entityType: "customer" | "company";
  entityId: string;
  address: AddressDto;
  onDelete: (state: CrmMutationState, formData: FormData) => Promise<CrmMutationState>;
  locale?: string | null | undefined;
  canManage?: boolean;
  useCard?: boolean;
}>) {
  const lineParts = [address.line1, address.line2].filter(Boolean).join(", ");
  const locationParts = [address.district, address.city, address.state, address.country]
    .filter(Boolean)
    .join(", ");

  const content = (
    <>
      <CardHeader>
        <CardTitle className="text-base">{tCrm("crm.address.title", locale)}</CardTitle>
        <CardDescription>
          {[lineParts, locationParts, address.zipCode].filter(Boolean).join(" - ") ||
            tCrm("crm.address.noSummary", locale)}
        </CardDescription>
      </CardHeader>
      {canManage ? (
        <CardContent className="space-y-6">
          {!address.isDefault ? (
            <form action={setDefaultAddressAction.bind(null, entityType, entityId, address.id)}>
              <input name="confirm" type="hidden" value="set-default-address" />
              <Button type="submit" variant="outline">
                {tCrm("crm.address.actions.setDefault", locale)}
              </Button>
            </form>
          ) : null}
          <AddressForm
            mode="edit"
            initialValues={{
              id: address.id,
              addressType: Number(address.addressType),
              line1: address.line1,
              line2: address.line2 ?? "",
              district: address.district ?? "",
              city: address.city ?? "",
              state: address.state ?? "",
              country: address.country ?? "",
              zipCode: address.zipCode ?? "",
              isDefault: address.isDefault,
              rowVersion: address.rowVersion,
            }}
            action={updateAddressAction.bind(null, entityType, entityId, address.id)}
          />
          <div className="space-y-2 border-t border-border pt-4">
            <p className="text-sm font-medium text-destructive">
              {tCrm("crm.address.delete.title", locale)}
            </p>
            <p className="text-xs text-muted-foreground">
              {tCrm("crm.address.delete.description", locale)}
            </p>
            <AddressDeleteForm action={onDelete} />
          </div>
        </CardContent>
      ) : null}
    </>
  );

  return useCard ? <Card>{content}</Card> : <section className="space-y-4">{content}</section>;
}
