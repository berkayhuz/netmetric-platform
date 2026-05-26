// <copyright file="AddCustomerAddressCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;

public sealed record AddCustomerAddressCommand(
    Guid CustomerId,
    AddressType AddressType,
    string Line1,
    string? Line2,
    string? District,
    string? City,
    string? State,
    string? Country,
    string? ZipCode,
    bool IsDefault) : IRequest<AddressDto>;
