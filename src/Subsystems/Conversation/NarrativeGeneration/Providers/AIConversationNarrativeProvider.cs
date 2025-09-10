using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
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
    /// Phase 1: Generates NPC dialogue and environmental narrative only.
    /// Analyzes active cards to create NPC dialogue that all cards can respond to.
    /// Returns immediately after NPC dialogue generation for quick UI update.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npcData">NPC information for context</param>
    /// <param name="activeCards">Cards available to player</param>
    /// <returns>NarrativeOutput with NPCDialogue/NarrativeText filled, CardNarratives empty</returns>
    public async Task<NarrativeOutput> GenerateNPCDialogueAsync(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards)
    {
        // Step 1: Use backwards construction to analyze cards
        CardAnalysis analysis = generator.AnalyzeActiveCards(activeCards);
        
        // Step 2: Determine prompt type and build appropriate prompt for NPC dialogue
        string npcPrompt = DetermineAndBuildPrompt(state, npcData, activeCards, analysis);
        
        // Step 3: Generate NPC dialogue using AI with 5 second timeout
        string npcResponse;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            npcResponse = await GenerateAIResponseAsync(npcPrompt, cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Timeout - use fallback
            npcResponse = string.Empty;
        }
        catch
        {
            // AI generation failed - use fallback
            npcResponse = string.Empty;
        }
        
        // Step 4: Parse NPC response into structured output
        NarrativeOutput output = ParseAIResponse(npcResponse, activeCards, state);
        
        // Ensure we have NPC dialogue (use fallback if needed)
        if (string.IsNullOrEmpty(output.NPCDialogue))
        {
            output.NPCDialogue = GenerateFallbackNPCDialogue(state, npcData, analysis);
        }
        
        // Add environmental narrative if missing
        if (string.IsNullOrEmpty(output.NarrativeText))
        {
            output.NarrativeText = GenerateFallbackEnvironmental(state, npcData);
        }
        
        // Set provider source
        output.ProviderSource = !string.IsNullOrEmpty(npcResponse) ? 
            NarrativeProviderType.AIGenerated : NarrativeProviderType.JsonFallback;
        
        return output;
    }
    
    /// <summary>
    /// Phase 2: Generates card-specific narratives based on NPC dialogue.
    /// Uses the NPC dialogue from Phase 1 to create contextually appropriate card responses.
    /// Called separately after UI has been updated with NPC dialogue.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npcData">NPC information for context</param>
    /// <param name="activeCards">Cards available to player</param>
    /// <param name="npcDialogue">The NPC dialogue generated in Phase 1</param>
    /// <returns>List of card narratives with their IDs and text</returns>
    public async Task<List<CardNarrative>> GenerateCardNarrativesAsync(
        ConversationState state,
        NPCData npcData,
        CardCollection activeCards,
        string npcDialogue)
    {
        Console.WriteLine($"[AIProvider] GenerateCardNarrativesAsync called with {activeCards.Cards.Count} cards");
        List<CardNarrative> cardNarratives = new List<CardNarrative>();
        
        if (string.IsNullOrEmpty(npcDialogue))
        {
            Console.WriteLine("[AIProvider] No NPC dialogue - using fallback card narratives");
            // Return fallback narratives if no NPC dialogue
            return GenerateFallbackCardNarratives(activeCards, state.Rapport);
        }
        
        // Build prompt for card generation
        string cardPrompt = promptBuilder.BuildBatchCardGenerationPrompt(state, npcData, activeCards, npcDialogue);
        Console.WriteLine($"[AIProvider] Card generation prompt built, length: {cardPrompt.Length}");
        
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            string cardResponse = await GenerateAIResponseAsync(cardPrompt, cts.Token);
            Console.WriteLine($"[AIProvider] AI card response received, length: {cardResponse?.Length ?? 0}");
            
            if (!string.IsNullOrEmpty(cardResponse))
            {
                // Parse the card narratives from AI response
                cardNarratives = ParseCardNarrativesAsList(cardResponse, activeCards);
                Console.WriteLine($"[AIProvider] Parsed {cardNarratives.Count} card narratives from AI response");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[AIProvider] Card generation timed out - using fallback");
            // Timeout - use fallback
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AIProvider] Card generation failed: {ex.Message}");
            // AI generation failed - use fallback
        }
        
        // Fill in any missing card narratives with fallbacks
        foreach (CardInfo card in activeCards.Cards)
        {
            if (!cardNarratives.Any(cn => cn.CardId == card.Id))
            {
                cardNarratives.Add(new CardNarrative
                {
                    CardId = card.Id,
                    NarrativeText = GenerateFallbackCardNarrative(card, state.Rapport),
                    ProviderSource = NarrativeProviderType.JsonFallback
                });
            }
        }
        
        return cardNarratives;
    }
    
    /// <summary>
    /// Checks if the AI provider is available for use.
    /// Tests Ollama connectivity and configuration.
    /// PRINCIPLE: Always use async/await properly. Never block async operations
    /// with .Wait() or .Result as this causes deadlocks in ASP.NET Core.
    /// </summary>
    /// <returns>True if AI provider can generate content, false otherwise</returns>
    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            // Properly await the async operation instead of blocking
            bool isAvailable = await ollamaClient.CheckHealthAsync();
            if (!isAvailable)
            {
                Console.WriteLine("[AIConversationNarrativeProvider] Health check returned false");
            }
            return isAvailable;
        }
        catch (Exception ex)
        {
            // Any exception means provider is not available
            Console.WriteLine($"[AIConversationNarrativeProvider] Health check exception: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Gets the provider type for identifying this provider.
    /// </summary>
    /// <returns>Provider type enum value</returns>
    public NarrativeProviderType GetProviderType()
    {
        return NarrativeProviderType.AIGenerated;
    }
    
    private async Task<string> GenerateAIResponseAsync(string prompt, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[AIProvider] GenerateAIResponseAsync called, prompt length: {prompt?.Length ?? 0}");
        StringBuilder responseBuilder = new StringBuilder();
        
        try
        {
            await foreach (string token in ollamaClient.StreamCompletionAsync(prompt).WithCancellation(cancellationToken))
            {
                responseBuilder.Append(token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AIProvider] GenerateAIResponseAsync error: {ex.Message}");
            throw;
        }
        
        Console.WriteLine($"[AIProvider] GenerateAIResponseAsync response length: {responseBuilder.Length}");
        return responseBuilder.ToString();
    }
    
    /// <summary>
    /// Parses the card narrative response from the AI and returns a list of CardNarrative objects.
    /// </summary>
    /// <param name="jsonResponse">JSON response from batch card generation AI call</param>
    /// <param name="activeCards">Active cards for validation</param>
    /// <returns>List of parsed card narratives</returns>
    private List<CardNarrative> ParseCardNarrativesAsList(string jsonResponse, CardCollection activeCards)
    {
        List<CardNarrative> cardNarratives = new List<CardNarrative>();
        
        if (string.IsNullOrEmpty(jsonResponse))
        {
            Console.WriteLine("[AIProvider] ParseCardNarrativesAsList: Empty response");
            return cardNarratives;
        }
        
        Console.WriteLine($"[AIProvider] ParseCardNarrativesAsList: Parsing response: {jsonResponse.Substring(0, Math.Min(200, jsonResponse.Length))}...");
        
        try
        {
            var cardNarrativeOutput = ParseCardGenerationJSON(jsonResponse, null);
            
            // Convert dictionary to list of CardNarrative objects
            if (cardNarrativeOutput.CardNarratives != null)
            {
                Console.WriteLine($"[AIProvider] Found {cardNarrativeOutput.CardNarratives.Count} card narratives in parsed output");
                foreach (var cn in cardNarrativeOutput.CardNarratives)
                {
                    Console.WriteLine($"[AIProvider] Adding card narrative: {cn.CardId} = '{cn.NarrativeText}'");
                    cardNarratives.Add(new CardNarrative
                    {
                        CardId = cn.CardId,
                        NarrativeText = cn.NarrativeText,
                        ProviderSource = NarrativeProviderType.AIGenerated
                    });
                }
            }
            else
            {
                Console.WriteLine("[AIProvider] No card narratives found in parsed output");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AIProvider] JSON parsing failed: {ex.Message}");
            // JSON parsing failed - return empty list for fallback handling
        }
        
        return cardNarratives;
    }
    
    /// <summary>
    /// Generates fallback card narratives for all active cards.
    /// </summary>
    private List<CardNarrative> GenerateFallbackCardNarratives(CardCollection activeCards, int rapport)
    {
        List<CardNarrative> cardNarratives = new List<CardNarrative>();
        
        foreach (CardInfo card in activeCards.Cards)
        {
            cardNarratives.Add(new CardNarrative
            {
                CardId = card.Id,
                NarrativeText = GenerateFallbackCardNarrative(card, rapport),
                ProviderSource = NarrativeProviderType.JsonFallback
            });
        }
        
        return cardNarratives;
    }
    
    private NarrativeOutput ParseAIResponse(string aiResponse, CardCollection activeCards, ConversationState state)
    {
        NarrativeOutput output = new NarrativeOutput
        {
            CardNarratives = new List<CardNarrative>()
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
            CardNarratives = new List<CardNarrative>()
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
            CardNarratives = new List<CardNarrative>()
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
            CardNarratives = new List<CardNarrative>()
        };
        
        if (root.TryGetProperty("card_narratives", out JsonElement cardNarratives))
        {
            foreach (var cardProp in cardNarratives.EnumerateObject())
            {
                string cardId = cardProp.Name;
                if (cardProp.Value.TryGetProperty("card_text", out JsonElement cardText))
                {
                    output.CardNarratives.Add(new CardNarrative
                    {
                        CardId = cardId,
                        NarrativeText = cardText.GetString(),
                        ProviderSource = NarrativeProviderType.AIGenerated
                    });
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
            CardNarratives = new List<CardNarrative>()
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
    
    private void ParseCardNarrative(string line, List<CardNarrative> cardNarratives)
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
                cardNarratives.Add(new CardNarrative
                {
                    CardId = cardId,
                    NarrativeText = narrative,
                    ProviderSource = NarrativeProviderType.AIGenerated
                });
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
            if (!output.CardNarratives.Any(cn => cn.CardId == card.Id))
            {
                output.CardNarratives.Add(new CardNarrative
                {
                    CardId = card.Id,
                    NarrativeText = GenerateFallbackCardNarrative(card, state.Rapport),
                    ProviderSource = NarrativeProviderType.JsonFallback
                });
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