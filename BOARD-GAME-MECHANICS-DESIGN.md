# Wayfarer Board Game Mechanics Design

## Executive Summary

This document defines the complete board game mechanics that generate Wayfarer's literary UI. Like the letter queue system (8 physical slots with weight and deadlines), all UI content emerges from mechanical operations, not pre-written narrative.

## Core Design Principle

**Mechanics generate narrative, not vice versa.** Every piece of text displayed to the player is the result of board game systems operating on existing resources (connection tokens, location tags, attention points).

## THE TOKEN STAKES SYSTEM

### Foundation: Hidden Bidding with Connection Tokens

NPCs don't have stored resources. Instead, they generate "stakes" deterministically each conversation:

```
NPC Stakes = Hash(npc_type + location_tags + time_of_day + world_tick)
```

Each conversation becomes a hidden bidding game where:
- NPCs stake invisible tokens based on their needs
- Players spend attention to reveal and match stakes
- Outcomes modify the player's actual token pools

### Stake Generation Algorithm

```csharp
Function GenerateStakes(npc_type, location_tags, time_block)
{
    seed = Hash(npc_type + location + time_block);
    rng = SeededRandom(seed);
    
    // Base stakes from NPC archetype
    base_stakes = GetArchetypeStakes(npc_type);
    // Example: Merchant = [Trust:1, Commerce:3, Status:1, Shadow:0]
    
    // Apply location modifiers
    foreach (tag in location_tags)
    {
        stakes = ApplyTagModifier(stakes, tag);
        // Market-day: Commerce +2
        // After-dark: Shadow +2
        // Temple: Trust +2
    }
    
    // Add controlled randomness
    foreach (token_type in TokenTypes)
    {
        stakes[token_type] *= rng.Range(0.8, 1.2);
        stakes[token_type] = Clamp(0, 5);
    }
    
    return stakes;
}
```

### NPC Archetypes (6 Total)

1. **Merchant**: Base [Trust:1, Commerce:4, Status:1, Shadow:0]
2. **Noble**: Base [Trust:1, Commerce:1, Status:4, Shadow:0]
3. **Common**: Base [Trust:3, Commerce:2, Status:0, Shadow:0]
4. **Official**: Base [Trust:0, Commerce:1, Status:3, Shadow:1]
5. **Shadow**: Base [Trust:1, Commerce:1, Status:0, Shadow:3]
6. **Desperate**: Base [Trust:4, Commerce:2, Status:1, Shadow:1]

### Location Tag Modifiers

```
Market-day:     Commerce +2, Trust -1
After-dark:     Shadow +2, Status -1
Festival:       Status +1, Trust +1
Temple:         Trust +2, Shadow -2
Crowded:        Shadow -1, Commerce +1
Private:        Trust +1, Status -1
Official:       Status +2, Trust -1
```

## THE ATTENTION ECONOMY

### Three Attention Points Per Scene

Players allocate 3 attention points each conversation to:

1. **OBSERVE** (1 AP): Reveal one stake type
   - Shows which token type NPC values most
   - Updates body language with new information
   - Cannot observe same stake twice

2. **PRESS** (2 AP): Match highest revealed stake
   - Win that negotiation track
   - Extract information or commitment
   - Costs tokens equal to stake value

3. **GUARD** (1 AP): Protect your tokens
   - Prevent NPC extraction on one type
   - Reduce their stake by 1
   - Reveals what you're protecting

### Attention Allocation Examples

```
Conservative: Observe → Observe → Observe
   Result: Full information, no commitment

Aggressive: Press → Guard → Exit
   Result: Quick negotiation, protected exit

Balanced: Observe → Press → Guard
   Result: Informed decision with protection
```

## BODY LANGUAGE GENERATION

### Template Structure (80 Total)

Body language emerges from stake patterns through template selection:

```csharp
Template SelectBodyLanguage(stakes, revealed_stakes)
{
    highest_stake = GetHighest(stakes);
    intensity = stakes[highest_stake];
    
    // Primary tell from highest stake
    primary = GetPrimaryTell(highest_stake, intensity);
    
    // Secondary tells from other high stakes
    secondary = [];
    foreach (stake in stakes)
    {
        if (stake.value >= 3 && stake != highest_stake)
            secondary.Add(GetSecondaryTell(stake));
    }
    
    // Combine into full description
    return CombineTells(primary, secondary);
}
```

### Primary Tell Library (20 templates)

**Trust Tells by Intensity:**
- 1-2: "maintaining polite distance"
- 3: "occasional eye contact"
- 4: "leaning forward attentively"
- 5: "searching your face desperately"

**Commerce Tells by Intensity:**
- 1-2: "hands folded calmly"
- 3: "fingers near purse"
- 4: "drumming fingers on table"
- 5: "gripping ledger tightly"

**Status Tells by Intensity:**
- 1-2: "casual posture"
- 3: "straightening slightly"
- 4: "formal bearing evident"
- 5: "radiating authority"

**Shadow Tells by Intensity:**
- 1-2: "relaxed awareness"
- 3: "occasional glances around"
- 4: "checking exits subtly"
- 5: "coiled like a spring"

### Emotional State Patterns (15 templates)

Specific stake combinations create emotional overlays:

```
DESPERATION: Trust 4+ AND any other 3+
   → "words tumbling out", "barely contained panic"

CALCULATION: Commerce 4+ AND others low
   → "measuring each word", "precise gestures"

SUSPICION: Shadow 3+ AND Guard stance
   → "narrowed eyes", "defensive posture"

AUTHORITY: Status 5
   → "commanding presence", "expects obedience"

VULNERABILITY: Trust 5 alone
   → "trembling slightly", "voice catching"
```

## PROGRESSIVE REVELATION SYSTEM

### Conversation Flow States

```
State: OPENING (Round 1)
- No stakes visible
- Body language at 33% intensity
- Base choices only
- Can exit freely

State: PROBING (Round 2)
- 1-2 stakes revealed
- Body language at 66% intensity
- Conditional choices appear
- Exit costs social friction

State: CORE (Round 3)
- 2-3 stakes revealed
- Body language at 100% intensity
- Binding choices available
- Exit damages relationship
```

### Information Revelation

Each Observe action reveals in priority order:
1. Highest unrevealed stake
2. Adds specific tell to body language
3. Enables new choice options
4. Updates peripheral awareness

## CHOICE GENERATION SYSTEM

### Choice Templates (30 patterns)

Choices emerge from mechanical state:

```csharp
Function GenerateChoices(revealed_stakes, attention_remaining, player_tokens)
{
    choices = [];
    
    // Free choice - always available
    choices.Add(CreateFreeChoice(revealed_stakes));
    
    // Observe choices - if attention available
    if (attention_remaining >= 1)
    {
        foreach (stake in unrevealed_stakes)
            choices.Add(CreateObserveChoice(stake));
    }
    
    // Press choices - if stakes revealed and attention available
    if (attention_remaining >= 2)
    {
        foreach (stake in revealed_stakes where stake.value >= 3)
            choices.Add(CreatePressChoice(stake));
    }
    
    // Binding choices - high stakes only
    if (revealed_stakes.Any(s => s.value >= 4) && attention_remaining >= 2)
    {
        choices.Add(CreateBindingChoice(highest_revealed));
    }
    
    return choices;
}
```

### Choice Cost Indicators

```
Free (0 AP):     No symbol, always available
Light (1 AP):    "◆" - Observation, simple queries
Medium (2 AP):   "◆◆" - Pressing, commitments  
Heavy (3 AP):    "◆◆◆" - Deep investigation, oaths
```

## LOCATION ATMOSPHERE SYSTEM

### Activity Token Generation

Locations generate activity tokens at dawn:

```csharp
Function GenerateActivityTokens(location_type, day_type)
{
    base_activity = location_type switch
    {
        Market => 8,
        Tavern => 6,
        Temple => 3,
        Street => 5,
        Private => 2
    };
    
    modifiers = day_type switch
    {
        Festival => +4,
        Market_Day => +2,
        Holy_Day => -1,
        Rain => -2
    };
    
    return Clamp(base_activity + modifiers, 0, 12);
}
```

### Atmosphere Description Generation

```csharp
Function GenerateAtmosphere(activity_tokens, location_type, weather)
{
    density = activity_tokens switch
    {
        >= 10 => "packed",
        >= 7 => "bustling", 
        >= 4 => "lively",
        >= 2 => "quiet",
        _ => "empty"
    };
    
    sound = activity_tokens switch
    {
        >= 10 => "cacophony of voices",
        >= 7 => "animated chatter",
        >= 4 => "gentle murmur",
        >= 2 => "occasional voices",
        _ => "heavy silence"
    };
    
    return $"{density} {location_type}, {sound}";
}
```

### Feeling Tags

Generated from location + time + weather + activity:

```
Tavern + Evening + Rainy = "🔥 Hearth-warmed"
Market + Morning + Sunny = "☀️ Sun-drenched"
Temple + Any + Any = "🕊️ Sacred-quiet"
Street + Night + Clear = "⭐ Star-watched"
Any + Any + Activity>8 = "👥 Crowd-pressed"
```

## OBSERVATION DISCOVERY SYSTEM

### Observation Dice Allocation

Players allocate 3 observation dice to different areas:

```csharp
enum ObservationType
{
    Passive = d4,   // Background awareness
    Active = d6,    // Focused observation
    Deep = d8       // Investigation
}

Function RollDiscovery(dice_count, dice_type, location)
{
    discoveries = [];
    threshold = location.hidden_info_threshold;
    
    for (i = 0; i < dice_count; i++)
    {
        roll = Roll(dice_type);
        if (roll >= threshold)
        {
            discovery = SelectDiscovery(location.available_discoveries);
            discoveries.Add(discovery);
        }
        else if (roll >= threshold - 1)
        {
            discoveries.Add("❓ Something interesting...");
        }
    }
    
    return discoveries;
}
```

## TRAVEL MECHANICS

### Route Segment System

Travel uses segments with terrain-based costs:

```csharp
struct RouteSegment
{
    TerrainType terrain;  // Forest, Hill, River, Road
    int base_hours;       // 1-4 hours
    int stamina_cost;     // 1-5 stamina
    string[] descriptors; // "winding", "steep", "muddy"
}

Function GenerateTravelNarrative(segment, stamina_ratio, weather)
{
    effort = stamina_ratio switch
    {
        > 0.5 => "exhausting",
        > 0.3 => "demanding",
        > 0.1 => "manageable",
        _ => "easy"
    };
    
    terrain_desc = GetTerrainDescription(segment.terrain, weather);
    
    return $"The {effort} journey {terrain_desc}";
}
```

## CONTENT REQUIREMENTS SUMMARY

### Minimum Viable Content (40-50 hours total)

1. **Body Language Templates**: 80 pieces (32 hours)
   - 20 primary tells (4 types × 5 intensities)
   - 40 combination patterns
   - 20 emotional overlays

2. **Choice Templates**: 30 patterns (8 hours)
   - 10 observe choices
   - 10 press choices
   - 10 special/binding choices

3. **Atmosphere Descriptors**: 20 pieces (5 hours)
   - 5 density descriptions
   - 5 sound descriptions
   - 10 feeling combinations

4. **Travel Descriptors**: 20 pieces (5 hours)
   - 5 effort levels
   - 5 terrain types
   - 10 weather modifiers

**Total: 150 content pieces generating 500-800 unique experiences**

## IMPLEMENTATION PRIORITY

### Phase 1: Core Conversation System (Week 1)
- Token stakes generation
- Attention allocation
- Body language selection
- Basic choice generation

### Phase 2: Location Atmosphere (Week 2)
- Activity token system
- Feeling tag generation
- Observation dice mechanics

### Phase 3: Progressive Systems (Week 3)
- Conversation flow states
- Information revelation
- Binding obligations

### Phase 4: Polish & Variation (Week 4)
- Additional templates
- Edge case handling
- Balance tuning

## KEY DESIGN VICTORIES

1. **Uses Existing Resources**: Connection tokens, location tags, no new currencies
2. **True Board Game Mechanics**: Hidden bidding, resource allocation, dice rolling
3. **Generates Literary UI**: All text emerges from mechanical state
4. **Scalable**: 150 templates create hundreds of unique experiences  
5. **Deterministic**: Same inputs always generate same output
6. **Elegant**: One core system (token stakes) drives everything

## COMPARISON TO LETTER QUEUE

Just as the letter queue has:
- **Physical slots** (8 positions)
- **Weight** (affecting stamina)
- **Deadlines** (creating pressure)
- **Token costs** (for reordering)

Conversations have:
- **Token stakes** (hidden bids)
- **Attention points** (limited resource)
- **Progressive revelation** (information game)
- **Mechanical consequences** (token changes)

Both systems use board game mechanics to create narrative experiences without requiring pre-written content.