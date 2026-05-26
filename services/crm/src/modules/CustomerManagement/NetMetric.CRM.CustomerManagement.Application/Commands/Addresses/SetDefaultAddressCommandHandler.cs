// <copyright file="SetDefaultAddressCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;

public sealed class SetDefaultAddressCommandHandler(IAddressAdministrationService administrationService)
    : IRequestHandler<SetDefaultAddressCommand, Unit>
{
    public async Task<Unit> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SetDefaultAsync(request.AddressId, cancellationToken);
        return Unit.Value;
    }
}
