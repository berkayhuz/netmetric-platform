// <copyright file="CreateSalesOrderCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.FinanceOperations.Contracts.DTOs;

namespace NetMetric.CRM.FinanceOperations.Application.Features.Orders.Commands.CreateSalesOrder;

public sealed record CreateSalesOrderCommand(
    string Code,
    string Name,
    string? Description) : IRequest<FinanceOperationsSummaryDto>;
