// <copyright file="CreateContractRecordCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.ContractLifecycle.Application.Abstractions.Persistence;
using NetMetric.CRM.ContractLifecycle.Contracts.DTOs;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Contracts;

namespace NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Commands.CreateContractRecord;

public sealed class CreateContractRecordCommandHandler(IContractLifecycleDbContext dbContext)
    : IRequestHandler<CreateContractRecordCommand, ContractLifecycleSummaryDto>
{
    public async Task<ContractLifecycleSummaryDto> Handle(CreateContractRecordCommand request, CancellationToken cancellationToken)
    {
        var entity = new ContractRecord(request.Code, request.Name, request.Description);
        await dbContext.Contracts.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ContractLifecycleSummaryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }
}
