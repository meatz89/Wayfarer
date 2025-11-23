/// <summary>
/// Immutable snapshot of PlacementFilter used at spawn time
/// Captures categorical search criteria for debugging entity resolution
/// </summary>
public class PlacementFilterSnapshot
{
    public PlacementType PlacementType { get; set; }
    public PlacementSelectionStrategy SelectionStrategy { get; set; }

    // NPC filters
    public List<PersonalityType> PersonalityTypes { get; set; }
    public List<Professions> Professions { get; set; }
    public List<NPCSocialStanding> SocialStandings { get; set; }
    public List<NPCStoryRole> StoryRoles { get; set; }

    // Location filters
    public List<LocationTypes> LocationTypes { get; set; }
    public List<LocationPrivacy> PrivacyLevels { get; set; }
    public List<LocationSafety> SafetyLevels { get; set; }
    public List<LocationActivity> ActivityLevels { get; set; }
    public List<LocationPurpose> Purposes { get; set; }

    // Route filters
    public List<string> TerrainTypes { get; set; }
    public int? RouteTier { get; set; }
}
