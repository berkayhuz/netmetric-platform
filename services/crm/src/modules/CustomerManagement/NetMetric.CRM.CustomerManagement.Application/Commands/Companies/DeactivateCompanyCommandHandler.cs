// <copyright file="DeactivateCompanyCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Companies;

public sealed class DeactivateCompanyCommandHandler(ICompanyAdministrationService administrationService)
    : IRequestHandler<DeactivateCompanyCommand, Unit>
{
    public async Task<Unit> Handle(DeactivateCompanyCommand request, CancellationToken cancellationToken)
    {
        await administrationService.DeactivateAsync(request.CompanyId, cancellationToken);
        return Unit.Value;
    }
}
