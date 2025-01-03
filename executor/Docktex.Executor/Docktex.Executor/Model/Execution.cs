namespace Docktex.Executor.Model;

public record Execution(
    long Id,
    IReadOnlyCollection<string> TexFiles,
    IReadOnlyCollection<string> FontFiles
    );
