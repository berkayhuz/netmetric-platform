// <copyright file="GetCustomer360WorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Queries.GetCustomer360Workspace;

public sealed record GetCustomer360WorkspaceQuery(Guid CustomerId) : IRequest<Customer360WorkspaceDto>;
