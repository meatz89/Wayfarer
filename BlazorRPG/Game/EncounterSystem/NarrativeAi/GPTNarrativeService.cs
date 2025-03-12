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
        string conversationId = $"{location}_{DateTime.Now.Ticks}";

        // Enhanced system message with tag explanation
        string systemMessage = @"You are the narrative engine for Wayfarer, a medieval life simulation RPG. Your writing style should be:
- Vivid and sensory, emphasizing sights, sounds, smells, and atmosphere
- Character-focused, with NPCs that feel real and distinctive
- Historically grounded, avoiding fantasy tropes
- Socially realistic, reflecting medieval social hierarchies and tensions

UNDERSTANDING TAGS:
- Approach Tags represent HOW the player acts (Dominance, Rapport, Analysis, Precision, Concealment)
- Focus Tags represent WHAT the player focuses on (Relationship, Information, Physical, Environment, Resource)
- Active Tags represent special conditions in the current scene

Higher tag values (3-5) should strongly influence your description, while lower values (1-2) should subtly color it.";

        // Format tags with explanations
        string formattedTags = FormatTagsWithExplanations(state);

        // Construct improved prompt
        string prompt = $@"
Create an introduction scene for the player character (PC) who has just {incitingAction} at the {location}.

{formattedTags}

Your introduction should:
1. Establish a vivid sense of place with sensory details
2. Hint at opportunities and challenges matching the active tags
3. Be 2-3 paragraphs in length

Make higher-value tags more prominent in your description. A tag value of 4-5 should strongly influence the scene, while 1-2 should be more subtle.";

        // Initialize new conversation
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

        // Enhanced system message with tag interpretation guidelines
        string systemMessage = @"You are the narrative engine for Wayfarer, a medieval life simulation. Your role is to create coherent, immersive narrative that responds to player choices. Your writing style:

- Consequences-focused: Every choice affects the world and NPCs
- Historically authentic: Reflect medieval social dynamics and realities
- Character-driven: NPCs, if present, have consistent personalities, desires, and reactions
- Sensory and immersive: Rich descriptions create a tangible world

INTERPRETING TAGS:
- Approach Tags (HOW): 
  * Dominance: Force, authority, intimidation
  * Rapport: Social connection, charm, persuasion
  * Analysis: Intelligence, observation, problem-solving
  * Precision: Careful execution, finesse, accuracy
  * Concealment: Stealth, hiding, subterfuge

- Focus Tags (WHAT):
  * Relationship: Social dynamics, connections with others
  * Information: Knowledge, facts, understanding
  * Physical: Bodies, movement, physical objects
  * Environment: Surroundings, spaces, terrain
  * Resource: Items, money, supplies, valuables

- Active Tags: Special conditions influencing the scene and available options

Changes in tag values reflect the player's evolving approach. Incorporate these changes naturally in your narrative.";

        // Format tags with explanations
        string formattedTags = FormatTagsWithExplanations(newState);

        // Create a concise narrative summary instead of using raw context
        string narrativeSummary = CreateNarrativeSummary(context);

        // Construct improved prompt
        string prompt = $@"
### Narrative Summary
{narrativeSummary}

### Player's Latest Choice
The player chose: {chosenOption.Name} ({chosenOption.Approach} approach focused on {chosenOption.Focus})
Specific action: {choiceDescription}

### Outcome and Tag Changes
{outcome.Description}
- Momentum Gained: {outcome.MomentumGain} (Progress toward success)
- Pressure Built: {outcome.PressureGain} (Complications or tension)

{formattedTags}

### Your Task
1. Write how the environment and NPCs react to the player's specific choice ({chosenOption.Name})
2. Show how the changes in tags manifest in the scene (especially: {GetSignificantTagChanges(newState)})
3. Set up the next stage of the encounter
4. Create 2-3 paragraphs that maintain narrative continuity

Make your response feel like a natural continuation of the ongoing story.";

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
            // Update existing conversation
            history = _conversationHistories[conversationId];

            // Replace system message with updated version
            if (history.Count > 0 && history[0].Role == "system")
            {
                history[0] = new ConversationEntry { Role = "system", Content = systemMessage };
            }
            else
            {
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

        // Enhanced system message with clearer guidance
        string systemMessage = @"You are the narrative engine for Wayfarer, a medieval life simulation. Your task is to translate game choices into vivid, specific actions the player character (PC) can visualize.

For each choice, create narrative text that:
1. Shows exactly what the player character would DO or SAY
2. Matches the approach (HOW) and focus (WHAT) of the choice
3. Feels organic within the current scene
4. Is written in second person perspective

APPROACH TYPES (HOW):
- Force: Authoritative, direct, sometimes physically imposing
- Charm: Persuasive, friendly, socially adept
- Wit: Analytical, observant, intellectually focused
- Finesse: Careful, precise, skillful
- Stealth: Hidden, subtle, secretive

FOCUS TYPES (WHAT):
- Relationship: Interactions with others, social dynamics
- Information: Knowledge, learning, understanding
- Physical: Bodies, items, physical manipulation
- Environment: Surroundings, terrain, physical space
- Resource: Items, money, supplies

WRITING STYLE:
- For social encounters: Use direct speech with quotation marks
- For intellectual encounters: Use internal monologue
- For physical encounters: Use action descriptions
- Keep descriptions concise (1-2 sentences)
- Make each choice distinct and true to its approach/focus";

        // Create a concise narrative summary instead of using raw context
        string narrativeSummary = CreateNarrativeSummary(context);

        // Format tags with explanations
        string formattedTags = FormatTagsWithExplanations(state);

        // Format choices with better explanations
        StringBuilder choicesPrompt = new StringBuilder();
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            choicesPrompt.AppendLine($"Choice {i + 1}: {choice.Name}");
            choicesPrompt.AppendLine($"- Core Concept: {choice.Description}");
            choicesPrompt.AppendLine($"- Approach: {choice.Approach} (HOW they act)");
            choicesPrompt.AppendLine($"- Focus: {choice.Focus} (WHAT they focus on)");

            if (choice.EffectType == EffectTypes.Momentum)
                choicesPrompt.AppendLine($"- Effect: Makes progress (+{projection.MomentumGained} Momentum)");
            else
                choicesPrompt.AppendLine($"- Effect: Increases complications (+{projection.PressureBuilt} Pressure)");

            if (projection.ApproachTagChanges.Count > 0 || projection.FocusTagChanges.Count > 0)
            {
                choicesPrompt.AppendLine("- Changes to player's approach:");
                foreach (KeyValuePair<ApproachTags, int> change in projection.ApproachTagChanges)
                    choicesPrompt.AppendLine($"  * {ExplainApproachTag(change.Key)}: {(change.Value > 0 ? "+" : "")}{change.Value}");
                foreach (KeyValuePair<FocusTags, int> change in projection.FocusTagChanges)
                    choicesPrompt.AppendLine($"  * {ExplainFocusTag(change.Key)}: {(change.Value > 0 ? "+" : "")}{change.Value}");
            }

            if (projection.NewlyActivatedTags.Count > 0)
            {
                choicesPrompt.AppendLine("- New conditions that will trigger:");
                foreach (string tag in projection.NewlyActivatedTags)
                {
                    string tagExplanation = ExplainTag(tag);
                    choicesPrompt.AppendLine($"  * {tag}: {tagExplanation}");
                }
            }

            choicesPrompt.AppendLine();
        }

        // Construct improved prompt
        string prompt = $@"
### Current Scene
{narrativeSummary}

### Current State
- Turn: {state.CurrentTurn}/{state.MaxTurns}
- Momentum: {state.Momentum} (Progress toward success)
- Pressure: {state.Pressure} (Level of complication/tension)

{formattedTags}

### Available Choices
{choicesPrompt}

### Your Task
For each choice, write a vivid description of what the player character would specifically do or say if they selected this option. 

Format your response exactly as:
Choice 1: [Your vivid action description]
Choice 2: [Your vivid action description]
...and so on.

Make sure each description:
- Matches the approach and focus of the choice
- Fits naturally in the current scene
- Shows the specific action, not just the intention
- Is written from first person perspective";

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
            // Update existing conversation
            history = _conversationHistories[conversationId];

            // Replace system message with updated version
            if (history.Count > 0 && history[0].Role == "system")
            {
                history[0] = new ConversationEntry { Role = "system", Content = systemMessage };
            }
            else
            {
                history.Insert(0, new ConversationEntry { Role = "system", Content = systemMessage });
            }

            // Add new user message
            history.Add(new ConversationEntry { Role = "user", Content = prompt });
        }

        // Call GPT and log
        string response = await CallGPTWithLoggingAsync(conversationId);

        // Add assistant message
        history.Add(new ConversationEntry { Role = "assistant", Content = response });

        // Parse the response into a dictionary (using existing parsing logic)
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

    // Helper method to format tags with explanations
    private string FormatTagsWithExplanations(EncounterStatus state)
    {
        StringBuilder tagInfo = new StringBuilder("### Current Game State\n");

        // Format Approach Tags with explanations
        tagInfo.AppendLine("APPROACH TAGS (how the player acts):");
        foreach (var tag in state.ApproachTags.OrderByDescending(t => t.Value))
        {
            string explanation = ExplainApproachTag((ApproachTags)Enum.Parse(typeof(ApproachTags), tag.Key.ToString()));
            string significance = tag.Value >= 4 ? " (MAJOR)" : tag.Value >= 2 ? " (significant)" : " (minor)";
            tagInfo.AppendLine($"- {tag.Key}: {tag.Value}{significance} - {explanation}");
        }

        // Format Focus Tags with explanations
        tagInfo.AppendLine("\nFOCUS TAGS (what the player focuses on):");
        foreach (var tag in state.FocusTags.OrderByDescending(t => t.Value))
        {
            string explanation = ExplainFocusTag((FocusTags)Enum.Parse(typeof(FocusTags), tag.Key.ToString()));
            string significance = tag.Value >= 4 ? " (MAJOR)" : tag.Value >= 2 ? " (significant)" : " (minor)";
            tagInfo.AppendLine($"- {tag.Key}: {tag.Value}{significance} - {explanation}");
        }

        // Format Active Tags with explanations
        if (state.ActiveTagNames.Any())
        {
            tagInfo.AppendLine("\nACTIVE TAGS (special conditions):");
            foreach (var tag in state.ActiveTagNames)
            {
                string explanation = ExplainTag(tag);
                tagInfo.AppendLine($"- {tag}: {explanation}");
            }
        }

        return tagInfo.ToString();
    }

    // Create a narrative summary from context
    private string CreateNarrativeSummary(NarrativeContext context)
    {
        // Extract the most recent narrative events (up to 3)
        var recentEvents = context.Events
            .OrderByDescending(e => e.TurnNumber)
            .Take(3)
            .OrderBy(e => e.TurnNumber)
            .ToList();

        if (!recentEvents.Any())
            return "The encounter has just begun.";

        StringBuilder summary = new StringBuilder();

        // Get location and initial action
        summary.AppendLine($"Location: {context.LocationName}");
        summary.AppendLine($"Initial action: {context.IncitingAction}");
        summary.AppendLine();

        // Add recent narrative events
        foreach (var evt in recentEvents)
        {
            // If it's the first event, include its description
            if (evt.TurnNumber == 0)
            {
                summary.AppendLine("Scene began:");
                summary.AppendLine(evt.SceneDescription.Trim());
                summary.AppendLine();
            }
            else if (!string.IsNullOrEmpty(evt.ChosenOption.Description))
            {
                summary.AppendLine($"Turn {evt.TurnNumber}: Player chose \"{evt.ChosenOption?.Name}\"");
                summary.AppendLine($"Action: {evt.ChosenOption.Description}");
                summary.AppendLine($"Result: {evt.SceneDescription.Trim()}");
                summary.AppendLine();
            }
        }

        return summary.ToString();
    }

    // Helper method to get significant tag changes
    private string GetSignificantTagChanges(EncounterStatus state)
    {
        // In a real implementation, you would compare with previous state
        // For now, return tags with high values as a placeholder
        var highApproachTags = state.ApproachTags
            .Where(t => t.Value >= 3)
            .OrderByDescending(t => t.Value)
            .Take(2)
            .Select(t => $"{t.Key} ({t.Value})");

        var highFocusTags = state.FocusTags
            .Where(t => t.Value >= 3)
            .OrderByDescending(t => t.Value)
            .Take(2)
            .Select(t => $"{t.Key} ({t.Value})");

        return string.Join(", ", highApproachTags.Concat(highFocusTags));
    }

    // Helper methods to explain tags
    private string ExplainApproachTag(ApproachTags tag)
    {
        switch (tag)
        {
            case ApproachTags.Dominance:
                return "Using authority, force, or intimidation";
            case ApproachTags.Rapport:
                return "Building social connections through charm and empathy";
            case ApproachTags.Analysis:
                return "Using intelligence and careful observation";
            case ApproachTags.Precision:
                return "Acting with careful skill and finesse";
            case ApproachTags.Concealment:
                return "Using stealth or hiding intentions";
            default:
                return tag.ToString();
        }
    }

    private string ExplainFocusTag(FocusTags tag)
    {
        switch (tag)
        {
            case FocusTags.Relationship:
                return "Focusing on social connections and dynamics";
            case FocusTags.Information:
                return "Seeking knowledge and understanding";
            case FocusTags.Physical:
                return "Engaging with bodies, movement, or physical objects";
            case FocusTags.Environment:
                return "Interacting with surroundings and spaces";
            case FocusTags.Resource:
                return "Managing items, money, or supplies";
            default:
                return tag.ToString();
        }
    }

    private string ExplainTag(string tagName)
    {
        // Map common tag names to explanations
        if (tagName.Contains("Respect"))
            return "The player has earned respect through social skills";
        else if (tagName.Contains("Eye") || tagName.Contains("Haggler"))
            return "The player shows skill in negotiations";
        else if (tagName.Contains("Wisdom"))
            return "The player demonstrates intellectual insight";
        else if (tagName.Contains("Network"))
            return "The player has established social connections";
        else if (tagName.Contains("Guard"))
            return "Security presence is heightened";
        else if (tagName.Contains("Suspicion"))
            return "Others are wary of the player's intentions";
        else if (tagName.Contains("Surrounded"))
            return "The player has limited movement options";
        else if (tagName.Contains("Drawn Weapons"))
            return "The situation has escalated to potential violence";
        else if (tagName.Contains("Marketplace"))
            return "Open public setting with many witnesses";
        else if (tagName.Contains("Room"))
            return "Indoor setting with limited privacy";

        // Generate a reasonable explanation for unknown tags
        if (tagName.Contains("Tension"))
            return "The situation is becoming more volatile";
        else if (tagName.Contains("Ready"))
            return "Prepared for conflict or challenge";
        else if (tagName.Contains("Favor"))
            return "Someone has a positive disposition toward the player";
        else if (tagName.Contains("Patron"))
            return "The player has established a privileged position";

        return "Special condition affecting available options";
    }

    private async Task<string> CallGPTWithLoggingAsync(string conversationId)
    {
        // Get a unique log file path for this request
        string logFilePath = _logManager.GetNextLogFilePath();

        // Get conversation history
        var history = _conversationHistories[conversationId];

        // Prepare API messages - including full history
        var messages = history.Select(m => new { role = m.Role, content = m.Content }).ToArray();

        var requestBody = new
        {
            model = _modelName,
            messages = messages,
            temperature = 0.7
        };

        // Log conversation history to file
        using (StreamWriter writer = File.CreateText(logFilePath))
        {
            var options = new JsonSerializerOptions { WriteIndented = true };

            // Write conversation history
            writer.WriteLine("// Full Conversation History");
            writer.WriteLine(JsonSerializer.Serialize(history, options));

            // Write API request
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