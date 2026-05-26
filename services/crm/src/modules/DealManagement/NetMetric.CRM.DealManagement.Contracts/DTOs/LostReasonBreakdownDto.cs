// <copyright file="LostReasonBreakdownDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Contracts.DTOs;

public sealed record LostReasonBreakdownDto(Guid? LostReasonId, string Label, int Count, decimal TotalAmount);
