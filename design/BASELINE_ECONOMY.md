# Baseline Economy Reference

This document provides **authoritative numeric values** for Wayfarer's game economy. When implementing costs, rewards, or requirements, use these values as the baseline.

---

## Resource Pools

| Resource | Pool Range | Restoration | Notes |
|----------|------------|-------------|-------|
| **Time** | 8-10 blocks/day | New day only | Cannot buy more |
| **Coins** | No upper limit | Jobs, quests | Universal currency |
| **Focus** | 0-10 | Rest (2 time blocks) | Mental capability |
| **Stamina** | 0-10 | Rest (2 time blocks) | Physical capability |
| **Resolve** | 0-10 | Pleasant activities | Emotional resilience |
| **Health** | 0-10 | Healing services | 0 = incapacitation |

---

## Income Sources

| Source | Amount | Notes |
|--------|--------|-------|
| **Delivery completion** | 20-50 coins | Primary income |
| **Quest rewards** | 10-30 coins | Investigation/request completion |
| **NPC gifts** | 5-15 coins | From high relationships |
| **Found treasures** | 5-20 coins | Exploration rewards |

---

## Mandatory Expenses

| Expense | Cost | Frequency |
|---------|------|-----------|
| **Food** | 5-15 coins | Per meal (at least 1/day) |
| **Lodging** | 10-20 coins | Per night |
| **Healing** | 15-30 coins + 2 time | Per injury |

---

## Economic Baseline: Delivery Cycle

**Typical successful delivery:**
- Gross payment: 30 coins
- Food during travel: ~10 coins
- Lodging at destination: ~15 coins
- **Net profit: ~5 coins**

**Equipment upgrade: 60 coins = 12 successful deliveries**

This tight margin is **intentional**—every coin matters.

---

## Challenge Entry Costs

| Challenge Type | Entry Cost | Pool Affected |
|----------------|------------|---------------|
| **Mental** | 2-5 Focus | Focus |
| **Physical** | 2-5 Stamina | Stamina |
| **Social** | 2-5 Resolve | Resolve |

---

## Travel Costs

| Cost Type | Amount | Notes |
|-----------|--------|-------|
| **Stamina per segment** | 1-3 | Terrain dependent |
| **Time per segment** | 1-2 blocks | Distance dependent |

---

## Service Costs

| Service | Coin Cost | Time Cost |
|---------|-----------|-----------|
| **Healing (minor)** | 15 coins | 2 blocks |
| **Healing (major)** | 30 coins | 2 blocks |
| **Information** | 10-30 coins | 1 block |
| **Training** | 20-50 coins | 2-3 blocks |
| **Bribes/Gifts** | 5-30 coins | — |

---

## Stat Requirements by Tier

| Progression Tier | A-Story | Single Stat Gate | Multi-Stat Gate |
|------------------|---------|------------------|-----------------|
| **Tutorial** | A1-A3 | 2-3 | N/A |
| **Early** | A4-A6 | 3-4 | 3 AND 2 |
| **Mid** | A7-A12 | 4-5 | 3 AND 3 |
| **Late** | A13-A20 | 5-6 | 4 AND 3 |
| **Endgame** | A21+ | 6+ | 5 AND 4 |

---

## Stat Gains

| Gain Type | Amount | Example |
|-----------|--------|---------|
| **Standard single stat** | +2 | Pure specialization |
| **Multi-stat symmetric** | +1 each (2 stats) | Breadth build |
| **Three-stat** | +1 each (3 stats) | Rare, generalist |
| **Asymmetric trade** | +2/-1 or +3/-1 | Character transformation |
| **Even trade** | +2/-2 | Pure directional shift |

---

## Categorical Property Multipliers

These modify base costs/rewards based on entity context:

**NPCDemeanor:**
| Demeanor | Cost Multiplier |
|----------|-----------------|
| Friendly | 0.8x |
| Neutral | 1.0x |
| Suspicious | 1.2x |
| Hostile | 1.5x |

**Quality:**
| Quality | Cost Multiplier |
|---------|-----------------|
| Poor | 0.7x |
| Standard | 1.0x |
| Fine | 1.3x |
| Exceptional | 1.6x |

**PowerDynamic:**
| Dynamic | Cost Multiplier |
|---------|-----------------|
| Subordinate | 0.8x |
| Equal | 1.0x |
| Superior | 1.2x |
| Authority | 1.5x |

---

## Relationship Scaling

| Relationship Level | Range | Effect |
|--------------------|-------|--------|
| Hostile | -5 to -3 | +50% costs, blocked actions |
| Negative | -2 to -1 | +25% costs |
| Neutral | 0 | Baseline |
| Positive | +1 to +2 | -10% costs, bonus options |
| Allied | +3 to +5 | -25% costs, unique options |

---

## Cross-References

- **Economy Philosophy**: See [05_resource_economy.md](05_resource_economy.md) for detailed rationale
- **Balance Methodology**: See [08_balance_philosophy.md](08_balance_philosophy.md) for tuning principles
- **GDD Overview**: See [gdd/06_balance.md](../gdd/06_balance.md) for design principles
