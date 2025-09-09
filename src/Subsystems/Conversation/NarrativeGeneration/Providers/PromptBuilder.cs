using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Builds prompts from templates for AI conversation generation.
/// Loads templates from the Data/Prompts directory and replaces placeholders
/// with actual conversation context and card analysis results.
/// </summary>
public class PromptBuilder
{
    private readonly IContentDirectory contentDirectory;
    
    /// <summary>
    /// Initializes the prompt builder with content directory path.
    /// </summary>
    /// <param name="contentDirectory">Content directory for loading prompt templates</param>
    public PromptBuilder(IContentDirectory contentDirectory)
    {
        this.contentDirectory = contentDirectory;
    }
    
    /// <summary>
    /// Builds a conversation prompt from templates and current context.
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
        string templatePath = Path.Combine(contentDirectory.Path, "Prompts", "conversation-template.md");
        string template = LoadTemplate(templatePath);
        
        if (string.IsNullOrEmpty(template))
        {
            // Fallback template if file doesn't exist
            template = GetFallbackTemplate();
        }
        
        string contextualizedPrompt = ReplaceContextPlaceholder(template, state, npc, cards, analysis);
        return contextualizedPrompt;
    }
    
    private string LoadTemplate(string templatePath)
    {
        if (!File.Exists(templatePath))
            return null;
        
        return File.ReadAllText(templatePath);
    }
    
    private string GetFallbackTemplate()
    {
        return @"# WAYFARER CONVERSATION GENERATION

{PROMPT_CONTEXT}

## Generation Requirements

Generate NPC dialogue and card responses using backwards construction principle:

1. **NPC Dialogue**: Create dialogue that ALL available cards can meaningfully respond to
2. **Card Responses**: Map each card to an appropriate response narrative
3. **Narrative Coherence**: Ensure all responses make sense in context

## Response Format

```
NPC_DIALOGUE: ""[The NPC's statement]""
NARRATIVE: ""[Environmental description]""
CARD_[ID]: ""[Response for specific card]""
PROGRESSION: ""[Optional hint]""
```

Generate contextually appropriate content based on the provided game state.";
    }
    
    private string ReplaceContextPlaceholder(string template, ConversationState state, NPCData npc, 
        CardCollection cards, CardAnalysis analysis)
    {
        string context = BuildPromptContext(state, npc, cards, analysis);
        return template.Replace("{PROMPT_CONTEXT}", context);
    }
    
    private string BuildPromptContext(ConversationState state, NPCData npc, CardCollection cards, CardAnalysis analysis)
    {
        StringBuilder context = new StringBuilder();
        
        // Add mechanical state information
        context.AppendLine("## Current Conversation State");
        context.AppendLine($"- Flow: {state.Flow}/24 (connection level)");
        context.AppendLine($"- Rapport: {state.Rapport} (trust level)");
        context.AppendLine($"- Atmosphere: {state.Atmosphere}");
        context.AppendLine($"- Focus Available: {state.Focus}");
        context.AppendLine($"- Patience Remaining: {state.Patience}");
        context.AppendLine($"- Connection State: {state.CurrentState}");
        context.AppendLine();
        
        // Add NPC context
        context.AppendLine("## NPC Information");
        context.AppendLine($"- Name: {npc.Name}");
        context.AppendLine($"- Personality: {npc.Personality}");
        context.AppendLine($"- Current Topic: {npc.CurrentTopic ?? "General conversation"}");
        if (!string.IsNullOrEmpty(npc.CurrentCrisis))
        {
            context.AppendLine($"- Crisis/Problem: {npc.CurrentCrisis}");
        }
        context.AppendLine();
        
        // Add card analysis results
        context.AppendLine("## Card Analysis (Backwards Construction Requirements)");
        context.AppendLine($"- Has Impulse Cards: {analysis.HasImpulse} (needs urgent response)");
        context.AppendLine($"- Has Opening Cards: {analysis.HasOpening} (needs inviting response)");
        context.AppendLine($"- Focus Pattern: {analysis.FocusPattern}");
        context.AppendLine($"- Dominant Category: {analysis.DominantCategory}");
        context.AppendLine($"- Requires Urgency: {analysis.RequiresUrgency}");
        context.AppendLine($"- Requires Invitation: {analysis.RequiresInvitation}");
        context.AppendLine();
        
        // Add detailed card information
        context.AppendLine("## Available Cards (Player Options)");
        foreach (CardInfo card in cards.Cards)
        {
            context.AppendLine($"### Card: {card.Id}");
            context.AppendLine($"- Focus Cost: {card.Focus}");
            context.AppendLine($"- Difficulty: {card.Difficulty}");
            context.AppendLine($"- Effect: {card.Effect}");
            context.AppendLine($"- Persistence: {card.Persistence}");
            context.AppendLine($"- Category: {card.NarrativeCategory}");
            context.AppendLine();
        }
        
        // Add rapport-based guidance
        context.AppendLine("## Rapport-Based Narrative Guidelines");
        RapportStage stage = GetRapportStage(state.Rapport);
        context.AppendLine($"- Current Stage: {stage}");
        context.AppendLine(GetRapportGuidance(stage));
        context.AppendLine();
        
        // Add backwards construction instructions
        context.AppendLine("## Backwards Construction Instructions");
        context.AppendLine("1. Generate NPC dialogue that ALL listed cards can respond to meaningfully");
        context.AppendLine("2. Include urgent elements if HasImpulse is true");
        context.AppendLine("3. Include inviting elements if HasOpening is true");
        context.AppendLine($"4. Match intensity level to focus pattern: {analysis.FocusPattern}");
        context.AppendLine($"5. Align with dominant narrative category: {analysis.DominantCategory}");
        
        return context.ToString();
    }
    
    private RapportStage GetRapportStage(int rapport)
    {
        if (rapport <= 5) return RapportStage.Surface;
        if (rapport <= 10) return RapportStage.Gateway;
        if (rapport <= 15) return RapportStage.Personal;
        return RapportStage.Intimate;
    }
    
    private string GetRapportGuidance(RapportStage stage)
    {
        return stage switch
        {
            RapportStage.Surface => "Focus on observations, polite deflections, testing boundaries",
            RapportStage.Gateway => "Show understanding, share relevant experience, gentle challenges",
            RapportStage.Personal => "Direct emotional support, personal commitments, confronting issues",
            RapportStage.Intimate => "Deep vulnerability, unconditional support, life-changing offers",
            _ => "Engage appropriately for current relationship level"
        };
    }
}