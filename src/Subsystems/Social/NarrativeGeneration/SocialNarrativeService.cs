/// <summary>
/// Integration service that bridges the game's existing conversation system 
/// with the new narrative generation system. Converts between game models
/// and narrative models, manages provider selection, and handles fallback scenarios.
/// </summary>
public class SocialNarrativeService
{
    private readonly NarrativeProviderFactory _providerFactory;

    public SocialNarrativeService(
        NarrativeProviderFactory providerFactory)
    {
        _providerFactory = providerFactory;
    }

    /// <summary>
    /// Phase 1 only: Generates NPC dialogue without card narratives.
    /// For progressive UI updates - show NPC dialogue first.
    /// </summary>
    public async Task<NarrativeOutput> GenerateOnlyNPCDialogueAsync(
        SocialSession session,
        NPC npc,
        List<CardInstance> activeCards)
    {
        try
        {
            // Get provider from factory
            INarrativeProvider provider = await _providerFactory.GetProviderAsync();
            // Convert game models to narrative models
            SocialChallengeState conversationState = BuildConversationState(session);
            NPCData npcData = BuildNPCData(npc);
            CardCollection cardCollection = BuildCardCollection(activeCards);

            // Generate NPC dialogue only (Phase 1)
            NarrativeOutput narrativeOutput = await provider.GenerateNPCDialogueAsync(
                conversationState,
                npcData,
                cardCollection);

            // Add provider source for UI styling
            narrativeOutput.ProviderSource = provider.GetProviderType();

            return narrativeOutput;
        }
        catch
        {
            // If narrative generation fails, provide minimal fallback
            return CreateMinimalFallbackNarrative(session, npc, activeCards);
        }
    }

    /// <summary>
    /// Phase 2 only: Generates card narratives based on existing NPC dialogue.
    /// For progressive UI updates - show card narratives after NPC dialogue.
    /// </summary>
    public async Task<List<CardNarrative>> GenerateOnlyCardNarrativesAsync(
        SocialSession session,
        NPC npc,
        List<CardInstance> activeCards,
        string npcDialogue)
    {
        try
        {
            // Get provider from factory
            INarrativeProvider provider = await _providerFactory.GetProviderAsync();
            // Convert game models to narrative models
            SocialChallengeState conversationState = BuildConversationState(session);
            NPCData npcData = BuildNPCData(npc);
            CardCollection cardCollection = BuildCardCollection(activeCards);

            // Generate card narratives only (Phase 2)
            List<CardNarrative> cardNarratives = await provider.GenerateCardNarrativesAsync(
                conversationState,
                npcData,
                cardCollection,
                npcDialogue);

            return cardNarratives ?? new List<CardNarrative>();
        }
        catch
        {
            // If narrative generation fails, provide empty list
            return new List<CardNarrative>();
        }
    }

    /// <summary>
    /// Generates narrative content for current conversation state using available providers.
    /// Converts game models to narrative models, calls provider, and handles fallback.
    /// This does BOTH phases at once for backward compatibility.
    /// </summary>
    /// <param name="session">Current conversation session</param>
    /// <param name="npc">NPC being conversed with</param>
    /// <param name="activeCards">List of cards currently available to player</param>
    /// <returns>Generated narrative output or fallback content</returns>
    public async Task<NarrativeOutput> GenerateNarrativeAsync(
        SocialSession session,
        NPC npc,
        List<CardInstance> activeCards)
    {
        try
        {
            // Get provider from factory - properly await async operation
            INarrativeProvider provider = await _providerFactory.GetProviderAsync();
            // Convert game models to narrative models
            SocialChallengeState conversationState = BuildConversationState(session);
            NPCData npcData = BuildNPCData(npc);
            CardCollection cardCollection = BuildCardCollection(activeCards);

            // Generate NPC dialogue first
            NarrativeOutput narrativeOutput = await provider.GenerateNPCDialogueAsync(
                conversationState,
                npcData,
                cardCollection);

            // Then generate card narratives using the NPC dialogue
            List<CardNarrative> cardNarratives = await provider.GenerateCardNarrativesAsync(
                conversationState,
                npcData,
                cardCollection,
                narrativeOutput.NPCDialogue);

            // Add card narratives to output
            narrativeOutput.CardNarratives = cardNarratives;

            // Add provider source for UI styling
            narrativeOutput.ProviderSource = provider.GetProviderType();

            return narrativeOutput;
        }
        catch
        {
            // If narrative generation fails completely, provide minimal fallback
            // This ensures the game continues functioning even if narrative systems fail
            return CreateMinimalFallbackNarrative(session, npc, activeCards);
        }
    }

    /// <summary>
    /// Creates minimal fallback narrative when all providers fail.
    /// Ensures game continues functioning with basic descriptive text.
    /// </summary>
    private NarrativeOutput CreateMinimalFallbackNarrative(
        SocialSession session,
        NPC npc,
        List<CardInstance> activeCards)
    {
        NarrativeOutput fallback = new NarrativeOutput
        {
            NPCDialogue = $"{npc.Name} looks at you expectantly.",
            NarrativeText = $"The conversation continues with {npc.Name}.",
            CardNarratives = new List<CardNarrative>()
        };

        // Generate basic card narratives
        foreach (CardInstance card in activeCards)
        {
            string basicNarrative = card.SocialCardTemplate.InitiativeCost <= 1 ? "Say something carefully" :
                                   card.SocialCardTemplate.InitiativeCost >= 3 ? "Speak boldly" :
                                   "Continue the conversation";
            fallback.CardNarratives.Add(new CardNarrative
            {
                CardId = card.SocialCardTemplate.Id,
                NarrativeText = basicNarrative,
                ProviderSource = NarrativeProviderType.JsonFallback
            });
        }

        return fallback;
    }

    /// <summary>
    /// Converts ConversationSession to ConversationState for narrative generation.
    /// </summary>
    /// <param name="session">Game conversation session</param>
    /// <returns>Narrative conversation state</returns>
    private SocialChallengeState BuildConversationState(SocialSession session)
    {
        int momentum = session.CurrentMomentum;
        TopicLayer currentLayer = DetermineTopicLayer(momentum);

        return new SocialChallengeState
        {
            Flow = session.Cadence, // Cadence system (-10 to +10)
            Momentum = momentum, // Use momentum as rapport for narrative compatibility
            Focus = session.GetCurrentInitiative(),
            Doubt = session.MaxDoubt - session.CurrentDoubt, // Convert doubt to patience for narrative
            CurrentState = session.CurrentState,
            CurrentTopicLayer = currentLayer,
            HighestTopicLayerReached = currentLayer, // Will need session tracking for persistence
            TotalTurns = session.TurnNumber,
            ConversationHistory = BuildConversationHistory(session.TurnHistory)
        };
    }

    /// <summary>
    /// Converts conversation turn history to strings for AI narrative generation.
    /// </summary>
    /// <param name="turnHistory">List of conversation turns</param>
    /// <returns>List of formatted conversation history strings</returns>
    private List<string> BuildConversationHistory(List<SocialTurn> turnHistory)
    {
        List<string> history = new List<string>();

        foreach (SocialTurn turn in turnHistory)
        {
            // Format each turn as a readable conversation entry
            if (turn.ActionType == ActionType.Listen)
            {
                // Listen turns - show NPC dialogue
                if (!string.IsNullOrEmpty(turn.Narrative?.NPCDialogue))
                {
                    history.Add($"NPC: \"{turn.Narrative.NPCDialogue}\"");
                }
            }
            else if (turn.ActionType == ActionType.Speak)
            {
                // Speak turns - show what card was played
                if (turn.CardPlayed != null)
                {
                    string cardDescription = turn.CardPlayed.SocialCardTemplate.Title ?? turn.CardPlayed.SocialCardTemplate.Id;
                    history.Add($"Player: {cardDescription}");

                    // Also include NPC's response if available
                    if (!string.IsNullOrEmpty(turn.Narrative?.NPCDialogue))
                    {
                        history.Add($"NPC: \"{turn.Narrative.NPCDialogue}\"");
                    }
                }
            }
        }

        return history;
    }

    /// <summary>
    /// Determines the topic layer based on momentum level.
    /// </summary>
    /// <param name="momentum">Current momentum value</param>
    /// <returns>Appropriate topic layer</returns>
    private TopicLayer DetermineTopicLayer(int momentum)
    {
        if (momentum <= 5) return TopicLayer.Deflection;
        if (momentum <= 10) return TopicLayer.Gateway;
        return TopicLayer.Core;
    }

    /// <summary>
    /// Converts NPC to NPCData for narrative generation.
    /// </summary>
    /// <param name="npc">Game NPC instance</param>
    /// <returns>Narrative NPC data</returns>
    private NPCData BuildNPCData(NPC npc)
    {
        return new NPCData
        {
            // HIGHLANDER: NpcId deleted - Name is sufficient for AI narrative generation
            Name = npc.Name,
            Personality = npc.PersonalityType,
            CurrentCrisis = ExtractCurrentCrisis(npc),
            CurrentTopic = DetermineCurrentTopic(npc)
        };
    }

    /// <summary>
    /// Converts List<CardInstance> to CardCollection for narrative generation.
    /// </summary>
    /// <param name="cards">Game card instances</param>
    /// <returns>Narrative card collection</returns>
    private CardCollection BuildCardCollection(List<CardInstance> cards)
    {
        CardCollection collection = new CardCollection();

        foreach (CardInstance card in cards)
        {
            SocialCard template = card.SocialCardTemplate;
            CardEffectFormula formula = template.EffectFormula;

            CardInfo cardInfo = new CardInfo
            {
                Id = template.Id,
                InitiativeCost = template.InitiativeCost,
                Move = template.Move,
                BoundStat = template.BoundStat,
                Depth = template.Depth,
                PrimaryTargetResource = formula?.FormulaType == EffectFormulaType.Compound ? null : formula?.TargetResource,
                PrimaryFormulaType = formula?.FormulaType,
                IsCompound = formula?.FormulaType == EffectFormulaType.Compound,
                Persistence = DetermineCardPersistence(card),
                NarrativeCategory = DetermineNarrativeCategory(card),
                HasDrawEffect = DetermineHasDrawEffect(card),
                HasFocusEffect = DetermineHasFocusEffect(card)
            };

            collection.Cards.Add(cardInfo);
        }

        return collection;
    }

    /// <summary>
    /// Determines if card has a draw effect (draws additional cards).
    /// </summary>
    private bool DetermineHasDrawEffect(CardInstance card)
    {
        // Check if card's effect formula targets the Cards resource
        if (card.SocialCardTemplate.EffectFormula != null)
        {
            if (card.SocialCardTemplate.EffectFormula.TargetResource == SocialChallengeResourceType.Cards)
                return true;

            // Check compound effects
            if (card.SocialCardTemplate.EffectFormula.CompoundEffects != null)
            {
                foreach (CardEffectFormula effect in card.SocialCardTemplate.EffectFormula.CompoundEffects)
                {
                    if (effect.TargetResource == SocialChallengeResourceType.Cards)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Determines if card has a focus effect (manipulates initiative/momentum).
    /// </summary>
    private bool DetermineHasFocusEffect(CardInstance card)
    {
        // Check if card's effect formula targets Initiative or Momentum resources
        if (card.SocialCardTemplate.EffectFormula != null)
        {
            if (card.SocialCardTemplate.EffectFormula.TargetResource == SocialChallengeResourceType.Initiative ||
                card.SocialCardTemplate.EffectFormula.TargetResource == SocialChallengeResourceType.Momentum)
                return true;

            // Check compound effects
            if (card.SocialCardTemplate.EffectFormula.CompoundEffects != null)
            {
                foreach (CardEffectFormula effect in card.SocialCardTemplate.EffectFormula.CompoundEffects)
                {
                    if (effect.TargetResource == SocialChallengeResourceType.Initiative ||
                        effect.TargetResource == SocialChallengeResourceType.Momentum)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Extracts current crisis information from NPC data.
    /// </summary>
    /// <param name="npc">NPC to analyze</param>
    /// <returns>Crisis description or null if no crisis</returns>
    private string ExtractCurrentCrisis(NPC npc)
    {
        // Return crisis type as string for narrative generation
        // Crisis is set at parse-time or spawning, not detected via string matching
        if (npc.CurrentState == ConnectionState.DISCONNECTED && npc.Crisis != CrisisType.None)
        {
            return npc.Crisis switch
            {
                CrisisType.ForcedMarriage => "forced_marriage",
                CrisisType.FinancialTroubles => "financial_troubles",
                CrisisType.FamilyCrisis => "family_crisis",
                CrisisType.PersonalTroubles => "personal_troubles",
                _ => null
            };
        }

        return null;
    }

    /// <summary>
    /// Determines current conversation topic based on rapport and NPC state.
    /// </summary>
    /// <param name="npc">NPC to analyze</param>
    /// <returns>Current topic string</returns>
    private string DetermineCurrentTopic(NPC npc)
    {
        // Start with general topics, escalate based on connection state
        return npc.CurrentState switch
        {
            ConnectionState.DISCONNECTED => "personal_troubles",
            ConnectionState.GUARDED => "local_matters",
            ConnectionState.NEUTRAL => "general",
            ConnectionState.RECEPTIVE => "personal_interests",
            ConnectionState.TRUSTING => "deep_concerns",
            _ => "general"
        };
    }

    /// <summary>
    /// Maps CardInstance properties to CardPersistence for narrative timing.
    /// </summary>
    /// <param name="card">Card instance to analyze</param>
    /// <returns>Persistence type for narrative generation</returns>
    private PersistenceType DetermineCardPersistence(CardInstance card)
    {
        return card.Persistence;
    }

    /// <summary>
    /// Determines narrative category for backwards construction based on card properties and effects.
    /// </summary>
    /// <param name="card">Card instance to categorize</param>
    /// <returns>Narrative category string</returns>
    private NarrativeCategoryType DetermineNarrativeCategory(CardInstance card)
    {
        // Check for atmosphere effects (indicates risk/pressure cards)
        if (card.SocialCardTemplate.SuccessType == SuccessEffectType.None)
        {
            return NarrativeCategoryType.Atmosphere;
        }

        // DELETED: Difficulty-based risk assessment
        // Risk now assessed through card depth and Initiative cost

        // Card persistence-based categories
        if (card.Persistence == PersistenceType.Statement)
            return NarrativeCategoryType.Pressure;

        // Token type indicates support/connection building
        if (card.SocialCardTemplate.TokenType == ConnectionType.Trust)
            return NarrativeCategoryType.SupportTrust;
        if (card.SocialCardTemplate.TokenType == ConnectionType.Diplomacy)
            return NarrativeCategoryType.SupportDiplomacy;
        if (card.SocialCardTemplate.TokenType == ConnectionType.Status)
            return NarrativeCategoryType.SupportStatus;
        if (card.SocialCardTemplate.TokenType == ConnectionType.Shadow)
            return NarrativeCategoryType.SupportShadow;

        // Default
        return NarrativeCategoryType.Standard;
    }
}