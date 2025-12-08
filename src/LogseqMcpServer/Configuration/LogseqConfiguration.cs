namespace LogseqMcpServer.Configuration;

/// <summary>
/// Configuration for connecting to the Logseq HTTP API server.
/// </summary>
public class LogseqConfiguration
{
    private const string DefaultHost = "host.docker.internal";
    private const int DefaultPort = 12315;

    /// <summary>
    /// Gets the Logseq HTTP API host address.
    /// </summary>
    public string Host { get; init; }

    /// <summary>
    /// Gets the Logseq HTTP API port.
    /// </summary>
    public int Port { get; init; }

    /// <summary>
    /// Gets the authentication token for the Logseq HTTP API.
    /// </summary>
    public string AuthToken { get; init; }

    /// <summary>
    /// Gets the full base URL for the Logseq HTTP API.
    /// </summary>
    public string BaseUrl => $"http://{Host}:{Port}";

    private LogseqConfiguration(string host, int port, string authToken)
    {
        Host = host;
        Port = port;
        AuthToken = authToken;
    }

    /// <summary>
    /// Loads configuration from environment variables.
    /// </summary>
    /// <returns>A configured <see cref="LogseqConfiguration"/> instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing or invalid.</exception>
    public static LogseqConfiguration LoadFromEnvironment()
    {
        var authToken = Environment.GetEnvironmentVariable("LOGSEQ_AUTH_TOKEN");
        if (string.IsNullOrWhiteSpace(authToken))
        {
            throw new InvalidOperationException(
                "LOGSEQ_AUTH_TOKEN environment variable is required. " +
                "Please configure an authorization token in Logseq (Settings > Features > HTTP APIs server) " +
                "and set the LOGSEQ_AUTH_TOKEN environment variable.");
        }

        var host = Environment.GetEnvironmentVariable("LOGSEQ_HOST");
        if (string.IsNullOrWhiteSpace(host))
        {
            host = DefaultHost;
        }

        var portString = Environment.GetEnvironmentVariable("LOGSEQ_PORT");
        int port;
        if (string.IsNullOrWhiteSpace(portString))
        {
            port = DefaultPort;
        }
        else if (!int.TryParse(portString, out port) || port <= 0 || port > 65535)
        {
            throw new InvalidOperationException(
                $"LOGSEQ_PORT environment variable must be a valid port number (1-65535). Got: {portString}");
        }

        return new LogseqConfiguration(host, port, authToken);
    }

    /// <summary>
    /// Validates the configuration and logs warnings for potentially problematic settings.
    /// </summary>
    public void Validate()
    {
        // Validate URL format
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || uri.Scheme != "http")
        {
            throw new InvalidOperationException(
                $"Invalid Logseq API URL: {BaseUrl}. The URL must be a valid HTTP URL.");
        }

        // Warn about non-localhost connections
        if (Host != "127.0.0.1" && Host != "localhost" && Host != "host.docker.internal")
        {
            Console.WriteLine($"WARNING: Connecting to non-localhost Logseq instance at {Host}. " +
                            "The Logseq HTTP API is designed for local use and connections may be insecure.");
        }
    }
}
