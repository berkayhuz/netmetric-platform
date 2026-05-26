// <copyright file="SoftDeleteCompanyCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Companies;

public sealed class SoftDeleteCompanyCommandHandler(ICompanyAdministrationService administrationService)
    : IRequestHandler<SoftDeleteCompanyCommand, Unit>
{
    public async Task<Unit> Handle(SoftDeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        await administrationService.SoftDeleteAsync(request.CompanyId, cancellationToken);
        return Unit.Value;
    }
}
