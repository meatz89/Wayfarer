# USER STORIES - WAYFARER GAME DESIGN REQUIREMENTS

This document defines the user-facing feature requirements and game design principles for Wayfarer, focusing on the distinction between game design vs application design.

## CORE GAME DESIGN PRINCIPLES

### **Games vs Apps: Fundamental Difference**

**Games create interactive optimization puzzles for the player to solve, not automated systems that solve everything for them.**

✅ **GAME DESIGN PRINCIPLES**
- **GAMEPLAY IS IN THE PLAYER'S HEAD** - Fun comes from systems interacting in clever ways that create optimization challenges
- **DISCOVERY IS GAMEPLAY** - Players must explore, experiment, and learn to find profitable trades, efficient routes, optimal strategies
- **EMERGENT COMPLEXITY** - Simple systems (pricing, time blocks, stamina) that interact to create deep strategic decisions
- **MEANINGFUL CHOICES** - Every decision should involve sacrificing something valuable (time vs money vs stamina)
- **PLAYER AGENCY** - Players discover patterns, build mental models, develop personal strategies through exploration

❌ **APP DESIGN ANTI-PATTERNS (NEVER IMPLEMENT)**
- **NO AUTOMATED CONVENIENCES** - Don't create `GetProfitableItems()` or `GetBestRoute()` methods that solve the puzzle for the player
- **NO GAMEPLAY SHORTCUTS** - No "Trading Opportunities" UI panels that tell players exactly what to do
- **NO OPTIMIZATION AUTOMATION** - Never show "Buy herbs at town_square (4 coins) → Sell at dusty_flagon (5 coins) = 1 profit"

## USER STORIES BY CATEGORY

### **Strategic Trading System**

**As a merchant player, I want to discover profitable trade routes through exploration and experimentation, so that I can develop my own trading strategies.**

**Implementation Requirements:**
- Markets show current item prices without automation suggestions
- No "profitable opportunities" indicators or automated trade route suggestions
- Price differences discovered through manual exploration and memory
- Market information must be gathered by visiting locations personally

**Success Criteria:**
- Players build mental models of market patterns
- Profitable trades discovered through player exploration, not system recommendations
- Strategic decisions involve resource trade-offs (time vs profit vs risk)

**Anti-Pattern Example:** ❌ Trading Opportunities panel showing "Buy herbs at market → Sell at tavern = 50% profit"
**Correct Example:** ✅ Player visits market, notes herb prices, visits tavern, discovers price difference, develops trading strategy

---

### **Logical Equipment System**

**As a player, I want equipment to enable or block specific activities based on logical real-world relationships, so that I can understand and plan for requirements intuitively.**

**Implementation Requirements:**
- Equipment categories determine capability access, not stat bonuses
- Clear logical relationships between equipment types and terrain requirements
- Visual indicators show equipment categories and their effects on route access
- No arbitrary level requirements or hidden modifiers

**Success Criteria:**
- Players understand equipment value through logical necessity
- Route planning involves equipment strategy decisions
- All equipment-terrain relationships are discoverable and logical

**Anti-Pattern Example:** ❌ "Requires level 5 to use mountain routes"
**Correct Example:** ✅ "Mountain terrain requires climbing equipment for safe passage"

---

### **Time Pressure Contract System**

**As a player, I want contracts with meaningful deadlines that create strategic pressure around time management, so that I must balance urgency vs thoroughness.**

**Implementation Requirements:**
- Contracts have clear deadlines that affect payment/reputation
- Time blocks consumed by travel, work, and contract completion
- Late delivery consequences affect future opportunities, not arbitrary penalties
- Players can still attempt late deliveries but face reputation consequences

**Success Criteria:**
- Strategic decisions about which contracts to prioritize
- Time pressure creates meaningful trade-offs between speed and profit
- Reputation system affects contract availability naturally

**Anti-Pattern Example:** ❌ "-50% payment penalty for late delivery"
**Correct Example:** ✅ "Late delivery reduces reputation with client, affecting future contract offers"

---

### **Weather-Terrain Interaction System**

**As a player, I want weather and terrain to create logical constraints that I can understand and plan around, so that I can make informed decisions about travel timing and equipment.**

**Implementation Requirements:**
- Weather affects terrain accessibility based on logical relationships
- Equipment requirements change based on weather-terrain combinations
- All interactions visible and understandable in UI
- No arbitrary efficiency modifiers or hidden calculations

**Success Criteria:**
- Players learn weather patterns through experience
- Route planning considers weather forecasts and equipment needs
- Strategic decisions about timing vs equipment investment

**Anti-Pattern Example:** ❌ "Rain reduces travel efficiency by 20%"
**Correct Example:** ✅ "Rain blocks exposed mountain routes unless you have weather protection equipment"

---

### **Social Access System**

**As a player, I want social interactions to depend on logical factors like appearance and reputation, so that I can understand and plan for social requirements.**

**Implementation Requirements:**
- Social access based on visible equipment categories (clothing, status symbols)
- NPC interactions depend on logical social class expectations
- Relationship building through consistent positive interactions
- All social requirements discoverable through logical trial and error

**Success Criteria:**
- Players understand social requirements through clear feedback
- Investment in appropriate equipment/appearance opens new opportunities
- Social strategy becomes part of economic planning

**Anti-Pattern Example:** ❌ "Requires reputation level 10 to enter noble district"
**Correct Example:** ✅ "Noble district requires appropriate attire to pass the doorkeeper"

---

### **Information Discovery System**

**As a player, I want to gather useful information through exploration and NPC interaction, so that I can make better strategic decisions without automated assistance.**

**Implementation Requirements:**
- NPCs provide information based on their profession and relationship
- Market conditions, route hazards, and opportunities discovered through conversation
- No automated information gathering or market analysis tools
- Information quality depends on NPC trust and expertise

**Success Criteria:**
- Players build information networks through NPC relationships
- Strategic value in talking to different types of NPCs
- Information gathering becomes part of business strategy

**Anti-Pattern Example:** ❌ "Market Analysis" screen showing all profitable trades
**Correct Example:** ✅ "Trader NPCs share price information when you build relationships with them"

## DESIGN VALIDATION CHECKLIST

For every feature implementation, verify:

1. ✅ **Player Agency**: Does this feature require player decision-making and strategy?
2. ✅ **Logical Basis**: Are all constraints based on understandable real-world logic?
3. ✅ **Discovery Gameplay**: Will players learn and improve through experimentation?
4. ✅ **No Automation**: Does this avoid solving puzzles for the player?
5. ✅ **Meaningful Choices**: Do decisions involve sacrificing something valuable?
6. ✅ **Emergent Complexity**: Do simple rules create interesting strategic depth?

## PLAYER EXPERIENCE GOALS

### **Short-term (Single Session)**
- Player discovers 2-3 new pieces of useful information through exploration
- Makes 1-2 strategic decisions involving resource trade-offs
- Learns something new about how game systems interact

### **Medium-term (Several Sessions)**
- Develops personal trading routes and strategies
- Builds relationship networks with useful NPCs
- Understands equipment and social requirements for different activities

### **Long-term (Campaign)**
- Becomes expert at optimizing time/resource management
- Discovers advanced strategies through system mastery
- Develops unique personal approach to economic success

## ANTI-PATTERN EXAMPLES TO AVOID

### ❌ **Automated Optimization**
- "Best Route" calculators
- "Profitable Opportunities" panels
- Automatic market analysis tools
- Route efficiency comparisons

### ❌ **Arbitrary Restrictions**
- Level requirements for activities
- Stat thresholds for equipment
- Hidden modifiers and calculations
- Percentage-based bonuses/penalties

### ❌ **Gameplay Shortcuts**
- Quest markers showing optimal paths
- Automatic inventory management
- Simplified decision-making interfaces
- Progress bars for non-skill activities

## SUCCESS METRICS

### **Engagement Indicators**
- Players discuss strategies and discoveries
- Multiple viable approaches to challenges
- Player-generated optimization content
- Questions about system mechanics

### **Learning Indicators**
- Players experiment with different equipment combinations
- Strategic planning visible in player behavior
- Adaptation to changing game conditions
- Development of personal playstyles

### **Strategic Depth Indicators**
- Decisions have meaningful long-term consequences
- Multiple factors influence optimal choices
- Trade-offs exist between different approaches
- Mastery requires understanding system interactions

The goal is creating a game where the fun comes from the player's strategic thinking and discovery, not from automated systems that solve challenges for them.