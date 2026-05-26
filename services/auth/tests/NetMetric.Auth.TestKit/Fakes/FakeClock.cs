// <copyright file="FakeClock.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Clock;

namespace NetMetric.Auth.TestKit.Fakes;

public sealed class FakeClock(DateTimeOffset? utcNow = null) : IClock
{
    private DateTimeOffset _utcNow = utcNow ?? new DateTimeOffset(
        2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public DateTimeOffset UtcNow => _utcNow;

    public DateTime UtcDateTime => _utcNow.UtcDateTime;

    public void Set(DateTimeOffset utcNow)
    {
        _utcNow = utcNow.ToUniversalTime();
    }

    public void Set(DateTime utcNow)
    {
        _utcNow = new DateTimeOffset(
            DateTime.SpecifyKind(utcNow, DateTimeKind.Utc));
    }

    public void Advance(TimeSpan delta)
    {
        _utcNow = _utcNow.Add(delta);
    }
}
