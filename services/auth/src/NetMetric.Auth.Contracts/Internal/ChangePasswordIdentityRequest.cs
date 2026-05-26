// <copyright file="ChangePasswordIdentityRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Internal;

public sealed record ChangePasswordIdentityRequest(
    string CurrentPassword,
    string NewPassword,
    bool RevokeOtherSessions);
