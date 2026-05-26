// <copyright file="AnalyticsReadModels.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.AnalyticsReporting.Domain.Entities;

public sealed class AnalyticsTenantSnapshot : EntityBase
{
    private AnalyticsTenantSnapshot() { }

    public AnalyticsTenantSnapshot(
        Guid tenantId,
        string tenantName,
        int activeUsers,
        int customers,
        decimal revenue,
        int openTickets,
        DateTime snapshotAtUtc)
    {
        TenantId = tenantId;
        TenantName = Guard.AgainstNullOrWhiteSpace(tenantName);
        ActiveUsers = Math.Max(0, activeUsers);
        Customers = Math.Max(0, customers);
        Revenue = revenue;
        OpenTickets = Math.Max(0, openTickets);
        SnapshotAtUtc = snapshotAtUtc;
        CreatedAt = snapshotAtUtc;
    }

    public string TenantName { get; private set; } = null!;
    public int ActiveUsers { get; private set; }
    public int Customers { get; private set; }
    public decimal Revenue { get; private set; }
    public int OpenTickets { get; private set; }
    public DateTime SnapshotAtUtc { get; private set; }
}

public sealed class AnalyticsSalesFunnelSnapshot : EntityBase
{
    private AnalyticsSalesFunnelSnapshot() { }

    public AnalyticsSalesFunnelSnapshot(
        Guid tenantId,
        int openLeads,
        int qualifiedLeads,
        int openOpportunities,
        int wonDeals,
        decimal pipelineValue,
        DateTime snapshotAtUtc)
    {
        TenantId = tenantId;
        OpenLeads = Math.Max(0, openLeads);
        QualifiedLeads = Math.Max(0, qualifiedLeads);
        OpenOpportunities = Math.Max(0, openOpportunities);
        WonDeals = Math.Max(0, wonDeals);
        PipelineValue = pipelineValue;
        SnapshotAtUtc = snapshotAtUtc;
        CreatedAt = snapshotAtUtc;
    }

    public int OpenLeads { get; private set; }
    public int QualifiedLeads { get; private set; }
    public int OpenOpportunities { get; private set; }
    public int WonDeals { get; private set; }
    public decimal PipelineValue { get; private set; }
    public DateTime SnapshotAtUtc { get; private set; }
}

public sealed class AnalyticsCampaignRoiSnapshot : EntityBase
{
    private AnalyticsCampaignRoiSnapshot() { }

    public AnalyticsCampaignRoiSnapshot(
        Guid tenantId,
        string campaignName,
        decimal spend,
        decimal revenue,
        DateTime snapshotAtUtc)
    {
        TenantId = tenantId;
        CampaignName = Guard.AgainstNullOrWhiteSpace(campaignName);
        Spend = spend;
        Revenue = revenue;
        RoiPercentage = spend == 0 ? 0 : decimal.Round(((revenue - spend) / spend) * 100, 2);
        SnapshotAtUtc = snapshotAtUtc;
        CreatedAt = snapshotAtUtc;
    }

    public string CampaignName { get; private set; } = null!;
    public decimal Spend { get; private set; }
    public decimal Revenue { get; private set; }
    public decimal RoiPercentage { get; private set; }
    public DateTime SnapshotAtUtc { get; private set; }
}

public sealed class AnalyticsRevenueAgingSnapshot : EntityBase
{
    private AnalyticsRevenueAgingSnapshot() { }

    public AnalyticsRevenueAgingSnapshot(
        Guid tenantId,
        decimal currentAmount,
        decimal days30,
        decimal days60,
        decimal days90Plus,
        DateTime snapshotAtUtc)
    {
        TenantId = tenantId;
        CurrentAmount = currentAmount;
        Days30 = days30;
        Days60 = days60;
        Days90Plus = days90Plus;
        SnapshotAtUtc = snapshotAtUtc;
        CreatedAt = snapshotAtUtc;
    }

    public decimal CurrentAmount { get; private set; }
    public decimal Days30 { get; private set; }
    public decimal Days60 { get; private set; }
    public decimal Days90Plus { get; private set; }
    public DateTime SnapshotAtUtc { get; private set; }
}

public sealed class AnalyticsSupportKpiSnapshot : EntityBase
{
    private AnalyticsSupportKpiSnapshot() { }

    public AnalyticsSupportKpiSnapshot(
        Guid tenantId,
        int openTickets,
        int overdueTickets,
        decimal firstResponseHours,
        decimal resolutionHours,
        DateTime snapshotAtUtc)
    {
        TenantId = tenantId;
        OpenTickets = Math.Max(0, openTickets);
        OverdueTickets = Math.Max(0, overdueTickets);
        FirstResponseHours = firstResponseHours < 0 ? 0 : firstResponseHours;
        ResolutionHours = resolutionHours < 0 ? 0 : resolutionHours;
        SnapshotAtUtc = snapshotAtUtc;
        CreatedAt = snapshotAtUtc;
    }

    public int OpenTickets { get; private set; }
    public int OverdueTickets { get; private set; }
    public decimal FirstResponseHours { get; private set; }
    public decimal ResolutionHours { get; private set; }
    public DateTime SnapshotAtUtc { get; private set; }
}

public sealed class AnalyticsUserProductivitySnapshot : EntityBase
{
    private AnalyticsUserProductivitySnapshot() { }

    public AnalyticsUserProductivitySnapshot(
        Guid tenantId,
        Guid userId,
        string userName,
        int activitiesCompleted,
        int ticketsClosed,
        int dealsWon,
        DateTime snapshotAtUtc)
    {
        TenantId = tenantId;
        UserId = userId;
        UserName = Guard.AgainstNullOrWhiteSpace(userName);
        ActivitiesCompleted = Math.Max(0, activitiesCompleted);
        TicketsClosed = Math.Max(0, ticketsClosed);
        DealsWon = Math.Max(0, dealsWon);
        SnapshotAtUtc = snapshotAtUtc;
        CreatedAt = snapshotAtUtc;
    }

    public Guid UserId { get; private set; }
    public string UserName { get; private set; } = null!;
    public int ActivitiesCompleted { get; private set; }
    public int TicketsClosed { get; private set; }
    public int DealsWon { get; private set; }
    public DateTime SnapshotAtUtc { get; private set; }
}

public sealed class AnalyticsProjectionRun
{
    private AnalyticsProjectionRun() { }

    public AnalyticsProjectionRun(string correlationId, DateTime startedAtUtc)
    {
        Id = Guid.NewGuid();
        CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId);
        StartedAtUtc = startedAtUtc;
        Status = AnalyticsProjectionRunStatus.Running;
    }

    public Guid Id { get; private set; }
    public string CorrelationId { get; private set; } = null!;
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public AnalyticsProjectionRunStatus Status { get; private set; }
    public int ProjectedTenantCount { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void MarkSucceeded(int projectedTenantCount, DateTime completedAtUtc)
    {
        ProjectedTenantCount = Math.Max(0, projectedTenantCount);
        CompletedAtUtc = completedAtUtc;
        Status = AnalyticsProjectionRunStatus.Succeeded;
        ErrorMessage = null;
    }

    public void MarkFailed(string errorMessage, DateTime completedAtUtc)
    {
        CompletedAtUtc = completedAtUtc;
        Status = AnalyticsProjectionRunStatus.Failed;
        ErrorMessage = string.IsNullOrWhiteSpace(errorMessage) ? "Analytics projection failed." : errorMessage.Trim();
    }
}

public enum AnalyticsProjectionRunStatus
{
    Running = 0,
    Succeeded = 1,
    Failed = 2
}
