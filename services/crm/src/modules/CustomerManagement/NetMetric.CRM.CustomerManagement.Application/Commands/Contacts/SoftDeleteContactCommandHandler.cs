// <copyright file="SoftDeleteContactCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Contacts;

public sealed class SoftDeleteContactCommandHandler(IContactAdministrationService administrationService)
    : IRequestHandler<SoftDeleteContactCommand, Unit>
{
    public async Task<Unit> Handle(SoftDeleteContactCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SoftDeleteAsync(request.ContactId, cancellationToken);
        return Unit.Value;
    }
}
