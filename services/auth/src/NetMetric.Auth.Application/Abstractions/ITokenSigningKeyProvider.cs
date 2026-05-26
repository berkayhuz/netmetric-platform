// <copyright file="ITokenSigningKeyProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.IdentityModel.Tokens;

namespace NetMetric.Auth.Application.Abstractions;

public interface ITokenSigningKeyProvider
{
    SigningCredentials GetCurrentSigningCredentials();

    IReadOnlyCollection<SecurityKey> GetValidationKeys();

    string CurrentKeyId { get; }

    object GetJwksDocument(string issuer);
}
