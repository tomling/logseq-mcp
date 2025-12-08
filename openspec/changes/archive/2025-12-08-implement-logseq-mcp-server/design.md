# Design: Implement Logseq MCP Server

## Architecture Overview

The Logseq MCP server follows a simple layered architecture:

```
┌─────────────────────────────────────┐
│   MCP Client (Claude Desktop)       │
└──────────────┬──────────────────────┘
               │ stdio/MCP protocol
┌──────────────▼──────────────────────┐
│   Logseq MCP Server (C#/.NET)       │
│  ┌──────────────────────────────┐   │
│  │ MCP Tool Handlers            │   │
│  └──────────┬───────────────────┘   │
│  ┌──────────▼───────────────────┐   │
│  │ LogseqHttpClient             │   │
│  └──────────┬───────────────────┘   │
└─────────────┼───────────────────────┘
              │ HTTP + Bearer auth
┌─────────────▼───────────────────────┐
│   Logseq Desktop Application        │
│   (HTTP API Server :12315)          │
└─────────────────────────────────────┘
```

## Key Design Decisions

### 1. Technology Stack

**Decision**: Use C# with .NET 10 and the official ModelContextProtocol SDK

**Rationale**:
- Aligns with project conventions specified in `openspec/project.md`
- Official C# SDK provides type safety and proper MCP protocol handling
- .NET HttpClient provides robust HTTP communication with async/await patterns
- Strong JSON serialization support with System.Text.Json
- Easy containerization with minimal .NET runtime images

**Alternatives considered**:
- Python (simpler but less type-safe, harder to containerize efficiently)
- Node.js (familiar for Logseq plugin developers but not project convention)

### 2. API Communication Pattern

**Decision**: Single HTTP endpoint (`POST /api`) with method-based routing

**Rationale**:
- Matches Logseq's actual HTTP API design
- Simple request format: `{ "method": "logseq.Editor.getBlock", "args": [...] }`
- Single authorization header per request
- No need for session management or cookies
- Direct mapping from Logseq plugin API to HTTP calls

**Implementation**:
```csharp
public class LogseqApiRequest
{
    public string Method { get; set; }
    public object[]? Args { get; set; }
}

public class LogseqHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _authToken;

    public async Task<T> CallApiAsync<T>(string method, params object[] args)
    {
        var request = new LogseqApiRequest { Method = method, Args = args };
        // Set Authorization header, POST to /api, deserialize response
    }
}
```

### 3. MCP Tool Design

**Decision**: Map Obsidian-style tools to Logseq equivalents with similar naming

**Mapping**:
| Obsidian Tool | Logseq Tool | Logseq API Method(s) |
|---------------|-------------|---------------------|
| `list_files_in_vault` | `list_pages` | `logseq.Editor.getAllPages` |
| `list_files_in_dir` | `list_blocks_in_page` | `logseq.Editor.getPageBlocksTree` |
| `get_file_contents` | `get_page_content` | `logseq.Editor.getPage`, `logseq.Editor.getPageBlocksTree` |
| `search` | `search_content` | Custom search via `getAllPages` + content filtering |
| `append_content` | `append_block` | `logseq.Editor.appendBlockInPage` |
| `patch_content` | `update_block` | `logseq.Editor.updateBlock` |
| `delete_file` | `delete_page`, `delete_block` | `logseq.Editor.deletePage`, `logseq.Editor.removeBlock` |
| N/A | `create_page` | `logseq.Editor.createPage` |
| N/A | `insert_block` | `logseq.Editor.insertBlock` |
| N/A | `get_block` | `logseq.Editor.getBlock` |

**Rationale**:
- Familiar patterns for users coming from Obsidian MCP
- Logseq's block-centric model requires some additional tools (get_block, insert_block)
- Tool names clearly indicate Logseq concepts (pages, blocks) vs file system concepts
- Each tool has a single, clear responsibility

### 4. Configuration Management

**Decision**: Environment variables for all configuration

**Required variables**:
- `LOGSEQ_HOST` (default: `127.0.0.1`)
- `LOGSEQ_PORT` (default: `12315`)
- `LOGSEQ_AUTH_TOKEN` (required, no default)

**Rationale**:
- Standard approach for containerized applications
- Docker MCP Gateway supports environment variable injection
- Avoids hardcoding sensitive tokens
- Easy to override for different Logseq instances

**Alternative considered**:
- Configuration files: More complex to mount in containers, less secure for tokens

### 5. Error Handling Strategy

**Decision**: Graceful degradation with clear error messages

**Pattern**:
```csharp
try
{
    var result = await _logseqClient.CallApiAsync<BlockEntity>("logseq.Editor.getBlock", blockId);
    return new GetBlockResult { Block = result };
}
catch (HttpRequestException ex)
{
    throw new McpException($"Cannot connect to Logseq at {_baseUrl}. Ensure Logseq is running with HTTP API enabled.", ex);
}
catch (JsonException ex)
{
    throw new McpException($"Invalid response from Logseq API. The API may have changed.", ex);
}
```

**Rationale**:
- Clear feedback helps users diagnose issues (Logseq not running, wrong token, API changes)
- MCP protocol supports error responses that Claude can present to users
- Avoid silent failures or generic error messages

### 6. Data Model Mapping

**Decision**: Create C# records that mirror Logseq entities

**Key entities**:
```csharp
public record BlockEntity
{
    public string Uuid { get; init; }
    public string Content { get; init; }
    public string? Page { get; init; }
    public BlockEntity[]? Children { get; init; }
    public Dictionary<string, object>? Properties { get; init; }
}

public record PageEntity
{
    public string Name { get; init; }
    public string Uuid { get; init; }
    public Dictionary<string, object>? Properties { get; init; }
}
```

**Rationale**:
- Type safety prevents runtime errors
- Records provide immutability and value semantics
- Nullable reference types make optional fields explicit
- Easy to serialize/deserialize with System.Text.Json

### 7. Search Implementation

**Decision**: Client-side filtering approach for initial implementation

**Rationale**:
- Logseq HTTP API doesn't expose a dedicated search endpoint
- Can fetch all pages and filter by content match
- Simple to implement and understand
- Performance acceptable for typical graph sizes (hundreds to low thousands of pages)

**Future optimization**: If performance becomes an issue, could use Logseq's query API or implement server-side indexing

### 8. Docker Container Design

**Decision**: Multi-stage build with minimal runtime image

**Dockerfile pattern**:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "LogseqMcpServer.dll"]
```

**Rationale**:
- Minimal runtime image reduces attack surface and size
- Multi-stage build keeps build dependencies out of final image
- Standard .NET containerization pattern
- Compatible with Docker MCP Gateway requirements

## Trade-offs

### Simplicity vs Feature Completeness
**Choice**: Start with core operations, defer advanced features

**Trade-off**: Initial version won't support:
- Namespace hierarchies
- Complex graph queries
- Block properties manipulation beyond basic updates
- Batch operations

**Justification**: Core operations cover 80% of use cases. Advanced features can be added based on user feedback.

### Performance vs Simplicity
**Choice**: Simple HTTP calls per operation, no caching

**Trade-off**: Multiple operations require multiple HTTP round-trips to Logseq

**Justification**: Premature optimization. Local HTTP calls are fast. Add caching only if profiling shows it's needed.

### Error Detail vs Security
**Choice**: Detailed error messages without exposing sensitive data

**Trade-off**: Error messages guide users but don't include token values or full request bodies

**Justification**: Debugging is easier with good errors, but security is non-negotiable.

## Testing Strategy

### Unit Tests
- Mock `HttpClient` to test API call construction
- Test serialization/deserialization of Logseq entities
- Test error handling paths

### Integration Tests
- Require running Logseq instance with test graph
- Test each MCP tool end-to-end
- Verify proper authentication
- Test error scenarios (Logseq not running, invalid token, invalid block IDs)

### Container Tests
- Verify container builds successfully
- Test environment variable injection
- Ensure stdio communication works correctly

## Future Enhancements

1. **Advanced Search**: Use Logseq's query DSL for more powerful searches
2. **Batch Operations**: Support multiple operations in a single MCP call
3. **Block Properties**: Full CRUD for block properties and metadata
4. **Namespace Operations**: Navigate and manipulate page hierarchies
5. **Transaction Support**: Multi-step operations with rollback
6. **Caching Layer**: Reduce HTTP round-trips for frequently accessed data
7. **WebSocket Support**: If Logseq adds real-time API, enable push updates

## Security Considerations

1. **Token Storage**: Auth token in environment variable, never logged or exposed
2. **Input Validation**: Validate all user inputs before sending to Logseq API
3. **Error Messages**: Don't expose tokens or sensitive data in error messages
4. **Network Security**: Only connects to local Logseq instance by default
5. **Container Isolation**: Runs as non-root user in container

## Open Technical Questions

None - the design is straightforward given the available Logseq HTTP API and existing MCP server patterns.
