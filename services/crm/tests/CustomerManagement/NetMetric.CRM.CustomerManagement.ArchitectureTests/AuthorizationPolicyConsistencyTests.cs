// <copyright file="AuthorizationPolicyConsistencyTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using NetMetric.CRM.API.Compatibility;

namespace NetMetric.CRM.CustomerManagement.ArchitectureTests;

public sealed class AuthorizationPolicyConsistencyTests
{
    [Fact]
    public void All_Used_Controller_Policies_Should_Exist_In_AuthorizationPolicies()
    {
        var declaredPolicies = new HashSet<string>(StringComparer.Ordinal);
        AddConstantStrings(typeof(AuthorizationPolicies), declaredPolicies, includeSymbolicNames: true);
        AddConstantStrings(typeof(Permissions), declaredPolicies, includeSymbolicNames: true);

        var usedPolicies = CustomerManagementControllerDiscovery
            .GetControllerTypes()
            .SelectMany(type => type.GetCustomAttributes<AuthorizeAttribute>())
            .Concat(
                CustomerManagementControllerDiscovery
                    .GetControllerTypes()
                    .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    .SelectMany(method => method.GetCustomAttributes<AuthorizeAttribute>()))
            .Select(attribute => attribute.Policy)
            .Where(policy => !string.IsNullOrWhiteSpace(policy))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        usedPolicies.Should().NotBeEmpty();
        usedPolicies.Should().OnlyContain(policy => declaredPolicies.Contains(policy));
    }

    private static void AddConstantStrings(
        Type type,
        ISet<string> sink,
        bool includeSymbolicNames,
        string? symbolicPrefix = null)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

        symbolicPrefix ??= type.Name;

        foreach (var field in type.GetFields(flags)
                     .Where(field => field is { IsLiteral: true, IsInitOnly: false } && field.FieldType == typeof(string)))
        {
            var value = (string?)field.GetRawConstantValue();
            if (!string.IsNullOrWhiteSpace(value))
            {
                sink.Add(value!);
            }

            if (includeSymbolicNames)
            {
                sink.Add($"{symbolicPrefix}.{field.Name}");

                if (!string.IsNullOrWhiteSpace(type.FullName))
                {
                    sink.Add($"{type.FullName!.Replace('+', '.')}.{field.Name}");
                }
            }
        }

        foreach (var nestedType in type.GetNestedTypes(BindingFlags.Public))
        {
            AddConstantStrings(
                nestedType,
                sink,
                includeSymbolicNames,
                $"{symbolicPrefix}.{nestedType.Name}");
        }
    }
}
