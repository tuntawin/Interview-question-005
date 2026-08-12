namespace QueueApp.Api.Models;

public record QueueGenerateResponse(
    string QueueCode,
    int CurrentIndex,
    DateTime GeneratedAt
);

public record QueueResetResponse(
    string Message,
    int CurrentIndex
);

public record QueueCurrentResponse(
    string QueueCode,
    int CurrentIndex,
    DateTime? LastActive
);

