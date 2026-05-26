// <copyright file="CreateCustomFieldDefinitionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.CustomFields;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.CreateCustomFieldDefinition;

public sealed class CreateCustomFieldDefinitionCommandHandler(ICustomerManagementDbContext dbContext)
    : IRequestHandler<CreateCustomFieldDefinitionCommand, CustomFieldDefinitionDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<CustomFieldDefinitionDto> Handle(CreateCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();
        var normalizedEntity = request.EntityName.Trim().ToLowerInvariant();

        var exists = await _dbContext.Set<CustomFieldDefinition>()
            .AnyAsync(x => !x.IsDeleted && x.EntityName == normalizedEntity && x.Name == normalizedName, cancellationToken);

        if (exists)
            throw new ConflictAppException("A custom field definition with the same name already exists for this entity.");

        var definition = new CustomFieldDefinition
        {
            Name = normalizedName,
            Label = request.Label.Trim(),
            EntityName = normalizedEntity,
            DataType = request.DataType,
            IsRequired = request.IsRequired,
            IsUnique = request.IsUnique,
            IsSystem = request.IsSystem,
            DefaultValue = TrimToNull(request.DefaultValue),
            Placeholder = TrimToNull(request.Placeholder),
            HelpText = TrimToNull(request.HelpText),
            OrderNo = request.OrderNo
        };

        await _dbContext.Set<CustomFieldDefinition>().AddAsync(definition, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CustomFieldDefinitionDto
        {
            Id = definition.Id,
            Name = definition.Name,
            Label = definition.Label,
            EntityName = definition.EntityName,
            DataType = definition.DataType.ToString(),
            IsRequired = definition.IsRequired,
            IsUnique = definition.IsUnique,
            IsSystem = definition.IsSystem,
            DefaultValue = definition.DefaultValue,
            Placeholder = definition.Placeholder,
            HelpText = definition.HelpText,
            OrderNo = definition.OrderNo,
            Options = []
        };
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
