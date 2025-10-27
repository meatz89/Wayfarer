using System;
using System.Collections.Generic;

// Simple ViewModels for UI data transfer - no business logic
// All content comes from JSON or mechanical states

public class LocationScreenViewModel
{
    public string CurrentTime { get; set; }
    public List<string> LocationPath { get; set; } = new();
    public string LocationName { get; set; }
    public List<string> LocationTraits { get; set; } = new();
    public string CurrentSpotName { get; set; }
    public string AtmosphereText { get; set; }
    public string Familiarity { get; set; }
    public List<LocationActionViewModel> QuickActions { get; set; } = new();
    public List<NPCInteractionViewModel> NPCsPresent { get; set; } = new();
    public string ObservationHeader { get; set; } = "Observations Available";
    public List<ObservationViewModel> Observations { get; set; } = new();
    public List<AreaWithinLocationViewModel> AreasWithinLocation { get; set; } = new();
    public List<RouteOptionViewModel> Routes { get; set; } = new();
    public TravelProgressViewModel TravelProgress { get; set; }
}

public class LocationActionViewModel
{
    public string Id { get; set; }
    public string Icon { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
    public string Cost { get; set; }
    public string ActionType { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string LockReason { get; set; }
    public string EngagementType { get; set; }
    public string ObligationLabel { get; set; }
}

public class NPCInteractionViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ConnectionStateName { get; set; }
    public string Description { get; set; }
    public List<InteractionOptionViewModel> Interactions { get; set; } = new();
}

public class InteractionOptionViewModel
{
    public string Text { get; set; }
    public string Cost { get; set; }
    public string ConversationTypeId { get; set; } = "friendly_chat";
}

public class ObservationViewModel
{
    public string Id { get; set; }  // Unique identifier for the observation
    public string Icon { get; set; }
    public string Text { get; set; }
    public bool IsUnknown { get; set; }
    public string Relevance { get; set; }
    public bool IsObserved { get; set; }
}

// View model for the GetObservationsViewModel method
public class ObservationsViewModel
{
    public List<ObservationSummaryViewModel> AvailableObservations { get; set; } = new();
}

// Summary view model for UI display of observations
public class ObservationSummaryViewModel
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Type { get; set; }
}

public class AreaWithinLocationViewModel
{
    public string Name { get; set; }
    public string Detail { get; set; }
    public string LocationId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsTravelHub { get; set; } // Indicates if this location allows travel
}

public class RouteOptionViewModel
{
    public string RouteId { get; set; }
    public string Destination { get; set; }
    public string TravelTime { get; set; }
    public string Detail { get; set; }
    public bool IsLocked { get; set; }
    public string LockReason { get; set; }
    public string EngagementType { get; set; }
    public string ObligationLabel { get; set; }
    public bool CanUnlockWithPermit { get; set; }

    // Modal-specific properties
    public string Familiarity { get; set; }
    public bool SupportsCart { get; set; } = true;
    public bool SupportsCarriage { get; set; } = true;
    public string TransportMethod { get; set; } = "walk";
}

// Simplified route view model for TravelContent component
public class SimpleRouteViewModel
{
    public string Id { get; set; }
    public string Destination { get; set; }
    public string TransportType { get; set; }
    public int TravelTimeInSegments { get; set; }
    public int Cost { get; set; }
    public string FamiliarityLevel { get; set; }
}

public class TravelProgressViewModel
{
    public string Title { get; set; }
    public int ProgressPercent { get; set; }
    public string TimeWalked { get; set; }
    public string TimeRemaining { get; set; }
}

public class MechanicEffectViewModel
{
    public string Description { get; set; }
    public MechanicEffectType Type { get; set; }
}

public enum MechanicEffectType
{
    Neutral,
    Positive,
    Negative
}

public class LeverageViewModel
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public ConnectionType TokenType { get; set; }
    public int TotalLeverage { get; set; }
    public int TokenDebtLeverage { get; set; }
    public int ObligationLeverage { get; set; }
    public int FailureLeverage { get; set; }
    public int TargetQueuePosition { get; set; }
    public int DisplacementCost { get; set; }
    public string Level { get; set; }
    public string Narrative { get; set; }

    public string LeverageIcon => TotalLeverage switch
    {
        >= 10 => "🔴",
        >= 5 => "🟠",
        >= 3 => "🟡",
        >= 1 => "⚪",
        _ => ""
    };

    public string LeverageColor => TotalLeverage switch
    {
        >= 10 => "danger",
        >= 5 => "warning",
        >= 3 => "caution",
        >= 1 => "info",
        _ => "default"
    };
}

// ============================================
// LOCATION CONTENT VIEW MODELS
// ============================================
// View models for LocationContent component - all filtering/querying done in backend

/// <summary>
/// Complete view model for LocationContent component
/// Contains ALL data needed for all sub-views pre-built by LocationFacade
/// </summary>
public class LocationContentViewModel
{
    // Header information
    public LocationHeaderViewModel Header { get; set; } = new();

    // Landing view data
    public List<LocationActionViewModel> TravelActions { get; set; } = new();
    public List<LocationActionViewModel> PlayerActions { get; set; } = new();
    public bool HasSpots { get; set; }

    // LookingAround view data (NPCs with their social situations PRE-GROUPED)
    public List<NpcWithSituationsViewModel> NPCsWithSituations { get; set; } = new();

    // Mental challenges - grouped by obstacles
    public List<SituationCardViewModel> AmbientMentalSituations { get; set; } = new();  // Situations without obstacles
    public List<ObstacleWithSituationsViewModel> MentalObstacles { get; set; } = new();  // Situations from obstacles

    // Physical challenges - grouped by obstacles
    public List<SituationCardViewModel> AmbientPhysicalSituations { get; set; } = new();  // Situations without obstacles
    public List<ObstacleWithSituationsViewModel> PhysicalObstacles { get; set; } = new();  // Situations from obstacles

    // Spots view data
    public List<SpotWithNpcsViewModel> AvailableSpots { get; set; } = new();

    // Scene-Situation Architecture: Locked situations with requirement gaps
    // Perfect information: player sees what they need to unlock
    public List<LockedSituationViewModel> LockedSituations { get; set; } = new();
}

/// <summary>
/// Location header information (venue name, spot name, time, traits)
/// </summary>
public class LocationHeaderViewModel
{
    public string VenueName { get; set; }
    public string SpotName { get; set; }
    public string TimeOfDayTrait { get; set; }
    public List<string> SpotTraits { get; set; } = new();
    public string AtmosphereText { get; set; }
    public TimeBlocks CurrentTime { get; set; }
    public List<NPC> NPCsPresent { get; set; } = new();
}

/// <summary>
/// NPC with their social situations already filtered and attached
/// NO FILTERING NEEDED IN UI - backend pre-groups situations by NPC and obstacles
/// </summary>
public class NpcWithSituationsViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string PersonalityType { get; set; }
    public string ConnectionState { get; set; }
    public string StateClass { get; set; }  // CSS class for connection state
    public string Description { get; set; }

    // Social situations FOR THIS NPC - grouped by obstacles
    public List<SituationCardViewModel> AmbientSocialSituations { get; set; } = new();  // Situations without obstacles
    public List<ObstacleWithSituationsViewModel> SocialObstacles { get; set; } = new();  // Situations from obstacles

    // Exchange availability for MERCANTILE NPCs
    public bool HasExchange { get; set; }
    public string ExchangeDescription { get; set; }
}

/// <summary>
/// Situation card for display - simplified from domain Situation entity
/// Contains all display information pre-calculated
/// </summary>
public class SituationCardViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SystemType { get; set; }  // "social", "mental", "physical"
    public int Difficulty { get; set; }  // Pre-calculated difficulty
    public string DifficultyLabel { get; set; }  // "Doubt", "Exposure", "Danger"
    public string ObligationId { get; set; }
    public bool IsIntroAction { get; set; }

    // Costs
    public int FocusCost { get; set; }
    public int StaminaCost { get; set; }
}

/// <summary>
/// Obstacle with its situations for hierarchical display
/// Shows obstacle context (name, description, intensity, contexts) with nested situations
/// </summary>
public class ObstacleWithSituationsViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Intensity { get; set; }
    public List<string> Contexts { get; set; } = new();  // e.g., ["Search", "Deduction", "Pattern"]
    public string ContextsDisplay { get; set; }  // e.g., "Search, Deduction, Pattern"

    // Situations that belong to this obstacle
    public List<SituationCardViewModel> Situations { get; set; } = new();
}

/// <summary>
/// Spot with NPCs already attached
/// NO FILTERING NEEDED IN UI - backend pre-attaches NPCs to spots
/// </summary>
public class SpotWithNpcsViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public bool IsCurrentSpot { get; set; }
    public List<NpcAtSpotViewModel> NPCs { get; set; } = new();
}

/// <summary>
/// Simplified NPC info for spot list
/// </summary>
public class NpcAtSpotViewModel
{
    public string Name { get; set; }
    public string ConnectionState { get; set; }
}

/// <summary>
/// Locked situation with strongly-typed requirement gaps for UI rendering
/// Perfect information pattern: player sees exactly what they need to unlock
/// Each requirement type enables different UI execution context
/// </summary>
public class LockedSituationViewModel
{
    public string SituationId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SystemType { get; set; }  // "social", "mental", "physical", "instant", "navigation"
    public string LockReason { get; set; }  // Human-readable summary "Requires X OR Y"

    // CONTEXTUAL PROPERTIES - each enables type-specific UI rendering
    // Bond requirements: Render NPC portrait + progress bar + "Talk to X" guidance
    public List<UnmetBondRequirement> UnmetBonds { get; set; } = new List<UnmetBondRequirement>();

    // Scale requirements: Render behavioral spectrum visualization with current position
    public List<UnmetScaleRequirement> UnmetScales { get; set; } = new List<UnmetScaleRequirement>();

    // Resolve requirements: Render progress bar with current/required resolve
    public List<UnmetResolveRequirement> UnmetResolve { get; set; } = new List<UnmetResolveRequirement>();

    // Coins requirements: Render coin amount with "Earn X more coins" guidance
    public List<UnmetCoinsRequirement> UnmetCoins { get; set; } = new List<UnmetCoinsRequirement>();

    // Situation count requirements: Render completion counter "Complete X more situations"
    public List<UnmetSituationCountRequirement> UnmetSituationCount { get; set; } = new List<UnmetSituationCountRequirement>();

    // Achievement requirements: Render achievement badge with link to earning situation
    public List<UnmetAchievementRequirement> UnmetAchievements { get; set; } = new List<UnmetAchievementRequirement>();

    // State requirements: Render state icon with "Gain/Remove X state" guidance
    public List<UnmetStateRequirement> UnmetStates { get; set; } = new List<UnmetStateRequirement>();
}