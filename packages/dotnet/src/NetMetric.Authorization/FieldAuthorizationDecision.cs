// <copyright file="FieldAuthorizationDecision.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Authorization;

public sealed record FieldAuthorizationDecision(
    string Resource,
    string Field,
    FieldVisibility Visibility);
