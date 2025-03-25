using System.Text.Json;

public class ClaudeNarrativeService : BaseNarrativeAIService
{
    private readonly NarrativeContextManager _contextManager;

    public ClaudeNarrativeService(IConfiguration configuration, ILogger<EncounterSystem> logger)
        : base(new ClaudeProvider(configuration, logger), configuration, logger)
    {
        _contextManager = new NarrativeContextManager();
    }

    public override async Task<string> GenerateIntroductionAsync(NarrativeContext context, EncounterStatusModel state, string memoryContent)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Consistent ID
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildIntroductionPrompt(context, state, memoryContent);

        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, response, MessageType.Introduction);

        return response;
    }

    public override async Task<Dictionary<IChoice, ChoiceNarrative>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatusModel state)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildChoicesPrompt(
            context, choices, projections, state);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.ChoiceGeneration, null);
        }

        string jsonResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, jsonResponse, MessageType.ChoiceGeneration);
        return NarrativeJsonParser.ParseChoiceResponse(jsonResponse, choices);
    }

    public override async Task<string> GenerateEncounterNarrative(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildReactionPrompt(
            context, chosenOption, choiceNarrative, outcome, newState);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.PlayerChoice, choiceNarrative);
        }

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public override async Task<string> GenerateEndingAsync(
        NarrativeContext context,
        IChoice chosenOption,
        ChoiceNarrative choiceNarrative,
        ChoiceOutcome outcome,
        EncounterStatusModel newState)
    {
        string conversationId = $"{context.LocationName}_encounter";
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildEncounterEndPrompt(
            context, newState, outcome.Outcome, chosenOption, choiceNarrative);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.PlayerChoice, choiceNarrative);
        }

        string narrativeResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        _contextManager.AddAssistantMessage(conversationId, narrativeResponse, MessageType.Narrative);
        return narrativeResponse;
    }

    public override async Task<string> GenerateMemoryFileAsync(
        NarrativeContext context,
        ChoiceOutcome outcome,
        EncounterStatusModel newState,
        string oldMemory)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Same ID as introduction
        string systemMessage = _promptManager.GetSystemMessage();

        string prompt = _promptManager.BuildMemoryPrompt(
            context, outcome, newState, oldMemory);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.MemoryUpdate, null);
        }
        string memoryContentResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        return memoryContentResponse;
    }

    public override async Task<string> GenerateStateChangesAsync(
        NarrativeContext context,
        ChoiceOutcome outcome,
        EncounterStatusModel newState)
    {
        string conversationId = $"{context.LocationName}_encounter"; // Same ID as introduction
        string systemMessage = _promptManager.GetSystemMessage();

        string prompt = _promptManager.BuildStateChangesPrompt(
            context, outcome, newState);

        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt, MessageType.StateChanges, null);
        }
        string stateChangesResponse = await _aiClient.GetCompletionAsync(
            _contextManager.GetOptimizedConversationHistory(conversationId));

        return stateChangesResponse;
    }

    public override Task<DiscoveredEntities> ExtractWorldDiscoveries(string encounterNarrative, WorldContext worldContext)
    {
        throw new NotImplementedException();
    }

    public override Task<EntityDetails> DevelopEntityDetails(string entityType, string entityId, EntityContext entityContext)
    {
        throw new NotImplementedException();
    }

    public override Task<StateChangeRecommendations> GenerateStateChanges(string encounterOutcome, EncounterContext context)
    {
        throw new NotImplementedException();
    }

    public override async Task<LocationDetails> GenerateLocationDetailsAsync(LocationGenerationContext context)
    {
        string conversationId = $"location_generation_{context.LocationType}"; // Unique conversation ID
        string systemMessage = _promptManager.GetSystemMessage();
        string prompt = _promptManager.BuildLocationGenerationPrompt(context);

        ConversationEntry entrySystem = new ConversationEntry { Role = "system", Content = systemMessage };
        ConversationEntry entryUser = new ConversationEntry { Role = "user", Content = prompt };

        List<ConversationEntry> messages = [entrySystem, entryUser];

        string jsonResponse = await _aiClient.GetCompletionAsync(
            messages);

        // Parse the JSON response into location details
        return LocationJsonParser.ParseLocationDetails(jsonResponse);
    }
}

// Add this class to parse the location JSON response
public static class LocationJsonParser
{
    public static LocationDetails ParseLocationDetails(string jsonResponse)
    {
        try
        {
            // Extract JSON from text response if needed
            string json = ExtractJsonFromText(jsonResponse);
            JsonElement jsonObj = JsonDocument.Parse(json).RootElement;

            // Create the location details
            LocationDetails details = new LocationDetails
            {
                Name = GetStringProperty(jsonObj, "name") ?? "UnknownLocation",
                Description = GetStringProperty(jsonObj, "description") ?? "",
                DetailedDescription = GetStringProperty(jsonObj, "detailedDescription") ?? "",
                History = GetStringProperty(jsonObj, "history") ?? "",
                PointsOfInterest = GetStringProperty(jsonObj, "pointsOfInterest") ?? "",
                TravelTimeMinutes = GetIntProperty(jsonObj, "travelTimeMinutes") ?? 120,
                TravelDescription = GetStringProperty(jsonObj, "travelDescription") ?? "",
                ConnectedLocationIds = ParseStringArray(jsonObj, "connectedLocationIds"),
                TimeProperties = ParseTimeProperties(jsonObj, "timeProperties"),
                Spots = ParseSpots(jsonObj, "spots"),
            };

            return details;
        }
        catch (Exception ex)
        {
            // Return a default location if parsing fails
            return CreateDefaultLocation("DefaultLocation");
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String)
            return property.GetString();
        return null;
    }

    private static int? GetIntProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.Number)
            return property.GetInt32();
        return null;
    }

    private static List<string> ParseStringArray(JsonElement root, string propertyName)
    {
        List<string> result = new List<string>();

        if (root.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                    result.Add(item.GetString() ?? "");
            }
        }

        return result;
    }

    private static Dictionary<string, List<IEnvironmentalProperty>> ParseTimeProperties(JsonElement root, string propertyName)
    {
        Dictionary<string, List<IEnvironmentalProperty>> timeProperties = new Dictionary<string, List<IEnvironmentalProperty>>();

        if (root.TryGetProperty(propertyName, out JsonElement timeObject) &&
            timeObject.ValueKind == JsonValueKind.Object)
        {
            foreach (JsonProperty timeProp in timeObject.EnumerateObject())
            {
                string timeOfDay = timeProp.Name;
                List<IEnvironmentalProperty> properties = new List<IEnvironmentalProperty>();

                if (timeProp.Value.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement prop in timeProp.Value.EnumerateArray())
                    {
                        if (prop.TryGetProperty("type", out JsonElement typeElement) &&
                            prop.TryGetProperty("value", out JsonElement valueElement))
                        {
                            string type = typeElement.GetString() ?? "";
                            string value = valueElement.GetString() ?? "";

                            IEnvironmentalProperty? property = CreateEnvironmentalProperty(type, value);
                            if (property != null)
                                properties.Add(property);
                        }
                    }
                }

                timeProperties[timeOfDay] = properties;
            }
        }

        return timeProperties;
    }

    private static List<SpotDetails> ParseSpots(JsonElement root, string propertyName)
    {
        List<SpotDetails> spots = new List<SpotDetails>();

        if (root.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement spot in arrayElement.EnumerateArray())
            {
                SpotDetails spotDetails = new SpotDetails
                {
                    Name = GetStringProperty(spot, "name") ?? "UnknownSpot",
                    Description = GetStringProperty(spot, "description") ?? "",
                    InteractionType = GetStringProperty(spot, "interactionType") ?? "",
                    InteractionDescription = GetStringProperty(spot, "interactionDescription") ?? "",
                    Position = GetStringProperty(spot, "position") ?? "",
                    ActionNames = ParseActionNames(spot)
                };

                spots.Add(spotDetails);
            }
        }

        return spots;
    }

    private static List<ActionNames> ParseActionNames(JsonElement spot)
    {
        List<ActionNames> actionNames = new List<ActionNames>();

        if (spot.TryGetProperty("actionNames", out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement action in arrayElement.EnumerateArray())
            {
                if (action.ValueKind == JsonValueKind.String)
                {
                    string actionName = action.GetString() ?? "";
                    if (Enum.TryParse<ActionNames>(actionName, out ActionNames result))
                        actionNames.Add(result);
                }
            }
        }

        return actionNames;
    }

    private static IEnvironmentalProperty? CreateEnvironmentalProperty(string type, string value)
    {
        switch (type.ToLower())
        {
            case "illumination":
                switch (value.ToLower())
                {
                    case "bright": return Illumination.Bright;
                    case "shadowy": return Illumination.Shadowy;
                    case "dark": return Illumination.Dark;
                    default: return null;
                }

            case "population":
                switch (value.ToLower())
                {
                    case "crowded": return Population.Crowded;
                    case "quiet": return Population.Quiet;
                    case "isolated": return Population.Isolated;
                    default: return null;
                }

            case "atmosphere":
                switch (value.ToLower())
                {
                    case "tense": return Atmosphere.Tense;
                    case "formal": return Atmosphere.Formal;
                    case "chaotic": return Atmosphere.Chaotic;
                    default: return null;
                }

            case "economic":
                switch (value.ToLower())
                {
                    case "wealthy": return Economic.Wealthy;
                    case "commercial": return Economic.Commercial;
                    case "humble": return Economic.Humble;
                    default: return null;
                }

            case "physical":
                switch (value.ToLower())
                {
                    case "confined": return Physical.Confined;
                    case "expansive": return Physical.Expansive;
                    case "hazardous": return Physical.Hazardous;
                    default: return null;
                }

            default:
                return null;
        }
    }

    private static LocationDetails CreateDefaultLocation(string name)
    {
        // Create a basic default location if parsing fails
        LocationDetails details = new LocationDetails
        {
            Name = name,
            Description = "A basic location",
            DetailedDescription = "This location was created as a fallback due to parsing errors.",
            TravelTimeMinutes = 60,
            TravelDescription = "A simple path leads to this location.",
            ConnectedLocationIds = new List<string> { "Village" },
            EnvironmentalProperties = new List<IEnvironmentalProperty>
        {
            Illumination.Bright,
            Population.Quiet,
            Atmosphere.Formal,
            Economic.Humble,
            Physical.Confined
        }
        };

        // Add default time properties
        details.TimeProperties = new Dictionary<string, List<IEnvironmentalProperty>>
    {
        { "Morning", new List<IEnvironmentalProperty> { Illumination.Bright, Population.Quiet } },
        { "Afternoon", new List<IEnvironmentalProperty> { Illumination.Bright, Population.Crowded } },
        { "Evening", new List<IEnvironmentalProperty> { Illumination.Shadowy, Population.Crowded } },
        { "Night", new List<IEnvironmentalProperty> { Illumination.Dark, Population.Quiet } }
    };

        // Add a default spot
        details.Spots = new List<SpotDetails>
        {
            new SpotDetails
            {
                Name = "Central Area",
                Description = "The main area of this location.",
                InteractionType = "Feature",
                InteractionDescription = "Explore the area",
                Position = "Center",
                ActionNames = new List<ActionNames> { ActionNames.RentRoom }
            }
        };

        return details;
    }

    // Helper to extract JSON from text that might contain other content
    private static string ExtractJsonFromText(string text)
    {
        // Find JSON start and end
        int startIndex = text.IndexOf('{');
        int endIndex = text.LastIndexOf('}');

        if (startIndex >= 0 && endIndex > startIndex)
        {
            return text.Substring(startIndex, endIndex - startIndex + 1);
        }

        // If no JSON is found, return an empty object
        return "{}";
    }
}