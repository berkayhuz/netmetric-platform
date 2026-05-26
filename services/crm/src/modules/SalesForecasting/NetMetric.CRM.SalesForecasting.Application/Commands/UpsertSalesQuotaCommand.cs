// <copyright file="UpsertSalesQuotaCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Commands;

public sealed record UpsertSalesQuotaCommand(DateOnly PeriodStart, DateOnly PeriodEnd, Guid? OwnerUserId, decimal Amount, string CurrencyCode, string? Notes, string? RowVersion) : IRequest<SalesQuotaDto>;
