// <copyright file="SearchCustomerManagementQueryValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.Features.Search.Queries.SearchCustomerManagement;
using NetMetric.CRM.CustomerManagement.Application.Validators;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Validators;

public sealed class SearchCustomerManagementQueryValidatorTests
{
    [Fact]
    public void Validate_Should_Fail_For_Invalid_Paging()
    {
        var validator = new SearchCustomerManagementQueryValidator();
        var query = new SearchCustomerManagementQuery { Term = "crm", PageNumber = 0, PageSize = 0 };

        validator.Validate(query).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_For_Valid_Request()
    {
        var validator = new SearchCustomerManagementQueryValidator();
        var query = new SearchCustomerManagementQuery { Term = "crm", PageNumber = 1, PageSize = 25 };

        validator.Validate(query).IsValid.Should().BeTrue();
    }
}
