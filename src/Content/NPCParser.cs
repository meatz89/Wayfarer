using System.Text.Json;
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
        };
        
        Console.WriteLine($"[DEBUG] NPCParser: Parsing NPC {npc.ID} with locationId: '{locationId}'");

        // Parse profession with mapping from JSON values to enum
        string professionStr = GetStringProperty(root, "profession", "");
        npc.Profession = MapProfessionFromJson(professionStr);

        // Parse schedule from JSON if available, otherwise use default based on profession
        string scheduleStr = GetStringProperty(root, "availabilitySchedule", "");
        if (!string.IsNullOrEmpty(scheduleStr))
        {
            npc.AvailabilitySchedule = ParseScheduleFromJson(scheduleStr);
        }
        else
        {
            npc.AvailabilitySchedule = GetDefaultScheduleForProfession(npc.Profession);
        }

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
        var letterTokenTypes = GetStringArray(root, "letterTokenTypes");
        foreach (var tokenTypeStr in letterTokenTypes)
        {
            var tokenType = ParseConnectionType(tokenTypeStr);
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
            "Craftsman" => Professions.Merchant, // Workshop Master
            "Merchant" => Professions.Merchant,
            "Innkeeper" => Professions.Merchant,
            "Woodsman" => Professions.Ranger, // Logger
            "Herbalist" => Professions.Ranger, // Herb Gatherer
            "Foreman" => Professions.Soldier, // Camp Boss
            "Harbor_Master" => Professions.Merchant, // Dock Master
            "Merchant_Captain" => Professions.Merchant, // Trade Captain
            "Laborer" => Professions.Soldier, // River Worker
            "Scholar" => Professions.Scholar, // Test NPC
            _ => Professions.Merchant // Default fallback
        };
    }

    private static Schedule GetDefaultScheduleForProfession(Professions profession)
    {
        return profession switch
        {
            Professions.Merchant => Schedule.Market_Hours,
            Professions.Soldier => Schedule.Workshop_Hours,
            Professions.Ranger => Schedule.Morning_Afternoon,
            Professions.Scholar => Schedule.Library_Hours,
            _ => Schedule.Business_Hours
        };
    }

    private static Schedule ParseScheduleFromJson(string scheduleStr)
    {
        return scheduleStr switch
        {
            "Always" => Schedule.Always,
            "Market_Hours" => Schedule.Market_Hours,
            "Workshop_Hours" => Schedule.Workshop_Hours,
            "Library_Hours" => Schedule.Library_Hours,
            "Business_Hours" => Schedule.Business_Hours,
            "Morning_Evening" => Schedule.Morning_Evening,
            "Morning_Afternoon" => Schedule.Morning_Afternoon,
            "Afternoon_Evening" => Schedule.Afternoon_Evening,
            "Evening_Only" => Schedule.Evening_Only,
            "Morning_Only" => Schedule.Morning_Only,
            "Afternoon_Only" => Schedule.Afternoon_Only,
            "Evening_Night" => Schedule.Evening_Night,
            "Dawn_Only" => Schedule.Dawn_Only,
            "Night_Only" => Schedule.Night_Only,
            _ => Schedule.Business_Hours // Default fallback
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
            "trade" => ConnectionType.Trade,
            "noble" => ConnectionType.Noble,
            "common" => ConnectionType.Common,
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