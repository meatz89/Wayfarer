using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages observable events and partial information at locations
/// These appear in the "YOU NOTICE:" section of location screens
/// </summary>
public class ObservationSystem
{
    private readonly GameWorld _gameWorld;
    private readonly ITimeManager _timeManager;
    private readonly NPCRepository _npcRepository;
    private readonly Dictionary<string, List<Observable>> _locationObservables;
    private readonly HashSet<string> _revealedObservations;

    public ObservationSystem(
        GameWorld gameWorld,
        ITimeManager timeManager,
        NPCRepository npcRepository)
    {
        _gameWorld = gameWorld;
        _timeManager = timeManager;
        _npcRepository = npcRepository;
        _revealedObservations = new HashSet<string>();
        _locationObservables = InitializeObservables();
    }

    /// <summary>
    /// Get current observations for a location
    /// </summary>
    public List<ObservableViewModel> GetObservations(string locationId)
    {
        List<ObservableViewModel> results = new List<ObservableViewModel>();

        if (!_locationObservables.ContainsKey(locationId))
            return results;

        List<Observable> observables = _locationObservables[locationId];
        ObservationContext context = CreateContext();

        foreach (Observable obs in observables)
        {
            if (obs.DisplayCondition(context))
            {
                bool isRevealed = _revealedObservations.Contains(obs.Id) ||
                                  obs.RevealCondition(context);

                if (isRevealed)
                {
                    _revealedObservations.Add(obs.Id);
                }

                results.Add(new ObservableViewModel
                {
                    Icon = obs.Icon,
                    Text = isRevealed ? obs.FullText : obs.PartialText,
                    IsKnown = isRevealed,
                    Type = obs.Type
                });
            }
        }

        // Add dynamic NPCs moving through location
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        List<NPC> npcsHere = allNpcs.Where(n => n.Location == locationId).ToList();
        foreach (NPC? npc in npcsHere.Take(2)) // Max 2 NPC observations
        {
            results.Add(new ObservableViewModel
            {
                Icon = "👤",
                Text = $"{npc.Name} {GetNPCActivity(npc)}",
                IsKnown = true,
                Type = ObservationType.NPC
            });
        }

        return results.OrderByDescending(o => o.Type == ObservationType.Important)
                      .Take(3) // Max 3 observations shown
                      .ToList();
    }

    private Dictionary<string, List<Observable>> InitializeObservables()
    {
        return new Dictionary<string, List<Observable>>
        {
            ["market_square"] = new List<Observable>
            {
                new Observable
                {
                    Id = "market_guards",
                    Icon = "💂",
                    FullText = "Guards blocking north road",
                    PartialText = "Guards blocking north road",
                    Type = ObservationType.Important,
                    DisplayCondition = ctx => ctx.Hour >= 14 && ctx.Hour <= 18,
                    RevealCondition = ctx => true // Always revealed
                },
                new Observable
                {
                    Id = "market_courier",
                    Icon = "❓",
                    FullText = "Garrett watching you from the shadows",
                    PartialText = "Courier watching you",
                    Type = ObservationType.Mystery,
                    DisplayCondition = ctx => ctx.HasShadowDebt,
                    RevealCondition = ctx => ctx.ShadowTokens >= 3
                },
                new Observable
                {
                    Id = "market_merchant",
                    Icon = "🛒",
                    FullText = "Marcus heading to his stall",
                    PartialText = "Marcus heading to his stall",
                    Type = ObservationType.Normal,
                    DisplayCondition = ctx => ctx.Hour >= 7 && ctx.Hour <= 10,
                    RevealCondition = ctx => true
                }
            },
            ["noble_district"] = new List<Observable>
            {
                new Observable
                {
                    Id = "noble_carriage",
                    Icon = "🚪",
                    FullText = "Lord Aldwin's carriage preparing to leave",
                    PartialText = "Fancy carriage being loaded",
                    Type = ObservationType.Important,
                    DisplayCondition = ctx => ctx.Hour >= 15 && ctx.Hour <= 17,
                    RevealCondition = ctx => ctx.HasNobleInfo
                },
                new Observable
                {
                    Id = "noble_servants",
                    Icon = "👥",
                    FullText = "Servants whispering about a scandal",
                    PartialText = "Servants whispering urgently",
                    Type = ObservationType.Mystery,
                    DisplayCondition = ctx => ctx.Hour >= 6 && ctx.Hour <= 9,
                    RevealCondition = ctx => ctx.StatusTokens >= 2
                }
            },
            ["merchant_row"] = new List<Observable>
            {
                new Observable
                {
                    Id = "merchant_deal",
                    Icon = "💰",
                    FullText = "Rare goods shipment arriving at noon",
                    PartialText = "Something big happening at noon",
                    Type = ObservationType.Opportunity,
                    DisplayCondition = ctx => ctx.Hour >= 10 && ctx.Hour <= 11,
                    RevealCondition = ctx => ctx.CommerceTokens >= 1
                },
                new Observable
                {
                    Id = "merchant_thugs",
                    Icon = "⚠️",
                    FullText = "Debt collectors looking for someone",
                    PartialText = "Rough types loitering around",
                    Type = ObservationType.Important,
                    DisplayCondition = ctx => ctx.HasDebt,
                    RevealCondition = ctx => ctx.HasDebt
                }
            },
            ["riverside"] = new List<Observable>
            {
                new Observable
                {
                    Id = "river_smugglers",
                    Icon = "🚢",
                    FullText = "Smugglers unloading after dark",
                    PartialText = "Unusual activity at the docks",
                    Type = ObservationType.Mystery,
                    DisplayCondition = ctx => ctx.Hour >= 20 || ctx.Hour <= 4,
                    RevealCondition = ctx => ctx.ShadowTokens >= 2
                },
                new Observable
                {
                    Id = "river_guards",
                    Icon = "🔍",
                    FullText = "Guards searching all packages",
                    PartialText = "Guards searching all packages",
                    Type = ObservationType.Important,
                    DisplayCondition = ctx => ctx.Hour >= 8 && ctx.Hour <= 16,
                    RevealCondition = ctx => true
                }
            },
            ["city_gates"] = new List<Observable>
            {
                new Observable
                {
                    Id = "gates_closing",
                    Icon = "🚪",
                    FullText = "Gates close at 9 PM sharp",
                    PartialText = "Gates close at 9 PM sharp",
                    Type = ObservationType.Important,
                    DisplayCondition = ctx => ctx.Hour >= 19,
                    RevealCondition = ctx => true
                },
                new Observable
                {
                    Id = "gates_traveler",
                    Icon = "🎒",
                    FullText = "Wealthy traveler seeking courier",
                    PartialText = "Traveler looking around anxiously",
                    Type = ObservationType.Opportunity,
                    DisplayCondition = ctx => ctx.Hour >= 8 && ctx.Hour <= 12,
                    RevealCondition = ctx => ctx.TrustTokens >= 1
                }
            }
        };
    }

    private string GetNPCActivity(NPC npc)
    {
        int hour = _timeManager.GetCurrentTimeHours();

        return npc.Profession switch
        {
            Professions.Merchant when hour < 12 => "setting up shop",
            Professions.Merchant => "counting coins",
            Professions.Soldier when hour >= 20 => "starting night patrol",
            Professions.Soldier => "on patrol",
            Professions.Noble => "passing through",
            Professions.Scholar => "reading documents",
            _ => "going about their business"
        };
    }

    private ObservationContext CreateContext()
    {
        Player player = _gameWorld.GetPlayer();
        Dictionary<ConnectionType, int> tokens = player.ConnectionTokens;

        return new ObservationContext
        {
            Hour = _timeManager.GetCurrentTimeHours(),
            HasDebt = tokens.Values.Any(v => v < 0),
            HasShadowDebt = tokens.ContainsKey(ConnectionType.Shadow) &&
                tokens[ConnectionType.Shadow] < 0,
            TrustTokens = GetTokenValue(tokens, ConnectionType.Trust),
            CommerceTokens = GetTokenValue(tokens, ConnectionType.Commerce),
            StatusTokens = GetTokenValue(tokens, ConnectionType.Status),
            ShadowTokens = GetTokenValue(tokens, ConnectionType.Shadow),
            HasNobleInfo = false // Simplified for now
        };
    }

    private int GetTokenValue(Dictionary<ConnectionType, int> tokens, ConnectionType type)
    {
        return tokens.ContainsKey(type) ? Math.Max(0, tokens[type]) : 0;
    }
}

/// <summary>
/// Represents a single observable event or entity
/// </summary>
public class Observable
{
    public string Id { get; set; }
    public string Icon { get; set; }
    public string FullText { get; set; }
    public string PartialText { get; set; }
    public ObservationType Type { get; set; }
    public Func<ObservationContext, bool> DisplayCondition { get; set; }
    public Func<ObservationContext, bool> RevealCondition { get; set; }
}

/// <summary>
/// View model for displaying observations
/// </summary>
public class ObservableViewModel
{
    public string Icon { get; set; }
    public string Text { get; set; }
    public bool IsKnown { get; set; }
    public ObservationType Type { get; set; }
}

/// <summary>
/// Types of observations for prioritization
/// </summary>
public enum ObservationType
{
    Normal,
    Important,
    Mystery,
    Opportunity,
    NPC
}

/// <summary>
/// Context for evaluating observation conditions
/// </summary>
public class ObservationContext
{
    public int Hour { get; set; }
    public bool HasDebt { get; set; }
    public bool HasShadowDebt { get; set; }
    public int TrustTokens { get; set; }
    public int CommerceTokens { get; set; }
    public int StatusTokens { get; set; }
    public int ShadowTokens { get; set; }
    public bool HasNobleInfo { get; set; }
}