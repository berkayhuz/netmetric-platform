// <copyright file="DeleteCustomFieldOptionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomFields;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.DeleteCustomFieldOption;

public sealed class DeleteCustomFieldOptionCommandHandler(ICustomerManagementDbContext dbContext)
    : IRequestHandler<DeleteCustomFieldOptionCommand, Unit>
{
    private readonly ICustomerManagementDbContext _dbContext = dbContext;

    public async Task<Unit> Handle(DeleteCustomFieldOptionCommand request, CancellationToken cancellationToken)
    {
        var option = await _dbContext.Set<CustomFieldOption>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.OptionId, cancellationToken)
            ?? throw new NotFoundAppException("Custom field option not found.");

        _dbContext.Set<CustomFieldOption>().Remove(option);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
