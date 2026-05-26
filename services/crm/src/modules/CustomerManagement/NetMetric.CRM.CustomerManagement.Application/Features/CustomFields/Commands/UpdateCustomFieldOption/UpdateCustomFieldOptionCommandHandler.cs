// <copyright file="UpdateCustomFieldOptionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.DTOs.CustomFields;
using NetMetric.CRM.CustomFields;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.UpdateCustomFieldOption;

public sealed class UpdateCustomFieldOptionCommandHandler(ICustomerManagementDbContext dbContext)
    : IRequestHandler<UpdateCustomFieldOptionCommand, CustomFieldOptionDto>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<CustomFieldOptionDto> Handle(UpdateCustomFieldOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await _dbContext.Set<CustomFieldOption>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.OptionId, cancellationToken)
            ?? throw new NotFoundAppException("Custom field option not found.");

        var exists = await _dbContext.Set<CustomFieldOption>()
            .AnyAsync(x => !x.IsDeleted && x.CustomFieldDefinitionId == option.CustomFieldDefinitionId && x.Id != option.Id && x.Value == request.Value.Trim(), cancellationToken);

        if (exists)
            throw new ConflictAppException("A custom field option with the same value already exists.");

        option.Label = request.Label.Trim();
        option.Value = request.Value.Trim();
        option.OrderNo = request.OrderNo;

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
