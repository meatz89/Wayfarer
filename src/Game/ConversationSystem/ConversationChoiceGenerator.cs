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
    private readonly NPCDeckFactory _deckFactory;

    public ConversationChoiceGenerator(
        LetterQueueManager queueManager,
        TokenMechanicsManager tokenManager,
        NPCStateResolver stateCalculator,
        ITimeManager timeManager,
        Player player,
        GameWorld gameWorld,
        Wayfarer.GameState.TimeBlockAttentionManager timeBlockAttentionManager,
        NPCDeckFactory deckFactory)
    {
        _queueManager = queueManager;
        _tokenManager = tokenManager;
        _stateCalculator = stateCalculator;
        _timeManager = timeManager;
        _player = player;
        _gameWorld = gameWorld;
        _timeBlockAttentionManager = timeBlockAttentionManager;
        _deckFactory = deckFactory;
    }

    public List<ConversationChoice> GenerateChoices(SceneContext context, ConversationState state)
    {
        
        // Initialize NPC deck if needed
        context.TargetNPC.InitializeConversationDeck(_deckFactory);
        
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
                PatienceCost = card.PatienceCost,  // FIXED: Use PatienceCost not AttentionCost
                IsAffordable = true, // Will be set based on patience in conversation
                IsAvailable = card.CanPlay(tokenDict, 0),
                MechanicalDescription = GetRichMechanicalDescription(card, context.TargetNPC),
                MechanicalEffects = card.MechanicalEffects ?? new List<IMechanicalEffect>()
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
                PatienceCost = 0,  // FIXED: Use PatienceCost
                IsAffordable = false,
                IsAvailable = false,
                MechanicalDescription = "Conversation blocked - deliver overdue letter to restore communication",
                MechanicalEffects = new List<IMechanicalEffect>()
            },
            new ConversationChoice
            {
                ChoiceID = "exit",
                NarrativeText = "Step away quietly.",
                PatienceCost = 0,  // FIXED: Use PatienceCost
                IsAffordable = true,
                IsAvailable = true,
                MechanicalDescription = "→ End conversation",
                MechanicalEffects = new List<IMechanicalEffect>()
            }
        };
    }
    
    /// <summary>
    /// Generate rich mechanical descriptions by reading from actual mechanical effects
    /// </summary>
    private string GetRichMechanicalDescription(ConversationCard card, NPC npc)
    {
        var parts = new List<string>();
        
        // Add comfort gain/loss (this is a card property, not hardcoded)
        if (card.ComfortGain > 0)
        {
            parts.Add($"+{card.ComfortGain} comfort");
        }
        else if (card.ComfortGain < 0)
        {
            parts.Add($"{card.ComfortGain} comfort");
        }
        
        // Add difficulty indication (derived from card property)
        if (card.Difficulty > 6)
        {
            parts.Add("High difficulty");
        }
        else if (card.Difficulty < 3)
        {
            parts.Add("Easy approach");
        }
        
        // Add special effects based on card type (categorical)
        if (card.Category == RelationshipCardCategory.Crisis)
        {
            parts.Add("Emergency option");
        }
        
        // Read from actual mechanical effects
        if (card.MechanicalEffects != null && card.MechanicalEffects.Any())
        {
            foreach (var effect in card.MechanicalEffects)
            {
                var descriptions = effect.GetDescriptionsForPlayer();
                foreach (var desc in descriptions)
                {
                    parts.Add(TranslateEffectDescription(desc));
                }
            }
        }
        
        return string.Join(" • ", parts);
    }
    
    /// <summary>
    /// Translate mechanical effect descriptions without ugly Unicode symbols
    /// CSS classes will handle beautiful icons
    /// </summary>
    private string TranslateEffectDescription(MechanicalEffectDescription desc)
    {
        return desc.Text; // No Unicode symbols - let CSS handle icons via effect categorization
    }
    
}