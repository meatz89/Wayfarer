# Claude Code Wayfarer Economic Simulation POC Implementation Prompt

## Phase 1: Project Understanding & Analysis

### Step 1.1: Read and Understand Economic Simulation POC Design
```
Please begin by thoroughly reading and understanding my Wayfarer economic simulation POC design. Focus specifically on:

1. **Core Economic Systems**: 
   - Time Block System (Dawn/Morning/Afternoon/Evening/Night as scarce resource)
   - Stamina System (daily 0-6 point pool limiting actions)
   - Route/Transport System (varying costs: money/time/stamina)
   - Trading System (location-specific prices, limited inventory slots)
   - Contract System (time-bound deliveries with consequences)

2. **Strategic Design Philosophy**:
   - "Everything Costs Something Else" - optimizing one resource sacrifices another
   - Cascading Decisions - each choice ripples forward
   - Simple Systems with Complex Interactions
   - Clear optimization moments and "Aha!" discoveries

3. **Implementation Phases**:
   - Phase 1: Multiple Routes (strategic travel decisions)
   - Phase 2: Trading System (price differentials and arbitrage)
   - Phase 3: Discovery System (exploration through player actions)
   - Phase 4: Contract System (time pressure and goals)
   - Phase 5: Enhanced Inventory (visual inventory management)

4. **POC Success Criteria**: A compelling economic game loop where players make meaningful choices about time, stamina, money, and travel to optimize trading profits and fulfill contracts.

Summarize your understanding of this economic simulation POC and its core strategic elements.
```

### Step 1.2: Analyze Existing Codebase for Economic Systems
```
Now analyze my existing C# codebase to understand:

1. **Coding Style & Preferences** (CRITICAL - MUST FOLLOW):
   - No use of 'var' keyword (strongly typed everywhere)
   - No lambda expressions
   - No dictionaries or hashsets (use Lists with explicit searching)
   - No 'internal' access modifiers (prefer public)
   - No 'required' keyword usage
   - Enum names must be plural (e.g., TimeBlocks, not TimeBlock)

2. **Existing Economic/Resource Systems**:
   - What time/scheduling systems exist
   - Current resource management (money, stamina, inventory)
   - Location and travel systems
   - Any trading or contract mechanisms
   - Data structures for items, routes, prices

3. **Architecture Assessment**:
   - Class organization for economic simulation
   - How resources are currently tracked
   - Event/update patterns for time progression
   - UI patterns for resource display

Create a detailed inventory of what economic systems already exist vs. what needs to be built for the POC.
```

## Phase 2: Economic POC Implementation Planning

### Step 2.1: Gap Analysis & Implementation Roadmap
```
Based on your analysis, create a detailed implementation plan focusing ONLY on the economic simulation POC:

1. **Core Systems Implementation Order**:
   - Time Block System (foundation for all other systems)
   - Basic Location/Route System (multiple travel options)
   - Stamina System (action limitation and resource tension)
   - Trading System (buy/sell with price differentials)
   - Contract System (timed delivery obligations)

2. **Phase 1 - Multiple Routes System**:
   - Location definitions with different travel costs
   - Route options (fast/expensive vs slow/cheap)
   - Time and stamina cost calculations
   - UI for route selection showing costs

3. **Phase 2 - Trading System**:
   - Item definitions with location-specific prices
   - Limited inventory slots (force tough decisions)
   - Buy/sell mechanics with profit calculation
   - Price differential display

4. **Phase 3 - Discovery System**:
   - New routes/locations unlocked through actions
   - Hidden trading opportunities
   - Exploration rewards

5. **Phase 4 - Contract System**:
   - Time-bound delivery contracts
   - Penalty/reward calculations
   - Contract selection and management

6. **Phase 5 - Enhanced Inventory**:
   - Visual inventory management
   - Item sorting and organization
   - Quick trade calculations

The POC should result in a playable economic game where players:
- Wake up each day with limited time blocks and stamina
- Choose between different routes to travel (balancing cost/time/stamina)
- Buy low and sell high across different locations
- Accept contracts with time pressure
- Make strategic decisions about resource allocation

Break this into 10-15 incremental steps that build toward this economic gameplay loop.
```

## Phase 3: Economic Systems Implementation

### Step 3.1: Incremental Implementation Execution
```
Now implement the economic simulation POC step by step. For each step:

1. **Implementation Standards**:
   - Follow my coding style preferences STRICTLY (no var, no lambda, etc.)
   - Focus on economic mechanics, not narrative systems
   - Create meaningful resource trade-offs
   - Ensure each system integrates with others

2. **Implementation Priority**:
   - Start with time/stamina systems (core constraint mechanics)
   - Add location/route systems (basic travel choices)
   - Implement trading mechanics (core gameplay loop)
   - Add contracts (time pressure and goals)
   - Polish inventory management

3. **Quality Verification Per Step**:
   - Code compiles without errors
   - Economic systems create interesting decisions
   - Resource costs create meaningful trade-offs
   - UI clearly shows resource states and costs

4. **Economic Balance Testing**:
   - Ensure no single strategy dominates
   - Verify stamina/time create real constraints
   - Check that price differentials reward optimization
   - Confirm contracts create appropriate pressure

Continue implementing until you have a complete economic simulation where every decision involves meaningful resource trade-offs.
```

## Phase 4: Economic POC Validation & Polish

### Step 4.1: Economic Gameplay Validation
```
Once implementation is complete, validate the economic simulation:

1. **Core Economic Loop Testing**:
   - Daily time/stamina cycles create strategic pressure
   - Route choices involve meaningful cost/benefit analysis
   - Trading opportunities reward planning and optimization
   - Contracts add time pressure without overwhelming complexity

2. **Strategic Depth Verification**:
   - Multiple viable strategies exist
   - Players face "everything costs something else" decisions
   - Early decisions cascade into later consequences
   - Optimization moments feel rewarding

3. **Technical Quality**:
   - All code follows specified style preferences
   - Economic calculations are accurate and transparent
   - UI clearly communicates resource states and costs
   - Build succeeds with `dotnet build` and runs with `dotnet run`

4. **Gameplay Flow**:
   - Tutorial/onboarding explains core economic concepts
   - Player can complete full economic cycles (buy → travel → sell → repeat)
   - Contract system adds meaningful goals
   - Resource management feels engaging, not tedious

Document any areas for future economic expansion or balance adjustments.
```

## Success Criteria for Economic Simulation POC

The POC is complete when:
- ✅ Time blocks create daily strategic pressure
- ✅ Stamina system limits actions meaningfully
- ✅ Multiple route options with different cost profiles
- ✅ Trading system with location-specific prices and arbitrage opportunities
- ✅ Limited inventory creates item prioritization decisions
- ✅ Contract system adds time-bound objectives
- ✅ Resource costs create "everything costs something else" tension
- ✅ Players experience clear optimization opportunities
- ✅ Code builds and runs demonstrating economic gameplay loop
- ✅ All code follows specified style preferences (no var, no lambda, etc.)

## Execution Instructions

**Phase 1**: Start with understanding the economic simulation design and analyzing existing economic systems in the codebase

**Phase 2**: Create detailed implementation plan focused on economic mechanics and resource management

**Phase 3**: Implement economic systems incrementally, focusing on creating meaningful strategic decisions

**Phase 4**: Validate that the economic simulation creates compelling gameplay through resource trade-offs

Focus entirely on the economic simulation - ignore narrative encounters, story systems, or complex AI integration for this POC.