# Wayfarer Mechanical Content Contracts

## Core Design Philosophy

This system generates all narrative through mechanical causality and contextual AI interpretation. Every description, observation, and story element emerges from mechanical relationships, temporal states, and hierarchical world organization. The AI translates mechanical configurations into appropriate narrative, but the underlying system is purely deterministic.

## World Hierarchy

### Region (Thematic Container)

**Properties:**
- `id`: Unique identifier
- `prosperityLevel`: Enum (Thriving/Stable/Declining/Desperate)
- `authorityType`: Enum (Royal/Military/Merchant/Criminal/Religious)
- `culturalTags`: List of strings (architectural style, customs, dress)
- `currentEvents`: List of active event IDs
- `basePriceModifier`: Integer percentage (affects all commerce)
- `narrativeThemes`: List of strings for AI tone

**Implementation Notes:**
Regions set the overall tone and economic reality. A Declining region with Military authority generates different narrative than a Thriving region with Merchant authority. These tags affect all AI-generated content within the region.

### District (Neighborhood)

**Properties:**
- `id`: Unique identifier
- `parentRegionId`: String
- `wealthLevel`: Enum (Opulent/Wealthy/Modest/Poor/Destitute)
- `populationDensity`: Enum (Packed/Crowded/Busy/Sparse/Empty)
- `primaryFunction`: Enum (Residential/Commercial/Industrial/Administrative/Religious)
- `dangerLevel`: Integer (0-5, affects encounter probability)
- `architecturalTags`: List of strings
- `districtTraits`: List of mechanical modifiers

**District Trait Entry:**
- `traitType`: Enum (GuardPresence/BlackMarket/SafeHaven/Lawless)
- `intensity`: Integer (1-3)
- `timeRestriction`: Time period or null

**Implementation Notes:**
Districts create local flavor within regions. Wealth level affects NPC dress descriptions, available services, and building conditions. Population density affects crowd descriptions and observation availability. The AI uses these tags to generate appropriate atmosphere.

### Location (Actual Places)

**Properties:**
- `id`: Unique identifier
- `parentDistrictId`: String
- `locationType`: Enum (Market/Tavern/Temple/Palace/Street/Square/Shop/Home)
- `operatingSchedule`: List of time periods when accessible
- `ownerNpcId`: String or null
- `baseCapacity`: Integer (how many people fit)
- `locationSpots`: List of spot IDs
- `connectedRoutes`: List of route IDs
- `mechanicalTraits`: List of traits
- `ambienceTags`: List of strings per time period

**Mechanical Trait Entry:**
- `trait`: Enum (Public/Private/Sacred/Commercial/Dangerous/Safe/Loud/Quiet)
- `timePeriods`: List of periods when active

**Time-Based Configuration:**
- `timeSchedules`: List of schedule entries

**Schedule Entry:**
- `timePeriod`: Enum (Morning/Midday/Afternoon/Evening/Night/DeepNight)
- `presentNpcs`: List of NPC IDs
- `crowdLevel`: Enum (Empty/Sparse/Moderate/Busy/Packed)
- `availableObservations`: List of observation templates
- `atmosphereTags`: List of strings for this time period

**Implementation Notes:**
Locations change dramatically by time. A market is Packed at Morning with merchant NPCs and commerce observations, but Empty at Night with only guard NPCs and danger observations. The AI generates different descriptions for each time period using the atmospheric tags.

### Location Spot (Interaction Points)

**Properties:**
- `id`: Unique identifier
- `parentLocationId`: String
- `spotType`: Enum (Table/Corner/Bar/Stage/Altar/Counter/Booth/Alcove)
- `privacyLevel`: Enum (Exposed/Public/Discrete/Private/Hidden)
- `capacity`: Integer (1-4 typically)
- `comfortModifier`: Integer (-2 to +2, affects patience)
- `specialProperties`: List of strings
- `narrativeTags`: List of strings

**Special Property Examples:**
- "OverhearsBarConversations"
- "ViewsMainEntrance"
- "NearFireplace"
- "DarkCorner"

**Implementation Notes:**
Spots enable specific interactions. A Private booth allows sensitive conversations. An Exposed table might limit what NPCs will discuss. The AI uses tags to describe the specific area within the location.

## Travel System

### Route (Direct Path)

**Properties:**
- `id`: Unique identifier
- `originLocationId`: String
- `destinationLocationId`: String
- `transportType`: Enum (Walk/Cart/Horse/Boat/Carriage)
- `baseMinutes`: Integer
- `costCoins`: Integer
- `familiarityLevel`: Enum (Known/Learning/Unfamiliar/Dangerous)
- `statusRequirement`: Integer or null
- `routeTraits`: List of traits
- `narrativeTags`: List of strings

**Route Trait Entry:**
- `trait`: Enum (Scenic/Direct/Dangerous/Patrolled/Hidden/Public)
- `effect`: Mechanical modifier

**Familiarity Effects:**
- Known: No encounter
- Learning: Draw 2 encounters, choose 1
- Unfamiliar: Draw 1 encounter, must face
- Dangerous: Draw 1 encounter, must face, negative weight

**Implementation Notes:**
Each route is a single transport option between two specific locations. You cannot travel directly between districts - only between connected locations. A cart route from Market to Docks is different from a boat route between the same locations. Travel time is always in minutes or hours, never days.

### Encounter (Travel Event)

**Properties:**
- `id`: Unique identifier
- `encounterTemplate`: Enum (GuardCheckpoint/Merchant/Beggar/Accident/Discovery)
- `validRouteTraits`: List of traits where this can occur
- `validTimePerods`: List of when this can occur
- `mechanicalChoice`: Choice structure
- `contextualTags`: List of strings for AI generation

**Choice Structure:**
- `option1`: {cost: Resource object, outcome: Effect object, narrativeTags: List}
- `option2`: {cost: Resource object, outcome: Effect object, narrativeTags: List}
- `option3`: Optional third choice

**Resource Object:**
- `type`: Enum (Coins/Minutes/Attention/Status/Health)
- `amount`: Integer

**Effect Object:**
- `type`: Enum (Nothing/Observation/StateChange/RouteUnlock/ItemGain)
- `value`: Appropriate value for type

**Implementation Notes:**
Encounters are generated based on route traits and time. A Patrolled route at Night might generate guard encounters. A Hidden route at Morning might generate discovery encounters. The AI creates specific narrative from templates and context.

## Temporal System

### Time Period (Clock Segments)

**Properties:**
- `periodName`: Enum (Morning/Midday/Afternoon/Evening/Night/DeepNight)
- `hourRange`: [start, end] in 24-hour format
- `globalModifiers`: List of mechanical effects
- `narrativeTone`: List of strings

**Period Definitions:**
- Morning: 6:00-10:00 (busy, fresh, merchants opening)
- Midday: 10:00-14:00 (peak activity, hot, crowded)
- Afternoon: 14:00-18:00 (winding down, golden light)
- Evening: 18:00-22:00 (social time, taverns busy, shops closing)
- Night: 22:00-2:00 (quiet, dangerous, illicit)
- DeepNight: 2:00-6:00 (empty, very dangerous, secret)

**Implementation Notes:**
Time periods globally affect the world. All locations reference these periods for their schedules. NPCs move between locations at period boundaries. Observations refresh each period.

### World Event (Dynamic Modifiers)

**Properties:**
- `id`: Unique identifier
- `eventType`: Enum (Festival/Raid/Storm/Curfew/Market/Funeral/Arrival)
- `affectedRegions`: List of region IDs
- `affectedDistricts`: List of district IDs
- `duration`: Integer hours
- `mechanicalEffects`: List of effects
- `narrativeImpact`: List of strings

**Mechanical Effect Entry:**
- `effectType`: Enum (CrowdIncrease/PriceChange/RouteBlock/NPCMood)
- `magnitude`: Integer
- `targetIds`: List of affected entity IDs

**Implementation Notes:**
Events temporarily modify the world state. A Festival increases crowds and improves NPC moods. A Guard Raid makes certain NPCs unavailable and increases tension. The AI incorporates active events into all generated narrative.

## Dynamic Content Generation

### Observation (Generated Knowledge)

**Properties:**
- `templateId`: Unique identifier
- `generationType`: Enum (Social/Environmental/Commercial/Authority/Secret)
- `validLocationTypes`: List of location types where applicable
- `validTimePeriods`: List of when it can appear
- `requiredTraits`: List of location traits needed
- `cardWeight`: Integer (1-2)
- `cardPersistence`: Always "Opportunity"
- `attentionCost`: Integer
- `mechanicalTrigger`: State modification or null
- `relevanceFilters`: List of NPC filters
- `generationTags`: List of strings for AI

**Relevance Filter Entry:**
- `filterType`: Enum (NPCType/TokenType/RelationshipLevel/State)
- `value`: Appropriate value

**Generation Rules:**
- Each location + time period can generate 3-5 observations
- Observations cannot repeat within 3 time periods
- Player's past observations influence future generation
- Current world events modify available observations

**Implementation Notes:**
Observations are generated fresh each time period based on location, time, events, and history. The same market might offer "merchant arguing about prices" at Morning but "guards changing shift" at Evening. The AI creates specific narrative from generation tags and context.

### NPC Schedule (Movement Patterns)

**Properties:**
- `npcId`: String
- `scheduleType`: Enum (Fixed/Flexible/Random/Event-driven)
- `scheduleEntries`: List of entries

**Schedule Entry:**
- `timePeriod`: Enum
- `locationId`: String
- `spotPreference`: String or null
- `alternativeLocationId`: String (if primary unavailable)
- `activityTags`: List of strings

**Implementation Notes:**
NPCs move through the world on schedules. A merchant is at Market during Morning, Tavern during Evening. This affects where conversations can happen and what states NPCs are in based on player punctuality.

## Conversation Integration

### NPC (Expanded for World Integration)

All previous NPC properties plus:

**Additional Properties:**
- `homeLocationId`: String (where they live)
- `workLocationId`: String (where they work)
- `scheduleId`: String
- `currentLocationId`: String (dynamic)
- `currentSpotId`: String or null (dynamic)
- `mobilityType`: Enum (Stationary/Local/District/Regional)

**Implementation Notes:**
NPCs exist in the world beyond conversations. Their location affects their emotional state (comfortable at home, tense at work). Their schedule determines availability. Missing them at expected locations might trigger obligations.

### Letter (Expanded for World Context)

All previous Letter properties plus:

**Additional Properties:**
- `pickupLocationId`: String (where letter originates)
- `deliveryLocationId`: String (where recipient will be)
- `routeRestrictions`: List of route IDs that cannot be used
- `weatherSensitive`: Boolean (storm affects deadline)
- `eventSensitive`: List of event types that affect urgency

**Implementation Notes:**
Letters exist in the world geography. Pickup and delivery locations matter for route planning. World events can affect urgency (military letters during raids become Critical).

## AI Narrative Generation

### Context Assembly

When generating narrative, the AI receives:

**For Location Descriptions:**
- Region prosperity and authority
- District wealth and density
- Location type and traits
- Current time period
- Current weather
- Active events
- Crowd level
- Present NPCs
- Player's familiarity level

**For Observations:**
- All location context above
- Observation template type
- Mechanical relevance filters
- Recent player observations (avoid repetition)
- NPC states that would care

**For NPC Dialogue:**
- Current emotional state
- Location context (comfortable/uncomfortable)
- Time pressure from obligations
- Recent world events
- Relationship history with player

**For Travel Narrative:**
- Route traits
- Time of day
- Weather conditions
- Origin and destination context
- Familiarity level
- Transport type

### Tagging Philosophy

Tags are mechanical, not literary. Instead of "mysterious" use mechanical tags like:
- LowVisibility
- InfrequentVisitors
- HiddenPurpose
- UnknownOwner

The AI interprets these mechanical states into appropriate narrative. A location with LowVisibility + InfrequentVisitors + Night generates different narrative than the same tags + Morning.
