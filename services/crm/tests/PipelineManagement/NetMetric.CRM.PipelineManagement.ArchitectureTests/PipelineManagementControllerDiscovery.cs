// <copyright file="PipelineManagementControllerDiscovery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Controllers.Pipelines;

namespace NetMetric.CRM.PipelineManagement.ArchitectureTests;

internal static class PipelineManagementControllerDiscovery
{
    public static IReadOnlyList<Type> GetControllerTypes()
    {
        var assembly = typeof(LostReasonsController).Assembly;

        return assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(ControllerBase).IsAssignableFrom(type) &&
                type.Name.EndsWith("Controller", StringComparison.Ordinal) &&
                type.Namespace is not null &&
                type.Namespace.StartsWith("NetMetric.CRM.API.Controllers.Pipelines", StringComparison.Ordinal) &&
                (type.Name.Contains("LostReason", StringComparison.Ordinal)
                 || type.Name.Contains("Pipeline", StringComparison.Ordinal)
                 || type.Name.Contains("LeadConversion", StringComparison.Ordinal)))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToList();
    }
}
