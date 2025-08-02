using System.Collections.Generic;

// ========== CONVERSATION VIEW MODELS ==========

public class ConversationViewModel
{
    public string NpcName { get; set; }
    public string NpcId { get; set; }
    public string CurrentText { get; set; }
    public List<ConversationChoiceViewModel> Choices { get; set; }
    public bool IsComplete { get; set; }
    public string ConversationTopic { get; set; }
}

public class ConversationChoiceViewModel
{
    public string Id { get; set; }
    public string Text { get; set; }
    public bool IsAvailable { get; set; }
    public string UnavailableReason { get; set; }
}

// ========== TRAVEL VIEW MODELS ==========

public class TravelDestinationViewModel
{
    public string LocationId { get; set; }
    public string LocationName { get; set; }
    public string Description { get; set; }
    public bool CanTravel { get; set; }
    public string CannotTravelReason { get; set; }
    public int MinimumCost { get; set; }
    public int MinimumTime { get; set; }
    public bool IsCurrent { get; set; }
    public List<TravelRouteViewModel> Routes { get; set; } = new();
}

// New travel context ViewModel
public class TravelContextViewModel
{
    // Player travel resources
    public int CurrentStamina { get; set; }
    public int TotalWeight { get; set; }
    public string WeightStatus { get; set; } // "Light load", "Medium load (+1 stamina)", etc.
    public string WeightClass { get; set; } // CSS class: "", "warning", "danger"
    public int BaseStaminaPenalty { get; set; } // 0, 1, or 2
    
    // Letter effects
    public int CarriedLetterCount { get; set; }
    public bool HasHeavyLetters { get; set; }
    public bool HasFragileLetters { get; set; }
    public bool HasBulkyLetters { get; set; }
    public bool HasPerishableLetters { get; set; }
    public string LetterWarning { get; set; }
    
    // Equipment
    public List<ItemCategory> CurrentEquipmentCategories { get; set; } = new();
    
    // Weather
    public WeatherCondition CurrentWeather { get; set; }
    public string WeatherIcon { get; set; }
}

public class TravelRouteViewModel
{
    public string RouteId { get; set; }
    public string RouteName { get; set; }
    public string Description { get; set; }
    public TravelMethods TransportMethod { get; set; }
    public int TimeCost { get; set; }
    public int BaseStaminaCost { get; set; }
    public int TotalStaminaCost { get; set; } // Includes weight and letter penalties
    public int CoinCost { get; set; }
    public bool CanTravel { get; set; }
    public string CannotTravelReason { get; set; }
    
    // New properties for enhanced UI
    public bool IsDiscovered { get; set; }
    public bool IsBlocked { get; set; }
    public string BlockingReason { get; set; }
    public string HintMessage { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<TerrainCategory> TerrainCategories { get; set; } = new();
    public TimeBlocks? DepartureTime { get; set; }
    
    // Token requirements
    public Dictionary<string, RouteTokenRequirementViewModel> TokenRequirements { get; set; } = new();
    
    // Discovery information for locked routes
    public List<RouteDiscoveryOptionViewModel> DiscoveryOptions { get; set; } = new();
}

public class RouteTokenRequirementViewModel
{
    public string RequirementKey { get; set; } // e.g., "type_Trust" or "npc_123"
    public int RequiredAmount { get; set; }
    public int CurrentAmount { get; set; }
    public string DisplayName { get; set; }
    public string Icon { get; set; }
    public bool IsMet { get; set; }
}

public class RouteDiscoveryOptionViewModel
{
    public string TeachingNPCId { get; set; }
    public string TeachingNPCName { get; set; }
    public int RequiredTokens { get; set; }
    public int PlayerTokens { get; set; }
    public bool CanAfford { get; set; }
}

// ========== INVENTORY VIEW MODELS ==========

public class InventoryViewModel
{
    public List<InventoryItemViewModel> Items { get; set; }
    public int TotalWeight { get; set; }
    public int MaxSlots { get; set; }
    public int UsedSlots { get; set; }
    public int Coins { get; set; }
}

public class InventoryItemViewModel
{
    public string ItemId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Weight { get; set; }
    public int Value { get; set; }
    public bool CanUse { get; set; }
    public bool CanRead { get; set; }
}

// ========== NARRATIVE VIEW MODELS ==========

public class NarrativeStateViewModel
{
    public List<NarrativeViewModel> ActiveNarratives { get; set; }
    public bool IsTutorialActive { get; set; }
    public bool TutorialComplete { get; set; }
}

public class NarrativeViewModel
{
    public string NarrativeId { get; set; }
    public string CurrentStepId { get; set; }
    public string StepName { get; set; }
    public string StepDescription { get; set; }
    public bool IsComplete { get; set; }
}

public class TutorialGuidanceViewModel
{
    public bool IsActive { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string StepTitle { get; set; }
    public string GuidanceText { get; set; }
    public List<string> AllowedActions { get; set; }
}

// ========== REST VIEW MODELS ==========

public class RestOptionsViewModel
{
    public List<RestOptionViewModel> RestOptions { get; set; }
    public List<LocationActionViewModel> LocationActions { get; set; }
    public List<WaitOptionViewModel> WaitOptions { get; set; }
}

/// <summary>
/// ViewModel for a rest option
/// </summary>
public class RestOptionViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int StaminaRecovery { get; set; }
    public int CoinCost { get; set; }
    public int TimeHours { get; set; }
    public string RequiredItem { get; set; }
    public bool CanAffordCoins { get; set; }
    public bool HasRequiredItem { get; set; }
    public bool IsAvailable { get; set; }
    public string UnavailableReason { get; set; }
}

/// <summary>
/// ViewModel for a location action
/// </summary>
public class LocationActionViewModel
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string NPCName { get; set; }
    public string NPCProfession { get; set; }
    public int TimeCost { get; set; }
    public int StaminaCost { get; set; }
    public int CoinCost { get; set; }
    public int StaminaReward { get; set; }
    public bool IsAvailable { get; set; }
    public string UnavailableReason { get; set; }
    public bool CanBeRemedied { get; set; }
    public string RemediationHint { get; set; }
}

/// <summary>
/// ViewModel for wait options
/// </summary>
public class WaitOptionViewModel
{
    public int Hours { get; set; }
    public string Description { get; set; }
}

// ========== LETTER BOARD VIEW MODELS ==========

public class LetterBoardViewModel
{
    public bool IsAvailable { get; set; }
    public string UnavailableReason { get; set; }
    public List<LetterOfferViewModel> Offers { get; set; }
    public TimeBlocks CurrentTime { get; set; }
}

public class LetterOfferViewModel
{
    public string Id { get; set; }
    public string SenderName { get; set; }
    public string RecipientName { get; set; }
    public string Description { get; set; }
    public int Payment { get; set; }
    public int DeadlineDays { get; set; }
    public bool CanAccept { get; set; }
    public string CannotAcceptReason { get; set; }
    public List<string> TokenTypes { get; set; }
}

// ========== NPC & RELATIONSHIP VIEW MODELS ==========

public class TimeBlockServiceViewModel
{
    public TimeBlocks TimeBlock { get; set; }
    public bool IsCurrentTimeBlock { get; set; }
    public List<ServiceTypes> AvailableServices { get; set; }
    public List<string> AvailableNPCs { get; set; }
}

public class NPCWithOffersViewModel
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public string Role { get; set; }
    public bool HasDirectOfferAvailable { get; set; }
    public int PendingOfferCount { get; set; }
    public bool IsAvailable { get; set; }
}

public class NPCRelationshipViewModel
{
    public string NPCId { get; set; }
    public string NPCName { get; set; }
    public string Role { get; set; }
    public string LocationId { get; set; }
    public string LocationName { get; set; }
    public int ConnectionTokens { get; set; }
    public bool CanMakeDirectOffer { get; set; }
    public Dictionary<ConnectionType, int> TokensByType { get; set; } = new();
}

// ========== OBLIGATION VIEW MODELS ==========

public class ObligationViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public int Priority { get; set; }
}

// ========== DETAILED OBLIGATION VIEW MODELS ==========

public class DetailedObligationsViewModel
{
    public List<ActiveObligationViewModel> ActiveObligations { get; set; } = new();
    public List<DebtObligationViewModel> DebtObligations { get; set; } = new();
    public List<ThresholdWarningViewModel> ThresholdWarnings { get; set; } = new();
    public List<ObligationTemplateViewModel> AvailableTemplates { get; set; } = new();
}

public class ActiveObligationViewModel
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }
    public int DaysSinceAccepted { get; set; }
    public List<string> BenefitDescriptions { get; set; } = new();
    public List<string> ConstraintDescriptions { get; set; } = new();
    public ConnectionType? RelatedTokenType { get; set; }
    public int TokenCount { get; set; }
    public bool HasConflicts { get; set; }
    public bool HasForcedLetterWarning { get; set; }
    public int DaysUntilForcedLetter { get; set; }
}

public class DebtObligationViewModel
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LeverageHolderName { get; set; }
    public int LeverageAmount { get; set; }
    public List<string> EffectDescriptions { get; set; } = new();
}

public class ThresholdWarningViewModel
{
    public string WarningType { get; set; } // "Debt" or "HighTokens"
    public string Message { get; set; }
    public string NPCName { get; set; }
    public int CurrentValue { get; set; }
    public int ThresholdValue { get; set; }
    public ConnectionType? TokenType { get; set; }
}

public class ObligationTemplateViewModel
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string TriggerDescription { get; set; }
    public string RequirementDescription { get; set; }
    public bool CanAccept { get; set; }
    public string CannotAcceptReason { get; set; }
    public List<string> ConflictingObligations { get; set; } = new();
}

// ========== INFORMATION DISCOVERY VIEW MODELS ==========

/// <summary>
/// View model for discovered information
/// </summary>
public class InformationDiscoveryViewModel
{
    public List<DiscoveredInfoViewModel> RouteInformation { get; set; } = new();
    public List<DiscoveredInfoViewModel> LocationInformation { get; set; } = new();
    public List<DiscoveredInfoViewModel> NPCInformation { get; set; } = new();
    public List<DiscoveredInfoViewModel> ServiceInformation { get; set; } = new();
    public List<DiscoveredInfoViewModel> SecretInformation { get; set; } = new();
    public int TotalDiscovered { get; set; }
    public int TotalUnlocked { get; set; }
}

/// <summary>
/// View model for individual discovered information
/// </summary>
public class DiscoveredInfoViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public InformationType Type { get; set; }
    public int Tier { get; set; }
    public bool IsAccessUnlocked { get; set; }
    public int DayDiscovered { get; set; }
    
    // Access requirements
    public Dictionary<ConnectionType, int> TokenRequirements { get; set; } = new();
    public List<string> SealRequirements { get; set; } = new();
    public List<string> EquipmentRequirements { get; set; } = new();
    public int CoinCost { get; set; }
    public bool CanAfford { get; set; }
    
    // Leverage info
    public bool CanBeUsedAsLeverage { get; set; }
    public string LeverageTargetName { get; set; }
    public int LeverageValue { get; set; }
}

// ========== OBLIGATION EFFECT VIEW MODELS ==========

/// <summary>
/// ViewModel for displaying real-time obligation effects on letters
/// </summary>
public class ObligationEffectViewModel
{
    public string ObligationName { get; set; }
    public string Type { get; set; } // PositionBoost, PaymentBonus, Restriction, etc.
    public string Description { get; set; }
    public int Value { get; set; } // Effect magnitude
}

// ========== LOCATION ACTIONS VIEW MODELS ==========

/// <summary>
/// ViewModel for location actions display
/// </summary>
public class LocationActionsViewModel
{
    public string LocationName { get; set; }
    public string CurrentTimeBlock { get; set; }
    public int HoursRemaining { get; set; }
    public int PlayerStamina { get; set; }
    public int PlayerCoins { get; set; }

    public List<ActionGroupViewModel> ActionGroups { get; set; } = new();
}

/// <summary>
/// ViewModel for a group of actions
/// </summary>
public class ActionGroupViewModel
{
    public string ActionType { get; set; }
    public List<ActionOptionViewModel> Actions { get; set; } = new();
}

/// <summary>
/// ViewModel for an individual action option
/// </summary>
public class ActionOptionViewModel
{
    public string Id { get; set; }
    public string Description { get; set; }
    public string NPCName { get; set; }
    public string NPCProfession { get; set; }

    // Costs
    public int TimeCost { get; set; }
    public int StaminaCost { get; set; }
    public int CoinCost { get; set; }

    // Affordability
    public bool HasEnoughTime { get; set; }
    public bool HasEnoughStamina { get; set; }
    public bool HasEnoughCoins { get; set; }

    // Rewards
    public int StaminaReward { get; set; }
    public int CoinReward { get; set; }
    public string TokenType { get; set; }
    public int TokenReward { get; set; }
    public string RewardsDescription { get; set; }

    // Overall availability
    public bool IsAvailable { get; set; }
    public List<string> UnavailableReasons { get; set; } = new();
    
    // Service availability info
    public bool IsServiceClosed { get; set; }
    public string NextAvailableTime { get; set; }
    public string ServiceSchedule { get; set; }
    
    // Tutorial restriction
    public bool IsAllowedInTutorial { get; set; } = true;
}