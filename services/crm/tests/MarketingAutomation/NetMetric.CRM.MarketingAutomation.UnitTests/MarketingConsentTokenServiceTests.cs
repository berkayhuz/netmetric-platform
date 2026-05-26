// <copyright file="MarketingConsentTokenServiceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Security;

namespace NetMetric.CRM.MarketingAutomation.UnitTests;

public sealed class MarketingConsentTokenServiceTests
{
    [Fact]
    public async Task Valid_unsubscribe_token_is_tenant_scoped_and_single_use()
    {
        var tenantId = Guid.NewGuid();
        var cache = new InMemoryDistributedCache();
        var service = CreateService(cache: cache);
        var token = service.Issue(new MarketingConsentTokenIssueRequest(
            tenantId,
            "Ada@Example.com ",
            MarketingConsentTokenPurposes.Unsubscribe,
            "email-footer"));

        var first = await service.ValidateAndConsumeAsync(
            tenantId,
            token,
            MarketingConsentTokenPurposes.Unsubscribe,
            CancellationToken.None);
        var second = await service.ValidateAndConsumeAsync(
            tenantId,
            token,
            MarketingConsentTokenPurposes.Unsubscribe,
            CancellationToken.None);

        first.IsValid.Should().BeTrue();
        first.EmailAddress.Should().Be("ada@example.com");
        second.IsValid.Should().BeFalse();
        second.Reason.Should().Be("replay");
    }

    [Fact]
    public async Task Consent_token_rejects_expired_tampered_and_cross_tenant_requests()
    {
        var tenantId = Guid.NewGuid();
        var clock = new MutableTimeProvider(DateTimeOffset.Parse("2026-05-16T10:00:00Z"));
        var service = CreateService(clock);
        var token = service.Issue(new MarketingConsentTokenIssueRequest(
            tenantId,
            "ada@example.com",
            MarketingConsentTokenPurposes.Consent,
            "signup-form",
            MarketingConsentStatuses.Granted,
            ExpiresAtUtc: clock.GetUtcNow().AddMinutes(5)));

        var crossTenant = await service.ValidateAndConsumeAsync(
            Guid.NewGuid(),
            token,
            MarketingConsentTokenPurposes.Consent,
            CancellationToken.None);
        var tampered = await service.ValidateAndConsumeAsync(
            tenantId,
            $"{token[..^1]}x",
            MarketingConsentTokenPurposes.Consent,
            CancellationToken.None);
        clock.Advance(TimeSpan.FromMinutes(6));
        var expired = await service.ValidateAndConsumeAsync(
            tenantId,
            token,
            MarketingConsentTokenPurposes.Consent,
            CancellationToken.None);

        crossTenant.IsValid.Should().BeFalse();
        tampered.IsValid.Should().BeFalse();
        expired.IsValid.Should().BeFalse();
        expired.Reason.Should().Be("expired");
    }

    private static MarketingConsentTokenService CreateService(
        TimeProvider? clock = null,
        IDistributedCache? cache = null) =>
        new(
            Options.Create(new MarketingConsentTokenOptions
            {
                SigningKey = "unit-test-signing-key-with-enough-entropy-12345",
                LifetimeMinutes = 30
            }),
            cache ?? new InMemoryDistributedCache(),
            clock ?? new MutableTimeProvider(DateTimeOffset.Parse("2026-05-16T10:00:00Z")));

    private sealed class MutableTimeProvider(DateTimeOffset now) : TimeProvider
    {
        private DateTimeOffset _now = now;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan delta) => _now = _now.Add(delta);
    }

    private sealed class InMemoryDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _entries = [];

        public byte[]? Get(string key) => _entries.GetValueOrDefault(key);

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) =>
            Task.FromResult(Get(key));

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => _entries.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) =>
            _entries[key] = value;

        public Task SetAsync(
            string key,
            byte[] value,
            DistributedCacheEntryOptions options,
            CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }
    }
}
