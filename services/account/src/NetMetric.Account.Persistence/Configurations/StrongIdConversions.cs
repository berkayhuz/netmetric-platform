// <copyright file="StrongIdConversions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Persistence.Configurations;

internal static class StrongIdConversions
{
    public static readonly ValueConverter<TenantId, Guid> TenantId = new(
        id => id.Value,
        value => NetMetric.Account.Domain.Common.TenantId.From(value));

    public static readonly ValueConverter<UserId, Guid> UserId = new(
        id => id.Value,
        value => NetMetric.Account.Domain.Common.UserId.From(value));
}
