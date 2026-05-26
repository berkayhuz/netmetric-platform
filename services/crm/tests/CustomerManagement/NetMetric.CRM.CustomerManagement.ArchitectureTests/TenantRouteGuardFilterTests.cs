// <copyright file="TenantRouteGuardFilterTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NetMetric.CRM.API.Security;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerManagement.ArchitectureTests;

public sealed class TenantRouteGuardFilterTests
{
    [Fact]
    public async Task TenantRouteGuard_Should_Reject_CrossTenant_ActionArguments()
    {
        var authenticatedTenantId = Guid.NewGuid();
        var requestedTenantId = Guid.NewGuid();
        var context = CreateContext(("tenantId", requestedTenantId));
        var filter = new TenantRouteGuardFilter(new FakeCurrentUserService(authenticatedTenantId));
        var nextWasCalled = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextWasCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        });

        nextWasCalled.Should().BeFalse();
        context.Result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task TenantRouteGuard_Should_Allow_Matching_Tenant_ActionArguments()
    {
        var tenantId = Guid.NewGuid();
        var context = CreateContext(("tenantId", tenantId));
        var filter = new TenantRouteGuardFilter(new FakeCurrentUserService(tenantId));
        var nextWasCalled = false;

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextWasCalled = true;
            return Task.FromResult(CreateExecutedContext(context));
        });

        nextWasCalled.Should().BeTrue();
        context.Result.Should().BeNull();
    }

    private static ActionExecutingContext CreateContext(params (string Name, object Value)[] arguments)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            [],
            arguments.ToDictionary(argument => argument.Name, argument => (object?)argument.Value),
            controller: new object());
    }

    private static ActionExecutedContext CreateExecutedContext(ActionExecutingContext context) =>
        new(context, [], controller: new object());

    private sealed class FakeCurrentUserService(Guid tenantId) : ICurrentUserService
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public bool IsAuthenticated => true;
        public string? UserName => "phase8-test";
        public string? Email => "phase8-test@netmetric.test";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [];
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission) => false;
    }
}
