# Literary UI Implementation Documentation

## Overview

This document describes the complete transformation of Wayfarer's UI from a traditional RPG interface with stats and numbers to an immersive literary experience where all mechanics are conveyed through narrative descriptions.

## Architecture Overview

### 1. SceneContext System (Formerly ConversationContext)

The `SceneContext` class is the central data structure that bridges game mechanics with narrative presentation.

**Location**: `/src/Game/ConversationSystem/SceneContext.cs`

**Key Components**:
- **AttentionManager**: Manages 3 attention points per scene
- **Context Tags**: Pressure, Relationship, Discovery, Resource, Feeling
- **Scene Metrics**: Minutes until deadline, letter queue size
- **NPC State**: Current target, relationship tokens

### 2. Attention System

**Location**: `/src/GameState/AttentionManager.cs`

**Mechanics**:
- Players have 3 attention points per scene
- Choices cost 0-3 attention based on depth
- 0 Cost: Basic responses, leaving conversation
- 1 Cost: Information gathering, observations
- 2 Cost: Binding promises, major actions
- 3 Cost: Exhaustive investigations

**Narrative Descriptions**:
```csharp
3 points: "Your mind is clear and focused, ready to absorb every detail."
2 points: "You remain attentive, though some of your focus has been spent."
1 point: "Your concentration wavers. You must choose your focus carefully."
0 points: "Mental fatigue clouds your thoughts. You can only respond simply."
```

### 3. Context Tags System

**Location**: `/src/GameState/SceneTags.cs`

#### Pressure Tags
- `DEADLINE_IMMINENT`: Less than 3 hours to deadline
- `QUEUE_OVERFLOW`: 6+ letters in queue
- `DEBT_PRESENT`: Any negative tokens
- `DEBT_CRITICAL`: -3 or worse in any token type
- `OBLIGATION_ACTIVE`: Standing obligation affecting choices
- `PATRON_WATCHING`: Patron has expectations

#### Relationship Tags
- `TRUST_HIGH`: 4+ trust tokens
- `COMMERCE_ESTABLISHED`: 2+ commerce tokens
- `STATUS_RECOGNIZED`: 3+ status tokens
- `SHADOW_COMPLICIT`: Any shadow tokens
- Plus negative versions for debt states

#### Discovery Tags
- `RUMOR_AVAILABLE`: NPC has rumors to share
- `ROUTE_UNKNOWN`: Undiscovered route nearby
- `NPC_HIDDEN`: Hidden NPC could be revealed
- `INFORMATION_HINTED`: Information can be gleaned
- `SECRET_PRESENT`: Secret knowledge available

#### Resource Tags
- Coins: `ABUNDANT` (20+), `SUFFICIENT` (5-19), `LOW` (1-4), `NONE` (0)
- Stamina: `FULL`, `RESTED` (70%+), `TIRED` (30-69%), `EXHAUSTED` (<30%)
- Inventory: `FULL`, `EMPTY`

#### Feeling Tags
- Temperature: `HEARTH_WARMED`, `SUN_DRENCHED`, `RAIN_SOAKED`, `FROST_TOUCHED`
- Social: `BUSTLING`, `INTIMATE`, `TENSE`, `CELEBRATORY`, `HOSTILE`
- Sensory: `ALE_SCENTED`, `SMOKE_FILLED`, `MUSIC_DRIFTING`, `SILENCE_HEAVY`
- Emotional: `URGENCY_GNAWS`, `COMFORT_EMBRACES`, `MYSTERY_WHISPERS`, `DANGER_LURKS`

### 4. Context Tag Calculator

**Location**: `/src/GameState/ContextTagCalculator.cs`

Analyzes GameWorld state and populates SceneContext with appropriate tags:
- Calculates pressure from deadlines and queue size
- Determines relationship status from token counts
- Identifies discovery opportunities
- Assesses resource availability
- Generates feeling tags based on location, weather, and time

### 5. Rumor System

**Location**: `/src/GameState/Rumor.cs`, `/src/GameState/RumorManager.cs`

**Confidence Levels**:
- `???` Unknown - No idea if true
- `?` Doubtful - Probably false
- `◐` Possible - Might be true
- `◕` Likely - Probably true
- `✓` Verified - Confirmed true
- `✗` False - Confirmed false

**Categories**:
- Trade, Social, Political, Location, Opportunity, Danger, General

**Mechanics**:
- Rumors discovered through conversation and observation
- Can be traded for value
- Verification through gameplay reveals truth
- Expire after certain days if relevant

## UI Components Architecture

### Phase 2: Literary Conversation Screen

**Components to Create**:

#### LiteraryConversationScreen.razor + .razor.cs
- Main conversation interface
- Replaces ConversationView entirely
- No @code blocks (follows UI standards)

#### AttentionDisplay.razor + .razor.cs
- Shows 3 golden circles for attention points
- Animates spending/recovery
- Positioned at top center

#### PeripheralAwareness.razor + .razor.cs
- Deadline pressure (top-right): "⚡ Lord B: 2h 15m"
- Environmental hints (bottom-right): "Guards shifting nervously..."
- Binding obligations (top-left): Active promises

#### InternalThoughtChoice.razor + .razor.cs
- Choices displayed as italicized thoughts
- Attention cost badges (◆ 1, ◆◆ 2, ◆◆◆ 3)
- Mechanical effects shown narratively
- Example: *"I'll prioritize your letter. Let me check what that means..."*

#### BodyLanguageDisplay.razor + .razor.cs
- Replaces token count displays
- "Fingers worrying her shawl" instead of "Trust: 5"
- "Leaning forward eagerly" instead of "Commerce: 3"
- Dynamic based on RelationshipTags

### Phase 3: Literary Location Screens

#### LiteraryLocationScreen.razor + .razor.cs
- Atmospheric description focus
- Feeling tags displayed subtly
- NPCs described through behavior

#### LocationFeelingTags.razor + .razor.cs
- Shows atmosphere: "🔥 Hearth-warmed, 🍺 Ale-scented"
- Changes with time and weather
- Positioned below location name

#### ObservationsList.razor + .razor.cs
- "You notice..." section
- Each observation costs attention to investigate
- Unknown items marked with ❓

#### LiteraryTravelScreen.razor + .razor.cs
- Journey progress as narrative
- Random encounters described literarily
- Progress bar replaced with description

### Phase 4: Physical Queue System

#### PhysicalSatchelScreen.razor + .razor.cs
- Letters as physical objects with weight
- Descriptions of seals, paper quality
- Urgency through physical cues

#### LetterPhysicalDescription.razor + .razor.cs
- "Heavy parchment sealed with red wax"
- "Hastily scrawled note, ink still damp"
- "Official document bearing noble crest"

#### QueueReorderView.razor + .razor.cs
- Drag to reorder with narrative feedback
- "You shuffle Elena's letter to the top of your satchel"
- Token burning described as betrayal

### Phase 5: CSS & Integration

#### literary-ui.css
Location: `/src/wwwroot/css/literary-ui.css`

```css
:root {
    --attention-gold: #ffd700;
    --pressure-red: #8b0000;
    --comfort-warm: #d2691e;
    --mystery-purple: #4b0082;
    --shadow-dark: #2c2416;
    --parchment: #fefdfb;
}

.attention-point {
    width: 24px;
    height: 24px;
    border-radius: 50%;
    background: var(--attention-gold);
    transition: all 0.3s;
}

.attention-point.spent {
    background: var(--shadow-dark);
    opacity: 0.5;
}

.choice-thought {
    font-style: italic;
    background: var(--parchment);
    padding: 12px;
    border-left: 3px solid var(--attention-gold);
}

.peripheral-pressure {
    position: fixed;
    top: 50px;
    right: 10px;
    background: var(--pressure-red);
    color: white;
    padding: 6px 10px;
    font-size: 11px;
    border-radius: 4px;
    opacity: 0.9;
}
```

## Integration with Existing Systems

### GameFacade Updates

The GameFacade needs minimal changes since it already returns ViewModels. The UI components will interpret these ViewModels through a literary lens.

### ConversationManager Integration

- ProcessPlayerChoice now spends attention through SceneContext
- Choices generated with AttentionCost property
- Affordability checked against AttentionManager

### MainGameplayView Updates

Replace screen components:
- `ConversationView` → `LiteraryConversationScreen`
- `LocationScreen` → `LiteraryLocationScreen`
- `TravelSelection` → `LiteraryTravelScreen`
- `LetterQueueScreen` → `PhysicalSatchelScreen`

Remove all numeric displays:
- Health/stamina bars
- Token counts
- Coin displays (except in markets)
- Time as numbers (use descriptive time)

## Testing Strategy

### Manual Testing Checklist

1. **Attention System**
   - [ ] 3 points reset on new conversation
   - [ ] Choices properly cost attention
   - [ ] Unaffordable choices are disabled
   - [ ] Narrative descriptions update with spent attention

2. **Context Tags**
   - [ ] Pressure tags appear with deadlines
   - [ ] Relationship tags reflect token counts
   - [ ] Feeling tags match location/time
   - [ ] Discovery tags highlight opportunities

3. **Rumor System**
   - [ ] Rumors discovered through conversation
   - [ ] Confidence symbols display correctly
   - [ ] Trading rumors removes from tradeable list
   - [ ] Expired rumors disappear

4. **UI Components**
   - [ ] No numeric displays visible
   - [ ] All mechanics described narratively
   - [ ] Peripheral awareness doesn't distract
   - [ ] Body language replaces token displays

## GitHub Issues Mapping

- **#27**: Attention as limited resource → AttentionManager
- **#28**: Partial information → Rumor system
- **#29**: Physical queue → PhysicalSatchelScreen
- **#30**: Rumor discovery → RumorManager
- **#31**: Binding obligations → High attention cost choices
- **#32**: Peripheral awareness → PeripheralAwareness component
- **#33**: Feeling tags → FeelingTag enum and calculator
- **#34**: Body language → BodyLanguageDisplay component
- **#35**: Internal thoughts → InternalThoughtChoice component
- **#36**: Narrative costs → All costs described narratively

## Key Design Decisions

### Why Rename ConversationContext to SceneContext?
The context now encompasses more than just conversations - it includes location atmosphere, pressure states, and discovery opportunities. SceneContext better reflects its expanded role.

### Why 3 Attention Points?
- Matches common narrative structure (beginning, middle, end)
- Enough for meaningful choices without overwhelming
- Forces prioritization without being too restrictive

### Why Context Tags Instead of Direct Values?
- Tags create narrative categories rather than numeric thresholds
- Easier to generate appropriate narrative from tags
- Tags can combine for emergent narrative situations

## Future Enhancements

1. **Dynamic Attention Recovery**
   - Rest actions restore 1 attention
   - Certain items or locations refresh focus
   - Time passage gradually recovers attention

2. **Contextual Attention Costs**
   - Same choice costs different amounts based on context
   - Relationship level affects conversation costs
   - Pressure situations increase costs

3. **Rumor Networks**
   - Rumors spread between NPCs
   - Player can influence rumor propagation
   - False rumors can be planted strategically

4. **Feeling Tag Combinations**
   - Certain tag combinations create unique atmospheres
   - Tags influence NPC behavior
   - Player actions can change location feelings

## Reference UI Mockups

See `/UI-MOCKUPS/` directory:
- `conversation-elena.html` - Target conversation interface
- `location-screens.html` - Location screen examples

These mockups demonstrate the literary approach with attention points, feeling tags, and narrative descriptions replacing all numeric displays.