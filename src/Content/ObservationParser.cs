using System;
using System.Collections.Generic;
using System.Text.Json;

public static class ObservationParser
{
    public static ObservationsData ParseObservations(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        ObservationsData data = new ObservationsData
        {
            locations = new Dictionary<string, List<Observation>>(),
            observationTypes = new Dictionary<string, ObservationTypeData>()
        };

        // Parse locations section
        if (root.TryGetProperty("locations", out JsonElement locationsElement))
        {
            foreach (JsonProperty locationProp in locationsElement.EnumerateObject())
            {
                string locationId = locationProp.Name;
                List<Observation> observations = new List<Observation>();

                // The structure is locations > location_id > spot_id > [observations]
                // So we need to iterate through spots within each location
                if (locationProp.Value.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty spotProp in locationProp.Value.EnumerateObject())
                    {
                        string spotId = spotProp.Name;

                        if (spotProp.Value.ValueKind == JsonValueKind.Array)
                        {
                            foreach (JsonElement obsElement in spotProp.Value.EnumerateArray())
                            {
                                Observation obs = ParseObservation(obsElement);
                                if (obs != null)
                                {
                                    // Store which spot this observation is for
                                    obs.SpotId = spotId;
                                    observations.Add(obs);
                                }
                            }
                        }
                    }
                }

                data.locations[locationId] = observations;
            }
        }

        // Parse observation types section (currently not used but part of JSON structure)
        if (root.TryGetProperty("observationTypes", out JsonElement typesElement))
        {
            foreach (JsonProperty typeProp in typesElement.EnumerateObject())
            {
                string typeName = typeProp.Name;
                ObservationTypeData typeData = ParseObservationTypeData(typeProp.Value);
                if (typeData != null)
                {
                    data.observationTypes[typeName] = typeData;
                }
            }
        }

        return data;
    }

    private static Observation ParseObservation(JsonElement element)
    {
        Observation obs = new Observation
        {
            Id = GetStringProperty(element, "id", ""),
            Text = GetStringProperty(element, "text", ""),
            AttentionCost = GetIntProperty(element, "attentionCost", 0),
            Description = GetStringProperty(element, "description", ""),
            CreatesUrgency = GetBoolProperty(element, "createsUrgency", false),
            Automatic = GetBoolProperty(element, "automatic", false)
        };

        // Parse type enum from string
        string typeStr = GetStringProperty(element, "type", "Normal");
        obs.Type = MapObservationType(typeStr);

        // Parse connection state if present
        string stateStr = GetStringProperty(element, "createsState", "");
        if (!string.IsNullOrEmpty(stateStr))
        {
            obs.CreatesState = MapConnectionState(stateStr);
        }

        // Parse information type if present
        string infoStr = GetStringProperty(element, "providesInfo", "");
        if (!string.IsNullOrEmpty(infoStr))
        {
            obs.ProvidesInfo = MapObservationInfoType(infoStr);
        }

        // Parse card template reference
        obs.CardTemplate = GetStringProperty(element, "cardTemplate", "");

        // Parse relevant NPCs array
        obs.RelevantNPCs = GetStringArray(element, "relevantNPCs").ToArray();

        return obs;
    }

    private static ObservationTypeData ParseObservationTypeData(JsonElement element)
    {
        return new ObservationTypeData
        {
            Focus = GetIntProperty(element, "focus", 1),
            BaseFlow = GetIntProperty(element, "baseFlow", 0),
            CreatesOpening = GetBoolProperty(element, "createsOpening", false),
            ProvidesInformation = GetBoolProperty(element, "providesInformation", false),
            CreatesUrgency = GetBoolProperty(element, "createsUrgency", false),
            RequiresShadowTokens = GetBoolProperty(element, "requiresShadowTokens", false)
        };
    }

    private static ObservationType MapObservationType(string typeStr)
    {
        return typeStr switch
        {
            "Important" => ObservationType.Important,
            "Normal" => ObservationType.Normal,
            "Useful" => ObservationType.Useful,
            "Critical" => ObservationType.Critical,
            "Shadow" => ObservationType.Shadow,
            "NPC" => ObservationType.NPC,
            _ => ObservationType.Normal
        };
    }

    private static ConnectionState MapConnectionState(string stateStr)
    {
        return stateStr switch
        {
            "NEUTRAL" => ConnectionState.NEUTRAL,
            "GUARDED" => ConnectionState.GUARDED,
            "RECEPTIVE" => ConnectionState.RECEPTIVE,
            "CONNECTED" => ConnectionState.TRUSTING,
            "DISCONNECTED" => ConnectionState.DISCONNECTED,
            _ => ConnectionState.NEUTRAL
        };
    }

    private static ObservationInfoType MapObservationInfoType(string infoStr)
    {
        return infoStr.ToLower() switch
        {
            "transport" => ObservationInfoType.Transport,
            "timing" => ObservationInfoType.Timing,
            "secret" => ObservationInfoType.Secret,
            "location" => ObservationInfoType.Location,
            _ => ObservationInfoType.Location
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

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
        }
        return defaultValue;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property))
        {
            if (property.ValueKind == JsonValueKind.True) return true;
            if (property.ValueKind == JsonValueKind.False) return false;
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