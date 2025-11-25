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
| **Resolve** | 0-10 | Social challenge entry | Pleasant activities | Emotional resilience |
| **Health** | 0-10 | Injury, disease | Healing services (coins + time) | Survival threshold |

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

Every A-story situation presents four path types:

| Path | Requirement | Cost | Risk | Purpose |
|------|-------------|------|------|---------|
| **Stat-Gated** | High stat threshold | Free | None | Rewards specialization |
| **Resource** | None | Coins/consumables | None | Universal fallback |
| **Challenge** | Moderate stat | Session entry | Failure possible | Skill expression |
| **Fallback** | None | Time/social cost | None | Guaranteed progress |

**Why this works:**
- At least one path ALWAYS available (no soft-locks)
- Orthogonal costs prevent single "best" answer
- Specialists rewarded but not required
- Challenge path offers risk/reward for skill expression

---

## Cross-References

- **Balance Philosophy**: See [06_balance.md](06_balance.md) for difficulty scaling
- **Resource Details**: See [design/05_resource_economy.md](../design/05_resource_economy.md) for exhaustive treatment
- **Challenge Details**: See [design/04_challenge_mechanics.md](../design/04_challenge_mechanics.md) for card mechanics
- **Technical Implementation**: See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) for system architecture
