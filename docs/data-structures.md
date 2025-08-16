{
  "enums": {
    "TimeBlocks": ["Morning", "Afternoon", "Evening", "Night"],
    "NPCPersonalities": ["Devoted", "Mercantile", "Proud", "Cunning", "Steadfast"],
    "EmotionalStates": ["Desperate", "Anxious", "Calculating", "Neutral", "Hostile"],
    "RelationshipTypes": ["Trust", "Commerce", "Status", "Shadow"],
    "LetterTypes": ["Trust", "Commerce", "Status", "Shadow", "AccessPermit", "Introduction", "Information", "Payment"],
    "LetterStakes": ["PersonalSafety", "Reputation", "Wealth", "Secret", "Political", "Family"],
    "CardCategories": ["Basic", "Relationship", "Crisis", "Negative", "Special"],
    "CardEffects": ["AddComfort", "AddToken", "ReduceDifficulty", "ExhaustCard", "GenerateLetter", "CreateObligation"],
    "LocationDistricts": ["CommonQuarter", "MerchantQuarter", "NobleDistrict", "Castle", "Docks", "Temple", "Outskirts"],
    "LocationSpots": ["Tavern", "Market", "Square", "Shop", "Home", "Gate", "Alley", "Garden", "Court", "Chapel"],
    "TransportTypes": ["Walking", "Cart", "Carriage", "Horse", "Boat"],
    "RouteStates": ["Locked", "Available", "Discovered"],
    "ObligationTypes": ["Priority", "Deadline", "NoRefusal", "Payment"],
    "QueuePositions": [1, 2, 3, 4, 5, 6, 7, 8],
    "DifficultyLevels": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
  },
  
  "entities": {
    "NPC": {
      "id": "string",
      "name": "string",
      "personality": "NPCPersonalities",
      "basePatience": "number (3-10)",
      "availableTimeBlocks": ["TimeBlocks"],
      "homeLocation": {
        "district": "LocationDistricts",
        "spot": "LocationSpots"
      },
      "schedule": [
        {
          "timeBlock": "TimeBlocks",
          "location": {
            "district": "LocationDistricts",
            "spot": "LocationSpots"
          },
          "probability": "number (0-1)"
        }
      ],
      "conversationDeck": {
        "deckId": "string",
        "maxSize": "number (20)",
        "minSize": "number (10)"
      },
      "unlockRequirements": {
        "introductionLetter": "boolean",
        "relationshipMinimum": {
          "type": "RelationshipTypes",
          "value": "number"
        },
        "storyEvent": "string"
      }
    },
    
    "ConversationCard": {
      "id": "string",
      "name": "string",
      "category": "CardCategories",
      "difficulty": "DifficultyLevels",
      "patienceCost": "number (0-5)",
      "requirements": {
        "relationshipMinimum": [
          {
            "type": "RelationshipTypes",
            "value": "number"
          }
        ],
        "comfortMinimum": "number",
        "previousCardPlayed": "string",
        "npcState": "EmotionalStates",
        "locationDistrict": "LocationDistricts"
      },
      "outcomes": {
        "success": {
          "comfortChange": "number",
          "tokenChange": {
            "type": "RelationshipTypes",
            "value": "number"
          },
          "addCardToDeck": "string",
          "generateLetter": "boolean",
          "createObligation": "ObligationTypes",
          "specialEffect": "string"
        },
        "neutral": {
          "comfortChange": "number"
        },
        "failure": {
          "comfortChange": "number",
          "addNegativeCard": "string",
          "tokenChange": {
            "type": "RelationshipTypes",
            "value": "number"
          }
        }
      }
    },
    
    "Letter": {
      "id": "string",
      "sender": "string (NPC id)",
      "recipient": "string (NPC id)",
      "type": "LetterTypes",
      "stakes": "LetterStakes",
      "tier": "number (1-3)",
      "weight": "number (1-3)",
      "deadline": {
        "hours": "number",
        "turnsRemaining": "number"
      },
      "rewards": {
        "coins": "number",
        "tokenType": "RelationshipTypes",
        "tokenAmount": "number",
        "deckReward": {
          "type": ["AddCard", "UpgradeCard", "RemoveNegative"],
          "cardId": "string"
        },
        "specialReward": {
          "unlockRoute": "string",
          "unlockNPC": "string",
          "information": "string"
        }
      },
      "queueBehavior": {
        "basePosition": "QueuePositions",
        "leverageModifiers": [
          {
            "condition": {
              "relationshipType": "RelationshipTypes",
              "threshold": "number",
              "comparison": ["greater", "less", "equal"]
            },
            "positionModifier": "number"
          }
        ]
      }
    },
    
    "Location": {
      "district": "LocationDistricts",
      "spots": [
        {
          "id": "string",
          "name": "LocationSpots",
          "availableActions": [
            {
              "type": ["Observe", "Rest", "Shop", "Special"],
              "cost": {
                "attention": "number",
                "coins": "number"
              },
              "effect": "string"
            }
          ],
          "npcSpawnPoints": [
            {
              "npcId": "string",
              "timeBlock": "TimeBlocks",
              "probability": "number"
            }
          ]
        }
      ],
      "accessRequirements": {
        "alwaysAccessible": "boolean",
        "relationshipGate": {
          "type": "RelationshipTypes",
          "totalRequired": "number"
        },
        "permitRequired": "string",
        "storyGate": "string"
      }
    },
    
    "Route": {
      "id": "string",
      "from": {
        "district": "LocationDistricts",
        "spot": "LocationSpots"
      },
      "to": {
        "district": "LocationDistricts",
        "spot": "LocationSpots"
      },
      "transportType": "TransportTypes",
      "state": "RouteStates",
      "travelTime": {
        "minutes": "number",
        "turns": "number"
      },
      "cost": {
        "coins": "number",
        "attention": "number"
      },
      "unlockRequirements": {
        "permitLetter": "boolean",
        "relationshipMinimum": {
          "type": "RelationshipTypes",
          "value": "number"
        },
        "discoveryAction": "boolean"
      },
      "hazards": [
        {
          "type": ["Delay", "Toll", "Encounter"],
          "probability": "number",
          "effect": "string"
        }
      ]
    },
    
    "StandingObligation": {
      "id": "string",
      "npcId": "string",
      "type": "ObligationTypes",
      "createdDay": "number",
      "effect": {
        "queuePosition": {
          "override": "boolean",
          "position": "QueuePositions"
        },
        "deadlineModifier": {
          "strict": "boolean",
          "bonusHours": "number"
        },
        "refusalBlocked": "boolean",
        "paymentRequired": {
          "coins": "number",
          "frequency": ["Once", "PerLetter", "Daily"]
        }
      },
      "breakingPenalty": {
        "tokenType": "RelationshipTypes",
        "tokenLoss": "number",
        "addNegativeCard": "string",
        "npcBecomes": "EmotionalStates"
      }
    },
    
    "Queue": {
      "slots": [
        {
          "position": "QueuePositions",
          "letter": "Letter | null",
          "locked": "boolean",
          "properties": {
            "canDeliver": "boolean",
            "decayRate": "number",
            "specialEffect": "string"
          }
        }
      ],
      "totalWeight": "number",
      "maxWeight": "number (12)",
      "displacementRules": {
        "tokenBurnPerPosition": "number (1)",
        "tokenTypeBurned": "RelationshipTypes",
        "exemptions": [
          {
            "condition": "Shadow >= 5",
            "effect": "No token burn"
          }
        ]
      }
    },
    
    "PlayerRelationship": {
      "npcId": "string",
      "tokens": {
        "trust": "number (-5 to 10)",
        "commerce": "number (-5 to 10)",
        "status": "number (-5 to 10)",
        "shadow": "number (-5 to 10)"
      },
      "activeObligation": "StandingObligation | null",
      "letterHistory": [
        {
          "letterId": "string",
          "delivered": "boolean",
          "onTime": "boolean",
          "day": "number"
        }
      ],
      "unlockedCards": ["string"],
      "negativeCards": ["string"]
    },
    
    "GameState": {
      "currentDay": "number (1-30)",
      "currentTimeBlock": "TimeBlocks",
      "currentTime": {
        "hour": "number (0-23)",
        "minute": "number (0-59)"
      },
      "playerLocation": {
        "district": "LocationDistricts",
        "spot": "LocationSpots"
      },
      "resources": {
        "attention": {
          "current": "number",
          "maximum": "number",
          "refreshedThisBlock": "boolean"
        },
        "coins": "number"
      },
      "queue": "Queue",
      "relationships": ["PlayerRelationship"],
      "unlockedRoutes": ["string"],
      "unlockedNPCs": ["string"],
      "activeLetters": ["Letter"],
      "completedStorylines": ["string"],
      "globalModifiers": [
        {
          "type": "string",
          "effect": "string",
          "duration": "number"
        }
      ]
    },
    
    "ConversationState": {
      "npcId": "string",
      "currentPatience": "number",
      "startingPatience": "number",
      "currentComfort": "number",
      "cardsInHand": ["ConversationCard"],
      "cardsPlayed": ["ConversationCard"],
      "availableCards": ["ConversationCard"],
      "roundNumber": "number",
      "letterUnlocked": "boolean",
      "activeModifiers": [
        {
          "type": ["DifficultyReduction", "CostReduction", "ComfortBonus"],
          "value": "number",
          "source": "RelationshipTypes"
        }
      ]
    },
    
    "AIContext": {
      "npcProfile": {
        "id": "string",
        "personality": "NPCPersonalities",
        "currentState": "EmotionalStates",
        "relationships": {
          "trust": "number",
          "commerce": "number",
          "status": "number",
          "shadow": "number"
        },
        "dominantRelationship": "RelationshipTypes",
        "activeLetter": "Letter | null",
        "currentNeed": "LetterStakes | null"
      },
      "conversationContext": {
        "location": {
          "district": "LocationDistricts",
          "spot": "LocationSpots"
        },
        "timeOfDay": "TimeBlocks",
        "comfortLevel": "number",
        "lastCardPlayed": "ConversationCard",
        "outcome": ["Success", "Neutral", "Failure"]
      },
      "narrativePrompts": {
        "bodyLanguage": ["Nervous", "Relaxed", "Eager", "Suspicious", "Desperate"],
        "toneOfVoice": ["Pleading", "Businesslike", "Formal", "Whispered", "Friendly"],
        "environmentalDetails": ["Crowded", "Quiet", "Tense", "Festive", "Dangerous"]
      }
    }
  },
  
  "content_categories": {
    "starter_decks": {
      "description": "Initial card configurations for each personality type",
      "count": 5,
      "modifiable": true
    },
    "letter_templates": {
      "description": "Base letter configurations by type and tier",
      "count": 36,
      "modifiable": true
    },
    "route_network": {
      "description": "Complete map of routes between locations",
      "count": 50,
      "modifiable": false
    },
    "npc_database": {
      "description": "All NPCs with personalities and schedules",
      "count": 20,
      "modifiable": true
    },
    "card_library": {
      "description": "All possible conversation cards",
      "count": 100,
      "modifiable": true
    },
    "obligation_templates": {
      "description": "Types of promises and their effects",
      "count": 10,
      "modifiable": false
    },
    "storyline_chains": {
      "description": "Multi-letter narrative sequences",
      "count": 15,
      "modifiable": true
    },
    "ending_conditions": {
      "description": "Relationship patterns that trigger endings",
      "count": 8,
      "modifiable": false
    }
  },
  
  "validation_rules": {
    "relationships": {
      "min": -5,
      "max": 10,
      "startingMax": 3
    },
    "queue": {
      "maxSlots": 8,
      "maxWeight": 12,
      "deliveryPosition": 1
    },
    "patience": {
      "baseMin": 3,
      "baseMax": 10,
      "modifierMax": 5
    },
    "comfort": {
      "startingValue": 0,
      "maxValue": 20
    },
    "attention": {
      "morning": 5,
      "afternoon": 5,
      "evening": 3,
      "night": 2
    },
    "deadlines": {
      "minHours": 2,
      "maxHours": 48
    },
    "deckSize": {
      "min": 10,
      "max": 20
    }
  }
}