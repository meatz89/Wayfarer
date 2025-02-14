# Introduction and Goals

"Wayfarer" is a medieval life simulation where you play as an ordinary traveler making their way in the world. No epic quests or chosen one narrative - just the authentic challenges of survival, relationships, and finding your place in a grounded medieval setting.

Core Gameplay Loop:
1. Players move between locations (inns, markets, docks, etc.)
2. At each location, they find spots where they can take actions (like the bar in a tavern)
3. Taking an action starts an encounter
4. Each encounter presents choices that determine success/failure
5. Success brings rewards (coins, knowledge, relationships)
6. These rewards enable accessing new locations and opportunities

Current Systems:
- Four encounter types: Physical, Focus, Negotiation, and Relationship
- Each encounter tracks success progress and failure progress
- Players spend energy (physical, focus, social) on choices
- Encounter stages have "tags" that affect what choices are available
- Player skills unlock better choices in encounters
- Status effects modify encounter difficulty

The game revolves around the life of a wayfarer - taking odd jobs, building relationships, uncovering opportunities. Each location offers different encounters through its spots (like bars, back alleys, merchant stalls). 

Core Systems:
1. Player Stats
- Health (injury risk)
- Energy Types (physical/focus/social)
- Reputation (how you're seen)
- Skills (unlock better choices)

Encounter Structure
- Every action triggers an encounter
- Success/Failure progress tracked
- Takes multiple choices to complete
- Energy spent per encounter, not per choice
- Skills unlock powerful choices

Tag System Tags represent the current narrative state mechanically:
- FATIGUED shows physical strain
- SUSPICIOUS marks distrust
- LEVERAGE indicates advantage
- etc.


# Core System Rules

## Encounter Structure
- Momentum Range: -5 to +5
- Starting Momentum: 0
- Success Threshold: +5
- Failure Threshold: -5
- Pressure Range: +0 to +10
- Pressure Increase: +1 after each player choice
- Pressure Threshhold: +10

## Location Properties
- Each location spot has 3-4 properties

## Tag System
- Tags are binary (present/absent)
- Tags are unique and not related to each other
- Tags can be added or removed by choices, but not directly
- Location properties determine starting tags
- Tags modify momentum gains/losses
- Tags modify pressure gains/losses
- Tags enable/disable certain choices

## Encounter Types
- Physical
- Focus
- Social
- Each type weights certain action types more heavily

## Skills Knowledge and Relations
- Don't directly affect encounters
- Unlock special choices when tag requirements met
- Special choices always available if requirements met

# Encounter system for the medieval life simulation game "Wayfarer":

The initial design challenge was creating meaningful encounters that reflect authentic medieval life through mechanical systems. The core game has encounters based on momentum (-5 to +5), with success at +5 and failure at -5. Each encounter involves the player selecting from generated choices, with progress tracking and tag systems affecting outcomes.
We refined this system by focusing on how location properties and encounter tags interact to create authentic, strategic gameplay. For example, when a player attempts to haggle for a room price (rather than paying the full 5 coins), they enter a social encounter where each choice they make generates both immediate momentum changes and potential tag effects. The brilliance of the system emerges from how location properties determine the response to these choices.

# Location Properties as Adversaries:

Each location has specific properties that define not just difficulty, but behavioral patterns. A REPUTABLE property, for example, has a clear logic:
- Always reduces momentum by 1 after any player choice
- Doubles its reduction if the AGGRESSIVE tag is present
- Ignores other tags

This creates a "personality" for the location that players must understand and work with. The property defines both what the location cares about and how it reacts to different approaches.

# Interplay between location properties and encounter tags

So the key here is the interplay between location properties and encounter tags. The location properties determine the response after each player choice. Each location property has a different effect. Each encounter tag, positive and negative, influences the behavior of the location properties and response. This means in practice that in one location haggling action, the aggressive approach is the best because the location property does not react to the tag added by the aggressive choice but in another locations haggling action, aggressive choices are always a bad choice because of the interplay of the location property and the encounter tag.

As a Fresh RPG System:
- Replaces traditional combat mechanics with universal interaction rules
- Location properties function like unique "enemy AI patterns"
- Tags create state changes similar to status effects
- Skills unlock strategic options rather than raw power increases

For Player Development:
- Learning new skills gives access to tag manipulation choices
- Understanding location properties becomes a form of mastery
- Knowledge of property-tag interactions enables advanced strategies
- Players can develop different approaches for different situations

For Narrative Authenticity:
- Forces players to "read the room" like in real social situations
- Different locations having different "personalities" feels natural
- Success requires adapting approach to the context
- Failure comes from misreading situations, not just bad luck

For Strategic Depth:
- Clear rules allow for intentional strategy
- Property-tag interactions create interesting puzzles
- Multiple valid approaches depending on context
- Ability to set up advantageous situations through tag manipulation

For AI Generation:
- Tags provide clear mechanical state for narrative generation
- Property behaviors guide appropriate tone and content
- Success/failure flows naturally from player choices
- System creates coherent situations for AI to describe

## Examples

DESPERATE tag effects:
- At AFFLUENT locations: Reduce momentum by 2
- At HARDENED locations: No effect (they're used to desperate people)
- At REPUTABLE locations: All choices count as aggressive
- At REGULATED locations: Adds SUSPICIOUS tag after each choice

This creates rich strategic implications - being desperate at a rough dockside tavern (HARDENED) isn't a problem, but the same state at a wealthy merchant's shop (AFFLUENT) or noble's estate (REPUTABLE) creates serious challenges.

ASSERTIVE tag effects:
- At REPUTABLE locations: Counts as aggressive, reduce momentum by 1
- At HARDENED locations: Prevents momentum reduction after choices
- At VIGILANT locations: Draws extra scrutiny, -1 momentum after each choice
- At TRADITIONAL locations: Seen as disrespectful, reduces momentum by 2

# Unkown Location Properties

A merchant's SHREWD property might first appear as ??? to the player. They could dive straight into haggling, but without understanding how the property reacts to different approaches, they're essentially flying blind. The encounter becomes not just about succeeding, but about learning - each reaction to their choices revealing something about how this merchant operates.

Alternatively, they could gather intelligence first. Maybe spending time observing the merchant's interactions with others through a careful encounter. Or talking to other merchants about their experiences. Or even taking a job as a porter in the market to gain insider knowledge. Each successful information-gathering action could reveal part of the property's behavior - first showing it's SHREWD, then revealing how it responds to specific tags.

This creates a beautiful risk-reward dynamic. Players must choose between:
- Immediate engagement with unknown properties (high risk, potentially high reward)
- Investment in gathering information (delayed reward, but strategic advantage)
- Using partially understood properties (calculated risks based on incomplete information)

The property discovery becomes almost like solving a puzzle. Each location reaction provides clues about its properties. A particularly harsh response to an AGGRESSIVE tag might suggest a REPUTABLE property. No reaction to forceful approaches might indicate a HARDENED property. Players can form theories and test them, with each encounter adding to their understanding.

Knowledge itself becomes valuable currency in this system. A player who invests time in understanding how different types of locations operate gains significant advantages. This creates natural progression - not just through mechanical bonuses, but through genuine mastery of the game's social dynamics.

# Equipped Item Effects
the player equipment works not through numerical stat boosts but by adding advantageous player status tags. These have a specific rule that either modify choices or generate a new choice if certain encounter state is reached

A merchant's signet ring doesn't just mark you as wealthy - it gives you the CREDENTIALED tag. When interacting with a location that has the REPUTABLE property, this tag might transform what would normally be a risky aggressive choice into a more acceptable "Assert Merchant Rights" option, reflecting how your status changes the social dynamics.

Well-worn work gloves could grant SEASONED_LABORER. At the docks, when encounter tags indicate mounting physical strain, this status might enable a special "Proper Technique" choice that helps manage fatigue more effectively than standard options.

A guard's badge might give you AUTHORITY. In encounters where negative tags like SUSPICIOUS accumulate, this status could generate a new "Show Badge" choice that helps defuse tension. The same badge, when dealing with a REGULATED property, might unlock choices that let you work within official channels more effectively.

This creates fascinating dynamics where equipment becomes about expanding your strategic options rather than just improving your numbers. Players choose their gear based on what kind of situations they expect to face and what properties they'll need to work with, making equipment choices feel more like tactical decisions than simple upgrades.


# System overview

An encounter represents a challenge, like negotiating with a merchant or unloading cargo. The encounter tracks three things: momentum (progress toward success), pressure (progress toward failure), and currently active encounter tags that modify the encounter state.

Each encounter has several location properties. Each property defines exactly what approach types (Assertive, Methodical, or Adaptive) will trigger tag additions when chosen, using three trigger rules: Primary adds a negative tag, Counter removes a negative tag, Boost adds a positive tag. These triggers create systematic, predictable relationships between player choices and tag changes.

When a player reaches a stage in the encounter, they receive 2-6 choices. Each choice has an approach type that defines its base momentum gain and pressure increase, plus requirements for when it can appear. The choice generation system checks current momentum and pressure against each approach type's requirements to determine available choices.

When the player selects a choice, we process effects in this sequence:

* First apply the approach type's base momentum and pressure changes. Then check if the approach's category triggers any property rules. For each property in sequence: check if Primary Trigger adds a negative tag, then if Counter Trigger removes a negative tag, finally if Boost Trigger adds a positive tag. Each tag can only exist once - additions of already-present tags are ignored.
* Each encounter tag has exactly one effect on the encounter state. Some modify momentum gains (adding or subtracting from base values), some modify pressure increases, and some affect what approach types can appear in choice generation. These modifications happen after base calculations but before new choices are generated for the next stage.
* The encounter continues stage by stage until either momentum reaches the success threshold or pressure reaches the failure threshold. Each stage follows the same sequence: generate choices based on current state and requirements, player selects choice, apply momentum/pressure changes, check property triggers for tag changes, apply tag effects, repeat.

# Choice Generation

* For social encounters, we have approach types that represent different social strategies, each with fixed momentum/pressure values and clear requirements. For example, a "Charm" approach always gives +2 momentum and +1 pressure, and becomes available when any positive tag is present. A "Threaten" approach might give +3 momentum but +2 pressure, requiring momentum to be above 5 to become available - representing that threats work better from a position of strength.
* For physical encounters, approach types represent different ways of applying effort. A "Careful" approach might give +1 momentum with no pressure increase, always available as a safe option. A "Push Through" approach could give +3 momentum and +2 pressure, but only become available when pressure is below 5, representing that risky exertion is only viable when you're not already exhausted.
* For focus encounters, approaches represent different mental strategies. A "Methodical" approach might give +1 momentum with no pressure, always available. An "Intuitive Leap" could give +3 momentum and +1 pressure, but require a specific positive tag showing understanding of the problem.

Then the choice generation system looks at current encounter state and generates 2-6 choices from approaches whose requirements are met. This ensures players always have options while maintaining strategic depth through requirement gating.
After the player selects an approach, a separate system determines tag changes based on the approach type and location properties. This creates cleaner separation between choice mechanics and tag effects.

Each location property defines exactly three things:
1. One positive and one negative tag it controls
2. Which approach types will trigger negative tag addition
3. Which approach types will trigger positive tag addition

When a player selects a choice, we simply check if its approach type is on any property's negative or positive trigger list. If it is, that property's corresponding tag gets added (if not already present). Properties check their triggers independently, so one approach could trigger multiple properties.
For example, approach types for social encounters might include: Charm, Assert, Threaten, Plead, Reason, Boast, Deflect, Yield, Inspire. Each carries its fixed momentum/pressure values and requirements. When chosen, we check each property's trigger lists for that approach type.
To keep it systemic rather than narrative, we don't try to encode "personality" into properties. Instead, each property just maintains its trigger lists of approach types. This creates clear, deterministic relationships between approaches and tag changes while allowing different locations to react differently to the same approaches through different trigger list combinations.
The tag changes happen after momentum and pressure are applied but before the next stage's choices are generated. This creates a clean sequence: Choose approach -> Apply momentum/pressure -> Check property triggers -> Add/remove tags -> Generate next choices.

Let me walk through how we can translate this sequence into a deterministic rule system. This helps clarify how we can encode these interactions without any intelligence or optimization.

Choice Generation Rules:
1. Always attempt to fill exactly 4 slots
2. Each slot has fixed category assignment and stat check order:
   - Slot 1: Assertive 
   - Slot 2: Methodical (one momentum/pressure check)
   - Slot 3: Adaptive (sum momentum/pressure check)
   - Slot 4: Best ratio choice, but no duplicate allowed

For each slot, we follow this sequence:
1. Check if slot category is blocked by tags
2. If not blocked, find approach from that category matching current state values
3. Apply all active tag modifiers to momentum/pressure values
4. If slot remains empty, leave it empty (no smart backfilling)

Tag Processing Rules:
1. When choice is made, check each property in sequence for trigger matches
2. Apply triggered tag additions/removals in property order
3. Calculate final momentum/pressure by:
   - Start with approach base values
   - Apply all momentum modifying tags in order added
   - Apply all pressure modifying tags in order added

Choice Selection Within Categories:
- Assertive choices determined by momentum value
- Methodical choices determined by pressure value
- Adaptive choices determined by momentum + pressure sum
- No optimization or comparison between choices

This creates a purely mechanical system where:
1. Every stage starts with fixed slot assignments 
2. Tags modify or block choices through explicit rules
3. Properties process triggers in strict sequence
4. Tag effects apply in order of addition
5. Final values calculate through fixed steps

# Location Properties

First, let's establish our ruleset for how approach types interact with properties. Each approach type fits into exactly one of three trigger categories: Assertive, Methodical, or Adaptive. These categories create mechanical relationships rather than narrative ones.

Each location property defines its triggers using these categories:
1. Primary Trigger - When an approach from this category is chosen, the property adds its negative tag
2. Counter Trigger - When an approach from this category is chosen, the property removes its negative tag if present
3. Boost Trigger - When an approach from this category is chosen while no negative tag is present, the property adds its positive tag

This creates clear mechanical flows. For example, a location property might have Assertive as its Primary Trigger, Methodical as its Counter Trigger, and Adaptive as its Boost Trigger. Each time the player makes a choice, we check these triggers in sequence: First check if the approach triggers any negative tags, then check for negative tag removal, finally check for positive tag addition if eligible.
The sequence becomes completely deterministic: First apply the choice's momentum and pressure changes, then each property checks its triggers against the chosen approach type, then tags are modified according to the triggered rules, finally new choices are generated based on the resulting state.

Every approach type belongs to exactly one trigger category:

* Assertive approaches deliver high momentum gains with high pressure costs.
* Methodical approaches provide low momentum gains with low pressure costs.
* Adaptive approaches give medium momentum gains with medium pressure costs.

Each location property controls exactly two tags (one positive, one negative) and defines three trigger rules:

* Primary Trigger - Which category, when used, adds this property's negative tag
* Counter Trigger - Which category removes this property's negative tag if present
* Boost Trigger - Which category adds this property's positive tag if no negative tag exists

When a player selects a choice, we process these rules in strict sequence:

* First, we apply the choice's fixed momentum and pressure changes.
* Then each property checks if the choice's approach category matches its Primary Trigger. If yes, that property adds its negative tag (unless already present).
* Next, properties check their Counter Triggers. If the approach category matches and that property's negative tag exists, remove it.
* Finally, properties check their Boost Triggers. If the approach category matches and that property has no negative tag present, add its positive tag.

This creates systematic strategic depth because properties can have different trigger configurations. For example, one property might add negative tags from Assertive approaches but allow them to be countered by Methodical approaches. Another property might do the opposite, creating locations that respond differently to the same approach types through purely mechanical relationships.

# Tag Effects

Instead of complex narrative-based effects, each tag (positive or negative) controls exactly one mechanical aspect of the encounter. There are three possible effect types a tag can have:

* First effect type: The tag modifies momentum gains. When present, it adds or subtracts a fixed amount to all momentum gains from choices. For example, a negative tag might subtract 1 from all momentum gains, while its corresponding positive tag adds 1 to all momentum gains.
* Second effect type: The tag modifies pressure increases. When present, it adds or subtracts a fixed amount to pressure increases from choices. A negative tag might add 1 to all pressure increases, while its positive counterpart subtracts 1 from pressure increases.
* Third effect type: The tag modifies choice availability. When present, it enables or disables specific approach categories from appearing in choice generation. A negative tag might prevent Assertive approaches from being generated, while its positive counterpart might allow an additional Assertive choice to be generated.

Each location property must define which of these three effect types its tags use. This creates clear mechanical relationships - when a property adds or removes its tags, we know exactly how it will affect the encounter's mechanical flow.
The effects stack in a deterministic way: First calculate base momentum and pressure from the chosen approach, then apply all momentum-modifying tags, then all pressure-modifying tags, finally use tag effects to modify what choices can appear next stage.

stateDiagram-v2
    state "Current Stage" as Current {
        [*] --> MomentumPressure
        MomentumPressure --> ActiveTags
        ActiveTags --> AvailableChoices
    }
    
    state "Choice Selection" as Selection {
        state "Player Picks Choice" as Pick
        state "Apply Base Changes" as Base {
            Changes: Add Base Momentum
            Changes: Add Base Pressure
        }
    }
    
    state "Property Reactions" as Properties {
        state "Check Primary Triggers" as Primary
        state "Check Counter Triggers" as Counter
        state "Check Boost Triggers" as Boost
    }
    
    state "Tag Effects" as Effects {
        state "Apply Momentum Mods" as Momentum
        state "Apply Pressure Mods" as Pressure
        state "Determine Next Choices" as Choices
    }
    
    Current --> Selection
    Selection --> Properties
    Properties --> Effects
    Effects --> Current: Next Stage

    note right of Current
        Encounter State:
        - Momentum (-5 to +10)
        - Pressure (0 to 10)
        - Active Tags
    end note

    note right of Selection
        Each Approach Type has:
        - Fixed momentum gain
        - Fixed pressure increase
        - Belongs to trigger category
    end note
    
    note right of Properties
        Each Property defines:
        - Primary: Add negative tag
        - Counter: Remove negative tag
        - Boost: Add positive tag
    end note

    note right of Effects
        Each Tag has one effect:
        - Modify momentum gains
        - Modify pressure gains
        - Control choice availability
    end note