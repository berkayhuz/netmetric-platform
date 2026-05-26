// <copyright file="GetSalesOrderByIdQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.FinanceOperations.Contracts.DTOs;

namespace NetMetric.CRM.FinanceOperations.Application.Features.Orders.Queries.GetSalesOrderById;

public sealed record GetSalesOrderByIdQuery(Guid Id) : IRequest<FinanceOperationsSummaryDto?>;
