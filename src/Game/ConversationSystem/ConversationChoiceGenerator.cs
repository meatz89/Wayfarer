using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates conversation choices based on game state, NOT narrative
/// This is where the actual mechanics are determined
/// </summary>
public class ConversationChoiceGenerator
{
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly VerbContextualizer _verbContextualizer;
    private readonly ITimeManager _timeManager;
    private readonly ObservationTemplates _observationTemplates;

    public ConversationChoiceGenerator(
        LetterQueueManager queueManager,
        ConnectionTokenManager tokenManager,
        NPCEmotionalStateCalculator stateCalculator,
        VerbContextualizer verbContextualizer,
        ITimeManager timeManager)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _verbContextualizer = verbContextualizer;
        _timeManager = timeManager;
        _observationTemplates = new ObservationTemplates();
    }

    public List<ConversationChoice> GenerateChoices(SceneContext context, ConversationState state)
    {
        var choices = new List<ConversationChoice>();
        
        // Check if no attention left - only allow exit
        if (context?.AttentionManager != null && context.AttentionManager.Current == 0)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "leave",
                NarrativeText = "\"I need to go.\"", // Basic text, will be enhanced by narrative provider
                AttentionCost = 0,
                IsAffordable = true,
                MechanicalDescription = "→ End conversation",
                MechanicalEffects = new List<IMechanicalEffect> { new NoEffect() }
            });
            return choices;
        }

        // Use VerbContextualizer for systemic choice generation FIRST
        if (_verbContextualizer != null && context?.TargetNPC != null && context.AttentionManager != null)
        {
            choices = _verbContextualizer.GenerateChoicesFromQueueState(
                context.TargetNPC,
                context.AttentionManager,
                _stateCalculator);
            
            // Add observation choices based on location tags
            if (context.LocationName != null)
            {
                var locationTags = LocationTagExtensions.GetLocationTags(context.LocationName);
                var observationActions = LocationTagObservations.GetObservationActions(locationTags);
                
                // Add up to 2 observation choices if attention available
                int observationIndex = 0;
                foreach (var observation in observationActions.Take(2))
                {
                    if (context.AttentionManager.Current >= observation.AttentionCost)
                    {
                        // Generate templated observation text
                        var observationText = "";
                        if (locationTags.Any())
                        {
                            // Use the first relevant tag for this observation
                            var relevantTag = locationTags.First();
                            var seed = context.LocationName.GetHashCode() + observationIndex + _timeManager.GetCurrentTimeHours();
                            observationText = _observationTemplates.GenerateObservation(relevantTag, context.LocationName.ToLower(), seed);
                        }
                        
                        choices.Add(new ConversationChoice
                        {
                            ChoiceID = $"observe_{observation.Id}",
                            NarrativeText = $"[Observe: {observation.Description}]",
                            AttentionCost = observation.AttentionCost,
                            IsAffordable = true,
                            MechanicalDescription = "→ Notice environmental detail",
                            MechanicalEffects = new List<IMechanicalEffect> 
                            { 
                                new CreateMemoryEffect(
                                    observation.Id, 
                                    observationText.Length > 0 ? observationText : observation.Description, 
                                    1, // Low priority
                                    1  // 1 day duration
                                )
                            }
                        });
                        observationIndex++;
                    }
                }
            }
            
            if (choices.Any())
            {
                return choices.Take(5).ToList(); // Max 5 choices total
            }
        }

        // FALLBACK: Special handling for Elena conversation (temporary for mockup)
        // Only use this if VerbContextualizer didn't generate choices
        if (context?.TargetNPC != null && context.TargetNPC.ID == "elena")
        {
            Console.WriteLine("[WARNING] Falling back to hardcoded Elena choices - VerbContextualizer returned no choices");
            return GenerateElenaChoices(context, state);
        }

        // Fallback: basic exit option
        choices.Add(new ConversationChoice
        {
            ChoiceID = "end",
            NarrativeText = "End conversation",
            AttentionCost = 0,
            IsAffordable = true,
            MechanicalEffects = new List<IMechanicalEffect> { new NoEffect() }
        });

        return choices;
    }

    private List<ConversationChoice> GenerateElenaChoices(SceneContext context, ConversationState state)
    {
        var choices = new List<ConversationChoice>();
        
        // Find Elena's letter in the queue
        var elenaLetter = _queueManager.GetActiveLetters()
            .FirstOrDefault(l => l.SenderName == "Elena");
        
        // Free Response - Acknowledge without action
        choices.Add(new ConversationChoice
        {
            ChoiceID = "maintain_state",
            NarrativeText = "\"I understand. Your letter is second in my queue.\"",
            AttentionCost = 0,
            IsAffordable = true,
            MechanicalDescription = "→ Maintains current state",
            MechanicalEffects = new List<IMechanicalEffect> { new NoEffect() }
        });

        // 1 Attention: Queue Negotiation (as in mockup)
        if (context.AttentionManager?.Current >= 1 && elenaLetter != null)
        {
            // Find Lord Aldwin's letter to determine token cost
            var lordAldwinLetter = _queueManager.GetActiveLetters()
                .FirstOrDefault(l => l.RecipientName == "Lord Aldwin" || l.SenderName == "Lord Aldwin");
            
            choices.Add(new ConversationChoice
            {
                ChoiceID = "prioritize",
                NarrativeText = "\"I'll prioritize your letter. Let me check what that means...\"",
                AttentionCost = 1,
                IsAffordable = true,
                MechanicalDescription = "✓ Move letter to position 1 | ⚠ Burn 1 Status token with Lord Aldwin",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new LetterReorderEffect(
                        elenaLetter.Id, 
                        1, // Target position
                        1, // Token cost
                        ConnectionType.Status,
                        _queueManager,
                        _tokenManager,
                        "lord_aldwin"),
                    new CreateMemoryEffect("elena_prioritized", "Promised to prioritize Elena's marriage refusal letter", 3, 1)
                }
            });

            // 1 Attention: Information Trade (as in mockup)
            choices.Add(new ConversationChoice
            {
                ChoiceID = "investigate",
                NarrativeText = "\"Lord Aldwin from Riverside? Tell me about the situation...\"",
                AttentionCost = 1,
                IsAffordable = true,
                MechanicalDescription = "ℹ Gain rumor: \"Noble carriage schedule\" | ⏱ +20 minutes conversation",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new CreateMemoryEffect("noble_carriage_schedule", "Noble carriages leave at dawn for Riverside", 2, -1),
                    new ConversationTimeEffect(20, _timeManager) // 20 minutes conversation time
                }
            });
        }

        // 2 Attention: Binding Obligation (as in mockup)
        if (context.AttentionManager?.Current >= 2 && elenaLetter != null)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = "swear",
                NarrativeText = "\"I swear I'll deliver your letter before any others today.\"",
                AttentionCost = 2,
                IsAffordable = true,
                MechanicalDescription = "♥ +2 Trust tokens immediately | ⛓ Creates Binding Obligation",
                MechanicalEffects = new List<IMechanicalEffect>
                {
                    new GainTokensEffect(ConnectionType.Trust, 2, "elena", _tokenManager),
                    // TODO: Add obligation effect when ObligationType is defined
                    // For now, use memory effect to track the obligation</                    
                    new CreateMemoryEffect("elena_binding_oath", "Sworn to deliver Elena's marriage refusal before all others", 5, 1)
                }
            });
        }

        // 3 Attention: Deep Investigation (always locked as teaching mechanic)
        choices.Add(new ConversationChoice
        {
            ChoiceID = "deep_investigate",
            NarrativeText = "\"Let me investigate Lord Aldwin's current position at court...\"",
            AttentionCost = 3,
            IsAffordable = false, // Can never afford with max 3 attention - teaching mechanic
            MechanicalDescription = "[Requires 3 attention points]",
            MechanicalEffects = new List<IMechanicalEffect>()
        });

        return choices;
    }
}