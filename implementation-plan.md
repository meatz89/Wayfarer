Refined Obstacle System Design

  ✅ DESIGN QUALITY: EXCELLENT

  The refined design is fundamentally sound and solves the verisimilitude problem elegantly. The key insight - distributed interaction pattern - is brilliant.

  ---
  🎯 CRITICAL ARCHITECTURAL INSIGHT

  The Distributed Interaction Pattern

  This is the design's killer feature:

  Obstacle: "Suspicious Gatekeeper" (exists in GameWorld.Obstacles only)

  Goals scattered across world:
  ├─ "Pay Fee" → appears at north_gate (PlacementLocationId)
  ├─ "Ask About Miller" → appears at town_square
  ├─ "Get Official Pass" → appears at town_hall
  └─ "Show Official Pass" → appears at north_gate (unlocked by prep)

  Why this is genius:
  - Obstacle doesn't "live" anywhere - it's conceptual
  - Goals appear at different locations
  - Player discovers connections through exploration
  - "I should ask townspeople about the miller" feels organic
  - No string matching, no ID targeting (except within obstacle JSON)

  Player experience:
  1. North gate: "Pay Fee" (hard, 10 coins)
  2. Explore → town square: "Ask About Miller" (grants knowledge card)
  3. Explore → town hall: "Get Official Pass" (reduces SocialDifficulty 1→0)
  4. Return to gate: NOW see "Show Official Pass" (free, authority-based)

  The guard obstacle radiates influence across the town. This is perfect verisimilitude.

  ---
  🔴 CURRENT IMPLEMENTATION GAPS

  Gap 1: Property Count

  Current: 5 properties (PhysicalDanger, MentalComplexity, SocialDifficulty, StaminaCost, TimeCost)
  Design: 3 properties only

  Why 3 is correct:
  - Properties represent approach difficulty, not resource costs
  - StaminaCost/TimeCost belong in challenge outcomes, not obstacle properties
  - Simpler mental model: "How hard is this approach?"

  Action: Remove StaminaCost and TimeCost from Obstacle entity

  ---
  Gap 2: Goal Placement Semantics

  Current:
  Goal {
      string LocationId;  // Goal "belongs" to one location
      string NpcId;       // OR belongs to one NPC
  }

  Design:
  Goal {
      string PlacementLocationId;  // Where the BUTTON appears
      string PlacementNpcId;        // OR appears with NPC
  }

  Critical difference:
  - Current: Goal "belongs to" a location (wrong - limits to one place)
  - Design: Goal's button appears at a location (correct - just UI placement)

  Why this matters:
  Current (broken):
    Goal "Ask About Miller" has LocationId: "town_square"
    → Can ONLY appear at town square
    → All goals for guard must be at gate
    → Can't do distributed interaction

  Design (correct):
    Goal "Ask About Miller" has PlacementLocationId: "town_square"
    → Button appears at town square
    → Goal still belongs to gatekeeper obstacle
    → Distributed interaction works

  Action: Rename to PlacementLocationId/PlacementNpcId, clarify semantics

  ---
  Gap 3: Obstacle Location

  Current:
  Obstacle {
      string LocationId;  // Obstacle "lives" at location
  }
  Location {
      List<Obstacle> Obstacles;  // Direct containment
  }

  Design:
  Obstacle {
      // No location property - obstacles don't "live" anywhere
  }
  Location {
      List<string> ObstacleIds;  // References to obstacles with goals here
  }
  GameWorld {
      Dictionary<string, Obstacle> Obstacles;  // Single source of truth
  }

  Why this is critical:
  The guard obstacle can't "live" at the gate if it also has goals at town square and town hall. It must be location-agnostic.

  Storage pattern:
  GameWorld.Obstacles["gatekeeper"] = { ...obstacle with all goals... }

  Location "north_gate" {
      ObstacleIds: ["gatekeeper"]  // "Has goals from this obstacle"
  }

  Location "town_square" {
      ObstacleIds: ["gatekeeper"]  // Same obstacle, different goals visible
  }

  UI lookup:
  Player at north_gate:
  → location.ObstacleIds = ["gatekeeper"]
  → GameWorld.Obstacles["gatekeeper"] = { goals: [...] }
  → Filter goals where goal.PlacementLocationId == "north_gate"
  → Show "Pay Fee" and "Show Official Pass" (if requirements met)

  Player at town_square:
  → location.ObstacleIds = ["gatekeeper"]  // Same obstacle!
  → Filter goals where goal.PlacementLocationId == "town_square"
  → Show "Ask About Miller"

  Action: Remove location from Obstacle, change Location.Obstacles to Location.ObstacleIds

  ---
  Gap 4: Consequence Types

  Current:
  enum GoalEffectType {
      None,
      ReduceProperties,
      RemoveObstacle
  }

  Design:
  enum ConsequenceType {
      Resolution,  // Permanently overcome, removed from play
      Bypass,      // Player passes, obstacle persists for world
      Transform,   // Fundamentally changed (properties → 0, new description)
      Modify,      // Properties reduced, unlocks new goals
      Grant        // Give knowledge cards/items, no obstacle change
  }

  Why 5 types:
  Each has genuine trade-offs and different narrative outcomes:

  - Resolution: Violent, costly, permanent world change
  - Bypass: Quick, cheap, no relationship, obstacle remains
  - Transform: Diplomatic, builds relationships, creates obligations
  - Modify: Incremental progress, unlocks better approaches
  - Grant: Information gathering, tactical advantage, no immediate progress

  Example:
  Guard obstacle:
  - "Force Past" → Resolution (violent, permanent, Hostile relationship)
  - "Pay Fee" → Bypass (quick, costs coins, Neutral relationship)
  - "Befriend Guard" → Transform (time investment, Friendly relationship)
  - "Get Official Pass" → Modify (reduces SocialDifficulty, unlocks better goals)
  - "Ask About Miller" → Grant (knowledge card, tactical advantage)

  Action: Replace GoalEffectType with ConsequenceType, implement all 5 types

  ---
  Gap 5: State Tracking

  Current:
  Obstacle {
      bool IsCompleted;  // Simple flag
  }

  Design:
  Obstacle {
      ObstacleState State;              // Active/Resolved/Transformed
      ResolutionMethod ResolutionMethod; // How it was overcome
      RelationshipOutcome Relationship;  // Social impact
  }

  enum ObstacleState {
      Active,      // Currently blocking
      Resolved,    // Permanently overcome
      Transformed  // Changed but still exists
  }

  enum ResolutionMethod {
      Unresolved,
      Violence,     // Forced, destroyed, attacked
      Diplomacy,    // Negotiated, befriended, persuaded
      Stealth,      // Sneaked, avoided, bypassed undetected
      Authority,    // Official channels, credentials, legal power
      Cleverness,   // Outsmarted, found workaround
      Preparation   // Methodical reduction over multiple attempts
  }

  enum RelationshipOutcome {
      Hostile,    // Made enemies
      Neutral,    // No relationship
      Friendly,   // Built positive relationship
      Allied,     // Deep alliance
      Obligated   // Favors owed
  }

  Why this is critical for verisimilitude:
  The enums provide semantic context for AI narrative generation:

  If ResolutionMethod == Violence:
    AI generates: "You forced your way through, leaving destruction in your wake."

  If ResolutionMethod == Authority:
    AI generates: "The guard snaps to attention upon seeing your official seal."

  If ResolutionMethod == Stealth:
    AI generates: "You slip past unnoticed, leaving no trace of your passage."

  No string matching needed - just check enum value, generate appropriate narrative.

  Future goal requirements can check:
  "requirements": {
    "requiresRelationshipOutcome": "Friendly"
  }

  This enables: "Guard captain only talks to you if you befriended the gate guard (Friendly outcome)".

  Action: Add State, ResolutionMethod, RelationshipOutcome to Obstacle

  ---
  Gap 6: Knowledge Cards

  Current: Missing entirely

  Design:
  KnowledgeCardGrant {
      string CardId;
      string CardText;                    // "You mention the miller's reputation"
      CardEffect Effect;                  // +3 Momentum, -1 Doubt
      string ScopedToObstacleId;         // Default: parent obstacle
      SystemType TargetSystemType;       // Social/Mental/Physical
  }

  User says: "Optional"

  My assessment: High value, but not blocking

  Why knowledge cards matter for verisimilitude:

  Without cards (abstract):
  Player: Complete "Ask About Miller"
  System: SocialDifficulty reduced 1→0
  Player: Talk to guard
  Guard: "You may pass." (why did guard change mind?)

  With cards (verisimilitude):
  Player: Complete "Ask About Miller"
  System: Granted knowledge card "Miller's Reputation"
  Player: Talk to guard, draw "Miller's Reputation" card
  Player: Play card: "You mention you're visiting the miller on business"
  Guard: "Ah, the miller! Good man. Go on through."

  The card text provides narrative context the AI can reference. The player uses their knowledge tactically during the challenge.

  Recommendation: Phase 2 (after core architecture), high value

  ---
  ✅ WHAT'S ALREADY CORRECT

  Containment Pattern

  Obstacle {
      List<Goal> Goals;  // Inline children
  }

  This is correct per the design. Goals are not separate entities, they're children of obstacles.

  Within the obstacle JSON, local ID references are OK:
  {
    "goals": [
      {
        "id": "get_pass",
        "consequence": "Modify",
        ...
      },
      {
        "id": "show_pass",
        "requirements": {
          "requiredKnowledgeIds": ["get_pass"]  // Local reference OK
        }
      }
    ]
  }

  Property-Based Gating

  goal.Requirements.MaxPhysicalDanger <= obstacle.PhysicalDanger

  This works perfectly. Simple numerical threshold, no string matching.

  Investigation Integration

  Investigation phases spawn obstacles. This is correct per design.

  ---
  📋 IMPLEMENTATION PLAN

  Phase 1: Core Architecture (CRITICAL)

  1.1: Reduce to 3 Properties
  - Remove StaminaCost and TimeCost from Obstacle entity
  - Update Obstacle.cs, ObstacleDTO.cs, ObstacleParser.cs
  - Update ObstacleDisplay.razor to show only 3 properties
  - Grep for StaminaCost/TimeCost references, remove

  1.2: Goal Placement Semantics
  - Rename Goal.LocationId → Goal.PlacementLocationId
  - Rename Goal.NpcId → Goal.PlacementNpcId
  - Update Goal.cs, GoalDTO.cs, GoalParser.cs
  - Clarify comments: "Where this goal's button appears in UI"

  1.3: Remove Obstacle Location
  - Remove LocationId/RouteId/NpcId from Obstacle
  - Change Location.Obstacles: List<Obstacle> → Location.ObstacleIds: List<string>
  - Change NPC.Obstacles: List<Obstacle> → NPC.ObstacleIds: List<string>
  - Add GameWorld.Obstacles: Dictionary<string, Obstacle>
  - Update parsers to register in GameWorld.Obstacles
  - This is the big refactor

  1.4: UI Lookup Pattern
  Update LocationContent.razor.cs:
  // OLD:
  CurrentObstacles = CurrentSpot.Obstacles;

  // NEW:
  List<Obstacle> obstaclesAtLocation = new();
  foreach (string obstacleId in CurrentSpot.ObstacleIds) {
      Obstacle obstacle = GameWorld.Obstacles[obstacleId];
      obstaclesAtLocation.Add(obstacle);
  }

  // Then filter goals:
  foreach (Obstacle obstacle in obstaclesAtLocation) {
      List<Goal> goalsHere = obstacle.Goals
          .Where(g => g.PlacementLocationId == CurrentSpot.Id)
          .Where(g => RequirementsChecker.Check(g, obstacle))
          .ToList();
  }

  1.5: Update ObstacleGoalFilter
  Change from filtering by location's obstacles to:
  public List<Goal> GetVisibleLocationGoals(Location location) {
      List<Goal> visible = new();

      foreach (string obstacleId in location.ObstacleIds) {
          Obstacle obstacle = _gameWorld.Obstacles[obstacleId];

          foreach (Goal goal in obstacle.Goals) {
              if (goal.PlacementLocationId == location.Id &&
                  CheckRequirements(goal, obstacle)) {
                  visible.Add(goal);
              }
          }
      }

      return visible;
  }

  ---
  Phase 2: Rich Consequences (HIGH VALUE)

  2.1: Consequence Types
  - Replace GoalEffectType with ConsequenceType enum
  - Add all 5 types: Resolution, Bypass, Transform, Modify, Grant
  - Update Goal.cs, GoalDTO.cs

  2.2: State Tracking Enums
  - Add ObstacleState enum (Active/Resolved/Transformed)
  - Add ResolutionMethod enum (7 values)
  - Add RelationshipOutcome enum (5 values)
  - Add to Obstacle entity
  - Update Obstacle.cs, ObstacleDTO.cs

  2.3: Facade Updates
  Update MentalFacade, PhysicalFacade, SocialFacade:
  if (goal.Consequence == ConsequenceType.Resolution) {
      obstacle.State = ObstacleState.Resolved;
      obstacle.ResolutionMethod = DetermineMethod(goal.SystemType);
      location.ObstacleIds.Remove(obstacle.Id);
  }
  else if (goal.Consequence == ConsequenceType.Bypass) {
      obstacle.ResolutionMethod = ResolutionMethod.Stealth;
      obstacle.RelationshipOutcome = RelationshipOutcome.Neutral;
      // Obstacle remains, player just passed it
  }
  else if (goal.Consequence == ConsequenceType.Transform) {
      obstacle.State = ObstacleState.Transformed;
      obstacle.PhysicalDanger = 0;
      obstacle.SocialDifficulty = 0;
      obstacle.MentalComplexity = 0;
      obstacle.Description = goal.TransformDescription;
      obstacle.ResolutionMethod = ResolutionMethod.Diplomacy;
      obstacle.RelationshipOutcome = goal.TransformRelationship;
  }
  else if (goal.Consequence == ConsequenceType.Modify) {
      ApplyPropertyReduction(obstacle, goal.PropertyReduction);
      obstacle.ResolutionMethod = ResolutionMethod.Preparation;
  }
  else if (goal.Consequence == ConsequenceType.Grant) {
      GrantKnowledgeCards(player, goal.KnowledgeGrants);
      // No obstacle change
  }

  2.4: UI Updates
  - Show ObstacleState in ObstacleDisplay
  - Show ResolutionMethod and RelationshipOutcome for resolved obstacles
  - Visual indicators for Active/Resolved/Transformed

  ---
  Phase 3: Knowledge Cards (OPTIONAL, HIGH VALUE)

  3.1: Data Structures
  class KnowledgeCardGrant {
      string CardId;
      string CardText;
      CardEffect Effect;
      string ScopedToObstacleId;  // null = parent obstacle
      SystemType TargetSystemType;
  }

  class PlayerKnowledge {
      Dictionary<string, List<Card>> GrantedCardsByObstacle;
  }

  3.2: Grant System
  When goal with Grant consequence succeeds:
  foreach (KnowledgeCardGrant grant in goal.KnowledgeGrants) {
      Card card = new Card {
          Id = grant.CardId,
          Text = grant.CardText,
          Effect = grant.Effect,
          ...
      };

      string scope = grant.ScopedToObstacleId ?? goal.ParentObstacleId;
      player.Knowledge.GrantedCardsByObstacle[scope].Add(card);
  }

  3.3: Challenge Deck Augmentation
  When creating challenge context:
  // Get base deck
  List<Card> deck = GetChallengeDeck(goal.DeckId);

  // Add knowledge cards for this obstacle
  if (player.Knowledge.GrantedCardsByObstacle.TryGetValue(obstacleId, out List<Card> knowledgeCards)) {
      deck.AddRange(knowledgeCards.Where(c => c.SystemType == goal.SystemType));
  }

  // Player draws from augmented deck

  3.4: AI Narrative Hooks
  When player plays knowledge card:
  Card played = player.PlayCard("millers_reputation");
  // Card text: "You mention you're visiting the miller on business"

  // AI sees card text, generates contextual response:
  NarrativeService.GenerateResponse(npc, played.Text);
  // → "Ah, the miller! Good man. Go on through."

  ---
  Phase 4: Template System (AI GENERATION)

  4.1: Obstacle Archetypes
  Define in JSON/config (not code):
  {
    "archetypes": [
      {
        "id": "authority_gate",
        "description": "Someone with power blocking access",
        "propertyRanges": {
          "physicalDanger": [1, 2],
          "socialDifficulty": [1, 2],
          "mentalComplexity": [0, 1]
        },
        "commonIn": ["gates", "checkpoints", "bureaucracy"]
      },
      {
        "id": "physical_barrier",
        "description": "Environmental obstacle blocking path",
        "propertyRanges": {
          "physicalDanger": [1, 3],
          "socialDifficulty": [0, 0],
          "mentalComplexity": [1, 3]
        },
        "commonIn": ["collapsed_passages", "natural_hazards"]
      }
    ]
  }

  4.2: Goal Templates
  {
    "goalTemplates": [
      {
        "id": "direct_confrontation",
        "systemType": "Physical",
        "consequence": "Resolution",
        "requiresPhysicalDanger": 2,
        "description": "Force through with violence",
        "resolutionMethod": "Violence",
        "relationshipOutcome": "Hostile"
      },
      {
        "id": "negotiate_passage",
        "systemType": "Social",
        "consequence": "Bypass",
        "requiresSocialDifficulty": 1,
        "description": "Pay or persuade for temporary passage",
        "resolutionMethod": "Diplomacy",
        "relationshipOutcome": "Neutral"
      }
    ]
  }

  4.3: AI Generation Process
  When investigation phase activates:
  // 1. AI selects archetype based on context
  ObstacleArchetype archetype = SelectArchetype(phase.Context);

  // 2. AI generates properties within ranges
  int physicalDanger = Random(archetype.PhysicalDangerMin, archetype.PhysicalDangerMax);

  // 3. AI selects 3-6 goal templates that fit
  List<GoalTemplate> templates = SelectGoalTemplates(archetype, 3-6);

  // 4. AI writes narrative for obstacle and each goal
  string obstacleDesc = AI.Generate("Obstacle description", archetype, context);
  foreach (GoalTemplate template in templates) {
      string goalDesc = AI.Generate("Goal description", template, context);
  }

  // 5. Spawn complete obstacle
  Obstacle obstacle = new Obstacle {
      Description = obstacleDesc,
      PhysicalDanger = physicalDanger,
      Goals = generatedGoals
  };

  ---
  🎯 CRITICAL DESIGN DECISIONS RESOLVED

  1. Containment vs Reference: ✅ CONTAINMENT

  Goals live inline inside obstacles. Correct per design.

  2. Property Count: ✅ 3 PROPERTIES

  PhysicalDanger, SocialDifficulty, MentalComplexity only.
  Remove StaminaCost and TimeCost (belong in challenge outcomes).

  3. Knowledge Cards: ✅ OPTIONAL (Phase 3)

  High value for verisimilitude, not blocking for MVP.
  Implement after core architecture stable.

  4. Goal "Manipulation": ✅ JUST PROPERTY REDUCTION

  No literal goal swapping needed. Property reduction unlocks goals with stricter thresholds.

  Example:
  Goal A: maxSocialDifficulty = 2 (visible when SD ≤ 2)
  Goal B: maxSocialDifficulty = 0 (visible when SD ≤ 0)

  Goal C reduces SD 2→0
  → Goal B automatically becomes visible
  → No manual goal manipulation needed

  5. Obstacle Location: ✅ LOCATION-AGNOSTIC

  Obstacles don't have location. They live in GameWorld.Obstacles dictionary.
  Locations/NPCs have ObstacleIds (references to obstacles with goals appearing there).

  ---
  🏆 DESIGN STRENGTHS

  Verisimilitude Through Mechanics

  - Properties are numerical (no string matching)
  - Consequences have semantic meaning (enums, not flags)
  - AI generates narrative from ResolutionMethod/RelationshipOutcome
  - Knowledge cards provide narrative hooks
  - Discovery through exploration feels organic

  Simple Rules, Emergent Complexity

  - Property thresholds gate availability (simple)
  - Multiple goals with different thresholds (emergent choice)
  - Distributed interaction (strategic depth)
  - No formulas, no hidden mechanics (perfect information)

  No String Matching, No Pre-Authoring

  - All gating via numerical properties
  - All narrative from enums + AI generation
  - Templates guide AI, not runtime entities
  - Content generated from mechanics + context

  ---
  ⚡ IMPLEMENTATION COMPLEXITY ASSESSMENT

  Phase 1: Core Architecture

  Complexity: HIGH (major refactor)
  Critical: YES (foundation for everything)
  Time Estimate: 3-4 hours
  Blockers: None, ready to implement

  Changes:
  - Remove 2 properties (simple)
  - Rename LocationId → PlacementLocationId (find/replace)
  - Refactor storage: Location.Obstacles → Location.ObstacleIds (moderate)
  - Update UI lookup pattern (moderate)
  - Update ObstacleGoalFilter (moderate)
  - Update parsers (moderate)

  Phase 2: Rich Consequences

  Complexity: MEDIUM
  Critical: YES (semantic meaning for AI)
  Time Estimate: 2-3 hours
  Blockers: Requires Phase 1 complete

  Changes:
  - Replace enum (simple)
  - Add 3 new enums (simple)
  - Update facades with consequence logic (moderate)
  - Update UI display (simple)

  Phase 3: Knowledge Cards

  Complexity: MEDIUM-HIGH
  Critical: NO (optional, high value)
  Time Estimate: 3-4 hours
  Blockers: Requires Phase 1 complete

  Changes:
  - Data structures (simple)
  - Grant system (moderate)
  - Challenge deck augmentation (moderate)
  - AI narrative hooks (complex)

  Phase 4: Templates

  Complexity: MEDIUM
  Critical: NO (AI generation only)
  Time Estimate: 2-3 hours
  Blockers: None (independent system)

  Changes:
  - Define archetypes (simple)
  - Define goal templates (simple)
  - AI generation process (moderate)

  ---
  🎓 FINAL RECOMMENDATION

  Implement in Order:

  Phase 1: Core Architecture (MUST DO)
  - 3 properties
  - PlacementLocationId/NpcId
  - Location-agnostic obstacles
  - Distributed interaction pattern
  - New UI lookup

  Phase 2: Rich Consequences (MUST DO)
  - 5 consequence types
  - State tracking enums
  - Semantic meaning for AI

  Phase 3: Knowledge Cards (SHOULD DO)
  - Verisimilitude through card content
  - AI narrative hooks
  - Tactical gameplay depth

  Phase 4: Templates (CAN DEFER)
  - AI generation guidance
  - Not needed if hand-authoring obstacles

  Why This Order:

  1. Phase 1 is foundation - everything else builds on this
  2. Phase 2 adds semantic richness - critical for verisimilitude
  3. Phase 3 is polish - high value but not blocking
  4. Phase 4 is generation - only needed when AI creates content

  Estimated Total Time:

  - Phase 1: 3-4 hours
  - Phase 2: 2-3 hours
  - Phase 3: 3-4 hours (optional)
  - Phase 4: 2-3 hours (optional)

  Core system (Phases 1+2): 5-7 hours
  Full system (all phases): 10-14 hours

  ---
  📝 PLAN SUMMARY

  I will implement Phase 1 (Core Architecture) and Phase 2 (Rich Consequences) to establish the foundation:

  1. Reduce to 3 properties (remove StaminaCost, TimeCost)
  2. Rename Goal.LocationId → Goal.PlacementLocationId (semantic clarity)
  3. Remove Obstacle.LocationId (obstacles are location-agnostic)
  4. Refactor Location.Obstacles → Location.ObstacleIds (references, not containment)
  5. Add GameWorld.Obstacles dictionary (single source of truth)
  6. Update UI lookup pattern (distributed interaction)
  7. Replace GoalEffectType with ConsequenceType (5 types)
  8. Add ObstacleState, ResolutionMethod, RelationshipOutcome enums
  9. Update facades to set state based on consequences
  10. Update UI to show rich state

  Phase 3 (Knowledge Cards) will follow after core is stable and validated.

  This achieves: Full design foundation, verisimilitude through mechanics, no string matching, emergent complexity through simple rules.