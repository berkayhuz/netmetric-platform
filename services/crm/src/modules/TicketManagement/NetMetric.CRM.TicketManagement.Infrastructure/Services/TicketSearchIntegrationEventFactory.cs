// <copyright file="TicketSearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Support;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Services;

public static class TicketSearchIntegrationEventFactory
{
    public const string TicketReadPermission = "tickets.read";
    private const string EntityType = "ticket";

    public static SearchDocumentIndexRequestedV1 CreateTicketIndexRequested(
        Ticket ticket,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(ticket);
        ArgumentException.ThrowIfNullOrWhiteSpace(ticket.Subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(ticket.TicketNumber);

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, ticket.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: ticket.Subject.Trim(),
            Summary: "Support ticket.",
            Content: BuildSafeSearchContent(ticket),
            Url: $"/tickets/{ticket.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [TicketReadPermission],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "tickets", "ticket"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(ticket.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(ticket.UpdatedAt ?? ticket.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(ticket, tenantId),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreateTicketDeleteRequested(
        Guid ticketId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        return new SearchDocumentDeleteRequestedV1(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, ticketId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static string BuildDocumentId(Guid tenantId, Guid ticketId)
        => $"crm-ticket-{tenantId:N}-{ticketId:N}";

    private static IReadOnlyDictionary<string, string> BuildMetadata(Ticket ticket, Guid tenantId)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = ticket.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N"),
            ["ticketNumber"] = ticket.TicketNumber.Trim()
        };

    private static string BuildSafeSearchContent(Ticket ticket)
        => string.Join(
            '\n',
            new[]
            {
                ticket.Subject.Trim(),
                ticket.TicketNumber.Trim()
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static DateTimeOffset ToUtcDateTimeOffset(DateTime value)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return new DateTimeOffset(utcValue);
    }
}
