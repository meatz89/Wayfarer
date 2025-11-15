# Wayfarer Baseline Economy Configuration

## Purpose

This document provides concrete numeric values for all economic balance parameters. These values serve as the authoritative source of truth for:
- Content authors designing situations
- Developers implementing balance formulas
- Playtesters validating economic progression
- AI systems generating balanced content

**Status:** Production baseline values
**Last Updated:** 2025-11

---

## Core Design Principles (Summary)

**Tight Margins Philosophy:**
- Early game: Barely scraping by (every coin matters)
- Mid game: Comfortable buffer possible (still requires planning)
- Late game: Numerical scale increases but margins stay proportionally tight

**Proportional Scarcity:**
- Premium options scale with income (luxury costs more as you earn more)
- Multiple simultaneous needs compete for resources
- Coin costs should feel meaningful (20-40% of typical reserve)

---

## Progression Tier Definitions

| Tier | Scene Range | Player State | Description |
|------|-------------|--------------|-------------|
| 1 | A1-A6 | Identity building → Early specialization | Tutorial phase, tight margins, learning systems |
| 2 | A7-A12 | Moderate specialization | Build identity emerges, economic buffer possible |
| 3 | A13-A20 | Deep specialization | Mastery in primary domain, scope escalates |
| 4 | A21+ | Mastery and extremes | Continental-tier situations, margins stay tight |

---

## B-Story Reward Baseline

### Coin Rewards by Tier

| Tier | Min Coins | Max Coins | Typical | Notes |
|------|-----------|-----------|---------|-------|
| 1 | 5 | 10 | 7-8 | Early game baseline |
| 2 | 15 | 25 | 18-20 | Mid game comfortable but not trivial |
| 3 | 30 | 50 | 35-40 | Late game higher scale |
| 4 | 50 | 80 | 60-65 | Very late game, premium options expensive |

**Categorical Scaling Modifiers:**

Simple B-story (basic delivery): 1.0× (baseline)
Complex B-story (investigation): 1.2× (20% more)
High-risk B-story (dangerous route): 1.4× (40% more)

**Example Tier 2:**
- Simple delivery: 18 coins (baseline)
- Investigation mission: 22 coins (18 × 1.2)
- Dangerous delivery: 25 coins (18 × 1.4)

### Stat Rewards (All Tiers)

| Reward Type | Amount | Notes |
|-------------|--------|-------|
| Single stat gain | +1 or +2 | +2 for primary path, +1 for secondary |
| Multi-stat symmetric gain | +1 to each of 2 stats | Total +2 distributed |
| Multi-stat trade (2-for-1) | +2 one stat, -1 another | Net +1, character shift |
| Multi-stat trade (3-for-1) | +3 one stat, -1 another | Net +2, dramatic shift |

**Frequency Guidelines:**
- Single-stat gains: 75-80% of stat-granting choices
- Multi-stat symmetric: 10-15% of stat-granting choices
- Multi-stat asymmetric: 5-10% of stat-affecting choices

### Relationship Rewards (All Tiers)

| Path Quality | Relationship Change | Notes |
|--------------|---------------------|-------|
| Best path (stat-gated optimal) | +2 | High specialization creates strong bonds |
| Good path (money-gated, challenge success) | +1 | Adequate relationship building |
| Adequate path (fallback, challenge failure) | +0 or +1 | Minimal but acceptable |
| Negative outcome | -1 or -2 | Crisis situations, failed obligations |

---

## Cost Baseline Values

### Food Costs (All Tiers)

| Quality | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Notes |
|---------|--------|--------|--------|--------|-------|
| Basic meal | 3 | 4 | 5 | 5 | Bread, water, minimal nutrition |
| Standard meal | 5 | 6 | 7 | 8 | Adequate nutrition, common fare |
| Premium meal | 8 | 10 | 12 | 15 | Good food, restaurant quality |
| Luxury feast | 15 | 20 | 25 | 30 | Special occasions, high-quality |

**Percentage of B-Story Earnings:**
- Basic meal: 40-60% of single B-story (Tier 1), 10-15% (Tier 4)
- Standard meal: 60-70% of single B-story (Tier 1), 12-15% (Tier 4)

### Lodging Costs by Quality and Tier

| Quality | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Restoration | Notes |
|---------|--------|--------|--------|--------|-------------|-------|
| Basic shelter | 5 | 8 | 12 | 15 | 1× baseline | Barn, common room floor |
| Standard room | 8 | 12 | 18 | 25 | 2× baseline | Private room, adequate comfort |
| Premium room | 15 | 25 | 35 | 50 | 3× baseline | High quality, excellent service |
| Luxury suite | 30 | 50 | 70 | 100 | 4× baseline | Exceptional, rare |

**Restoration Amounts:**
- Basic: 100% baseline (6-8 Health, 50% Stamina)
- Standard: 200% baseline (12-16 Health, 100% Stamina)
- Premium: 300% baseline (18-24 Health, 100% Stamina + Focus)
- Luxury: 400% baseline (24-32 Health, 100% all resources)

**Percentage of B-Story Earnings:**
- Basic shelter: 60-80% of single B-story (Tier 1), 20-25% (Tier 4)
- Standard room: 100-120% of single B-story (Tier 1), 35-40% (Tier 4)
- Premium room: 200-250% of single B-story (Tier 1), 55-65% (Tier 4)

**Design Note:** Player must complete 1-2 B-stories to afford comfortable lodging at all tiers. Premium options remain expensive throughout.

### Equipment Costs by Tier

| Equipment Type | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Notes |
|----------------|--------|--------|--------|--------|-------|
| Basic tool/weapon | 10 | 20 | 35 | 50 | Entry-level gear |
| Standard equipment | 25 | 40 | 70 | 110 | Reliable quality |
| Premium equipment | 50 | 80 | 140 | 220 | High quality, specialized |
| Masterwork gear | 100 | 180 | 300 | 500 | Exceptional, rare |

**B-Story Equivalent:**
- Basic: 1-2 B-stories
- Standard: 3-4 B-stories
- Premium: 6-8 B-stories
- Masterwork: 12-15 B-stories

**Design Note:** Equipment is expensive investment. Players must save over multiple sessions or prioritize economic B-stories.

### Training/Service Costs by Tier

| Service Type | Tier 1 | Tier 2 | Tier 3 | Tier 4 | Benefit |
|--------------|--------|--------|--------|--------|---------|
| Stat training | 20 | 35 | 60 | 100 | +1 stat (alternative to challenge XP) |
| Skill training | 15 | 25 | 45 | 75 | Unlock new capability |
| Information purchase | 10 | 20 | 35 | 60 | Learn location, shortcut, secret |
| Healing service | 8 | 15 | 25 | 40 | Restore Health (cheaper than premium lodging) |

**Design Note:** Training provides alternative progression path for wealthy players. Same cost effectiveness as XP grinding but faster.

---

## A-Story Situation Costs

### Stat Requirement Thresholds by Tier

| Tier | Scene Range | Stat Requirement | Expected Player State | Notes |
|------|-------------|------------------|----------------------|-------|
| 1 | A1-A3 | 0 (identity building) | 0-1 all stats | No requirements, offer stat gains |
| 1 | A4-A6 | 2-3 | 1 stat at 2-3, others 0-1 | First specialization gates |
| 2 | A7-A12 | 4-5 | 1-2 stats at 4-5 | Moderate specialization |
| 3 | A13-A20 | 6-7 | 1-2 stats at 6-7, 1 at 3-4 | Deep specialization |
| 4 | A21+ | 7-8+ | 1-2 stats at 7-8, 1-2 at 4-5 | Mastery level |

**Multiple Stat Paths:**
- Offer 2-3 different stat requirements at same threshold
- Validates different build specializations
- Example A9: "Insight 5 OR Authority 5 OR Cunning 4"

**Multi-Stat Compound Requirements:**
- Two-stat equal: Both ≥ 3 (total 6 points, vs single stat ≥ 5)
- Two-stat asymmetric: Primary ≥ 5, Secondary ≥ 2 (total 7 points)
- Frequency: 10-15% of stat-gated paths
- Must offer better rewards than single-stat path

### Money-Gated Path Costs by Tier

| Tier | Scene Range | Coin Cost | % of Typical Reserve | B-Story Equivalent |
|------|-------------|-----------|---------------------|-------------------|
| 1 | A4-A6 | 10-15 | 30-40% of 30-50 coins | 1-2 B-stories |
| 2 | A7-A12 | 20-30 | 30-40% of 60-80 coins | 1-2 B-stories |
| 3 | A13-A20 | 35-50 | 30-40% of 100-150 coins | 1-2 B-stories |
| 4 | A21+ | 60-100 | 30-40% of 150-250 coins | 1-2 B-stories |

**Design Principle:** Money-gated path should cost 1-2 B-story completions at all progression levels. Feels substantial but achievable.

**Categorical Scaling:**
- Friendly NPC: 0.6× (easier to bribe/pay)
- Neutral NPC: 1.0× (baseline)
- Hostile NPC: 1.4× (harder to pay off)

**Quality Scaling:**
- Basic service: 0.6× (cheap)
- Standard service: 1.0× (baseline)
- Premium service: 1.6× (expensive)
- Luxury service: 2.4× (very expensive)

**Example Tier 2 (A9):**
- Baseline cost: 25 coins
- Friendly + Basic: 25 × 0.6 × 0.6 = 9 coins (very affordable)
- Hostile + Luxury: 25 × 1.4 × 2.4 = 84 coins (very expensive)

### Session Resource Costs (Challenge Paths) by Tier

| Tier | Scene Range | Resolve/Stamina/Focus Cost | % of Session Pool | Notes |
|------|-------------|---------------------------|-------------------|-------|
| 1 | A4-A6 | 2-3 | 20-30% of 10 pool | Early challenges |
| 2 | A7-A12 | 3-4 | 25-35% of 12 pool | Mid challenges |
| 3 | A13-A20 | 4-5 | 30-40% of 15 pool | Late challenges |
| 4 | A21+ | 5-6 | 35-45% of 18 pool | Mastery challenges |

**Session Resource Pool Sizes:**
- Resolve (Social challenges): 8-12 baseline, scales to 15-18 late game
- Stamina (Physical challenges): 15-20 baseline, scales to 25-30 late game
- Focus (Mental challenges): 10-15 baseline, scales to 18-25 late game

**Design Note:** Challenge cost should allow 2-4 challenges per session before depletion. Forces resource management across multiple situations.

### Time Costs (Fallback Paths - All Tiers)

| Path Type | Time Block Cost | In-Game Hours | Opportunity Cost |
|-----------|----------------|---------------|------------------|
| Fallback path | 3-5 blocks | 2-4 hours | 1-2 B-stories, or alternative A-scene progress |
| Extended help | 6-8 blocks | 4-6 hours | Half a day, significant opportunity cost |
| Long-term wait | 12+ blocks (multiple days) | Days | Major opportunity cost, rare |

**Time Structure:**
- 4 periods per day (Morning, Midday, Afternoon, Evening)
- 4 segments per period = 16 total segments per day
- Most actions cost 1-3 segments
- Time blocks in fallback paths: 3-5 segments typical

**Design Note:** Time cost doesn't scale with tier (universal constraint). Meaningful because player could spend time elsewhere, not because numbers increase.

---

## Resource Restoration Values

### Rest and Recovery

| Rest Quality | Health Restored | Stamina Restored | Focus Restored | Resolve Restored | Cost |
|--------------|-----------------|------------------|----------------|------------------|------|
| Basic shelter | 6-8 Health | 50% Stamina | 0% Focus | 25% Resolve | 5-15 coins |
| Standard room | 12-16 Health | 100% Stamina | 50% Focus | 50% Resolve | 8-25 coins |
| Premium room | 18-24 Health | 100% Stamina | 100% Focus | 75% Resolve | 15-50 coins |
| Luxury suite | 24-32 Health | 100% Stamina | 100% Focus | 100% Resolve | 30-100 coins |

**Time Cost:** All rest options cost same time (overnight = 8 segments in Evening period). Quality determines restoration amount, not time.

### Healing Services

| Service Type | Health Restored | Cost (Tier 1) | Cost (Tier 2) | Cost (Tier 3) | Cost (Tier 4) |
|--------------|-----------------|---------------|---------------|---------------|---------------|
| Basic treatment | 4-6 Health | 5 | 8 | 12 | 18 |
| Standard healing | 8-12 Health | 8 | 15 | 25 | 40 |
| Premium healing | 16-24 Health | 15 | 30 | 50 | 80 |
| Emergency stabilization | Prevent death | 20 | 35 | 60 | 100 |

**Time Cost:** 1-2 segments for treatment. Faster than rest but more expensive per Health point.

---

## Challenge Session Values

### Target Thresholds by Challenge Tier

| Challenge Tier | Insight Target | Rapport Target | Authority Target | Cunning Target | Notes |
|----------------|----------------|----------------|------------------|----------------|-------|
| 1 (Easy) | 6-8 | 5-7 | 6-8 | 5-7 | Tutorial level |
| 2 (Moderate) | 8-10 | 7-9 | 8-10 | 7-9 | Early-mid game |
| 3 (Hard) | 10-12 | 9-11 | 10-12 | 9-11 | Mid-late game |
| 4 (Very Hard) | 12-15 | 11-14 | 12-15 | 11-14 | Mastery level |

**Danger/Doubt Thresholds:** 10 for all challenge types (automatic failure trigger)

### Challenge Rewards

| Outcome | Stat Gain | Resource Gain | Relationship Change | Notes |
|---------|-----------|---------------|---------------------|-------|
| Excellent (exceed by 4+) | +2 stat | Bonus resource | +2 | Exceptional performance |
| Success (meet or exceed by 1-3) | +1 stat | Standard resource | +1 | Good performance |
| Marginal (1-2 below target) | +1 stat | Reduced resource | +0 | Barely succeeded |
| Failure (3+ below target) | +0 | No resource | -1 | Did not reach threshold |
| Catastrophic (danger/doubt 10) | -1 stat or Health | Loss | -2 | Critical failure |

---

## Economic Pressure Validation

### Player Reserve Expectations by Tier

| Tier | Typical Coin Reserve | After Food/Lodging | Available for A-Paths | Notes |
|------|---------------------|-------------------|---------------------|-------|
| 1 | 30-50 coins | 15-30 coins | 10-20 coins | Tight margins, every coin matters |
| 2 | 60-80 coins | 35-55 coins | 20-40 coins | Comfortable buffer, still planning required |
| 3 | 100-150 coins | 70-110 coins | 40-80 coins | Larger scale, premium options expensive |
| 4 | 150-250 coins | 100-180 coins | 60-140 coins | High numbers, margins proportionally tight |

**Validation Formula:**

```
After covering basic needs (food + standard lodging):
- Tier 1: Reserve 40 → Spend 15 (food 5 + lodging 10) → Remaining 25 → A-path 15 costs 60% ✓
- Tier 2: Reserve 70 → Spend 20 (food 6 + lodging 14) → Remaining 50 → A-path 25 costs 50% ✓
- Tier 3: Reserve 125 → Spend 25 (food 7 + lodging 18) → Remaining 100 → A-path 45 costs 45% ✓
- Tier 4: Reserve 200 → Spend 35 (food 8 + lodging 27) → Remaining 165 → A-path 80 costs 48% ✓
```

**Design Validation:** A-story money-gated paths should cost 40-60% of remaining reserve after basic needs. Feels substantial.

### Spending Pattern Example (Tier 2, Mid-Game)

**Player starts day with 65 coins:**

Morning:
- Breakfast: -6 coins (standard meal)
- Current: 59 coins

Midday:
- Complete B-story delivery: +20 coins
- Current: 79 coins

Afternoon:
- Encounter A-story money-gated path: -25 coins
- Current: 54 coins

Evening:
- Standard lodging: -12 coins
- Dinner: -6 coins
- Current: 36 coins

**End of day: 36 coins (comfortable buffer for tomorrow)**

**Validation:**
- Started with reserve (65 coins)
- Earned income (20 coins from B-story)
- Spent on progression (25 coins for A-path)
- Covered basic needs (food 12 + lodging 12 = 24 coins)
- Ended with buffer (36 coins, can handle tomorrow's needs)

**Margins tight but manageable.** Player had to work (B-story) to afford A-path + basic needs. ✓

---

## Stat Progression Expectations

### Total XP Investment by Tier

| Tier | Scene Range | Total XP Earned | Expected Distribution | Notes |
|------|-------------|-----------------|----------------------|-------|
| 1 | A1-A6 | 3-5 stat points | 1 stat at 2-3, others 0-1 | Early specialization begins |
| 2 | A7-A12 | 8-12 stat points | 1-2 stats at 4-5, others 1-2 | Moderate specialization |
| 3 | A13-A20 | 15-25 stat points | 1-2 stats at 6-7, 1 at 3-4 | Deep specialization |
| 4 | A21+ | 30+ stat points | 1-2 stats at 7-8, 1-2 at 4-5 | Mastery achieved |

**XP Gain Rate:**
- A-story situation completion: +1 or +2 stat (depending on path quality)
- B-story completion: +1 stat (occasionally +2)
- Challenge success: +1 or +2 stat (based on performance)
- Average: 1-2 stat points per 3-4 situations completed

### Build Archetype Examples by Tier 3

**Investigator Specialist:**
- Insight: 7
- Cunning: 5
- Others: 1-2 each
- Total: ~18 points invested

**Social Specialist:**
- Rapport: 7
- Diplomacy: 6
- Others: 1-2 each
- Total: ~19 points invested

**Leadership Specialist:**
- Authority: 7
- Diplomacy: 5
- Others: 1-2 each
- Total: ~18 points invested

**Balanced Generalist:**
- All stats: 4-5
- Total: ~22 points invested (more total but less depth)

---

## Categorical Property Multipliers

### NPCDemeanor Multipliers

| Demeanor | Stat Threshold | Coin Cost | Restoration | Notes |
|----------|---------------|-----------|-------------|-------|
| Friendly | 0.6× | 0.8× | 1.2× | Easier to persuade, cheaper services, better care |
| Neutral | 1.0× | 1.0× | 1.0× | Baseline values |
| Hostile | 1.4× | 1.4× | 0.8× | Harder to persuade, expensive services, poor care |

### Quality Multipliers

| Quality | Coin Cost | Restoration | Notes |
|---------|-----------|-------------|-------|
| Basic | 0.6× | 1.0× | Cheap but adequate |
| Standard | 1.0× | 2.0× | Baseline comfort |
| Premium | 1.6× | 3.0× | High quality, expensive |
| Luxury | 2.4× | 4.0× | Exceptional, very expensive |

### PowerDynamic Multipliers

| PowerDynamic | Authority Check | Rapport Check | Notes |
|--------------|----------------|---------------|-------|
| Dominant | 0.6× | 1.4× | Player has power (Authority easier, Rapport harder) |
| Equal | 1.0× | 1.0× | Baseline balanced |
| Submissive | 1.4× | 0.6× | NPC has power (Authority harder, Rapport easier) |

### EnvironmentQuality Multipliers

| Environment | Restoration | Coin Cost | Notes |
|-------------|-------------|-----------|-------|
| Basic | 1.0× | 0.6× | Minimal comfort, cheap |
| Standard | 2.0× | 1.0× | Good comfort, baseline cost |
| Premium | 3.0× | 1.6× | Exceptional comfort, expensive |

---

## Compound Scaling Examples

### Example 1: Lodging Negotiation (Tier 2, A9)

**Entity Properties:**
- NPC: Friendly innkeeper (Demeanor: Friendly)
- Service: Standard room (Quality: Standard)
- Archetype: service_negotiation

**Base Values (from archetype):**
- BaseStatThreshold: 5
- BaseCoinCost: 12

**Calculation:**
- Stat threshold: 5 × 0.6 (Friendly) = 3
- Coin cost: 12 × 1.0 (Standard) = 12 coins

**Result:**
- Stat-gated path: Rapport 3 → Room for 3 coins (discounted), relationship +2
- Money-gated path: 12 coins → Room for 12 coins, relationship +1
- Challenge path: Social challenge → Success: Room for 5 coins, relationship +2 | Failure: Room for 12 coins
- Fallback path: 4 time blocks helping → Room free, relationship +1

**Validation:**
- Friendly innkeeper easier to negotiate with (Rapport 3 vs baseline 5) ✓
- Standard room costs baseline (12 coins) ✓
- Proportional rewards (stat-gated best, fallback minimal) ✓

### Example 2: Luxury Hostile Merchant (Tier 3, A15)

**Entity Properties:**
- NPC: Hostile merchant (Demeanor: Hostile)
- Service: Premium equipment (Quality: Premium)
- Archetype: trade_negotiation

**Base Values:**
- BaseStatThreshold: 7
- BaseCoinCost: 70

**Calculation:**
- Stat threshold: 7 × 1.4 (Hostile) = 10 (rounded)
- Coin cost: 70 × 1.6 (Premium) = 112 coins

**Result:**
- Stat-gated path: Authority 10 → Equipment for 50 coins (discount), relationship +0
- Money-gated path: 112 coins → Equipment for 112 coins, relationship +0
- Challenge path: Social challenge → Success: Equipment for 80 coins | Failure: Equipment for 112 coins
- Fallback path: Find alternative merchant (no time-based fallback for trade)

**Validation:**
- Hostile merchant very hard to negotiate (Authority 10, extreme requirement) ✓
- Premium equipment very expensive (112 coins, ~70% of Tier 3 reserve) ✓
- Hostile demeanor means minimal relationship gains regardless of path ✓

---

## Crisis Situation Baseline Values

### Crisis Cost Multipliers

Crisis situations have HIGHER costs than normal situations to reflect damage control nature.

| Resource Type | Normal Situation Cost | Crisis Situation Cost | Multiplier |
|---------------|----------------------|----------------------|------------|
| Health loss | 0 (no loss) | 3-6 Health | n/a |
| Coin loss | 15-30 (payment) | 20-40 (forced loss) | 1.5× |
| Stat loss | 0 (gains only) | -1 or -2 stat | Penalty instead of gain |
| Relationship loss | +1 or +2 | -1 or -2 | Negative instead of positive |

### Example Crisis (Tier 2, A10): Bandit Ambush

**Path 1: Fight Back (Authority 5 - Stat-Gated Minimize Loss)**
- Outcome: Lose 4 Health, keep all coins, gain +1 Authority
- Tag: "FoughtBandits"

**Path 2: Pay Them Off (25 coins - Money-Gated Economic Damage)**
- Cost: 25 coins (125% of normal Tier 2 money-gated path)
- Outcome: Lose 25 coins, keep Health, bandit relationship +1

**Path 3: Physical Challenge (4 Stamina - Risk Management)**
- Success: Lose 3 Health, keep 20 coins (some stolen), +1 Cunning
- Failure: Lose 6 Health, lose 15 coins, +0 stat

**Path 4: Surrender Everything (Fallback - Maximum Loss)**
- Outcome: Lose ALL coins (~70 coins typical Tier 2 reserve), lose 2 Health, lose 1 Rapport
- Guaranteed survival, spawns next scene

**Validation:**
- All outcomes negative (crisis) ✓
- Different resources lost (Health vs Coins vs Both) ✓
- Worst-case player survives (fallback guarantees progression) ✓
- Asymmetric costs create balance (wealthy player picks Path 2, healthy player picks Path 1) ✓

---

## Edge Case Validation Values

### Minimum Viable Paths by Player State

**Worst-Case Player (0 coins, 0 stats, 0 session resources):**
- Available paths: Fallback only (time cost, zero requirements)
- Must progress: YES (fallback spawns next scene)
- Outcome quality: Minimal but adequate

**Scattered-Stat Player (e.g., Rapport 3, Cunning 2, Authority 6, others 1):**
- Available paths in A13 (requires 6-7): Authority path unlocked occasionally
- Fallback: Always available
- Must progress: YES
- Outcome quality: Variable (optimal when Authority applies, suboptimal otherwise)

**Generalist Player (All stats 4-5 at A13):**
- Available paths in A13 (requires 6-7): All stat-gated paths LOCKED (4-5 below threshold)
- Alternative paths: Money, challenge, fallback all available
- Must progress: YES
- Outcome quality: Moderate (never optimal, always viable)

**Validation Rule:** If generalist is locked out of ALL stat-gated paths but can still complete game efficiently via money/challenge paths, stat requirements are appropriate. Specialization provides advantage, not requirement.

---

## Usage Examples for Content Authors

### Example 1: Designing A8 (Tier 2 Mid-Game)

**Context:** Player needs information about next destination from local scholar.

**Step 1: Determine costs from tables**
- Stat requirement: 4-5 (Tier 2 range)
- Money-gated path: 20-30 coins
- Challenge path: 3-4 session resource
- Fallback: 3-5 time blocks

**Step 2: Select stat for context**
- Scholar situation → Insight makes sense
- Requirement: Insight 5

**Step 3: Design paths**
- Path 1: Insight 5 → Detailed information, relationship +2, free
- Path 2: 25 coins → Complete information, relationship +1
- Path 3: Mental challenge (4 Focus) → Success: Detailed info, +2 relationship, +1 Insight | Failure: Basic info
- Path 4: 4 time blocks researching → Complete information, relationship +1

**Step 4: Validate**
- Orthogonal costs ✓ (stat, coins, Focus, time)
- Proportional rewards ✓ (Insight path best, fallback minimal)
- Progression guarantee ✓ (Path 4 zero requirements)
- Cost appropriate for Tier 2 ✓ (25 coins is 35% of 70 coin reserve)

### Example 2: Designing Crisis A16 (Tier 3 Late-Game)

**Context:** Flash flood threatens player's camp.

**Step 1: Determine crisis costs**
- Health losses: 3-6 typical
- Coin costs: 35-50 × 1.5 = 50-75 coins
- Stat losses: -1 or -2

**Step 2: Design damage control paths**
- Path 1: Cunning 7 → Lose 3 Health (minimize), +1 Cunning (learn from crisis)
- Path 2: 60 coins → Pay for emergency rescue, lose 60 coins, keep Health
- Path 3: Physical challenge (5 Stamina) → Success: Lose 4 Health, save most gear | Failure: Lose 7 Health, lose 30 coins in equipment
- Path 4: Abandon camp → Lose ALL equipment (~100 coins value), lose 2 Health, lose 1 Rapport (shame)

**Step 3: Validate**
- All outcomes negative ✓
- Asymmetric costs ✓ (Health vs Coins vs Both vs Everything)
- Wealthy player has option (Path 2) ✓
- Specialist has advantage (Cunning minimizes damage) ✓
- Worst-case survives (Path 4 guarantees progression) ✓

---

## Automated Validation Formulas

### Formula 1: Money-Gated Path Validation

```
VALID if:
  CoinCost >= (TypicalReserve × 0.20) AND
  CoinCost <= (TypicalReserve × 0.40) AND
  CoinCost >= (B-StoryReward × 1.0) AND
  CoinCost <= (B-StoryReward × 2.0)

Example Tier 2:
  TypicalReserve = 70 coins
  B-StoryReward = 20 coins

  Valid range: 20-30 coins
  Lower bound: max(70 × 0.20, 20 × 1.0) = max(14, 20) = 20 ✓
  Upper bound: min(70 × 0.40, 20 × 2.0) = min(28, 40) = 28 ✓

  CoinCost = 25 → VALID ✓
```

### Formula 2: Session Resource Validation

```
VALID if:
  SessionResourceCost >= (TypicalPool × 0.20) AND
  SessionResourceCost <= (TypicalPool × 0.40)

Example Tier 2 Social Challenge:
  TypicalResolvePool = 12

  Valid range: 3-5 Resolve
  Lower: 12 × 0.20 = 2.4 → 3 (rounded up) ✓
  Upper: 12 × 0.40 = 4.8 → 5 (rounded up) ✓

  ResolveCost = 4 → VALID ✓
```

### Formula 3: Reward Proportionality Validation

```
VALID if:
  StatGatedReward > MoneyGatedReward AND
  MoneyGatedReward >= FallbackReward AND
  ChallengeSuccessReward >= MoneyGatedReward AND
  ChallengeFailureReward >= FallbackReward

Example (relationship gains):
  StatGated = +2
  MoneyGated = +1
  ChallengeSuccess = +2
  ChallengeFailure = +0
  Fallback = +1

  +2 > +1 ✓
  +1 >= +1 ✓
  +2 >= +1 ✓
  +0 >= +1 ✗ INVALID

FIX: ChallengeFailure should be +1 (not +0) to maintain proportionality
```

---

## Related Documentation

**Design Philosophy:**
- [08_balance_philosophy.md](08_balance_philosophy.md) - Complete balance philosophy, rationale, and extensive examples

**Practical Methodology:**
- [DESIGN_GUIDE.md](DESIGN_GUIDE.md) - Step-by-step process for designing balanced situations

**Core Concepts:**
- [05_resource_economy.md](05_resource_economy.md) - Impossible choices, orthogonal costs, tight margins philosophy
- [03_progression_systems.md](03_progression_systems.md) - Stat progression, economic systems, content unlocking

**Implementation:**
- [07_content_generation.md](07_content_generation.md) - 21 archetypes using these baseline values
- [arc42/08_crosscutting_concepts.md](../08_crosscutting_concepts.md) - Catalogue pattern implementing scaling formulas

---

**Document Status:** Production baseline configuration
**Last Updated:** 2025-11
**Maintained By:** Design team, balance team
