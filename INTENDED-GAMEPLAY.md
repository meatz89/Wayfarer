## THE CORE PRINCIPLE: Categorical Interconnection

Each entity should have **multiple categorical properties**, and simple logical rules connect these properties across different systems. The restrictions emerge from **obvious real-world logic**, not arbitrary game balance.

## FOUNDATIONAL SYSTEMS WITH CATEGORIES

### ITEMS (Multiple Categories Per Item)

**Function Categories**: [Climbing, Navigation, Social, Crafting, Protection, Information]
**Material Categories**: [Metal, Leather, Cloth, Paper, Wood, Glass]  
**Size Categories**: [Tiny, Small, Medium, Large, Massive]
**Fragility Categories**: [Sturdy, Standard, Delicate, Fragile]

### ROUTES (Multiple Categories Per Route)

**Terrain Categories**: [Forest, Mountain, River, Road, Urban, Desert]
**Difficulty Categories**: [Easy, Moderate, Challenging, Extreme]
**Width Categories**: [Narrow_Path, Standard, Wide_Road, Open]
**Traffic Categories**: [Busy, Moderate, Quiet, Abandoned]

### NPCS (Multiple Categories Per NPC)

**Social_Class Categories**: [Commoner, Merchant, Craftsman]
**Profession Categories**: [Trader, Smith, Guard, Official, Farmer]
**Location_Preference Categories**: [Market, Workshop, Tavern, Manor, Street]
**Time_Available Categories**: [Morning, Afternoon, Evening, Always, Special_Events]
**Relationship_Memory Categories**: [Helpful, Neutral, Wary, Hostile]

### LOCATIONS (Multiple Categories Per Location)

**Function Categories**: [Commerce, Crafting, Social, Official, Residential]
**Access_Level Categories**: [Public, Semi_Private, Private, Restricted]
**Social_Expectation Categories**: [Any, Merchant_Class, Professional]
**Service_Type Categories**: [Trade, Repair, Information, Lodging, Authority]

## SIMPLE LOGICAL RULES CONNECTING SYSTEMS

### Equipment-Terrain Rules
- **Terrain [Mountain] + No Function [Climbing]** = Route Physically Impossible
- **Terrain [River] + No Function [Navigation]** = High chance of getting lost/delayed
- **Terrain [Urban] + No Social_Signal [Appropriate_Class]** = Restricted access to quality services

### Transport-Terrain Rules  
- **Transport [Cart] + Terrain [Mountain/Forest]** = Physically Incompatible
- **Transport [Boat] + Terrain [Not_River]** = Cannot Use
- **Size [Large] Items + Transport [Walking]** = Severe stamina penalty

### Material-Condition Rules
- **Material [Paper] + Rough Travel** = Fragility damage risk
- **Material [Metal] + Size [Large]** = Heavy (affects stamina/transport)
- **Material [Cloth] + Social_Signal [Noble]** = Requires careful maintenance

### Information-Network Rules
- **Profession [Trader] + Relationship [Helpful]** = Shares price information
- **Location_Preference [Tavern] + Time [Evening]** = Information exchange opportunity
- **Social_Class [Local_Commoner]** = Knows local route conditions

## CONCRETE WAYFARER EXAMPLES

### Example 1: The Mountain Shortcut
**Scenario**: Discovered route cuts travel time in half

**System Interactions**:
- **Route Categories**: Terrain [Mountain], Difficulty [Extreme], Width [Narrow_Path]
- **Current Equipment**: No Function [Climbing] items
- **Transport**: Considering cart rental for cargo capacity
- **Rules**: 
  - Terrain [Mountain] + No Function [Climbing] = Route Blocked
  - Width [Narrow_Path] + Transport [Cart] = Physically Incompatible

**Player Experience**: "This shortcut would save a day, but I need climbing gear AND I can't bring the cart. Do I prioritize speed or cargo capacity?"

### Example 2: The Fragile Cargo
**Scenario**: Valuable glassware trade opportunity

**System Interactions**:
- **Item Categories**: Material [Glass], Fragility [Fragile], Size [Medium], Social_Signal [Luxury]
- **Route Options**: Mountain path (rough) or river route (smooth but longer)
- **Rules**: 
  - Material [Glass] + Fragility [Fragile] + Rough Travel = High damage risk

**Player Experience**: "Glassware sells for triple to nobles, but the mountain route might shatter it. River route is safer but takes longer and costs transport fees..."

## ACTIONABLE DESIGN CHANGES FOR WAYFARER

### 1. Replace All Numerical Modifiers with Categorical Rules

### 2. Give Every Item Multiple Categories
**Instead of**: "Fine Cloak" with arbitrary stats
**Create**: 
- Function [Protection], Material [Wool], Size [Medium]
- Social_Signal [Merchant], Fragility [Standard]
- **Rules**: Enables cold weather travel, signals merchant status, takes inventory space

### 3. Make All Route Restrictions Logical
**Instead of**: "Route locked until level 5"
**Create**: 
- Terrain [Mountain] route requires Function [Climbing] equipment
- Access [Private] route requires relationship with landowner
- Difficulty [Extreme] route requires high stamina + appropriate gear

### 4. Create Equipment Prerequisites, Not Stat Requirements
**Instead of**: "Requires 15 Strength"
**Create**: Size [Massive] items require Transport [Cart] or Function [Lifting] equipment

### 5. Make Social Access Equipment-Based
**Instead of**: Reputation thresholds
**Create**: Social_Expectation categories matched with Social_Signal equipment

### 6. Create Information Prerequisites  
**Instead of**: Hidden content that unlocks
**Create**: Profession [Craftsman] NPCs know where to source materials; Profession [Trader] NPCs know profitable routes

### 7. Make Service Availability Logical
**Instead of**: "Shop opens at level 3"
**Create**: Location Access_Level [Semi_Private] requires Social_Signal [Merchant] or relationship with owner

## THE PLAYER EXPERIENCE TARGET

**Current**: "I can't use this route because my climbing skill isn't high enough"
**Target**: "I can't use this route because it goes through mountain terrain and I don't have climbing equipment"

**Current**: "I can't carry this item because it's too heavy"
**Target**: "I can carry this massive item, but it takes up 2 inventory slots and I'll need a cart if I want to travel efficiently"

Every constraint emerges from **logical categorical connections** that players intuitively understand, creating the complex strategic experience through simple, obvious rules rather than hidden math systems.