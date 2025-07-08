# Skill-Scaled Action System

The Skill-Scaled Action System creates persistent value for all actions by linking outcomes directly to skill levels through elegant, deterministic rules.

## Core Mechanics

### Action Structure
Each action has:
- **Required Skill**: Minimum skill level to perform the action
- **Primary Output**: Resource that scales with skill level
- **Secondary Output**: Fixed resource regardless of skill level
- **Scaling Factor**: Determines efficiency of skill scaling
- **AP Cost**: Action points required

### Scaling Formula
```
Primary Output Quantity = Base + (Current Skill - Required Skill) ÷ Scaling Factor
```
*All divisions rounded down*

### Resource Categories
Resources belong to simple categories:
- Food: Meat, Vegetable, Fruit, Grain, Fish
- Materials: Hide, Cloth, Wood, Stone, Metal
- Medicinal: Herb, Root, Mushroom, Leaf, Flower

## Action Examples

### Forest Location: Woodsman's Grove
All actions use the Foraging skill but remain valuable at all skill levels:

**1. Gather Berries**
- Required: Foraging 1
- Primary: Fruit (scales 1:1)
- Secondary: None
- AP Cost: 1
- Formula: 1 + (Foraging - 1) Fruit

**2. Hunt Rabbits**
- Required: Foraging 3
- Primary: Meat (scales 1:2)
- Secondary: 1 Hide (always)
- AP Cost: 2
- Formula: 1 + (Foraging - 3) ÷ 2 Meat, plus 1 Hide

**3. Find Medicinal Mushrooms**
- Required: Foraging 5
- Primary: Mushroom (scales 1:3)
- Secondary: 1 Root (always)
- AP Cost: 2
- Formula: 1 + (Foraging - 5) ÷ 3 Mushroom, plus 1 Root

### Skill Level Comparison

At **Foraging 6**:
- Gather Berries: 6 Fruit (6 resources for 1 AP)
- Hunt Rabbits: 2 Meat + 1 Hide (3 resources for 2 AP)
- Find Mushrooms: 1 Mushroom + 1 Root (2 resources for 2 AP)

At **Foraging 12**:
- Gather Berries: 12 Fruit (12 resources for 1 AP)
- Hunt Rabbits: 5 Meat + 1 Hide (6 resources for 2 AP)
- Find Mushrooms: 3 Mushroom + 1 Root (4 resources for 2 AP)

## Strategic Depth

This system creates meaningful decisions because:

1. **Different Resource Needs**: At Foraging 12, the player decides which resources they need rather than automatically choosing the "best" action

2. **Efficiency Variations**: 
   - Berries: Most efficient (1:1 scaling, 1 AP)
   - Rabbits: Medium efficiency (1:2 scaling, 2 AP)
   - Mushrooms: Least efficient (1:3 scaling, 2 AP)

3. **Diminishing Returns**: Higher skill requirements mean slower scaling, naturally balancing more valuable resources

4. **Resource Combinations**: Different actions provide unique resource combinations that remain relevant regardless of skill level

## System-Wide Implementation

This pattern applies across all locations:

- **Farm**: Weeding (Farming 1), Harvesting (Farming 3), Crop Rotation (Farming 5)
- **Workshop**: Simple Repairs (Crafting 1), Furniture Making (Crafting 3), Fine Joinery (Crafting 5)
- **Market**: Haggling (Commerce 1), Bulk Purchasing (Commerce 3), Rare Goods Trading (Commerce 5)

Each location maintains this elegant skill-scaling structure, ensuring all actions remain viable choices throughout the game while rewarding skill advancement.