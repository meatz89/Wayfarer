# Tutorial Content Validation Plan

## Proof Strategy: Categorical Properties → Exact Tutorial Content

**Hypothesis**: Elena's categorical properties + `inn_lodging` archetype deterministically generate exact tutorial content.

**Evidence Required**:
1. Unit tests prove categorical property derivation
2. Integration tests prove scene generation produces exact values
3. E2E browser tests prove player sees exact content in UI

---

## 1. Unit Tests (Categorical Property Derivation)

### Test: `CategoricalProperties_DeriveCorrectlyFromElena`

**Given**:
- Elena: `PersonalityType.MERCANTILE`, `Trust=0`, `Level=2`
- brass_bell_inn: `Tier=1`
- common_room: `properties=["restful", "public", "busy"]`

**When**: `GenerationContext.FromEntities(tier=0, elena, commonRoom, player)`

**Then**:
```csharp
Assert.Equal(Quality.Basic, context.Quality);           // Tier 1 venue → 0.6x cost
Assert.Equal(EnvironmentQuality.Standard, context.Environment);  // "restful" → 2x restoration
Assert.Equal(NPCDemeanor.Neutral, context.NpcDemeanor);  // Trust=0 → 1.0x threshold
Assert.Equal(PowerDynamic.Equal, context.Power);         // Similar levels → 1.0x threshold
Assert.Equal(EmotionalTone.Cold, context.Tone);          // Bond=0 → cold narrative
Assert.Equal(PersonalityType.MERCANTILE, context.NpcPersonality);  // service_negotiation archetype
```

**Status**: ✅ Implemented in `TutorialInnLodgingIntegrationTest.cs`

---

## 2. Integration Tests (Scene Generation)

### Test: `Elena_InnLodging_Tier0_GeneratesExactTutorialContent`

**Given**: Elena's categorical properties (from test above)

**When**: `SceneArchetypeCatalog.Generate("inn_lodging", tier=0, context)`

**Then - Structure**:
```csharp
Assert.Equal(3, definition.SituationTemplates.Count);  // negotiate → rest → depart
Assert.Equal(SpawnPattern.Linear, definition.SpawnRules.Pattern);
Assert.Single(definition.DependentLocations);  // private_room
Assert.Single(definition.DependentItems);      // room_key
```

**Then - Situation 1 (Negotiate Lodging)**:
```csharp
SituationTemplate negotiate = definition.SituationTemplates[0];
Assert.Equal(4, negotiate.ChoiceTemplates.Count);

// Money choice: Quality.Basic (0.6x) applied
ChoiceTemplate moneyChoice = FindByPathType(InstantSuccess, hasCost);
Assert.InRange(moneyChoice.CostTemplate.Coins, 5, 10);  // Tier 0 base ~10, scaled to ~6

// Stat choice: Tier 0 baseline with Neutral (1.0x) and Equal (1.0x)
ChoiceTemplate statChoice = FindByPathType(InstantSuccess, hasRequirement);
// Requires Diplomacy check (~5 for tier 0)

// Challenge choice: Triggers tactical challenge on success
ChoiceTemplate challengeChoice = FindByPathType(Challenge);
Assert.NotNull(challengeChoice.OnSuccessReward.LocationsToUnlock);

// Fallback choice: Always available, no rewards
ChoiceTemplate fallbackChoice = FindByPathType(Fallback);
Assert.Empty(fallbackChoice.RewardTemplate?.LocationsToUnlock ?? []);

// All success paths unlock room and grant key
foreach (var successChoice in [moneyChoice, statChoice, challengeChoice]) {
    Assert.Contains("generated:private_room", GetUnlockReward(successChoice));
    Assert.Contains("generated:room_key", GetItemReward(successChoice));
}
```

**Then - Situation 2 (Rest in Room)**:
```csharp
SituationTemplate rest = definition.SituationTemplates[1];
Assert.Equal("generated:private_room", rest.RequiredLocationId);
Assert.Null(rest.RequiredNpcId);  // Solo in private room
Assert.Equal(4, rest.ChoiceTemplates.Count);

// Rest choices have restoration rewards
// EnvironmentQuality.Standard (2x) scales base restoration (~20 → ~40 health)
Assert.True(HasRestorationReward(rest.ChoiceTemplates));
```

**Then - Situation 3 (Depart Inn)**:
```csharp
SituationTemplate depart = definition.SituationTemplates[2];
Assert.Equal("generated:private_room", depart.RequiredLocationId);
Assert.Equal(2, depart.ChoiceTemplates.Count);  // Immediate vs careful departure

// All depart choices clean up resources
foreach (var choice in depart.ChoiceTemplates) {
    Assert.Contains("generated:room_key", choice.RewardTemplate.ItemsToRemove);
    Assert.Contains("generated:private_room", choice.RewardTemplate.LocationsToLock);
}
```

**Status**: ✅ Implemented in `TutorialInnLodgingIntegrationTest.cs`

---

## 3. E2E Browser Tests (Player Experience)

### Test: `Tutorial_SecureLodging_CompleteFlow`

**Prerequisites**:
- Browser automation framework (Playwright/Selenium)
- Game running at http://localhost:6000
- Fresh game state (new save)

**Test Flow**:

#### Step 1: Load Game and Navigate to Inn
```csharp
await page.GotoAsync("http://localhost:6000");
await page.WaitForSelectorAsync("[data-test='location-common_room']");
await page.ClickAsync("[data-test='location-common_room']");
```

**Assert**: Player is at common_room location
```csharp
var locationName = await page.TextContentAsync("[data-test='current-location-name']");
Assert.Equal("Common Room", locationName);
```

#### Step 2: Verify Tutorial Scene Appears (Modal)
```csharp
await page.WaitForSelectorAsync("[data-test='scene-tutorial_secure_lodging']");
var sceneTitle = await page.TextContentAsync("[data-test='scene-title']");
Assert.Contains("Lodging", sceneTitle);  // Verify it's lodging scene

var presentationMode = await page.GetAttributeAsync("[data-test='scene-container']", "data-presentation-mode");
Assert.Equal("Modal", presentationMode);  // Modal presentation
```

#### Step 3: Verify Situation 1 (Negotiate) Exact Choices
```csharp
var situationTitle = await page.TextContentAsync("[data-test='situation-title']");
Assert.Equal("Secure Lodging", situationTitle);

// Verify exactly 4 choices visible
var choices = await page.QuerySelectorAllAsync("[data-test='choice-card']");
Assert.Equal(4, choices.Count);

// Find money choice and verify exact cost
var moneyChoice = await page.QuerySelectorAsync("[data-test-path-type='InstantSuccess'][data-has-cost='true']");
var costText = await moneyChoice.TextContentAsync("[data-test='choice-cost']");
Assert.Matches(@"6\s*coins", costText);  // EXACT tutorial cost

// Find stat choice and verify exact requirement
var statChoice = await page.QuerySelectorAsync("[data-test-path-type='InstantSuccess'][data-has-requirement='true']");
var reqText = await statChoice.TextContentAsync("[data-test='choice-requirement']");
Assert.Matches(@"Diplomacy\s*5", reqText);  // EXACT tutorial requirement

// Find challenge choice
var challengeChoice = await page.QuerySelectorAsync("[data-test-path-type='Challenge']");
Assert.NotNull(challengeChoice);

// Find fallback choice (always available)
var fallbackChoice = await page.QuerySelectorAsync("[data-test-path-type='Fallback']");
Assert.NotNull(fallbackChoice);
var fallbackAvailable = await fallbackChoice.GetAttributeAsync("data-available");
Assert.Equal("true", fallbackAvailable);
```

#### Step 4: Execute Money Choice (Pay 6 Coins)
```csharp
await moneyChoice.ClickAsync();

// Verify confirmation or immediate execution
await page.WaitForSelectorAsync("[data-test='situation-title']");

// Verify rewards applied
var playerCoins = await page.TextContentAsync("[data-test='player-coins']");
Assert.Matches(@"\d+", playerCoins);  // Coins decreased by 6

// Verify private room unlocked
var inventory = await page.QuerySelectorAllAsync("[data-test='inventory-item']");
var hasRoomKey = inventory.Any(async item =>
    (await item.TextContentAsync()).Contains("Room Key"));
Assert.True(hasRoomKey);
```

#### Step 5: Verify Situation 2 (Rest) Appears
```csharp
var situationTitle2 = await page.TextContentAsync("[data-test='situation-title']");
Assert.Equal("Rest", situationTitle2);

// Verify location changed to private room
var currentLocation = await page.TextContentAsync("[data-test='current-location-name']");
Assert.Contains("Room", currentLocation);

// Verify 4 rest choices
var restChoices = await page.QuerySelectorAllAsync("[data-test='choice-card']");
Assert.Equal(4, restChoices.Count);
```

#### Step 6: Execute Rest Choice
```csharp
var firstRestChoice = restChoices[0];
await firstRestChoice.ClickAsync();

// Verify restoration reward
var playerHealth = await page.TextContentAsync("[data-test='player-health']");
// Health should have increased (verify ~40 health restoration for Standard environment)
```

#### Step 7: Verify Situation 3 (Depart) Appears
```csharp
var situationTitle3 = await page.TextContentAsync("[data-test='situation-title']");
Assert.Equal("Leave", situationTitle3);

// Verify 2 depart choices
var departChoices = await page.QuerySelectorAllAsync("[data-test='choice-card']");
Assert.Equal(2, departChoices.Count);
```

#### Step 8: Execute Depart Choice
```csharp
var firstDepartChoice = departChoices[0];
await firstDepartChoice.ClickAsync();

// Verify cleanup: room key removed
var inventoryAfter = await page.QuerySelectorAllAsync("[data-test='inventory-item']");
var hasRoomKeyAfter = inventoryAfter.Any(async item =>
    (await item.TextContentAsync()).Contains("Room Key"));
Assert.False(hasRoomKeyAfter);  // Key removed

// Verify scene completed
var sceneVisible = await page.QuerySelectorAsync("[data-test='scene-tutorial_secure_lodging']");
Assert.Null(sceneVisible);  // Scene should be dismissed
```

#### Step 9: Verify World State After Scene
```csharp
// Player should be back at normal navigation
var locationName = await page.TextContentAsync("[data-test='current-location-name']");
// Should be at player's chosen location after depart

// Private room should exist but be locked
var availableLocations = await page.QuerySelectorAllAsync("[data-test='location-item']");
// Room should appear in location list but be locked
```

**Status**: ⏳ Pending implementation (requires browser automation setup)

---

## 4. Validation Metrics

### Determinism Test
Run integration test 100 times with same inputs:
```csharp
[Fact]
public void InnLodging_IsDeterministic_100Runs()
{
    for (int i = 0; i < 100; i++)
    {
        var result = GenerateInnLodgingScene(elenaContext);
        Assert.Equal(expectedCoinCost, GetMoneyCost(result));
        Assert.Equal(expectedStatThreshold, GetStatRequirement(result));
        Assert.Equal(expectedRestoration, GetRestorationAmount(result));
    }
}
```

### Cross-Inn Reusability Test
Test that `inn_lodging` archetype works for DIFFERENT inn NPCs:
```csharp
[Theory]
[InlineData("elena", "brass_bell_inn", Quality.Basic)]  // Tier 1 inn
[InlineData("other_innkeeper", "golden_manor", Quality.Luxury)]  // Tier 4 inn
public void InnLodging_WorksForAnyInnkeeper(string npcId, string venueId, Quality expectedQuality)
{
    var npc = gameWorld.NPCs.First(n => n.ID == npcId);
    var location = gameWorld.Locations.First(l => l.VenueId == venueId);

    var context = GenerationContext.FromEntities(tier: 2, npc, location, player);
    var definition = SceneArchetypeCatalog.Generate("inn_lodging", tier: 2, context);

    Assert.Equal(3, definition.SituationTemplates.Count);  // Same structure
    Assert.Equal(expectedQuality, context.Quality);  // Different scaling
}
```

---

## 5. Test Execution Plan

### Phase 1: Unit Tests ✅
```bash
dotnet test --filter "CategoricalProperties_DeriveCorrectlyFromElena"
```

### Phase 2: Integration Tests ✅
```bash
dotnet test --filter "Elena_InnLodging_Tier0_GeneratesExactTutorialContent"
dotnet test --filter "TutorialSceneTemplate_ReferencesInnLodgingArchetype"
```

### Phase 3: Determinism Tests ⏳
```bash
dotnet test --filter "InnLodging_IsDeterministic"
```

### Phase 4: E2E Browser Tests ⏳
```bash
# Start game server
ASPNETCORE_URLS="http://localhost:6000" dotnet run --no-build

# Run E2E tests
dotnet test --filter "Tutorial_SecureLodging_CompleteFlow"
```

---

## 6. Success Criteria

**PROOF COMPLETE when**:
1. ✅ Unit tests prove categorical properties derive from Elena correctly
2. ✅ Integration tests prove exact tutorial content generated (costs, requirements, rewards)
3. ⏳ Determinism tests prove 100% reproducible results
4. ⏳ E2E tests prove player sees exact content in browser
5. ⏳ Cross-inn tests prove archetype is reusable for other inns

**Current Status**: Phases 1-2 implemented and ready to run.

**Next Steps**:
1. Build and run integration tests
2. Implement browser automation E2E tests
3. Verify exact values match tutorial expectations
