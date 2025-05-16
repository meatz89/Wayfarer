# The Strategic Commission System: Creating Genuine Decisions

You've identified the critical issue—a system where players simply match skills to requirements isn't strategically interesting. After thoroughly analyzing the problem, I've rebuilt the system to create genuine strategic depth through interlocking constraints and meaningful choices.

## Core Strategic Tensions

### 1. Time Block Economy

Each day consists of exactly 3 time blocks (Morning, Afternoon, Evening), creating the game's fundamental scarcity. Every action consumes a time block:

- Traveling between locations consumes a time block
- Completing a commission step consumes a time block
- Resting to refresh cards consumes a time block

This creates an immediate strategic tension: you can only do 3 things per day, and just traveling twice already consumes most of your day.

### 2. Approach Selection System

Each commission step offers multiple valid approaches with different strategic implications:

```
Commission Step: Acquire Guild Records 
Location: Merchant Hall
Time Remaining: 1 day

Approaches:
1. Official Request (Social)
   Requirements: Charm Check (DC 3)
   Outcome: Clean copy of records, +2 Guild reputation
   Risk: Takes full time block

2. Bribe Clerk (Social)
   Requirements: Deception Check (DC 2), 5 Silver
   Outcome: Original records with notes, may reveal bonus information
   Risk: -3 Guild reputation if failed

3. After-Hours Examination (Physical)
   Requirements: Agility Check (DC 3)
   Outcome: Unrestricted access to records, +1 Analysis bonus for next step
   Risk: Generates Suspicion if failed (blocks certain locations)

4. Assistant Review (Intellectual)
   Requirements: Analysis Check (DC 2)
   Outcome: Incomplete records, takes only half a time block
   Risk: May miss critical information (increases difficulty of next step)
```

This creates genuine strategic decisions—not just "do I have the right card?" but "which approach best fits my current situation, resources, and future plans?"

### 3. Competing Commission Sources

Commissions come from five distinct sources that compete for your attention:

- **Town Authority**: Highest reputation gains, moderate silver
- **Merchant Guild**: Best silver rewards, requires discretion
- **Church**: Unique knowledge rewards, tests moral choices
- **Nobles**: Highest silver, but reputation-dependent requirements
- **Commoners**: Accessible early, reputation gains with townsfolk

Taking commissions from one source reduces availability from rivals and may affect your reputation with them. This creates strategic specialization decisions.

### 4. Card Specialization & Synergy

Cards of the same type have specialized contexts where they excel:

```
"Forceful Strength" (Physical)
Specialization: +2 bonus to intimidation approaches
Synergy: +1 bonus when used after any Social card

"Technical Strength" (Physical)
Specialization: +2 bonus to crafting approaches
Synergy: +1 bonus when used after any Precision card
```

This transforms card selection from "I need a Physical card" to "I need the RIGHT Physical card for my planned approach sequence."

### 5. Dynamic Commission Market

The commission market refreshes daily with a limited selection:

- Each source offers 1-2 commissions daily
- Total available commissions: 5-8 per day
- Players can hold maximum 3 active commissions
- Commission difficulty and rewards scale with reputation
- Taking one commission might remove access to another

This creates opportunity cost decisions—which limited-time opportunities should you prioritize?

### 6. Complication System

Commission steps can generate complications requiring immediate resolution:

```
Complication: "Suspicious Guard"
Triggered by: Failed Agility check during document retrieval
Resolution Options:
1. Pay Bribe (5 Silver)
2. Persuade Guard (Charm DC 4)
3. Flee Scene (Agility DC 3, generates Suspicion)
4. Accept Capture (Lose half day in jail, -5 Reputation)
```

Complications create unexpected decision points that test adaptability and resource management.

### 7. Exhaustion Economy

Card refreshing follows a progressive cost curve:

- First refresh of any card type per day: 2 Silver
- Second refresh: 5 Silver
- Third refresh: 9 Silver
- Fourth+ refresh: 15 Silver each

This creates a genuine economy around card management, where silver becomes a strategic resource, not just a reward counter.

## A Day of Strategic Decisions

Here's how these systems create constant, meaningful decisions:

**Morning:**
- Commission market has refreshed with 6 new options
- Do you take the high-silver Merchant commission or the reputation-boosting Town one?
- Which cards do you prepare based on your commission plans?
- Which location do you visit first, knowing travel consumes your limited time blocks?

**At First Location:**
- Which approach do you select for the commission step?
- Do you choose the safer approach with your strongest card, or risk a higher-reward approach?
- If a complication arises, how do you resolve it with your remaining cards?
- Is it worth spending silver to refresh an exhausted card now?

**Afternoon:**
- Given your morning results, do you continue with the same commission or pivot?
- Is it worth traveling to another location or staying put to save a time block?
- Should you refresh exhausted cards now or save silver for potential complications?

**Evening:**
- With only one time block remaining, what's your priority?
- Complete a commission step, travel to position for tomorrow, or rest to refresh cards?
- Are there any expiring commissions that need immediate attention?

## Implementation Example

```
Day 3 Morning:

Available Commissions:
1. Investigate Warehouse Records (Town Guard)
   - Reward: 8 Silver, +12 Reputation with Town
   - Expires: 2 days
   - Steps: Document Retrieval, Pattern Analysis, Report Findings

2. Deliver Sensitive Message (Noble)
   - Reward: 15 Silver, +5 Reputation with Nobility
   - Expires: 1 day
   - Steps: Receive Message, Secure Delivery, Await Response

Player Status:
- Location: Inn
- Time Blocks: 3 remaining today
- Available Cards: Forceful Strength, Detailed Analysis, Diplomatic Charm
- Resources: 11 Silver, Reputation Tier 2
- Active Commission: Document Retrieval (1/3 complete)

Strategic Considerations:
- Traveling to Town Guard office takes 1 time block
- Diplomatic Charm has +2 bonus for official interactions
- Warehouse commission expires soon but requires multiple steps
- Message delivery has higher silver but requires traveling twice
```

The player faces genuine strategic decisions about how to use their limited time blocks, which commission to prioritize, which cards to use where, when to refresh exhausted cards, and how to approach each commission step.

## Why This Creates Strategy

This system forces players to constantly evaluate:

1. **Time Allocation**: Only 3 blocks per day forces prioritization
2. **Route Planning**: Travel consumes time blocks, making location sequencing critical
3. **Approach Selection**: Multiple valid ways to complete each step with different risk/reward profiles
4. **Card Management**: Specialized cards and increasing refresh costs create genuine scarcity
5. **Commission Selection**: Competing sources with different reward profiles force specialization
6. **Risk Assessment**: Complications require preparation and adaptability
7. **Economy Balancing**: Silver becomes a strategic resource for card refreshing, not just an accumulation goal

The challenge comes from juggling these constraints simultaneously, where optimizing for one inevitably creates pressure on the others. This creates the "tight" gameplay you're seeking, where every decision has meaningful consequences and no obvious optimal path exists.

Each profession would approach these challenges differently, creating diverse viable strategies while using the same core mechanics.


# Strategic Depth Within Wayfarer's Framework: Finding the Fun

After thoroughly reexamining your documentation, I understand I need to identify the strategic fun within your existing encounter and commission framework without adding complexity. Let me distill where the genuine decisions emerge in your system.

## The Strategic Triangle: Three Key Tensions

After multiple analyses, I've identified three fundamental constraints that create strategic depth when properly balanced:

### 1. Time Block Scarcity (3 per day)

Time blocks are your most precious resource. Each day provides exactly 3 blocks (Morning, Afternoon, Evening), and every significant activity consumes one:
- Traveling to a new location
- Initiating an encounter at a location
- Resting to recover Energy/Concentration

This simple constraint creates immediate strategic tension - you can only do 3 things per day, period.

### 2. Card Specialization & Exhaustion

Cards aren't just generic "Physical/Intellectual/Social" tokens - they represent specific skills:
- **Physical**: Strength, Endurance, Precision, Agility
- **Intellectual**: Analysis, Observation, Knowledge, Planning
- **Social**: Charm, Persuasion, Deception, Intimidation

When you use a card, it's exhausted until properly refreshed. This creates strategic card selection because:
- Commission steps require specific location + card type combinations
- Encounter skill checks benefit from specific skill types
- Limited deck space forces prioritization of certain capabilities each day

### 3. Encounter Decision Points

Encounters aren't just pass/fail skill checks - they're multi-stage decision trees where each choice:
- Can succeed/fail based on skill checks
- Affects subsequent encounter stages
- Determines the quality of commission progress
- Potentially creates unexpected complications

The strategy emerges from how these three constraints interact without needing additional systems.

## How These Create Strategic Depth

Here's how these three constraints create genuine strategic decisions:

### 1. Card Selection Strategy

Your daily deck isn't just "what cards do I have" but "which specific skills do I need today":

```
Commission: Investigate Merchant Discrepancy
Required steps:
1. Examine Records (at Guild Hall, Intellectual card required)
2. Question Witnesses (at Market, Social card required)
3. Search Warehouse (at Docks, Physical card required)

Available cards (maximum 5):
- Strength (Physical) - +2 to forceful approaches
- Precision (Physical) - +1 to detailed examination
- Agility (Physical) - +2 to stealth and quick movement
- Analysis (Intellectual) - +2 to pattern recognition
- Knowledge (Intellectual) - +1 to historical or theoretical understanding
- Observation (Intellectual) - +2 to environmental details
- Charm (Social) - +2 to friendly persuasion
- Intimidation (Social) - +1 to forceful questioning
```

This creates genuine decisions - not just "I need a Physical card" but "I need the right Physical card for the encounter choices I plan to make."

### 2. Time Block Optimization

With only 3 time blocks daily, you constantly face decisions about priority:

```
Day planning considerations:
- Commission A step must be completed today (deadline)
- Commission B offers higher reward but requires traveling twice
- Current Energy is low, suggesting using a block for rest
- Specific card needed for Commission C is exhausted
```

These constraints force prioritization between competing goals without needing additional systems.

### 3. Encounter Path Selection

Within encounters, your choices create branching outcomes:

```
Encounter: Examine Merchant Records
Stage 1 - Gaining Access:
- Official request (requires Persuasion)
- Sneak in after hours (requires Agility)
- Bribe clerk (requires Deception, costs Silver)
- Ask for public records only (no requirement, limits information)

[Based on choice, proceeds to different Stage 2 options...]
```

Each path has different requirements, risks, and potential rewards, creating meaningful decision points even within a single encounter.

## The Daily Strategic Loop

This creates a tight strategic loop where players must constantly evaluate:

1. **Morning Planning**:
   - Which commissions should I prioritize today?
   - Which cards best support my plans?
   - What's my optimal route between locations?

2. **Encounter Execution**:
   - Which approach gives me the best chance with my cards?
   - Is it worth attempting higher-difficulty options?
   - How do I adapt if early encounter stages go poorly?

3. **Resource Management**:
   - Should I use a time block to refresh exhausted cards?
   - Is it better to complete one commission fully or make progress on multiple?
   - How do I balance immediate rewards vs. positioning for tomorrow?

## Implementation Example

```
Day 3 - Starting Situation:

Time Blocks: 3 (Morning, Afternoon, Evening)
Location: Inn

Active Commissions:
1. Investigate Theft (2/3 complete)
   - Final step: Confront Suspect (at Tavern, requires Social card)
   - Deadline: Today
   - Reward: 12 Silver, +5 Reputation

2. Document Irregularities (1/3 complete)
   - Next step: Compare Ledgers (at Guild Hall, requires Intellectual card)
   - Deadline: Tomorrow
   - Reward: 8 Silver, +10 Reputation

Available Cards:
- Strength (Physical, +2) - Exhausted
- Observation (Intellectual, +1)
- Analysis (Intellectual, +2)
- Charm (Social, +1)
- Persuasion (Social, +2)

Energy: 2/10 (Low)
Concentration: 8/10 (High)
```

This creates meaningful decisions without additional systems:
- Do I prioritize completing the Theft commission before it expires?
- Should I use a time block to rest and refresh my Strength card?
- Is it worth traveling to Guild Hall to make progress on the second commission?
- Which Social card should I use for the confrontation? (Charm for friendly approach or Persuasion for direct questioning)

## Where the Strategic Fun Lies

The strategic depth emerges from:

1. **Opportunity Cost**: Every choice means not doing something else
2. **Resource Juggling**: Managing cards, time blocks, and commission deadlines
3. **Risk Assessment**: Judging when to attempt difficult skill checks
4. **Path Optimization**: Finding efficient routes through commission steps
5. **Encounter Navigation**: Selecting appropriate approaches based on available cards

This creates meaningful gameplay without additional complexity because the constraints create natural tension and force trade-offs between competing priorities.

## Profession Differentiation

Each profession approaches these constraints differently:

- **Warrior**: More efficient with Physical cards, can succeed with fewer of them
- **Scholar**: Intellectual encounters require less Concentration
- **Courtier**: Social cards refresh more easily, reducing resource pressure

This creates diverse playstyles using the same core mechanics.

The strategic fun comes not from complex systems, but from how simple constraints interact to create meaningful decisions at every level - from daily planning to encounter execution.