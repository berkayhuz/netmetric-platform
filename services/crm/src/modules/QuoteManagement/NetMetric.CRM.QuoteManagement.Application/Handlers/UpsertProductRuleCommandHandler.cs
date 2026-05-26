// <copyright file="UpsertProductRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.QuoteManagement.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class UpsertProductRuleCommandHandler(IQuoteManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpsertProductRuleCommand, ProductRuleDto>
{
    public async Task<ProductRuleDto> Handle(UpsertProductRuleCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = request.ProductRuleId.HasValue
            ? await dbContext.ProductRules.FirstOrDefaultAsync(x => x.Id == request.ProductRuleId.Value, cancellationToken)
            : null;

        if (request.ProductRuleId.HasValue && entity is null)
            throw new KeyNotFoundException("Product rule was not found.");

        entity ??= new ProductRule
        {
            TenantId = currentUserService.TenantId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName
        };

        if (request.ProductRuleId.HasValue)
        {
            var expected = QuoteHandlerHelpers.ParseRowVersion(request.RowVersion);
            if (expected.Length > 0)
                entity.RowVersion = expected;
        }

        entity.Name = request.Name.Trim();
        entity.RuleType = request.RuleType.Trim();
        entity.TriggerProductId = request.TriggerProductId;
        entity.TargetProductId = request.TargetProductId;
        entity.MinimumQuantity = request.MinimumQuantity;
        entity.MaximumDiscountRate = request.MaximumDiscountRate;
        entity.Severity = request.Severity.Trim();
        entity.Message = request.Message.Trim();
        entity.CriteriaJson = string.IsNullOrWhiteSpace(request.CriteriaJson) ? null : request.CriteriaJson.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        if (!request.ProductRuleId.HasValue)
            await dbContext.ProductRules.AddAsync(entity, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
