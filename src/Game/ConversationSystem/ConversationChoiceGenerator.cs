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
        NPCEmotionalState emotionalState = _stateCalculator.CalculateState(context.TargetNPC);

        // HOSTILE NPCs refuse conversation entirely
        if (!ConversationPatienceCalculator.CanConverse(emotionalState))
        {
            return GetHostileChoices(context.TargetNPC);
        }

        // Get current relationship tokens
        Dictionary<ConnectionType, int> tokenDict = _tokenManager.GetTokensWithNPC(context.TargetNPC.ID);

        // Calculate starting patience based on emotional state and relationships
        int startingPatience = ConversationPatienceCalculator.CalculateStartingPatience(
            context.TargetNPC, emotionalState, tokenDict);

        // Store emotional state context for conversation flow
        context.CurrentTokens = tokenDict;

        // Draw 5 cards from NPC's deck filtered by emotional state
        List<ConversationCard> drawnCards = context.TargetNPC.ConversationDeck.DrawCards(tokenDict, 0, emotionalState);

        // Convert cards to ConversationChoice objects
        List<ConversationChoice> choices = new List<ConversationChoice>();
        foreach (ConversationCard card in drawnCards)
        {
            choices.Add(new ConversationChoice
            {
                ChoiceID = card.Id,
                NarrativeText = card.Description,
                PatienceCost = card.PatienceCost,  // FIXED: Use PatienceCost not AttentionCost
                ComfortGain = card.ComfortGain,    // Pass comfort gain from card
                IsAffordable = true, // Will be set based on patience in conversation
                IsAvailable = card.CanPlay(tokenDict, 0),
                MechanicalDescription = GetRichMechanicalDescription(card, context.TargetNPC),
                MechanicalEffects = card.MechanicalEffects ?? new List<IMechanicalEffect>()
            });
        }

        // LETTER OFFER INTEGRATION: Add letter offer choices when comfort threshold reached
        if (state.HasReachedLetterThreshold())
        {
            List<ConversationChoice> letterOfferChoices = GenerateLetterOfferChoices(context.TargetNPC, state);
            choices.AddRange(letterOfferChoices);
        }

        return choices;
    }

    /// <summary>
    /// Generate letter offer choices when comfort threshold is reached
    /// Prevents automatic letter generation - requires explicit player choice
    /// </summary>
    private List<ConversationChoice> GenerateLetterOfferChoices(NPC npc, ConversationState state)
    {
        List<ConversationChoice> offerChoices = new List<ConversationChoice>();

        // Get current relationship tokens to determine offer type
        Dictionary<ConnectionType, int> tokenDict = _tokenManager.GetTokensWithNPC(npc.ID);
        
        // Determine primary relationship type for letter offer
        ConnectionType offerType = GetHighestRelationshipType(tokenDict);
        
        // Generate letter offer text based on relationship type and NPC
        string offerNarrative = GetLetterOfferNarrative(npc, offerType, state.HasReachedPerfectThreshold());
        string declineNarrative = GetDeclineOfferNarrative(npc);
        
        // Accept letter offer choice
        offerChoices.Add(new ConversationChoice
        {
            ChoiceID = $"accept_offer_{offerType}",
            NarrativeText = offerNarrative,
            PatienceCost = 1, // Meaningful action costs patience
            ComfortGain = 1, // Accepting strengthens relationship
            IsAffordable = true,
            IsAvailable = true,
            ChoiceType = ConversationChoiceType.AcceptLetterOffer,
            OfferTokenType = offerType,
            OfferCategory = state.HasReachedPerfectThreshold() ? LetterCategory.Premium : LetterCategory.Quality,
            MechanicalDescription = GetLetterOfferMechanicalDescription(offerType, state.HasReachedPerfectThreshold()),
            MechanicalEffects = new List<IMechanicalEffect>() // Will be handled by GameFacade
        });

        // Decline letter offer choice
        offerChoices.Add(new ConversationChoice
        {
            ChoiceID = $"decline_offer_{offerType}",
            NarrativeText = declineNarrative,
            PatienceCost = 0, // Polite decline costs no patience
            ComfortGain = 0, // No relationship change
            IsAffordable = true,
            IsAvailable = true,
            ChoiceType = ConversationChoiceType.DeclineLetterOffer,
            OfferTokenType = offerType, // Store token type for decline processing too
            MechanicalDescription = "Politely decline • No letter offered • Relationship maintained",
            MechanicalEffects = new List<IMechanicalEffect>()
        });

        return offerChoices;
    }

    /// <summary>
    /// Determine the highest relationship type with an NPC for letter offers
    /// </summary>
    private ConnectionType GetHighestRelationshipType(Dictionary<ConnectionType, int> tokens)
    {
        if (!tokens.Any() || tokens.Values.All(v => v == 0))
        {
            return ConnectionType.Trust; // Default to Trust for new relationships
        }

        return tokens.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    /// <summary>
    /// Generate contextual narrative for letter offers based on relationship and NPC
    /// </summary>
    private string GetLetterOfferNarrative(NPC npc, ConnectionType offerType, bool isPerfectConversation)
    {
        string intensifier = isPerfectConversation ? "You're exactly the person I need for this. " : "";
        
        return offerType switch
        {
            ConnectionType.Trust => $"{intensifier}I have a personal letter that needs someone I can truly depend on.",
            ConnectionType.Commerce => $"{intensifier}I have a business matter that requires a reliable courier.",
            ConnectionType.Status => $"{intensifier}There's a formal correspondence that needs proper handling.",
            ConnectionType.Shadow => $"{intensifier}I have... sensitive correspondence that requires discretion.",
            _ => $"{intensifier}I have a letter that could use your particular skills."
        };
    }

    /// <summary>
    /// Generate polite decline narrative
    /// </summary>
    private string GetDeclineOfferNarrative(NPC npc)
    {
        return "I appreciate the trust, but I'm quite overwhelmed with current commitments.";
    }

    /// <summary>
    /// Generate mechanical description for letter offers
    /// </summary>
    private string GetLetterOfferMechanicalDescription(ConnectionType offerType, bool isPerfectConversation)
    {
        string letterQuality = isPerfectConversation ? "Premium" : "Quality";
        string bonus = isPerfectConversation ? " • Generous deadline • Possible bonus letter" : " • Good payment";
        
        return $"+ {letterQuality} {offerType} letter{bonus} • Queue position based on relationship strength";
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
        List<string> parts = new List<string>();

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
            foreach (IMechanicalEffect effect in card.MechanicalEffects)
            {
                List<MechanicalEffectDescription> descriptions = effect.GetDescriptionsForPlayer();
                foreach (MechanicalEffectDescription desc in descriptions)
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