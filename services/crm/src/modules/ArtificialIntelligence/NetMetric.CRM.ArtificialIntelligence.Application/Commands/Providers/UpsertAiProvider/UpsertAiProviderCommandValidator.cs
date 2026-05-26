// <copyright file="UpsertAiProviderCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.Security.EndpointGuard;

namespace NetMetric.CRM.ArtificialIntelligence.Application.Commands.Providers.UpsertAiProvider;

public sealed class UpsertAiProviderCommandValidator : AbstractValidator<UpsertAiProviderCommand>
{
    public UpsertAiProviderCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.ModelName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Endpoint)
            .NotEmpty()
            .MaximumLength(300)
            .Must(ExternalEndpointGuard.IsTrustedHttpsEndpoint)
            .WithMessage("Endpoint must be a trusted HTTPS endpoint and cannot point to local or private network hosts.");
        RuleFor(x => x.SecretReference)
            .NotEmpty()
            .MaximumLength(300)
            .Must(ExternalEndpointGuard.IsSecretReference)
            .WithMessage("SecretReference must be a secret manager reference such as secret://, vault://, keyvault://, azure-keyvault://, aws-secretsmanager://, or gcp-secretmanager://.");
        RuleFor(x => x.Capabilities).NotEmpty();
        RuleForEach(x => x.Capabilities).IsInEnum();
        RuleFor(x => x.Provider).IsInEnum();
    }
}
