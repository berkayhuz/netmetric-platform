// <copyright file="UpdateCompanyCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Contracts.DTOs;

namespace NetMetric.CRM.CustomerManagement.Application.Commands.Companies;

public sealed class UpdateCompanyCommandHandler(ICompanyAdministrationService administrationService)
    : IRequestHandler<UpdateCompanyCommand, CompanyDetailDto>
{
    public Task<CompanyDetailDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
        => administrationService.UpdateAsync(request, cancellationToken);
}
