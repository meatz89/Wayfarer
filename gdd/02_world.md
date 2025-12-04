# 2. World and Setting

## Why This Document Exists

This document defines the world players inhabit and WHY the setting reinforces our design pillars. The low-fantasy grounded aesthetic isn't arbitrary—it creates the conditions for impossible choices and meaningful consequence.

---

## 2.1 Setting: Grounded Historical-Fantasy

### The Tone

A world that feels lived-in, dangerous, and indifferent. Magic exists but is rare, mysterious, and never the player's primary tool. Danger comes from mundane sources—terrain, weather, bandits, politics, human nature.

**Why this matters for gameplay:**
- Verisimilitude creates stakes (injury feels real, not just a number)
- Grounded systems mirror intuitive real-world dynamics
- Low-magic prevents "just cast a spell" solutions
- Human-scale problems create human-scale drama

### Verisimilitude Principle

All systems mirror real-world dynamics:
- Relationships deplete when used (social capital is finite)
- Extreme positions have consequences (helping faction A angers faction B)
- Resources are genuinely scarce (the world doesn't provide infinite abundance)
- Geography constrains options (you can't teleport past obstacles)

---

## 2.2 Spatial Hierarchy

### Geography Matters

Hex-based travel through varied terrain where distance creates strategic decisions. The world is organized in nested spatial containers:

| Level | Description | Gameplay Function |
|-------|-------------|-------------------|
| **Region** | Largest container (a kingdom, territory) | Long-term travel goals, faction boundaries |
| **District** | Area within region | Mid-term exploration zones |
| **Venue** | 7-hex cluster with thematic coherence | Immediate exploration area (inn, market, temple) |
| **Location** | Single hex with specific function | Individual interaction point |

### Routes Connect Venues

Travel between venues follows defined routes with:
- Fixed environmental challenges (terrain, weather hazards)
- Procedural encounter opportunities
- Distance affecting time and resource costs
- Opacity levels (known routes vs unexplored paths)

**Why hex-based:** Creates concrete distance with calculable costs. Players can plan routes, estimate travel time, weigh alternatives. Perfect information at strategic layer.

---

## 2.3 Venues as Thematic Clusters

Venues group related locations into coherent spaces:

**Example: The Brass Bell Inn (Lodging Venue)**
- Common Room (social hub, job board)
- Private Rooms (rest, recovery)
- Kitchen (food services)
- Stable (mount care, storage)

**Why venues matter:**
- Intra-venue movement is cheap (adjacent hexes)
- Inter-venue travel consumes significant resources
- Thematic clustering creates intuitive navigation
- Each venue has a "personality" through fixed challenges and NPCs

---

## 2.4 The Living World

### Time Passes

Days have limited time blocks. NO Seasons (timespan is days or weeks). Nothing happens without player action. This creates:
- Opportunity cost for every decision
- Urgency without artificial timers

---

## Cross-References

- **Travel Mechanics**: See [03_core_loop.md](03_core_loop.md) for route gameplay
- **Technical Implementation**: See [arc42/05_building_block_view.md](../arc42/05_building_block_view.md) for spatial entity architecture
