// <copyright file="TrustedInternalSourceRequirement.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;

namespace NetMetric.Auth.API.Security;

public sealed class TrustedInternalSourceRequirement : IAuthorizationRequirement;
