# Spawn Graph Visualization Implementation Plan

## Overview

This document describes the implementation of a full-screen node-graph canvas visualization for the Procedural Content Trace system. The visualization replaces the existing tree-list SpawnGraphViewer with an interactive canvas where nodes represent game entities (Scenes, Situations, Choices, Locations, NPCs, Routes) and edges represent relationships between them.

The goal is to provide a visual debugging tool similar to node-based editors in creative software (Blender nodes, Unreal Blueprints, audio DAW graphs).

---

## Problem Statement

### Current State

The existing `SpawnGraphViewer` displays procedural content as a collapsible tree list in the debug panel. While functional, this representation has limitations:

1. **Hierarchy obscures relationships** - Cross-links (Choice spawning Scene) are hard to visualize in tree form
2. **Entity context hidden** - Locations, NPCs, Routes referenced by Situations are displayed as text, not as connected entities
3. **No spatial layout** - Related nodes cannot be positioned to show logical groupings
4. **Limited screen space** - Embedded in debug panel sidebar, cannot expand for complex graphs

### Desired State

A full-screen interactive canvas where:

1. All SpawnTrace entities appear as draggable nodes with distinct visual styles
2. Parent-child relationships shown as connecting lines between nodes
3. Entity references (Location, NPC, Route) appear as separate nodes with reference lines
4. Automatic layout algorithm positions nodes to minimize overlap and edge crossings
5. Pan, zoom, and selection support for navigation and inspection

---

## Design Decisions

### Layout Direction: Left-to-Right

The graph flows horizontally like a timeline:
- Root Scenes appear at the left edge
- Situations appear to the right of their parent Scenes
- Choices appear to the right of their parent Situations
- Spawned Scenes appear further right, connected back from the spawning Choice
- Entity nodes (Location/NPC/Route) appear below/above their referencing Situations

This mirrors the narrative flow of gameplay: time progresses left-to-right.

### Entity Node Visibility: Always Visible

Location, NPC, and Route nodes are always rendered as separate graph nodes (not collapsed into their parent Situation). This provides full context for debugging placement issues and entity relationships without requiring manual expansion.

### Page Type: Dedicated Full-Screen Route

The visualization lives at `/spawngraph` as a dedicated page, not embedded in a panel or modal. This provides maximum screen real estate and allows the browser's native navigation (back button, bookmarks) to work naturally.

### Update Mode: Manual Refresh

The graph does not update in real-time during gameplay. Users click a "Refresh" button to rebuild the graph from the current ProceduralContentTracer state. This avoids performance issues from constant re-layout and allows users to study a stable snapshot.

---

## Visual Design

### Node Types

Six distinct node types with consistent visual language:

**Content Flow Nodes (Primary Hierarchy)**

| Node Type | Color | Purpose |
|-----------|-------|---------|
| Scene | Purple | Narrative container - highest level grouping |
| Situation | Blue | Decision point with associated entity context |
| Choice | Green (executed) / Gray (unexecuted) | Player action that was or could be taken |

**Entity Reference Nodes (Context)**

| Node Type | Color | Purpose |
|-----------|-------|---------|
| Location | Orange | Where the Situation takes place |
| NPC | Red | Who the Situation involves |
| Route | Brown | Travel path the Situation uses |

### Node Content

Each node displays:
- Type badge (small label indicating node type)
- Primary label (name/title of the entity)
- Secondary info (category, state, system type as applicable)
- Visual indicator of state (completed, active, deferred for Scenes)

### Edge Types

Edges connect nodes with distinct styles indicating relationship type:

| Relationship | Style | Description |
|--------------|-------|-------------|
| Scene contains Situation | Solid dark | Hierarchical ownership |
| Situation presents Choice | Solid dark | Hierarchical ownership |
| Choice spawns Scene | Dashed green | Cross-link consequence |
| Choice spawns Situation | Dashed blue | Cascade consequence |
| Situation references Location | Dotted orange | Entity context |
| Situation references NPC | Dotted red | Entity context |
| Situation references Route | Dotted brown | Entity context |

---

## Data Architecture

### Source of Truth

The `ProceduralContentTracer` service (accessed via `GameFacade.GameWorld.ProceduralTracer`) contains all data needed:

- `RootScenes` - List of top-level SceneSpawnNodes (entry points to the graph)
- `AllSceneNodes` - Flat list of all scene nodes for filtering/searching
- `AllSituationNodes` - Flat list of all situation nodes
- `AllChoiceNodes` - Flat list of all choice execution nodes

### Graph Construction

A `SpawnGraphBuilder` service traverses the SpawnTrace collections and creates:

1. **Nodes** - One node per SceneSpawnNode, SituationSpawnNode, ChoiceExecutionNode
2. **Entity Nodes** - One node per unique LocationSnapshot, NPCSnapshot, RouteSnapshot (deduplicated by name)
3. **Edges** - Connections based on parent-child references and entity references

### Entity Deduplication

The same Location or NPC may appear in multiple Situations. The graph builder creates only one node per unique entity (identified by name) and draws multiple edges to it from different Situations. This shows entity reuse clearly.

---

## Layout Algorithm

### Why Dagre.js (Sugiyama Algorithm)

The graph is a Directed Acyclic Graph (DAG) with:
- Clear hierarchy (Scene → Situation → Choice)
- Cross-links (Choice → spawned Scene)
- Mixed branching (one Scene has many Situations; one Situation has many Choices)

The Sugiyama algorithm (implemented in Dagre.js) is purpose-built for DAGs:
- Assigns nodes to discrete ranks (layers)
- Minimizes edge crossings between layers
- Produces deterministic, repeatable layouts
- Fast enough for graphs with hundreds of nodes

Force-directed layouts (d3-force) were rejected because:
- Non-deterministic (different layout each time)
- Poor at preserving hierarchical structure
- Can produce chaotic results for DAGs with cross-links

### Layout Configuration

- Direction: Left-to-right (rank direction = LR)
- Node separation: Adequate spacing for labels
- Rank separation: Space between hierarchy levels
- Edge type: Polyline with orthogonal segments (optional curved fallback)

---

## Blazor Integration

### Library Choice: Z.Blazor.Diagrams

A native Blazor component library chosen for:
- 95% C# implementation (minimal JavaScript interop complexity)
- MIT license (no commercial restrictions)
- Active maintenance (v3.0.3, December 2024)
- Built-in pan, zoom, drag support
- Custom node/link widget support
- Model/UI separation for performance

### Integration Pattern

1. **DiagramCanvas** - Z.Blazor.Diagrams provides the canvas container with pan/zoom
2. **Custom NodeModel classes** - Extend library's NodeModel with SpawnTrace data
3. **Custom Node Widgets** - Razor components render each node type with appropriate styling
4. **GraphBuilder Service** - Converts ProceduralContentTracer data to diagram model
5. **Layout via JS Interop** - Dagre.js called via IJSRuntime to compute node positions

The separation allows:
- Data model changes without re-rendering canvas
- Layout computation in JavaScript (optimal performance)
- Node rendering in Blazor (type-safe, component-based)

---

## User Interaction

### Navigation

| Action | Behavior |
|--------|----------|
| Mouse drag on canvas | Pan the viewport |
| Mouse wheel | Zoom in/out |
| Click node | Select node, show detail panel |
| Double-click Scene | Zoom to fit that Scene's subtree |
| "Fit to View" button | Zoom to show entire graph |

### Detail Panel

When a node is selected, a side panel displays full information:
- All properties from the corresponding SpawnNode
- Timestamps (spawn time, activation time, completion time)
- Snapshot data (if entity node, show full snapshot properties)
- Links to parent and child nodes (clickable to navigate)

### Filtering

Toolbar controls allow filtering the visible graph:
- Node type toggles (show/hide Scenes, Situations, Choices, Entities)
- Category filter (MainStory, SideStory, Service)
- State filter (Active, Completed, Deferred)
- Search by name (highlights matching nodes)

### Refresh

A "Refresh" button rebuilds the graph from current ProceduralContentTracer state. This is necessary because:
- The tracer records events as they happen during gameplay
- The graph visualization is a snapshot, not live-updating
- Re-layout is computationally expensive, so it's user-triggered

---

## File Structure

### New Files to Create

**Pages**
- `src/Pages/SpawnGraph.razor` - Full-screen page with DiagramCanvas
- `src/Pages/SpawnGraph.razor.cs` - Page logic, graph construction orchestration

**Components (in src/Pages/Components/SpawnGraph/)**
- `SceneNodeWidget.razor` - Renders Scene nodes
- `SituationNodeWidget.razor` - Renders Situation nodes
- `ChoiceNodeWidget.razor` - Renders Choice nodes
- `EntityNodeWidget.razor` - Renders Location/NPC/Route nodes
- `DetailPanel.razor` - Side panel showing selected node details

**Models (in src/GameState/SpawnGraph/)**
- `SpawnGraphNodeModel.cs` - Base node model extending Z.Blazor.Diagrams NodeModel
- `SceneNodeModel.cs` - Scene-specific node model with SpawnNode reference
- `SituationNodeModel.cs` - Situation-specific node model
- `ChoiceNodeModel.cs` - Choice-specific node model
- `EntityNodeModel.cs` - Entity-specific node model (Location/NPC/Route)

**Services**
- `src/Services/SpawnGraphBuilder.cs` - Converts ProceduralContentTracer to diagram

**JavaScript**
- `src/wwwroot/js/dagre-layout.js` - Dagre.js wrapper for layout computation

**Styles**
- `src/wwwroot/css/spawngraph.css` - Graph-specific CSS

### Existing Files to Modify

- `src/Program.cs` - Register SpawnGraphBuilder service, configure Z.Blazor.Diagrams
- `src/Pages/Components/DebugPanel.razor` - Add link to /spawngraph page
- `src/wwwroot/index.html` - Include Dagre.js script (from CDN or local)
- `src/Wayfarer.csproj` - Add Z.Blazor.Diagrams NuGet package reference

---

## Implementation Phases

### Phase 1: Foundation

**Goal:** Basic canvas page that renders and allows pan/zoom

Tasks:
1. Add Z.Blazor.Diagrams NuGet package
2. Configure services in Program.cs
3. Create SpawnGraph.razor page with basic DiagramCanvas
4. Add route and navigation link
5. Verify pan/zoom works with placeholder nodes

### Phase 2: Node Rendering

**Goal:** Custom node widgets render SpawnTrace data

Tasks:
1. Create SpawnGraphNodeModel base class
2. Create Scene/Situation/Choice/Entity node model classes
3. Create widget components for each node type
4. Register widgets with diagram component
5. Test with manually-created nodes

### Phase 3: Graph Building

**Goal:** Full graph builds from ProceduralContentTracer data

Tasks:
1. Create SpawnGraphBuilder service
2. Implement traversal of RootScenes → Situations → Choices
3. Implement entity node creation and deduplication
4. Implement edge creation for all relationship types
5. Integrate Dagre.js layout via JS interop
6. Test with actual gameplay data

### Phase 4: Interactivity

**Goal:** Selection, detail panel, filtering

Tasks:
1. Implement node selection with visual highlight
2. Create DetailPanel component
3. Wire selection to detail panel display
4. Add filtering toolbar (node types, categories, search)
5. Implement "Fit to View" and "Refresh" buttons

### Phase 5: Polish

**Goal:** Visual refinement and edge cases

Tasks:
1. Add icons to nodes (from game-icons.net)
2. Refine color scheme and typography
3. Handle empty graph state gracefully
4. Add edge labels where helpful
5. Performance testing with large graphs
6. Add export as SVG/PNG (stretch goal)

---

## Success Criteria

The implementation is complete when:

1. Navigating to `/spawngraph` shows a full-screen canvas
2. All SpawnTrace nodes appear with correct visual styling
3. All edges correctly connect parent-child and entity relationships
4. Automatic layout produces readable graph without manual positioning
5. Pan/zoom navigation works smoothly
6. Clicking a node shows its full details in the side panel
7. Filtering controls hide/show node types as expected
8. Refresh button rebuilds graph from current game state
9. Performance is acceptable (< 500ms to render typical game graph)

---

## Dependencies

### NuGet Packages

- `Z.Blazor.Diagrams` - Core diagram component library

### JavaScript Libraries

- `dagre` - DAG layout algorithm (loaded via CDN or bundled)

### Existing Wayfarer Systems

- `ProceduralContentTracer` - Data source
- `SceneSpawnNode`, `SituationSpawnNode`, `ChoiceExecutionNode` - Node data
- `LocationSnapshot`, `NPCSnapshot`, `RouteSnapshot` - Entity snapshots
- `GameFacade` - Access to GameWorld.ProceduralTracer

---

## References

- Z.Blazor.Diagrams documentation: https://blazor-diagrams.zhaytam.com/
- Dagre.js repository: https://github.com/dagrejs/dagre
- Sugiyama algorithm: https://en.wikipedia.org/wiki/Layered_graph_drawing
- Existing SpawnGraphViewer: `src/Pages/Components/SpawnTrace/SpawnGraphViewer.razor`
