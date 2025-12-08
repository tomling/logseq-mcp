# Capability: Enhanced Deployment Guide

## ADDED Requirements

### Requirement: Platform-Specific Network Configuration
The deployment guide MUST provide clear network configuration instructions for all major platforms.

#### Scenario: Linux network configuration
- GIVEN a user on Linux deploying the MCP server in Docker
- WHEN they read the network configuration section
- THEN they MUST see:
  - Option 1: Use `host.docker.internal` with extra_hosts configuration
  - Option 2: Use `--network host` mode
  - Explanation of trade-offs between options
  - How to verify connectivity

#### Scenario: macOS/Windows network configuration
- GIVEN a user on macOS or Windows with Docker Desktop
- WHEN they configure network access to host Logseq
- THEN the guide MUST explain:
  - Use `host.docker.internal` (built-in Docker Desktop feature)
  - How to verify the mapping works
  - Troubleshooting steps if connection fails

#### Scenario: Network troubleshooting
- GIVEN network connectivity issues
- WHEN users consult the troubleshooting section
- THEN it MUST provide:
  - Commands to test connectivity from container
  - Common error messages and solutions
  - Firewall configuration guidance
  - Logseq HTTP API verification steps

### Requirement: Docker MCP Gateway Setup Instructions
The deployment guide MUST provide step-by-step MCP Gateway installation instructions.

#### Scenario: Catalog import
- GIVEN a user wants to install the MCP server
- WHEN they follow the catalog import instructions
- THEN the guide MUST include:
  - Command: `docker mcp catalog import ./logseq-catalog.yaml`
  - Expected output indicating success
  - Explanation of where catalog is stored (~/.docker/mcp/catalogs/)

#### Scenario: Server enablement
- GIVEN the catalog is imported
- WHEN the user enables the server
- THEN the guide MUST provide:
  - Command: `docker mcp server enable logseq-mcp`
  - How to configure required secrets (LOGSEQ_AUTH_TOKEN)
  - How to set optional environment variables

#### Scenario: Installation verification
- GIVEN the server is enabled
- WHEN the user wants to verify installation
- THEN the guide MUST document:
  - `docker mcp catalog ls` - verify catalog is imported
  - `docker mcp server ls` - verify server is enabled
  - `docker mcp server inspect logseq-mcp` - view server details
  - How to check server logs for errors

### Requirement: Environment Variable Management
The deployment guide MUST explain how to manage environment variables.

#### Scenario: Update auth token
- GIVEN the Logseq auth token has changed
- WHEN the user needs to update the configuration
- THEN the guide MUST explain:
  - How to update secrets in MCP Gateway
  - How to restart the server to pick up changes
  - Verification that new token works

#### Scenario: Local development with .env file
- GIVEN a developer running locally without MCP Gateway
- WHEN they use docker-compose
- THEN the guide MUST explain:
  - Copy .env.example to .env
  - Set required variables
  - Use `docker compose up` to start

### Requirement: System Architecture Diagram
The deployment guide MUST include a visual architecture diagram.

#### Scenario: Architecture diagram content
- GIVEN a user wants to understand the system architecture
- WHEN they view the architecture diagram
- THEN it MUST show:
  - Claude client (external)
  - Docker MCP Gateway (host)
  - MCP Server container
  - Logseq application (host)
  - Network boundaries and communication paths
  - Data flow direction (arrows)

#### Scenario: Diagram format
- GIVEN the architecture diagram
- WHEN included in documentation
- THEN it MUST be:
  - In a web-friendly format (PNG or SVG)
  - Clear and readable at documentation size
  - Properly captioned and explained
