# Board Game Mechanics to Literary UI Mapping

## Overview
This document provides the EXACT mapping from mechanical board game states to literary UI displays. Each mechanical value generates specific narrative descriptions through deterministic algorithms.

## CONVERSATION MECHANICS → UI MAPPING

### 1. PRESSURE POOLS (0-15) → BODY LANGUAGE

The pressure system uses two pools (Player and NPC) ranging from 0-15, generating physical descriptions:

```csharp
// Mechanical State
PlayerPressure: 8
NPCPressure: 12

// UI Generation Algorithm
string GenerateBodyLanguage(int pressure) {
    if (pressure >= 13) return "rigid posture, white knuckles";
    if (pressure >= 10) return "shoulders drawn tight, jaw clenched";
    if (pressure >= 7)  return "leaning forward, fingers drumming";
    if (pressure >= 4)  return "shifting weight, occasional sighs";
    return "relaxed stance, open gestures";
}

// Literary UI Output
"Elena's shoulders drawn tight, jaw clenched"
"You find yourself leaning forward, fingers drumming"
```

### 2. ATTENTION POINTS (3) → VISUAL INDICATORS

```csharp
// Mechanical State
AttentionPoints: [SPENT, AVAILABLE, AVAILABLE]

// UI Display
<div class="attention-bar">
    <div class="attention-point spent"></div>    <!-- Dark gold circle -->
    <div class="attention-point"></div>          <!-- Bright gold circle -->
    <div class="attention-point"></div>          <!-- Bright gold circle -->
</div>

// Narrative Description
AttentionRemaining == 3: "Your mind is clear and focused"
AttentionRemaining == 2: "You remain attentive, though some focus spent"
AttentionRemaining == 1: "Your concentration wavers"
AttentionRemaining == 0: "Mental fatigue clouds your thoughts"
```

### 3. MOMENTUM TRACK (-5 to +5) → CONVERSATION FLOW

```csharp
// Mechanical State
Momentum: -3 (NPC controlling)

// UI Generation
string GenerateFlowDescription(int momentum) {
    if (momentum <= -4) return "They dominate the conversation completely";
    if (momentum <= -2) return "They steer the discussion firmly";
    if (momentum <= 0)  return "The conversation flows naturally";
    if (momentum <= 2)  return "You guide the topic gently";
    return "You control the exchange entirely";
}

// Choice Tone Modifiers
if (momentum < -2) {
    choiceTones.Add("Defensive");
    choicePrefixes.Add("Trying to regain ground:");
}
else if (momentum > 2) {
    choiceTones.Add("Assertive");
    choicePrefixes.Add("Pressing your advantage:");
}
```

### 4. ATTENTION ALLOCATION → CHOICE MECHANICS

```csharp
// Mechanical: Player allocates 3 points among Press/Guard/Observe
Press: 2, Guard: 0, Observe: 1

// UI Generation
if (Press == 2) {
    narrativeContext = "aggressive questioning stance";
    availableChoices.Add(new Choice {
        Text = "*I need answers about the missing shipment*",
        Cost = 0, // Aggressive choices cost less when pressing
        Effect = "Forces topic to shipment"
    });
}

if (Observe == 1) {
    revealedInfo.Add("You notice: ink stains on their fingers");
    peripheralHints.Add("A letter half-hidden beneath ledger");
}

if (Guard == 0) {
    vulnerability = "Exposed to counter-questions";
    npcCanProbe = true;
}
```

## LOCATION MECHANICS → UI MAPPING

### 5. ACTIVITY TOKENS (0-12) → ATMOSPHERIC DESCRIPTIONS

```csharp
// Mechanical State
ActivityTokens: 8
TimeOfDay: EVENING
LocationType: TAVERN

// Generation Algorithm
string GenerateAtmosphere(int tokens, TimeBlock time, LocationType type) {
    var density = tokens switch {
        >= 10 => "packed",
        >= 7 => "bustling",
        >= 4 => "lively",
        >= 2 => "quiet",
        _ => "empty"
    };
    
    var energy = tokens switch {
        >= 10 => "raucous laughter, shouted conversations",
        >= 7 => "animated chatter, clinking tankards",
        >= 4 => "gentle murmur, occasional laughter",
        >= 2 => "hushed voices, soft footsteps",
        _ => "oppressive silence"
    };
    
    return $"The {type} is {density}, {energy}";
}

// Literary Output
"The tavern is bustling, animated chatter mixing with clinking tankards"
```

### 6. OBSERVATION DICE (3) → DISCOVERY OPPORTUNITIES

```csharp
// Mechanical: Player allocates 3 dice to different areas
BarArea: 2 dice
DarkCorner: 1 die
MainFloor: 0 dice

// UI Generation
foreach (var allocation in observationDice) {
    var discoveries = RollForDiscoveries(allocation.DiceCount);
    
    if (allocation.Area == "BarArea" && allocation.DiceCount == 2) {
        discoveries.Add("Overheard: 'The morning caravan leaves early'");
        discoveries.Add("Noticed: Merchant's seal on unopened letter");
    }
    
    if (allocation.Area == "DarkCorner" && allocation.DiceCount == 1) {
        discoveries.Add("Glimpsed: Hooded figure counting coins");
    }
}

// UI Display
<div class="observation-results">
    <div class="observation high-attention">
        📍 Bar Area (focused observation)
        • Overheard: "The morning caravan leaves early"
        • Noticed: Merchant's seal on unopened letter
    </div>
    <div class="observation low-attention">
        📍 Dark Corner (glanced)
        • Glimpsed: Hooded figure counting coins
    </div>
</div>
```

### 7. FLUX CARDS → LOCATION STATE CHANGES

```csharp
// Mechanical: Draw flux card each hour
FluxCard: "GUARD_PATROL"
CurrentHour: 14

// State Change Algorithm
void ApplyFluxCard(FluxCard card, Location location) {
    switch (card.Type) {
        case "GUARD_PATROL":
            location.AddTemporaryTag("TENSE");
            location.ActivityTokens -= 2;
            narrative = "Guards enter, conversations drop to whispers";
            break;
        case "MERCHANT_ARRIVAL":
            location.ActivityTokens += 3;
            location.AddOpportunity("TRADE_RUMOR");
            narrative = "A merchant caravan arrives, bringing news and noise";
            break;
    }
}

// UI Update
<div class="location-event fade-in">
    ⚠️ Guards enter, conversations drop to whispers
    <div class="event-effects">
        • Atmosphere becomes tense
        • Some patrons leave quietly
    </div>
</div>
```

## TRAVEL MECHANICS → UI MAPPING

### 8. ROUTE SEGMENTS → JOURNEY NARRATIVE

```csharp
// Mechanical State
RouteSegments: [FOREST, HILL, BRIDGE]
CurrentSegment: 1 (HILL)
TerrainCost: 3
StaminaRemaining: 7

// Narrative Generation
string GenerateSegmentNarrative(Segment segment, int stamina) {
    var effort = (segment.Cost / (float)stamina) switch {
        > 0.5f => "Each step grows heavier",
        > 0.3f => "The path demands effort",
        > 0.1f => "You maintain steady pace",
        _ => "The journey feels effortless"
    };
    
    var terrain = segment.Type switch {
        FOREST => "through shadowed woods, roots catching at your feet",
        HILL => "up the winding slope, breath coming harder",
        BRIDGE => "across weathered planks, water rushing below",
        _ => "along the dusty road"
    };
    
    return $"{effort} {terrain}";
}

// UI Display
"Each step grows heavier up the winding slope, breath coming harder"
```

### 9. STAMINA POOL (10) → PHYSICAL STATE

```csharp
// Mechanical State
CurrentStamina: 4
MaxStamina: 10
EncumbranceLevel: HEAVY

// Physical Description Algorithm
string GeneratePhysicalState(int stamina, int max, Encumbrance enc) {
    var fatigue = ((max - stamina) / (float)max) switch {
        >= 0.8f => "Exhaustion weighs on every movement",
        >= 0.6f => "Weariness seeps into your bones",
        >= 0.4f => "Your pace has slowed noticeably",
        >= 0.2f => "A light fatigue touches your steps",
        _ => "You feel fresh and ready"
    };
    
    var burden = enc switch {
        HEAVY => ", the satchel cutting into your shoulder",
        MEDIUM => ", your pack a constant reminder",
        LIGHT => ", traveling light and easy",
        _ => ""
    };
    
    return fatigue + burden;
}

// UI Output
"Weariness seeps into your bones, the satchel cutting into your shoulder"
```

### 10. WEATHER DICE → ENVIRONMENTAL NARRATIVE

```csharp
// Mechanical: Roll weather dice per segment
WeatherRoll: 4 (Rain)
WindRoll: 2 (Light)
VisibilityRoll: 1 (Poor)

// Environmental Generation
string GenerateWeatherNarrative(int weather, int wind, int visibility) {
    var precipitation = weather switch {
        >= 5 => "Heavy rain soaks through your cloak",
        >= 3 => "Light drizzle mists your face",
        >= 1 => "Clouds threaten overhead",
        _ => "Clear skies stretch ahead"
    };
    
    var windEffect = wind switch {
        >= 4 => ", wind tearing at your clothes",
        >= 2 => ", gentle breeze at your back",
        _ => ""
    };
    
    var sightLine = visibility switch {
        <= 1 => ". Fog shrouds the path ahead",
        <= 2 => ". Mist limits your view",
        _ => ""
    };
    
    return precipitation + windEffect + sightLine;
}

// UI Display
"Light drizzle mists your face, gentle breeze at your back. Fog shrouds the path ahead"
```

## COMPOUND MECHANICS → EMERGENT NARRATIVE

### 11. PRESSURE + MOMENTUM → CHOICE AVAILABILITY

```csharp
// Mechanical State
PlayerPressure: 11
NPCPressure: 6
Momentum: -2

// Choice Generation
List<Choice> GenerateChoices(GameState state) {
    var choices = new List<Choice>();
    
    // High pressure + negative momentum = desperate options
    if (state.PlayerPressure > 10 && state.Momentum < 0) {
        choices.Add(new Choice {
            Text = "*I'll do anything—just give me more time!*",
            Tone = "Desperate",
            Cost = 0, // Desperation makes rash choices free
            Effect = "Reveals vulnerability"
        });
    }
    
    // Low NPC pressure + positive momentum = opportunity to probe
    if (state.NPCPressure < 5 && state.Momentum > 0) {
        choices.Add(new Choice {
            Text = "*Now, about that favor you mentioned...*",
            Tone = "Opportunistic",
            Cost = 1,
            Effect = "Extract commitment"
        });
    }
    
    return choices;
}
```

### 12. ACTIVITY TOKENS + TIME → CROWD BEHAVIOR

```csharp
// Mechanical State
ActivityTokens: 9
TimeBlock: EVENING
FluxCard: "CELEBRATION"

// Crowd Generation
string GenerateCrowdBehavior(int activity, TimeBlock time, string flux) {
    var baseBehavior = (activity, time) switch {
        (>= 8, EVENING) => "Patrons jostle for space at the bar",
        (>= 8, MORNING) => "Workers crowd in for quick breakfast",
        (>= 5, _) => "Groups cluster around tables",
        _ => "A few solitary figures dot the room"
    };
    
    if (flux == "CELEBRATION") {
        baseBehavior += ", infectious merriment spreading through the crowd";
    }
    
    return baseBehavior;
}

// UI Output
"Patrons jostle for space at the bar, infectious merriment spreading through the crowd"
```

### 13. STAMINA + TERRAIN + WEATHER → TRAVEL DIFFICULTY

```csharp
// Mechanical Calculation
BaseStaminaCost: 2
TerrainModifier: +1 (HILL)
WeatherModifier: +1 (RAIN)
EncumbranceModifier: +1 (HEAVY)
TotalCost: 5

// Narrative Generation
string GenerateTravelDifficulty(TravelState state) {
    var difficulty = state.TotalCost switch {
        >= 6 => "This will be grueling",
        >= 4 => "The journey demands much",
        >= 2 => "A manageable trek",
        _ => "An easy stroll"
    };
    
    var factors = new List<string>();
    if (state.TerrainMod > 0) factors.Add("steep climbs");
    if (state.WeatherMod > 0) factors.Add("foul weather");
    if (state.EncumbranceMod > 0) factors.Add("heavy burden");
    
    if (factors.Any()) {
        difficulty += $" - {string.Join(", ", factors)} compound the challenge";
    }
    
    return difficulty;
}

// UI Display
"The journey demands much - steep climbs, foul weather, heavy burden compound the challenge"
```

## VISUAL HIERARCHY RULES

### Information Priority (Top to Bottom)
1. **Attention Points** - Always visible, top center
2. **Pressure Indicators** - Peripheral, when relevant
3. **Character/Location Name** - Context anchor
4. **Body Language/Atmosphere** - Immediate state
5. **Narrative Content** - Main focus area
6. **Choices with Costs** - Action options
7. **Minimal Status** - Bottom bar, only essentials

### Color Coding
```css
--attention-gold: #ffd700;     /* Attention points, important */
--pressure-red: #8b0000;       /* Deadlines, urgency */
--comfort-warm: #d2691e;       /* Safe, positive */
--mystery-purple: #4b0082;     /* Hidden information */
--shadow-dark: #2c2416;        /* Spent resources, locked */
```

### Animation Rules
- Attention spending: 0.3s fade transition
- Pressure changes: Subtle pulse animation
- Discovery reveals: Fade in over 0.5s
- State changes: Smooth color transitions

## IMPLEMENTATION CHECKLIST

### Conversation UI
- [ ] Pressure → Body language generator
- [ ] Attention points → Visual circles
- [ ] Momentum → Conversation flow text
- [ ] Allocation → Available choices

### Location UI
- [ ] Activity tokens → Atmosphere text
- [ ] Observation dice → Discovery reveals
- [ ] Flux cards → Event notifications

### Travel UI
- [ ] Segments → Journey narrative
- [ ] Stamina → Physical descriptions
- [ ] Weather → Environmental text
- [ ] Combined mechanics → Difficulty narrative

## KEY PRINCIPLE

**NEVER show the numbers directly**. Always transform mechanical state into narrative description through these deterministic mappings. The player should feel the mechanics through the prose, not see them as statistics.