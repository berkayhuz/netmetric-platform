// <copyright file="AssertionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using FluentAssertions;

namespace NetMetric.Auth.TestKit.Extensions;

public static class AssertionExtensions
{
    public static void ShouldHaveStatus(this HttpResponseMessage response, HttpStatusCode expected)
    {
        response.StatusCode.Should().Be(expected);
    }
}

