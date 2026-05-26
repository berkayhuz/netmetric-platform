import { AddressCard } from "@/components/address/address-card";
import { deleteAddressAction } from "@/features/addresses/actions/address-mutation-actions";
import type { AddressDto } from "@/lib/crm-api";

export function AddressList({
  entityType,
  entityId,
  addresses,
  locale,
  canManage = true,
  useCard = true,
}: Readonly<{
  entityType: "customer" | "company";
  entityId: string;
  addresses: AddressDto[];
  locale?: string | null | undefined;
  canManage?: boolean;
  useCard?: boolean;
}>) {
  return (
    <div className="grid gap-4">
      {addresses.map((address) => (
        <AddressCard
          key={address.id}
          entityType={entityType}
          entityId={entityId}
          address={address}
          onDelete={deleteAddressAction.bind(null, entityType, entityId, address.id)}
          locale={locale}
          canManage={canManage}
          useCard={useCard}
        />
      ))}
    </div>
  );
}
