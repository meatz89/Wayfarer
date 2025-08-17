using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Hybrid narrative system that combines authored content with systemic variations.
/// Core stories are authored, variations emerge from game state.
/// </summary>
public class HybridNarrativeProvider : INarrativeProvider
{
    private readonly ConversationRepository _conversationRepository;
    private readonly NPCStateResolver _stateCalculator;
    private readonly LetterQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly BodyLanguageTemplates _bodyLanguage;

    // Authored core stories for each NPC
    private readonly Dictionary<string, List<CoreStory>> _authoredStories;

    public HybridNarrativeProvider(
        ConversationRepository conversationRepository,
        NPCStateResolver stateCalculator,
        LetterQueueManager queueManager,
        TokenMechanicsManager tokenManager)
    {
        _conversationRepository = conversationRepository;
        _stateCalculator = stateCalculator;
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _bodyLanguage = LoadBodyLanguageTemplates();
        _authoredStories = LoadAuthoredStories();
    }

    public Task<string> GenerateIntroduction(SceneContext context, ConversationState state)
    {
        if (context.TargetNPC == null)
            return Task.FromResult("You begin a conversation.");

        // Check for authored story first
        CoreStory? coreStory = GetCoreStory(context.TargetNPC.ID, context);
        if (coreStory != null)
        {
            // Modify authored story based on emotional state
            NPCEmotionalState npcState = _stateCalculator.CalculateState(context.TargetNPC);
            string modifiedStory = ModifyForEmotionalState(coreStory, npcState);

            // Add body language from templates
            string bodyLanguage = GetBodyLanguage(npcState);

            return Task.FromResult($"{context.TargetNPC.Name} {bodyLanguage}.\n\n{modifiedStory}");
        }

        // Fall back to systemic generation if no authored story
        return GenerateSystemicIntroduction(context, state);
    }

    public Task<string> GenerateIntroduction(SceneContext context)
    {
        return GenerateIntroduction(context, null);
    }

    public Task<List<ConversationChoice>> GenerateChoices(SceneContext context, ConversationState state, List<ChoiceTemplate> availableTemplates)
    {
        // Choices are generated mechanically by ConversationChoiceGenerator
        // This method exists for interface compliance but shouldn't be used
        return Task.FromResult(new List<ConversationChoice>());
    }

    public Task<string> GenerateReaction(SceneContext context, ConversationState state, ConversationChoice selectedChoice, bool success)
    {
        if (context.TargetNPC == null)
            return Task.FromResult("They react to your choice.");

        // Generate reaction based on choice type and emotional state
        NPCEmotionalState npcState = _stateCalculator.CalculateState(context.TargetNPC);
        string reaction = GenerateReactionText(context.TargetNPC, npcState, selectedChoice, success);

        // Add character action from templates
        string characterAction = success
            ? _bodyLanguage.GetRandomAction("afterPositiveChoice")
            : _bodyLanguage.GetRandomAction("afterNegativeChoice");

        return Task.FromResult($"{reaction}\n\n{characterAction}");
    }

    public Task<string> GenerateConclusion(SceneContext context, ConversationState state, ConversationChoice selectedChoice)
    {
        if (context.TargetNPC == null)
            return Task.FromResult("The conversation comes to an end.");

        NPCEmotionalState npcState = _stateCalculator.CalculateState(context.TargetNPC);

        // Generate conclusion based on emotional state
        string conclusion = npcState switch
        {
            NPCEmotionalState.DESPERATE => $"{context.TargetNPC.Name} watches you leave with desperate hope in their eyes.",
            NPCEmotionalState.CALCULATING => $"{context.TargetNPC.Name} nods thoughtfully as you depart.",
            NPCEmotionalState.HOSTILE => $"{context.TargetNPC.Name} dismisses you with a sharp gesture.",
            NPCEmotionalState.WITHDRAWN => $"{context.TargetNPC.Name} turns away, lost in thought.",
            _ => $"You part ways with {context.TargetNPC.Name}."
        };

        return Task.FromResult(conclusion);
    }

    public Task<bool> IsAvailable()
    {
        // Always available as the hybrid provider
        return Task.FromResult(true);
    }

    private CoreStory? GetCoreStory(string npcId, SceneContext context)
    {
        // Special case for Elena's marriage proposal
        if (npcId == "elena")
        {
            return new CoreStory
            {
                Id = "elena_marriage_proposal",
                BaseText = "\"The letter contains Lord Aldwin's marriage proposal. My refusal.\" She meets your eyes. \"If he learns before my cousin can intervene at court, I'll be ruined.\"\n\nHer hand reaches toward yours across the table, trembling slightly.\n\n\"Please. I know you have obligations, but I need this delivered today.\"",
                DesperateVariation = "\"Please, you must understand - if Lord Aldwin receives my refusal before my cousin can speak at court, I'm ruined!\" Her voice cracks with desperation. \"I have no one else to turn to.\"",
                HostileVariation = "\"The letter... it's my refusal to Lord Aldwin's proposal.\" She glances nervously around. \"If he learns before my cousin intervenes at court, there will be consequences.\"",
                CalculatingVariation = "\"I need this letter delivered with precision. It contains my refusal to Lord Aldwin.\" She studies you carefully. \"The timing is crucial - my cousin must have time to speak at court first.\""
            };
        }

        // Check for other authored stories
        if (_authoredStories.ContainsKey(npcId))
        {
            // Could select based on context (which letter, what urgency, etc.)
            return _authoredStories[npcId].FirstOrDefault();
        }

        return null;
    }

    private string ModifyForEmotionalState(CoreStory story, NPCEmotionalState state)
    {
        return state switch
        {
            NPCEmotionalState.DESPERATE => story.DesperateVariation ?? story.BaseText,
            NPCEmotionalState.HOSTILE => story.HostileVariation ?? story.BaseText,
            NPCEmotionalState.CALCULATING => story.CalculatingVariation ?? story.BaseText,
            _ => story.BaseText
        };
    }

    private string GetBodyLanguage(NPCEmotionalState state)
    {
        string key = state switch
        {
            NPCEmotionalState.DESPERATE => "desperate",
            NPCEmotionalState.HOSTILE => "hostile",
            NPCEmotionalState.CALCULATING => "calculating",
            NPCEmotionalState.WITHDRAWN => "withdrawn",
            _ => "neutral"
        };

        return _bodyLanguage.GetRandomBodyLanguage(key);
    }

    private Task<string> GenerateSystemicIntroduction(SceneContext context, ConversationState state)
    {
        // Fallback to pure systemic generation when no authored content
        NPCEmotionalState npcState = _stateCalculator.CalculateState(context.TargetNPC);

        // Find their most urgent letter
        Letter? npcLetters = _queueManager.GetActiveLetters()
            .Where(l => l.SenderId == context.TargetNPC.ID || l.SenderName == context.TargetNPC.Name)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();

        // Generate dialogue from letter properties and emotional state
        string dialogue = _stateCalculator.GenerateNPCDialogue(
            context.TargetNPC,
            npcState,
            npcLetters,
            context);

        // Add body language
        string bodyLanguage = GetBodyLanguage(npcState);

        return Task.FromResult($"{context.TargetNPC.Name} {bodyLanguage}.\n\n{dialogue}");
    }

    private string GenerateReactionText(NPC npc, NPCEmotionalState state, ConversationChoice choice, bool success)
    {
        // Generate contextual reaction based on choice type
        if (choice.ChoiceID.Contains("prioritize"))
        {
            return success
                ? $"Relief washes over {npc.Name}'s face. \"Thank you. I knew I could count on you.\""
                : $"{npc.Name}'s expression hardens. \"I see. I'll remember this.\"";
        }

        if (choice.ChoiceID.Contains("investigate"))
        {
            return $"{npc.Name} leans in closer, sharing what they know.";
        }

        if (choice.ChoiceID.Contains("swear"))
        {
            return $"A profound gratitude fills {npc.Name}'s eyes. \"I won't forget this oath.\"";
        }

        // Default reactions based on state
        return state switch
        {
            NPCEmotionalState.DESPERATE => success ? "Hope flickers in their eyes." : "Despair deepens.",
            NPCEmotionalState.HOSTILE => success ? "Some hostility eases." : "Anger intensifies.",
            NPCEmotionalState.CALCULATING => success ? "A satisfied nod." : "Eyes narrow thoughtfully.",
            _ => success ? "They seem pleased." : "They seem disappointed."
        };
    }

    private BodyLanguageTemplates LoadBodyLanguageTemplates()
    {
        try
        {
            string path = Path.Combine("Content", "Templates", "body_language.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<BodyLanguageTemplates>(json) ?? new BodyLanguageTemplates();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading body language templates: {ex.Message}");
        }

        // Return default templates if file not found
        return new BodyLanguageTemplates();
    }

    private Dictionary<string, List<CoreStory>> LoadAuthoredStories()
    {
        // In a full implementation, this would load from JSON files
        // For now, return empty dictionary (Elena is hardcoded above)
        return new Dictionary<string, List<CoreStory>>();
    }
}

/// <summary>
/// Represents an authored story with emotional variations
/// </summary>
public class CoreStory
{
    public string Id { get; set; }
    public string BaseText { get; set; }
    public string? DesperateVariation { get; set; }
    public string? CalculatingVariation { get; set; }
    public string? HostileVariation { get; set; }
    public string? WithdrawnVariation { get; set; }
}

/// <summary>
/// Body language templates loaded from JSON
/// </summary>
public class BodyLanguageTemplates
{
    public Dictionary<string, List<string>> BodyLanguage { get; set; } = new();
    public Dictionary<string, List<string>> PeripheralHints { get; set; } = new();
    public Dictionary<string, List<string>> CharacterActions { get; set; } = new();

    private Random _random = new Random();

    public string GetRandomBodyLanguage(string emotionalState)
    {
        if (BodyLanguage.ContainsKey(emotionalState) && BodyLanguage[emotionalState].Any())
        {
            List<string> options = BodyLanguage[emotionalState];
            return options[_random.Next(options.Count)];
        }
        return "regards you carefully";
    }

    public string GetRandomAction(string actionType)
    {
        if (CharacterActions.ContainsKey(actionType) && CharacterActions[actionType].Any())
        {
            List<string> options = CharacterActions[actionType];
            return options[_random.Next(options.Count)];
        }
        return "";
    }

    public string GetRandomHint(string hintType)
    {
        if (PeripheralHints.ContainsKey(hintType) && PeripheralHints[hintType].Any())
        {
            List<string> options = PeripheralHints[hintType];
            return options[_random.Next(options.Count)];
        }
        return "";
    }
}