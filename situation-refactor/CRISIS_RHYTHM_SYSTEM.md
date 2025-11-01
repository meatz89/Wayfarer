# Crisis Rhythm System

## Overview

The Crisis Rhythm System introduces **escalating narrative tension** through a predictable pattern: regular situations build resources and preparation, then a Crisis situation tests whether the player prepared correctly.

**Core Concept:** Every Scene follows the rhythm **Build → Build → Build → TEST**, creating strategic depth where player choices during preparation determine the cost of the final crisis resolution.

## Design Philosophy

### Inspired by Sir Brante's Crisis Accumulation

The system implements the **"suffocation engine"** pattern from Sir Brante, where:
- **Early choices feel manageable** - modest costs, visible stat gains
- **Crisis moments expose preparation quality** - high stat requirements or expensive alternatives
- **Failure has permanent consequences** - scenes lock, NPCs turn away, locations bar access

### Divergence from 80 Days' Steady State

Unlike 80 Days' constant resource tension with full recovery options:
- **Wayfarer accumulates pressure** - preparation choices compound toward crisis
- **Crisis moments are binary tests** - prepared correctly = easy, unprepared = expensive/risky
- **Consequences persist** - failed crises affect future options

## Architecture

### SituationType Enum

```csharp
public enum SituationType
{
    Normal,   // Standard situation with normal costs/requirements
    Crisis    // High-stakes test of player preparation
}
```

**Location:** `src/GameState/Enums/SituationType.cs`

### Entity Properties

**Situation.cs:**
```csharp
public SituationType Type { get; set; } = SituationType.Normal;
```

**SituationTemplate.cs:**
```csharp
public SituationType Type { get; init; } = SituationType.Normal;
```

**SituationTemplateDTO.cs:**
```csharp
public string Type { get; set; }  // "Normal" or "Crisis"
```

### Parsing & Instantiation

**SceneTemplateParser.cs:**
- Parses `Type` field from JSON
- Validates enum value ("Normal" or "Crisis")
- Defaults to `Normal` if field missing (backward compatibility)
- Throws clear error if invalid value

**SceneInstantiator.cs:**
- Copies `Type` from template to runtime Situation instance
- Ensures semantic marking persists through entire lifecycle

## Usage Patterns

### The 3-5-1 Scene Structure

**3-5 Regular Situations** (Build phase):
- Present 2-4 choices each
- Visible costs (energy, coins, time)
- Build stats incrementally (+1 Authority, +1 Diplomacy, etc.)
- Gather resources and information
- Feel manageable, encourage experimentation

**1 Crisis Situation** (Test phase):
- High stat requirement (Authority 4+, Diplomacy 4+, etc.)
- Expensive alternative (20+ coins)
- Risky gamble option (Physical challenge)
- Failure option with permanent consequence

### JSON Authoring Example

```json
{
  "id": "merchant_guild_dispute",
  "archetype": "Linear",
  "situationTemplates": [
    {
      "id": "observe_argument",
      "type": "Normal",
      "narrativeTemplate": "You witness a heated argument between merchants...",
      "choiceTemplates": [
        {
          "actionTextTemplate": "Listen carefully",
          "costTemplate": { "timeSegments": 1 },
          "rewardTemplate": { "statChanges": { "Insight": 1 } }
        },
        {
          "actionTextTemplate": "Move closer",
          "costTemplate": { "energy": 2 },
          "rewardTemplate": { "statChanges": { "Authority": 1 } }
        }
      ]
    },
    {
      "id": "gather_information",
      "type": "Normal",
      "narrativeTemplate": "The dispute involves a shipment contract...",
      "choiceTemplates": [
        {
          "actionTextTemplate": "Ask merchants",
          "costTemplate": { "energy": 2 },
          "rewardTemplate": { "statChanges": { "Diplomacy": 1 } }
        },
        {
          "actionTextTemplate": "Investigate documents",
          "costTemplate": { "timeSegments": 1 },
          "rewardTemplate": { "statChanges": { "Insight": 1 } }
        }
      ]
    },
    {
      "id": "choose_side",
      "type": "Normal",
      "narrativeTemplate": "Both parties notice your interest...",
      "choiceTemplates": [
        {
          "actionTextTemplate": "Support the seller",
          "rewardTemplate": { "statChanges": { "Rapport": 1 } }
        },
        {
          "actionTextTemplate": "Support the buyer",
          "rewardTemplate": { "statChanges": { "Authority": 1 } }
        }
      ]
    },
    {
      "id": "guild_confrontation",
      "type": "Crisis",
      "narrativeTemplate": "The guild master steps forward, demanding you resolve this NOW.",
      "choiceTemplates": [
        {
          "id": "assert_authority",
          "actionTextTemplate": "Assert your authority and make a ruling",
          "requirementFormula": {
            "logicType": "Simple",
            "numericRequirement": {
              "requirementType": "PlayerStat",
              "context": "Authority",
              "thresholdValue": 4,
              "comparisonOperator": "GreaterThanOrEqual"
            }
          },
          "costTemplate": { "energy": 2 },
          "rewardTemplate": {
            "statChanges": { "Reputation": 1 }
          }
        },
        {
          "id": "bribe_guild",
          "actionTextTemplate": "Pay off both parties to end the dispute",
          "costTemplate": { "coins": 20 },
          "rewardTemplate": {
            "statChanges": { "Reputation": -1 }
          }
        },
        {
          "id": "physical_threat",
          "actionTextTemplate": "Threaten them into submission",
          "actionType": "StartChallenge",
          "challengeType": "Physical",
          "costTemplate": { "energy": 3 }
        },
        {
          "id": "flee_scene",
          "actionTextTemplate": "Walk away from the confrontation",
          "rewardTemplate": {
            "sceneEnds": true,
            "statChanges": { "Reputation": -2 },
            "npcBondChanges": [
              { "npcId": "guild_master", "bondChange": -3 }
            ]
          }
        }
      ]
    }
  ]
}
```

## Player Experience Flow

### Strategic Forecasting

**Scene shows Crisis indicator before engagement:**

```
📍 Merchant Quarter

Active Scenes:
- [💬 Normal] "Witness Dispute" (Situation 1/4)
- [⚠️ Crisis] "Guild Confrontation" (Situation 4/4) ← CRISIS READY!
```

**Player strategic thinking:**
- "I'm at situation 3/4, crisis coming next"
- "This scene needs Authority or Diplomacy"
- "Should I build stats now or save resources?"

### Preparation Phase

**Situation 1: "Observe argument"**
- Player chooses "Move closer" (+1 Authority, 2 energy)
- Authority now at 3

**Situation 2: "Gather information"**
- Player chooses "Ask merchants" (+1 Diplomacy, 2 energy)
- Diplomacy now at 3

**Situation 3: "Choose side"**
- Player chooses "Support buyer" (+1 Authority)
- Authority now at 4 ✓

### Crisis Resolution

**Situation 4: "Guild confrontation" (CRISIS)**

Player sees choices:
1. ✅ **"Assert authority" (Authority 4+)** - Player has 4, costs only 2 energy
2. **"Pay off guild" (20 coins)** - Expensive alternative
3. **"Threaten them" (Physical challenge)** - Risky gamble
4. **"Walk away" (Scene fails)** - Permanent consequences

**Outcome:** Player prepared correctly, crisis costs only 2 energy instead of 20 coins or scene failure.

### Unprepared Example

**If player had Authority 3 instead of 4:**

Choices available:
1. ❌ **"Assert authority" (Authority 4+)** - LOCKED (requirement not met)
2. **"Pay off guild" (20 coins)** - Must spend most delivery earnings
3. **"Threaten them" (Physical challenge)** - Risk losing even more
4. **"Walk away" (Scene fails)** - Lose scene access, NPC bond -3, Reputation -2

**Decision:** Pay 20 coins (expensive but guaranteed) or risk Physical challenge (might lose more).

## Implementation Details

### Backward Compatibility

✅ **100% compatible with existing content**
- All existing situations default to `Normal`
- No changes required to current JSON
- Type field is optional
- Parser handles missing field gracefully

### Validation

**Parser validates Type values:**
```csharp
if (!Enum.TryParse<SituationType>(dto.Type, true, out situationType))
{
    throw new InvalidDataException(
        $"SituationTemplate '{dto.Id}' has invalid Type value: '{dto.Type}'. " +
        "Must be 'Normal' or 'Crisis'."
    );
}
```

**Clear error messages:**
- Tells author exactly which situation has invalid type
- Lists valid values
- Prevents runtime type mismatches

### Data Flow

```
JSON ("type": "Crisis")
  ↓
SituationTemplateDTO.Type (string)
  ↓
SceneTemplateParser validates + parses
  ↓
SituationTemplate.Type (SituationType enum)
  ↓
SceneInstantiator copies to runtime instance
  ↓
Situation.Type (SituationType enum)
  ↓
UI detects Crisis situations for visual treatment
```

## Future Enhancements

### Phase 3: UI Indicators

**Visual treatment for Crisis situations:**
- Red border or highlight
- ⚠️ Warning icon
- "CRISIS MOMENT" label
- Tension music/sound cues
- Animated entrance

**CSS example:**
```css
.situation-card.crisis {
    border: 2px solid var(--danger-color);
    box-shadow: 0 0 10px rgba(220, 53, 69, 0.3);
    animation: pulse-danger 2s infinite;
}
```

### Phase 4: Domain Forecasting

**Extend to show Crisis domain:**
```json
{
  "type": "Crisis",
  "domain": "Social",  // Requires Social stats (Diplomacy/Rapport)
  "narrativeTemplate": "..."
}
```

**Player sees:**
- "Crisis (Social Domain)" - knows to build Diplomacy/Rapport
- Strategic preparation without spoiling exact requirement

### Phase 5: Consequence Tiers

**Structured failure outcomes:**
```json
{
  "type": "Crisis",
  "failureConsequences": {
    "tier": "LocationAccess",
    "sceneLocked": true,
    "locationAccessDays": 2,
    "npcBondDamage": -3,
    "reputationDamage": -2
  }
}
```

**Failure tiers:**
1. **Soft Lock** - Miss one option, alternatives remain
2. **Scene Lock** - Scene removed, cannot retry
3. **Location Access** - Barred from location for X days
4. **NPC Relationship** - Bond permanently damaged
5. **Reputation Cascade** - Other NPCs react negatively

### Phase 6: Dynamic Crisis Timing

**Variable crisis position:**
```json
{
  "crisisPosition": "Dynamic",  // Crisis fires at variable situation count
  "minSituations": 3,
  "maxSituations": 5
}
```

**Creates uncertainty:**
- "Crisis might be next... or maybe two more situations"
- Player must stay prepared throughout scene
- Increases tension

## Design Principles

### 1. Perfect Information on Costs

**Always show before selection:**
- Stat requirements visible ("Authority 4+")
- Resource costs transparent ("20 coins")
- Locked choices shown with requirements

**Never show:**
- When crisis will hit (situation number hidden)
- Alternative crisis paths until crisis presents
- Exact consequence details

### 2. No Randomness in Requirements

**Pure threshold checks:**
- Have Authority 4+ = pass
- Have Authority 3 = fail
- No dice rolls, no chance

**Why:** Player preparation has deterministic outcome. Strategic planning rewarded, not luck.

### 3. Every Regular Choice Matters

**Framing effect:**
- Without crisis: "Free choice (0 cost) is optimal"
- With crisis coming: "Stat-building choice is strategic investment"

**Example:**
- Regular Situation: "Help merchant?"
  - Choice A: Help (2 energy, +1 Diplomacy)
  - Choice B: Refuse (0 cost, 0 stats)
- Player thinks: "Crisis needs Diplomacy, invest now while cheap"

### 4. Impossible Optimization

**No "perfect" path:**
- Situations offer choices between different stats
- Can't maximize all stats equally
- Must choose which crises to prepare for

**Strategic depth:**
- "Should I build Authority for Guard scenes or Diplomacy for Merchant scenes?"
- "I can afford to fail social crises but not physical ones"

## Verisimilitude

### Narrative Coherence

**Crisis situations must make sense:**
- Guild confrontation demands authority or payment (realistic)
- Guard challenge requires combat or submission (makes sense)
- Scholar puzzle needs insight or time investment (logical)

**Anti-pattern:**
- "Random stat check" - no narrative reason for requirement
- "Sudden violence" - crisis doesn't flow from preparation
- "Arbitrary consequence" - punishment doesn't match failure

### Player Mental State

**What player experiences:**
1. **Situation 1-3:** "I'm gathering information, building relationships"
2. **Approaching Crisis:** "Things are escalating, I need to be ready"
3. **Crisis Moment:** "This is it - do I have what it takes?"
4. **Resolution:** "My preparation paid off" OR "I should have invested more"

**Emotional arc:**
- Permissive → Tense → Critical → Relief/Regret

## Testing Checklist

### Content Authoring

- [ ] Mark final situation in scene as `"type": "Crisis"`
- [ ] Crisis has stat-gated choice (threshold 4+)
- [ ] Crisis has expensive alternative (15-25 coins)
- [ ] Crisis has risky gamble option
- [ ] Crisis has failure option with consequences
- [ ] Regular situations build relevant stats
- [ ] Scene has 3-5 situations total

### Balance Validation

- [ ] Stat threshold achievable through preparation (3 situations × +1 = 3, need 4 means player must focus)
- [ ] Expensive alternative is genuinely costly but not impossible (20 coins ≈ 1-2 deliveries)
- [ ] Failure consequence is permanent and meaningful
- [ ] Prepared path is clearly easier than alternatives

### Player Experience

- [ ] Crisis feels earned (narrative escalation)
- [ ] Preparation phase offers real choices
- [ ] Success feels like reward for planning
- [ ] Failure feels like learning opportunity

## Integration with Existing Systems

### Scenes

**No changes required:**
- Scenes continue to contain 3-5 Situations
- Spawn patterns unchanged
- Archetype system unchanged

**Crisis marking is pure semantic addition.**

### Situations

**No mechanical changes:**
- Requirements system unchanged
- Costs system unchanged
- Rewards system unchanged

**Type property adds meaning, not mechanics.**

### Challenges

**Crisis situations can trigger challenges:**
```json
{
  "type": "Crisis",
  "choiceTemplates": [
    {
      "actionType": "StartChallenge",
      "challengeType": "Physical"
    }
  ]
}
```

**Integration:** Risky gamble option in crisis can be Physical/Social/Mental challenge.

## Summary

The Crisis Rhythm System adds **strategic tension through preparation-test cycles** without adding mechanical complexity. It uses:

- **One new enum** (SituationType)
- **Three property additions** (entities + DTO)
- **Minimal parser changes** (parse + copy Type)
- **Zero breaking changes** (backward compatible)

**Result:** Board game elegance with psychological depth. Every regular choice matters because crisis is coming. Preparation is rewarded, lack of preparation is expensive.

**"Build → Build → Build → TEST"** - The rhythm that transforms tactical choices into strategic planning.
