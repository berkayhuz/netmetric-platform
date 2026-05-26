// <copyright file="GetCustomerPortalSummaryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Insights;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerTimelineEntrys;
using System.Text.Json;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Insights.Queries.GetCustomerPortalSummary;

public sealed class GetCustomerPortalSummaryQueryHandler(
    ICustomerIntelligenceDbContext dbContext,
    ICustomerPortalSummaryMetricsProvider metricsProvider) : IRequestHandler<GetCustomerPortalSummaryQuery, CustomerPortalSummaryDto>
{
    private static readonly string[] HealthScoreJsonKeys = ["healthScore", "score", "value", "healthIndex"];
    private static readonly string[] OpenTicketsJsonKeys = ["openTickets", "ticketsOpen"];
    private static readonly string[] OpenOpportunitiesJsonKeys = ["openOpportunities", "opportunitiesOpen"];
    private static readonly string[] OpenInvoicesJsonKeys = ["openInvoices", "invoicesOpen"];

    private static readonly string[] OpenStatusTokens = ["open", "new", "in-progress", "pending", "active"];
    private static readonly string[] ClosedTicketTokens = ["closed", "resolved", "done", "cancelled", "canceled"];
    private static readonly string[] ClosedOpportunityTokens = ["closed-won", "closed-lost", "won", "lost"];
    private static readonly string[] ClosedInvoiceTokens = ["paid", "void", "cancelled", "canceled"];

    public async Task<CustomerPortalSummaryDto> Handle(GetCustomerPortalSummaryQuery request, CancellationToken cancellationToken)
    {
        var healthRows = await dbContext.CustomerHealthScores
            .AsNoTracking()
            .Where(x => x.RelatedEntityId == request.CustomerId || (x.EntityType == "Customer" && x.RelatedEntityId == request.CustomerId))
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        var latestHealthRow = healthRows.FirstOrDefault();
        var healthScore = ResolveHealthScore(healthRows);

        var timelineRows = await dbContext.CustomerTimelineEntrys
            .AsNoTracking()
            .Where(x => x.SubjectType == "Customer" && x.SubjectId == request.CustomerId)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(300)
            .ToListAsync(cancellationToken);

        var timelineOpenTickets = CountOpenByEntityType(timelineRows, "ticket", ClosedTicketTokens);
        var timelineOpenOpportunities = CountOpenByEntityType(timelineRows, "opportunity", ClosedOpportunityTokens);
        var timelineOpenInvoices = CountOpenByEntityType(timelineRows, "invoice", ClosedInvoiceTokens);

        var providerMetrics = await metricsProvider.GetMetricsAsync(request.CustomerId, cancellationToken);

        var openTickets = providerMetrics.OpenTickets >= 0
            ? providerMetrics.OpenTickets
            : timelineOpenTickets >= 0
            ? timelineOpenTickets
            : ResolveCounterFromHealthRows(healthRows, OpenTicketsJsonKeys);
        var openOpportunities = providerMetrics.OpenOpportunities >= 0
            ? providerMetrics.OpenOpportunities
            : timelineOpenOpportunities >= 0
            ? timelineOpenOpportunities
            : ResolveCounterFromHealthRows(healthRows, OpenOpportunitiesJsonKeys);
        var openInvoices = providerMetrics.OpenInvoices >= 0
            ? providerMetrics.OpenInvoices
            : timelineOpenInvoices >= 0
            ? timelineOpenInvoices
            : ResolveCounterFromHealthRows(healthRows, OpenInvoicesJsonKeys);

        return new CustomerPortalSummaryDto
        {
            CustomerId = request.CustomerId,
            DisplayName = !string.IsNullOrWhiteSpace(latestHealthRow?.Name)
                ? latestHealthRow!.Name
                : $"Customer {request.CustomerId:D}",
            HealthScore = healthScore,
            OpenTickets = Math.Max(0, openTickets),
            OpenOpportunities = Math.Max(0, openOpportunities),
            OpenInvoices = Math.Max(0, openInvoices),
        };
    }

    private static int CountOpenByEntityType(IReadOnlyList<CustomerTimelineEntry> timelineRows, string entityType, string[] closedTokens)
    {
        var candidates = timelineRows
            .Where(x => MatchesEntityType(x, entityType))
            .ToList();

        if (candidates.Count == 0)
        {
            return -1;
        }

        return candidates.Count(x => IsOpenTimelineItem(x, closedTokens));
    }

    private static decimal ResolveHealthScore(IReadOnlyList<Domain.Entities.CustomerHealthScores.CustomerHealthScore> healthRows)
    {
        foreach (var row in healthRows)
        {
            if (TryExtractDecimalFromJson(row.DataJson, HealthScoreJsonKeys, out var score))
            {
                return decimal.Clamp(score, 0m, 100m);
            }
        }

        return 0m;
    }

    private static int ResolveCounterFromHealthRows(IReadOnlyList<Domain.Entities.CustomerHealthScores.CustomerHealthScore> healthRows, string[] keys)
    {
        foreach (var row in healthRows)
        {
            if (TryExtractIntFromJson(row.DataJson, keys, out var value))
            {
                return value;
            }
        }

        return 0;
    }

    private static bool TryExtractDecimalFromJson(string? json, string[] candidateKeys, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var key in candidateKeys)
            {
                if (!TryGetPropertyIgnoreCase(doc.RootElement, key, out var property))
                {
                    continue;
                }

                switch (property.ValueKind)
                {
                    case JsonValueKind.Number when property.TryGetDecimal(out var numericValue):
                        value = numericValue;
                        return true;
                    case JsonValueKind.String when decimal.TryParse(property.GetString(), out var parsedString):
                        value = parsedString;
                        return true;
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    private static bool TryExtractIntFromJson(string? json, string[] candidateKeys, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            foreach (var key in candidateKeys)
            {
                if (!TryGetPropertyIgnoreCase(doc.RootElement, key, out var property))
                {
                    continue;
                }

                switch (property.ValueKind)
                {
                    case JsonValueKind.Number when property.TryGetInt32(out var numericValue):
                        value = numericValue;
                        return true;
                    case JsonValueKind.String when int.TryParse(property.GetString(), out var parsedString):
                        value = parsedString;
                        return true;
                }
            }
        }
        catch (JsonException)
        {
            return false;
        }

        return false;
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement root, string name, out JsonElement value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static bool MatchesEntityType(CustomerTimelineEntry entry, string entityType)
    {
        var normalizedEntityType = (entry.EntityType ?? string.Empty).Trim();
        var normalizedCategory = (entry.Category ?? string.Empty).Trim();
        return normalizedEntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase)
            || normalizedCategory.Equals(entityType, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsOpenTimelineItem(CustomerTimelineEntry entry, string[] closedTokens)
    {
        var status = ReadStatusValue(entry.DataJson) ?? entry.Category ?? entry.Name;
        if (string.IsNullOrWhiteSpace(status))
        {
            return true;
        }

        var normalized = status.Trim().ToLowerInvariant();
        if (closedTokens.Any(token => normalized.Contains(token, StringComparison.Ordinal)))
        {
            return false;
        }

        return OpenStatusTokens.Any(token => normalized.Contains(token, StringComparison.Ordinal))
            || !closedTokens.Any(token => normalized.Contains(token, StringComparison.Ordinal));
    }

    private static string? ReadStatusValue(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            if (TryGetPropertyIgnoreCase(doc.RootElement, "status", out var statusValue))
            {
                return statusValue.ValueKind == JsonValueKind.String ? statusValue.GetString() : statusValue.ToString();
            }
        }
        catch (JsonException)
        {
            return null;
        }

        return null;
    }
}
