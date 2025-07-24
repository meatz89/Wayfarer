using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// DTO for deserializing token favors from JSON.
/// </summary>
public class TokenFavorDTO
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("npcId")]
    public string NPCId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("favorType")]
    public string FavorType { get; set; }

    [JsonPropertyName("requiredTokenType")]
    public string RequiredTokenType { get; set; }

    [JsonPropertyName("tokenCost")]
    public int TokenCost { get; set; }

    [JsonPropertyName("minimumRelationshipLevel")]
    public int MinimumRelationshipLevel { get; set; } = 0;

    [JsonPropertyName("grantsId")]
    public string GrantsId { get; set; }

    [JsonPropertyName("additionalData")]
    public Dictionary<string, string> AdditionalData { get; set; } = new Dictionary<string, string>();

    [JsonPropertyName("isOneTime")]
    public bool IsOneTime { get; set; } = true;

    [JsonPropertyName("offerText")]
    public string OfferText { get; set; }

    [JsonPropertyName("purchaseText")]
    public string PurchaseText { get; set; }

    [JsonPropertyName("refusalText")]
    public string RefusalText { get; set; }
}