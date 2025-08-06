# Wayfarer Game Design

## Token System Overview

The game uses four connection token types that represent HOW you relate to NPCs:
- **Trust** - Personal bonds and emotional connections
- **Commerce** - Business relationships and trade networks  
- **Status** - Social standing and noble connections
- **Shadow** - Underground and illicit connections

Tokens represent relationship context, not NPC identity. An NPC might grant different token types based on how you interact with them.

## Information Discovery System

Two-phase progression for all game content:
1. **Learn Existence** - Discover that something exists (NPCs, locations, mechanics)
2. **Gain Access** - Earn the right to interact through tokens, permits, or capabilities

## Special Letters

Four types provide unique progression mechanics:
1. **Introduction Letters** (Trust) - Meet new NPCs in trust networks
2. **Access Permits** (Commerce) - Enter restricted commercial locations
3. **Endorsements** (Status) - Access high-society venues
4. **Information Letters** (Shadow) - Learn hidden knowledge

## Tier System

Everything has tiers 1-5 with triple-gated access:
1. **Knowledge Gate** - Must know it exists
2. **Permission Gate** - Must have access rights
3. **Capability Gate** - Must have resources/skills

## Standing Obligations

Dynamic debt mechanics per token type:
- **Trust Debt** - Personal betrayals affecting deadlines
- **Commerce Debt** - Business leverage affecting positions
- **Status Debt** - Social obligations restricting refusals
- **Shadow Debt** - Dangerous entanglements

---

## Original Game Design Notes

Great. That's a good tutorial. Looking at the system interactions, what's left need to make this a real "game". I'm not talking about blinking lights or great story. I'm also not talking about winning or losing or end game. 

I just want to have a vertical slice, a small starting area after the tutorial. What's important to me is that the player can only do a limited number of actions and can only visit a limited number of locations. But the player should still see what progression paths there are, like special characters, special locations, new action options, level up and so on, and should have some understanding of how to reach it. 

Progressing in those paths should not be trivial, it shouldn't be just an obvious sequence of actions that get the player there, I want the player to have to think and strategize with the tools given to him, how he can progress. But it must not be static either. Each action the player takes should ripple through the systems in such a way to make him change his plans and having to react to the new game state while still trying to progress, or risk losing the game. This to me is what a game is about. 

What i want:
- Limited actions/locations (vertical slice)
- Non-trivial progression (not just do A then B then C)
- Limited actions/locations but visible progression paths that aren't immediately accessible
- Risk of failure/setback
- Players must strategize to progress
- Actions create ripple effects that force adaptation
- Strategic thinking required to progress (not just following obvious steps)
- Dynamic game state that changes based on player actions

Successful games that do this:
- Yes, Your Grace
- dotAGE
- Banner Saga
- Roadwarden
- The Life and Suffering of Sir Brante
- Cultist Simulator: Limited actions but each changes the board state, creating cascading effects
- FTL: Limited resources force hard choices, each jump changes situation, Limited actions per jump, but each choice cascades
- This War of Mine: Resource scarcity, relationship management, every choice matters
- Slay the Spire: Limited energy, but card combinations create depth
- Papers Please: Daily pressure with long-term goals, every decision affects future state
- 80 Days: Route optimization with time pressure and resource management

Key design patterns that make these work:
- These games create depth through COMPETING OBJECTIVES that use the SAME RESOURCES, not through adding more systems.
- Visible Goals with Hidden Complexity: You can see what you want but not how to get there
- Resource Scarcity: Never enough to do everything
- Competing goals - multiple things you want but can't have all
- Cascading consequences - each choice affects future options
- Multiple viable strategies - but each has trade-offs
- Time pressure that makes every action count
- Discovery/unlock mechanics that open new strategies
- Failure states that force restart/adaptation
- Dynamic Obstacles: Game state changes, forcing adaptation
- Visible but gated progression - See what you can't have yet

What's missing from Wayfarer for a real "game":
- Competing Goals: Multiple desirable outcomes that conflict
- Competing Pressures: Multiple urgent needs at once
- Synergies: Combinations that are more than sum of parts
- Stakes: Something to lose beyond individual letters
- Resource bottlenecks that force hard choices
- Dynamic pressures that disrupt plans
- Cascading Time Pressure: Every letter has deadlines, but new urgent letters keep arriving
- Relationship Decay: Unused relationships don't magically deteriorate, but force difficulty through letters. if letters are ignored, relationship decays automatically through our already setup systems
- Economic Pressure: Rent/food/equipment costs force constant income
- Progression Gates: Clear goals that require specific achievements
- Strategic Trade-offs: Every gain costs something else
- A clear mid-term goal (visible but challenging)
- Multiple paths to that goal (strategic choice)
- Failure states that teach lessons
- Every "solution" creates new problems
- Every player chosen progression creates future problems
- Player has to overextend himself to be able to reach the next progression stage (items, locations, level up, larger queue). each progression makes his life easier, so the player will naturally gravitate towards the progression and overextend himself

What i don't want:
- no NPC "rivals"
- no "traps" (low pay, bad position, no tokens)
- no "maintenance" (i.e. equipment)
- no predefined "progression paths" that are just content and not mechanics
- no artificial resource "decays". instead of relationship decaying artificially, we can spawn a "havent heard from you in a long time" letter, clogging up the queue. this way it wont feel artificial and not immediately punish the player but the player has a CHOICE. if he can't deliver it, we get the same effect we wanted, relationship loss, but through established systems instead of artificial rules.

I want a game that creates strategic depth through competing objectives using the same limited resources, not through additional systems. Use existing systems to create cascading pressure, not new decay mechanics.

Think for 60 minutes. Let's plan deeply, step back and study how others do it, and try to apply it to wayfarer. don't only generate content, but rules