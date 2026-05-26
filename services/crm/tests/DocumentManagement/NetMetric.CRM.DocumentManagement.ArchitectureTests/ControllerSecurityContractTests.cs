// <copyright file="ControllerSecurityContractTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.DocumentManagement.Application.Security;

namespace NetMetric.CRM.DocumentManagement.ArchitectureTests;

public sealed class ControllerSecurityContractTests
{
    [Fact]
    public void All_DocumentManagement_Controllers_Should_Be_ApiControllers_With_Authorize_And_Route()
    {
        var controllerTypes = DocumentManagementControllerDiscovery.GetControllerTypes();

        controllerTypes.Should().NotBeEmpty();

        foreach (var controllerType in controllerTypes)
        {
            controllerType.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull();
            controllerType.GetCustomAttribute<RouteAttribute>().Should().NotBeNull();

            var hasControllerAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>() is not null;
            var actionMethods = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .ToList();

            actionMethods.Should().NotBeEmpty();
            var allActionsAreSecured = actionMethods.All(method => method.GetCustomAttribute<AuthorizeAttribute>() is not null);
            (hasControllerAuthorize || allActionsAreSecured).Should().BeTrue();
        }
    }

    [Fact]
    public void All_DocumentManagement_Controllers_Should_Use_Declared_Policies()
    {
        var declaredPolicies = new HashSet<string>(StringComparer.Ordinal);
        AddConstantStrings(typeof(AuthorizationPolicies), declaredPolicies);
        AddConstantStrings(typeof(Permissions), declaredPolicies);

        AddConstantStrings(typeof(DocumentManagementPermissions), declaredPolicies);


        var policies = DocumentManagementControllerDiscovery.GetControllerTypes()
            .SelectMany(type => type.GetCustomAttributes<AuthorizeAttribute>())
            .Concat(DocumentManagementControllerDiscovery.GetControllerTypes()
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .SelectMany(method => method.GetCustomAttributes<AuthorizeAttribute>()))
            .Select(x => x.Policy)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct()
            .ToList();

        policies.Should().NotBeEmpty();
        policies.Should().OnlyContain(x => declaredPolicies.Contains(x));
    }

    private static void AddConstantStrings(Type type, ISet<string> sink)
    {
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                     .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string)))
        {
            var value = (string?)field.GetRawConstantValue();
            if (!string.IsNullOrWhiteSpace(value))
                sink.Add(value!);
        }

        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public))
            AddConstantStrings(nestedType, sink);
    }
}
