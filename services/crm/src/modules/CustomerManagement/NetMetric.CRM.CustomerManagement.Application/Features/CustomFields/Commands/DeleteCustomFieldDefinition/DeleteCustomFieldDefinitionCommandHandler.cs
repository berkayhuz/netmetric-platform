// <copyright file="DeleteCustomFieldDefinitionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomFields;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.DeleteCustomFieldDefinition;

public sealed class DeleteCustomFieldDefinitionCommandHandler(ICustomerManagementDbContext dbContext)
    : IRequestHandler<DeleteCustomFieldDefinitionCommand, Unit>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Unit> Handle(DeleteCustomFieldDefinitionCommand request, CancellationToken cancellationToken)
    {
        var definition = await _dbContext.Set<CustomFieldDefinition>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.DefinitionId, cancellationToken)
            ?? throw new NotFoundAppException("Custom field definition not found.");

        if (definition.IsSystem)
            throw new ConflictAppException("System custom field definitions cannot be deleted.");

        var values = await _dbContext.Set<CustomFieldValue>()
            .Where(x => !x.IsDeleted && x.DefinitionId == request.DefinitionId)
            .ToListAsync(cancellationToken);

        var options = await _dbContext.Set<CustomFieldOption>()
            .Where(x => !x.IsDeleted && x.CustomFieldDefinitionId == request.DefinitionId)
            .ToListAsync(cancellationToken);

        _dbContext.Set<CustomFieldValue>().RemoveRange(values);
        _dbContext.Set<CustomFieldOption>().RemoveRange(options);
        _dbContext.Set<CustomFieldDefinition>().Remove(definition);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
