using System.Text.Json.Serialization;

namespace LogseqMcpServer.Models;

/// <summary>
/// Represents a request to the Logseq HTTP API.
/// </summary>
public class LogseqApiRequest
{
    /// <summary>
    /// Gets or sets the API method to call (e.g., "logseq.Editor.getBlock").
    /// </summary>
    [JsonPropertyName("method")]
    public required string Method { get; set; }

    /// <summary>
    /// Gets or sets the arguments to pass to the API method.
    /// </summary>
    [JsonPropertyName("args")]
    public object?[]? Args { get; set; }
}
