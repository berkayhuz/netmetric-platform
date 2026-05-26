// <copyright file="ReportDtos.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.AnalyticsReporting.Application.DTOs;

public sealed record SalesFunnelSummaryDto(int OpenLeads, int QualifiedLeads, int OpenOpportunities, int WonDeals, decimal PipelineValue);
public sealed record CampaignRoiDto(string CampaignName, decimal Spend, decimal Revenue, decimal RoiPercentage);
public sealed record SupportKpiDto(int OpenTickets, int OverdueTickets, decimal FirstResponseHours, decimal ResolutionHours);
public sealed record RevenueAgingDto(decimal CurrentAmount, decimal Days30, decimal Days60, decimal Days90Plus);
public sealed record ProductivityDto(Guid UserId, string UserName, int ActivitiesCompleted, int TicketsClosed, int DealsWon);
public sealed record TenantReportSummaryDto(Guid TenantId, string TenantName, int ActiveUsers, int Customers, decimal Revenue, int OpenTickets);
public sealed record AnalyticsProjectionStatusDto(
    string Status,
    DateTime? LastAttemptAtUtc,
    DateTime? LastSuccessfulRunAtUtc,
    int ProjectedTenantCount,
    string? ErrorMessage,
    DateTime? TenantSummarySnapshotAtUtc,
    DateTime? SalesFunnelSnapshotAtUtc,
    DateTime? CampaignRoiSnapshotAtUtc,
    DateTime? RevenueAgingSnapshotAtUtc,
    DateTime? SupportKpiSnapshotAtUtc,
    DateTime? UserProductivitySnapshotAtUtc);
