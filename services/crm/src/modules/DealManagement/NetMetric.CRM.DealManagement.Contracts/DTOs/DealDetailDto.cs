// <copyright file="DealDetailDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.DealManagement.Contracts.Enums;

namespace NetMetric.CRM.DealManagement.Contracts.DTOs;

public sealed record DealDetailDto(Guid Id, string DealCode, string Name, decimal? TotalAmount, DateTime ClosedDate, Guid? OpportunityId, Guid? CompanyId, Guid? OwnerUserId, DealLifecycleStage Stage, WinLossOutcomeType Outcome, Guid? LostReasonId, string? LostNote, bool IsActive, WinLossReviewDto? Review, IReadOnlyList<DealOutcomeHistoryDto> History, string RowVersion);
