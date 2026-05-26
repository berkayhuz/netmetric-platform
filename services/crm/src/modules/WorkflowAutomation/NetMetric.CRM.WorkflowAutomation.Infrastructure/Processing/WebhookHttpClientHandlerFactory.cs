// <copyright file="WebhookHttpClientHandlerFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using System.Net.Sockets;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public static class WebhookHttpClientHandlerFactory
{
    public static readonly HttpRequestOptionsKey<IPAddress[]> ValidatedAddressesOption =
        new("NetMetric.CRM.WorkflowAutomation.ValidatedWebhookAddresses");

    public static SocketsHttpHandler Create(bool strictConnectionPinning = true) =>
        Create(strictConnectionPinning, ConnectToValidatedAddressAsync);

    public static SocketsHttpHandler Create(
        bool strictConnectionPinning,
        Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> connectCallback) =>
        new()
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            PooledConnectionLifetime = strictConnectionPinning ? TimeSpan.Zero : TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = strictConnectionPinning ? TimeSpan.Zero : TimeSpan.FromMinutes(1),
            ConnectCallback = connectCallback
        };

    public static void ApplyRequestConnectionPolicy(HttpRequestMessage request, bool strictConnectionPinning)
    {
        if (!strictConnectionPinning)
        {
            return;
        }

        request.Version = HttpVersion.Version11;
        request.VersionPolicy = HttpVersionPolicy.RequestVersionExact;
        request.Headers.ConnectionClose = true;
    }

    private static async ValueTask<Stream> ConnectToValidatedAddressAsync(
        SocketsHttpConnectionContext context,
        CancellationToken cancellationToken)
    {
        if (!context.InitialRequestMessage.Options.TryGetValue(ValidatedAddressesOption, out var addresses) ||
            addresses.Length == 0)
        {
            throw new HttpRequestException("Webhook outbound request is missing validated DNS addresses.");
        }

        var failures = new List<Exception>();
        foreach (var address in addresses)
        {
            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                NoDelay = true
            };

            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (Exception exception) when (exception is SocketException or OperationCanceledException)
            {
                failures.Add(exception);
                socket.Dispose();

                if (exception is OperationCanceledException)
                {
                    throw;
                }
            }
        }

        throw new HttpRequestException("Webhook outbound request could not connect to a validated address.", new AggregateException(failures));
    }
}
