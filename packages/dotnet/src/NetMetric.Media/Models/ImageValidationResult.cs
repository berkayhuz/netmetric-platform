// <copyright file="ImageValidationResult.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Models;

public sealed record ImageValidationResult(
    bool IsValid,
    string? FailureReason,
    string? CanonicalContentType,
    string? Extension);
