// <copyright file="CreateCustomFieldDefinitionCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.CreateCustomFieldDefinition;
using NetMetric.CRM.CustomerManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerManagement.Tests.Validators;

public sealed class CreateCustomFieldDefinitionCommandValidatorTests
{
    [Fact]
    public void Should_Pass_For_Valid_Request()
    {
        var validator = new CreateCustomFieldDefinitionCommandValidator();
        var result = validator.Validate(new CreateCustomFieldDefinitionCommand
        {
            Name = "industry_segment",
            Label = "Industry Segment",
            EntityName = "company",
            DataType = CustomFieldDataType.Text,
            OrderNo = 1
        });

        Assert.True(result.IsValid);
    }
}
