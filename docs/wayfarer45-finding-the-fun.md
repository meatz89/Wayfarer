# Refined Resource Economy & Reward Structure for Wayfarer

After thoroughly examining the system, I've refined the core resource economy to create tight, meaningful progression loops without unnecessary complexity.

## Reward Structure Analysis

### Core Resource Types
1. **Action Resources**
   - Time Blocks (Morning, Afternoon, Evening, Night)
   - Action Points (18 per day, one for each waking hour)
   - Cards (Physical, Intellectual, Social) (hand limit: 4)
   - Energy (limits Physical cards)
   - Concentration (limits Intellectual cards)
   - Silver (universal currency)

2. **Progression Resources**
   - Reputation (0-100) - Primary measure of social standing
   - Insight Points - Used for Chronicle advancements
   - Commission Progress - Steps toward commission completion

### Resource Loops That Create Tension

The system creates three interconnected resource loops:

**Basic Survival Loop**
- Spend Cards → Complete Actions → Earn Silver → Pay for Lodging → Refresh Cards

**Progression Loop**
- Complete Commissions → Earn Reputation → Access Better Commissions → Earn More Resources

**Development Loop**
- Earn Insight Points → Improve Skills → Enhance Card Effectiveness → Handle Harder Commissions

## Action Reward Refinement

### Repeatable Actions (Always Available)
**Physical Approaches**
- **Primary Reward**: Silver (2-3)
- **Secondary Reward**: None or minimal Reputation (0-1)
- **NEVER Provides**: Insight Points, significant Reputation

**Intellectual Approaches**
- **Primary Reward**: Insight Points (1)
- **Secondary Reward**: Minimal Silver (1-2)
- **NEVER Provides**: Significant Silver, significant Reputation

**Social Approaches**
- **Primary Reward**: Reputation (1-2)
- **Secondary Reward**: Minimal Silver (1)
- **NEVER Provides**: Insight Points, significant Silver

### Commission Actions (One-Time Only)
**Labor Commissions**
- **Primary Reward**: Silver (8-15)
- **Secondary Reward**: Moderate Reputation (3-8)
- **NEVER Provides**: Significant Insight Points

**Research Commissions**
- **Primary Reward**: Insight Points (1-3)
- **Secondary Reward**: Moderate Silver (5-10), Moderate Reputation (3-5)
- **NEVER Provides**: Highest Silver rewards

**Diplomatic Commissions**
- **Primary Reward**: Reputation (8-15)
- **Secondary Reward**: Moderate Silver (5-8)
- **NEVER Provides**: Highest Silver rewards, Significant Insight Points

## Progression Gates & Critical Requirements

**Reputation Thresholds**
- **Basic Commissions**: 0 Reputation (available immediately)
- **Intermediate Commissions**: 15 Reputation
- **Advanced Commissions**: 30 Reputation
- **Position Offers**: 40+ Reputation AND completion of specific commission chains

**Lodging Options**
- **Floor Space**: 2 Silver/night (minimal card refresh)
- **Shared Room**: 5 Silver/night (partial card refresh)
- **Private Room**: 8 Silver/night (full card refresh, counts for success)
- **Rented Cottage**: 100 Silver deposit (permanent lodging, counts for success)

**Card Refresh Costs**
- First refresh: 2 Silver
- Second refresh: 5 Silver
- Third refresh: 10 Silver
- Fourth+ refresh: 15 Silver each

## Procedural Encounter System Refinement

### Structured Approach Options
Each action must offer 1-3 approach options with these differences:
- Different card type requirement (Physical/Intellectual/Social)
- Different reward profiles aligned with card type
- Different skill check paths in the generated encounter

### Encounter Generation Parameters
- **Stage Count**: 2-3 stages based on commission complexity
- **Progress Threshold**: 8-15 based on commission tier
- **Base Progress Formula**: `2 × stage_number`
- **Skill Check Difficulty**: `1 + stage_number + location_modifiers`
- **Bonus Progress**: +2 for primary skill, +0 for secondary skill
- **Failure Impact**: Minimal progress (+1) or minor setback (-1)

### Sample Structured Progress Thresholds
- **Tier 1 Commission**: 8 progress needed (completable in one good encounter)
- **Tier 2 Commission**: 12 progress needed (requires strategic choices)
- **Tier 3 Commission**: 15 progress needed (requires optimal skill usage)

## Sample Implementation

### Repeatable Action
```
Action: "Inn Assistance"
Location: Dusty Flagon (Common Room)
Approaches:
- Physical: "Manual Labor" (Physical card)
  • Reward: 3 Silver
  • Suitable for: Quick silver generation

- Intellectual: "Organize Records" (Intellectual card)
  • Reward: 1 Silver, 1 Insight Point
  • Suitable for: Chronicle advancement

- Social: "Customer Service" (Social card)
  • Reward: 1 Silver, 2 Reputation
  • Suitable for: Building initial reputation
```

### Commission Action (Tier 1)
```
Commission: "Repair Damaged Furniture"
Location: Dusty Flagon
Progress Threshold: 8
Reputation Requirement: 0
Expires: 2 days

Approaches:
- Physical: "Direct Repairs" (Physical card)
  • Reward: 8 Silver, 3 Reputation
  • Encounter focuses on Strength/Precision checks

- Intellectual: "Design-Based Approach" (Intellectual card)
  • Reward: 5 Silver, 3 Reputation, 1 Insight Point
  • Encounter focuses on Analysis/Knowledge checks

- Social: "Coordinate Helpers" (Social card)
  • Reward: 5 Silver, 5 Reputation
  • Encounter focuses on Persuasion/Charm checks
```

### Commission Action (Tier 2)
```
Commission: "Investigate Merchant Discrepancies"
Location: Market District
Progress Threshold: 12
Reputation Requirement: 15
Expires: 3 days

Approaches:
- Physical: "Shadow Suspects" (Physical card)
  • Reward: 10 Silver, 5 Reputation
  • Encounter focuses on Endurance/Agility checks

- Intellectual: "Audit Records" (Intellectual card)
  • Reward: 8 Silver, 5 Reputation, 2 Insight Points
  • Encounter focuses on Analysis/Knowledge checks

- Social: "Question Merchants" (Social card)
  • Reward: 7 Silver, 8 Reputation
  • Encounter focuses on Charm/Deception checks
```

### Position Offer Commission (Tier 3)
```
Commission: "Guard Captain Assessment"
Location: Town Square
Progress Threshold: 15
Reputation Requirement: 30
Prerequisite: Complete "Town Guard Training"
Expires: 2 days

Approaches:
- Physical: "Combat Demonstration" (Physical card)
  • Reward: 15 Silver, 10 Reputation, Guard Position Offer
  • Encounter focuses on Strength/Endurance checks

- Intellectual: "Tactical Assessment" (Intellectual card)
  • Reward: 12 Silver, 8 Reputation, 2 Insight Points, Guard Position Offer
  • Encounter focuses on Analysis/Planning checks

- Social: "Leadership Demonstration" (Social card)
  • Reward: 10 Silver, 12 Reputation, Guard Position Offer
  • Encounter focuses on Persuasion/Intimidation checks
```

This refined system creates genuine strategic tension through resource scarcity and meaningful choices, while maintaining mechanical clarity and implementation feasibility. Each resource has a clear purpose in the progression loop, and rewards are tightly calibrated to create distinct paths to success based on player approach preferences.
