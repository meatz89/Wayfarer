using BlazorRPG.Game.EncounterManager.NarrativeAi;
using BlazorRPG.Game.EncounterManager;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

public class GPTNarrativeService : INarrativeAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelName;
    private readonly string _apiKey;
    private readonly ILogger<GPTNarrativeService> _logger;
    private readonly NarrativeLogManager _logManager;

    // Dictionary to track conversation history per encounter context
    private readonly Dictionary<string, List<ConversationEntry>> _conversationHistories = new();

    public GPTNarrativeService(IConfiguration configuration, ILogger<GPTNarrativeService> logger)
    {
        _httpClient = new HttpClient();
        _modelName = "gpt-4";
        _apiKey = configuration.GetValue<string>("OpenAiApiKey");
        _logger = logger;
        _logManager = new NarrativeLogManager();
    }

    public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
    {
        // Create conversation ID from location
        string conversationId = $"{location}_{DateTime.Now.Ticks}";

        // Create a custom system message for introduction generation
        string systemMessage = "You are the narrator for a medieval life simulation game called Wayfarer. Your task is to create immersive, atmospheric scene descriptions that set the stage for player interactions. Focus on sensory details, NPCs, and environmental elements that will influence the player's choices.";

        // Construct prompt
        string prompt = $@"
            The player is at the {location} and has just {incitingAction}.

            Create an introduction scene that sets up this encounter. Use vivid descriptive language.
            Consider the following game state in your description:
            - Active Tags: {string.Join(", ", state.ActiveTagNames)}
            - Approach Tags: {string.Join(", ", state.ApproachTags.Select(t => $"{t.Key}: {t.Value}"))}
            - Focus Tags: {string.Join(", ", state.FocusTags.Select(t => $"{t.Key}: {t.Value}"))}

            Your introduction should be 2-3 paragraphs long and should establish the scene, introducing 
            any relevant NPCs or environmental elements the player will interact with.
        ";

        // This is a new conversation, initialize history with just system and user messages
        List<ConversationEntry> history = new()
        {
            new ConversationEntry { Role = "system", Content = systemMessage },
            new ConversationEntry { Role = "user", Content = prompt }
        };

        _conversationHistories[conversationId] = history;

        // Call GPT and log
        string response = await CallGPTWithLoggingAsync(conversationId);

        // Add assistant message to history
        history.Add(new ConversationEntry { Role = "assistant", Content = response });

        return response;
    }

    public async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        string choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string conversationId = context.LocationName;

        // Create a custom system message for generating reactions
        string systemMessage = "You are the narrator for a medieval life simulation game called Wayfarer. Your task is to describe how the environment and NPCs react to player choices, maintaining narrative continuity and setting up the next scene. Focus on consequences, changing dynamics, and the evolving situation based on the player's approach.";

        // Construct prompt
        string prompt = $@"
            ### Full Encounter Context
            {context.ToPrompt()}

            ### Player's Latest Choice
            The player has chosen: {chosenOption.Name}
            Player action: {choiceDescription}

            ### Outcome
            {outcome.Description}
            - Momentum Gained: {outcome.MomentumGain}
            - Pressure Built: {outcome.PressureGain}

            ### Current Game State
            - Active Tags: {string.Join(", ", newState.ActiveTagNames)}
            - Approach Tags: {string.Join(", ", newState.ApproachTags.Select(t => $"{t.Key}: {t.Value}"))}
            - Focus Tags: {string.Join(", ", newState.FocusTags.Select(t => $"{t.Key}: {t.Value}"))}

            ### Your Task
            Please generate:
            1. A narrative description of how the environment and NPCs react to the player's choice
            2. A description of the new scene that sets up the next set of choices the player will face

            Your response should be 2-3 paragraphs that maintain narrative continuity with previous events.
        ";

        // Check if this is a new conversation or continuation
        List<ConversationEntry> history;
        if (!_conversationHistories.ContainsKey(conversationId))
        {
            // New conversation - only system and current user message
            history = new List<ConversationEntry>
            {
                new ConversationEntry { Role = "system", Content = systemMessage },
                new ConversationEntry { Role = "user", Content = prompt }
            };
            _conversationHistories[conversationId] = history;
        }
        else
        {
            // Existing conversation - update system message and add new user message
            history = _conversationHistories[conversationId];

            // Replace the system message with the task-specific one
            if (history.Count > 0 && history[0].Role == "system")
            {
                history[0] = new ConversationEntry { Role = "system", Content = systemMessage };
            }
            else
            {
                // Insert system message at the beginning if not present
                history.Insert(0, new ConversationEntry { Role = "system", Content = systemMessage });
            }

            // Add new user message
            history.Add(new ConversationEntry { Role = "user", Content = prompt });
        }

        // Call GPT and log
        string response = await CallGPTWithLoggingAsync(conversationId);

        // Add assistant message
        history.Add(new ConversationEntry { Role = "assistant", Content = response });

        return response;
    }

    public async Task<Dictionary<IChoice, string>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = context.LocationName;

        // Create a custom system message for generating choice descriptions
        string systemMessage = "You are the narrator for a medieval life simulation game called Wayfarer. Your task is to transform abstract game choices into vivid, context-specific actions the player can imagine performing. Focus on what each action would look like, sound like, and feel like in the current scene, maintaining consistency with the established narrative.";

        // Construct detailed choice descriptions
        StringBuilder choicesPrompt = new StringBuilder();
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            choicesPrompt.AppendLine($"Choice {i + 1}: {choice.Name}");
            choicesPrompt.AppendLine($"- Description: {choice.Description}");
            choicesPrompt.AppendLine($"- Approach: {choice.Approach}, Focus: {choice.Focus}, Type: {choice.EffectType}");
            choicesPrompt.AppendLine($"- Effect: {(choice.EffectType == EffectTypes.Momentum ? $"+{projection.MomentumGained} Momentum" : $"+{projection.PressureBuilt} Pressure")}");

            if (projection.ApproachTagChanges.Count > 0 || projection.FocusTagChanges.Count > 0)
            {
                choicesPrompt.AppendLine("- Tag Changes:");
                foreach (KeyValuePair<ApproachTags, int> change in projection.ApproachTagChanges)
                    choicesPrompt.AppendLine($"  * {change.Key}: {(change.Value > 0 ? "+" : "")}{change.Value}");
                foreach (KeyValuePair<FocusTags, int> change in projection.FocusTagChanges)
                    choicesPrompt.AppendLine($"  * {change.Key}: {(change.Value > 0 ? "+" : "")}{change.Value}");
            }

            if (projection.NewlyActivatedTags.Count > 0)
            {
                choicesPrompt.AppendLine("- New Tags:");
                foreach (string tag in projection.NewlyActivatedTags)
                    choicesPrompt.AppendLine($"  * {tag}");
            }

            choicesPrompt.AppendLine();
        }

        // Construct the full prompt
        string prompt = $@"
            ### Full Encounter Context
            {context.ToPrompt()}

            ### Current Game State
            - Current Turn: {state.CurrentTurn}/{state.MaxTurns}
            - Momentum: {state.Momentum}, Pressure: {state.Pressure}
            - Active Tags: {string.Join(", ", state.ActiveTagNames)}

            ### Available Choices
            {choicesPrompt}

            ### Your Task
            For each choice, create a vivid, context-appropriate description of what this action would look like.
            Maintain consistency with the established narrative and previous events.
            
            Format your response as:
            Choice 1: [Your narrative description]
            Choice 2: [Your narrative description]
            ...and so on.
        ";

        // Check if this is a new conversation or continuation
        List<ConversationEntry> history;
        if (!_conversationHistories.ContainsKey(conversationId))
        {
            // New conversation - only system and current user message
            history = new List<ConversationEntry>
            {
                new ConversationEntry { Role = "system", Content = systemMessage },
                new ConversationEntry { Role = "user", Content = prompt }
            };
            _conversationHistories[conversationId] = history;
        }
        else
        {
            // Existing conversation - update system message and add new user message
            history = _conversationHistories[conversationId];

            // Replace the system message with the task-specific one
            if (history.Count > 0 && history[0].Role == "system")
            {
                history[0] = new ConversationEntry { Role = "system", Content = systemMessage };
            }
            else
            {
                // Insert system message at the beginning if not present
                history.Insert(0, new ConversationEntry { Role = "system", Content = systemMessage });
            }

            // Add new user message
            history.Add(new ConversationEntry { Role = "user", Content = prompt });
        }

        // Call GPT and log
        string response = await CallGPTWithLoggingAsync(conversationId);

        // Add assistant message
        history.Add(new ConversationEntry { Role = "assistant", Content = response });

        // Parse the response into a dictionary
        Dictionary<IChoice, string> result = new Dictionary<IChoice, string>();
        string[] lines = response.Split('\n');

        int currentChoice = -1;
        StringBuilder currentDescription = new StringBuilder();

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Check if this is a new choice marker
            if (trimmedLine.StartsWith("Choice ") && trimmedLine.Contains(":"))
            {
                // Save previous choice if exists
                if (currentChoice >= 0 && currentChoice < choices.Count && currentDescription.Length > 0)
                {
                    result[choices[currentChoice]] = currentDescription.ToString().Trim();
                    currentDescription.Clear();
                }

                // Parse new choice number
                string[] parts = trimmedLine.Split(':', 2);
                string choiceNumStr = parts[0].Substring("Choice ".Length).Trim();

                if (int.TryParse(choiceNumStr, out int choiceNum) && choiceNum > 0 && choiceNum <= choices.Count)
                {
                    currentChoice = choiceNum - 1;
                    if (parts.Length > 1)
                    {
                        currentDescription.AppendLine(parts[1].Trim());
                    }
                }
            }
            else if (currentChoice >= 0 && currentChoice < choices.Count && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                // Add to current description
                currentDescription.AppendLine(trimmedLine);
            }
        }

        // Add the last choice if not added
        if (currentChoice >= 0 && currentChoice < choices.Count && currentDescription.Length > 0)
        {
            result[choices[currentChoice]] = currentDescription.ToString().Trim();
        }

        // Fill in any missing choices with defaults
        foreach (IChoice choice in choices.Where(c => !result.ContainsKey(c)))
        {
            result[choice] = choice.Description;
        }

        return result;
    }

    private async Task<string> CallGPTWithLoggingAsync(string conversationId)
    {
        // Get a unique log file path for this request
        string logFilePath = _logManager.GetNextLogFilePath();

        // Get conversation history
        var history = _conversationHistories[conversationId];

        // Prepare API messages
        var messages = new List<object>();

        // Always include the system message
        var systemMessage = history.FirstOrDefault(m => m.Role == "system");
        if (systemMessage != null)
        {
            messages.Add(new { role = "system", content = systemMessage.Content });
        }

        // For conversation continuity, include previous exchanges (but not system message again)
        if (history.Count > 2) // More than just system + current user message
        {
            // Get all previous exchanges except system message and the latest user message
            var previousExchanges = history
                .Where(m => m.Role != "system" && m != history.Last())
                .ToList();

            foreach (var entry in previousExchanges)
            {
                messages.Add(new { role = entry.Role, content = entry.Content });
            }
        }

        // Add current user message (last in history)
        var currentUserMessage = history.LastOrDefault(m => m.Role == "user");
        if (currentUserMessage != null)
        {
            messages.Add(new { role = "user", content = currentUserMessage.Content });
        }

        var requestBody = new
        {
            model = _modelName,
            messages = messages.ToArray(),
            temperature = 0.7
        };

        // Log conversation history to file
        using (StreamWriter writer = File.CreateText(logFilePath))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            // Write conversation history
            writer.WriteLine("// Full Conversation History");
            writer.WriteLine(JsonSerializer.Serialize(history, options));

            // Write API request (what's actually being sent)
            writer.WriteLine("\n// API Request");
            writer.WriteLine(JsonSerializer.Serialize(requestBody, options));
        }

        // Make API call
        StringContent content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        HttpResponseMessage response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (response.IsSuccessStatusCode)
        {
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Parse response
            using (JsonDocument document = JsonDocument.Parse(jsonResponse))
            {
                JsonElement root = document.RootElement;
                JsonElement choices = root.GetProperty("choices");
                JsonElement firstChoice = choices[0];
                JsonElement message = firstChoice.GetProperty("message");
                string? generatedContent = message.GetProperty("content").GetString();

                // Append response to log file
                using (StreamWriter writer = File.AppendText(logFilePath))
                {
                    writer.WriteLine("\n// API Response");
                    writer.WriteLine(jsonResponse);
                    writer.WriteLine("\n// Final Content");
                    writer.WriteLine(generatedContent);
                }

                return generatedContent;
            }
        }

        // Log error
        using (StreamWriter writer = File.AppendText(logFilePath))
        {
            writer.WriteLine("\n// API Error");
            writer.WriteLine($"Status Code: {response.StatusCode}");
            writer.WriteLine(await response.Content.ReadAsStringAsync());
        }

        throw new Exception($"Failed to get response from GPT: {response.StatusCode}");
    }
}