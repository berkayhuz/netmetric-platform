// <copyright file="ValidateQuoteConfigurationQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class ValidateQuoteConfigurationQueryHandler(IQuoteManagementDbContext dbContext) : IRequestHandler<ValidateQuoteConfigurationQuery, CpqValidationResultDto>
{
    public async Task<CpqValidationResultDto> Handle(ValidateQuoteConfigurationQuery request, CancellationToken cancellationToken)
    {
        var quote = await dbContext.Quotes.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.QuoteId, cancellationToken);
        if (quote is null)
            throw new KeyNotFoundException("Quote was not found.");

        var rules = await dbContext.ProductRules.AsNoTracking().Where(x => x.IsActive).ToListAsync(cancellationToken);
        var violations = new List<string>();

        foreach (var rule in rules)
        {
            var triggerLine = rule.TriggerProductId.HasValue
                ? quote.Items.FirstOrDefault(x => x.ProductId == rule.TriggerProductId.Value)
                : null;

            if (rule.RuleType.Equals("RequireProduct", StringComparison.OrdinalIgnoreCase) &&
                triggerLine is not null &&
                rule.TargetProductId.HasValue &&
                quote.Items.All(x => x.ProductId != rule.TargetProductId.Value))
            {
                violations.Add(rule.Message);
            }

            if (rule.RuleType.Equals("ExcludeProduct", StringComparison.OrdinalIgnoreCase) &&
                triggerLine is not null &&
                rule.TargetProductId.HasValue &&
                quote.Items.Any(x => x.ProductId == rule.TargetProductId.Value))
            {
                violations.Add(rule.Message);
            }

            if (rule.RuleType.Equals("MinimumQuantity", StringComparison.OrdinalIgnoreCase) &&
                triggerLine is not null &&
                rule.MinimumQuantity.HasValue &&
                triggerLine.Quantity < rule.MinimumQuantity.Value)
            {
                violations.Add(rule.Message);
            }

            if (rule.RuleType.Equals("DiscountCap", StringComparison.OrdinalIgnoreCase) &&
                triggerLine is not null &&
                rule.MaximumDiscountRate.HasValue &&
                triggerLine.DiscountRate > rule.MaximumDiscountRate.Value)
            {
                violations.Add(rule.Message);
            }
        }

        return new CpqValidationResultDto(violations.Count == 0, violations);
    }
}
