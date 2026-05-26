// <copyright file="MarkCustomerAsVipCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Customers;

public sealed class MarkCustomerAsVipCommandHandler(ICustomerAdministrationService administrationService)
    : IRequestHandler<MarkCustomerAsVipCommand, Unit>
{
    public async Task<Unit> Handle(MarkCustomerAsVipCommand request, CancellationToken cancellationToken)
    {
        await administrationService.MarkVipAsync(request.CustomerId, request.IsVip, cancellationToken);
        return Unit.Value;
    }
}
