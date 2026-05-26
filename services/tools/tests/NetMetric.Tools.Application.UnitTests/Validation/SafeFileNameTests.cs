// <copyright file="SafeFileNameTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.Tools.Application.Common;

namespace NetMetric.Tools.Application.UnitTests.Validation;

public sealed class SafeFileNameTests
{
    [Fact]
    public void Normalize_ShouldStripUnsafeSegments()
    {
        var result = SafeFileName.Normalize("..\\../../evil.png");
        result.Should().NotContain("..");
        result.Should().NotContain("\\");
        result.Should().NotContain("/");
    }
}
