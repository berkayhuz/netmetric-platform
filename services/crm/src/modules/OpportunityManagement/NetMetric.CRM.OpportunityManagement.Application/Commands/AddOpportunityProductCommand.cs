// <copyright file="AddOpportunityProductCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed record AddOpportunityProductCommand(Guid OpportunityId, Guid ProductId, int Quantity, decimal UnitPrice, decimal DiscountRate, decimal VatRate) : IRequest<OpportunityProductDto>;
