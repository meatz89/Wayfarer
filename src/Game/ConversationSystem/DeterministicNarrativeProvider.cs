using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Provides deterministic narrative content for conversations with conversation override support.
/// NOW USES SYSTEMIC MECHANICS - choices emerge from queue state, not templates!
/// </summary>
public class DeterministicNarrativeProvider : INarrativeProvider
{
    private readonly ConversationRepository _conversationRepository;
    private readonly VerbContextualizer _verbContextualizer;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly LetterQueueManager _queueManager;

    public DeterministicNarrativeProvider(
        ConversationRepository conversationRepository,
        VerbContextualizer verbContextualizer,
        NPCEmotionalStateCalculator stateCalculator,
        LetterQueueManager queueManager)
    {
        _conversationRepository = conversationRepository;
        _verbContextualizer = verbContextualizer;
        _stateCalculator = stateCalculator;
        _queueManager = queueManager;
    }

    public Task<string> GenerateIntroduction(SceneContext context)
    {
        // Check for conversation override
        if (context.TargetNPC != null)
        {
            string introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(introduction))
            {
                return Task.FromResult(introduction);
            }
        }

        // Default introduction
        return Task.FromResult("You begin a conversation.");
    }

    public Task<string> GenerateIntroduction(SceneContext context, ConversationState state)
    {
        // Check for conversation override (tutorial, special conversations)
        if (context.TargetNPC != null)
        {
            Console.WriteLine($"[DeterministicNarrativeProvider] Checking dialogue for NPC: {context.TargetNPC.ID}");
            string? introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
            Console.WriteLine($"[DeterministicNarrativeProvider] Got dialogue: {introduction ?? "null"}");
            if (!string.IsNullOrEmpty(introduction))
            {
                return Task.FromResult(introduction);
            }
        }

        // Generate systemic dialogue from queue state
        if (context.TargetNPC != null)
        {
            var npcState = _stateCalculator.CalculateState(context.TargetNPC);
            
            // Find their most urgent letter
            var npcLetters = _queueManager.GetActiveLetters()
                .Where(l => l.SenderId == context.TargetNPC.ID || l.SenderName == context.TargetNPC.Name)
                .OrderBy(l => l.DeadlineInDays)
                .FirstOrDefault();
            
            // Generate dialogue from letter properties and emotional state
            string dialogue = _stateCalculator.GenerateNPCDialogue(
                context.TargetNPC, 
                npcState, 
                npcLetters, 
                context);
                
            // Add body language for literary presentation
            string bodyLanguage = npcLetters != null 
                ? _stateCalculator.GenerateBodyLanguage(npcState, npcLetters.Stakes)
                : _stateCalculator.GenerateBodyLanguage(npcState, StakeType.REPUTATION);
                
            string fullIntroduction = $"{context.TargetNPC.Name} {bodyLanguage}. {dialogue}";
            
            return Task.FromResult(fullIntroduction);
        }

        // Fallback
        Console.WriteLine("[DeterministicNarrativeProvider] Using default introduction");
        string npcName = context.TargetNPC?.Name ?? "the person";
        string greeting = GenerateContextualGreeting(context, state);
        return Task.FromResult(greeting);
    }

    public Task<List<ConversationChoice>> GenerateChoices(SceneContext context, ConversationState state, List<ChoiceTemplate> availableTemplates)
    {
        List<ConversationChoice> choices = new List<ConversationChoice>();

        // Check if we have a conversation override - if so, provide choices from the conversation
        if (context.TargetNPC != null && _conversationRepository.HasConversation(context.TargetNPC.ID))
        {
            // For special conversations (like tutorial), provide a simple continue option
            choices.Add(new ConversationChoice
            {
                ChoiceID = "continue",
                NarrativeText = "Continue",
                AttentionCost = 0,  // Free response
                IsAffordable = true
            });
            return Task.FromResult(choices);
        }

        // NEW: Generate choices from queue state using VerbContextualizer
        if (context.TargetNPC != null && _verbContextualizer != null && context.AttentionManager != null)
        {
            choices = _verbContextualizer.GenerateChoicesFromQueueState(
                context.TargetNPC, 
                context.AttentionManager);
            
            if (choices.Count > 0)
            {
                return Task.FromResult(choices);
            }
        }

        // FALLBACK: If no systemic choices generated, provide default exit
        choices.Add(new ConversationChoice
        {
            ChoiceID = "end",
            NarrativeText = "End conversation",
            AttentionCost = 0,
            IsAffordable = true
        });

        return Task.FromResult(choices);
    }

    public Task<string> GenerateReaction(SceneContext context, ConversationState state, ConversationChoice selectedChoice, bool success)
    {
        // Special handling for "continue" choice in tutorial dialogues
        if (selectedChoice.ChoiceID == "continue")
        {
            // Mark conversation as complete after the continue choice
            state.IsConversationComplete = true;
            return Task.FromResult(""); // Empty response since conversation is ending
        }

        // Check for conversation override
        if (context.TargetNPC != null)
        {
            string introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(introduction))
            {
                return Task.FromResult(introduction);
            }
        }

        // Default reaction
        return Task.FromResult("They respond to your choice.");
    }

    public Task<string> GenerateConclusion(SceneContext context, ConversationState state, ConversationChoice lastChoice)
    {
        // Check for conversation override
        if (context.TargetNPC != null)
        {
            string introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(introduction))
            {
                return Task.FromResult(introduction);
            }
        }

        // Default conclusion
        return Task.FromResult("The conversation ends.");
    }

    public Task<bool> IsAvailable()
    {
        // STUB: Always available
        return Task.FromResult(true);
    }

    private int DetermineAttentionCost(ChoiceTemplate template)
    {
        // Basic responses are free
        if (template.TemplateName.ToLower().Contains("goodbye") ||
            template.TemplateName.ToLower().Contains("leave") ||
            template.TemplateName.ToLower().Contains("nevermind"))
            return 0;

        // Information gathering costs 1
        if (template.TemplateName.ToLower().Contains("ask") ||
            template.TemplateName.ToLower().Contains("tell") ||
            template.TemplateName.ToLower().Contains("rumor"))
            return 1;

        // Major actions cost 2
        if (template.TemplateName.ToLower().Contains("promise") ||
            template.TemplateName.ToLower().Contains("swear") ||
            template.TemplateName.ToLower().Contains("negotiate"))
            return 2;

        // Default cost is 1 for any meaningful interaction
        return 1;
    }

    private string GenerateContextualGreeting(SceneContext context, ConversationState state)
    {
        NPC npc = context.TargetNPC;
        if (npc == null) return "You approach someone.";

        // Generate appropriate greeting based on NPC
        List<string> greetings = new List<string>();

        // Add generic greetings - we'll keep it simple for now
        greetings.Add($"{npc.Name} looks up as you approach.");
        greetings.Add($"{npc.Name} acknowledges your presence.");
        greetings.Add($"You catch {npc.Name}'s attention.");
        greetings.Add($"{npc.Name} turns to face you. \"Yes?\"");
        greetings.Add($"\"Can I help you?\" {npc.Name} asks.");
        greetings.Add($"{npc.Name} pauses their work to speak with you.");

        // Pick a random greeting for variety
        Random random = new Random();
        return greetings[random.Next(greetings.Count)];
    }
}

