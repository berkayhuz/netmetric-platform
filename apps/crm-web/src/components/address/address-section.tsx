import { AddressForm } from "@/components/address/address-form";
import { AddressList } from "@/components/address/address-list";
import {
  createCompanyAddressAction,
  createCustomerAddressAction,
} from "@/features/addresses/actions/address-mutation-actions";
import type { AddressDto } from "@/lib/crm-api";
import { tCrm } from "@/lib/i18n/crm-i18n";
import { getRequestLocale } from "@/lib/i18n/request-locale";

export async function AddressSection({
  entityType,
  entityId,
  addresses,
  canManage = true,
}: Readonly<{
  entityType: "customer" | "company";
  entityId: string;
  addresses?: AddressDto[] | null;
  canManage?: boolean;
}>) {
  const locale = await getRequestLocale();
  const list = Array.isArray(addresses) ? addresses : [];
  const entityKey =
    entityType === "customer" ? "crm.address.entity.customer" : "crm.address.entity.company";

  return (
    <section aria-labelledby={`${entityType}-address-section`} className="w-full">
      {canManage ? (
        <div className="space-y-4">
          <div className="space-y-1">
            <h3 className="text-sm font-semibold text-foreground">
              {tCrm("crm.address.actions.add", locale)}
            </h3>
            <p className="text-xs text-muted-foreground">
              {tCrm("crm.address.section.addDescription", locale, {
                entity: tCrm(entityKey, locale),
              })}
            </p>
          </div>
          <div>
            <AddressForm
              mode="create"
              action={
                entityType === "customer"
                  ? createCustomerAddressAction.bind(null, entityId)
                  : createCompanyAddressAction.bind(null, entityId)
              }
            />
          </div>
        </div>
      ) : null}
    </section>
  );
}
