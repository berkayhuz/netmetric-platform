// <copyright file="MediaLocalOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Options;

public sealed class MediaLocalOptions
{
    public string RootPath { get; init; } = ".runlogs/media";
    public string RequestPath { get; init; } = "/uploads";
}
