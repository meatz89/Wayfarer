using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

/// <summary>
/// AI-powered narrative provider using Ollama for conversation generation.
/// Implements backwards construction principle through AI generation,
/// analyzing cards first and generating appropriate NPC dialogue.
/// </summary>
public class AIConversationNarrativeProvider : INarrativeProvider
{
    private readonly OllamaClient ollamaClient;
    private readonly ConversationNarrativeGenerator generator;
    private readonly PromptBuilder promptBuilder;
    private readonly IConfiguration configuration;
    
    /// <summary>
    /// Initializes the AI narrative provider with required dependencies.
    /// </summary>
    /// <param name="ollamaClient">Ollama client for AI generation</param>
    /// <param name="generator">Backwards construction algorithm generator</param>
    /// <param name="promptBuilder">Prompt template builder</param>
    /// <param name="configuration">Application configuration</param>
    public AIConversationNarrativeProvider(
        OllamaClient ollamaClient,
        ConversationNarrativeGenerator generator,
        PromptBuilder promptBuilder,
        IConfiguration configuration)
    {
        this.ollamaClient = ollamaClient;
        this.generator = generator;
        this.promptBuilder = promptBuilder;
        this.configuration = configuration;
    }
    
    /// <summary>
    /// Generates narrative content using AI and backwards construction.
    /// First analyzes cards to understand player options, then uses AI
    /// to generate contextually appropriate NPC dialogue and responses.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npcData">NPC information for context</param>
    /// <param name="activeCards">Cards available to player</param>
    /// <returns>Generated narrative output with NPC dialogue and card responses</returns>
    public NarrativeOutput GenerateNarrativeContent(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards)
    {
        // Step 1: Use backwards construction to analyze cards
        CardAnalysis analysis = generator.AnalyzeActiveCards(activeCards);
        
        // Step 2: Determine prompt type and build appropriate prompt
        string prompt = DetermineAndBuildPrompt(state, npcData, activeCards, analysis);
        
        // Step 3: Generate content using AI with 5 second timeout
        string aiResponse;
        try
        {
            Task<string> aiTask = GenerateAIResponseAsync(prompt);
            // Give AI generation up to 5 seconds (changed from 10)
            if (aiTask.Wait(5000))
            {
                aiResponse = aiTask.Result;
            }
            else
            {
                // Timeout - use fallback
                aiResponse = string.Empty;
            }
        }
        catch
        {
            // AI generation failed - use fallback
            aiResponse = string.Empty;
        }
        
        // Step 4: Parse AI response into structured output
        NarrativeOutput output = ParseAIResponse(aiResponse, activeCards, state);
        
        // Step 5: Validate and fill gaps if needed
        return ValidateAndEnhanceOutput(output, state, npcData, activeCards, analysis);
    }
    
    /// <summary>
    /// Checks if the AI provider is available for use.
    /// Tests Ollama connectivity and configuration.
    /// </summary>
    /// <returns>True if AI provider can generate content, false otherwise</returns>
    public bool IsAvailable()
    {
        try
        {
            // Use async properly with timeout
            Task<bool> healthTask = ollamaClient.CheckHealthAsync();
            // Wait max 1 second for health check
            if (healthTask.Wait(1000))
            {
                return healthTask.Result;
            }
            return false; // Timeout
        }
        catch
        {
            // Any exception means provider is not available
            return false;
        }
    }
    
    /// <summary>
    /// Gets the display name of this provider for debugging and logging.
    /// </summary>
    /// <returns>Human-readable provider name</returns>
    public string GetProviderName()
    {
        return "AI (Ollama)";
    }
    
    private async Task<string> GenerateAIResponseAsync(string prompt)
    {
        StringBuilder responseBuilder = new StringBuilder();
        
        await foreach (string token in ollamaClient.StreamCompletionAsync(prompt))
        {
            responseBuilder.Append(token);
        }
        
        return responseBuilder.ToString();
    }
    
    private NarrativeOutput ParseAIResponse(string aiResponse, CardCollection activeCards, ConversationState state)
    {
        NarrativeOutput output = new NarrativeOutput
        {
            CardNarratives = new Dictionary<string, string>()
        };
        
        // If no AI response, return empty output for fallback handling
        if (string.IsNullOrEmpty(aiResponse))
        {
            return output;
        }
        
        try
        {
            // Parse as JSON based on conversation state
            if (state.TotalTurns == 0)
            {
                // Introduction JSON format
                return ParseIntroductionJSON(aiResponse, activeCards);
            }
            else
            {
                // Try parsing as dialogue JSON first, then fall back to card generation JSON
                return ParseDialogueJSON(aiResponse, activeCards) ?? ParseCardGenerationJSON(aiResponse, activeCards);
            }
        }
        catch
        {
            // JSON parsing failed - try legacy text parsing as fallback
            return ParseLegacyTextResponse(aiResponse, activeCards);
        }
    }
    
    /// <summary>
    /// Determines which prompt template to use and builds the appropriate prompt.
    /// </summary>
    private string DetermineAndBuildPrompt(ConversationState state, NPCData npcData, CardCollection activeCards, CardAnalysis analysis)
    {
        if (state.TotalTurns == 0)
        {
            // First turn - use introduction prompt
            return promptBuilder.BuildIntroductionPrompt(state, npcData, activeCards, analysis);
        }
        else
        {
            // Subsequent turns - use dialogue prompt for LISTEN actions with conversation history
            return promptBuilder.BuildDialoguePrompt(state, npcData, activeCards, analysis, state.ConversationHistory);
        }
    }
    
    /// <summary>
    /// Parses introduction JSON response format.
    /// </summary>
    private NarrativeOutput ParseIntroductionJSON(string jsonResponse, CardCollection activeCards)
    {
        var jsonDoc = JsonDocument.Parse(jsonResponse);
        var root = jsonDoc.RootElement;
        
        var output = new NarrativeOutput
        {
            CardNarratives = new Dictionary<string, string>()
        };
        
        if (root.TryGetProperty("introduction", out JsonElement intro))
        {
            output.NPCDialogue = intro.GetString();
        }
        
        if (root.TryGetProperty("body_language", out JsonElement bodyLang))
        {
            output.NarrativeText = bodyLang.GetString();
        }
        
        if (root.TryGetProperty("emotional_tone", out JsonElement emotionalTone))
        {
            output.ProgressionHint = emotionalTone.GetString();
        }
        
        // For introduction, we still need to generate card narratives separately
        // This will be handled by the batch card generation in a separate AI call
        
        return output;
    }
    
    /// <summary>
    /// Parses dialogue JSON response format.
    /// </summary>
    private NarrativeOutput ParseDialogueJSON(string jsonResponse, CardCollection activeCards)
    {
        var jsonDoc = JsonDocument.Parse(jsonResponse);
        var root = jsonDoc.RootElement;
        
        var output = new NarrativeOutput
        {
            CardNarratives = new Dictionary<string, string>()
        };
        
        if (root.TryGetProperty("dialogue", out JsonElement dialogue))
        {
            output.NPCDialogue = dialogue.GetString();
        }
        else if (root.TryGetProperty("npc_dialogue", out JsonElement npcDialogue))
        {
            output.NPCDialogue = npcDialogue.GetString();
        }
        
        if (root.TryGetProperty("emotional_tone", out JsonElement emotionalTone))
        {
            output.NarrativeText = emotionalTone.GetString();
        }
        
        if (root.TryGetProperty("topic_progression", out JsonElement topicProgression))
        {
            output.ProgressionHint = topicProgression.GetString();
        }
        
        return output;
    }
    
    /// <summary>
    /// Parses batch card generation JSON response format.
    /// </summary>
    private NarrativeOutput ParseCardGenerationJSON(string jsonResponse, CardCollection activeCards)
    {
        var jsonDoc = JsonDocument.Parse(jsonResponse);
        var root = jsonDoc.RootElement;
        
        var output = new NarrativeOutput
        {
            CardNarratives = new Dictionary<string, string>()
        };
        
        if (root.TryGetProperty("card_narratives", out JsonElement cardNarratives))
        {
            foreach (var cardProp in cardNarratives.EnumerateObject())
            {
                string cardId = cardProp.Name;
                if (cardProp.Value.TryGetProperty("card_text", out JsonElement cardText))
                {
                    output.CardNarratives[cardId] = cardText.GetString();
                }
            }
        }
        
        if (root.TryGetProperty("narrative_coherence", out JsonElement coherence))
        {
            output.ProgressionHint = coherence.GetString();
        }
        
        return output;
    }
    
    /// <summary>
    /// Fallback parser for legacy text format responses.
    /// </summary>
    private NarrativeOutput ParseLegacyTextResponse(string aiResponse, CardCollection activeCards)
    {
        NarrativeOutput output = new NarrativeOutput
        {
            CardNarratives = new Dictionary<string, string>()
        };
        
        string[] lines = aiResponse.Split('\n');
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            if (trimmedLine.StartsWith("NPC_DIALOGUE:"))
            {
                output.NPCDialogue = ExtractQuotedText(trimmedLine);
            }
            else if (trimmedLine.StartsWith("NARRATIVE:"))
            {
                output.NarrativeText = ExtractQuotedText(trimmedLine);
            }
            else if (trimmedLine.StartsWith("PROGRESSION:"))
            {
                output.ProgressionHint = ExtractQuotedText(trimmedLine);
            }
            else if (trimmedLine.StartsWith("CARD_"))
            {
                ParseCardNarrative(trimmedLine, output.CardNarratives);
            }
        }
        
        return output;
    }
    
    private string ExtractQuotedText(string line)
    {
        int firstQuote = line.IndexOf('"');
        int lastQuote = line.LastIndexOf('"');
        
        if (firstQuote != -1 && lastQuote != -1 && firstQuote != lastQuote)
        {
            return line.Substring(firstQuote + 1, lastQuote - firstQuote - 1);
        }
        
        // Fallback: extract after colon
        int colonIndex = line.IndexOf(':');
        if (colonIndex != -1 && colonIndex < line.Length - 1)
        {
            return line.Substring(colonIndex + 1).Trim().Trim('"');
        }
        
        return line;
    }
    
    private void ParseCardNarrative(string line, Dictionary<string, string> cardNarratives)
    {
        // Expected format: CARD_[ID]: "narrative text"
        int colonIndex = line.IndexOf(':');
        if (colonIndex == -1) return;
        
        string cardPart = line.Substring(0, colonIndex).Trim();
        string narrativePart = line.Substring(colonIndex + 1).Trim();
        
        // Extract card ID
        if (cardPart.StartsWith("CARD_"))
        {
            string cardId = cardPart.Substring(5); // Remove "CARD_" prefix
            string narrative = ExtractQuotedText(narrativePart);
            
            if (!string.IsNullOrEmpty(cardId) && !string.IsNullOrEmpty(narrative))
            {
                cardNarratives[cardId] = narrative;
            }
        }
    }
    
    private NarrativeOutput ValidateAndEnhanceOutput(
        NarrativeOutput output, 
        ConversationState state, 
        NPCData npcData, 
        CardCollection activeCards,
        CardAnalysis analysis)
    {
        // Ensure we have NPC dialogue
        if (string.IsNullOrEmpty(output.NPCDialogue))
        {
            output.NPCDialogue = GenerateFallbackNPCDialogue(state, npcData, analysis);
        }
        
        // Ensure all cards have narratives
        foreach (CardInfo card in activeCards.Cards)
        {
            if (!output.CardNarratives.ContainsKey(card.Id) || string.IsNullOrEmpty(output.CardNarratives[card.Id]))
            {
                output.CardNarratives[card.Id] = GenerateFallbackCardNarrative(card, state.Rapport);
            }
        }
        
        // Add environmental narrative if missing
        if (string.IsNullOrEmpty(output.NarrativeText))
        {
            output.NarrativeText = GenerateFallbackEnvironmental(state, npcData);
        }
        
        return output;
    }
    
    private string GenerateFallbackNPCDialogue(ConversationState state, NPCData npcData, CardAnalysis analysis)
    {
        // Use the generator's logic as fallback
        NarrativeConstraints constraints = generator.DetermineNarrativeConstraints(analysis);
        return generator.GenerateNPCDialogue(constraints, npcData, state);
    }
    
    private string GenerateFallbackCardNarrative(CardInfo card, int rapport)
    {
        // Simple fallback based on card category and focus
        string intensity = card.Focus <= 1 ? "carefully" : card.Focus >= 3 ? "boldly" : "directly";
        string action = card.NarrativeCategory switch
        {
            "risk" => "challenge their perspective",
            "support" => "offer understanding",
            "atmosphere" => "shift the conversation tone",
            "utility" => "ask for more information",
            _ => "respond thoughtfully"
        };
        
        return $"{intensity.Substring(0, 1).ToUpper()}{intensity.Substring(1)} {action}";
    }
    
    private string GenerateFallbackEnvironmental(ConversationState state, NPCData npcData)
    {
        return state.Flow switch
        {
            <= 4 => $"{npcData.Name} maintains a careful distance, watching your reactions closely.",
            <= 9 => $"{npcData.Name} seems more at ease, though still somewhat guarded in their manner.",
            <= 14 => $"{npcData.Name} speaks with growing comfort, their posture relaxing slightly.",
            <= 19 => $"{npcData.Name} leans in slightly, showing genuine interest in the conversation.",
            _ => $"{npcData.Name} speaks with complete openness, their trust evident in every gesture."
        };
    }
}