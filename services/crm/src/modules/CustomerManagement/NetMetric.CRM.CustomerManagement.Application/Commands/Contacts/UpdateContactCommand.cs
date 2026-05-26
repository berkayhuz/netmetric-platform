// <copyright file="UpdateContactCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;

public sealed record UpdateContactCommand(
    Guid ContactId,
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? MobilePhone,
    string? WorkPhone,
    string? PersonalPhone,
    DateTime? BirthDate,
    GenderType Gender,
    string? Department,
    string? JobTitle,
    string? Description,
    string? Notes,
    Guid? OwnerUserId,
    Guid? CompanyId,
    Guid? CustomerId,
    bool IsPrimaryContact,
    string? RowVersion) : IRequest<ContactDetailDto>;
