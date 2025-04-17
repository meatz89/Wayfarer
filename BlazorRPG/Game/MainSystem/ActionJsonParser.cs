using System.Text.Json;

public static class ActionJsonParser
{
    public static ActionCreationResult Parse(string json)
    {
        json = json.Replace("```json", "");
        json = json.Replace("```", "");

        // Initialize with default values in case parsing fails
        ActionCreationResult result = new ActionCreationResult();

        try
        {
            // Parse the JSON document
            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // Extract action data if available
            if (root.TryGetProperty("action", out JsonElement actionElement))
            {
                result.Action = ParseActionTemplate(actionElement);
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

    private static ActionTemplate ParseActionTemplate(JsonElement element)
    {
        var Name = GetStringProperty(element, "name", "name");

        // Parse basic action type
        string actionTypeStr = GetStringProperty(element, "actionType", "Encounter");
        
        BasicActionTypes BasicActionType = BasicActionTypes.Physical;
        
        if (element.TryGetProperty("basicActionType", out JsonElement basicTypeElement) &&
            basicTypeElement.ValueKind == JsonValueKind.String)
        {
            string basicTypeStr = basicTypeElement.GetString() ?? "";
            BasicActionType = ParseBasicActionType(basicTypeStr);
        }
        else
        {
            // Try to infer from actionType if basicActionType isn't specified
            BasicActionType = ParseBasicActionType(actionTypeStr);
        }

        ActionTemplate model = new ActionTemplate(Name, Name, 1, 50, BasicActionType, true);

        // Extract basic text fields
        model.Description = GetStringProperty(element, "description", model.Description);
        model.Goal = GetStringProperty(element, "goal", model.Goal);
        model.Complication = GetStringProperty(element, "complication", model.Complication);
        model.LocationName = GetStringProperty(element, "locationName", model.LocationName);
        model.LocationSpotName = GetStringProperty(element, "spotName", model.LocationSpotName);

        return model;
    }

    private static BasicActionTypes ParseBasicActionType(string actionType)
    {
        // Direct enum parsing
        if (Enum.TryParse<BasicActionTypes>(actionType, true, out BasicActionTypes type))
        {
            return type;
        }

        // Handle common action type strings
        return actionType.ToLower() switch
        {
            "physical" => BasicActionTypes.Physical,
            "intellectual" => BasicActionTypes.Intellectual,
            "social" => BasicActionTypes.Social,
            _ => BasicActionTypes.Physical // Default fallback
        };
    }

    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString();
            return !string.IsNullOrEmpty(value) ? value : defaultValue;
        }
        return defaultValue;
    }


    private static EncounterTemplateModel ParseEncounterTemplate(JsonElement element)
    {
        EncounterTemplateModel template = CreateDefaultEncounterTemplate();

        // Parse name
        if (element.TryGetProperty("name", out JsonElement encounterNameElement) &&
            encounterNameElement.ValueKind == JsonValueKind.String)
        {
            template.Name = encounterNameElement.GetString() ?? template.Name;
        }

        // Parse numeric properties
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

        // Parse focus arrays
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

    private static void ValidateEncounterTemplate(EncounterTemplateModel template)
    {
        // Ensure name is not empty
        if (string.IsNullOrWhiteSpace(template.Name))
        {
            template.Name = "GeneratedEncounter";
        }

        // Ensure duration is within reasonable bounds
        if (template.Duration < 3 || template.Duration > 7)
        {
            template.Duration = Math.Clamp(template.Duration, 3, 7);
        }

        // Ensure thresholds make sense and have proper spacing
        if (template.PartialThreshold >= template.StandardThreshold)
        {
            template.StandardThreshold = template.PartialThreshold + 4;
        }

        if (template.StandardThreshold >= template.ExceptionalThreshold)
        {
            template.ExceptionalThreshold = template.StandardThreshold + 4;
        }

        // Ensure max pressure is appropriate
        if (template.MaxPressure < template.StandardThreshold)
        {
            template.MaxPressure = template.StandardThreshold * 2;
        }

        // Ensure we have enough strategic tags
        if (template.StrategicTags.Count < 4)
        {
            // Add default tags to reach at least 4
            string[] defaultProps = { "Bright", "Quiet", "Neutral", "Commercial" };
            string[] defaultNames = { "Natural Light", "Calm Setting", "Standard Atmosphere", "Trading Area" };

            for (int i = template.StrategicTags.Count; i < 4; i++)
            {
                template.StrategicTags.Add(new StrategicTagModel
                {
                    Name = defaultNames[i],
                    EnvironmentalProperty = defaultProps[i]
                });
            }
        }

        // Ensure we have at least two narrative tags
        if (template.NarrativeTags.Count < 2)
        {
            template.NarrativeTags.Add("DetailFixation");

            if (template.NarrativeTags.Count < 2)
            {
                template.NarrativeTags.Add("ColdCalculation");
            }
        }
    }

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
            Name = "GeneratedEncounter",
            Duration = 4,
            MaxPressure = 10,
            PartialThreshold = 8,
            StandardThreshold = 12,
            ExceptionalThreshold = 16,
            Hostility = "Neutral",
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
