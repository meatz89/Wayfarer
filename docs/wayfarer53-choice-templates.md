The payload system (and thus payload IDs as dynamic lookup keys) is no longer needed.
 * This is because the Choice Template system directly defines the singular, specific mechanical effect for each outcome (success/failure).
 * Every single mechanical effect is a separate C# class.

This radically simplifies how mechanical consequences are determined and executed, making the templates even more central and self-contained. The AI's role is to choose a template, and that choice inherently dictates the precise, non-generic mechanical effect that will occur.
Let's construct the high-level concept overview reflecting this direct, class-based approach to mechanical effects, fully driven by the template definition.
Wayfarer: AI-Driven Narrative Encounters with Directly Enforced Template Mechanics and a Polling UI
The core concept for Wayfarer is a game system where an AI Game Master (AI GM) generates dynamic, narratively rich encounters. The system masterfully balances the AI's storytelling capabilities with a highly structured mechanical framework. This is primarily achieved through a Choice Template system, where each template selected by the AI directly dictates the single, specific C# class representing the mechanical effect for its success or failure outcome, alongside its input requirements.

Key Conceptual Pillars (Reflecting All Your Inputs, Especially the Latest Clarification on Effects):
 * AI-Powered Original Narrative Generation:
   * The AI GM is the central creative force, generating unique narrative beats, descriptive text for player choices, and distinct narrative outcomes for both success and failure. This narrative is originally crafted by the AI, guided by its interpretation of the selected template's structured definition (which is provided to it as JSON).
 * Backend State Management & Orchestration:
   * A robust C#-based backend (driven by a GameWorldManager and supported by an EncounterSystem) manages all game logic and the canonical GameWorld state.
   * GameWorld includes player data, world state, active encounter state, and a dedicated StreamingContentState for AI-generated text.
 * Polling Blazor UI (Decoupled Frontend):
   * The Blazor UI operates on a strict polling mechanism, querying the backend's GameState (via a snapshot from GameWorldManager) at regular intervals (e.g., every ~100ms) to display changes.
   * No direct backend-to-UI events or notifications are used for state updates; the UI pulls information.
 * Streaming AI Responses for Incremental UI Updates:
   * AI-generated text is streamed token-by-token into GameWorld.StreamingContentState.
   * The polling UI displays these incremental updates, creating a live text effect.
   * An AIStreamingService (or similar component within AIGameMaster) handles the streaming protocol.
 * Choice Templates: Self-Contained Packages of Mechanics & Conceptual Guidance (via JSON):
   * A catalog of ChoiceTemplate C# objects defines the available choice archetypes.
   * Each ChoiceTemplate contains:
     * A unique TemplateName (ID).
     * A StrategicPurpose and descriptive hints for AI understanding.
     * A Weight (hinting at desired frequency for AI selection).
     * Complex C# objects detailing InputMechanics (e.g., specific FocusCost, SkillCheckRequirement with concrete SCD and skill category), serialized to JSON for the AI.
     * A direct reference to the specific C# class (e.g., via System.Type or a specific enum mapping to a class) for the single mechanical effect upon success (e.g., Type SuccessEffectClass = typeof(SetStandardProgressFlagEffect);).
     * A direct reference to the specific C# class for the single mechanical effect upon failure (e.g., Type FailureEffectClass = typeof(ApplyMinorSetbackEffect);).
     * Conceptual descriptions of the output and success/failure consequences (e.g., ConceptualOutput, SuccessOutcomeNarrativeGuidance) to guide the AI's narrative generation around these fixed mechanical effects. These are also provided as JSON to the AI.
   * Templates do NOT contain generic strings for effects or lists of payload IDs. They point directly to the code that is the effect.
 * AI Autonomy in Template Selection & Narrative Instantiation (Mechanics are Template-Fixed):
   * For each encounter beat, the AI GM receives the entire catalog of all available ChoiceTemplate objects, with their input mechanics and conceptual outputs presented as embedded JSON structures, within its prompt.
   * The AI is instructed to select 3-4 appropriate templates.
   * For each selected template, the AI generates:
     * Original narrative text for the player choice.
     * Original narrative text for the success and failure outcomes.
     * The specific mechanical values for the choice's inputs (focusCost, skillOptions including SCD), which it must derive directly from the JSON definition of the chosen template's InputMechanics.
     * The AI must specify the templateUsed (by TemplateName) in its response.
     * The AI does NOT specify the mechanical outcome/effect. This is now entirely determined by the system looking up the SuccessEffectClass or FailureEffectClass associated with the templateUsed.
   * The game system does not pre-filter templates for the AI, nor does it validate or correct the AI's output for input mechanics (like focus cost or SCD). It trusts the AI to have correctly instantiated these from the template's JSON definition.
 * Structured AI Interaction & Response Processing:
   * The AIPromptBuilder provides the AI with game context and the full template catalog (as JSON objects defining inputs and conceptual outputs/guidance). Clear instructions are given on selecting templates, instantiating their input mechanics, generating original narrative, and structuring the JSON response (including templateUsed, focusCost, SCD).
   * The AIResponseProcessor parses the AI's JSON response. For each AI choice, it identifies the templateUsed.
 * Direct Execution of Template-Defined Effect Classes:
   * Core game elements (Focus Points, Skills, UESTs) are manipulated.
   * When a player's choice (based on an AI-selected template) is resolved (e.g., skill check succeeds or fails):
     * The system retrieves the ChoiceTemplate object corresponding to the templateUsed by the AI.
     * It then instantiates and executes the single, specific effect class (e.g., template.SuccessEffectClass.Apply(...) or template.FailureEffectClass.Apply(...)) associated with that template's outcome.
   * There is no PayloadRegistry for string ID lookups. The "payload" is now an actual C# effect class directly tied to the template definition.

This refined understanding places the Choice Template as an even more powerful and direct controller of game mechanics. The AI's role is to select these potent template packages, generate compelling original narrative around their fixed input mechanics and conceptual outcomes, and correctly specify the input mechanics in its response. The system then executes the specific, singular C# effect class hardwired to that template's success or failure path. This ensures absolute mechanical consistency per template choice.

# Integrated Wayfarer AI-Driven Encounter System

I've thoroughly reviewed the project documentation and developed an integrated implementation that aligns with your architectural vision. This implementation eliminates the payload system in favor of direct template-to-effect connections, creates a polling-based UI architecture, and properly handles streaming content.

## Core System Architecture

The system is built around these key components:

1. **Choice Templates** that directly reference specific effect classes
2. **Mechanical Effects** as discrete C# classes with specific responsibilities
3. **Universal State Flags** for tracking encounter state
4. **Streaming Content** for incremental UI updates
5. **Polling-Based UI** that queries the backend at regular intervals

Let's implement each component:

## 1. Universal State Flag System (UEST)

```csharp
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
    ObstaclePresent,
    
    // Environmental Flags
    AreaSecured,
    DistractionCreated,
    HazardNeutralized,
    HazardPresent,
    
    // Emotional Flags
    TensionIncreased,
    ConfidenceBuilt,
    FearInstilled,
    UrgencyCreated
}

public enum FlagCategories
{
    Positional,
    Relational,
    Informational,
    Tactical,
    Environmental,
    Emotional
}

public class FlagDefinition
{
    public FlagStates FlagState { get; private set; }
    public FlagCategories Category { get; private set; }
    public FlagStates OpposingFlag { get; private set; }
    
    public FlagDefinition(FlagStates flagState, FlagCategories category, FlagStates opposingFlag)
    {
        FlagState = flagState;
        Category = category;
        OpposingFlag = opposingFlag;
    }
}

public class EncounterFlagManager
{
    private List<FlagStates> activeFlags;
    private List<FlagDefinition> flagDefinitions;
    
    public EncounterFlagManager(List<FlagDefinition> flagDefinitions)
    {
        this.activeFlags = new List<FlagStates>();
        this.flagDefinitions = flagDefinitions;
    }
    
    public void SetFlag(FlagStates flag)
    {
        // Find flag definition
        FlagDefinition definition = GetFlagDefinition(flag);
        
        // Clear opposing flag if one exists
        if (activeFlags.Contains(definition.OpposingFlag))
        {
            activeFlags.Remove(definition.OpposingFlag);
        }
        
        // Add flag if not already active
        if (!activeFlags.Contains(flag))
        {
            activeFlags.Add(flag);
        }
    }
    
    public void ClearFlag(FlagStates flag)
    {
        if (activeFlags.Contains(flag))
        {
            activeFlags.Remove(flag);
        }
    }
    
    public bool IsActive(FlagStates flag)
    {
        return activeFlags.Contains(flag);
    }
    
    public List<FlagStates> GetActiveFlagsByCategory(FlagCategories category)
    {
        List<FlagStates> result = new List<FlagStates>();
        
        foreach (FlagStates flag in activeFlags)
        {
            FlagDefinition definition = GetFlagDefinition(flag);
            if (definition.Category == category)
            {
                result.Add(flag);
            }
        }
        
        return result;
    }
    
    public List<FlagStates> GetAllActiveFlags()
    {
        // Return a copy of the list to prevent external modification
        return new List<FlagStates>(activeFlags);
    }
    
    private FlagDefinition GetFlagDefinition(FlagStates flag)
    {
        foreach (FlagDefinition definition in flagDefinitions)
        {
            if (definition.FlagState == flag)
            {
                return definition;
            }
        }
        
        throw new ArgumentException($"No definition found for flag {flag}");
    }
}
```

## 2. Encounter State and Skill System

```csharp
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

public enum SkillCategories
{
    Physical,
    Intellectual,
    Social
}

public class SkillCard
{
    public string Name { get; private set; }
    public SkillTypes SkillType { get; private set; }
    public SkillCategories Category { get; private set; }
    public int Level { get; private set; }
    public string Description { get; private set; }
    public bool IsExhausted { get; private set; }
    
    public SkillCard(string name, SkillTypes skillType, SkillCategories category, int level, string description)
    {
        Name = name;
        SkillType = skillType;
        Category = category;
        Level = level;
        Description = description;
        IsExhausted = false;
    }
    
    public void Exhaust()
    {
        IsExhausted = true;
    }
    
    public void Refresh()
    {
        IsExhausted = false;
    }
    
    public int GetEffectiveLevel(EncounterState state)
    {
        // Start with base level
        int effectiveLevel = Level;
        
        // Add any modifiers from state
        effectiveLevel += state.GetModifiersForSkill(SkillType);
        
        return effectiveLevel;
    }
}

public class Player
{
    public List<SkillCard> AvailableCards { get; private set; }
    
    public Player()
    {
        AvailableCards = new List<SkillCard>();
    }
    
    public void AddCard(SkillCard card)
    {
        AvailableCards.Add(card);
    }
    
    public int GetSkillLevel(SkillCategories category)
    {
        // Find highest level non-exhausted card in the category
        int highestLevel = 0;
        
        foreach (SkillCard card in AvailableCards)
        {
            if (card.Category == category && !card.IsExhausted && card.Level > highestLevel)
            {
                highestLevel = card.Level;
            }
        }
        
        return highestLevel;
    }
    
    public int GetSkillLevel(SkillTypes skillType)
    {
        // Find the card with the specified skill type
        foreach (SkillCard card in AvailableCards)
        {
            if (card.SkillType == skillType && !card.IsExhausted)
            {
                return card.Level;
            }
        }
        
        return 0; // No card found or all are exhausted
    }
}

public class SkillModifier
{
    public SkillTypes TargetSkill { get; private set; }
    public int Value { get; private set; }
    public int RemainingDuration { get; private set; }
    
    public SkillModifier(SkillTypes targetSkill, int value, int duration)
    {
        TargetSkill = targetSkill;
        Value = value;
        RemainingDuration = duration;
    }
    
    public void DecrementDuration()
    {
        if (RemainingDuration > 0)
        {
            RemainingDuration--;
        }
    }
    
    public bool HasExpired()
    {
        return RemainingDuration <= 0;
    }
}

public class EncounterState
{
    // Core state tracking
    public int FocusPoints { get; set; }
    public int MaxFocusPoints { get; private set; }
    public int DurationCounter { get; private set; }
    public int MaxDuration { get; private set; }
    public int ConsecutiveRecoveryCount { get; set; }
    public bool IsEncounterComplete { get; private set; }
    
    // Flag management
    public EncounterFlagManager FlagManager { get; private set; }
    
    // Current context
    public NPC CurrentNPC { get; set; }
    
    // Skill modifiers
    private List<SkillModifier> activeModifiers;
    private int nextCheckModifier;
    
    // Player reference
    public Player Player { get; private set; }
    
    // Goal flags
    private List<FlagStates> goalFlags;
    
    public EncounterState(Player player, int maxFocusPoints, int maxDuration, List<FlagStates> goalFlags, List<FlagDefinition> flagDefinitions)
    {
        Player = player;
        MaxFocusPoints = maxFocusPoints;
        FocusPoints = maxFocusPoints;
        MaxDuration = maxDuration;
        DurationCounter = 0;
        ConsecutiveRecoveryCount = 0;
        IsEncounterComplete = false;
        FlagManager = new EncounterFlagManager(flagDefinitions);
        this.goalFlags = goalFlags;
        this.activeModifiers = new List<SkillModifier>();
        this.nextCheckModifier = 0;
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
    
    public void SetFlag(FlagStates flag)
    {
        FlagManager.SetFlag(flag);
        CheckGoalCompletion();
    }
    
    public void ClearFlag(FlagStates flag)
    {
        FlagManager.ClearFlag(flag);
    }
    
    public bool IsFlagSet(FlagStates flag)
    {
        return FlagManager.IsActive(flag);
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
    
    public int GetModifiersForSkill(SkillTypes skillType)
    {
        int totalModifier = 0;
        
        foreach (SkillModifier modifier in activeModifiers)
        {
            if (modifier.TargetSkill == skillType)
            {
                totalModifier += modifier.Value;
            }
        }
        
        return totalModifier;
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
    
    public void CompleteEncounter()
    {
        IsEncounterComplete = true;
    }
}
```

## 3. Mechanical Effects Implementation

```csharp
// Core interface for all mechanical effects
public interface IMechanicalEffect
{
    void Apply(EncounterState state);
    string GetDescriptionForPlayer();
}

// Flag effects
public class SetFlagEffect : IMechanicalEffect
{
    private FlagStates flagToSet;
    
    public SetFlagEffect(FlagStates flagToSet)
    {
        this.flagToSet = flagToSet;
    }
    
    public void Apply(EncounterState state)
    {
        state.SetFlag(flagToSet);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Sets {flagToSet} flag";
    }
}

public class ClearFlagEffect : IMechanicalEffect
{
    private FlagStates flagToClear;
    
    public ClearFlagEffect(FlagStates flagToClear)
    {
        this.flagToClear = flagToClear;
    }
    
    public void Apply(EncounterState state)
    {
        state.ClearFlag(flagToClear);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Clears {flagToClear} flag";
    }
}

// Focus effects
public class ModifyFocusEffect : IMechanicalEffect
{
    private int amount;
    
    public ModifyFocusEffect(int amount)
    {
        this.amount = amount;
    }
    
    public void Apply(EncounterState state)
    {
        if (amount > 0)
        {
            // Gain focus, capped at maximum
            state.FocusPoints = Math.Min(state.FocusPoints + amount, state.MaxFocusPoints);
        }
        else
        {
            // Lose focus, minimum 0
            state.FocusPoints = Math.Max(0, state.FocusPoints + amount);
        }
    }
    
    public string GetDescriptionForPlayer()
    {
        if (amount > 0)
        {
            return $"Gain {amount} Focus Points";
        }
        else
        {
            return $"Lose {Math.Abs(amount)} Focus Points";
        }
    }
}

// Duration effects
public class AdvanceDurationEffect : IMechanicalEffect
{
    private int amount;
    
    public AdvanceDurationEffect(int amount)
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

// Skill modifier effects
public class AddSkillModifierEffect : IMechanicalEffect
{
    private SkillTypes targetSkill;
    private int value;
    private int duration;
    
    public AddSkillModifierEffect(SkillTypes targetSkill, int value, int duration)
    {
        this.targetSkill = targetSkill;
        this.value = value;
        this.duration = duration;
    }
    
    public void Apply(EncounterState state)
    {
        state.AddModifier(new SkillModifier(targetSkill, value, duration));
    }
    
    public string GetDescriptionForPlayer()
    {
        string direction = value >= 0 ? "+" : "";
        return $"{direction}{value} to {targetSkill} for {duration} turns";
    }
}

// Combined effects
public class CompositeEffect : IMechanicalEffect
{
    private List<IMechanicalEffect> effects;
    private string description;
    
    public CompositeEffect(List<IMechanicalEffect> effects, string description)
    {
        this.effects = effects;
        this.description = description;
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
        return description;
    }
}

// Specific effect classes for direct template use
public class EstablishTrustEffect : IMechanicalEffect
{
    public void Apply(EncounterState state)
    {
        state.SetFlag(FlagStates.TrustEstablished);
    }
    
    public string GetDescriptionForPlayer()
    {
        return "Trust established with NPC";
    }
}

public class TriggerDistrustEffect : IMechanicalEffect
{
    public void Apply(EncounterState state)
    {
        state.SetFlag(FlagStates.DistrustTriggered);
    }
    
    public string GetDescriptionForPlayer()
    {
        return "Distrust triggered with NPC";
    }
}

public class GainInsightEffect : IMechanicalEffect
{
    public void Apply(EncounterState state)
    {
        state.SetFlag(FlagStates.InsightGained);
    }
    
    public string GetDescriptionForPlayer()
    {
        return "Gained valuable insight";
    }
}

public class CreateConfusionEffect : IMechanicalEffect
{
    public void Apply(EncounterState state)
    {
        state.SetFlag(FlagStates.ConfusionCreated);
    }
    
    public string GetDescriptionForPlayer()
    {
        return "Became confused by conflicting information";
    }
}

public class RecoverFocusEffect : IMechanicalEffect
{
    private int amount;
    
    public RecoverFocusEffect(int amount)
    {
        this.amount = amount;
    }
    
    public void Apply(EncounterState state)
    {
        state.FocusPoints = Math.Min(state.FocusPoints + amount, state.MaxFocusPoints);
        state.ConsecutiveRecoveryCount++;
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Recover {amount} Focus Points";
    }
}

public class WasteTimeEffect : IMechanicalEffect
{
    public void Apply(EncounterState state)
    {
        state.AdvanceDuration(1);
    }
    
    public string GetDescriptionForPlayer()
    {
        return "Waste valuable time";
    }
}
```

## 4. Choice Template System

```csharp
public class FocusCost
{
    public int Amount { get; private set; }
    
    public FocusCost(int amount)
    {
        Amount = amount;
    }
    
    // For JSON serialization
    public object ToJsonObject()
    {
        return new { Amount = Amount };
    }
}

public class SkillCheckRequirement
{
    public SkillCategories SkillCategory { get; private set; }
    public int StandardCheckDifficulty { get; private set; }
    
    public SkillCheckRequirement(SkillCategories skillCategory, int standardCheckDifficulty)
    {
        SkillCategory = skillCategory;
        StandardCheckDifficulty = standardCheckDifficulty;
    }
    
    // For JSON serialization
    public object ToJsonObject()
    {
        return new 
        { 
            SkillCategory = SkillCategory.ToString(), 
            StandardCheckDifficulty = StandardCheckDifficulty 
        };
    }
}

public class InputMechanics
{
    public FocusCost FocusCost { get; private set; }
    public SkillCheckRequirement SkillCheckRequirement { get; private set; }
    
    public InputMechanics(FocusCost focusCost, SkillCheckRequirement skillCheckRequirement)
    {
        FocusCost = focusCost;
        SkillCheckRequirement = skillCheckRequirement;
    }
    
    // For JSON serialization
    public object ToJsonObject()
    {
        return new 
        { 
            FocusCost = FocusCost.ToJsonObject(), 
            SkillCheckRequirement = SkillCheckRequirement?.ToJsonObject() 
        };
    }
}

public class ChoiceTemplate
{
    // Template identity
    public string TemplateName { get; private set; }
    public string StrategicPurpose { get; private set; }
    public int Weight { get; private set; }
    
    // Input mechanics
    public InputMechanics InputMechanics { get; private set; }
    
    // Direct effect class references
    public Type SuccessEffectClass { get; private set; }
    public Type FailureEffectClass { get; private set; }
    
    // Narrative guidance for AI
    public string ConceptualOutput { get; private set; }
    public string SuccessOutcomeNarrativeGuidance { get; private set; }
    public string FailureOutcomeNarrativeGuidance { get; private set; }
    
    public ChoiceTemplate(
        string templateName,
        string strategicPurpose,
        int weight,
        InputMechanics inputMechanics,
        Type successEffectClass,
        Type failureEffectClass,
        string conceptualOutput,
        string successOutcomeNarrativeGuidance,
        string failureOutcomeNarrativeGuidance)
    {
        if (!typeof(IMechanicalEffect).IsAssignableFrom(successEffectClass))
        {
            throw new ArgumentException($"Success effect class must implement IMechanicalEffect: {successEffectClass.Name}");
        }
        
        if (!typeof(IMechanicalEffect).IsAssignableFrom(failureEffectClass))
        {
            throw new ArgumentException($"Failure effect class must implement IMechanicalEffect: {failureEffectClass.Name}");
        }
        
        TemplateName = templateName;
        StrategicPurpose = strategicPurpose;
        Weight = weight;
        InputMechanics = inputMechanics;
        SuccessEffectClass = successEffectClass;
        FailureEffectClass = failureEffectClass;
        ConceptualOutput = conceptualOutput;
        SuccessOutcomeNarrativeGuidance = successOutcomeNarrativeGuidance;
        FailureOutcomeNarrativeGuidance = failureOutcomeNarrativeGuidance;
    }
    
    // Create and execute the success effect
    public void ExecuteSuccessEffect(EncounterState state)
    {
        IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(SuccessEffectClass);
        effect.Apply(state);
    }
    
    // Create and execute the failure effect
    public void ExecuteFailureEffect(EncounterState state)
    {
        IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(FailureEffectClass);
        effect.Apply(state);
    }
    
    // For JSON serialization - provides template info to AI
    public object ToJsonObject()
    {
        return new
        {
            TemplateName = TemplateName,
            StrategicPurpose = StrategicPurpose,
            Weight = Weight,
            InputMechanics = InputMechanics.ToJsonObject(),
            ConceptualOutput = ConceptualOutput,
            SuccessOutcomeNarrativeGuidance = SuccessOutcomeNarrativeGuidance,
            FailureOutcomeNarrativeGuidance = FailureOutcomeNarrativeGuidance
        };
    }
}

public static class TemplateLibrary
{
    public static List<ChoiceTemplate> GetAllTemplates()
    {
        return new List<ChoiceTemplate>
        {
            new ChoiceTemplate(
                templateName: "EstablishTrust",
                strategicPurpose: "Build relationship with NPC",
                weight: 10,
                inputMechanics: new InputMechanics(
                    new FocusCost(1),
                    new SkillCheckRequirement(SkillCategories.Social, 3)
                ),
                successEffectClass: typeof(EstablishTrustEffect),
                failureEffectClass: typeof(TriggerDistrustEffect),
                conceptualOutput: "Player attempts to build trust with NPC",
                successOutcomeNarrativeGuidance: "NPC becomes more trusting toward player",
                failureOutcomeNarrativeGuidance: "NPC becomes suspicious of player's intentions"
            ),
            
            new ChoiceTemplate(
                templateName: "GatherInformation",
                strategicPurpose: "Acquire knowledge about situation",
                weight: 8,
                inputMechanics: new InputMechanics(
                    new FocusCost(1),
                    new SkillCheckRequirement(SkillCategories.Intellectual, 3)
                ),
                successEffectClass: typeof(GainInsightEffect),
                failureEffectClass: typeof(CreateConfusionEffect),
                conceptualOutput: "Player attempts to gather information",
                successOutcomeNarrativeGuidance: "Player gains valuable insight",
                failureOutcomeNarrativeGuidance: "Player becomes confused by conflicting information"
            ),
            
            new ChoiceTemplate(
                templateName: "RecoverFocus",
                strategicPurpose: "Regain lost Focus Points",
                weight: 5,
                inputMechanics: new InputMechanics(
                    new FocusCost(0),
                    new SkillCheckRequirement(SkillCategories.Physical, 2)
                ),
                successEffectClass: typeof(RecoverFocusEffect),
                failureEffectClass: typeof(WasteTimeEffect),
                conceptualOutput: "Player attempts to recover focus",
                successOutcomeNarrativeGuidance: "Player regains composure and energy",
                failureOutcomeNarrativeGuidance: "Player wastes time without recovering"
            )
            
            // Additional templates would be defined here...
        };
    }
}
```

## 5. AI Response Structures and Processing

```csharp
public class EncounterChoice
{
    public string ChoiceID { get; set; }
    public string NarrativeText { get; set; }
    public int FocusCost { get; set; }
    public string TemplateUsed { get; set; }
    public SkillCheck SkillCheck { get; set; }
    public string SuccessNarrative { get; set; }
    public string FailureNarrative { get; set; }
}

public class SkillCheck
{
    public SkillCategories SkillCategory { get; set; }
    public int SCD { get; set; }
    public string DifficultyLabel { get; set; }
}

public class AIResponse
{
    public string BeatNarration { get; set; }
    public List<EncounterChoice> Choices { get; set; }
}

public class EncounterManager
{
    private Dictionary<string, ChoiceTemplate> templateDictionary;
    private StreamingContentState streamingContentState;
    
    public EncounterManager(List<ChoiceTemplate> templates, StreamingContentState streamingContentState)
    {
        this.streamingContentState = streamingContentState;
        templateDictionary = new Dictionary<string, ChoiceTemplate>();
        
        foreach (ChoiceTemplate template in templates)
        {
            templateDictionary[template.TemplateName] = template;
        }
    }
    
    public void ProcessPlayerChoice(string choiceId, AIResponse aiResponse, EncounterState state)
    {
        // Find the chosen option
        EncounterChoice choice = null;
        foreach (EncounterChoice c in aiResponse.Choices)
        {
            if (c.ChoiceID == choiceId)
            {
                choice = c;
                break;
            }
        }
        
        if (choice == null)
        {
            throw new InvalidOperationException($"Choice with ID {choiceId} not found");
        }
        
        // Get the template
        if (!templateDictionary.ContainsKey(choice.TemplateUsed))
        {
            throw new InvalidOperationException($"Template {choice.TemplateUsed} not found");
        }
        
        ChoiceTemplate template = templateDictionary[choice.TemplateUsed];
        
        // Process focus cost
        state.FocusPoints -= choice.FocusCost;
        
        // Process skill check
        bool success = true;
        if (choice.SkillCheck != null)
        {
            // Find player's skill level for this category
            int playerSkillLevel = state.Player.GetSkillLevel(choice.SkillCheck.SkillCategory);
            
            // Apply modifiers
            playerSkillLevel += state.GetNextCheckModifier();
            
            // Determine success
            success = playerSkillLevel >= choice.SkillCheck.SCD;
        }
        
        // Execute the appropriate effect
        if (success)
        {
            template.ExecuteSuccessEffect(state);
            // Begin streaming success narrative
            streamingContentState.BeginStreaming(choice.SuccessNarrative);
        }
        else
        {
            template.ExecuteFailureEffect(state);
            // Begin streaming failure narrative
            streamingContentState.BeginStreaming(choice.FailureNarrative);
        }
        
        // Update encounter state
        state.AdvanceDuration(1);
        state.ProcessModifiers();
        state.CheckGoalCompletion();
    }
}
```


## 7. GameWorld State Management

```csharp
public class GameWorld
{
    public Player Player { get; private set; }
    public EncounterState CurrentEncounter { get; private set; }
    public StreamingContentState StreamingContentState { get; private set; }
    
    // Other game state would be here (locations, NPCs, etc.)
    
    public GameWorld()
    {
        Player = new Player();
        StreamingContentState = new StreamingContentState();
    }
    
    public void StartEncounter(EncounterState encounterState)
    {
        CurrentEncounter = encounterState;
    }
    
    public void EndEncounter()
    {
        CurrentEncounter = null;
    }
}

public class GameWorldManager
{
    private GameWorld gameWorld;
    
    public GameWorldManager()
    {
        gameWorld = new GameWorld();
    }
    
    public GameWorldSnapshot GetSnapshot()
    {
        // Create a read-only snapshot of the current game state for UI
        return new GameWorldSnapshot(gameWorld);
    }
    
    public void StartEncounter(List<FlagDefinition> flagDefinitions, List<FlagStates> goalFlags)
    {
        EncounterState encounterState = new EncounterState(
            gameWorld.Player, 
            6, // Standard 6 Focus Points
            10, // Maximum 10 duration before forced end
            goalFlags,
            flagDefinitions
        );
        
        gameWorld.StartEncounter(encounterState);
    }
    
    public void ProcessPlayerChoice(string choiceId, AIResponse aiResponse)
    {
        if (gameWorld.CurrentEncounter == null)
        {
            throw new InvalidOperationException("No active encounter");
        }
        
        EncounterManager processor = new EncounterManager(
            TemplateLibrary.GetAllTemplates(),
            gameWorld.StreamingContentState
        );
        
        processor.ProcessPlayerChoice(choiceId, aiResponse, gameWorld.CurrentEncounter);
        
        if (gameWorld.CurrentEncounter.IsEncounterComplete)
        {
            // Handle encounter completion
            gameWorld.EndEncounter();
        }
    }
}

// Read-only snapshot for UI
public class GameWorldSnapshot
{
    public bool HasActiveEncounter { get; private set; }
    public int CurrentFocusPoints { get; private set; }
    public int MaxFocusPoints { get; private set; }
    public List<FlagStates> ActiveFlags { get; private set; }
    public string StreamingText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    
    public GameWorldSnapshot(GameWorld gameWorld)
    {
        HasActiveEncounter = gameWorld.CurrentEncounter != null;
        
        if (HasActiveEncounter)
        {
            CurrentFocusPoints = gameWorld.CurrentEncounter.FocusPoints;
            MaxFocusPoints = gameWorld.CurrentEncounter.MaxFocusPoints;
            ActiveFlags = gameWorld.CurrentEncounter.FlagManager.GetAllActiveFlags();
        }
        
        StreamingText = gameWorld.StreamingContentState.CurrentText;
        IsStreaming = gameWorld.StreamingContentState.IsStreaming;
        StreamProgress = gameWorld.StreamingContentState.StreamProgress;
    }
}
```

## 9. Blazor UI Polling Component

```csharp
@page "/"
@using System.Threading
@inject GameWorldManager GameWorldManager

<div class="encounter-container">
    @if (currentSnapshot != null && currentSnapshot.HasActiveEncounter)
    {
        <div class="encounter-header">
            <div class="focus-display">
                Focus Points: @currentSnapshot.CurrentFocusPoints / @currentSnapshot.MaxFocusPoints
            </div>
            
            <div class="flag-display">
                @foreach (var flag in currentSnapshot.ActiveFlags)
                {
                    <span class="flag-badge">@flag</span>
                }
            </div>
        </div>
        
        <div class="narrative-container">
            @if (currentSnapshot.IsStreaming)
            {
                <div class="streaming-text">
                    @currentSnapshot.StreamingText<span class="cursor">|</span>
                </div>
                <div class="stream-progress">
                    <div class="progress-bar" style="width: @(currentSnapshot.StreamProgress * 100)%"></div>
                </div>
            }
            else
            {
                <div class="narrative-text">
                    @currentSnapshot.StreamingText
                </div>
                
                @if (currentAIResponse != null)
                {
                    <div class="choices-container">
                        @foreach (var choice in currentAIResponse.Choices)
                        {
                            <button class="choice-button" @onclick="() => MakeChoice(choice.ChoiceID)">
                                <div class="choice-header">
                                    <span class="choice-focus">@choice.FocusCost Focus</span>
                                    <span class="choice-difficulty">@choice.SkillCheck.DifficultyLabel</span>
                                </div>
                                <div class="choice-text">@choice.NarrativeText</div>
                            </button>
                        }
                    </div>
                }
            }
        </div>
    }
    else
    {
        <div class="no-encounter">
            <p>No active encounter</p>
            <button @onclick="StartEncounter">Start Encounter</button>
        </div>
    }
</div>

@code {
    private Timer pollingTimer;
    private GameWorldSnapshot currentSnapshot;
    private AIResponse currentAIResponse;
    
    protected override void OnInitialized()
    {
        // Set up polling timer
        pollingTimer = new Timer(_ => 
        {
            InvokeAsync(() => 
            {
                PollGameState();
                StateHasChanged();
            });
        }, null, 0, 100); // Poll every 100ms
    }
    
    private void PollGameState()
    {
        currentSnapshot = GameWorldManager.GetSnapshot();
    }
    
    private void StartEncounter()
    {
        // In a real implementation, this would set up a proper encounter
        List<FlagDefinition> flagDefinitions = GetFlagDefinitions();
        List<FlagStates> goalFlags = new List<FlagStates> { FlagStates.TrustEstablished };
        
        GameWorldManager.StartEncounter(flagDefinitions, goalFlags);
        
        // Simulate getting AI response
        GetAIResponse();
    }
    
    private void MakeChoice(string choiceId)
    {
        if (currentAIResponse != null)
        {
            GameWorldManager.ProcessPlayerChoice(choiceId, currentAIResponse);
            
            // Wait for streaming to complete before showing new choices
            // In a real implementation, this would be handled by the polling system
            // detecting when streaming is complete
            if (!currentSnapshot.IsStreaming)
            {
                GetAIResponse();
            }
        }
    }
    
    private void GetAIResponse()
    {
        // In a real implementation, this would call the AI service
        // For this example, we'll use a mock response
        currentAIResponse = new AIResponse
        {
            BeatNarration = "The innkeeper eyes you suspiciously as you approach the counter.",
            Choices = new List<EncounterChoice>
            {
                new EncounterChoice
                {
                    ChoiceID = "CHOICE_1",
                    NarrativeText = "Engage in polite conversation to build rapport",
                    TemplateUsed = "EstablishTrust",
                    FocusCost = 1,
                    SkillCheck = new SkillCheck
                    {
                        SkillCategory = SkillCategories.Social,
                        SCD = 3,
                        DifficultyLabel = "Standard"
                    },
                    SuccessNarrative = "Your friendly demeanor puts the innkeeper at ease. He smiles slightly and his posture relaxes as he begins to open up to you.",
                    FailureNarrative = "Your attempt at friendly conversation comes across as forced. The innkeeper's expression hardens as he regards you with increased suspicion."
                },
                new EncounterChoice
                {
                    ChoiceID = "CHOICE_2",
                    NarrativeText = "Carefully observe the innkeeper for insights",
                    TemplateUsed = "GatherInformation",
                    FocusCost = 1,
                    SkillCheck = new SkillCheck
                    {
                        SkillCategory = SkillCategories.Intellectual,
                        SCD = 3,
                        DifficultyLabel = "Standard"
                    },
                    SuccessNarrative = "You notice several telling details about the innkeeper: his calloused hands suggest a military background, and the way he scans the room indicates he's cautious but not fearful.",
                    FailureNarrative = "You try to read the innkeeper but find his mannerisms confusing and contradictory, leaving you unsure what to make of him."
                },
                new EncounterChoice
                {
                    ChoiceID = "CHOICE_3",
                    NarrativeText = "Take a moment to collect yourself",
                    TemplateUsed = "RecoverFocus",
                    FocusCost = 0,
                    SkillCheck = new SkillCheck
                    {
                        SkillCategory = SkillCategories.Physical,
                        SCD = 2,
                        DifficultyLabel = "Easy"
                    },
                    SuccessNarrative = "You take a deep breath and center yourself, feeling more focused and ready to face the challenges ahead.",
                    FailureNarrative = "You try to gather your thoughts, but the busy atmosphere of the inn makes it difficult to concentrate."
                }
            }
        };
    }
    
    private List<FlagDefinition> GetFlagDefinitions()
    {
        // In a real implementation, this would come from a configuration
        return new List<FlagDefinition>
        {
            new FlagDefinition(FlagStates.TrustEstablished, FlagCategories.Relational, FlagStates.DistrustTriggered),
            new FlagDefinition(FlagStates.DistrustTriggered, FlagCategories.Relational, FlagStates.TrustEstablished),
            new FlagDefinition(FlagStates.InsightGained, FlagCategories.Informational, FlagStates.ConfusionCreated),
            new FlagDefinition(FlagStates.ConfusionCreated, FlagCategories.Informational, FlagStates.InsightGained)
        };
    }
    
    public void Dispose()
    {
        pollingTimer?.Dispose();
    }
}
```

# Wayfarer Integration: The Glue Between Components

After reviewing your requirements more carefully, I understand you're looking for the actual integration points between these classes - the "glue code" that makes the system work as a cohesive whole. Let me focus on the key integration points with special attention to your architecture preferences:

1. No template selection logic (AI receives all templates and chooses freely)
2. Strict polling architecture (no events or callbacks anywhere)
3. Direct effect class execution from templates

## 1. GameWorldManager: Central Integration Point

The GameWorldManager serves as the main integration point between all components:

```csharp
public class GameWorldManager
{
    private GameWorld gameWorld;
    private AIGameMaster AIGameMaster;
    private List<ChoiceTemplate> allTemplates;
    
    // Fixed update interval for game simulation
    private const int UPDATE_INTERVAL_MS = 100;
    private System.Threading.Timer updateTimer;
    
    public GameWorldManager()
    {
        // Initialize game world
        gameWorld = new GameWorld();
        
        // Load all templates - no filtering occurs here
        allTemplates = TemplateLibrary.GetAllTemplates();
        
        // Initialize AI service
        AIGameMaster = new AIGameMaster();
        
        // Start update loop (not using events, just regular updates)
        updateTimer = new System.Threading.Timer(UpdateGameState, null, 0, UPDATE_INTERVAL_MS);
    }
    
    private void UpdateGameState(object state)
    {
        // Process any pending AI responses
        ProcessPendingAIResponses();
        
        // Process streaming content updates
        UpdateStreamingContent();
        
        // Check for encounter completion
        CheckEncounterState();
    }
    
    public void StartEncounter(string encounterType, NPC targetNPC)
    {
        // Create new encounter state
        List<FlagDefinition> flagDefinitions = FlagDefinitionLibrary.GetAllFlagDefinitions();
        List<FlagStates> goalFlags = DetermineGoalFlags(encounterType);
        
        EncounterState encounterState = new EncounterState(
            gameWorld.Player,
            6, // Standard focus points
            10, // Max duration
            goalFlags,
            flagDefinitions
        );
        
        encounterState.CurrentNPC = targetNPC;
        gameWorld.StartEncounter(encounterState);
        
        // Request initial choices from AI - sending ALL templates
        RequestEncounterChoices();
    }
    
    public void ProcessPlayerChoice(string choiceId)
    {
        if (gameWorld.CurrentEncounter == null || gameWorld.CurrentAIResponse == null)
        {
            return; // No active encounter or no AI response
        }
        
        // Find selected choice
        EncounterChoice selectedChoice = null;
        foreach (EncounterChoice choice in gameWorld.CurrentAIResponse.Choices)
        {
            if (choice.ChoiceID == choiceId)
            {
                selectedChoice = choice;
                break;
            }
        }
        
        if (selectedChoice == null)
        {
            return; // Choice not found
        }
        
        // Find the template used by the AI
        ChoiceTemplate template = FindTemplateByName(selectedChoice.TemplateUsed);
        if (template == null)
        {
            return; // Template not found
        }
        
        // Process focus cost
        gameWorld.CurrentEncounter.FocusPoints -= selectedChoice.FocusCost;
        
        // Perform skill check
        bool success = PerformSkillCheck(selectedChoice.SkillCheck);
        
        // Apply mechanical effect directly from template
        if (success)
        {
            // Begin streaming success narrative
            gameWorld.StreamingContentState.BeginStreaming(selectedChoice.SuccessNarrative);
            
            // Create and apply the success effect (direct instantiation)
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.SuccessEffectClass);
            effect.Apply(gameWorld.CurrentEncounter);
        }
        else
        {
            // Begin streaming failure narrative
            gameWorld.StreamingContentState.BeginStreaming(selectedChoice.FailureNarrative);
            
            // Create and apply the failure effect (direct instantiation)
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.FailureEffectClass);
            effect.Apply(gameWorld.CurrentEncounter);
        }
        
        // Update encounter state
        gameWorld.CurrentEncounter.AdvanceDuration(1);
        gameWorld.CurrentEncounter.ProcessModifiers();
        gameWorld.CurrentEncounter.CheckGoalCompletion();
        
        // Clear AI response while streaming occurs
        gameWorld.CurrentAIResponse = null;
    }
    
    private bool PerformSkillCheck(SkillCheck skillCheck)
    {
        if (skillCheck == null)
        {
            return true; // No skill check required
        }
        
        // Get player's skill level
        int playerSkillLevel = gameWorld.Player.GetSkillLevel(skillCheck.SkillCategory);
        
        // Apply any modifiers
        playerSkillLevel += gameWorld.CurrentEncounter.GetNextCheckModifier();
        
        // Compare to difficulty
        return playerSkillLevel >= skillCheck.SCD;
    }
    
    private void RequestEncounterChoices()
    {
        if (gameWorld.CurrentEncounter == null || gameWorld.IsAwaitingAIResponse)
        {
            return; // No encounter or already waiting
        }
        
        // Mark that we're waiting for AI response
        gameWorld.IsAwaitingAIResponse = true;
        
        // Build the prompt - sending ALL templates without any filtering
        AIPromptBuilder promptBuilder = new AIPromptBuilder();
        string prompt = promptBuilder.BuildPrompt(gameWorld, allTemplates);
        
        // Send to AI service
        AIGameMaster.RequestChoices(prompt, OnAIResponseReceived);
    }
    
    private void OnAIResponseReceived(AIResponse response)
    {
        // Store response in game world - no events, just state update
        gameWorld.CurrentAIResponse = response;
        gameWorld.IsAwaitingAIResponse = false;
        
        // Begin streaming the beat narration
        gameWorld.StreamingContentState.BeginStreaming(response.BeatNarration);
    }
    
    private void ProcessPendingAIResponses()
    {
        // No event handling, just checking state
        if (gameWorld.IsAwaitingAIResponse && AIGameMaster.HasResponse)
        {
            AIResponse response = AIGameMaster.GetResponse();
            OnAIResponseReceived(response);
        }
    }
    
    private void UpdateStreamingContent()
    {
        // If streaming is complete and we need new choices, request them
        if (gameWorld.CurrentEncounter != null && 
            !gameWorld.IsAwaitingAIResponse && 
            !gameWorld.StreamingContentState.IsStreaming && 
            gameWorld.CurrentAIResponse == null &&
            !gameWorld.CurrentEncounter.IsEncounterComplete)
        {
            RequestEncounterChoices();
        }
    }
    
    private void CheckEncounterState()
    {
        if (gameWorld.CurrentEncounter != null && gameWorld.CurrentEncounter.IsEncounterComplete)
        {
            // Handle encounter completion
            if (!gameWorld.StreamingContentState.IsStreaming)
            {
                // Only end encounter after streaming is complete
                gameWorld.EndEncounter();
            }
        }
    }
    
    private ChoiceTemplate FindTemplateByName(string templateName)
    {
        foreach (ChoiceTemplate template in allTemplates)
        {
            if (template.TemplateName == templateName)
            {
                return template;
            }
        }
        
        return null;
    }
    
    private List<FlagStates> DetermineGoalFlags(string encounterType)
    {
        // In a real implementation, this would return different goals based on encounter type
        List<FlagStates> goalFlags = new List<FlagStates>();
        
        if (encounterType == "SocialIntroduction")
        {
            goalFlags.Add(FlagStates.TrustEstablished);
        }
        else if (encounterType == "Investigation")
        {
            goalFlags.Add(FlagStates.InsightGained);
            goalFlags.Add(FlagStates.SecretRevealed);
        }
        
        return goalFlags;
    }
    
    // Public method to get current game state - used by polling UI
    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(gameWorld);
    }
}
```

## 2. Expanded GameWorld State for Integration

The GameWorld class needs to be expanded to support integration points:

```csharp
public class GameWorld
{
    public Player Player { get; private set; }
    public EncounterState CurrentEncounter { get; private set; }
    public StreamingContentState StreamingContentState { get; private set; }
    
    // Added integration fields
    public AIResponse CurrentAIResponse { get; set; }
    public bool IsAwaitingAIResponse { get; set; }
    
    public GameWorld()
    {
        Player = new Player();
        StreamingContentState = new StreamingContentState();
        CurrentAIResponse = null;
        IsAwaitingAIResponse = false;
    }
    
    public void StartEncounter(EncounterState encounterState)
    {
        CurrentEncounter = encounterState;
    }
    
    public void EndEncounter()
    {
        CurrentEncounter = null;
        CurrentAIResponse = null;
        IsAwaitingAIResponse = false;
    }
}

// Expanded snapshot to include AI response
public class GameWorldSnapshot
{
    public bool HasActiveEncounter { get; private set; }
    public int CurrentFocusPoints { get; private set; }
    public int MaxFocusPoints { get; private set; }
    public List<FlagStates> ActiveFlags { get; private set; }
    public string StreamingText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    
    // Added fields for integration
    public AIResponse CurrentAIResponse { get; private set; }
    public bool IsAwaitingAIResponse { get; private set; }
    public bool CanSelectChoice { get; private set; }
    
    public GameWorldSnapshot(GameWorld gameWorld)
    {
        HasActiveEncounter = gameWorld.CurrentEncounter != null;
        
        if (HasActiveEncounter)
        {
            CurrentFocusPoints = gameWorld.CurrentEncounter.FocusPoints;
            MaxFocusPoints = gameWorld.CurrentEncounter.MaxFocusPoints;
            ActiveFlags = gameWorld.CurrentEncounter.FlagManager.GetAllActiveFlags();
        }
        
        StreamingText = gameWorld.StreamingContentState.CurrentText;
        IsStreaming = gameWorld.StreamingContentState.IsStreaming;
        StreamProgress = gameWorld.StreamingContentState.StreamProgress;
        
        CurrentAIResponse = gameWorld.CurrentAIResponse;
        IsAwaitingAIResponse = gameWorld.IsAwaitingAIResponse;
        
        // Player can select a choice when:
        // 1. There is an AI response with choices
        // 2. We're not waiting for a new AI response
        // 3. No text is currently streaming
        CanSelectChoice = gameWorld.CurrentAIResponse != null && 
                           !gameWorld.IsAwaitingAIResponse && 
                           !gameWorld.StreamingContentState.IsStreaming;
    }
}
```

## 3. AI Service Implementation Without Events

```csharp
public class AIGameMaster
{
    private AIResponse pendingResponse;
    private bool hasResponse;
    
    // Since we can't use events, we'll use a callback approach with state storage
    private Action<AIResponse> responseCallback;
    
    public bool HasResponse => hasResponse;
    
    public AIGameMaster()
    {
        pendingResponse = null;
        hasResponse = false;
    }
    
    public void RequestChoices(string prompt, Action<AIResponse> callback)
    {
        // Store callback (but don't invoke it - polling will handle this)
        responseCallback = callback;
        
        // Clear any pending response
        pendingResponse = null;
        hasResponse = false;
        
        // Start async request - no events, just state updates
        Task.Run(() => ExecuteRequest(prompt));
    }
    
    private async Task ExecuteRequest(string prompt)
    {
        // Simulate API request
        await Task.Delay(1000); // Replace with actual API call
        
        // In a real implementation, this would parse JSON from the AI
        AIResponse response = CreateMockResponse();
        
        // Store response - no events, just state update
        pendingResponse = response;
        hasResponse = true;
        
        // Note: We do NOT call the callback directly
        // The polling system will detect hasResponse and handle it
    }
    
    public AIResponse GetResponse()
    {
        if (!hasResponse)
        {
            return null;
        }
        
        AIResponse response = pendingResponse;
        pendingResponse = null;
        hasResponse = false;
        
        // Execute callback if provided
        responseCallback?.Invoke(response);
        responseCallback = null;
        
        return response;
    }
    
    private AIResponse CreateMockResponse()
    {
        // This would be replaced with actual AI response parsing
        AIResponse response = new AIResponse
        {
            BeatNarration = "The innkeeper eyes you suspiciously as you approach the counter.",
            Choices = new List<EncounterChoice>
            {
                new EncounterChoice
                {
                    ChoiceID = "CHOICE_1",
                    NarrativeText = "Engage in polite conversation to build rapport",
                    TemplateUsed = "EstablishTrust", // Direct reference to template
                    FocusCost = 1,
                    SkillCheck = new SkillCheck
                    {
                        SkillCategory = SkillCategories.Social,
                        SCD = 3,
                        DifficultyLabel = "Standard"
                    },
                    SuccessNarrative = "Your friendly demeanor puts the innkeeper at ease.",
                    FailureNarrative = "Your attempt at friendly conversation comes across as forced."
                },
                new EncounterChoice
                {
                    ChoiceID = "CHOICE_2",
                    NarrativeText = "Carefully observe the innkeeper for insights",
                    TemplateUsed = "GatherInformation", // Direct reference to template
                    FocusCost = 1,
                    SkillCheck = new SkillCheck
                    {
                        SkillCategory = SkillCategories.Intellectual,
                        SCD = 3,
                        DifficultyLabel = "Standard"
                    },
                    SuccessNarrative = "You notice several telling details about the innkeeper.",
                    FailureNarrative = "You try to read the innkeeper but find his mannerisms confusing."
                }
            }
        };
        
        return response;
    }
}
```

## 4. Pure Polling-Based Blazor UI Component

```csharp
@page "/"
@using System.Threading
@inject GameWorldManager GameWorldManager

<div class="encounter-container">
    @if (currentSnapshot != null && currentSnapshot.HasActiveEncounter)
    {
        <div class="encounter-header">
            <div class="focus-display">
                Focus Points: @currentSnapshot.CurrentFocusPoints / @currentSnapshot.MaxFocusPoints
            </div>
            
            <div class="flag-display">
                @foreach (var flag in currentSnapshot.ActiveFlags)
                {
                    <span class="flag-badge">@flag</span>
                }
            </div>
        </div>
        
        <div class="narrative-container">
            @if (currentSnapshot.IsStreaming)
            {
                <div class="streaming-text">
                    @currentSnapshot.StreamingText<span class="cursor">|</span>
                </div>
                <div class="stream-progress">
                    <div class="progress-bar" style="width: @(currentSnapshot.StreamProgress * 100)%"></div>
                </div>
            }
            else if (currentSnapshot.CurrentAIResponse != null && !currentSnapshot.IsAwaitingAIResponse)
            {
                <div class="narrative-text">
                    @currentSnapshot.StreamingText
                </div>
                
                <div class="choices-container">
                    @foreach (var choice in currentSnapshot.CurrentAIResponse.Choices)
                    {
                        <button class="choice-button" 
                                @onclick="() => MakeChoice(choice.ChoiceID)" 
                                disabled="@(!currentSnapshot.CanSelectChoice)">
                            <div class="choice-header">
                                <span class="choice-focus">@choice.FocusCost Focus</span>
                                <span class="choice-difficulty">@choice.SkillCheck.DifficultyLabel</span>
                                <span class="choice-template">@choice.TemplateUsed</span>
                            </div>
                            <div class="choice-text">@choice.NarrativeText</div>
                        </button>
                    }
                </div>
            }
            else if (currentSnapshot.IsAwaitingAIResponse)
            {
                <div class="loading-indicator">
                    Thinking...
                </div>
            }
        </div>
    }
    else
    {
        <div class="no-encounter">
            <p>No active encounter</p>
            <button @onclick="StartEncounter">Start Encounter</button>
        </div>
    }
</div>

@code {
    private Timer pollingTimer;
    private GameWorldSnapshot currentSnapshot;
    
    protected override void OnInitialized()
    {
        // Set up polling timer - no events, just regular polling
        pollingTimer = new Timer(_ => 
        {
            InvokeAsync(() => 
            {
                PollGameState();
                StateHasChanged();
            });
        }, null, 0, 100); // Poll every 100ms
    }
    
    private void PollGameState()
    {
        // Poll for current game state
        currentSnapshot = GameWorldManager.GetGameSnapshot();
    }
    
    private void StartEncounter()
    {
        GameWorldManager.StartEncounter("SocialIntroduction", null);
    }
    
    private void MakeChoice(string choiceId)
    {
        if (currentSnapshot != null && currentSnapshot.CanSelectChoice)
        {
            GameWorldManager.ProcessPlayerChoice(choiceId);
        }
    }
    
    public void Dispose()
    {
        pollingTimer?.Dispose();
    }
}
```

## 5. Template Serialization for AI Integration

```csharp
public class AIPromptBuilder
{
    public string BuildPrompt(GameWorld gameWorld, List<ChoiceTemplate> availableTemplates)
    {
        StringBuilder prompt = new StringBuilder();
        
        prompt.AppendLine("You are the AI Game Master for the game Wayfarer.");
        prompt.AppendLine();
        
        // Add game state context
        AddGameStateContext(prompt, gameWorld);
        
        // Add ALL templates as JSON with their input mechanics and conceptual guidance
        prompt.AppendLine("AVAILABLE TEMPLATES:");
        AddTemplatesAsJson(prompt, availableTemplates);
        
        // Add instructions
        AddInstructions(prompt);
        
        return prompt.ToString();
    }
    
    private void AddGameStateContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("ENCOUNTER CONTEXT:");
        
        if (gameWorld.CurrentEncounter != null)
        {
            // Add focus points
            prompt.AppendLine($"- Focus Points: {gameWorld.CurrentEncounter.FocusPoints}/{gameWorld.CurrentEncounter.MaxFocusPoints}");
            
            // Add active flags
            prompt.AppendLine("- Active State Flags:");
            List<FlagStates> activeFlags = gameWorld.CurrentEncounter.FlagManager.GetAllActiveFlags();
            foreach (FlagStates flag in activeFlags)
            {
                prompt.AppendLine($"  * {flag}");
            }
            
            // Add NPC information if available
            if (gameWorld.CurrentEncounter.CurrentNPC != null)
            {
                prompt.AppendLine($"- Current NPC: {gameWorld.CurrentEncounter.CurrentNPC.Name}");
                prompt.AppendLine($"  * Role: {gameWorld.CurrentEncounter.CurrentNPC.Role}");
                prompt.AppendLine($"  * Attitude: {gameWorld.CurrentEncounter.CurrentNPC.Attitude}");
            }
            
            // Add duration information
            prompt.AppendLine($"- Encounter Duration: {gameWorld.CurrentEncounter.DurationCounter}/{gameWorld.CurrentEncounter.MaxDuration}");
        }
        
        // Add player skills
        prompt.AppendLine("- Player Skills Available:");
        foreach (SkillCard card in gameWorld.Player.AvailableCards)
        {
            if (!card.IsExhausted)
            {
                prompt.AppendLine($"  * {card.Name} (Level {card.Level}, {card.Category})");
            }
        }
        
        prompt.AppendLine();
    }
    
    private void AddTemplatesAsJson(StringBuilder prompt, List<ChoiceTemplate> templates)
    {
        // Add ALL templates without filtering
        foreach (ChoiceTemplate template in templates)
        {
            // Convert template to JSON using System.Text.Json
            string templateJson = System.Text.Json.JsonSerializer.Serialize(
                template.ToJsonObject(),
                new JsonSerializerOptions { WriteIndented = true }
            );
            
            prompt.AppendLine(templateJson);
            prompt.AppendLine();
        }
    }
    
}
```

## 6. StreamingContentState Implementation

```csharp
public class StreamingContentState
{
    public string CurrentText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    private string targetText;
    private int currentCharIndex;
    private DateTime lastUpdateTime;
    private const int CHARS_PER_UPDATE = 3;
    
    public StreamingContentState()
    {
        CurrentText = string.Empty;
        IsStreaming = false;
        StreamProgress = 0;
        targetText = string.Empty;
        currentCharIndex = 0;
    }
    
    public void BeginStreaming(string fullText)
    {
        CurrentText = string.Empty;
        targetText = fullText;
        IsStreaming = true;
        StreamProgress = 0;
        currentCharIndex = 0;
        lastUpdateTime = DateTime.Now;
    }
    
    public void Update()
    {
        if (!IsStreaming)
        {
            return;
        }
        
        // Only update if enough time has passed (simulates token streaming)
        DateTime now = DateTime.Now;
        if ((now - lastUpdateTime).TotalMilliseconds < 50)
        {
            return;
        }
        
        // Update a few characters at a time
        currentCharIndex += CHARS_PER_UPDATE;
        if (currentCharIndex >= targetText.Length)
        {
            // Streaming complete
            currentCharIndex = targetText.Length;
            IsStreaming = false;
            StreamProgress = 1.0f;
        }
        else
        {
            StreamProgress = (float)currentCharIndex / targetText.Length;
        }
        
        // Update current text
        CurrentText = targetText.Substring(0, currentCharIndex);
        lastUpdateTime = now;
    }
}
```

## 8. Full Integration Example

Here's an example showing the flow of execution in a typical interaction:

1. **Game Starts:**
   - `GameUI : ComponentBase` initializes all systems and loads all templates
   - UI polls for state but no encounter is active

2. **Player Starts Encounter:**
   - UI calls `GameWorldManager.StartEncounter()`
   - `GameWorldManager` creates a new `EncounterState`
   - `GameWorldManager` calls `EncounterManager` which calls `RequestEncounterChoices()` on `AIGameMaster` with ALL templates

3. **AI Response Processing:**
   - `AIGameMaster` processes the request asynchronously
   - When complete, it sets `hasResponse = true` but DOESN'T invoke callbacks
   - In the next update, `GameWorldManager` polls `AIGameMaster.HasResponse`
   - When true, `GameWorldManager` calls `AIGameMaster.GetResponse()`
   - Response is stored in `gameWorld.CurrentAIResponse`
   - Beat narration begins streaming

4. **UI Updates via Polling:**
   - UI regularly polls `GameWorldManager.GetGameSnapshot()`
   - UI sees `isStreaming = true` and displays streaming text
   - When streaming completes, UI shows available choices

5. **Player Makes Choice:**
   - UI calls `GameWorldManager.ProcessPlayerChoice(choiceId)`
   - `GameWorldManager` finds the selected choice and its template
   - Template's effect class is instantiated directly: `Activator.CreateInstance(template.SuccessEffectClass)`
   - Effect is applied to the encounter state
   - Success/failure narrative begins streaming

6. **Game State Updates:**
   - In the next update cycle, `GameWorldManager` sees streaming is complete
   - If encounter is not complete, it requests new choices from AI
   - Process repeats until encounter completion

7. **Encounter Completion:**
   - When `gameWorld.CurrentEncounter.IsEncounterComplete = true`
   - After any final streaming completes, `GameWorldManager` ends the encounter

This architecture ensures:
1. No template filtering - AI always receives ALL templates
2. No events or callbacks - everything uses polling
3. Direct effect class execution from templates
4. Streaming content updates incrementally

Each component maintains its own state, and the GameWorldManager acts as the central integration point, but without using events or callbacks to notify components of changes.