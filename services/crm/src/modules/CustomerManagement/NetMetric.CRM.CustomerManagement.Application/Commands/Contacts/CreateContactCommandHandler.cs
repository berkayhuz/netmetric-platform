// <copyright file="CreateContactCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;

public sealed class CreateContactCommandHandler(IContactAdministrationService administrationService)
    : IRequestHandler<CreateContactCommand, ContactDetailDto>
{
    public Task<ContactDetailDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        => administrationService.CreateAsync(request, cancellationToken);
}
