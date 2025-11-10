# Tutorial Integration Validation Plan

## Test Philosophy

**What we TEST:**
- Scene structure (3 situations with correct order)
- Choice structure (4 PathTypes present)
- Resource lifecycle (created → used → cleaned up)
- Player flow (can complete scene end-to-end)
- Categorical property application (derivation works, scaling applies)

**What we DON'T test:**
- Exact numerical values (balance will change)
- Specific coin costs or stat requirements
- Exact restoration amounts

---

## Integration Tests (C#)

### Test 1: `InnLodging_GeneratesCorrectStructure`

**Tests**:
- Scene has 3 situations (negotiate → rest → depart)
- Linear spawn pattern with correct transitions
- 1 dependent location + 1 dependent item
- Situation 1 has 4 choices (money/stat/challenge/fallback PathTypes)
- Success paths grant room access
- Situation 2 uses generated private room
- Situation 3 cleans up resources (removes key, locks room)

**Does NOT test**: Exact coin cost, exact stat threshold

### Test 2: `InnLodging_CategoricalPropertiesApplied`

**Tests**:
- Categorical properties are derived from entities (Quality, Environment, etc.)
- Properties are not default/null values
- PersonalityType preserved correctly

**Does NOT test**: Specific derived values (Elena = Basic vs Premium)

### Test 3: `Tutorial_UsesGeneralArchetype`

**Tests**:
- tutorial_secure_lodging scene exists
- Scene has 3 situations from archetype
- Linear spawn pattern
- Modal presentation mode
- IsStarter flag set

**Does NOT test**: Specific placement details

### Test 4: `InnLodging_CompletePlayerFlow`

**Tests**:
- Full end-to-end integration with real facades
- Scene creation → finalization → situations spawned
- Dependent resources created (1 location, 1 item)
- Marker resolution works (generated:* → actual IDs)
- 3 situations created with resolved location IDs
- Rest and depart require same resolved private room

**Does NOT test**: Actual choice execution or reward application

---

## E2E Browser Tests (Playwright/Selenium)

### Test: `Tutorial_InnLodging_PlayerExperience`

**Setup**:
- Fresh game save
- Navigate to common_room location
- Elena is present

**Flow**:

#### 1. Scene Appears
```typescript
await page.goto('/');
await page.waitForSelector('[data-test="scene-container"]');
const sceneTitle = await page.textContent('[data-test="scene-title"]');
// Assert scene is visible (don't care about exact title)
```

#### 2. Situation 1: Negotiate
```typescript
// Verify 4 choices visible
const choices = await page.$$('[data-test="choice-card"]');
assert.equal(choices.length, 4);

// Verify PathTypes present (not exact order)
assert.isTrue(await hasChoiceWithPathType('InstantSuccess'));
assert.isTrue(await hasChoiceWithPathType('Challenge'));
assert.isTrue(await hasChoiceWithPathType('Fallback'));

// Verify fallback is always available
const fallbackAvailable = await isChoiceAvailable(ChoicePathType.Fallback);
assert.isTrue(fallbackAvailable);
```

#### 3. Execute Success Choice
```typescript
const successChoice = await findFirstAvailableSuccessChoice();
await successChoice.click();

// Wait for next situation (don't verify exact situation name)
await page.waitForSelector('[data-test="situation-container"]');
```

#### 4. Situation 2: Rest
```typescript
// Verify we're in a different situation (situation changed)
const situationChanged = await hasSituationChanged();
assert.isTrue(situationChanged);

// Verify choices available
const restChoices = await page.$$('[data-test="choice-card"]');
assert.isTrue(restChoices.length > 0);

// Execute rest choice
await restChoices[0].click();
```

#### 5. Situation 3: Depart
```typescript
// Verify progression happened
await page.waitForSelector('[data-test="situation-container"]');

// Verify depart choices available
const departChoices = await page.$$('[data-test="choice-card"]');
assert.isTrue(departChoices.length > 0);

// Execute depart choice
await departChoices[0].click();
```

#### 6. Scene Completes
```typescript
// Scene should be dismissed or completed
await page.waitForFunction(() => {
    return !document.querySelector('[data-test="scene-container"]');
});

// Player should be back at navigation
const backAtLocation = await page.isVisible('[data-test="location-name"]');
assert.isTrue(backAtLocation);
```

**Tests**:
- Scene appears when expected
- Situations progress in order
- Choices are available and clickable
- Player can complete full flow
- Scene dismisses on completion

**Does NOT test**:
- Exact choice text or costs
- Exact situation names
- Specific UI layout details

---

## Running Tests

### Integration Tests
```bash
cd Wayfarer.Tests.Project
dotnet test --filter "FullyQualifiedName~TutorialInnLodgingIntegrationTest"
```

### E2E Tests (when implemented)
```bash
# Start game server
cd src
ASPNETCORE_URLS="http://localhost:6000" dotnet run --no-build &

# Run E2E tests
cd ../e2e-tests
npm test
```

---

## Success Criteria

✅ **Integration tests pass** → Scene structure correct, resources created/cleaned up
✅ **E2E flow completes** → Player can complete scene start to finish in browser

❌ **NOT required** → Exact balance values match specific numbers
