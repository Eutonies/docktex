using Docktex.Executor.Configuration;
using Docktex.Executor.Model;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Docktex.Executor.Services;

public class ExecutionService : IExecutionService
{
    private string _executionFolder;
    private string _luaLatexCommand;
    private readonly SemaphoreSlim _executionsLock = new SemaphoreSlim(1);

    private string ExecutionFolder(long executionId) => $"{_executionFolder}/{executionId}";
    private string ExecutionFile(long executionId, string fileName) => $"{ExecutionFolder(executionId)}/{fileName}"; 
    public ExecutionService(IOptions<ExecutorConfiguration> conf)
    {
        _executionFolder = conf.Value.ExecutionsPath;
        _luaLatexCommand = conf.Value.LuaLatexBaseCommand;
    }

    public async Task AddFileToExecution(long executionId, string fileName, byte[] data)
    {
        var executionFolder = ExecutionFolder(executionId);
       if (Directory.Exists(executionFolder))
        {
            var exFileName = ExecutionFile(executionId, fileName);
            if(File.Exists(exFileName))
                File.Delete(exFileName);
            await File.WriteAllBytesAsync(exFileName, data);
        }
    }

    public async Task<Execution> CreateNewExecution()
    {
        await _executionsLock.WaitAsync();
        var executionId = DateTime.Now.ToFileTime();
        try
        {
            while(Path.Exists(ExecutionFolder(executionId)))
            {
                executionId += 1;
            }
            Directory.CreateDirectory(ExecutionFolder(executionId));

        }
        finally
        {
            _executionsLock.Release();
        }
        return new Execution(Id: executionId, [], []);
    }

    public async Task<Execution?> LoadExecution(long id)
    {
        await Task.CompletedTask;
        if(Directory.Exists(ExecutionFolder(id)))
            return DefLoad(id);
        return null;
    }


    private static readonly Regex _idRegex = new Regex("([0-9]+)");
    public async Task<IReadOnlyCollection<Execution>> LoadExecutions()
    {
        await Task.CompletedTask;
        var allFolders = Directory.GetDirectories(_executionFolder)
            .Select(_ => (Matcher: _idRegex.Match(_), File: _))
            .Where(_ => _.Matcher.Groups.Count > 1)
            .Select(_ => (Id: long.Parse(_.Matcher.Groups[1].Value), File: _.File))
            .ToList();
        var executions = allFolders
            .Select(_ => DefLoad(_.Id))
            .OrderBy(_ => _.Id)
            .ToList();
        return executions;
    }

    private static readonly HashSet<string> _fontFileExtensions = new List<string> {
        "ttf",
        "otf",
        "woff",
        "woff2",
        "eot"
    }.ToHashSet();
    private Execution DefLoad(long id)
    {
        var folder = ExecutionFolder(id);
        var allFiles = Directory.GetFiles(folder);
        var texFiles = allFiles
            .Where(_ => _.ToLower().EndsWith(".tex"))
            .ToList();
        var fontFiles = allFiles
            .Where(fil => _fontFileExtensions.Any(_ => fil.ToLower().EndsWith(_)))
            .ToList();
        return new Execution(id, texFiles, fontFiles);    

    }

    public async Task<ExecutionOutput?> ExecuteFile(long executionId, string mainTexFile)
    {
        if (!mainTexFile.ToLower().EndsWith("tex"))
            return new ExecutionErrorOutput(
                InfoOutput: [],
                ErrorOutput: ["Execution file has to be .tex file"]
                );
        var fileToExecute = ExecutionFile(executionId, mainTexFile);
        if (!Path.Exists(fileToExecute))
            return new ExecutionErrorOutput(
                InfoOutput: [],
                ErrorOutput: ["TeX file to execute does not exist"]
                );
        var infoOutput = new List<string>();
        var errorOutput = new List<string>();
        var processStart = new ProcessStartInfo()
        {
            FileName = _luaLatexCommand,
            Arguments = $"--pdf {fileToExecute}",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        var process = new Process() { StartInfo = processStart };
        process.OutputDataReceived += (sender, data) => infoOutput.Add(data.Data ?? "");
        process.ErrorDataReceived += (sender, data) => errorOutput.Add(data.Data ?? "");
        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            var outputFile = mainTexFile.Substring(0, mainTexFile.Length - 3) + "pdf";
            if(!File.Exists(outputFile))
                return new ExecutionErrorOutput(
                                InfoOutput: infoOutput.ToList(),
                                ErrorOutput: errorOutput.ToList().Prepend(
                                    "Output PDF file does not exist"
                                    ).ToList()
                                );
            var fileBytes = await File.ReadAllBytesAsync(outputFile);
            var returnee = new ExecutionSuccesOutput(outputFile, fileBytes);
            return returnee;
        }
        catch (Exception ex)
        {
            var returnee = new ExecutionErrorOutput(
                InfoOutput: infoOutput.ToList(),
                ErrorOutput: errorOutput.ToList().Concat(
                    [ex.Message, ex.StackTrace ?? ""]
                    ).ToList()

                );
            return returnee;
        }

    }




}
