using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.GameState;

/// <summary>
/// Simplified conversation choice generator for card-based system
/// Generates basic placeholder choices while card system is being integrated
/// </summary>
public class ConversationChoiceGenerator
{
    private readonly LetterQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly NPCStateResolver _stateCalculator;
    private readonly ITimeManager _timeManager;
    private readonly Player _player;
    private readonly GameWorld _gameWorld;
    private readonly Wayfarer.GameState.TimeBlockAttentionManager _timeBlockAttentionManager;

    public ConversationChoiceGenerator(
        LetterQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        NPCStateResolver stateCalculator,
        ITimeManager timeManager,
        Player player,
        GameWorld gameWorld,
        Wayfarer.GameState.TimeBlockAttentionManager timeBlockAttentionManager)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _timeManager = timeManager;
        _player = player;
        _gameWorld = gameWorld;
        _timeBlockAttentionManager = timeBlockAttentionManager;
    }

    public List<ConversationChoice> GenerateChoices(SceneContext context, ConversationState state)
    {
        if (context?.TargetNPC == null)
        {
            return GetFallbackChoices();
        }
        
        // Initialize NPC deck if needed
        context.TargetNPC.InitializeConversationDeck();
        
        // Calculate NPC emotional state based on current letters and deadlines
        var emotionalState = _stateCalculator.CalculateState(context.TargetNPC);
        
        // HOSTILE NPCs refuse conversation entirely
        if (!ConversationPatienceCalculator.CanConverse(emotionalState))
        {
            return GetHostileChoices(context.TargetNPC);
        }
        
        // Get current relationship tokens
        var tokenDict = _tokenManager.GetTokensWithNPC(context.TargetNPC.ID);
        
        // Calculate starting patience based on emotional state and relationships
        var startingPatience = ConversationPatienceCalculator.CalculateStartingPatience(
            context.TargetNPC, emotionalState, tokenDict);
            
        // Store emotional state context for conversation flow
        context.CurrentTokens = tokenDict;
        
        // Draw 5 cards from NPC's deck filtered by emotional state
        var drawnCards = context.TargetNPC.ConversationDeck.DrawCards(tokenDict, 0, emotionalState);
        
        // Convert cards to ConversationChoice objects
        var choices = new List<ConversationChoice>();
        foreach (var card in drawnCards)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = card.Id,
                NarrativeText = card.Description,
                AttentionCost = card.PatienceCost,
                IsAffordable = true, // Will be set based on patience in conversation
                IsAvailable = card.CanPlay(tokenDict, 0),
                MechanicalDescription = $"Difficulty {card.Difficulty} | +{card.ComfortGain} comfort",
                MechanicalEffects = new List<IMechanicalEffect>()
            });
        }
        
        return choices;
    }
    
    private List<ConversationChoice> GetHostileChoices(NPC npc)
    {
        // HOSTILE NPCs refuse conversation - show why and how to resolve
        return new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "hostile_blocked",
                NarrativeText = $"{npc.Name} refuses to speak with you. Their letter deadline has passed.",
                AttentionCost = 0,
                IsAffordable = false,
                IsAvailable = false,
                MechanicalDescription = "→ Conversation blocked - deliver overdue letter to restore communication",
                MechanicalEffects = new List<IMechanicalEffect>()
            },
            new ConversationChoice
            {
                ChoiceID = "exit",
                NarrativeText = "Step away quietly.",
                AttentionCost = 0,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "→ End conversation",
                MechanicalEffects = new List<IMechanicalEffect>()
            }
        };
    }
    
    private List<ConversationChoice> GetFallbackChoices()
    {
        // Simple fallback choices when no NPC deck is available
        return new List<ConversationChoice>
        {
            new ConversationChoice
            {
                ChoiceID = "exit",
                NarrativeText = "I should go.",
                AttentionCost = 0,
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "→ End conversation",
                MechanicalEffects = new List<IMechanicalEffect>()
            }
        };
    }
}