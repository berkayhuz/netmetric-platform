using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTaskById;

public sealed record GetWorkTaskByIdQuery(Guid TaskId) : IRequest<WorkTaskDto?>;
