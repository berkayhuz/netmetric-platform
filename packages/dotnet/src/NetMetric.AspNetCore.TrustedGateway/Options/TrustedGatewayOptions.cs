// <copyright file="TrustedGatewayOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.AspNetCore.TrustedGateway.Options;

public sealed class TrustedGatewayOptions
{
    public const string SectionName = "Security:TrustedGateway";

    public bool RequireTrustedGateway { get; set; } = true;

    public string Source { get; set; } = "NetMetric.ApiGateway";

    public string[] AllowedSources { get; set; } = ["NetMetric.ApiGateway"];

    public string CurrentKeyId { get; set; } = string.Empty;

    public string SignatureHeaderName { get; set; } = "X-Gateway-Signature";

    public string TimestampHeaderName { get; set; } = "X-Gateway-Timestamp";

    public string KeyIdHeaderName { get; set; } = "X-Gateway-Key-Id";

    public string SourceHeaderName { get; set; } = "X-Gateway-Source";

    public string NonceHeaderName { get; set; } = "X-Gateway-Request-Id";

    public string ContentHashHeaderName { get; set; } = "X-Gateway-Content-SHA256";

    public int AllowedClockSkewSeconds { get; set; } = 30;

    public int ReplayProtectionWindowSeconds { get; set; } = 90;

    public string[] AllowedGatewayProxies { get; set; } = [];

    public string[] AllowedGatewayNetworks { get; set; } = [];

    public List<TrustedGatewayKeyOptions> Keys { get; set; } = [];
}
