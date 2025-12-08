# Capability: Configuration Management

## ADDED Requirements

### Requirement: Environment Variable Configuration
The system MUST support configuration via environment variables for all required settings.

#### Scenario: Configure Logseq connection details
- GIVEN the environment variables are set:
  - `LOGSEQ_HOST=127.0.0.1`
  - `LOGSEQ_PORT=12315`
  - `LOGSEQ_AUTH_TOKEN=my-secret-token-123`
- WHEN the MCP server initializes
- THEN it MUST connect to Logseq at `http://127.0.0.1:12315`
- AND use `Bearer my-secret-token-123` for authentication

#### Scenario: Use default host and port
- GIVEN only `LOGSEQ_AUTH_TOKEN` is set
- AND `LOGSEQ_HOST` and `LOGSEQ_PORT` are not set
- WHEN the MCP server initializes
- THEN it MUST default to `http://127.0.0.1:12315`

#### Scenario: Custom Logseq port
- GIVEN `LOGSEQ_PORT=8080` and other required variables
- WHEN the MCP server initializes
- THEN it MUST connect to `http://127.0.0.1:8080`

#### Scenario: Invalid configuration detection
- GIVEN `LOGSEQ_PORT=abc` (non-numeric)
- WHEN the MCP server initializes
- THEN initialization MUST fail with an error indicating invalid port configuration

### Requirement: Required Configuration Validation
The system MUST validate that all required configuration is provided before starting.

#### Scenario: Missing auth token
- GIVEN the `LOGSEQ_AUTH_TOKEN` environment variable is not set
- WHEN the MCP server attempts to start
- THEN it MUST fail immediately
- AND log a clear error: "LOGSEQ_AUTH_TOKEN environment variable is required"
- AND exit with a non-zero status code

#### Scenario: Empty auth token
- GIVEN `LOGSEQ_AUTH_TOKEN` is set to an empty string or whitespace
- WHEN the MCP server attempts to start
- THEN it MUST fail with an error indicating the token cannot be empty

### Requirement: Configuration Documentation
The system MUST provide clear documentation for all configuration options.

#### Scenario: README includes configuration section
- GIVEN the project README file
- WHEN a user reads the configuration section
- THEN it MUST list all environment variables:
  - `LOGSEQ_HOST` (optional, default: `127.0.0.1`)
  - `LOGSEQ_PORT` (optional, default: `12315`)
  - `LOGSEQ_AUTH_TOKEN` (required, no default)
- AND provide examples of setting each variable
- AND explain where to find the auth token in Logseq

#### Scenario: Docker Compose example provided
- GIVEN the project includes Docker deployment documentation
- WHEN a user reviews the Docker Compose example
- THEN it MUST show how to pass environment variables to the container
- AND include a `.env.example` file template

### Requirement: Secure Token Handling
The system MUST handle authentication tokens securely.

#### Scenario: Token not logged
- GIVEN the MCP server is running with logging enabled
- WHEN any operation is performed
- THEN the auth token MUST NOT appear in any log output
- AND the token MUST NOT be included in error messages

#### Scenario: Token not exposed in container
- GIVEN the server is running in a Docker container
- WHEN someone inspects the container
- THEN the token SHOULD only be visible via environment variables
- AND not stored in any files within the container

### Requirement: Connection String Validation
The system MUST validate that the constructed Logseq URL is valid.

#### Scenario: Validate URL format
- GIVEN configuration values for host and port
- WHEN the server constructs the Logseq API URL
- THEN it MUST validate the URL is well-formed
- AND the scheme MUST be `http` (Logseq HTTP API does not use HTTPS locally)

#### Scenario: Warn about non-localhost configuration
- GIVEN `LOGSEQ_HOST` is set to a non-localhost value (e.g., `192.168.1.10`)
- WHEN the MCP server initializes
- THEN it SHOULD log a warning that remote Logseq connections may be insecure
- AND indicate that Logseq's HTTP API is designed for local use

### Requirement: Runtime Configuration Changes
The system MUST NOT support runtime configuration updates; all configuration MUST be read at startup only.

#### Scenario: No runtime changes for auth token
- GIVEN the MCP server is running
- WHEN the `LOGSEQ_AUTH_TOKEN` environment variable changes
- THEN the server MUST NOT automatically pick up the change
- AND a restart MUST be required to use the new token

#### Scenario: Configuration at startup only
- GIVEN any environment variable changes
- WHEN the MCP server is already running
- THEN configuration MUST remain unchanged
- AND a server restart is required to apply new configuration
