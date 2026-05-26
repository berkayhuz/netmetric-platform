// <copyright file="SoftDeleteCustomerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Customers;

public sealed class SoftDeleteCustomerCommandHandler(ICustomerAdministrationService administrationService)
    : IRequestHandler<SoftDeleteCustomerCommand, Unit>
{
    public async Task<Unit> Handle(SoftDeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SoftDeleteAsync(request.CustomerId, cancellationToken);
        return Unit.Value;
    }
}
