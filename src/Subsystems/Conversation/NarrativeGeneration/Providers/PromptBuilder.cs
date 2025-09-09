using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;

/// <summary>
/// Builds prompts from templates for AI conversation generation.
/// Loads markdown templates from Data/Prompts subdirectories and replaces {{placeholders}}
/// with actual conversation context, NPC data, and card analysis results.
/// Supports conditional blocks like {{#if condition}}...{{/if}} and caching for performance.
/// </summary>
public class PromptBuilder
{
    private readonly IContentDirectory contentDirectory;
    private readonly ConcurrentDictionary<string, string> templateCache = new ConcurrentDictionary<string, string>();
    private readonly Regex placeholderRegex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);
    private readonly Regex conditionalRegex = new Regex(@"\{\{#if\s+([^}]+)\}\}(.*?)\{\{/if\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
    
    /// <summary>
    /// Initializes the prompt builder with content directory path.
    /// </summary>
    /// <param name="contentDirectory">Content directory for loading prompt templates</param>
    public PromptBuilder(IContentDirectory contentDirectory)
    {
        this.contentDirectory = contentDirectory;
    }
    
    /// <summary>
    /// Builds an NPC introduction prompt using template-based generation.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npc">NPC data for context</param>
    /// <param name="cards">Active cards for backwards construction</param>
    /// <param name="analysis">Card analysis results</param>
    /// <param name="conversationType">Type of conversation being started</param>
    /// <returns>Complete prompt ready for AI generation</returns>
    public string BuildIntroductionPrompt(ConversationState state, NPCData npc, CardCollection cards, CardAnalysis analysis, string conversationType = "standard")
    {
        var template = LoadTemplateFromPath("npc/introduction.md");
        var placeholders = ExtractPlaceholders(state, npc, cards, analysis);
        placeholders["conversation_type"] = conversationType;
        
        return ProcessTemplate(template, placeholders);
    }
    
    /// <summary>
    /// Builds an NPC dialogue prompt for LISTEN actions using template-based generation.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npc">NPC data for context</param>
    /// <param name="cards">Active cards for backwards construction</param>
    /// <param name="analysis">Card analysis results</param>
    /// <param name="conversationHistory">Previous turns in the conversation</param>
    /// <returns>Complete prompt ready for AI generation</returns>
    public string BuildDialoguePrompt(ConversationState state, NPCData npc, CardCollection cards, CardAnalysis analysis, List<string> conversationHistory = null)
    {
        var template = LoadTemplateFromPath("npc/dialogue.md");
        var placeholders = ExtractPlaceholders(state, npc, cards, analysis);
        
        // Add conversation history
        if (conversationHistory != null && conversationHistory.Any())
        {
            placeholders["conversation_history"] = string.Join("\n", conversationHistory);
        }
        else
        {
            placeholders["conversation_history"] = "No previous dialogue.";
        }
        
        return ProcessTemplate(template, placeholders);
    }
    
    /// <summary>
    /// Builds a batch card narrative generation prompt using template-based generation.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npc">NPC data for context</param>
    /// <param name="cards">Active cards for narrative generation</param>
    /// <param name="npcDialogue">The NPC's current dialogue that cards need to respond to</param>
    /// <returns>Complete prompt ready for AI generation</returns>
    public string BuildBatchCardGenerationPrompt(ConversationState state, NPCData npc, CardCollection cards, string npcDialogue)
    {
        var template = LoadTemplateFromPath("cards/batch_generation.md");
        var placeholders = ExtractPlaceholders(state, npc, cards, null);
        placeholders["npc_dialogue"] = npcDialogue ?? "No dialogue provided";
        
        return ProcessTemplate(template, placeholders);
    }
    
    /// <summary>
    /// Builds a conversation prompt from templates and current context (legacy method).
    /// Loads the base conversation template and replaces placeholders with
    /// actual game state, NPC data, and card analysis results.
    /// </summary>
    /// <param name="state">Current conversation mechanical state</param>
    /// <param name="npc">NPC data for context</param>
    /// <param name="cards">Active cards for backwards construction</param>
    /// <param name="analysis">Card analysis results</param>
    /// <returns>Complete prompt ready for AI generation</returns>
    public string BuildConversationPrompt(ConversationState state, NPCData npc, CardCollection cards, CardAnalysis analysis)
    {
        // Legacy method - use dialogue prompt as fallback
        return BuildDialoguePrompt(state, npc, cards, analysis);
    }
    
    /// <summary>
    /// Loads a template from the Prompts directory with caching.
    /// </summary>
    /// <param name="templatePath">Relative path from Prompts directory (e.g., "npc/introduction.md")</param>
    /// <returns>Template content or fallback if not found</returns>
    private string LoadTemplateFromPath(string templatePath)
    {
        return templateCache.GetOrAdd(templatePath, path =>
        {
            var fullPath = Path.Combine(contentDirectory.Path, "Prompts", path);
            if (File.Exists(fullPath))
            {
                return File.ReadAllText(fullPath);
            }
            
            // Fallback template for missing files
            return GetFallbackTemplate(path);
        });
    }
    
    /// <summary>
    /// Processes a template by replacing placeholders and handling conditionals.
    /// </summary>
    /// <param name="template">Template content with {{placeholders}}</param>
    /// <param name="placeholders">Dictionary of placeholder values</param>
    /// <returns>Processed template with placeholders replaced</returns>
    private string ProcessTemplate(string template, Dictionary<string, string> placeholders)
    {
        var processed = template;
        
        // First handle base_system inclusion
        if (processed.Contains("{{base_system}}"))
        {
            var baseSystem = LoadTemplateFromPath("system/base_system.md");
            var baseSystemProcessed = ProcessPlaceholders(baseSystem, placeholders);
            processed = processed.Replace("{{base_system}}", baseSystemProcessed);
        }
        
        // Handle conditional blocks
        processed = ProcessConditionals(processed, placeholders);
        
        // Handle regular placeholders
        processed = ProcessPlaceholders(processed, placeholders);
        
        return processed;
    }
    
    /// <summary>
    /// Processes conditional blocks in templates like {{#if condition}}...{{/if}}.
    /// </summary>
    /// <param name="template">Template content</param>
    /// <param name="placeholders">Placeholder values for condition evaluation</param>
    /// <returns>Template with conditionals processed</returns>
    private string ProcessConditionals(string template, Dictionary<string, string> placeholders)
    {
        return conditionalRegex.Replace(template, match =>
        {
            var condition = match.Groups[1].Value.Trim();
            var content = match.Groups[2].Value;
            
            // Evaluate condition
            if (EvaluateCondition(condition, placeholders))
            {
                return content;
            }
            else
            {
                return string.Empty;
            }
        });
    }
    
    /// <summary>
    /// Evaluates a condition for conditional blocks.
    /// </summary>
    /// <param name="condition">Condition to evaluate</param>
    /// <param name="placeholders">Available placeholder values</param>
    /// <returns>True if condition is met</returns>
    private bool EvaluateCondition(string condition, Dictionary<string, string> placeholders)
    {
        // Handle boolean conditions
        if (placeholders.TryGetValue(condition, out string value))
        {
            if (bool.TryParse(value, out bool boolValue))
            {
                return boolValue;
            }
            
            // Non-empty string is true
            return !string.IsNullOrEmpty(value);
        }
        
        return false;
    }
    
    /// <summary>
    /// Replaces {{placeholder}} patterns with actual values.
    /// </summary>
    /// <param name="template">Template content</param>
    /// <param name="placeholders">Placeholder values</param>
    /// <returns>Template with placeholders replaced</returns>
    private string ProcessPlaceholders(string template, Dictionary<string, string> placeholders)
    {
        return placeholderRegex.Replace(template, match =>
        {
            var placeholder = match.Groups[1].Value.Trim();
            
            if (placeholders.TryGetValue(placeholder, out string value))
            {
                return value;
            }
            
            // Return placeholder unchanged if not found (for debugging)
            return match.Value;
        });
    }
    
    /// <summary>
    /// Extracts all placeholder values from game state objects.
    /// </summary>
    /// <param name="state">Conversation state</param>
    /// <param name="npc">NPC data</param>
    /// <param name="cards">Card collection</param>
    /// <param name="analysis">Card analysis (can be null)</param>
    /// <returns>Dictionary of placeholder names to values</returns>
    private Dictionary<string, string> ExtractPlaceholders(ConversationState state, NPCData npc, CardCollection cards, CardAnalysis analysis)
    {
        var placeholders = new Dictionary<string, string>
        {
            // Mechanical values
            ["flow"] = state.Flow.ToString(),
            ["rapport"] = state.Rapport.ToString(),
            ["atmosphere"] = state.Atmosphere.ToString(),
            ["connection_state"] = state.CurrentState.ToString(),
            ["focus_available"] = state.Focus.ToString(),
            ["patience"] = state.Patience.ToString(),
            ["turn_count"] = state.TotalTurns.ToString(),
            ["topic_layer"] = state.CurrentTopicLayer.ToString(),
            
            // NPC properties
            ["npc_name"] = npc.Name ?? "Unknown",
            ["npc_personality"] = npc.Personality.ToString(),
            ["npc_crisis"] = npc.CurrentCrisis ?? "None",
            ["npc_activity"] = "Current activity", // Could be added to NPCData later
            ["npc_emotional_state"] = "Determined by flow and rapport", // Derived value
            ["current_topic"] = npc.CurrentTopic ?? "General conversation",
            
            // Card properties
            ["card_count"] = cards.Cards.Count.ToString(),
            ["focus_pattern"] = analysis?.FocusPattern.ToString() ?? "Unknown",
            
            // Boolean flags
            ["has_impulse"] = (analysis?.HasImpulse ?? false).ToString().ToLower(),
            ["has_opening"] = (analysis?.HasOpening ?? false).ToString().ToLower(),
            ["has_observation"] = "false", // Could be determined from card analysis
            
            // Template-specific placeholders
            ["card_summary"] = BuildCardSummary(cards),
            ["cards_detail"] = BuildCardsDetail(cards),
            ["impulse_requirement"] = BuildImpulseRequirement(analysis),
            ["opening_requirement"] = BuildOpeningRequirement(analysis),
            ["card_narrative_template"] = BuildCardNarrativeTemplate(cards),
            
            // Additional context
            ["previous_conversations"] = "0", // Could be tracked
            ["time_of_day"] = "Unknown", // Could be added
            ["location"] = "Unknown", // Could be added
            ["conversation_type"] = "standard" // Default value, can be overridden
        };
        
        return placeholders;
    }
    
    /// <summary>
    /// Builds a summary of cards for templates.
    /// </summary>
    private string BuildCardSummary(CardCollection cards)
    {
        var summary = new StringBuilder();
        
        for (int i = 0; i < cards.Cards.Count; i++)
        {
            var card = cards.Cards[i];
            summary.AppendLine($"- Card {i + 1}: {card.Focus} focus, {card.Persistence} type, {card.Difficulty} difficulty");
        }
        
        return summary.ToString().Trim();
    }
    
    /// <summary>
    /// Builds detailed card information for templates.
    /// </summary>
    private string BuildCardsDetail(CardCollection cards)
    {
        var detail = new StringBuilder();
        
        foreach (var card in cards.Cards)
        {
            detail.AppendLine($"Card ID: {card.Id}");
            detail.AppendLine($"- Focus Cost: {card.Focus}");
            detail.AppendLine($"- Success Effect: {card.Effect}");
            detail.AppendLine($"- Difficulty: {card.Difficulty}");
            detail.AppendLine($"- Persistence: {card.Persistence}");
            detail.AppendLine($"- Category: {card.NarrativeCategory}");
            detail.AppendLine();
        }
        
        return detail.ToString().Trim();
    }
    
    /// <summary>
    /// Builds impulse requirement text for templates.
    /// </summary>
    private string BuildImpulseRequirement(CardAnalysis analysis)
    {
        if (analysis?.HasImpulse == true)
        {
            return "4. Include urgency that Impulse cards can address";
        }
        return "";
    }
    
    /// <summary>
    /// Builds opening requirement text for templates.
    /// </summary>
    private string BuildOpeningRequirement(CardAnalysis analysis)
    {
        if (analysis?.HasOpening == true)
        {
            return "5. Invite elaboration that Opening cards can explore";
        }
        return "";
    }
    
    /// <summary>
    /// Builds card narrative template for JSON structure.
    /// </summary>
    private string BuildCardNarrativeTemplate(CardCollection cards)
    {
        var template = new StringBuilder();
        
        for (int i = 0; i < cards.Cards.Count; i++)
        {
            var card = cards.Cards[i];
            template.Append($"    \"{card.Id}\": {{");
            template.Append($"      \"card_text\": \"One sentence that appears on card\",");
            template.Append($"      \"approach_type\": \"Type of approach (probe/support/demand/deflect)\"");
            template.Append($"    }}");
            
            if (i < cards.Cards.Count - 1)
            {
                template.Append(",");
            }
        }
        
        return template.ToString();
    }
    
    /// <summary>
    /// Provides fallback templates for missing files.
    /// </summary>
    private string GetFallbackTemplate(string templatePath)
    {
        return templatePath switch
        {
            "npc/introduction.md" => "Generate an NPC introduction for {{npc_name}} with {{npc_personality}} personality. Flow: {{flow}}, Rapport: {{rapport}}. Create dialogue that allows all {{card_count}} cards to respond meaningfully.",
            "npc/dialogue.md" => "Generate NPC dialogue for {{npc_name}} ({{npc_personality}}). Current state: Flow {{flow}}, Rapport {{rapport}}, {{atmosphere}}. Create response that all cards can address.",
            "cards/batch_generation.md" => "Generate narratives for all cards responding to: \"{{npc_dialogue}}\". Cards: {{cards_detail}}",
            "system/base_system.md" => "You are generating dialogue for Wayfarer. Current context: {{npc_name}} ({{npc_personality}}), Flow: {{flow}}, Rapport: {{rapport}}, Atmosphere: {{atmosphere}}.",
            _ => $"Template missing: {templatePath}. Using fallback generation."
        };
    }
}