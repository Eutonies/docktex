namespace Docktex.Executor.Model;

public abstract record ExecutionOutput(
    );


public record ExecutionSuccesOutput(
    string FileName,
    byte[] FileBytes
    ) : ExecutionOutput;


public record ExecutionErrorOutput(
    IReadOnlyCollection<string> InfoOutput,
    IReadOnlyCollection<string> ErrorOutput
    ) : ExecutionOutput;

