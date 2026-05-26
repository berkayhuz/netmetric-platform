// <copyright file="GetDealWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DealManagement.Contracts.DTOs;

namespace NetMetric.CRM.DealManagement.Application.Queries.Deals;

public sealed record GetDealWorkspaceQuery(Guid DealId) : IRequest<DealWorkspaceDto?>;
