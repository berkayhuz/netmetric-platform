// <copyright file="ControllerSecurityContractTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.PipelineManagement.Application.Security;

namespace NetMetric.CRM.PipelineManagement.ArchitectureTests;

public sealed class ControllerSecurityContractTests
{
    [Fact]
    public void All_PipelineManagement_Controllers_Should_Be_ApiControllers_With_Authorize_And_Route()
    {
        var controllerTypes = PipelineManagementControllerDiscovery.GetControllerTypes();
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
    public void All_PipelineManagement_Controllers_Should_Use_Declared_Policies()
    {
        var declared = new HashSet<string>(StringComparer.Ordinal)
        {
            PipelineManagementAuthorizationPolicies.LostReasonsManage,
            PipelineManagementAuthorizationPolicies.LostReasonsRead,
            PipelineManagementAuthorizationPolicies.OpportunityPipelineManage,
            PipelineManagementAuthorizationPolicies.OpportunityStageHistoryRead,
            PipelineManagementAuthorizationPolicies.LeadConversionsManage,
            PipelineManagementAuthorizationPolicies.LeadConversionsRead,
            PipelineManagementAuthorizationPolicies.PipelinesManage,
            PipelineManagementAuthorizationPolicies.PipelinesRead
        };

        var policies = PipelineManagementControllerDiscovery
            .GetControllerTypes()
            .SelectMany(t => t.GetCustomAttributes<AuthorizeAttribute>())
            .Concat(PipelineManagementControllerDiscovery.GetControllerTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                .SelectMany(m => m.GetCustomAttributes<AuthorizeAttribute>()))
            .Select(a => a.Policy)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToList();

        policies.Should().NotBeEmpty();
        policies.Should().OnlyContain(p => declared.Contains(p));
    }
}
