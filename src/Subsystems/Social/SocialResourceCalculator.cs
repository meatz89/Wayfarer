/// <summary>
/// Calculates and applies resource changes during social challenges.
/// Extracted from SocialFacade for file size compliance (COMPOSITION pattern).
/// Handles cadence, doubt, momentum, initiative, and card playability calculations.
/// </summary>
public class SocialResourceCalculator
{
    private readonly GameWorld _gameWorld;
    private readonly SocialEffectResolver _effectResolver;
    private readonly MessageSystem _messageSystem;

    public SocialResourceCalculator(
        GameWorld gameWorld,
        SocialEffectResolver effectResolver,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _effectResolver = effectResolver;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Apply cadence change using DUAL BALANCE SYSTEM
    /// DUAL BALANCE: Action type (SPEAK = +1) + Card Delivery property
    /// Standard: +1, Commanding: +2, Measured: +0, Yielding: -1
    /// Total: SPEAK (+1) + Delivery = combined Cadence change
    /// </summary>
    public void ApplyCadenceFromDelivery(CardInstance card, SocialSession session)
    {
        // DUAL BALANCE SYSTEM:
        // 1. Action-based balance (SPEAK action)
        int actionBalance = +1; // SPEAK action always +1

        // 2. Delivery-based balance (card property)
        int deliveryBalance = card.SocialCardTemplate.Delivery switch
        {
            DeliveryType.Yielding => -1,
            DeliveryType.Measured => 0,
            DeliveryType.Standard => +1,
            DeliveryType.Commanding => +2,
            _ => +1 // Default to Standard
        };

        // Combine both balance effects
        int totalCadenceChange = actionBalance + deliveryBalance;

        session.Cadence = Math.Clamp(session.Cadence + totalCadenceChange, -10, 10);
    }

    /// <summary>
    /// Process Cadence effects on LISTEN action - NEW REFACTORED SYSTEM
    /// 1. Calculate doubt to clear
    /// 2. Reset doubt to 0
    /// 3. Reduce momentum by doubt cleared
    /// 4. Check and unlock tiers (momentum may have changed)
    /// 5. Convert positive cadence to doubt
    /// BEFORE card draw calculation (cadence reduction happens AFTER draw)
    /// </summary>
    public void ProcessCadenceEffectsOnListen(SocialSession session)
    {
        // NEW REFACTORED LISTEN MECHANICS (Per Spec lines 862-896):
        // 1. Calculate doubt that will be cleared
        int doubtCleared = session.CurrentDoubt;

        // 2. Reset doubt to 0 (complete relief)
        session.CurrentDoubt = 0;

        // 3. Reduce MOMENTUM by amount of doubt cleared (minimum 0)
        // CRITICAL: Understanding is NOT reduced - it persists through LISTEN
        session.CurrentMomentum = Math.Max(0, session.CurrentMomentum - doubtCleared);

        // 4. Check tier unlocks (uses Understanding, NOT Momentum)
        // Tiers are based on Understanding thresholds (6/12/18), not Momentum
        // Understanding is NOT reduced during LISTEN, so tiers stay unlocked
        session.CheckAndUnlockTiers();

        // 5. Convert positive cadence to doubt (CRITICAL: Check for conversation death)
        if (session.Cadence > 0)
        {
            int cadenceToDoubt = session.Cadence;
            session.CurrentDoubt = Math.Min(session.MaxDoubt, session.CurrentDoubt + cadenceToDoubt);
            // CRITICAL: If doubt reaches max (10), conversation ends immediately
            // This is the "cadence trap" - listening while dominating can end the conversation
            if (session.CurrentDoubt >= session.MaxDoubt)
            {
                // Conversation will end - ExecuteListen will detect this in ShouldEnd() check
            }
        }

        // 6. Cadence reduction by -1 happens AFTER draw calculation (NOT here)
        // This is handled in ReduceCadenceAfterDraw() method called after ExecuteNewListenCardDraw
    }

    /// <summary>
    /// Apply LISTEN action-type balance AFTER card draw (step 7 of LISTEN sequence)
    /// DUAL BALANCE: LISTEN action = -2 cadence (action-type balance, no card played so no Delivery)
    /// </summary>
    public void ReduceCadenceAfterDraw(SocialSession session)
    {
        // DUAL BALANCE SYSTEM: LISTEN action contributes -2 to Cadence
        session.Cadence = Math.Max(-10, session.Cadence - 2);
    }

    /// <summary>
    /// Handle card persistence after playing
    /// Standard: Goes to Spoken pile
    /// Echo: Returns to hand if conditions met
    /// Persistent: Stays in hand
    /// Banish: Removed entirely
    /// </summary>
    public void ProcessCardPersistence(SocialSession session)
    {
        // Handle cards that need persistence processing
        // This is handled by the deck system based on card persistence types
        session.Deck.ProcessCardPersistence();
    }

    /// <summary>
    /// Check if situation cards should become active based on momentum thresholds
    /// Basic: 8, Enhanced: 12, Premium: 16
    /// </summary>
    public void CheckSituationCardActivation(SocialSession session)
    {
        int currentMomentum = session.CurrentMomentum;

        // Move request cards that meet momentum threshold from request pile to hand
        List<CardInstance> activatedCards = session.Deck.CheckRequestThresholds(currentMomentum);

        foreach (CardInstance card in activatedCards)
        {
            _messageSystem.AddSystemMessage(
                $"{card.SocialCardTemplate.Title} is now available (Momentum threshold met)",
                SystemMessageTypes.Success, null);
        }
    }

    /// <summary>
    /// Execute card draw with tier-based filtering
    /// </summary>
    public List<CardInstance> ExecuteNewListenCardDraw(SocialSession session, int cardsToDraw)
    {
        // Draw with tier and stat filtering
        Player player = _gameWorld.GetPlayer();
        session.Deck.DrawToHand(cardsToDraw, session, player);

        // Return the newly drawn cards (last N cards in hand)
        return session.Deck.HandCards.TakeLast(cardsToDraw).ToList();
    }

    /// <summary>
    /// Update card playability based on Initiative system and Statement requirements
    /// </summary>
    public void UpdateCardPlayabilityForInitiative(SocialSession session)
    {
        int currentInitiative = session.CurrentInitiative;

        foreach (CardInstance card in session.Deck.HandCards)
        {
            // Skip request cards - their playability is based on momentum thresholds
            if (card.CardType == CardTypes.Situation)
            {
                continue;
            }

            // Check if player can afford this card's Initiative cost
            int initiativeCost = GetCardInitiativeCost(card);
            bool canAffordInitiative = currentInitiative >= initiativeCost;

            // Check if Statement requirements are met
            bool meetsStatementRequirements = card.SocialCardTemplate.MeetsStatementRequirements(session);

            // Card is playable if BOTH conditions are met
            card.IsPlayable = canAffordInitiative && meetsStatementRequirements;
        }
    }

    /// <summary>
    /// Get Initiative cost for a card (replaces Focus cost)
    /// FAIL FAST: Situation cards have no SocialCardTemplate (cost is 0), regular cards MUST have template
    /// </summary>
    public int GetCardInitiativeCost(CardInstance card)
    {
        // Situation cards have no SocialCardTemplate - their cost is always 0
        if (card.CardType == CardTypes.Situation)
            return 0;

        // All non-Situation cards MUST have SocialCardTemplate
        if (card.SocialCardTemplate == null)
            throw new InvalidOperationException($"Card {card.InstanceId} is missing required SocialCardTemplate");

        return card.SocialCardTemplate.InitiativeCost;
    }

    /// <summary>
    /// Validate personality rules for Initiative system
    /// Proud: Ascending Initiative order (not Focus)
    /// Mercantile: Highest Initiative card gets +30% success
    /// </summary>
    public bool ValidateInitiativePersonalityRules(PersonalityRuleEnforcer personalityEnforcer, CardInstance selectedCard, out string violationMessage)
    {
        violationMessage = string.Empty;

        // Use existing personality enforcer but it will need updating for Initiative
        return personalityEnforcer.ValidatePlay(selectedCard, out violationMessage);
    }

    /// <summary>
    /// Calculate card success with Initiative system
    /// Base% + (2% × Current Momentum) + (10% × Bound Stat Level)
    /// </summary>
    public bool CalculateInitiativeCardSuccess(CardInstance selectedCard, SocialSession session)
    {
        // Use existing deterministic success calculation for now
        // This handles momentum and stat bonuses correctly
        return _effectResolver.CheckCardSuccess(selectedCard, session);
    }

    /// <summary>
    /// Process card play results with Initiative system
    /// PROJECTION PRINCIPLE: Get projection from resolver, then apply to session
    /// </summary>
    public CardPlayResult ProcessInitiativeCardPlay(CardInstance selectedCard, bool success, SocialSession session)
    {
        if (success)
        {
            // Get projection from resolver (single source of truth)
            CardEffectResult projection = _effectResolver.ProcessSuccessEffect(selectedCard, session);

            // Apply projection to session state
            ApplyProjectionToSession(projection, session);
        }

        // Check if card ends conversation (Request, Promise, Burden cards)
        bool endsConversation = selectedCard.CardType == CardTypes.Situation;

        // Create play result
        return new CardPlayResult
        {
            Results = new List<SingleCardResult>
            {
                new SingleCardResult
                {
                    Card = selectedCard,
                    Success = success,
                    Flow = 0, // No flow
                    Roll = 0, // Deterministic system
                    SuccessChance = success ? 100 : 0
                }
            },
            MomentumGenerated = 0, // No flow
            EndsConversation = endsConversation // Request cards end conversation
        };
    }

    /// <summary>
    /// Apply a projection result to actual session state
    /// PROJECTION PRINCIPLE: This is the ONLY place where projections become reality
    /// </summary>
    public void ApplyProjectionToSession(CardEffectResult projection, SocialSession session)
    {
        // Apply Initiative changes
        if (projection.InitiativeChange != 0)
        {
            session.AddInitiative(projection.InitiativeChange);
        }

        // Apply Momentum changes (NO TIER UNLOCKS - that's Understanding's job)
        if (projection.MomentumChange != 0)
        {
            session.CurrentMomentum = Math.Max(0, session.CurrentMomentum + projection.MomentumChange);
        }

        // Apply Understanding changes (TRIGGERS TIER UNLOCKS)
        if (projection.UnderstandingChange != 0)
        {
            session.AddUnderstanding(projection.UnderstandingChange);
            // Tier unlocks happen inside AddUnderstanding via CheckAndUnlockTiers()
        }

        // Apply Doubt changes
        if (projection.DoubtChange > 0)
        {
            session.AddDoubt(projection.DoubtChange);
        }
        else if (projection.DoubtChange < 0)
        {
            session.ReduceDoubt(-projection.DoubtChange);
        }

        // Apply Cadence changes
        if (projection.CadenceChange != 0)
        {
            session.Cadence = Math.Clamp(session.Cadence + projection.CadenceChange, -10, 10);
        }

        // Apply card draw
        if (projection.CardsToDraw > 0)
        {
            Player player = _gameWorld.GetPlayer();
            session.Deck.DrawToHand(projection.CardsToDraw, session, player);
        }

        // Add any specific card instances (legacy support)
        if (projection.CardsToAdd != null && projection.CardsToAdd.Any())
        {
            session.Deck.AddCardsToMind(projection.CardsToAdd);
        }
    }

    /// <summary>
    /// Process card after playing based on persistence type
    /// </summary>
    public void ProcessCardAfterPlay(CardInstance selectedCard, bool success, SocialSession session)
    {
        // Handle card based on its persistence type
        session.Deck.PlayCard(selectedCard);
    }

    /// <summary>
    /// Calculate XP amount based on conversation difficulty
    /// </summary>
    public int CalculateXPAmount(SocialSession session)
    {
        if (session.IsStrangerConversation && session.StrangerLevel.HasValue)
        {
            return session.StrangerLevel.Value; // 1-3x XP
        }
        else if (session.NPC != null)
        {
            return session.NPC.ConversationDifficulty; // 1-3x XP
        }

        return 1; // Base XP
    }
}
