// <copyright file="IntegrationHubController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Providers;
using NetMetric.CRM.IntegrationHub.Application.Commands.CancelIntegrationJob;
using NetMetric.CRM.IntegrationHub.Application.Commands.RegisterWebhook;
using NetMetric.CRM.IntegrationHub.Application.Commands.ReplayIntegrationJob;
using NetMetric.CRM.IntegrationHub.Application.Commands.ScheduleIntegrationJob;
using NetMetric.CRM.IntegrationHub.Application.Commands.UpsertIntegrationConnection;
using NetMetric.CRM.IntegrationHub.Application.Commands.ValidateWebhookDelivery;
using NetMetric.CRM.IntegrationHub.Application.Queries.GetConnectorHealth;
using NetMetric.CRM.IntegrationHub.Application.Queries.GetIntegrationJobDetail;
using NetMetric.CRM.IntegrationHub.Application.Queries.GetIntegrationOverview;
using NetMetric.CRM.IntegrationHub.Application.Queries.GetIntegrationWorkerStatus;
using NetMetric.CRM.IntegrationHub.Application.Queries.ListIntegrationDeadLetters;
using NetMetric.CRM.IntegrationHub.Application.Queries.ListIntegrationJobs;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Tenancy;

namespace NetMetric.CRM.API.Controllers.Integrations;

public sealed class IntegrationHubControllerServices(
    IIntegrationWebhookSecurityService webhookSecurityService,
    IIntegrationProviderCatalog providerCatalog,
    IProviderCredentialValidator providerCredentialValidator,
    IProviderConnectionTester providerConnectionTester,
    IHttpClientFactory httpClientFactory)
{
    public IIntegrationWebhookSecurityService WebhookSecurityService { get; } = webhookSecurityService;

    public IIntegrationProviderCatalog ProviderCatalog { get; } = providerCatalog;

    public IProviderCredentialValidator ProviderCredentialValidator { get; } = providerCredentialValidator;

    public IProviderConnectionTester ProviderConnectionTester { get; } = providerConnectionTester;

    public IHttpClientFactory HttpClientFactory { get; } = httpClientFactory;
}

[ApiController]
[Route("api/integrations")]
[Authorize(Policy = AuthorizationPolicies.IntegrationsRead)]
public sealed class IntegrationHubController(
    IMediator mediator,
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext,
    IntegrationHubControllerServices services) : ControllerBase
{
    [HttpGet("tenants/{tenantId:guid}/overview")]
    public async Task<IActionResult> GetOverview(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetIntegrationOverviewQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/jobs")]
    public async Task<IActionResult> ListJobs(
        Guid tenantId,
        [FromQuery] string? status,
        [FromQuery] string? providerKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new ListIntegrationJobsQuery(tenantId, status, providerKey, page, pageSize), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/jobs/{jobId:guid}")]
    public async Task<IActionResult> GetJobDetail(Guid tenantId, Guid jobId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetIntegrationJobDetailQuery(tenantId, jobId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/dead-letters")]
    public async Task<IActionResult> ListDeadLetters(
        Guid tenantId,
        [FromQuery] string? providerKey,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new ListIntegrationDeadLettersQuery(tenantId, providerKey, page, pageSize), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/connector-health")]
    public async Task<IActionResult> GetConnectorHealth(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetConnectorHealthQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/worker-status")]
    public async Task<IActionResult> GetWorkerStatus(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetIntegrationWorkerStatusQuery(tenantId), cancellationToken));

    [HttpPut("tenants/{tenantId:guid}/connections")]
    [Authorize(Policy = AuthorizationPolicies.IntegrationsManage)]
    public async Task<IActionResult> UpsertConnection(
        Guid tenantId,
        [FromBody] UpsertIntegrationConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new UpsertIntegrationConnectionCommand(
                tenantId,
                request.ProviderKey,
                request.DisplayName,
                request.Category,
                request.SettingsJson,
                request.IsEnabled),
            cancellationToken);

        return Ok(new { id });
    }

    [HttpPost("tenants/{tenantId:guid}/jobs")]
    [Authorize(Policy = AuthorizationPolicies.IntegrationsManage)]
    public async Task<IActionResult> ScheduleJob(
        Guid tenantId,
        [FromBody] ScheduleIntegrationJobRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new ScheduleIntegrationJobCommand(
                tenantId,
                request.JobType,
                request.Direction,
                request.PayloadJson,
                request.ScheduledAtUtc,
                request.ProviderKey,
                request.IdempotencyKey,
                request.MaxAttempts),
            cancellationToken);

        return CreatedAtAction(nameof(GetOverview), new { tenantId }, new { id });
    }

    [HttpPost("tenants/{tenantId:guid}/jobs/{jobId:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.IntegrationsManage)]
    public async Task<IActionResult> CancelJob(
        Guid tenantId,
        Guid jobId,
        [FromBody] CancelIntegrationJobRequest request,
        CancellationToken cancellationToken)
    {
        await mediator.Send(new CancelIntegrationJobCommand(tenantId, jobId, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("tenants/{tenantId:guid}/jobs/{jobId:guid}/replay")]
    [Authorize(Policy = AuthorizationPolicies.IntegrationsManage)]
    public async Task<IActionResult> ReplayJob(
        Guid tenantId,
        Guid jobId,
        [FromBody] ReplayIntegrationJobRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new ReplayIntegrationJobCommand(tenantId, jobId, request.IdempotencyKey), cancellationToken);
        return CreatedAtAction(nameof(GetJobDetail), new { tenantId, jobId = id }, new { id });
    }

    [HttpPost("tenants/{tenantId:guid}/webhooks")]
    [Authorize(Policy = AuthorizationPolicies.CrmWebhooksManage)]
    public async Task<IActionResult> RegisterWebhook(
        Guid tenantId,
        [FromBody] RegisterWebhookRequest request,
        CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new RegisterWebhookCommand(
                tenantId,
                request.Name,
                request.EventKey,
                request.TargetUrl,
                request.SecretKey,
                request.TimeoutSeconds,
                request.MaxAttempts),
            cancellationToken);
        return CreatedAtAction(nameof(GetOverview), new { tenantId }, new { id });
    }

    [HttpPost("tenants/{tenantId:guid}/webhook-deliveries/{providerKey}/validate")]
    [Authorize(Policy = AuthorizationPolicies.IntegrationsManage)]
    public async Task<IActionResult> ValidateWebhookDelivery(
        Guid tenantId,
        string providerKey,
        [FromBody] ValidateWebhookDeliveryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ValidateWebhookDeliveryCommand(
                tenantId,
                providerKey,
                request.EventId,
                request.Timestamp,
                request.Signature,
                request.PayloadJson),
            cancellationToken);

        return Ok(result);
    }

    public sealed record UpsertIntegrationConnectionRequest(string ProviderKey, string DisplayName, string Category, string SettingsJson, [property: JsonRequired] bool IsEnabled);

    public sealed record ScheduleIntegrationJobRequest(
        string JobType,
        string Direction,
        string PayloadJson,
        [property: JsonRequired] DateTime ScheduledAtUtc,
        string? ProviderKey,
        string? IdempotencyKey,
        int? MaxAttempts);

    public sealed record RegisterWebhookRequest(string Name, string EventKey, string TargetUrl, string SecretKey, [property: JsonRequired] int TimeoutSeconds = 10, [property: JsonRequired] int MaxAttempts = 3);

    public sealed record CancelIntegrationJobRequest(string? Reason);

    public sealed record ReplayIntegrationJobRequest(string? IdempotencyKey);

    public sealed record ValidateWebhookDeliveryRequest(string EventId, string Timestamp, string Signature, string PayloadJson);

    [HttpGet("tenants/{tenantId:guid}/api-keys")]
    [Authorize(Policy = AuthorizationPolicies.CrmApiKeysRead)]
    public async Task<IActionResult> ListApiKeys(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var nowUtc = DateTime.UtcNow;
        var keys = await dbContext.IntegrationApiKeys
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                x.KeyPrefix,
                scopes = x.Scopes.Select(s => s.Scope),
                x.CreatedAt,
                x.CreatedBy,
                x.ExpiresAtUtc,
                x.LastUsedAtUtc,
                x.RevokedAtUtc,
                status = x.RevokedAtUtc.HasValue ? "revoked" : (x.ExpiresAtUtc.HasValue && x.ExpiresAtUtc.Value <= nowUtc ? "expired" : "active")
            })
            .ToListAsync(cancellationToken);

        return Ok(keys);
    }

    [HttpPost("tenants/{tenantId:guid}/api-keys")]
    [Authorize(Policy = AuthorizationPolicies.CrmApiKeysManage)]
    public async Task<IActionResult> CreateApiKey(Guid tenantId, [FromBody] CreateApiKeyRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var generated = ApiKeyHashingService.CreateNew();
        var entity = new IntegrationApiKey(
            tenantId,
            request.Name,
            request.Description,
            generated.Prefix,
            generated.Salt,
            generated.Hash,
            request.Scopes ?? [],
            request.ExpiresAtUtc);

        await dbContext.IntegrationApiKeys.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            id = entity.Id,
            apiKey = generated.PlaintextKey,
            keyPrefix = entity.KeyPrefix,
            status = "active",
            createdAtUtc = entity.CreatedAt
        });
    }

    [HttpDelete("tenants/{tenantId:guid}/api-keys/{apiKeyId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CrmApiKeysManage)]
    public async Task<IActionResult> RevokeApiKey(Guid tenantId, Guid apiKeyId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var apiKey = await dbContext.IntegrationApiKeys
            .FirstOrDefaultAsync(x => x.Id == apiKeyId && x.TenantId == tenantId, cancellationToken);

        if (apiKey is null)
        {
            return NotFound();
        }

        apiKey.Revoke(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("tenants/{tenantId:guid}/webhooks")]
    [Authorize(Policy = AuthorizationPolicies.CrmWebhooksRead)]
    public async Task<IActionResult> ListWebhookSubscriptions(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var rows = await dbContext.WebhookSubscriptions
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.EventKey,
                targetUrl = MaskUrl(x.TargetUrl),
                secret = Mask(x.SecretKey),
                x.IsEnabled,
                x.TimeoutSeconds,
                x.MaxAttempts,
                x.CreatedAt,
                x.UpdatedAt,
                x.LastSuccessAtUtc,
                x.LastFailureAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpPatch("tenants/{tenantId:guid}/webhooks/{webhookId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CrmWebhooksManage)]
    public async Task<IActionResult> UpdateWebhookSubscription(Guid tenantId, Guid webhookId, [FromBody] UpdateWebhookRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var entity = await dbContext.WebhookSubscriptions.FirstOrDefaultAsync(x => x.Id == webhookId && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Reconfigure(request.Name, request.EventKey, request.TargetUrl, request.SecretKey, request.IsEnabled, request.TimeoutSeconds, request.MaxAttempts);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("tenants/{tenantId:guid}/webhooks/{webhookId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CrmWebhooksManage)]
    public async Task<IActionResult> DeleteWebhookSubscription(Guid tenantId, Guid webhookId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var entity = await dbContext.WebhookSubscriptions.FirstOrDefaultAsync(x => x.Id == webhookId && x.TenantId == tenantId, cancellationToken);
        if (entity is null)
        {
            return NotFound();
        }

        entity.Disable();
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("tenants/{tenantId:guid}/webhooks/{webhookId:guid}/test")]
    [Authorize(Policy = AuthorizationPolicies.CrmWebhooksManage)]
    public async Task<IActionResult> TestWebhookSubscription(Guid tenantId, Guid webhookId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var webhook = await dbContext.WebhookSubscriptions.FirstOrDefaultAsync(x => x.Id == webhookId && x.TenantId == tenantId, cancellationToken);
        if (webhook is null)
        {
            return NotFound();
        }

        var eventId = $"evt_{Guid.NewGuid():N}";
        var payloadObject = new
        {
            eventId,
            eventType = webhook.EventKey,
            tenantId,
            occurredAtUtc = DateTime.UtcNow
        };

        var payload = JsonSerializer.Serialize(payloadObject);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var signature = services.WebhookSecurityService.CreateSignature(webhook.SecretKey, payload, timestamp);

        var delivery = new WebhookDeliveryAttempt(tenantId, webhook.Id, eventId, webhook.EventKey, DateTime.UtcNow);
        await dbContext.WebhookDeliveryAttempts.AddAsync(delivery, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(webhook.TimeoutSeconds));

            var client = services.HttpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Post, webhook.TargetUrl)
            {
                Content = JsonContent.Create(payloadObject)
            };
            request.Headers.Add("X-NetMetric-Event-Id", eventId);
            request.Headers.Add("X-NetMetric-Event-Type", webhook.EventKey);
            request.Headers.Add("X-NetMetric-Tenant-Id", tenantId.ToString("D"));
            request.Headers.Add("X-NetMetric-Timestamp", timestamp);
            request.Headers.Add("X-NetMetric-Signature", $"sha256={signature}");

            using var response = await client.SendAsync(request, cts.Token);
            if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
            {
                delivery.MarkDelivered((int)response.StatusCode, DateTime.UtcNow);
                webhook.MarkDeliveryResult(success: true, DateTime.UtcNow);
            }
            else
            {
                delivery.MarkFailed((int)response.StatusCode, $"http_{(int)response.StatusCode}", DateTime.UtcNow);
                webhook.MarkDeliveryResult(success: false, DateTime.UtcNow);
            }
        }
        catch (OperationCanceledException)
        {
            delivery.MarkFailed(null, "timeout", DateTime.UtcNow);
            webhook.MarkDeliveryResult(success: false, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            delivery.MarkFailed(null, ex.GetType().Name, DateTime.UtcNow);
            webhook.MarkDeliveryResult(success: false, DateTime.UtcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new { deliveryId = delivery.Id, status = delivery.Status, attempts = delivery.AttemptCount });
    }

    [HttpGet("tenants/{tenantId:guid}/webhooks/{webhookId:guid}/deliveries")]
    [Authorize(Policy = AuthorizationPolicies.CrmWebhooksRead)]
    public async Task<IActionResult> ListWebhookDeliveries(Guid tenantId, Guid webhookId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var exists = await dbContext.WebhookSubscriptions.AnyAsync(x => x.Id == webhookId && x.TenantId == tenantId, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        var rows = await dbContext.WebhookDeliveryAttempts
            .Where(x => x.TenantId == tenantId && x.WebhookSubscriptionId == webhookId)
            .OrderByDescending(x => x.TriggeredAtUtc)
            .Take(100)
            .Select(x => new
            {
                x.Id,
                x.EventId,
                x.EventType,
                x.Status,
                x.AttemptCount,
                x.HttpStatusCode,
                x.LastErrorSummary,
                x.TriggeredAtUtc,
                x.LastAttemptAtUtc
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("tenants/{tenantId:guid}/providers/mock")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsRead)]
    public async Task<IActionResult> GetMockProvider(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var credential = await dbContext.ProviderCredentials
            .Where(x => x.TenantId == tenantId && x.ProviderKey == "mock")
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (credential is null)
        {
            return Ok(new { connected = false });
        }

        return Ok(new
        {
            connected = true,
            id = credential.Id,
            providerKey = credential.ProviderKey,
            displayName = credential.DisplayName,
            endpointKey = credential.EndpointKey,
            isEnabled = credential.IsEnabled,
            status = credential.IsEnabled ? "connected" : "disabled",
            webhookPath = $"/api/integrations/webhooks/mock/{credential.EndpointKey}",
            maskedToken = Mask(credential.AccessToken),
            maskedSigningSecret = Mask(credential.WebhookSigningSecret)
        });
    }

    [HttpPut("tenants/{tenantId:guid}/providers/mock")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsManage)]
    public async Task<IActionResult> UpsertMockProvider(Guid tenantId, [FromBody] UpsertMockProviderRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var credential = await dbContext.ProviderCredentials
            .Where(x => x.TenantId == tenantId && x.ProviderKey == "mock")
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (credential is null)
        {
            credential = new ProviderCredential(tenantId, "mock", request.DisplayName, request.AccessToken, request.WebhookSigningSecret);
            await dbContext.ProviderCredentials.AddAsync(credential, cancellationToken);
        }
        else
        {
            credential.Reconfigure(request.DisplayName, request.AccessToken, request.WebhookSigningSecret, request.IsEnabled);
        }

        var validation = services.ProviderCredentialValidator.Validate(new ProviderValidationInput(
            "mock",
            request.DisplayName,
            request.AccessToken,
            request.WebhookSigningSecret,
            null));
        credential.RecordValidation(validation.Status, validation.Code, validation.Message, DateTime.UtcNow);
        credential.SetStatus(request.IsEnabled ? validation.Status : ProviderCredentialStatuses.Disabled);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new { id = credential.Id, endpointKey = credential.EndpointKey, isEnabled = credential.IsEnabled });
    }

    [HttpGet("tenants/{tenantId:guid}/providers/catalog")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsRead)]
    public IActionResult GetProviderCatalog(Guid tenantId)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        return Ok(services.ProviderCatalog.List());
    }

    [HttpGet("tenants/{tenantId:guid}/providers")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsRead)]
    public async Task<IActionResult> ListProviders(Guid tenantId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var catalog = services.ProviderCatalog.List().ToDictionary(x => x.ProviderKey, StringComparer.OrdinalIgnoreCase);
        var credentials = await dbContext.ProviderCredentials
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var rows = credentials.Select(x =>
        {
            catalog.TryGetValue(x.ProviderKey, out var provider);
            return new
            {
                x.Id,
                providerKey = x.ProviderKey,
                displayName = x.DisplayName,
                endpointKey = x.EndpointKey,
                webhookPath = BuildWebhookPath(x.ProviderKey, x.EndpointKey),
                isEnabled = x.IsEnabled,
                status = x.Status,
                lastValidationStatus = x.LastValidationStatus,
                lastValidationCode = x.LastValidationCode,
                lastValidationMessage = x.LastValidationMessage,
                lastValidatedAtUtc = x.LastValidatedAtUtc,
                createdAtUtc = x.CreatedAt,
                updatedAtUtc = x.UpdatedAt,
                productionReady = provider?.ProductionReady ?? false,
                maskedToken = Mask(x.AccessToken),
                maskedSigningSecret = Mask(x.WebhookSigningSecret)
            };
        });

        return Ok(rows);
    }

    [HttpGet("tenants/{tenantId:guid}/providers/{providerCredentialId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsRead)]
    public async Task<IActionResult> GetProvider(Guid tenantId, Guid providerCredentialId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var credential = await dbContext.ProviderCredentials
            .FirstOrDefaultAsync(x => x.Id == providerCredentialId && x.TenantId == tenantId, cancellationToken);
        if (credential is null)
        {
            return NotFound();
        }

        return Ok(new
        {
            credential.Id,
            credential.ProviderKey,
            credential.DisplayName,
            credential.EndpointKey,
            webhookPath = BuildWebhookPath(credential.ProviderKey, credential.EndpointKey),
            credential.IsEnabled,
            credential.Status,
            credential.LastValidationStatus,
            credential.LastValidationCode,
            credential.LastValidationMessage,
            credential.LastValidatedAtUtc,
            credential.CreatedAt,
            credential.UpdatedAt,
            maskedToken = Mask(credential.AccessToken),
            maskedSigningSecret = Mask(credential.WebhookSigningSecret)
        });
    }

    [HttpPut("tenants/{tenantId:guid}/provider-connections/{providerKey}")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsManage)]
    public async Task<IActionResult> UpsertProvider(Guid tenantId, string providerKey, [FromBody] UpsertProviderRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var normalizedProviderKey = providerKey.Trim().ToLowerInvariant();
        var catalogItem = services.ProviderCatalog.Find(normalizedProviderKey);
        if (catalogItem is null)
        {
            return NotFound(new { errorCode = "provider_not_found" });
        }

        var validation = services.ProviderCredentialValidator.Validate(new ProviderValidationInput(
            normalizedProviderKey,
            request.DisplayName,
            request.AccessToken,
            request.WebhookSigningSecret,
            request.ConfigurationJson));

        var credential = await dbContext.ProviderCredentials
            .Where(x => x.TenantId == tenantId && x.ProviderKey == normalizedProviderKey)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (credential is null)
        {
            credential = new ProviderCredential(
                tenantId,
                normalizedProviderKey,
                request.DisplayName,
                request.AccessToken ?? string.Empty,
                request.WebhookSigningSecret ?? string.Empty);
            credential.SetStatus(validation.Status);
            credential.RecordValidation(validation.Status, validation.Code, validation.Message, DateTime.UtcNow);
            credential.Reconfigure(request.DisplayName, request.AccessToken ?? string.Empty, request.WebhookSigningSecret ?? string.Empty, request.IsEnabled, request.ConfigurationJson);
            await dbContext.ProviderCredentials.AddAsync(credential, cancellationToken);
        }
        else
        {
            credential.Reconfigure(request.DisplayName, request.AccessToken ?? credential.AccessToken, request.WebhookSigningSecret ?? credential.WebhookSigningSecret, request.IsEnabled, request.ConfigurationJson);
            credential.RecordValidation(validation.Status, validation.Code, validation.Message, DateTime.UtcNow);
            credential.SetStatus(request.IsEnabled ? validation.Status : ProviderCredentialStatuses.Disabled);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            credential.Id,
            credential.ProviderKey,
            credential.DisplayName,
            credential.EndpointKey,
            webhookPath = BuildWebhookPath(credential.ProviderKey, credential.EndpointKey),
            credential.IsEnabled,
            credential.Status,
            credential.LastValidationStatus,
            credential.LastValidationCode,
            credential.LastValidationMessage,
            credential.LastValidatedAtUtc,
            validation.IsValid,
            validation.Code,
            validation.Message
        });
    }

    [HttpPost("tenants/{tenantId:guid}/providers/{providerCredentialId:guid}/test")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsManage)]
    public async Task<IActionResult> TestProvider(Guid tenantId, Guid providerCredentialId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var credential = await dbContext.ProviderCredentials
            .FirstOrDefaultAsync(x => x.Id == providerCredentialId && x.TenantId == tenantId, cancellationToken);
        if (credential is null)
        {
            return NotFound(new { errorCode = "provider_not_found" });
        }

        var result = services.ProviderConnectionTester.Test(new ProviderValidationInput(
            credential.ProviderKey,
            credential.DisplayName,
            credential.AccessToken,
            credential.WebhookSigningSecret,
            credential.ConfigurationJson));

        credential.RecordValidation(result.Status, result.Code, result.Message, DateTime.UtcNow);
        credential.SetStatus(credential.IsEnabled ? result.Status : ProviderCredentialStatuses.Disabled);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(result);
    }

    [HttpPost("tenants/{tenantId:guid}/providers/{providerCredentialId:guid}/disable")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsManage)]
    public async Task<IActionResult> DisableProvider(Guid tenantId, Guid providerCredentialId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var credential = await dbContext.ProviderCredentials
            .FirstOrDefaultAsync(x => x.Id == providerCredentialId && x.TenantId == tenantId, cancellationToken);
        if (credential is null)
        {
            return NotFound();
        }

        credential.Reconfigure(credential.DisplayName, credential.AccessToken, credential.WebhookSigningSecret, false, credential.ConfigurationJson);
        credential.SetStatus(ProviderCredentialStatuses.Disabled);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("tenants/{tenantId:guid}/providers/{providerCredentialId:guid}/enable")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsManage)]
    public async Task<IActionResult> EnableProvider(Guid tenantId, Guid providerCredentialId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var credential = await dbContext.ProviderCredentials
            .FirstOrDefaultAsync(x => x.Id == providerCredentialId && x.TenantId == tenantId, cancellationToken);
        if (credential is null)
        {
            return NotFound();
        }

        credential.Reconfigure(credential.DisplayName, credential.AccessToken, credential.WebhookSigningSecret, true, credential.ConfigurationJson);
        var validation = services.ProviderCredentialValidator.Validate(new ProviderValidationInput(
            credential.ProviderKey,
            credential.DisplayName,
            credential.AccessToken,
            credential.WebhookSigningSecret,
            credential.ConfigurationJson));
        credential.RecordValidation(validation.Status, validation.Code, validation.Message, DateTime.UtcNow);
        credential.SetStatus(validation.Status);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("tenants/{tenantId:guid}/providers/{providerCredentialId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CrmIntegrationsManage)]
    public async Task<IActionResult> DeleteProvider(Guid tenantId, Guid providerCredentialId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId != tenantId)
        {
            return Forbid();
        }

        var credential = await dbContext.ProviderCredentials
            .FirstOrDefaultAsync(x => x.Id == providerCredentialId && x.TenantId == tenantId, cancellationToken);
        if (credential is null)
        {
            return NotFound();
        }

        credential.Reconfigure(credential.DisplayName, credential.AccessToken, credential.WebhookSigningSecret, false, credential.ConfigurationJson);
        credential.SetStatus(ProviderCredentialStatuses.Disabled);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    public sealed record UpsertMockProviderRequest(string DisplayName, string AccessToken, string WebhookSigningSecret, bool IsEnabled = true);
    public sealed record UpsertProviderRequest(string DisplayName, string? AccessToken, string? WebhookSigningSecret, bool IsEnabled = true, string? ConfigurationJson = null);
    public sealed record CreateApiKeyRequest(string Name, string? Description, string[]? Scopes, DateTime? ExpiresAtUtc);
    public sealed record UpdateWebhookRequest(string Name, string EventKey, string TargetUrl, string SecretKey, [property: JsonRequired] bool IsEnabled, [property: JsonRequired] int TimeoutSeconds = 10, [property: JsonRequired] int MaxAttempts = 3);

    private static string Mask(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "****";
        }

        var trimmed = value.Trim();
        if (trimmed.Length <= 6)
        {
            return "****";
        }

        return $"{trimmed[..3]}***{trimmed[^3..]}";
    }

    private static string MaskUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return "****";
        }

        return $"{uri.Scheme}://{uri.Host}/***";
    }

    private static string BuildWebhookPath(string providerKey, string endpointKey)
    {
        var normalized = providerKey.Trim().ToLowerInvariant();
        if (normalized is "whatsapp" or "instagram")
        {
            return $"/api/integrations/webhooks/meta/{endpointKey}";
        }

        return $"/api/integrations/webhooks/{normalized}/{endpointKey}";
    }
}
