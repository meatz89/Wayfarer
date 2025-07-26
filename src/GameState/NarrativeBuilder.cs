using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Fluent API builder for creating narrative definitions
/// </summary>
public class NarrativeBuilder
{
    private readonly NarrativeDefinition _definition;
    private NarrativeStep _currentStep;
    
    private NarrativeBuilder(string id, string title)
    {
        _definition = new NarrativeDefinition
        {
            Id = id,
            Title = title,
            Steps = new List<NarrativeStep>(),
            StartingConditions = new List<NarrativeCondition>(),
            StartingEffects = new List<NarrativeEffect>(),
            CompletionRewards = new List<NarrativeEffect>()
        };
    }
    
    /// <summary>
    /// Create a new narrative builder
    /// </summary>
    public static NarrativeBuilder Create(string id, string title)
    {
        return new NarrativeBuilder(id, title);
    }
    
    /// <summary>
    /// Set the narrative description
    /// </summary>
    public NarrativeBuilder WithDescription(string description)
    {
        _definition.Description = description;
        return this;
    }
    
    /// <summary>
    /// Add a starting condition
    /// </summary>
    public NarrativeBuilder RequireFlag(string flagName, bool shouldBeSet = true)
    {
        _definition.StartingConditions.Add(new NarrativeCondition
        {
            Type = shouldBeSet ? ConditionType.FlagSet : ConditionType.FlagNotSet,
            Value = flagName
        });
        return this;
    }
    
    /// <summary>
    /// Add a starting effect
    /// </summary>
    public NarrativeBuilder SetFlagOnStart(string flagName)
    {
        _definition.StartingEffects.Add(new NarrativeEffect
        {
            Type = EffectType.SetFlag,
            Value = flagName
        });
        return this;
    }
    
    /// <summary>
    /// Set a counter value on start
    /// </summary>
    public NarrativeBuilder SetCounterOnStart(string counterName, int value)
    {
        _definition.StartingEffects.Add(new NarrativeEffect
        {
            Type = EffectType.SetCounter,
            Key = counterName,
            Value = value.ToString()
        });
        return this;
    }
    
    /// <summary>
    /// Add a completion reward
    /// </summary>
    public NarrativeBuilder SetFlagOnComplete(string flagName)
    {
        _definition.CompletionRewards.Add(new NarrativeEffect
        {
            Type = EffectType.SetFlag,
            Value = flagName
        });
        return this;
    }
    
    /// <summary>
    /// Start defining a new step
    /// </summary>
    public NarrativeBuilder AddStep(string id, string name)
    {
        // Save previous step if exists
        if (_currentStep != null)
        {
            _definition.Steps.Add(_currentStep);
        }
        
        _currentStep = new NarrativeStep
        {
            Id = id,
            Name = name,
            AllowedActions = new List<string>(),
            VisibleNPCs = new List<string>(),
            DialogueOverrides = new Dictionary<string, string>(),
            CompletionRequirements = new List<NarrativeCondition>(),
            CompletionEffects = new List<NarrativeEffect>()
        };
        
        return this;
    }
    
    /// <summary>
    /// Set step description
    /// </summary>
    public NarrativeBuilder WithStepDescription(string description)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.Description = description;
        return this;
    }
    
    /// <summary>
    /// Set step guidance text
    /// </summary>
    public NarrativeBuilder WithGuidance(string guidance)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.GuidanceText = guidance;
        return this;
    }
    
    /// <summary>
    /// Allow specific actions during this step
    /// </summary>
    public NarrativeBuilder AllowActions(params string[] actions)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.AllowedActions.AddRange(actions);
        return this;
    }
    
    /// <summary>
    /// Make specific NPCs visible during this step
    /// </summary>
    public NarrativeBuilder ShowNPCs(params string[] npcIds)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.VisibleNPCs.AddRange(npcIds);
        return this;
    }
    
    /// <summary>
    /// Add dialogue override for an NPC
    /// </summary>
    public NarrativeBuilder WithNPCDialogue(string npcId, string dialogue)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.DialogueOverrides[npcId] = dialogue;
        return this;
    }
    
    /// <summary>
    /// Require a flag to be set to complete this step
    /// </summary>
    public NarrativeBuilder CompleteWhenFlag(string flagName)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.CompletionRequirements.Add(new NarrativeCondition
        {
            Type = ConditionType.FlagSet,
            Value = flagName
        });
        return this;
    }
    
    /// <summary>
    /// Require a counter value to complete this step
    /// </summary>
    public NarrativeBuilder CompleteWhenCounter(string counterName, int value, ConditionType comparison = ConditionType.CounterEquals)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.CompletionRequirements.Add(new NarrativeCondition
        {
            Type = comparison,
            Key = counterName,
            Value = value.ToString()
        });
        return this;
    }
    
    /// <summary>
    /// Set a flag when this step completes
    /// </summary>
    public NarrativeBuilder SetFlagOnStepComplete(string flagName)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.CompletionEffects.Add(new NarrativeEffect
        {
            Type = EffectType.SetFlag,
            Value = flagName
        });
        return this;
    }
    
    /// <summary>
    /// Force specific game state when step starts
    /// </summary>
    public NarrativeBuilder ForceLocationOnStepStart(string locationId, string spotId)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.ForcedLocation = locationId;
        _currentStep.ForcedSpot = spotId;
        return this;
    }
    
    /// <summary>
    /// Set time of day when step starts
    /// </summary>
    public NarrativeBuilder SetTimeOnStepStart(int hour)
    {
        if (_currentStep == null) throw new InvalidOperationException("No step being defined");
        _currentStep.ForcedHour = hour;
        return this;
    }
    
    /// <summary>
    /// Build the narrative definition
    /// </summary>
    public NarrativeDefinition Build()
    {
        // Add last step if exists
        if (_currentStep != null)
        {
            _definition.Steps.Add(_currentStep);
        }
        
        return _definition;
    }
    
    /// <summary>
    /// Build and register the narrative
    /// </summary>
    public NarrativeDefinition BuildAndRegister()
    {
        var narrative = Build();
        NarrativeDefinitions.Add(narrative);
        return narrative;
    }
}

/// <summary>
/// Static class to build all narrative definitions
/// </summary>
public static class NarrativeContentBuilder
{
    /// <summary>
    /// Build the Wayfarer tutorial narrative
    /// </summary>
    public static void BuildTutorialNarrative()
    {
        NarrativeBuilder.Create("wayfarer_tutorial", "From Destitute to Patronage")
            .WithDescription("A 3-day journey teaching all game mechanics through forced narrative")
            .RequireFlag(FlagService.TUTORIAL_COMPLETE, false) // Don't start if already completed
            .SetFlagOnStart(FlagService.TUTORIAL_STARTED)
            .SetFlagOnComplete(FlagService.TUTORIAL_COMPLETE)
            // Set initial tutorial state
            .SetCounterOnStart("tutorial_day", 1)
            .SetCounterOnStart("time_blocks_passed", 0)
            .SetCounterOnStart("letters_in_queue", 0)
            .SetCounterOnStart("npcs_talked_to", 0)
            
            // DAY 1: SURVIVAL AND FIRST TOKEN
            // Step 1: Wake up with minimal resources
            .AddStep("day1_wake", "Awakening")
            .WithStepDescription("You wake in the Abandoned Warehouse with only 2 coins and dangerously low stamina (4/10)")
            .WithGuidance("You need to gather your strength before venturing out. Rest for a moment to clear your head.")
            .AllowActions("Rest") // FORCED: Only allow rest
            .ShowNPCs() // No NPCs visible yet
            .ForceLocationOnStepStart("lower_ward", "abandoned_warehouse")
            .SetTimeOnStepStart(6) // Dawn
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_REST)
            
            // Step 2: Forced movement tutorial
            .AddStep("day1_square", "The Lower Ward Square")
            .WithStepDescription("After resting, you gather the strength to venture out. You stumble into the square. Your vision blurs from exhaustion. You need work or food immediately.")
            .WithGuidance("Find someone who can help - talk to the locals")
            .AllowActions("Converse") // FORCED: Only allow talking
            .ShowNPCs("tam_beggar", "elena_scribe") // Only show specific NPCs
            .ForceLocationOnStepStart("lower_ward", "lower_ward_square") // Move player to the square
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_NPC_TALK)
            
            // Step 3: Learn about work from Tam
            .AddStep("day1_tam_advice", "Tam's Advice")
            .WithStepDescription("Tam notices your desperation and offers guidance")
            .WithGuidance("Listen to Tam's advice about finding work")
            .AllowActions("Converse", "Travel") // Can now travel
            .ShowNPCs("tam_beggar")
            .WithNPCDialogue("tam_beggar", "You look about ready to collapse, friend. The docks - Martha's always hiring there. Hard work, but honest pay. Go now, before you fall over. Tell her Tam sent you.")
            .CompleteWhenFlag("tutorial_tam_advice_received")
            .SetFlagOnStepComplete("tutorial_tam_advice_received")
            
            // Step 4: Travel to docks and meet Martha
            .AddStep("day1_meet_martha", "Finding Martha")
            .WithStepDescription("Following Tam's advice, you head to the docks")
            .WithGuidance("Travel to Millbrook Docks and find Martha")
            .AllowActions("Travel", "Converse")
            .ShowNPCs("martha_docker")
            .ForceLocationOnStepStart("millbrook_docks", "wharf")
            .CompleteWhenFlag("tutorial_martha_met")
            
            // Step 5: Forced to take dock work
            .AddStep("day1_first_work", "Desperate for Work")
            .WithStepDescription("Martha eyes you skeptically but sees your desperation")
            .WithGuidance("Accept Martha's work offer - you have no choice")
            .AllowActions("Work") // FORCED: Only work available
            .ShowNPCs("martha_docker")
            .WithNPCDialogue("martha_docker", "Tam sent you? Hmm. You look half-dead already, but I suppose you'll do. 2 stamina for 4 coins. Take it or leave it - though by the look of you, you don't have much choice.")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_WORK)
            
            // Step 6: Work action tutorial (2 stamina → 4 coins + 1 Trade token)
            .AddStep("day1_work_complete", "First Day's Labor")
            .WithStepDescription("You complete the work, earning 4 coins and Martha's grudging respect (1 Trade token)")
            .WithGuidance("You now have 6 coins total. Buy food before you collapse!")
            .AllowActions("Browse", "Travel") // Can browse market
            .ShowNPCs("martha_docker")
            .CompleteWhenFlag("tutorial_food_purchased")
            
            // Step 7: Buy food to avoid collapse
            .AddStep("day1_buy_food", "Survival Secured")
            .WithStepDescription("With food in your belly, you feel strength returning")
            .WithGuidance("Return to Martha - she mentioned having something special for hard workers")
            .AllowActions("Travel", "Converse", "Rest")
            .ShowNPCs("martha_docker", "elena_scribe")
            .CompleteWhenFlag("tutorial_martha_letter_offered")
            
            // Step 8: Accept Martha's first letter delivery
            .AddStep("day1_first_letter", "A Letter Job")
            .WithStepDescription("Martha has a letter that needs delivering")
            .WithGuidance("Accept Martha's letter delivery job")
            .AllowActions("QueueAction") // FORCED: Must accept letter
            .ShowNPCs("martha_docker")
            .WithNPCDialogue("martha_docker", "You did good work today. I have a letter for the fishmonger - simple delivery, pays 3 coins. Consider it a test. Don't mess it up.")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_LETTER_ACCEPTED)
            .SetFlagOnStepComplete(FlagService.TUTORIAL_MARTHA_LETTER_OFFERED)
            
            // Step 9: Meet Elena and accept personal letter
            .AddStep("day1_elena_letter", "A Second Opportunity")
            .WithStepDescription("Elena the Scribe has noticed your new profession")
            .WithGuidance("Talk to Elena about her letter")
            .AllowActions("Converse", "QueueAction", "CollectLetter")
            .ShowNPCs("martha_docker", "elena_scribe")
            .WithNPCDialogue("elena_scribe", "Oh! You're doing letter work now? Perfect timing - I have a personal letter for my cousin. It's not urgent, but I'd appreciate the help. I'll remember your kindness.")
            .CompleteWhenFlag(FlagService.TUTORIAL_ELENA_LETTER_ACCEPTED)
            .SetFlagOnStepComplete(FlagService.TUTORIAL_FIRST_LETTER_OFFERED)
            
            // Step 10: End of Day 1
            .AddStep("day1_complete", "Day's End")
            .WithStepDescription("As night falls, you reflect on your first day. You've learned basic survival and discovered the letter trade.")
            .WithGuidance("Rest for the night. Tomorrow will bring new challenges.")
            .AllowActions("Rest", "AdvanceTime") // Force time advancement
            .CompleteWhenCounter("tutorial_day", 2, ConditionType.CounterEquals)
            .SetFlagOnStepComplete("tutorial_day1_complete")
            
            // DAY 2: QUEUE PRIORITY AND TOKEN BURNING
            // Step 11: Morning - Multiple letters create conflict
            .AddStep("day2_morning", "A Busy Morning")
            .WithStepDescription("You wake to find merchants eager for your services. Multiple letter offers await.")
            .WithGuidance("Check your letter queue and accept new offers")
            .AllowActions("Travel", "Converse", "QueueAction", "CollectLetter", "DeliverLetter")
            .ShowNPCs("martha_docker", "elena_scribe", "fishmonger_frans", "merchant_guild")
            .CompleteWhenCounter("letters_in_queue", 3, ConditionType.CounterGreaterThan)
            
            // Step 12: Elena's urgent letter forces queue management
            .AddStep("day2_urgent", "An Urgent Request")
            .WithStepDescription("Elena rushes to you with an urgent letter that must be delivered immediately!")
            .WithGuidance("Elena's urgent letter enters at position 1, pushing others down. You must manage your queue!")
            .AllowActions("QueueAction") // FORCED: Must handle queue
            .ShowNPCs("elena_scribe")
            .WithNPCDialogue("elena_scribe", "Please! My mother is ill and this medicine request must reach the apothecary TODAY! I know you have other letters, but... I have these Trust tokens. Would burning one help move my letter up?")
            .CompleteWhenFlag("tutorial_urgent_letter_handled")
            
            // Step 13: Learn about token burning
            .AddStep("day2_token_burn", "The Power of Tokens")
            .WithStepDescription("You learn that tokens can be burned to manipulate queue positions")
            .WithGuidance("Burn a token to manage your queue priorities")
            .AllowActions("QueueAction", "CollectLetter", "DeliverLetter")
            .ShowNPCs("elena_scribe")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_TOKEN_BURNED)
            
            // Step 14: Merchant Guild trial
            .AddStep("day2_guild_trial", "The Merchant Guild Trial")
            .WithStepDescription("The Merchant Guild offers you a trial - complete it to earn their standing obligation")
            .WithGuidance("Complete the Guild's delivery challenge")
            .AllowActions("Travel", "Converse", "QueueAction", "CollectLetter", "DeliverLetter")
            .ShowNPCs("merchant_guild")
            .WithNPCDialogue("merchant_guild", "We've heard of your reliability. Complete this trade route delivery within the time limit, and we'll establish a standing arrangement. Our 'Merchant's Priority' obligation will ensure our letters always get favorable queue positions.")
            .CompleteWhenFlag("tutorial_guild_trial_complete")
            
            // Step 15: Learn about standing obligations
            .AddStep("day2_obligation", "Standing Obligations")
            .WithStepDescription("You've earned the 'Merchant's Priority' obligation - Guild letters now get +1 queue position")
            .WithGuidance("Check your obligations panel to see how this affects future letters")
            .AllowActions("Travel", "Rest", "Converse")
            .CompleteWhenFlag("tutorial_obligation_understood")
            .SetFlagOnStepComplete("tutorial_day2_complete")
            
            // DAY 3: DEBT AND PATRONAGE
            // Step 16: Financial crisis
            .AddStep("day3_crisis", "Financial Crisis")
            .WithStepDescription("You wake to find your money stolen! With no coins and letters to deliver, you face a crisis.")
            .WithGuidance("You need money for food and supplies. Someone might lend you coins...")
            .AllowActions("Converse", "BorrowMoney") // FORCED: Must borrow
            .ShowNPCs("elena_scribe", "martha_docker")
            .CompleteWhenFlag("tutorial_money_borrowed")
            
            // Step 17: Borrow from Elena (-2 Trust tokens)
            .AddStep("day3_debt", "The Weight of Debt")
            .WithStepDescription("Elena lends you money but the debt costs you 2 Trust tokens")
            .WithGuidance("Your debt to Elena has consequences...")
            .AllowActions("Travel", "Converse")
            .ShowNPCs("elena_scribe")
            .WithNPCDialogue("elena_scribe", "Of course I'll help you! But... borrowing creates obligations. Those 2 Trust tokens I'm losing represent my faith in you. And now... well, my letters will naturally take priority in your queue. It's only fair, right?")
            .CompleteWhenFlag("tutorial_debt_created")
            
            // Step 18: Debt activates "Elena's Leverage"
            .AddStep("day3_leverage", "The Leverage System")
            .WithStepDescription("Your debt has activated 'Elena's Leverage' - her letters now jump ahead in queue")
            .WithGuidance("Check how Elena's leverage affects your letter queue")
            .AllowActions("QueueAction", "Travel", "CollectLetter", "DeliverLetter")
            .ShowNPCs("elena_scribe")
            .CompleteWhenFlag("tutorial_leverage_understood")
            
            // Step 19: Mysterious letter arrives
            .AddStep("day3_mystery", "A Mysterious Letter")
            .WithStepDescription("Tam finds you with news of a gold-sealed letter waiting at Merchant's Rest")
            .WithGuidance("Travel to Merchant's Rest to investigate")
            .AllowActions("Travel", "Converse")
            .ShowNPCs("tam_beggar")
            .ForceLocationOnStepStart("lower_ward", "lower_ward_square")
            .WithNPCDialogue("tam_beggar", "There you are! Someone's been asking about you - someone important. There's a letter waiting at Merchant's Rest. Gold seal and everything. This could change your life, friend.")
            .CompleteWhenFlag(FlagService.TUTORIAL_PATRON_LETTER_RECEIVED)
            
            // Step 20: Meet the patron
            .AddStep("day3_patron", "The Patron")
            .WithStepDescription("In the private room at Merchant's Rest, a well-dressed intermediary awaits")
            .WithGuidance("Speak with the mysterious patron")
            .AllowActions("Converse") // FORCED: Must talk
            .ShowNPCs("patron_intermediary")
            .ForceLocationOnStepStart("merchants_rest", "private_room")
            .WithNPCDialogue("patron_intermediary", "Ah, the industrious letter carrier. My employer has watched your progress with interest. From nothing to a reliable courier in just three days. Impressive. We have a proposition that could lift you from these streets permanently.")
            .CompleteWhenFlag(FlagService.TUTORIAL_PATRON_MET)
            
            // Step 21: Accept patronage
            .AddStep("day3_patronage", "The Offer of Patronage")
            .WithStepDescription("The patron offers regular work, good pay, and social advancement")
            .WithGuidance("Accept the patronage to secure your future")
            .AllowActions("Converse") // FORCED: Must accept
            .ShowNPCs("patron_intermediary")
            .WithNPCDialogue("patron_intermediary", "We offer patronage: steady Noble letters, premium rates, and entry to exclusive locations. In return, our letters take absolute priority. You'll wear our seal and represent our interests. Do you accept?")
            .CompleteWhenFlag(FlagService.TUTORIAL_PATRON_ACCEPTED)
            
            // Step 22: First patron letter
            .AddStep("day3_first_patron", "Your First Patron Letter")
            .WithStepDescription("Your patron immediately provides a Noble letter - it enters at queue position 1")
            .WithGuidance("Deliver your first patron letter to complete the tutorial")
            .AllowActions("QueueAction", "CollectLetter", "DeliverLetter", "Travel")
            .ShowNPCs("patron_intermediary")
            .CompleteWhenFlag("tutorial_first_patron_letter_delivered")
            
            // Step 23: Tutorial complete
            .AddStep("tutorial_complete", "From Rags to Patronage")
            .WithStepDescription("In just 3 days, you've risen from destitute to patronized. The real journey begins now.")
            .WithGuidance("Tutorial complete! You now understand survival, letters, tokens, obligations, and leverage.")
            .CompleteWhenFlag(FlagService.TUTORIAL_COMPLETE_FINAL)
            .SetFlagOnStepComplete(FlagService.TUTORIAL_COMPLETE)
            
            .BuildAndRegister();
    }
    
    /// <summary>
    /// Build all narrative content
    /// </summary>
    public static void BuildAllNarratives()
    {
        NarrativeDefinitions.Clear();
        BuildTutorialNarrative();
        // Future narratives (quests, stories) can be added here
    }
}