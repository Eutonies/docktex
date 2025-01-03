using Docktex.Executor;

var builder = WebApplication.CreateBuilder(args);
builder.AddExecutorConfiguration();
builder.AddExecutor();
var app = builder.Build();
app.UseExecutor();

app.Run();
