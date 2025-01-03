namespace Docktex.Executor.Controllers.Dtos;

public record ExecutionOutputDto(
    string? FileName = null,
    byte[]? FileBytes = null,
    IReadOnlyCollection<string>? InfoOutput = null,
    IReadOnlyCollection<string>? ErrorOutput = null
    );
