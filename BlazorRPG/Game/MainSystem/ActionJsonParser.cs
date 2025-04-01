using System;
using System.Collections.Generic;
using System.Text.Json;

public static class ActionJsonParser
{
    public static ActionCreationResult Parse(string json)
    {
        // Initialize with default values in case parsing fails
        ActionCreationResult result = new ActionCreationResult
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

        try
        {
            // Parse the JSON document
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // Extract action data if available
            if (root.TryGetProperty("action", out JsonElement actionElement))
            {
                result.Action = ParseActionModel(actionElement);
            }

            // Extract encounter template data if available
            if (root.TryGetProperty("encounterTemplate", out JsonElement templateElement))
            {
                result.EncounterTemplate = ParseEncounterTemplate(templateElement);
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing action and encounter JSON: {ex.Message}");
            Console.WriteLine($"Raw JSON: {json}");
            // Return the default result we already initialized
            return result;
        }
    }

    private static ActionModel ParseActionModel(JsonElement element)
    {
        ActionModel model = new ActionModel
        {
            Name = "Explore Area",
            Goal = "Discover what's available in this location",
            Complication = "Limited visibility and unknown terrain",
            ActionType = BasicActionTypes.Investigate,
            CoinCost = 0
        };

        // Extract values if they exist
        if (element.TryGetProperty("name", out JsonElement nameElement) &&
            nameElement.ValueKind == JsonValueKind.String)
        {
            model.Name = nameElement.GetString() ?? model.Name;
        }

        if (element.TryGetProperty("goal", out JsonElement goalElement) &&
            goalElement.ValueKind == JsonValueKind.String)
        {
            model.Goal = goalElement.GetString() ?? model.Goal;
        }

        if (element.TryGetProperty("complication", out JsonElement compElement) &&
            compElement.ValueKind == JsonValueKind.String)
        {
            model.Complication = compElement.GetString() ?? model.Complication;
        }

        if (element.TryGetProperty("actionType", out JsonElement typeElement) &&
            typeElement.ValueKind == JsonValueKind.String)
        {
            string typeStr = typeElement.GetString() ?? "";
            if (Enum.TryParse<BasicActionTypes>(typeStr, true, out BasicActionTypes type))
            {
                model.ActionType = type;
            }
        }

        if (element.TryGetProperty("coinCost", out JsonElement costElement))
        {
            if (costElement.ValueKind == JsonValueKind.Number)
            {
                model.CoinCost = costElement.GetInt32();
            }
            else if (costElement.ValueKind == JsonValueKind.String &&
                     int.TryParse(costElement.GetString(), out int cost))
            {
                model.CoinCost = cost;
            }
        }

        return model;
    }

    private static EncounterTemplateModel ParseEncounterTemplate(JsonElement element)
    {
        EncounterTemplateModel template = CreateDefaultEncounterTemplate();

        // Parse simple numeric properties - fixing the ref error
        if (element.TryGetProperty("name", out JsonElement encounterNameElement) &&
            encounterNameElement.ValueKind == JsonValueKind.String)
        {
            template.Name = encounterNameElement.GetString() ?? string.Empty;
        }

        template.Duration = GetInt32Property(element, "duration", template.Duration);
        template.MaxPressure = GetInt32Property(element, "maxPressure", template.MaxPressure);
        template.PartialThreshold = GetInt32Property(element, "partialThreshold", template.PartialThreshold);
        template.StandardThreshold = GetInt32Property(element, "standardThreshold", template.StandardThreshold);
        template.ExceptionalThreshold = GetInt32Property(element, "exceptionalThreshold", template.ExceptionalThreshold);

        // Parse hostility
        if (element.TryGetProperty("hostility", out JsonElement hostilityElement) &&
            hostilityElement.ValueKind == JsonValueKind.String)
        {
            template.Hostility = hostilityElement.GetString() ?? template.Hostility;
        }

        // Parse string arrays - fixing the ref error
        template.PressureReducingFocuses = GetStringArray(element, "pressureReducingFocuses", template.PressureReducingFocuses);
        template.MomentumReducingFocuses = GetStringArray(element, "momentumReducingFocuses", template.MomentumReducingFocuses);
        template.NarrativeTags = GetStringArray(element, "narrativeTags", template.NarrativeTags);

        // Parse strategic tags
        if (element.TryGetProperty("strategicTags", out JsonElement tagsElement) &&
            tagsElement.ValueKind == JsonValueKind.Array)
        {
            List<StrategicTagModel> strategicTags = new List<StrategicTagModel>();

            foreach (JsonElement tagElement in tagsElement.EnumerateArray())
            {
                StrategicTagModel tag = new StrategicTagModel
                {
                    Name = "Default Tag",
                    EnvironmentalProperty = "Bright"
                };

                if (tagElement.TryGetProperty("name", out JsonElement nameElement) &&
                    nameElement.ValueKind == JsonValueKind.String)
                {
                    tag.Name = nameElement.GetString() ?? tag.Name;
                }

                if (tagElement.TryGetProperty("environmentalProperty", out JsonElement propElement) &&
                    propElement.ValueKind == JsonValueKind.String)
                {
                    tag.EnvironmentalProperty = propElement.GetString() ?? tag.EnvironmentalProperty;
                }

                strategicTags.Add(tag);
            }

            // Only replace the list if we actually found some tags
            if (strategicTags.Count > 0)
            {
                template.StrategicTags = strategicTags;
            }
        }

        return template;
    }

    // New non-ref method to replace TryGetInt32Property
    private static int GetInt32Property(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement valueElement))
        {
            if (valueElement.ValueKind == JsonValueKind.Number)
            {
                return valueElement.GetInt32();
            }
            else if (valueElement.ValueKind == JsonValueKind.String &&
                     int.TryParse(valueElement.GetString(), out int value))
            {
                return value;
            }
        }
        return defaultValue;
    }

    // New non-ref method to replace ParseStringArray
    private static List<string> GetStringArray(JsonElement element, string propertyName, List<string> defaults)
    {
        List<string> result = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement))
        {
            if (arrayElement.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in arrayElement.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        string value = item.GetString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            result.Add(value);
                        }
                    }
                }
            }
            else if (arrayElement.ValueKind == JsonValueKind.String)
            {
                // Handle the case where a single string is provided instead of an array
                string value = arrayElement.GetString();
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }
        }

        // If we found no values, return the defaults
        return result.Count > 0 ? result : defaults;
    }

    private static EncounterTemplateModel CreateDefaultEncounterTemplate()
    {
        return new EncounterTemplateModel
        {
            Name = "Encounter Template",
            Duration = 4,
            MaxPressure = 10,
            PartialThreshold = 8,
            StandardThreshold = 12,
            ExceptionalThreshold = 16,
            Hostility = "Neutral",
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