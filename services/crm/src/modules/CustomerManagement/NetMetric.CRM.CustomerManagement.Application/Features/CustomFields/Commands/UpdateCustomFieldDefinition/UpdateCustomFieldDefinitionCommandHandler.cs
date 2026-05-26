// <copyright file="UpdateCustomFieldDefinitionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.CustomFields;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.UpdateCustomFieldDefinition;

public sealed class UpdateCustomFieldDefinitionCommandHandler(ICustomerManagementDbContext dbContext)
    : IRequestHandler<UpdateCustomFieldDefinitionCommand, CustomFieldDefinitionDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<CustomFieldDefinitionDto> Handle(UpdateCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _dbContext.Set<CustomFieldDefinition>()
            .Include(x => x.Options)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.DefinitionId, cancellationToken)
            ?? throw new NotFoundAppException("Custom field definition not found.");

        if (definition.IsSystem)
            throw new ConflictAppException("System custom field definitions cannot be modified.");

        definition.Label = request.Label.Trim();
        definition.DataType = request.DataType;
        definition.IsRequired = request.IsRequired;
        definition.IsUnique = request.IsUnique;
        definition.DefaultValue = TrimToNull(request.DefaultValue);
        definition.Placeholder = TrimToNull(request.Placeholder);
        definition.HelpText = TrimToNull(request.HelpText);
        definition.OrderNo = request.OrderNo;

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
            Options = definition.Options.OrderBy(x => x.OrderNo).Select(x => new CustomFieldOptionDto
            {
                Id = x.Id,
                Label = x.Label,
                Value = x.Value,
                OrderNo = x.OrderNo
            }).ToList()
        };
    }

    private static string? TrimToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
