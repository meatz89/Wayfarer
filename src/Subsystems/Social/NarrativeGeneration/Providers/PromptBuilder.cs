using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Builds prompts from templates for AI conversation generation.
/// Loads markdown templates from Data/Prompts subdirectories and replaces {{placeholders}}
/// with actual conversation context, NPC data, and card analysis results.
/// Supports conditional blocks like {{#if condition}}...{{/if}} and caching for performance.
/// REFACTORED: Uses strongly-typed TemplatePlaceholders instead of Dictionary<string, object>.
/// </summary>
public class PromptBuilder
{
private readonly IContentDirectory contentDirectory;
private readonly ConcurrentDictionary<string, string> templateCache = new ConcurrentDictionary<string, string>();
private readonly Regex placeholderRegex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);
private readonly Regex conditionalRegex = new Regex(@"\{\{#if\s+([^}]+)\}\}(.*?)\{\{/if\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
private readonly Regex eachLoopRegex = new Regex(@"\{\{#each\s+([^}]+)\}\}(.*?)\{\{/each\}\}", RegexOptions.Compiled | RegexOptions.Singleline);

public PromptBuilder(IContentDirectory contentDirectory)
{
    this.contentDirectory = contentDirectory;
}

public string BuildIntroductionPrompt(SocialChallengeState state, NPCData npc, CardCollection cards, CardAnalysis analysis, string conversationType = "standard")
{
    string template = LoadTemplateFromPath("npc/introduction.md");
    TemplatePlaceholders placeholders = BuildPlaceholders(state, npc, cards, analysis);
    placeholders.ConversationType = conversationType;

    return ProcessTemplate(template, placeholders);
}

public string BuildDialoguePrompt(SocialChallengeState state, NPCData npc, CardCollection cards, CardAnalysis analysis, List<string> conversationHistory)
{
    string template = LoadTemplateFromPath("npc/dialogue.md");
    TemplatePlaceholders placeholders = BuildPlaceholders(state, npc, cards, analysis);

    placeholders.ConversationHistory = conversationHistory.Any()
        ? string.Join("\n", conversationHistory)
        : "No previous dialogue.";

    return ProcessTemplate(template, placeholders);
}

public string BuildBatchCardGenerationPrompt(SocialChallengeState state, NPCData npc, CardCollection cards, string npcDialogue)
{
    if (string.IsNullOrEmpty(npcDialogue))
        throw new ArgumentException("NPC dialogue cannot be null or empty", nameof(npcDialogue));

    string template = LoadTemplateFromPath("cards/batch_generation.md");
    TemplatePlaceholders placeholders = BuildPlaceholders(state, npc, cards, null);
    placeholders.NPCDialogue = npcDialogue;

    return ProcessTemplate(template, placeholders);
}

public string BuildConversationPrompt(SocialChallengeState state, NPCData npc, CardCollection cards, CardAnalysis analysis)
{
    return BuildDialoguePrompt(state, npc, cards, analysis, new List<string>());
}

private string LoadTemplateFromPath(string templatePath)
{
    return templateCache.GetOrAdd(templatePath, path =>
    {
        string fullPath = Path.Combine(contentDirectory.Path, "Prompts", path);
        if (File.Exists(fullPath))
        {
            return File.ReadAllText(fullPath);
        }

        return GetFallbackTemplate(path);
    });
}

private string ProcessTemplate(string template, TemplatePlaceholders placeholders)
{
    string processed = template;

    if (processed.Contains("{{base_system}}"))
    {
        string baseSystem = LoadTemplateFromPath("system/base_system.md");
        string baseSystemProcessed = ProcessPlaceholders(baseSystem, placeholders);
        processed = processed.Replace("{{base_system}}", baseSystemProcessed);
    }

    processed = ProcessEachLoops(processed, placeholders);
    processed = ProcessConditionals(processed, placeholders);
    processed = ProcessPlaceholders(processed, placeholders);

    return processed;
}

private string ProcessEachLoops(string template, TemplatePlaceholders placeholders)
{
    return eachLoopRegex.Replace(template, match =>
    {
        string collectionName = match.Groups[1].Value.Trim();
        string loopContent = match.Groups[2].Value;

        if (collectionName != "cards")
            return string.Empty;

        if (placeholders.Cards == null || !placeholders.Cards.Any())
            return string.Empty;

        StringBuilder result = new StringBuilder();
        List<CardInfo> cards = placeholders.Cards;

        for (int i = 0; i < cards.Count; i++)
        {
            CardInfo card = cards[i];
            CardPlaceholders cardPlaceholders = new CardPlaceholders
            {
                Index = i,
                IsFirst = i == 0,
                IsLast = i == cards.Count - 1,
                Count = cards.Count,
                Id = card.Id,
                InitiativeCost = card.InitiativeCost.ToString(),
                Effect = card.Effect.ToString(),
                Persistence = card.Persistence.ToString(),
                NarrativeCategory = card.NarrativeCategory
            };

            string processedContent = ProcessCardTemplate(loopContent, cardPlaceholders);
            result.Append(processedContent);
        }

        return result.ToString();
    });
}

private string ProcessCardTemplate(string template, CardPlaceholders cardPlaceholders)
{
    string processed = template;

    Regex unlessLastRegex = new Regex(@"\{\{#unless\s+@last\}\}(.*?)\{\{/unless\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
    processed = unlessLastRegex.Replace(processed, match =>
    {
        string content = match.Groups[1].Value;
        return cardPlaceholders.IsLast ? string.Empty : content;
    });

    Regex cardPlaceholderRegex = new Regex(@"\{\{([^}]+)\}\}", RegexOptions.Compiled);
    processed = cardPlaceholderRegex.Replace(processed, match =>
    {
        string placeholder = match.Groups[1].Value.Trim();
        string value = cardPlaceholders.GetValue(placeholder);

        if (value == null)
            throw new InvalidOperationException($"Placeholder '{placeholder}' has null value");

        return value;
    });

    return processed;
}

private string ProcessConditionals(string template, TemplatePlaceholders placeholders)
{
    return conditionalRegex.Replace(template, match =>
    {
        string condition = match.Groups[1].Value.Trim();
        string content = match.Groups[2].Value;

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

private bool EvaluateCondition(string condition, TemplatePlaceholders placeholders)
{
    string value = placeholders.GetValue(condition);

    if (value == null)
        return false;

    if (value == "true")
        return true;

    if (value == "false")
        return false;

    return !string.IsNullOrEmpty(value);
}

private string ProcessPlaceholders(string template, TemplatePlaceholders placeholders)
{
    return placeholderRegex.Replace(template, match =>
    {
        string placeholder = match.Groups[1].Value.Trim();
        string value = placeholders.GetValue(placeholder);

        if (value == null)
            throw new InvalidOperationException($"Placeholder '{placeholder}' has null value");

        return value;
    });
}

private TemplatePlaceholders BuildPlaceholders(SocialChallengeState state, NPCData npc, CardCollection cards, CardAnalysis analysis)
{
    return new TemplatePlaceholders
    {
        // Mechanical values
        Flow = state.Flow.ToString(),
        Rapport = state.Momentum.ToString(),
        ConnectionState = state.CurrentState.ToString(),
        FocusAvailable = state.Focus.ToString(),
        Patience = state.Doubt.ToString(),
        TurnCount = state.TotalTurns.ToString(),
        TopicLayer = state.CurrentTopicLayer.ToString(),

        // NPC properties
        NPCName = npc.Name,
        NPCPersonality = npc.Personality.ToString(),
        NPCCrisis = npc.CurrentCrisis,
        NPCActivity = "Current activity",
        NPCEmotionalState = "Determined by flow and rapport",
        CurrentTopic = npc.CurrentTopic,

        // Card properties
        CardCount = cards.Cards.Count.ToString(),
        FocusPattern = analysis != null ? analysis.InitiativePattern.ToString() : "Unknown",

        // Boolean flags
        HasObservation = "false",

        // Template-specific placeholders
        CardSummary = BuildCardSummary(cards),
        CardsDetail = BuildCardsDetail(cards),
        CardNarrativeTemplate = BuildCardNarrativeTemplate(cards),

        // Additional context
        PreviousConversations = "0",
        TimeOfDay = "Unknown",
        Location = "Unknown",
        ConversationType = "standard",

        // Collections for {{#each}} loops
        Cards = cards.Cards
    };
}

private string BuildCardSummary(CardCollection cards)
{
    StringBuilder summary = new StringBuilder();

    for (int i = 0; i < cards.Cards.Count; i++)
    {
        CardInfo card = cards.Cards[i];
        summary.AppendLine($"- Card {i + 1}: {card.InitiativeCost} initiative, {card.Persistence} type");
    }

    return summary.ToString().Trim();
}

private string BuildCardsDetail(CardCollection cards)
{
    StringBuilder detail = new StringBuilder();

    foreach (CardInfo card in cards.Cards)
    {
        detail.AppendLine($"Card ID: {card.Id}");
        detail.AppendLine($"- Initiative Cost: {card.InitiativeCost}");
        detail.AppendLine($"- Success Effect: {card.Effect}");
        detail.AppendLine($"- Persistence: {card.Persistence}");
        detail.AppendLine($"- Category: {card.NarrativeCategory}");
        detail.AppendLine();
    }

    return detail.ToString().Trim();
}

private string BuildCardNarrativeTemplate(CardCollection cards)
{
    StringBuilder template = new StringBuilder();

    for (int i = 0; i < cards.Cards.Count; i++)
    {
        CardInfo card = cards.Cards[i];
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

private string GetFallbackTemplate(string templatePath)
{
    return templatePath switch
    {
        "npc/introduction.md" => "Generate an NPC introduction for {{npc_name}} with {{npc_personality}} personality. Flow: {{flow}}, Rapport: {{rapport}}. Create dialogue that allows all {{card_count}} cards to respond meaningfully.",
        "npc/dialogue.md" => "Generate NPC dialogue for {{npc_name}} ({{npc_personality}}). Current state: Flow {{flow}}, Rapport {{rapport}}. Create response that all cards can address.",
        "cards/batch_generation.md" => "Generate narratives for all cards responding to: \"{{npc_dialogue}}\". Cards: {{cards_detail}}",
        "system/base_system.md" => "You are generating dialogue for Wayfarer. Current context: {{npc_name}} ({{npc_personality}}), Flow: {{flow}}, Rapport: {{rapport}}.",
        _ => $"Template missing: {templatePath}. Using fallback generation."
    };
}
}

/// <summary>
/// Strongly-typed placeholder container for template processing.
/// Replaces Dictionary<string, object> with explicit properties and switch-based lookup.
/// All properties return strings for template compatibility.
/// </summary>
public class TemplatePlaceholders
{
// Mechanical values
public string Flow { get; set; }
public string Rapport { get; set; }
public string ConnectionState { get; set; }
public string FocusAvailable { get; set; }
public string Patience { get; set; }
public string TurnCount { get; set; }
public string TopicLayer { get; set; }

// NPC properties
public string NPCName { get; set; }
public string NPCPersonality { get; set; }
public string NPCCrisis { get; set; }
public string NPCActivity { get; set; }
public string NPCEmotionalState { get; set; }
public string CurrentTopic { get; set; }

// Card properties
public string CardCount { get; set; }
public string FocusPattern { get; set; }

// Boolean flags
public string HasObservation { get; set; }

// Template-specific content
public string CardSummary { get; set; }
public string CardsDetail { get; set; }
public string CardNarrativeTemplate { get; set; }

// Conversation-specific
public string ConversationType { get; set; }
public string ConversationHistory { get; set; }

// Additional context
public string PreviousConversations { get; set; }
public string TimeOfDay { get; set; }
public string Location { get; set; }
public string NPCDialogue { get; set; }

// Collections for loops (not strings)
public List<CardInfo> Cards { get; set; }

/// <summary>
/// Switch-based placeholder lookup (acceptable pattern like catalogues).
/// Returns placeholder value or null if not found.
/// </summary>
public string GetValue(string placeholderName)
{
    return placeholderName switch
    {
        "flow" => Flow,
        "rapport" => Rapport,
        "connection_state" => ConnectionState,
        "focus_available" => FocusAvailable,
        "patience" => Patience,
        "turn_count" => TurnCount,
        "topic_layer" => TopicLayer,
        "npc_name" => NPCName,
        "npc_personality" => NPCPersonality,
        "npc_crisis" => NPCCrisis,
        "npc_activity" => NPCActivity,
        "npc_emotional_state" => NPCEmotionalState,
        "current_topic" => CurrentTopic,
        "card_count" => CardCount,
        "focus_pattern" => FocusPattern,
        "has_observation" => HasObservation,
        "card_summary" => CardSummary,
        "cards_detail" => CardsDetail,
        "card_narrative_template" => CardNarrativeTemplate,
        "conversation_type" => ConversationType,
        "conversation_history" => ConversationHistory,
        "previous_conversations" => PreviousConversations,
        "time_of_day" => TimeOfDay,
        "location" => Location,
        "npc_dialogue" => NPCDialogue,
        _ => $"{{{{{placeholderName}}}}}" // Preserve unknown placeholders for debugging
    };
}
}

/// <summary>
/// Strongly-typed placeholders for card iteration in {{#each cards}} loops.
/// </summary>
public class CardPlaceholders
{
public int Index { get; set; }
public bool IsFirst { get; set; }
public bool IsLast { get; set; }
public int Count { get; set; }
public string Id { get; set; }
public string InitiativeCost { get; set; }
public string Effect { get; set; }
public string Persistence { get; set; }
public string NarrativeCategory { get; set; }

/// <summary>
/// Switch-based placeholder lookup for card properties.
/// </summary>
public string GetValue(string placeholderName)
{
    return placeholderName switch
    {
        "@index" => Index.ToString(),
        "@first" => IsFirst.ToString().ToLower(),
        "@last" => IsLast.ToString().ToLower(),
        "@count" => Count.ToString(),
        "id" => Id,
        "initiativeCost" => InitiativeCost,
        "effect" => Effect,
        "persistence" => Persistence,
        "narrativeCategory" => NarrativeCategory,
        _ => $"{{{{{placeholderName}}}}}"
    };
}
}
