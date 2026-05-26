"use client";

import { useState } from "react";
import { Button } from "@netmetric/ui";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@netmetric/ui/client";

import { getSelectDisplayLabel } from "@/features/shared/forms/select-display";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

type Connection = {
  id: string;
  name: string;
};

type SupportInboxFilterFormProps = {
  connections: Connection[];
  connectionId?: string | undefined;
  linkedToTicketValue?: string | undefined;
  pageSize: number;
};

export function SupportInboxFilterForm({
  connections,
  connectionId,
  linkedToTicketValue,
  pageSize,
}: Readonly<SupportInboxFilterFormProps>) {
  const allConnections = "__all_connections__";
  const allLinked = "__all_linked__";
  const [selectedConnectionId, setSelectedConnectionId] = useState(connectionId ?? allConnections);
  const [selectedLinkedToTicket, setSelectedLinkedToTicket] = useState(
    linkedToTicketValue ?? allLinked,
  );
  const [selectedPageSize, setSelectedPageSize] = useState(String(pageSize));
  const connectionDisplayOptions = [
    { value: allConnections, label: tCrmClient("crm.supportInbox.filters.allConnections") },
    ...connections.map((connection) => ({ value: connection.id, label: connection.name })),
  ];
  const linkedToTicketDisplayOptions = [
    { value: allLinked, label: tCrmClient("crm.supportInbox.filters.all") },
    { value: "true", label: tCrmClient("crm.supportInbox.filters.linked") },
    { value: "false", label: tCrmClient("crm.supportInbox.filters.notLinked") },
  ];
  const pageSizeDisplayOptions = [
    { value: "10", label: "10" },
    { value: "20", label: "20" },
    { value: "50", label: "50" },
  ];

  return (
    <form method="get" className="grid gap-3 sm:grid-cols-4">
      <input
        type="hidden"
        name="connectionId"
        value={selectedConnectionId === allConnections ? "" : selectedConnectionId}
      />
      <input
        type="hidden"
        name="linkedToTicket"
        value={selectedLinkedToTicket === allLinked ? "" : selectedLinkedToTicket}
      />
      <input type="hidden" name="pageSize" value={selectedPageSize} />

      <div className="space-y-2">
        <label htmlFor="support-inbox-connectionId" className="text-sm font-medium">
          {tCrmClient("crm.supportInbox.filters.connection")}
        </label>
        <Select
          value={selectedConnectionId}
          onValueChange={(value) => setSelectedConnectionId(value ?? allConnections)}
        >
          <SelectTrigger id="support-inbox-connectionId">
            <SelectValue placeholder={tCrmClient("crm.supportInbox.filters.allConnections")}>
              {getSelectDisplayLabel(selectedConnectionId, connectionDisplayOptions)}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={allConnections}>
              {tCrmClient("crm.supportInbox.filters.allConnections")}
            </SelectItem>
            {connections.map((connection) => (
              <SelectItem key={connection.id} value={connection.id}>
                {connection.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-2">
        <label htmlFor="support-inbox-linkedToTicket" className="text-sm font-medium">
          {tCrmClient("crm.supportInbox.filters.ticketLink")}
        </label>
        <Select
          value={selectedLinkedToTicket}
          onValueChange={(value) => setSelectedLinkedToTicket(value ?? allLinked)}
        >
          <SelectTrigger id="support-inbox-linkedToTicket">
            <SelectValue placeholder={tCrmClient("crm.supportInbox.filters.all")}>
              {getSelectDisplayLabel(selectedLinkedToTicket, linkedToTicketDisplayOptions)}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={allLinked}>{tCrmClient("crm.supportInbox.filters.all")}</SelectItem>
            <SelectItem value="true">{tCrmClient("crm.supportInbox.filters.linked")}</SelectItem>
            <SelectItem value="false">
              {tCrmClient("crm.supportInbox.filters.notLinked")}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="space-y-2">
        <label htmlFor="support-inbox-pageSize" className="text-sm font-medium">
          {tCrmClient("crm.supportInbox.filters.pageSize")}
        </label>
        <Select
          value={selectedPageSize}
          onValueChange={(value) => setSelectedPageSize(value ?? "20")}
        >
          <SelectTrigger id="support-inbox-pageSize">
            <SelectValue>
              {getSelectDisplayLabel(selectedPageSize, pageSizeDisplayOptions)}
            </SelectValue>
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="10">10</SelectItem>
            <SelectItem value="20">20</SelectItem>
            <SelectItem value="50">50</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div className="flex items-end">
        <Button type="submit" variant="outline" className="w-full">
          {tCrmClient("crm.supportInbox.filters.apply")}
        </Button>
      </div>
    </form>
  );
}
