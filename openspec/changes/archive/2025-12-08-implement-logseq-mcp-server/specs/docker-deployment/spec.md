# Capability: Docker Deployment and MCP Gateway Integration

## ADDED Requirements

### Requirement: Dockerfile Implementation
The system MUST provide a Dockerfile for containerizing the Logseq MCP server.

#### Scenario: Multi-stage build for minimal image
- GIVEN the project Dockerfile
- WHEN the Docker image is built
- THEN it MUST use a multi-stage build with:
  - Build stage using `mcr.microsoft.com/dotnet/sdk:10.0`
  - Runtime stage using `mcr.microsoft.com/dotnet/aspnet:10.0`
- AND the final image MUST only contain the published application and runtime dependencies
- AND the image size SHOULD be minimized by excluding build tools

#### Scenario: Dockerfile builds successfully
- GIVEN the project source code
- WHEN a user runs `docker build -t logseq-mcp:latest .`
- THEN the build MUST complete without errors
- AND the resulting image MUST be tagged as `logseq-mcp:latest`

#### Scenario: Container runs as non-root user
- GIVEN the built Docker image
- WHEN the container starts
- THEN it SHOULD run as a non-root user for security
- AND file permissions MUST allow the application to run correctly

### Requirement: Docker Compose Configuration
The system MUST provide a Docker Compose file for easy local deployment.

#### Scenario: Docker Compose with environment variables
- GIVEN a `docker-compose.yml` file
- WHEN a user runs `docker compose up`
- THEN the service MUST start with environment variables loaded from `.env` file
- AND the service MUST be named clearly (e.g., `logseq-mcp-server`)

#### Scenario: Docker Compose example includes all variables
- GIVEN the `docker-compose.yml` and `.env.example` files
- WHEN a user copies `.env.example` to `.env` and populates values
- THEN all required environment variables MUST be defined:
  - `LOGSEQ_AUTH_TOKEN`
  - `LOGSEQ_HOST` (optional)
  - `LOGSEQ_PORT` (optional)

### Requirement: MCP Gateway Catalog Configuration
The system MUST provide a catalog YAML file for Docker MCP Gateway registration.

#### Scenario: Catalog file structure
- GIVEN the catalog YAML file (e.g., `logseq-catalog.yaml`)
- WHEN the file is parsed
- THEN it MUST include:
  - `version: 3`
  - `name: logseq-mcp-catalog` (or similar unique name)
  - `displayName: Logseq MCP Server`
  - Registry entry for the server with:
    - Unique `server-id` (e.g., `logseq-mcp`)
    - Description explaining the server's purpose
    - Title: "Logseq MCP Server"
    - Docker image name
    - List of all tool names
    - Metadata with category and tags

#### Scenario: Catalog lists all MCP tools
- GIVEN the catalog YAML file
- WHEN the registry entry for the server is examined
- THEN the `tools` array MUST list all provided tool names:
  - `list_pages`
  - `get_page_content`
  - `get_block`
  - `create_page`
  - `append_block`
  - `insert_block`
  - `update_block`
  - `delete_page`
  - `delete_block`
  - `search_content`

#### Scenario: Catalog metadata for discoverability
- GIVEN the catalog YAML file
- WHEN examining the metadata section
- THEN it MUST include:
  - `category: productivity` (or appropriate category)
  - `tags` array with relevant tags like: `logseq`, `knowledge-management`, `notes`, `outliner`

### Requirement: Network Configuration for Local Logseq Access
The system MUST support network configuration that allows the container to access Logseq running on the host.

#### Scenario: Container accesses host Logseq on Linux
- GIVEN the MCP server running in a Docker container on Linux
- AND Logseq running on the host at `127.0.0.1:12315`
- WHEN the container attempts to connect
- THEN the configuration MUST use `host.docker.internal` or `--network host`
- AND documentation MUST explain the network setup

#### Scenario: Container accesses host Logseq on macOS/Windows
- GIVEN the MCP server running in Docker Desktop on macOS or Windows
- AND Logseq running on the host
- WHEN `LOGSEQ_HOST=host.docker.internal` is configured
- THEN the container MUST successfully connect to Logseq on the host

#### Scenario: Documentation explains network modes
- GIVEN the deployment documentation
- WHEN a user reads the network configuration section
- THEN it MUST explain:
  - Using `host.docker.internal` for Docker Desktop
  - Using `--network host` for Linux (or explaining routing)
  - How to verify Logseq is reachable from the container

### Requirement: Docker MCP Gateway Installation Instructions
The system MUST provide clear instructions for installing the server via Docker MCP Gateway.

#### Scenario: Import catalog command
- GIVEN the deployment documentation
- WHEN a user follows the installation steps
- THEN the documentation MUST include:
  ```bash
  docker mcp catalog import ./logseq-catalog.yaml
  ```
- AND explain that this creates the catalog at `~/.docker/mcp/catalogs/`

#### Scenario: Enable server command
- GIVEN the catalog is imported
- WHEN the user enables the server
- THEN the documentation MUST include:
  ```bash
  docker mcp server enable logseq-mcp
  ```
- AND explain that this registers the server in the gateway

#### Scenario: Verify installation commands
- GIVEN the server is enabled
- WHEN the user wants to verify installation
- THEN the documentation MUST provide:
  ```bash
  docker mcp catalog ls
  docker mcp server ls
  docker mcp server inspect logseq-mcp
  ```

### Requirement: Environment Variable Injection via Gateway
The system MUST support environment variable injection through Docker MCP Gateway configuration.

#### Scenario: Configure environment variables in gateway
- GIVEN the Docker MCP Gateway registry configuration
- WHEN a user adds environment variables to the server configuration
- THEN the variables MUST be passed to the container on startup
- AND the server MUST read these variables correctly

#### Scenario: Document environment variable configuration
- GIVEN the deployment documentation
- WHEN a user needs to configure the auth token
- THEN the documentation MUST explain how to:
  - Set environment variables in the Docker MCP Gateway
  - Or pass them via Docker Compose
  - Or set them in the shell before enabling the server

### Requirement: MCP Gateway Catalog Secrets Configuration
The system MUST support secure configuration of sensitive values via Docker MCP Gateway secrets.

#### Scenario: Catalog defines required secrets
- GIVEN the catalog YAML file
- WHEN the secrets section is examined
- THEN it MUST define:
  - `logseq_auth_token` secret (required) mapped to LOGSEQ_AUTH_TOKEN env var
  - `logseq_host` secret (optional) mapped to LOGSEQ_HOST env var
  - `logseq_port` secret (optional) mapped to LOGSEQ_PORT env var
- AND each secret MUST include a description explaining where to find the value

#### Scenario: Extra hosts configuration for host access
- GIVEN the catalog YAML file includes extra_hosts configuration
- WHEN the container is started via MCP Gateway
- THEN it MUST map `host.docker.internal:host-gateway`
- AND this enables the container to access Logseq running on the Docker host

### Requirement: Comprehensive Testing
The system MUST include comprehensive unit tests for all components.

#### Scenario: Test coverage for configuration
- GIVEN the test suite
- WHEN tests for LogseqConfiguration are executed
- THEN they MUST verify:
  - Loading from environment variables
  - Validation of required fields
  - Default value handling
  - Invalid configuration detection

#### Scenario: Test coverage for HTTP client
- GIVEN the test suite
- WHEN tests for LogseqHttpClient are executed
- THEN they MUST verify:
  - API request formatting
  - Response deserialization
  - Error handling for network failures
  - Authentication error handling

#### Scenario: Test coverage for MCP tools
- GIVEN the test suite
- WHEN tests for LogseqTools are executed
- THEN they MUST verify:
  - All 10 MCP tools work correctly
  - Input validation and error handling
  - Proper formatting of Logseq API calls
  - Response transformation to MCP format

#### Scenario: All tests pass
- GIVEN the complete test suite
- WHEN `dotnet test` is executed
- THEN all tests MUST pass (25+ tests)
- AND there MUST be no test failures or skipped tests

### Requirement: Continuous Integration
The system MUST include GitHub Actions workflow for automated build and test.

#### Scenario: GitHub Actions workflow on push
- GIVEN a `.github/workflows/build.yml` file exists
- WHEN code is pushed to main/master branches
- THEN the workflow MUST execute automatically
- AND perform: restore dependencies, build, and run tests

#### Scenario: GitHub Actions workflow on pull request
- GIVEN a pull request is created targeting main/master
- WHEN the PR is opened or updated
- THEN the workflow MUST execute automatically
- AND verify the build succeeds and all tests pass

#### Scenario: Workflow uses correct .NET version
- GIVEN the GitHub Actions workflow
- WHEN the workflow executes
- THEN it MUST use .NET 10.0.x SDK
- AND run on ubuntu-latest

#### Scenario: Workflow runs tests
- GIVEN the GitHub Actions workflow
- WHEN the test step executes
- THEN it MUST run `dotnet test` against the test project
- AND fail the workflow if any tests fail

### Requirement: Build and Push to Registry
The system MUST provide instructions for publishing the Docker image.

#### Scenario: Build and tag image
- GIVEN the project documentation
- WHEN a maintainer wants to publish a release
- THEN the documentation MUST include:
  ```bash
  docker build -t <registry>/logseq-mcp:latest -t <registry>/logseq-mcp:v1.0.0 .
  docker push <registry>/logseq-mcp:latest
  docker push <registry>/logseq-mcp:v1.0.0
  ```

#### Scenario: Update catalog with registry image
- GIVEN a published Docker image
- WHEN updating the catalog YAML
- THEN the `image` field MUST reference the published registry path
- AND version tags SHOULD be used for production deployments
