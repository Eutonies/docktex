using Docktex.Executor.Model;

namespace Docktex.Executor.Services;

public interface IExecutionService
{
    Task<IReadOnlyCollection<Execution>> LoadExecutions();
    Task<Execution> CreateNewExecution();
    Task AddFileToExecution(long executionId, string fileName, byte[] data);
    Task<Execution?> LoadExecution(long id);

    Task<ExecutionOutput?> ExecuteFile(long executionId, string mainTexFile);
}
