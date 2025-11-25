# Procedural Content Tracing System - Implementation Plan

## Status: Phase 1 Complete ✅, Phase 2-5 Remaining

---

## PHASE 2: INTEGRATION HOOKS (Critical - Must Complete Next)

### Hook 1: SceneParser.ConvertDTOToScene
**File:** `/home/user/Wayfarer/src/Content/SceneParser.cs`
**Location:** After line 218 (after scene fully constructed with all situations)

**Implementation:**
```csharp
// After line 218: return scene;
// ADD BEFORE return:

// PROCEDURAL CONTENT TRACING: Record initial scene spawn (authored content)
if (gameWorld.ProceduralTracer != null && gameWorld.ProceduralTracer.IsEnabled)
{
    // Get player from gameWorld (need to find player reference)
    Player player = gameWorld.GetPlayer(); // TODO: Verify correct method

    SceneSpawnNode sceneNode = gameWorld.ProceduralTracer.RecordSceneSpawn(
        scene,
        dto.TemplateId,
        isProcedurallyGenerated: false, // Authored content from JSON
        SpawnTriggerType.Initial,
        player
    );

    // Record all initial situations
    foreach (Situation situation in scene.Situations)
    {
        gameWorld.ProceduralTracer.RecordSituationSpawn(
            situation,
            sceneNode.NodeId,
            SituationSpawnTriggerType.InitialScene
        );
    }
}

return scene;
```

### Hook 2: SceneInstanceFacade.SpawnScene
**File:** `/home/user/Wayfarer/src/Subsystems/ProceduralContent/SceneInstanceFacade.cs`
**Location:** After scene spawn completes (around line 114)

**Implementation:**
```csharp
// After scene successfully spawned, BEFORE return
if (gameWorld.ProceduralTracer != null && gameWorld.ProceduralTracer.IsEnabled)
{
    SceneSpawnNode sceneNode = gameWorld.ProceduralTracer.RecordSceneSpawn(
        scene,
        template.Id,
        isProcedurallyGenerated: true,
        SpawnTriggerType.ChoiceReward, // Or determine from context
        player
    );

    // Record all situations (they're created during parsing)
    foreach (Situation situation in scene.Situations)
    {
        gameWorld.ProceduralTracer.RecordSituationSpawn(
            situation,
            sceneNode.NodeId,
            SituationSpawnTriggerType.InitialScene
        );
    }
}
```

### Hook 3: RewardApplicationService.ApplyChoiceReward
**File:** `/home/user/Wayfarer/src/Subsystems/Consequence/RewardApplicationService.cs`
**Location:** TWO-PHASE - before reward application AND after spawns complete

**Phase 1 - Record Choice Execution (BEFORE applying rewards):**
```csharp
// At method start, BEFORE applying any rewards
ChoiceExecutionNode choiceNode = null;
if (gameWorld.ProceduralTracer != null && gameWorld.ProceduralTracer.IsEnabled)
{
    string situationNodeId = gameWorld.ProceduralTracer.GetNodeIdForSituation(currentSituation);

    choiceNode = gameWorld.ProceduralTracer.RecordChoiceExecution(
        choiceTemplate,
        situationNodeId,
        choiceTemplate.ActionTextTemplate, // Or resolved text
        playerMetRequirements: true // Determine actual value
    );

    // Push choice context for auto-linking spawned scenes
    gameWorld.ProceduralTracer.PushChoiceContext(choiceNode.NodeId);
}
```

**Phase 2 - Pop Context (AFTER all spawns complete):**
```csharp
// At method end, AFTER all rewards applied and scenes spawned
if (gameWorld.ProceduralTracer != null && gameWorld.ProceduralTracer.IsEnabled)
{
    gameWorld.ProceduralTracer.PopChoiceContext();
}
```

### Hook 4: Scene State Transitions
**Files:** Wherever `scene.State =` is assigned

**Implementation:**
```csharp
// Whenever scene state changes
scene.State = SceneState.Active; // or Completed, Expired

if (gameWorld.ProceduralTracer != null && gameWorld.ProceduralTracer.IsEnabled)
{
    gameWorld.ProceduralTracer.UpdateSceneState(scene, scene.State, DateTime.UtcNow);
}
```

**Search for state transitions:**
```bash
grep -rn "\.State = SceneState\." src/
```

---

## PHASE 3: VISUALIZATION COMPONENTS

### Component 1: CSS Styling
**File:** `/home/user/Wayfarer/src/wwwroot/css/spawn-trace.css`

```css
.trace-tree {
    padding: 20px;
    font-family: 'Consolas', 'Monaco', monospace;
    background: #1e1e1e;
    color: #d4d4d4;
    font-size: 13px;
    line-height: 1.5;
}

.trace-node {
    margin: 8px 0 8px 20px;
    padding: 12px;
    border-left: 3px solid #444;
    background: #2d2d2d;
    border-radius: 4px;
    cursor: pointer;
    transition: all 0.2s;
}

.trace-node:hover {
    background: #3a3a3a;
}

.trace-node-scene {
    border-left-color: #4ec9b0;
    margin-left: 0;
}

.trace-node-situation {
    border-left-color: #dcdcaa;
}

.trace-node-choice {
    border-left-color: #569cd6;
}

.trace-node-header {
    font-weight: bold;
    font-size: 14px;
    margin-bottom: 6px;
    display: flex;
    align-items: center;
    gap: 8px;
}

.trace-node-metadata {
    font-size: 12px;
    color: #888;
    line-height: 1.8;
}

.trace-badge {
    display: inline-block;
    padding: 2px 8px;
    background: #444;
    border-radius: 3px;
    font-size: 10px;
    font-weight: bold;
    letter-spacing: 0.5px;
}

.scene-main-story { border-left-color: #f14c4c; }
.scene-side-story { border-left-color: #3794ff; }
.scene-service { border-left-color: #89d185; }

.trace-node-collapsed > .trace-node-children {
    display: none;
}

.trace-node-expanded > .trace-node-children {
    display: block;
}

.trace-node-highlighted {
    background: #3a3a00 !important;
    box-shadow: 0 0 8px #ffff00;
}

.trace-link {
    color: #4fc1ff;
    cursor: pointer;
    text-decoration: underline;
}

.trace-link:hover {
    color: #7fdbff;
}

.filter-panel {
    background: #2d2d2d;
    padding: 16px;
    margin-bottom: 20px;
    border-radius: 4px;
    display: flex;
    gap: 12px;
    flex-wrap: wrap;
    align-items: center;
}

.filter-panel input,
.filter-panel select,
.filter-panel button {
    padding: 6px 12px;
    background: #1e1e1e;
    color: #d4d4d4;
    border: 1px solid #444;
    border-radius: 3px;
    font-family: inherit;
    font-size: 13px;
}

.filter-panel button {
    background: #3794ff;
    border: none;
    cursor: pointer;
}

.filter-panel button:hover {
    background: #5aa4ff;
}
```

### Component 2: SpawnGraphViewer.razor
**File:** `/home/user/Wayfarer/src/Pages/Components/SpawnTrace/SpawnGraphViewer.razor`

```razor
@if (Tracer == null || !Tracer.IsEnabled)
{
    <div class="trace-tree">
        <p>Procedural content tracing is disabled.</p>
    </div>
}
else
{
    <div class="spawn-graph-viewer">
        <div class="filter-panel">
            <input type="text" @bind="SearchText" @bind:event="oninput" placeholder="Search scenes..." />

            <select @bind="CategoryFilter">
                <option value="">All Categories</option>
                <option value="MainStory">Main Story</option>
                <option value="SideStory">Side Story</option>
                <option value="Service">Service</option>
            </select>

            <label>Day:</label>
            <input type="number" @bind="MinDay" placeholder="Min" style="width: 80px" />
            <input type="number" @bind="MaxDay" placeholder="Max" style="width: 80px" />

            <button @onclick="ApplyFilters">Apply</button>
            <button @onclick="ClearFilters">Clear</button>
            <button @onclick="ExpandAll">Expand All</button>
            <button @onclick="CollapseAll">Collapse All</button>
        </div>

        <div class="trace-tree">
            @if (FilteredRootScenes == null || FilteredRootScenes.Count == 0)
            {
                <p>No scenes to display. Scenes will appear here as they spawn during gameplay.</p>
            }
            else
            {
                @foreach (var rootScene in FilteredRootScenes)
                {
                    <SceneNode Node="@rootScene"
                               Tracer="@Tracer"
                               OnNodeSelected="@SelectNode"
                               IsExpanded="@ExpandedNodeIds.Contains(rootScene.NodeId)"
                               IsHighlighted="@HighlightedNodeIds.Contains(rootScene.NodeId)" />
                }
            }
        </div>
    </div>
}
```

### Component 3: SpawnGraphViewer.razor.cs
**File:** `/home/user/Wayfarer/src/Pages/Components/SpawnTrace/SpawnGraphViewer.razor.cs`

```csharp
using Microsoft.AspNetCore.Components;

public partial class SpawnGraphViewer
{
    [Parameter] public ProceduralContentTracer Tracer { get; set; }

    private List<SceneSpawnNode> FilteredRootScenes { get; set; } = new List<SceneSpawnNode>();
    private string SearchText { get; set; } = "";
    private string CategoryFilter { get; set; } = "";
    private int? MinDay { get; set; } = null;
    private int? MaxDay { get; set; } = null;
    private string SelectedNodeId { get; set; }
    private HashSet<string> ExpandedNodeIds { get; set; } = new HashSet<string>();
    private HashSet<string> HighlightedNodeIds { get; set; } = new HashSet<string>();

    protected override void OnParametersSet()
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (Tracer == null || Tracer.RootScenes == null)
        {
            FilteredRootScenes = new List<SceneSpawnNode>();
            return;
        }

        FilteredRootScenes = Tracer.RootScenes
            .Where(scene => MatchesFilters(scene))
            .ToList();

        StateHasChanged();
    }

    private bool MatchesFilters(SceneSpawnNode scene)
    {
        if (!string.IsNullOrEmpty(SearchText) &&
            !scene.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrEmpty(CategoryFilter) &&
            scene.Category.ToString() != CategoryFilter)
            return false;

        if (MinDay.HasValue && scene.GameDay < MinDay.Value)
            return false;

        if (MaxDay.HasValue && scene.GameDay > MaxDay.Value)
            return false;

        return true;
    }

    private void ClearFilters()
    {
        SearchText = "";
        CategoryFilter = "";
        MinDay = null;
        MaxDay = null;
        ApplyFilters();
    }

    private void SelectNode(string nodeId)
    {
        SelectedNodeId = nodeId;
        StateHasChanged();
    }

    private void ExpandAll()
    {
        ExpandedNodeIds.Clear();
        if (Tracer != null)
        {
            foreach (var node in Tracer.AllSceneNodes)
                ExpandedNodeIds.Add(node.NodeId);
        }
        StateHasChanged();
    }

    private void CollapseAll()
    {
        ExpandedNodeIds.Clear();
        StateHasChanged();
    }
}
```

### Component 4-6: Node Components

See design document PROCEDURAL_CONTENT_TRACING.md for complete SceneNode.razor, SituationNode.razor, and ChoiceNode.razor implementations.

### Wire into DebugPanel.razor

**Add after existing debug sections:**
```razor
<div class="debug-section">
    <h3 @onclick="ToggleSpawnTrace">Spawn Trace</h3>
    @if (showSpawnTrace)
    {
        <SpawnGraphViewer Tracer="@GameWorld.ProceduralTracer" />
    }
</div>
```

---

## PHASE 4: INTERACTIVE FEATURES

See design document for:
- Search/filter implementation (already in SpawnGraphViewer above)
- Jump-to-node navigation
- Trace-to-root path highlighting
- Detail panel

---

## PHASE 5: POLISH & VERIFICATION

- Timeline view (optional)
- Export to JSON
- Statistical summary
- End-to-end testing
- Verification of parent-child links

---

## NEXT STEPS

1. **Complete Phase 2 Integration Hooks** (CRITICAL)
   - Add tracing calls to SceneParser
   - Add tracing calls to SceneInstanceFacade
   - Add tracing calls to RewardApplicationService
   - Find and hook scene state transitions

2. **Implement Phase 3 Visualization** (HIGH PRIORITY)
   - Create CSS file
   - Create all Razor components
   - Wire into DebugPanel

3. **Test End-to-End** (VERIFICATION)
   - Start game
   - Play through tutorial
   - Execute choices that spawn scenes
   - Open debug panel
   - Verify trace graph displays correctly

4. **Implement Phase 4 & 5** (POLISH)
   - Add interactive features
   - Optimize performance
   - Add export functionality

---

## FILES CREATED SO FAR (16 files)

✅ All Phase 1 foundation files complete
✅ GameWorld.ProceduralTracer property added
✅ Program.cs tracer injection added

**Remaining:** ~12 files (Razor components + integration hooks)
