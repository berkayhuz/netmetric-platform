// <copyright file="InMemoryToolJobQueue.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Collections.Concurrent;
using MediatR;
using NetMetric.Tools.Application.History.Commands;
using NetMetric.Tools.Contracts.History;
using NetMetric.Tools.Contracts.Jobs;

namespace NetMetric.Tools.API.Jobs;

public interface IToolJobQueue
{
    Task<CreateToolJobResponse> EnqueueAsync(CreateToolRunRequest request, byte[] content, long length, CancellationToken cancellationToken);
    bool TryGet(Guid jobId, out ToolJobStatusResponse response);
}

public sealed class InMemoryToolJobQueue(IMediator mediator) : BackgroundService, IToolJobQueue
{
    private readonly ConcurrentQueue<(Guid JobId, CreateToolRunRequest Request, byte[] Content, long Length)> _queue = new();
    private readonly ConcurrentDictionary<Guid, ToolJobStatusResponse> _jobs = new();

    public Task<CreateToolJobResponse> EnqueueAsync(CreateToolRunRequest request, byte[] content, long length, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var created = DateTimeOffset.UtcNow;
        _jobs[id] = new ToolJobStatusResponse(id, ToolJobStatus.Queued, 0, null, null, created);
        _queue.Enqueue((id, request, content, length));
        return Task.FromResult(new CreateToolJobResponse(id, ToolJobStatus.Queued, created));
    }

    public bool TryGet(Guid jobId, out ToolJobStatusResponse response) => _jobs.TryGetValue(jobId, out response!);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (!_queue.TryDequeue(out var item))
            {
                await Task.Delay(100, stoppingToken);
                continue;
            }

            _jobs[item.JobId] = _jobs[item.JobId] with { Status = ToolJobStatus.Running, ProgressPercent = 10, UpdatedAtUtc = DateTimeOffset.UtcNow };
            try
            {
                await using var ms = new MemoryStream(item.Content);
                var result = await mediator.Send(new CreateMyToolRunCommand(item.Request, ms, item.Length), stoppingToken);
                _jobs[item.JobId] = _jobs[item.JobId] with { Status = ToolJobStatus.Succeeded, ProgressPercent = 100, RunId = result.RunId, UpdatedAtUtc = DateTimeOffset.UtcNow };
            }
            catch (Exception ex)
            {
                _jobs[item.JobId] = _jobs[item.JobId] with { Status = ToolJobStatus.Failed, Error = ex.Message, UpdatedAtUtc = DateTimeOffset.UtcNow };
            }
        }
    }
}
