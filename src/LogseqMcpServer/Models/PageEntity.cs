using System.Text.Json.Serialization;

namespace LogseqMcpServer.Models;

/// <summary>
/// Represents a page in a Logseq graph.
/// Pages are named documents that contain blocks.
/// </summary>
public record PageEntity
{
    /// <summary>
    /// Gets the name of the page.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the original name of the page (if different from name).
    /// </summary>
    [JsonPropertyName("original-name")]
    public string? OriginalName { get; init; }

    /// <summary>
    /// Gets the unique identifier for this page.
    /// </summary>
    [JsonPropertyName("uuid")]
    public required string Uuid { get; init; }

    /// <summary>
    /// Gets the properties (metadata) attached to this page.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; init; }

    /// <summary>
    /// Gets the ID of the page (numeric identifier).
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; init; }
}
