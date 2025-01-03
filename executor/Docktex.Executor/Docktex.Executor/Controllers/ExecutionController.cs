using Docktex.Executor.Controllers.Dtos;
using Docktex.Executor.Model;
using Docktex.Executor.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Docktex.Executor.Controllers;

[Route("{controller}")]
public class ExecutionController : ControllerBase
{
    [HttpPost("upload")]
    public async Task UploadFile([FromQuery] long executionId, [FromBody] FormFile file, [FromServices] IExecutionService executionService)
    {
        using (var stream = new MemoryStream()) 
        {
            await file.CopyToAsync(stream);
            var bytes = stream.ToArray();
            await executionService.AddFileToExecution(executionId, file.FileName, bytes);
        }
    }

    [HttpPost("create-execution")]
    public async Task<Ok<ExecutionDto>> CreateNewExecution([FromServices] IExecutionService executionService)
    {
        var execution = await executionService.CreateNewExecution();
        var returnee = execution.ToDto();
        return TypedResults.Ok(returnee);
    }

    [HttpGet("executions")]
    public async Task<Ok<IReadOnlyCollection<ExecutionDto>>> LoadExecutions([FromServices] IExecutionService executionService)
    {
        var executions = await executionService.LoadExecutions();
        IReadOnlyCollection<ExecutionDto> returnee = executions
            .Select(_ => _.ToDto())
            .ToList();
        return TypedResults.Ok(returnee);
    }

    [HttpGet("execution")]
    public async Task<Results<BadRequest<string>,Ok<ExecutionDto>>> LoadExecution([FromQuery] long executionId, [FromServices] IExecutionService executionService)
    {
        
        var execution = await executionService.LoadExecution(executionId);
        if (execution == null)
            return TypedResults.BadRequest($"No execution with ID {executionId} exists");
        var returnee = execution.ToDto();
        return TypedResults.Ok(returnee);
    }

    [HttpPost("execute")]
    public async Task<Results<InternalServerError<ExecutionOutputDto>,Ok<ExecutionOutputDto>>> Execute(
        [FromServices] IExecutionService executionService, 
        [FromQuery] long executionId, 
        [FromQuery] string mainTexFile)
    {
        var executionResult = await executionService.ExecuteFile(executionId, mainTexFile);
        if (executionResult is ExecutionSuccesOutput succ)
            return TypedResults.Ok(new ExecutionOutputDto(
                FileName: succ.FileName,
                FileBytes: succ.FileBytes
            ));
        var error = (executionResult as ExecutionErrorOutput)!;
        var returnee = new ExecutionOutputDto(
            InfoOutput: error.InfoOutput,
            ErrorOutput: error.ErrorOutput
            );
        return TypedResults.InternalServerError(returnee);
    }




}
