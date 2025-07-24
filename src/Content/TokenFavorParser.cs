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
        List<TokenFavor> favors = new List<TokenFavor>();

        using JsonDocument doc = JsonDocument.Parse(json);
        foreach (JsonElement element in doc.RootElement.EnumerateArray())
        {
            TokenFavor favor = ParseTokenFavor(element);
            if (favor != null)
            {
                favors.Add(favor);
            }
        }

        return favors;
    }

    public static TokenFavor ParseTokenFavor(JsonElement element)
    {
        TokenFavorDTO? dto = JsonSerializer.Deserialize<TokenFavorDTO>(element.GetRawText());
        if (dto == null)
            return null;

        TokenFavor favor = new TokenFavor
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
        if (EnumParser.TryParse<TokenFavorType>(dto.FavorType, out TokenFavorType favorType))
        {
            favor.FavorType = favorType;
        }
        else
        {
            Console.WriteLine($"[WARNING] Unknown favor type: {dto.FavorType}");
            return null;
        }

        // Parse required token type
        if (EnumParser.TryParse<ConnectionType>(dto.RequiredTokenType, out ConnectionType tokenType))
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