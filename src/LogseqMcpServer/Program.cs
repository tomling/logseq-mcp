using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http.Resilience;
using ModelContextProtocol.Server;
using LogseqMcpServer.Configuration;
using LogseqMcpServer.Services;

// Load configuration
LogseqConfiguration? config = null;
try
{
    config = LogseqConfiguration.LoadFromEnvironment();
    config.Validate();
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Configuration error: {ex.Message}");
    return 1;
}

// Create host builder
var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr
builder.Logging.ClearProviders();
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register configuration as singleton
builder.Services.AddSingleton(config);

// Configure HttpClient with IHttpClientFactory and retry policies
builder.Services.AddHttpClient<LogseqHttpClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<LogseqConfiguration>();
    client.BaseAddress = new Uri(configuration.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.AuthToken}");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddStandardResilienceHandler(options =>
{
    // Configure retry policy for transient failures
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    options.Retry.UseJitter = true;

    // Configure circuit breaker
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);

    // Configure timeout
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
});

// Add MCP server with stdio transport and tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var host = builder.Build();

// Validate connection before starting (non-fatal - just warn if it fails)
try
{
    var logseqClient = host.Services.GetRequiredService<LogseqHttpClient>();
    var logger = host.Services.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Validating connection to Logseq at {BaseUrl}...", config.BaseUrl);
    await logseqClient.ValidateConnectionAsync();
    logger.LogInformation("Successfully connected to Logseq HTTP API");
}
catch (LogseqApiException ex)
{
    Console.Error.WriteLine($"Warning: Failed to connect to Logseq: {ex.Message}");
    Console.Error.WriteLine("The MCP server will start anyway. Ensure Logseq is running before using tools.");
}

// Run the MCP server
await host.RunAsync();
return 0;
