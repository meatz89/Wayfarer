using System.Text.Json;

public static class ActionJsonParser
{
    public static ActionAndEncounterResult Parse(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ActionAndEncounterResult>(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing action and encounter JSON: {ex.Message}");
            return new ActionAndEncounterResult
            {
                Action = new ActionModel
                {
                    Name = "Explore Area",
                    Goal = "Discover what's available in this location",
                    Complication = "Limited visibility and unknown terrain",
                    ActionType = BasicActionTypes.Investigate,
                    CoinCost = 0
                },
                EncounterTemplate = CreateDefaultEncounterTemplate()
            };
        }
    }

    private static EncounterTemplateModel CreateDefaultEncounterTemplate()
    {
        return new EncounterTemplateModel
        {
            Duration = 4,
            MaxPressure = 10,
            PartialThreshold = 8,
            StandardThreshold = 12,
            ExceptionalThreshold = 16,
            Hostility = "Neutral",
            MomentumBoostApproaches = new List<string> { "Analysis" },
            DangerousApproaches = new List<string> { "Dominance" },
            PressureReducingFocuses = new List<string> { "Information" },
            MomentumReducingFocuses = new List<string> { "Physical" },
            StrategicTags = new List<StrategicTagModel>
            {
                new StrategicTagModel { Name = "Natural Light", EnvironmentalProperty = "Bright" },
                new StrategicTagModel { Name = "Open Area", EnvironmentalProperty = "Expansive" },
                new StrategicTagModel { Name = "Quiet Space", EnvironmentalProperty = "Quiet" },
                new StrategicTagModel { Name = "Simple Setting", EnvironmentalProperty = "Humble" }
            },
            NarrativeTags = new List<string> { "DetailFixation", "ColdCalculation" }
        };
    }
}
