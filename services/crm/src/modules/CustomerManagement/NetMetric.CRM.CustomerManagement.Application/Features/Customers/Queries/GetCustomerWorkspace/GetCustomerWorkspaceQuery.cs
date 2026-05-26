// <copyright file="GetCustomerWorkspaceQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Customers;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Customers.Queries.GetCustomerWorkspace;

public sealed record GetCustomerWorkspaceQuery(Guid CustomerId) : IRequest<CustomerWorkspaceDto>;
