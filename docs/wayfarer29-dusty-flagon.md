# The Dusty Flagon: Complete POC Implementation Plan

## I. Core Location Structure

The Dusty Flagon is a two-story inn situated at a crossroads, forced to become your temporary home due to circumstances beyond your control (depleted funds, winter weather, or an injury). Your goal is to survive, recover resources, and eventually continue your travels—unless the inn's secrets change your plans.

### Initial Location Spots (Available at Start)

1. **Common Room** (Feature)
   - Central gathering area with fireplace, tables, and bar counter
   - Primary social hub where most interactions begin
   - Natural starting point for exploration

2. **Innkeeper: Bertram Oaks** (Character)
   - Gruff but fair proprietor in his fifties
   - Controls access to lodging and meals
   - Key gatekeeper to deeper inn exploration

### Resource Systems

1. **Core Resources**
   - **Energy** (0-10): Physical capacity, depletes with physical actions
   - **Focus** (0-10): Mental capacity, depletes with mental efforts
   - **Coins** (0-∞): Currency for purchasing goods and services
   - **Food** (0-5): Consumable items that restore Energy

2. **Character Stats**
   - **Skills**: Labor, Precision, Persuasion, Rapport, Observation, Contemplation
   - Each starts at level 0, improves through use
   - Each additional level provides +1 to relevant action outcomes

3. **Spot Progression**
   - **Location XP**: Gained through repeatable actions
   - **Level Thresholds**: 10 XP for Level 1, 25 XP for Level 2
   - **Relationship Levels**: Correspond to character location spot levels

## II. Complete Location Spot Catalog

### A. Ground Floor Spots

1. **Common Room** (Feature)
   - **Description**: The central gathering space with worn wooden tables, a long bar counter, and a stone fireplace that struggles to warm the drafty space.
   - **Initial Action**: "Survey the Room" (Observation)
   - **Unlockable Actions**: "Listen to Conversations", "Study Architecture"
   - **Leads to**: Cellar, Kitchen, Guest Room

2. **Innkeeper: Bertram Oaks** (Character)
   - **Description**: A broad-shouldered man with graying hair and a neatly trimmed beard. His eyes constantly scan the room, missing nothing.
   - **Initial Action**: "Introduce Yourself" (Rapport/Persuasion)
   - **Unlockable Actions**: "Request Work", "Inquire About Area"
   - **Leads to**: Storage Closet, Ledger Book

3. **Kitchen** (Feature)
   - **Description**: A cramped space with a large hearth, hanging pots, and well-used cutting tables. The smell of herbs and smoke permeates everything.
   - **Initial Action**: "Examine Kitchen" (Observation)
   - **Unlockable Actions**: "Help with Cooking", "Search for Scraps"
   - **Leads to**: Cook, Herb Garden

4. **Cook: Eliza** (Character)
   - **Description**: A middle-aged woman with strong arms and a no-nonsense demeanor. Burns mark her forearms, badges of her profession.
   - **Initial Action**: "Meet the Cook" (Rapport)
   - **Unlockable Actions**: "Assist with Meals", "Learn Recipe"
   - **Leads to**: Family Recipes, Market Contact

5. **Cellar** (Feature)
   - **Description**: A cool, damp underground room with barrels of ale, wine racks, and food stores. Light filters weakly through small ground-level windows.
   - **Initial Action**: "Inspect Cellar" (Observation)
   - **Unlockable Actions**: "Organize Supplies", "Check Wine Stock"
   - **Leads to**: Hidden Wall, Old Chest

6. **Storage Closet** (Feature)
   - **Description**: A cluttered space filled with cleaning supplies, spare linens, and maintenance tools. Everything has a musty, forgotten smell.
   - **Initial Action**: "Search Closet" (Observation)
   - **Unlockable Actions**: "Clean Inn", "Repair Furniture"
   - **Leads to**: Broken Locket, Odd Key

7. **Regular Patron: Henrik** (Character)
   - **Description**: A weathered farmer who visits daily. His calloused hands and sunburnt face tell of a life working the land.
   - **Initial Action**: "Greet Patron" (Rapport)
   - **Unlockable Actions**: "Share a Drink", "Discuss Local Farms"
   - **Leads to**: Farm Work, Local Gossip

8. **Traveling Merchant: Silas** (Character)
   - **Description**: A well-dressed man with an easy smile and calculating eyes. His fine clothes mark him as someone who deals in valuable goods.
   - **Initial Action**: "Approach Merchant" (Persuasion)
   - **Unlockable Actions**: "Negotiate Prices", "Offer Services"
   - **Leads to**: Rare Goods, Travel Information

### B. Second Floor Spots

9. **Guest Room** (Feature)
   - **Description**: A small room with a simple bed, washbasin, and wooden chest. The straw mattress has seen better days.
   - **Initial Action**: "Inspect Room" (Observation)
   - **Unlockable Actions**: "Rest Properly", "Secure Belongings"
   - **Leads to**: Window View, Loose Floorboard

10. **Mysterious Guest: Lydia** (Character)
    - **Description**: A quiet woman who keeps to herself. She wears scholarly attire yet seems uncomfortable when attention turns her way.
    - **Initial Action**: "Notice Stranger" (Observation)
    - **Unlockable Actions**: "Casual Conversation", "Observe Behavior"
    - **Leads to**: Hidden Knowledge, Secretive Task

11. **Innkeeper's Office** (Feature)
    - **Description**: A small room with a cluttered desk, account books, and personal items. Maps and notices cover one wall.
    - **Initial Action**: "Glimpse Office" (Observation)
    - **Unlockable Actions**: "Examine Records", "Search Drawers"
    - **Leads to**: Inn History, Debtor List

### C. Hidden/Special Spots

12. **Hidden Alcove** (Feature)
    - **Description**: A concealed space behind a bookshelf containing a chair, small table, and unusual collection of items.
    - **Initial Action**: "Investigate Alcove" (Observation)
    - **Unlockable Actions**: "Study Privately", "Examine Collection"
    - **Leads to**: Strange Manuscript, Personal Journal

13. **Herb Garden** (Feature)
    - **Description**: A small, carefully tended garden behind the kitchen growing culinary and medicinal herbs.
    - **Initial Action**: "Discover Garden" (Observation)
    - **Unlockable Actions**: "Tend Plants", "Harvest Herbs"
    - **Leads to**: Medicinal Knowledge, Special Ingredients

14. **Old Chest** (Feature)
    - **Description**: A dust-covered wooden chest with iron fittings, pushed against the far wall of the cellar.
    - **Initial Action**: "Examine Chest" (Observation)
    - **Unlockable Actions**: "Force Lock", "Find Key"
    - **Leads to**: Family Heirloom, Old Map

## III. Complete Action Definitions

### A. Starting Location Actions

#### Common Room (Initial)

1. **"Survey the Room"** (One-time, Observation)
   - **Approach 1**: "Study the Patrons" (requires Observation 1+)
     - **Outcome**: Notice Regular Patron Henrik, unlock "Listen to Conversations" action
     - **Narrative**: You observe the inn's visitors, noting a farmer who seems to be a regular fixture here.
   
   - **Approach 2**: "Examine Architecture" (requires Contemplation 1+)
     - **Outcome**: Notice unusual structural features, unlock "Study Architecture" action
     - **Narrative**: You study the building itself, noting that some portions appear much older than others.

2. **"Listen to Conversations"** (Repeatable, Observation)
   - **Requirements**: Unlocked by "Study the Patrons" approach
   - **Cost**: 2 Focus
   - **Outcome**: +1 Coin (overhearing useful information), +3 Location XP
   - **Scaling**: +1 Coin per Observation level
   - **Narrative**: Positioning yourself strategically, you pick up fragments of conversation from around the room.

3. **"Study Architecture"** (Repeatable, Contemplation)
   - **Requirements**: Unlocked by "Examine Architecture" approach
   - **Cost**: 2 Focus
   - **Outcome**: +1 Focus recovery (finding peaceful spots), +3 Location XP
   - **Scaling**: +1 Focus recovery per Contemplation level
   - **Narrative**: You examine the inn's construction, discovering quieter areas where the noise doesn't reach.

#### Innkeeper: Bertram (Initial)

1. **"Introduce Yourself"** (One-time, Social)
   - **Approach 1**: "Formal Introduction" (requires Persuasion 1+)
     - **Outcome**: Business-like relationship, unlock "Request Work" action
     - **Narrative**: You present yourself professionally, expressing interest in potential employment during your stay.
   
   - **Approach 2**: "Friendly Greeting" (requires Rapport 1+)
     - **Outcome**: Cordial relationship, unlock "Inquire About Area" action
     - **Narrative**: You approach with a warm greeting, striking up a conversation about the inn and area.

2. **"Request Work"** (Repeatable, Persuasion)
   - **Requirements**: Unlocked by "Formal Introduction" approach
   - **Cost**: 3 Energy
   - **Outcome**: +3 Coins, +2 Location XP
   - **Scaling**: +1 Coin per Persuasion level
   - **Narrative**: You perform various tasks around the inn—cleaning, delivering messages, or running errands.

3. **"Inquire About Area"** (Repeatable, Rapport)
   - **Requirements**: Unlocked by "Friendly Greeting" approach
   - **Cost**: 2 Focus
   - **Outcome**: +1 Observation skill XP, +2 Location XP
   - **Scaling**: +1 skill XP per Rapport level
   - **Narrative**: Bertram shares information about the surrounding area, giving you insights into local customs.

### B. Sample Level-Up Actions (Common Room)

When the Common Room reaches Level 2 (25 XP), it unlocks:

1. **"Notice Unusual Activity"** (One-time, Observation)
   - **Approach 1**: "Follow Suspicious Guest" (requires Observation 2+)
     - **Outcome**: Discover Mysterious Guest: Lydia (new Character spot)
     - **Narrative**: You notice a hooded figure quietly observing other patrons. When they leave, you discreetly follow.
   
   - **Approach 2**: "Investigate Strange Sounds" (requires Labor 2+)
     - **Outcome**: Discover Cellar (new Feature spot)
     - **Narrative**: Late at night, you hear odd noises coming from below. Investigating leads you to the inn's cellar entrance.

## IV. Complete Progression Tree

### Starting Path A: Common Room → Patrons Focus

1. **Common Room** (Level 0)
   - Action: "Survey Room" → "Study Patrons" approach
   - Result: Common Room (Level 1), "Listen to Conversations" unlocked

2. **Common Room** (Level 1)
   - Action: "Listen to Conversations" (repeated for XP)
   - Result: Common Room (Level 2), "Notice Unusual Activity" unlocked

3. **Common Room** (Level 2)
   - Action: "Notice Unusual Activity" → "Follow Suspicious Guest" approach
   - Result: Discover "Mysterious Guest: Lydia" (Level 0)

4. **Mysterious Guest: Lydia** (Level 0)
   - Action: "Notice Stranger" → approaches determine relationship
   - Result: Lydia (Level 1), new action unlocked

5. **Mysterious Guest: Lydia** (Level 1)
   - Action: Repeatable action for XP
   - Result: Lydia (Level 2), "Secretive Task" unlocked

6. **Lydia** (Level 2)
   - Action: "Secretive Task" → approaches lead to either:
     - Hidden Alcove discovery
     - Strange Manuscript discovery

### Starting Path B: Common Room → Architecture Focus

1. **Common Room** (Level 0)
   - Action: "Survey Room" → "Examine Architecture" approach
   - Result: Common Room (Level 1), "Study Architecture" unlocked

2. **Common Room** (Level 1)
   - Action: "Study Architecture" (repeated for XP)
   - Result: Common Room (Level 2), "Notice Unusual Activity" unlocked

3. **Common Room** (Level 2)
   - Action: "Notice Unusual Activity" → "Investigate Strange Sounds" approach
   - Result: Discover "Cellar" (Level 0)

4. **Cellar** (Level 0)
   - Action: "Inspect Cellar" → approaches determine focus
   - Result: Cellar (Level 1), new action unlocked

5. **Cellar** (Level 1)
   - Action: Repeatable action for XP
   - Result: Cellar (Level 2), "Discover Hidden Area" unlocked

6. **Cellar** (Level 2)
   - Action: "Discover Hidden Area" → approaches lead to either:
     - Old Chest discovery
     - Hidden Wall discovery

### Starting Path C: Innkeeper → Formal Relationship

1. **Innkeeper: Bertram** (Level 0)
   - Action: "Introduce Yourself" → "Formal Introduction" approach
   - Result: Bertram (Level 1), "Request Work" unlocked

2. **Innkeeper: Bertram** (Level 1)
   - Action: "Request Work" (repeated for XP)
   - Result: Bertram (Level 2), "Offered Additional Responsibility" unlocked

3. **Innkeeper: Bertram** (Level 2)
   - Action: "Offered Additional Responsibility" → approaches lead to either:
     - Storage Closet (opportunity for better pay)
     - Innkeeper's Office (access to records)

Each path similarly branches out, creating dozens of potential discovery chains based on player choices. The full progression tree would include all 14 location spots with their level-up paths.

## V. Detailed Action Economy

### Energy Economy

- **Starting Value**: 10 Energy (maximum)
- **Daily Natural Recovery**: +5 Energy each morning
- **Consumption**: Physical actions cost 2-4 Energy each
- **Recovery Actions**:
  - Basic Rest (free): +2 Energy, no Focus recovery
  - Proper Rest (costs 1 Coin): +4 Energy, +2 Focus
  - Quality Rest (costs 3 Coins): +6 Energy, +4 Focus
  - Meal (costs 2 Coins): +3 Energy, +1 Focus
  - Quality Meal (costs 4 Coins): +5 Energy, +2 Focus

### Focus Economy

- **Starting Value**: 8 Focus (maximum)
- **Daily Natural Recovery**: +3 Focus each morning
- **Consumption**: Mental actions cost 1-3 Focus each
- **Recovery Actions**:
  - Quiet Contemplation (free): +2 Focus, no Energy recovery
  - Reading (requires book): +3 Focus
  - Deep Sleep (requires private room): +3 Focus

### Coin Economy

- **Starting Value**: 5 Coins
- **Acquisition**:
  - Basic labor: 2-4 Coins per action
  - Skilled services: 3-6 Coins per action
  - Finding/selling items: Variable
- **Expenses**:
  - Lodging: 1-3 Coins per night
  - Meals: 1-4 Coins per meal
  - Specialty items: 5-20 Coins

### Skill Advancement

- All skills start at Level 0
- Each skill level provides +1 to relevant action outcomes
- Advancement costs (in skill-specific XP):
  - Level 0 → Level 1: 5 XP
  - Level 1 → Level 2: 12 XP
  - Level 2 → Level 3: 20 XP
- Skills improve through:
  - Using related actions
  - Learning from skilled characters
  - Practicing specific techniques

## VI. Sample Narrative Content

### Common Room: Initial Description

*The Dusty Flagon's common room spreads before you, a study in worn comfort and lingering history. Mismatched wooden tables crowd the space, their surfaces etched with countless initials and marks from years of use. A substantial stone fireplace commands the far wall, its flames casting dancing shadows across the room while struggling to fully dispel the persistent chill. The air carries a complex mixture of aromas—ale and woodsmoke, stew and sweat, leather and polish—the unmistakable scent of a well-used gathering place.*

### Character Introduction: Bertram

*The innkeeper notices your approach, setting aside the mug he's been polishing with mechanical precision. He's a broad-shouldered man with hair more gray than brown, his face weathered like cracked leather from years of worry and work. His eyes, sharp and assessing, take your measure instantly.*

*"Haven't seen you before," he states rather than asks, his voice a gravelly rumble. "Room's a silver piece a night, meals extra. We've got ale, wine if you're particular, and whatever Eliza's cooking up today." He waits expectantly, a man accustomed to making dozens of quick judgments about strangers every day.*

### Action Outcome: Listen to Conversations

*You position yourself at a corner table with your back to the wall, nursing a mug of watered ale while letting the conversations wash over you. Two farmers near the hearth discuss unusual weather patterns affecting their crops. A trio of locals debate the merits of a new toll being levied on the north road. Most valuable, though, is the merchant complaining to his companion about prices at the next town over—information that might prove useful should you travel that way.*

*As conversations shift and voices rise and fall, you notice the innkeeper watching you with a thoughtful expression. He's noticed your attentiveness, though whether he approves remains unclear.*

## VII. Implementation Requirements

### Core Systems

1. **State Tracking**
   - Current values of all resources (Energy, Focus, Coins)
   - Current level and XP for each location spot
   - Discovered location spots
   - Completed one-time actions and chosen approaches
   - Unlocked repeatable actions
   - Skill levels and progression

2. **Action Processing**
   - Validate action requirements
   - Deduct costs
   - Apply outcomes (including skill-based scaling)
   - Update location XP
   - Check for level-up triggers
   - Apply narrative outcome

3. **Progression System**
   - Track location spot XP
   - Trigger level-up events
   - Unlock new one-time actions
   - Process discovery of new location spots

4. **Time System**
   - Track day count
   - Manage morning/afternoon/evening/night periods
   - Apply daily resource recovery
   - Control character availability based on time

### AI Integration Points

1. **Location Descriptions**
   - Generate atmospheric details based on location tags
   - Adjust descriptions based on time of day and weather
   - Incorporate narrative elements reflecting spot level

2. **Character Interactions**
   - Generate dialogue appropriate to character personality
   - Adapt responses based on relationship level
   - Include hints about potential discoveries

3. **Action Outcomes**
   - Create varied narrative descriptions for repeatable actions
   - Generate discovery moments for one-time actions
   - Include subtle hints about other possible paths

4. **Special Events**
   - Create occasional random events based on player circumstances
   - Generate weather and seasonal effects
   - Introduce periodic visitor characters

## VIII. Balancing Considerations

### Time as Resource

The daily cycle creates natural constraints:
- Each day has limited time periods (morning, afternoon, evening, night)
- Each time period allows 1-2 significant actions
- Some actions/characters are only available during specific periods
- Days advance regardless of player action, creating steady pressure

### Skill Specialization Trade-offs

Skill advancement creates natural specialization:
- Investing in Labor improves physical work outcomes but not social options
- Developing Rapport improves relationship building but not analytical tasks
- Each skill path opens unique opportunities while leaving others unexplored

### Resource Tension

Critical resource decisions include:
- Spending Energy on immediate needs vs. long-term opportunities
- Investing Coins in comfort (better rest) vs. saving for special items
- Using Focus on information gathering vs. skill improvement
- Balancing relationship building with resource acquisition

This comprehensive plan provides everything needed to implement the proof-of-concept for Wayfarer's single-location system. The design creates meaningful player choices, multiple progression paths, and a balanced resource economy while maintaining the elegant simplicity requested.