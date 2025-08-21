using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enriches conversation cards with categorical context.
/// NO TEXT GENERATION - only adds context objects to cards.
/// Frontend will use this context to generate appropriate text.
/// </summary>
public class CardContextGenerator
{
    private readonly NPCStateResolver _stateResolver;
    private readonly ObligationQueueManager _queueManager;
    private readonly NPCRelationshipTracker _relationshipTracker;
    private readonly ITimeManager _timeManager;
    private readonly ObservationSystem _observationSystem;

    public CardContextGenerator(
        NPCStateResolver stateResolver,
        ObligationQueueManager queueManager,
        NPCRelationshipTracker relationshipTracker,
        ITimeManager timeManager,
        ObservationSystem observationSystem)
    {
        _stateResolver = stateResolver;
        _queueManager = queueManager;
        _relationshipTracker = relationshipTracker;
        _timeManager = timeManager;
        _observationSystem = observationSystem;
    }

    /// <summary>
    /// Enrich a card with full categorical context
    /// </summary>
    public void EnrichCard(ConversationCard card, NPC npc, EmotionalState conversationState)
    {
        // Get NPC's emotional condition
        NPCEmotionalState npcState = _stateResolver.CalculateState(npc);
        
        // Get urgency context
        UrgencyContext urgency = CalculateUrgencyContext(npc);
        
        // Get relationship context
        RelationshipContext relationship = GetRelationshipContext(npc);
        
        // Determine card tone based on all factors
        CardTone tone = DetermineCardTone(card.Template, npcState, conversationState);
        
        // Calculate emotional weight
        EmotionalWeight emotionalWeight = CalculateEmotionalWeight(card, npcState);
        
        // Context must be set during card creation since it's init-only
        // This enrichment method should be called BEFORE card creation
        // TODO: Refactor to pass context during card creation
        /*
        card.Context = new CardContext
        {
            Personality = npc.PersonalityType,
            EmotionalState = conversationState,
            UrgencyLevel = (int)urgency.Level,
            HasDeadline = urgency.HoursRemaining.HasValue,
            MinutesUntilDeadline = urgency.HoursRemaining * 60,
            ObservationType = card.IsObservation ? ObservationType.Normal : null
        };
        */
    }

    /// <summary>
    /// Convert an observation into a conversation card
    /// </summary>
    public ConversationCard ConvertObservationToCard(ObservableViewModel observation, NPC targetNPC)
    {
        // Map observation type to card template
        CardTemplateType template = MapObservationToTemplate(observation.Type, targetNPC.PersonalityType);
        
        // Determine card properties based on observation importance
        CardProperties properties = DetermineCardProperties(observation);
        
        // Create the card
        var card = new ConversationCard
        {
            Template = template,
            Type = CardType.Trust, // Default to Trust for now
            Persistence = PersistenceType.Opportunity, // Observations are always fleeting
            Weight = properties.Weight,
            BaseComfort = properties.ComfortValue,
            IsObservation = true,
            ObservationSource = MapObservationSource(observation),
            
            // State change properties based on observation type
            IsStateCard = properties.CanChangeState,
            SuccessState = properties.SuccessState,
            FailureState = properties.FailureState
        };
        
        // Enrich with context
        EnrichCard(card, targetNPC, EmotionalState.NEUTRAL);
        
        return card;
    }

    /// <summary>
    /// Generate crisis cards for desperate situations
    /// </summary>
    public ConversationCard GenerateCrisisCard(NPC npc, EmotionalState state)
    {
        NPCEmotionalState npcState = _stateResolver.CalculateState(npc);
        
        // Determine crisis type based on NPC state and personality
        CrisisCardType crisisType = DetermineCrisisType(npcState, npc.PersonalityType);
        
        // Map to appropriate template
        CardTemplateType template = MapCrisisToTemplate(crisisType);
        
        var card = new ConversationCard
        {
            Template = template,
            Type = CardType.Trust, // Crisis cards usually Trust-based
            Persistence = PersistenceType.Crisis,
            Weight = 5, // Heavy but free in DESPERATE
            BaseComfort = 8, // High reward if successful
            IsCrisis = true,
            
            // Crisis cards can generate letters or obligations
            CanDeliverLetter = crisisType == CrisisCardType.DesperatePromise,
            ManipulatesObligations = crisisType == CrisisCardType.EmergencyIntervention,
            
            // State effects
            IsStateCard = true,
            SuccessState = EmotionalState.NEUTRAL, // Crisis resolved, return to neutral
            FailureState = EmotionalState.HOSTILE
        };
        
        // Enrich with crisis context
        EnrichCard(card, npc, state);
        
        return card;
    }

    private UrgencyContext CalculateUrgencyContext(NPC npc)
    {
        var queue = _queueManager.GetActiveObligations();
        var npcLetters = queue.Where(l => l.SenderId == npc.ID || l.SenderName == npc.Name).ToList();
        
        if (!npcLetters.Any())
        {
            return new UrgencyContext 
            { 
                Level = UrgencyLevel.None,
                Stakes = null,
                HoursRemaining = null
            };
        }
        
        var mostUrgent = npcLetters.OrderBy(l => l.DeadlineInMinutes).First();
        
        UrgencyLevel level = mostUrgent.DeadlineInMinutes switch
        {
            <= 2 => UrgencyLevel.Critical,
            <= 6 => UrgencyLevel.High,
            <= 12 => UrgencyLevel.Moderate,
            _ => UrgencyLevel.Low
        };
        
        return new UrgencyContext
        {
            Level = level,
            Stakes = mostUrgent.Stakes,
            HoursRemaining = mostUrgent.DeadlineInMinutes
        };
    }

    private RelationshipContext GetRelationshipContext(NPC npc)
    {
        var relationship = _relationshipTracker.GetRelationship(npc.ID);
        
        // Find dominant token type
        ConnectionType dominant = ConnectionType.Trust;
        int maxTokens = relationship.Trust;
        
        if (relationship.Commerce > maxTokens)
        {
            dominant = ConnectionType.Commerce;
            maxTokens = relationship.Commerce;
        }
        if (relationship.Status > maxTokens)
        {
            dominant = ConnectionType.Status;
            maxTokens = relationship.Status;
        }
        if (relationship.Shadow > maxTokens)
        {
            dominant = ConnectionType.Shadow;
            maxTokens = relationship.Shadow;
        }
        
        // Calculate relationship level
        int totalTokens = relationship.Trust + relationship.Commerce + 
                         relationship.Status + relationship.Shadow;
        
        RelationshipLevel level = totalTokens switch
        {
            >= 15 => RelationshipLevel.Intimate,
            >= 10 => RelationshipLevel.Close,
            >= 5 => RelationshipLevel.Friendly,
            >= 1 => RelationshipLevel.Acquaintance,
            _ => RelationshipLevel.Stranger
        };
        
        return new RelationshipContext
        {
            Level = level,
            DominantType = dominant,
            TotalTokens = totalTokens
        };
    }

    private CardTone DetermineCardTone(CardTemplateType template, NPCEmotionalState npcState, EmotionalState convState)
    {
        // Crisis states override
        if (npcState == NPCEmotionalState.DESPERATE || convState == EmotionalState.DESPERATE)
            return CardTone.Desperate;
        
        // Template-based tones
        if (IsComfortingTemplate(template))
            return CardTone.Supportive;
        
        if (IsQuestioningTemplate(template))
            return CardTone.Inquisitive;
        
        if (IsPersonalTemplate(template))
            return CardTone.Intimate;
        
        // State-based tones
        if (convState == EmotionalState.TENSE)
            return CardTone.Cautious;
        
        if (convState == EmotionalState.CONNECTED)
            return CardTone.Warm;
        
        return CardTone.Neutral;
    }

    private EmotionalWeight CalculateEmotionalWeight(ConversationCard card, NPCEmotionalState npcState)
    {
        // Crisis cards are always heavy
        if (card.IsCrisis)
            return EmotionalWeight.CRITICAL;
        
        // Based on card weight and state
        if (card.Weight >= 3)
            return EmotionalWeight.HIGH;
        
        if (card.Weight == 2)
        {
            if (npcState == NPCEmotionalState.ANXIOUS || npcState == NPCEmotionalState.DESPERATE)
                return EmotionalWeight.HIGH;
            return EmotionalWeight.MEDIUM;
        }
        
        if (card.Weight == 1)
            return EmotionalWeight.LOW;
        
        return EmotionalWeight.LOW; // No TRIVIAL value, use LOW
    }

    private CardTemplateType MapObservationToTemplate(ObservationType type, PersonalityType personality)
    {
        return (type, personality) switch
        {
            (ObservationType.Important, PersonalityType.DEVOTED) => CardTemplateType.ShareUrgentNews,
            (ObservationType.Important, PersonalityType.MERCANTILE) => CardTemplateType.MentionOpportunity,
            (ObservationType.Important, _) => CardTemplateType.ShareInformation,
            
            (ObservationType.Mystery, PersonalityType.CUNNING) => CardTemplateType.HintAtSecret,
            (ObservationType.Mystery, _) => CardTemplateType.ExpressCuriosity,
            
            (ObservationType.Opportunity, PersonalityType.MERCANTILE) => CardTemplateType.ProposeDeal,
            (ObservationType.Opportunity, _) => CardTemplateType.SuggestAction,
            
            _ => CardTemplateType.MakeCasualObservation
        };
    }

    private CardProperties DetermineCardProperties(ObservableViewModel observation)
    {
        var properties = new CardProperties();
        
        switch (observation.Type)
        {
            case ObservationType.Important:
                properties.Type = ConnectionType.Trust;
                properties.Weight = 2;
                properties.ComfortValue = 3;
                properties.CanChangeState = true;
                properties.SuccessState = EmotionalState.TENSE; // Important news creates tension
                break;
                
            case ObservationType.Mystery:
                properties.Type = ConnectionType.Shadow;
                properties.Weight = 1;
                properties.ComfortValue = 2;
                properties.CanChangeState = false;
                break;
                
            case ObservationType.Opportunity:
                properties.Type = ConnectionType.Commerce;
                properties.Weight = 2;
                properties.ComfortValue = 4;
                properties.CanChangeState = true;
                properties.SuccessState = EmotionalState.EAGER; // Opportunities create eagerness
                break;
                
            default:
                properties.Type = ConnectionType.Trust;
                properties.Weight = 1;
                properties.ComfortValue = 1;
                properties.CanChangeState = false;
                break;
        }
        
        return properties;
    }

    private string MapObservationSource(ObservableViewModel observation)
    {
        // This would map to actual location
        // For now, return generic source
        return observation.Type switch
        {
            ObservationType.Important => "Urgent Observation",
            ObservationType.Mystery => "Mysterious Discovery",
            ObservationType.Opportunity => "Noticed Opportunity",
            _ => "Casual Observation"
        };
    }

    private CrisisCardType DetermineCrisisType(NPCEmotionalState state, PersonalityType personality)
    {
        return (state, personality) switch
        {
            (NPCEmotionalState.DESPERATE, PersonalityType.DEVOTED) => CrisisCardType.DesperatePromise,
            (NPCEmotionalState.DESPERATE, PersonalityType.MERCANTILE) => CrisisCardType.LastMinuteDeal,
            (NPCEmotionalState.DESPERATE, _) => CrisisCardType.EmergencyIntervention,
            (NPCEmotionalState.HOSTILE, _) => CrisisCardType.DefuseConfrontation,
            _ => CrisisCardType.UrgentAction
        };
    }

    private CardTemplateType MapCrisisToTemplate(CrisisCardType crisisType)
    {
        return crisisType switch
        {
            CrisisCardType.DesperatePromise => CardTemplateType.MakeDesperatePromise,
            CrisisCardType.LastMinuteDeal => CardTemplateType.OfferEverything,
            CrisisCardType.EmergencyIntervention => CardTemplateType.TakeImmediateAction,
            CrisisCardType.DefuseConfrontation => CardTemplateType.CalmTheSituation,
            _ => CardTemplateType.ActDecisively
        };
    }

    private bool IsComfortingTemplate(CardTemplateType template)
    {
        return template == CardTemplateType.OfferHelp ||
               template == CardTemplateType.ExpressEmpathy ||
               template == CardTemplateType.ProvideReassurance;
    }

    private bool IsQuestioningTemplate(CardTemplateType template)
    {
        return template == CardTemplateType.CasualInquiry ||
               template == CardTemplateType.AskDirectQuestion ||
               template == CardTemplateType.ExpressCuriosity;
    }

    private bool IsPersonalTemplate(CardTemplateType template)
    {
        return template == CardTemplateType.SharePersonal ||
               template == CardTemplateType.RevealSecret ||
               template == CardTemplateType.ExpressVulnerability;
    }

    // Helper classes
    private class UrgencyContext
    {
        public UrgencyLevel Level { get; set; }
        public StakeType? Stakes { get; set; }
        public int? HoursRemaining { get; set; }
    }

    private class RelationshipContext
    {
        public RelationshipLevel Level { get; set; }
        public ConnectionType DominantType { get; set; }
        public int TotalTokens { get; set; }
    }

    private class CardProperties
    {
        public ConnectionType Type { get; set; }
        public int Weight { get; set; }
        public int ComfortValue { get; set; }
        public bool CanChangeState { get; set; }
        public EmotionalState? SuccessState { get; set; }
        public EmotionalState? FailureState { get; set; }
    }
}

// Additional categorical enums for card context
public enum UrgencyLevel { None, Low, Moderate, High, Critical }
public enum RelationshipLevel { Stranger, Acquaintance, Friendly, Close, Intimate }
public enum CardTone 
{ 
    Neutral, Warm, Supportive, Inquisitive, Intimate, 
    Cautious, Desperate, Urgent, Playful, Serious 
}
public enum CrisisCardType 
{ 
    DesperatePromise, LastMinuteDeal, EmergencyIntervention, 
    DefuseConfrontation, UrgentAction 
}