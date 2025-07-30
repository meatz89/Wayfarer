# Wayfarer Action Flow Diagrams

## Location Action Complete Flow

```mermaid
sequenceDiagram
    participant UI as LocationActions.razor
    participant GF as GameFacade
    participant LAS as LocationActionsUIService
    participant CDS as CommandDiscoveryService
    participant NM as NarrativeManager
    participant CE as CommandExecutor
    participant CMD as Command
    participant GW as GameWorld
    participant MSG as MessageSystem

    UI->>GF: GetLocationActions()
    GF->>LAS: GetLocationActionsViewModel()
    LAS->>CDS: DiscoverCommands(gameWorld)
    
    Note over CDS: Discovery Phase
    CDS->>CDS: DiscoverNPCCommands()
    CDS->>CDS: DiscoverLocationCommands()
    CDS->>CDS: DiscoverLetterCommands()
    
    alt Has Narrative Manager
        CDS->>NM: FilterCommands(allCommands)
        NM-->>CDS: Filtered commands
    end
    
    CDS-->>LAS: CommandDiscoveryResult
    LAS->>LAS: ConvertCommands()
    LAS-->>GF: LocationActionsViewModel
    GF-->>UI: ViewModel with actions
    
    Note over UI: User clicks action button
    UI->>GF: ExecuteLocationActionAsync(commandId)
    GF->>LAS: ExecuteActionAsync(commandId)
    LAS->>CDS: DiscoverCommands(gameWorld)
    CDS-->>LAS: Find command by ID
    
    LAS->>CE: ExecuteAsync(command)
    CE->>CMD: CanExecute(gameWorld)
    CMD-->>CE: ValidationResult
    
    alt Command Valid
        CE->>CMD: Execute(gameWorld)
        CMD->>GW: Modify state
        CMD->>MSG: AddMessage()
        CMD-->>CE: Success
        CE-->>LAS: CommandResult
        LAS-->>GF: Success
        GF-->>UI: true
    else Command Invalid
        CE->>MSG: AddMessage(error)
        CE-->>LAS: CommandResult
        LAS-->>GF: Failure
        GF-->>UI: false
    end
    
    UI->>UI: StateHasChanged()
    UI->>UI: OnActionExecuted callback
```

## Letter Queue Action Flow

```mermaid
sequenceDiagram
    participant UI as LetterQueueDisplay.razor
    participant GF as GameFacade
    participant LQS as LetterQueueUIService
    participant LQM as LetterQueueManager
    participant CTM as ConnectionTokenManager
    participant GW as GameWorld

    Note over UI: Skip Letter Action
    UI->>GF: SkipLetterAsync(position)
    GF->>LQS: SkipLetterAsync(position)
    
    LQS->>LQM: GetLetterAtPosition(position)
    LQM-->>LQS: Letter
    
    LQS->>CTM: GetTokensForNPC(npcId)
    CTM-->>LQS: Token count
    
    LQS->>LQS: Calculate skip cost
    Note over LQS: Base cost × position multiplier
    
    alt Has enough tokens
        LQS->>CTM: SpendTokens(npcId, cost)
        CTM->>GW: Update token state
        LQS->>LQM: MoveToPosition(letter, 1)
        LQM->>GW: Update queue state
        LQS-->>GF: Success
    else Insufficient tokens
        LQS-->>GF: Failure
    end
    
    GF-->>UI: Result
    UI->>UI: RefreshViewModel()
    UI->>UI: StateHasChanged()
```

## Conversation Flow with State Management

```mermaid
sequenceDiagram
    participant UI as ConversationView.razor
    participant GF as GameFacade
    participant CSM as ConversationStateManager
    participant CF as ConversationFactory
    participant CM as ConversationManager
    participant GW as GameWorld

    Note over UI: Start Conversation
    UI->>GF: StartConversationAsync(npcId)
    GF->>CSM: StartConversationAsync(npcId)
    
    CSM->>CF: BuildConversationForNPC(npcId)
    CF->>CF: Load conversation JSON
    CF->>CF: Build conversation tree
    CF-->>CSM: ConversationManager
    
    CSM->>CSM: SetPendingConversation()
    CSM->>GW: Set ConversationPending = true
    CSM->>CM: StartConversation()
    CM-->>CSM: Initial beat
    
    CSM->>CSM: CreateViewModel()
    CSM-->>GF: ConversationViewModel
    GF-->>UI: ViewModel
    
    Note over UI: Player makes choice
    UI->>GF: ContinueConversationAsync(choiceId)
    GF->>CSM: ContinueConversationAsync(choiceId)
    
    CSM->>CM: ProcessChoice(choiceId)
    CM->>CM: Execute outcome scripts
    CM->>GW: Apply state changes
    CM-->>CSM: Next beat or complete
    
    alt Conversation continues
        CSM->>CSM: CreateViewModel()
        CSM-->>GF: ConversationViewModel
    else Conversation complete
        CSM->>CSM: ClearPendingConversation()
        CSM->>GW: Set ConversationPending = false
        CSM-->>GF: Final ViewModel
    end
    
    GF-->>UI: Updated ViewModel
```

## Market Transaction Flow

```mermaid
sequenceDiagram
    participant UI as Market.razor
    participant GF as GameFacade
    participant MUS as MarketUIService
    participant MM as MarketManager
    participant INV as Inventory
    participant GW as GameWorld

    Note over UI: Buy Item
    UI->>GF: BuyItemAsync(itemId, traderId)
    GF->>MUS: BuyItemAsync(itemId, traderId)
    
    MUS->>MM: GetTraderById(traderId)
    MM-->>MUS: Trader
    
    MUS->>MUS: Validate purchase
    Note over MUS: Check coins, inventory space
    
    alt Can purchase
        MUS->>GW: player.Coins -= price
        MUS->>INV: AddItem(itemId)
        MUS->>MM: RemoveFromTrader(itemId)
        MUS-->>GF: Success
    else Cannot purchase
        MUS-->>GF: Failure with reason
    end
    
    GF-->>UI: Result
    UI->>GF: GetMarket()
    GF->>MUS: GetMarketViewModel()
    MUS-->>GF: Updated ViewModel
    GF-->>UI: New market state
    UI->>UI: StateHasChanged()
```

## Travel Action Flow with Route Validation

```mermaid
sequenceDiagram
    participant UI as TravelSelection.razor
    participant GF as GameFacade
    participant TUS as TravelUIService
    participant TM as TravelManager
    participant RR as RouteRepository
    participant TC as TravelCommand
    participant GW as GameWorld

    Note over UI: Get available destinations
    UI->>GF: GetTravelDestinationsWithRoutes()
    GF->>GF: GetTravelContext()
    Note over GF: Calculate weight, equipment
    
    GF->>RR: GetRoutesFromLocation(currentId)
    RR-->>GF: All routes
    
    loop For each destination
        GF->>GF: CalculateRouteAvailability()
        Note over GF: Check requirements, blocks
    end
    
    GF-->>UI: TravelDestinationViewModels
    
    Note over UI: User selects route
    UI->>GF: TravelToDestinationAsync(destId, routeId)
    GF->>RR: GetRouteById(routeId)
    RR-->>GF: Route
    
    GF->>TC: new TravelCommand(route)
    GF->>CE: ExecuteAsync(travelCommand)
    
    TC->>TC: CanExecute()
    Note over TC: Validate stamina, time, equipment
    
    alt Can travel
        TC->>TM: ExecuteTravel(route)
        TM->>GW: Update location
        TM->>GW: Spend time/stamina
        TM->>GW: Process travel events
        TC-->>CE: Success
    else Cannot travel
        TC-->>CE: Failure with reason
    end
    
    CE-->>GF: Result
    GF-->>UI: Success/Failure
```

## Command Discovery Filtering Flow

```mermaid
flowchart TD
    Start[CommandDiscoveryService.DiscoverCommands]
    
    Start --> CheckLoc{Player has location?}
    CheckLoc -->|No| ReturnEmpty[Return empty result]
    CheckLoc -->|Yes| GetNPCs[Get NPCs at location/time]
    
    GetNPCs --> FilterNPCs{Has NarrativeManager?}
    FilterNPCs -->|Yes| ApplyVisibility[Filter by IsNPCVisible]
    FilterNPCs -->|No| KeepAll[Keep all NPCs]
    
    ApplyVisibility --> DiscoverNPC[Discover NPC Commands]
    KeepAll --> DiscoverNPC
    
    DiscoverNPC --> DiscoverLoc[Discover Location Commands]
    DiscoverLoc --> DiscoverLetter[Discover Letter Commands]
    
    DiscoverLetter --> CheckNarrative{Has NarrativeManager?}
    CheckNarrative -->|Yes| FilterCommands[NarrativeManager.FilterCommands]
    CheckNarrative -->|No| ReturnAll[Return all commands]
    
    FilterCommands --> BuildResult[Build filtered result]
    ReturnAll --> ReturnResult[Return result]
    BuildResult --> ReturnResult
```

## Error Propagation Flow

```mermaid
flowchart LR
    UI[UI Component]
    GF[GameFacade]
    SVC[UIService]
    CE[CommandExecutor]
    CMD[Command]
    MSG[MessageSystem]
    
    UI -->|Action| GF
    GF -->|Delegate| SVC
    SVC -->|Execute| CE
    CE -->|Validate| CMD
    
    CMD -->|Error| CE
    CE -->|Add Message| MSG
    CE -->|Result| SVC
    SVC -->|bool| GF
    GF -->|bool| UI
    
    MSG -.->|System Messages| UI
    
    style MSG fill:#f9f,stroke:#333,stroke-width:2px
    style UI fill:#bbf,stroke:#333,stroke-width:2px
```

## State Change Notification Gap

```mermaid
flowchart TD
    subgraph Current Flow
        Action[User Action]
        Backend[Backend State Change]
        Poll[UI Polls State]
        Update[UI Updates]
        
        Action --> Backend
        Backend -.->|No notification| Poll
        Poll --> Update
    end
    
    subgraph Recommended Flow
        Action2[User Action]
        Backend2[Backend State Change]
        Event[State Change Event]
        Update2[UI Updates Reactively]
        
        Action2 --> Backend2
        Backend2 --> Event
        Event --> Update2
    end
    
    style Event fill:#9f9,stroke:#333,stroke-width:2px
```

These diagrams illustrate the complete action flows through the Wayfarer system, highlighting both the successful paths and the gaps identified in the audit.