// <copyright file="CustomerManagementIntegrationEventContractTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.CustomerManagement.Application.IntegrationEvents;

namespace NetMetric.CRM.CustomerManagement.UnitTests.IntegrationEvents;

public sealed class CustomerManagementIntegrationEventContractTests
{
    [Theory]
    [InlineData(CustomerManagementIntegrationEventNames.CustomerCreated, "crm.customer.created")]
    [InlineData(CustomerManagementIntegrationEventNames.CustomerUpdated, "crm.customer.updated")]
    [InlineData(CustomerManagementIntegrationEventNames.CustomerDeleted, "crm.customer.deleted")]
    [InlineData(CustomerManagementIntegrationEventNames.ContactCreated, "crm.contact.created")]
    [InlineData(CustomerManagementIntegrationEventNames.ContactUpdated, "crm.contact.updated")]
    [InlineData(CustomerManagementIntegrationEventNames.ContactDeleted, "crm.contact.deleted")]
    public void Event_Names_Should_Remain_Stable(string actual, string expected)
    {
        actual.Should().Be(expected);
    }
}
