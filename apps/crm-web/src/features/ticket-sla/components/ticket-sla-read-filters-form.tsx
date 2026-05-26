"use client";

import { useState } from "react";
import { Button, Input } from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

type Policy = {
  id: string;
  name: string;
};

type TicketSlaReadFiltersFormProps = {
  policies: Policy[];
  policyId?: string | undefined;
  ticketIdValue: string;
};

export function TicketSlaReadFiltersForm({
  policies,
  policyId,
  ticketIdValue,
}: Readonly<TicketSlaReadFiltersFormProps>) {
  const noPolicy = "__no_policy__";
  const [selectedPolicyId, setSelectedPolicyId] = useState(policyId ?? noPolicy);
  const policyDisplayOptions = [
    { value: noPolicy, label: tCrmClient("crm.ticketSla.filters.noPolicySelected") },
    ...policies.map((policy) => ({ value: policy.id, label: policy.name })),
  ];

  return (
    <form method="get" className="grid gap-3 sm:grid-cols-3">
      <input
        type="hidden"
        name="policyId"
        value={selectedPolicyId === noPolicy ? "" : selectedPolicyId}
      />
      <div className="space-y-2">
        <label htmlFor="ticket-sla-policyId" className="text-sm font-medium">
          {tCrmClient("crm.ticketSla.filters.policy")}
        </label>
        <Select
          value={selectedPolicyId}
          onValueChange={(value) => setSelectedPolicyId(value ?? noPolicy)}
        >
          <SelectTrigger id="ticket-sla-policyId">
            <SelectValue placeholder={tCrmClient("crm.ticketSla.filters.noPolicySelected")}>
              {getSelectDisplayLabel(selectedPolicyId, policyDisplayOptions)}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={noPolicy}>
              {tCrmClient("crm.ticketSla.filters.noPolicySelected")}
            </SelectItem>
            {policies.map((policy) => (
              <SelectItem key={policy.id} value={policy.id}>
                {policy.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
      <div className="space-y-2">
        <label htmlFor="ticket-sla-ticketId" className="text-sm font-medium">
          {tCrmClient("crm.ticketSla.filters.ticketId")}
        </label>
        <Input id="ticket-sla-ticketId" name="ticketId" defaultValue={ticketIdValue} />
      </div>
      <div className="flex items-end">
        <Button type="submit" variant="outline" className="w-full">
          {tCrmClient("crm.ticketSla.filters.load")}
        </Button>
      </div>
    </form>
  );
}
