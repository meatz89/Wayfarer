using System;
using System.Collections.Generic;
/// <summary>
/// Defines a favor that can be purchased from an NPC using specific token types.
/// </summary>
public class TokenFavor
{
    /// <summary>
    /// Unique identifier for this favor.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The NPC who offers this favor.
    /// </summary>
    public string NPCId { get; set; }
    
    /// <summary>
    /// Human-readable name for this favor.
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Description of what this favor provides.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// The type of favor being offered.
    /// </summary>
    public TokenFavorType FavorType { get; set; }
    
    /// <summary>
    /// The token type required to purchase this favor.
    /// </summary>
    public ConnectionType RequiredTokenType { get; set; }
    
    /// <summary>
    /// Number of tokens required.
    /// </summary>
    public int TokenCost { get; set; }
    
    /// <summary>
    /// Minimum total tokens with this NPC required to unlock this favor.
    /// </summary>
    public int MinimumRelationshipLevel { get; set; } = 0;
    
    /// <summary>
    /// What the favor grants (route ID, item ID, location ID, etc.)
    /// </summary>
    public string GrantsId { get; set; }
    
    /// <summary>
    /// Additional data for the favor (e.g., item quantity).
    /// </summary>
    public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();
    
    /// <summary>
    /// Whether this favor can only be purchased once.
    /// </summary>
    public bool IsOneTime { get; set; } = true;
    
    /// <summary>
    /// Narrative text shown when the favor is offered.
    /// </summary>
    public string OfferText { get; set; }
    
    /// <summary>
    /// Narrative text shown when the favor is purchased.
    /// </summary>
    public string PurchaseText { get; set; }
    
    /// <summary>
    /// Narrative text shown when the favor is refused.
    /// </summary>
    public string RefusalText { get; set; }
    
    /// <summary>
    /// Whether this favor has been purchased.
    /// </summary>
    public bool IsPurchased { get; set; } = false;
}

public enum TokenFavorType
{
    RouteDiscovery,      // Reveals a new route
    ItemPurchase,        // Grants an item
    LocationAccess,      // Grants access to a location
    NPCIntroduction,     // Introduces a new NPC
    LetterOpportunity,   // Offers a special letter
    InformationPurchase, // Provides valuable information
    ServiceAccess        // Unlocks a special service
}