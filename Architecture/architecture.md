= Game Logic System Architecture
:toc:

== 1. Introduction and Goals

=== 1.1 Requirements Overview
The Game Logic System is the core engine for a medieval-themed game that emphasizes systemic interactions and emergent gameplay. It manages:

- Game state and progression
- Location-based actions and interactions
- Character relationships and knowledge
- Resource and energy management
- Time and schedule systems

=== 1.2 Quality Goals

[%autowidth]
|===
|Quality Goal |Motivation |Driver

|Maintainability
|Clean separation between game logic and external systems
|Development Team

|Extensibility
|Easy addition of new actions, locations, and characters
|Development Team

|Consistency
|Predictable and deterministic game mechanics
|Game Design

|Performance
|Efficient state management and calculations
|Technical Lead

|Testability
|Isolated core logic for comprehensive testing
|QA Team
|===

=== 1.3 Stakeholders

[%autowidth]
|===
|Role |Expectations

|Development Team
|Clean architecture with clear boundaries between systems

|Game Designers
|Flexible system that supports complex gameplay mechanics

|Content Writers
|Clear interfaces for integrating narrative content

|QA Team
|Testable and predictable behavior

|Players
|Consistent and engaging gameplay experience
|===

== 2. Architecture Constraints

=== 2.1 Technical Constraints
- Strongly-typed C# implementation
- No direct UI dependencies
- Pure game logic separated from content generation
- Deterministic outcomes for all actions

=== 2.2 Organizational Constraints
- Must support external content creation pipeline
- Must facilitate automated testing
- Must enable clear separation between content and mechanics

== 3. System Scope and Context

=== 3.1 Business Context

[plantuml,"business-context",svg]
----
@startuml
actor "Game UI" as UI
actor "Content System" as CS
component [Game Logic System] as GLS
UI --> GLS : "Executes actions"
CS --> GLS : "Provides content"
GLS --> UI : "Provides state"
@enduml
----

[%autowidth]
|===
|Element |Description

|Game Logic System
|Core system managing game rules and state

|Game UI
|External system handling player interaction

|Content System
|External system providing location/character content
|===

=== 3.2 Technical Context

[plantuml,"technical-context",svg]
----
@startuml
package "Game Logic System" {
    [GameState]
    [GameContentProvider]
    [ActionManager]
    [LocationSystem]
    [CharacterSystem]
    [GameSetup]
}

database "Content Store" as CS
[UI System] as UI

UI --> ActionManager
UI --> GameState
CS --> GameContentProvider
GameSetup --> GameState
GameContentProvider --> LocationSystem
GameContentProvider --> CharacterSystem
ActionManager --> GameState
ActionManager --> LocationSystem
ActionManager --> CharacterSystem
@enduml
----

== 4. Solution Strategy

The system employs several key strategic decisions:

1. *State Isolation*: GameState class contains all mutable game state with no external dependencies

2. *Clean Actions*: ActionManager handles all state modifications through well-defined actions

3. *Content Independence*: Location and character systems are populated at startup but operate independently

4. *Deterministic Design*: All game mechanics produce predictable outcomes based on current state

== 5. Building Block View

=== 5.1 Level 1
[plantuml,"buildingBlock1",svg]
----
@startuml
package "Game Logic System" {
    component [GameState] as GS
    component [ActionManager] as AM
    component [LocationSystem] as LS
    component [CharacterSystem] as CS
    component [GameSetup] as GST
}

GS -- AM
AM -- LS
AM -- CS
GST -- LS
GST -- CS
GST -- GS

note right of GS
  Contains all mutable game state
end note

note right of AM
  Handles all state modifications
end note

note right of LS
  Manages location definitions
end note

note right of CS
  Manages character definitions
end note

note right of GST
  Initializes game systems
end note
@enduml
----

=== 5.2 Level 2 - Core Components
[plantuml,"buildingBlock2",svg]
----
@startuml
package "GameState" {
    [PlayerState]
    [LocationStates]
    [CharacterStates]
    [TimeState]
    [KnowledgeState]
}

package "ActionManager" {
    [ActionValidator]
    [ActionExecutor]
    [OutcomeGenerator]
}

package "LocationSystem" {
    [LocationDefinitions]
    [SpotDefinitions]
    [ActivityTypes]
}

package "CharacterSystem" {
    [CharacterDefinitions]
    [ScheduleDefinitions]
    [RelationshipTypes]
}

[ActionExecutor] --> GameState
[LocationDefinitions] --> [ActionValidator]
[CharacterDefinitions] --> [ActionValidator]
@enduml
----

=== 5.3 Level 3 - UI Integration Components
[plantuml,"buildingBlock3",svg]
----
package "UI Integration" {
    component [UserActionOption] 
    component [UserLocationTravelOption]
    component [UserLocationSpotOption]
    component [ActionResultMessages]
}
----

The UI integration layer provides strongly-typed data structures for communicating game state to the UI while maintaining system independence.

== 6. Runtime View

=== 6.1 Action Execution Flow

[plantuml,"sequence1",svg]
----
@startuml
participant "UI" as UI
participant "ActionManager" as AM
participant "LocationSystem" as LS
participant "CharacterSystem" as CS
participant "GameState" as GS

UI -> AM: ExecuteAction(actionData)
AM -> LS: GetLocationInfo()
LS --> AM: locationInfo
AM -> CS: GetCharacterInfo()
CS --> AM: characterInfo
AM -> AM: ValidateAction()
AM -> GS: ModifyState()
GS --> AM: newState
AM --> UI: actionResult
@enduml
----

=== 6.2 Knowledge Acquisition Flow

[plantuml,"sequence2",svg]
----
@startuml
participant "UI" as UI
participant "ActionManager" as AM
participant "LocationSystem" as LS
participant "GameState" as GS

UI -> AM: ExecuteInvestigateAction()
AM -> LS: GetLocationKnowledgeOptions()
LS --> AM: knowledgeOptions
AM -> GS: CheckExistingKnowledge()
GS --> AM: currentKnowledge
AM -> AM: DetermineOutcome()
AM -> GS: UpdateKnowledge()
GS --> AM: newState
AM --> UI: investigationResult
@enduml
----

=== 6.3 State Change Processing
The system uses a two-phase state change approach:

1. Pending Changes Phase
- Changes are collected in ActionResultMessages
- Multiple outcomes can be queued
- No state is modified

2. Application Phase  
- Changes are applied atomically
- Results are validated
- Changes generate new messages
- Process repeats until no new changes

This ensures consistent state transitions and proper change propagation.

=== 6.4 Action Execution Flow 
[plantuml,"sequence3",svg]
----
@startuml
participant "UI" as UI
participant "ActionManager" as AM
participant "Requirements" as REQ
participant "GameState" as GS

UI -> AM: ExecuteAction
AM -> REQ: ValidateRequirements
REQ -> GS: CheckResources
GS --> REQ: ValidationResult
REQ --> AM: Requirements Met/Not Met
AM -> GS: ApplyChanges
GS --> AM: New State
AM --> UI: ActionResult
@enduml
----

== 7. Cross-cutting Concepts

=== 7.1 Domain Models

Core domain models include:

* Actions (INVESTIGATE, LABOR, DISCUSS, etc.)
* Knowledge Flags (binary states that unlock capabilities)
* Energy Types (Physical, Social, Focus)
* Time Windows (Morning, Afternoon, Evening, Night)

=== 7.2 State Management

* GameState is the single source of truth
* All state modifications go through ActionManager
* State changes are atomic and deterministic
* External systems can only read state

=== 7.3 Exception Handling

* Validation errors returned as Results
* No exceptions for expected failure cases
* System exceptions only for true errors

=== 7.4 Content Creation Pattern
The system employs a comprehensive builder pattern for game content:

- LocationBuilder: Constructs location definitions
- CharacterBuilder: Creates character definitions  
- BasicActionDefinitionBuilder: Defines available actions
- NarrativeBuilder: Constructs narrative sequences

This provides:
- Type-safe content creation
- Consistent validation
- Flexible composition

== 8. Architectural Decisions

=== 8.1 Strict State Isolation
GameState has no dependencies on other systems to ensure clean separation and testability

=== 8.2 Content Independence
Location and character systems are populated at startup but operate independently of content generation

=== 8.3 Deterministic Design
All game mechanics produce predictable outcomes based on current state for testing and debugging

=== 8.4 Energy Management
The game features three distinct energy types:

- Physical Energy: Manual labor and physical tasks
- Focus Energy: Investigation and observation 
- Social Energy: Character interactions

Each energy type:
- Has a maximum capacity
- Regenerates through rest
- Is consumed by specific actions
- Can receive time window bonuses

=== 8.5 Game Initialization
The system uses a GameSetup class to create consistent initial states:

- Starting resources (health, energy, coins)
- Initial location setup
- Basic action availability
- Starting inventory capacity
- Default skill levels

=== 8.6 Resource System
Resources are managed through:

- Fixed inventory slots
- Resource types enumeration
- Add/remove validation
- Capacity tracking
- Resource transformation rules

== 9. Quality Requirements

[%autowidth]
|===
|Quality Goal |Scenario

|Maintainability
|New action types can be added without modifying existing code

|Extensibility
|New location/character types can be integrated without core changes

|Consistency
|Same inputs always produce same outputs

|Testability
|All game logic can be tested without UI or content dependencies
|===

== 10. Risks and Technical Debt

=== 10.1 Action Validation
Ensuring all action combinations remain valid as the system grows requires careful design

== 11. Glossary

[%autowidth]
|===
|Term |Definition

|GameState
|Central class holding all mutable game state

|ActionManager
|Coordinates all state modifications through defined actions

|LocationSystem
|Manages definitions and rules for game locations

|CharacterSystem
|Manages definitions and rules for game characters

|Knowledge Flag
|Binary state indicating specific player knowledge

|Energy Type
|Resource used for performing actions (Physical, Social, Focus)
|===