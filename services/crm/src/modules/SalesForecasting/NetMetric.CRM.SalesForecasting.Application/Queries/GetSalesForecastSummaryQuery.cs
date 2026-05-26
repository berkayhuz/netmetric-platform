// <copyright file="GetSalesForecastSummaryQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Queries;

public sealed record GetSalesForecastSummaryQuery(DateOnly PeriodStart, DateOnly PeriodEnd, Guid? OwnerUserId) : IRequest<SalesForecastSummaryDto>;
