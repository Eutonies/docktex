using Docktex.Executor.Configuration;
using Docktex.Executor.Controllers;
using Docktex.Executor.Services;
using NSwag.AspNetCore;

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
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        if(!string.IsNullOrWhiteSpace(conf.HostingBasePath))
          app.UsePathBase("/" + conf.HostingBasePath);
        app.UseRouting();
        var openApiDocPath = "openapi/v1.json";
        if(!string.IsNullOrWhiteSpace(conf.HostingBasePath)) {
            openApiDocPath = $"/{conf.HostingBasePath}/{openApiDocPath}";
        }
        logger.LogInformation($"Will use OpenAPI document path: {openApiDocPath}");
        app.MapOpenApi(openApiDocPath);
        app.UseOpenApi(opts => {
            if(!string.IsNullOrWhiteSpace(conf.HostingBasePath)) {
                opts.PostProcess = (doc,_) => {doc.BasePath = $"/{conf.HostingBasePath}";};
            }
        });
        app.UseSwaggerUi(opts => {
           if(!string.IsNullOrWhiteSpace(conf.HostingBasePath)) {
                opts.SwaggerRoutes.Add(new SwaggerUiRoute("swagger", $"/{conf.HostingBasePath}/swagger"));
                logger.LogInformation($"Using swagger routes:");
                foreach(var rt in opts.SwaggerRoutes)
                   logger.LogInformation($"  {rt.Name}: {rt.Url}");
            }
             
            opts.DocumentPath = openApiDocPath;
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
