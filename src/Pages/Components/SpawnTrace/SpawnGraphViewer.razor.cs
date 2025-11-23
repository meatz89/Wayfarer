using Microsoft.AspNetCore.Components;

/// <summary>
/// Main viewer component for procedural content tracing
/// Displays spawn graph as interactive tree with filtering
/// </summary>
public partial class SpawnGraphViewer : ComponentBase
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
            .OrderBy(scene => scene.SpawnTimestamp)
            .ToList();

        StateHasChanged();
    }

    private bool MatchesFilters(SceneSpawnNode scene)
    {
        if (!string.IsNullOrEmpty(SearchText) &&
            !scene.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            return false;

        if (!string.IsNullOrEmpty(CategoryFilter))
        {
            if (!Enum.TryParse<StoryCategory>(CategoryFilter, out StoryCategory category))
                return false;

            if (scene.Category != category)
                return false;
        }

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

    private void ToggleExpand(string nodeId)
    {
        if (ExpandedNodeIds.Contains(nodeId))
        {
            ExpandedNodeIds.Remove(nodeId);
        }
        else
        {
            ExpandedNodeIds.Add(nodeId);
        }
        StateHasChanged();
    }

    private void ExpandAll()
    {
        ExpandedNodeIds.Clear();
        if (Tracer != null)
        {
            foreach (SceneSpawnNode node in Tracer.AllSceneNodes)
                ExpandedNodeIds.Add(node.NodeId);

            foreach (SituationSpawnNode node in Tracer.AllSituationNodes)
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
