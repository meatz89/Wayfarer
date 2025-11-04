using Wayfarer.GameState;

namespace Wayfarer.Models;

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
    public int Tier { get; set; }
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
    public string PriorChoicesWithNPC { get; set; } // TODO: Populate from Player.ChoiceHistory when implemented

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

    /// <summary>
    /// Convert to dictionary for template processing compatibility
    /// Used by PromptBuilder for string interpolation in AI prompts
    /// </summary>
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        // Basic context
        dict["tier"] = Tier.ToString();
        dict["archetype_id"] = ArchetypeId ?? "unknown";
        dict["scene_display_name"] = SceneDisplayName ?? "Unknown Scene";
        dict["tone"] = Tone ?? "neutral";
        dict["theme"] = Theme ?? "general";
        dict["context"] = Context ?? "standard";
        dict["style"] = Style ?? "descriptive";

        // World state
        dict["time_block"] = CurrentTimeBlock.ToString();
        dict["weather"] = CurrentWeather ?? "clear";
        dict["current_day"] = CurrentDay.ToString();

        // NPC context (if present)
        if (NPC != null)
        {
            dict["npc_name"] = NPC.Name;
            dict["npc_personality"] = NPC.PersonalityType.ToString();
            dict["npc_personality_description"] = NPC.PersonalityDescription ?? "Unknown personality";
            dict["npc_bond_level"] = NPCBondLevel.ToString();
            dict["prior_choices_with_npc"] = PriorChoicesWithNPC ?? "No prior interactions";
        }

        // Location context (if present)
        if (Location != null)
        {
            dict["location_name"] = Location.Name;
            dict["location_description"] = Location.Description ?? "A location";
            dict["location_properties"] = string.Join(", ", Location.LocationProperties.Select(p => p.ToString()));
        }

        // Route context (if present)
        if (Route != null)
        {
            dict["route_name"] = Route.Name;
            dict["route_description"] = Route.Description ?? "A route";
            dict["route_danger"] = Route.DangerRating.ToString();
        }

        // Player context
        if (Player != null)
        {
            dict["player_authority"] = Player.Stats.GetLevel(PlayerStatType.Authority).ToString();
            dict["player_diplomacy"] = Player.Stats.GetLevel(PlayerStatType.Diplomacy).ToString();
            dict["player_rapport"] = Player.Stats.GetLevel(PlayerStatType.Rapport).ToString();
            dict["player_insight"] = Player.Stats.GetLevel(PlayerStatType.Insight).ToString();
            dict["player_cunning"] = Player.Stats.GetLevel(PlayerStatType.Cunning).ToString();
        }

        return dict;
    }
}
