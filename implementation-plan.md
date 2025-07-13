# Wayfarer POC Content Implementation Plan

## Executive Summary

After analyzing the current JSON content and verified game systems, this document outlines a focused proof-of-concept content expansion. The goal is to create a minimal but complete ecosystem that demonstrates all strategic gameplay systems working together, using 5-10 entities per JSON file.

## Current State Analysis

### Existing Content Gaps
- **Locations:** Only 2 locations (dusty_flagon, town_square) - insufficient for strategic route planning
- **Items:** 11 items with missing equipment categories needed for verified systems
- **Contracts:** 14 contracts but limited strategic interconnection
- **Routes:** 9 routes in tiny network - no meaningful transport decisions
- **NPCs:** 8 NPCs providing basic services
- **Information:** 12 items but not integrated with location-based discovery

### Verified Systems Needing Content
1. **Equipment-Route Integration:** Mountain routes requiring climbing equipment
2. **Transport Compatibility:** Cart/boat restrictions, equipment size blocking
3. **Contract Equipment Requirements:** Multi-category equipment prerequisites
4. **Location-Based Discovery:** Equipment-gated information gathering
5. **Inventory Constraints:** Size-based strategic trade-offs
6. **Time Block Scheduling:** NPC availability and transport departures

## POC Content Strategy

### Core Design Principle
Create the minimum viable content that demonstrates **meaningful strategic trade-offs** where every decision involves sacrificing something valuable (time vs money vs equipment vs opportunities).

### Target Player Experience
*"I have 5 inventory slots and need climbing equipment for the mountain contract, but carrying it blocks boat transport to the port city where the high-value trading opportunity is. Do I specialize for mountain exploration or maritime trade?"*

## Implementation Plan

### 1. Locations (5-7 total)
**Strategic Hub Design:**
- **Millbrook** (starting town) - Tutorial area, basic services
- **Eastport** (port city) - Maritime routes, exotic goods, boat-only access
- **Ironhold** (mountain village) - Climbing-required access, mining contracts
- **Crossbridge** (trading post) - Route intersection, transport hub
- **Ancient Ruins** (hidden) - Exploration contracts, information-gated discovery

**Strategic Purpose:** Create route planning decisions where equipment choices affect destination accessibility.

### 2. Items (8-10 total, covering all equipment categories)
**Complete Equipment Categories:**
- Climbing_Equipment: Rope (1 slot), Climbing Harness (2 slots)
- Water_Transport: Ferry Pass (1 slot), Maritime Charts (1 slot)
- Navigation_Tools: Compass (1 slot), Survey Kit (2 slots)
- Weather_Protection: Travel Cloak (1 slot)
- Social_Signaling: Merchant Papers (1 slot), Noble Seal (1 slot)
- Light_Source: Lantern (1 slot)

**Strategic Purpose:** Force inventory trade-offs where equipment loadout determines route access and contract eligibility.

### 3. Routes (8-10 total)
**Transport Method Coverage:**
- Walking paths (flexible, free, 3 slots)
- Cart routes (scheduled, blocked by mountains, 5 slots)
- Boat routes (weather dependent, water only, 4 slots)
- Caravan routes (expensive, scheduled, 6 slots)

**Strategic Purpose:** Create transport decisions where equipment affects route access and timing.

### 4. Contracts (8-10 total)
**Progressive Complexity:**
- **Simple:** Herb delivery (no requirements, 3 coins)
- **Equipment-Gated:** Mountain survey (climbing equipment required, 25 coins)
- **Multi-Category:** Noble audience (social + documentation required, 50 coins)
- **Information-Gated:** Ruins exploration (location secrets required, 80 coins)

**Strategic Purpose:** Equipment investment drives contract access and earnings potential.

### 5. NPCs (6-8 total)
**Specialized Roles:**
- Magnus (Tavern) - Route information, evening only
- Elena (Market) - Trade intelligence, market hours
- Master Erik (Workshop) - Equipment commissioning, workshop hours
- Captain Seaworth (Port) - Maritime contracts, tide dependent
- Scholar Aldric (Library) - Ancient knowledge, appointment only

**Strategic Purpose:** Time-based information gathering requiring schedule coordination.

### 6. Information (8-10 total)
**Strategic Intelligence:**
- Route conditions (weather, equipment needs)
- Market prices (trade opportunities)
- Hidden locations (secret access)
- Professional techniques (efficiency gains)

**Strategic Purpose:** Equipment-gated discovery with economic value and expiration.

### 7. Location Spots (10-12 total)
**Equipment-Gated Access:**
- Workshop interior (requires trade tools)
- Library archives (requires documentation)
- Port authority (requires maritime papers)
- Mountain overlook (requires climbing equipment)

**Strategic Purpose:** Location exploration requires strategic equipment choices.

### 8. Actions (10-12 total)
**System Integration:**
- Equipment commissioning (workshop + trade tools)
- Route intelligence (tavern + evening + silver cost)
- Market analysis (marketplace + documentation + trade samples)
- Contract negotiation (various NPCs + social requirements)

**Strategic Purpose:** Connect all systems through equipment, time, and resource requirements.

## Expected Strategic Gameplay

### Decision Points Created
1. **Equipment Loadout:** "Climbing gear for mountain access vs trade tools for commerce contracts?"
2. **Transport Planning:** "Expensive caravan to make deadline vs walking with equipment risk?"
3. **Time Management:** "Gather route intelligence vs immediately depart?"
4. **Inventory Trade-offs:** "Carry trade goods vs keep equipment flexibility?"
5. **Information Investment:** "Pay for market intelligence vs discover through exploration?"

### System Interactions
- **Equipment → Routes:** Climbing gear unlocks mountain paths
- **Size → Transport:** Large items block certain transport methods
- **Time → NPCs:** Schedule coordination affects information access
- **Reputation → Contracts:** Social standing gates high-value opportunities
- **Information → Discovery:** Intelligence enables hidden location access

## Implementation Order

1. **Locations:** Expand to 5-7 strategic locations
2. **Items:** Complete equipment categories (8-10 items)
3. **Routes:** Connect locations with strategic transport choices
4. **NPCs:** Place specialists with time-based availability
5. **Contracts:** Create equipment-gated progression
6. **Information:** Establish location-based discovery economy
7. **Location Spots:** Enable equipment-gated exploration
8. **Actions:** Connect all systems through strategic choices

## Success Metrics

### Player Experience Validation
- Players discuss equipment trade-offs in decision-making
- Multiple viable approaches exist for achieving goals
- Time and resource allocation become optimization puzzles
- Equipment categories drive strategic specialization choices

### System Integration Verification
- All equipment categories have strategic purpose
- Transport compatibility creates meaningful constraints
- Location access requires deliberate preparation
- Information gathering involves equipment and time investment

## Next Steps

1. Document this plan in CLAUDE.md for permanent reference
2. Begin implementation with locations.json expansion
3. Systematically implement each content category
4. Test strategic decision-making at each step
5. Validate system integration creates intended gameplay

This POC will demonstrate how our verified game systems create deep strategic gameplay through minimal but well-designed content that forces meaningful optimization decisions.