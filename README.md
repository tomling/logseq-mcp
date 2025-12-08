# 🧠 Logseq MCP Server

A Model Context Protocol (MCP) server that enables Claude and other AI assistants to interact with [Logseq](https://logseq.com/) knowledge graphs via the Logseq HTTP API.

## ✅ Prerequisites

1. **Logseq** with HTTP API server enabled:
   - Open Logseq Settings > Features
   - Enable "HTTP APIs server"
   - Configure an authorization token
   - The API runs on `http://127.0.0.1:12315` by default

2. **Docker MCP Gateway** installed ([Installation Guide](https://github.com/docker/mcp))

3. **Docker** for building and running the container

4. **MCP Client** (e.g., Claude Desktop, Claude Code) connected to the Docker MCP Gateway
   - Ensure your agent/client is configured to use the Docker MCP Gateway as an MCP server.

## 🚀 Quick Start

### 1. Build the Docker Image

```bash
cd src/LogseqMcpServer
docker build -t logseq-mcp:latest .
```

### 2. Import the Catalog

```bash
docker mcp catalog import ./logseq-catalog.yaml
```

This creates the catalog at `~/.docker/mcp/catalogs/logseq-mcp-catalog.yaml`.

### 3. Enable the Server

```bash
docker mcp server enable logseq-mcp
```

### 4. Configure Secrets

Set the required secrets using the Docker MCP secret store:

```bash
# Required: Your Logseq authorization token
docker mcp secret set logseq_auth_token=YOUR_TOKEN_HERE

# Optional: Host address (default: host.docker.internal)
# Use "host.docker.internal" to reach Logseq on the host machine (since 127.0.0.1 refers to the container)
docker mcp secret set logseq_host=host.docker.internal

# Optional: Port (defaults to 12315)
docker mcp secret set logseq_port=12315
```

**Finding your Logseq authorization token:**

- Open Logseq Settings > Features > HTTP APIs server
- Copy the Authorization Token
- Replace `YOUR_TOKEN_HERE` with your actual token

### 5. Verify Installation

```bash
# List enabled servers
docker mcp server ls

# Verify secrets are configured
docker mcp secret ls

# The server is now ready to use with Claude Code or Claude Desktop
# Test by asking: "List my Logseq pages"
```

## 🛠️ Available MCP Tools

| Tool | Description |
|------|-------------|
| `list_pages` | Lists all pages in the Logseq graph |
| `get_page_content` | Gets the full content of a page including all blocks |
| `get_block` | Gets a specific block by its UUID |
| `create_page` | Creates a new page in the graph |
| `append_block` | Appends a new block to the end of a page |
| `insert_block` | Inserts a block relative to an existing block |
| `update_block` | Updates the content of an existing block |
| `delete_page` | Deletes a page from the graph |
| `delete_block` | Deletes a block (and its children) |
| `search_content` | Searches for content across all pages |

## 💡 Example Workflows

### Creating a Daily Note

```
Claude, create a new page called "2025-12-03" and add a block with today's goals.
```

### Searching and Organizing

```
Claude, search my Logseq graph for "project ideas" and create a new page called "Consolidated Project Ideas" with all the results.
```

### Reading and Summarizing

```
Claude, get the content of my "Reading List" page and summarize the top 5 books I want to read.
```

## 💻 Development

### Local Development

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run (requires Logseq with HTTP API enabled)
LOGSEQ_AUTH_TOKEN=your-token dotnet run
```

### Project Structure

```
src/LogseqMcpServer/
├── Configuration/       # Configuration management
├── Models/             # Data models (BlockEntity, PageEntity)
├── Services/           # LogseqHttpClient
├── Tools/              # MCP tool implementations
├── Program.cs          # Application entry point
└── Dockerfile          # Docker configuration
```

## 🔒 Security Considerations

- The authorization token is sensitive—never commit it to version control
- Use environment variables or Docker secrets for token management
- The container runs as a non-root user for security
- Logseq's HTTP API is designed for local use only

## 🤝 Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📜 License

See [LICENSE](../LICENSE) file.

## 📚 References

- [Logseq HTTP API Documentation](https://github.com/logseq/logseq/blob/master/resources/docs/api_server.html)
- [Logseq Plugin API](https://logseq.github.io/plugins/)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Docker MCP Gateway](https://github.com/docker/mcp)

## 🙏 Acknowledgments

Built with:

- [ModelContextProtocol C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [.NET 10](https://dotnet.microsoft.com/)
- Inspired by the [Obsidian MCP Server](https://github.com/docker/mcp-obsidian)