// <copyright file="FileSizeBytesTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Domain.UnitTests.ValueObjects;

public sealed class FileSizeBytesTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenNotPositive()
    {
        var action = () => new FileSizeBytes(0);
        action.Should().Throw<ArgumentException>();
    }
}
