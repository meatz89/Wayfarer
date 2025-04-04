# Environmental Property Card System

## Overview

The Wayfarer property card system enhances the tiered card progression with environmental property synergies. This system creates dynamic, changing environments that interact with player cards to produce varying strategies based on time, place, and circumstance.

## Environmental & Situational Properties

Strategic tags are now based on properties that can be combined to create varied environments:

### Illumination Properties
- **Bright** - Well-lit areas with clear visibility
  - *Effect Example*: Enhances Precision and Analysis approaches
  - *Locations*: Open market (day), noble court, temples
  
- **Shadowy** - Areas with mixed lighting and many shadows
  - *Effect Example*: Enhances Evasion approach, creates tension
  - *Locations*: Forest edge, taverns, evening streets
  
- **Dark** - Poorly lit or nighttime environments
  - *Effect Example*: Significantly boosts Evasion, penalizes Precision
  - *Locations*: Night environments, caves, cellars

### Population Properties
- **Crowded** - Densely populated areas with many people
  - *Effect Example*: Enhances Rapport and Dominance approaches
  - *Locations*: Markets, festivals, public gatherings
  
- **Quiet** - Sparsely populated with few observers
  - *Effect Example*: Enhances Analysis and Precision approaches
  - *Locations*: Libraries, private studies, early morning streets
  
- **Isolated** - Completely private or secluded environments
  - *Effect Example*: Boosts extreme approaches (very high Dominance or Evasion)
  - *Locations*: Wilderness, abandoned buildings, private chambers

### Atmosphere Properties
- **Tense** - High-pressure, stressful environments
  - *Effect Example*: Increases pressure generation, enhances decisive actions
  - *Locations*: Negotiations, dangerous areas, confrontations
  
- **Formal** - Rule-bound, ceremonial, or structured settings
  - *Effect Example*: Enhances Rapport and Analysis, penalizes Dominance
  - *Locations*: Courts, ceremonies, guild meetings
  
- **Chaotic** - Unpredictable, disordered environments
  - *Effect Example*: Enhances adaptable approaches, penalizes rigid strategies
  - *Locations*: Tavern brawls, markets, celebrations

### Economic Properties
- **Wealthy** - Affluent areas with valuable resources
  - *Effect Example*: Enhances Resource focus, creates opportunities
  - *Locations*: Noble districts, guild halls, treasure rooms
  
- **Commercial** - Trading environments with economic activity
  - *Effect Example*: Enhances Rapport and Resource focus
  - *Locations*: Markets, shops, trading posts
  
- **Humble** - Simple environments with limited resources
  - *Effect Example*: Enhances Physical focus, rewards creativity
  - *Locations*: Peasant homes, wilderness camps, slums

### Physical Properties
- **Confined** - Tight spaces with limited movement
  - *Effect Example*: Enhances Precision, penalizes Dominance
  - *Locations*: Narrow alleys, small rooms, caves
  
- **Expansive** - Open areas with room to maneuver
  - *Effect Example*: Enhances Dominance, creates movement options
  - *Locations*: Fields, large halls, town squares
  
- **Hazardous** - Physically dangerous environments
  - *Effect Example*: Increases pressure, rewards careful approaches
  - *Locations*: Crumbling ruins, battlefield, stormy conditions

## Strategic Tag Implementation

Strategic tags are defined by:

1. **Property** - One of the environmental/situational properties
2. **Base Effect** - What the tag does by default (increase/decrease momentum/pressure)
3. **Scaling Approach** - Which approach the effect scales with

### Example Strategic Tags

- **"Scholar's Focus"** (Quiet, Increases Momentum, scales with Analysis)
  - *Description*: The calm, undisturbed environment enhances analytical thinking
  - *Effect*: +1 momentum per 2 Analysis points

- **"Social Scrutiny"** (Formal, Increases Pressure, scales with Rapport)
  - *Description*: The formal setting creates social pressure when trying to be charming
  - *Effect*: +1 pressure per 2 Rapport points

- **"Shadow's Embrace"** (Shadowy, Decreases Pressure, scales with Evasion)
  - *Description*: The mixed lighting provides ample hiding spots, reducing risk
  - *Effect*: -1 pressure per 2 Evasion points

- **"Crowd Command"** (Crowded, Increases Momentum, scales with Dominance)
  - *Description*: The presence of observers amplifies dominant displays
  - *Effect*: +1 momentum per 2 Dominance points

- **"Precision in Chaos"** (Chaotic, Decreases Momentum, scales with Precision)
  - *Description*: The unpredictable environment makes precise work difficult
  - *Effect*: -1 momentum per 2 Precision points

## Card Property Synergies

Cards now include conditional effects that trigger only when certain properties are present in the environment:

### Example Card with Property Synergies

```
"Perfect Deduction" (Analysis + Information)
Tier 4 | Requires Analysis 5+
Effect: +4 Momentum
Approach: -2 Analysis, +1 Information

Property Synergies:
- Quiet: +1 Momentum per 2 Analysis points
- Formal: -1 Pressure
- Dark: Card is blocked
```

### Synergy Types

Cards can have various types of property synergies:

1. **Bonus Effects** - Additional mechanical benefits when a property is present
2. **Resource Generation** - Generate or preserve resources in specific environments
3. **Requirement Modifications** - Lower requirements in favorable environments
4. **Card Transformations** - Card changes function in certain environments
5. **Blocking Conditions** - Card cannot be used in certain environments

## Dynamic Location Design

Locations combine multiple properties to create unique strategic environments:

### Example Location: Market Square

**Morning Market** (7am-11am)
- Properties: Bright + Crowded + Commercial + Chaotic
- Strategic Tags:
  - "Merchant's Eye" (Commercial, Increases Momentum, scales with Analysis)
  - "Public Spectacle" (Crowded, Increases Momentum, scales with Dominance)
  - "Overwhelming Stimuli" (Chaotic, Increases Pressure, scales with Precision)
  - "Clear Visibility" (Bright, Decreases Momentum, scales with Evasion)

**Afternoon Market** (12pm-5pm)
- Properties: Bright + Crowded + Commercial + Tense
- Strategic Tags:
  - "Merchant's Eye" (Commercial, Increases Momentum, scales with Analysis)
  - "Public Spectacle" (Crowded, Increases Momentum, scales with Dominance)
  - "Social Pressure" (Tense, Increases Pressure, scales with Rapport)
  - "Clear Visibility" (Bright, Decreases Momentum, scales with Evasion)

**Evening Market** (6pm-9pm)
- Properties: Shadowy + Crowded + Commercial + Chaotic
- Strategic Tags:
  - "Merchant's Eye" (Commercial, Increases Momentum, scales with Analysis)
  - "Public Spectacle" (Crowded, Increases Momentum, scales with Dominance)
  - "Overwhelming Stimuli" (Chaotic, Increases Pressure, scales with Precision)
  - "Shadow's Embrace" (Shadowy, Decreases Pressure, scales with Evasion)

**Night Market** (10pm-12am)
- Properties: Dark + Quiet + Commercial + Tense
- Strategic Tags:
  - "Merchant's Eye" (Commercial, Increases Momentum, scales with Analysis)
  - "Whispered Deals" (Quiet, Increases Momentum, scales with Rapport)
  - "Social Pressure" (Tense, Increases Pressure, scales with Rapport)
  - "Cloak of Darkness" (Dark, Increases Momentum, scales with Evasion)

## Gameplay Impact

The property-based strategic tag system transforms gameplay in several ways:

### 1. Timing Strategy

Players consider not just where to go, but when to go there:
- Sneaky characters prefer night versions of locations
- Socially dominant characters prefer crowded daytime settings
- Analytical characters might prefer quiet morning hours
- Different quests may be easier at different times of day

### 2. Card Collection Strategy

Players build card collections with complementary property synergies:
- Cards that work well in the same property combinations
- Cards that excel in properties that align with their approach preferences
- Versatile cards that have benefits in multiple environment types

### 3. Adaptive Play

Players adapt to changing conditions:
- Weather changes might alter available properties
- Time progression changes illumination properties
- Player actions may alter population properties
- Special events create unique property combinations

### 4. Increased Replayability

The same location feels different based on:
- Time of day
- Weather conditions
- Player actions in previous encounters

## Implementation Notes

When implementing the property-based card system:

1. **Location Design**: Each location should have 4-5 different property combinations based on time of day
2. **Gradual Transitions**: Properties should change gradually (Bright → Shadowy → Dark)
3. **Weather Impact**: Weather conditions should modify available properties
4. **Card Balance**: Ensure cards have synergies with diverse property combinations
5. **UI Elements**: Clearly show active properties and card synergies to players
6. **Property Icons**: Create distinctive icons for each property
7. **Time System**: Implement a day/night cycle that affects all locations

## Conclusion

The Environmental Property Card System creates a dynamic, living world where timing and context matter as much as location. By integrating property-based strategic tags with the existing card tier system, Wayfarer achieves a rich strategic landscape that feels natural and intuitive while offering tremendous depth and replayability.