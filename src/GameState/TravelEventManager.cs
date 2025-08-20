using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages travel event card drawing and resolution during route travel
/// Integrates with existing travel system to add narrative depth
/// </summary>
public class TravelEventManager
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly TimeBlockAttentionManager _attentionManager;
    private readonly MessageSystem _messageSystem;
    private readonly ITimeManager _timeManager;
    private readonly Dictionary<string, RouteDeck> _routeDecks;
    
    public TravelEventManager(
        GameWorld gameWorld,
        TokenMechanicsManager tokenManager,
        TimeBlockAttentionManager attentionManager,
        MessageSystem messageSystem,
        ITimeManager timeManager)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _attentionManager = attentionManager;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
        _routeDecks = new Dictionary<string, RouteDeck>();
    }
    
    /// <summary>
    /// Get or create route deck for a specific route
    /// </summary>
    private RouteDeck GetRouteDeck(RouteOption route)
    {
        if (!_routeDecks.ContainsKey(route.Id))
        {
            // Create deck based on route personality
            _routeDecks[route.Id] = new RouteDeck(route.Id, route.Personality);
        }
        return _routeDecks[route.Id];
    }
    
    /// <summary>
    /// Draw travel event cards for a route based on familiarity
    /// </summary>
    public TravelEventResult DrawTravelEvents(RouteOption route, TransportType transport)
    {
        var player = _gameWorld.GetPlayer();
        var familiarity = player.GetRouteFamiliarity(route.Id);
        var weather = _gameWorld.WorldState.CurrentWeather;
        var timeBlock = _timeManager.GetCurrentTimeBlock();
        
        // Get route deck and draw cards
        var deck = GetRouteDeck(route);
        var drawnCards = deck.DrawCards(familiarity, weather, timeBlock);
        
        // Special case: Mastered routes can skip events entirely
        bool canSkipEvents = familiarity >= 5 && transport == TransportType.Walking;
        
        // Modify cards based on transport type
        var availableCards = ModifyCardsForTransport(drawnCards, transport);
        
        return new TravelEventResult
        {
            DrawnCards = availableCards,
            CanSkipAllEvents = canSkipEvents,
            MustResolveCount = GetMustResolveCount(familiarity, transport),
            Transport = transport,
            Familiarity = familiarity
        };
    }
    
    /// <summary>
    /// Modify cards based on transport type
    /// </summary>
    private List<TravelEventCard> ModifyCardsForTransport(List<TravelEventCard> cards, TransportType transport)
    {
        switch (transport)
        {
            case TransportType.Walking:
                // Walking: Must resolve all negative cards
                return cards; // All cards available as-is
                
            case TransportType.Cart:
                // Cart: Negative cards are optional
                foreach (var card in cards)
                {
                    if (card.IsNegative)
                    {
                        // Add "Pay to avoid" option
                        card.Options.Add(new TravelEventOption
                        {
                            Description = "Pay driver to avoid (Cart benefit)",
                            RequiredCoins = 1,
                            Effect = new TravelEventEffect { CoinChange = -1, TimeChangeMinutes = 0 }
                        });
                    }
                }
                return cards;
                
            case TransportType.Carriage:
                // Carriage: Ignore negative cards, double comfort benefits
                var filteredCards = cards.Where(c => !c.IsNegative).ToList();
                foreach (var card in filteredCards.Where(c => c.IsComfort))
                {
                    // Double positive effects for comfort cards
                    foreach (var option in card.Options)
                    {
                        if (option.Effect.TimeChangeMinutes < 0)
                            option.Effect.TimeChangeMinutes *= 2;
                        if (option.Effect.CoinChange > 0)
                            option.Effect.CoinChange *= 2;
                    }
                }
                return filteredCards;
                
            default:
                return cards;
        }
    }
    
    /// <summary>
    /// Determine how many cards must be resolved
    /// </summary>
    private int GetMustResolveCount(int familiarity, TransportType transport)
    {
        if (transport == TransportType.Carriage)
            return 0; // Carriage can ignore all events
            
        return familiarity switch
        {
            0 => 1,  // Unknown: Must resolve the one card drawn
            _ => 1   // All other levels: Choose 1 from drawn cards
        };
    }
    
    /// <summary>
    /// Resolve a travel event option choice
    /// </summary>
    public TravelResolutionResult ResolveEventOption(TravelEventCard card, TravelEventOption option, RouteOption route)
    {
        var player = _gameWorld.GetPlayer();
        var result = new TravelResolutionResult
        {
            Success = true,
            TimeChange = option.Effect.TimeChangeMinutes,
            Messages = new List<string>()
        };
        
        // Check if player can afford the option
        if (!option.CanChoose(player, _tokenManager))
        {
            result.Success = false;
            result.Messages.Add("Cannot afford this option!");
            return result;
        }
        
        // Apply resource costs
        if (option.RequiredCoins.HasValue)
        {
            player.ModifyCoins(-option.RequiredCoins.Value);
            result.Messages.Add($"Paid {option.RequiredCoins.Value} coins");
        }
        
        if (option.RequiredAttention.HasValue)
        {
            // Get current attention from the manager
            var (current, max) = _attentionManager.GetAttentionState();
            if (current >= option.RequiredAttention.Value)
            {
                // Spend attention through the attention manager
                var currentAttention = _attentionManager.GetCurrentAttention(_timeManager.GetCurrentTimeBlock());
                if (currentAttention.TrySpend(option.RequiredAttention.Value))
                {
                    result.Messages.Add($"Spent {option.RequiredAttention.Value} attention");
                }
                else
                {
                    result.Success = false;
                    result.Messages.Add("Not enough attention!");
                    return result;
                }
            }
            else
            {
                result.Success = false;
                result.Messages.Add("Not enough attention!");
                return result;
            }
        }
        
        // Apply effects
        ApplyEventEffects(option.Effect, route, result);
        
        // Show resolution in message system
        _messageSystem.AddSystemMessage(
            $"[{card.Title}] {option.Description}",
            result.Success ? SystemMessageTypes.Success : SystemMessageTypes.Warning
        );
        
        foreach (var message in result.Messages)
        {
            _messageSystem.AddSystemMessage(message, SystemMessageTypes.Info);
        }
        
        return result;
    }
    
    /// <summary>
    /// Apply the effects of a travel event choice
    /// </summary>
    private void ApplyEventEffects(TravelEventEffect effect, RouteOption route, TravelResolutionResult result)
    {
        var player = _gameWorld.GetPlayer();
        
        // Time changes are tracked in result
        result.TimeChange = effect.TimeChangeMinutes;
        if (effect.TimeChangeMinutes != 0)
        {
            var timeText = effect.TimeChangeMinutes > 0 ? $"+{effect.TimeChangeMinutes}" : effect.TimeChangeMinutes.ToString();
            result.Messages.Add($"Travel time {timeText} minutes");
        }
        
        // Coin changes
        if (effect.CoinChange != 0)
        {
            player.ModifyCoins(effect.CoinChange);
            var coinText = effect.CoinChange > 0 ? $"Found {effect.CoinChange}" : $"Lost {-effect.CoinChange}";
            result.Messages.Add($"{coinText} coins");
        }
        
        // Information reveals
        if (effect.RevealsNPCStates)
        {
            // This would reveal NPC emotional states at destination
            result.Messages.Add("Learned about NPCs at destination");
            result.RevealedInformation.Add("npc_states");
        }
        
        if (effect.RevealsMarketInfo)
        {
            // This would reveal market prices or opportunities
            result.Messages.Add("Discovered market information");
            result.RevealedInformation.Add("market_info");
        }
        
        // Route unlocking
        if (!string.IsNullOrEmpty(effect.UnlockedRouteId))
        {
            // Mark route as discovered
            var unlockedRoute = GetRouteById(effect.UnlockedRouteId);
            if (unlockedRoute != null && !unlockedRoute.IsDiscovered)
            {
                unlockedRoute.IsDiscovered = true;
                result.Messages.Add($"Discovered new route: {unlockedRoute.Name}!");
                result.UnlockedRouteId = effect.UnlockedRouteId;
            }
        }
        
        // DeliveryObligation effects - REMOVED
        // Letters are only delivered through player actions, not travel events
        
        if (effect.DeadlineChangeMinutes != 0)
        {
            // This affects all letters in queue
            AdjustAllDeadlines(effect.DeadlineChangeMinutes);
            var deadlineText = effect.DeadlineChangeMinutes > 0 ? "delayed" : "expedited";
            result.Messages.Add($"All deliveries {deadlineText} by {Math.Abs(effect.DeadlineChangeMinutes)} minutes");
        }
        
        // Familiarity changes
        if (effect.GrantsBonusFamiliarity)
        {
            player.IncreaseRouteFamiliarity(route.Id, 1);
            result.Messages.Add("Gained extra route familiarity!");
            result.FamiliarityGained = 2; // 1 base + 1 bonus
        }
        else if (!effect.PreventsFamiliarityGain)
        {
            result.FamiliarityGained = 1; // Normal gain
        }
        else
        {
            result.FamiliarityGained = 0; // Prevented (carriage)
        }
    }
    
    /// <summary>
    /// Get route by ID from world state
    /// </summary>
    private RouteOption GetRouteById(string routeId)
    {
        foreach (var location in _gameWorld.WorldState.locations)
        {
            foreach (var connection in location.Connections)
            {
                var route = connection.RouteOptions.FirstOrDefault(r => r.Id == routeId);
                if (route != null) return route;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Adjust all letter deadlines (from travel events)
    /// </summary>
    private void AdjustAllDeadlines(int minuteChange)
    {
        var player = _gameWorld.GetPlayer();
        
        // Adjust queue letters
        foreach (var letter in player.ObligationQueue.Where(l => l != null))
        {
            // This would need proper deadline adjustment logic
            // For now just track that it happened
        }
        
        // Adjust carried letters
        foreach (var letter in player.CarriedLetters)
        {
            // This would need proper deadline adjustment logic
        }
    }
    
    /// <summary>
    /// Complete travel and apply familiarity gains
    /// </summary>
    public void CompleteTravelWithEvents(RouteOption route, TravelResolutionResult eventResult)
    {
        var player = _gameWorld.GetPlayer();
        
        // Apply familiarity gain from successful travel
        if (eventResult.FamiliarityGained > 0)
        {
            player.IncreaseRouteFamiliarity(route.Id, eventResult.FamiliarityGained);
            
            var newFamiliarity = player.GetRouteFamiliarity(route.Id);
            if (newFamiliarity == 5)
            {
                _messageSystem.AddSystemMessage(
                    $"Route Mastered: {route.Name}! You can now skip travel events on this route.",
                    SystemMessageTypes.Success
                );
            }
            else if (eventResult.FamiliarityGained > 1)
            {
                _messageSystem.AddSystemMessage(
                    $"Route familiarity increased to {newFamiliarity}/5 (bonus gain!)",
                    SystemMessageTypes.Info
                );
            }
            else
            {
                _messageSystem.AddSystemMessage(
                    $"Route familiarity: {newFamiliarity}/5",
                    SystemMessageTypes.Info
                );
            }
        }
    }
}

/// <summary>
/// Result of drawing travel event cards
/// </summary>
public class TravelEventResult
{
    public List<TravelEventCard> DrawnCards { get; set; } = new List<TravelEventCard>();
    public bool CanSkipAllEvents { get; set; }
    public int MustResolveCount { get; set; }
    public TransportType Transport { get; set; }
    public int Familiarity { get; set; }
}

/// <summary>
/// Result of resolving a travel event
/// </summary>
public class TravelResolutionResult
{
    public bool Success { get; set; }
    public int TimeChange { get; set; }
    public int FamiliarityGained { get; set; } = 1;
    public List<string> Messages { get; set; } = new List<string>();
    public List<string> RevealedInformation { get; set; } = new List<string>();
    public string UnlockedRouteId { get; set; }
    public bool SecondaryLetterDelivered { get; set; }
}

/// <summary>
/// Transport type affects card resolution
/// </summary>
public enum TransportType
{
    Walking,   // Free, must resolve negative, builds familiarity
    Cart,      // 2 coins, can pay to avoid negative, normal familiarity
    Carriage   // 5 coins, ignore negative, double comfort, no familiarity
}