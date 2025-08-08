# Atmospheric Tag System Implementation Plan

## Overview
Transform the visible atmospheric descriptors from the UI mockups into a unified system that serves both narrative atmosphere AND mechanical gameplay effects. Tags are displayed with icons as literary descriptors while secretly driving game mechanics.

## Core Philosophy
- **Atmospheric tags ARE the mechanics** - not separate systems
- **Visible but literary** - players see "👥 Crowded", not "Busy [-1 Attention]"
- **Dual purpose** - each tag creates atmosphere AND affects gameplay
- **Discovery through play** - players learn effects naturally, not through tooltips

## Current State Analysis

### What's Built But Disconnected
1. **EnvironmentalHintSystem** - Exists, properly registered, generates hints based on time/location
2. **ObservationSystem** - Exists, registered, but NEVER CALLED in UI
3. **NPCEmotionalStateCalculator** - Fully working, calculates states from queue
4. **ActionBeatGenerator** - Exists but uses Random (not deterministic)
5. **BindingObligationSystem** - Exists but NOT INJECTED into GameFacade

### What Needs Building
1. **Atmospheric tag data structure** connecting visual to mechanical
2. **Tag-driven action filtering** for location-based choices
3. **Emotional state transitions** from player actions
4. **Template content system** to avoid writing 1000+ pieces

## Tag System Design

### Location Tags (From Mockups)

```csharp
public enum LocationTag
{
    // Atmosphere tags (affect attention/concentration)
    Crowded,        // 👥 Reduces attention points by 1
    Quiet,          // 🤫 Enables private conversations
    Public,         // 🏛️ Reputation effects spread faster
    Private,        // 🔒 Shadow dealings possible
    
    // Environmental tags (affect available actions)
    HearthWarmed,   // 🔥 Comfort enables longer conversations
    AleScented,     // 🍺 Loosens tongues (more info)
    MusicDrifting,  // 🎵 Covers whispered conversations
    Sunny,          // ☀️ Positive mood modifier
    
    // Activity tags (affect NPC availability)
    Crossroads,     // 🔄 NPCs coming and going
    MarketDay,      // 🛒 Commerce actions available
    GuardPatrol,    // 💂 Restricted actions
}

public class LocationTagDefinition
{
    public LocationTag Tag { get; set; }
    public string Icon { get; set; }           // "👥"
    public string Display { get; set; }        // "Crowded"
    public MechanicalEffect Effect { get; set; }
}

public class MechanicalEffect
{
    public int AttentionModifier { get; set; }     // -1, 0, +1
    public float ReputationSpread { get; set; }    // 0.5x to 2.0x
    public List<string> EnabledActions { get; set; }
    public List<string> DisabledActions { get; set; }
    public int EmotionalModifier { get; set; }     // -10 to +10 starting mood
}
```

### NPC Emotional States (Dynamic)

```csharp
public class NPCEmotionalState
{
    public string NpcId { get; set; }
    public int EmotionalValue { get; set; }  // -100 to 100 (hidden)
    
    // Visual representation (what players see)
    public string GetMoodIcon()
    {
        return EmotionalValue switch
        {
            < -50 => "😠",  // Hostile
            < -20 => "😤",  // Frustrated  
            < 20 => "😐",   // Neutral
            < 50 => "🙂",   // Pleasant
            _ => "😊"       // Friendly
        };
    }
    
    // Mechanical effects
    public ConversationModifiers GetModifiers()
    {
        return new ConversationModifiers
        {
            MaxAttention = EmotionalValue < -30 ? 2 : 3,
            VerbCosts = new Dictionary<Verb, int>
            {
                [Verb.HELP] = EmotionalValue < -30 ? 999 : 1,      // Locked if hostile
                [Verb.NEGOTIATE] = EmotionalValue < 0 ? 2 : 1,
                [Verb.INVESTIGATE] = 1
            },
            ExtraOptions = EmotionalValue > 30 ? GetFriendlyOptions() : null
        };
    }
}
```

## Implementation Tasks

### Phase 1: Fix Core Systems (CRITICAL)

#### Task 1: Wire BindingObligationSystem
**File:** `/mnt/c/git/wayfarer/src/Services/GameFacade.cs`
```csharp
// Add to constructor parameters (NOT optional)
BindingObligationSystem bindingObligationSystem,

// Add to constructor body
_bindingObligationSystem = bindingObligationSystem;

// In CreateConversationViewModel method, add:
BindingObligations = _bindingObligationSystem.GetActiveObligations()
```

**File:** `/mnt/c/git/wayfarer/src/ServiceConfiguration.cs`
```csharp
// GameFacade registration needs BindingObligationSystem passed
services.AddSingleton<GameFacade>(sp => new GameFacade(
    // ... other parameters ...
    sp.GetRequiredService<BindingObligationSystem>(),
    // ... rest ...
));
```

#### Task 2: Connect ObservationSystem to UI
**File:** `/mnt/c/git/wayfarer/src/Services/GameFacade.cs`
```csharp
// In CreateLocationViewModel method:
Observations = _observationSystem.GetObservations(locationId)
```

**File:** `/mnt/c/git/wayfarer/src/Pages/MainGameplayView.razor`
```razor
@if (Model.Observations?.Any() == true)
{
    <div class="observations-section">
        <h3>YOU NOTICE:</h3>
        @foreach (var obs in Model.Observations)
        {
            <div class="observation">
                <span class="obs-icon">@obs.Icon</span>
                <span class="obs-text">@obs.Text</span>
            </div>
        }
    </div>
}
```

#### Task 3: Fix ActionBeatGenerator Determinism
**File:** `/mnt/c/git/wayfarer/src/GameState/ActionBeatGenerator.cs`
```csharp
public class ActionBeatGenerator
{
    // Remove: private readonly Random _random = new Random();
    
    public string GenerateActionBeat(
        NPCEmotionalState emotionalState,
        StakeType? stakes,
        int conversationTurn,
        bool isUrgent,
        string npcId)  // Add NPC ID for seed
    {
        // Create deterministic random
        var seed = npcId.GetHashCode() + conversationTurn;
        var random = new Random(seed);
        
        // Use random.Next() instead of _random.Next()
    }
}
```

### Phase 2: Implement Tag System

#### Task 4: Add Location Tags
**File:** `/mnt/c/git/wayfarer/src/GameState/Location.cs`
```csharp
public class Location
{
    // Add to existing properties
    public List<LocationTag> AtmosphereTags { get; set; } = new();
    
    // Helper to get display tags
    public List<(string Icon, string Label)> GetDisplayTags()
    {
        return AtmosphereTags.Select(t => 
            TagDefinitions.LocationTags[t].GetDisplay()).ToList();
    }
}
```

**File:** `/mnt/c/git/wayfarer/src/Content/Templates/locations.json`
```json
{
  "locationId": "market_square",
  "atmosphereTags": ["Crowded", "Public", "MarketDay"],
  // ... rest of location data
}
```

#### Task 5: Add NPC Emotional Tracking
**File:** `/mnt/c/git/wayfarer/src/Game/MainSystem/NPC.cs`
```csharp
public class NPC
{
    // Add to existing properties
    public int EmotionalValue { get; set; } = 0;  // -100 to 100
    
    // Starting personality (affects initial emotional value)
    public NPCPersonality BasePersonality { get; set; }
}
```

**File:** `/mnt/c/git/wayfarer/src/GameState/NPCEmotionalStateManager.cs` (NEW)
```csharp
public class NPCEmotionalStateManager
{
    private Dictionary<string, int> _emotionalStates = new();
    
    public void ModifyEmotionalState(string npcId, VerbResult result)
    {
        var current = _emotionalStates.GetValueOrDefault(npcId, 0);
        
        var change = result.Verb switch
        {
            Verb.HELP when result.Success => +15,
            Verb.HELP when !result.Success => -5,
            Verb.NEGOTIATE when result.Fair => +5,
            Verb.NEGOTIATE when result.Exploited => -10,
            Verb.INVESTIGATE => -5,  // Always annoys
            _ => 0
        };
        
        _emotionalStates[npcId] = Math.Clamp(current + change, -100, 100);
    }
    
    public EmotionalBand GetEmotionalBand(string npcId)
    {
        var value = _emotionalStates.GetValueOrDefault(npcId, 0);
        return value switch
        {
            < -33 => EmotionalBand.Hostile,
            > 33 => EmotionalBand.Friendly,
            _ => EmotionalBand.Neutral
        };
    }
}
```

### Phase 3: Mechanical Integration

#### Task 6: Tag-Driven Action Filtering
**File:** `/mnt/c/git/wayfarer/src/Services/ActionGenerator.cs`
```csharp
public class ActionGenerator
{
    public List<GameAction> GetAvailableActions(
        Location location,
        NPCEmotionalState npcState = null)
    {
        var baseActions = GetBaseActionsForLocation(location.Type);
        var filtered = new List<GameAction>();
        
        // Apply location tag filters
        foreach (var action in baseActions)
        {
            bool enabled = true;
            bool disabled = false;
            
            foreach (var tag in location.AtmosphereTags)
            {
                var tagDef = TagDefinitions.LocationTags[tag];
                if (tagDef.Effect.EnabledActions.Contains(action.Id))
                    enabled = true;
                if (tagDef.Effect.DisabledActions.Contains(action.Id))
                    disabled = true;
            }
            
            if (enabled && !disabled)
                filtered.Add(action);
        }
        
        // Apply NPC emotional filters if in conversation
        if (npcState != null)
        {
            var modifiers = npcState.GetModifiers();
            filtered = filtered.Where(a => 
                modifiers.VerbCosts.GetValueOrDefault(a.Verb, 999) < 999).ToList();
        }
        
        return filtered;
    }
}
```

#### Task 7: Conversation Choice Modification
**File:** `/mnt/c/git/wayfarer/src/Game/ConversationSystem/ConversationChoiceGenerator.cs`
```csharp
public class ConversationChoiceGenerator
{
    public List<ConversationChoice> GenerateChoices(
        Verb verb,
        NPC npc,
        Location location,
        AttentionManager attention)
    {
        // Get base choices for verb
        var baseChoices = GetBaseChoicesForVerb(verb);
        
        // Apply location tag modifiers
        var locationMods = CalculateLocationModifiers(location.AtmosphereTags);
        
        // Apply NPC emotional modifiers
        var emotionalMods = npc.GetEmotionalModifiers();
        
        // Filter and modify choices
        var finalChoices = new List<ConversationChoice>();
        foreach (var choice in baseChoices)
        {
            // Modify attention cost
            choice.AttentionCost = Math.Max(1, 
                choice.BaseAttentionCost + 
                locationMods.AttentionModifier + 
                emotionalMods.AttentionModifier);
            
            // Check if available
            if (choice.AttentionCost <= attention.Current)
            {
                finalChoices.Add(choice);
            }
        }
        
        // Add special choices for high emotional states
        if (npc.EmotionalValue > 50)
        {
            finalChoices.Add(GetSpecialFriendlyChoice());
        }
        
        return finalChoices.Take(5).ToList();  // Max 5 choices
    }
}
```

### Phase 4: Content Templates

#### Task 8: Create Template System
**File:** `/mnt/c/git/wayfarer/src/Content/Templates/conversation_templates.json` (NEW)
```json
{
  "emotionalModifiers": {
    "hostile": [
      "Through gritted teeth,",
      "With barely contained anger,",
      "Coldly,"
    ],
    "neutral": [
      "Considering carefully,",
      "After a pause,",
      "Thoughtfully,"
    ],
    "friendly": [
      "With a warm smile,",
      "Leaning in conspiratorially,",
      "Cheerfully,"
    ]
  },
  
  "verbTemplates": {
    "HELP": "I need {TASK} because {STAKES}",
    "NEGOTIATE": "I can offer {PAYMENT} for {SERVICE}",
    "INVESTIGATE": "Tell me about {TOPIC}"
  },
  
  "npcFlavors": {
    "merchant": ", and time is money",
    "noble": ", as befits my station",
    "scholar": ", according to my research"
  }
}
```

**File:** `/mnt/c/git/wayfarer/src/GameState/ConversationTemplateEngine.cs` (NEW)
```csharp
public class ConversationTemplateEngine
{
    private ConversationTemplates _templates;
    
    public string GenerateDialogue(
        Verb verb,
        EmotionalBand emotional,
        string npcType,
        ConversationContext context)
    {
        // Select components
        var emotionalMod = SelectRandom(_templates.EmotionalModifiers[emotional]);
        var verbTemplate = _templates.VerbTemplates[verb];
        var npcFlavor = _templates.NpcFlavors[npcType];
        
        // Fill template slots
        verbTemplate = verbTemplate
            .Replace("{TASK}", context.Task)
            .Replace("{STAKES}", context.Stakes)
            .Replace("{PAYMENT}", context.Payment)
            .Replace("{SERVICE}", context.Service)
            .Replace("{TOPIC}", context.Topic);
        
        // Combine
        return $"{emotionalMod} \"{verbTemplate}{npcFlavor}\"";
    }
    
    private string SelectRandom(List<string> options)
    {
        // Use deterministic selection based on context
        var seed = DateTime.Now.Day + options.Count;
        return options[seed % options.Count];
    }
}
```

## UI Integration

### Location Screen Tags
```razor
<div class="location-header">
    <div class="location-name">@Model.LocationName</div>
    <div class="location-tags">
        @foreach (var tag in Model.AtmosphereTags)
        {
            <span class="tag">@tag.Icon @tag.Label</span>
        }
    </div>
</div>
```

### Conversation Screen Mood
```razor
<div class="character-focus">
    <div class="character-name">
        @Model.NpcName 
        <span class="mood-icon">@Model.MoodIcon</span>
    </div>
    <div class="character-state">@Model.BodyLanguage</div>
</div>
```

## Files to Modify Summary

1. **ServiceConfiguration.cs** - Fix GameFacade dependency injection
2. **GameFacade.cs** - Wire all systems, add emotional tracking
3. **Location.cs** - Add AtmosphereTags property
4. **NPC.cs** - Add EmotionalValue property
5. **BindingObligationSystem.cs** - Already exists, needs wiring
6. **ObservationSystem.cs** - Already exists, needs UI connection
7. **ActionBeatGenerator.cs** - Fix Random to be deterministic
8. **ConversationChoiceGenerator.cs** - Apply tag modifiers
9. **ActionGenerator.cs** - Filter by location tags
10. **MainGameplayView.razor** - Display observations
11. **ConversationScreen.razor** - Show mood icons

## New Files to Create

1. **NPCEmotionalStateManager.cs** - Track emotional changes
2. **ConversationTemplateEngine.cs** - Generate dialogue from templates
3. **TagDefinitions.cs** - Central registry of all tags and effects
4. **conversation_templates.json** - Template content

## Testing Checklist

- [ ] BindingObligationSystem creates obligations from conversation choices
- [ ] ObservationSystem displays in location screens
- [ ] ActionBeatGenerator produces same output for same inputs
- [ ] Location tags visible in UI with icons
- [ ] NPC mood icons change based on emotional value
- [ ] Crowded locations reduce attention points
- [ ] Hostile NPCs lock out HELP verb
- [ ] Friendly NPCs offer bonus conversation options
- [ ] Template system generates varied but coherent dialogue
- [ ] All systems work together without conflicts

## Content Requirements

**Minimal Viable Content (25 hours):**
- 20 emotional modifier phrases (split across 3 bands)
- 3 verb templates with variable slots
- 7 NPC personality flavors
- 15 location-specific environmental hints
- 10 observation templates

**This creates 1000+ combinations from just 55 content pieces.**

## Success Criteria

1. **Players see atmospheric tags** that feel literary, not mechanical
2. **Tags secretly drive mechanics** without explicit explanation
3. **Emotional states persist** between conversations
4. **Content feels varied** despite template system
5. **Discovery through play** - players learn effects naturally
6. **Queue pressure enhanced** - not obscured by new systems

## Next Session Starting Point

Begin with Task 1: Fix BindingObligationSystem injection in GameFacade.cs and ServiceConfiguration.cs. This is the most critical disconnected system that already exists but isn't wired up.