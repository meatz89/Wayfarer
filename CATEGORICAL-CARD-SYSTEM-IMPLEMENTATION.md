# Categorical Card System Implementation Plan

## Overview
Complete replacement of the legacy property-based card system with categorical properties that define behavior through context rather than hardcoded values.

## 1. Data Structure Changes

### Remove Legacy:
- `List<CardProperty> Properties` - DELETED
- `CardEffect SuccessEffect` - DELETED
- `CardEffect FailureEffect` - DELETED
- `CardEffect ExhaustEffect` - DELETED
- All helper properties checking Properties list - DELETED

### Add Categorical:
```csharp
public PersistenceType Persistence { get; set; }
public SuccessEffectType SuccessType { get; set; }
public FailureEffectType FailureType { get; set; }
public ExhaustEffectType ExhaustType { get; set; }
public AtmosphereType? TargetAtmosphere { get; set; } // For Atmospheric success
```

## 2. Enum Definitions

### PersistenceType
- **Thought** - Remains in hand through LISTEN
- **Impulse** - Removed after any SPEAK if unplayed
- **Opening** - Removed after LISTEN if unplayed

### SuccessEffectType
- **None** - No effect
- **Rapport** - Connection change (magnitude from difficulty)
- **Threading** - Draw cards (magnitude from difficulty)
- **Atmospheric** - Set specific atmosphere
- **Focusing** - Restore focus (magnitude from difficulty)
- **Promising** - Move obligation to position 1
- **Advancing** - Advance connection state by 1 (ignores magnitude)

### FailureEffectType
- **None** - Just forces LISTEN
- **Overreach** - Clear entire hand
- **Backfire** - Negative rapport (magnitude from difficulty)
- **Disrupting** - Discard cards with focus 3+

### ExhaustEffectType
- **None** - No effect
- **Threading** - Draw cards when exhausted
- **Focusing** - Restore focus when exhausted
- **Regret** - Lose rapport when not played

## 3. Magnitude System

Difficulty determines magnitude:
- **Very Easy**: 1
- **Easy**: 1
- **Medium**: 2
- **Hard**: 3
- **Very Hard**: 4

Atmosphere modifiers:
- **Volatile**: Rapport effects ±1
- **Focused**: All success magnitudes +1
- **Exposed**: All magnitudes doubled
- **Synchronized**: Effect happens twice

## 4. Files to Update

### Phase 1: Core Classes
1. ✅ `/src/GameState/Enums/CardCategoricalProperties.cs` - Create enums
2. ✅ `/src/GameState/ConversationCard.cs` - Replace properties with categorical
3. `/src/GameState/CardEffect.cs` - DELETE ENTIRELY
4. `/src/GameState/CardProperty.cs` - DELETE ENTIRELY

### Phase 2: DTOs and Parsing
5. ✅ `/src/Content/ConversationCardParser.cs` - Update DTO and parsing
6. Remove ALL Properties parsing
7. Parse categorical fields directly
8. Remove effect value parsing

### Phase 3: JSON Updates
9. `/src/Content/Core/core_game_package.json` - Convert ALL cards
10. Remove "properties" array
11. Remove "successEffect", "failureEffect", "exhaustEffect" objects
12. Add "persistence", "successType", "failureType", "exhaustType" strings

### Phase 4: Effect Resolution
13. Create `/src/Subsystems/Conversation/CategoricalEffectResolver.cs`
14. Calculate magnitude from difficulty
15. Apply atmosphere modifiers
16. Resolve effects based on type + magnitude

### Phase 5: Mechanics Updates
17. `/src/Subsystems/Conversation/CardDeckManager.cs`
    - Replace IsPersistent check with Persistence == PersistenceType.Thought
    - Handle Impulse removal after SPEAK
    - Handle Opening removal after LISTEN

18. `/src/Subsystems/Conversation/ConversationOrchestrator.cs`
    - Use CategoricalEffectResolver
    - Handle Overreach (clear hand on failure)
    - Handle Disrupting (remove high-focus cards)
    - Process exhaust effects (Threading/Focusing/Regret)

### Phase 6: UI Updates
19. `/src/Pages/Components/ConversationContent.razor`
    - Display persistence type badge
    - Show success/failure/exhaust type badges
    - Remove old property display code

20. `/src/Pages/Components/ConversationContent.razor.cs`
    - Update card filtering for new persistence types
    - Handle exhaust effects in UI

## 5. Example Card Transformation

### Before (Legacy):
```json
{
  "id": "i_hear_you",
  "properties": ["Persistent"],
  "successEffect": {
    "type": "AddRapport",
    "value": "1"
  },
  "failureEffect": null,
  "exhaustEffect": null
}
```

### After (Categorical):
```json
{
  "id": "i_hear_you",
  "persistence": "Thought",
  "successType": "Rapport",
  "failureType": "None",
  "exhaustType": "None"
}
```

The +1 rapport comes from Easy difficulty = magnitude 1.

## 6. Search and Destroy Operations

### Find and eliminate ALL references to:
- `CardProperty` enum
- `Properties` list
- `IsPersistent`, `IsImpulse`, `IsOpening` using Properties
- `CardEffect` class
- `SuccessEffect`, `FailureEffect`, `ExhaustEffect` properties
- Any "backward compatibility" code

### Replace with:
- Direct categorical property checks
- CategoricalEffectResolver calls
- Magnitude-based calculations

## 7. Testing Requirements

1. Verify Thought cards survive LISTEN
2. Verify Impulse cards removed after any SPEAK
3. Verify Opening cards removed after LISTEN
4. Test all success effect types with proper magnitudes
5. Test failure effects (especially Overreach hand clearing)
6. Test exhaust effects trigger properly
7. Verify atmosphere modifiers apply correctly

## 8. No Compatibility Layers

- NO fallback code
- NO migration helpers
- NO backward compatibility properties
- Clean break - old saves will not work
- Delete ALL legacy code immediately

## Implementation Order

### Step 1: Delete Legacy Files
- Delete CardProperty.cs
- Delete CardEffect.cs

### Step 2: Core Infrastructure
- Rename CardEffectProcessor.cs to CategoricalEffectResolver.cs
- Implement magnitude calculation
- Implement categorical effect processing

### Step 3: Update Card Classes
- Update CardInstance.cs to use categorical properties
- Update SessionCardDeck.cs
- Remove all Properties list references

### Step 4: Update Mechanics
- Update CardDeckManager.cs persistence checks
- Update ConversationOrchestrator.cs effect processing
- Implement Overreach and Disrupting failures

### Step 5: Convert JSON
- Update ALL cards in core_game_package.json
- Remove legacy properties
- Add categorical properties

### Step 6: Update UI
- Update ConversationContent.razor badges
- Update card filtering logic

### Step 7: Final Cleanup
- Global search for legacy references
- Delete all found references
- Build and test

## Key Implementation Details

### Magnitude Calculation
```csharp
int GetMagnitude(Difficulty difficulty) => difficulty switch {
    Difficulty.VeryEasy => 1,
    Difficulty.Easy => 1,
    Difficulty.Medium => 2,
    Difficulty.Hard => 3,
    Difficulty.VeryHard => 4,
    _ => 1
};
```

### Effect Resolution Pattern
```csharp
switch (card.SuccessType) {
    case SuccessEffectType.Rapport:
        result.RapportChange = GetMagnitude(card.Difficulty);
        break;
    case SuccessEffectType.Threading:
        result.CardsToAdd = DrawCards(GetMagnitude(card.Difficulty));
        break;
    case SuccessEffectType.Atmospheric:
        result.AtmosphereChange = card.TargetAtmosphere;
        break;
    // etc...
}
```

### Persistence Checks
```csharp
// OLD: if (card.Properties.Contains(CardProperty.Impulse))
// NEW: if (card.Persistence == PersistenceType.Impulse)

// OLD: if (card.IsPersistent)
// NEW: if (card.Persistence == PersistenceType.Thought)
```

## Success Criteria

- Zero references to CardProperty enum remain
- Zero references to CardEffect class remain
- All cards use categorical properties
- Effects calculated from difficulty + atmosphere
- UI shows categorical badges
- Game compiles and runs
- Conversations function correctly