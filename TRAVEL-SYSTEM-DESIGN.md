# Wayfarer Travel System Design
## Routes, Permits, and Transport NPCs

### Core Concept
Travel in Wayfarer is not just movement between locations - it's a core progression mechanic. Routes connect locations, and each route has specific requirements, costs, and transport methods. Like in "80 Days", the journey matters as much as the destination.

## Route System Architecture

### Route Types
1. **Basic Walking Routes** - Always available, slow, cheap
2. **Carriage Routes** - Faster, costs money, limited schedule
3. **Boat Routes** - Access to water-connected locations, weather-dependent
4. **Restricted Routes** - Requires permits or special relationships
5. **Secret Routes** - Must be discovered through gameplay

### Route Properties
```csharp
class RouteOption {
    string Id
    string Origin
    string Destination
    TravelMethods Method // Walking, Carriage, Boat, etc.
    int TravelTimeHours
    int StaminaCost
    int CoinCost
    bool IsDiscovered // Player knows about it
    bool RequiresPermit // Needs travel permit letter
    string TransportNPCId // NPC who operates this route
    Schedule DepartureSchedule // When available
    int MaxLuggageWeight // How many letters you can carry
}
```

## Travel Permits - Special Letters

### What Are Travel Permits?
Travel permits are a special type of letter that grant access to restricted routes. Unlike normal letters that are delivered to recipient NPCs for tokens/payment, travel permits are "delivered" to Transport NPCs at departure locations to unlock routes.

### How Permits Work
1. **Acquisition**: Player receives permit letter like any other letter
2. **Decision Point**: Keep for route access OR deliver for normal rewards
3. **Usage**: "Deliver" to Transport NPC at departure location
4. **Effect**: Permanently unlocks that route for future use

### Example Permit Flow
```
1. Accept letter: "Harbor Master's Permit" from Noble
2. Letter shows recipient: "Captain Morris at Riverside Docks"
3. Player travels to Riverside Docks
4. Finds Captain Morris (Transport NPC)
5. "Delivers" permit through conversation
6. Captain Morris: "Ah, the Harbor Master's seal! You can sail with me now."
7. Route Unlocked: Riverside → Island District (Boat, 2 hours, 5 coins)
```

## Transport NPCs

### Role in the Game
Transport NPCs are special NPCs stationed at departure locations who control access to specific routes. They are the gatekeepers of advanced travel options.

### Types of Transport NPCs
1. **Carriage Drivers** - Found at coaching inns
2. **Boat Captains** - Found at docks/harbors
3. **Gate Guards** - Control access to restricted districts
4. **Caravan Masters** - Organize group travel to distant locations
5. **Ferry Operators** - Run regular water crossings

### Transport NPC Properties
```csharp
class TransportNPC : NPC {
    List<string> ControlledRoutes // Routes this NPC operates
    Schedule OperatingHours // When they run their service
    List<string> AcceptedPermits // Permit types they recognize
    int RelationshipRequirement // Minimum tokens needed for special rates
    Dictionary<string, int> SpecialPricing // Discounts for high relationship
}
```

### Interaction Examples

#### Without Permit
```
Player: "I need passage to the Noble District"
Carriage Driver: "That's a restricted route. You'll need a permit from the Magistrate."
```

#### With Permit
```
Player: [Deliver Magistrate's Travel Permit]
Carriage Driver: "Everything's in order. You can ride my carriage anytime now."
[Route Permanently Unlocked]
```

#### With High Relationship
```
Player: "Any chance of a discount?" [Trust: ●●●●○]
Boat Captain: "For you, friend? Half price."
[Route Cost: 10 coins → 5 coins]
```

## Route Discovery & Progression

### Discovery Methods
1. **Exploration** - Spend time exploring locations to find routes
2. **Information Trading** - NPCs reveal routes through conversation
3. **Permits** - Unlock restricted routes
4. **Relationship Building** - Transport NPCs reveal routes to trusted players
5. **Letter Content** - Some letters contain route information

### Progression Example
```
Day 1: Only walking routes available (slow, exhausting)
Day 2: Discover carriage route through exploration
Day 3: Receive permit letter - must choose: keep or deliver?
Day 4: Use permit, unlock boat route
Day 5: High trust with boat captain, get discount rates
Day 6: Captain reveals secret night route to smuggler's cove
```

## Strategic Decisions

### Permit Dilemmas
- **Keep Permit**: Unlock valuable route for optimization
- **Deliver Normally**: Get tokens/payment, maintain relationships
- **Trade/Sell**: Some NPCs might buy permits from you

### Route Optimization
- **Time vs Cost**: Fast routes cost more
- **Weight Limits**: Boats/carriages can carry more letters
- **Schedule Planning**: Some routes only available at certain times
- **Weather Dependencies**: Some routes blocked by weather

### Network Effects
- Building relationships with Transport NPCs creates a network
- One transport NPC might recommend you to another
- Chain of permits can unlock entire regions

## Implementation Priority

### Phase 1 (MVP)
- Basic walking routes between all locations
- One carriage route with coin cost
- Simple permit system (binary: have/don't have)
- 2-3 Transport NPCs

### Phase 2 (Enhanced)
- Full schedule system for transport
- Weather effects on routes
- Relationship-based pricing
- Route discovery through exploration

### Phase 3 (Full)
- Complex permit chains
- Seasonal routes
- Group travel with other NPCs
- Route-specific encounters

## Content Requirements

### Minimum Viable Content
- 5 Locations
- 10 Routes (mix of walking/carriage/boat)
- 3 Transport NPCs
- 2 Travel Permits as special letters

### Full Game Content
- 10+ Locations
- 25+ Routes with various methods
- 8+ Transport NPCs with personalities
- 5+ Permit types with different access levels

## UI/UX Considerations

### Route Display
- Show all discovered routes from current location
- Clear indicators for:
  - Locked routes (grayed out)
  - Available routes (highlighted)
  - Requirements not met (red text)
  - Schedule constraints (clock icon)

### Permit Management
- In queue, permits show special icon
- Tooltip explains: "Can be used to unlock routes"
- At Transport NPC, special dialogue option appears

### Progress Feedback
- "New Route Discovered!" notifications
- Map updates to show new connections
- Transport NPC relationship indicators

## Narrative Integration

### Story Through Routes
Each route tells a story:
- Why is this route restricted?
- Who uses this route normally?
- What's the history of this path?

### Character Through Transport NPCs
Transport NPCs are full characters:
- Boat captain who lost his license (needs permit to sail legally)
- Carriage driver who only trusts certain people
- Ferryman who knows all the gossip

### World-Building Through Permits
Permits reveal power structures:
- Who has authority to grant access?
- What are they trying to control?
- How does the permit system reflect social hierarchy?

## Balance Considerations

### Prevent Exploitation
- Permits are one-time use
- Can't duplicate or forge permits
- Some routes require BOTH permit AND payment

### Maintain Pressure
- Even with fast routes, time is limited
- Better routes often have limited schedules
- Weather can block routes unexpectedly

### Reward Investment
- Players who keep permits get long-term efficiency
- Building transport NPC relationships pays off
- Discovery creates permanent advantages

## Success Metrics

### System Working When:
1. Players agonize over keeping vs delivering permits
2. Route discovery feels like meaningful progression
3. Transport NPCs become memorable characters
4. Optimizing travel routes is key strategy
5. Each playthrough can have different route unlocks

### Red Flags:
1. All routes unlocked too quickly
2. Permits always kept, never delivered
3. Transport NPCs feel like vending machines
4. Routes don't meaningfully change gameplay
5. No reason to explore once routes known

## Technical Notes

### State Management
- Route discovery state persists between days
- Permit usage is tracked permanently
- Transport NPC relationships carry over

### AI Considerations
- Transport NPCs need personality templates
- Permit generation should consider player's needs
- Route revelation should feel organic

### Testing Priorities
1. Permit delivery actually unlocks routes
2. Transport NPC schedules work correctly
3. Route requirements properly enforced
4. Save/load preserves route states
5. UI clearly shows route availability

---

## Summary
The travel system in Wayfarer is not just about movement - it's about discovery, relationships, and strategic choices. Travel permits create meaningful decisions, Transport NPCs add personality to travel, and route optimization becomes a core skill. Like choosing between trains and ships in "80 Days", every route choice matters.