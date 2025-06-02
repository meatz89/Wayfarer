using System.Text.Json;

public static class OpportunityParser
{
    public static OpportunityDefinition ParseOpportunity(string json)
    {
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        string id = GetStringProperty(root, "id", "id");
        string name = GetStringProperty(root, "name", id);
        string description = GetStringProperty(root, "description", "");

        // Parse opportunity type
        OpportunityTypes type = Enum.TryParse<OpportunityTypes>(
            GetStringProperty(root, "type", "ACCUMULATIVE"), true,
            out OpportunityTypes parsedType) ? parsedType : OpportunityTypes.Accumulative;

        // Parse thresholds and requirements
        int progressThreshold = GetIntProperty(root, "progressThreshold", 8);
        int expirationDays = GetIntProperty(root, "expirationDays", 3);
        int reputationRequirement = GetIntProperty(root, "reputationRequirement", 0);

        // Parse rewards
        int silverReward = GetIntProperty(root, "silverReward", 0);
        int reputationReward = GetIntProperty(root, "reputationReward", 0);
        int insightPointReward = GetIntProperty(root, "insightPointReward", 0);

        // Parse location and tier
        string initialLocationId = GetStringProperty(root, "initialLocationId", "");
        int tier = GetIntProperty(root, "tier", 1);

        // Create the opportunity
        OpportunityDefinition opportunity = new OpportunityDefinition
        {
            Id = id,
            Name = name,
            Description = description,
            Type = type,
            ProgressThreshold = progressThreshold,
            ExpirationDays = expirationDays,
            ReputationRequirement = reputationRequirement,
            InitialLocationId = initialLocationId,
            SilverReward = silverReward,
            ReputationReward = reputationReward,
            InsightPointReward = insightPointReward,
            Tier = tier
        };

        // Parse approaches
        if (root.TryGetProperty("approaches", out JsonElement approachesElement) &&
            approachesElement.ValueKind == JsonValueKind.Array)
        {
            opportunity.Approaches = ParseApproaches(approachesElement);
        }

        // For sequential Opportunities, parse the initial step
        if (type == OpportunityTypes.Sequential &&
            root.TryGetProperty("initialStep", out JsonElement initialStepElement))
        {
            opportunity.InitialStep = ParseOpportunitiestep(initialStepElement);
        }

        return opportunity;
    }

    private static Opportunitiestep ParseOpportunitiestep(JsonElement stepElement)
    {
        string name = GetStringProperty(stepElement, "name", "");
        string description = GetStringProperty(stepElement, "description", "");
        string locationId = GetStringProperty(stepElement, "locationId", "");
        int progressGoal = GetIntProperty(stepElement, "progressGoal", 5);

        Opportunitiestep step = new Opportunitiestep
        {
            Name = name,
            Description = description,
            LocationId = locationId,
            ProgressGoal = progressGoal
        };

        // Parse approaches for this step
        if (stepElement.TryGetProperty("approaches", out JsonElement approachesElement) &&
            approachesElement.ValueKind == JsonValueKind.Array)
        {
            step.Approaches = ParseApproaches(approachesElement);
        }

        return step;
    }

    private static List<ApproachDefinition> ParseApproaches(JsonElement approachesElement)
    {
        List<ApproachDefinition> approaches = new List<ApproachDefinition>();

        foreach (JsonElement approachElement in approachesElement.EnumerateArray())
        {
            string id = GetStringProperty(approachElement, "id", "");
            string name = GetStringProperty(approachElement, "name", "");
            string description = GetStringProperty(approachElement, "description", "");

            // Parse card type
            string cardTypeStr = GetStringProperty(approachElement, "requiredCardType", "");
            SkillCategories requiredCardType = Enum.TryParse<SkillCategories>(cardTypeStr, true,
                out SkillCategories parsedCardType) ? parsedCardType : SkillCategories.Physical;

            ApproachDefinition approach = new ApproachDefinition
            {
                Id = id,
                Name = name,
                Description = description,
                RequiredCardType = requiredCardType,
            };

            approaches.Add(approach);
        }

        return approaches;
    }

    // Property getters
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

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out int value))
            {
                return value;
            }
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