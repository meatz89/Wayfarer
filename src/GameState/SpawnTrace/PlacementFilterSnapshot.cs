/// <summary>
/// Immutable snapshot of PlacementFilter used at spawn time
/// Captures categorical search criteria for debugging entity resolution
/// </summary>
public class PlacementFilterSnapshot
{
    public PlacementType PlacementType { get; set; }
    public PlacementSelectionStrategy SelectionStrategy { get; set; }

    // NPC filters
    public PersonalityType? PersonalityType { get; set; }
    public Professions? Profession { get; set; }
    public NPCSocialStanding? SocialStanding { get; set; }
    public NPCStoryRole? StoryRole { get; set; }

    // Location filters
    public LocationRole? LocationRole { get; set; }
    public LocationPrivacy? Privacy { get; set; }
    public LocationSafety? Safety { get; set; }
    public LocationActivity? Activity { get; set; }
    public LocationPurpose? Purpose { get; set; }

    // Route filters (orthogonal)
    public TerrainType? Terrain { get; set; }
    public StructureType? Structure { get; set; }
}
