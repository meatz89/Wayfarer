"Your job is to analyze the completed encounter and extract specific information into separate JSON objects.

Carefully review the entire encounter narrative and extract ALL relevant information about changes, discoveries, and outcomes. For each category, you'll create a separate JSON object rather than nesting them all together.

## REQUIRED OUTPUT SECTIONS
You must provide the following sections, each with its own JSON object:

### 1. ENCOUNTER OUTCOME
```json
{
  "outcome_level": "Exceptional/Standard/Partial/Failure", // Use exactly one of these values
  "goal_achieved": true/false, // Boolean (lowercase)
  "success_details": "Brief description of how the goal was achieved or failed"
}
```

### 2. INVENTORY CHANGES
List of all items acquired or lost:
```json
[
  {
    "item": "Item name",
    "quantity": 1, // Integer
    "acquired": true/false, // Boolean (true = gained, false = lost)
    "source": "How/where/from whom it was acquired", 
    "description": "Brief description of the item"
  }
]
```

### 3. RESOURCE CHANGES
Track all resource changes in one flat object:
```json
{
  "money_amount": 0, // Integer (positive = gained, negative = spent)
  "money_currency": "copper/silver/gold",
  "money_details": "How money was gained/lost",
  
  "food_quantity": 0, // Integer (count of food items)
  "food_meals_worth": 0, // Integer (how many meals this provides)
  "food_description": "Description of food acquired/consumed",
  "food_duration": "How long the food will last",
  
  "health_change": 0, // Integer (positive = gained, negative = lost)
  "health_details": "How health changed during encounter"
}
```

### 4. DISCOVERED LOCATIONS
List of all newly discovered places:
```json
[
  {
    "name": "Location name",
    "type": "Market/Inn/Church/Farm/etc.",
    "description": "Brief description of the location",
    "npcs_present": ["NPC1", "NPC2"], // Array of strings, can be empty
    "opportunities": ["Opportunity1", "Opportunity2"] // Array of strings, can be empty
  }
]
```

### 5. NEW NPCS
List of all new characters encountered:
```json
[
  {
    "name": "NPC name",
    "role": "Baker/Guard/Innkeeper/etc.",
    "description": "Physical description and personality",
    "location": "Where they can be found",
    "relationship": "Friendly/Neutral/Hostile/etc.",
    "disposition": "How they feel toward the player"
  }
]
```

### 6. RELATIONSHIP CHANGES
List of all relationship developments:
```json
[
  {
    "npc": "NPC name",
    "change": "Improved/Worsened/Established",
    "details": "What caused the relationship change",
    "current_state": "Brief description of current relationship"
  }
]
```

### 7. QUESTS
List of all potential quests:
```json
[
  {
    "title": "Brief quest title",
    "giver": "Who offered this quest",
    "description": "What needs to be done",
    "location": "Where it takes place",
    "reward": "What was promised as reward",
    "urgency": "Immediate/Soon/Whenever",
    "expires": true/false, // Boolean (lowercase)
    "expiration_condition": "When/how the opportunity expires, if applicable"
  }
]
```

### 8. JOBS
List of all work opportunities:
```json
[
  {
    "title": "Job title",
    "employer": "Who's offering the work",
    "description": "What the work entails",
    "location": "Where the work is done",
    "payment": "Promised compensation",
    "duration": "How long the job lasts",
    "start_time": "When the job begins",
    "recurring": false // Boolean (lowercase)
  }
]
```

### 9. RUMORS
List of all information learned:
```json
[
  {
    "content": "What was learned",
    "source": "Who shared this information",
    "reliability": "How trustworthy it seems",
    "related_to": "What locations/people/events this connects to"
  }
]
```

### 10. TIME PASSAGE
Information about time:
```json
{
  "time_elapsed": "How much time passed during encounter",
  "current_time": "Current time of day",
  "current_day": "Current day/date if relevant"
}
```

## EXTRACTION INSTRUCTIONS

1. If no items exist for a category that expects an array, return an empty array [].
2. For object properties:
   - Use empty strings "" for string properties if information is not mentioned
   - Use 0 for numerical properties if not mentioned 
   - Use false for boolean properties if not mentioned
3. Include ONLY information explicitly stated or strongly implied in the encounter
4. DO NOT invent details not present in the narrative

For each section, provide the JSON object with this format:
### SECTION NAME
```json
[your json here]
```

Remember to provide all 10 sections even if some contain empty arrays or default values."

This approach:
1. Creates separate, flat JSON objects for each category
2. Eliminates nesting to reduce parsing errors
3. Provides clear examples and instructions for each section
4. Flattens the ResourceChange object to avoid nested properties
5. Removes tag changes completely
6. Includes defaults for all property types

