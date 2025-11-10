/// <summary>
/// Data Transfer Object for deserializing Scene runtime instances from JSON
/// Represents a specific playthrough instance spawned from a SceneTemplate
/// Maps to Scene domain entity
/// </summary>
public class SceneDTO
{
/// <summary>
/// Unique identifier for this scene instance
/// Generated at spawn time: "scene_{templateId}_{guid}"
/// </summary>
public string Id { get; set; }

/// <summary>
/// Template identifier - which SceneTemplate spawned this instance
/// References SceneTemplate in GameWorld.SceneTemplates
/// Used for save/load persistence and template property lookup
/// </summary>
public string TemplateId { get; set; }

/// <summary>
/// Where this scene is placed (Location/NPC/Route)
/// Values: "Location", "NPC", "Route"
/// Determines which GameWorld collection to query for placement entity
/// </summary>
public string PlacementType { get; set; }

/// <summary>
/// Concrete entity ID where this scene is placed
/// LocationId, NpcId, or RouteId depending on PlacementType
/// Resolved from PlacementFilter categorical queries at spawn time
/// </summary>
public string PlacementId { get; set; }

/// <summary>
/// Current lifecycle state
/// Values: "Provisional", "Active", "Completed", "Expired"
/// </summary>
public string State { get; set; }

/// <summary>
/// Day when this scene expires (time-limited scenes)
/// null = no expiration (persists until completed)
/// </summary>
public int? ExpiresOnDay { get; set; }

/// <summary>
/// Spawn pattern archetype
/// Values: "Linear", "Branching", "Converging", "Standalone"
/// Mirrors template archetype for runtime reference
/// </summary>
public string Archetype { get; set; }

/// <summary>
/// Generated display name with placeholders replaced
/// Example: "The Plea of Elena" (from template "{NPCName}'s Plea")
/// </summary>
public string DisplayName { get; set; }

/// <summary>
/// Generated intro narrative with placeholders replaced
/// Shown when scene first becomes available
/// </summary>
public string IntroNarrative { get; set; }

/// <summary>
/// How this scene presents to the player
/// Values: "Atmospheric", "Modal"
/// Atmospheric = menu option, Modal = fullscreen takeover
/// </summary>
public string PresentationMode { get; set; }

/// <summary>
/// How situations progress through the scene
/// Values: "Breathe", "Cascade"
/// Breathe = return to menu after each situation
/// Cascade = continuous flow through situations
/// </summary>
public string ProgressionMode { get; set; }

/// <summary>
/// Story category classification (copied from template)
/// Values: "MainStory", "SideStory", "Service"
/// MainStory = A-story progression (infinite main quest)
/// SideStory = B-story content (optional side content)
/// Service = C-story content (repeatable services)
/// </summary>
public string Category { get; set; }

/// <summary>
/// Main story sequence number (copied from template)
/// 1-10 = Authored tutorial scenes
/// 11+ = Procedural continuation (infinite)
/// null = Not part of A-story
/// </summary>
public int? MainStorySequence { get; set; }

/// <summary>
/// Situation ID player is currently engaged with
/// null = scene not started or completed
/// References Situation.Id in embedded Situations collection
/// </summary>
public string CurrentSituationId { get; set; }

/// <summary>
/// Source situation that spawned this provisional scene
/// null for finalized scenes
/// Used for cleanup if provisional scene expires
/// </summary>
public string SourceSituationId { get; set; }

/// <summary>
/// Situation spawn rules defining cascade structure
/// Controls how situations lead into each other
/// </summary>
public SituationSpawnRulesDTO SpawnRules { get; set; }

/// <summary>
/// Embedded situations owned by this scene
/// Scene OWNS situations (composition pattern)
/// Situations are NOT in GameWorld.Situations, only in this collection
/// </summary>
public List<SituationDTO> Situations { get; set; } = new List<SituationDTO>();

/// <summary>
/// Location IDs created by this scene (self-contained pattern)
/// Forensic tracking for dependent resources
/// </summary>
public List<string> CreatedLocationIds { get; set; } = new List<string>();

/// <summary>
/// Item IDs created by this scene (self-contained pattern)
/// Forensic tracking for dependent resources
/// </summary>
public List<string> CreatedItemIds { get; set; } = new List<string>();

/// <summary>
/// Dynamic package ID for dependent resources
/// Format: "scene_{sceneId}_dep"
/// null = no dependent resources generated
/// </summary>
public string DependentPackageId { get; set; }

/// <summary>
/// Marker resolution map for self-contained scenes
/// Maps template markers to actual resource IDs
/// Example: "generated:private_room" → "scene_abc123_private_room"
/// </summary>
public Dictionary<string, string> MarkerResolutionMap { get; set; } = new Dictionary<string, string>();
}
