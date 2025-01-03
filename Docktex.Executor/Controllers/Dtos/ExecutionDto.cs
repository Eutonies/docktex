using Docktex.Executor.Model;

namespace Docktex.Executor.Controllers.Dtos;

public record ExecutionDto(
    long ExecutionId,
    IReadOnlyCollection<string> TexFiles,
    IReadOnlyCollection<string> FontFiles
    );

public static class ExecutionDtoExtensions
{
    public static ExecutionDto ToDto(this Execution execution) => new ExecutionDto(
        ExecutionId: execution.Id,
        TexFiles: execution.TexFiles,
        FontFiles: execution.FontFiles
    );

}
