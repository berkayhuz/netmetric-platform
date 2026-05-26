// <copyright file="DealOutcomeHistoryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Contracts.DTOs;

public sealed record DealOutcomeHistoryDto(Guid Id, string Outcome, string Stage, DateTime OccurredAt, Guid? ChangedByUserId, Guid? LostReasonId, string? Note);
