// <copyright file="DealOutcomeRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Contracts.Requests;

public sealed record DealOutcomeRequest(DateTime? OccurredAt, Guid? LostReasonId, string? Note, string? RowVersion);
