# Player Stats System - Detailed Implementation Plan

## Executive Summary
Transform the current card XP system into a player stat-based progression where cards are bound to specific stats, and playing cards grants XP to those stats instead of individual cards. This creates character progression through practice of different conversational approaches.

## Core Design Philosophy
- **Stats as Character**: Player's 5 core stats represent their conversational and problem-solving approaches
- **Cards as Tools**: Cards are tools that become more effective as the bound stat improves
- **Practice Makes Perfect**: Playing any card (success or failure) grants XP to its bound stat
- **Universal Application**: Stats affect conversations, investigations, and travel

## The Five Core Stats

### 1. Insight (Analytical Intelligence)
- **Card Examples**: "Let me analyze this", "I notice a pattern", "Consider the evidence"
- **Investigation Bonus**: Systematic Observation (+1 familiarity at level 2+)
- **Travel Access**: Scholar shortcuts, library passages, observatory routes
- **Level 3 Effect**: Cards gain Thought persistence
- **Level 5 Effect**: Cards never force LISTEN on failure

### 2. Rapport (Emotional Intelligence)
- **Card Examples**: "I understand", "Let me help", "We're in this together"
- **Investigation Bonus**: Local Inquiry (learn NPC observation preferences at level 2+)
- **Travel Access**: Local favors, community paths, friendship routes
- **Level 3 Effect**: Cards gain Thought persistence
- **Level 5 Effect**: Cards never force LISTEN on failure

### 3. Authority (Leadership & Persuasion)
- **Card Examples**: "Listen carefully", "This is how it works", "You will comply"
- **Investigation Bonus**: Demand Access (force restricted spots at level 2+)
- **Travel Access**: Noble gates, checkpoint bypasses, command routes
- **Level 3 Effect**: Cards gain Thought persistence
- **Level 5 Effect**: Cards never force LISTEN on failure

### 4. Commerce (Negotiation & Trade)
- **Card Examples**: "What's in it for you", "Let's make a deal", "Consider the profit"
- **Investigation Bonus**: Purchase Information (pay coins for instant familiarity at level 2+)
- **Travel Access**: Merchant caravans, trade routes, market passages
- **Level 3 Effect**: Cards gain Thought persistence
- **Level 5 Effect**: Cards never force LISTEN on failure

### 5. Cunning (Subtlety & Indirection)
- **Card Examples**: "Perhaps you misunderstood", "There's another angle", "What they don't know"
- **Investigation Bonus**: Covert Search (investigate without alerts at level 2+)
- **Travel Access**: Shadow paths, hidden routes, thief passages
- **Level 3 Effect**: Cards gain Thought persistence
- **Level 5 Effect**: Cards never force LISTEN on failure

## Progression Mechanics

### XP Thresholds
- **Level 1→2**: 10 XP
- **Level 2→3**: 25 XP
- **Level 3→4**: 50 XP
- **Level 4→5**: 100 XP

### Success Rate Bonuses
- **Level 1**: Base card percentages
- **Level 2**: +5% success to all cards of this stat
- **Level 3**: +10% success + special effect
- **Level 4**: +15% success
- **Level 5**: +20% success + mastery effect

### XP Gain Scaling
- **Stranger Level 1**: 1 XP per card played
- **Stranger Level 2**: 2 XP per card played
- **Stranger Level 3**: 3 XP per card played
- **Named NPC Easy**: 1 XP per card played
- **Named NPC Medium**: 2 XP per card played
- **Named NPC Hard**: 3 XP per card played

## Implementation Pipeline

### Phase 1: JSON Content (Foundation)

#### 1.1 Player Stats Configuration
**File**: `/mnt/c/git/wayfarer/src/Content/Core/player_stats_config.json`
```json
{
  "packageId": "player_stats_001",
  "metadata": {
    "name": "Player Stats System",
    "version": "1.0.0"
  },
  "stats": [
    {
      "id": "insight",
      "name": "Insight",
      "description": "Analytical intelligence and observation",
      "conversationBenefit": "Better success with analytical cards",
      "investigationUnlock": "Systematic Observation at level 2",
      "travelUnlock": "Scholar shortcuts at level 2"
    }
    // ... other stats
  ],
  "progression": {
    "xpThresholds": [10, 25, 50, 100],
    "levelBonuses": [
      {"level": 2, "successBonus": 5, "description": "+5% success rate"},
      {"level": 3, "successBonus": 10, "effect": "gains_thought_persistence"},
      {"level": 4, "successBonus": 15, "description": "+15% success rate"},
      {"level": 5, "successBonus": 20, "effect": "ignores_failure_listen"}
    ]
  }
}
```

#### 1.2 Update Existing Cards
**File**: `/mnt/c/git/wayfarer/src/Content/Core/cards_and_decks_package.json`
Each card needs:
- `"boundStat": "rapport"` (or appropriate stat)
- Cards should be distributed roughly evenly across stats
- Starting deck (20 cards) should have 4 cards per stat

#### 1.3 Stranger NPCs Package
**File**: `/mnt/c/git/wayfarer/src/Content/Core/strangers_package.json`
```json
{
  "packageId": "strangers_001",
  "strangers": [
    {
      "id": "market_vendor_morning",
      "name": "Fresh Bread Vendor",
      "level": 1,
      "personality": "Steadfast",
      "locationId": "market_square",
      "timeBlock": "morning",
      "conversationTypes": [
        {
          "type": "friendly_chat",
          "rapportThresholds": [5, 10, 15],
          "rewards": [
            {"coins": 2},
            {"coins": 4, "item": "bread"},
            {"coins": 6, "item": "medicine"}
          ]
        }
      ]
    }
  ]
}
```

#### 1.4 Travel Path Updates
**File**: `/mnt/c/git/wayfarer/src/Content/Core/travel_package.json`
Add to existing paths:
```json
{
  "id": "scholar_shortcut",
  "statRequirements": {"insight": 2},
  "narrativeText": "Only those who understand the city's layout can navigate this route"
}
```

### Phase 2: DTOs (Data Transfer Objects)

#### 2.1 Core DTOs
**New File**: `/mnt/c/git/wayfarer/src/Content/DTOs/PlayerStatDTO.cs`
```csharp
public class PlayerStatDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ConversationBenefit { get; set; }
    public string InvestigationUnlock { get; set; }
    public string TravelUnlock { get; set; }
}

public class StatProgressionDTO
{
    public List<int> XpThresholds { get; set; }
    public List<StatLevelBonusDTO> LevelBonuses { get; set; }
}

public class StatLevelBonusDTO
{
    public int Level { get; set; }
    public int SuccessBonus { get; set; }
    public string Effect { get; set; }
    public string Description { get; set; }
}
```

#### 2.2 Stranger DTO
**New File**: `/mnt/c/git/wayfarer/src/Content/DTOs/StrangerNPCDTO.cs`
```csharp
public class StrangerNPCDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public string Personality { get; set; }
    public string LocationId { get; set; }
    public string TimeBlock { get; set; }
    public List<StrangerConversationDTO> ConversationTypes { get; set; }
}

public class StrangerConversationDTO
{
    public string Type { get; set; }
    public List<int> RapportThresholds { get; set; }
    public List<StrangerRewardDTO> Rewards { get; set; }
}
```

#### 2.3 Update Existing DTOs
**File**: `/mnt/c/git/wayfarer/src/Content/DTOs/ConversationCardDTO.cs`
- Add: `public string BoundStat { get; set; }`

**File**: `/mnt/c/git/wayfarer/src/Content/DTOs/PathCardDTO.cs`
- Add: `public Dictionary<string, int> StatRequirements { get; set; }`

### Phase 3: Parsers

#### 3.1 Stat Parser
**New File**: `/mnt/c/git/wayfarer/src/Content/PlayerStatParser.cs`
```csharp
public class PlayerStatParser : BaseValidator
{
    public (List<PlayerStatDefinition> stats, StatProgression progression) ParseStatsPackage(JsonElement root)
    {
        // Parse stat definitions
        // Parse progression rules
        // Validate all references
    }
}
```

#### 3.2 Stranger Parser
**New File**: `/mnt/c/git/wayfarer/src/Content/StrangerParser.cs`
```csharp
public class StrangerParser : BaseValidator
{
    public List<StrangerNPC> ParseStrangers(JsonElement root)
    {
        // Parse stranger definitions
        // Validate locations exist
        // Validate time blocks
        // Create StrangerNPC entities
    }
}
```

#### 3.3 Update Card Parser
**File**: `/mnt/c/git/wayfarer/src/Content/ConversationCardParser.cs`
- Parse `boundStat` field
- Validate stat exists
- Map string to PlayerStat enum

### Phase 4: Entity Layer

#### 4.1 PlayerStat Enum
**File**: `/mnt/c/git/wayfarer/src/GameState/Enums/PlayerStat.cs`
```csharp
public enum PlayerStat
{
    Insight,
    Rapport,
    Authority,
    Commerce,
    Cunning
}
```

#### 4.2 Replace PlayerSkills with PlayerStats
**File**: `/mnt/c/git/wayfarer/src/GameState/PlayerStats.cs`
```csharp
public class StatProgress
{
    public int Level { get; set; } = 1;
    public int XP { get; set; } = 0;

    public int GetXPToNextLevel()
    {
        return Level switch
        {
            1 => 10,
            2 => 25,
            3 => 50,
            4 => 100,
            _ => int.MaxValue
        };
    }
}

public class PlayerStats
{
    private Dictionary<PlayerStat, StatProgress> stats = new();

    public PlayerStats()
    {
        foreach (PlayerStat stat in Enum.GetValues<PlayerStat>())
        {
            stats[stat] = new StatProgress();
        }
    }

    public int GetLevel(PlayerStat stat) => stats[stat].Level;
    public int GetXP(PlayerStat stat) => stats[stat].XP;

    public void AddXP(PlayerStat stat, int amount)
    {
        var progress = stats[stat];
        progress.XP += amount;

        // Check for level up
        while (progress.XP >= progress.GetXPToNextLevel() && progress.Level < 5)
        {
            progress.XP -= progress.GetXPToNextLevel();
            progress.Level++;
        }
    }

    public int GetSuccessBonus(PlayerStat stat)
    {
        return (stats[stat].Level - 1) * 5;
    }

    public bool HasPersistenceBonus(PlayerStat stat)
    {
        return stats[stat].Level >= 3;
    }

    public bool IgnoresFailureListen(PlayerStat stat)
    {
        return stats[stat].Level >= 5;
    }
}
```

#### 4.3 Update ConversationCard
**File**: `/mnt/c/git/wayfarer/src/GameState/ConversationCard.cs`
- Add: `public PlayerStat BoundStat { get; init; }`

#### 4.4 Update CardInstance
**File**: `/mnt/c/git/wayfarer/src/GameState/CardInstance.cs`
- Remove: XP and Level properties
- Add: `GetEffectiveLevel(PlayerStats stats)` method
- Update: Success calculation to use player stats

#### 4.5 StrangerNPC Entity
**New File**: `/mnt/c/git/wayfarer/src/GameState/StrangerNPC.cs`
```csharp
public class StrangerNPC
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Level { get; set; } // 1-3, affects XP multiplier
    public PersonalityType Personality { get; set; }
    public string LocationId { get; set; }
    public TimeBlock AvailableTime { get; set; }
    public Dictionary<string, StrangerConversation> ConversationTypes { get; set; }
    public bool HasBeenTalkedTo { get; set; } // Resets per time block
}
```

### Phase 5: Mechanics Layer

#### 5.1 ConversationOrchestrator Changes
**File**: `/mnt/c/git/wayfarer/src/Subsystems/Conversation/ConversationOrchestrator.cs`
Line ~252, after card play:
```csharp
// Grant XP to stat instead of card
if (session.IsStrangerConversation)
{
    int xpAmount = session.StrangerLevel; // 1-3
    player.Stats.AddXP(selectedCard.Template.BoundStat, xpAmount);
}
else
{
    int xpAmount = 1; // Or based on conversation difficulty
    player.Stats.AddXP(selectedCard.Template.BoundStat, xpAmount);
}
```

#### 5.2 Investigation Approaches
**File**: `/mnt/c/git/wayfarer/src/Subsystems/Location/LocationFacade.cs`
```csharp
public enum InvestigationApproach
{
    Standard,
    Systematic,    // Insight 2+: +1 familiarity
    LocalInquiry,  // Rapport 2+: Learn NPC preferences
    DemandAccess,  // Authority 2+: Force restricted spots
    PurchaseInfo,  // Commerce 2+: Pay for familiarity
    CovertSearch   // Cunning 2+: No alerts
}

public List<InvestigationApproach> GetAvailableApproaches(Player player)
{
    var approaches = new List<InvestigationApproach> { InvestigationApproach.Standard };

    if (player.Stats.GetLevel(PlayerStat.Insight) >= 2)
        approaches.Add(InvestigationApproach.Systematic);
    // ... etc

    return approaches;
}
```

#### 5.3 Travel Path Validation
**File**: `/mnt/c/git/wayfarer/src/Subsystems/Travel/TravelFacade.cs`
Add stat checking:
```csharp
public bool CanUsePath(PathCard path, Player player)
{
    // Check stat requirements
    if (path.StatRequirements != null)
    {
        foreach (var req in path.StatRequirements)
        {
            if (player.Stats.GetLevel(req.Key) < req.Value)
                return false;
        }
    }
    // ... existing weight and stamina checks
}
```

### Phase 6: UI Components

#### 6.1 Player Stats Display
**New File**: `/mnt/c/git/wayfarer/src/Pages/Components/PlayerStatsDisplay.razor`
```razor
@inject GameFacade GameFacade

<div class="player-stats-panel">
    <h3>Character Stats</h3>
    @foreach (var stat in CurrentStats)
    {
        <div class="stat-row">
            <div class="stat-header">
                <span class="stat-icon @GetStatIconClass(stat.Type)"></span>
                <span class="stat-name">@stat.Name</span>
                <span class="stat-level">Lvl @stat.Level</span>
            </div>
            <div class="xp-bar">
                <div class="xp-fill" style="width: @stat.ProgressPercent%">
                    <span class="xp-text">@stat.XP / @stat.XPToNext</span>
                </div>
            </div>
            @if (stat.Level >= 2)
            {
                <div class="stat-bonuses">
                    <span>+@((stat.Level - 1) * 5)% success</span>
                    @if (stat.Level >= 3)
                    {
                        <span>Cards gain persistence</span>
                    }
                    @if (stat.Level >= 5)
                    {
                        <span>Never forced to LISTEN</span>
                    }
                </div>
            }
        </div>
    }
</div>
```

#### 6.2 Card Display Updates
**File**: `/mnt/c/git/wayfarer/src/Pages/Components/CardDisplay.razor`
Add stat indicator:
```razor
<div class="card-stat-indicator @GetStatClass(Card.BoundStat)">
    <span class="stat-icon"></span>
    <span class="stat-bonus">+@GetStatBonus()%</span>
</div>
```

#### 6.3 Investigation UI
**File**: `/mnt/c/git/wayfarer/src/Pages/Components/LocationContent.razor`
Add approach selection:
```razor
@if (ShowInvestigationOptions)
{
    <div class="investigation-approaches">
        @foreach (var approach in AvailableApproaches)
        {
            <button class="approach-btn @GetApproachClass(approach)"
                    @onclick="() => InvestigateWithApproach(approach)"
                    disabled="@(!CanUseApproach(approach))">
                <span class="approach-name">@approach</span>
                <span class="approach-desc">@GetApproachDescription(approach)</span>
                @if (!CanUseApproach(approach))
                {
                    <span class="approach-locked">Requires @GetRequirement(approach)</span>
                }
            </button>
        }
    </div>
}
```

### Phase 7: CSS Styling

#### 7.1 Stats Panel
**New File**: `/mnt/c/git/wayfarer/src/wwwroot/css/player-stats.css`
```css
.player-stats-panel {
    background: var(--panel-bg);
    border: 1px solid var(--border-color);
    padding: 1rem;
    border-radius: 8px;
}

.stat-row {
    margin-bottom: 1rem;
}

.stat-header {
    display: flex;
    align-items: center;
    margin-bottom: 0.5rem;
}

.stat-icon {
    width: 24px;
    height: 24px;
    margin-right: 0.5rem;
}

.stat-icon.insight { background-image: url('/icons/insight.svg'); }
.stat-icon.rapport { background-image: url('/icons/rapport.svg'); }
.stat-icon.authority { background-image: url('/icons/authority.svg'); }
.stat-icon.commerce { background-image: url('/icons/commerce.svg'); }
.stat-icon.cunning { background-image: url('/icons/cunning.svg'); }

.xp-bar {
    height: 20px;
    background: var(--xp-bar-bg);
    border-radius: 10px;
    overflow: hidden;
}

.xp-fill {
    height: 100%;
    background: linear-gradient(90deg, var(--xp-color-start), var(--xp-color-end));
    transition: width 0.3s ease;
    display: flex;
    align-items: center;
    justify-content: center;
}

.stat-bonuses {
    font-size: 0.85rem;
    color: var(--bonus-text-color);
    margin-top: 0.25rem;
}
```

## Testing Strategy

### Unit Tests Required
1. Stat XP accumulation and leveling
2. Success bonus calculations
3. Persistence and failure-ignore effects
4. Stranger refresh on time block change
5. Investigation approach availability

### Integration Tests Required
1. Card play → stat XP flow
2. Stat requirements for paths
3. Investigation with different approaches
4. Stranger conversation rewards

### E2E Tests Required
1. Complete conversation with visible stat gains
2. Level up during conversation
3. Unlock new investigation approach
4. Access stat-gated travel path
5. Stranger encounter with scaled XP

## Migration Notes

### From Current System
1. Remove XP from CardInstance
2. Initialize all players with level 1 in all stats
3. Convert existing cards to have boundStat
4. Preserve token system (unchanged)
5. Preserve queue system (unchanged)

### Backwards Compatibility
- Save games will need migration script
- Card XP converts to stat XP (divide by number of cards in that stat)
- Skill levels map to closest stat

## Balance Considerations

### XP Economy
- Average conversation: 10-15 cards played
- At stranger level 1: 10-15 XP gained
- Level 2 requires 10 XP: ~1 conversation
- Level 3 requires 25 XP: ~2-3 conversations
- Level 4 requires 50 XP: ~4-5 conversations
- Level 5 requires 100 XP: ~8-10 conversations

### Stat Distribution
- Starting deck: 4 cards per stat
- Total cards by mid-game: ~40 cards
- Aim for roughly even distribution
- Player specialization through play patterns

### Stranger Availability
- Each location: 2-3 strangers
- Each time block: Different strangers
- Total strangers: ~30-40
- Refresh rate: Per time block
- Prevents infinite grinding

## Success Metrics
1. Players develop distinct stat profiles
2. Stats meaningfully affect all three game loops
3. Progression feels natural and rewarding
4. No stat becomes mandatory
5. Multiple viable builds exist