using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Defines base conversation choices for each NPC emotional state.
/// These are minimal (1-2 choices) that are always available regardless of letters.
/// </summary>
public class BaseConversationTemplate
{
    private readonly ConnectionTokenManager _tokenManager;
    private readonly ITimeManager _timeManager;
    
    public BaseConversationTemplate(
        ConnectionTokenManager tokenManager,
        ITimeManager timeManager)
    {
        _tokenManager = tokenManager;
        _timeManager = timeManager;
    }
    
    /// <summary>
    /// Get base choices for an NPC in a specific emotional state.
    /// These are the minimal, always-available choices.
    /// </summary>
    public List<ConversationChoice> GetBaseChoices(NPC npc, NPCEmotionalState state)
    {
        var choices = new List<ConversationChoice>();
        
        // EXIT is always available for every state
        choices.Add(CreateExitChoice());
        
        // Add state-specific base choice
        switch (state)
        {
            case NPCEmotionalState.DESPERATE:
                choices.Add(CreateHelpChoice(npc));
                break;
                
            case NPCEmotionalState.HOSTILE:
                choices.Add(CreateNegotiateChoice(npc));
                break;
                
            case NPCEmotionalState.CALCULATING:
                choices.Add(CreateInvestigateChoice(npc));
                break;
                
            case NPCEmotionalState.WITHDRAWN:
                choices.Add(CreateAcknowledgeChoice(npc));
                break;
        }
        
        return choices;
    }
    
    private ConversationChoice CreateExitChoice()
    {
        return new ConversationChoice
        {
            ChoiceID = "base_exit",
            NarrativeText = "\"I should go. Time is pressing.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.EXIT,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "→ Leave conversation",
            MechanicalEffects = new List<IMechanicalEffect> 
            { 
                new EndConversationEffect() 
            }
        };
    }
    
    private ConversationChoice CreateHelpChoice(NPC npc)
    {
        return new ConversationChoice
        {
            ChoiceID = "base_help_desperate",
            NarrativeText = "\"Let me help you with something.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.HELP,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "♥ +1 Trust token",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new GainTokensEffect(ConnectionType.Trust, 1, npc.ID, _tokenManager)
            }
        };
    }
    
    private ConversationChoice CreateNegotiateChoice(NPC npc)
    {
        return new ConversationChoice
        {
            ChoiceID = "base_negotiate_hostile",
            NarrativeText = "\"Perhaps we can come to an arrangement.\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.NEGOTIATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "🪙 Trade tokens for cooperation",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new OpenNegotiationEffect(),
                new ConversationTimeEffect(15, _timeManager)
            }
        };
    }
    
    private ConversationChoice CreateInvestigateChoice(NPC npc)
    {
        return new ConversationChoice
        {
            ChoiceID = "base_investigate_calculating",
            NarrativeText = "\"Tell me more about your situation.\"",
            AttentionCost = 1,
            BaseVerb = BaseVerb.INVESTIGATE,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "ℹ Learn information | ⏱ +20 minutes",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new GainInformationEffect($"{npc.Name}'s current concerns", InfoType.Rumor),
                new ConversationTimeEffect(20, _timeManager)
            }
        };
    }
    
    private ConversationChoice CreateAcknowledgeChoice(NPC npc)
    {
        return new ConversationChoice
        {
            ChoiceID = "base_acknowledge_withdrawn",
            NarrativeText = "\"I understand.\"",
            AttentionCost = 0,
            BaseVerb = BaseVerb.EXIT,
            IsAffordable = true,
            IsAvailable = true,
            MechanicalDescription = "→ Maintains current state",
            MechanicalEffects = new List<IMechanicalEffect>
            {
                new MaintainStateEffect()
            }
        };
    }
}