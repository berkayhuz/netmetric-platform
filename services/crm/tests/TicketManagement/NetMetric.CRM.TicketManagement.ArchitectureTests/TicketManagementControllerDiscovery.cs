// <copyright file="TicketManagementControllerDiscovery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Controllers.Tickets;

namespace NetMetric.CRM.TicketManagement.ArchitectureTests;

internal static class TicketManagementControllerDiscovery
{
    private static readonly HashSet<string> ControllerNames = new(StringComparer.Ordinal)
    {
        nameof(TicketsController)
    };

    public static IReadOnlyList<Type> GetControllerTypes()
    {
        var apiAssembly = typeof(TicketsController).Assembly;

        return apiAssembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(ControllerBase).IsAssignableFrom(type) &&
                type.Namespace == "NetMetric.CRM.API.Controllers.Tickets" &&
                ControllerNames.Contains(type.Name))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToList();
    }
}
