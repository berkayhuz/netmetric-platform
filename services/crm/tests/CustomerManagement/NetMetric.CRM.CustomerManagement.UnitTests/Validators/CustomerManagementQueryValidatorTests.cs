// <copyright file="CustomerManagementQueryValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;
using NetMetric.CRM.CustomerManagement.Application.Queries.Customers;
using NetMetric.CRM.CustomerManagement.Application.Validators;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Validators;

public sealed class CustomerManagementQueryValidatorTests
{
    [Fact]
    public void GetCustomersQueryValidator_Should_Accept_Allowed_Sort()
    {
        var validator = new GetCustomersQueryValidator();
        var result = validator.Validate(new GetCustomersQuery(SortBy: "createdAt", SortDirection: "desc"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetCustomersQueryValidator_Should_Reject_Unknown_Sort()
    {
        var validator = new GetCustomersQueryValidator();
        var result = validator.Validate(new GetCustomersQuery(SortBy: "rawSql", SortDirection: "desc"));

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void GetContactsQueryValidator_Should_Reject_Invalid_Page_Size()
    {
        var validator = new GetContactsQueryValidator();
        var result = validator.Validate(new GetContactsQuery(PageSize: 500));

        result.IsValid.Should().BeFalse();
    }
}
