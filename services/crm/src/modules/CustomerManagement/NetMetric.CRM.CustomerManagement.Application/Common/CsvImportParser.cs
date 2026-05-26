// <copyright file="CsvImportParser.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text;

namespace NetMetric.CRM.CustomerManagement.Application.Common;

public static class CsvImportParser
{
    public static CsvImportDocument Parse(string csvContent, char separator = ',')
    {
        if (string.IsNullOrWhiteSpace(csvContent))
            throw new ArgumentException("CSV content cannot be empty.", nameof(csvContent));

        var rows = ParseRows(csvContent, separator);
        if (rows.Count == 0)
            return new CsvImportDocument(Array.Empty<string>(), Array.Empty<IReadOnlyDictionary<string, string?>>());

        var headers = rows[0]
            .Select(NormalizeHeader)
            .ToArray();

        var documents = new List<IReadOnlyDictionary<string, string?>>(Math.Max(0, rows.Count - 1));

        for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
        {
            var sourceRow = rows[rowIndex];
            var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < headers.Length; i++)
            {
                var value = i < sourceRow.Count ? NormalizeValue(sourceRow[i]) : null;
                values[headers[i]] = value;
            }

            if (values.Values.All(string.IsNullOrWhiteSpace))
                continue;

            documents.Add(values);
        }

        return new CsvImportDocument(headers, documents);
    }

    private static List<List<string>> ParseRows(string csvContent, char separator)
    {
        var rows = new List<List<string>>();
        var currentRow = new List<string>();
        var currentCell = new StringBuilder(csvContent.Length > 0 ? Math.Min(csvContent.Length, 256) : 16);

        var inQuotes = false;

        for (var i = 0; i < csvContent.Length; i++)
        {
            var ch = csvContent[i];

            if (inQuotes)
            {
                if (ch == '"')
                {
                    var nextIsQuote = i + 1 < csvContent.Length && csvContent[i + 1] == '"';
                    if (nextIsQuote)
                    {
                        currentCell.Append('"');
                        i++;
                        continue;
                    }

                    inQuotes = false;
                    continue;
                }

                currentCell.Append(ch);
                continue;
            }

            if (ch == '"')
            {
                inQuotes = true;
                continue;
            }

            if (ch == separator)
            {
                currentRow.Add(currentCell.ToString());
                currentCell.Clear();
                continue;
            }

            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                currentRow.Add(currentCell.ToString());
                currentCell.Clear();

                rows.Add(currentRow);
                currentRow = new List<string>();
                continue;
            }

            currentCell.Append(ch);
        }

        currentRow.Add(currentCell.ToString());

        if (currentRow.Count > 1 || !string.IsNullOrWhiteSpace(currentRow[0]))
            rows.Add(currentRow);

        return rows;
    }

    private static string NormalizeHeader(string value)
    {
        var normalized = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return string.Empty;

        return normalized
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);
    }

    private static string? NormalizeValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}

public sealed record CsvImportDocument(
    IReadOnlyList<string> Headers,
    IReadOnlyList<IReadOnlyDictionary<string, string?>> Rows);
