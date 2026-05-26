// <copyright file="SalesQuotaDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SalesForecasting.Contracts.DTOs;

public sealed record SalesQuotaDto(
    Guid Id,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    Guid? OwnerUserId,
    decimal Amount,
    string CurrencyCode,
    string? Notes,
    string RowVersion);
