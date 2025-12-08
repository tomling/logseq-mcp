# Capability: MCP Tools - Core Operations

## ADDED Requirements

### Requirement: List Pages Tool
The system MUST provide an MCP tool to list all pages in the Logseq graph.

#### Scenario: List all pages in graph
- GIVEN the Logseq graph contains multiple pages
- WHEN Claude invokes the `list_pages` tool
- THEN the tool MUST call `logseq.Editor.getAllPages`
- AND return a list of page names and UUIDs
- AND the result MUST be formatted as a readable list for Claude

#### Scenario: Handle empty graph
- GIVEN the Logseq graph contains no pages
- WHEN Claude invokes the `list_pages` tool
- THEN the tool MUST return an empty list
- AND the result MUST indicate no pages were found

### Requirement: Get Page Content Tool
The system MUST provide an MCP tool to retrieve the full content of a page including its blocks.

#### Scenario: Get page with blocks as tree
- GIVEN a page name exists in the Logseq graph
- WHEN Claude invokes the `get_page_content` tool with the page name
- THEN the tool MUST call `logseq.Editor.getPageBlocksTree` with the page name
- AND return the page content with all blocks in tree structure
- AND blocks MUST include their content, UUIDs, and child blocks
- AND the result MUST be formatted for readability

#### Scenario: Get non-existent page
- GIVEN a page name does not exist in the Logseq graph
- WHEN Claude invokes the `get_page_content` tool with that page name
- THEN the tool MUST return a clear error indicating the page was not found

#### Scenario: Page parameter validation
- GIVEN Claude invokes the `get_page_content` tool without a page name
- WHEN the tool executes
- THEN it MUST return an error indicating the page name is required

### Requirement: Get Block Tool
The system MUST provide an MCP tool to retrieve a specific block by UUID.

#### Scenario: Get block with children
- GIVEN a block UUID exists in the Logseq graph
- WHEN Claude invokes the `get_block` tool with the UUID
- THEN the tool MUST call `logseq.Editor.getBlock` with the UUID and `{includeChildren: true}`
- AND return the block content, properties, and all child blocks
- AND the result MUST include the parent page reference

#### Scenario: Get block without children
- GIVEN a block UUID exists in the Logseq graph
- AND Claude specifies not to include children
- WHEN Claude invokes the `get_block` tool
- THEN the tool MUST call `logseq.Editor.getBlock` without the includeChildren option
- AND return only the single block's content and properties

#### Scenario: Get non-existent block
- GIVEN a block UUID does not exist
- WHEN Claude invokes the `get_block` tool with that UUID
- THEN the tool MUST return a clear error indicating the block was not found

### Requirement: List Blocks in Page Tool
The system MUST provide an MCP tool to list all top-level blocks in a page.

#### Scenario: List blocks in existing page
- GIVEN a page contains multiple top-level blocks
- WHEN Claude invokes the `list_blocks_in_page` tool with the page name
- THEN the tool MUST call `logseq.Editor.getPageBlocksTree`
- AND return a list of top-level blocks with their content and UUIDs
- AND nested blocks SHOULD be indicated but not fully expanded by default

#### Scenario: List blocks in empty page
- GIVEN a page exists but has no blocks
- WHEN Claude invokes the `list_blocks_in_page` tool
- THEN the tool MUST return an empty list
- AND indicate the page exists but has no content

### Requirement: Create Page Tool
The system MUST provide an MCP tool to create new pages in the Logseq graph.

#### Scenario: Create page with name only
- GIVEN a page name that does not exist
- WHEN Claude invokes the `create_page` tool with the page name
- THEN the tool MUST call `logseq.Editor.createPage` with the page name
- AND return the created page's UUID and name
- AND the page MUST be created in Logseq

#### Scenario: Create page with properties
- GIVEN a page name and initial properties
- WHEN Claude invokes the `create_page` tool with name and properties
- THEN the tool MUST call `logseq.Editor.createPage` with the name and properties object
- AND the created page MUST have the specified properties

#### Scenario: Create duplicate page
- GIVEN a page name already exists
- WHEN Claude invokes the `create_page` tool with that name
- THEN the tool MUST return an error indicating the page already exists
- OR return the existing page without creating a duplicate

### Requirement: Insert Block Tool
The system MUST provide an MCP tool to insert a new block adjacent to an existing block.

#### Scenario: Insert block after target
- GIVEN an existing block UUID
- AND new block content
- WHEN Claude invokes the `insert_block` tool with the UUID, content, and position "after"
- THEN the tool MUST call `logseq.Editor.insertBlock` with the UUID, content, and `{before: false}`
- AND the new block MUST appear immediately after the target block
- AND return the new block's UUID

#### Scenario: Insert block before target
- GIVEN an existing block UUID
- AND new block content
- WHEN Claude invokes the `insert_block` tool with the UUID, content, and position "before"
- THEN the tool MUST call `logseq.Editor.insertBlock` with the UUID, content, and `{before: true}`
- AND the new block MUST appear immediately before the target block

#### Scenario: Insert nested block
- GIVEN an existing block UUID
- AND new block content
- AND Claude specifies the block should be a child
- WHEN Claude invokes the `insert_block` tool with `sibling: false`
- THEN the tool MUST call `logseq.Editor.insertBlock` with `{sibling: false}`
- AND the new block MUST be created as a child of the target block

### Requirement: Append Block to Page Tool
The system MUST provide an MCP tool to append a block to the end of a page.

#### Scenario: Append block to existing page
- GIVEN a page name that exists
- AND new block content
- WHEN Claude invokes the `append_block` tool with the page name and content
- THEN the tool MUST call `logseq.Editor.appendBlockInPage` with the page name and content
- AND the block MUST be added as the last block in the page
- AND return the new block's UUID

#### Scenario: Append block with properties
- GIVEN a page name and block content with properties
- WHEN Claude invokes the `append_block` tool with properties option
- THEN the tool MUST call `logseq.Editor.appendBlockInPage` with the properties in the options
- AND the created block MUST have the specified properties

#### Scenario: Append to non-existent page
- GIVEN a page name that does not exist
- WHEN Claude invokes the `append_block` tool
- THEN the tool MUST return an error indicating the page was not found

### Requirement: Update Block Tool
The system MUST provide an MCP tool to update the content of an existing block.

#### Scenario: Update block content only
- GIVEN an existing block UUID
- AND new content text
- WHEN Claude invokes the `update_block` tool with the UUID and content
- THEN the tool MUST call `logseq.Editor.updateBlock` with the UUID and content
- AND the block's content MUST be replaced with the new text
- AND block properties SHOULD be preserved

#### Scenario: Update block with properties
- GIVEN an existing block UUID
- AND new content and properties
- WHEN Claude invokes the `update_block` tool with content and properties
- THEN the tool MUST call `logseq.Editor.updateBlock` with content and `{properties: {...}}`
- AND both content and properties MUST be updated

#### Scenario: Update non-existent block
- GIVEN a block UUID that does not exist
- WHEN Claude invokes the `update_block` tool
- THEN the tool MUST return an error indicating the block was not found

### Requirement: Delete Page Tool
The system MUST provide an MCP tool to delete pages from the Logseq graph.

#### Scenario: Delete existing page
- GIVEN a page name that exists
- WHEN Claude invokes the `delete_page` tool with the page name
- THEN the tool MUST call `logseq.Editor.deletePage` with the page name
- AND the page MUST be removed from Logseq
- AND return confirmation of deletion

#### Scenario: Delete non-existent page
- GIVEN a page name that does not exist
- WHEN Claude invokes the `delete_page` tool
- THEN the tool MUST return an error indicating the page was not found

### Requirement: Delete Block Tool
The system MUST provide an MCP tool to delete blocks from the Logseq graph.

#### Scenario: Delete existing block
- GIVEN a block UUID that exists
- WHEN Claude invokes the `delete_block` tool with the UUID
- THEN the tool MUST call `logseq.Editor.removeBlock` with the UUID
- AND the block MUST be removed from Logseq
- AND all child blocks MUST also be deleted

#### Scenario: Delete block with children confirmation
- GIVEN a block UUID with nested children
- AND Claude does not explicitly confirm deletion
- WHEN Claude invokes the `delete_block` tool
- THEN the tool SHOULD warn that child blocks will also be deleted
- OR require explicit confirmation

#### Scenario: Delete non-existent block
- GIVEN a block UUID that does not exist
- WHEN Claude invokes the `delete_block` tool
- THEN the tool MUST return an error indicating the block was not found
