using System.Text.Json;

namespace Wayfarer.Content;

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

        NPC npc = new NPC
        {
            ID = GetStringProperty(root, "id", ""),
            Name = GetStringProperty(root, "name", ""),
            Role = GetStringProperty(root, "name", ""), // Use name as role for current JSON structure
            Description = GetStringProperty(root, "description", ""),
            Location = GetStringProperty(root, "locationId", ""), // Use locationId for location
        };

        // Parse profession with mapping from JSON values to enum
        string professionStr = GetStringProperty(root, "profession", "");
        npc.Profession = MapProfessionFromJson(professionStr);

        // Set default schedule based on profession (since not in JSON)
        npc.AvailabilitySchedule = GetDefaultScheduleForProfession(npc.Profession);

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

        // Parse contract categories
        List<string> contractCategories = GetStringArray(root, "contractCategories");
        npc.ContractCategories = contractCategories;

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
            "Foreman" => Professions.Warrior, // Camp Boss
            "Harbor_Master" => Professions.Merchant, // Dock Master
            "Merchant_Captain" => Professions.Merchant, // Trade Captain
            "Laborer" => Professions.Warrior, // River Worker
            "Scholar" => Professions.Scholar, // Test NPC
            _ => Professions.Merchant // Default fallback
        };
    }

    private static Schedule GetDefaultScheduleForProfession(Professions profession)
    {
        return profession switch
        {
            Professions.Merchant => Schedule.Market_Hours,
            Professions.Warrior => Schedule.Workshop_Hours,
            Professions.Ranger => Schedule.Morning_Afternoon,
            Professions.Scholar => Schedule.Library_Hours,
            _ => Schedule.Business_Hours
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