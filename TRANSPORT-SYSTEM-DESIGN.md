# Transport System Design - How It's SUPPOSED to Work

## Core Principle: Routes ARE Transport Methods

**CRITICAL**: Routes do NOT have selectable transport methods. Each route IS a specific way to travel between locations.

## The Correct Mental Model

Think of routes like real-world travel options:
- "Take the highway" (driving route)
- "Take the ferry" (boat route)
- "Walk through the park" (walking route)
- "Take the subway" (underground route)

You don't "take the highway by boat" or "take the ferry by walking" - that's nonsensical!

## How Routes Work in Wayfarer

### 1. Each Route Has a Fixed Transport Method

From `routes.json`:
```json
{
  "id": "docks_guild_ferry",
  "name": "Guild Ferry Service",
  "method": "Boat",  // <-- This IS the transport method!
  "description": "A comfortable ferry service..."
}
```

### 2. Route Names Reflect Their Transport Method

- **"Guild Ferry Service"** - It's a FERRY, which means BOAT
- **"Market Shortcuts"** - Secret paths through alleys, which means WALKING
- **"Merchant Avenue"** - Main road, could be WALKING or CARRIAGE
- **"Scholar's Library Route"** - Quiet path, likely WALKING

### 3. Different Routes Exist for Different Transport Options

Between the same two locations, you might have:
- "Dockside Path" - Walking route (free, takes stamina)
- "Guild Ferry Service" - Boat route (costs coins, no stamina)
- "Market Shortcuts" - Walking through alleys (requires tokens, fast)

## What Was Wrong with Transport Selection

The broken UI was asking players to choose HOW to use a route:
1. Select "Market Shortcuts" route
2. Choose transport: Walking/Horseback/Carriage/Cart/Boat (?!?)
3. Travel

This makes no sense because:
- Market Shortcuts are narrow alleys - you can't take a carriage!
- Guild Ferry is a boat service - you can't walk on water!
- Each route already defines its transport method

## The Correct UI Flow

1. Show available routes with their transport methods
2. Player selects a route (which includes the transport method)
3. Travel happens using that route's defined method

Example display:
```
Routes to Merchant's Rest:
- Merchant Avenue (🚶 Walking) - 2 hours, 2 stamina
- Guild Ferry Service (⛵ Boat) - 1 hour, 1 coin
- Market Shortcuts (🚶 Walking) - 1 hour, 1 stamina, requires Commerce tokens
```

## Technical Implementation

### Route Data Structure
```csharp
public class RouteOption
{
    public string Id { get; set; }
    public string Name { get; set; }
    public TravelMethods Method { get; set; }  // Fixed transport method
    // ... costs, requirements, etc.
}
```

### UI Components
- **TravelSelection.razor** - Shows routes with their transport methods
- **NO TransportSelector** - This component should not exist!
- Routes display as: "Route Name (🚶 Walking)"

### Travel Execution
```csharp
// Correct
await ExecuteTravel(route);  // Route already has its method

// Wrong
await ExecuteTravelWithTransport(route, selectedTransport);  // NO!
```

## Common Misconceptions to Avoid

### ❌ WRONG: "Players should choose transport for flexibility"
Routes represent specific paths/services. A ferry is always a boat. Market alleys are always walked.

### ❌ WRONG: "Transport selection adds strategic depth"
Strategic depth comes from choosing between different routes, not changing how a route works.

### ❌ WRONG: "All routes should support all transport methods"
This breaks verisimilitude. You can't take a horse through narrow market stalls.

### ✅ RIGHT: "Different routes offer different trade-offs"
- Walking routes: Free but cost stamina
- Ferry routes: Cost coins but save stamina
- Shortcut routes: Require tokens but save time

## Game Design Alignment

This design aligns with core Wayfarer principles:
- **Verisimilitude**: Routes work like real-world travel options
- **No Special Rules**: Each route consistently uses its method
- **Emergent Gameplay**: Choose between routes based on resources/needs

## Testing Checklist

When implementing travel:
- [ ] Routes display their transport method
- [ ] No transport selection UI exists
- [ ] Travel uses the route's defined method
- [ ] Route names make sense for their transport type
- [ ] Players understand the trade-offs between routes

## Red Flags in Code

If you see any of these, something is wrong:
- `ShowTransportSelector` or `ShowTransportOptions`
- `OnTransportSelected` callbacks
- `TransportSelector` component
- Routes without a defined `method` property
- UI asking players to choose transport after selecting a route

## Summary

**Routes ARE transport methods. You don't choose how to use a route - the route IS the how.**

This is like asking "How do you want to take the subway? By car? By boat?" It's nonsensical. The subway IS the transport method.