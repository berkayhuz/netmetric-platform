// <copyright file="UpdateContactCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;

public sealed class UpdateContactCommandHandler(IContactAdministrationService administrationService)
    : IRequestHandler<UpdateContactCommand, ContactDetailDto>
{
    public Task<ContactDetailDto> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
        => administrationService.UpdateAsync(request, cancellationToken);
}
