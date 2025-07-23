using System;
using System.Collections.Generic;

/// <summary>
/// Builder pattern for creating narrative definitions programmatically
/// Provides a fluent API for defining tutorials, quests, and story sequences
/// </summary>
public class NarrativeBuilder
{
    private NarrativeDefinition _narrative;
    private NarrativeStep _currentStep;
    private List<NarrativeStep> _steps;
    
    public NarrativeBuilder(string id, string name)
    {
        _narrative = new NarrativeDefinition
        {
            Id = id,
            Name = name,
            Steps = new List<NarrativeStep>()
        };
        _steps = _narrative.Steps;
    }
    
    public NarrativeBuilder WithDescription(string description)
    {
        _narrative.Description = description;
        return this;
    }
    
    public NarrativeBuilder WithIntroduction(string message)
    {
        _narrative.IntroductionMessage = message;
        return this;
    }
    
    public NarrativeBuilder WithCompletion(string message)
    {
        _narrative.CompletionMessage = message;
        return this;
    }
    
    // Starting conditions
    public NarrativeBuilder WithStartingConditions(Action<StartingConditionsBuilder> configure)
    {
        var conditionsBuilder = new StartingConditionsBuilder();
        configure(conditionsBuilder);
        _narrative.StartingConditions = conditionsBuilder.Build();
        return this;
    }
    
    // Add a new step
    public NarrativeBuilder AddStep(string id, string name, Action<StepBuilder> configure)
    {
        var stepBuilder = new StepBuilder(id, name);
        configure(stepBuilder);
        _steps.Add(stepBuilder.Build());
        return this;
    }
    
    // Rewards
    public NarrativeBuilder WithRewards(Action<RewardsBuilder> configure)
    {
        var rewardsBuilder = new RewardsBuilder();
        configure(rewardsBuilder);
        _narrative.Rewards = rewardsBuilder.Build();
        return this;
    }
    
    // NPCs that are hidden until unlocked
    public NarrativeBuilder WithRestrictedNPCs(params string[] npcIds)
    {
        _narrative.RestrictedNPCs = new List<string>(npcIds);
        return this;
    }
    
    public NarrativeDefinition Build()
    {
        return _narrative;
    }
}

/// <summary>
/// Builder for narrative starting conditions
/// </summary>
public class StartingConditionsBuilder
{
    private NarrativeStartingConditions _conditions = new NarrativeStartingConditions();
    
    public StartingConditionsBuilder WithCoins(int coins)
    {
        _conditions.PlayerCoins = coins;
        return this;
    }
    
    public StartingConditionsBuilder WithStamina(int stamina)
    {
        _conditions.PlayerStamina = stamina;
        return this;
    }
    
    public StartingConditionsBuilder AtLocation(string locationId, string spotId = null)
    {
        _conditions.StartingLocation = locationId;
        _conditions.StartingSpot = spotId;
        return this;
    }
    
    public StartingConditionsBuilder ClearInventory()
    {
        _conditions.ClearInventory = true;
        return this;
    }
    
    public StartingConditionsBuilder ClearLetterQueue()
    {
        _conditions.ClearLetterQueue = true;
        return this;
    }
    
    public StartingConditionsBuilder ClearObligations()
    {
        _conditions.ClearObligations = true;
        return this;
    }
    
    public NarrativeStartingConditions Build()
    {
        return _conditions;
    }
}

/// <summary>
/// Builder for individual narrative steps
/// </summary>
public class StepBuilder
{
    private NarrativeStep _step;
    
    public StepBuilder(string id, string name)
    {
        _step = new NarrativeStep
        {
            Id = id,
            Name = name,
            AllowedActions = new List<LocationAction>()
        };
    }
    
    public StepBuilder WithDescription(string description)
    {
        _step.Description = description;
        return this;
    }
    
    public StepBuilder RequiresAction(LocationAction action)
    {
        _step.RequiredAction = action;
        return this;
    }
    
    public StepBuilder AtLocation(string locationId)
    {
        _step.RequiredLocation = locationId;
        return this;
    }
    
    public StepBuilder WithNPC(string npcId)
    {
        _step.RequiredNPC = npcId;
        return this;
    }
    
    public StepBuilder AllowActions(params LocationAction[] actions)
    {
        _step.AllowedActions.AddRange(actions);
        return this;
    }
    
    public StepBuilder WithGuidance(string guidanceText)
    {
        _step.GuidanceText = guidanceText;
        return this;
    }
    
    public StepBuilder CompletesWhenFlagSet(string flagName)
    {
        _step.CompletionFlag = flagName;
        return this;
    }
    
    public StepBuilder WithConversationIntro(string introText)
    {
        _step.ConversationIntroduction = introText;
        return this;
    }
    
    public NarrativeStep Build()
    {
        // Auto-generate completion flag if not specified
        if (string.IsNullOrEmpty(_step.CompletionFlag))
        {
            _step.CompletionFlag = $"narrative_step_{_step.Id}_complete";
        }
        
        return _step;
    }
}

/// <summary>
/// Builder for narrative rewards
/// </summary>
public class RewardsBuilder
{
    private NarrativeRewards _rewards = new NarrativeRewards();
    
    public RewardsBuilder WithCoins(int coins)
    {
        _rewards.Coins = coins;
        return this;
    }
    
    public RewardsBuilder WithStamina(int stamina)
    {
        _rewards.Stamina = stamina;
        return this;
    }
    
    public RewardsBuilder WithItems(params string[] itemIds)
    {
        _rewards.Items = new List<string>(itemIds);
        return this;
    }
    
    public RewardsBuilder WithMessage(string message)
    {
        _rewards.Message = message;
        return this;
    }
    
    public NarrativeRewards Build()
    {
        return _rewards;
    }
}

/// <summary>
/// Static factory class for creating predefined narratives
/// </summary>
public static class NarrativeDefinitions
{
    /// <summary>
    /// Creates the 10-day tutorial narrative "From Destitute to Patronage"
    /// </summary>
    public static NarrativeDefinition CreateTutorial()
    {
        return new NarrativeBuilder("wayfarer_tutorial", "From Destitute to Patronage")
            .WithDescription("Learn the ways of a Wayfarer through 10 days of guided survival")
            .WithIntroduction("You wake on cold stone, stomach cramping from hunger. You have 3 coins - enough for one meal. But when?")
            .WithCompletion("Tutorial Complete: You've learned to survive through relationships, obligations, and difficult choices. Your journey as a Wayfarer truly begins now.")
            
            // Starting conditions
            .WithStartingConditions(conditions => conditions
                .WithCoins(3)
                .WithStamina(5)
                .AtLocation("millbrook", "lower_ward_warehouse")
                .ClearInventory()
                .ClearLetterQueue()
                .ClearObligations())
            
            // Day 1: Movement and Survival
            .AddStep("leave_warehouse", "Leave the Warehouse", step => step
                .WithDescription("Exit the abandoned warehouse")
                .RequiresAction(LocationAction.Rest)  // Use Rest as placeholder for movement tutorial
                .AtLocation("lower_ward_square")
                .WithGuidance("Click on locations to move. Each movement takes time.")
                .CompletesWhenFlagSet("tutorial_first_movement"))
                
            .AddStep("meet_tam", "Meet Tam", step => step
                .WithDescription("Talk to Tam the Beggar")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("tam_beggar")
                .WithGuidance("NPCs give information. Remember what they tell you.")
                .WithConversationIntro("New to the gutters? Word of advice - Martha at the docks sometimes has work. Hard labor, but honest coin.")
                .CompletesWhenFlagSet("tutorial_tam_conversation"))
                
            .AddStep("travel_to_docks", "Go to Millbrook Docks", step => step
                .WithDescription("Travel to the docks to find work")
                .RequiresAction(LocationAction.Rest)
                .AtLocation("millbrook_docks")
                .WithGuidance("Travel between districts takes 1 hour.")
                .CompletesWhenFlagSet("tutorial_docks_visited"))
                
            .AddStep("martha_not_available", "Martha Not Available", step => step
                .WithDescription("Return to Lower Ward - Martha works mornings only")
                .RequiresAction(LocationAction.Rest)
                .AtLocation("lower_ward_square")
                .WithGuidance("NPCs have schedules. Martha works mornings.")
                .CompletesWhenFlagSet("tutorial_schedule_learned"))
                
            .AddStep("survival_choice", "Eat or Save Money", step => step
                .WithDescription("Decide whether to spend your last coins on food")
                .RequiresAction(LocationAction.Rest)
                .AllowActions(LocationAction.Rest, LocationAction.Trade)
                .WithGuidance("Your stomach gnaws at you. Spend your last coins on food?")
                .CompletesWhenFlagSet("tutorial_day1_complete"))
                
            // Day 2: Work and Token Introduction
            .AddStep("morning_docks", "Go to Docks in Morning", step => step
                .WithDescription("Travel to docks to meet Martha")
                .RequiresAction(LocationAction.Converse)
                .AtLocation("millbrook_docks")
                .WithNPC("martha_docks")
                .WithConversationIntro("You look desperate. I'll give you a chance. Work hard, and there might be more.")
                .CompletesWhenFlagSet("tutorial_martha_met"))
                
            .AddStep("first_work", "Load Cargo", step => step
                .WithDescription("Work for Martha at the docks")
                .RequiresAction(LocationAction.Work)
                .WithNPC("martha_docks")
                .WithGuidance("Work trades stamina for coins.")
                .CompletesWhenFlagSet("first_token_earned"))
                
            .AddStep("meet_elena", "Meet Elena", step => step
                .WithDescription("Encounter Elena in the Lower Ward")
                .RequiresAction(LocationAction.Converse)
                .AtLocation("lower_ward_square")
                .WithNPC("elena_scribe")
                .WithConversationIntro("You're new. I... I used to have work copying letters. Before the fire destroyed my master's shop.")
                .CompletesWhenFlagSet("tutorial_elena_met"))
                
            // Day 3: Discovering Letter Opportunities
            .AddStep("martha_letter_offer", "Martha's Package", step => step
                .WithDescription("Talk to Martha about letter delivery")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("martha_docks")
                .WithConversationIntro("You've been reliable. I have a package for the fishmonger. Just a small thing. 2 coins on delivery. Want it?")
                .WithGuidance("NPCs with tokens offer letter opportunities!")
                .CompletesWhenFlagSet("first_letter_accepted"))
                
            .AddStep("collect_package", "Collect the Package", step => step
                .WithDescription("Collect Martha's package for delivery")
                .RequiresAction(LocationAction.Collect)
                .WithConversationIntro("Here's the package. Fish oil. Don't drop it.")
                .WithGuidance("Letters must be collected before delivery. They take inventory space.")
                .CompletesWhenFlagSet("first_letter_collected"))
                
            .AddStep("deliver_fishmonger", "Deliver to Fishmonger", step => step
                .WithDescription("Deliver the package to the fishmonger")
                .RequiresAction(LocationAction.Deliver)
                .WithGuidance("You can only deliver the letter in position 1.")
                .CompletesWhenFlagSet("first_letter_delivered"))
                
            .AddStep("elena_excited", "Elena's Discovery", step => step
                .WithDescription("Talk to Elena about new opportunities")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("elena_scribe")
                .WithConversationIntro("I overheard the fishmonger. He needs someone to carry messages to other merchants. He mentioned you!")
                .CompletesWhenFlagSet("tutorial_day3_complete"))
                
            // Day 4: Queue Pressure
            .AddStep("fishmonger_letter", "Fishmonger's Letter", step => step
                .WithDescription("Accept letter from fishmonger")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("fishmonger")
                .WithGuidance("Your letter queue shows delivery priority.")
                .CompletesWhenFlagSet("fishmonger_letter_accepted"))
                
            .AddStep("martha_emergency", "Martha's Emergency", step => step
                .WithDescription("Martha has urgent medicine for her daughter")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("martha_docks")
                .WithConversationIntro("Emergency! Medicine for my sick daughter. Only 1 day to deliver!")
                .WithGuidance("Urgent letters from people with leverage enter higher in queue.")
                .CompletesWhenFlagSet("tutorial_queue_crisis"))
                
            // Day 5: Learning Token Burning
            .AddStep("elena_desperate", "Elena's Plea", step => step
                .WithDescription("Elena begs you to help Martha")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("elena_scribe")
                .WithConversationIntro("Martha's daughter is dying! There must be something you can do!")
                .WithGuidance("The queue forces impossible choices.")
                .CompletesWhenFlagSet("tutorial_token_burning_learned"))
                
            // Day 6-7: Desperation Deepens
            .AddStep("elena_loan", "Elena's Offer", step => step
                .WithDescription("Elena offers to loan you money")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("elena_scribe")
                .WithConversationIntro("I have 5 coins saved. You can borrow them, but... I need them back someday.")
                .WithGuidance("You can go into token debt. Negative tokens mean they have leverage over you.")
                .CompletesWhenFlagSet("tutorial_debt_introduced"))
                
            // Day 8: Rock Bottom
            .AddStep("tam_returns", "Tam's Observation", step => step
                .WithDescription("Tam notices your struggles")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("tam_beggar")
                .WithConversationIntro("I've been watching you. You're drowning. But sometimes... sometimes people notice those who help others despite having nothing.")
                .CompletesWhenFlagSet("tutorial_tam_prophecy"))
                
            // Day 9: The Letter
            .AddStep("mysterious_letter", "A Mysterious Letter", step => step
                .WithDescription("Elena brings you a mysterious letter")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("elena_scribe")
                .WithConversationIntro("A courier left this for you. Real parchment. Wax seal. Who do you know with money?")
                .WithGuidance("Some letters carry special significance.")
                .CompletesWhenFlagSet("tutorial_patron_letter_received"))
                
            // Day 10: The Patron
            .AddStep("merchant_rest_inn", "Go to Merchant's Rest Inn", step => step
                .WithDescription("Travel to the inn to meet your mysterious contact")
                .RequiresAction(LocationAction.Rest)
                .AtLocation("middle_ward_inn")
                .WithGuidance("Your determination to deliver that medicine despite personal cost shows character.")
                .CompletesWhenFlagSet("tutorial_inn_reached"))
                
            .AddStep("meet_intermediary", "The Patron's Offer", step => step
                .WithDescription("Meet with the Patron's intermediary")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("patron_intermediary")
                .WithConversationIntro("My employer values discretion and loyalty above all else. They offer patronage - monthly funds, equipment when needed, protection. In exchange, you carry their letters when asked. These letters take priority above all others. Do you accept?")
                .WithGuidance("Standing Obligations permanently modify game rules.")
                .CompletesWhenFlagSet("patron_contact_met"))
                
            .AddStep("first_patron_letter", "Your First Task", step => step
                .WithDescription("Receive your first patron letter")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("patron_intermediary")
                .WithConversationIntro("Your first task. Deliver this to the Harbor Master. Tell no one of its contents. You have three days.")
                .WithGuidance("Your patron's needs now take precedence over all others.")
                .CompletesWhenFlagSet("tutorial_patron_letter_accepted"))
                
            .AddStep("elena_warning", "Elena's Warning", step => step
                .WithDescription("Elena finds you after your transformation")
                .RequiresAction(LocationAction.Converse)
                .WithNPC("elena_scribe")
                .WithConversationIntro("You look... different. Found a patron, didn't you? Be careful. I've seen what happens to those who can't balance their obligations.")
                .WithGuidance("You've learned to survive through relationships, obligations, and difficult choices. But patronage brings its own chains. Your queue will never be simple again.")
                .CompletesWhenFlagSet("tutorial_completed"))
                
            // Rewards
            .WithRewards(rewards => rewards
                .WithCoins(20)
                .WithStamina(10)
                .WithMessage("You've survived your first days as a Wayfarer!"))
                
            // Restricted NPCs (appear as tutorial progresses)
            .WithRestrictedNPCs("patron_intermediary", "guild_merchant", "noble_contact", "shadow_dealer")
            
            .Build();
    }
    
    /// <summary>
    /// Example of a simple quest narrative
    /// </summary>
    public static NarrativeDefinition CreateMerchantQuest()
    {
        return new NarrativeBuilder("merchant_favor", "A Merchant's Trust")
            .WithDescription("Help Marcus expand his trade network")
            .WithIntroduction("Marcus pulls you aside. 'I need someone discreet for a delicate matter.'")
            
            .AddStep("accept_quest", "Accept the Task", step => step
                .RequiresAction(LocationAction.Converse)
                .WithNPC("marcus_merchant")
                .WithConversationIntro("This isn't ordinary merchant business. Are you interested?"))
                
            .AddStep("collect_documents", "Collect Trade Documents", step => step
                .RequiresAction(LocationAction.Collect)
                .AtLocation("merchant_guild")
                .WithGuidance("The guild keeps these documents under close watch."))
                
            .AddStep("deliver_secretly", "Secret Delivery", step => step
                .RequiresAction(LocationAction.Deliver)
                .WithNPC("shadow_trader")
                .WithGuidance("Deliver without drawing attention from other merchants."))
                
            .WithRewards(rewards => rewards
                .WithCoins(50)
                .WithMessage("Marcus smiles. 'You've earned more than coins today - you've earned trust.'"))
                
            .Build();
    }
}