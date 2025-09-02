# Exhaust Mechanics & UI Transparency Implementation Plan

## Overview
This document outlines the implementation of the refined exhaust mechanics system for Wayfarer's card-based conversations, replacing the special-case "Final Word" property with systemic mechanics using card properties.

## Core Design Principles

### 1. Properties List Instead of Booleans
- Cards have a `List<CardProperty>` instead of multiple boolean flags
- Allows any combination of properties without schema changes
- Goal cards are just cards with `["Fleeting", "Opportunity"]` properties
- No special cases needed - everything uses the same system

### 2. Three Separate Effect Systems
Every card can define three distinct effects:
- **Success Effect**: Triggers when card succeeds
- **Failure Effect**: Triggers when card fails (optional)
- **Exhaust Effect**: Triggers when card vanishes unplayed (optional)

### 3. Systemic Goal Pressure
Goals create pressure through normal mechanics:
- Having both Fleeting AND Opportunity properties
- Exhaust effect of EndConversation
- This means: Cannot SPEAK other cards (Fleeting exhausts), Cannot LISTEN (Opportunity exhausts)
- Must play immediately or conversation fails

## Phase 1: Core Backend Implementation

### 1.1 Card Property System

```csharp
public enum CardProperty
{
    Persistent,    // Default - stays until played
    Fleeting,      // Exhausts after SPEAK if unplayed
    Opportunity,   // Exhausts after LISTEN if unplayed
    Skeleton,      // System-generated placeholder
    Burden,        // Blocks deck slots
    Observable     // From observations
}

public class ConversationCard
{
    // Single list replaces all boolean flags
    public List<CardProperty> Properties { get; set; } = new();
    
    // Helper properties for compatibility
    public bool IsFleeting => Properties.Contains(CardProperty.Fleeting);
    public bool IsOpportunity => Properties.Contains(CardProperty.Opportunity);
    public bool IsPersistent => !Properties.Contains(CardProperty.Fleeting) 
                                && !Properties.Contains(CardProperty.Opportunity);
    public bool IsGoal => Properties.Contains(CardProperty.Fleeting) 
                          && Properties.Contains(CardProperty.Opportunity);
}
```

### 1.2 Three-Effect System

```csharp
public class CardEffect
{
    public CardEffectType Type { get; set; }
    public string Value { get; set; }  // Number, formula, or atmosphere name
    public Dictionary<string, object> Data { get; set; }  // Additional effect data
}

public enum CardEffectType
{
    None,
    AddComfort,
    DrawCards,
    AddWeight,
    SetAtmosphere,     // Value = atmosphere name
    EndConversation,
    ScaleByTokens,     // Value = formula like "Trust"
    ScaleByComfort,    // Value = formula like "4 - comfort"
    ScaleByPatience,   // Value = formula like "patience / 3"
    ScaleByWeight,     // Value = formula like "weight"
    ComfortReset,      // Sets comfort to 0
    WeightRefresh,     // Refreshes weight to max
    FreeNextAction     // Next action costs 0 patience
}

public class ConversationCard
{
    public CardEffect SuccessEffect { get; set; }
    public CardEffect FailureEffect { get; set; }
    public CardEffect ExhaustEffect { get; set; }
}
```

### 1.3 Exhaust Mechanics in HandManager

```csharp
public bool OnSpeakAction(ConversationCard playedCard)
{
    // Play the selected card
    currentHand.Remove(playedCard);
    discardPile.Add(playedCard);
    
    // Exhaust all Fleeting cards (including goals with Fleeting + Opportunity)
    var fleetingCards = currentHand
        .Where(c => c.Properties.Contains(CardProperty.Fleeting))
        .Where(c => c != playedCard)  // Don't exhaust the played card
        .ToList();
        
    foreach (var card in fleetingCards)
    {
        // Execute exhaust effect if it exists
        if (card.ExhaustEffect?.Type != CardEffectType.None)
        {
            var continueConversation = ExecuteExhaustEffect(card);
            if (!continueConversation) 
            {
                // Exhaust effect ended conversation (e.g., goal card)
                return false;
            }
        }
        
        currentHand.Remove(card);
        exhaustedPile.Add(card);  // Track separately from discard
    }
    
    return true;  // Conversation continues
}

public bool OnListenAction()
{
    // Exhaust all Opportunity cards (including goals with Fleeting + Opportunity)
    var opportunityCards = currentHand
        .Where(c => c.Properties.Contains(CardProperty.Opportunity))
        .ToList();
        
    foreach (var card in opportunityCards)
    {
        // Execute exhaust effect if it exists
        if (card.ExhaustEffect?.Type != CardEffectType.None)
        {
            var continueConversation = ExecuteExhaustEffect(card);
            if (!continueConversation)
            {
                // Exhaust effect ended conversation
                return false;
            }
        }
        
        currentHand.Remove(card);
        exhaustedPile.Add(card);
    }
    
    return true;
}

private bool ExecuteExhaustEffect(ConversationCard card)
{
    switch (card.ExhaustEffect.Type)
    {
        case CardEffectType.EndConversation:
            // Goal cards typically have this
            EndConversation(ConversationEndReason.GoalExhausted, card);
            return false;  // Conversation ends
            
        case CardEffectType.SetAtmosphere:
            var atmosphere = Enum.Parse<AtmosphereType>(card.ExhaustEffect.Value);
            atmosphereManager.SetAtmosphere(atmosphere);
            return true;
            
        case CardEffectType.DrawCards:
            var count = int.Parse(card.ExhaustEffect.Value);
            DrawCards(count);
            return true;
            
        case CardEffectType.AddComfort:
            var comfort = int.Parse(card.ExhaustEffect.Value);
            comfortManager.AddComfort(comfort);
            return true;
            
        // ... other effects
        
        default:
            return true;
    }
}
```

## Phase 2: JSON Content Structure

### 2.1 Goal Card Example
```json
{
  "id": "elena_letter_goal",
  "name": "Deliver Elena's Refusal",
  "weight": 5,
  "difficulty": "VeryHard",
  
  "properties": ["Fleeting", "Opportunity"],  // Both = must play now!
  
  "successEffect": {
    "type": "EndConversation",
    "value": "success",
    "data": {
      "createsObligation": true,
      "obligationType": "delivery",
      "recipientId": "lord_blackwood",
      "deadline": 73,
      "payment": 10,
      "tokenReward": {"Trust": 2}
    }
  },
  
  "failureEffect": {
    "type": "EndConversation",
    "value": "failure",
    "data": {
      "createsObligation": true,
      "obligationType": "delivery",
      "recipientId": "lord_blackwood",
      "deadline": 24,
      "payment": -5,
      "penalty": "burden_card"
    }
  },
  
  "exhaustEffect": {
    "type": "EndConversation",
    "value": "abandoned",
    "data": {
      "reason": "Failed to act on urgent letter",
      "relationshipPenalty": -2
    }
  },
  
  "dialogueFragment": "Please, take this letter to Lord Blackwood before sunset!"
}
```

### 2.2 Normal Card Examples
```json
[
  {
    "id": "interrupt",
    "name": "Interrupt",
    "weight": 1,
    "difficulty": "Hard",
    
    "properties": ["Opportunity"],  // Only exhausts on LISTEN
    
    "successEffect": {
      "type": "SetAtmosphere",
      "value": "Receptive"
    },
    
    "failureEffect": {
      "type": "AddComfort",
      "value": "-2"
    },
    
    "exhaustEffect": {
      "type": "SetAtmosphere",
      "value": "Pressured"
    },
    
    "dialogueFragment": "Wait, let me say something..."
  },
  
  {
    "id": "desperate_plea",
    "name": "Desperate Plea",
    "weight": 3,
    "difficulty": "Hard",
    
    "properties": ["Fleeting"],  // Only exhausts on SPEAK
    
    "successEffect": {
      "type": "ScaleByComfort",
      "value": "4 - comfort"  // Formula
    },
    
    "failureEffect": null,  // No special failure effect
    
    "exhaustEffect": {
      "type": "DrawCards",
      "value": "1"
    },
    
    "dialogueFragment": "Please, you have to understand!"
  },
  
  {
    "id": "final_statement",
    "name": "Final Statement",
    "weight": 5,
    "difficulty": "VeryHard",
    
    "properties": ["Fleeting"],
    
    "successEffect": {
      "type": "AddComfort",
      "value": "5"
    },
    
    "failureEffect": {
      "type": "SetAtmosphere",
      "value": "Volatile"
    },
    
    "exhaustEffect": {
      "type": "SetAtmosphere",
      "value": "Final"  // Creates high stakes
    },
    
    "dialogueFragment": "This is my final offer."
  }
]
```

## Phase 3: UI Implementation

### 3.1 Property-Based Card Display

```html
<!-- Card component -->
<div class="card @GetCardClasses(card)">
  <!-- Property badges -->
  <div class="card-properties">
    @foreach (var prop in card.Properties)
    {
      <span class="property-badge @prop.ToString().ToLower()" 
            title="@GetPropertyTooltip(prop)">
        @GetPropertyIcon(prop) @GetPropertyLabel(prop)
      </span>
    }
  </div>
  
  <!-- Critical warning for goal cards -->
  @if (card.IsGoal)
  {
    <div class="goal-warning">
      ⚠️ MUST PLAY NOW - Exhausts on ANY action!
    </div>
  }
  
  <!-- Card content -->
  <div class="card-header">
    <span class="card-name">@card.Name</span>
    <span class="card-weight">@card.Weight</span>
  </div>
  
  <!-- Three-effect display -->
  <div class="card-effects">
    <!-- Success -->
    <div class="effect success">
      <span class="chance">@GetSuccessChance(card)%</span>
      <span class="label">Success:</span>
      <span class="description">@DescribeEffect(card.SuccessEffect)</span>
    </div>
    
    <!-- Failure -->
    <div class="effect failure">
      <span class="chance">@GetFailureChance(card)%</span>
      <span class="label">Failure:</span>
      <span class="description">
        @if (card.FailureEffect?.Type != CardEffectType.None)
        {
          @DescribeEffect(card.FailureEffect)
        }
        else
        {
          <span class="no-effect">No effect</span>
        }
      </span>
    </div>
    
    <!-- Exhaust (only show if exists) -->
    @if (card.ExhaustEffect?.Type != CardEffectType.None)
    {
      <div class="effect exhaust">
        <span class="icon">💨</span>
        <span class="label">If not played:</span>
        <span class="description">@DescribeEffect(card.ExhaustEffect)</span>
      </div>
    }
  </div>
</div>
```

### 3.2 Helper Methods

```csharp
string GetPropertyIcon(CardProperty prop) => prop switch
{
    CardProperty.Fleeting => "⚡",
    CardProperty.Opportunity => "⏰",
    CardProperty.Burden => "⛓️",
    CardProperty.Skeleton => "💀",
    CardProperty.Observable => "👁️",
    _ => ""
};

string GetPropertyLabel(CardProperty prop) => prop switch
{
    CardProperty.Fleeting => "Fleeting",
    CardProperty.Opportunity => "Opportunity",
    CardProperty.Burden => "Burden",
    _ => prop.ToString()
};

string GetPropertyTooltip(CardProperty prop) => prop switch
{
    CardProperty.Fleeting => "Removed after SPEAK if unplayed",
    CardProperty.Opportunity => "Removed after LISTEN if unplayed",
    CardProperty.Burden => "Blocks a deck slot",
    CardProperty.Observable => "From an observation",
    _ => ""
};

string DescribeEffect(CardEffect effect)
{
    if (effect == null || effect.Type == CardEffectType.None)
        return "No effect";
        
    return effect.Type switch
    {
        CardEffectType.AddComfort => $"{(effect.Value.StartsWith("-") ? "" : "+")}{effect.Value} comfort",
        CardEffectType.DrawCards => $"Draw {effect.Value} cards",
        CardEffectType.AddWeight => $"Add {effect.Value} weight",
        CardEffectType.SetAtmosphere => $"Atmosphere: {effect.Value}",
        CardEffectType.EndConversation => GetEndConversationDescription(effect),
        CardEffectType.ScaleByTokens => $"+X comfort (X = {effect.Value} tokens)",
        CardEffectType.ScaleByComfort => $"+X comfort (X = {effect.Value})",
        _ => effect.Type.ToString()
    };
}
```

### 3.3 Action Button Previews

```html
<!-- SPEAK button hover preview -->
<div class="speak-preview">
  @if (SelectedCard != null)
  {
    <div class="selected-action">
      ✓ Play: @SelectedCard.Name (costs @SelectedCard.Weight weight)
    </div>
  }
  
  @{
    var exhaustingCards = GetFleetingCards().Where(c => c != SelectedCard);
    var criticalExhausts = exhaustingCards.Where(c => c.IsGoal);
  }
  
  @if (criticalExhausts.Any())
  {
    <div class="critical-warning">
      ⚠️ GOAL CARDS WILL EXHAUST - CONVERSATION WILL END!
      @foreach (var goal in criticalExhausts)
      {
        <div>• @goal.Name → CONVERSATION FAILS</div>
      }
    </div>
  }
  
  @if (exhaustingCards.Except(criticalExhausts).Any())
  {
    <div class="exhaust-list">
      Cards that will exhaust:
      @foreach (var card in exhaustingCards.Except(criticalExhausts))
      {
        <div>
          • @card.Name
          @if (card.ExhaustEffect != null)
          {
            <span>→ @DescribeEffect(card.ExhaustEffect)</span>
          }
        </div>
      }
    </div>
  }
</div>

<!-- LISTEN button hover preview -->
<div class="listen-preview">
  <div class="listen-effects">
    • Draw @GetCardDrawCount() cards
    • Refresh weight to @GetMaxWeight()
  </div>
  
  @{
    var exhaustingCards = GetOpportunityCards();
    var criticalExhausts = exhaustingCards.Where(c => c.IsGoal);
  }
  
  @if (criticalExhausts.Any())
  {
    <div class="critical-warning">
      ⚠️ GOAL CARDS WILL EXHAUST - CONVERSATION WILL END!
      @foreach (var goal in criticalExhausts)
      {
        <div>• @goal.Name → CONVERSATION FAILS</div>
      }
    </div>
  }
  
  @if (exhaustingCards.Except(criticalExhausts).Any())
  {
    <div class="exhaust-list">
      Cards that will exhaust:
      @foreach (var card in exhaustingCards.Except(criticalExhausts))
      {
        <div>
          • @card.Name
          @if (card.ExhaustEffect != null)
          {
            <span>→ @DescribeEffect(card.ExhaustEffect)</span>
          }
        </div>
      }
    </div>
  }
</div>
```

### 3.4 CSS Styling

```css
/* Property badges */
.property-badge {
    display: inline-block;
    padding: 2px 6px;
    border-radius: 3px;
    font-size: 11px;
    font-weight: bold;
    margin-right: 4px;
    text-transform: uppercase;
}

.property-badge.fleeting {
    background: #f4c430;
    color: #000;
}

.property-badge.opportunity {
    background: #ff8c00;
    color: #fff;
}

.property-badge.burden {
    background: #4a3a2a;
    color: #fff;
}

/* Card styling based on properties */
.card.has-fleeting {
    border-left: 4px solid #f4c430;
}

.card.has-opportunity {
    border-left: 4px solid #ff8c00;
}

/* Goal cards (both fleeting AND opportunity) */
.card.has-fleeting.has-opportunity {
    border: 3px solid #ff0000;
    background: linear-gradient(135deg, #fff5f5 0%, #ffe5e5 100%);
    animation: urgent-pulse 1.5s ease-in-out infinite;
}

.goal-warning {
    background: #ff0000;
    color: white;
    padding: 4px 8px;
    text-align: center;
    font-weight: bold;
    animation: blink 1s infinite;
}

/* Effect sections */
.card-effects {
    border-top: 1px solid #ddd;
    padding: 8px;
}

.effect {
    display: flex;
    align-items: center;
    padding: 4px 0;
    gap: 8px;
}

.effect.success {
    background: #e8f5e9;
    color: #2e7d32;
}

.effect.failure {
    background: #ffebee;
    color: #c62828;
}

.effect.exhaust {
    background: #f3e5f5;
    color: #6a1b9a;
}

.effect .chance {
    font-weight: bold;
    min-width: 40px;
}

.effect .label {
    font-weight: 600;
    min-width: 80px;
}

.effect .no-effect {
    opacity: 0.6;
    font-style: italic;
}

/* Action preview styling */
.speak-preview,
.listen-preview {
    background: #fff;
    border: 2px solid #333;
    border-radius: 4px;
    padding: 12px;
    box-shadow: 0 4px 8px rgba(0,0,0,0.2);
    max-width: 400px;
}

.critical-warning {
    background: #ff0000;
    color: white;
    padding: 8px;
    margin: 8px 0;
    border-radius: 4px;
    font-weight: bold;
}

.exhaust-list {
    background: #fff3cd;
    border: 1px solid #ffc107;
    padding: 8px;
    margin: 8px 0;
    border-radius: 4px;
}

/* Animations */
@keyframes urgent-pulse {
    0%, 100% { 
        box-shadow: 0 0 10px rgba(255,0,0,0.3); 
    }
    50% { 
        box-shadow: 0 0 20px rgba(255,0,0,0.6); 
    }
}

@keyframes blink {
    0%, 50%, 100% { opacity: 1; }
    25%, 75% { opacity: 0.5; }
}
```

## Phase 4: Testing Plan

### 4.1 Core Mechanics Tests
1. **Fleeting Only**: Card exhausts on SPEAK, not on LISTEN
2. **Opportunity Only**: Card exhausts on LISTEN, not on SPEAK
3. **Both Properties**: Card exhausts on EITHER action (goal behavior)
4. **Exhaust Effects**: Effects trigger when cards exhaust
5. **Conversation End**: Goal exhaust ends conversation

### 4.2 Edge Cases
1. **Multiple Goals**: All exhaust together
2. **Mixed Properties**: Each card exhausts appropriately
3. **No Exhaust Effect**: Cards vanish without effect
4. **Chain Reactions**: Exhaust effects that affect other cards

### 4.3 UI Tests
1. **Badge Display**: All properties show correctly
2. **Preview Accuracy**: Hover shows correct exhausts
3. **Critical Warnings**: Goals highlighted properly
4. **Effect Display**: All three effects visible and clear

## Phase 5: Migration Strategy

### 5.1 Data Migration
```csharp
// Convert old format to new
if (oldCard.IsFleeting || oldCard.Persistence == "Fleeting")
{
    newCard.Properties.Add(CardProperty.Fleeting);
}

if (oldCard.HasFinalWord)
{
    // Goal cards get both properties
    newCard.Properties.Add(CardProperty.Fleeting);
    newCard.Properties.Add(CardProperty.Opportunity);
    newCard.ExhaustEffect = new CardEffect
    {
        Type = CardEffectType.EndConversation,
        Value = "goal_exhausted"
    };
}

// Convert old effects to new three-effect system
newCard.SuccessEffect = new CardEffect
{
    Type = MapOldEffectType(oldCard.EffectType),
    Value = oldCard.EffectValue.ToString()
};
```

### 5.2 Backwards Compatibility
- Support reading old format during transition
- Write new format exclusively
- Provide migration tool for content creators

## Advantages of This Design

1. **No Special Cases**: Goals use normal mechanics (properties + effects)
2. **Extensible**: Easy to add new properties without breaking changes
3. **Clear Mental Model**: Properties determine when cards exhaust
4. **Transparent**: All effects visible before playing
5. **Emergent Complexity**: Simple rules create deep tactics
6. **Clean Code**: No `HasFinalWord` checks scattered through codebase
7. **JSON Friendly**: Arrays and objects instead of many booleans

## Example Scenarios

### Scenario 1: Goal Card Pressure
1. Player draws goal with `["Fleeting", "Opportunity"]`
2. Cannot SPEAK other cards (goal would exhaust → fail)
3. Cannot LISTEN for more cards (goal would exhaust → fail)
4. Must play goal immediately with current resources

### Scenario 2: Desperate Interrupt
1. In Desperate state (1 card draw, 3 weight)
2. Draw Opportunity "Interrupt" card
3. Choice: Risk play for atmosphere, or LISTEN and take exhaust penalty
4. Creates meaningful decision under pressure

### Scenario 3: Building to Climax
1. Draw high-weight Fleeting card early
2. Must decide: Play now with limited setup, or risk exhausting
3. If exhaust has "SetAtmosphere: Final", creates escalation

## Success Metrics

✅ All cards use properties list
✅ Goals create pressure through normal mechanics
✅ No special handling for "Final Word"
✅ UI shows all mechanical information
✅ Players understand consequences before acting
✅ Code is clean and maintainable
✅ System is extensible for future mechanics