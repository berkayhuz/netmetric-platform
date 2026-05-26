// <copyright file="ControllerSecurityContractTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using NetMetric.CRM.API.Compatibility;

namespace NetMetric.CRM.ProductCatalog.ArchitectureTests;

public sealed class ControllerSecurityContractTests
{
    [Fact]
    public void All_ProductCatalog_Controllers_Should_Be_ApiControllers_With_Authorize_And_Route()
    {
        var controllerTypes = ProductCatalogControllerDiscovery.GetControllerTypes();
        controllerTypes.Should().NotBeEmpty();

        foreach (var controllerType in controllerTypes)
        {
            controllerType.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            controllerType.GetCustomAttributes<RouteAttribute>().Should().NotBeEmpty();

            var hasControllerAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>() is not null;
            var actionMethods = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName && m.GetCustomAttributes().Any(a => a is HttpMethodAttribute))
                .ToList();

            actionMethods.Should().NotBeEmpty();
            var allActionsAreSecured = actionMethods.All(method => method.GetCustomAttribute<AuthorizeAttribute>() is not null);
            (hasControllerAuthorize || allActionsAreSecured).Should().BeTrue();
        }
    }

    [Fact]
    public void All_ProductCatalog_Controllers_Should_Use_Declared_Policies()
    {
        var declaredPolicies = new HashSet<string>(StringComparer.Ordinal);
        AddConstantStrings(typeof(AuthorizationPolicies), declaredPolicies);
        AddConstantStrings(typeof(Permissions), declaredPolicies);

        var policies = ProductCatalogControllerDiscovery.GetControllerTypes()
            .SelectMany(type => type.GetCustomAttributes<AuthorizeAttribute>()
                .Concat(type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .SelectMany(method => method.GetCustomAttributes<AuthorizeAttribute>())))
            .Select(x => x.Policy)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        policies.Should().NotBeEmpty();
        policies.Should().OnlyContain(policy => declaredPolicies.Contains(policy));
    }

    private static void AddConstantStrings(Type type, ISet<string> sink)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        foreach (var field in type.GetFields(flags).Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string)))
        {
            var value = (string?)field.GetRawConstantValue();
            if (!string.IsNullOrWhiteSpace(value))
                sink.Add(value!);
        }

        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public))
            AddConstantStrings(nestedType, sink);
    }
}
