// <copyright file="HttpCurrentUserAccessorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NetMetric.Account.Infrastructure.Security;

namespace NetMetric.Account.Api.UnitTests;

public sealed class HttpCurrentUserAccessorTests
{
    [Fact]
    public void GetRequired_Should_Read_Mapped_Auth_Claims()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim("tenant_id", tenantId.ToString("D")),
            new Claim("sub", userId.ToString("D")),
            new Claim("sid", sessionId.ToString("D")),
            new Claim(ClaimTypes.AuthenticationInstant, now.ToUnixTimeSeconds().ToString()),
            new Claim(ClaimTypes.AuthenticationMethod, "pwd")
        ], "test"));

        var httpContext = new DefaultHttpContext { User = principal };
        var accessor = new HttpCurrentUserAccessor(new HttpContextAccessor { HttpContext = httpContext });

        var current = accessor.GetRequired();

        current.TenantId.Should().Be(tenantId);
        current.UserId.Should().Be(userId);
        current.SessionId.Should().Be(sessionId);
        current.AuthenticatedAt.Should().NotBeNull();
        current.AuthenticationMethods.Should().Contain("pwd");
    }
}

