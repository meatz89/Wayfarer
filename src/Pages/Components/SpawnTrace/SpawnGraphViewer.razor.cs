using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components.SpawnTrace;

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
    private object SelectedNode { get; set; }
    private HashSet<object> ExpandedNodes { get; set; } = new HashSet<object>();
    private HashSet<object> HighlightedNodes { get; set; } = new HashSet<object>();

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

    private void SelectNode(object node)
    {
        SelectedNode = node;
        StateHasChanged();
    }

    private void ToggleExpand(object node)
    {
        if (ExpandedNodes.Contains(node))
        {
            ExpandedNodes.Remove(node);
        }
        else
        {
            ExpandedNodes.Add(node);
        }
        StateHasChanged();
    }

    private void ExpandAll()
    {
        ExpandedNodes.Clear();
        if (Tracer != null)
        {
            foreach (SceneSpawnNode node in Tracer.AllSceneNodes)
                ExpandedNodes.Add(node);

            foreach (SituationSpawnNode node in Tracer.AllSituationNodes)
                ExpandedNodes.Add(node);
        }
        StateHasChanged();
    }

    private void CollapseAll()
    {
        ExpandedNodes.Clear();
        StateHasChanged();
    }
}
