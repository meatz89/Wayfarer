# Conversation Redesign Work Packets

## Overview
This document defines specific work packets for implementing the conversation redesign. Each packet is self-contained and can be assigned to a specialized agent. All agents MUST read all documentation in the root folder before starting work.

## Work Packet 1: Update Draw Counts Configuration

**Agent Type**: systems-architect
**Estimated Time**: 30 minutes
**Dependencies**: None

### Objective
Update the LISTEN action to draw 3-5 cards instead of 1-3 cards based on connection state.

### Required Changes

1. **Add to `src/Content/Core/game_rules.json`**:
```json
{
  "listenDrawCounts": {
    "Disconnected": 3,
    "Guarded": 4,
    "Neutral": 4,
    "Receptive": 5,
    "Trusting": 5
  }
}
```

2. **Update `src/Services/GameRules.cs`**:
- Add property: `public Dictionary<ConnectionState, int> ListenDrawCounts { get; set; }`

3. **Update `src/Content/GameRulesParser.cs`**:
- Parse listenDrawCounts from JSON
- Map to GameRules.ListenDrawCounts dictionary

4. **Update `src/Subsystems/Conversation/ConversationOrchestrator.cs`** (lines 372-376):
- Replace hardcoded draw logic with: `GameRules.ListenDrawCounts[connectionState]`

### Verification Criteria
- [ ] game_rules.json contains listenDrawCounts
- [ ] GameRules class has ListenDrawCounts property
- [ ] Parser correctly loads the values
- [ ] ConversationOrchestrator uses config values
- [ ] Each state draws correct number of cards in game

---

## Work Packet 2: Adjust Card Persistence Balance

**Agent Type**: content-integrator
**Estimated Time**: 30 minutes
**Dependencies**: None

### Objective
Change starter deck from 80% persistent to only 20% persistent cards.

### Required Changes

1. **Update `src/Content/Core/core_game_package.json`** starter deck:

**Keep as Thought (4 cards = 20%)**:
- "i_hear_you" (1 copy)
- "let_me_think" (1 copy)
- "active_listening" (1 copy)
- "deep_breath" (1 copy)

**Change to Impulse (10 cards = 50%)**:
- "tell_me_more" (2 copies)
- "how_can_i_assist" (2 copies)
- "trust_me_on_this" (2 copies)
- "everything_will_work_out" (2 copies)
- "bold_claim" (2 copies)

**Change to Opening (6 cards = 30%)**:
- "i_understand" (2 copies)
- "let_me_prepare" (2 copies)
- "we_can_figure_this_out" (2 copies)

### Verification Criteria
- [ ] Exactly 4 cards have persistence: "Thought"
- [ ] Exactly 10 cards have persistence: "Impulse"
- [ ] Exactly 6 cards have persistence: "Opening"
- [ ] Total is still 20 cards
- [ ] Game loads without errors

---

## Work Packet 3: Add NPC Progression Cards

**Agent Type**: narrative-designer
**Estimated Time**: 1 hour
**Dependencies**: None

### Objective
Create unique progression cards for each NPC that unlock at token thresholds.

### Required Changes

1. **Update each NPC in `src/Content/Core/core_game_package.json`**:

**Marcus (Commerce tokens)**:
```json
"progressionDeck": [
  {
    "id": "marcus_bargain",
    "description": "Marcus's Bargain - A deal you can't refuse",
    "minimumTokensRequired": 1,
    "focus": 2,
    "difficulty": "Hard",
    "persistence": "Thought",
    "successType": "Rapport",
    "failureType": "None",
    "exhaustType": "None",
    "connectionType": "Commerce"
  },
  {
    "id": "trade_knowledge",
    "description": "Trade Knowledge - Marcus shares market secrets",
    "minimumTokensRequired": 3,
    "focus": 3,
    "difficulty": "Medium",
    "persistence": "Impulse",
    "successType": "Threading",
    "failureType": "None",
    "exhaustType": "Threading",
    "connectionType": "Commerce"
  },
  {
    "id": "commercial_trust",
    "description": "Commercial Trust - Years of reliable business",
    "minimumTokensRequired": 6,
    "focus": 1,
    "difficulty": "Easy",
    "persistence": "Thought",
    "successType": "Atmospheric",
    "targetAtmosphere": "Focused",
    "failureType": "None",
    "exhaustType": "None",
    "connectionType": "Commerce"
  },
  {
    "id": "marcus_favor",
    "description": "Marcus's Favor - He owes you one",
    "minimumTokensRequired": 10,
    "focus": 4,
    "difficulty": "VeryHard",
    "persistence": "Thought",
    "successType": "Advancing",
    "failureType": "None",
    "exhaustType": "None",
    "connectionType": "Commerce"
  },
  {
    "id": "master_trader_secret",
    "description": "Master Trader's Secret - The ultimate negotiation",
    "minimumTokensRequired": 15,
    "focus": 5,
    "difficulty": "Hard",
    "persistence": "Thought",
    "successType": "Rapport",
    "failureType": "None",
    "exhaustType": "Regret",
    "connectionType": "Commerce"
  }
]
```

**Elena (Trust tokens)** - Similar structure with Trust-themed cards
**Other NPCs** - Create 5 unique cards each

### Verification Criteria
- [ ] Each NPC has exactly 5 progression cards
- [ ] Cards have thresholds at 1, 3, 6, 10, 15
- [ ] Each card has unique, NPC-specific flavor
- [ ] Cards match NPC personality and token type
- [ ] Cards load and unlock correctly in game

---

## Work Packet 4: Implement Personality Rule Enforcer

**Agent Type**: game-mechanics-designer
**Estimated Time**: 3 hours
**Dependencies**: Work Packet 1 must be complete

### Objective
Create a system that enforces personality-specific conversation rules.

### Required Files to Create

1. **Create `src/GameState/Enums/PersonalityModifierType.cs`**:
```csharp
public enum PersonalityModifierType
{
    None,
    AscendingFocusRequired,  // Proud
    RapportLossMultiplier,   // Devoted
    HighestFocusBonus,        // Mercantile
    RepeatFocusPenalty,       // Cunning
    RapportChangeCap          // Steadfast
}
```

2. **Create `src/GameState/PersonalityModifier.cs`**:
```csharp
public class PersonalityModifier
{
    public PersonalityModifierType Type { get; set; }
    public Dictionary<string, int> Parameters { get; set; } = new();
}
```

3. **Create `src/Subsystems/Conversation/PersonalityRuleEnforcer.cs`**:
- Track state between plays
- Validate legal plays
- Modify success rates
- Modify rapport changes
- Reset on LISTEN

### Integration Points

1. **Update `src/GameState/NPC.cs`**:
- Add: `public PersonalityModifier ConversationModifier { get; set; }`

2. **Update `src/Subsystems/Conversation/ConversationOrchestrator.cs`**:
- Create PersonalityRuleEnforcer at conversation start
- Check ValidatePlay() before allowing SPEAK
- Apply ModifySuccessRate() to calculations
- Apply ModifyRapportChange() to effects
- Call OnListen() during LISTEN action

### Verification Criteria
- [ ] Proud: Blocks non-ascending focus plays
- [ ] Devoted: Doubles negative rapport changes
- [ ] Mercantile: Adds 30% to highest focus card
- [ ] Cunning: Applies -2 rapport for repeat focus
- [ ] Steadfast: Caps all rapport changes at ±2
- [ ] Rules reset properly on LISTEN
- [ ] UI shows rule violations

---

## Work Packet 5: Implement Card XP/Leveling System

**Agent Type**: systems-architect
**Estimated Time**: 3 hours
**Dependencies**: Work Packets 1-4 should be complete

### Objective
Add experience and leveling system to conversation cards.

### Required Changes

1. **Update `src/GameState/CardInstance.cs`**:
```csharp
public int XP { get; set; }
public int Level => CalculateLevel(XP);
public bool IgnoresFailureListen { get; set; }

private int CalculateLevel(int xp)
{
    if (xp < 3) return 1;
    if (xp < 7) return 2;
    if (xp < 15) return 3;
    if (xp < 30) return 4;
    if (xp < 50) return 5;
    // Continue pattern...
}
```

2. **Create `src/GameState/CardLevelBonus.cs`**:
```csharp
public class CardLevelBonus
{
    public int? SuccessBonus { get; set; }
    public PersistenceType? AddPersistence { get; set; }
    public int? AddDrawOnSuccess { get; set; }
    public bool? IgnoreFailureListen { get; set; }
}
```

3. **Update `src/GameState/ConversationCard.cs`**:
- Add: `public Dictionary<int, CardLevelBonus> LevelBonuses { get; set; }`

4. **Update `src/Subsystems/Conversation/ConversationOrchestrator.cs`**:
- After successful SPEAK: `card.XP += 1;`
- Apply cumulative level bonuses to success rate
- Check IgnoresFailureListen before forcing LISTEN

5. **Update save/load system**:
- Ensure XP persists between sessions
- Handle missing XP in old saves (default to 0)

### Verification Criteria
- [ ] Cards gain 1 XP per successful play
- [ ] Level calculated correctly from XP
- [ ] Level 2 adds +10% success
- [ ] Level 3 adds persistence
- [ ] Level 4 adds +10% success
- [ ] Level 5 ignores forced LISTEN
- [ ] XP saves and loads correctly
- [ ] No level cap exists

---

## Work Packet 6: Update UI Components

**Agent Type**: wayfarer-design-auditor
**Estimated Time**: 2 hours
**Dependencies**: Work Packets 1-5 must be complete

### Objective
Update UI to display all new conversation features.

### Required Changes

1. **Update `src/Pages/Components/ConversationContent.razor`**:
- Show card XP as number (e.g., "XP: 12")
- Show card level (e.g., "Level 3")
- Display active personality rule
- Highlight illegal plays with red border
- Show modified success percentages

2. **Update `src/Pages/Components/NPCDeckViewer.razor`**:
- Show progression cards with lock icons
- Display token requirements
- Highlight unlocked cards

3. **Update conversation flow display**:
- Show correct draw counts on LISTEN button
- Display personality modifiers in effect
- Show XP gain animation on success

### Verification Criteria
- [ ] XP and level visible on cards
- [ ] Personality rules clearly displayed
- [ ] Illegal plays prevented with visual feedback
- [ ] Draw counts show correctly
- [ ] Progression cards show lock status
- [ ] All numbers update in real-time

---

## Verification Protocol

After each work packet is completed by an agent:

1. **Code Review**:
- Check all specified files were modified
- Verify no compilation errors
- Ensure no legacy code remains

2. **Functional Testing**:
- Run the game with `dotnet run`
- Test each feature works as specified
- Verify save/load compatibility

3. **Integration Testing**:
- Test features work together
- Check for regressions
- Verify UI updates correctly

4. **Documentation Update**:
- Mark completed items in CONVERSATION-REDESIGN-IMPLEMENTATION.md
- Update this work packet document
- Document any deviations or issues

## Agent Assignment Order

1. **First Wave** (can work in parallel):
   - systems-architect: Work Packet 1 (Draw Counts)
   - content-integrator: Work Packet 2 (Persistence Balance)
   - narrative-designer: Work Packet 3 (Progression Cards)

2. **Second Wave** (after first wave):
   - game-mechanics-designer: Work Packet 4 (Personality Rules)

3. **Third Wave** (after second wave):
   - systems-architect: Work Packet 5 (Card Leveling)

4. **Final Wave** (after all mechanics complete):
   - wayfarer-design-auditor: Work Packet 6 (UI Updates)

5. **Verification** (after all complete):
   - change-validator: Verify all changes work correctly

Each agent MUST read all documentation before starting and report completion status clearly.