using System.ComponentModel;
using System.Text.Json;
using LogseqMcpServer.Models;
using LogseqMcpServer.Services;
using ModelContextProtocol.Server;

namespace LogseqMcpServer.Tools;

/// <summary>
/// MCP tools for interacting with Logseq graphs.
/// </summary>
[McpServerToolType]
public static class LogseqTools
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [McpServerTool(Name = "list_pages"), Description("Lists all pages in the Logseq graph")]
    public static async Task<string> ListPages(LogseqHttpClient logseqClient)
    {
        try
        {
            var pages = await logseqClient.CallApiAsync<PageEntity[]>("logseq.Editor.getAllPages", default);

            if (pages == null || pages.Length == 0)
            {
                return "No pages found in the graph.";
            }

            return $"Found {pages.Length} pages:\n\n" +
                   string.Join("\n", pages.Select((p, i) => $"{i + 1}. {p.Name} (UUID: {p.Uuid})"));
        }
        catch (LogseqApiException ex)
        {
            return $"Error listing pages: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_page_content"), Description("Gets the full content of a page including all its blocks")]
    public static async Task<string> GetPageContent(
        LogseqHttpClient logseqClient,
        [Description("The name of the page to retrieve")] string pageName)
    {
        try
        {
            var blocks = await logseqClient.CallApiAsync<BlockEntity[]>("logseq.Editor.getPageBlocksTree", default, pageName);

            if (blocks == null || blocks.Length == 0)
            {
                return $"Page '{pageName}' exists but has no content blocks.";
            }

            return $"Page: {pageName}\n\n" + FormatBlockTree(blocks, 0);
        }
        catch (LogseqApiException ex)
        {
            return $"Error getting page content: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_block"), Description("Gets a specific block by its UUID")]
    public static async Task<string> GetBlock(
        LogseqHttpClient logseqClient,
        [Description("The UUID of the block to retrieve")] string blockUuid,
        [Description("Whether to include child blocks (default: true)")] bool includeChildren = true)
    {
        try
        {
            var options = includeChildren ? new { includeChildren = true } : null;
            var block = await logseqClient.CallApiAsync<BlockEntity>("logseq.Editor.getBlock", default, blockUuid, options);

            if (block == null)
            {
                return $"Block with UUID '{blockUuid}' not found.";
            }

            return $"Block UUID: {block.Uuid}\nContent: {block.Content}\n\n" +
                   (block.Children != null && block.Children.Length > 0
                       ? "Children:\n" + FormatBlockTree(block.Children, 0)
                       : "No child blocks.");
        }
        catch (LogseqApiException ex)
        {
            return $"Error getting block: {ex.Message}";
        }
    }

    [McpServerTool(Name = "create_page"), Description("Creates a new page in the Logseq graph with optional properties. Use append_block to add content after creation.")]
    public static async Task<string> CreatePage(
        LogseqHttpClient logseqClient,
        [Description("The name of the page to create")] string pageName,
        [Description("Optional properties to set on the page as JSON (e.g., {\"tags\": [\"tag1\", \"tag2\"]})")] string? propertiesJson = null)
    {
        try
        {
            Dictionary<string, object>? properties = null;
            if (!string.IsNullOrWhiteSpace(propertiesJson))
            {
                try
                {
                    properties = JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson);
                    if (properties == null)
                    {
                        return "Error: Invalid JSON in properties parameter.";
                    }
                }
                catch (JsonException ex)
                {
                    return $"Error: Invalid JSON in properties parameter: {ex.Message}";
                }
            }

            var page = await logseqClient.CallApiAsync<PageEntity>("logseq.Editor.createPage", default,
                pageName, properties ?? new Dictionary<string, object>());

            if (page == null)
            {
                return $"Failed to create page '{pageName}'. It may already exist.";
            }

            return $"Successfully created page '{page.Name}' with UUID: {page.Uuid}";
        }
        catch (LogseqApiException ex)
        {
            return $"Error creating page: {ex.Message}";
        }
    }

    [McpServerTool(Name = "append_block"), Description("""
        Appends a new block to the end of a page.

        IMPORTANT LOGSEQ FORMATTING RULES:
        - Each block contains ONE item only (one paragraph, one heading, one list item, or one code block)
        - DO NOT put multiple headings or list items in one block - use separate blocks
        - DO NOT combine different content types (heading + list, paragraph + code) in one block
        - For structured content, create parent/child block hierarchy instead
        - Nested content uses child blocks with sibling=false in insert_block

        LOGSEQ MARKDOWN CONVENTIONS:
        - Headings: Use # with space (e.g., "# Title", "## Subtitle", "### Section")
        - Page links: Use [[double brackets]] (e.g., "[[My Page]]")
        - Tags: Use # without space (e.g., "#project #urgent")
        - Bold: **text**, Italic: *text* or _text_
        - Code: `inline code` or ```language for blocks
        - Bullets are automatic (Logseq adds - to each block)

        Examples of CORRECT usage:
        - Block 1: "# Introduction"
        - Block 2 (child of 1): "This section covers [[MCP]] basics"
        - Block 3: "**First point** - explanation here"
        - Block 4: "Related to #development"

        Examples of INCORRECT usage (will render poorly):
        - "# Header 1\n## Header 2" ❌ (multiple headings)
        - "- Item 1\n- Item 2\n- Item 3" ❌ (multiple list items)
        - "# Title\nParagraph text\n- Bullet" ❌ (mixed content)
        """)]
    public static async Task<string> AppendBlock(
        LogseqHttpClient logseqClient,
        [Description("The name of the page to append to")] string pageName,
        [Description("Single content item only: one paragraph, one heading, one list item, or one code block. See tool description for formatting rules.")] string content,
        [Description("Optional properties to set on the block as JSON (e.g., {\"priority\": \"high\"})")] string? propertiesJson = null)
    {
        try
        {
            Dictionary<string, object>? properties = null;
            if (!string.IsNullOrWhiteSpace(propertiesJson))
            {
                try
                {
                    properties = JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson);
                    if (properties == null)
                    {
                        return "Error: Invalid JSON in properties parameter.";
                    }
                }
                catch (JsonException ex)
                {
                    return $"Error: Invalid JSON in properties parameter: {ex.Message}";
                }
            }

            var options = properties != null ? new { properties } : null;
            var block = await logseqClient.CallApiAsync<BlockEntity>("logseq.Editor.appendBlockInPage", default,
                pageName, content, options);

            if (block == null)
            {
                return $"Failed to append block to page '{pageName}'.";
            }

            return $"Successfully appended block to '{pageName}' with UUID: {block.Uuid}";
        }
        catch (LogseqApiException ex)
        {
            return $"Error appending block: {ex.Message}";
        }
    }

    [McpServerTool(Name = "insert_block"), Description("""
        Inserts a new block relative to an existing block.

        IMPORTANT LOGSEQ FORMATTING RULES:
        - Each block contains ONE item only (one paragraph, one heading, one list item, or one code block)
        - DO NOT put multiple headings, list items, or mixed content in one block
        - Use sibling=true to insert at the same level as target block
        - Use sibling=false to insert as a child (nested) under target block

        LOGSEQ MARKDOWN CONVENTIONS:
        - Headings: # with space ("# Title", "## Subtitle")
        - Page links: [[double brackets]] ("[[Page Name]]")
        - Tags: # without space ("#tag")
        - Bold: **text**, Italic: *text*
        - Bullets are automatic (don't add - manually)

        Examples of CORRECT usage:
        - Insert "# Sub-section" as child (sibling=false) under main section
        - Insert "Next point about [[Topic]]" as sibling (sibling=true)
        - Insert each list item as separate sibling blocks

        Examples of INCORRECT usage:
        - "- Item 1\n- Item 2" in single block ❌
        - "## Header\nContent text" in single block ❌
        """)]
    public static async Task<string> InsertBlock(
        LogseqHttpClient logseqClient,
        [Description("The UUID of the target block")] string targetBlockUuid,
        [Description("Single content item only: one paragraph, one heading, one list item, or one code block. See tool description for formatting rules.")] string content,
        [Description("Position: 'before' or 'after' the target block (default: 'after')")] string position = "after",
        [Description("Whether to insert as a sibling (true, same level) or child (false, nested under) the target block (default: true)")] bool sibling = true)
    {
        try
        {
            var before = position.ToLowerInvariant() == "before";
            var options = new { before, sibling };

            var block = await logseqClient.CallApiAsync<BlockEntity>("logseq.Editor.insertBlock", default,
                targetBlockUuid, content, options);

            if (block == null)
            {
                return $"Failed to insert block {position} target block '{targetBlockUuid}'.";
            }

            return $"Successfully inserted block {position} target with UUID: {block.Uuid}";
        }
        catch (LogseqApiException ex)
        {
            return $"Error inserting block: {ex.Message}";
        }
    }

    [McpServerTool(Name = "update_block"), Description("""
        Updates the content of an existing block.

        IMPORTANT LOGSEQ FORMATTING RULES:
        - Each block contains ONE item only (one paragraph, one heading, one list item, or one code block)
        - DO NOT replace a single-item block with multiple items
        - Maintain the block structure - if it was a heading, keep it as one heading
        - To add more content, use insert_block to add sibling or child blocks instead

        LOGSEQ MARKDOWN CONVENTIONS:
        - Headings: # with space ("# Title", "## Subtitle")
        - Page links: [[double brackets]] ("[[Page Name]]")
        - Tags: # without space ("#tag")
        - Bullets are automatic (don't add - manually)

        Examples of CORRECT usage:
        - Update "# Old heading" to "# New heading"
        - Update "Old text" to "Updated text with [[link]]"
        - Update "Point about X" to "Point about X #important"

        Examples of INCORRECT usage:
        - Update to "# Header 1\n## Header 2" ❌
        - Update to "- Item 1\n- Item 2\n- Item 3" ❌
        """)]
    public static async Task<string> UpdateBlock(
        LogseqHttpClient logseqClient,
        [Description("The UUID of the block to update")] string blockUuid,
        [Description("Single content item only: one paragraph, one heading, one list item, or one code block. See tool description for formatting rules.")] string content,
        [Description("Optional properties to set on the block as JSON (e.g., {\"status\": \"done\"})")] string? propertiesJson = null)
    {
        try
        {
            Dictionary<string, object>? properties = null;
            if (!string.IsNullOrWhiteSpace(propertiesJson))
            {
                try
                {
                    properties = JsonSerializer.Deserialize<Dictionary<string, object>>(propertiesJson);
                    if (properties == null)
                    {
                        return "Error: Invalid JSON in properties parameter.";
                    }
                }
                catch (JsonException ex)
                {
                    return $"Error: Invalid JSON in properties parameter: {ex.Message}";
                }
            }

            var options = properties != null ? new { properties } : null;
            await logseqClient.CallApiAsync<object>("logseq.Editor.updateBlock", default,
                blockUuid, content, options);

            return $"Successfully updated block '{blockUuid}'.";
        }
        catch (LogseqApiException ex)
        {
            return $"Error updating block: {ex.Message}";
        }
    }

    [McpServerTool(Name = "delete_page"), Description("Deletes a page from the Logseq graph")]
    public static async Task<string> DeletePage(
        LogseqHttpClient logseqClient,
        [Description("The name of the page to delete")] string pageName)
    {
        try
        {
            await logseqClient.CallApiAsync<object>("logseq.Editor.deletePage", default, pageName);
            return $"Successfully deleted page '{pageName}'.";
        }
        catch (LogseqApiException ex)
        {
            return $"Error deleting page: {ex.Message}";
        }
    }

    [McpServerTool(Name = "delete_block"), Description("Deletes a block (and all its children) from the Logseq graph")]
    public static async Task<string> DeleteBlock(
        LogseqHttpClient logseqClient,
        [Description("The UUID of the block to delete")] string blockUuid)
    {
        try
        {
            await logseqClient.CallApiAsync<object>("logseq.Editor.removeBlock", default, blockUuid);
            return $"Successfully deleted block '{blockUuid}' and all its children.";
        }
        catch (LogseqApiException ex)
        {
            return $"Error deleting block: {ex.Message}";
        }
    }

    [McpServerTool(Name = "search_content"), Description("Searches for content across all pages in the graph")]
    public static async Task<string> SearchContent(
        LogseqHttpClient logseqClient,
        [Description("The search query string")] string query,
        [Description("Maximum number of results to return (default: 50)")] int maxResults = 50)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return "Error: Search query cannot be empty.";
            }

            var pages = await logseqClient.CallApiAsync<PageEntity[]>("logseq.Editor.getAllPages", default);
            if (pages == null || pages.Length == 0)
            {
                return "No pages found in the graph.";
            }

            var results = new List<string>();
            var queryLower = query.ToLowerInvariant();

            foreach (var page in pages)
            {
                if (results.Count >= maxResults) break;

                // Check if page name matches
                if (page.Name.ToLowerInvariant().Contains(queryLower))
                {
                    results.Add($"Page: {page.Name}");
                }

                // Get page blocks and search content
                try
                {
                    var blocks = await logseqClient.CallApiAsync<BlockEntity[]>("logseq.Editor.getPageBlocksTree", default, page.Name);
                    if (blocks != null)
                    {
                        SearchBlocks(blocks, queryLower, page.Name, results, maxResults);
                    }
                }
                catch (LogseqApiException)
                {
                    // Skip individual pages that can't be retrieved and continue search
                    continue;
                }
            }

            if (results.Count == 0)
            {
                return $"No results found for query '{query}'.";
            }

            var resultText = $"Found {results.Count} result(s) for '{query}':\n\n" + string.Join("\n\n", results);
            if (results.Count >= maxResults)
            {
                resultText += $"\n\n(Limited to {maxResults} results)";
            }

            return resultText;
        }
        catch (LogseqApiException ex)
        {
            return $"Error searching content: {ex.Message}";
        }
    }

    // Helper methods
    private static string FormatBlockTree(BlockEntity[] blocks, int indent)
    {
        var result = new System.Text.StringBuilder();
        foreach (var block in blocks)
        {
            var indentStr = new string(' ', indent * 2);
            result.AppendLine($"{indentStr}- {block.Content} (UUID: {block.Uuid})");

            if (block.Children != null && block.Children.Length > 0)
            {
                result.Append(FormatBlockTree(block.Children, indent + 1));
            }
        }
        return result.ToString();
    }

    private static void SearchBlocks(BlockEntity[] blocks, string queryLower, string pageName,
        List<string> results, int maxResults)
    {
        const int SnippetMaxLength = 100;

        foreach (var block in blocks)
        {
            if (results.Count >= maxResults) return;

            if (block.Content.ToLowerInvariant().Contains(queryLower))
            {
                var snippet = block.Content.Length > SnippetMaxLength
                    ? string.Concat(block.Content.AsSpan(0, SnippetMaxLength), "...")
                    : block.Content;
                results.Add($"Page: {pageName}\nBlock UUID: {block.Uuid}\nContent: {snippet}");
            }

            if (block.Children != null && block.Children.Length > 0)
            {
                SearchBlocks(block.Children, queryLower, pageName, results, maxResults);
            }
        }
    }
}
