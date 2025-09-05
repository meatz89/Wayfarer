using System;
using System.Collections.Generic;
using System.Text.Json;

public static class NPCParser
{
    /// <summary>
    /// Convert an NPCDTO to an NPC domain model
    /// </summary>
    public static NPC ConvertDTOToNPC(NPCDTO dto)
    {
        NPC npc = new NPC
        {
            ID = dto.Id ?? "",
            Name = dto.Name ?? "",
            Role = dto.Role ?? dto.Name ?? "", // Use role if provided, otherwise use name
            Description = dto.Description ?? "",
            Location = dto.LocationId ?? "",
            SpotId = dto.SpotId ?? "",
            Tier = dto.Tier
        };

        Console.WriteLine($"[DEBUG] NPCParser: Parsing NPC {npc.ID} with locationId: '{dto.LocationId}'");

        // Parse profession with mapping from JSON values to enum
        npc.Profession = MapProfessionFromJson(dto.Profession ?? "");

        // Parse personality - preserve authentic description and map to categorical type
        npc.PersonalityDescription = dto.Personality ?? "";

        // Parse personalityType directly from DTO - NO FALLBACKS
        Console.WriteLine($"[NPCParser] Parsing NPC '{npc.Name}' - personalityType from DTO: '{dto.PersonalityType}'");

        if (!string.IsNullOrEmpty(dto.PersonalityType) && Enum.TryParse<PersonalityType>(dto.PersonalityType, true, out PersonalityType parsedType))
        {
            npc.PersonalityType = parsedType;
            Console.WriteLine($"[NPCParser] Successfully parsed PersonalityType: {parsedType} for {npc.Name}");
        }
        else
        {
            // NO FALLBACKS - crash if personalityType not in DTO
            throw new InvalidOperationException($"NPC '{npc.Name}' (ID: {npc.ID}) is missing 'personalityType' in DTO or has invalid value '{dto.PersonalityType}' - fix DTO data");
        }

        // Parse services and map to ServiceTypes enum
        if (dto.Services != null)
        {
            foreach (string serviceStr in dto.Services)
            {
                ServiceTypes? mappedService = MapServiceFromJson(serviceStr);
                if (mappedService.HasValue)
                {
                    npc.ProvidedServices.Add(mappedService.Value);
                }
            }
        }

        // Set default player relationship
        npc.PlayerRelationship = NPCRelationship.Neutral;

        // Parse letter token types for letter queue system
        if (dto.LetterTokenTypes != null)
        {
            foreach (string tokenTypeStr in dto.LetterTokenTypes)
            {
                ConnectionType? tokenType = ParseConnectionType(tokenTypeStr);
                if (tokenType.HasValue && !npc.LetterTokenTypes.Contains(tokenType.Value))
                {
                    npc.LetterTokenTypes.Add(tokenType.Value);
                }
            }
        }

        // Parse initial connection state and convert to flow value
        string currentStateStr = dto.CurrentState ?? "NEUTRAL";
        if (Enum.TryParse<ConnectionState>(currentStateStr, true, out ConnectionState connectionState))
        {
            // Convert initial state to flow value (start at neutral position within state)
            npc.RelationshipFlow = connectionState switch
            {
                ConnectionState.DISCONNECTED => 2,  // Position 2 (neutral) in DISCONNECTED range
                ConnectionState.GUARDED => 7,       // Position 2 (neutral) in GUARDED range  
                ConnectionState.NEUTRAL => 12,      // Position 2 (neutral) in NEUTRAL range
                ConnectionState.RECEPTIVE => 17,    // Position 2 (neutral) in RECEPTIVE range
                ConnectionState.TRUSTING => 22,     // Position 2 (neutral) in TRUSTING range
                _ => 12
            };
        }
        else
        {
            npc.RelationshipFlow = 12; // Default to NEUTRAL at neutral flow
        }

        return npc;
    }
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

        // Parse personalityType directly from JSON - NO FALLBACKS
        string personalityTypeStr = GetStringProperty(root, "personalityType", "");
        Console.WriteLine($"[NPCParser] Parsing NPC '{npc.Name}' - personalityType from JSON: '{personalityTypeStr}'");

        if (!string.IsNullOrEmpty(personalityTypeStr) && Enum.TryParse<PersonalityType>(personalityTypeStr, true, out PersonalityType parsedType))
        {
            npc.PersonalityType = parsedType;
            Console.WriteLine($"[NPCParser] Successfully parsed PersonalityType: {parsedType} for {npc.Name}");
        }
        else
        {
            // NO FALLBACKS - crash if personalityType not in JSON
            throw new InvalidOperationException($"NPC '{npc.Name}' (ID: {npc.ID}) is missing 'personalityType' in JSON or has invalid value '{personalityTypeStr}' - add valid personalityType to npcs.json");
        }

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

        // REMOVED: Boolean flags violate deck-based architecture
        // Letters are detected by checking Request deck contents
        // Burden history detected by counting burden cards in conversation deck

        // Parse initial connection state and convert to flow value
        string currentStateStr = GetStringProperty(root, "currentState", "NEUTRAL");
        if (Enum.TryParse<ConnectionState>(currentStateStr, true, out ConnectionState connectionState))
        {
            // Convert initial state to flow value (start at neutral position within state)
            npc.RelationshipFlow = connectionState switch
            {
                ConnectionState.DISCONNECTED => 2,  // Position 2 (neutral) in DISCONNECTED range
                ConnectionState.GUARDED => 7,       // Position 2 (neutral) in GUARDED range  
                ConnectionState.NEUTRAL => 12,      // Position 2 (neutral) in NEUTRAL range
                ConnectionState.RECEPTIVE => 17,    // Position 2 (neutral) in RECEPTIVE range
                ConnectionState.TRUSTING => 22,     // Position 2 (neutral) in TRUSTING range
                _ => 12
            };
        }
        else
        {
            npc.RelationshipFlow = 12; // Default to NEUTRAL at neutral flow
        }

        // REMOVED: ActiveLetter violates deck-based architecture
        // Letters are now handled as request cards in the Request deck

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
            "Scribe" => Professions.Scribe,
            "Noble" => Professions.Noble,
            "Smuggler" => Professions.Agent,
            "Information_Broker" => Professions.Information_Broker,
            _ => throw new ArgumentException($"Unknown profession in JSON: '{jsonProfession}' - add to profession mapping")
        };
    }



    private static ServiceTypes? MapServiceFromJson(string jsonService)
    {
        return jsonService switch
        {
            "Trade" => ServiceTypes.Trade,
            "Work" => ServiceTypes.Work,
            "Information" => ServiceTypes.Information,
            "Lodging" => ServiceTypes.Rest,
            // "Shadow" is not a service type, skip it
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
            "commerce" => ConnectionType.Commerce,
            "trade" => ConnectionType.Commerce,
            "status" => ConnectionType.Status,
            "noble" => ConnectionType.Status,
            "common" => ConnectionType.Trust,
            "shadow" => ConnectionType.Shadow,
            _ => null
        };
    }

    // REMOVED: ParseActiveLetter violates deck-based architecture
    // Letters are now handled as request cards in the Request deck

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
        }
        return defaultValue;
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