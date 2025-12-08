# Capability: Logseq HTTP Client

## ADDED Requirements

### Requirement: HTTP Client Configuration
The system MUST provide a configurable HTTP client for communicating with the Logseq HTTP API server.

#### Scenario: Client initialization with environment variables
- GIVEN the environment variables `LOGSEQ_HOST`, `LOGSEQ_PORT`, and `LOGSEQ_AUTH_TOKEN` are set
- WHEN the MCP server initializes
- THEN a configured HttpClient MUST be created with the base URL `http://{LOGSEQ_HOST}:{LOGSEQ_PORT}`
- AND the client MUST include the `Authorization: Bearer {LOGSEQ_AUTH_TOKEN}` header on all requests

#### Scenario: Client initialization with default values
- GIVEN the `LOGSEQ_AUTH_TOKEN` environment variable is set
- AND `LOGSEQ_HOST` and `LOGSEQ_PORT` are not set
- WHEN the MCP server initializes
- THEN the client MUST default to `http://127.0.0.1:12315`

#### Scenario: Missing authentication token
- GIVEN the `LOGSEQ_AUTH_TOKEN` environment variable is not set
- WHEN the MCP server initializes
- THEN initialization MUST fail with a clear error message indicating the token is required

### Requirement: API Request Execution
The HTTP client MUST support calling any Logseq plugin API method via the HTTP API.

#### Scenario: Successful API call
- GIVEN a valid Logseq API method name (e.g., `logseq.Editor.getBlock`)
- AND valid arguments for that method
- WHEN the client executes the API call
- THEN it MUST POST to the `/api` endpoint with JSON body `{"method": "<method-name>", "args": [<arguments>]}`
- AND it MUST include the `Content-Type: application/json` header
- AND it MUST include the `Authorization: Bearer <token>` header
- AND it MUST deserialize the JSON response to the expected return type

#### Scenario: API call with no arguments
- GIVEN a Logseq API method that requires no arguments (e.g., `logseq.Editor.getAllPages`)
- WHEN the client executes the API call
- THEN the request body MUST include `"args": []` or omit the args field

#### Scenario: Network error handling
- GIVEN Logseq is not running or unreachable
- WHEN the client executes any API call
- THEN it MUST throw an exception with a clear message indicating Logseq is unreachable
- AND the message MUST suggest checking if Logseq is running with HTTP API enabled

#### Scenario: Authentication error handling
- GIVEN an invalid or missing authorization token
- WHEN the client executes any API call
- THEN it MUST throw an exception indicating authentication failure
- AND the message MUST suggest checking the token configuration

#### Scenario: Invalid response handling
- GIVEN the Logseq API returns malformed JSON or unexpected structure
- WHEN the client attempts to deserialize the response
- THEN it MUST throw an exception indicating an API response error
- AND the message MUST suggest the Logseq API may have changed

### Requirement: Response Deserialization
The HTTP client MUST support deserializing Logseq API responses to strongly-typed C# objects.

#### Scenario: Deserialize BlockEntity
- GIVEN a Logseq API response for a block (from `getBlock`)
- WHEN the client deserializes the response
- THEN it MUST map to a `BlockEntity` record with properties: `Uuid`, `Content`, `Page`, `Children`, `Properties`
- AND nullable properties MUST correctly handle missing fields in the JSON response

#### Scenario: Deserialize PageEntity
- GIVEN a Logseq API response for a page (from `getPage`)
- WHEN the client deserializes the response
- THEN it MUST map to a `PageEntity` record with properties: `Name`, `Uuid`, `Properties`

#### Scenario: Deserialize array responses
- GIVEN a Logseq API that returns arrays (e.g., `getAllPages`)
- WHEN the client deserializes the response
- THEN it MUST correctly deserialize to a typed collection (e.g., `List<PageEntity>`)

### Requirement: Connection Validation
The HTTP client MUST provide a method to validate connectivity to Logseq.

#### Scenario: Validate connection on startup
- GIVEN the MCP server is starting
- WHEN the server attempts to validate the Logseq connection
- THEN it MUST call a lightweight Logseq API method (e.g., `logseq.App.getUserConfigs`)
- AND if the call succeeds, the connection is validated
- AND if the call fails, startup MUST continue with a warning (non-fatal)
- AND the warning MUST suggest ensuring Logseq is running before using tools

### Requirement: Resilience and Retry Policies
The HTTP client MUST implement retry and resilience patterns using Microsoft.Extensions.Http.Resilience.

#### Scenario: Standard resilience handler configuration
- GIVEN the HTTP client is configured
- WHEN transient failures occur (network timeouts, temporary unavailability)
- THEN the client MUST use AddStandardResilienceHandler with:
  - Retry policy with 3 maximum attempts
  - Exponential backoff with jitter
  - Circuit breaker with 30-second sampling duration
  - Per-attempt timeout of 10 seconds
  - Total request timeout of 30 seconds

#### Scenario: Retry on transient HTTP errors
- GIVEN a Logseq API call fails with a transient error (5xx, network timeout)
- WHEN the retry policy is triggered
- THEN the client MUST automatically retry up to 3 times
- AND use exponential backoff between retries
- AND apply jitter to avoid thundering herd

#### Scenario: Circuit breaker opens on repeated failures
- GIVEN multiple consecutive API calls fail
- WHEN the circuit breaker threshold is exceeded
- THEN subsequent calls MUST fail fast without attempting the request
- AND the circuit MUST automatically reset after the sampling duration
