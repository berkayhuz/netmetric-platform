// <copyright file="UpsertGuidedSellingPlaybookCommandHandler.cs" company="NetMetric">
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

public sealed class UpsertGuidedSellingPlaybookCommandHandler(IQuoteManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpsertGuidedSellingPlaybookCommand, GuidedSellingPlaybookDto>
{
    public async Task<GuidedSellingPlaybookDto> Handle(UpsertGuidedSellingPlaybookCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = request.GuidedSellingPlaybookId.HasValue
            ? await dbContext.GuidedSellingPlaybooks.FirstOrDefaultAsync(x => x.Id == request.GuidedSellingPlaybookId.Value, cancellationToken)
            : null;

        if (request.GuidedSellingPlaybookId.HasValue && entity is null)
            throw new KeyNotFoundException("Guided selling playbook was not found.");

        entity ??= new GuidedSellingPlaybook
        {
            TenantId = currentUserService.TenantId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName
        };

        if (request.GuidedSellingPlaybookId.HasValue)
        {
            var expected = QuoteHandlerHelpers.ParseRowVersion(request.RowVersion);
            if (expected.Length > 0)
                entity.RowVersion = expected;
        }

        entity.Name = request.Name.Trim();
        entity.Segment = string.IsNullOrWhiteSpace(request.Segment) ? null : request.Segment.Trim();
        entity.Industry = string.IsNullOrWhiteSpace(request.Industry) ? null : request.Industry.Trim();
        entity.MinimumBudget = request.MinimumBudget;
        entity.MaximumBudget = request.MaximumBudget;
        entity.RequiredCapabilities = string.IsNullOrWhiteSpace(request.RequiredCapabilities) ? null : request.RequiredCapabilities.Trim();
        entity.RecommendedBundleCodes = string.Join(',', request.RecommendedBundleCodes.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().ToUpperInvariant()).Distinct(StringComparer.OrdinalIgnoreCase));
        entity.QualificationJson = string.IsNullOrWhiteSpace(request.QualificationJson) ? null : request.QualificationJson.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        if (!request.GuidedSellingPlaybookId.HasValue)
            await dbContext.GuidedSellingPlaybooks.AddAsync(entity, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
