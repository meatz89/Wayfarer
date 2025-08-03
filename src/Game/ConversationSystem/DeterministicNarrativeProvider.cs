using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provides deterministic narrative content for conversations with conversation override support.
/// </summary>
public class DeterministicNarrativeProvider : INarrativeProvider
{
    private readonly ConversationRepository _conversationRepository;

    public DeterministicNarrativeProvider(ConversationRepository conversationRepository)
    {
        _conversationRepository = conversationRepository;
    }

    public Task<string> GenerateIntroduction(ConversationContext context)
    {
        // Check for conversation override
        if (context.TargetNPC != null)
        {
            var introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(introduction))
            {
                return Task.FromResult(introduction);
            }
        }

        // Default introduction
        return Task.FromResult("You begin a conversation.");
    }

    public Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        // Check for conversation override
        if (context.TargetNPC != null)
        {
            Console.WriteLine($"[DeterministicNarrativeProvider] Checking dialogue for NPC: {context.TargetNPC.ID}");
            var introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
            Console.WriteLine($"[DeterministicNarrativeProvider] Got dialogue: {introduction ?? "null"}");
            if (!string.IsNullOrEmpty(introduction))
            {
                return Task.FromResult(introduction);
            }
        }

        // Generate contextual default introduction based on NPC
        Console.WriteLine("[DeterministicNarrativeProvider] Using default introduction");
        var npcName = context.TargetNPC?.Name ?? "the person";
        var greeting = GenerateContextualGreeting(context, state);
        return Task.FromResult(greeting);
    }

    public Task<List<ConversationChoice>> GenerateChoices(ConversationContext context, ConversationState state, List<ChoiceTemplate> availableTemplates)
    {
        var choices = new List<ConversationChoice>();
        
        // Check if we have a conversation override - if so, provide choices from the conversation
        if (context.TargetNPC != null && _conversationRepository.HasConversation(context.TargetNPC.ID))
        {
            // For special conversations, provide a simple continue option for now
            // TODO: Load actual choices from conversation JSON
            choices.Add(new ConversationChoice
            {
                ChoiceID = "continue",
                NarrativeText = "Continue",
                FocusCost = 0,
                IsAffordable = true
            });
            return Task.FromResult(choices);
        }
        
        // If we have available templates, use them
        if (availableTemplates != null && availableTemplates.Count > 0)
        {
            foreach (var template in availableTemplates)
            {
                choices.Add(new ConversationChoice
                {
                    ChoiceID = template.TemplateName,
                    NarrativeText = template.Description ?? template.TemplateName,
                    FocusCost = template.FocusCost,
                    IsAffordable = true // In tutorial, choices are always affordable
                });
            }
        }
        
        // If no choices available, provide a default end conversation option
        if (choices.Count == 0)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "end",
                NarrativeText = "End conversation",
                FocusCost = 0,
                IsAffordable = true
            });
        }
        
        return Task.FromResult(choices);
    }

    public Task<string> GenerateReaction(ConversationContext context, ConversationState state, ConversationChoice selectedChoice, bool success)
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
            var introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
            if (!string.IsNullOrEmpty(introduction))
            {
                return Task.FromResult(introduction);
            }
        }

        // Default reaction
        return Task.FromResult("They respond to your choice.");
    }

    public Task<string> GenerateConclusion(ConversationContext context, ConversationState state, ConversationChoice lastChoice)
    {
        // Check for conversation override
        if (context.TargetNPC != null)
        {
            var introduction = _conversationRepository.GetNpcIntroduction(context.TargetNPC.ID);
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
    
    private string GenerateContextualGreeting(ConversationContext context, ConversationState state)
    {
        var npc = context.TargetNPC;
        if (npc == null) return "You approach someone.";
        
        // Generate appropriate greeting based on NPC
        var greetings = new List<string>();
        
        // Add generic greetings - we'll keep it simple for now
        greetings.Add($"{npc.Name} looks up as you approach.");
        greetings.Add($"{npc.Name} acknowledges your presence.");
        greetings.Add($"You catch {npc.Name}'s attention.");
        greetings.Add($"{npc.Name} turns to face you. \"Yes?\"");
        greetings.Add($"\"Can I help you?\" {npc.Name} asks.");
        greetings.Add($"{npc.Name} pauses their work to speak with you.");
        
        // Pick a random greeting for variety
        var random = new Random();
        return greetings[random.Next(greetings.Count)];
    }
}

