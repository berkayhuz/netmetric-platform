// <copyright file="QuoteStatusHistoryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record QuoteStatusHistoryDto(Guid Id, QuoteStatusType? OldStatus, QuoteStatusType NewStatus, DateTime ChangedAt, Guid? ChangedByUserId, string? Note);
