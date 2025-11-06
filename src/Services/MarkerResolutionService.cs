/// <summary>
/// Service for resolving markers to actual resource IDs
/// Self-contained scene pattern: "generated:{templateId}" → actual created ID
/// Used at finalization time (building map) and execution time (resolving references)
/// </summary>
public class MarkerResolutionService
{
    /// <summary>
    /// Resolve marker to actual resource ID using marker resolution map
    /// If ID is in the marker map, return the resolved ID
    /// If ID is not in the marker map, return as-is (may be actual ID or non-marker)
    /// null/empty IDs return null (no marker resolution needed)
    /// </summary>
    public string ResolveMarker(string id, Dictionary<string, string> markerMap)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        // Check if this ID is in the marker map
        if (markerMap.TryGetValue(id, out string resolvedId))
        {
            Console.WriteLine($"[MarkerResolutionService] Resolved marker '{id}' → '{resolvedId}'");
            return resolvedId;
        }

        // Not a marker (or actual ID), return as-is
        return id;
    }

    /// <summary>
    /// Build marker resolution map for scene
    /// Maps "generated:{templateId}" → actual created entity IDs
    /// Called at finalization time after dependent resources are created
    /// </summary>
    public Dictionary<string, string> BuildMarkerResolutionMap(Scene scene)
    {
        Dictionary<string, string> map = new Dictionary<string, string>();

        if (scene.Template.DependentLocations == null || scene.Template.DependentItems == null)
            return map;

        // Map location markers
        for (int i = 0; i < scene.Template.DependentLocations.Count; i++)
        {
            if (i < scene.CreatedLocationIds.Count)
            {
                DependentLocationSpec spec = scene.Template.DependentLocations[i];
                string marker = $"generated:{spec.TemplateId}";
                string actualId = scene.CreatedLocationIds[i];
                map[marker] = actualId;
                Console.WriteLine($"[MarkerResolutionService] Mapped location marker: {marker} → {actualId}");
            }
        }

        // Map item markers
        for (int i = 0; i < scene.Template.DependentItems.Count; i++)
        {
            if (i < scene.CreatedItemIds.Count)
            {
                DependentItemSpec spec = scene.Template.DependentItems[i];
                string marker = $"generated:{spec.TemplateId}";
                string actualId = scene.CreatedItemIds[i];
                map[marker] = actualId;
                Console.WriteLine($"[MarkerResolutionService] Mapped item marker: {marker} → {actualId}");
            }
        }

        return map;
    }

    /// <summary>
    /// Resolve list of IDs (for rewards, requirements, etc.)
    /// Returns new list with markers resolved to actual IDs
    /// </summary>
    public List<string> ResolveMarkers(List<string> ids, Dictionary<string, string> markerMap)
    {
        if (ids == null || ids.Count == 0)
            return new List<string>();

        List<string> resolved = new List<string>();
        foreach (string id in ids)
        {
            resolved.Add(ResolveMarker(id, markerMap));
        }
        return resolved;
    }
}
