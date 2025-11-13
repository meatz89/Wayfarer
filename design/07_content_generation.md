# Section 7: Content Generation and Archetype Systems

## Overview

Wayfarer generates infinite content through archetype-based procedural systems. Archetypes define reusable mechanical patterns independent of narrative content, scaled contextually via categorical entity properties. This enables AI to generate balanced content without global game state knowledge, supporting the infinite A-story and extensive B/C side content.

This section describes the archetype philosophy, three-tier hierarchy, categorical property scaling, AI narrative generation enablement, and the complete catalog of 21 situation archetypes providing the mechanical foundation for all content generation.

## The Content Scaling Problem

Traditional narrative RPGs face exponential authoring burden. Every service location, every checkpoint, every transaction requires explicit JSON defining situations, choices, costs, and rewards. Elena's tavern lodging needs JSON. Marcus's bathhouse needs separate JSON. Thalia's healing temple needs more JSON. Each mechanically similar but requiring separate authoring effort.

The naive solution - copy-paste and modify - creates maintenance nightmares. Balance changes require updating hundreds of files. Mechanical improvements require finding every affected template. Bugs multiply across duplicated content.

The traditional procedural solution - generate everything from scratch - produces broken content. Generated scenes have wrong situation ordering, unbalanced economics, narrative incoherence, and broken state management. AI can write compelling dialogue for individual moments but cannot orchestrate complex multi-situation arcs with mechanical coherence.

**The core insight**: Separate reusable patterns from entity context. Patterns encode proven mechanical structures. Entity properties drive contextually appropriate difficulty scaling. Same pattern + different properties = infinite variations maintaining mechanical integrity.

## Archetype Philosophy

Archetypes define mechanical structure independent of narrative content. Same archetype generates contextually appropriate difficulty via categorical property scaling applied universally.

### Archetype as Mechanical Pattern

A situation archetype specifies:

**Structure Definition**:
- Number of choices (usually 4)
- Path types per choice (InstantSuccess/Challenge/Fallback distribution)
- Base cost formulas (stat threshold formula, coin cost formula, challenge difficulty formula)
- Base reward formulas (resource restoration formula, state change formula)
- Requirement formulas (who can attempt which paths)

**What Archetype Does NOT Specify**:
- Narrative text (generated from entity context by AI)
- Specific entity IDs (uses placement context, not hardcoded references)
- Absolute numeric values (uses base values × scaling multipliers)
- Domain-specific branches (no "if service type == lodging" logic)

Archetype is pure mechanical pattern. All variability comes from entity properties scaling the pattern, not from modifying the pattern itself.

### Universal Reusability Through Categorical Scaling

Same archetype applies across radically different fictional contexts without modification. The "negotiation" archetype works for:
- Securing lodging from innkeeper
- Gaining passage through checkpoint from guard
- Acquiring information from scholar
- Purchasing goods from merchant
- Requesting healing from priest

Each instance feels contextually appropriate because entity properties (NPC demeanor, location quality, power dynamic, environment quality) scale difficulty and costs. Same mechanical structure, different scaling factors, contextually appropriate experience.

**Test for proper reusability**: Can this archetype apply to 5+ different fictional contexts without modification? If yes, properly reusable. If requires modification or conditional branches, archetype is too specific - extract common pattern.

### Archetype Abstraction Level

Archetypes must balance between too specific (limited reuse) and too generic (no useful structure).

**Correct Abstraction**:
- Defines mechanical structure (choice count, path types, cost patterns)
- Defines scaling behavior (which context properties affect difficulty)
- Provides narrative hints (tone, theme, context - not specific text)
- Encodes proven game design patterns

**Too Specific** (anti-pattern):
- Hardcodes entity types (if personality == Innkeeper)
- Includes domain-specific branches (switch on service type)
- Embeds concrete narrative text
- Couples to specific content

**Too Generic** (anti-pattern):
- Accepts arbitrary parameters for everything
- Provides no structural constraints
- Just a data container with no embedded design
- Forces authors to specify everything manually

**The Goldilocks Zone**: Specific mechanical pattern (4 choices, stat/money/challenge/fallback structure) with universal categorical scaling (demeanor/quality/power dynamic multipliers apply to any service type).

### Mathematical Variety from Finite Archetypes

Archetype reusability creates vast variety from finite catalog:

**Base Variation**:
- 21 situation archetypes (different interaction patterns)
- 12 scene archetypes (different multi-situation structures)

**Property Scaling**:
- 3 NPC demeanor values (Friendly/Neutral/Hostile)
- 4 quality values (Basic/Standard/Premium/Luxury)
- 3 power dynamic values (Dominant/Equal/Submissive)
- 3 environment quality values (Basic/Standard/Premium)

**Mathematical Combinations**:
- 21 archetypes × 3 demeanors × 4 qualities × 3 power dynamics × 3 environment qualities
- = 21 × 3 × 4 × 3 × 3 = **2,268 base mechanical variations**

**Narrative Multiplication**:
- Infinite entity-specific narrative from AI generation
- Entity context (personality, history, location atmosphere)
- Player journey callbacks (prior choices, relationships)
- Effectively infinite content from finite archetype foundation

## Three-Tier Archetype Hierarchy

Situation archetypes organize into three tiers of increasing specificity.

### TIER 1: Core Archetypes (5 Fundamental Interaction Types)

Core archetypes define fundamental ways players interact with game world. Broadest patterns representing essential interaction philosophies.

**1. CONFRONTATION (Authority/Dominance)**

When to use: Gatekeepers, obstacles, authority challenges, power-based interactions.

Mechanical pattern:
- Choice 1 (Optimal): Authority stat threshold → Immediate success, best outcome
- Choice 2 (Reliable): Pay substantial coins → Success through resources
- Choice 3 (Risky): Physical challenge → Variable outcome, advances either way
- Choice 4 (Guaranteed): Submit or negotiate slowly → Minimal outcome but certain

Primary stat: Authority
Coin cost base: 15
Challenge type: Physical

Example contexts: Checkpoint guards blocking passage, authority figures demanding justification, physical obstacles requiring force or cleverness, dominance-based social hierarchies.

**2. NEGOTIATION (Diplomacy/Trade)**

When to use: Merchants, transactional exchanges, deals, balanced discussions, diplomatic resolutions.

Mechanical pattern:
- Choice 1 (Optimal): Diplomacy/Rapport stat threshold → Best terms achieved
- Choice 2 (Reliable): Pay premium price → Good terms guaranteed
- Choice 3 (Risky): Mental challenge debate → Variable terms, advances either way
- Choice 4 (Guaranteed): Accept standard terms → Basic outcome but certain

Primary stat: Diplomacy/Rapport
Coin cost base: 15
Challenge type: Mental

Example contexts: Service negotiations (lodging, healing, bathing), merchant transactions, information exchanges, collaborative problem-solving, finding middle ground.

**3. INVESTIGATION (Insight/Discovery)**

When to use: Information gathering, puzzle solving, deduction, mystery unraveling, research.

Mechanical pattern:
- Choice 1 (Optimal): Insight/Cunning stat threshold → Deduce efficiently
- Choice 2 (Reliable): Hire expert or pay for access → Information purchased
- Choice 3 (Risky): Mental challenge puzzle → Variable insight, advances either way
- Choice 4 (Guaranteed): Exhaustive methodical approach → Slow but certain discovery

Primary stat: Insight/Cunning
Coin cost base: 10
Challenge type: Mental

Example contexts: Crime scene analysis, historical research, puzzle solving, pattern recognition, evidence gathering, connecting disparate clues.

**4. SOCIAL MANEUVERING (Rapport/Manipulation)**

When to use: Social circles, subtle influence, persuasion, reading people, navigating complex social dynamics.

Mechanical pattern:
- Choice 1 (Optimal): Rapport/Cunning stat threshold → Read situation and respond perfectly
- Choice 2 (Reliable): Offer valuable gift → Goodwill purchased
- Choice 3 (Risky): Social challenge bold gambit → Variable reaction, advances either way
- Choice 4 (Guaranteed): Patient relationship building → Slow acceptance certain

Primary stat: Rapport/Cunning
Coin cost base: 10
Challenge type: Social

Example contexts: Navigating social hierarchies, influencing group decisions, building coalitions, subtle manipulation, reading hidden agendas, establishing trust in complex situations.

**5. CRISIS (Emergency Response)**

When to use: Urgent situations, decisive action, time pressure, emergency decisions, critical moments.

Mechanical pattern:
- Choice 1 (Optimal): Authority/Insight stat threshold → Expert immediate action, best outcome
- Choice 2 (Reliable): Expensive immediate solution → Problem solved through resources
- Choice 3 (Risky): Physical challenge personal risk → Variable outcome, advances either way
- Choice 4 (Guaranteed): Safe conservative approach or flee → Minimal outcome but certain

Primary stat: Authority/Insight
Coin cost base: 25 (higher due to urgency)
Challenge type: Physical

Example contexts: Medical emergencies, combat crises, environmental disasters, social catastrophes, political emergencies, any urgent situation requiring immediate decisive action.

### TIER 2: Expanded Archetypes (10 Domain-Specific Variations)

Expanded archetypes specialize core patterns for specific interaction domains while maintaining reusability across contexts within domain.

**6. SERVICE TRANSACTION**

When: Paying for services (lodging, food, passage, treatment, bathing).

Specialization: Simplified negotiation focused on payment and service quality. Emphasizes economic exchange over social maneuvering.

Pattern:
- Choice 1: Negotiate discount (Rapport threshold)
- Choice 2: Pay standard rate
- Choice 3: Mental challenge haggle
- Choice 4: Accept basic service or barter labor

Primary stat: None (emphasizes economic over social)
Coin cost base: 5 (lower, represents base service cost)
Challenge type: Mental

Reusability: Any service economy interaction across all service types.

**7. ACCESS CONTROL**

When: Gatekeepers, locked doors, restricted areas, permission-based barriers.

Specialization: Confrontation focused on legitimacy and authorization. Emphasizes credentials and authority.

Pattern:
- Choice 1: Authority/credentials prove legitimacy
- Choice 2: Bribe substantially
- Choice 3: Physical challenge force or sneak
- Choice 4: Wait for permission or find alternate route

Primary stat: Authority/Cunning
Coin cost base: 15
Challenge type: Physical

Reusability: Any barrier where someone controls access (checkpoints, locked areas, restricted knowledge, exclusive social circles).

**8. INFORMATION GATHERING**

When: Rumors, gossip, local knowledge, casual information exchange.

Specialization: Investigation focused on social sources. Emphasizes rapport and paying attention.

Pattern:
- Choice 1: Rapport threshold connect naturally
- Choice 2: Buy drinks/offer payment for information
- Choice 3: Social challenge carefully probe
- Choice 4: Listen patiently, information emerges eventually

Primary stat: Rapport/Insight
Coin cost base: 8
Challenge type: Social

Reusability: Any informal information acquisition from people (tavern gossip, market rumors, casual conversations, overhead discussions).

**9. SKILL DEMONSTRATION**

When: Proving competence, showing credentials, demonstrating capability, earning trust through expertise.

Specialization: Reputation establishment through capability display.

Pattern:
- Choice 1: Diplomacy/Insight threshold demonstrate naturally
- Choice 2: Pay for formal certification or references
- Choice 3: Mental challenge solve test problem
- Choice 4: Offer to demonstrate through low-stakes trial

Primary stat: Diplomacy/Insight
Coin cost base: 12
Challenge type: Mental

Reusability: Any situation requiring proof of capability (guild admission, earning trust, securing employment, accessing expert circles).

**10. REPUTATION CHALLENGE**

When: Defending honor, responding to accusations, addressing reputation damage, navigating social censure.

Specialization: Social defense and reputation management.

Pattern:
- Choice 1: Authority/Diplomacy threshold defend confidently
- Choice 2: Compensate accusers to resolve quietly
- Choice 3: Social challenge public defense
- Choice 4: Accept social penalty, rebuild slowly

Primary stat: Authority/Diplomacy
Coin cost base: 10
Challenge type: Social

Reusability: Any situation involving reputation defense (false accusations, misunderstandings, social slights, honor challenges).

**11. EMERGENCY AID**

When: Medical crisis, rescue situations, urgent help needed.

Specialization: Crisis focused on helping others under time pressure.

Pattern:
- Choice 1: Insight/Authority threshold expert intervention
- Choice 2: Pay for professional help immediately
- Choice 3: Physical challenge personal rescue attempt
- Choice 4: Provide basic assistance, ensure survival

Primary stat: Insight/Authority
Coin cost base: 20
Challenge type: Physical

Reusability: Any urgent assistance situation (medical emergencies, rescue from danger, preventing immediate harm, crisis intervention).

**12. ADMINISTRATIVE PROCEDURE**

When: Bureaucracy, permits, official processes, formal requirements.

Specialization: Negotiation within formal structures.

Pattern:
- Choice 1: Diplomacy/Insight threshold navigate efficiently
- Choice 2: Pay expediting fees
- Choice 3: Mental challenge find procedural shortcuts
- Choice 4: Complete standard process slowly

Primary stat: Diplomacy/Insight
Coin cost base: 12
Challenge type: Mental

Reusability: Any bureaucratic interaction (permits, official approvals, guild registrations, legal procedures).

**13. TRADE DISPUTE**

When: Disagreements over goods, quality disputes, contract conflicts, economic conflicts.

Specialization: Negotiation focused on resolution of economic disagreement.

Pattern:
- Choice 1: Insight/Diplomacy threshold fair resolution
- Choice 2: Pay difference to resolve
- Choice 3: Mental challenge prove your position
- Choice 4: Accept compromise

Primary stat: Insight/Diplomacy
Coin cost base: 15
Challenge type: Mental

Reusability: Any economic disagreement (quality disputes, pricing conflicts, contract interpretation, trade fairness).

**14. CULTURAL FAUX PAS**

When: Social blunders, tradition violations, unintentional offenses, cultural misunderstandings.

Specialization: Social recovery from mistakes.

Pattern:
- Choice 1: Rapport/Insight threshold recover gracefully
- Choice 2: Offer compensation for offense
- Choice 3: Social challenge sincere apology
- Choice 4: Accept social penalty, learn from mistake

Primary stat: Rapport/Insight
Coin cost base: 10
Challenge type: Social

Reusability: Any cultural misunderstanding or social error (etiquette violations, tradition ignorance, accidental insults).

**15. RECRUITMENT**

When: Join requests, commitment decisions, faction membership offers, ongoing obligation decisions.

Specialization: Social decision about long-term affiliation.

Pattern:
- Choice 1: Cunning/Diplomacy threshold negotiate favorable terms
- Choice 2: Pay entry fee for membership
- Choice 3: Social challenge prove worthiness
- Choice 4: Politely decline or defer decision

Primary stat: Cunning/Diplomacy
Coin cost base: 8
Challenge type: Social

Reusability: Any affiliation decision (guild membership, faction joining, mentor relationships, long-term commitments).

### TIER 3: Specialized Service Archetypes (6 Multi-Phase Compositions)

Specialized archetypes compose into complete multi-situation service flows. Used by scene archetypes to construct complex service sequences (inn_lodging, bathhouse_service, healer_treatment).

**16. SERVICE_NEGOTIATION**

Purpose: First situation in service sequences. Secures access to service.

Pattern: 4 choices (stat/money/challenge/fallback) → Grants access item (key, token, pass) → Transitions to service execution.

Mechanical structure:
- Choice 1: Stat-gated path (best rate and quality)
- Choice 2: Pay standard rate
- Choice 3: Challenge for discount
- Choice 4: Negotiate labor exchange or basic service

Reward: Dependent item granting access (room key, bathhouse token, treatment pass).

Transition: Always to service execution situation.

**17. SERVICE_EXECUTION_REST**

Purpose: Core situation in rest services. Provides resource restoration with variation options.

Pattern: 4 rest variants → Restores resources → Advances time to next day Morning.

Mechanical structure:
- Choice 1: Balanced rest (restore Stamina and Focus evenly)
- Choice 2: Physical focus (maximize Stamina restoration)
- Choice 3: Mental focus (maximize Focus restoration)
- Choice 4: Minimal rest (basic restoration, faster time passage)

Reward: Resource restoration scaled by service quality, time advancement.

Transition: Always to next situation (usually departure).

**18. SERVICE_DEPARTURE**

Purpose: Final situation in service sequences. Cleanup and exit.

Pattern: 2 choices → Removes access item → Optional preparation effects.

Mechanical structure:
- Choice 1: Leave carefully (check for forgotten items, take time)
- Choice 2: Leave immediately (quick departure)

Reward: Removes access item (consumed), optional small preparation buff.

Transition: Always to scene completion.

**19. REST_PREPARATION**

Purpose: Optional pre-rest optimization. Player prepares for better rest outcomes.

Pattern: 4 preparation choices → Enhances subsequent rest effectiveness.

Mechanical structure:
- Choice 1: Optimize conditions (Insight threshold, best preparation)
- Choice 2: Purchase comfort items (coins, good preparation)
- Choice 3: Force relaxation (mental challenge, variable preparation)
- Choice 4: Collapse immediately (no preparation, just rest)

Reward: Preparation modifier affecting next rest effectiveness.

Transition: Always to rest execution.

**20. ENTERING_PRIVATE_SPACE**

Purpose: First entry into private location. Player acclimates and optimizes.

Pattern: 4 entry choices → Sets conditions for service → Consumes access item.

Mechanical structure:
- Choice 1: Inspect and optimize (Insight threshold, best conditions)
- Choice 2: Request amenities (coins, good conditions)
- Choice 3: Push through discomfort (Physical challenge, variable conditions)
- Choice 4: Collapse immediately (basic conditions)

Reward: Condition modifiers affecting service effectiveness, access item consumed, location unlocked.

Transition: Always to service situation.

**21. DEPARTING_PRIVATE_SPACE**

Purpose: Leaving private location after service. Cleanup and optional gratuity.

Pattern: 3 departure choices → Removes access state → Optional relationship effects.

Mechanical structure:
- Choice 1: Check carefully before leaving (thorough, takes time)
- Choice 2: Leave gratuity and depart (costs coins, relationship boost)
- Choice 3: Rush out immediately (fastest, no benefits)

Reward: Access state removed, location re-locked, optional relationship modifier.

Transition: Always to scene completion or next situation.

## Two-Level Composition

Content generation operates through two distinct archetype layers that compose to create complete scenes.

### Level 1: Scene Archetypes (Multi-Situation Structures)

Scene archetypes generate complete narrative arcs containing multiple situations with transitions.

**Responsibility**:
- Defines HOW situations connect and flow
- Decides which situation archetypes to use
- Specifies transition patterns (Linear/Branching/Converging)
- Manages dependent resources (created locations, granted items)

**Granularity**: 1-4 situations per scene.

**Example Scene Archetypes**:
- `service_with_location_access`: 4 situations (negotiation → access → service → departure)
- `transaction_sequence`: 3 situations (approach → negotiate → conclude)
- `single_negotiation`: 1 situation (standalone interaction)

### Level 2: Situation Archetypes (4-Choice Patterns)

Situation archetypes generate individual player decision moments with 4-choice structure.

**Responsibility**:
- Defines HOW players interact with single situation
- Returns choice structure with costs, requirements, action types
- Applies entity property scaling to base values

**Granularity**: 1 situation = 4 choices (stat-gated, money, challenge, fallback).

**Composition Pattern**: Scene archetypes CALL situation archetypes. SceneArchetypeCatalog.Generate() invokes SituationArchetypeCatalog.GetArchetype() to produce choices for each situation position. The scene archetype defines WHICH situation archetypes appear and in WHAT configuration.

**Example Composition**:

Scene archetype: `service_with_location_access`

Internally decides:
- Situation 1: Uses `social_maneuvering` OR `service_transaction` archetype (based on NPC personality property)
- Situation 2-4: Uses inline single-choice instant actions (not archetype, too simple)
- Transitions: Linear Always connections (situation 1 → 2 → 3 → 4 → completion)

The archetype ID encodes the complete pattern. No parameters configuring structure. Scene identity IS its structure. This creates predictable, testable, maintainable patterns.

## Categorical Property Scaling

Entity properties scale archetypes universally without modifying archetype definitions. Same archetype + different entity properties = contextually appropriate difficulty via universal scaling formulas.

### Nine Universal Categorical Properties

Four core properties scale mechanical values (stat thresholds, costs, rewards), while five additional properties control specialized generation aspects (consequences, social impact, timing, emotional context, moral framing).

#### Core Scaling Properties (1-4)

**1. NPCDemeanor (Friendly/Neutral/Hostile)**

Scales: Stat thresholds for social paths.

Values:
- Friendly: 0.6× threshold (easier to persuade, naturally receptive)
- Neutral: 1.0× threshold (baseline, neither helpful nor hostile)
- Hostile: 1.4× threshold (harder to persuade, naturally resistant)

Example: Base Authority threshold 5.
- vs Friendly NPC: 5 × 0.6 = 3 (easier)
- vs Neutral NPC: 5 × 1.0 = 5 (baseline)
- vs Hostile NPC: 5 × 1.4 = 7 (harder)

Same archetype, different NPC demeanor, contextually appropriate difficulty.

**2. Quality (Basic/Standard/Premium/Luxury)**

Scales: Coin costs for service and money-gated paths.

Values:
- Basic: 0.7× cost (cheap services, minimal amenities)
- Standard: 1.0× cost (baseline, typical quality)
- Premium: 1.6× cost (expensive services, enhanced amenities)
- Luxury: 2.5× cost (very expensive services, exceptional amenities)

Example: Base service cost 10 coins.
- Basic quality: 10 × 0.7 = 7 coins
- Standard quality: 10 × 1.0 = 10 coins
- Premium quality: 10 × 1.6 = 16 coins
- Luxury quality: 10 × 2.5 = 25 coins

Same archetype, different service quality, contextually appropriate cost.

**3. PowerDynamic (Dominant/Equal/Submissive)**

Scales: Authority checks and confrontation thresholds.

Values:
- Dominant: 0.6× threshold (player has power advantage)
- Equal: 1.0× threshold (balanced relationship)
- Submissive: 1.4× threshold (NPC has power advantage)

Example: Base Authority threshold 6.
- Dominant dynamic: 6 × 0.6 = 3.6 → 4 (player powerful)
- Equal dynamic: 6 × 1.0 = 6 (balanced)
- Submissive dynamic: 6 × 1.4 = 8.4 → 8 (NPC powerful)

Same archetype, different power relationship, contextually appropriate authority requirements.

**4. EnvironmentQuality (Basic/Standard/Premium)**

Scales: Rewards and restoration effectiveness.

Values:
- Basic: 1.0× restoration (minimal comfort, standard recovery)
- Standard: 2.0× restoration (good comfort, enhanced recovery)
- Premium: 3.0× restoration (exceptional comfort, maximum recovery)

Example: Base Stamina restoration 30.
- Basic environment: 30 × 1.0 = 30 stamina
- Standard environment: 30 × 2.0 = 60 stamina
- Premium environment: 30 × 3.0 = 90 stamina

Same archetype, different environment quality, contextually appropriate benefits.

#### Additional Categorical Properties (5-9)

Beyond the four core scaling properties, GenerationContext includes five additional categorical properties that control specialized aspects of content generation:

**5. DangerLevel (Safe/Risky)**

Scales: Crisis consequences, Physical challenge damage, Confrontation escalation severity.

Values:
- Safe: Baseline consequences (standard risks, expected outcomes)
- Risky: Escalated consequences (higher health costs, harsher failure outcomes)

Derivation: Automatically derived from location properties (Guarded/Outdoor), NPC hostility (RelationshipFlow ≤ 5), and player health (< 30).

Example: Physical challenge failure.
- Safe danger: 10 health loss (manageable setback)
- Risky danger: 20 health loss (serious consequence)

Same archetype, different danger context, contextually appropriate risk.

**6. SocialStakes (Private/Witnessed/Public)**

Scales: Reputation impact, face-saving costs, romance intimacy options, social consequence severity.

Values:
- Private: Minimal social consequences (intimate settings, no witnesses)
- Witnessed: Standard social consequences (semi-public, limited audience)
- Public: Maximum social consequences (public venues, reputation at stake)

Derivation: Automatically derived from location properties. Public/Market → Public, Private/Isolated/Intimate → Private, otherwise Witnessed.

Example: Social challenge failure.
- Private stakes: No reputation loss (only affects direct relationship)
- Witnessed stakes: Minor reputation loss (small circle aware)
- Public stakes: Significant reputation loss (community-wide awareness)

Same archetype, different social context, contextually appropriate social impact.

**7. TimePressure (Leisurely/Urgent)**

Scales: Available choices, time costs, retry availability, decision complexity.

Values:
- Leisurely: Full choice availability (all 4 choices present, time to consider)
- Urgent: Reduced choices or increased costs (emergency decisions, pressure)

Derivation: Automatically derived from location properties (Guarded/Official → Urgent) and crisis context.

Example: Decision time cost.
- Leisurely pressure: Standard time segments (careful consideration possible)
- Urgent pressure: Compressed time segments or forced immediate choice

Same archetype, different time context, contextually appropriate urgency.

**8. EmotionalTone (Cold/Warm/Passionate)**

Scales: Social rewards, negotiation rapport bonuses, romance options, relationship progression speed.

Values:
- Cold: Professional transactions (minimal emotional engagement)
- Warm: Friendly interactions (positive emotional engagement, friendship)
- Passionate: Intense interactions (strong emotions, love or hate)

Derivation: Automatically derived from NPC BondStrength. High bond (≥15) or very low bond (≤3) → Passionate, medium bond (8-14) → Warm, otherwise Cold.

Example: Social success reward.
- Cold tone: Standard relationship progress (transactional)
- Warm tone: Enhanced relationship progress (friendship building)
- Passionate tone: Maximum relationship progress (deep connection or rivalry)

Same archetype, different emotional context, contextually appropriate intimacy.

**9. MoralClarity (Clear/Ambiguous)**

Scales: Narrative framing, conscience tracking, faction reputation effects, moral choice consequences.

Values:
- Clear: Obvious right/wrong framing (moral certainty, faction alignment obvious)
- Ambiguous: Gray morality (complex trade-offs, unclear best choice)

Derivation: Automatically derived from location properties (Temple → Clear) and NPC context. Most situations default to Ambiguous.

Example: Choice consequence framing.
- Clear morality: Narrative explicitly frames choice as righteous or wrongful
- Ambiguous morality: Narrative presents complex trade-offs, no clear answer

Same archetype, different moral context, contextually appropriate framing.

### Property Derivation Philosophy

All nine categorical properties derive automatically from entity state via `GenerationContext.FromEntities()`. Content authors specify entity properties (NPC personality, location properties, player state), and the system calculates categorical properties algorithmically.

This automatic derivation ensures:
- **Consistency**: Same entity state always produces same categorical properties
- **Simplicity**: Authors describe entities, system derives mechanics
- **Maintainability**: Derivation formulas centralized in one place
- **AI Enablement**: AI describes entities categorically, system handles numeric balance

Example derivation flow:
1. JSON specifies: `"npcDemeanor": "Hostile"`, `"locationProperties": ["Guarded"]`
2. Parser reads entity properties
3. `GenerationContext.FromEntities()` derives: `Danger = Risky` (Guarded location), `NpcDemeanor = Hostile`
4. Archetype generation applies: Hostile 1.4× threshold, Risky danger consequences
5. Result: Appropriately difficult, appropriately dangerous encounter

Authors never manually calculate. System derives from categorical entity descriptions.

### Scaling Formula Application

Generator receives entity objects and queries properties. Formulas apply automatically at generation time (parse-time, not runtime).

**Generation Process**:

1. Scene specifies archetype ID + target entities (concrete IDs or categorical filters)
2. Parser resolves entities via GameWorld lookup or spawn-context matching
3. Generator receives entity objects: `GenerateNegotiation(npc, location, spot)`
4. Generator queries properties: `npc.Demeanor`, `location.Quality`, entity-specific `PowerDynamic`
5. Generator looks up scaling multipliers from properties
6. Generator applies formulas: `scaledThreshold = baseThreshold × demeanorMultiplier × powerMultiplier`
7. Generator creates ChoiceTemplates with scaled values
8. Parser adds ChoiceTemplates to Situation
9. Runtime uses concrete scaled values directly (no catalogue calls, no string matching)

**Critical Point**: Scaling happens ONCE at parse time. Runtime never calls catalogues, never does string matching, never calculates dynamically. Concrete values stored in entity properties. This is catalogue pattern enforcement.

### Procedural Variety Through Property Combinations

Same archetype generates vastly different experiences via property combinations:

**Low-Difficulty Instance**:
- Friendly NPC (0.6× stat threshold)
- Basic quality (0.7× coin cost)
- Dominant power dynamic (0.6× authority requirement)
- Result: Easy interaction, low costs, accessible to early-game characters

**High-Difficulty Instance**:
- Hostile NPC (1.4× stat threshold)
- Luxury quality (2.5× coin cost)
- Submissive power dynamic (1.4× authority requirement)
- Result: Hard interaction, high costs, challenging for late-game characters

**SAME archetype definition. DIFFERENT entity properties. CONTEXTUALLY APPROPRIATE difficulty via universal scaling formulas.**

Mathematical variety: 3 demeanors × 4 qualities × 3 power dynamics × 3 environment qualities = 108 property combinations per archetype. 21 archetypes × 108 combinations = 2,268 mechanical variations. Add narrative variety from entity context = effectively infinite content.

## AI Narrative Generation Enablement

Categorical property system enables AI to generate balanced content without global game state knowledge.

### The AI Knowledge Problem

AI generating content doesn't know:
- Player's current progression level (are they hour 1 or hour 100?)
- Current economy balance (what's expensive vs cheap at this point?)
- Stat distribution (which thresholds are appropriate?)
- Challenge difficulty curves (what's easy vs hard now?)

If AI must know these things, AI becomes tightly coupled to game balance. Every balance change requires regenerating all AI content. AI cannot generate on-the-fly procedural content because it lacks real-time game state.

### The Categorical Solution

AI writes entities with CATEGORICAL descriptions:
- "This innkeeper is friendly and welcoming"
- "This bathhouse offers premium amenities"
- "You have little leverage in this negotiation"
- "The healing temple provides exceptional care"

These categorical descriptions map to enum properties:
- Friendly → NPCDemeanor.Friendly
- Premium → Quality.Premium
- Little leverage → PowerDynamic.Submissive
- Exceptional → EnvironmentQuality.Premium

System translates categorical properties to balanced numbers via universal formulas at parse time. AI doesn't calculate. System calculates from AI's categorical descriptions.

### AI Content Generation Process

**Step 1: AI Writes Fictional Entity**

AI describes a character named Elena as a warm, welcoming innkeeper with friendly demeanor who treats travelers like family. This is fiction-appropriate description requiring no game balance knowledge.

**Step 2: AI References Archetype**

AI specifies which archetype pattern to use by referencing a service archetype ID and identifying the target NPC and location. Doesn't define mechanics, just selects pattern.

**Step 3: Parser Derives Context from Entities**

Parser creates generation context by extracting categorical properties from entities. Friendly demeanor from NPC, standard quality from location, equal power dynamic by default, standard environment quality from location. No AI involvement. Pure data extraction.

**Step 4: Catalogue Generates Scaled Choices**

System takes base thresholds from archetype and applies scaling multipliers from context. Base authority threshold multiplied by demeanor multiplier produces scaled threshold. Base negotiation cost multiplied by quality multiplier produces scaled cost. Choice templates created with these scaled values. System handles all numeric balance. AI wrote friendly innkeeper, system made negotiation easier. AI wrote standard quality, system set baseline cost.

**Step 5: AI Enriches with Narrative**

After mechanical structure created, AI generates narrative text using entity context. For friendly innkeeper Elena, AI generates warm welcoming dialogue appropriate to the personality. Same archetype with hostile NPC would generate suspicious, terse dialogue instead. Same mechanical structure, different narrative texture from entity properties.

### Why This Matters

AI writes fiction-appropriate content ("This character seems friendly and welcoming"). System derives appropriate mechanics from fiction properties (Friendly → 0.6× stat threshold scaling). AI doesn't need balance knowledge. System provides balance automatically.

This enables:
- **Procedural generation without balance knowledge**: AI can generate entities at runtime using only fictional context
- **Balance consistency**: All content uses same universal formulas, no manual tuning per entity
- **Mechanical flexibility**: Change scaling multipliers in one place, all content adjusts
- **Narrative-mechanical alignment**: Properties serving narrative (friendly innkeeper) automatically affect mechanics appropriately
- **Infinite content potential**: AI can generate unlimited entities with categorical properties, system generates balanced mechanics automatically

## The Complete 21 Situation Archetypes

This catalog documents all 21 implemented situation archetypes. Each entry describes when to use the archetype, its mechanical pattern, and example contexts.

### TIER 1: Core Archetypes (5)

**1. CONFRONTATION (Authority/Dominance)**
- **When**: Gatekeepers, obstacles, authority challenges
- **Primary Stat**: Authority
- **Coin Cost**: 15
- **Challenge Type**: Physical
- **Pattern**: Authority stat → Pay off → Physical challenge → Submit

**2. NEGOTIATION (Diplomacy/Trade)**
- **When**: Merchants, transactional exchanges, deals
- **Primary Stat**: Diplomacy/Rapport
- **Coin Cost**: 15
- **Challenge Type**: Mental
- **Pattern**: Persuade → Pay premium → Debate → Accept terms

**3. INVESTIGATION (Insight/Discovery)**
- **When**: Information gathering, puzzle solving, deduction
- **Primary Stat**: Insight/Cunning
- **Coin Cost**: 10
- **Challenge Type**: Mental
- **Pattern**: Deduce → Hire expert → Work puzzle → Give up

**4. SOCIAL MANEUVERING (Rapport/Manipulation)**
- **When**: Social circles, subtle influence, persuasion
- **Primary Stat**: Rapport/Cunning
- **Coin Cost**: 10
- **Challenge Type**: Social
- **Pattern**: Read people → Offer gift → Bold gambit → Alienate

**5. CRISIS (Emergency Response)**
- **When**: Urgent situations, decisive action, time pressure
- **Primary Stat**: Authority/Insight
- **Coin Cost**: 25
- **Challenge Type**: Physical
- **Pattern**: Expert action → Expensive solution → Personal risk → Flee

### TIER 2: Expanded Archetypes (10)

**6. SERVICE TRANSACTION**
- **When**: Paying for services (lodging, food, passage)
- **Primary Stat**: None
- **Coin Cost**: 5
- **Challenge Type**: Mental

**7. ACCESS CONTROL**
- **When**: Gatekeepers, locked doors, restricted areas
- **Primary Stat**: Authority/Cunning
- **Coin Cost**: 15
- **Challenge Type**: Physical

**8. INFORMATION GATHERING**
- **When**: Rumors, gossip, local knowledge
- **Primary Stat**: Rapport/Insight
- **Coin Cost**: 8
- **Challenge Type**: Social

**9. SKILL DEMONSTRATION**
- **When**: Proving competence, showing credentials
- **Primary Stat**: Diplomacy/Insight
- **Coin Cost**: 12
- **Challenge Type**: Mental

**10. REPUTATION CHALLENGE**
- **When**: Defending honor, responding to accusations
- **Primary Stat**: Authority/Diplomacy
- **Coin Cost**: 10
- **Challenge Type**: Social

**11. EMERGENCY AID**
- **When**: Medical crisis, rescue situations
- **Primary Stat**: Insight/Authority
- **Coin Cost**: 20
- **Challenge Type**: Physical

**12. ADMINISTRATIVE PROCEDURE**
- **When**: Bureaucracy, permits, official processes
- **Primary Stat**: Diplomacy/Insight
- **Coin Cost**: 12
- **Challenge Type**: Mental

**13. TRADE DISPUTE**
- **When**: Disagreements over goods, quality, terms
- **Primary Stat**: Insight/Diplomacy
- **Coin Cost**: 15
- **Challenge Type**: Mental

**14. CULTURAL FAUX PAS**
- **When**: Social blunders, tradition violations
- **Primary Stat**: Rapport/Insight
- **Coin Cost**: 10
- **Challenge Type**: Social

**15. RECRUITMENT**
- **When**: Join requests, commitment decisions
- **Primary Stat**: Cunning/Diplomacy
- **Coin Cost**: 8
- **Challenge Type**: Social

### TIER 3: Specialized Service Archetypes (6)

**16. SERVICE_NEGOTIATION**
- **Pattern**: 4 choices (stat/money/challenge/fallback) → Secures service access → Grants key/token item

**17. SERVICE_EXECUTION_REST**
- **Pattern**: 4 rest variants (balanced/physical/mental/special) → Restores resources → Advances to next day Morning

**18. SERVICE_DEPARTURE**
- **Pattern**: 2 choices (immediate/careful) → Cleanup and exit → Optional preparation buff

**19. REST_PREPARATION**
- **Pattern**: Preparing to rest → Optimize recovery → Comfort items → Force relaxation → Collapse

**20. ENTERING_PRIVATE_SPACE**
- **Pattern**: First entry into private room → Inspect and optimize → Request amenities → Push through → Collapse

**21. DEPARTING_PRIVATE_SPACE**
- **Pattern**: Leaving private space → Check carefully → Leave gratuity → Rush out

## Archetype Usage Patterns

### Creating vs Reusing

**Create NEW Situation Archetype When**:
- Different choice count (not 4 choices)
- Different PathType distribution (not stat/money/challenge/fallback pattern)
- Different scaling properties (new categorical properties affect difficulty)
- Different cost structure (new resource types involved)

Example: Investigation archetype needs Attention cost (not Resolve), different from negotiation.

**Reuse EXISTING Situation Archetype When**:
- Same mechanical pattern (4 choices with same PathType distribution)
- Same scaling properties (NPCDemeanor/Quality/PowerDynamic apply same way)
- Different fictional context (access bathhouse vs access library - same mechanics)

Example: Negotiation archetype applies to services, information, passage, access control.

**Create NEW Scene Archetype When**:
- Different situation count (not 3-4 situations)
- Different transition pattern (not linear Always transitions)
- Different resource dependencies (new types of generated locations/items)
- Different progression structure (new relationship between situations)

Example: Investigation scene needs hub-and-spoke (3 parallel leads + convergence), not linear.

**Reuse EXISTING Scene Archetype When**:
- Same situation count (3-4 situations)
- Same transition pattern (linear Always transitions)
- Different domain (inn lodging vs bathhouse service vs healer treatment)

Example: All services follow negotiate → execute → depart pattern regardless of service type.

### Placement Filters and Spawn Conditions

PlacementFilter and SpawnConditions are CONFIGURATION, not STRUCTURE. They determine WHERE and WHEN archetypes spawn, never WHAT archetypes contain.

**PlacementFilter** (WHERE scene spawns):

Filters operate as pure eligibility checks examining entity properties:

- **NPC Filters**: Personality types, bond ranges, regional affiliation, NPC-specific tags
- **Location Filters**: Property types, settlement tiers, danger levels, available services, district membership
- **Route Filters**: Terrain types, danger ratings, travel tiers, route state
- **Player State Filters**: Required/forbidden states, achievement gates, scale positions, inventory requirements

**Critical Distinction**: PlacementFilter never affects what a scene IS, only where it appears. A `single_negotiation` scene has identical structure whether spawning at Merchant in city or Diplomat in wilderness. Only narrative hints and stat requirements adjust based on context properties.

**SpawnConditions** (WHEN scene becomes eligible):

Pure eligibility checks examining temporal and state conditions:

- **Player State Conditions**: Completed scenes, choice history, stat thresholds, inventory possession, tag requirements
- **World State Conditions**: Day ranges, time blocks, weather conditions, seasonal timing
- **Entity State Conditions**: NPC bond levels, location reputation, route travel counts, entity-specific flags
- **Cooldown Management**: Time since last spawn, completion count limits, exclusivity constraints

SpawnConditions gate access without changing what happens when scene spawns. They answer "is this the right time?" not "what should this scene do?"

**Both are pure configuration**: Content authors specify filters and conditions in JSON. This is configuration space - choosing where and when pre-designed patterns appear, never modifying patterns themselves.

## Content Examples

### Example 1: Tutorial Scene "Secure Lodging"

**Template Specification**:
```json
{
  "sceneArchetypeId": "service_with_location_access",
  "targetNpcId": "elena_npc",
  "targetLocationId": "common_room",
  "targetDependentLocation": "private_room"
}
```

**Entity Properties**:
- Elena: Personality=Innkeeper, Demeanor=Friendly
- Common Room: Services=Lodging, Quality=Standard, DangerLevel=Safe
- Private Room: PrivacyLevel=Private, Comfort=Standard

**Generation Process**:

1. Generator receives entities and queries properties
2. Elena.Demeanor=Friendly → difficulty modifier -1 (0.6× threshold)
3. Common Room.Quality=Standard → cost modifier baseline (1.0× cost)
4. Private Room.Comfort=Standard → restoration baseline (1.0× restoration)
5. Total difficulty: -1 (friendly offsets standard costs)

**Generated Situation 1 (Negotiation)**:

Choice 1: "Use your natural rapport" (Rapport ≥ 3) → Immediate key grant, best relationship
Choice 2: "Pay 10 coins" → Key grant, cordial relationship
Choice 3: "Negotiate shrewdly" → Social challenge, variable outcome, both advance
Choice 4: "Offer to help with work tomorrow" → Time cost, basic outcome, certain

**Generated Situation 2-4**: Access (consume key, unlock room) → Rest (restore resources) → Depart (remove key, lock room).

**Tutorial Value**: Teaches four-choice pattern, demonstrates guaranteed progression, shows resource trade-offs, establishes arrival pattern for all A-story.

### Example 2: Procedural Scene "Find Lodging"

**Template Specification**:
```json
{
  "sceneArchetypeId": "service_with_location_access",
  "placementFilter": {
    "npcPersonality": "Innkeeper",
    "npcRegion": "CurrentRegion"
  },
  "targetLocationFilter": {
    "settlement": "Urban",
    "services": ["Lodging"]
  }
}
```

**Runtime Resolution**:

Player enters new city. System finds "marcus_npc" matching filter (Personality=Innkeeper, Demeanor=Professional, Region=CurrentRegion). System finds "Golden Rest Inn" matching filter (Settlement=Urban, Services=Lodging, Quality=Premium).

**Entity Properties**:
- Marcus: Demeanor=Professional
- Golden Rest Inn: Quality=Premium
- Generated Room: Comfort=Premium

**Generation Process**:

1. Marcus.Demeanor=Professional → difficulty modifier 0 (1.0× threshold)
2. Inn.Quality=Premium → cost modifier +60% (1.6× cost)
3. Room.Comfort=Premium → restoration +200% (3.0× restoration)
4. Total difficulty: +2 (premium quality increases costs significantly)

**Generated Situation 1 (Negotiation)**:

Choice 1: "Use your reputation" (Rapport ≥ 6) → Immediate key grant, elevated relationship
Choice 2: "Pay 16 coins" → Key grant, professional relationship
Choice 3: "Negotiate terms" → Mental challenge, variable outcome, both advance
Choice 4: "Inquire about work exchange" → Time cost, basic outcome, certain

Higher costs, higher thresholds, but also higher restoration benefits. Same archetype, different properties, contextually appropriate difficulty.

### Example 3: Crisis Archetype Reusability

Same CRISIS archetype applies to radically different contexts:

**Context 1: Medical Emergency at Temple**
- Priest: Demeanor=Compassionate, PowerDynamic=Equal
- Temple: EnvironmentQuality=Premium
- Crisis: Injured traveler needs immediate care
- Choice 1: "Provide expert medical aid" (Insight ≥ 6) → Best outcome
- Choice 2: "Pay for temple healers" (25 coins) → Good outcome
- Choice 3: "Attempt emergency treatment" (Physical challenge) → Variable outcome
- Choice 4: "Provide basic first aid" → Minimal outcome, certain

**Context 2: Combat Emergency in Wilderness**
- Bandit: Demeanor=Hostile, PowerDynamic=Dominant
- Wilderness: EnvironmentQuality=Basic
- Crisis: Bandits ambush, need immediate response
- Choice 1: "Take command decisively" (Authority ≥ 8) → Best outcome
- Choice 2: "Bribe bandits heavily" (40 coins) → Good outcome
- Choice 3: "Fight your way out" (Physical challenge) → Variable outcome
- Choice 4: "Flee immediately" → Minimal outcome, certain

**Context 3: Political Emergency in Palace**
- Noble: Demeanor=Neutral, PowerDynamic=Submissive
- Palace: EnvironmentQuality=Premium
- Crisis: Diplomatic incident requires immediate resolution
- Choice 1: "Navigate diplomatically" (Authority ≥ 7) → Best outcome
- Choice 2: "Offer substantial compensation" (35 coins) → Good outcome
- Choice 3: "Take personal responsibility" (Social challenge) → Variable outcome
- Choice 4: "Defer to authorities" → Minimal outcome, certain

Same archetype ID. Same mechanical structure. Different entity properties. Different narrative context. Contextually appropriate difficulty and tone. This is archetype reusability in practice.

## Technical Implementation Cross-Reference

This section describes content generation design. Technical implementation details covered in arc42 documentation:

- SceneArchetypeCatalog class: arc42 Section 5 (Building Block View)
- SituationArchetypeCatalog class: arc42 Section 5 (Building Block View)
- Generation pipeline: arc42 Section 4 (Solution Strategy)
- Parser and catalogue timing: arc42 Section 8 (Cross-Cutting Concepts)
- Property-driven difficulty: arc42 Section 8 (Cross-Cutting Concepts)
- Entity resolution: arc42 Section 5 (Building Block View)

## Entity Content Requirements

### Mandatory Location Properties

**All Locations MUST Have Hex Positions**: Every location entity in the game world must specify HexPosition coordinates. The hex-based spatial architecture requires explicit hex coordinates for all travel calculations, route generation, and map display. Locations without hex positions break the three-tier loop structure (SHORT/MEDIUM/LONG) and prevent proper route travel mechanics.

**Rationale**: The hex grid is the fundamental spatial model for Wayfarer. Route segments connect hex positions. Travel time calculates from hex distance. Map visualization renders hex positions. Fractal map generation depends on hex coordinates for regional hierarchy. Content generation archetypes assume valid hex positions for all location entities.

**Validation**: Parser must enforce hex position presence for all location types (settlements, venues, specific locations within venues). Missing hex positions should fail validation and prevent game initialization.

## Dynamic Location Generation

### Overview

Beyond archetype-based content generation, Wayfarer supports runtime creation of locations and venues when scenes spawn. This enables self-contained scenes to materialize their own spatial context on demand, supporting infinite world expansion without exhaustive pre-authoring.

**Core Pattern**: Scenes specify categorical requirements for locations they need. System attempts to match existing content first (prefer authored over generated). If no match exists and explicit generation requested via DependentLocationSpec, system generates location procedurally with validation. **All generated locations persist forever** - no cleanup system exists.

### Design Philosophy

**Lazy Materialization**: World expands in response to narrative need, not pre-emptively. Tutorial inn exists from day one (authored). Side quest hideout materializes when quest spawns (generated). Avoids authoring unused content.

**Bootstrap Gradient**:
- Early game (Prologue/Act 1): 95% authored, 5% generated (stability priority)
- Mid game (Act 2/3): 60% authored, 40% generated (variety increases)
- Late game (Act 4+): 20% authored, 80% generated (infinite expansion)

**Match First, Generate Last**: System prefers existing content over generation. Query authored locations with PlacementFilter categorical matching. Generate only when explicitly requested via DependentLocationSpec. Fail-fast if PlacementFilter finds no match (no silent fallback).

**Bounded Infinity**: Generation operates under capacity constraints. Venues have MaxLocations (small town: 5-10, large city: 50-100, wilderness: unlimited). Capacity applies to ALL locations (authored + generated), not just generated. Prevents infinite uncontrolled expansion while enabling variety.

### Categorical Matching vs Generation

**PlacementFilter Matching**:
- Scene specifies categorical requirements (LocationProperties, LocationTags, DistrictId)
- System queries GameWorld.Locations for matches
- SelectionStrategy chooses from multiple matches (Closest, LeastRecent, WeightedRandom)
- Fail-fast if no match (no silent degradation, no fallback generation)

**Example Filter**:

A placement filter specifies location type, required properties including private indoor secluded characteristics, district identification for lower wards, and closest selection strategy. Matches authored locations with all specified properties in specified district. System throws exception if no match, forcing content design to either author matching content or relax filter constraints.

**Explicit Generation via DependentLocationSpec**:
- DependentLocationSpec defines location to generate (self-contained scenes)
- NamePattern and DescriptionPattern with placeholder support
- Properties define available actions
- HexPlacementStrategy determines spatial positioning
- VenueIdSource determines containing venue (SameAsBase or GenerateNew)

**Example Spec**:

A dependent location specification defines a private room template with name and description patterns incorporating NPC name placeholders, venue source as same as base location, hex placement within same venue, properties for sleeping space with restful indoor private characteristics, and initially locked status.

Generates location deterministically when scene spawns. Location flows through standard JSON → DTO → Parser → Entity pipeline (Catalogue Pattern compliance).

### Persistence Model

**All Locations Persist Forever**: Generated locations are never cleaned up. Once materialized, they become permanent world features.

**Provenance Tracking**: Every generated location stores SceneProvenance (which scene created it, when, why). Authored locations have Provenance = null. This metadata is for tracking/debugging only - not used for lifecycle decisions.

**Budget Enforcement**: Since locations persist forever, venue capacity budgets are **critically important**. Budget validation happens at generation time (fail-fast) to prevent unbounded world growth.

### Validation and Playability

**Fail-Fast Principle**: ALL locations must be functionally playable. Unplayable content worse than crash (forces fixing root cause).

**LocationPlayabilityValidator checks** (applies to authored + generated):
1. **Hex Position**: Location must have valid hex coordinates
2. **Reachability**: Must be reachable from player (same venue OR route exists)
3. **Venue**: Must belong to valid venue
4. **Properties**: Must have at least one property (enables action generation)
5. **Unlock Mechanism**: If locked, must have unlock path in scene

Validator throws InvalidOperationException if any check fails. System crashes rather than creating inaccessible content.

### Generation Budget System

**Venue Capacity**: Each venue enforces capacity budget through maximum location property with default of twenty locations and capacity checking method comparing current location count against maximum. Budget is **derived** (counts existing LocationIds) not **tracked** (no separate counter). This enforces Catalogue Pattern: generated locations become indistinguishable from authored locations after parsing. Capacity applies to ALL locations equally.

**Bidirectional Relationship Maintenance**:
- `Venue.LocationIds` ↔ `Location.Venue` must stay synchronized
- **Authored locations**: `VenueParser` copies `dto.locations` → `venue.LocationIds` from JSON
- **All locations**: `GameWorld.AddOrUpdateLocation()` ensures `venue.LocationIds` contains location when added
- **Thread-safe**: Capacity check and add operation locked on venue object to prevent race conditions
- **CRITICAL**: Capacity budget depends on LocationIds.Count being accurate

**Budget Exhaustion**: When venue reaches capacity, BuildLocationDTO throws InvalidOperationException (fail-fast). Generation fails before DTO creation. Content authors must ensure sufficient capacity or use different venues. Since locations persist forever, budget violations cannot be cleaned up - prevention is critical.

**Budget Design**:
- Small venues (temporary camps): 5 locations
- Medium venues (villages): 10-20 locations
- Large venues (cities): 50-100 locations
- Wilderness venues: Unlimited (or very high cap like 500)

### Hex Grid Integration

**Spatial Requirements**: All locations MUST have HexPosition. Hex grid is fundamental spatial model for travel system.

**Placement Strategies**:
- **SameVenue**: Adjacent hex in same venue cluster (intra-venue movement instant/free)
- **Adjacent**: One of 6 neighbors of base location (cross-venue requires route)
- **Distance/Random**: Future extensions for specific spatial patterns

**Hex Synchronization**: Location.HexPosition is source of truth, Hex.LocationId is derived lookup. HexSynchronizationService maintains consistency.

### Integration with Archetype System

**Complementary Patterns**:
- **Archetypes**: Define interaction mechanics (how player engages with content)
- **Dynamic Generation**: Define spatial context (where interactions occur)

**Example Flow**:
1. Scene specifies archetype `service_with_location_access` (mechanical structure)
2. Scene specifies PlacementFilter for location (categorical matching) OR DependentLocationSpec (explicit generation)
3. System matches existing location (PlacementFilter) OR generates new location (DependentLocationSpec)
4. Archetype generates situations using location properties (scaled mechanics)

Archetypes remain pure mechanical patterns. Dynamic generation provides spatial context. Separation of concerns maintained.

### Technical Implementation Cross-Reference

Implementation details in arc42 documentation:
- VenueGeneratorService: Section 5 (Building Block View)
- LocationPlayabilityValidator: Section 5 (Building Block View)
- HexSynchronizationService: Section 5 (Building Block View)
- SceneInstantiator.BuildLocationDTO: Section 5 (Building Block View)
- Dynamic World Building pattern: Section 8 (Crosscutting Concepts)

## Conclusion

Wayfarer's archetype-based content generation enables infinite variety from finite patterns. The three-tier hierarchy (5 core, 10 expanded, 6 specialized archetypes) provides mechanical foundation for all content. Categorical property scaling (demeanor, quality, power dynamic, environment quality) creates contextually appropriate difficulty without modifying archetypes. Two-level composition (scene archetypes calling situation archetypes) generates complete multi-situation flows.

Dynamic location generation complements archetype system by materializing spatial context on demand. Match-first approach preserves authored content priority. **All generated locations persist forever** - no cleanup system. Generation budgets bound procedural expansion through fail-fast validation at generation time. Fail-fast validation ensures playability. PlacementFilter never falls back to generation (explicit paths only).

The system enables AI to generate balanced content by writing categorical entity descriptions. AI provides fiction-appropriate properties. System derives appropriate mechanics via universal scaling formulas. This decoupling allows infinite procedural content generation without requiring AI to understand game balance.

The result: effectively infinite content from 21 situation archetypes and 12 scene archetypes, all mechanically consistent, all contextually appropriate, all spatially integrated through dynamic world building, supporting the infinite A-story and extensive B/C side content that defines Wayfarer's narrative experience.
