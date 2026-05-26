// <copyright file="DuplicateMatchTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.DuplicateMatchs;

namespace NetMetric.CRM.CustomerIntelligence.UnitTests;

public sealed class DuplicateMatchTests
{
    [Fact]
    public void Create_Should_Set_Name()
    {
        var entity = DuplicateMatch.Create("Sample");

        entity.Name.Should().Be("Sample");
        entity.Id.Should().NotBeEmpty();
    }
}
