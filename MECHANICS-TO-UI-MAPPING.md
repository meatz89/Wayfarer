# Mechanics to UI Mapping Documentation
## Updated: 2025-01-06

## Overview
This document defines the exact mapping from mechanical game states to literary UI displays. Every mechanical value generates specific narrative descriptions through deterministic algorithms, with the letter queue as the core narrative engine.

## LETTER MECHANICS → UI MAPPING

### 1. Letter Properties → Narrative Context

```csharp
// Mechanical State
Letter {
    Type: Trust,
    Stakes: REPUTATION,
    Weight: 3,
    TTL: 2,
    SenderId: "Elena",
    RecipientId: "Lord_Aldwin"
}

// UI Generation
string GenerateLetterNarrative(Letter letter) {
    var urgency = letter.TTL switch {
        <= 1 => "desperately urgent",
        <= 3 => "pressing",
        <= 5 => "important",
        _ => "routine"
    };
    
    var weight = letter.Weight switch {
        5 => "heavy package weighing down your satchel",
        4 => "thick correspondence",
        3 => "substantial letter",
        2 => "standard missive",
        1 => "simple note"
    };
    
    var stakes = GenerateStakesHint(letter.Type, letter.Stakes);
    
    return $"A {urgency} {weight} concerning {stakes}";
}

// Literary UI Output
"A pressing substantial letter concerning a matter of personal honor"
```

### 2. NPC Emotional State → Body Language

```csharp
// Mechanical Calculation
NPCState = CalculateFromLetterQueue(npc);
// Result: DESPERATE (based on TTL:2 + REPUTATION stakes)

// UI Generation
string GenerateBodyLanguage(NPCEmotionalState state, StakeType stakes) {
    return (state, stakes) switch {
        (DESPERATE, REPUTATION) => "fingers worrying their shawl, eyes darting",
        (DESPERATE, WEALTH) => "counting coins nervously, sweat on brow",
        (DESPERATE, SAFETY) => "glancing over shoulder, voice barely a whisper",
        (DESPERATE, SECRET) => "leaning close, trembling hands",
        
        (HOSTILE, REPUTATION) => "chin raised, eyes cold with disdain",
        (HOSTILE, WEALTH) => "arms crossed, tapping foot impatiently",
        (HOSTILE, SAFETY) => "hand near weapon, stance aggressive",
        (HOSTILE, SECRET) => "lips pursed, suspicious glare",
        
        (CALCULATING, _) => "measured breathing, thoughtful pause",
        (WITHDRAWN, _) => "distant gaze, minimal acknowledgment",
        _ => "watching with guarded interest"
    };
}

// Literary UI Output
"Elena's fingers worry her shawl, eyes darting to the door"
```

### 3. Attention Points → Visual and Narrative

```csharp
// Mechanical State
AttentionPoints: 3 (starting scene)
AttentionSpent: 1 (focused on Elena)
AttentionRemaining: 2

// Visual Display
<div class="attention-orbs">
    <span class="orb golden">●</span>  <!-- Available -->
    <span class="orb golden">●</span>  <!-- Available -->
    <span class="orb spent">●</span>   <!-- Spent -->
</div>

// Narrative Description
string GetAttentionNarrative(int remaining) {
    return remaining switch {
        3 => "Your mind is clear and focused, ready to absorb every detail",
        2 => "You remain attentive, though some of your focus has been spent",
        1 => "Your concentration wavers, you must choose your focus carefully",
        0 => "Mental fatigue clouds your thoughts",
        _ => ""
    };
}
```

### 4. Verb + Context → Choice Presentation

```csharp
// Mechanical State
BaseVerb: PLACATE
Context: Trust
NPCState: DESPERATE
Tokens: 5

// Choice Generation
string GenerateChoiceText(BaseVerb verb, TokenType context, 
                         NPCEmotionalState state, int tokens) {
    return (verb, context, state, tokens >= 3) switch {
        (PLACATE, Trust, DESPERATE, true) 
            => "Take her trembling hand in comfort",
        (PLACATE, Trust, DESPERATE, false) 
            => "Offer vague reassurances",
        
        (EXTRACT, _, DESPERATE, _)
            => "Ask what's really in the letter",
        
        (COMMIT, _, DESPERATE, true)
            => "I swear I'll deliver it before any others today",
        
        (DEFLECT, _, HOSTILE, _)
            => "Perhaps you should speak with the postmaster",
        
        _ => GetGenericPresentation(verb)
    };
}

// UI Display (Never shows verb name)
<div class="thought-choice">
    <em>"Take her trembling hand in comfort"</em>
    <span class="attention-dot">●</span>  <!-- 1 attention cost -->
</div>
```

## QUEUE MECHANICS → UI MAPPING

### 5. Queue Pressure → Peripheral Awareness

```csharp
// Mechanical State
Queue: [
    Letter1 { TTL: 1, Sender: "Lord B" },
    Letter2 { TTL: 3, Sender: "Marcus" },
    Letter3 { TTL: 5, Sender: "Elena" }
]
TotalWeight: 8 (of 20 max)

// Peripheral Generation
string GenerateQueuePressure(LetterQueue queue) {
    var mostUrgent = queue.OrderBy(l => l.TTL).First();
    
    if (mostUrgent.TTL <= 1) {
        return $"⚡ {mostUrgent.Sender}'s letter burns in your satchel";
    }
    else if (mostUrgent.TTL <= 3) {
        return $"The weight of {mostUrgent.Sender}'s letter presses against your ribs";
    }
    else if (queue.TotalWeight > 15) {
        return "Your satchel strains with accumulated correspondence";
    }
    else {
        return null; // No pressure warning needed
    }
}

// UI Display (Subtle, peripheral)
<div class="deadline-whisper">
    ⚡ Lord B's letter burns in your satchel
</div>
```

### 6. Letter Stakes → Consequence Hints

```csharp
// Mechanical State
ActiveLetter: { Type: Trust, Stakes: REPUTATION }

// Narrative Hint Generation
string GenerateStakesHint(TokenType type, StakeType stakes) {
    return (type, stakes) switch {
        (Trust, REPUTATION) => "a matter of personal honor",
        (Trust, WEALTH) => "a family's financial crisis",
        (Trust, SAFETY) => "a warning between friends",
        (Trust, SECRET) => "a dangerous confession",
        
        (Commerce, REPUTATION) => "a merchant's credibility",
        (Commerce, WEALTH) => "an urgent trade arrangement",
        (Commerce, SAFETY) => "dangerous cargo manifest",
        (Commerce, SECRET) => "smuggler's instructions",
        
        (Status, REPUTATION) => "a noble's standing",
        (Status, WEALTH) => "an inheritance dispute",
        (Status, SAFETY) => "a challenge to duel",
        (Status, SECRET) => "court intrigue",
        
        (Shadow, REPUTATION) => "blackmail material",
        (Shadow, WEALTH) => "thieves' guild dues",
        (Shadow, SAFETY) => "an assassin's warning",
        (Shadow, SECRET) => "information that kills",
        
        _ => "correspondence"
    };
}
```

## SCENE MECHANICS → UI MAPPING

### 7. Scene Phase → Available Actions

```csharp
// Mechanical State
ScenePhase: PERIPHERAL_AWARENESS
AttentionAvailable: 3
FocusableEntities: ["Elena", "Guards", "Bartender"]

// UI Generation
void RenderPeripheralPhase() {
    // Always visible, no attention cost
    ShowEnvironmentDescription();
    ShowVisibleNPCs();
    ShowQueuePressure();
    
    // Focus options (each costs 1 attention)
    foreach (var entity in FocusableEntities) {
        ShowFocusOption($"Focus on {entity}", cost: 1);
    }
}

// Literary Output
"The Copper Kettle hums with nervous energy. Elena sits alone at 
a corner table, fingers drumming. Two guards shift near the door. 
The bartender polishes glasses with unusual intensity."

[Focus on Elena] ●
[Observe the guards] ●
[Approach the bartender] ●
```

### 8. Attention Allocation → Depth vs Breadth

```csharp
// Mechanical Options
// Player has 3 attention points to allocate

// Breadth Strategy (1-1-1)
FocusOn("Elena", depth: 1);     // Learn basic state
FocusOn("Guards", depth: 1);    // Notice they're watching
FocusOn("Room", depth: 1);      // Find hidden exit

// Depth Strategy (0-3-0)
IgnoreEntity("Guards");          // Miss their interest
FocusOn("Elena", depth: 3);     // Deep conversation, unlock promises
IgnoreEntity("Room");            // Miss environmental hints

// UI Reflects Depth
string GenerateDepthOptions(int attentionSpent) {
    return attentionSpent switch {
        0 => "You notice Elena seems troubled",
        1 => "Elena has a Trust letter with REPUTATION stakes",
        2 => "You can make binding promises about her letter",
        3 => "Complete understanding of her crisis unlocked",
        _ => ""
    };
}
```

## CONSEQUENCE MECHANICS → UI MAPPING

### 9. Expired Letters → Narrative Consequences

```csharp
// Mechanical Event
Letter expired = { 
    Type: Commerce, 
    Stakes: WEALTH,
    Sender: "Marcus"
};

// Consequence Generation
void ProcessExpiredLetter(Letter letter) {
    var consequence = (letter.Type, letter.Stakes) switch {
        (_, REPUTATION) => 
            "Word spreads of your unreliability. Future letters arrive deeper in queue.",
        (Commerce, WEALTH) => 
            $"{letter.Sender} withdraws from all trade. Commerce opportunities vanish.",
        (_, SAFETY) => 
            "Danger goes unwarned. Shadow letters increase as desperation grows.",
        (_, SECRET) => 
            "The secret spreads like wildfire. Everyone knows what was hidden.",
        _ => "Trust erodes."
    };
    
    DisplayConsequence(consequence);
}

// UI Display (Dramatic moment)
<div class="consequence-reveal">
    "Marcus withdraws from all trade. The merchant district turns cold 
    to your presence. Commerce letters cease arriving."
</div>
```

### 10. Attention Depletion → Scene End

```csharp
// Mechanical Trigger
AttentionPoints: 0
UnresolvedSituations: ["Guards suspicious", "Elena waiting"]

// Scene End Generation
void GenerateSceneEnd(List<string> unresolved) {
    var narrative = "Mental exhaustion forces you to withdraw.";
    
    foreach (var situation in unresolved) {
        narrative += GenerateUnresolvedConsequence(situation);
    }
    
    return narrative;
}

// UI Display
"Mental exhaustion forces you to withdraw. The guards' suspicion 
deepens as you leave abruptly. Elena's hope fades as you turn away 
without resolution."
```

## AI NARRATIVE GENERATION

### Tag Generation Pipeline

```csharp
// Mechanical State → Tags for AI
List<string> GenerateNarrativeTags(GameState state) {
    var tags = new List<string>();
    
    // Letter tags
    tags.Add($"[{currentLetter.Type}]");        // [Trust]
    tags.Add($"[{currentLetter.Stakes}]");      // [REPUTATION]
    tags.Add($"[TTL:{currentLetter.TTL}]");     // [TTL:2]
    
    // NPC tags
    tags.Add($"[{npcState}]");                  // [DESPERATE]
    tags.Add($"[Tokens:{tokenCount}]");         // [Tokens:5]
    
    // Scene tags
    tags.Add($"[Location:{location}]");         // [Location:Tavern]
    tags.Add($"[Attention:{remaining}]");       // [Attention:2]
    
    return tags;
}

// AI Prompt
"Generate dialogue for NPC with tags: [Trust] [REPUTATION] [TTL:2] 
[DESPERATE] [Tokens:5]. Context: Letter refusal with social consequences."

// AI Output
"The letter contains Lord Aldwin's marriage proposal. My refusal. 
If he learns before my cousin can intervene at court, I'll be ruined."
```

## Visual Hierarchy Rules

### Information Priority (Top to Bottom)
1. **Attention Points** - Golden orbs, top center
2. **Scene Description** - Environmental narrative
3. **NPC State** - Body language, not numbers
4. **Choices** - Italicized thoughts with cost dots
5. **Peripheral Info** - Deadline pressure, subtle

### Never Display
- Mechanical verb names (PLACATE, EXTRACT)
- Numerical values (Pressure: 11, Tokens: 5)
- State enums (DESPERATE, HOSTILE)
- Raw calculation results

### Always Display
- Narrative descriptions
- Body language
- Environmental details
- Thoughts as choices
- Golden attention orbs

## Implementation Checklist

### Letter → UI
- [ ] Letter properties generate narrative descriptions
- [ ] Stakes create appropriate hints
- [ ] Weight affects physical descriptions
- [ ] TTL drives urgency language

### NPC State → UI
- [ ] Emotional states generate body language
- [ ] Stakes modify physical descriptions
- [ ] No state names visible
- [ ] Contextual variations work

### Attention → UI
- [ ] Golden orbs display correctly
- [ ] Narrative descriptions match points
- [ ] Cost dots on choices
- [ ] Scene ends at 0

### Verbs → UI
- [ ] Verbs never shown by name
- [ ] Context creates variations
- [ ] Choices appear as thoughts
- [ ] Costs integrated subtly

## Key Principle

**The UI must NEVER show the mechanical scaffolding.** Players should experience a literary narrative that emerges from hidden mechanics. Every number becomes a description. Every state becomes body language. Every verb becomes a thought. The mechanics drive the story, but remain invisible.