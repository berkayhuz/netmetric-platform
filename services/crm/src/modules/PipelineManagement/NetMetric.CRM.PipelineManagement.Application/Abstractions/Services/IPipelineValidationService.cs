// <copyright file="IPipelineValidationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;

namespace NetMetric.CRM.PipelineManagement.Application.Abstractions.Services;

public interface IPipelineValidationService
{
    Task<(bool IsValid, List<string> Errors)> ValidateStageTransitionAsync(
        Opportunity opportunity,
        Guid targetStageId,
        CancellationToken cancellationToken = default);
}
