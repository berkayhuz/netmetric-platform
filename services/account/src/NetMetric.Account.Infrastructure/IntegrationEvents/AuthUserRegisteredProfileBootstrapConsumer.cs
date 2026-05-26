// <copyright file="AuthUserRegisteredProfileBootstrapConsumer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Auth.Contracts.IntegrationEvents;
using NetMetric.Clock;
using NetMetric.Localization;
using NetMetric.Messaging.RabbitMq.Connection;
using NetMetric.Messaging.RabbitMq.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NetMetric.Account.Infrastructure.IntegrationEvents;

public sealed class AuthUserRegisteredProfileBootstrapConsumer(
    IServiceScopeFactory scopeFactory,
    RabbitMqConnectionProvider connectionProvider,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IOptions<AuthProfileBootstrapOptions> options,
    IClock clock,
    ILogger<AuthUserRegisteredProfileBootstrapConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Auth user profile bootstrap consumer is disabled.");
            return;
        }

        var rabbitOptions = rabbitMqOptions.Value;
        var consumerOptions = options.Value;
        var connection = await connectionProvider.GetConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.ExchangeDeclareAsync(
            rabbitOptions.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            consumerOptions.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            consumerOptions.QueueName,
            rabbitOptions.Exchange,
            UserRegisteredV1.RoutingKey,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, rabbitOptions.PrefetchCount, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                var payload = Encoding.UTF8.GetString(eventArgs.Body.Span);
                var message = JsonSerializer.Deserialize<UserRegisteredV1>(payload, SerializerOptions)
                    ?? throw new InvalidOperationException("UserRegisteredV1 payload could not be deserialized.");

                await EnsureProfileAsync(message, stoppingToken);
                await channel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: true, cancellationToken: CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Auth profile bootstrap processing failed. RoutingKey={RoutingKey} MessageId={MessageId}",
                    eventArgs.RoutingKey,
                    eventArgs.BasicProperties.MessageId);
                await channel.BasicNackAsync(eventArgs.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(consumerOptions.QueueName, autoAck: false, consumer, stoppingToken);
        logger.LogInformation("Auth user profile bootstrap consumer started. Queue={QueueName}", consumerOptions.QueueName);
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task EnsureProfileAsync(UserRegisteredV1 message, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var profiles = scope.ServiceProvider.GetRequiredService<IRepository<IAccountDbContext, UserProfile>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAccountDbContext>();
        var tenantId = TenantId.From(message.TenantId);
        var userId = UserId.From(message.UserId);

        var existing = await profiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var (firstName, lastName) = ResolveNames(message);
        var culture = NetMetricCultures.NormalizeOrDefault(message.Culture);
        var profile = UserProfile.Create(tenantId, userId, firstName, lastName, clock.UtcNow, culture);

        await profiles.AddAsync(profile, cancellationToken);
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            var duplicate = await profiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
            if (duplicate is null)
            {
                throw;
            }
        }
    }

    private static (string FirstName, string LastName) ResolveNames(UserRegisteredV1 message)
    {
        var firstName = NormalizeName(message.FirstName);
        var lastName = NormalizeName(message.LastName);
        if (firstName is not null && lastName is not null)
        {
            return (firstName, lastName);
        }

        var fallback = NormalizeName(message.UserName) ?? "Member";
        return (firstName ?? fallback, lastName ?? "Member");
    }

    private static string? NormalizeName(string? value)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Length <= 100 ? normalized : normalized[..100];
    }
}
