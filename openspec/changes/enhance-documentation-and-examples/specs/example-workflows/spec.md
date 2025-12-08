# Capability: Example Workflows

## ADDED Requirements

### Requirement: Daily Note Creation Example
The documentation MUST provide an example workflow for creating daily notes.

#### Scenario: Basic daily note creation
- GIVEN a user wants to create a daily note
- WHEN they read the daily note example
- THEN it MUST include:
  - Example Claude prompt: "Create today's daily note with my standard template"
  - Tools used: `create_page`, `append_block`
  - Expected page structure
  - How to add initial blocks (tasks, meetings, notes sections)

#### Scenario: Daily note variations
- GIVEN the basic daily note example
- WHEN users need different note types
- THEN the example MUST show variations:
  - Weekly notes with different structure
  - Meeting notes with attendees and agenda
  - Project notes with status and tasks

### Requirement: Search and Organization Example
The documentation MUST provide an example workflow for searching and organizing content.

#### Scenario: Search for topic and add blocks
- GIVEN a user wants to research a topic and add related notes
- WHEN they read the search example
- THEN it MUST demonstrate:
  - Using `search_content` to find existing mentions
  - Creating a new research page with `create_page`
  - Adding findings with `append_block`
  - Linking to related pages with [[page links]]
  - Tagging with #topics

#### Scenario: Example prompt for search workflow
- GIVEN the search example
- WHEN showing Claude interaction
- THEN it MUST include:
  - Example user prompt: "Find all mentions of 'microservices' and create a summary page"
  - Tool call sequence
  - Expected output format
  - How Claude would summarize findings

### Requirement: Content Reorganization Example
The documentation MUST provide an example workflow for reorganizing content.

#### Scenario: Move and update blocks
- GIVEN a user wants to reorganize their notes
- WHEN they read the reorganization example
- THEN it MUST demonstrate:
  - Reading current structure with `get_page_content`
  - Updating block content with `update_block`
  - Moving blocks with `insert_block` (sibling/child positioning)
  - Creating nested hierarchies

#### Scenario: Example prompt for reorganization
- GIVEN the reorganization example
- WHEN showing Claude interaction
- THEN it MUST include:
  - Example prompt: "Reorganize my project tasks by priority"
  - How Claude would analyze current structure
  - Tool calls to restructure blocks
  - Resulting organization

### Requirement: Data Extraction Example
The documentation MUST provide an example workflow for extracting and summarizing data.

#### Scenario: Extract data from graph
- GIVEN a user wants to analyze their knowledge graph
- WHEN they read the data extraction example
- THEN it MUST demonstrate:
  - Using `list_pages` to enumerate content
  - Filtering with `search_content`
  - Aggregating information across pages
  - Creating a summary page

#### Scenario: Example prompt for data extraction
- GIVEN the data extraction example
- WHEN showing Claude interaction
- THEN it MUST include:
  - Example prompt: "Summarize all my learning notes from the past month"
  - How Claude would gather relevant pages
  - Tool calls to collect information
  - Example summary output with key insights

### Requirement: Example Format and Structure
All example workflows MUST follow a consistent format for clarity.

#### Scenario: Example workflow structure
- GIVEN any example workflow
- WHEN presented in documentation
- THEN it MUST include:
  - **Use Case**: Clear description of the scenario
  - **User Goal**: What the user wants to achieve
  - **Example Prompt**: What to say to Claude
  - **Tools Used**: Which MCP tools are called
  - **Step-by-Step**: Breakdown of the process
  - **Expected Result**: What the user should see
  - **Variations**: Alternative approaches or extensions

#### Scenario: Code examples
- GIVEN tool calls in examples
- WHEN showing technical details
- THEN they MUST:
  - Use proper JSON formatting
  - Show actual parameter values
  - Include explanatory comments
  - Be copy-pasteable for testing
