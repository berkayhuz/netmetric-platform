// <copyright file="SoftDeleteAddressCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;

public sealed class SoftDeleteAddressCommandHandler(IAddressAdministrationService administrationService)
    : IRequestHandler<SoftDeleteAddressCommand, Unit>
{
    public async Task<Unit> Handle(SoftDeleteAddressCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SoftDeleteAsync(request.AddressId, cancellationToken);
        return Unit.Value;
    }
}
