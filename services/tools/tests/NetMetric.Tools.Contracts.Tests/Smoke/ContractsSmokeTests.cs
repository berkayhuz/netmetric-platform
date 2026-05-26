// <copyright file="ContractsSmokeTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.Tools.Contracts.Catalog;

namespace NetMetric.Tools.Contracts.Tests.Smoke;

public sealed class ContractsSmokeTests
{
    [Fact]
    public void ToolCategoryResponse_ShouldCreate()
    {
        var response = new ToolCategoryResponse("image", "Image", "desc", 1);
        response.Slug.Should().Be("image");
    }
}
