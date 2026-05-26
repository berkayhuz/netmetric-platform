// <copyright file="ToolSlugTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Domain.UnitTests.ValueObjects;

public sealed class ToolSlugTests
{
    [Fact]
    public void Constructor_ShouldNormalize_ValidSlug()
    {
        var slug = new ToolSlug("PNG-To-JPG");
        slug.Value.Should().Be("png-to-jpg");
    }

    [Fact]
    public void Constructor_ShouldThrow_ForInvalidSlug()
    {
        var action = () => new ToolSlug("bad slug");
        action.Should().Throw<ArgumentException>();
    }
}
