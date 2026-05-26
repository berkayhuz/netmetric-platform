// <copyright file="WinLossReviewDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Contracts.DTOs;

public sealed record WinLossReviewDto(Guid Id, Guid DealId, string Outcome, string? Summary, string? Strengths, string? Risks, string? CompetitorName, decimal? CompetitorPrice, string? CustomerFeedback, DateTime? ReviewedAt, Guid? ReviewedByUserId, string? RowVersion);
