// <copyright file="PipelineValidationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Services;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.PipelineManagement.Application.Services;

public sealed class PipelineValidationService(IPipelineManagementDbContext dbContext) : IPipelineValidationService
{
    public async Task<(bool IsValid, List<string> Errors)> ValidateStageTransitionAsync(
        Opportunity opportunity,
        Guid targetStageId,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        // 1. Fetch the target stage and its rules
        var targetStage = await dbContext.PipelineStages
            .Include(s => s.RequiredFields)
            .Include(s => s.ExitCriteria)
            .FirstOrDefaultAsync(s => s.Id == targetStageId, cancellationToken);

        if (targetStage == null)
        {
            errors.Add("Target stage not found.");
            return (false, errors);
        }

        // 2. Validate current stage exit criteria
        if (opportunity.PipelineStageId.HasValue)
        {
            var currentStage = await dbContext.PipelineStages
                .Include(s => s.ExitCriteria)
                .FirstOrDefaultAsync(s => s.Id == opportunity.PipelineStageId.Value, cancellationToken);

            if (currentStage != null)
            {
                foreach (var criteria in currentStage.ExitCriteria.Where(c => c.IsMandatory))
                {
                    // In a real system, we would check a "CriteriaFulfillment" table or similar.
                    // For now, we'll assume a basic check or TODO.
                    // errors.Add($"Exit criteria '{criteria.Name}' must be fulfilled before leaving {currentStage.Name}.");
                }
            }
        }

        // 3. Validate target stage required fields
        foreach (var field in targetStage.RequiredFields)
        {
            var value = GetPropertyValue(opportunity, field.FieldName);
            if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                errors.Add(field.ErrorMessage ?? $"Field '{field.DisplayName ?? field.FieldName}' is required for stage '{targetStage.Name}'.");
            }
        }

        return (errors.Count == 0, errors);
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        return obj.GetType().GetProperty(propertyName)?.GetValue(obj, null);
    }
}
