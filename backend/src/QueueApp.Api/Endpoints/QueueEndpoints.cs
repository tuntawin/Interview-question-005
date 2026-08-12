using QueueApp.Api.Services;

namespace QueueApp.Api.Endpoints;

public static class QueueEndpoints
{
    public static void MapQueueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/queue");

        group.MapPost("/generate", async (IQueueService queueService, CancellationToken cancellationToken) =>
        {
            var result = await queueService.GenerateNextQueueAsync(cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GenerateQueue")
        .WithSummary("Generate Next Queue Ticket")
        .WithDescription("Generates the next sequential queue ticket code (A0 to Z9).");

        group.MapPost("/reset", async (IQueueService queueService, CancellationToken cancellationToken) =>
        {
            var result = await queueService.ResetQueueAsync(cancellationToken);
            return Results.Ok(result);
        })
        .WithName("ResetQueue")
        .WithSummary("Reset Queue Index")
        .WithDescription("Resets the queue sequence back to index -1.");

        group.MapGet("/current", async (IQueueService queueService, CancellationToken cancellationToken) =>
        {
            var result = await queueService.GetCurrentQueueAsync(cancellationToken);
            return Results.Ok(result);
        })
        .WithName("GetCurrentQueue")
        .WithSummary("Get Current Queue Ticket")
        .WithDescription("Gets the current active queue ticket code or 00 if reset.");
    }
}
