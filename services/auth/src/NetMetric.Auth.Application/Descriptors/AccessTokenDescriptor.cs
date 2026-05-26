// <copyright file="AccessTokenDescriptor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Descriptors;

public sealed record AccessTokenDescriptor(string Token, DateTime ExpiresAtUtc);
