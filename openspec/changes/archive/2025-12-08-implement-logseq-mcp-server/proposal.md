# Proposal: Implement Logseq MCP Server

## Change ID
`implement-logseq-mcp-server`

## Overview
Create a Model Context Protocol (MCP) server that enables Claude and other AI assistants to interact with Logseq graphs through the Logseq HTTP API server. This server will provide MCP tools that mirror the functionality of the Obsidian MCP server, adapted for Logseq's data model and API patterns.

## Motivation
Logseq is a popular privacy-first, open-source knowledge management tool that uses an outliner-based approach (blocks) rather than traditional note-taking. Users want to leverage AI assistants to:
- Read and navigate their Logseq graphs
- Create and update blocks and pages
- Search across their knowledge base
- Automate knowledge management workflows

By creating an MCP server that wraps Logseq's HTTP API, we enable Claude to work with Logseq graphs in the same way it can work with Obsidian vaults, providing a consistent experience for knowledge management tasks.

## Scope

### In Scope
- MCP server implementation in C# using .NET 10 and the official ModelContextProtocol SDK
- HTTP client for communicating with Logseq's local HTTP API server (default: `http://127.0.0.1:12315`)
- MCP tools for core Logseq operations:
  - Listing pages and blocks
  - Reading page and block content
  - Creating pages and blocks
  - Updating blocks
  - Searching content
  - Deleting pages and blocks
- Configuration via environment variables (Logseq host, port, auth token)
- Docker containerization for Docker MCP Gateway compatibility
- Catalog configuration for registration with Docker MCP Gateway

### Out of Scope
- Logseq plugin development (this is an external HTTP API client)
- Advanced features like namespace management, page properties, or graph queries (can be added later)
- File system access (Logseq's HTTP API operates on the logical graph structure)
- Authentication mechanisms beyond bearer token
- Real-time sync or webhooks (Logseq HTTP API is request/response only)

## User Impact
**Who**: Logseq users who want to use Claude or other MCP clients to work with their knowledge graphs

**What changes**: Users will be able to install and enable the Logseq MCP server in their Docker MCP Gateway, providing Claude with tools to interact with their Logseq graph

**Migration needed**: None - this is a new capability

## Dependencies
- Logseq must be running locally with the HTTP API server enabled (Settings > Features > HTTP APIs server)
- Users must configure an authorization token in Logseq
- Docker MCP Gateway must be installed for deployment

## Risks & Mitigations
1. **Risk**: Logseq HTTP API may have rate limits or performance constraints
   - **Mitigation**: Implement proper error handling and retry logic; document any known limitations

2. **Risk**: Logseq HTTP API is relatively underdocumented compared to official REST APIs
   - **Mitigation**: Base implementation on documented plugin API patterns; test extensively with common operations

3. **Risk**: Logseq must be running for the server to work
   - **Mitigation**: Provide clear error messages when Logseq is unreachable; document prerequisites

4. **Risk**: Breaking changes in Logseq's HTTP API
   - **Mitigation**: Pin to tested Logseq versions in documentation; implement graceful degradation

## Alternatives Considered
1. **Direct file system access**: Would require parsing Markdown/EDN files, more complex and brittle
2. **Logseq plugin**: Would run inside Logseq but couldn't be used by external MCP clients
3. **GraphQL layer**: Over-engineering for initial implementation; HTTP API is sufficient

## Open Questions
None - the technical approach is clear based on existing patterns (Obsidian MCP server) and available API documentation.

## Success Criteria
- [x] MCP server successfully connects to Logseq HTTP API
- [x] All core tools (list, read, create, update, search, delete) work correctly
- [x] Server can be deployed via Docker and registered with Docker MCP Gateway
- [x] Claude can perform common Logseq workflows (create pages, add blocks, search, read content)
- [x] Error handling provides clear feedback when Logseq is unreachable or API calls fail

## References
- [Obsidian MCP Server](https://github.com/docker/mcp-obsidian/tree/b20ad46eea43988a4c1a6f8ec7947ef4c6cbf9a9)
- [Logseq HTTP API Documentation](https://github.com/logseq/logseq/blob/master/resources/docs/api_server.html)
- [Logseq Plugin API (IEditorProxy)](https://logseq.github.io/plugins/interfaces/IEditorProxy.html)
- [Logseq Plugins Documentation](https://plugins-doc.logseq.com/)
