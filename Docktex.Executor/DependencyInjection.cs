using Docktex.Executor.Configuration;
using Docktex.Executor.Controllers;
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
        builder.Services.AddControllers()
          .AddApplicationPart(typeof(ExecutionController).Assembly);
        builder.Services.AddOpenApi(opts => {
            opts.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
        });
        builder.Services.AddSingleton<IExecutionService, ExecutionService>();
        return builder;
    }

    public static WebApplication UseExecutor(this WebApplication app)
    {
        var conf = app.ExecutorConfig();
        if(!string.IsNullOrWhiteSpace(conf.HostingBasePath))
          app.UsePathBase(conf.HostingBasePath);
        app.UseRouting();
        app.MapOpenApi("openapi/v1.json");
        app.UseSwaggerUi(opts => {
            if(!string.IsNullOrWhiteSpace(conf.HostingBasePath)) {
                opts.Path = $"/{conf.HostingBasePath}";
            }
            opts.DocumentPath = "openapi/v1.json";
        });
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
