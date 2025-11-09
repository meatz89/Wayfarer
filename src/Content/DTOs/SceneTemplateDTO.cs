/// <summary>
/// DTO for SceneTemplate - immutable archetype that spawns Scene instances
/// Orchestrates spawning of multiple related Situations with placement strategies
/// Maps to SceneTemplate domain entity
/// </summary>
public class SceneTemplateDTO
{
/// <summary>
/// Unique template identifier
/// </summary>
public string Id { get; set; }

/// <summary>
/// Scene pattern classification (Linear, HubAndSpoke, Branching, Converging, Discovery, etc.)
/// Maps to SpawnPattern enum
/// </summary>
public string Archetype { get; set; }

/// <summary>
/// Display name template with {placeholders} for entity names
/// Example: "The Mystery of {LocationName}"
/// </summary>
public string DisplayNameTemplate { get; set; }

/// <summary>
/// Categorical filters for procedural entity selection
/// Determines what NPCs/Locations/Routes match this Scene's context
/// </summary>
public PlacementFilterDTO PlacementFilter { get; set; }

/// <summary>
/// Temporal eligibility conditions for scene spawning
/// null = always eligible (no temporal filtering)
/// Defines player state, world state, and entity state requirements
/// </summary>
public SpawnConditionsDTO SpawnConditions { get; set; }

/// <summary>
/// Optional time limit before Scene expires
/// null = no expiration
/// </summary>
public int? ExpirationDays { get; set; }

/// <summary>
/// Flag for initial game setup scenes
/// </summary>
public bool IsStarter { get; set; } = false;

/// <summary>
/// Intro narrative template with {placeholders}
/// Example: "As you approach {LocationName}, {NPCName} appears..."
/// </summary>
public string IntroNarrativeTemplate { get; set; }

/// <summary>
/// Complexity tier (0-4)
/// Tier 0: Safety net, Tier 1: Low, Tier 2: Standard, Tier 3: High, Tier 4: Climactic
/// </summary>
public int Tier { get; set; } = 1;

/// <summary>
/// Presentation mode - how this Scene appears to the player
/// "Atmospheric": Scene appears as menu option (existing behavior)
/// "Modal": Scene takes over full screen on location entry (Sir Brante forced moment)
/// Defaults to "Atmospheric" if not specified
/// </summary>
public string PresentationMode { get; set; }

/// <summary>
/// Progression mode - how situations within this Scene flow after choices
/// "Breathe": Return to menu after each situation (player-controlled pacing)
/// "Cascade": Continue to next situation immediately (pressure and momentum)
/// Defaults to "Breathe" if not specified
/// </summary>
public string ProgressionMode { get; set; }

/// <summary>
/// Scene archetype ID for procedural scene generation
/// SceneArchetypeCatalog generates complete SituationTemplates array at PARSE TIME
/// REQUIRED for all scenes - defines both structure and content
///
/// Multi-situation archetypes (2-4 situations):
///   - "service_with_location_access": 4-situation service arc (negotiate→access→service→depart)
///   - "transaction_sequence": 3-situation trade arc (browse→negotiate→complete)
///   - "gatekeeper_sequence": 2-situation authority arc (confront→pass)
///   - "inn_crisis_escalation": 4-situation escalating conflict
///
/// Single-situation archetypes (1 situation with 4-choice pattern):
///   - "single_negotiation": Diplomatic trade/bargaining
///   - "single_confrontation": Authority challenge/dominance
///   - "single_investigation": Analytical puzzle/discovery
///   - "single_social_maneuvering": Rapport building/manipulation
///   - "single_crisis": Emergency response/decisive action
///   - "single_service_transaction": Simple service request
///   - "consequence_reflection": Consequence acknowledgment
///
/// Scene archetype defines WHAT the scene contains (design)
/// PlacementFilter defines WHERE/WHEN it appears (configuration)
/// </summary>
public string SceneArchetypeId { get; set; }

/// <summary>
/// Service type for service_with_location_access archetype
/// Values: "lodging", "bathing", "healing", "storage", "training"
/// Ignored if SceneArchetypeId is null or not service-related
/// Used to generate service-specific rewards and narrative hints
/// </summary>
public string ServiceType { get; set; }

/// <summary>
/// Dependent location specifications for self-contained scenes
/// Scene dynamically creates these locations via JSON package generation
/// Empty/null = Scene uses only pre-existing locations (traditional pattern)
/// Self-contained pattern: Catalogue generates specs → Parser converts to domain → SceneInstantiator creates resources
/// </summary>
public List<DependentLocationSpecDTO> DependentLocations { get; set; }

/// <summary>
/// Dependent item specifications for self-contained scenes
/// Scene dynamically creates these items via JSON package generation
/// Empty/null = Scene uses only pre-existing items (traditional pattern)
/// Self-contained pattern: Catalogue generates specs → Parser converts to domain → SceneInstantiator creates resources
/// Example: Room keys, special access permits, quest items
/// </summary>
public List<DependentItemSpecDTO> DependentItems { get; set; }
}
