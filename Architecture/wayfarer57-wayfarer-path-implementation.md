# Wayfarer: Concrete Implementation of 80 Days Design Patterns

Based on the latest project knowledge and your code architecture, I'll provide specific implementation details for integrating the 80 Days design patterns into Wayfarer's existing systems.

## I. The Urgency Engine: Implementation Details

### A. GameWorld Enhancements for Time Tracking

```csharp
public class GameWorld
{
    // Existing properties
    public Player Player { get; private set; }
    public EncounterState CurrentEncounter { get; private set; }
    public StreamingContentState StreamingContentState { get; private set; }
    
    // New time tracking properties
    public int CurrentDay { get; private set; }
    public TimeOfDay CurrentTimeOfDay { get; private set; }
    public int DeadlineDay { get; private set; }
    public string DeadlineReason { get; private set; }
    
    // New methods
    public void AdvanceTime(TimeSpan amount)
    {
        // Update CurrentTimeOfDay based on amount
        // If day changes, trigger DayChanged event
        // Check if deadline has been reached
    }
    
    public bool IsDeadlineReached()
    {
        return CurrentDay >= DeadlineDay;
    }
}

public enum TimeOfDay
{
    Morning,
    Noon,
    Afternoon,
    Evening,
    Night
}
```

### B. Location Time-State Properties

```csharp
public class Location
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    
    // Time-based properties
    public Dictionary<TimeOfDay, List<FlagStates>> TimeStateFlags { get; private set; }
    public Dictionary<TimeOfDay, List<string>> AvailableActions { get; private set; }
    public Dictionary<TimeOfDay, string> TimeSpecificDescription { get; private set; }
    
    // Method to get current state based on time
    public List<FlagStates> GetCurrentFlags(TimeOfDay timeOfDay)
    {
        return TimeStateFlags.ContainsKey(timeOfDay) 
            ? TimeStateFlags[timeOfDay] 
            : new List<FlagStates>();
    }
}
```

### C. Time-Limited Opportunities

```csharp
public class Opportunity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int AppearanceDay { get; private set; }
    public int ExpirationDay { get; private set; }
    public List<TimeOfDay> AvailableTimes { get; private set; }
    public List<ChoiceTemplate> OpportunityTemplates { get; private set; }
    
    public bool IsAvailable(int currentDay, TimeOfDay currentTime)
    {
        return currentDay >= AppearanceDay && 
               currentDay <= ExpirationDay && 
               AvailableTimes.Contains(currentTime);
    }
}

// Add to GameWorldManager
public class GameWorldManager
{
    // Existing code...
    
    private List<Opportunity> availableOpportunities = new List<Opportunity>();
    
    public void UpdateAvailableOpportunities()
    {
        foreach (var opportunity in gameWorld.AllOpportunities)
        {
            if (opportunity.IsAvailable(gameWorld.CurrentDay, gameWorld.CurrentTimeOfDay))
            {
                if (!availableOpportunities.Contains(opportunity))
                {
                    availableOpportunities.Add(opportunity);
                }
            }
            else
            {
                availableOpportunities.Remove(opportunity);
            }
        }
    }
    
    // Include in UpdateState method
    public void UpdateState()
    {
        // Existing code...
        UpdateAvailableOpportunities();
    }
}
```

## II. The Confluence Engine: Resource Interdependence

### A. Enhanced Player Resource System

```csharp
public class Player
{
    // Existing properties
    public List<SkillCard> AvailableCards { get; private set; }
    
    // New core resources
    public int Energy { get; private set; }
    public int MaxEnergy { get; private set; }
    public int Reputation { get; private set; } // -100 to 100 scale
    public int Money { get; private set; }
    
    // Reputation levels for easier checks
    public ReputationLevel GetReputationLevel()
    {
        if (Reputation >= 75) return ReputationLevel.Revered;
        if (Reputation >= 50) return ReputationLevel.Respected;
        if (Reputation >= 25) return ReputationLevel.Trusted;
        if (Reputation >= 0) return ReputationLevel.Neutral;
        if (Reputation >= -25) return ReputationLevel.Suspicious;
        if (Reputation >= -50) return ReputationLevel.Distrusted;
        return ReputationLevel.Hated;
    }
    
    // Resource modification with interdependence
    public bool SpendEnergy(int amount)
    {
        if (Energy < amount) return false;
        
        Energy -= amount;
        
        // Automatic time advancement based on energy expenditure
        gameWorld.AdvanceTime(TimeSpan.FromHours(amount));
        
        return true;
    }
    
    public bool SpendMoney(int amount)
    {
        // Reputation affects costs
        int actualCost = CalculateAdjustedCost(amount);
        
        if (Money < actualCost) return false;
        
        Money -= actualCost;
        return true;
    }
    
    private int CalculateAdjustedCost(int baseCost)
    {
        // Higher reputation means lower costs
        float multiplier = 1.0f;
        
        if (Reputation >= 75) multiplier = 0.8f;
        else if (Reputation >= 50) multiplier = 0.9f;
        else if (Reputation >= 25) multiplier = 0.95f;
        else if (Reputation >= 0) multiplier = 1.0f;
        else if (Reputation >= -25) multiplier = 1.1f;
        else if (Reputation >= -50) multiplier = 1.25f;
        else multiplier = 1.5f;
        
        return (int)(baseCost * multiplier);
    }
}

public enum ReputationLevel
{
    Revered,
    Respected,
    Trusted,
    Neutral,
    Suspicious,
    Distrusted,
    Hated
}
```

### B. Contextual Approach Cost System

```csharp
public class ApproachCost
{
    public int EnergyCost { get; private set; }
    public int MoneyCost { get; private set; }
    public int ReputationImpact { get; private set; }
    public TimeSpan TimeCost { get; private set; }
    
    public ApproachCost(int energyCost, int moneyCost, int reputationImpact, TimeSpan timeCost)
    {
        EnergyCost = energyCost;
        MoneyCost = moneyCost;
        ReputationImpact = reputationImpact;
        TimeCost = timeCost;
    }
}

// Modify ChoiceTemplate to include contextual costs
public class ChoiceTemplate
{
    // Existing properties
    public string TemplateName { get; private set; }
    public string StrategicPurpose { get; private set; }
    public int Weight { get; private set; }
    public InputMechanics InputMechanics { get; private set; }
    public Type SuccessEffectClass { get; private set; }
    public Type FailureEffectClass { get; private set; }
    
    // New contextual cost properties
    public Dictionary<SkillCategories, ApproachCost> ContextualCosts { get; private set; }
    
    // Get the appropriate cost based on approach and context
    public ApproachCost GetCost(SkillCategories approach, Location location, TimeOfDay timeOfDay)
    {
        // Base cost from the approach
        ApproachCost baseCost = ContextualCosts.ContainsKey(approach) 
            ? ContextualCosts[approach] 
            : new ApproachCost(1, 0, 0, TimeSpan.FromHours(1));
        
        // Modify based on location properties
        List<FlagStates> locationFlags = location.GetCurrentFlags(timeOfDay);
        
        // Apply modifiers based on flags
        // This is a simple example - you would expand this based on your flag system
        if (locationFlags.Contains(FlagStates.Crowded) && approach == SkillCategories.Intellectual)
        {
            // Intellectual approaches cost more energy in crowded places
            return new ApproachCost(
                baseCost.EnergyCost + 1,
                baseCost.MoneyCost,
                baseCost.ReputationImpact,
                baseCost.TimeCost
            );
        }
        
        // More contextual modifiers here...
        
        return baseCost;
    }
}
```

## III. The Narrative Waterwheel: Memory and Recontextualization

### A. Memory Flag System

```csharp
public class MemoryFlag
{
    public string Key { get; private set; }
    public string Description { get; private set; }
    public int CreationDay { get; private set; }
    public int ExpirationDay { get; private set; } // -1 for never expires
    public int Importance { get; private set; } // 1-10 scale
    
    public bool IsActive(int currentDay)
    {
        return currentDay >= CreationDay && 
               (ExpirationDay == -1 || currentDay <= ExpirationDay);
    }
}

// Add to Player class
public class Player
{
    // Existing properties...
    
    // Memory system
    public List<MemoryFlag> Memories { get; private set; } = new List<MemoryFlag>();
    
    public void AddMemory(string key, string description, int importance, int expirationDays = -1)
    {
        // Remove any existing memory with same key
        Memories.RemoveAll(m => m.Key == key);
        
        // Add new memory
        Memories.Add(new MemoryFlag
        {
            Key = key,
            Description = description,
            CreationDay = gameWorld.CurrentDay,
            ExpirationDay = expirationDays == -1 ? -1 : gameWorld.CurrentDay + expirationDays,
            Importance = importance
        });
    }
    
    public bool HasMemory(string key)
    {
        return Memories.Any(m => m.Key == key && m.IsActive(gameWorld.CurrentDay));
    }
    
    public List<MemoryFlag> GetRecentMemories(int count = 5)
    {
        return Memories
            .Where(m => m.IsActive(gameWorld.CurrentDay))
            .OrderByDescending(m => m.Importance)
            .ThenByDescending(m => m.CreationDay)
            .Take(count)
            .ToList();
    }
}
```

### B. Enhanced AI Prompt With Memory Integration

```csharp
public class AIPromptBuilder
{
    // Existing code...
    
    public string BuildPrompt(GameWorld gameWorld, List<ChoiceTemplate> availableTemplates)
    {
        StringBuilder prompt = new StringBuilder();
        
        prompt.AppendLine("You are the AI Game Master for Wayfarer.");
        
        // Add game state context
        AddGameStateContext(prompt, gameWorld);
        
        // Add memory context - NEW
        AddMemoryContext(prompt, gameWorld.Player);
        
        // Add templates as JSON
        AddTemplatesAsJson(prompt, availableTemplates);
        
        // Add instructions
        AddInstructions(prompt);
        
        return prompt.ToString();
    }
    
    private void AddMemoryContext(StringBuilder prompt, Player player)
    {
        prompt.AppendLine("MEMORY CONTEXT:");
        
        List<MemoryFlag> recentMemories = player.GetRecentMemories();
        
        if (recentMemories.Count == 0)
        {
            prompt.AppendLine("- No significant memories yet.");
        }
        else
        {
            foreach (MemoryFlag memory in recentMemories)
            {
                prompt.AppendLine($"- {memory.Description} (from {gameWorld.CurrentDay - memory.CreationDay} days ago)");
            }
        }
        
        prompt.AppendLine();
    }
}
```

### C. Memory Effect Implementation

```csharp
// Create a memory effect class
public class CreateMemoryEffect : IMechanicalEffect
{
    private string memoryKey;
    private string memoryDescription;
    private int importance;
    private int expirationDays;
    
    public CreateMemoryEffect(string memoryKey, string memoryDescription, int importance, int expirationDays = -1)
    {
        this.memoryKey = memoryKey;
        this.memoryDescription = memoryDescription;
        this.importance = importance;
        this.expirationDays = expirationDays;
    }
    
    public void Apply(EncounterState state)
    {
        state.Player.AddMemory(memoryKey, memoryDescription, importance, expirationDays);
    }
    
    public string GetDescriptionForPlayer()
    {
        return "You will remember this event";
    }
}

// Create a memory check effect class
public class CheckMemoryEffect : IMechanicalEffect
{
    private string memoryKey;
    private IMechanicalEffect effectIfPresent;
    private IMechanicalEffect effectIfAbsent;
    
    public CheckMemoryEffect(string memoryKey, IMechanicalEffect effectIfPresent, IMechanicalEffect effectIfAbsent)
    {
        this.memoryKey = memoryKey;
        this.effectIfPresent = effectIfPresent;
        this.effectIfAbsent = effectIfAbsent;
    }
    
    public void Apply(EncounterState state)
    {
        if (state.Player.HasMemory(memoryKey))
        {
            effectIfPresent.Apply(state);
        }
        else
        {
            effectIfAbsent.Apply(state);
        }
    }
    
    public string GetDescriptionForPlayer()
    {
        return "Your past experiences affect this outcome";
    }
}
```

## IV. The Purpose Engine: Goal Structure Implementation

### A. Goal System Classes

```csharp
public enum GoalType
{
    Core,       // Main story goal
    Supporting, // Major milestone toward core goal
    Opportunity // Optional side goal
}

public class Goal
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public GoalType Type { get; private set; }
    public int CreationDay { get; private set; }
    public int Deadline { get; private set; } // -1 for no deadline
    public List<string> Requirements { get; private set; } = new List<string>();
    public List<string> CompletedRequirements { get; private set; } = new List<string>();
    public bool IsCompleted { get; private set; }
    public bool HasFailed { get; private set; }
    
    public float Progress => Requirements.Count > 0 
        ? (float)CompletedRequirements.Count / Requirements.Count 
        : 0f;
    
    public void CompleteRequirement(string requirement)
    {
        if (!Requirements.Contains(requirement) || CompletedRequirements.Contains(requirement))
            return;
            
        CompletedRequirements.Add(requirement);
        
        if (CompletedRequirements.Count == Requirements.Count)
        {
            IsCompleted = true;
        }
    }
    
    public bool CheckFailure(int currentDay)
    {
        if (Deadline != -1 && currentDay > Deadline && !IsCompleted)
        {
            HasFailed = true;
            return true;
        }
        
        return false;
    }
}

// Add to GameWorld
public class GameWorld
{
    // Existing properties...
    
    // Goal tracking
    public List<Goal> ActiveGoals { get; private set; } = new List<Goal>();
    public List<Goal> CompletedGoals { get; private set; } = new List<Goal>();
    public List<Goal> FailedGoals { get; private set; } = new List<Goal>();
    
    public void AddGoal(Goal goal)
    {
        ActiveGoals.Add(goal);
    }
    
    public void UpdateGoals()
    {
        for (int i = ActiveGoals.Count - 1; i >= 0; i--)
        {
            Goal goal = ActiveGoals[i];
            
            if (goal.IsCompleted)
            {
                ActiveGoals.RemoveAt(i);
                CompletedGoals.Add(goal);
            }
            else if (goal.CheckFailure(CurrentDay))
            {
                ActiveGoals.RemoveAt(i);
                FailedGoals.Add(goal);
            }
        }
    }
    
    public List<Goal> GetGoalsByType(GoalType type)
    {
        return ActiveGoals.Where(g => g.Type == type).ToList();
    }
}
```

### B. Goal-Related Effects

```csharp
public class CompleteGoalRequirementEffect : IMechanicalEffect
{
    private string goalName;
    private string requirementName;
    
    public CompleteGoalRequirementEffect(string goalName, string requirementName)
    {
        this.goalName = goalName;
        this.requirementName = requirementName;
    }
    
    public void Apply(EncounterState state)
    {
        Goal goal = state.Player.GameWorld.ActiveGoals.FirstOrDefault(g => g.Name == goalName);
        if (goal != null)
        {
            goal.CompleteRequirement(requirementName);
        }
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Progress toward {goalName}";
    }
}

public class AddGoalEffect : IMechanicalEffect
{
    private Goal goalToAdd;
    
    public AddGoalEffect(Goal goalToAdd)
    {
        this.goalToAdd = goalToAdd;
    }
    
    public void Apply(EncounterState state)
    {
        state.Player.GameWorld.AddGoal(goalToAdd);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"New goal: {goalToAdd.Name}";
    }
}
```

### C. AI Goal Awareness

```csharp
// Enhance AIPromptBuilder
private void AddGoalContext(StringBuilder prompt, GameWorld gameWorld)
{
    prompt.AppendLine("GOAL CONTEXT:");
    
    // Core goal
    List<Goal> coreGoals = gameWorld.GetGoalsByType(GoalType.Core);
    if (coreGoals.Any())
    {
        Goal coreGoal = coreGoals.First(); // Typically only one core goal
        prompt.AppendLine($"- Main Goal: {coreGoal.Name}");
        prompt.AppendLine($"  * {coreGoal.Description}");
        prompt.AppendLine($"  * Progress: {coreGoal.Progress * 100:0}%");
        
        if (coreGoal.Deadline != -1)
        {
            int daysRemaining = coreGoal.Deadline - gameWorld.CurrentDay;
            prompt.AppendLine($"  * Deadline: {daysRemaining} days remaining");
        }
    }
    
    // Supporting goals
    List<Goal> supportingGoals = gameWorld.GetGoalsByType(GoalType.Supporting);
    if (supportingGoals.Any())
    {
        prompt.AppendLine("- Supporting Goals:");
        foreach (Goal goal in supportingGoals)
        {
            prompt.AppendLine($"  * {goal.Name}: {goal.Progress * 100:0}% complete");
        }
    }
    
    // Opportunity goals (limit to 3 most recent)
    List<Goal> opportunityGoals = gameWorld.GetGoalsByType(GoalType.Opportunity)
        .OrderByDescending(g => g.CreationDay)
        .Take(3)
        .ToList();
        
    if (opportunityGoals.Any())
    {
        prompt.AppendLine("- Recent Opportunities:");
        foreach (Goal goal in opportunityGoals)
        {
            prompt.AppendLine($"  * {goal.Name}: {goal.Progress * 100:0}% complete");
        }
    }
    
    prompt.AppendLine();
}
```

## V. The Regional Tapestry: Cultural Depth Implementation

### A. Cultural Property System

```csharp
public class CulturalProperty
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> RelatedValues { get; private set; } = new List<string>();
    public List<string> Taboos { get; private set; } = new List<string>();
    public List<SkillCategories> FavoredApproaches { get; private set; } = new List<SkillCategories>();
    public List<SkillCategories> DislikedApproaches { get; private set; } = new List<SkillCategories>();
    
    public int GetApproachModifier(SkillCategories approach)
    {
        if (FavoredApproaches.Contains(approach)) return 1;
        if (DislikedApproaches.Contains(approach)) return -1;
        return 0;
    }
}

// Add to Location class
public class Location
{
    // Existing properties...
    
    // Cultural properties
    public List<CulturalProperty> CulturalProperties { get; private set; } = new List<CulturalProperty>();
    
    // Get total approach modifier for this location
    public int GetApproachModifier(SkillCategories approach)
    {
        int modifier = 0;
        
        foreach (CulturalProperty property in CulturalProperties)
        {
            modifier += property.GetApproachModifier(approach);
        }
        
        return modifier;
    }
}
```

### B. Factional System

```csharp
public class Faction
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public List<string> Values { get; private set; } = new List<string>();
    public List<string> Enemies { get; private set; } = new List<string>();
    public List<string> Allies { get; private set; } = new List<string>();
    public Dictionary<string, int> Relations { get; private set; } = new Dictionary<string, int>();
    
    // Check if this faction is at odds with another
    public bool IsHostileTo(string factionName)
    {
        return Enemies.Contains(factionName) || 
               (Relations.ContainsKey(factionName) && Relations[factionName] < 0);
    }
    
    // Check if this faction is friendly with another
    public bool IsFriendlyTo(string factionName)
    {
        return Allies.Contains(factionName) || 
               (Relations.ContainsKey(factionName) && Relations[factionName] > 0);
    }
}

// Add to GameWorld
public class GameWorld
{
    // Existing properties...
    
    // Faction tracking
    public Dictionary<string, Faction> Factions { get; private set; } = new Dictionary<string, Faction>();
    public Dictionary<string, int> PlayerFactionStanding { get; private set; } = new Dictionary<string, int>();
    
    public int GetPlayerStandingWithFaction(string factionName)
    {
        return PlayerFactionStanding.ContainsKey(factionName) 
            ? PlayerFactionStanding[factionName] 
            : 0;
    }
    
    public void ModifyPlayerFactionStanding(string factionName, int amount)
    {
        if (!PlayerFactionStanding.ContainsKey(factionName))
        {
            PlayerFactionStanding[factionName] = 0;
        }
        
        PlayerFactionStanding[factionName] += amount;
        
        // Clamp between -100 and 100
        PlayerFactionStanding[factionName] = Math.Max(-100, Math.Min(100, PlayerFactionStanding[factionName]));
        
        // Affect relations with allied/enemy factions
        if (Factions.ContainsKey(factionName))
        {
            Faction faction = Factions[factionName];
            
            // Helping this faction hurts relations with enemies
            if (amount > 0)
            {
                foreach (string enemy in faction.Enemies)
                {
                    ModifyPlayerFactionStanding(enemy, -amount / 2);
                }
            }
            
            // Hurting this faction helps relations with enemies
            if (amount < 0)
            {
                foreach (string enemy in faction.Enemies)
                {
                    ModifyPlayerFactionStanding(enemy, -amount / 2);
                }
            }
            
            // Helping/hurting this faction affects allies similarly
            foreach (string ally in faction.Allies)
            {
                ModifyPlayerFactionStanding(ally, amount / 2);
            }
        }
    }
}
```

### C. Cultural Effect Implementation

```csharp
public class FactionStandingEffect : IMechanicalEffect
{
    private string factionName;
    private int amount;
    
    public FactionStandingEffect(string factionName, int amount)
    {
        this.factionName = factionName;
        this.amount = amount;
    }
    
    public void Apply(EncounterState state)
    {
        state.Player.GameWorld.ModifyPlayerFactionStanding(factionName, amount);
    }
    
    public string GetDescriptionForPlayer()
    {
        if (amount > 0)
            return $"Improved standing with {factionName}";
        else
            return $"Reduced standing with {factionName}";
    }
}

// Extend ChoiceTemplate to check cultural context
public class ChoiceTemplate
{
    // Existing properties...
    
    // Check if this template is appropriate for the cultural context
    public bool IsAppropriateForCulture(Location location, SkillCategories approach)
    {
        int modifier = location.GetApproachModifier(approach);
        
        // Highly negative modifier means this approach is taboo
        if (modifier <= -2)
            return false;
            
        return true;
    }
    
    // Get cultural success/failure bonus
    public int GetCulturalBonus(Location location, SkillCategories approach)
    {
        return location.GetApproachModifier(approach);
    }
}
```

## VI. The Travel Value Engine: Meaningful Journeys

### A. Travel System Implementation

```csharp
public class TravelRoute
{
    public Location Origin { get; private set; }
    public Location Destination { get; private set; }
    public int BaseTimeCost { get; private set; }
    public int BaseEnergyCost { get; private set; }
    public int DangerLevel { get; private set; } // 0-10 scale
    public List<string> RequiredEquipment { get; private set; } = new List<string>();
    public List<TravelEncounter> PotentialEncounters { get; private set; } = new List<TravelEncounter>();
    
    // Knowledge level tracking - how well player knows this route
    public int KnowledgeLevel { get; private set; }
    
    // Increase knowledge when traveling this route
    public void IncreaseKnowledge()
    {
        if (KnowledgeLevel < 3)
            KnowledgeLevel++;
    }
    
    // Calculate actual costs based on knowledge level
    public int GetActualTimeCost()
    {
        // Knowledge reduces time cost
        return BaseTimeCost - KnowledgeLevel;
    }
    
    public int GetActualEnergyCost()
    {
        // Knowledge reduces energy cost
        return BaseEnergyCost - KnowledgeLevel;
    }
    
    // Check if player has required equipment
    public bool CanTravel(Player player)
    {
        foreach (string equipment in RequiredEquipment)
        {
            if (!player.HasItem(equipment))
                return false;
        }
        
        return true;
    }
    
    // Get a travel encounter if one should occur
    public TravelEncounter GetEncounter(int seed)
    {
        if (PotentialEncounters.Count == 0)
            return null;
            
        // Knowledge reduces encounter chance
        int encounterChance = 20 + (DangerLevel * 5) - (KnowledgeLevel * 10);
        
        // Determine if encounter happens
        Random random = new Random(seed);
        if (random.Next(100) < encounterChance)
        {
            // Select an encounter based on danger level
            List<TravelEncounter> appropriateEncounters = PotentialEncounters
                .Where(e => e.DangerLevel <= DangerLevel)
                .ToList();
                
            if (appropriateEncounters.Count > 0)
            {
                int index = random.Next(appropriateEncounters.Count);
                return appropriateEncounters[index];
            }
        }
        
        return null;
    }
}

public class TravelEncounter
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int DangerLevel { get; private set; }
    public List<ChoiceTemplate> EncounterTemplates { get; private set; } = new List<ChoiceTemplate>();
}

// Add travel methods to GameWorldManager
public class GameWorldManager
{
    // Existing code...
    
    public void Travel(TravelRoute route)
    {
        // Check if player can travel this route
        if (!route.CanTravel(gameWorld.Player))
            return;
            
        // Apply costs
        int timeCost = route.GetActualTimeCost();
        int energyCost = route.GetActualEnergyCost();
        
        gameWorld.Player.SpendEnergy(energyCost);
        gameWorld.AdvanceTime(TimeSpan.FromHours(timeCost));
        
        // Increase knowledge of this route
        route.IncreaseKnowledge();
        
        // Check for encounters
        int seed = gameWorld.CurrentDay + gameWorld.Player.GetHashCode();
        TravelEncounter encounter = route.GetEncounter(seed);
        
        if (encounter != null)
        {
            // Start a travel encounter
            StartEncounter(encounter.EncounterTemplates, encounter.Description);
        }
        else
        {
            // Arrived safely
            gameWorld.CurrentLocation = route.Destination;
            UpdateState();
        }
    }
}
```

### B. Route Discovery System

```csharp
public class GameWorld
{
    // Existing properties...
    
    // Travel knowledge tracking
    public Dictionary<string, List<TravelRoute>> KnownRoutes { get; private set; } = new Dictionary<string, List<TravelRoute>>();
    
    public void AddKnownRoute(TravelRoute route)
    {
        string originName = route.Origin.Name;
        
        if (!KnownRoutes.ContainsKey(originName))
        {
            KnownRoutes[originName] = new List<TravelRoute>();
        }
        
        // Only add if not already known
        if (!KnownRoutes[originName].Any(r => r.Destination.Name == route.Destination.Name))
        {
            KnownRoutes[originName].Add(route);
        }
    }
    
    public List<TravelRoute> GetRoutesFromCurrentLocation()
    {
        string currentLocationName = CurrentLocation.Name;
        
        if (KnownRoutes.ContainsKey(currentLocationName))
        {
            return KnownRoutes[currentLocationName];
        }
        
        return new List<TravelRoute>();
    }
}

// Route discovery effect
public class DiscoverRouteEffect : IMechanicalEffect
{
    private TravelRoute routeToDiscover;
    
    public DiscoverRouteEffect(TravelRoute routeToDiscover)
    {
        this.routeToDiscover = routeToDiscover;
    }
    
    public void Apply(EncounterState state)
    {
        state.Player.GameWorld.AddKnownRoute(routeToDiscover);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Discovered route to {routeToDiscover.Destination.Name}";
    }
}
```

## VII. The Anticipation Engine: Planning and Preparation

### A. Information System

```csharp
public class InformationItem
{
    public string Key { get; private set; }
    public string Title { get; private set; }
    public string Content { get; private set; }
    public List<string> Tags { get; private set; } = new List<string>();
    public bool IsSecretKnowledge { get; private set; }
    public int DiscoveryDay { get; private set; }
    
    public InformationItem(string key, string title, string content, List<string> tags, bool isSecretKnowledge)
    {
        Key = key;
        Title = title;
        Content = content;
        Tags = tags;
        IsSecretKnowledge = isSecretKnowledge;
        DiscoveryDay = gameWorld.CurrentDay;
    }
}

public class Player
{
    // Existing properties...
    
    // Knowledge tracking
    public List<InformationItem> KnownInformation { get; private set; } = new List<InformationItem>();
    
    public void LearnInformation(InformationItem item)
    {
        if (!KnownInformation.Any(i => i.Key == item.Key))
        {
            KnownInformation.Add(item);
        }
    }
    
    public bool KnowsInformation(string key)
    {
        return KnownInformation.Any(i => i.Key == key);
    }
    
    public List<InformationItem> GetInformationByTag(string tag)
    {
        return KnownInformation.Where(i => i.Tags.Contains(tag)).ToList();
    }
}

// Information discovery effect
public class LearnInformationEffect : IMechanicalEffect
{
    private InformationItem informationToLearn;
    
    public LearnInformationEffect(InformationItem informationToLearn)
    {
        this.informationToLearn = informationToLearn;
    }
    
    public void Apply(EncounterState state)
    {
        state.Player.LearnInformation(informationToLearn);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Learned information about {informationToLearn.Title}";
    }
}
```

### B. Preparation System

```csharp
public class PreparationAdvantage
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public TimeSpan Duration { get; private set; }
    public int SkillBonus { get; private set; }
    public List<SkillCategories> AffectedSkills { get; private set; } = new List<SkillCategories>();
    public List<string> RequiredItems { get; private set; } = new List<string>();
    public List<string> RequiredKnowledge { get; private set; } = new List<string>();
    public List<string> SpecificScenarios { get; private set; } = new List<string>();
    
    public DateTime ExpirationTime { get; private set; }
    
    public bool IsActive(DateTime currentTime)
    {
        return currentTime < ExpirationTime;
    }
    
    public bool IsApplicableToScenario(string scenario)
    {
        // If no specific scenarios listed, applies generally
        if (SpecificScenarios.Count == 0)
            return true;
            
        return SpecificScenarios.Contains(scenario);
    }
    
    public int GetBonusForSkill(SkillCategories skill)
    {
        if (AffectedSkills.Contains(skill))
            return SkillBonus;
            
        return 0;
    }
}

public class Player
{
    // Existing properties...
    
    // Active preparations
    public List<PreparationAdvantage> ActivePreparations { get; private set; } = new List<PreparationAdvantage>();
    
    public void AddPreparation(PreparationAdvantage preparation)
    {
        // Set expiration time
        preparation.ExpirationTime = gameWorld.CurrentTime + preparation.Duration;
        
        // Remove any existing preparation with same name
        ActivePreparations.RemoveAll(p => p.Name == preparation.Name);
        
        // Add new preparation
        ActivePreparations.Add(preparation);
    }
    
    public void UpdatePreparations()
    {
        // Remove expired preparations
        ActivePreparations.RemoveAll(p => !p.IsActive(gameWorld.CurrentTime));
    }
    
    public int GetPreparationBonus(SkillCategories skill, string scenario)
    {
        int bonus = 0;
        
        foreach (PreparationAdvantage prep in ActivePreparations)
        {
            if (prep.IsApplicableToScenario(scenario))
            {
                bonus += prep.GetBonusForSkill(skill);
            }
        }
        
        return bonus;
    }
}

// Preparation effect
public class GainPreparationEffect : IMechanicalEffect
{
    private PreparationAdvantage preparationToGain;
    
    public GainPreparationEffect(PreparationAdvantage preparationToGain)
    {
        this.preparationToGain = preparationToGain;
    }
    
    public void Apply(EncounterState state)
    {
        state.Player.AddPreparation(preparationToGain);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Gained preparation advantage: {preparationToGain.Name}";
    }
}
```

## VIII. Integration With AI Prompting

To bring all these systems together, enhance the AI prompt to include awareness of all these new systems:

```csharp
public class AIPromptBuilder
{
    public string BuildPrompt(GameWorld gameWorld, List<ChoiceTemplate> availableTemplates)
    {
        StringBuilder prompt = new StringBuilder();
        
        prompt.AppendLine("You are the AI Game Master for Wayfarer.");
        
        // Add core game state context
        AddGameStateContext(prompt, gameWorld);
        
        // Add time context - NEW
        AddTimeContext(prompt, gameWorld);
        
        // Add resource context - NEW
        AddResourceContext(prompt, gameWorld.Player);
        
        // Add memory context - NEW
        AddMemoryContext(prompt, gameWorld.Player);
        
        // Add goal context - NEW
        AddGoalContext(prompt, gameWorld);
        
        // Add cultural context - NEW
        AddCulturalContext(prompt, gameWorld.CurrentLocation);
        
        // Add faction context - NEW
        AddFactionContext(prompt, gameWorld);
        
        // Add preparation context - NEW
        AddPreparationContext(prompt, gameWorld.Player);
        
        // Add travel context - NEW
        AddTravelContext(prompt, gameWorld);
        
        // Add templates as JSON
        AddTemplatesAsJson(prompt, FilterTemplatesForContext(availableTemplates, gameWorld));
        
        // Add instructions with context-awareness
        AddEnhancedInstructions(prompt, gameWorld);
        
        return prompt.ToString();
    }
    
    // Filter templates based on current context
    private List<ChoiceTemplate> FilterTemplatesForContext(List<ChoiceTemplate> allTemplates, GameWorld gameWorld)
    {
        // In your current design, you don't filter templates - AI gets all of them
        // But this method could be used to prioritize contextually appropriate ones
        return allTemplates;
    }
    
    private void AddTimeContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("TIME CONTEXT:");
        prompt.AppendLine($"- Current Day: {gameWorld.CurrentDay}");
        prompt.AppendLine($"- Time of Day: {gameWorld.CurrentTimeOfDay}");
        
        if (gameWorld.DeadlineDay > 0)
        {
            int daysRemaining = gameWorld.DeadlineDay - gameWorld.CurrentDay;
            prompt.AppendLine($"- Deadline: {daysRemaining} days remaining");
            prompt.AppendLine($"- Deadline Reason: {gameWorld.DeadlineReason}");
        }
        
        prompt.AppendLine();
    }
    
    private void AddResourceContext(StringBuilder prompt, Player player)
    {
        prompt.AppendLine("RESOURCE CONTEXT:");
        prompt.AppendLine($"- Energy: {player.Energy}/{player.MaxEnergy}");
        prompt.AppendLine($"- Money: {player.Money} coins");
        prompt.AppendLine($"- Reputation: {player.Reputation} ({player.GetReputationLevel()})");
        
        prompt.AppendLine();
    }
    
    private void AddCulturalContext(StringBuilder prompt, Location location)
    {
        prompt.AppendLine("CULTURAL CONTEXT:");
        
        foreach (CulturalProperty property in location.CulturalProperties)
        {
            prompt.AppendLine($"- {property.Name}: {property.Description}");
            
            if (property.FavoredApproaches.Any())
            {
                prompt.AppendLine($"  * Favored approaches: {string.Join(", ", property.FavoredApproaches)}");
            }
            
            if (property.DislikedApproaches.Any())
            {
                prompt.AppendLine($"  * Disliked approaches: {string.Join(", ", property.DislikedApproaches)}");
            }
        }
        
        prompt.AppendLine();
    }
    
    private void AddFactionContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("FACTION CONTEXT:");
        
        // List active factions at current location
        List<string> localFactions = gameWorld.CurrentLocation.GetActiveFactions();
        
        foreach (string factionName in localFactions)
        {
            if (gameWorld.Factions.ContainsKey(factionName))
            {
                Faction faction = gameWorld.Factions[factionName];
                int standing = gameWorld.GetPlayerStandingWithFaction(factionName);
                
                prompt.AppendLine($"- {faction.Name}: Standing {standing}");
                prompt.AppendLine($"  * {faction.Description}");
                
                // Only include values if player has interacted with this faction
                if (standing != 0)
                {
                    prompt.AppendLine($"  * Values: {string.Join(", ", faction.Values)}");
                }
            }
        }
        
        prompt.AppendLine();
    }
    
    private void AddPreparationContext(StringBuilder prompt, Player player)
    {
        prompt.AppendLine("PREPARATION CONTEXT:");
        
        if (player.ActivePreparations.Any())
        {
            foreach (PreparationAdvantage prep in player.ActivePreparations)
            {
                prompt.AppendLine($"- {prep.Name}: {prep.Description}");
                prompt.AppendLine($"  * Provides +{prep.SkillBonus} to {string.Join(", ", prep.AffectedSkills)}");
                
                if (prep.SpecificScenarios.Any())
                {
                    prompt.AppendLine($"  * Specific to: {string.Join(", ", prep.SpecificScenarios)}");
                }
            }
        }
        else
        {
            prompt.AppendLine("- No active preparations");
        }
        
        prompt.AppendLine();
    }
    
    private void AddTravelContext(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("TRAVEL CONTEXT:");
        prompt.AppendLine($"- Current Location: {gameWorld.CurrentLocation.Name}");
        
        List<TravelRoute> availableRoutes = gameWorld.GetRoutesFromCurrentLocation();
        
        if (availableRoutes.Any())
        {
            prompt.AppendLine("- Available Routes:");
            
            foreach (TravelRoute route in availableRoutes)
            {
                prompt.AppendLine($"  * To {route.Destination.Name}:");
                prompt.AppendLine($"    - Time: {route.GetActualTimeCost()} hours");
                prompt.AppendLine($"    - Energy: {route.GetActualEnergyCost()} points");
                prompt.AppendLine($"    - Danger: {route.DangerLevel}/10");
                prompt.AppendLine($"    - Knowledge: Level {route.KnowledgeLevel}/3");
                
                if (route.RequiredEquipment.Any())
                {
                    bool canTravel = route.CanTravel(gameWorld.Player);
                    string status = canTravel ? "Requirements met" : "Missing requirements";
                    prompt.AppendLine($"    - Requirements: {string.Join(", ", route.RequiredEquipment)} ({status})");
                }
            }
        }
        else
        {
            prompt.AppendLine("- No known routes from this location");
        }
        
        prompt.AppendLine();
    }
    
    private void AddEnhancedInstructions(StringBuilder prompt, GameWorld gameWorld)
    {
        prompt.AppendLine("INSTRUCTIONS:");
        prompt.AppendLine("1. Generate a narrative beat description (2-3 sentences) appropriate to the current time of day and cultural context.");
        prompt.AppendLine("2. Create 3-4 distinct choices that advance toward the player's goals and are culturally appropriate.");
        prompt.AppendLine("3. For each choice:");
        prompt.AppendLine("   - Set Focus cost (0-2)");
        prompt.AppendLine("   - Define which skill cards can be used");
        prompt.AppendLine("   - Set appropriate difficulty (Easy=2, Standard=3, Hard=4, Exceptional=5)");
        prompt.AppendLine("   - Include any special bonuses from preparations");
        prompt.AppendLine("   - Select appropriate template from the provided options");
        prompt.AppendLine("   - Ensure narrative descriptions match cultural context");
        
        // Add contextual guidance
        if (gameWorld.DeadlineDay > 0)
        {
            int daysRemaining = gameWorld.DeadlineDay - gameWorld.CurrentDay;
            
            if (daysRemaining <= 3)
            {
                prompt.AppendLine("4. IMPORTANT: Create a sense of urgency due to the approaching deadline.");
            }
        }
        
        // Add cultural guidance
        if (gameWorld.CurrentLocation.CulturalProperties.Any())
        {
            prompt.AppendLine("5. Ensure choices respect local cultural values and taboos.");
        }
        
        // Add memory guidance
        if (gameWorld.Player.Memories.Any())
        {
            prompt.AppendLine("6. Reference the player's past experiences where relevant.");
        }
        
        prompt.AppendLine();
    }
}
```

## IX. Conclusion: Integration Roadmap

To implement these systems incrementally without disrupting existing Wayfarer architecture:

1. **Phase 1: Time System Foundation**
   - Implement GameWorld time tracking (CurrentDay, CurrentTimeOfDay)
   - Add TimeOfDay-based property system to Location
   - Create the Deadline framework for time pressure

2. **Phase 2: Resource Extension**
   - Extend Player with Energy, Reputation, and Money
   - Implement interdependence effects
   - Create contextual cost calculation

3. **Phase 3: Memory and Narrative Continuity**
   - Add Memory system to Player
   - Create Memory-related effect classes
   - Enhance AI prompting with memory context

4. **Phase 4: Goal Structure**
   - Implement Goal tracking system
   - Create goal-related effects
   - Add goal awareness to AI prompting

5. **Phase 5: Culture and Faction Systems**
   - Add cultural properties to locations
   - Implement faction tracking
   - Create cultural and factional effects

6. **Phase 6: Travel and Preparation**
   - Implement route system
   - Create travel encounters
   - Add preparation advantages

7. **Phase 7: AI Prompt Enhancement**
   - Update AIPromptBuilder to include all new context
   - Create enhanced template system aware of new dimensions
   - Refine AI instructions to leverage new systems

This implementation path preserves your current architecture while layering in the rich dynamics inspired by 80 Days, creating a much more cohesive and engaging player journey.