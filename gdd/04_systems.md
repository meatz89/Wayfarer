# 4. Game Systems

## Why This Document Exists

This document provides an overview of Wayfarer's interconnected systems and WHY they create strategic depth through resource competition. For detailed mechanics, see the reference documentation.

---

## 4.1 Resource Economy

### Universal Resources

Resources that compete across all systems, forcing strategic prioritization:

| Resource | Pool | Consumed By | Restored By | Strategic Role |
|----------|------|-------------|-------------|----------------|
| **Time** | 8-10 blocks/day | Travel, services, rest | New day | Most constrained—cannot buy more |
| **Coins** | Unlimited | Purchases, services, bribes | Delivery jobs | Universal exchange medium |
| **Focus** | 0-10 | Mental challenge entry | Rest (costs time) | Mental capability pool |
| **Stamina** | 0-10 | Physical challenge entry, travel | Rest (costs time) | Physical capability pool |
| **Resolve** | -10 to ∞ | Meaningful story choices | Earned through choices | Willpower gate (Sir Brante pattern) |
| **Health** | 0-10 | Injury, disease | Healing services (coins + time) | Survival threshold |

### Resolve: The Willpower Gate (Sir Brante Pattern)

Resolve follows the "Sir Brante Willpower" design pattern—a resource that creates **meaningful choice through scarcity and consequence**, not through abundance.

#### Core Design Principles

| Principle | Implementation | Why It Matters |
|-----------|---------------|----------------|
| **Starts Empty** | Resolve begins at 0 | Players must EARN before they can SPEND |
| **Can Go Negative** | Minimum -10 | Consequences for overcommitting |
| **Large Gains** | +5, +10 per choice | Meaningful rewards feel significant |
| **Large Costs** | -5, -10 per choice | Choices feel weighty, not trivial |
| **Dual Nature** | Always condition AND consequence | Creates true "willpower" fantasy |

#### The Dual-Nature Rule

**Every choice that costs Resolve has the structure:**
- **Requirement:** Resolve >= 0 (must have positive willpower to attempt)
- **Consequence:** Resolve -5 or -10 (spending willpower depletes reserve)

This creates the "willpower gate": players can only make costly choices when they have built up enough resolve through earlier positive choices. A player at Resolve 0 CAN still take a costly choice (because 0 >= 0), but will go negative afterward. A player at Resolve -5 CANNOT take costly choices until they rebuild.

**Key distinction from other resources:** Coins, Health, Stamina, Focus use *affordability* logic (`resource >= cost`). Resolve uses *gate* logic (`Resolve >= 0`). See [arc42/08 §8.20](../arc42/08_crosscutting_concepts.md#820-sir-brante-willpower-pattern) for implementation details.

#### Why This Pattern Creates Depth

**Traditional resource design (WRONG):**
```
Resolve starts at 30
Choice costs -1 Resolve
Player has 29 Resolve remaining
Result: Meaningless—player can make 30 choices before any constraint
```

**Sir Brante pattern (CORRECT):**
```
Resolve starts at 0
First opportunity: Choice gives +10 Resolve (from 0 to 10)
Later opportunity: Choice requires Resolve >= 0, costs -5 Resolve (from 10 to 5)
Difficult opportunity: Choice requires Resolve >= 0, costs -10 Resolve (from 5 to -5)
Result: Player went negative—they CANNOT make any more costly choices until they rebuild
```

**The meaningful tension:** Players must decide whether to spend their hard-earned resolve on the current opportunity, knowing they may need it later. Going negative means losing access to costly choices until resolve is rebuilt through positive actions.

#### Example Progression

| Scene | Choice Made | Resolve Before | Change | Resolve After | Strategic State |
|-------|-------------|----------------|--------|---------------|-----------------|
| A1 | Stay true to principles | 0 | +10 | 10 | Can now make costly choices |
| A2 | Negotiate hard (requires ≥0) | 10 | -5 | 5 | Still above threshold |
| A3 | Stand firm despite pressure (requires ≥0) | 5 | -10 | -5 | **Below threshold—locked out** |
| A4 | Cannot select costly choice | -5 | — | -5 | Must find +Resolve opportunity |
| A5 | Accept help despite pride | -5 | +5 | 0 | Back at threshold—options open |

#### Integration with Four-Choice Archetype

Resolve costs appear primarily in:
- **Stat-Gated choices:** High stat + costly Resolve creates "this is who I am" moments
- **Challenge choices:** Tactical success + Resolve cost rewards skillful play with narrative weight

Resolve costs do NOT appear in:
- **Fallback choices:** These must remain always-available (no soft-locks)
- **Pure resource choices:** Coins/consumables are separate cost axis

### Why Shared Resources Create Depth

When multiple systems compete for the same resource, players must prioritize. Time spent resting is time not spent traveling. Coins spent on healing are coins not spent on equipment. Focus used for investigation depletes Mental challenge capability.

**The principle:** Orthogonal resource costs ensure every choice sacrifices something.

---

## 4.2 The Five Stats

Stats determine capability in challenges and gate certain choices:

| Stat | Domain | Governs |
|------|--------|---------|
| **Insight** | Mental | Investigation, puzzle-solving, observation |
| **Cunning** | Mental | Deception, misdirection, reading situations |
| **Rapport** | Social | Building trust, friendly persuasion, empathy |
| **Diplomacy** | Social | Formal negotiation, authority appeal, compromise |
| **Authority** | Physical | Intimidation, command presence, physical dominance |

### Specialization Through Scarcity

Limited total stat points force specialization:
- Cannot maximize all five stats
- High stats unlock optimal paths (free, efficient)
- Low stats force resource-expensive alternatives
- Build identity emerges from allocation choices

**Why this matters:** A Diplomat (Rapport 4, Diplomacy 3) experiences different gameplay than an Investigator (Insight 4, Cunning 3). Same content, different viable paths.

---

## 4.3 Three Tactical Challenge Systems

When players choose challenge paths, they enter one of three card-based tactical systems:

### Mental Challenges

**Fantasy:** Investigation, puzzle-solving, research
**Core mechanic:** Build Progress through Leads and Details
**Cards bound to:** Insight, Cunning
**Session resource:** Attention (derived from Focus)
**Threshold resource:** Exposure (detection risk)

### Physical Challenges

**Fantasy:** Combat, athletics, endurance
**Core mechanic:** Overcome obstacles through Exertion
**Cards bound to:** Authority, Stamina-related
**Session resource:** Exertion (derived from Stamina)
**Threshold resource:** Danger (injury risk)

### Social Challenges

**Fantasy:** Persuasion, negotiation, conversation
**Core mechanic:** Build Momentum through dialogue
**Cards bound to:** Rapport, Diplomacy
**Session resource:** Initiative (built through Foundation cards)
**Threshold resource:** Doubt (conversation breakdown risk)

### Strategic-Tactical Bridge

**Strategic layer** shows: Entry cost, stat requirements, success/failure outcomes
**Tactical layer** provides: Card play, resource management, execution skill

Player knows WHETHER to attempt (strategic). Tactical layer tests HOW well they execute.

---

## 4.4 Progression Model

### Stat Advancement

Stats increase through deliberate investment:
- Experience points earned from challenge completion
- Must choose which stat to advance
- Each point increases challenge viability
- Opportunity cost: advancing one stat means not advancing another

### Equipment

Provides incremental advantages:
- Better cards in tactical challenges
- Reduced costs for specific actions
- Never transformative power (no "broken builds")

### Relationships

NPCs provide benefits when cultivated:
- Information, discounts, assistance
- BUT: Relationships have upkeep costs
- AND: Helping one NPC may disappoint another

**The pattern:** Every advancement avenue has costs and trade-offs.

---

## 4.5 The Four-Choice Archetype

Every A-story situation offers four paths, but the STRUCTURE varies by narrative rhythm:

| Rhythm | Choice Structure | Player Experience |
|--------|-----------------|-------------------|
| **Building** | All positive outcomes | "Which stat do I want to grow?" |
| **Crisis** | All negative outcomes | "Which loss can I minimize?" |
| **Mixed** | Trade-offs | "What am I willing to sacrifice?" |

**The guaranteed constant:** At least one path is ALWAYS available without requirements (no soft-locks).

### Mixed Rhythm Pattern (Most Common)

| Path | Requirement | Cost | Purpose |
|------|-------------|------|---------|
| **Stat-Gated** | High stat threshold | Free | Rewards specialization |
| **Resource** | None | Coins/consumables | Wealth alternative |
| **Challenge** | Moderate stat | Session entry | Skill expression |
| **Fallback** | None | Time/social cost | Guaranteed progress |

**Why this works:**
- Orthogonal costs prevent single "best" answer
- Specialists rewarded but not required
- Challenge path offers risk/reward for skill expression

### Fallback Context Rules

The Fallback path guarantees forward progress, but its meaning changes based on player commitment:

| Context | Fallback Meaning | Consequences |
|---------|-----------------|--------------|
| **Pre-commitment** | "Exit, return later" | None |
| **Post-commitment** | "Break commitment" | Penalty (e.g., -1 stat) |

**Key Rules:**
- Fallback NEVER has requirements (would create soft-locks)
- Fallback CAN have consequences (preserves scarcity)
- No two situations should have semantically identical Fallback choices

**Example:** In a delivery contract scene:
- Situation 1 Fallback: "Not right now" (decline offer, no cost)
- Situation 2 Fallback: "Back out of the deal" (break accepted contract, -1 Rapport)

Both guarantee progress, but the consequences scale with commitment level.

See [arc42/08_crosscutting_concepts.md §8.16](../arc42/08_crosscutting_concepts.md#816-fallback-context-rules-no-soft-lock-guarantee) for technical implementation.

---

## 4.6 AI Narrative Generation

### What AI Narrative Does

AI generates atmospheric text that brings mechanical situations to life. Every scene in Wayfarer has both **mechanical data** (who, where, what choices) and **narrative flavor** (atmosphere, mood, sensory details).

| Component | Source | Example |
|-----------|--------|---------|
| **Mechanical** | Archetypes + catalogues | "NPC: Martha Holloway, Innkeeper" |
| **Narrative** | AI generation | "The firelight dances across worn floorboards as the rain drums steadily on the roof." |

### Why AI, Not Hand-Written

| Hand-Written Narrative | AI Narrative |
|----------------------|--------------|
| Limited by author time | Unlimited unique variations |
| Repetitive across playthroughs | Fresh each playthrough |
| Disconnected from mechanical context | Responds to time, weather, NPC mood |
| Static | Dynamic (tired NPC at evening feels different than morning) |

**Design goal:** Every scene FEELS unique because the narrative responds to the complete context (time of day, weather, NPC personality, location atmosphere, situation type).

### The Additive Principle

AI narrative is **ADDITIVE** to mechanical context, never **CONFLICTING**.

| What This Means | Player Experience |
|-----------------|-------------------|
| AI adds mood and atmosphere | World feels alive and immersive |
| AI respects mechanical facts | No confusing contradictions |
| Names match visible entities | Coherent, trustworthy game world |

**Example of VALID narrative:**
- Player sees "Martha Holloway - Innkeeper" in NPC panel
- Narrative: "The innkeeper offers a tired smile, gesturing to the empty rooms upstairs."
- Result: Coherent—"innkeeper" matches visible profession

**Example of INVALID narrative:**
- Player sees "Martha Holloway - Innkeeper" in NPC panel
- Narrative: "Sarah greets you warmly from behind the bar."
- Result: **BROKEN**—who is Sarah? Contradicts visible entity

### Player-Facing Quality

Players experience AI narrative as seamless storytelling. They don't know (or care) that it's generated. What matters:

| Quality | Why It Matters |
|---------|---------------|
| **Brevity** | UI space is limited; 1-2 sentences max |
| **Atmosphere** | Creates mood without info-dumping |
| **Consistency** | Names, places match what player sees elsewhere |
| **Variety** | Same situation in different contexts feels different |

### Design Goals Summary

| Goal | How AI Achieves It |
|------|-------------------|
| **Infinite replayability** | Unique narrative each playthrough |
| **Contextual immersion** | Narrative responds to time, weather, mood |
| **Mechanical integrity** | AI enriches, never contradicts |
| **Production scalability** | No hand-authoring bottleneck |

See [arc42/08_crosscutting_concepts.md §8.28](../arc42/08_crosscutting_concepts.md#828-two-pass-procedural-generation) for the two-pass generation architecture.
See [arc42/08_crosscutting_concepts.md §8.29](../arc42/08_crosscutting_concepts.md#829-ai-narrative-additive-principle) for the additive principle implementation.
See [SOP-01: AI Narrative Optimization](../sop/01_ai_narrative_optimization.md) for prompt optimization and quality validation workflow.

---

## Cross-References

- **Balance Philosophy**: See [06_balance.md](06_balance.md) for difficulty scaling
- **Technical Implementation**: See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) for system architecture
