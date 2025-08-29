using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Loads travel encounter cards from travel_cards.json
/// </summary>
public class TravelCardLoader
{
    private readonly string _contentPath;
    private readonly JsonSerializerOptions _jsonOptions;
    private List<TravelCard> _travelCards = new();
    
    public TravelCardLoader(string contentPath)
    {
        _contentPath = contentPath;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() },
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
    }
    
    /// <summary>
    /// Load travel cards from travel_cards.json
    /// </summary>
    public List<TravelCard> LoadTravelCards()
    {
        var filePath = Path.Combine(_contentPath, "travel_cards.json");
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"WARNING: travel_cards.json not found at {filePath}");
            return new List<TravelCard>();
        }
        
        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<JsonElement>(json, _jsonOptions);
        
        if (data.TryGetProperty("travelCards", out var travelCards))
        {
            foreach (var cardElement in travelCards.EnumerateArray())
            {
                try
                {
                    var travelCard = JsonSerializer.Deserialize<TravelCard>(cardElement.GetRawText(), _jsonOptions);
                    if (travelCard != null)
                    {
                        _travelCards.Add(travelCard);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: Failed to deserialize travel card: {ex.Message}");
                }
            }
        }
        
        Console.WriteLine($"[TravelCardLoader] Loaded {_travelCards.Count} travel cards");
        return _travelCards;
    }
    
    /// <summary>
    /// Get travel cards filtered by category and terrain type
    /// </summary>
    public List<TravelCard> GetTravelCardsByCategory(string category, string terrainType = null)
    {
        var filteredCards = new List<TravelCard>();
        
        foreach (var card in _travelCards)
        {
            if (card.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true)
            {
                if (terrainType == null || 
                    card.Context?.TerrainType?.Equals(terrainType, StringComparison.OrdinalIgnoreCase) == true)
                {
                    filteredCards.Add(card);
                }
            }
        }
        
        return filteredCards;
    }
}

/// <summary>
/// Represents a travel encounter card loaded from JSON
/// </summary>
public class TravelCard
{
    public string Id { get; set; }
    public string Category { get; set; }
    public string Type { get; set; }
    public string DisplayName { get; set; }
    public string Description { get; set; }
    public int Weight { get; set; }
    public int BaseComfort { get; set; }
    public string Persistence { get; set; }
    public string ConnectionType { get; set; }
    public TravelCardMechanics Mechanics { get; set; }
    public TravelCardContext Context { get; set; }
}

/// <summary>
/// Mechanics specific to travel cards
/// </summary>
public class TravelCardMechanics
{
    public bool RequiresPermit { get; set; }
    public bool DelayTravel { get; set; }
    public bool RequiresCaution { get; set; }
    public bool RequiresPayment { get; set; }
    public bool RequiresDetour { get; set; }
    public bool ProvidesProtection { get; set; }
    public bool TradingOpportunity { get; set; }
    public bool ShelterRequired { get; set; }
    public bool ProvidesInformation { get; set; }
    public bool CompanionshipBonus { get; set; }
    public bool MoralChoice { get; set; }
    public bool PotentialReward { get; set; }
    public bool RestOpportunity { get; set; }
    public bool SafetyBonus { get; set; }
    public bool ScrutinyRisk { get; set; }
    public bool RequiresExplanation { get; set; }
    public bool MinorDelay { get; set; }
    public bool PeacefulEncounter { get; set; }
    public bool TimeSaving { get; set; }
    public bool RiskVsReward { get; set; }
    public List<string> AlternativeRoutes { get; set; } = new List<string>();
}

/// <summary>
/// Context information for travel cards
/// </summary>
public class TravelCardContext
{
    public string TerrainType { get; set; }
    public string ThreatLevel { get; set; }
}