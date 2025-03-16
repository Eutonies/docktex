using Microsoft.Extensions.Configuration;

var configBuilder = new ConfigurationBuilder();
configBuilder
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.local.json", optional: true)
    .AddEnvironmentVariables();


