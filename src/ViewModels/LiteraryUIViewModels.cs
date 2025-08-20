using System;
using System.Collections.Generic;

/// <summary>
/// ViewModel for the conversation screen matching the mockup structure
/// </summary>
public class ConversationViewModel
{
    // Core conversation data
    public string NpcName { get; set; }
    public string NpcId { get; set; }
    public string CurrentText { get; set; }
    public bool IsComplete { get; set; }
    public string ConversationTopic { get; set; }

    // NPC Emotional State (from NPCStateResolver)
    public NPCEmotionalState? EmotionalState { get; set; }
    public StakeType? CurrentStakes { get; set; }
    public int? HoursToDeadline { get; set; }

    // Attention System
    public int CurrentAttention { get; set; }
    public int MaxAttention { get; set; } = 3;
    public string AttentionNarrative { get; set; }

    // Location Context
    public string LocationName { get; set; }
    public string LocationAtmosphere { get; set; }
    public List<string> LocationPath { get; set; } = new();

    // Character Focus
    public string CharacterName { get; set; }
    public string CharacterState { get; set; } // Body language description
    public Dictionary<string, string> RelationshipStatus { get; set; } = new();

    // Dialogue
    public List<string> SpokenWords { get; set; } = new();
    public string CharacterAction { get; set; } // Mid-dialogue action

    // Choices with Mechanics
    public List<ConversationChoiceViewModel> Choices { get; set; } = new();

    // Peripheral Awareness
    public string DeadlinePressure { get; set; }
    public List<string> EnvironmentalHints { get; set; } = new();
    public string BodyLanguageDescription { get; set; }
    public List<string> PeripheralObservations { get; set; } = new();
    public string InternalMonologue { get; set; }
    public List<BindingObligationViewModel> BindingObligations { get; set; } = new();

    // Bottom Status
    public string CurrentLocation { get; set; }
    public string QueueStatus { get; set; } // e.g., "3/8"
    public string CoinStatus { get; set; } // e.g., "12s"
    public string CurrentTime { get; set; } // e.g., "TUE 3:45 PM"

    // Context tags for narrative generation
    public List<string> PressureTags { get; set; } = new();
    public List<string> RelationshipTags { get; set; } = new();
    public List<string> FeelingTags { get; set; } = new();
    public List<string> DiscoveryTags { get; set; } = new();
    public List<string> ResourceTags { get; set; } = new();

    // Scene pressure metrics
    public int MinutesUntilDeadline { get; set; }
    public int LetterQueueSize { get; set; }
}

/// <summary>
/// ViewModel for conversation choices with mechanical display
/// </summary>
public class ConversationChoiceViewModel
{
    public string Id { get; set; }
    public string Text { get; set; } // The italicized thought text
    public int PatienceCost { get; set; }
    public string PatienceDisplay { get; set; } // "Free", "◆ 1", "◆◆ 2", etc.
    public bool IsLocked { get; set; }
    public List<MechanicEffectViewModel> Mechanics { get; set; } = new();

    // Additional properties for compatibility
    public bool IsAvailable { get; set; } = true;
    public string UnavailableReason { get; set; }
    public string PatienceDescription { get; set; }
    public bool IsInternalThought { get; set; }
    public string EmotionalTone { get; set; }
}

/// <summary>
/// ViewModel for displaying mechanical effects of choices
/// </summary>
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

/// <summary>
/// ViewModel for location screens matching the mockup structure
/// </summary>
public class LocationScreenViewModel
{
    // Header Bar
    public string CurrentTime { get; set; }
    public string DeadlineTimer { get; set; }

    // Location Info
    public List<string> LocationPath { get; set; } = new();
    public string LocationName { get; set; }
    // Tags removed - atmosphere now calculated from NPC presence

    // Atmosphere
    public string AtmosphereText { get; set; }

    // Actions
    public List<LocationActionViewModel> QuickActions { get; set; } = new();

    // NPCs Present
    public List<NPCPresenceViewModel> NPCsPresent { get; set; } = new();

    // Observations
    public string ObservationHeader { get; set; } = "YOU NOTICE:";
    public List<ObservationViewModel> Observations { get; set; } = new();

    // Movement Options
    public List<RouteOptionViewModel> Routes { get; set; } = new();

    // Travel Progress (if traveling)
    public TravelProgressViewModel TravelProgress { get; set; }
}

// LocationTagViewModel and LocationTagType removed - tags replaced with atmosphere calculation

/// <summary>
/// ViewModel for location actions (the grid cards)
/// </summary>
public class LocationActionViewModel
{
    public string Icon { get; set; } // Emoji
    public string Title { get; set; }
    public string Detail { get; set; }
    public string Cost { get; set; } // "FREE", "1c", "10m", etc.
    public string ActionType { get; set; } // Optional type for special actions like "wait"

    // Tier-based availability
    public bool IsAvailable { get; set; } = true;
    public string LockReason { get; set; } // e.g., "Requires Associate status"
    public TierLevel? RequiredTier { get; set; } // The tier needed to unlock this action
}

/// <summary>
/// ViewModel for NPCs at a location
/// </summary>
public class NPCPresenceViewModel
{
    public string Id { get; set; }  // NPC identifier for starting conversations
    public string Name { get; set; }
    public string MoodEmoji { get; set; }
    public string Description { get; set; }
    public List<InteractionOptionViewModel> Interactions { get; set; } = new();
}

/// <summary>
/// ViewModel for NPC interaction options
/// </summary>
public class InteractionOptionViewModel
{
    public string Text { get; set; }
    public string Cost { get; set; } // Time or money
}

/// <summary>
/// ViewModel for observations at a location
/// </summary>
public class ObservationViewModel
{
    public string Icon { get; set; } // Emoji or "❓" for unknown
    public string Text { get; set; }
    public bool IsUnknown { get; set; }
}

/// <summary>
/// ViewModel for route/movement options
/// </summary>
public class RouteOptionViewModel
{
    public string RouteId { get; set; } // Added to enable travel
    public string Destination { get; set; }
    public string TravelTime { get; set; }
    public string Detail { get; set; } // e.g., "West side", "Uphill"

    // Tier and accessibility information
    public bool IsLocked { get; set; }
    public string LockReason { get; set; } // e.g., "Requires T2 (Associate)", "Needs Transport Permit"
    public TierLevel RequiredTier { get; set; }
    public bool CanUnlockWithPermit { get; set; }
}

/// <summary>
/// ViewModel for travel progress display
/// </summary>
public class TravelProgressViewModel
{
    public string Title { get; set; }
    public int ProgressPercent { get; set; }
    public string TimeWalked { get; set; }
    public string TimeRemaining { get; set; }
}

/// <summary>
/// ViewModel for displaying leverage (power dynamics) data
/// </summary>
public class LeverageViewModel
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public ConnectionType TokenType { get; set; }

    // Leverage components
    public int TotalLeverage { get; set; }
    public int TokenDebtLeverage { get; set; }
    public int ObligationLeverage { get; set; }
    public int FailureLeverage { get; set; }

    // Calculated effects
    public int TargetQueuePosition { get; set; }
    public int DisplacementCost { get; set; }

    // Display data
    public string Level { get; set; } // None, Low, Moderate, High, Extreme
    public string Narrative { get; set; } // Human-readable leverage description

    // Visual indicators
    public string LeverageIcon => TotalLeverage switch
    {
        >= 10 => "🔴", // Extreme
        >= 5 => "🟠",  // High
        >= 3 => "🟡",  // Moderate
        >= 1 => "⚪",  // Low
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