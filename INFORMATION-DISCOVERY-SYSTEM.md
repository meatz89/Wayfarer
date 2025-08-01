# Information Discovery System

## Overview

The Information Discovery System is a core progression mechanic in Wayfarer that creates depth through hidden knowledge and gated access. Unlike traditional "unlock" systems, this creates a two-phase progression where players must first learn that something exists, then earn the right to interact with it.

## Core Principles

### Two-Phase Discovery

1. **Learn Existence** - Discover that content exists through:
   - NPC conversations mentioning people, places, or opportunities
   - Letters containing hints or references
   - Exploration revealing new locations
   - Special information letters with explicit revelations

2. **Gain Access** - Earn the right to interact through:
   - Building sufficient tokens with gatekeepers
   - Obtaining special letters (Introduction, Permit, Endorsement)
   - Meeting tier requirements
   - Having necessary resources or equipment

### Not Mechanical Unlocks

**CRITICAL**: This is NOT a traditional unlock system where reaching X tokens suddenly makes Y appear. Instead:
- NPCs introduce you to their friends through dialogue
- Locations are revealed through conversation and letters
- Access is granted through narrative reasons, not arbitrary thresholds
- Players understand WHY they gained access, not just that they did

## Implementation Architecture

### Discovery States

Every discoverable entity has three states:

```csharp
public enum DiscoveryState
{
    Unknown,      // Player has never heard of it
    Known,        // Player knows it exists but can't access
    Accessible    // Player can interact with it
}
```

### Discovery Tracking

```csharp
public class DiscoveryTracker
{
    // Track what player has discovered
    Dictionary<string, DiscoveryState> NPCDiscovery;
    Dictionary<string, DiscoveryState> LocationDiscovery;
    Dictionary<string, DiscoveryState> MechanicDiscovery;
    
    // Track how discoveries were made
    Dictionary<string, string> DiscoverySource; // "Learned from Elena"
}
```

## Special Letters as Access Mechanisms

### Introduction Letters (Trust)
- **Purpose**: Formally introduce player to new NPCs
- **Mechanics**: Must be delivered to target NPC to gain access
- **Example**: "Elena writes to introduce you to her colleague Sarah"
- **UI**: Shows introduction text when delivered, NPC becomes accessible

### Access Permits (Commerce)
- **Purpose**: Grant entry to restricted commercial locations
- **Mechanics**: Acts as a key for tier 2+ markets and shops
- **Example**: "Merchant Guild permit for the Exclusive Bazaar"
- **UI**: Location becomes enterable when permit is in inventory

### Endorsements (Status)
- **Purpose**: Vouch for player's social standing
- **Mechanics**: Required for noble events and high-society locations
- **Example**: "Lord Pemberton's endorsement for the Governor's Ball"
- **UI**: Must maintain positive Status tokens to keep access

### Information Letters (Shadow)
- **Purpose**: Reveal hidden knowledge
- **Mechanics**: Reading the letter updates discovery state
- **Example**: "The real power in this city meets at midnight..."
- **UI**: New locations/NPCs appear on map after reading

## Tier System Integration

### Triple-Gated Access

Everything has tiers 1-5 with three gates:

1. **Knowledge Gate** - Must discover it exists
   - Tier 1: Common knowledge, easily discovered
   - Tier 5: Deep secrets requiring extensive investigation

2. **Permission Gate** - Must have access rights
   - Tier 1: Open to all once discovered
   - Tier 5: Requires multiple endorsements or rare permits

3. **Capability Gate** - Must have resources/skills
   - Tier 1: Basic requirements (coins, stamina)
   - Tier 5: Extensive requirements (equipment, tokens, reputation)

### Progressive Reveal

- **Tier 1 Content**: Visible immediately once discovered
- **Tier 2-3 Content**: Shows requirements when hovering
- **Tier 4-5 Content**: Mystery indicators until prerequisites partially met

## UI Implementation Requirements

### Discovery Indicators

```razor
@if (discovery.State == DiscoveryState.Unknown)
{
    <!-- Don't show anything -->
}
else if (discovery.State == DiscoveryState.Known)
{
    <div class="known-but-inaccessible">
        <span class="name">@discovery.Name</span>
        <span class="lock-icon">🔒</span>
        <div class="requirements">
            @foreach (var req in discovery.Requirements)
            {
                <span class="requirement @(req.Met ? "met" : "unmet")">
                    @req.Description
                </span>
            }
        </div>
    </div>
}
else // Accessible
{
    <div class="accessible">
        <span class="name">@discovery.Name</span>
        <span class="tier">Tier @discovery.Tier</span>
    </div>
}
```

### Conversation Integration

NPCs should naturally mention discoveries:

```csharp
// In conversation logic
if (playerTokens >= 3 && !HasDiscovered("exclusive_merchant"))
{
    AddDialogue("You know, there's a merchant who deals in rare goods. " +
                "But she only works with those who have proper permits...");
    MarkAsDiscovered("exclusive_merchant", DiscoveryState.Known);
}
```

### Letter Integration

Letters can trigger discoveries:

```csharp
// When delivering certain letters
if (letter.Type == "Introduction" && letter.Delivered)
{
    var targetNPC = letter.MetaData["TargetNPC"];
    UpdateDiscovery(targetNPC, DiscoveryState.Accessible);
    AddNotification($"You've been introduced to {targetNPC}!");
}
```

## Gameplay Impact

### Strategic Depth

- Players must actively seek information
- Building relationships becomes investment in future opportunities
- Letters gain value beyond just payment
- Exploration rewards attention to dialogue

### Emergent Storytelling

- Each player's discovery path is unique
- Relationships determine what content you access
- Missed opportunities create meaningful choices
- Information becomes a valuable resource

### Natural Progression

- No arbitrary level gates or XP requirements
- Progress tied to actions and relationships
- Clear cause-and-effect for access
- Rewards curiosity and attention

## Anti-Patterns to Avoid

### Mechanical Unlocks
❌ "Reach 5 tokens to unlock Sarah"
✅ "Elena introduces you to Sarah when she trusts you"

### Silent Appearances
❌ New NPC suddenly appears in list
✅ Introduction scene with dialogue explaining connection

### Unclear Requirements
❌ Location locked with no explanation
✅ "Requires Commerce Permit from Merchant Guild"

### Breaking Immersion
❌ "Achievement Unlocked: Found Secret Shop"
✅ Shadow contact whispers location during conversation

## Testing Considerations

### Discovery Flow Testing
- Verify discoveries trigger at correct moments
- Ensure access requirements are clearly communicated
- Test that special letters grant appropriate access
- Validate tier progression makes sense

### Save/Load Integrity
- Discovery states must persist across saves
- Special letters in inventory maintain their access rights
- Partially met requirements remain tracked

### UI Clarity
- Unknown content truly hidden
- Known content shows clear requirements
- Accessible content easily distinguishable
- Tooltips provide helpful information

## Future Enhancements

### Dynamic Discovery
- NPCs share different information based on context
- Time-sensitive discoveries that expire
- Discoveries that require multiple sources
- False information and misdirection

### Discovery Chains
- Multi-step investigation paths
- Combining information from multiple sources
- Discovery prerequisites creating complex webs
- Faction-specific discovery paths

### Information Trading
- Sell discoveries to interested parties
- Information as currency for Shadow dealings
- Exclusive information creating monopolies
- Broker relationships through shared knowledge

## Conclusion

The Information Discovery System transforms Wayfarer from a game about delivering letters into one about building relationships and uncovering secrets. By making information itself a gated resource, we create emergent gameplay where every conversation matters and every relationship is an investment in future opportunities.