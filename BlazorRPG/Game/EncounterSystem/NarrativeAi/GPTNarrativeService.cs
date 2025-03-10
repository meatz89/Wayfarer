using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BlazorRPG.Game.EncounterManager.NarrativeAi
{
    /// <summary>
    /// Implementation of the Narrative AI Service using OpenAI's GPT
    /// </summary>
    public class GPTNarrativeService : INarrativeAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _modelName;

        public GPTNarrativeService(string apiKey, string modelName = "gpt-4")
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _modelName = modelName;
        }

        public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
        {
            // Construct prompt for the AI
            string prompt = $@"
You are the narrator for a medieval life simulation game called Wayfarer. 
The player is at the {location} and has just {incitingAction}.

Create an introduction scene that sets up this encounter. Use vivid descriptive language.
Consider the following game state in your description:
- Active Tags: {string.Join(", ", state.ActiveTagNames)}
- Approach Tags: {string.Join(", ", state.ApproachTags.Select(t => $"{t.Key}: {t.Value}"))}
- Focus Tags: {string.Join(", ", state.FocusTags.Select(t => $"{t.Key}: {t.Value}"))}

Your introduction should be 2-3 paragraphs long and should establish the scene, introducing 
any relevant NPCs or environmental elements the player will interact with.
";

            return await CallGPTAsync(prompt);
        }

        public async Task<string> GenerateReactionAndSceneAsync(
            NarrativeContext context,
            IChoice chosenOption,
            string choiceDescription,
            ChoiceOutcome outcome,
            EncounterStatus newState)
        {
            // Construct prompt using the full narrative context
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
For social encounters, use direct speech dialogue.
For intellectual encounters, use internal monologue.
For physical encounters, use action descriptions.
";

            return await CallGPTAsync(prompt);
        }

        public async Task<Dictionary<IChoice, string>> GenerateChoiceDescriptionsAsync(
            NarrativeContext context,
            List<IChoice> choices,
            List<ChoiceProjection> projections,
            EncounterStatus state)
        {
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
If this is a social encounter, use direct speech dialogue.
If this is an intellectual encounter, use internal monologue.
If this is a physical encounter, use action descriptions.

Format your response as:
Choice 1: [Your narrative description]
Choice 2: [Your narrative description]
...and so on.

Each description should be 1-2 sentences that fit naturally with the established scene and context.
";

            string response = await CallGPTAsync(prompt);

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
            foreach (IChoice? choice in choices.Where(c => !result.ContainsKey(c)))
            {
                result[choice] = choice.Description;
            }

            return result;
        }

        private async Task<string> CallGPTAsync(string prompt)
        {
            var requestBody = new
            {
                model = _modelName,
                messages = new[]
                {
                    new { role = "system", content = "You are a narrative AI for a medieval life simulation game called Wayfarer." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            StringContent content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            HttpResponseMessage response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                // Parse the response to extract the generated text
                using (JsonDocument document = JsonDocument.Parse(jsonResponse))
                {
                    JsonElement root = document.RootElement;
                    JsonElement choices = root.GetProperty("choices");
                    JsonElement firstChoice = choices[0];
                    JsonElement message = firstChoice.GetProperty("message");
                    string? content2 = message.GetProperty("content").GetString();
                    return content2;
                }
            }

            throw new Exception($"Failed to get response from GPT: {response.StatusCode}");
        }
    }
}