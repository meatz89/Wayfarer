# Tag Resonance System: Complete Implementation Framework

After extensive analysis of Wayfarer's design documents and systemic design principles, I've refined the Tag Resonance System into a comprehensive framework that creates strategic depth through elegantly simple rules.

## I. Core Mechanical Structure

### Tag Categories and Distribution

Every card, location, NPC, and commission in Wayfarer carries three types of tags:

1. **Context Tags** (Where/When): Interior, Outdoor, Morning, Evening, Crowded, Quiet
   - Cards have 1 primary context tag
   - Locations have 2-3 context tags that shift with time of day

2. **Approach Tags** (How): Thorough, Swift, Bold, Cautious, Methodical, Direct
   - Cards have 1 primary approach tag
   - NPCs have 1 preferred and 1 disliked approach

3. **Domain Tags** (What): Crafting, Commerce, Travel, Investigation, Labor, Social
   - Cards have 1 primary domain tag
   - Locations have 1-2 domain specializations
   - Commissions require specific domains

### Resonance Mechanics

When tags match across systems, they create specific mechanical benefits:

1. **Context Resonance** (Card matches Location)
   - Each matching context tag reduces Energy/Concentration cost by 1
   - Example: Using an "Interior" card at the forge (Interior) costs 1 less Energy

2. **Approach Resonance** (Card matches NPC preference)
   - Each matching approach tag adds +1 to effectiveness
   - Using an approach an NPC dislikes creates -1 effectiveness
   - Example: Using a "Thorough" card with a smith who values thorough work gives +1 effectiveness

3. **Domain Resonance** (Card matches Commission)
   - Matching domain tags unlock special options
   - Example: Using "Crafting" cards on crafting commissions reveals specialized approaches

## II. System Integration

### Card System Enhancement

The existing card system transforms from generic categories to specialized tools:

**Card Structure Example:**
```
"Persistent Labor" (Physical, Strength Level 1)
Context: Interior
Approach: Thorough
Domain: Crafting
```

**Card Progression Path:**
- Level 1: Single tag in each category
- Level 2: Primary tags plus one secondary tag
- Level 3: Multiple primary tags or reduced conflict penalties

This creates meaningful differentiation between cards of the same skill, making each card uniquely valuable in specific situations.

### Daily Planning Strategic Depth

Morning planning becomes a strategic puzzle rather than rote optimization:

1. **Contextual Deck Building**
   - "I need to visit the blacksmith (Interior/Crafting) and market (Crowded/Commerce) today"
   - "Which combination of cards will work effectively in both contexts?"

2. **Time-Sensitive Planning**
   - Morning locations have different context tags than evening locations
   - "Should I visit the market now (Crowded) or later (Quiet)?"

3. **Resource Optimization**
   - Context matches reduce Energy/Concentration costs
   - "I'll save my Energy by using Interior cards in Interior locations"

This creates genuine strategic choices during daily planning, not just selecting "the best cards."

### Commission System Transformation

The commission system gains multiple valid strategies for completion:

1. **Approach Options**
   - Each commission offers multiple approaches with different requirements and outcomes
   - "Swift" approach: Faster completion, average quality
   - "Thorough" approach: Slower but higher quality
   - "Bold" approach: High risk/reward ratio

2. **Revealed Options**
   - Using domain-matching cards reveals hidden approaches
   - "Using a Crafting card revealed a specialized 'Ornamental' approach option"

3. **Outcome Diversity**
   - Different approaches produce different qualities of outcomes
   - Quality tiers: Poor (-1), Standard (0), Good (+1), Excellent (+2)
   - Higher quality yields better rewards

This creates multiple valid paths to success based on your character's specialization.

### Location System Dynamism

Locations become more dynamic without complex simulation:

1. **Time-Based Context Shifts**
   - Market: Crowded (day) → Quiet (night)
   - Inn: Quiet (morning) → Crowded (evening)

2. **Domain Specializations**
   - Forge specializes in Crafting/Labor domains
   - Market specializes in Commerce/Social domains
   - These specializations affect which cards work best there

3. **Environmental Modifiers**
   - Weather and events can temporarily add tags
   - Rain adds "Wet" to outdoor locations
   - Festivals add "Celebratory" to public spaces

This creates a living world that follows predictable patterns, just like a real medieval town.

### NPC Personality Mechanics

NPCs gain mechanical personality through approach preferences:

1. **Approach Preferences**
   - Blacksmith prefers Thorough approaches, dislikes Swift
   - Merchant prefers Swift approaches, dislikes Methodical
   - These preferences directly affect effectiveness

2. **Relationship Development**
   - Using preferred approaches builds rapport faster
   - Avoiding disliked approaches maintains trust
   - Relationships unlock additional tag benefits

3. **Domain Expertise**
   - NPCs provide bonuses to matching domain cards
   - "The blacksmith gives +1 to Crafting cards used in his presence"

This turns social navigation into strategic gameplay without complex dialogue trees.

## III. Emergent Strategic Depth

The Tag Resonance System creates rich strategic depth through these simple rules:

### Specialization Pathways

Players naturally develop distinct specializations based on play patterns:

1. **The Craftsperson**
   - Specializes in Interior/Thorough/Crafting tags
   - Excels at workshop crafting commissions
   - Develops relationships with craftspeople

2. **The Merchant**
   - Specializes in Crowded/Swift/Commerce tags
   - Excels at market trading commissions
   - Builds network of merchant contacts

3. **The Investigator**
   - Specializes in Quiet/Methodical/Investigation tags
   - Excels at mystery-solving commissions
   - Cultivates relationships with knowledgeable figures

These specializations emerge organically through play rather than forced character classes.

### Strategic Tensions

The system creates natural strategic tensions:

1. **Tag Conflicts**
   - Some tags naturally oppose others (Swift vs. Thorough)
   - Using conflicting tags creates penalties
   - Forces choices between different approaches

2. **Resource Tradeoffs**
   - Specialized decks excel in specific contexts but struggle elsewhere
   - Diverse decks provide flexibility but fewer resonance opportunities
   - Creates meaningful daily planning decisions

3. **Time Management**
   - Context tags shift with time of day
   - Some opportunities only available at specific times
   - Forces prioritization of activities

These tensions create the "meaningful choices from simple rules" that define great systemic design.

### Adaptive Gameplay

The system rewards adaptation to changing circumstances:

1. **Environmental Response**
   - Weather changes context tags, requiring adaptation
   - Events modify location properties temporarily
   - Creates dynamic gameplay without complex simulation

2. **Opportunity Recognition**
   - Identifying optimal tag combinations becomes valuable
   - "The smith prefers Thorough work, so I'll wait until I have those cards"
   - Creates strategic timing decisions

3. **Resource Optimization**
   - Context matching creates resource efficiency
   - Approach matching maximizes outcome quality
   - Domain matching unlocks special options
   - Multiple valid strategies for success

This creates a game where adaptation and strategic thinking are constantly rewarded.

## IV. Implementation Requirements

To implement this system, we need:

### Data Structure Updates

1. **Card Definition Extension**
   - Add context, approach, and domain tag fields to all cards
   - Ensure consistent tag distribution across card types

2. **Location Property Formalization**
   - Convert existing location properties to formal context tags
   - Add time-based tag transition rules
   - Define domain specializations for each location

3. **NPC Preference System**
   - Add preferred/disliked approach fields to NPCs
   - Create relationship progression tied to approach matching
   - Define domain expertise for each NPC

### User Interface Elements

1. **Tag Visualization**
   - Clear icons for each tag category
   - Visual indicators for matching tags
   - Benefit displays showing resonance effects

2. **Planning Assistance**
   - Location schedule showing context changes
   - NPC preference indicators
   - Commission tag requirements clearly displayed

3. **Feedback Systems**
   - Visual/audio feedback for tag matches
   - Warning indicators for tag conflicts
   - Quality tier indicators for outcomes

### Balance Considerations

1. **Tag Distribution**
   - Ensure even distribution of tags across card types
   - Create natural affinities without forced specialization
   - Maintain multiple viable strategies

2. **Benefit Scaling**
   - Context match: -1 Energy/Concentration per match
   - Approach match: +1 effectiveness per match
   - Domain match: Unique options based on specialization
   - Maximum benefits capped to prevent exploitation

3. **Progression Curve**
   - Beginning cards have fewer tags (easier to understand)
   - Advanced cards have more tag interactions
   - Advanced cards reduce conflict penalties
   - Creates natural skill progression

## V. Example Gameplay Scenario

To illustrate the system in action:

### Morning Planning

The player checks their commissions and finds:
- "Repair Guild Sign" (Interior/Crafting commission)
- "Negotiate Trade Deal" (Crowded/Commerce commission)

They select their daily deck considering:
- Need for Interior/Crafting cards for the repair
- Need for Crowded/Commerce cards for negotiation
- Cards with approaches matching relevant NPCs
- Energy and Concentration limitations

### Commission Execution

At the workshop (Interior/Crafting location):
- Player uses "Persistent Labor" (Interior/Thorough/Crafting)
- Benefits from context match: -1 Energy cost
- Meets master craftsman who prefers Thorough approach
- Benefits from approach match: +1 effectiveness
- Domain match reveals specialized "Ornamental" approach option
- Completes commission with "Good" quality outcome

At the market (Crowded/Commerce location):
- Context has changed to Crowded as day progresses
- Player uses "Market Haggling" (Crowded/Swift/Commerce)
- Benefits from all three tag matches
- Achieves "Excellent" quality outcome with minimal resource cost

This creates a satisfying progression where strategic tag matching directly translates to gameplay success.

## VI. Why This System Succeeds

The Tag Resonance System succeeds because it:

1. **Creates Depth Without Complexity**
   - The rule is simple: matching tags = benefits
   - The strategic depth comes from applying this rule across all systems

2. **Integrates All Existing Systems**
   - Enhances cards, commissions, locations, and NPCs
   - Creates connections between previously separate mechanics

3. **Enables Player Creativity**
   - Multiple valid strategies for any situation
   - Rewards experimental combinations
   - Creates "player stories" through specialization

4. **Maintains Medieval Authenticity**
   - The importance of context, approach, and domain expertise mirrors real medieval life
   - Creates believable world patterns without complex simulation

5. **Supports Wayfarer's Core Design**
   - Enhances the card system's strategic value
   - Deepens the commission system's replay value
   - Creates meaningful progression without stat inflation

This system embodies Warren Spector's principle that "it's not about how clever the designer is—it's about how clever players can be in interacting with the game world." The Tag Resonance System provides the framework for that player ingenuity to shine.