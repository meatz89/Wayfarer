/// <summary>
/// DTO for path card data from JSON packages
/// </summary>
public class PathCardDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool StartsRevealed { get; set; } = false;
    public bool IsHidden { get; set; } = false;
    public int ExplorationThreshold { get; set; } = 0;
    public bool IsOneTime { get; set; } = false;
    public string NarrativeText { get; set; }

    // Core Loop: Optional scene on this path (references GameWorld.Scenes)
    // Player can preview scene and see equipment applicability before committing
    public string SceneId { get; set; }
}