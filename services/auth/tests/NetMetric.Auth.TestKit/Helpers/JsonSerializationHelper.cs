// <copyright file="JsonSerializationHelper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;

namespace NetMetric.Auth.TestKit.Helpers;

public static class JsonSerializationHelper
{
    public static readonly JsonSerializerOptions WebOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public static StringContent ToJsonContent<T>(T value) =>
        new(JsonSerializer.Serialize(value, WebOptions), System.Text.Encoding.UTF8, "application/json");

    public static async Task<T?> ReadAsJsonAsync<T>(HttpContent content)
    {
        var payload = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(payload, WebOptions);
    }
}

