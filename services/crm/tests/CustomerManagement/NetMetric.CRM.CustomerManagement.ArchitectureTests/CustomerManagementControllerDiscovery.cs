// <copyright file="CustomerManagementControllerDiscovery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Controllers.CustomerManagement;

namespace NetMetric.CRM.CustomerManagement.ArchitectureTests;

internal static class CustomerManagementControllerDiscovery
{
    private static readonly string[] RoutePrefixes =
    [
        "api/customers",
        "api/customer-management",
        "api/companies",
        "api/contacts",
        "api/customer-documents",
        "api/customer-tags",
        "api/customer-notes",
        "api/customer-timeline",
        "api/customer-addresses",
        "api/customer-custom-fields",
        "api/customer-import",
        "api/customer-export"
    ];

    private static readonly string[] NamespaceMarkers =
    [
        "NetMetric.CRM.API.Controllers.Customers",
        "NetMetric.CRM.API.Controllers.CustomerManagement",
        "NetMetric.CRM.API.Controllers.Companies",
        "NetMetric.CRM.API.Controllers.Contacts"
    ];

    private static readonly string[] ControllerNameMarkers =
    [
        "Customer",
        "Customers",
        "Company",
        "Companies",
        "Contact",
        "Contacts"
    ];

    public static IReadOnlyList<Type> GetControllerTypes()
    {
        var apiAssembly = typeof(CustomersController).Assembly;

        return GetLoadableTypes(apiAssembly)
            .Where(IsCustomerManagementController)
            .Distinct()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToList();
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }

    private static bool IsCustomerManagementController(Type type)
    {
        if (!type.IsClass || type.IsAbstract)
            return false;

        if (!typeof(ControllerBase).IsAssignableFrom(type))
            return false;

        if (!type.Name.EndsWith("Controller", StringComparison.Ordinal))
            return false;

        var routeTemplates = type.GetCustomAttributes<RouteAttribute>()
            .Select(attribute => attribute.Template)
            .Where(template => !string.IsNullOrWhiteSpace(template))
            .Cast<string>();

        if (routeTemplates.Any(IsCustomerManagementRoute))
            return true;

        if (IsCustomerManagementNamespace(type.Namespace))
            return true;

        if (IsCustomerManagementControllerName(type.Name))
            return true;

        return false;
    }

    private static bool IsCustomerManagementRoute(string? routeTemplate)
    {
        if (string.IsNullOrWhiteSpace(routeTemplate))
            return false;

        return RoutePrefixes.Any(prefix =>
            routeTemplate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsCustomerManagementNamespace(string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
            return false;

        return NamespaceMarkers.Any(marker =>
            @namespace.StartsWith(marker, StringComparison.Ordinal));
    }

    private static bool IsCustomerManagementControllerName(string controllerTypeName)
    {
        var controllerName = controllerTypeName.Replace("Controller", string.Empty, StringComparison.Ordinal);

        return ControllerNameMarkers.Any(marker =>
            controllerName.Contains(marker, StringComparison.Ordinal));
    }
}
