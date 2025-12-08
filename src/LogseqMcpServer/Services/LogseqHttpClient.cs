using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using LogseqMcpServer.Configuration;
using LogseqMcpServer.Models;

namespace LogseqMcpServer.Services;

/// <summary>
/// HTTP client for communicating with the Logseq HTTP API server.
/// </summary>
public class LogseqHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly LogseqConfiguration _configuration;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LogseqHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client (injected via IHttpClientFactory).</param>
    /// <param name="configuration">The Logseq configuration.</param>
    public LogseqHttpClient(HttpClient httpClient, LogseqConfiguration configuration)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Calls a Logseq API method and deserializes the response.
    /// </summary>
    /// <typeparam name="T">The expected return type.</typeparam>
    /// <param name="method">The API method name (e.g., "logseq.Editor.getBlock").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="args">The arguments to pass to the method.</param>
    /// <returns>The deserialized response.</returns>
    /// <exception cref="LogseqApiException">Thrown when the API call fails.</exception>
    public async Task<T?> CallApiAsync<T>(string method, CancellationToken cancellationToken = default, params object?[] args)
    {
        try
        {
            var request = new LogseqApiRequest
            {
                Method = method,
                Args = args.Length > 0 ? args : null
            };

            var response = await _httpClient.PostAsJsonAsync("/api", request, JsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new LogseqApiException(
                        "Authentication failed. Please check your LOGSEQ_AUTH_TOKEN environment variable. " +
                        "You can find the token in Logseq Settings > Features > HTTP APIs server.",
                        new HttpRequestException($"HTTP {response.StatusCode}"));
                }

                throw new LogseqApiException(
                    $"Logseq API request failed with status {response.StatusCode}: {errorContent}",
                    new HttpRequestException($"HTTP {response.StatusCode}"));
            }

            var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return result;
        }
        catch (HttpRequestException ex)
        {
            throw new LogseqApiException(
                $"Cannot connect to Logseq at {_configuration.BaseUrl}. " +
                "Please ensure Logseq is running with the HTTP API server enabled " +
                "(Settings > Features > HTTP APIs server).",
                ex);
        }
        catch (JsonException ex)
        {
            throw new LogseqApiException(
                "Failed to parse Logseq API response. The API response format may have changed.",
                ex);
        }
        catch (LogseqApiException)
        {
            // Re-throw our own exceptions
            throw;
        }
        catch (Exception ex)
        {
            throw new LogseqApiException(
                $"Unexpected error calling Logseq API method '{method}': {ex.Message}",
                ex);
        }
    }

    /// <summary>
    /// Validates that the Logseq HTTP API is reachable and authentication is working.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the connection is valid.</returns>
    /// <exception cref="LogseqApiException">Thrown when connection validation fails.</exception>
    public async Task<bool> ValidateConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to call a simple API method to validate the connection
            await CallApiAsync<object>("logseq.App.getUserConfigs", cancellationToken);
            return true;
        }
        catch (LogseqApiException)
        {
            throw;
        }
    }
}

/// <summary>
/// Exception thrown when a Logseq API call fails.
/// </summary>
public class LogseqApiException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogseqApiException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LogseqApiException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
