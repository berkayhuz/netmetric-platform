// <copyright file="SetPrimaryContactCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;

public sealed class SetPrimaryContactCommandHandler(IContactAdministrationService administrationService)
    : IRequestHandler<SetPrimaryContactCommand, Unit>
{
    public async Task<Unit> Handle(SetPrimaryContactCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SetPrimaryAsync(request.ContactId, cancellationToken);
        return Unit.Value;
    }
}
