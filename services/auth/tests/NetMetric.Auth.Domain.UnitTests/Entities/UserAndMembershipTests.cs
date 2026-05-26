// <copyright file="UserAndMembershipTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.Auth.Domain.Entities;
using NetMetric.Entities;

namespace NetMetric.Auth.Domain.UnitTests.Entities;

public sealed class UserAndMembershipTests
{
    [Fact]
    public void User_GetRoles_Should_Return_Distinct_CaseInsensitive_Roles()
    {
        var user = new User
        {
            Roles = "tenant-user, Tenant-User ,tenant-admin,tenant-admin"
        };

        var roles = user.GetRoles();

        roles.Should().BeEquivalentTo(["tenant-user", "tenant-admin"]);
    }

    [Fact]
    public void UserTenantMembership_GetPermissions_Should_Trim_And_Deduplicate()
    {
        var membership = new UserTenantMembership
        {
            Permissions = "session:self, profile:self,SESSION:SELF"
        };

        var permissions = membership.GetPermissions();

        permissions.Should().BeEquivalentTo(["session:self", "profile:self"]);
    }

    [Fact]
    public void EntityBase_Activate_Deactivate_Should_Toggle_IsActive()
    {
        var entity = new TestEntity();

        entity.Deactivate();
        entity.IsActive.Should().BeFalse();

        entity.Activate();
        entity.IsActive.Should().BeTrue();
    }

    private sealed class TestEntity : EntityBase;
}
