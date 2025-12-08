using System.Text.Json.Serialization;

namespace LogseqMcpServer.Models;

/// <summary>
/// Represents a block in a Logseq graph.
/// Blocks are the fundamental unit of content in Logseq.
/// </summary>
public record BlockEntity
{
    /// <summary>
    /// Gets the unique identifier for this block.
    /// </summary>
    [JsonPropertyName("uuid")]
    public string Uuid { get; init; } = string.Empty;

    /// <summary>
    /// Gets the text content of this block.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the internal ID of this block.
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; init; }

    /// <summary>
    /// Gets the page this block belongs to (can be an object with id, name, etc.).
    /// </summary>
    [JsonPropertyName("page")]
    public object? Page { get; init; }

    /// <summary>
    /// Gets the child blocks nested under this block.
    /// </summary>
    [JsonPropertyName("children")]
    public BlockEntity[]? Children { get; init; }

    /// <summary>
    /// Gets the properties (metadata) attached to this block.
    /// </summary>
    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; init; }

    /// <summary>
    /// Gets the parent block reference if this is a nested block.
    /// </summary>
    [JsonPropertyName("parent")]
    public object? Parent { get; init; }

    /// <summary>
    /// Gets the format of the block (markdown, org, etc.).
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Gets the hierarchy level of this block.
    /// </summary>
    [JsonPropertyName("level")]
    public int? Level { get; init; }

    /// <summary>
    /// Gets the left sibling reference.
    /// </summary>
    [JsonPropertyName("left")]
    public object? Left { get; init; }

    /// <summary>
    /// Gets the path references for this block.
    /// </summary>
    [JsonPropertyName("pathRefs")]
    public object[]? PathRefs { get; init; }

    /// <summary>
    /// Gets whether this is a journal block.
    /// </summary>
    [JsonPropertyName("journal?")]
    public bool? IsJournal { get; init; }
}
