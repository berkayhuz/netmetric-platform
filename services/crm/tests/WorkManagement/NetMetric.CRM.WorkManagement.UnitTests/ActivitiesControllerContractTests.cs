using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.API.Controllers.Activities;
using NetMetric.CRM.API.Contracts.Activities;
using NetMetric.CRM.API.Features.Activities;

namespace NetMetric.CRM.WorkManagement.UnitTests;

public sealed class ActivitiesControllerContractTests
{
    [Fact]
    public void RouteAttributes_Should_Expose_Related_Endpoint_With_Guid_Constraint()
    {
        var controllerRoute = typeof(ActivitiesController).GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .OfType<RouteAttribute>()
            .Single();
        controllerRoute.Template.Should().Be("api/activities");

        var relatedRoute = typeof(ActivitiesController).GetMethod(nameof(ActivitiesController.GetRelated))!
            .GetCustomAttributes(typeof(HttpMethodAttribute), inherit: true)
            .OfType<HttpMethodAttribute>()
            .Single();
        relatedRoute.Template.Should().Be("related/{entityType}/{entityId:guid}");

        var detailRoute = typeof(ActivitiesController).GetMethod(nameof(ActivitiesController.GetById))!
            .GetCustomAttributes(typeof(HttpMethodAttribute), inherit: true)
            .OfType<HttpMethodAttribute>()
            .Single();
        detailRoute.Template.Should().Be("{activityId:guid}");
    }

    [Fact]
    public async Task GetRelated_WhenEntityTypeUnsupported_ReturnsValidationProblem()
    {
        var controller = new ActivitiesController(new FakeTimelineReadService(), new FakeTimelineWriteService());

        var result = await controller.GetRelated("unsupported", Guid.NewGuid(), null, null, cancellationToken: CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var objectResult = (BadRequestObjectResult)result.Result!;
        objectResult.StatusCode.Should().Be(400);
        objectResult.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task GetRelated_WhenEntityTypeSupported_ReturnsOk()
    {
        var controller = new ActivitiesController(new FakeTimelineReadService(), new FakeTimelineWriteService());
        var entityId = Guid.NewGuid();

        var result = await controller.GetRelated("lead", entityId, null, null, cancellationToken: CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result.Result!;
        ok.Value.Should().BeOfType<ActivityTimelineFeedDto>();
    }

    [Theory]
    [InlineData("customer")]
    [InlineData("company")]
    [InlineData("contact")]
    [InlineData("lead")]
    [InlineData("deal")]
    [InlineData("opportunity")]
    [InlineData("quote")]
    [InlineData("ticket")]
    [InlineData("task")]
    public void EntityTypeParser_Supports_Canonical_Entity_Names(string value)
    {
        ActivityEntityTypeParser.TryParse(value, out var parsed).Should().BeTrue();
        Enum.IsDefined(parsed).Should().BeTrue();
    }

    [Fact]
    public async Task Create_WhenTaskValid_ReturnsOk()
    {
        var controller = new ActivitiesController(new FakeTimelineReadService(), new FakeTimelineWriteService());
        var result = await controller.Create(
            new CreateActivityRequestDto(
                Type: "task",
                OccurredAtUtc: DateTime.UtcNow,
                Title: "task title",
                Description: null,
                RelatedRecords:
                [
                    new CreateActivityRelatedRecordDto("lead", Guid.NewGuid(), "primary")
                ],
                Payload: new CreateActivityPayloadDto(null, null, null, null, null, null, null, null, null, null, null, DateTime.UtcNow.AddDays(1), "normal", null, null, null, null)),
            CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_WhenNoteValid_ReturnsOk()
    {
        var controller = new ActivitiesController(new FakeTimelineReadService(), new FakeTimelineWriteService());
        var relatedId = Guid.NewGuid();
        var result = await controller.Create(
            new CreateActivityRequestDto(
                Type: "note",
                OccurredAtUtc: DateTime.UtcNow,
                Title: "note title",
                Description: "desc",
                RelatedRecords:
                [
                    new CreateActivityRelatedRecordDto("lead", relatedId, "primary")
                ],
                Payload: new CreateActivityPayloadDto("note body", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)),
            CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result.Result!).Value.Should().BeOfType<CreateActivityResponseDto>();
    }

    [Fact]
    public async Task Create_WhenMeetingValid_ReturnsOk()
    {
        var controller = new ActivitiesController(new FakeTimelineReadService(), new FakeTimelineWriteService());
        var result = await controller.Create(
            new CreateActivityRequestDto(
                Type: "meeting",
                OccurredAtUtc: DateTime.UtcNow,
                Title: "meeting title",
                Description: null,
                RelatedRecords:
                [
                    new CreateActivityRelatedRecordDto("lead", Guid.NewGuid(), "primary")
                ],
                Payload: new CreateActivityPayloadDto(null, null, null, null, null, null, null, null, null, null, null, null, null, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), null, null)),
            CancellationToken.None);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_WhenUnsupportedType_ReturnsValidationProblem()
    {
        var controller = new ActivitiesController(new FakeTimelineReadService(), new FakeTimelineWriteService());
        var result = await controller.Create(
            new CreateActivityRequestDto(
                Type: "unknown",
                OccurredAtUtc: DateTime.UtcNow,
                Title: "unknown",
                Description: null,
                RelatedRecords:
                [
                    new CreateActivityRelatedRecordDto("lead", Guid.NewGuid(), "primary")
                ],
                Payload: new CreateActivityPayloadDto(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)),
            CancellationToken.None);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public void Create_ShouldRequire_ActivitiesCreatePolicy()
    {
        var postRoute = typeof(ActivitiesController).GetMethod(nameof(ActivitiesController.Create))!;
        var authorizeAttributes = postRoute
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true)
            .OfType<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
            .ToList();

        authorizeAttributes.Should().ContainSingle(x => x.Policy == AuthorizationPolicies.ActivitiesCreate);
    }

    private sealed class FakeTimelineReadService : IActivityTimelineReadService
    {
        public Task<ActivityTimelineItemDto?> GetByIdAsync(Guid activityId, CancellationToken cancellationToken) =>
            Task.FromResult<ActivityTimelineItemDto?>(null);

        public Task<ActivityTimelineFeedDto> GetGlobalAsync(string? type, string? sourceModule, Guid? ownerUserId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize, CancellationToken cancellationToken) =>
            Task.FromResult(new ActivityTimelineFeedDto([], 0, Math.Max(1, page), Math.Clamp(pageSize, 1, 200)));

        public Task<ActivityTimelineFeedDto> GetRelatedAsync(ActivityEntityType entityType, Guid entityId, DateTime? fromUtc, DateTime? toUtc, int page, int pageSize, CancellationToken cancellationToken) =>
            Task.FromResult(new ActivityTimelineFeedDto(
                [new ActivityTimelineItemDto(
                    Id: $"{entityType}:{entityId}",
                    OccurredAtUtc: DateTime.UtcNow,
                    Type: "test",
                    Title: "test",
                    Description: null,
                    Status: null,
                    SourceModule: "test",
                    SourceEntityType: entityType.ToString().ToLowerInvariant(),
                    SourceEntityId: entityId,
                    ActorUserId: null,
                    OwnerUserId: null,
                    RelatedRecords: [new ActivityRelatedRecordDto(entityType.ToString().ToLowerInvariant(), entityId, null, "subject")],
                    Metadata: new Dictionary<string, string?>())],
                1,
                Math.Max(1, page),
                Math.Clamp(pageSize, 1, 200)));
    }

    private sealed class FakeTimelineWriteService : IActivityTimelineWriteService
    {
        public Task<CreateActivityResponseDto> CreateAsync(CreateActivityRequestDto request, CancellationToken cancellationToken)
        {
            if (string.Equals(request.Type, "unknown", StringComparison.OrdinalIgnoreCase))
            {
                throw new ActivityValidationException("type", "Unsupported type.");
            }

            var related = request.RelatedRecords![0];
            var now = DateTime.UtcNow;
            var item = new ActivityTimelineItemDto(
                Id: Guid.NewGuid().ToString(),
                OccurredAtUtc: now,
                Type: request.Type,
                Title: request.Title ?? "auto",
                Description: request.Description,
                Status: null,
                SourceModule: "work-management",
                SourceEntityType: related.EntityType,
                SourceEntityId: related.EntityId,
                ActorUserId: null,
                OwnerUserId: null,
                RelatedRecords: [new ActivityRelatedRecordDto(related.EntityType, related.EntityId, null, related.RelationRole)],
                Metadata: new Dictionary<string, string?>());
            return Task.FromResult(new CreateActivityResponseDto(
                Guid.NewGuid(),
                request.Type,
                now,
                related.EntityType,
                related.EntityId,
                item));
        }
    }
}
