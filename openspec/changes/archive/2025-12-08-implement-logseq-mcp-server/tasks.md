# Implementation Tasks

## Phase 1: Project Setup and Infrastructure (Foundation)

### 1.1 Initialize .NET Project Structure
- [x] Create .NET 10 console application project
- [x] Add ModelContextProtocol SDK NuGet package
- [x] Add System.Text.Json for JSON serialization
- [x] Set up project structure with folders: `Models/`, `Services/`, `Tools/`, `Configuration/`
- [x] Configure nullable reference types and C# 13 features
- [x] Create `.editorconfig` for code style consistency

**Validation**: Project builds successfully with `dotnet build`

**Dependencies**: None

### 1.2 Implement Configuration Management
- [x] Create `LogseqConfiguration` class to read environment variables
- [x] Implement validation for required `LOGSEQ_AUTH_TOKEN`
- [x] Provide defaults for `LOGSEQ_HOST` (127.0.0.1) and `LOGSEQ_PORT` (12315)
- [x] Add configuration validation on startup
- [x] Create unit tests for configuration parsing and validation

**Validation**: Configuration loads correctly from environment variables; missing token causes startup failure with clear error

**Dependencies**: 1.1

### 1.3 Implement Logseq HTTP Client
- [x] Create `LogseqHttpClient` class with HttpClient dependency injection
- [x] Implement `CallApiAsync<T>(string method, params object[] args)` method
- [x] Add request serialization with correct JSON structure: `{"method": "...", "args": [...]}`
- [x] Add response deserialization with error handling
- [x] Implement authentication header: `Authorization: Bearer {token}`
- [x] Add connection validation method (call `logseq.App.getUserConfigs`)
- [x] Create comprehensive error handling for network, auth, and deserialization errors
- [x] Configure IHttpClientFactory with AddHttpClient
- [x] Add Microsoft.Extensions.Http.Resilience for retry policies
- [x] Configure standard resilience handler with retry, circuit breaker, and timeouts
- [x] Add unit tests with mocked HttpClient

**Validation**: Can successfully call Logseq API methods and deserialize responses; clear errors for connection failures; resilient to transient failures

**Dependencies**: 1.2

**Parallelizable with**: Phase 2 model definitions (start both simultaneously)

## Phase 2: Data Models and Type Definitions

### 2.1 Create Logseq Entity Models
- [x] Create `BlockEntity` record with properties: `Uuid`, `Content`, `Page`, `Children`, `Properties`
- [x] Create `PageEntity` record with properties: `Name`, `Uuid`, `Properties`
- [x] Ensure proper nullable annotations for optional fields
- [x] Add JSON serialization attributes if needed for property name mapping
- [x] Create unit tests for serialization/deserialization

**Validation**: Models correctly deserialize from sample Logseq API responses

**Dependencies**: None (can start in parallel with 1.3)

### 2.2 Create MCP Tool Input/Output Models
- [x] Use ModelContextProtocol SDK's built-in parameter handling
- [x] Define tool parameters via method signatures with McpToolAttribute
- [x] Document expected formats in XML comments
- [x] Return formatted strings directly from tools

**Validation**: All tool signatures compile and MCP server exposes them correctly

**Dependencies**: 2.1

## Phase 3: Core MCP Tools Implementation

### 3.1 Implement Read-Only Tools
- [x] Implement `list_pages` tool (calls `logseq.Editor.getAllPages`)
- [x] Implement `get_page_content` tool (calls `logseq.Editor.getPageBlocksTree`)
- [x] Implement `get_block` tool (calls `logseq.Editor.getBlock` with `includeChildren`)
- [x] Add proper error handling for non-existent pages/blocks
- [x] Format output for readability by Claude
- [x] Add unit tests for each tool (with mocked LogseqHttpClient)

**Validation**: Each tool successfully retrieves data from Logseq and returns formatted results

**Dependencies**: 1.3, 2.1, 2.2

**Parallelizable with**: 3.2 (independent implementations)

### 3.2 Implement Write Tools - Pages
- [x] Implement `create_page` tool (calls `logseq.Editor.createPage`)
- [x] Add support for creating pages with properties
- [x] Handle duplicate page creation gracefully
- [x] Implement `delete_page` tool (calls `logseq.Editor.deletePage`)
- [x] Add proper error handling for non-existent pages
- [x] Add unit tests for page creation and deletion

**Validation**: Can create and delete pages in Logseq; appropriate errors for edge cases

**Dependencies**: 1.3, 2.1, 2.2

**Parallelizable with**: 3.1 (independent implementations)

### 3.3 Implement Write Tools - Blocks
- [x] Implement `insert_block` tool (calls `logseq.Editor.insertBlock`)
- [x] Support `before`, `after`, and `sibling` positioning options
- [x] Implement `append_block` tool (calls `logseq.Editor.appendBlockInPage`)
- [x] Support block creation with properties
- [x] Implement `update_block` tool (calls `logseq.Editor.updateBlock`)
- [x] Preserve existing properties when updating content only
- [x] Implement `delete_block` tool (calls `logseq.Editor.removeBlock`)
- [x] Add warning about child block deletion
- [x] Add unit tests for all block operations

**Validation**: Can insert, append, update, and delete blocks in Logseq; proper handling of edge cases

**Dependencies**: 1.3, 2.1, 2.2

**Parallelizable with**: 3.4 (independent implementations)

### 3.4 Implement Search Tool
- [x] Implement `search_content` tool using `getAllPages` + filtering
- [x] Retrieve all pages and their block trees
- [x] Implement case-insensitive text search across pages and blocks
- [x] Format results with page names, block UUIDs, and content snippets
- [x] Implement result limiting (max 50 results)
- [x] Add unit tests with mock data

**Validation**: Search returns relevant results; handles no matches gracefully; limits results appropriately

**Dependencies**: 1.3, 2.1, 2.2

**Parallelizable with**: 3.3 (independent implementations)

## Phase 4: MCP Server Integration

### 4.1 Set Up MCP Server Host
- [x] Create MCP server host using ModelContextProtocol SDK
- [x] Register all implemented tools with the server
- [x] Configure stdio transport for MCP Gateway communication
- [x] Add server initialization and shutdown logic
- [x] Implement graceful error handling for tool execution
- [x] Add logging for requests and errors (without exposing tokens)

**Validation**: MCP server starts successfully and responds to tool list requests

**Dependencies**: 1.1, 1.2, 1.3, 3.1, 3.2, 3.3, 3.4

### 4.2 Integration Testing
- [x] Test each tool end-to-end with real Logseq HTTP API via MCP Gateway
- [x] Verify list_pages retrieves all pages correctly
- [x] Verify get_page_content fetches page content
- [x] Verify search_content finds matching content
- [x] Verify create_page creates new pages
- [x] Verify append_block adds blocks to pages
- [x] Verify insert_block creates nested child blocks
- [x] Verify update_block modifies existing blocks
- [x] Verify delete_block removes blocks
- [x] Verify delete_page removes pages
- [x] Test error scenarios validated via unit tests

**Validation**: All tools work correctly against a live Logseq instance

**Dependencies**: 4.1

## Phase 5: Docker Containerization

### 5.1 Create Dockerfile
- [x] Create multi-stage Dockerfile with SDK and runtime stages
- [x] Build stage: Restore dependencies and publish app
- [x] Runtime stage: Copy published app to minimal aspnet image
- [x] Configure non-root user for security
- [x] Set ENTRYPOINT to run the MCP server
- [x] Test Docker build locally

**Validation**: `docker build -t logseq-mcp:latest .` succeeds and produces working image

**Dependencies**: 4.1

**Parallelizable with**: 5.2 (independent files)

### 5.2 Create Docker Compose Configuration
- [x] Create `docker-compose.yml` with service definition
- [x] Configure environment variable loading from `.env` file
- [x] Add network configuration for host access (`host.docker.internal`)
- [x] Create `.env.example` template with all variables
- [x] Test Docker Compose locally (verified via MCP Gateway)

**Validation**: `docker compose up` starts the server successfully with environment variables

**Dependencies**: 5.1

**Parallelizable with**: 5.3 (independent files)

### 5.3 Create MCP Gateway Catalog File
- [x] Create `logseq-catalog.yaml` with version 3 schema
- [x] Define catalog name and displayName
- [x] Add registry entry for `logseq-mcp` server
- [x] List all implemented tools in the `tools` array (10 tools)
- [x] Add appropriate metadata: category `productivity`, tags (logseq, knowledge-management, notes, outliner, pkm)
- [x] Set image name to `logseq-mcp:latest` (update for registry later)
- [x] Configure secrets for LOGSEQ_AUTH_TOKEN, LOGSEQ_HOST, LOGSEQ_PORT
- [x] Add extra_hosts configuration for host.docker.internal access
- [x] Validate YAML syntax
- [x] Test catalog import and server enable via MCP Gateway

**Validation**: Catalog file parses correctly, includes all tools, and MCP Gateway successfully loads it

**Dependencies**: 4.1 (need to know all tool names)

**Parallelizable with**: 5.1, 5.2 (independent files)

## Phase 5.5: CI/CD Setup

### 5.5.1 GitHub Actions Workflow
- [x] Create `.github/workflows/build.yml` workflow file
- [x] Configure workflow to trigger on push to main/master branches
- [x] Configure workflow to trigger on pull requests to main/master
- [x] Set up .NET 10.0.x SDK in workflow
- [x] Add restore dependencies step
- [x] Add build step
- [x] Add test step to run all unit tests
- [x] Verify workflow executes successfully

**Validation**: GitHub Actions workflow runs on push/PR and verifies build and tests pass

**Dependencies**: 5.1, 7.1 (requires tests to exist)

## Phase 6: Documentation and Deployment

### 6.1 Create README Documentation
- [x] Write project overview and purpose
- [x] Document prerequisites (Logseq with HTTP API enabled, Docker MCP Gateway)
- [x] Create "Getting Started" section with step-by-step setup
- [x] Document all environment variables with examples
- [x] Explain how to find auth token in Logseq (Settings > Features > HTTP APIs server)
- [x] Add Docker build and run instructions
- [x] Add Docker MCP Gateway installation instructions
- [x] Document all MCP tools with usage examples
- [x] Add troubleshooting section (Logseq not reachable, auth errors, network issues)
- [x] Include references to Logseq documentation

**Validation**: A new user can follow the README and successfully deploy the server

**Dependencies**: 5.1, 5.2, 5.3

### 6.2 Create Deployment Guide
- [ ] Document network configuration for accessing host Logseq (Linux vs macOS/Windows)
- [ ] Provide Docker MCP Gateway catalog import instructions
- [ ] Provide Docker MCP Gateway server enable instructions
- [ ] Document verification commands (`docker mcp server ls`, etc.)
- [ ] Add section on updating environment variables
- [ ] Add section on publishing to Docker registry (for maintainers)
- [ ] Create architectural diagram showing components and data flow

**Validation**: Documentation covers all deployment scenarios

**Dependencies**: 6.1

### 6.3 Create Example Workflows
- [ ] Create example: "Create a daily note page"
- [ ] Create example: "Search for a topic and add related blocks"
- [ ] Create example: "Organize blocks by moving and updating content"
- [ ] Create example: "Extract data from graph and summarize"
- [ ] Document expected Claude prompts and tool usage

**Validation**: Examples demonstrate common use cases clearly

**Dependencies**: 6.1

## Phase 7: Testing and Quality Assurance

### 7.1 Comprehensive Testing
- [x] Implement 25+ unit tests covering all components
- [x] Test LogseqConfiguration: environment variable loading, validation, defaults
- [x] Test LogseqHttpClient: API request formatting, response deserialization, error handling
- [x] Test LogseqTools: all 10 MCP tools with mocked client
- [x] Run all unit tests and verify 100% pass rate
- [x] Test error handling paths (network failures, auth errors, invalid inputs)
- [x] Test Docker deployment end-to-end via MCP Gateway
- [x] Verify all 10 tools work correctly against live Logseq instance

**Validation**: All tests pass; no critical bugs; performance acceptable for typical graphs

**Dependencies**: 4.2, 5.1, 5.2, 5.3

### 7.2 Security Review
- [x] Verify auth token is never logged or exposed in errors
- [x] Verify container runs as non-root user
- [x] Review input validation for all tools
- [x] Check for injection vulnerabilities (though Logseq API should handle this)
- [x] Verify secure default configuration
- [x] Document security best practices in README

**Validation**: No security issues identified; secure defaults in place

**Dependencies**: 7.1

### 7.3 Code Quality and Style
- [x] Run code formatter and ensure consistent style
- [x] Run static analysis (analyzers)
- [x] Add XML documentation comments to public APIs
- [x] Review and simplify complex methods
- [x] Ensure error messages are user-friendly
- [x] Remove any TODOs or placeholder code
- [x] Configure .editorconfig for consistent formatting
- [x] Address code style warnings (IDE0211, IDE0011 remain as minor)

**Validation**: Code passes all quality checks; build succeeds with minimal warnings

**Dependencies**: 7.1

## Phase 8: Release Preparation (Optional Advanced Features)

### 8.1 Implement Advanced Tools (Optional)
- [ ] Implement `get_linked_references` tool
- [ ] Implement `prepend_block` tool
- [ ] Implement `get_current_page` tool
- [ ] Implement `get_block_properties` tool
- [ ] Implement `set_block_property` tool
- [ ] Implement `remove_block_property` tool
- [ ] Update catalog YAML with new tools
- [ ] Add tests for advanced tools

**Validation**: Advanced tools work correctly if implemented

**Dependencies**: 4.1

**Note**: These are optional enhancements; core functionality is complete without them

### 8.2 Community Engagement
- [ ] Create project GitHub repository (if not already created)
- [ ] Add LICENSE file (check project requirements)
- [ ] Add CONTRIBUTING.md with guidelines
- [ ] Add issue templates for bug reports and feature requests
- [ ] Create discussion thread in Logseq community
- [ ] Submit to Docker MCP Gateway catalog repository (if applicable)

**Validation**: Project is discoverable and ready for community contributions

**Dependencies**: 7.3

## Summary of Dependencies

- **Phase 1**: Foundation, all others depend on this
- **Phase 2**: Can start in parallel with Phase 1
- **Phase 3**: Depends on Phases 1 & 2; tasks within Phase 3 are parallelizable
- **Phase 4**: Depends on Phases 1, 2, 3
- **Phase 5**: Depends on Phase 4; tasks within Phase 5 are parallelizable
- **Phase 6**: Depends on Phase 5
- **Phase 7**: Depends on Phases 4, 5
- **Phase 8**: Optional enhancements, depends on Phase 7

## Estimated Effort

- **Phases 1-4**: Core implementation - 2-3 days ✅ COMPLETE
- **Phase 5**: Docker setup - 0.5 day ✅ COMPLETE
- **Phase 5.5**: CI/CD setup - 0.25 day ✅ COMPLETE
- **Phase 6**: Documentation - 0.5 day ✅ COMPLETE (6.1), PARTIAL (6.2, 6.3 optional)
- **Phase 7**: Testing and QA - 1 day ✅ COMPLETE
- **Phase 8**: Optional enhancements - TBD (not started)

**Total Core Implementation**: ~4.5 days ✅ COMPLETE
