// <copyright file="ArtifactRulesTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.Tools.Application.Common;
using NetMetric.Tools.Domain.Entities;
using NetMetric.Tools.Domain.Enums;

namespace NetMetric.Tools.Application.UnitTests.Validation;

public sealed class ArtifactRulesTests
{
    [Fact]
    public void EnsureSaveAllowed_ShouldRejectSvg()
    {
        var tool = new ToolDefinition(
            "png-to-jpg",
            "PNG to JPG",
            "desc",
            "seo",
            "seo desc",
            Guid.NewGuid(),
            ToolExecutionMode.Browser,
            ToolAvailabilityStatus.Enabled,
            true,
            5 * 1024 * 1024,
            10 * 1024 * 1024,
            ["image/png", "image/jpeg"]);

        var action = () => ArtifactRules.EnsureSaveAllowed(tool, "image/svg+xml", 128);
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void EnsureSaveAllowed_ShouldRejectOverLimit()
    {
        var tool = new ToolDefinition(
            "png-to-jpg",
            "PNG to JPG",
            "desc",
            "seo",
            "seo desc",
            Guid.NewGuid(),
            ToolExecutionMode.Browser,
            ToolAvailabilityStatus.Enabled,
            true,
            5 * 1024 * 1024,
            10 * 1024 * 1024,
            ["image/png", "image/jpeg"]);

        var action = () => ArtifactRules.EnsureSaveAllowed(tool, "image/png", (10 * 1024 * 1024) + 1);
        action.Should().Throw<InvalidOperationException>();
    }
}
