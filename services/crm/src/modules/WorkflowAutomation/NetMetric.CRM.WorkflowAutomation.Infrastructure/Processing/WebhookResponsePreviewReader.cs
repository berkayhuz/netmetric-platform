// <copyright file="WebhookResponsePreviewReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public static class WebhookResponsePreviewReader
{
    public static async Task<string> ReadAsync(HttpContent content, int maxBytes, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(maxBytes, 1024, 1048576);
        var contentLength = content.Headers.ContentLength;
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream(capacity: Math.Min(limit, 8192));
        var bytes = new byte[8192];
        var remaining = limit;

        while (remaining > 0)
        {
            var read = await stream.ReadAsync(bytes.AsMemory(0, Math.Min(bytes.Length, remaining)), cancellationToken);
            if (read == 0)
            {
                break;
            }

            buffer.Write(bytes, 0, read);
            remaining -= read;
        }

        var text = Encoding.UTF8.GetString(buffer.ToArray());
        var truncated = contentLength.HasValue ? contentLength.Value > limit : remaining == 0;
        return truncated ? text + " [truncated]" : text;
    }
}

public static class WebhookSignatureAuditMetadata
{
    public static string Create(string? signatureHeader) =>
        string.IsNullOrWhiteSpace(signatureHeader)
            ? "signature=absent"
            : "signature=present;algorithm=hmac-sha256";
}

