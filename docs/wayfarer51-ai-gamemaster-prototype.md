# Revised Wayfarer's Resolve Implementation Plan

## I. Universal State Flag System (UEST)

First, let's implement the complete UEST system with all flag categories and opposing pairs:

```csharp
// Complete flag states as defined in the final design document
public enum FlagStates
{
    // Positional Flags
    AdvantageousPosition,
    DisadvantageousPosition,
    HiddenPosition,
    ExposedPosition,
    
    // Relational Flags
    TrustEstablished,
    DistrustTriggered,
    RespectEarned,
    HostilityProvoked,
    
    // Informational Flags
    InsightGained,
    SecretRevealed,
    DeceptionDetected,
    ConfusionCreated,
    
    // Tactical Flags
    SurpriseAchieved,
    PreparationCompleted,
    PathCleared,
    PathBlocked,
    ResourceSecured,
    
    // Environmental Flags
    AreaSecured,
    DistractionCreated,
    HazardNeutralized,
    ObstaclePresent,
    
    // Emotional Flags
    TensionIncreased,
    ConfidenceBuilt,
    FearInstilled,
    UrgencyCreated
}

// Initialize all flag definitions with proper opposing pairs
private void InitializeFlagDefinitions()
{
    // Positional flags
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.AdvantageousPosition,
        FlagCategories.Positional,
        FlagStates.DisadvantageousPosition
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.DisadvantageousPosition,
        FlagCategories.Positional,
        FlagStates.AdvantageousPosition
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.HiddenPosition,
        FlagCategories.Positional,
        FlagStates.ExposedPosition
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.ExposedPosition,
        FlagCategories.Positional,
        FlagStates.HiddenPosition
    ));
    
    // Relational flags
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.TrustEstablished,
        FlagCategories.Relational,
        FlagStates.DistrustTriggered
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.DistrustTriggered,
        FlagCategories.Relational,
        FlagStates.TrustEstablished
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.RespectEarned,
        FlagCategories.Relational,
        FlagStates.HostilityProvoked
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.HostilityProvoked,
        FlagCategories.Relational,
        FlagStates.RespectEarned
    ));
    
    // Informational flags
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.InsightGained,
        FlagCategories.Informational,
        FlagStates.ConfusionCreated
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.ConfusionCreated,
        FlagCategories.Informational,
        FlagStates.InsightGained
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.SecretRevealed,
        FlagCategories.Informational,
        FlagStates.DeceptionDetected
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.DeceptionDetected,
        FlagCategories.Informational,
        FlagStates.SecretRevealed
    ));
    
    // Tactical flags - some don't have natural opposites
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.PathCleared,
        FlagCategories.Tactical,
        FlagStates.PathBlocked
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.PathBlocked,
        FlagCategories.Tactical,
        FlagStates.PathCleared
    ));
    
    // Non-opposing tactical flags
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.SurpriseAchieved,
        FlagCategories.Tactical,
        FlagStates.SurpriseAchieved // Self-opposing indicates no natural opposite
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.PreparationCompleted,
        FlagCategories.Tactical,
        FlagStates.PreparationCompleted
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.ResourceSecured,
        FlagCategories.Tactical,
        FlagStates.ResourceSecured
    ));
    
    // Environmental flags
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.AreaSecured,
        FlagCategories.Environmental,
        FlagStates.ObstaclePresent
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.ObstaclePresent,
        FlagCategories.Environmental,
        FlagStates.AreaSecured
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.DistractionCreated,
        FlagCategories.Environmental,
        FlagStates.DistractionCreated
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.HazardNeutralized,
        FlagCategories.Environmental,
        FlagStates.HazardNeutralized
    ));
    
    // Emotional flags
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.ConfidenceBuilt,
        FlagCategories.Emotional,
        FlagStates.FearInstilled
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.FearInstilled,
        FlagCategories.Emotional,
        FlagStates.ConfidenceBuilt
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.TensionIncreased,
        FlagCategories.Emotional,
        FlagStates.TensionIncreased
    ));
    
    flagDefinitions.Add(new FlagDefinition(
        FlagStates.UrgencyCreated,
        FlagCategories.Emotional,
        FlagStates.UrgencyCreated
    ));
}
```

## II. Payload System Implementation

Let's update the payload system to match the AI's expected JSON structure:

```csharp
// Payload registry initialization with exact string IDs
public void InitializePayloadRegistry()
{
    // State flag payloads
    payloadRegistry.RegisterEffect("SET_FLAG_TRUST_ESTABLISHED", 
        new SetFlagEffect(FlagStates.TrustEstablished));
    payloadRegistry.RegisterEffect("SET_FLAG_INSIGHT_GAINED", 
        new SetFlagEffect(FlagStates.InsightGained));
    payloadRegistry.RegisterEffect("SET_FLAG_PATH_CLEARED", 
        new SetFlagEffect(FlagStates.PathCleared));
    payloadRegistry.RegisterEffect("CLEAR_FLAG_DISTRUST_TRIGGERED", 
        new ClearFlagEffect(FlagStates.DistrustTriggered));
    
    // Focus payloads
    payloadRegistry.RegisterEffect("GAIN_FOCUS_1", 
        new FocusChangeEffect(1));
    payloadRegistry.RegisterEffect("GAIN_FOCUS_2", 
        new FocusChangeEffect(2));
    payloadRegistry.RegisterEffect("LOSE_FOCUS_1", 
        new FocusChangeEffect(-1));
    payloadRegistry.RegisterEffect("LOSE_FOCUS_2", 
        new FocusChangeEffect(-2));
    
    // Duration payloads
    payloadRegistry.RegisterEffect("ADVANCE_DURATION_1", 
        new DurationAdvanceEffect(1));
    payloadRegistry.RegisterEffect("ADVANCE_DURATION_2", 
        new DurationAdvanceEffect(2));
    
    // Currency payloads (tiered)
    payloadRegistry.RegisterEffect("GAIN_CURRENCY_MINOR", 
        new CurrencyChangeEffect(1, 3));
    payloadRegistry.RegisterEffect("GAIN_CURRENCY_MODERATE", 
        new CurrencyChangeEffect(4, 7));
    payloadRegistry.RegisterEffect("GAIN_CURRENCY_MAJOR", 
        new CurrencyChangeEffect(8, 10));
    payloadRegistry.RegisterEffect("LOSE_CURRENCY_MINOR", 
        new CurrencyChangeEffect(-3, -1));
    payloadRegistry.RegisterEffect("LOSE_CURRENCY_MODERATE", 
        new CurrencyChangeEffect(-7, -4));
    payloadRegistry.RegisterEffect("LOSE_CURRENCY_MAJOR", 
        new CurrencyChangeEffect(-10, -8));
    
    // Skill check modifiers
    payloadRegistry.RegisterEffect("BUFF_NEXT_CHECK_1", 
        new NextCheckModifierEffect(1));
    payloadRegistry.RegisterEffect("BUFF_NEXT_CHECK_2", 
        new NextCheckModifierEffect(2));
    payloadRegistry.RegisterEffect("DEBUFF_NEXT_CHECK_1", 
        new NextCheckModifierEffect(-1));
    payloadRegistry.RegisterEffect("DEBUFF_NEXT_CHECK_2", 
        new NextCheckModifierEffect(-2));
    
    // Relationship payloads
    payloadRegistry.RegisterEffect("MODIFY_RELATIONSHIP_POSITIVE_MINOR", 
        new RelationshipModifierEffect(5, "Recent positive interaction"));
    payloadRegistry.RegisterEffect("MODIFY_RELATIONSHIP_POSITIVE_MODERATE", 
        new RelationshipModifierEffect(10, "Significant positive interaction"));
    payloadRegistry.RegisterEffect("MODIFY_RELATIONSHIP_POSITIVE_MAJOR", 
        new RelationshipModifierEffect(15, "Major positive development"));
    payloadRegistry.RegisterEffect("MODIFY_RELATIONSHIP_NEGATIVE_MINOR", 
        new RelationshipModifierEffect(-5, "Recent negative interaction"));
    payloadRegistry.RegisterEffect("MODIFY_RELATIONSHIP_NEGATIVE_MODERATE", 
        new RelationshipModifierEffect(-10, "Significant negative interaction"));
    payloadRegistry.RegisterEffect("MODIFY_RELATIONSHIP_NEGATIVE_MAJOR", 
        new RelationshipModifierEffect(-15, "Major negative development"));
    
    // Special recovery payload for 0-Focus options
    payloadRegistry.RegisterEffect("RECOVERY_SUCCESS", 
        new RecoveryEffect(true));
    payloadRegistry.RegisterEffect("RECOVERY_FAILURE", 
        new RecoveryEffect(false));
    
    // Compound effects for common combinations
    payloadRegistry.RegisterEffect("SUCCESS_WITH_INSIGHT", 
        new CompoundEffect(new List<IMechanicalEffect> {
            new SetFlagEffect(FlagStates.InsightGained),
            new FocusChangeEffect(1)
        }));
}

// New effect implementations
public class DurationAdvanceEffect : IMechanicalEffect
{
    private int amount;
    
    public DurationAdvanceEffect(int amount)
    {
        this.amount = amount;
    }
    
    public void Apply(EncounterState state)
    {
        state.AdvanceDuration(amount);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Advances encounter duration by {amount}";
    }
}

public class NextCheckModifierEffect : IMechanicalEffect
{
    private int modifierValue;
    
    public NextCheckModifierEffect(int modifierValue)
    {
        this.modifierValue = modifierValue;
    }
    
    public void Apply(EncounterState state)
    {
        state.SetNextCheckModifier(modifierValue);
    }
    
    public string GetDescriptionForPlayer()
    {
        string direction = modifierValue >= 0 ? "+" : "";
        return $"{direction}{modifierValue} to next skill check";
    }
}

public class RelationshipModifierEffect : IMechanicalEffect
{
    private int amount;
    private string source;
    
    public RelationshipModifierEffect(int amount, string source)
    {
        this.amount = amount;
        this.source = source;
    }
    
    public void Apply(EncounterState state)
    {
        // The NPC targeted by this effect would come from context
        // For now, we'll assume it's stored in the state's CurrentNPC
        if (state.CurrentNPC != null)
        {
            state.Player.ModifyRelationship(state.CurrentNPC.ID, amount, source);
        }
    }
    
    public string GetDescriptionForPlayer()
    {
        if (amount > 0)
        {
            return $"Improves relationship by {amount}";
        }
        else
        {
            return $"Worsens relationship by {Math.Abs(amount)}";
        }
    }
}

public class RecoveryEffect : IMechanicalEffect
{
    private bool success;
    
    public RecoveryEffect(bool success)
    {
        this.success = success;
    }
    
    public void Apply(EncounterState state)
    {
        if (success)
        {
            // Recovery success gives +1 Focus
            state.FocusPoints = Math.Min(state.FocusPoints + 1, state.MaxFocusPoints);
            
            // Track consecutive recoveries for diminishing returns
            state.ConsecutiveRecoveryCount++;
        }
        else
        {
            // Recovery failure advances duration by an additional 1 unit
            state.AdvanceDuration(1);
        }
    }
    
    public string GetDescriptionForPlayer()
    {
        if (success)
        {
            return "Regain 1 Focus Point";
        }
        else
        {
            return "Wastes valuable time";
        }
    }
}

public class CompoundEffect : IMechanicalEffect
{
    private List<IMechanicalEffect> effects;
    
    public CompoundEffect(List<IMechanicalEffect> effects)
    {
        this.effects = effects;
    }
    
    public void Apply(EncounterState state)
    {
        foreach (IMechanicalEffect effect in effects)
        {
            effect.Apply(state);
        }
    }
    
    public string GetDescriptionForPlayer()
    {
        StringBuilder description = new StringBuilder();
        
        for (int i = 0; i < effects.Count; i++)
        {
            if (i > 0)
            {
                description.Append(", ");
            }
            
            description.Append(effects[i].GetDescriptionForPlayer());
        }
        
        return description.ToString();
    }
}
```

## III. Skill System Implementation

Now let's update the skill system to match the 10 core specialized skill cards:

```csharp
// Updated skill types to match the 10 core specialized cards
public enum SkillTypes
{
    // Physical skills
    BruteForce,
    Acrobatics,
    Lockpicking,
    
    // Intellectual skills
    Investigation,
    Perception,
    Strategy,
    
    // Social skills
    Etiquette,
    Negotiation,
    Acting,
    Threatening
}

// Specialized skill card data
public class SkillCardDefinition
{
    public string Name { get; }
    public SkillTypes SkillType { get; }
    public SkillCategories Category { get; }
    public string Description { get; }
    
    public SkillCardDefinition(string name, SkillTypes skillType, SkillCategories category, string description)
    {
        Name = name;
        SkillType = skillType;
        Category = category;
        Description = description;
    }
}

// Initialize the 10 core skill cards
public void InitializeSkillCards()
{
    List<SkillCardDefinition> coreSkillCards = new List<SkillCardDefinition>
    {
        // Physical skills
        new SkillCardDefinition(
            "Brute Force",
            SkillTypes.BruteForce,
            SkillCategories.Physical,
            "Apply raw strength to overcome physical obstacles."
        ),
        
        new SkillCardDefinition(
            "Acrobatics",
            SkillTypes.Acrobatics,
            SkillCategories.Physical,
            "Move with agility and grace to navigate difficult terrain."
        ),
        
        new SkillCardDefinition(
            "Lockpicking",
            SkillTypes.Lockpicking,
            SkillCategories.Physical,
            "Manipulate mechanisms to open locks and disarm traps."
        ),
        
        // Intellectual skills
        new SkillCardDefinition(
            "Investigation",
            SkillTypes.Investigation,
            SkillCategories.Intellectual,
            "Carefully examine evidence to uncover hidden information."
        ),
        
        new SkillCardDefinition(
            "Perception",
            SkillTypes.Perception,
            SkillCategories.Intellectual,
            "Notice details and patterns that others might miss."
        ),
        
        new SkillCardDefinition(
            "Strategy",
            SkillTypes.Strategy,
            SkillCategories.Intellectual,
            "Plan and anticipate to gain tactical advantages."
        ),
        
        // Social skills
        new SkillCardDefinition(
            "Etiquette",
            SkillTypes.Etiquette,
            SkillCategories.Social,
            "Navigate social situations with grace and proper decorum."
        ),
        
        new SkillCardDefinition(
            "Negotiation",
            SkillTypes.Negotiation,
            SkillCategories.Social,
            "Persuade others through reasoned arguments and compromise."
        ),
        
        new SkillCardDefinition(
            "Acting",
            SkillTypes.Acting,
            SkillCategories.Social,
            "Convincingly portray emotions or personas to deceive others."
        ),
        
        new SkillCardDefinition(
            "Threatening",
            SkillTypes.Threatening,
            SkillCategories.Social,
            "Intimidate others through implied or explicit force."
        )
    };
    
    // Register these with the skill card manager
    foreach (SkillCardDefinition definition in coreSkillCards)
    {
        skillCardManager.RegisterSkillCard(definition);
    }
}

// Create a player's skill card instance from a definition
public SkillCard CreateSkillCard(SkillCardDefinition definition, int level)
{
    return new SkillCard(
        definition.Name,
        definition.SkillType,
        definition.Category,
        level,
        definition.Description
    );
}

// Handle untrained attempts explicitly
public SkillCheckResult PerformSkillCheck(EncounterState state, SkillTypes requiredSkill, int difficulty)
{
    // Try to find the required skill card in player's hand
    SkillCard card = state.Player.GetCard(requiredSkill);
    
    if (card != null && !card.IsExhausted)
    {
        // Player has the required card and it's not exhausted
        int effectiveLevel = card.GetEffectiveLevel(state);
        bool success = effectiveLevel >= difficulty;
        
        // Exhaust the card
        card.Exhaust();
        
        return new SkillCheckResult(
            requiredSkill,
            effectiveLevel,
            difficulty,
            success,
            false // Not untrained
        );
    }
    else
    {
        // Untrained attempt
        int effectiveLevel = 0; // Base level for untrained
        int untrainedDifficulty = difficulty + 2; // +2 difficulty for untrained
        bool success = effectiveLevel >= untrainedDifficulty;
        
        return new SkillCheckResult(
            requiredSkill,
            effectiveLevel,
            untrainedDifficulty,
            success,
            true // Untrained attempt
        );
    }
}

// Skill check result for UI display
public class SkillCheckResult
{
    public SkillTypes Skill { get; }
    public int EffectiveLevel { get; }
    public int Difficulty { get; }
    public bool IsSuccess { get; }
    public bool IsUntrained { get; }
    
    public SkillCheckResult(SkillTypes skill, int effectiveLevel, int difficulty, bool isSuccess, bool isUntrained)
    {
        Skill = skill;
        EffectiveLevel = effectiveLevel;
        Difficulty = difficulty;
        IsSuccess = isSuccess;
        IsUntrained = isUntrained;
    }
    
    public string GetDifficultyLabel()
    {
        int difference = EffectiveLevel - Difficulty;
        
        if (difference >= 2) return "Trivial (100%)";
        if (difference == 1) return "Easy (75%)";
        if (difference == 0) return "Standard (50%)";
        if (difference == -1) return "Hard (25%)";
        return "Exceptional (5%)";
    }
    
    public int GetSuccessChance()
    {
        int difference = EffectiveLevel - Difficulty;
        
        if (difference >= 2) return 100;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 25;
        return 5;
    }
}
```

## IV. Encounter State Implementation

Let's revise the EncounterState to remove progress markers and focus purely on UEST-based goal assessment:

```csharp
// Revised encounter state without progress markers
public class EncounterState
{
    // Core state tracking
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; }
    public int DurationCounter { get; private set; }
    public int MaxDuration { get; }
    public int ConsecutiveRecoveryCount { get; set; }
    public bool IsEncounterComplete { get; set; }
    public int EncounterSeed { get; }
    
    // Flag management
    public EncounterFlagManager FlagManager { get; }
    
    // Current context
    public NPC CurrentNPC { get; set; }
    
    // Skill modifiers
    private List<SkillModifier> activeModifiers = new List<SkillModifier>();
    private int nextCheckModifier = 0;
    
    // Player reference
    public Player Player { get; }
    
    // Success determination - AI defines what flags constitute success
    private List<FlagStates> goalFlags = new List<FlagStates>();
    
    public EncounterState(Player player, int maxFocusPoints, int maxDuration, List<FlagStates> goalFlags)
    {
        Player = player;
        MaxFocusPoints = maxFocusPoints;
        FocusPoints = maxFocusPoints;
        MaxDuration = maxDuration;
        DurationCounter = 0;
        ConsecutiveRecoveryCount = 0;
        IsEncounterComplete = false;
        FlagManager = new EncounterFlagManager();
        this.goalFlags = goalFlags;
        
        // Set encounter seed for deterministic randomness
        EncounterSeed = Environment.TickCount;
    }
    
    public void AdvanceDuration(int amount)
    {
        DurationCounter += amount;
        
        // Check for duration limit
        if (DurationCounter >= MaxDuration)
        {
            IsEncounterComplete = true;
        }
    }
    
    public void CheckGoalCompletion()
    {
        // Simple goal check: are all required flags active?
        bool allGoalFlagsActive = true;
        
        foreach (FlagStates flag in goalFlags)
        {
            if (!FlagManager.IsActive(flag))
            {
                allGoalFlagsActive = false;
                break;
            }
        }
        
        if (allGoalFlagsActive)
        {
            IsEncounterComplete = true;
        }
    }
    
    public void AddModifier(SkillModifier modifier)
    {
        activeModifiers.Add(modifier);
    }
    
    public void SetNextCheckModifier(int modifier)
    {
        nextCheckModifier = modifier;
    }
    
    public int GetNextCheckModifier()
    {
        int modifier = nextCheckModifier;
        nextCheckModifier = 0; // Reset after retrieval
        return modifier;
    }
    
    public void ProcessModifiers()
    {
        // Decrement durations and remove expired modifiers
        for (int i = activeModifiers.Count - 1; i >= 0; i--)
        {
            activeModifiers[i].DecrementDuration();
            if (activeModifiers[i].HasExpired())
            {
                activeModifiers.RemoveAt(i);
            }
        }
    }
    
    public List<SkillModifier> GetActiveModifiers(SkillTypes skillType)
    {
        List<SkillModifier> result = new List<SkillModifier>();
        
        foreach (SkillModifier modifier in activeModifiers)
        {
            if (modifier.TargetSkill == skillType)
            {
                result.Add(modifier);
            }
        }
        
        return result;
    }
    
    public int GetDeterministicRandom(int minValue, int maxValue)
    {
        // Use encounter seed for deterministic random
        Random random = new Random(EncounterSeed + DurationCounter);
        return random.Next(minValue, maxValue);
    }
}
```

## V. Choice Projection Service

Now, let's update the ChoiceProjectionService to work with the AI's structured JSON format:

```csharp
// Choice projection service for AI's JSON choice structure
public class ChoiceProjectionService
{
    private PayloadRegistry payloadRegistry;
    
    public ChoiceProjectionService(PayloadRegistry payloadRegistry)
    {
        this.payloadRegistry = payloadRegistry;
    }
    
    // Project the outcome of a choice based on AI's JSON
    public ChoiceProjection ProjectChoice(AIChoice choice, EncounterState state)
    {
        ChoiceProjection projection = new ChoiceProjection();
        projection.ChoiceID = choice.ChoiceID;
        projection.NarrativeText = choice.NarrativeText;
        projection.FocusCost = choice.FocusCost;
        
        // Check if player can afford this choice
        projection.IsAffordable = state.FocusPoints >= choice.FocusCost;
        
        // Process skill options
        List<SkillOptionProjection> skillProjections = new List<SkillOptionProjection>();
        
        foreach (SkillOption option in choice.SkillOptions)
        {
            SkillOptionProjection skillProjection = ProjectSkillOption(option, state);
            skillProjections.Add(skillProjection);
        }
        
        projection.SkillOptions = skillProjections;
        
        return projection;
    }
    
    // Project a specific skill option
    private SkillOptionProjection ProjectSkillOption(SkillOption option, EncounterState state)
    {
        SkillOptionProjection projection = new SkillOptionProjection();
        projection.SkillName = option.SkillName;
        projection.Difficulty = option.Difficulty;
        projection.SCD = option.SCD;
        
        // Find matching skill card
        SkillCard card = FindCardByName(state.Player.AvailableCards, option.SkillName);
        
        if (card != null && !card.IsExhausted)
        {
            // Player has the card and it's not exhausted
            projection.IsAvailable = true;
            projection.IsUntrained = false;
            projection.EffectiveLevel = card.GetEffectiveLevel(state);
        }
        else
        {
            // Untrained attempt
            projection.IsAvailable = true; // Still available, but untrained
            projection.IsUntrained = true;
            projection.EffectiveLevel = 0; // Base level for untrained
            projection.SCD = option.SCD + 2; // +2 difficulty for untrained
        }
        
        // Calculate success chance
        projection.SuccessChance = CalculateSuccessChance(projection.EffectiveLevel, projection.SCD);
        
        // Project payloads
        projection.SuccessPayload = ProjectPayload(option.SuccessPayload, state);
        projection.FailurePayload = ProjectPayload(option.FailurePayload, state);
        
        return projection;
    }
    
    // Project payload effects
    private PayloadProjection ProjectPayload(Payload payload, EncounterState state)
    {
        PayloadProjection projection = new PayloadProjection();
        projection.NarrativeEffect = payload.NarrativeEffect;
        
        // Get mechanical effect from registry
        if (payloadRegistry.HasEffect(payload.MechanicalEffectID))
        {
            IMechanicalEffect effect = payloadRegistry.GetEffect(payload.MechanicalEffectID);
            projection.MechanicalDescription = effect.GetDescriptionForPlayer();
        }
        else
        {
            projection.MechanicalDescription = "Unknown effect";
        }
        
        return projection;
    }
    
    // Calculate success chance based on effective level vs difficulty
    private int CalculateSuccessChance(int effectiveLevel, int difficulty)
    {
        int difference = effectiveLevel - difficulty;
        
        if (difference >= 2) return 100;
        if (difference == 1) return 75;
        if (difference == 0) return 50;
        if (difference == -1) return 25;
        return 5; // Not impossible, but very unlikely
    }
    
    // Helper to find card by name
    private SkillCard FindCardByName(List<SkillCard> cards, string name)
    {
        foreach (SkillCard card in cards)
        {
            if (card.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !card.IsExhausted)
            {
                return card;
            }
        }
        
        return null;
    }
}

// Projection data structures
public class ChoiceProjection
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int FocusCost { get; set; }
    public bool IsAffordable { get; set; }
    public List<SkillOptionProjection> SkillOptions { get; set; }
}

public class SkillOptionProjection
{
    public string SkillName { get; set; }
    public string Difficulty { get; set; }
    public int SCD { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsUntrained { get; set; }
    public int EffectiveLevel { get; set; }
    public int SuccessChance { get; set; }
    public PayloadProjection SuccessPayload { get; set; }
    public PayloadProjection FailurePayload { get; set; }
}

public class PayloadProjection
{
    public string NarrativeEffect { get; set; }
    public string MechanicalDescription { get; set; }
}
```

## VI. AI Response Processing

Finally, let's implement the system to process the AI's JSON response:

```csharp
// AI response processor
public class AIResponseProcessor
{
    private PayloadRegistry payloadRegistry;
    
    public AIResponseProcessor(PayloadRegistry payloadRegistry)
    {
        this.payloadRegistry = payloadRegistry;
    }
    
    // Process the selected choice and skill option
    public void ProcessChoice(AIChoice choice, string skillOptionName, EncounterState state)
    {
        // First, deduct Focus cost
        state.FocusPoints -= choice.FocusCost;
        
        // Find the selected skill option
        SkillOption selectedOption = null;
        foreach (SkillOption option in choice.SkillOptions)
        {
            if (option.SkillName.Equals(skillOptionName, StringComparison.OrdinalIgnoreCase))
            {
                selectedOption = option;
                break;
            }
        }
        
        if (selectedOption == null)
        {
            throw new InvalidOperationException($"Skill option {skillOptionName} not found in choice {choice.ChoiceID}");
        }
        
        // Find the skill card
        SkillCard card = FindCardByName(state.Player.AvailableCards, selectedOption.SkillName);
        bool isUntrained = (card == null || card.IsExhausted);
        
        // Perform skill check
        int effectiveLevel = 0;
        int difficulty = selectedOption.SCD;
        
        if (!isUntrained && card != null)
        {
            // Using a skill card
            effectiveLevel = card.GetEffectiveLevel(state);
            card.Exhaust(); // Exhaust the card
        }
        else
        {
            // Untrained attempt
            effectiveLevel = 0;
            difficulty += 2; // +2 difficulty for untrained
        }
        
        // Add any next check modifier
        effectiveLevel += state.GetNextCheckModifier();
        
        // Determine success
        bool success = effectiveLevel >= difficulty;
        
        // Apply appropriate payload
        if (success)
        {
            ApplyPayload(selectedOption.SuccessPayload.MechanicalEffectID, state);
        }
        else
        {
            ApplyPayload(selectedOption.FailurePayload.MechanicalEffectID, state);
        }
        
        // If this was a recovery action (0 Focus cost), increment consecutive recovery count
        if (choice.FocusCost == 0)
        {
            state.ConsecutiveRecoveryCount++;
        }
        else
        {
            // Reset consecutive recovery count for non-recovery actions
            state.ConsecutiveRecoveryCount = 0;
        }
        
        // Process skill modifiers
        state.ProcessModifiers();
        
        // Check if goal has been achieved
        state.CheckGoalCompletion();
        
        // Advance duration - basic duration advance for any action
        state.AdvanceDuration(1);
    }
    
    // Apply a payload effect
    private void ApplyPayload(string payloadID, EncounterState state)
    {
        if (payloadRegistry.HasEffect(payloadID))
        {
            IMechanicalEffect effect = payloadRegistry.GetEffect(payloadID);
            effect.Apply(state);
        }
    }
    
    // Helper to find card by name
    private SkillCard FindCardByName(List<SkillCard> cards, string name)
    {
        foreach (SkillCard card in cards)
        {
            if (card.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && !card.IsExhausted)
            {
                return card;
            }
        }
        
        return null;
    }
}
```
