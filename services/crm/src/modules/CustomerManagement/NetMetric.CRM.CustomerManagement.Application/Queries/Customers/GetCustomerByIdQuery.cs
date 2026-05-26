// <copyright file="GetCustomerByIdQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Queries.Customers;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : IRequest<CustomerDetailDto?>;
