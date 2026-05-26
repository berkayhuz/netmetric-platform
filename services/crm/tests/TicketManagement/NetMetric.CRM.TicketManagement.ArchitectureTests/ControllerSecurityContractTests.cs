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

namespace NetMetric.CRM.TicketManagement.ArchitectureTests;

public sealed class ControllerSecurityContractTests
{
    [Fact]
    public void All_TicketManagement_Controllers_Should_Be_ApiControllers_With_Authorize_And_Route()
    {
        var controllerTypes = TicketManagementControllerDiscovery.GetControllerTypes();

        controllerTypes.Should().NotBeEmpty();

        foreach (var controllerType in controllerTypes)
        {
            controllerType.GetCustomAttribute<ApiControllerAttribute>().Should().NotBeNull($"{controllerType.Name} must be [ApiController]");
            controllerType.GetCustomAttribute<RouteAttribute>().Should().NotBeNull($"{controllerType.Name} must declare a [Route]");

            var hasControllerAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>() is not null;

            var actionMethods = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName)
                .Where(method => method.GetCustomAttributes().Any(attribute => attribute is HttpMethodAttribute))
                .ToList();

            actionMethods.Should().NotBeEmpty($"{controllerType.Name} should expose at least one routable action");

            var allActionsAreSecured = actionMethods.All(method =>
                method.GetCustomAttribute<AuthorizeAttribute>() is not null
                || method.GetCustomAttribute<AllowAnonymousAttribute>() is not null);

            (hasControllerAuthorize || allActionsAreSecured)
                .Should()
                .BeTrue($"{controllerType.Name} must be secured at controller or action level");
        }
    }

    [Fact]
    public void All_TicketManagement_Controllers_Should_Use_Declared_Policies()
    {
        var controllerTypes = TicketManagementControllerDiscovery.GetControllerTypes();

        var declaredPolicies = new HashSet<string>(StringComparer.Ordinal);
        AddConstantStrings(typeof(AuthorizationPolicies), declaredPolicies, includeSymbolicNames: true, pathPrefix: null);
        AddConstantStrings(typeof(Permissions), declaredPolicies, includeSymbolicNames: true, pathPrefix: nameof(Permissions));

        var policies = controllerTypes
            .SelectMany(type => type.GetCustomAttributes<AuthorizeAttribute>())
            .Concat(controllerTypes
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .Where(method => !method.IsSpecialName)
                .Where(method => method.GetCustomAttributes().Any(attribute => attribute is HttpMethodAttribute))
                .SelectMany(method => method.GetCustomAttributes<AuthorizeAttribute>()))
            .Select(x => x.Policy)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        policies.Should().NotBeEmpty();
        policies.Should().OnlyContain(x => declaredPolicies.Contains(x));
    }

    [Fact]
    public void TicketManagement_Controller_Routes_Should_Be_Unique_After_ControllerToken_Expansion()
    {
        var routes = TicketManagementControllerDiscovery
            .GetControllerTypes()
            .Select(type => new
            {
                type.Name,
                Template = type.GetCustomAttribute<RouteAttribute>()?.Template
            })
            .Where(x => !string.IsNullOrWhiteSpace(x.Template))
            .Select(x => x.Template!.Replace("[controller]", x.Name.Replace("Controller", string.Empty, StringComparison.Ordinal), StringComparison.OrdinalIgnoreCase))
            .ToList();

        routes.Should().OnlyHaveUniqueItems();
    }

    private static void AddConstantStrings(Type type, ISet<string> sink, bool includeSymbolicNames, string? pathPrefix)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        foreach (var field in type.GetFields(flags).Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string)))
        {
            var value = (string?)field.GetRawConstantValue();
            if (!string.IsNullOrWhiteSpace(value))
            {
                sink.Add(value!);
            }

            if (includeSymbolicNames)
            {
                sink.Add(type.Name + "." + field.Name);

                if (!string.IsNullOrWhiteSpace(type.FullName))
                {
                    sink.Add(type.FullName + "." + field.Name);
                }

                if (!string.IsNullOrWhiteSpace(pathPrefix))
                {
                    sink.Add(pathPrefix + "." + field.Name);
                }
            }
        }

        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public))
        {
            var nestedPrefix = string.IsNullOrWhiteSpace(pathPrefix)
                ? nestedType.Name
                : pathPrefix + "." + nestedType.Name;

            AddConstantStrings(nestedType, sink, includeSymbolicNames, nestedPrefix);
        }
    }
}
