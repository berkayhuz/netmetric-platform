// <copyright file="ControllerSecurityContractTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace NetMetric.CRM.KnowledgeBaseManagement.ArchitectureTests;

public sealed class ControllerSecurityContractTests
{
    private static readonly HashSet<string> AllowedPolicies =
    [
        "knowledge-base.categories.read",
        "knowledge-base.categories.manage",
        "knowledge-base.articles.read",
        "knowledge-base.articles.manage",
        "knowledge-base.articles.publish"
    ];

    [Fact]
    public void All_KnowledgeBase_Controllers_Should_Be_ApiControllers_With_Authorize_And_Route()
    {
        var controllerTypes = KnowledgeBaseControllerDiscovery.GetControllerTypes();
        controllerTypes.Should().NotBeEmpty();
        foreach (var controllerType in controllerTypes)
        {
            controllerType.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            controllerType.GetCustomAttribute<RouteAttribute>().Should().NotBeNull();
            var actionMethods = controllerType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.GetCustomAttributes().Any(a => a is HttpMethodAttribute)).ToList();
            actionMethods.Should().NotBeEmpty();
            var hasControllerAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>() is not null;
            var allActionsAreSecured = actionMethods.All(method => method.GetCustomAttribute<AuthorizeAttribute>() is not null || method.GetCustomAttribute<AllowAnonymousAttribute>() is not null);
            (hasControllerAuthorize || allActionsAreSecured).Should().BeTrue();
        }
    }

    [Fact]
    public void All_KnowledgeBase_Controllers_Should_Use_Allowed_Policies()
    {
        var policies = KnowledgeBaseControllerDiscovery.GetControllerTypes()
            .SelectMany(type => type.GetCustomAttributes<AuthorizeAttribute>().Concat(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SelectMany(m => m.GetCustomAttributes<AuthorizeAttribute>())))
            .Select(x => x.Policy)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        policies.Should().NotBeEmpty();
        policies.Should().OnlyContain(x => AllowedPolicies.Contains(x));
    }
}
