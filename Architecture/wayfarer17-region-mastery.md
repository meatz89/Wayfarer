# Region Mastery: Integrated Location-Action System

## Core Architecture

The Region Mastery System requires a deep integration between locations and actions through a **Resource Node** architecture:

### Region Properties
- **Resource Quality Tier**: Determines maximum quality of resources (Novice to Master)
- **Skill Ceiling**: Maximum skill level achievable in this region (3/5/7/10)
- **Discovery Pool**: Unique discoverable elements that grant significant skill progression

### Location Spots as Resource Nodes
Location spots transform from static interaction points to dynamic resource nodes:

- **Resource Type**: Primary resource this node generates (Berries, Herbs, etc.)
- **Resource Quality**: Current quality tier of this node (Novice to Master)
- **Depletion State**: Current availability level (Abundant to Depleted)
- **Mastery Flag**: Whether player has "mastered" this specific node
- **Hidden Properties**: Discoverable aspects that reward exploration

### Integrated Actions

Actions become contextualized expressions of the resource node's properties:

- **Base Action Template**: Core mechanics (energy cost, encounter chance)
- **Node-Specific Expression**: How the action manifests at this specific node
- **Skill Requirement**: Minimum skill needed to perform optimally
- **Discovery Potential**: What new elements can be discovered here

## How The System Works

### 1. Dynamic Resource Nodes

When a player interacts with a location spot (resource node):

```
Berry Bush (Resource Node)
- Resource Type: Food (Berries)
- Quality: Novice (Tutorial Tier)
- Depletion: Abundant
- Visible Skills: Foraging
- Hidden Aspects: Discovering ➝ A subspecies of berry grows here
```

This node presents contextual actions based on the player's skills:

```
For Foraging Level 1:
- "Gather Berries" (Basic gathering)

For Foraging Level 2:
- "Gather Berries" (Improved efficiency)
- "Identify Berry Species" (New discovery action)

For Foraging Level 3:
- "Selective Harvesting" (Advanced technique)
- "Teach Gathering Technique" (If NPC present)
```

### 2. Discovery-Driven Progression

The system tracks three interaction states with each resource node:

- **First Discovery**: Significant skill XP when first finding the node
- **Property Discovery**: Major skill XP when revealing hidden aspects
- **Technique Discovery**: Skill XP when first using a new technique at this node
- **Repeated Harvest**: Minimal skill XP for repeated gathering

### 3. Visual Mastery Communication

As players interact with resource nodes, they receive visual feedback:

- **Fresh Discovery**: Node appears vibrant and prominent
- **Partially Explored**: Visual indicators show undiscovered aspects
- **Fully Mastered**: Node appears muted with "mastered" indicator
- **Depleted**: Temporary state showing resource needs to replenish

### 4. Natural Progression Flow

When a player approaches location mastery:

1. Resource nodes begin yielding diminishing returns
2. Visuals highlight undiscovered aspects of remaining nodes
3. Players can see higher-quality nodes in adjacent regions
4. The discovery log shows completion percentage for the region

## Elegant Implementation

What makes this system elegant is how it avoids artificial restrictions while creating natural progression:

### 1. Unified Data Structure

Resource nodes follow a unified structure that works for all resource types:

```
ResourceNode {
  type: FOOD/HERB/LANDMARK/etc.,
  quality: NOVICE/APPRENTICE/ADEPT/MASTER,
  baseYield: 2.0,
  depletion: 1.0, // multiplier from 0.0-1.0
  discoveryState: UNDISCOVERED/BASIC/ADVANCED/MASTERED,
  skillXPOnFirstDiscover: 0.7,
  skillXPOnPropertyDiscover: 0.5,
  skillXPOnRepeat: 0.1,
  replenishRate: 0.2 // per game-hour
}
```

### 2. Contextual Actions

Actions draw properties directly from the resource node:

```
// For a Berry Bush node with Foraging Level 2:
GatherAction {
  baseEnergyCost: 15,
  nodeEfficiencyMultiplier: 1.1, // from Foraging Level 2
  actualYield: baseYield * depletion * nodeEfficiencyMultiplier,
  skillProgression: node.discoveryState == MASTERED ? 
                   skillXPOnRepeat : skillXPOnPropertyDiscover
}
```

### 3. Regional Boundaries

Region transitions occur naturally when:

1. Player has discovered most nodes in current region (80%+)
2. Player has reached skill levels near the region ceiling
3. Resource depletion creates natural pressure to explore

## Tutorial Implementation

The tutorial region implements this system by:

1. Containing only Novice-tier resource nodes
2. Having a clear visual boundary showing higher-tier resources beyond
3. Including 3-4 discoverable aspects per skill
4. Naturally depleting resources as skills approach level 3
5. Creating a natural "resource pressure" to explore forward

This creates an elegant system where players learn the core mechanics while experiencing natural forward momentum without artificial restrictions.

# Tutorial Region: Deepwood Forest - Minimum Content

## Region Properties
- **Name**: Deepwood Forest (Tutorial Region)
- **Resource Quality Tier**: Novice
- **Skill Ceiling**: Level 3
- **Visual Indicator**: Smaller, less vibrant resources compared to visible Elmridge Valley beyond

## Resource Nodes

### 1. Forest Entrance
- **Type**: Landmark
- **First Interaction**: Player orientation, basic controls
- **Discoverable Aspects**: 
  - Trail markers (+0.5 Navigation)
  - Local wildlife signs (+0.5 Perception)
- **Available Actions**:
  - "Rest" (Restore Energy, 0% encounter chance)
  - "Search Surroundings" (40% encounter chance, intellectual)

### 2. Berry Thicket
- **Type**: Food Resource
- **Quality**: Novice
- **Depletion State**: Initially Abundant
- **Discoverable Aspects**:
  - Berry ripeness patterns (+0.7 Foraging)
  - Bird feeding patterns (+0.5 Perception)
  - Efficient gathering technique (+0.5 Foraging)
- **Available Actions**:
  - "Gather Berries" (25% encounter chance, physical)
  - "Identify Berry Types" (unlocks at Foraging 1)
  - "Selective Harvesting" (unlocks at Foraging 2)

### 3. Herb Patch
- **Type**: Medicinal Resource
- **Quality**: Novice
- **Depletion State**: Initially Abundant
- **Discoverable Aspects**:
  - Leaf pattern identification (+0.7 Herbalism)
  - Growth requirement understanding (+0.5 Herbalism)
  - Medicinal properties (+0.5 Herbalism)
- **Available Actions**:
  - "Gather Herbs" (30% encounter chance, intellectual)
  - "Identify Medicinal Properties" (unlocks at Herbalism 1)
  - "Careful Extraction" (unlocks at Herbalism 2)

### 4. Stream Crossing
- **Type**: Water Resource & Travel Node
- **Quality**: Novice
- **Discoverable Aspects**:
  - Water purity assessment (+0.7 Perception)
  - Safe crossing point (+0.7 Navigation)
  - Fish presence (+0.5 Foraging)
- **Available Actions**:
  - "Drink Water" (10% encounter chance, restores Health/Concentration)
  - "Cross Stream" (50% encounter chance, physical)
  - "Find Fishing Spot" (unlocks at Perception 2)

### 5. Ancient Tree
- **Type**: Landmark & Navigation Node
- **Quality**: Novice
- **Discoverable Aspects**:
  - Height advantage (+0.7 Navigation)
  - Seasonal markings (+0.5 Perception)
  - Cardinal direction determination (+0.5 Navigation)
- **Available Actions**:
  - "Climb Tree" (40% encounter chance, physical)
  - "Study Surroundings" (35% encounter chance, intellectual)
  - "Mark Trail" (unlocks at Navigation 2)

### 6. Forest Exit Path
- **Type**: Progression Node
- **Visible Beyond**: Elmridge Valley (Apprentice Tier resources clearly visible)
- **Final Challenge**:
  - "Find Path Out" (100% encounter chance - guaranteed "boss" encounter)
  - Mixed physical/intellectual challenge
  - Success required to exit tutorial region

## Visual Progression Indications

- **Region Boundary**: Forest thins, revealing lusher valley beyond
- **Resource Contrast**: Noticeably larger, more vibrant berries visible in distance
- **Quality Signaling**: Herb plants beyond tutorial boundary visibly different color/size
- **NPC Dialogue**: Occasional hints about "better foraging beyond the forest"

## Tutorial Flow

1. **Orientation**: Forest Entrance introduction
2. **Basic Resource Collection**: Berry Thicket for food
3. **Resource Management**: Herb Patch for healing resources
4. **Environmental Navigation**: Stream Crossing introduces travel challenges
5. **Strategic Discovery**: Ancient Tree teaches observation and planning
6. **Culmination Challenge**: Forest Exit Path tests all learned skills

## Progressive Resource Pressure

- Berry Thicket depletes to 50% after 3 gatherings
- Herb Patch depletes to 50% after 2 gatherings
- Stream water quality slightly decreases after 3 uses
- All resources replenish very slowly (20% per in-game day)

## Skill Progression Caps

Total possible skill gains in tutorial region:
- **Foraging**: Maximum Level 2.7 (insufficient for Level 3)
- **Herbalism**: Maximum Level 2.7 (insufficient for Level 3)
- **Navigation**: Maximum Level 2.7 (insufficient for Level 3)
- **Perception**: Maximum Level 2.7 (insufficient for Level 3)

This creates natural forward momentum as players approach but cannot reach Level 3 skills without advancing to the next region.