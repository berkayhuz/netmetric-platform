// <copyright file="DealArchitectureTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.API.Controllers.Deals;

namespace NetMetric.CRM.DealManagement.ArchitectureTests;

internal static class DealManagementControllerDiscovery
{
    public static IReadOnlyList<Type> GetControllerTypes()
    {
        var assembly = typeof(DealsController).Assembly;
        return assembly.GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type)
                           && !type.IsAbstract
                           && type.Namespace == typeof(DealsController).Namespace
                           && (type == typeof(DealsController) || type == typeof(DealWinLossController)))
            .ToList();
    }
}

public sealed class ControllerSecurityContractTests
{
    [Fact]
    public void All_DealManagement_Controllers_Should_Be_ApiControllers_With_Authorize_And_Route()
    {
        var controllerTypes = DealManagementControllerDiscovery.GetControllerTypes();
        controllerTypes.Should().NotBeEmpty();

        foreach (var controllerType in controllerTypes)
        {
            controllerType.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            controllerType.GetCustomAttribute<RouteAttribute>().Should().NotBeNull();

            var hasControllerAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>() is not null;
            var actionMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(x => !x.IsSpecialName).ToList();
            actionMethods.Should().NotBeEmpty();
            var allActionsAreSecured = actionMethods.All(x => x.GetCustomAttribute<AuthorizeAttribute>() is not null || hasControllerAuthorize);
            (hasControllerAuthorize || allActionsAreSecured).Should().BeTrue();
        }
    }

    [Fact]
    public void All_DealManagement_Controllers_Should_Use_Declared_Policies()
    {
        var declaredPolicies = new HashSet<string>(StringComparer.Ordinal)
        {
            AuthorizationPolicies.DealsRead,
            AuthorizationPolicies.DealsManage,
            AuthorizationPolicies.WinLossRead,
            AuthorizationPolicies.WinLossManage,
            Permissions.DealsRead,
            Permissions.DealsManage,
            Permissions.WinLossRead,
            Permissions.WinLossManage,
            "AuthorizationPolicies.DealsRead",
            "AuthorizationPolicies.DealsManage",
            "AuthorizationPolicies.WinLossRead",
            "AuthorizationPolicies.WinLossManage",
            "Permissions.DealsRead",
            "Permissions.DealsManage",
            "Permissions.WinLossRead",
            "Permissions.WinLossManage"
        };

        var policies = DealManagementControllerDiscovery.GetControllerTypes()
            .SelectMany(type => type.GetCustomAttributes<AuthorizeAttribute>())
            .Concat(DealManagementControllerDiscovery.GetControllerTypes().SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)).SelectMany(method => method.GetCustomAttributes<AuthorizeAttribute>()))
            .Select(x => x.Policy)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        policies.Should().NotBeEmpty();
        policies.Should().OnlyContain(x => declaredPolicies.Contains(x));
    }
}
