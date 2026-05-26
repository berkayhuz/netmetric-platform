// <copyright file="DocumentRecordTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentRecords;

namespace NetMetric.CRM.DocumentManagement.UnitTests;

public sealed class DocumentRecordTests
{
    [Fact]
    public void Create_Should_Set_Name()
    {
        var entity = DocumentRecord.Create("Sample");

        entity.Name.Should().Be("Sample");
        entity.Id.Should().NotBeEmpty();
    }
}
