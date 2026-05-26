// <copyright file="DocumentManagementControllerDiscovery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Controllers;

namespace NetMetric.CRM.DocumentManagement.ArchitectureTests;

internal static class DocumentManagementControllerDiscovery
{
    public static IReadOnlyList<Type> GetControllerTypes()
    {
        var apiAssembly = typeof(DocumentsController).Assembly;

        return apiAssembly.GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false }
                && typeof(ControllerBase).IsAssignableFrom(type)
                && type.Namespace == "NetMetric.CRM.API.Controllers"
                && (type.Name.StartsWith("Documents", StringComparison.Ordinal) || type.Name.StartsWith("DocumentVersions", StringComparison.Ordinal) || type.Name.StartsWith("DocumentApprovals", StringComparison.Ordinal) || type.Name.StartsWith("DocumentPreview", StringComparison.Ordinal)))
            .ToList();
    }
}
