using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Manages the deck of travel event cards for a specific route
/// Routes have personality-based card decks that create emergent travel narratives
/// </summary>
public class RouteDeck
{
private readonly string _routeId;
private readonly RoutePersonality _personality;
private readonly List<TravelEventCard> _allCards;
private readonly Random _random;

public RouteDeck(string routeId, RoutePersonality personality, int seed = 0)
{
    _routeId = routeId;
    _personality = personality;
    _random = seed > 0 ? new Random(seed) : new Random();
    _allCards = GenerateCardsForPersonality(personality);
}

/// <summary>
/// Draw cards based on route familiarity
/// Higher familiarity = more cards drawn, more choice
/// </summary>
public List<TravelEventCard> DrawCards(int familiarity, WeatherCondition weather, TimeBlocks timeBlock)
{
    // Determine how many cards to draw based on familiarity
    int cardsToDraw = familiarity switch
    {
        0 => 1,  // Unknown: Draw 1, must resolve
        1 or 2 => 2,  // Learning: Draw 2, choose 1
        3 or 4 => 3,  // Familiar: Draw 3, choose 1
        5 => 4,  // Mastered: Draw 4, choose 1 OR skip events
        _ => 1
    };
    
    // Filter available cards based on conditions
    var availableCards = _allCards
        .Where(c => c.IsAvailableIn(weather, timeBlock, familiarity))
        .ToList();
    
    // If no cards available, return empty list (safe travel)
    if (!availableCards.Any())
        return new List<TravelEventCard>();
    
    // Draw random cards
    var drawnCards = new List<TravelEventCard>();
    for (int i = 0; i < cardsToDraw && availableCards.Any(); i++)
    {
        var index = _random.Next(availableCards.Count);
        drawnCards.Add(availableCards[index]);
        availableCards.RemoveAt(index); // Don't draw same card twice
    }
    
    return drawnCards;
}

/// <summary>
/// Generate cards based on route personality
/// Each personality has different event distributions
/// </summary>
private List<TravelEventCard> GenerateCardsForPersonality(RoutePersonality personality)
{
    var cards = new List<TravelEventCard>();
    
    switch (personality)
    {
        case RoutePersonality.SAFE:
            // Main roads: Guards, merchants, fellow travelers
            cards.AddRange(CreateGuardCheckpointCards());
            cards.AddRange(CreateMerchantCaravanCards());
            cards.AddRange(CreateFellowTravelerCards());
            cards.Add(CreateSafeDelayCard());
            break;
            
        case RoutePersonality.OPPORTUNISTIC:
            // Back paths: Shortcuts, caches, suspicious figures
            cards.AddRange(CreateShortcutCards());
            cards.AddRange(CreateHiddenCacheCards());
            cards.AddRange(CreateSuspiciousFigureCards());
            cards.Add(CreateWeatherDelayCard());
            break;
            
        case RoutePersonality.DANGEROUS:
            // Wilderness: Bandits, natural hazards, wildlife
            cards.AddRange(CreateBanditEncounterCards());
            cards.AddRange(CreateNaturalHazardCards());
            cards.AddRange(CreateWildlifeCards());
            cards.Add(CreateSecretDiscoveryCard());
            break;
            
        case RoutePersonality.SOCIAL:
            // Urban: Crowds, pickpockets, useful contacts
            cards.AddRange(CreateCrowdCards());
            cards.AddRange(CreatePickpocketCards());
            cards.AddRange(CreateUsefulContactCards());
            cards.Add(CreateMarketDelayCard());
            break;
    }
    
    return cards;
}

// Card creation methods for each type
private List<TravelEventCard> CreateGuardCheckpointCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_guard_checkpoint",
            Title = "Guard Checkpoint",
            Description = "City guards are checking travelers.",
            CardType = TravelEventType.GuardCheckpoint,
            IsNegative = false,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Show status (requires Status 3+)",
                    RequiredTokenType = ConnectionType.Status,
                    RequiredTokenAmount = 3,
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
                },
                new TravelEventOption
                {
                    Description = "Pay toll",
                    RequiredCoins = 2,
                    Effect = new TravelEventEffect { CoinChange = -2, TimeChangeMinutes = 0 }
                },
                new TravelEventOption
                {
                    Description = "Argue your case",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 30 }
                }
            }
        }
    };
}

private List<TravelEventCard> CreateMerchantCaravanCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_merchant_caravan",
            Title = "Merchant Caravan",
            Description = "A merchant caravan is heading your way.",
            CardType = TravelEventType.MerchantCaravan,
            IsNegative = false,
            IsComfort = true,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Pay for faster travel",
                    RequiredCoins = 2,
                    Effect = new TravelEventEffect { CoinChange = -2, TimeChangeMinutes = -15 }
                },
                new TravelEventOption
                {
                    Description = "Learn market rumors",
                    RequiredAttention = 1,
                    Effect = new TravelEventEffect { AttentionSpent = 1, RevealsMarketInfo = true }
                },
                new TravelEventOption
                {
                    Description = "Pass by",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
                }
            }
        }
    };
}

private List<TravelEventCard> CreateFellowTravelerCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_fellow_messenger",
            Title = "Fellow Messenger",
            Description = "Another letter carrier is traveling this route.",
            CardType = TravelEventType.FellowTravelers,
            IsNegative = false,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Share route information",
                    RequiredAttention = 1,
                    Effect = new TravelEventEffect { AttentionSpent = 1, RevealsNPCStates = true }
                },
                new TravelEventOption
                {
                    Description = "Hire to deliver secondary letter",
                    RequiredCoins = 2,
                    Effect = new TravelEventEffect { CoinChange = -2 } // Hiring courier costs money
                },
                new TravelEventOption
                {
                    Description = "Travel together safely",
                    Effect = new TravelEventEffect { TimeChangeMinutes = -10 }
                }
            }
        }
    };
}

private TravelEventCard CreateSafeDelayCard()
{
    return new TravelEventCard
    {
        Id = $"{_routeId}_road_repairs",
        Title = "Road Repairs",
        Description = "Workers are repairing the main road.",
        CardType = TravelEventType.RoadRepairs,
        IsNegative = true,
        Options = new List<TravelEventOption>
        {
            new TravelEventOption
            {
                Description = "Wait for repairs",
                Effect = new TravelEventEffect { TimeChangeMinutes = 20 }
            },
            new TravelEventOption
            {
                Description = "Take detour",
                Effect = new TravelEventEffect { TimeChangeMinutes = 10 }
            }
        }
    };
}

private List<TravelEventCard> CreateShortcutCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_hidden_path",
            Title = "Hidden Path",
            Description = "You spot a barely visible trail through the woods.",
            CardType = TravelEventType.Shortcut,
            MinimumFamiliarity = 1, // Only appears after first travel
            IsNegative = false,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Take the shortcut",
                    Effect = new TravelEventEffect { TimeChangeMinutes = -20, GrantsBonusFamiliarity = true }
                },
                new TravelEventOption
                {
                    Description = "Stay on known path",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
                }
            }
        }
    };
}

private List<TravelEventCard> CreateHiddenCacheCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_hidden_cache",
            Title = "Hidden Cache",
            Description = "A hollow tree contains something valuable.",
            CardType = TravelEventType.HiddenCache,
            MinimumFamiliarity = 2,
            IsNegative = false,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Investigate carefully",
                    RequiredAttention = 1,
                    Effect = new TravelEventEffect { AttentionSpent = 1, CoinChange = 3 }
                },
                new TravelEventOption
                {
                    Description = "Quick grab",
                    Effect = new TravelEventEffect { CoinChange = 1 }
                },
                new TravelEventOption
                {
                    Description = "Leave it alone",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
                }
            }
        }
    };
}

private List<TravelEventCard> CreateSuspiciousFigureCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_shadow_contact",
            Title = "Suspicious Figure",
            Description = "A hooded figure signals to you from the shadows.",
            CardType = TravelEventType.SuspiciousFigures,
            IsNegative = false,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Approach (Shadow 3+ protects)",
                    RequiredTokenType = ConnectionType.Shadow,
                    RequiredTokenAmount = 3,
                    Effect = new TravelEventEffect { RevealsNPCStates = true }
                },
                new TravelEventOption
                {
                    Description = "Approach cautiously",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 15, CoinChange = -2 }
                },
                new TravelEventOption
                {
                    Description = "Avoid them",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 5 }
                }
            }
        }
    };
}

private TravelEventCard CreateWeatherDelayCard()
{
    return new TravelEventCard
    {
        Id = $"{_routeId}_weather_delay",
        Title = "Sudden Weather",
        Description = "Weather conditions worsen unexpectedly.",
        CardType = TravelEventType.WeatherDelay,
        RequiredWeather = WeatherCondition.Rain,
        IsNegative = true,
        Options = new List<TravelEventOption>
        {
            new TravelEventOption
            {
                Description = "Push through",
                Effect = new TravelEventEffect { TimeChangeMinutes = 10 }
            },
            new TravelEventOption
            {
                Description = "Seek shelter",
                Effect = new TravelEventEffect { TimeChangeMinutes = 30 }
            }
        }
    };
}

private List<TravelEventCard> CreateBanditEncounterCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_bandit_encounter",
            Title = "Bandit Ambush",
            Description = "Armed bandits block your path.",
            CardType = TravelEventType.BanditEncounter,
            IsNegative = true,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Pay them off",
                    RequiredCoins = 5,
                    Effect = new TravelEventEffect { CoinChange = -5 }
                },
                new TravelEventOption
                {
                    Description = "Shadow connections help",
                    RequiredTokenType = ConnectionType.Shadow,
                    RequiredTokenAmount = 5,
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
                },
                new TravelEventOption
                {
                    Description = "Run and hide",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 45, DeadlineChangeMinutes = 30 }
                }
            }
        }
    };
}

private List<TravelEventCard> CreateNaturalHazardCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_river_crossing",
            Title = "Dangerous River",
            Description = "The river is running high and fast.",
            CardType = TravelEventType.NaturalHazard,
            RequiredWeather = WeatherCondition.Rain,
            IsNegative = true,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Find safe crossing",
                    RequiredAttention = 2,
                    Effect = new TravelEventEffect { AttentionSpent = 2, TimeChangeMinutes = 20 }
                },
                new TravelEventOption
                {
                    Description = "Risk the ford",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0, CoinChange = -3 } // Risk losing items
                },
                new TravelEventOption
                {
                    Description = "Go around",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 60 }
                }
            }
        }
    };
}

private List<TravelEventCard> CreateWildlifeCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_wolf_pack",
            Title = "Wolf Pack",
            Description = "Wolves are stalking the path ahead.",
            CardType = TravelEventType.Wildlife,
            RequiredTimeBlock = TimeBlocks.Night,
            IsNegative = true,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Make noise to scare",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 15 }
                },
                new TravelEventOption
                {
                    Description = "Take long detour",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 45 }
                },
                new TravelEventOption
                {
                    Description = "Light torch and proceed",
                    RequiredAttention = 1,
                    Effect = new TravelEventEffect { AttentionSpent = 1, TimeChangeMinutes = 5 }
                }
            }
        }
    };
}

private TravelEventCard CreateSecretDiscoveryCard()
{
    return new TravelEventCard
    {
        Id = $"{_routeId}_secret_discovery",
        Title = "Ancient Ruins",
        Description = "You discover old ruins off the main path.",
        CardType = TravelEventType.SecretDiscovery,
        MinimumFamiliarity = 3,
        IsNegative = false,
        Options = new List<TravelEventOption>
        {
            new TravelEventOption
            {
                Description = "Explore thoroughly",
                RequiredAttention = 2,
                Effect = new TravelEventEffect 
                { 
                    AttentionSpent = 2, 
                    TimeChangeMinutes = 30,
                    UnlockedRouteId = $"{_routeId}_secret",
                    GrantsBonusFamiliarity = true
                }
            },
            new TravelEventOption
            {
                Description = "Quick look",
                Effect = new TravelEventEffect { TimeChangeMinutes = 10, CoinChange = 2 }
            },
            new TravelEventOption
            {
                Description = "Keep moving",
                Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
            }
        }
    };
}

private List<TravelEventCard> CreateCrowdCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_market_crowd",
            Title = "Market Day Crowds",
            Description = "The streets are packed with market-goers.",
            CardType = TravelEventType.Crowds,
            RequiredTimeBlock = TimeBlocks.Morning,
            IsNegative = true,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Push through crowds",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 20 }
                },
                new TravelEventOption
                {
                    Description = "Take side streets",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 10 }
                },
                new TravelEventOption
                {
                    Description = "Wait for clearing",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 30, RevealsMarketInfo = true }
                }
            }
        }
    };
}

private List<TravelEventCard> CreatePickpocketCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_pickpocket",
            Title = "Pickpocket Attempt",
            Description = "You feel a hand reaching for your purse.",
            CardType = TravelEventType.Pickpockets,
            IsNegative = true,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Catch them",
                    RequiredAttention = 1,
                    Effect = new TravelEventEffect { AttentionSpent = 1, TimeChangeMinutes = 5 }
                },
                new TravelEventOption
                {
                    Description = "Shadow allies intervene",
                    RequiredTokenType = ConnectionType.Shadow,
                    RequiredTokenAmount = 2,
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
                },
                new TravelEventOption
                {
                    Description = "They succeed",
                    Effect = new TravelEventEffect { CoinChange = -3 }
                }
            }
        }
    };
}

private List<TravelEventCard> CreateUsefulContactCards()
{
    return new List<TravelEventCard>
    {
        new TravelEventCard
        {
            Id = $"{_routeId}_useful_contact",
            Title = "Information Broker",
            Description = "A well-connected local offers information.",
            CardType = TravelEventType.UsefulContact,
            IsNegative = false,
            IsComfort = true,
            Options = new List<TravelEventOption>
            {
                new TravelEventOption
                {
                    Description = "Buy NPC information",
                    RequiredCoins = 3,
                    Effect = new TravelEventEffect { CoinChange = -3, RevealsNPCStates = true }
                },
                new TravelEventOption
                {
                    Description = "Trade information",
                    RequiredTokenType = ConnectionType.Commerce,
                    RequiredTokenAmount = 2,
                    Effect = new TravelEventEffect { RevealsMarketInfo = true }
                },
                new TravelEventOption
                {
                    Description = "Politely decline",
                    Effect = new TravelEventEffect { TimeChangeMinutes = 0 }
                }
            }
        }
    };
}

private TravelEventCard CreateMarketDelayCard()
{
    return new TravelEventCard
    {
        Id = $"{_routeId}_market_delay",
        Title = "Street Vendor",
        Description = "A persistent vendor blocks your path.",
        CardType = TravelEventType.MarketDelay,
        IsNegative = false,
        Options = new List<TravelEventOption>
        {
            new TravelEventOption
            {
                Description = "Buy something small",
                RequiredCoins = 1,
                Effect = new TravelEventEffect { CoinChange = -1, TimeChangeMinutes = 5 }
            },
            new TravelEventOption
            {
                Description = "Listen to pitch",
                Effect = new TravelEventEffect { TimeChangeMinutes = 10, RevealsMarketInfo = true }
            },
            new TravelEventOption
            {
                Description = "Push past",
                Effect = new TravelEventEffect { TimeChangeMinutes = 5 }
            }
        }
    };
}
}

/// <summary>
/// Represents a single travel event card
/// </summary>
public class TravelEventCard
{
public string Id { get; set; }
public string Title { get; set; }
public string Description { get; set; }
public TravelEventType CardType { get; set; }

// Card availability conditions
public WeatherCondition? RequiredWeather { get; set; }
public TimeBlocks? RequiredTimeBlock { get; set; }
public int MinimumFamiliarity { get; set; } = 0;

// Card properties
public bool IsNegative { get; set; } // Must resolve when walking
public bool IsComfort { get; set; } // Double benefits in carriage

// Player options
public List<TravelEventOption> Options { get; set; } = new List<TravelEventOption>();

public bool IsAvailableIn(WeatherCondition weather, TimeBlocks timeBlock, int familiarity)
{
    if (MinimumFamiliarity > familiarity)
        return false;
        
    if (RequiredWeather.HasValue && RequiredWeather.Value != weather)
        return false;
        
    if (RequiredTimeBlock.HasValue && RequiredTimeBlock.Value != timeBlock)
        return false;
        
    return true;
}
}

/// <summary>
/// A single option within a travel event card
/// </summary>
public class TravelEventOption
{
public string Description { get; set; }

// Requirements
public int? RequiredCoins { get; set; }
public int? RequiredAttention { get; set; }
public ConnectionType? RequiredTokenType { get; set; }
public int? RequiredTokenAmount { get; set; }

// Effects
public TravelEventEffect Effect { get; set; }

public bool CanChoose(Player player, TokenMechanicsManager tokenManager)
{
    if (RequiredCoins.HasValue && player.Coins < RequiredCoins.Value)
        return false;
        
    // Attention checking would go through TimeBlockAttentionManager
    // Token checking would go through TokenMechanicsManager
    
    if (RequiredTokenType.HasValue && RequiredTokenAmount.HasValue)
    {
        // Check tokens with all NPCs for now
        // This would ideally check tokens for the specific route context
        return true; // Simplified for now - proper token checking would be route-specific
    }
    
    return true;
}
}

/// <summary>
/// Effects from choosing a travel event option
/// </summary>
public class TravelEventEffect
{
// Time effects
public int TimeChangeMinutes { get; set; }

// Resource effects
public int CoinChange { get; set; }
public int AttentionSpent { get; set; }

// Discovery effects
public bool RevealsNPCStates { get; set; }
public bool RevealsMarketInfo { get; set; }
public string UnlockedRouteId { get; set; }

// DeliveryObligation effects
public int DeadlineChangeMinutes { get; set; } // Affects all obligations in queue

// Route effects
public bool PreventsFamiliarityGain { get; set; }
public bool GrantsBonusFamiliarity { get; set; }
}

/// <summary>
/// Types of travel events (categorical)
/// </summary>
public enum TravelEventType
{
// Encounters
GuardCheckpoint,
MerchantCaravan,
FellowTravelers,
SuspiciousFigures,

// Hazards
BanditEncounter,
Pickpockets,
WeatherDelay,
NaturalHazard,
Wildlife,

// Opportunities
Shortcut,
HiddenCache,
UsefulContact,
SecretDiscovery,
MarketOpportunity,

// Delays
Crowds,
MarketDelay,
RoadRepairs
}

/// <summary>
/// Route personality determines card composition
/// </summary>
public enum RoutePersonality
{
SAFE,           // Main roads
OPPORTUNISTIC,  // Back paths
DANGEROUS,      // Wilderness
SOCIAL          // Urban streets
}
