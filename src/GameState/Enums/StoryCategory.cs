
/// <summary>
/// Categorizes scene content by narrative role and player agency.
/// A/B/C stories are distinguished by a combination of properties, not a single axis.
/// All three use identical Scene-Situation-Choice structure; category determines rules and validation.
/// </summary>
public enum StoryCategory
{
    /// <summary>
    /// Main narrative progression (A-stories)
    /// Infinite sequential chain. Scenes chain: A1 → A2 → A3...
    /// Fallback required in every situation. Can never fail.
    /// Resource SINK (travel costs to reach). Player cannot decline.
    /// Primary purpose: world expansion (venues, districts, regions, routes, NPCs).
    /// </summary>
    MainStory,

    /// <summary>
    /// Quest content providing resources (B-stories)
    /// One scene with 3-8 situations forming a complete arc.
    /// Player CHOOSES to engage via job board, NPC quest giver, or peculiar location.
    /// Can accept or decline. Can fail with consequences. No mandatory fallback.
    /// Primary resource SOURCE. Significant rewards fund A-story travel.
    /// </summary>
    SideStory,

    /// <summary>
    /// Single-scene texture encounters (C-stories)
    /// Spawns probabilistically when player enters location with no active A/B scene.
    /// Spawn chance based on: location properties, player state (tired/hungry/cold), world state (weather).
    /// Player cannot decline or willingly spawn—surprises that flesh out the world.
    /// World TEXTURE (minor rewards). System-repeatable but player has no control.
    /// </summary>
    Encounter
}
