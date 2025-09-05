# Rapport System Refactoring Plan

## Overview
This document details the refactoring of Wayfarer's conversation system to implement the new rapport mechanics design. The key change is separating flow (which tracks success/failure balance) from rapport (which modifies success chances).

## Core Design Changes

### 1. Flow System Simplification
- **Current**: Flow is modified by card effects and determines state transitions
- **New**: Flow only tracks success/failure (+1 for success, -1 for failure)
- **Purpose**: Flow remains the connection state battery (-3 to +3) triggering transitions

### 2. New Rapport System
- **Purpose**: Separate resource that modifies all card success rates
- **Formula**: Each rapport point = +1% success chance on all cards
- **Starting Value**: Sum of all connection tokens × 3 (e.g., 1 Trust token = 3 starting rapport)
- **Range**: Can be negative (reducing success) or positive (increasing success)

### 3. Token System Changes
- **Current**: Tokens provide +5% success per token on matching card types
- **New**: Tokens only provide starting rapport (no per-card bonuses)
- **Example**: Elena with 1 Trust token starts conversation with 3 rapport (+3% to all cards)

### 4. Terminology Updates
- **Focus** → **Focus** (the resource pool for playing cards)
- **Flow Effects** → **Rapport Effects** (card effects that modify rapport)

## Implementation Architecture

### New Components

#### RapportManager (`/src/Subsystems/Conversation/RapportManager.cs`)
```csharp
public class RapportManager
{
    private int currentRapport = 0;
    private const int TOKEN_TO_RAPPORT_MULTIPLIER = 3;
    
    // Initialize with starting tokens
    public RapportManager(Dictionary<ConnectionType, int> tokens)
    {
        currentRapport = tokens.Values.Sum() * TOKEN_TO_RAPPORT_MULTIPLIER;
    }
    
    public int CurrentRapport => currentRapport;
    
    // Apply rapport changes from cards
    public void ApplyRapportChange(int change, AtmosphereType atmosphere = AtmosphereType.Neutral)
    {
        // Apply atmosphere modifiers if needed
        int modified = ModifyByAtmosphere(change, atmosphere);
        currentRapport += modified;
    }
    
    // Scaling methods for dynamic effects
    public int ScaleRapportByTokens(ConnectionType type, int tokenCount)
    {
        return tokenCount; // Direct scaling
    }
    
    public int ScaleRapportByFlow(int currentFlow)
    {
        return 4 - currentFlow; // Example: Better rapport when flow is negative
    }
    
    public int ScaleRapportByPatience(int patience)
    {
        return patience / 3; // Example scaling
    }
    
    public int ScaleRapportByFocus(int remainingFocus)
    {
        return remainingFocus;
    }
    
    // Get current success modifier
    public int GetSuccessModifier()
    {
        return currentRapport; // Each point = 1% success modifier
    }
    
    // Reset rapport to starting value
    public void ResetRapport(Dictionary<ConnectionType, int> tokens)
    {
        currentRapport = tokens.Values.Sum() * TOKEN_TO_RAPPORT_MULTIPLIER;
    }
}
```

### Modified Components

#### CardEffectType Enum Changes
**File**: `/src/GameState/CardEffectType.cs`

Rename existing enums:
- `AddFlow` → `AddRapport`
- `ScaleByTokens` → `ScaleRapportByTokens`
- `ScaleByFlow` → `ScaleRapportByFlow`
- `ScaleByPatience` → `ScaleRapportByPatience`
- `ScaleByFocus` → `ScaleRapportByFocus`
- `FlowReset` → `RapportReset`

Keep unchanged:
- `DrawCards`
- `AddFocus` → `AddFocus`
- `FocusRefresh` → `FocusRefresh`
- `SetAtmosphere`
- `EndConversation`
- `FreeNextAction`
- `Exchange`

#### FlowManager Simplification
**File**: `/src/Subsystems/Conversation/FlowManager.cs`

Modify `ApplyFlowChange` to remove card effect handling:
```csharp
public (bool stateChanged, ConnectionState newState, bool conversationEnds) 
    ApplyCardResult(bool success)
{
    // Simple: +1 for success, -1 for failure
    int change = success ? 1 : -1;
    return ApplyFlowChange(change);
}
```

#### ConversationSession Updates
**File**: `/src/GameState/ConversationSession.cs`

Add RapportManager:
```csharp
public class ConversationSession
{
    // Existing
    public FlowManager FlowManager { get; set; }
    
    // New
    public RapportManager RapportManager { get; set; }
    
    // Rename
    public int CurrentFocus { get; set; } // was CurrentFocus
    public int MaxFocus { get; set; } // was MaxFocus
}
```

#### CardEffectProcessor Updates
**File**: `/src/Content/CardEffectProcessor.cs`

Update to process rapport effects:
```csharp
private void ProcessCardEffect(CardEffect effect, ConversationSession session)
{
    switch (effect.Type)
    {
        case CardEffectType.AddRapport:
            int rapportChange = int.Parse(effect.Value);
            session.RapportManager.ApplyRapportChange(rapportChange, session.CurrentAtmosphere);
            break;
            
        case CardEffectType.ScaleRapportByTokens:
            // Implementation for token scaling
            break;
            
        // etc. for other rapport effects
    }
}
```

#### Success Rate Calculation
**File**: `/src/Content/CardEffectProcessor.cs`

Update success calculation:
```csharp
public int CalculateSuccessRate(CardInstance card, ConversationSession session)
{
    int baseSuccess = card.Template.GetBaseSuccessPercentage();
    
    // Rapport modifier instead of token modifier
    int rapportBonus = session.RapportManager.GetSuccessModifier();
    
    // Atmosphere bonus remains
    int atmosphereBonus = GetAtmosphereBonus(session.CurrentAtmosphere);
    
    return Math.Clamp(baseSuccess + rapportBonus + atmosphereBonus, 0, 100);
}
```

## Content Updates

### JSON Card Definitions
**File**: `/src/Content/core_game_package.json`

Update card effects from flow to rapport:
```json
{
  "id": "trust_understanding",
  "successEffect": {
    "type": "AddRapport",  // was AddFlow
    "value": "1"
  }
}
```

### Parser Updates
**Files**: 
- `/src/Content/ConversationCardParser.cs`
- `/src/Content/ObservationParser.cs`

Update parsing to recognize rapport effect types.

## UI Updates

### ConversationContent Razor
**File**: `/src/Pages/Components/ConversationContent.razor`

Display rapport bar:
```html
<div class="rapport-strip">
    <div class="rapport-display">
        <span class="rapport-label">Rapport:</span>
        <span class="rapport-value">@CurrentRapport</span>
        <span class="rapport-effect">(+@CurrentRapport% to all cards)</span>
    </div>
</div>
```

### Terminology in UI
Replace all instances:
- "Focus" → "Focus"
- "focus" → "focus"
- Token bonus displays → Rapport displays

## Testing Strategy

### Unit Tests
1. Test RapportManager initialization from tokens
2. Test rapport effect processing
3. Test flow only changes on success/failure
4. Test success rate calculations with rapport

### Integration Tests
1. Full conversation flow with rapport changes
2. Token to rapport conversion
3. State transitions still work with simplified flow
4. UI displays correct values

## Migration Notes

### Breaking Changes
- Card effects that modified flow now modify rapport
- Token bonuses no longer apply per-card
- Success rate calculations changed

### Backwards Compatibility
- Existing save games will need migration
- Content files need updating but structure remains same
- UI components need updating for new displays

## Implementation Order

1. **Create RapportManager** - New class with full functionality
2. **Rename CardEffectType enums** - Simple rename operation
3. **Update ConversationSession** - Add RapportManager reference
4. **Simplify FlowManager** - Remove effect processing, add success/failure method
5. **Update CardEffectProcessor** - Redirect to rapport effects
6. **Update parsers** - Recognize new effect names
7. **Update ConversationOrchestrator** - Initialize both managers
8. **Rename Focus to Focus** - Global find/replace with care
9. **Update UI components** - Display rapport instead of token bonuses
10. **Update content JSON** - Change effect types in cards
11. **Test thoroughly** - Ensure all systems work together

## Rollback Plan
If issues arise:
1. Git revert to previous commit
2. Restore original CardEffectType enums
3. Remove RapportManager
4. Restore FlowManager original functionality
5. Revert UI changes

## Success Criteria
- [ ] Conversations work with new rapport system
- [ ] Flow only tracks success/failure
- [ ] Tokens provide starting rapport
- [ ] UI shows rapport instead of token bonuses
- [ ] All tests pass
- [ ] No regression in existing features