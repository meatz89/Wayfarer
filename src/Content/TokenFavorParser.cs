using System;
using System.Collections.Generic;
using System.Text.Json;
/// <summary>
/// Parser for TokenFavor entities from JSON.
/// </summary>
public static class TokenFavorParser
{
    public static List<TokenFavor> ParseTokenFavorArray(string json)
    {
        var favors = new List<TokenFavor>();
        
        using JsonDocument doc = JsonDocument.Parse(json);
        foreach (JsonElement element in doc.RootElement.EnumerateArray())
        {
            var favor = ParseTokenFavor(element);
            if (favor != null)
            {
                favors.Add(favor);
            }
        }
        
        return favors;
    }
    
    public static TokenFavor ParseTokenFavor(JsonElement element)
    {
        var dto = JsonSerializer.Deserialize<TokenFavorDTO>(element.GetRawText());
        if (dto == null)
            return null;
            
        var favor = new TokenFavor
        {
            Id = dto.Id ?? Guid.NewGuid().ToString(),
            NPCId = dto.NPCId,
            Name = dto.Name ?? "Unknown Favor",
            Description = dto.Description ?? "",
            TokenCost = dto.TokenCost,
            MinimumRelationshipLevel = dto.MinimumRelationshipLevel,
            GrantsId = dto.GrantsId,
            AdditionalData = dto.AdditionalData ?? new Dictionary<string, string>(),
            IsOneTime = dto.IsOneTime,
            OfferText = dto.OfferText ?? "",
            PurchaseText = dto.PurchaseText ?? "",
            RefusalText = dto.RefusalText ?? ""
        };
        
        // Parse favor type
        if (Enum.TryParse<TokenFavorType>(dto.FavorType, true, out var favorType))
        {
            favor.FavorType = favorType;
        }
        else
        {
            Console.WriteLine($"[WARNING] Unknown favor type: {dto.FavorType}");
            return null;
        }
        
        // Parse required token type
        if (Enum.TryParse<ConnectionType>(dto.RequiredTokenType, true, out var tokenType))
        {
            favor.RequiredTokenType = tokenType;
        }
        else
        {
            Console.WriteLine($"[WARNING] Unknown token type: {dto.RequiredTokenType}");
            return null;
        }
        
        return favor;
    }
}