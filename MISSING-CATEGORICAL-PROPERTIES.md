# Missing Categorical Properties from UI Mockups

## Analysis Date: 2025-08-21
After analyzing both conversation-screen.html and location-screens.html mockups against our current implementation, here are the categorical properties that should influence mechanics but are not yet implemented:

## 1. Location Spot Properties (MISSING)
**Mockup Shows:**
- Spot traits: `Private`, `ViewsMainEntrance`, `+1 Comfort`
- Location traits: `Public Square`, `Moderate Crowd`, `Crossroads`, `Warm Hearth`, `Busy Evening`
- Time-specific traits that change by time period

**Should Influence:**
- Observation availability

## 3. Relationship Stakes (MISSING)
**Mockup Shows:**
- Stakes: `Safety` (Elena's arranged marriage)
- Different stake levels affecting urgency

**Should Influence:**
- Crisis card availability
- Emotional state transitions

## 4. Card Combination Markers (MISSING)
**Mockup Shows:**
- `Can Combine` marker on comfort cards
- `Solo Only` marker on state cards
- `FREE IN DESPERATE!` marker on crisis cards

**Should Influence:**
- Which cards can be played together
- Visual feedback before playing
- Tutorial/learning for players

## 5. Observation Type Categories (MISSING)
**Mockup Shows:**
- Observation types: `Authority`, `Commercial`, `Social`, `Secret`
- Generation source: `Event-Generated`, `Time-Generated`, `Location-Generated`

**Should Influence:**
- Which NPCs care about the observation
- Card type when converted
- Success rates based on NPC interests

## 6. Transport Types (MISSING)
**Mockup Shows:**
- Transport options: `Walk`, `Cart`, `???` (secret routes)
- Different time costs and requirements

**Should Influence:**
- Travel time between locations
- Coin cost for premium options
- Access requirements (Status for noble carriage)

## 9. Hierarchy Tags (MISSING)
**Mockup Shows:**
- Location hierarchy: Region → District → Location → Spot
- Tags at each level: `Modest`, `Tavern`, `Region`

**Should Influence:**
- Access restrictions
- Comfort modifiers based on class match

## 10. Effect Tags for Actions (MISSING)
**Mockup Shows:**
- Cost tags: `Status 3+ required`, `3 coins`
- Gain tags: `Pass immediately`, `+1 Authority observation`
- Effect tags: `Creates fatigue`, `Normal passage`

**Should Influence:**
- Action availability
- Resource management
- State changes