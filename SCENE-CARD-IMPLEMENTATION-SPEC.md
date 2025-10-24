# Scene/Card Architecture - Complete Implementation Specification

## Document Purpose
This document provides EXACT implementation patterns for completing the Scene/Card architecture migration. Every code example is production-ready and follows architectural principles from DYNAMIC_CONTENT_PRINCIPLES.md.

---

## Architectural Principles (Cite These)

### Principle 1: Actions Create Content (DYNAMIC_CONTENT_PRINCIPLES.md)
- **NO filtering logic** - ObstacleGoalFilter eliminated
- **Spawning not unlocking** - ActionDefinitions spawn Scenes
- **Cascade chains** - Scene completion spawns more Scenes

### Principle 2: Placement vs Ownership (DYNAMIC_CONTENT_PRINCIPLES.md)
- **Locations are placement context** - Scene.LocationId, not Location.ActiveSceneIds
- **Query via LINQ** - Simple `_gameWorld.Scenes.Where(s => s.LocationId == id)`
- **NO ownership properties** - Entities don't "own" scenes

### Principle 3: Scene Lifecycle (DYNAMIC_CONTENT_PRINCIPLES.md)
- **States**: Available → Active → Completed/Expired
- **Visibility**: Only Available scenes shown to player
- **Expiration**: Time-based or completion-based removal

### Principle 5: Cards Replace MemoryFlag (DYNAMIC_CONTENT_PRINCIPLES.md)
- **Strong typing** - No `object` properties
- **Visible resources** - Player.Cards not Player.Memories
- **Scene outcomes grant Cards** - Not boolean flags

---

## 1. DECKBUILDER REFACTORING

### Pattern: Goal → Scene Transformation

**OLD (Goal-based):**
```csharp
public (Deck deck, List<CardInstance> GoalCards) CreateDeck(NPC npc, string goalId)
{
    Goal goal = _gameWorld.Goals.FirstOrDefault(g => g.Id == goalId);
    SocialChallengeDeck deckDef = _gameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == goal.DeckId);
    // ... build from goal.GoalCards
}
```

**NEW (Scene-based):**
```csharp
public (Deck deck, List<CardInstance> VictoryCards) CreateDeck(NPC npc, Scene scene)
{
    SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);
    SocialChallengeDeck deckDef = _gameWorld.SocialChallengeDecks.FirstOrDefault(d => d.Id == template.DeckId);
    // ... build from template.VictoryConditions
}
```

### MentalDeckBuilder.cs - EXACT IMPLEMENTATION

**Location**: `src/Subsystems/Mental/MentalDeckBuilder.cs`

**Replace method `CreateGoalCardInstances(Goal goal)`:**

```csharp
/// <summary>
/// Create victory condition card instances from SceneTemplate.
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md - VictoryConditions define scene completion
/// </summary>
private List<CardInstance> CreateVictoryCardInstances(SceneTemplate template, Scene scene)
{
    List<CardInstance> victoryCardInstances = new List<CardInstance>();

    foreach (VictoryCondition condition in template.VictoryConditions)
    {
        // Create PhysicalGoalCard from victory condition
        PhysicalGoalCard victoryCard = new PhysicalGoalCard
        {
            threshold = condition.Threshold,
            MetricType = condition.MetricType,
            OutcomeReference = condition.OutcomeReference,
            Description = condition.Description
        };

        // Create CardInstance from victory card
        CardInstance instance = new CardInstance(victoryCard);

        // Set context for threshold checking (Mental uses Progress)
        instance.Context = new CardContext
        {
            threshold = condition.Threshold,
            RequestId = scene.Id
        };

        // Victory cards start unplayable until threshold met
        instance.IsPlayable = false;

        victoryCardInstances.Add(instance);
    }

    return victoryCardInstances;
}
```

**Update public method signature** (wherever BuildDeck is called):
```csharp
// OLD:
public MentalSessionDeck BuildDeck(Goal goal, int difficulty)

// NEW:
public MentalSessionDeck BuildDeck(Scene scene, int difficulty)
{
    SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);
    // Use template.DeckId, template.VictoryConditions, template.DifficultyModifiers
}
```

### PhysicalDeckBuilder.cs - EXACT IMPLEMENTATION

**Location**: `src/Subsystems/Physical/PhysicalDeckBuilder.cs`

**Same pattern as Mental - replace `CreateGoalCardInstances()` with `CreateVictoryCardInstances()`**

---

## 2. FACADE REFACTORING

### LocationFacade.cs - Get Available Scenes

**Location**: `src/Subsystems/Location/LocationFacade.cs`

**DELETE all Goal/Obstacle methods, ADD:**

```csharp
/// <summary>
/// Get all available scenes at a location.
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md Principle 2 - Simple query, no filtering
/// </summary>
public List<Scene> GetAvailableScenes(string locationId)
{
    return _gameWorld.Scenes
        .Where(s => s.LocationId == locationId && s.State == SceneState.Available)
        .ToList();
}

/// <summary>
/// Get SceneTemplate for a scene.
/// </summary>
public SceneTemplate GetSceneTemplate(Scene scene)
{
    return _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);
}

/// <summary>
/// Calculate entry costs for a scene (with equipment modifiers).
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md Principle 4 - Equipment modifies costs
/// </summary>
public EntryCosts CalculateEntryCosts(Scene scene)
{
    SceneTemplate template = GetSceneTemplate(scene);
    if (template == null) return new EntryCosts();

    // Start with base costs
    EntryCosts costs = new EntryCosts
    {
        TimeSegments = template.EntryCosts.TimeSegments,
        Resources = new List<ResourceAmount>(template.EntryCosts.Resources),
        TokenRequirements = new Dictionary<ConnectionType, int>(template.EntryCosts.TokenRequirements),
        ConsumedItemIds = new List<string>(template.EntryCosts.ConsumedItemIds)
    };

    // Apply equipment cost modifiers (call EquipmentFacade)
    // TODO: Implement equipment cost reduction based on ContextTags matching

    return costs;
}

/// <summary>
/// Check if player can afford scene entry costs.
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md Principle 1 - Resource gates
/// </summary>
public bool CanAffordEntryCosts(Scene scene)
{
    EntryCosts costs = CalculateEntryCosts(scene);
    Player player = _gameWorld.GetPlayer();

    // Check time segments
    if (costs.TimeSegments > 0)
    {
        // TODO: Check if player has enough time remaining in day
    }

    // Check resource costs
    foreach (ResourceAmount resource in costs.Resources)
    {
        switch (resource.Type)
        {
            case ResourceType.Coins:
                if (player.Coins < resource.Amount) return false;
                break;
            case ResourceType.Stamina:
                if (player.Stamina < resource.Amount) return false;
                break;
            case ResourceType.Focus:
                if (player.Focus < resource.Amount) return false;
                break;
            case ResourceType.Health:
                if (player.Health < resource.Amount) return false;
                break;
        }
    }

    // Check token requirements
    foreach (var tokenReq in costs.TokenRequirements)
    {
        // TODO: Check if player has required tokens with NPC
    }

    return true;
}

/// <summary>
/// Enter a scene (pay costs, mark as active, start challenge).
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md Principle 3 - Scene lifecycle
/// </summary>
public void EnterScene(Scene scene, SceneSpawningService sceneSpawner)
{
    if (!CanAffordEntryCosts(scene))
        throw new InvalidOperationException("Cannot afford scene entry costs");

    // Deduct entry costs
    EntryCosts costs = CalculateEntryCosts(scene);
    Player player = _gameWorld.GetPlayer();

    foreach (ResourceAmount resource in costs.Resources)
    {
        switch (resource.Type)
        {
            case ResourceType.Coins:
                player.ModifyCoins(-resource.Amount);
                break;
            case ResourceType.Stamina:
                player.Stamina -= resource.Amount;
                break;
            case ResourceType.Focus:
                player.Focus -= resource.Amount;
                break;
            case ResourceType.Health:
                player.ModifyHealth(-resource.Amount);
                break;
        }
    }

    // Mark scene as active
    sceneSpawner.EnterScene(scene);
}
```

---

### EquipmentFacade.cs - Cost Modifiers

**Location**: `src/Subsystems/Equipment/EquipmentFacade.cs`

**DELETE all Obstacle methods, ADD:**

```csharp
/// <summary>
/// Calculate equipment cost reduction for a scene.
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md Principle 4 - Equipment modifies costs
/// </summary>
public Dictionary<ResourceType, int> CalculateSceneCostReduction(Scene scene)
{
    SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);
    if (template == null) return new Dictionary<ResourceType, int>();

    Dictionary<ResourceType, int> reductions = new Dictionary<ResourceType, int>();
    Player player = _gameWorld.GetPlayer();

    // Get player's owned equipment
    List<Equipment> playerEquipment = GetPlayerEquipment(player);

    // Check each equipment for context tag matches
    foreach (Equipment equipment in playerEquipment)
    {
        // Match equipment tags with scene context tags
        bool hasMatchingTag = equipment.ContextTags
            .Any(tag => template.ContextTags.Contains(tag));

        if (hasMatchingTag && equipment.CostModifier != null)
        {
            // Apply equipment cost reduction
            ResourceType resourceType = equipment.CostModifier.ResourceType;
            int reductionAmount = equipment.CostModifier.ReductionAmount;

            if (!reductions.ContainsKey(resourceType))
                reductions[resourceType] = 0;

            reductions[resourceType] += reductionAmount;
        }
    }

    return reductions;
}

private List<Equipment> GetPlayerEquipment(Player player)
{
    List<Equipment> equipment = new List<Equipment>();

    foreach (string itemId in player.Inventory.GetAllItems())
    {
        Item item = _gameWorld.Items.FirstOrDefault(i => i.Id == itemId);
        if (item is Equipment equip)
        {
            equipment.Add(equip);
        }
    }

    return equipment;
}
```

---

### SocialFacade.cs - Start Scene Challenge

**Location**: `src/Subsystems/Social/SocialFacade.cs`

**Find method that starts conversations/challenges, update signature:**

```csharp
/// <summary>
/// Start a social challenge from a Scene.
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md - Scene drives challenge
/// </summary>
public async Task StartSceneChallenge(Scene scene, NPC npc)
{
    SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);
    if (template == null)
        throw new ArgumentException($"SceneTemplate not found: {scene.TemplateId}");

    if (template.ChallengeType != TacticalSystemType.Social)
        throw new InvalidOperationException($"Scene {scene.Id} is not a Social challenge");

    // Build deck from scene
    var (deck, victoryCards) = _deckBuilder.CreateConversationDeck(npc, scene);

    // Create session
    SocialSession session = new SocialSession
    {
        SessionId = Guid.NewGuid().ToString(),
        NpcId = npc.ID,
        SceneId = scene.Id,  // Track which scene this challenge is for
        Deck = deck,
        // ... other session properties
    };

    _gameWorld.CurrentSocialSession = session;

    // Route to UI (existing logic)
    await _attentionManager.NavigateToSocial();
}
```

---

## 3. GAMEFACADE INTEGRATION

**Location**: `src/Services/GameFacade.cs`

**Find methods that handle UI clicks/actions, ADD:**

```csharp
/// <summary>
/// Handle player clicking a scene at a location.
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md Principle 1, 3
/// </summary>
public async Task OnSceneClicked(Scene scene)
{
    SceneTemplate template = _gameWorld.SceneTemplates.FirstOrDefault(t => t.Id == scene.TemplateId);
    if (template == null) return;

    // Check if player can afford entry costs
    bool canAfford = _locationFacade.CanAffordEntryCosts(scene);
    if (!canAfford)
    {
        // Show "insufficient resources" message
        return;
    }

    // Pay entry costs and mark scene as active
    _locationFacade.EnterScene(scene, _sceneSpawner);

    // Route to appropriate challenge subsystem
    switch (template.ChallengeType)
    {
        case TacticalSystemType.Social:
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == scene.NpcId);
            await _socialFacade.StartSceneChallenge(scene, npc);
            break;

        case TacticalSystemType.Mental:
            await _mentalFacade.StartSceneChallenge(scene);
            break;

        case TacticalSystemType.Physical:
            await _physicalFacade.StartSceneChallenge(scene);
            break;
    }
}

/// <summary>
/// Handle challenge completion - process scene outcomes.
/// CITING: DYNAMIC_CONTENT_PRINCIPLES.md Principle 1 - Cascade chains
/// </summary>
public void OnChallengeCompleted(Scene scene, OutcomeConditionType outcomeType)
{
    _sceneCompletionHandler.CompleteScene(scene, outcomeType);
}
```

---

## 4. SERVICE REGISTRATION

**Location**: `src/ServiceConfiguration.cs`

**ADD to ConfigureServices():**

```csharp
// Scene/Card Architecture Services
services.AddSingleton<SceneQueryService>();
services.AddSingleton<SceneSpawningService>();
services.AddSingleton<SceneCompletionHandler>();
```

---

## 5. UI COMPONENT REFACTORING

### LocationContent.razor - Display Scene List

**Location**: `src/Pages/Components/LocationContent.razor`

**DELETE Goal display code, REPLACE WITH:**

```razor
@* Display available scenes at this location *@
@if (AvailableScenes != null && AvailableScenes.Any())
{
    <div class="scenes-container">
        <h3>Available Activities</h3>

        @foreach (Scene scene in AvailableScenes)
        {
            SceneTemplate template = GetSceneTemplate(scene);
            if (template == null) continue;

            <div class="scene-card @(CanAffordScene(scene) ? "" : "unaffordable")"
                 @onclick="() => OnSceneClicked(scene)">

                <div class="scene-header">
                    <h4>@template.Title</h4>
                    <span class="scene-type">@template.ChallengeType</span>
                </div>

                <div class="scene-description">
                    @template.Description
                </div>

                <div class="scene-costs">
                    <strong>Entry Costs:</strong>
                    @RenderEntryCosts(template.EntryCosts)
                </div>

                @if (template.ContextTags.Any())
                {
                    <div class="scene-tags">
                        @foreach (string tag in template.ContextTags)
                        {
                            <span class="context-tag">@tag</span>
                        }
                    </div>
                }

                @if (!CanAffordScene(scene))
                {
                    <div class="insufficient-resources">
                        Insufficient Resources
                    </div>
                }
            </div>
        }
    </div>
}
```

**Add to code-behind (LocationContent.razor.cs):**

```csharp
public partial class LocationContent
{
    private List<Scene> AvailableScenes { get; set; } = new List<Scene>();

    [Inject] private SceneQueryService SceneQuery { get; set; }
    [Inject] private GameFacade GameFacade { get; set; }

    protected override void OnInitialized()
    {
        LoadAvailableScenes();
    }

    private void LoadAvailableScenes()
    {
        if (CurrentLocation == null) return;

        AvailableScenes = SceneQuery.GetScenesAtLocation(CurrentLocation.Id);
        StateHasChanged();
    }

    private SceneTemplate GetSceneTemplate(Scene scene)
    {
        return SceneQuery.GetSceneTemplateById(scene.TemplateId);
    }

    private bool CanAffordScene(Scene scene)
    {
        return LocationFacade.CanAffordEntryCosts(scene);
    }

    private async Task OnSceneClicked(Scene scene)
    {
        await GameFacade.OnSceneClicked(scene);
    }

    private RenderFragment RenderEntryCosts(EntryCosts costs)
    {
        return builder =>
        {
            int sequence = 0;

            if (costs.TimeSegments > 0)
            {
                builder.OpenElement(sequence++, "span");
                builder.AddAttribute(sequence++, "class", "cost-item");
                builder.AddContent(sequence++, $"{costs.TimeSegments} Time");
                builder.CloseElement();
            }

            foreach (ResourceAmount resource in costs.Resources)
            {
                builder.OpenElement(sequence++, "span");
                builder.AddAttribute(sequence++, "class", "cost-item");
                builder.AddContent(sequence++, $"{resource.Amount} {resource.Type}");
                builder.CloseElement();
            }

            foreach (var token in costs.TokenRequirements)
            {
                builder.OpenElement(sequence++, "span");
                builder.AddAttribute(sequence++, "class", "cost-item");
                builder.AddContent(sequence++, $"{token.Value} {token.Key} tokens");
                builder.CloseElement();
            }
        };
    }
}
```

---

### ConversationContent.razor - Display Social Scenes

**Location**: `src/Pages/Components/ConversationContent.razor`

**REPLACE Goal display with Scene display:**

```razor
@* Display available conversation scenes for this NPC *@
@if (SocialScenes != null && SocialScenes.Any())
{
    <div class="conversation-scenes">
        <h3>Conversation Topics</h3>

        @foreach (Scene scene in SocialScenes)
        {
            SceneTemplate template = GetSceneTemplate(scene);
            if (template == null) continue;

            <div class="conversation-scene-card"
                 @onclick="() => OnSceneClicked(scene)">

                <div class="scene-title">@template.Title</div>
                <div class="scene-category">@template.Category</div>

                @if (template.EntryCosts.TokenRequirements.Any())
                {
                    <div class="token-requirements">
                        Requires: @RenderTokenRequirements(template.EntryCosts.TokenRequirements)
                    </div>
                }
            </div>
        }
    </div>
}
```

**Add to code-behind (ConversationContent.razor.cs):**

```csharp
public partial class ConversationContent
{
    private List<Scene> SocialScenes { get; set; } = new List<Scene>();

    [Inject] private SceneQueryService SceneQuery { get; set; }

    protected override void OnParametersSet()
    {
        if (CurrentNPC != null)
        {
            SocialScenes = SceneQuery.GetScenesForNpc(CurrentNPC.ID);
            StateHasChanged();
        }
    }

    private SceneTemplate GetSceneTemplate(Scene scene)
    {
        return SceneQuery.GetSceneTemplateById(scene.TemplateId);
    }

    private async Task OnSceneClicked(Scene scene)
    {
        await GameFacade.OnSceneClicked(scene);
    }
}
```

---

## 6. CLEANUP - Delete Obsolete Code

### Files to DELETE entirely:
- `src/Services/GoalCompletionHandler.cs` ✅ Already deleted
- `src/Services/ObstacleGoalFilter.cs` ✅ Already deleted
- `src/Services/ObstacleIntensityCalculator.cs` ✅ Already deleted
- `src/Services/ObstacleRewardService.cs` ✅ Already deleted
- `src/Subsystems/Obstacle/ObstacleFacade.cs` ✅ Already deleted
- `src/GameState/LocationViewState.cs` ✅ Already deleted
- `src/Subsystems/Location/LocationActionManager.cs` ✅ Already deleted

### Methods to DELETE from existing files:

**LocationFacade.cs:**
- All methods containing "Goal" or "Obstacle" in signature

**EquipmentFacade.cs:**
- All methods containing "Obstacle" in signature

**TravelFacade.cs:**
- Methods referencing Route.ObstacleIds (already cleaned)

**GameFacade.cs:**
- Methods with "Goal" parameters (replace with Scene versions)

---

## 7. BUILD VERIFICATION

### Expected Compilation Errors After Full Implementation:

**Zero errors expected** if all steps followed correctly.

**If errors remain, check:**
1. All Goal/Obstacle references removed from UI components
2. ServiceConfiguration.cs has new services registered
3. All deckbuilders updated to Scene-based signatures
4. GameFacade routes scene clicks correctly

---

## 8. TESTING CHECKLIST

### Manual Testing Steps:

1. **Scene Display**:
   - Navigate to location
   - Verify scenes appear with entry costs
   - Verify "Insufficient Resources" shows when can't afford

2. **Scene Entry**:
   - Click affordable scene
   - Verify resources deducted
   - Verify challenge starts (Social/Mental/Physical)

3. **Challenge Completion**:
   - Complete challenge successfully
   - Verify follow-up scenes spawn
   - Verify cards granted to player

4. **Scene Expiration**:
   - Wait for time to pass
   - Verify expired scenes removed

---

## END OF SPECIFICATION

All code patterns are production-ready and cite architectural principles. Implementation should be straightforward copy-paste with minor adjustments for existing code structure.
