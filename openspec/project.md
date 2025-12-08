# Project Context

## Purpose

MCP server that provides tools for interacting with Logseq knowledge graphs via the Logseq HTTP API. This server is designed to run as a Docker container compatible with the Docker MCP Gateway, enabling Claude and other AI assistants to read, create, update, search, and manage pages and blocks in a user's Logseq graph for knowledge management and automation workflows.

## Tech Stack

- .NET 10
- C#
- ModelContextProtocol SDK (official C# SDK)
- HttpClient for external API integration
- Docker for containerization

## Project Conventions

### Code Style

- Follow C# naming conventions (PascalCase for public members, camelCase for private fields)
- Use async/await patterns for all I/O operations
- Prefer modern C# features (pattern matching, records, nullable reference types, etc.)
- Keep tool implementations focused and single-purpose

### MCP C# SDK Attributes

The ModelContextProtocol C# SDK provides attributes for declaring MCP tools that are automatically discovered and registered by the server.

#### `[McpServerToolType]`

- **Purpose**: Marks a static class as containing MCP tool methods
- **Usage**: Apply to the class containing tool implementations
- **Example**: `[McpServerToolType] public static class LogseqTools`
- **Requirements**: Class must be static and contain methods decorated with `[McpServerTool]`

#### `[McpServerTool]`

- **Purpose**: Declares a method as an MCP tool that will be exposed to clients
- **Properties**:
  - `Name` (required): The tool name that clients use to invoke it (snake_case convention)
- **Usage**: Apply to public static async methods
- **Example**: `[McpServerTool(Name = "list_pages")]`
- **Method Requirements**:
  - Must be `public static async Task<string>`
  - First parameter should be an injected service (e.g., `LogseqHttpClient`)
  - Additional parameters become tool arguments
  - Return type must be `Task<string>` (JSON or formatted text response)

#### `[Description]`

- **Purpose**: Provides human-readable descriptions for tools and parameters
- **Usage**:
  - Apply to methods (after `[McpServerTool]`) to describe the tool
  - Apply to method parameters to describe arguments
- **Example**:
  ```csharp
  [McpServerTool(Name = "get_page_content")]
  [Description("Gets the full content of a page including all its blocks")]
  public static async Task<string> GetPageContent(
      LogseqHttpClient logseqClient,
      [Description("The name of the page to retrieve")] string pageName)
  ```
- **Best Practices**:
  - Tool descriptions: Clear, concise explanation of what the tool does
  - Parameter descriptions: Specify expected format, defaults, and constraints
  - Use present tense and active voice

#### Tool Implementation Pattern

```csharp
[McpServerToolType]
public static class LogseqTools
{
    [McpServerTool(Name = "tool_name")]
    [Description("What this tool does")]
    public static async Task<string> ToolName(
        LogseqHttpClient logseqClient,  // Injected dependency
        [Description("Parameter description")] string requiredParam,
        [Description("Optional parameter description")] string? optionalParam = null,
        [Description("Parameter with default")] int count = 10)
    {
        try
        {
            // Tool implementation
            return "Success message or data";
        }
        catch (LogseqApiException ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
```

#### Naming Conventions

- **Tool Names**: Use snake_case (e.g., `list_pages`, `get_page_content`, `search_content`)
- **Method Names**: Use PascalCase matching the tool name semantically (e.g., `ListPages`, `GetPageContent`, `SearchContent`)
- **Consistency**: Tool name should clearly indicate the action and target (verb_noun pattern preferred)

### Architecture Patterns

- Single-responsibility MCP tools (one tool = one clear function)
- Simple HTTP client pattern for external API integration
- Containerized deployment model for Docker MCP Gateway compatibility
- Minimal dependencies approach (lean containers)
- Configuration via environment variables where appropriate

### Error Handling

- **Non-fatal startup validation**: Connection validation to Logseq should warn but not prevent server startup
- Graceful degradation for external API failures
- Clear error messages returned to MCP clients
- Proper HTTP status code handling
- Rate limiting awareness and backoff strategies
- Tools should fail gracefully when Logseq is not reachable, providing clear error messages

### Testing Strategy

- Unit tests for core MCP tool functionality
- Integration tests for external API interactions
- Container validation tests
- Mock external dependencies in unit tests

### Git Workflow

- Main branch for stable releases
- Feature branches for new capabilities
- Conventional commits for clear change history
- Semantic versioning for releases

## Domain Context

This MCP server operates in the knowledge management and personal knowledge base domain, specifically with Logseq - an open-source, privacy-first outliner and knowledge management tool.

### Logseq Concepts

- **Graph**: A collection of interconnected pages and blocks representing a knowledge base
- **Page**: A named document in Logseq (similar to a note or wiki page)
- **Block**: The fundamental unit of content in Logseq; a single item in an outliner that can contain text, properties, and nested child blocks
- **UUID**: Universally unique identifier assigned to each block for referencing
- **Properties**: Key-value metadata attached to blocks or pages
- **Linked References**: Pages and blocks that link to a target page (backlinks)
- **Block Tree**: Hierarchical structure of blocks with parent-child relationships

### Data Formats

- **Input**: Page names (strings), block UUIDs (strings), content text (strings), properties (key-value objects)
- **Output**: JSON objects representing pages (`PageEntity`) and blocks (`BlockEntity`) with their content, properties, and relationships
- **API Communication**: JSON request/response via HTTP POST to Logseq's `/api` endpoint

## Important Constraints

- Must run as a Docker container compatible with Docker MCP Gateway
- Must use the official C# ModelContextProtocol SDK
- Rate limiting: No explicit rate limits on Logseq HTTP API (local communication)
- Network dependency: Requires local network access to reach Logseq HTTP server (default: `http://127.0.0.1:12315`)
- Authentication: Bearer token authentication required (configured in Logseq settings)
- Logseq must be running locally with HTTP API server enabled (Settings > Features > HTTP APIs server)
- Container must be able to access host network to reach local Logseq instance
- **Critical**: Server must start and register MCP tools even if Logseq connection fails during initialization
- Secrets must be configured using `docker mcp secret set`, not `docker mcp config set`

## External Dependencies

- **Logseq HTTP API Server**: [https://github.com/logseq/logseq/blob/master/resources/docs/api_server.html](https://github.com/logseq/logseq/blob/master/resources/docs/api_server.html)
  - Primary endpoint: `POST http://127.0.0.1:12315/api`
  - Returns JSON responses with data matching Logseq Plugin API types
  - Authentication: Bearer token (user-configured in Logseq)
  - Rate limits: None (local HTTP server)
  - Cost: Free (Logseq is open-source)
  - Exposes all methods from Logseq Plugin API: [https://logseq.github.io/plugins/](https://logseq.github.io/plugins/)
- **Docker MCP Gateway**: Container runtime environment for MCP servers
- **Logseq Desktop Application**: Must be running locally for the server to function

## Docker MCP Gateway Catalog Configuration

The server is registered in the Docker MCP Gateway catalog file (`catalog.yaml`) to enable discovery and usage by Claude Desktop and other MCP clients.

### Catalog Structure

```yaml
version: 3
name: logseq-mcp-catalog
displayName: Logseq MCP Server
registry:
  logseq-mcp:
    description: "Interact with Logseq knowledge graphs via HTTP API - read, create, update, search, and manage pages and blocks"
    title: Logseq MCP Server
    type: server
    dateAdded: "2025-12-03T18:00:00Z"
    image: logseq-mcp:latest
    ref: ""
    extra_hosts:
      - "host.docker.internal:host-gateway"
    secrets:
      - name: logseq_auth_token
        env: LOGSEQ_AUTH_TOKEN
        description: "Find your authorization token in Logseq Settings > Features > HTTP APIs server"
      - name: logseq_host
        env: LOGSEQ_HOST
        description: "Logseq HTTP API host address (default: host.docker.internal for Docker)"
        optional: true
      - name: logseq_port
        env: LOGSEQ_PORT
        description: "Logseq HTTP API port (default: 12315)"
        optional: true
    tools:
      - name: list_pages
      - name: get_page_content
      - name: get_block
      - name: create_page
      - name: insert_block
      - name: append_block
      - name: update_block
      - name: delete_page
      - name: delete_block
      - name: search_content
    prompts: 0
    resources: {}
    metadata:
      category: productivity
      tags:
        - logseq
        - knowledge-management
        - notes
        - outliner
        - pkm
```

### Catalog Field Guidelines

- **name**: Unique identifier for the catalog (lowercase, hyphenated)
- **displayName**: Human-readable catalog name
- **server-id**: Unique identifier for this server within the catalog (lowercase, hyphenated)
- **description**: Clear, concise explanation of server functionality (1-2 sentences)
- **title**: Display name for the server
- **dateAdded**: ISO 8601 timestamp when server was added to catalog
- **image**: Docker image name and tag (typically `latest` for development)
- **tools**: List all tool names exposed by this MCP server
- **prompts**: Number of predefined prompts (0 if none)
- **metadata.category**: Primary category (e.g., productivity, data, gaming, development, finance)
- **metadata.tags**: Descriptive tags for discoverability (3-5 recommended)

## Deployment to Docker MCP Gateway

### 1. Build image

```bash
cd LogseqMcpServer
docker build -t logseq-mcp:latest .
```

### 2. Import catalog (makes it available)

```bash
docker mcp catalog import ./logseq-catalog.yaml
```

This creates the catalog at `~/.docker/mcp/catalogs/logseq-mcp-catalog.yaml`.

### 3. Enable server (registers it)

```bash
docker mcp server enable logseq-mcp
```

### 4. Configure secrets

**Important**: Use `docker mcp secret set`, NOT `docker mcp config set`

```bash
# Required: Your Logseq authorization token
docker mcp secret set logseq_auth_token=YOUR_TOKEN_HERE

# Optional: Host address (defaults to host.docker.internal)
docker mcp secret set logseq_host=host.docker.internal

# Optional: Port (defaults to 12315)
docker mcp secret set logseq_port=12315
```

**Finding your Logseq authorization token:**
- Open Logseq Settings > Features > HTTP APIs server
- Copy the Authorization Token

### 5. Verify Installation

```bash
# List enabled servers
docker mcp server ls

# Verify secrets are configured
docker mcp secret ls

# Test the connection (in Claude Code or Claude Desktop)
# The server should now be accessible with all 10 tools
```

### 6. Usage with Claude Code

The server will be automatically available when using Claude Code with the Docker MCP Gateway integration. You can verify by asking Claude Code to list your Logseq pages or create a test page.

### Common Issues

- **"0 tools loaded"**: Secrets not configured correctly. Use `docker mcp secret set` not `docker mcp config set`.
- **"Cannot connect to Logseq"**: Ensure Logseq is running with HTTP API enabled, and secrets are set correctly.
- **Network issues**: The `extra_hosts` configuration in the catalog automatically maps `host.docker.internal` to access the host machine.
