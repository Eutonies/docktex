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
        builder.Services.AddOpenApiDocument();
        builder.Services.AddSingleton<IExecutionService, ExecutionService>();
        return builder;
    }

    public static WebApplication UseExecutor(this WebApplication app)
    {
        var conf = app.ExecutorConfig();
        if(!string.IsNullOrWhiteSpace(conf.HostingBasePath))
          app.UsePathBase(conf.HostingBasePath);
        app.UseRouting();
        app.MapOpenApi();
        app.UseSwaggerUi();
        app.MapControllers();
        return app;
    }

    public static ExecutorConfiguration ExecutorConfig(this WebApplication app) => app.Configuration.ExecutorConfig();
    public static ExecutorConfiguration ExecutorConfig(this IConfiguration config)
    {
        var returnee = new ExecutorConfiguration();
        config.Bind(ExecutorConfiguration.ConfigurationElementName, returnee);
        return returnee;
    }


}
