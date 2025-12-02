/// <summary>
/// Strongly-typed context for Scene/Situation narrative prompt generation.
/// Bundles entity objects (NPC, Location, Player) with complete properties for AI generation.
/// Used by SceneNarrativeService to generate contextually appropriate narrative.
/// </summary>
public class ScenePromptContext
{
    // Entity references (complete objects, not just IDs - for rich property access)
    public NPC NPC { get; set; }
    public Location Location { get; set; }
    public Player Player { get; set; }
    public RouteOption Route { get; set; }

    // Situation/Scene metadata
    public string ArchetypeId { get; set; }
    public string SceneDisplayName { get; set; }

    // Narrative hints from template
    public string Tone { get; set; }
    public string Theme { get; set; }
    public string Context { get; set; }
    public string Style { get; set; }

    // World state
    public TimeBlocks CurrentTimeBlock { get; set; }
    public string CurrentWeather { get; set; }
    public int CurrentDay { get; set; }

    // Player relationship history (for contextual generation)
    public int NPCBondLevel { get; set; }

    /// <summary>
    /// Validate that required entities are present for AI generation
    /// Throws InvalidOperationException if critical context missing
    /// </summary>
    public void ValidateRequiredContext(PlacementType placementType)
    {
        if (Player == null)
            throw new InvalidOperationException("ScenePromptContext requires Player entity");

        switch (placementType)
        {
            case PlacementType.NPC:
                if (NPC == null)
                    throw new InvalidOperationException("NPC placement requires NPC entity in context");
                break;
            case PlacementType.Location:
                if (Location == null)
                    throw new InvalidOperationException("Location placement requires Location entity in context");
                break;
            case PlacementType.Route:
                if (Route == null)
                    throw new InvalidOperationException("Route placement requires Route entity in context");
                break;
        }
    }

}
