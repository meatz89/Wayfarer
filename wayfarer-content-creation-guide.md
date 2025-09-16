# Wayfarer Content Creation Guide

## Table of Contents
1. [Overview](#overview)
2. [Package Structure](#package-structure)
3. [Content Types](#content-types)
4. [Card Creation](#card-creation)
5. [NPC Creation](#npc-creation)
6. [Location Creation](#location-creation)
7. [Stranger Encounters](#stranger-encounters)
8. [Observation System](#observation-system)
9. [Request System](#request-system)
10. [Skeleton System](#skeleton-system)
11. [Content Validation](#content-validation)
12. [Best Practices](#best-practices)

---

## Overview

Wayfarer uses a modular, package-based content system that supports:
- **Lazy Loading**: Content can reference items that don't exist yet
- **Skeleton Placeholders**: Missing content gets generic placeholders
- **Hot Loading**: Content can be added without restarts
- **AI Generation**: Future support for procedurally generated content

### Directory Structure

```
Content/
├── Core/                # Essential game content
│   └── core_game.json   # Starting deck, basic NPCs
├── Expansions/          # Additional content packs
│   └── elena_story.json # Story-specific content
├── Generated/           # AI-generated packages
│   └── (future)
└── TestPackages/        # Development testing
    └── test_npcs.json
```

### Load Priority

1. **Core** packages (priority 0) - Essential gameplay
2. **Base** content (priority 1) - Standard game content
3. **Expansions** (priority 2) - Additional stories
4. **Generated** (priority 3) - AI-created content

---

## Package Structure

Every content package follows this structure:

```json
{
  "packageId": "unique_package_identifier",
  "metadata": {
    "name": "Human-Readable Package Name",
    "version": "1.0.0",
    "author": "Creator Name",
    "description": "What this package adds",
    "dependencies": ["core_game"],
    "timestamp": "2025-01-20T10:00:00Z"
  },
  "content": {
    "cards": [...],
    "npcs": [...],
    "locations": [...],
    "spots": [...],
    "routes": [...],
    "strangers": [...],
    "observations": [...],
    "requests": [...]
  }
}
```

### Metadata Fields

- **packageId**: Unique identifier, lowercase with underscores
- **dependencies**: Other packages that must load first
- **version**: Semantic versioning (major.minor.patch)

---

## Content Types

### Core Concepts

Every piece of content follows these principles:

1. **Unique IDs**: Every item needs a globally unique identifier
2. **References**: Can reference other content (even if it doesn't exist)
3. **Mechanical Completeness**: All gameplay properties must be specified
4. **Narrative Flexibility**: Descriptions and flavor text are optional

---

## Card Creation

Cards are the fundamental unit of conversation gameplay. Each card must specify its mechanical properties and stat binding.

### Card Structure

```json
{
  "id": "thoughtful_analysis",
  "name": "Thoughtful Analysis",
  "text": "Let me think through this systematically...",
  "stat": "Insight",
  "focus": 2,
  "difficulty": "Medium",
  "persistence": "Thought",
  "successType": "Threading",
  "failureType": "None",
  "exhaustType": "None"
}
```

### Stat Bindings

Every card MUST be bound to exactly ONE stat:

- **Insight**: Analytical, observational, systematic thinking
- **Rapport**: Empathetic, supportive, emotional connection
- **Authority**: Commanding, decisive, forceful personality
- **Commerce**: Transactional, optimizing, deal-making
- **Cunning**: Indirect, subtle, hidden meanings

### Persistence Types

Determines when cards leave your hand:

- **Thought**: Stays until played (survives LISTEN)
- **Impulse**: Removed after SPEAK if unplayed
- **Opening**: Removed after LISTEN if unplayed

### Success Types

What happens when the card succeeds:

- **Rapport**: Changes connection (+magnitude based on difficulty)
- **Threading**: Draw cards (magnitude based on difficulty)
- **Atmospheric-[Type]**: Sets atmosphere (Patient/Focused/Receptive/Volatile)
- **Focusing**: Restore focus points
- **Advancing**: Move connection state forward
- **Promising**: Reorder queue + gain rapport
- **None**: No mechanical effect

### Failure Types

What happens on failure (beyond forced LISTEN):

- **Disrupting**: Discard all Focus 3+ cards
- **Overreach**: Discard entire hand
- **Backfire**: Negative rapport (doubled by Devoted)
- **None**: Just forces LISTEN

### Exhaust Types

Effects when discarded unplayed:

- **Threading**: Draw cards
- **Focusing**: Restore focus
- **Regret**: Lose rapport
- **None**: No effect

### Difficulty and Magnitude

Base success rates determine magnitude:

| Difficulty | Base Success | Magnitude |
|-----------|-------------|-----------|
| Very Easy | 85% | 1 |
| Easy | 75% | 2 |
| Medium | 60% | 2 |
| Hard | 50% | 3 |
| Very Hard | 40% | 4 |

### Example Cards by Stat

#### Insight Card
```json
{
  "id": "pattern_recognition",
  "name": "I See the Pattern",
  "text": "The real issue here is...",
  "stat": "Insight",
  "focus": 3,
  "difficulty": "Medium",
  "persistence": "Thought",
  "successType": "Threading",
  "failureType": "None",
  "exhaustType": "None"
}
```

#### Rapport Card
```json
{
  "id": "genuine_empathy",
  "name": "I Understand",
  "text": "That must be really difficult for you.",
  "stat": "Rapport",
  "focus": 1,
  "difficulty": "Easy",
  "persistence": "Thought",
  "successType": "Rapport",
  "failureType": "None",
  "exhaustType": "None"
}
```

#### Authority Card
```json
{
  "id": "take_charge",
  "name": "Listen Carefully",
  "text": "This is what needs to happen.",
  "stat": "Authority",
  "focus": 4,
  "difficulty": "Hard",
  "persistence": "Impulse",
  "successType": "Rapport",
  "failureType": "Backfire",
  "exhaustType": "Regret"
}
```

---

## NPC Creation

NPCs are persistent characters with relationships, personalities, and unique content.

### NPC Structure

```json
{
  "id": "marcus_merchant",
  "name": "Marcus",
  "profession": "Merchant",
  "personality": "Mercantile",
  "locationId": "market_square",
  "spotId": "merchant_stall",
  "conversationLevel": 2,
  "startingState": "Neutral",
  "tokenType": "Commerce",
  "signatureCards": {
    "1": "marcus_bargain",
    "3": "trade_knowledge",
    "6": "commercial_trust",
    "10": "marcus_favor",
    "15": "master_trader"
  },
  "requests": ["marcus_delivery", "urgent_package"],
  "exchanges": ["buy_bread", "sell_goods"]
}
```

### Personality Types

Each personality has a unique conversation rule:

| Personality | Rule | Token Preference |
|------------|------|-----------------|
| **Devoted** | Rapport losses doubled | Trust |
| **Mercantile** | Highest focus card +30% success | Commerce |
| **Proud** | Cards must ascend in focus | Status |
| **Cunning** | Same focus as previous -2 rapport | Shadow |
| **Steadfast** | Rapport changes capped at ±2 | Any |

### Connection States

NPCs progress through states based on conversation flow:

1. **Disconnected** (3 focus, 3 cards) - No relationship
2. **Guarded** (4 focus, 3 cards) - Cautious
3. **Neutral** (5 focus, 4 cards) - Standard
4. **Receptive** (5 focus, 4 cards) - Friendly
5. **Trusting** (6 focus, 5 cards) - Close bond

### Conversation Difficulty

- **Level 1**: +10% success all cards, 1 XP per card
- **Level 2**: Normal rates, 2 XP per card
- **Level 3**: -10% success all cards, 3 XP per card

### Signature Cards

Each NPC has 5 unique cards unlocked at token thresholds:

```json
{
  "id": "marcus_bargain",
  "name": "Marcus's Bargain",
  "text": "Remember our last deal? This one's even better.",
  "stat": "Commerce",
  "focus": 2,
  "difficulty": "Hard",
  "persistence": "Thought",
  "successType": "Rapport",
  "failureType": "None",
  "exhaustType": "None",
  "tokenThreshold": 1
}
```

---

## Location Creation

Locations are explorable areas with spots, NPCs, and investigation opportunities.

### Location Structure

```json
{
  "id": "market_square",
  "name": "Market Square",
  "description": "The bustling heart of commerce",
  "tier": 1,
  "locationType": "Hub",
  "maxFamiliarity": 3,
  "travelHubSpotId": "central_fountain",
  "spots": ["central_fountain", "merchant_row", "guard_post"]
}
```

### Location Spots

Each location contains multiple spots with time-based properties:

```json
{
  "id": "central_fountain",
  "locationId": "market_square",
  "name": "Central Fountain",
  "description": "A gathering place for locals",
  "properties": {
    "morning": ["quiet"],
    "midday": ["busy"],
    "afternoon": ["busy"],
    "evening": ["closing"]
  },
  "canInvestigate": true,
  "npcs": ["marcus_merchant"]
}
```

### Investigation Mechanics

Investigation costs 1 segment and increases familiarity:

```json
{
  "locationId": "market_square",
  "spotId": "central_fountain",
  "familiarityGain": {
    "quiet": 2,
    "busy": 1,
    "default": 1
  }
}
```

---

## Stranger Encounters

Strangers are unnamed NPCs for resource generation and stat grinding.

### Stranger Structure

```json
{
  "id": "market_vendor_morning",
  "name": "Bread Vendor",
  "location": "market_square",
  "spot": "merchant_row",
  "level": 1,
  "personality": "Steadfast",
  "availability": {
    "morning": true,
    "midday": false,
    "afternoon": false,
    "evening": false
  },
  "conversations": {
    "friendly_chat": {
      "basic": {"threshold": 5, "reward": "2 coins"},
      "enhanced": {"threshold": 10, "reward": "4 coins + bread"},
      "premium": {"threshold": 15, "reward": "6 coins + medicine"}
    },
    "trade_negotiation": {
      "basic": {"threshold": 5, "reward": "silk_scraps"},
      "enhanced": {"threshold": 10, "reward": "spice_bundle"},
      "premium": {"threshold": 15, "reward": "rare_wines"}
    }
  }
}
```

### Stranger Rules

- **Never give tokens** - Only named NPCs build relationships
- **One conversation per block** - Prevents infinite grinding
- **XP scales with level** - Level 3 gives 3 XP per card
- **No signature cards** - Use standard personality rules only

---

## Observation System

Observations are discoveries that add cards to NPC decks.

### Observation Structure

```json
{
  "id": "safe_passage_knowledge",
  "locationId": "market_square",
  "familiarityRequired": 1,
  "priorObservationRequired": null,
  "observationCard": {
    "name": "Safe Passage Knowledge",
    "text": "I know a route that avoids patrols.",
    "targetNpcId": "elena",
    "stat": "Insight",
    "focus": 0,
    "difficulty": "VeryEasy",
    "effect": "AdvanceConnectionState"
  }
}
```

### Observation Chain

Observations can require prior discoveries:

1. **First**: Requires familiarity 1+
2. **Second**: Requires familiarity 2+ AND first observation
3. **Third**: Requires familiarity 3+ AND second observation

---

## Request System

Requests are bundled conversation goals with multiple thresholds.

### Request Structure

```json
{
  "id": "elena_letter_request",
  "npcId": "elena",
  "name": "Deliver My Letter",
  "description": "Elena needs help with an urgent letter",
  "goals": {
    "basic": {
      "threshold": 5,
      "package": {"weight": 1, "deadline": "5PM"},
      "reward": {"trust": 0, "coins": 0}
    },
    "enhanced": {
      "threshold": 10,
      "package": {"weight": 2, "deadline": "4PM"},
      "reward": {"trust": 1, "coins": 0}
    },
    "premium": {
      "threshold": 15,
      "package": {"weight": 3, "deadline": "3PM"},
      "reward": {"trust": 2, "coins": 5}
    }
  }
}
```

### Request Cards

Each goal tier has a corresponding card:

```json
{
  "id": "elena_basic_request",
  "requestId": "elena_letter_request",
  "tier": "basic",
  "focusRequired": 5,
  "text": "I'll deliver your letter.",
  "successRate": 100
}
```

---

## Skeleton System

The skeleton system handles missing references gracefully.

### How It Works

1. **Detection**: Content references something that doesn't exist
2. **Generation**: System creates generic placeholder
3. **Registry**: Skeleton tracked for later resolution
4. **Resolution**: Real content replaces skeleton when loaded
5. **Preservation**: Progress (familiarity, cards) transfers

### Skeleton Examples

When "mysterious_tower" location doesn't exist:

```json
{
  "id": "mysterious_tower",
  "name": "Unknown Location #47",
  "tier": 1,
  "maxFamiliarity": 3,
  "spots": ["mysterious_tower_main"],
  "isSkeleton": true,
  "skeletonSource": "npc_wizard_reference"
}
```

### Benefits

- **No crashes** from missing content
- **Packages load in any order**
- **AI can reference anything**
- **Progress preserved** when resolved

---

## Content Validation

### Mandatory Checks

#### Cards
- [ ] Every card has exactly ONE stat binding
- [ ] All categorical properties specified
- [ ] Focus value between 0-5
- [ ] Difficulty from valid enum

#### NPCs
- [ ] Unique ID across all packages
- [ ] Valid personality type
- [ ] Location and spot exist (or create skeleton)
- [ ] 5 signature cards at correct thresholds
- [ ] Token type specified

#### Locations
- [ ] Travel hub spot exists
- [ ] Max familiarity 1-5
- [ ] At least one spot defined

#### Strangers
- [ ] Never give tokens
- [ ] Level 1-3 specified
- [ ] Availability schedule defined
- [ ] Rewards scale with level

### Testing Checklist

1. **Load Test**: Package loads without errors
2. **Reference Test**: Missing references create skeletons
3. **Play Test**: Content is mechanically playable
4. **Balance Test**: Rewards match difficulty
5. **Integration Test**: Works with existing content

---

## Best Practices

### Content Organization

1. **One Feature Per Package**: Keep packages focused
2. **Clear Dependencies**: List all required packages
3. **Consistent Naming**: Use lowercase_underscore for IDs
4. **Version Everything**: Track changes with semantic versioning

### Narrative Design

1. **Show Personality**: Let NPC personality show in card text
2. **Maintain Voice**: Keep character voices consistent
3. **Create Connections**: Reference other NPCs and locations
4. **Build Mystery**: Leave hooks for future content

### Mechanical Design

1. **Risk vs Reward**: Higher difficulty = better rewards
2. **Meaningful Choices**: Every stat should feel different
3. **Clear Progression**: Players see growth path
4. **Avoid Dead Ends**: Always provide alternatives

### Stat Design Guidelines

#### Insight
- Focuses on understanding and analysis
- Cards often have Threading effects
- Good for investigation and puzzles
- Unlocks analytical paths

#### Rapport
- Builds emotional connections
- Direct rapport building effects
- Safe but slower progression
- Opens social solutions

#### Authority
- High risk, high reward
- Often has Backfire effects
- Can bypass obstacles
- Commands respect

#### Commerce
- Finds mutual benefit
- Often sets Patient atmosphere
- Reduces costs everywhere
- Opens trade routes

#### Cunning
- Indirect approaches
- May have unusual effects
- Discovers hidden paths
- Avoids direct conflict

### Package Examples

#### Core Package
Contains starting deck, basic NPCs, tutorial locations

#### Story Package
Complete narrative arc with NPCs, locations, and quests

#### Mechanics Package
New cards, items, or systems without narrative

#### Region Package
New area with locations, strangers, and local color

---

## Advanced Topics

### Cross-Package References

Packages can enhance content from other packages:

```json
{
  "id": "elena_enhanced",
  "extends": "elena",
  "additionalSignatureCards": {
    "20": "elena_ultimate_trust"
  }
}
```

### Conditional Content

Content that only loads if dependencies met:

```json
{
  "conditional": {
    "requires": ["noble_expansion"],
    "content": {...}
  }
}
```

### Dynamic Generation Hooks

Markers for AI content generation:

```json
{
  "generationHints": {
    "theme": "merchant_district",
    "difficulty": "medium",
    "personality_preference": "Mercantile"
  }
}
```

---

## Conclusion

Creating content for Wayfarer requires understanding the interplay between:
- **Mechanical Systems**: Stats, cards, personalities
- **Narrative Elements**: Characters, locations, stories
- **Package Architecture**: Loading, dependencies, skeletons

Focus on creating mechanically complete content that enriches the game world while maintaining the core philosophy: conversations as character progression in a world where every choice has weight.