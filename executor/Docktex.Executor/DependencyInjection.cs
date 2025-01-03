using Docktex.Executor.Configuration;
using Docktex.Executor.Services;

namespace Docktex.Executor;

public static class DependencyInjection
{

    public static WebApplicationBuilder AddExecutorConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ExecutorConfiguration>(builder.Configuration.GetSection(ExecutorConfiguration.ConfigurationElementName));
        return builder;
    }


    public static WebApplicationBuilder AddExecutor(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        builder.Services.AddSingleton<IExecutionService, ExecutionService>();
        return builder;
    }

    public static WebApplication UseExecutor(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }


}
