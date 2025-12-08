# Capability: MCP Tools - Advanced Operations

## ADDED Requirements

### Requirement: Search Content Tool
The system MUST provide an MCP tool to search for content across all pages in the Logseq graph.

#### Scenario: Search by text query
- GIVEN a search query string
- WHEN Claude invokes the `search_content` tool with the query
- THEN the tool MUST retrieve all pages via `logseq.Editor.getAllPages`
- AND for each page, retrieve its content via `logseq.Editor.getPageBlocksTree`
- AND filter pages and blocks that contain the search query (case-insensitive)
- AND return matching pages with snippets showing the query in context
- AND results MUST include page names, block UUIDs, and matching content excerpts

#### Scenario: Search with no results
- GIVEN a search query that matches no content
- WHEN Claude invokes the `search_content` tool
- THEN the tool MUST return an empty result set
- AND indicate that no matches were found

#### Scenario: Search with empty query
- GIVEN an empty or whitespace-only search query
- WHEN Claude invokes the `search_content` tool
- THEN the tool MUST return an error indicating a valid query is required

#### Scenario: Search result pagination
- GIVEN a search query that matches many results (>50)
- WHEN Claude invokes the `search_content` tool
- THEN the tool SHOULD limit results to a reasonable number (e.g., 50)
- AND indicate if more results exist
- OR support pagination via additional parameters

### Requirement: Get Page Linked References Tool
The system MUST provide an MCP tool to find all pages and blocks that link to a target page.

#### Scenario: Get linked references for page
- GIVEN a page name that exists
- WHEN Claude invokes the `get_linked_references` tool with the page name
- THEN the tool MUST call `logseq.Editor.getPageLinkedReferences` with the page name
- AND return all pages and blocks that contain links to the target page
- AND results MUST include the linking page name, block UUID, and link context

#### Scenario: Get linked references for page with no links
- GIVEN a page name that exists but has no incoming links
- WHEN Claude invokes the `get_linked_references` tool
- THEN the tool MUST return an empty result set
- AND indicate the page has no linked references

### Requirement: Prepend Block to Page Tool
The system MUST provide an MCP tool to add a block at the beginning of a page.

#### Scenario: Prepend block to existing page
- GIVEN a page name that exists
- AND new block content
- WHEN Claude invokes the `prepend_block` tool with the page name and content
- THEN the tool MUST call `logseq.Editor.prependBlockInPage` with the page name and content
- AND the block MUST be added as the first block in the page
- AND return the new block's UUID

#### Scenario: Prepend to page with existing blocks
- GIVEN a page with existing blocks
- AND new block content
- WHEN Claude invokes the `prepend_block` tool
- THEN the new block MUST appear before all existing blocks
- AND existing blocks MUST remain unchanged

### Requirement: Get Current Page Tool
The system MUST provide an MCP tool to retrieve the currently active page in Logseq.

#### Scenario: Get current page when Logseq is focused
- GIVEN Logseq has a page currently open and focused
- WHEN Claude invokes the `get_current_page` tool
- THEN the tool MUST call `logseq.Editor.getCurrentPage`
- AND return the current page name and UUID
- AND this represents the page the user is actively viewing/editing

#### Scenario: Get current page when no page is active
- GIVEN Logseq has no page currently focused
- WHEN Claude invokes the `get_current_page` tool
- THEN the tool MUST return null or indicate no active page
- OR return the last viewed page

### Requirement: Get Block Properties Tool
The system MUST provide an MCP tool to retrieve all properties from a block.

#### Scenario: Get all block properties
- GIVEN a block UUID with multiple properties
- WHEN Claude invokes the `get_block_properties` tool with the UUID
- THEN the tool MUST call `logseq.Editor.getBlockProperties` with the UUID
- AND return all properties as key-value pairs
- AND properties MUST include both user-defined and system properties

#### Scenario: Get single block property
- GIVEN a block UUID and a property key
- WHEN Claude invokes the `get_block_property` tool with UUID and key
- THEN the tool MUST call `logseq.Editor.getBlockProperty` with UUID and key
- AND return only the specified property value

#### Scenario: Get properties from block with none
- GIVEN a block UUID with no properties
- WHEN Claude invokes the `get_block_properties` tool
- THEN the tool MUST return an empty properties object

### Requirement: Set Block Property Tool
The system MUST provide an MCP tool to set or update individual block properties.

#### Scenario: Set new property on block
- GIVEN a block UUID and a property key-value pair
- WHEN Claude invokes the `set_block_property` tool with UUID, key, and value
- THEN the tool MUST call `logseq.Editor.upsertBlockProperty` with UUID, key, and value
- AND the block MUST have the new property
- AND existing properties MUST be preserved

#### Scenario: Update existing property on block
- GIVEN a block UUID with an existing property
- AND a new value for that property
- WHEN Claude invokes the `set_block_property` tool
- THEN the tool MUST update the property value
- AND not create a duplicate property

### Requirement: Remove Block Property Tool
The system MUST provide an MCP tool to remove properties from blocks.

#### Scenario: Remove existing property
- GIVEN a block UUID with a specific property
- WHEN Claude invokes the `remove_block_property` tool with UUID and property key
- THEN the tool MUST call `logseq.Editor.removeBlockProperty` with UUID and key
- AND the property MUST be removed from the block
- AND other properties MUST remain unchanged

#### Scenario: Remove non-existent property
- GIVEN a block UUID without the specified property
- WHEN Claude invokes the `remove_block_property` tool
- THEN the tool SHOULD succeed without error (idempotent operation)
- OR return a message indicating the property didn't exist

## MODIFIED Requirements

None - these are all new capabilities.

## REMOVED Requirements

None - no existing functionality is being removed.
