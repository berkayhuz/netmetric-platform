// <copyright file="UpsertCustomFieldValueCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.CustomFields;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.UpsertCustomFieldValue;

public sealed class UpsertCustomFieldValueCommandHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<UpsertCustomFieldValueCommand, CustomFieldValueDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<CustomFieldValueDto> Handle(UpsertCustomFieldValueCommand request, CancellationToken cancellationToken)
    {
        var definition = await _dbContext.Set<CustomFieldDefinition>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.DefinitionId, cancellationToken)
            ?? throw new NotFoundAppException("Custom field definition not found.");

        if (!string.Equals(definition.EntityName, request.EntityName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ValidationAppException("Custom field definition entity mismatch.", new Dictionary<string, string[]>
            {
                [nameof(request.EntityName)] = ["Definition belongs to another entity type."]
            });
        }

        var value = await _dbContext.Set<CustomFieldValue>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.DefinitionId == request.DefinitionId && x.EntityName == request.EntityName && x.EntityId == request.EntityId, cancellationToken);

        if (value is null)
        {
            value = new CustomFieldValue
            {
                DefinitionId = request.DefinitionId,
                EntityName = request.EntityName,
                EntityId = request.EntityId,
                Value = request.Value
            };

            await _dbContext.Set<CustomFieldValue>().AddAsync(value, cancellationToken);
        }
        else
        {
            value.Value = request.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CustomFieldValueDto
        {
            DefinitionId = definition.Id,
            Name = definition.Name,
            Label = definition.Label,
            Value = value.Value,
            DataType = definition.DataType.ToString()
        };
    }
}
