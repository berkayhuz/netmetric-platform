// <copyright file="CreateCustomFieldOptionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.CustomFields;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.CreateCustomFieldOption;

public sealed class CreateCustomFieldOptionCommandHandler(ICustomerManagementDbContext dbContext)
    : IRequestHandler<CreateCustomFieldOptionCommand, CustomFieldOptionDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<CustomFieldOptionDto> Handle(CreateCustomFieldOptionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _dbContext.Set<CustomFieldDefinition>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.DefinitionId, cancellationToken)
            ?? throw new NotFoundAppException("Custom field definition not found.");

        var exists = await _dbContext.Set<CustomFieldOption>()
            .AnyAsync(x => !x.IsDeleted && x.CustomFieldDefinitionId == request.DefinitionId && x.Value == request.Value.Trim(), cancellationToken);

        if (exists)
            throw new ConflictAppException("A custom field option with the same value already exists.");

        var option = new CustomFieldOption
        {
            CustomFieldDefinitionId = definition.Id,
            Label = request.Label.Trim(),
            Value = request.Value.Trim(),
            OrderNo = request.OrderNo
        };

        await _dbContext.Set<CustomFieldOption>().AddAsync(option, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CustomFieldOptionDto
        {
            Id = option.Id,
            Label = option.Label,
            Value = option.Value,
            OrderNo = option.OrderNo
        };
    }
}
