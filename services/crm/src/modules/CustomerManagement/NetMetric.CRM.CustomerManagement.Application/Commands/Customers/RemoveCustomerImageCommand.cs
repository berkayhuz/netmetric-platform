// <copyright file="RemoveCustomerImageCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Customers;

public sealed record RemoveCustomerImageCommand(Guid CustomerId) : IRequest<CustomerDetailDto>;
