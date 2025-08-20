using System;
using System.Collections.Generic;
using System.Text.Json;
using Wayfarer.Game.MainSystem;
using Wayfarer.GameState;
using Wayfarer.GameState.Constants;
using Wayfarer.Content;

namespace Wayfarer.Content
public static class NPCParser
{
    public static NPC ParseNPC(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        string locationId = GetStringProperty(root, "locationId", "");

        NPC npc = new NPC
        {
            ID = GetStringProperty(root, "id", ""),
            Name = GetStringProperty(root, "name", ""),
            Role = GetStringProperty(root, "name", ""), // Use name as role for current JSON structure
            Description = GetStringProperty(root, "description", ""),
            Location = locationId, // Use locationId for location
            SpotId = GetStringProperty(root, "spotId", ""), // Map spotId from JSON
        };

        Console.WriteLine($"[DEBUG] NPCParser: Parsing NPC {npc.ID} with locationId: '{locationId}'");

        // Parse profession with mapping from JSON values to enum
        string professionStr = GetStringProperty(root, "profession", "");
        npc.Profession = MapProfessionFromJson(professionStr);

        // Parse personality - preserve authentic description and map to categorical type
        string personalityDescription = GetStringProperty(root, "personality", "");
        npc.PersonalityDescription = personalityDescription;
        npc.PersonalityType = PersonalityMappingService.GetPersonalityType(personalityDescription);

        // NPCs are always available - no schedule parsing needed

        // Parse services and map to ServiceTypes enum
        List<string> serviceStrings = GetStringArray(root, "services");
        foreach (string serviceStr in serviceStrings)
        {
            ServiceTypes? mappedService = MapServiceFromJson(serviceStr);
            if (mappedService.HasValue)
            {
                npc.ProvidedServices.Add(mappedService.Value);
            }
        }

        // Set default player relationship
        npc.PlayerRelationship = NPCRelationship.Neutral;

        // Parse letter token types for letter queue system
        List<string> letterTokenTypes = GetStringArray(root, "letterTokenTypes");
        foreach (string tokenTypeStr in letterTokenTypes)
        {
            ConnectionType? tokenType = ParseConnectionType(tokenTypeStr);
            if (tokenType.HasValue && !npc.LetterTokenTypes.Contains(tokenType.Value))
            {
                npc.LetterTokenTypes.Add(tokenType.Value);
            }
        }

        return npc;
    }

    private static Professions MapProfessionFromJson(string jsonProfession)
    {
        return jsonProfession switch
        {
            "Craftsman" => Professions.Craftsman,
            "Merchant" => Professions.Merchant,
            "Innkeeper" => Professions.Innkeeper,
            "Soldier" => Professions.Soldier,
            "Scholar" => Professions.Scholar,
            _ => Professions.Merchant // Default fallback
        };
    }



    private static ServiceTypes? MapServiceFromJson(string jsonService)
    {
        return jsonService switch
        {
            "equipment_commissioning" => ServiceTypes.EquipmentRepair,
            "workshop_contracts" => ServiceTypes.Training,
            "trade_goods" => ServiceTypes.Trade,
            "delivery_contracts" => ServiceTypes.Trading,
            "rest_services" => ServiceTypes.Rest,
            "labor_contracts" => ServiceTypes.Training,
            "lumber_sales" => ServiceTypes.Trade,
            "logging_contracts" => ServiceTypes.Training,
            "herb_sales" => ServiceTypes.Trade,
            "gathering_contracts" => ServiceTypes.Training,
            "heavy_labor" => ServiceTypes.Training,
            "equipment_repair" => ServiceTypes.EquipmentRepair,
            "fish_sales" => ServiceTypes.Trade,
            "dock_work" => ServiceTypes.Training,
            "transport_contracts" => ServiceTypes.Trading,
            "bulk_trade" => ServiceTypes.Trading,
            "simple_labor" => ServiceTypes.Training,
            "boat_maintenance" => ServiceTypes.EquipmentRepair,
            _ => null // Unknown service
        };
    }

    private static ConnectionType? ParseConnectionType(string connectionTypeStr)
    {
        return connectionTypeStr.ToLower() switch
        {
            "trust" => ConnectionType.Trust,
            "trade" => ConnectionType.Commerce,
            "noble" => ConnectionType.Status,
            "common" => ConnectionType.Trust,
            "shadow" => ConnectionType.Shadow,
            _ => null
        };
    }

    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString() ?? defaultValue;
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
        return defaultValue;
    }

    private static List<string> GetStringArray(JsonElement element, string propertyName)
    {
        List<string> results = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string value = item.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(value);
                    }
                }
            }
        }

        return results;
    }
}
}