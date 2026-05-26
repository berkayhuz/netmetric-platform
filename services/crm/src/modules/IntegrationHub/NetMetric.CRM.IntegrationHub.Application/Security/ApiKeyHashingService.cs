// <copyright file="ApiKeyHashingService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using System.Text;

namespace NetMetric.CRM.IntegrationHub.Application.Security;

public static class ApiKeyHashingService
{
    public static (string PlaintextKey, string Prefix, string Salt, string Hash) CreateNew()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var plaintext = $"nmk_{Convert.ToHexString(bytes).ToLowerInvariant()}";
        var prefix = plaintext[..Math.Min(14, plaintext.Length)];
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = SHA256.HashData(Combine(saltBytes, Encoding.UTF8.GetBytes(plaintext)));
        return (
            plaintext,
            prefix,
            Convert.ToBase64String(saltBytes),
            Convert.ToBase64String(hashBytes));
    }

    public static bool Verify(string plaintextKey, string salt, string hash)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var expected = Convert.FromBase64String(hash);
        var candidate = SHA256.HashData(Combine(saltBytes, Encoding.UTF8.GetBytes(plaintextKey ?? string.Empty)));
        return CryptographicOperations.FixedTimeEquals(expected, candidate);
    }

    private static byte[] Combine(byte[] left, byte[] right)
    {
        var combined = new byte[left.Length + right.Length];
        Buffer.BlockCopy(left, 0, combined, 0, left.Length);
        Buffer.BlockCopy(right, 0, combined, left.Length, right.Length);
        return combined;
    }
}
