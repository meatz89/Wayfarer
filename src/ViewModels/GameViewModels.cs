using System;
using System.Collections.Generic;

namespace Wayfarer.Pages;

// Simple ViewModels for UI data transfer - no business logic
// All content comes from JSON or mechanical states

public class LocationScreenViewModel
{
    public string CurrentTime { get; set; }
    public string DeadlineTimer { get; set; }
    public List<string> LocationPath { get; set; } = new();
    public string LocationName { get; set; }
    public List<string> LocationTraits { get; set; } = new();
    public string CurrentSpotName { get; set; }
    public string AtmosphereText { get; set; }
    public string Familiarity { get; set; }
    public List<LocationActionViewModel> QuickActions { get; set; } = new();
    public List<NPCFocusViewModel> NPCsPresent { get; set; } = new();
    public string ObservationHeader { get; set; } = "Observations Available";
    public List<ObservationViewModel> Observations { get; set; } = new();
    public List<AreaWithinLocationViewModel> AreasWithinLocation { get; set; } = new();
    public List<RouteOptionViewModel> Routes { get; set; } = new();
    public TravelProgressViewModel TravelProgress { get; set; }
}

public class LocationActionViewModel
{
    public string Icon { get; set; }
    public string Title { get; set; }
    public string Detail { get; set; }
    public string Cost { get; set; }
    public string ActionType { get; set; }
    public bool IsAvailable { get; set; } = true;
    public string LockReason { get; set; }
}

public class NPCFocusViewModel
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
    public int AttentionCost { get; set; }
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
    public string SpotId { get; set; }
    public bool IsCurrent { get; set; }
    public bool IsTravelHub { get; set; } // Indicates if this spot allows travel
}

public class RouteOptionViewModel
{
    public string RouteId { get; set; }
    public string Destination { get; set; }
    public string TravelTime { get; set; }
    public string Detail { get; set; }
    public bool IsLocked { get; set; }
    public string LockReason { get; set; }
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