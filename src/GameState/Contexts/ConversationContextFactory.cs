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
        ConversationType conversationType,
        NPC npc,
        ConversationSession session,
        List<CardInstance> observationCards,
        int attentionSpent,
        ResourceState playerResources,
        string locationName,
        string timeDisplay)
    {
        ConversationContextBase context = conversationType switch
        {
            // Commerce removed - exchanges use separate Exchange system
            ConversationType.Promise => new PromiseContext(),
            ConversationType.Delivery => new DeliveryContext(),
            ConversationType.FriendlyChat => new StandardContext(),
            ConversationType.Resolution => new ResolutionContext(),
            _ => new StandardContext() // Default to standard context
        };

        // Set common properties
        context.IsValid = true;
        context.NpcId = npc.ID;
        context.Npc = npc;
        context.Type = conversationType;
        context.InitialState = session?.InitialState ?? ConnectionState.NEUTRAL;
        context.Session = session;
        context.ObservationCards = observationCards ?? new List<CardInstance>();
        context.AttentionSpent = attentionSpent;
        context.PlayerResources = playerResources;
        context.LocationName = locationName;
        context.TimeDisplay = timeDisplay;

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
            // Commerce removed - exchanges use separate Exchange system
            case PromiseContext promiseContext:
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

    // REMOVED: InitializeCommerceContext deleted - exchanges use separate Exchange system
    
    private static void InitializePromiseContext(PromiseContext context, GameWorld gameWorld)
    {
        context.SetNPCPersonality(context.Npc.PersonalityType);
        context.GeneratesLetterOnSuccess = true;

        // Initialize promise-specific data if needed
        if (context.Npc?.RequestDeck != null)
        {
            // Check for promise cards in request deck
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

        // Set rapport goal based on connection state
        if (context.Session != null)
        {
            int rapportGoal = context.Session.InitialState switch
            {
                ConnectionState.DISCONNECTED => 15,
                ConnectionState.GUARDED => 20,
                ConnectionState.NEUTRAL => 25,
                ConnectionState.RECEPTIVE => 30,
                ConnectionState.TRUSTING => 35,
                _ => 25
            };
            context.SetRapportGoal(rapportGoal);
        }
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
            "attention" => ResourceType.Attention,
            _ => null
        };
    }
}