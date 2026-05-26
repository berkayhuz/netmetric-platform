// <copyright file="DatabaseResetHelper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.TestKit.Helpers;

public static class DatabaseResetHelper
{
    public static async Task ResetAsync(AuthDbContext dbContext)
    {
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

