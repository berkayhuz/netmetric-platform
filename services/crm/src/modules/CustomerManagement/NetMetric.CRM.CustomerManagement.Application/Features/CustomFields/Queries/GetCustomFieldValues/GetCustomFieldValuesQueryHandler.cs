// <copyright file="GetCustomFieldValuesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Queries.GetCustomFieldValues;

public sealed class GetCustomFieldValuesQueryHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<GetCustomFieldValuesQuery, IReadOnlyList<CustomFieldValueDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<CustomFieldValueDto>> Handle(GetCustomFieldValuesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<CustomFieldValue>()
            .AsNoTracking()
            .Include(x => x.Definition)
            .Where(x => !x.IsDeleted && x.EntityName == request.EntityName && x.EntityId == request.EntityId)
            .Where(x => x.Definition != null)
            .OrderBy(x => x.Definition!.OrderNo)
            .Select(x => new CustomFieldValueDto
            {
                DefinitionId = x.DefinitionId,
                Name = x.Definition!.Name,
                Label = x.Definition.Label,
                Value = x.Value,
                DataType = x.Definition.DataType.ToString()
            })
            .ToListAsync(cancellationToken);
    }
}
