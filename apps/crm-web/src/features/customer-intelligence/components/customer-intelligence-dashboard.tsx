"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  Activity,
  ArrowRightLeft,
  Calendar,
  CheckCircle2,
  Building2,
  Cpu,
  CornerDownRight,
  Database,
  Fingerprint,
  Heart,
  HelpCircle,
  Mail,
  Network,
  Shield,
  Ticket,
  User,
} from "lucide-react";
import {
  Badge,
  Button,
  Field,
  FieldContent,
  FieldLabel,
  MetricGrid,
  SectionCard,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  Text,
} from "@netmetric/ui";
import {
  Avatar,
  AvatarFallback,
  AvatarImage,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@netmetric/ui/client";

import type { CustomerDetailDto, CustomerListItemDto } from "@/lib/crm-api";
import { tCrmClient } from "@/lib/i18n/crm-i18n";

type ActivityItem = {
  id: string;
  name: string;
  category: string;
  channel?: string | null;
  entityType?: string | null;
  occurredAtUtc: string;
};

type RelationshipItem = {
  id: string;
  name: string;
  relationshipType: string;
  source: { entityType: string; entityId: string };
  target: { entityType: string; entityId: string };
  strengthScore: number;
  isBidirectional: boolean;
  occurredAtUtc: string;
};

type BehavioralEventItem = {
  id: string;
  source: string;
  eventName: string;
  identityKey?: string | null;
  channel?: string | null;
  propertiesJson?: string | null;
  occurredAtUtc: string;
};

type LinkedIdentityItem = {
  id: string;
  identityType: string;
  identityValue: string;
  confidenceScore: number;
  resolutionNotes?: string | null;
  lastResolvedAtUtc: string;
};

type CustomerIntelligenceDashboardProps = {
  selectedCustomer: CustomerDetailDto;
  customersList: CustomerListItemDto[];
  workspace: {
    customerId: string;
    activityStream: ActivityItem[];
    relationshipGraph: RelationshipItem[];
    recentBehavioralEvents: BehavioralEventItem[];
    linkedIdentities: LinkedIdentityItem[];
  };
  portalSummary: {
    customerId: string;
    displayName: string;
    healthScore: number;
    openTickets: number;
    openOpportunities: number;
    openInvoices: number;
  };
  locale: string;
};

export function CustomerIntelligenceDashboard({
  selectedCustomer,
  customersList,
  workspace,
  portalSummary,
  locale,
}: Readonly<CustomerIntelligenceDashboardProps>) {
  const router = useRouter();
  const [selectedCustomerId, setSelectedCustomerId] = useState(selectedCustomer.id);
  const [expandedEventId, setExpandedEventId] = useState<string | null>(null);

  const handleCustomerChange = (val: string | null) => {
    if (val && val !== selectedCustomerId) {
      setSelectedCustomerId(val);
      router.push(`/customer-intelligence?customerId=${val}`);
    }
  };

  const getHealthTone = (score: number) => {
    if (score >= 75) return "text-emerald-500 bg-emerald-500/10 border-emerald-500/20";
    if (score >= 40) return "text-amber-500 bg-amber-500/10 border-amber-500/20";
    return "text-rose-500 bg-rose-500/10 border-rose-500/20";
  };

  const getConfidenceTone = (score: number) => {
    const s = score * 100;
    if (s >= 80) return "bg-emerald-500/10 text-emerald-500 border-emerald-500/20";
    if (s >= 50) return "bg-amber-500/10 text-amber-500 border-amber-500/20";
    return "bg-rose-500/10 text-rose-500 border-rose-500/20";
  };

  const formatDateTime = (isoString: string) => {
    try {
      const date = new Date(isoString);
      return new Intl.DateTimeFormat(undefined, {
        dateStyle: "medium",
        timeStyle: "short",
      }).format(date);
    } catch {
      return isoString;
    }
  };

  return (
    <div className="space-y-6">
      {/* Top Bar: Selector and Title */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-border/30 pb-4">
        <div>
          <h2 className="text-xl font-bold tracking-tight text-foreground">
            Customer Intelligence Hub
          </h2>
          <p className="text-xs text-muted-foreground mt-0.5">
            Unified behavioral telemetry, identity resolution, relationship graph, and custom health
            scoring.
          </p>
        </div>
        <div className="w-full md:w-80">
          <Field>
            <FieldLabel
              htmlFor="ci-customer-select"
              className="text-xs font-semibold text-muted-foreground mb-1"
            >
              Select Customer to Analyze
            </FieldLabel>
            <FieldContent>
              <Select value={selectedCustomerId} onValueChange={handleCustomerChange}>
                <SelectTrigger
                  id="ci-customer-select"
                  className="w-full bg-background border-border/60"
                >
                  <SelectValue>{selectedCustomer.fullName}</SelectValue>
                </SelectTrigger>
                <SelectContent>
                  {customersList.map((cust) => (
                    <SelectItem key={cust.id} value={cust.id}>
                      {cust.fullName} {cust.companyName ? `(${cust.companyName})` : ""}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </FieldContent>
          </Field>
        </div>
      </div>

      {/* Customer 360 Health Banner */}
      <div className="grid gap-6 md:grid-cols-3 lg:grid-cols-4 items-stretch">
        {/* Profile Card */}
        <div className="md:col-span-2 rounded-xl border border-border/40 bg-card/40 p-5 flex flex-col sm:flex-row gap-5 items-center sm:items-start">
          <Avatar className="size-20 border border-border/40 shrink-0">
            {selectedCustomer.imageUrl ? (
              <AvatarImage src={selectedCustomer.imageUrl} alt={selectedCustomer.fullName} />
            ) : null}
            <AvatarFallback>
              <User className="size-9 text-muted-foreground/60" />
            </AvatarFallback>
          </Avatar>
          <div className="space-y-2 flex-1 min-w-0 text-center sm:text-left">
            <div>
              <div className="flex flex-wrap justify-center sm:justify-start items-center gap-2">
                <h3 className="text-base font-semibold tracking-tight text-foreground">
                  {selectedCustomer.fullName}
                </h3>
                {selectedCustomer.isVip ? (
                  <Badge
                    variant="outline"
                    className="bg-amber-500/10 text-amber-500 border-amber-500/20 text-[10px] py-0 px-1.5 h-4 font-semibold uppercase tracking-wider"
                  >
                    VIP
                  </Badge>
                ) : null}
                <Badge
                  variant="outline"
                  className={
                    selectedCustomer.isActive
                      ? "bg-emerald-500/10 text-emerald-500 border-emerald-500/20 text-[10px] py-0 px-1.5 h-4"
                      : "bg-muted text-muted-foreground text-[10px] py-0 px-1.5 h-4"
                  }
                >
                  {selectedCustomer.isActive ? "Active" : "Inactive"}
                </Badge>
              </div>
              <p className="text-xs text-muted-foreground mt-0.5 font-medium">
                {selectedCustomer.jobTitle ? `${selectedCustomer.jobTitle}` : ""}
                {selectedCustomer.department ? ` - ${selectedCustomer.department}` : ""}
              </p>
            </div>

            <div className="flex flex-wrap justify-center sm:justify-start gap-4 pt-1.5 text-xs text-muted-foreground">
              {selectedCustomer.companyName ? (
                <div className="flex items-center gap-1.5">
                  <Building2 className="size-3.5 shrink-0 text-muted-foreground/65" />
                  <span>{selectedCustomer.companyName}</span>
                </div>
              ) : null}
              {selectedCustomer.email ? (
                <div className="flex items-center gap-1.5">
                  <Mail className="size-3.5 shrink-0 text-muted-foreground/65" />
                  <span className="truncate">{selectedCustomer.email}</span>
                </div>
              ) : null}
            </div>
          </div>
        </div>

        <div className="md:col-span-1 lg:col-span-2">
          <MetricGrid
            columns="four"
            items={[
              {
                label: "Health Index",
                value: `${portalSummary.healthScore}%`,
                description: "Weighted by SLA, invoice status, and engagement.",
                icon: <Heart className="size-4" />,
                tone:
                  Number(portalSummary.healthScore) >= 75
                    ? "success"
                    : Number(portalSummary.healthScore) >= 40
                      ? "warning"
                      : "danger",
              },
              {
                label: "Open Tickets",
                value: portalSummary.openTickets,
                icon: <Ticket className="size-4" />,
                tone: "neutral",
              },
              {
                label: "Open Opportunities",
                value: portalSummary.openOpportunities,
                icon: <Activity className="size-4" />,
                tone: "info",
              },
              {
                label: "Open Invoices",
                value: portalSummary.openInvoices,
                icon: <Database className="size-4" />,
                tone: "warning",
              },
            ]}
          />
        </div>
      </div>

      {/* Main Tabs Workspace */}
      <Tabs defaultValue="identities" className="w-full">
        <TabsList className="grid grid-cols-2 md:grid-cols-4 gap-1 rounded-xl bg-muted/20 border p-1">
          <TabsTrigger value="identities" className="gap-2 text-xs">
            <Fingerprint className="size-3.5" />
            <span>Identity Matching</span>
          </TabsTrigger>
          <TabsTrigger value="relationships" className="gap-2 text-xs">
            <Network className="size-3.5" />
            <span>Relationship Graph</span>
          </TabsTrigger>
          <TabsTrigger value="activities" className="gap-2 text-xs">
            <Activity className="size-3.5" />
            <span>Activity Stream</span>
          </TabsTrigger>
          <TabsTrigger value="events" className="gap-2 text-xs">
            <Cpu className="size-3.5" />
            <span>Behavioral Events</span>
          </TabsTrigger>
        </TabsList>

        {/* Tab 1: Linked Identities */}
        <TabsContent value="identities" className="mt-4">
          <SectionCard
            title="Identity Resolution Profile"
            description="Cross-device and multi-channel profile links resolved via customer data platform."
            className="border-border/40 bg-card/25"
          >
            {workspace.linkedIdentities.length === 0 ? (
              <div className="text-center py-12 text-muted-foreground text-xs">
                No linked identities resolved for this customer yet.
              </div>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader className="bg-muted/10">
                    <TableRow>
                      <TableHead className="text-xs font-semibold">Identity Type</TableHead>
                      <TableHead className="text-xs font-semibold">Value</TableHead>
                      <TableHead className="text-xs font-semibold text-center">
                        Confidence
                      </TableHead>
                      <TableHead className="text-xs font-semibold">Resolution Notes</TableHead>
                      <TableHead className="text-xs font-semibold">Resolved At</TableHead>
                    </TableRow>
                  </TableHeader>
                  <TableBody>
                    {workspace.linkedIdentities.map((identity) => (
                      <TableRow key={identity.id} className="hover:bg-muted/5">
                        <TableCell className="font-semibold text-xs text-foreground">
                          <span className="capitalize">{identity.identityType}</span>
                        </TableCell>
                        <TableCell className="font-mono text-xs text-muted-foreground break-all">
                          {identity.identityValue}
                        </TableCell>
                        <TableCell className="text-center">
                          <Badge
                            variant="outline"
                            className={`font-semibold text-xs px-2 py-0.5 rounded ${getConfidenceTone(identity.confidenceScore)}`}
                          >
                            {Math.round(identity.confidenceScore * 100)}%
                          </Badge>
                        </TableCell>
                        <TableCell className="text-xs text-muted-foreground">
                          {identity.resolutionNotes ?? "-"}
                        </TableCell>
                        <TableCell className="text-xs text-muted-foreground font-mono">
                          {formatDateTime(identity.lastResolvedAtUtc)}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </SectionCard>
        </TabsContent>

        {/* Tab 2: Relationship Graph */}
        <TabsContent value="relationships" className="mt-4">
          <div className="rounded-xl border border-border/40 bg-card/25 p-5 space-y-4">
            <div className="flex items-center gap-2 border-b border-border/20 pb-3">
              <Network className="size-5 text-primary" />
              <div>
                <h4 className="text-sm font-semibold text-foreground">
                  Account Relationship Graph
                </h4>
                <p className="text-xs text-muted-foreground">
                  Map of connections, dependencies, and relationship strengths with other accounts
                  or contacts.
                </p>
              </div>
            </div>
            {workspace.relationshipGraph.length === 0 ? (
              <div className="text-center py-12 text-muted-foreground text-xs">
                No relationship edges defined for this customer yet.
              </div>
            ) : (
              <div className="grid gap-4 md:grid-cols-2">
                {workspace.relationshipGraph.map((rel) => (
                  <div
                    key={rel.id}
                    className="rounded-lg border bg-background/50 p-4 space-y-3 shadow-xs"
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <div className="text-xs font-semibold text-foreground flex items-center gap-1.5">
                          <span>{rel.name}</span>
                          {rel.isBidirectional ? (
                            <ArrowRightLeft className="size-3 text-muted-foreground/60" />
                          ) : null}
                        </div>
                        <p className="text-[10px] text-muted-foreground mt-0.5 uppercase tracking-wider font-semibold">
                          {rel.relationshipType}
                        </p>
                      </div>
                      <Badge variant="secondary" className="text-[10px] font-semibold">
                        Strength: {Math.round(rel.strengthScore * 100)}%
                      </Badge>
                    </div>
                    {/* Progress Bar of Strength */}
                    <div className="h-1.5 w-full bg-muted-foreground/10 rounded-full overflow-hidden">
                      <div
                        className="h-full bg-primary transition-all"
                        style={{ width: `${Math.round(rel.strengthScore * 100)}%` }}
                      />
                    </div>
                    {/* Node path description */}
                    <div className="text-[11px] text-muted-foreground flex items-center gap-1">
                      <span className="font-semibold text-foreground/80">
                        {rel.source.entityType}
                      </span>
                      <CornerDownRight className="size-3 text-muted-foreground" />
                      <span className="font-semibold text-foreground/80">
                        {rel.target.entityType}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </TabsContent>

        {/* Tab 3: Customer Activity Stream */}
        <TabsContent value="activities" className="mt-4">
          <div className="rounded-xl border border-border/40 bg-card/25 p-5 space-y-4">
            <div className="flex items-center gap-2 border-b border-border/20 pb-3">
              <Activity className="size-5 text-primary" />
              <div>
                <h4 className="text-sm font-semibold text-foreground">Timeline Interaction Feed</h4>
                <p className="text-xs text-muted-foreground">
                  Historical records of communications, support handoffs, status changes, and
                  meetings.
                </p>
              </div>
            </div>
            {workspace.activityStream.length === 0 ? (
              <div className="text-center py-12 text-muted-foreground text-xs">
                No timeline activity logged for this customer.
              </div>
            ) : (
              <div className="relative border-l border-border pl-6 ml-3 space-y-6 py-2">
                {workspace.activityStream.map((act) => (
                  <div key={act.id} className="relative group">
                    {/* Timeline marker */}
                    <div className="absolute -left-[31px] top-1 bg-background border border-border/80 rounded-full p-1 group-hover:border-primary/50 transition-colors shadow-xs">
                      <div className="size-2 rounded-full bg-primary" />
                    </div>
                    {/* Content */}
                    <div className="space-y-1.5">
                      <div className="flex flex-wrap items-center justify-between gap-2">
                        <h5 className="text-xs font-semibold text-foreground leading-none">
                          {act.name}
                        </h5>
                        <time className="text-[10px] text-muted-foreground/85 font-mono">
                          {formatDateTime(act.occurredAtUtc)}
                        </time>
                      </div>
                      <div className="flex flex-wrap items-center gap-2">
                        <Badge
                          variant="outline"
                          className="text-[9px] rounded-sm py-0 px-1 bg-muted/10 text-muted-foreground"
                        >
                          {act.category}
                        </Badge>
                        {act.channel ? (
                          <Badge
                            variant="outline"
                            className="text-[9px] rounded-sm py-0 px-1 bg-muted/20 text-muted-foreground"
                          >
                            Channel: {act.channel}
                          </Badge>
                        ) : null}
                        {act.entityType ? (
                          <Badge
                            variant="outline"
                            className="text-[9px] rounded-sm py-0 px-1 bg-muted/20 text-muted-foreground/75"
                          >
                            Ref: {act.entityType}
                          </Badge>
                        ) : null}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </TabsContent>

        {/* Tab 4: Behavioral telemetry (CDP Events) */}
        <TabsContent value="events" className="mt-4">
          <div className="rounded-xl border border-border/40 bg-card/25 p-5 space-y-4">
            <div className="flex items-center gap-2 border-b border-border/20 pb-3">
              <Cpu className="size-5 text-primary" />
              <div>
                <h4 className="text-sm font-semibold text-foreground">
                  Behavioral Telemetry Feed (CDP)
                </h4>
                <p className="text-xs text-muted-foreground">
                  Real-time digital signals, page views, and click streams tracked on client portals
                  and applications.
                </p>
              </div>
            </div>
            {workspace.recentBehavioralEvents.length === 0 ? (
              <div className="text-center py-12 text-muted-foreground text-xs">
                No behavioral events tracked for this customer.
              </div>
            ) : (
              <div className="space-y-3">
                {workspace.recentBehavioralEvents.map((evt) => {
                  const isExpanded = expandedEventId === evt.id;
                  return (
                    <div
                      key={evt.id}
                      className="rounded-lg border bg-background/40 hover:bg-background/70 transition-colors p-3.5 space-y-2"
                    >
                      <div className="flex flex-wrap items-center justify-between gap-3">
                        <div className="flex items-center gap-2.5 min-w-0">
                          <div className="p-1.5 rounded-md bg-primary/10 border border-primary/20 text-primary shrink-0">
                            <Calendar className="size-3.5" />
                          </div>
                          <span className="font-mono text-xs font-semibold text-foreground truncate">
                            {evt.eventName}
                          </span>
                        </div>
                        <div className="flex items-center gap-2 shrink-0">
                          <span className="text-[10px] text-muted-foreground font-mono">
                            {formatDateTime(evt.occurredAtUtc)}
                          </span>
                          <Button
                            size="xs"
                            variant="ghost"
                            onClick={() => setExpandedEventId(isExpanded ? null : evt.id)}
                            className="text-[10px] h-6 cursor-pointer"
                          >
                            {isExpanded ? "Hide Details" : "View Properties"}
                          </Button>
                        </div>
                      </div>
                      <div className="flex flex-wrap items-center gap-2 text-[10px] text-muted-foreground">
                        <span>
                          Source: <strong className="text-foreground/80">{evt.source}</strong>
                        </span>
                        <span>•</span>
                        <span>
                          Channel:{" "}
                          <strong className="text-foreground/80">{evt.channel ?? "Web"}</strong>
                        </span>
                        {evt.identityKey ? (
                          <>
                            <span>•</span>
                            <span className="font-mono">Identity: {evt.identityKey}</span>
                          </>
                        ) : null}
                      </div>

                      {/* Expandable JSON Properties */}
                      {isExpanded && evt.propertiesJson ? (
                        <div className="mt-2 p-3 rounded bg-muted/40 border text-[10px] font-mono text-muted-foreground overflow-auto max-h-40 whitespace-pre">
                          {(() => {
                            try {
                              return JSON.stringify(JSON.parse(evt.propertiesJson), null, 2);
                            } catch {
                              return evt.propertiesJson;
                            }
                          })()}
                        </div>
                      ) : null}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
