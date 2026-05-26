// <copyright file="SalesOrderTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.FinanceOperations.Domain.Entities.Orders;

namespace NetMetric.CRM.FinanceOperations.UnitTests;

public sealed class SalesOrderTests
{
    [Fact]
    public void Constructor_Should_Set_Required_Fields()
    {
        var entity = new SalesOrder("CODE-1", "Name 1", "Desc");
        entity.Code.Should().Be("CODE-1");
        entity.Name.Should().Be("Name 1");
        entity.Description.Should().Be("Desc");
        entity.IsActive.Should().BeTrue();
    }
}
