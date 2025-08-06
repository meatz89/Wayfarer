using System.Collections.Generic;

/// <summary>
/// ViewModel for the Travel Selection UI - contains only display data
/// </summary>
public class TravelViewModel
{
    public string CurrentLocationId { get; init; }
    public string CurrentLocationName { get; init; }

    // Travel context status
    public TravelStatusViewModel Status { get; init; }

    // Available destinations
    public List<DestinationViewModel> Destinations { get; init; } = new();
}

/// <summary>
/// ViewModel for travel status information
/// </summary>
public class TravelStatusViewModel
{
    public int TotalWeight { get; init; }
    public string WeightClass { get; init; }
    public string WeightStatus { get; init; }
    public int BaseStaminaCost { get; init; }
    public int CurrentStamina { get; init; }
    public List<string> CurrentEquipment { get; init; } = new();

    // Letter-related travel effects
    public int CarriedLetterCount { get; init; }
    public bool HasHeavyLetters { get; init; }
    public bool HasFragileLetters { get; init; }
    public bool HasValuableLetters { get; init; }
    public List<string> LetterWarnings { get; init; } = new();
}

/// <summary>
/// ViewModel for a travel destination
/// </summary>
public class DestinationViewModel
{
    public string LocationId { get; init; }
    public string LocationName { get; init; }
    public bool IsCurrent { get; init; }

    // Available routes to this destination
    public List<RouteViewModel> AvailableRoutes { get; init; } = new();
    public List<LockedRouteViewModel> LockedRoutes { get; init; } = new();
}

/// <summary>
/// ViewModel for an available route
/// </summary>
public class RouteViewModel
{
    public string RouteId { get; init; }
    public string TerrainType { get; init; }
    public int CoinCost { get; init; }
    public int StaminaCost { get; init; }
    public int TravelTimeHours { get; init; }
    public string TransportRequirement { get; init; }

    // Affordability
    public bool CanAffordCoins { get; init; }
    public bool CanAffordStamina { get; init; }
    public bool IsBlocked { get; init; }
    public string BlockedReason { get; init; }

    // Additional costs from letters
    public int LetterStaminaPenalty { get; init; }
    public int TotalStaminaCost { get; init; }
}

/// <summary>
/// ViewModel for a locked route
/// </summary>
public class LockedRouteViewModel
{
    public string RouteId { get; init; }
    public string TerrainType { get; init; }
    public List<RouteDiscoveryViewModel> DiscoveryOptions { get; init; } = new();
}

/// <summary>
/// ViewModel for route discovery options
/// </summary>
public class RouteDiscoveryViewModel
{
    public string DiscoveryId { get; init; }
    public DiscoveryMethodViewModel Method { get; init; }
    public bool CanAfford { get; init; }
}

/// <summary>
/// ViewModel for discovery method details
/// </summary>
public class DiscoveryMethodViewModel
{
    public string MethodType { get; init; }
    public string Description { get; init; }

    // For NPC teaching
    public string NPCName { get; init; }
    public string TokenType { get; init; }
    public int TokenCost { get; init; }
    public int AvailableTokens { get; init; }

    // For item requirement
    public string RequiredItem { get; init; }
    public bool HasItem { get; init; }

    // For coin cost
    public int CoinCost { get; init; }
    public int AvailableCoins { get; init; }
}