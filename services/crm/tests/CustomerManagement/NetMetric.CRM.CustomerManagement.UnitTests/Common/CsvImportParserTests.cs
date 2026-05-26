// <copyright file="CsvImportParserTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Common;

namespace NetMetric.CRM.CustomerManagement.Tests.Common;

public sealed class CsvImportParserTests
{
    [Fact]
    public void Parse_Should_Read_Headers_And_Rows()
    {
        var csv = """
                  Name,Email,Phone
                  Acme,info@acme.test,555
                  Beta,beta@test.com,777
                  """;

        var result = CsvImportParser.Parse(csv);

        result.Headers.Should().ContainInOrder("Name", "Email", "Phone");
        result.Rows.Should().HaveCount(2);
        result.Rows[0]["Name"].Should().Be("Acme");
        result.Rows[1]["Phone"].Should().Be("777");
    }

    [Fact]
    public void Parse_Should_Handle_Quoted_Commas_And_LineBreaks()
    {
        var csv = """
                  Name,Description
                  "Acme, Inc.","Line 1
                  Line 2"
                  """;

        var result = CsvImportParser.Parse(csv);

        result.Rows.Should().HaveCount(1);
        result.Rows[0]["Name"].Should().Be("Acme, Inc.");
        result.Rows[0]["Description"].Should().Be("Line 1\nLine 2");
    }

    [Fact]
    public void Parse_Should_Skip_Empty_Data_Rows()
    {
        var csv = """
                  Name,Email

                  Acme,info@acme.test
                  """;

        var result = CsvImportParser.Parse(csv);

        result.Rows.Should().HaveCount(1);
        result.Rows[0]["Name"].Should().Be("Acme");
    }
}
