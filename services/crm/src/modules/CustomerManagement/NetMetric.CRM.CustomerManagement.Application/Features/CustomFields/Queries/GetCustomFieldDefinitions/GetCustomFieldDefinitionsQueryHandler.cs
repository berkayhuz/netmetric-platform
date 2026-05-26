// <copyright file="GetCustomFieldDefinitionsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Queries.GetCustomFieldDefinitions;

public sealed class GetCustomFieldDefinitionsQueryHandler(ICustomerManagementDbContext dbContext) : IRequestHandler<GetCustomFieldDefinitionsQuery, IReadOnlyList<CustomFieldDefinitionDto>>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<IReadOnlyList<CustomFieldDefinitionDto>> Handle(GetCustomFieldDefinitionsQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<CustomFieldDefinition>()
            .AsNoTracking()
            .Include(x => x.Options)
            .Where(x => !x.IsDeleted && x.EntityName == request.EntityName)
            .OrderBy(x => x.OrderNo)
            .Select(x => new CustomFieldDefinitionDto
            {
                Id = x.Id,
                Name = x.Name,
                Label = x.Label,
                EntityName = x.EntityName,
                DataType = x.DataType.ToString(),
                IsRequired = x.IsRequired,
                IsUnique = x.IsUnique,
                IsSystem = x.IsSystem,
                DefaultValue = x.DefaultValue,
                Placeholder = x.Placeholder,
                HelpText = x.HelpText,
                OrderNo = x.OrderNo,
                Options = x.Options
                    .OrderBy(o => o.OrderNo)
                    .Select(o => new CustomFieldOptionDto
                    {
                        Id = o.Id,
                        Label = o.Label,
                        Value = o.Value,
                        OrderNo = o.OrderNo
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
