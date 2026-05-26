// <copyright file="DealDtoMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.DealManagement.Contracts.DTOs;
using NetMetric.CRM.DealManagement.Contracts.Enums;
using NetMetric.CRM.DealManagement.Domain.Entities;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.DealManagement.Application.Common;

public static class DealDtoMappings
{
    public static DealListItemDto ToListItemDto(this Deal entity, bool includeFinancialData = true)
        => new(entity.Id, entity.DealCode, entity.Name, includeFinancialData ? entity.TotalAmount : null, entity.ClosedDate, entity.OpportunityId, entity.CompanyId, entity.OwnerUserId, ResolveStage(entity), ResolveOutcome(entity), entity.IsActive);

    public static DealOutcomeHistoryDto ToDto(this DealOutcomeHistory entity)
        => new(entity.Id, entity.Outcome, entity.Stage, entity.OccurredAt, entity.ChangedByUserId, entity.LostReasonId, entity.Note);

    public static WinLossReviewDto ToDto(this WinLossReview entity, bool includeFinancialData = true, bool includeInternalNotes = true)
        => new(entity.Id, entity.DealId, entity.Outcome, includeInternalNotes ? entity.Summary : null, includeInternalNotes ? entity.Strengths : null, includeInternalNotes ? entity.Risks : null, includeInternalNotes ? entity.CompetitorName : null, includeFinancialData ? entity.CompetitorPrice : null, includeInternalNotes ? entity.CustomerFeedback : null, entity.ReviewedAt, entity.ReviewedByUserId, Convert.ToBase64String(entity.RowVersion));

    public static DealDetailDto ToDetailDto(this Deal entity, WinLossReview? review, IReadOnlyList<DealOutcomeHistory> history, bool includeFinancialData = true, bool includeInternalNotes = true)
        => new(entity.Id, entity.DealCode, entity.Name, includeFinancialData ? entity.TotalAmount : null, entity.ClosedDate, entity.OpportunityId, entity.CompanyId, entity.OwnerUserId, ResolveStage(entity), ResolveOutcome(entity), GetLatestLostReason(history), includeInternalNotes ? GetLatestLostNote(history) : null, entity.IsActive, review?.ToDto(includeFinancialData, includeInternalNotes), history.OrderByDescending(x => x.OccurredAt).Select(x => includeInternalNotes ? x.ToDto() : x.ToDto() with { Note = null }).ToList(), Convert.ToBase64String(entity.RowVersion));

    private static Guid? GetLatestLostReason(IReadOnlyList<DealOutcomeHistory> history)
        => history.OrderByDescending(x => x.OccurredAt).FirstOrDefault(x => string.Equals(x.Outcome, "Lost", StringComparison.OrdinalIgnoreCase))?.LostReasonId;

    private static string? GetLatestLostNote(IReadOnlyList<DealOutcomeHistory> history)
        => history.OrderByDescending(x => x.OccurredAt).FirstOrDefault(x => string.Equals(x.Outcome, "Lost", StringComparison.OrdinalIgnoreCase))?.Note;

    public static DealLifecycleStage ResolveStage(Deal entity)
    {
        var won = entity.ClosedDate <= DateTime.UtcNow && entity.TotalAmount > 0m;
        return won ? DealLifecycleStage.Won : DealLifecycleStage.Open;
    }

    public static WinLossOutcomeType ResolveOutcome(Deal entity)
        => entity.TotalAmount > 0m ? WinLossOutcomeType.Won : WinLossOutcomeType.Pending;
}
