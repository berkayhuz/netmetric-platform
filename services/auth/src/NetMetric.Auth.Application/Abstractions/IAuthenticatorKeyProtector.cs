// <copyright file="IAuthenticatorKeyProtector.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Abstractions;

public interface IAuthenticatorKeyProtector
{
    string Protect(string sharedKey);
    string Unprotect(string protectedSharedKey);
}
