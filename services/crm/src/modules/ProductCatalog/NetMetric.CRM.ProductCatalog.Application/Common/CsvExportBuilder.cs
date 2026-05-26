// <copyright file="CsvExportBuilder.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text;

namespace NetMetric.CRM.ProductCatalog.Application.Common;

public static class CsvExportBuilder
{
    public static byte[] Build<T>(IReadOnlyCollection<string> headers, IReadOnlyCollection<T> rows, Func<T, IReadOnlyCollection<string?>> selector)
    {
        ArgumentNullException.ThrowIfNull(headers);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(selector);

        var builder = new StringBuilder(2048);
        builder.AppendLine(string.Join(",", headers.Select(Escape)));

        foreach (var row in rows)
            builder.AppendLine(string.Join(",", selector(row).Select(Escape)));

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var normalized = NeutralizeFormula(value.Replace("\r\n", "\n").Replace("\r", "\n"));
        var mustQuote = normalized.Contains(',') || normalized.Contains('"') || normalized.Contains('\n');
        var escaped = normalized.Replace("\"", "\"\"");

        return mustQuote ? $"\"{escaped}\"" : escaped;
    }

    private static string NeutralizeFormula(string value)
    {
        var trimmed = value.TrimStart();
        return trimmed.Length > 0 && trimmed[0] is '=' or '+' or '-' or '@' or '\t'
            ? "'" + value
            : value;
    }
}
