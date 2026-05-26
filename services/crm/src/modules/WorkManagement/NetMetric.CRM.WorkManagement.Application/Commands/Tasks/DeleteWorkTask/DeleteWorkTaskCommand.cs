using MediatR;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Tasks.DeleteWorkTask;

public sealed record DeleteWorkTaskCommand(Guid TaskId) : IRequest<bool>;
