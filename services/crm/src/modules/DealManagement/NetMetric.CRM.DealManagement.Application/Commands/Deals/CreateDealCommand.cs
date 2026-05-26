// <copyright file="CreateDealCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DealManagement.Contracts.DTOs;

namespace NetMetric.CRM.DealManagement.Application.Commands.Deals;

public sealed record CreateDealCommand(string DealCode, string Name, decimal TotalAmount, DateTime ClosedDate, Guid? OpportunityId, Guid? CompanyId, Guid? OwnerUserId, string? Notes) : IRequest<DealDetailDto>;
