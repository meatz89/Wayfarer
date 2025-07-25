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
            .WithDescription("A 10-day journey teaching all game mechanics")
            .RequireFlag(FlagService.TUTORIAL_COMPLETE, false) // Don't start if already completed
            .SetFlagOnStart(FlagService.TUTORIAL_STARTED)
            .SetFlagOnComplete(FlagService.TUTORIAL_COMPLETE)
            
            // Day 1: Movement and Survival
            .AddStep("day1_wake", "Wake in Abandoned Warehouse")
            .WithStepDescription("You wake in the Lower Ward, destitute and alone")
            .WithGuidance("Leave the warehouse to explore the Lower Ward")
            .AllowActions("Travel", "Rest")
            .ShowNPCs("tam_beggar")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_MOVEMENT)
            
            .AddStep("day1_meet_tam", "Meet Tam the Beggar")
            .WithStepDescription("Tam offers advice about survival in the city")
            .WithGuidance("Talk to Tam to learn about finding work")
            .AllowActions("Converse", "Travel", "Rest")
            .ShowNPCs("tam_beggar")
            .WithNPCDialogue("tam_beggar", "Hey there, new blood. First day on the streets? Let me give you some advice - the docks always need workers. Martha's usually hiring if you can handle heavy lifting.")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_NPC_TALK)
            
            .AddStep("day1_survival_choice", "Make First Survival Choice")
            .WithStepDescription("You're hungry and tired. What will you prioritize?")
            .WithGuidance("Choose: Rest to recover stamina, or save your coins for later")
            .AllowActions("Rest", "Travel")
            .ShowNPCs("tam_beggar")
            .CompleteWhenCounter("time_blocks_passed", 3, ConditionType.CounterGreaterThan)
            
            // Day 2: Work and Tokens
            .AddStep("day2_find_work", "Find Work at the Docks")
            .WithStepDescription("Time to earn your first honest coins")
            .WithGuidance("Travel to the Docks and find Martha to work")
            .AllowActions("Travel", "Rest", "Converse", "Work")
            .ShowNPCs("tam_beggar", "martha_docks")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_WORK)
            
            .AddStep("day2_earn_token", "Earn Your First Token")
            .WithStepDescription("Martha appreciates hard workers")
            .WithGuidance("Continue working to build trust with Martha")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe")
            .WithNPCDialogue("martha_docks", "Good work today. You're not like the usual drifters. Keep this up and I might have something special for you.")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_TOKEN_EARNED)
            .SetFlagOnStepComplete("first_token_earned")
            
            .AddStep("day2_meet_elena", "Meet Elena the Scribe")
            .WithStepDescription("A chance encounter at Merchant's Rest")
            .WithGuidance("Talk to Elena at the inn")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe")
            .WithNPCDialogue("elena_scribe", "Oh! I haven't seen you before. New to Millbrook? I'm Elena - I handle correspondence for merchants. Speaking of which... no, never mind. You probably aren't interested in letter work.")
            .CompleteWhenCounter("npcs_talked_to", 3, ConditionType.CounterGreaterThan)
            
            // Day 3: Letter Discovery
            .AddStep("day3_letter_offer", "First Letter Offer")
            .WithStepDescription("Elena has a proposition for you")
            .WithGuidance("Talk to Elena about the letter delivery job")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe", "fishmonger")
            .WithNPCDialogue("elena_scribe", "Actually, I do have a simple letter that needs delivering. The fishmonger at the docks ordered supplies. It pays 3 coins - interested?")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_LETTER_OFFERED)
            
            .AddStep("day3_accept_letter", "Accept Your First Letter")
            .WithStepDescription("Your first step into the letter trade")
            .WithGuidance("Accept Elena's letter in your queue")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe", "fishmonger")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_LETTER_ACCEPTED)
            .SetFlagOnStepComplete("tutorial_first_letter_accepted")
            
            .AddStep("day3_collect_letter", "Collect the Letter")
            .WithStepDescription("Pick up the letter from Elena")
            .WithGuidance("Collect the letter from Elena to begin delivery")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "CollectLetter")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe", "fishmonger")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_LETTER_COLLECTED)
            
            .AddStep("day3_deliver_letter", "Complete First Delivery")
            .WithStepDescription("Deliver the letter to earn your payment")
            .WithGuidance("Find the fishmonger at the docks and deliver the letter")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "DeliverLetter")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe", "fishmonger")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_LETTER_DELIVERED)
            
            // Day 4-5: Queue Pressure
            .AddStep("day4_multiple_letters", "Managing Multiple Letters")
            .WithStepDescription("More merchants want your services")
            .WithGuidance("Accept and manage multiple letters in your queue")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction", "CollectLetter", "DeliverLetter")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe", "fishmonger")
            .CompleteWhenCounter("letters_in_queue", 2, ConditionType.CounterGreaterThan)
            
            .AddStep("day5_urgent_letter", "Crisis: Urgent Medicine")
            .WithStepDescription("Martha's child is sick and needs medicine urgently")
            .WithGuidance("Martha needs medicine delivered TODAY. Choose wisely.")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction", "CollectLetter", "DeliverLetter")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe", "fishmonger")
            .WithNPCDialogue("martha_docks", "Please! My daughter is burning with fever. This letter is for the apothecary - it's for medicine. I'll remember if you help me... or if you don't.")
            .CompleteWhenCounter("tutorial_day", 5, ConditionType.CounterGreaterThan)
            
            .AddStep("day5_token_burn", "Learn Token Burning")
            .WithStepDescription("Sometimes sacrifices must be made")
            .WithGuidance("Use tokens to manage your queue if needed")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction", "CollectLetter", "DeliverLetter")
            .ShowNPCs("tam_beggar", "martha_docks", "elena_scribe", "fishmonger")
            .CompleteWhenFlag(FlagService.TUTORIAL_FIRST_TOKEN_BURNED)
            
            // Day 6-7: Desperation
            .AddStep("day6_consequences", "Face the Consequences")
            .WithStepDescription("Your choices have shaped relationships")
            .WithGuidance("See how NPCs remember your actions")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction", "CollectLetter", "DeliverLetter", "BorrowMoney")
            .CompleteWhenCounter("hostile_npcs", 1, ConditionType.CounterGreaterThan)
            
            .AddStep("day7_desperation", "Reach Rock Bottom")
            .WithStepDescription("Debt, exhaustion, and few friends")
            .WithGuidance("Survive however you can")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction", "CollectLetter", "DeliverLetter", "BorrowMoney", "Gather")
            .CompleteWhenFlag(FlagService.TUTORIAL_DESPERATION_REACHED)
            
            // Day 8-9: Hope
            .AddStep("day8_tam_returns", "Tam Has News")
            .WithStepDescription("Your old friend brings unexpected tidings")
            .WithGuidance("Find Tam in the Lower Ward")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction", "CollectLetter", "DeliverLetter", "BorrowMoney", "Gather")
            .WithNPCDialogue("tam_beggar", "Hey! Remember me? I've been watching you work these streets. Someone else has been watching too. There's a letter waiting for you - not the usual kind. Gold seal and everything.")
            .CompleteWhenCounter("tutorial_day", 8, ConditionType.CounterGreaterThan)
            
            .AddStep("day9_mysterious_letter", "The Mysterious Letter")
            .WithStepDescription("A letter with a gold seal awaits")
            .WithGuidance("Investigate this unusual correspondence")
            .AllowActions("Travel", "Rest", "Converse", "Work", "Socialize", "QueueAction", "CollectLetter", "DeliverLetter", "BorrowMoney", "Gather")
            .CompleteWhenFlag(FlagService.TUTORIAL_PATRON_LETTER_RECEIVED)
            
            // Day 10: Transformation
            .AddStep("day10_meet_patron", "Meeting at Merchant's Rest")
            .WithStepDescription("Your mysterious benefactor awaits")
            .WithGuidance("Go to the private room at Merchant's Rest")
            .AllowActions("Travel", "Rest", "Converse")
            .ShowNPCs("patron_intermediary")
            .WithNPCDialogue("patron_intermediary", "Ah, the industrious letter carrier. My employer has been watching your progress with interest. Despite starting with nothing, you've shown resourcefulness and determination. We have a proposition.")
            .CompleteWhenFlag(FlagService.TUTORIAL_PATRON_MET)
            
            .AddStep("day10_choice", "The Patron's Offer")
            .WithStepDescription("A chance to change your life")
            .WithGuidance("Accept or refuse the patron's offer")
            .AllowActions("Converse")
            .ShowNPCs("patron_intermediary")
            .WithNPCDialogue("patron_intermediary", "We offer patronage: regular letters, good pay, and status. In return, you carry our letters first, always. You'll rise from these slums to serve Millbrook's elite. What say you?")
            .CompleteWhenFlag(FlagService.TUTORIAL_PATRON_ACCEPTED)
            
            .AddStep("day10_complete", "New Life Begins")
            .WithStepDescription("From destitute to patronage in 10 days")
            .WithGuidance("Your tutorial is complete. The real game begins!")
            .CompleteWhenFlag("tutorial_complete_final")
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