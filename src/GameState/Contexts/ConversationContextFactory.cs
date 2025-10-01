/// <summary>
/// Factory for creating type-specific conversation contexts
/// Eliminates Dictionary usage and provides strongly-typed contexts
/// </summary>
public static class ConversationContextFactory
{
    /// <summary>
    /// Create a typed conversation context based on conversation type
    /// </summary>
    public static ConversationContextBase CreateContext(
        string conversationTypeId,
        NPC npc,
        ConversationSession session,
        List<CardInstance> observationCards,
        ResourceState playerResources,
        string locationName,
        string timeDisplay)
    {
        // Create context based on conversation type ID
        ConversationContextBase context = conversationTypeId switch
        {
            "desperate_request" => new PromiseContext(),
            "trade_negotiation" => new PromiseContext(),
            "authority_challenge" => new PromiseContext(),
            "delivery" => new DeliveryContext(),
            "friendly_chat" => new StandardContext(),
            "information_gathering" => new StandardContext(),
            "intimate_confession" => new StandardContext(),
            "resolution" => new ResolutionContext(),
            _ => new StandardContext() // Default to standard context
        };

        // Set common properties
        context.IsValid = true;
        context.NpcId = npc.ID;
        context.Npc = npc;
        context.ConversationTypeId = conversationTypeId;
        context.InitialState = session?.InitialState ?? ConnectionState.NEUTRAL;
        context.Session = session;
        context.ObservationCards = observationCards ?? new List<CardInstance>();
        context.PlayerResources = playerResources;
        context.LocationName = locationName;
        context.TimeDisplay = timeDisplay;
        context.RequestText = session?.RequestText; // Pass request text from session

        return context;
    }

    /// <summary>
    /// Create an invalid context with error message
    /// </summary>
    public static ConversationContextBase CreateInvalidContext(string errorMessage)
    {
        return new StandardContext
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Initialize type-specific data for a context
    /// </summary>
    public static void InitializeContextData(ConversationContextBase context, GameWorld gameWorld, ObligationQueueManager queueManager)
    {
        switch (context)
        {
            // Diplomacy removed - exchanges use separate Exchange system
            case PromiseContext promiseContext: // Handles Request bundles
                InitializePromiseContext(promiseContext, gameWorld);
                break;
            case DeliveryContext deliveryContext:
                InitializeDeliveryContext(deliveryContext, queueManager);
                break;
            case StandardContext standardContext:
                InitializeStandardContext(standardContext);
                break;
            case ResolutionContext resolutionContext:
                InitializeResolutionContext(resolutionContext, queueManager);
                break;
        }
    }

    // REMOVED: InitializeDiplomacyContext deleted - exchanges use separate Exchange system

    private static void InitializePromiseContext(PromiseContext context, GameWorld gameWorld)
    {
        // PromiseContext now handles Request bundles (which contain promise cards)
        context.SetNPCPersonality(context.Npc.PersonalityType);
        context.GeneratesLetterOnSuccess = true;

        // Initialize request-specific data if needed
        if (context.Npc?.Requests != null && context.Npc.Requests.Count > 0)
        {
            // Request bundles contain both request and promise cards
            context.HasDeadline = false; // Will be set based on specific card data
        }
    }

    private static void InitializeDeliveryContext(DeliveryContext context, ObligationQueueManager queueManager)
    {
        if (queueManager != null)
        {
            DeliveryObligation[] activeObligations = queueManager.GetActiveObligations();
            context.LettersCarriedForNpc = activeObligations
                .Where(o => o.RecipientId == context.Npc.ID || o.RecipientName == context.Npc.Description)
                .ToList();

            if (context.LettersCarriedForNpc.Count > 0)
            {
                context.SetSelectedLetter(context.LettersCarriedForNpc.First());
            }
        }
    }

    private static void InitializeStandardContext(StandardContext context)
    {
        context.SetNPCPersonality(context.Npc.PersonalityType);

        // Rapport thresholds are now on individual cards, not a global goal
    }

    private static void InitializeResolutionContext(ResolutionContext context, ObligationQueueManager queueManager)
    {
        context.SetNPCPersonality(context.Npc.PersonalityType);

        // Load burden cards if available
        if (queueManager != null)
        {
            // Get burden cards from NPC's burden deck
            if (context.Npc?.BurdenDeck != null)
            {
                List<ConversationCard> burdenCards = context.Npc.BurdenDeck.GetAllCards();
                context.SetBurdenCards(burdenCards);
            }
            else
            {
                context.SetBurdenCards(new List<ConversationCard>());
            }
        }
    }

    private static ExchangeData ConvertExchangeDTO(ExchangeDTO dto)
    {
        ExchangeData exchangeData = new ExchangeData
        {
            Id = dto.Id,
            Name = dto.Name,
            ExchangeName = dto.Name,
            Description = "",
            Costs = new List<ResourceAmount>(),
            Rewards = new List<ResourceAmount>()
        };

        // Convert costs
        ResourceType? costType = ParseResourceType(dto.GiveCurrency);
        if (costType.HasValue)
        {
            exchangeData.Costs.Add(new ResourceAmount(costType.Value, dto.GiveAmount));
        }

        // Convert rewards
        ResourceType? rewardType = ParseResourceType(dto.ReceiveCurrency);
        if (rewardType.HasValue)
        {
            exchangeData.Rewards.Add(new ResourceAmount(rewardType.Value, dto.ReceiveAmount));
        }

        return exchangeData;
    }

    // REMOVED: ConvertExchangeCard deleted - exchange conversion handled in Exchange subsystem

    private static ResourceType? ParseResourceType(string name)
    {
        return name?.ToLower() switch
        {
            "coins" => ResourceType.Coins,
            "health" => ResourceType.Health,
            "food" => ResourceType.Hunger,
            "hunger" => ResourceType.Hunger,
            _ => null
        };
    }
}