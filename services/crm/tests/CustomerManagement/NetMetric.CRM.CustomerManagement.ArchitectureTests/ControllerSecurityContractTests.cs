// <copyright file="ControllerSecurityContractTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.API.Controllers.CustomerManagement;

namespace NetMetric.CRM.CustomerManagement.ArchitectureTests;

public sealed class ControllerSecurityContractTests
{
    [Fact]
    public void All_CustomerManagement_Controllers_Should_Be_ApiControllers_With_Authorize_And_Route()
    {
        var controllerTypes = CustomerManagementControllerDiscovery.GetControllerTypes();

        controllerTypes.Should().NotBeEmpty();

        foreach (var controllerType in controllerTypes)
        {
            controllerType.GetCustomAttribute<ApiControllerAttribute>()
                .Should()
                .NotBeNull($"{controllerType.Name} must be [ApiController]");

            controllerType.GetCustomAttributes<RouteAttribute>()
                .Should()
                .NotBeEmpty($"{controllerType.Name} must declare a [Route]");

            var hasControllerAuthorize = controllerType.GetCustomAttribute<AuthorizeAttribute>() is not null;
            var controllerAllowsAnonymous = controllerType.GetCustomAttribute<AllowAnonymousAttribute>() is not null;

            var actionMethods = controllerType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsSpecialName && method.GetCustomAttribute<NonActionAttribute>() is null)
                .ToList();

            actionMethods.Should().NotBeEmpty($"{controllerType.Name} should expose at least one action");

            var allActionsAreExplicitlyAccountedFor = actionMethods.All(
                method => method.GetCustomAttribute<AuthorizeAttribute>() is not null ||
                          method.GetCustomAttribute<AllowAnonymousAttribute>() is not null);

            (controllerAllowsAnonymous || hasControllerAuthorize || allActionsAreExplicitlyAccountedFor)
                .Should()
                .BeTrue($"{controllerType.Name} must be secured at controller level or each action must explicitly declare [Authorize]/[AllowAnonymous]");
        }
    }

    [Fact]
    public void CustomerManagement_Controller_Routes_Should_Be_Unique_After_ControllerToken_Expansion()
    {
        var routes = CustomerManagementControllerDiscovery
            .GetControllerTypes()
            .SelectMany(type => type.GetCustomAttributes<RouteAttribute>().Select(attribute => new
            {
                type.Name,
                Template = attribute.Template
            }))
            .Where(x => !string.IsNullOrWhiteSpace(x.Template))
            .Select(x => x.Template!
                .Replace(
                    "[controller]",
                    x.Name.Replace("Controller", string.Empty, StringComparison.Ordinal),
                    StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        routes.Should().OnlyHaveUniqueItems();
    }

    [Theory]
    [InlineData(typeof(CustomersController), nameof(CustomersController.Get), AuthorizationPolicies.CustomersRead)]
    [InlineData(typeof(CustomersController), nameof(CustomersController.GetById), AuthorizationPolicies.CustomersRead)]
    [InlineData(typeof(CustomersController), nameof(CustomersController.GetContacts), AuthorizationPolicies.ContactsRead)]
    [InlineData(typeof(CustomersController), nameof(CustomersController.Create), AuthorizationPolicies.CustomersManage)]
    [InlineData(typeof(CustomersController), nameof(CustomersController.Update), AuthorizationPolicies.CustomersManage)]
    [InlineData(typeof(CustomersController), nameof(CustomersController.MarkVip), AuthorizationPolicies.CustomersManage)]
    [InlineData(typeof(CustomersController), nameof(CustomersController.SoftDelete), AuthorizationPolicies.CustomersManage)]
    [InlineData(typeof(CompaniesController), nameof(CompaniesController.Get), AuthorizationPolicies.CompaniesRead)]
    [InlineData(typeof(CompaniesController), nameof(CompaniesController.GetById), AuthorizationPolicies.CompaniesRead)]
    [InlineData(typeof(CompaniesController), nameof(CompaniesController.Create), AuthorizationPolicies.CompaniesManage)]
    [InlineData(typeof(CompaniesController), nameof(CompaniesController.Update), AuthorizationPolicies.CompaniesManage)]
    [InlineData(typeof(CompaniesController), nameof(CompaniesController.Activate), AuthorizationPolicies.CompaniesManage)]
    [InlineData(typeof(CompaniesController), nameof(CompaniesController.Deactivate), AuthorizationPolicies.CompaniesManage)]
    [InlineData(typeof(CompaniesController), nameof(CompaniesController.SoftDelete), AuthorizationPolicies.CompaniesManage)]
    [InlineData(typeof(ContactsController), nameof(ContactsController.Get), AuthorizationPolicies.ContactsRead)]
    [InlineData(typeof(ContactsController), nameof(ContactsController.GetById), AuthorizationPolicies.ContactsRead)]
    [InlineData(typeof(ContactsController), nameof(ContactsController.Create), AuthorizationPolicies.ContactsManage)]
    [InlineData(typeof(ContactsController), nameof(ContactsController.Update), AuthorizationPolicies.ContactsManage)]
    [InlineData(typeof(ContactsController), nameof(ContactsController.SetPrimary), AuthorizationPolicies.ContactsManage)]
    [InlineData(typeof(ContactsController), nameof(ContactsController.SoftDelete), AuthorizationPolicies.ContactsManage)]
    [InlineData(typeof(AddressesController), nameof(AddressesController.AddToCompany), AuthorizationPolicies.CompaniesManage)]
    [InlineData(typeof(AddressesController), nameof(AddressesController.AddToCustomer), AuthorizationPolicies.CustomersManage)]
    [InlineData(typeof(AddressesController), nameof(AddressesController.Update), AuthorizationPolicies.CustomersManage)]
    [InlineData(typeof(AddressesController), nameof(AddressesController.SetDefault), AuthorizationPolicies.CustomersManage)]
    [InlineData(typeof(AddressesController), nameof(AddressesController.SoftDelete), AuthorizationPolicies.CustomersManage)]
    public void Core_CustomerManagement_Actions_Should_Use_Read_Or_Manage_Policies(Type controllerType, string actionName, string expectedPolicy)
    {
        var action = controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Single(method => method.Name == actionName);

        action.GetCustomAttributes<AuthorizeAttribute>()
            .Should()
            .ContainSingle()
            .Which
            .Policy
            .Should()
            .Be(expectedPolicy);
    }
}
