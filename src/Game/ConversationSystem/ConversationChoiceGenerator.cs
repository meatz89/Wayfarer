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
    private readonly ObligationQueueManager _queueManager;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly NPCStateResolver _stateCalculator;
    private readonly ITimeManager _timeManager;
    private readonly Player _player;
    private readonly GameWorld _gameWorld;
    private readonly Wayfarer.GameState.TimeBlockAttentionManager _timeBlockAttentionManager;
    private readonly NPCDeckFactory _deckFactory;

    public ConversationChoiceGenerator(
        ObligationQueueManager queueManager,
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

        // LETTER CARD INTEGRATION: Add letter request cards when any comfort is built
        // This must happen BEFORE drawing cards to ensure they appear in current conversation
        if (state.TotalComfort > 0 && !state.LetterCardAddedThisConversation)
        {
            AddLetterRequestCardToDeck(context.TargetNPC, state);
            state.LetterCardAddedThisConversation = true; // Track to avoid multiple additions per conversation
        }

        // SPECIAL LETTER CARD INTEGRATION: Add special letter request cards when token thresholds are met
        AddSpecialLetterRequestCardsToDeck(context.TargetNPC, tokenDict);

        // Draw 5 cards from NPC's deck filtered by emotional state and played cards - already ConversationChoice objects
        List<ConversationChoice> choices = context.TargetNPC.ConversationDeck.DrawCards(tokenDict, 0, emotionalState, state);
        
        // Update mechanical descriptions for the drawn choices
        foreach (ConversationChoice choice in choices)
        {
            choice.MechanicalDescription = GetRichMechanicalDescription(choice, context.TargetNPC);
            choice.IsAffordable = true; // Will be set based on patience in conversation
            // choice.IsAvailable and choice.ChoiceType are already set correctly by the deck
        }

        // LETTER DELIVERY: Check if player has a letter for this NPC in position 1
        DeliveryObligation letterInPosition1 = _queueManager.GetLetterAt(1);
        if (letterInPosition1 != null && 
            letterInPosition1.RecipientName.Equals(context.TargetNPC.Name, StringComparison.OrdinalIgnoreCase))
        {
            // Add delivery choice at the beginning of the list
            ConversationChoice deliveryChoice = CreateDeliveryChoice(letterInPosition1, context.TargetNPC);
            choices.Insert(0, deliveryChoice);
        }

        return choices;
    }

    /// <summary>
    /// Create a delivery choice for a letter in position 1
    /// </summary>
    private ConversationChoice CreateDeliveryChoice(DeliveryObligation letter, NPC recipient)
    {
        // Calculate trust reward based on letter urgency
        int trustReward = 3; // Base trust for keeping your word
        if (letter.DeadlineInMinutes < 24) trustReward = 4;
        if (letter.DeadlineInMinutes < 12) trustReward = 5;

        var deliveryEffect = new DeliverLetterEffect(
            letter.Id,
            letter,
            _queueManager,
            _timeManager,
            _tokenManager
        );

        return new ConversationChoice
        {
            ChoiceID = "deliver_letter",
            NarrativeText = $"\"I have a letter for you from {letter.SenderName}.\"",
            PatienceCost = 0, // Delivering a letter doesn't cost patience
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = $"Deliver letter | +{letter.Payment} coins | +{trustReward} Trust (kept promise)",
            ComfortGain = 2, // Delivering a letter builds some comfort
            Difficulty = 1, // Easy action
            ChoiceType = ConversationChoiceType.Deliver,
            Category = RelationshipCardCategory.Basic, // Delivery is a basic action
            MechanicalEffects = new List<IMechanicalEffect> { deliveryEffect }
        };
    }

    /// <summary>
    /// Add letter request card to NPC's deck when comfort is built
    /// Card persists in deck until successfully played
    /// </summary>
    private void AddLetterRequestCardToDeck(NPC npc, ConversationState state)
    {
        // Get current relationship tokens to determine which letter type to offer
        Dictionary<ConnectionType, int> tokenDict = _tokenManager.GetTokensWithNPC(npc.ID);
        
        // Determine primary relationship type for letter request card
        ConnectionType offerType = GetHighestRelationshipType(tokenDict);
        int relationshipLevel = tokenDict.ContainsKey(offerType) ? tokenDict[offerType] : 0;
        
        // Add letter request card to NPC's deck if not already present
        // This card will persist in their deck until successfully played
        if (npc.ConversationDeck != null && !npc.ConversationDeck.HasLetterRequestCard(offerType))
        {
            npc.ConversationDeck.AddLetterRequestCard(offerType, relationshipLevel);
            
            // Add narrative feedback that letter opportunity has emerged
            _gameWorld.SystemMessages.Add(new SystemMessage(
                $"Your growing relationship with {npc.Name} opens new possibilities...",
                SystemMessageTypes.Success
            ));
        }
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
    /// Add special letter request cards to NPC deck when token thresholds are met (5+ tokens)
    /// Only supports IntroductionDeliveryObligation (Trust) and AccessPermit (Commerce)
    /// </summary>
    private void AddSpecialLetterRequestCardsToDeck(NPC npc, Dictionary<ConnectionType, int> tokenDict)
    {
        if (npc.ConversationDeck == null) return;

        // Only check Trust and Commerce tokens for special letters
        ConnectionType[] supportedTypes = { ConnectionType.Trust, ConnectionType.Commerce };
        
        foreach (ConnectionType tokenType in supportedTypes)
        {
            int tokenCount = tokenDict.GetValueOrDefault(tokenType, 0);
            
            // Add special letter request card if meets threshold and not already present
            if (tokenCount >= 5)
            {
                npc.ConversationDeck.AddSpecialLetterRequestCard(tokenType, tokenCount);
            }
        }
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
    private string GetRichMechanicalDescription(ConversationChoice choice, NPC npc)
    {
        List<string> parts = new List<string>();

        // Add comfort gain/loss (this is a choice property, not hardcoded)
        if (choice.ComfortGain > 0)
        {
            parts.Add($"+{choice.ComfortGain} comfort");
        }
        else if (choice.ComfortGain < 0)
        {
            parts.Add($"{choice.ComfortGain} comfort");
        }

        // Add difficulty indication (derived from choice property)
        if (choice.Difficulty > 6)
        {
            parts.Add("High difficulty");
        }
        else if (choice.Difficulty < 3)
        {
            parts.Add("Easy approach");
        }

        // Add special effects based on choice category (categorical)
        if (choice.Category == RelationshipCardCategory.Crisis)
        {
            parts.Add("Emergency option");
        }

        // Read from actual mechanical effects
        if (choice.MechanicalEffects != null && choice.MechanicalEffects.Any())
        {
            foreach (IMechanicalEffect effect in choice.MechanicalEffects)
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