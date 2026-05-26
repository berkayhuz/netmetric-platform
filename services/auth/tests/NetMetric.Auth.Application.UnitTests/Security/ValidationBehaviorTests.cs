// <copyright file="ValidationBehaviorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using NetMetric.Auth.Application.Behaviors;

namespace NetMetric.Auth.Application.UnitTests.Security;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_Should_Throw_When_Any_Validator_Fails()
    {
        var validator = new InlineValidator<TestCommand>();
        validator.RuleFor(command => command.Value).NotEmpty();
        var validators = new IValidator<TestCommand>[] { validator };

        var sut = new ValidationBehavior<TestCommand, string>(validators);

        var action = async () => await sut.Handle(new TestCommand(string.Empty), _ => Task.FromResult("ok"), CancellationToken.None);

        await action.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_Should_Continue_When_Validators_Pass()
    {
        var validators = new IValidator<TestCommand>[]
        {
            new PassThroughValidator()
        };

        var sut = new ValidationBehavior<TestCommand, string>(validators);
        var response = await sut.Handle(new TestCommand("value"), _ => Task.FromResult("ok"), CancellationToken.None);

        response.Should().Be("ok");
    }

    private sealed record TestCommand(string Value);

    private sealed class PassThroughValidator : AbstractValidator<TestCommand>
    {
        public PassThroughValidator()
        {
            RuleFor(command => command.Value).Must(_ => true);
        }
    }
}
