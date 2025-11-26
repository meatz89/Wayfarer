# 6. Balance Philosophy

## Why This Document Exists

This document explains HOW balance creates strategic depth and WHY our principles differ from traditional RPG design. Balance in Wayfarer emerges from scarcity, not stat checks.

---

## 6.1 Core Balance Principle

**Challenge comes from resource scarcity, not mechanical difficulty.**

Traditional RPGs create difficulty through:
- Stat checks that block progress
- Enemies that deal more damage
- Skills that require higher levels

Wayfarer creates difficulty through:
- Orthogonal costs forcing prioritization
- Time scarcity preventing "do everything"

---

## 6.2 The Eight Balance Rules

### Rule 1: Perfect Information
All costs, requirements, and rewards visible before selection. No hidden gotchas. Players can calculate and plan.

### Rule 2: Resource Scarcity Creates Challenge
Difficulty comes from limited resources, not stat gates. Players with any build can progress—the question is COST.

### Rule 3: Orthogonal Costs Prevent Single Best Answer
When paths cost different resource types (time vs coins vs energy), no universal "optimal" exists.

### Rule 4: Specialization Creates Capability AND Vulnerability
High Insight opens Investigation paths but leaves Social paths expensive. Every strength implies weakness.

### Rule 5: No Unwinnable States
The four-choice archetype guarantees at least one path is ALWAYS available. Resource-strapped players pay more but progress.

### Rule 6: Mastery Through Optimization
Expert players complete content with fewer resources, not more power. Efficiency is the reward for skill.

### Rule 7: Tight Margins Throughout
Late-game margins remain tight. You never reach "resources don't matter" state. Optimization stays relevant.

### Rule 8: Build Diversity Is Mandatory
All stat distributions must have viable paths. If content only works for one build, content is broken.

---

## 6.3 The Four-Choice Balance Pattern

Every A-story situation balances across four path types:

| Path | Requirement | Cost | Target Player |
|------|-------------|------|---------------|
| **Stat-Gated** | High threshold | Free | Specialists in that stat |
| **Resource** | None | Significant coins/items | Resource-rich players |
| **Challenge** | Moderate stat | Session entry + time | Skill-expressive players |
| **Fallback** | None | Time + social cost | Anyone (guaranteed) |

### Balance Verification

✓ Specialists find their optimal path (stat-gated)
✓ Generalists have viable alternatives (resource/fallback)
✓ No single path dominates all situations
✓ Fallback always exists (no soft-locks)

---

## 6.4 Difficulty Scaling Mechanisms

### Categorical Properties, Not Level Gates

Difficulty scales through entity properties:
- **NPC Demeanor:** Friendly → Neutral → Suspicious → Hostile
- **Environment Quality:** Poor → Standard → Fine → Exceptional
- **Power Dynamic:** Subordinate → Equal → Superior → Authority

These multiply base archetype costs, creating contextual difficulty without explicit "level 5 required" gates.

### Progression Tier Expectations

As A-story advances, expected player state increases:

| Tier | A-Story | Expected Stats | Economic State |
|------|---------|----------------|----------------|
| Tutorial | A1-A3 | 1-2 in primary | Tight margins |
| Early | A4-A6 | 2-3 in primary | Moderate surplus |
| Mid | A7-A12 | 3-4 in primary | Strategic reserves |
| Late | A13+ | 4-5 in primary | Optimized efficiency |

**Key:** Margins remain tight at every tier. Late-game players face scaled challenges, not trivial content.

---

## 6.5 Build Diversity

### Why All Builds Must Work

If only one stat distribution succeeds, the game becomes a puzzle with a correct answer. We want:
- Multiple valid approaches to every situation
- Different builds experiencing different optimal paths
- No "trap builds" that fail content

### How We Ensure Diversity

1. **Four-choice archetype:** Every situation has paths for different specializations
2. **Stat-agnostic fallbacks:** Resource and time paths require no stats
3. **Challenge variety:** Mental, Physical, Social each favor different stats
4. **Orthogonal costs:** Different builds "pay" in different currencies

### Build Archetypes

| Build | Primary Stats | Excels At | Pays Extra For |
|-------|---------------|-----------|----------------|
| Investigator | Insight, Cunning | Mental challenges | Social situations |
| Diplomat | Rapport, Diplomacy | Social challenges | Physical obstacles |
| Enforcer | Authority | Intimidation, combat | Subtle investigation |
| Generalist | Balanced | Flexibility | Never gets free paths |

---

## 6.6 Balance Anti-Patterns

### Avoid: Single Resource Dominance
If everything costs coins, coin-rich players trivialize content. Ensure orthogonal costs.

### Avoid: Stat Gate Without Alternative
If only Insight 5+ can progress, Investigators proceed free while others are stuck. Ensure fallbacks.

### Avoid: Hidden Failure States
If choosing "wrong" option leads to unrecoverable loss, players feel cheated. Make consequences visible.

### Avoid: Power Creep
If late-game equipment trivializes challenges, tight margins philosophy breaks. Keep incremental advantages small.

---

## Cross-References

- **Numeric Values**: See [design/BASELINE_ECONOMY.md](../design/BASELINE_ECONOMY.md) for exact costs/rewards
- **Detailed Philosophy**: See [design/08_balance_philosophy.md](../design/08_balance_philosophy.md) for exhaustive treatment
- **Design Methodology**: See [design/DESIGN_GUIDE.md](../design/DESIGN_GUIDE.md) for practical balance workflow
