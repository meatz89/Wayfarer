# Wayfarer Card-Based Action System: Strategic Daily Planning

I love this direction! A card-based action system perfectly solves our mechanical diversity problem while maintaining the text-based nature of the game. Let me develop this concept into a complete system:

## Core Mechanics: The Daily Deck

### Daily Planning Phase
- Each morning, players construct their "day deck" based on their Action Point capacity
- Example: Player with 5 AP selects exactly 5 cards to form their day deck
- Cards have three primary types: Physical, Social, and Intellectual
- Players freely choose the type distribution (e.g., 3 Physical, 1 Social, 1 Intellectual)

### Card Exhaustion System
- Each action requires playing a card of the appropriate type
- Once used, the card is "exhausted" and unavailable until refreshed
- Similar actions require cards of the same type (preventing spamming)
- When all cards of a type are exhausted, related actions become unavailable
- Exhausted cards remain exhausted across days until properly refreshed

### Strategic Refresh System
- Cards don't automatically refresh when a new day begins
- Specific refreshing actions restore specific card types:
  - **Physical Refresh**: Sleeping, eating hearty meals, bathing
  - **Social Refresh**: Relaxing conversations, entertainment, drinking
  - **Intellectual Refresh**: Reading, meditation, quiet contemplation
- Each refresh action has specific costs, locations, and effectiveness
- Players must strategically allocate time between action and refreshing

### Card Effects & Specialization
- Each card can provide bonuses when used for specific actions
- Basic cards have no special effects
- Specialized cards give bonuses to related checks (+1 to Rapport, +1 to Analysis)
- Advanced cards might have unique properties (doesn't exhaust on simple actions)
- Players acquire better cards through experience and relationships

## Integration With Existing Systems

### Integration with Affliction Systems
- **Exhaustion** reduces Physical card refresh rates
- **Mental Strain** reduces Intellectual card refresh rates
- **Isolation** reduces Social card refresh rates
- **Hunger** limits maximum day deck size

### Integration with Approach Systems
- Physical cards naturally support Dominance/Precision approaches
- Social cards naturally support Rapport approaches
- Intellectual cards naturally support Analysis approaches

### Integration with Location Spots
- Different locations offer bonuses to specific card types:
  - Workshop enhances Physical card effects
  - Tavern enhances Social card effects
  - Library enhances Intellectual card effects

## First 10 Minutes Implementation

### Minute 1-2: Morning Arrival & Card Introduction

```
As dawn breaks over the Dusty Flagon, you consider how to approach the day ahead.

Your action capacity: 5 cards

Available cards:
□ Standard Physical (3) - For labor, movement, and manual tasks
□ Standard Social (3) - For conversation and relationship building
□ Standard Intellectual (3) - For observation, analysis, and planning

Select 5 cards for today:
```

The player constructs their first day deck, choosing how many of each type to include.

### Minute 3-4: First Card Usage

```
You enter the common room. What will you do?

□ Speak with Bertram, the innkeeper [Requires Social card]
□ Examine the room and its occupants [Requires Intellectual card]
□ Help move some furniture [Requires Physical card]
```

When they select an action:

```
You decide to speak with the innkeeper.
[Using 1 Social card - You have 1 Social card remaining]

The conversation is brief but informative. Bertram mentions...
```

### Minute 5-6: Card Exhaustion Experience

After using their last Social card:

```
You've exhausted all your Social cards for today. Social interactions requiring focus or energy are unavailable until you refresh these cards.

Available actions:
□ Look around the room carefully [Requires Intellectual card]
□ Help move some furniture [Requires Physical card]
□ Relax with a drink [Refreshes 1 Social card, costs 2 coins]
```

### Minute 7-8: Multi-Task Challenge

```
Bertram mentions he needs help with a delivery upstairs and then updating his ledger.

□ Carry crates to the storage room [Requires Physical card]
□ Review and update the ledger [Requires Intellectual card]

This sequence of tasks will require both Physical and Intellectual cards.
```

### Minute 9-10: Refresh Mechanics Introduction

```
After a busy morning, you feel depleted. You consider ways to recover:

□ Take a proper meal [Refreshes 1 Physical card, costs 3 coins]
□ Rest quietly in your room [Refreshes 1 Intellectual card, advances time]
□ Join the conversation at the bar [Refreshes 1 Social card, potential for information]

Note: Refreshed cards will be available for tomorrow's day deck.
```

## Card Progression System

### Starting Cards
- **Standard Physical**: No special effects
- **Standard Social**: No special effects 
- **Standard Intellectual**: No special effects

### Improved Cards (Acquired through experience)
- **Vigorous Physical**: +1 to Dominance checks
- **Graceful Physical**: +1 to Precision checks
- **Charming Social**: +1 to Rapport with commoners
- **Formal Social**: +1 to Rapport with authority figures
- **Analytical Intellectual**: +1 to Analysis for investigation
- **Scholarly Intellectual**: +1 to Analysis for research

### Specialized Cards (Acquired through relationships/quests)
- **Enduring Physical**: Doesn't exhaust on minor physical tasks
- **Diplomatic Social**: Can be used as any social approach
- **Insightful Intellectual**: Sometimes reveals hidden information

## Strategic Depth Elements

### Limited Substitution
Some specialized cards allow limited substitution:
- **Practical Intellectual**: Can be used as a Physical card at -1 effectiveness
- **Intimidating Physical**: Can be used as a Social card for Dominance approaches
- **Observant Social**: Can be used as an Intellectual card for people-focused analysis

### Card Synergies
Special bonuses when certain cards are used in sequence:
- Using **Analytical Intellectual** before **Diplomatic Social** provides +2 to persuasion
- Using **Observant Social** before **Vigorous Physical** provides better targeting

### Advanced Refresh Options
- **Quality meal**: Refreshes 1 card of any type (expensive)
- **Deep sleep**: Refreshes all Physical cards (takes substantial time)
- **Engaging conversation**: Refreshes 2 Social cards (requires good relationship)
- **Deep study**: Refreshes all Intellectual cards (requires proper materials)

## Reflection

In slay the spire, in each turn of combat the player has energy and a hand of cards drawn from his deck. The three types of cards are attack, defend and skill cards. Each card has a energy cost, 0 to 3. Some cards give additional energy for the same turn and some even let the player draw additional cards. But draw too many or too high cost cards and you don't have enough energy to play them this turn, forcing them to be discarded. Have too much energy and too few cards and you are forced to discard the leftover energy. Have too many defend cards on your hand, more than needed to block the enemy attack, and you can play them and it even adds to your block, but the unused block will still be discarded at the end of the turn, giving you nothing in return. On the other hand some cards allow you to keep your block for the next turn. This is a very elegant system. 

In our game instead of attack, defend and skill cards we could define them as physical, intellectual and social cards. If we remove the isolation resource from our game and rethink our afflictions and resources, we can say that adding physical cards to your deck costs your energy amount depending on the card, while adding intellectual cards to your deck costs focus/concentration. Social cards are special in that they have no direct costs associated to them, making them kind of special and useful for when the player is in a bad spot resource wise.

The player has a resource called action points that gate how many cards he can hold in his hand for one day, and has the additional limiting factors of energy and concentration, making a double requirement just like in slay the spire. He can optimize either AP or energy/concentration but he must find the right balance. In addition the cards he even has to put in his hand are also limited. If he has less cards than AP, he cannot use his full AP in a day. In addition, some cards are better than others, but also have a higher AP or energy or concentration cost, making the decision which card to pick critical. Some cards may even have a cost of 0.

Resources like energy and concentration are not trivial to refresh. There isn't always a place you can go to to just refresh them, that would be trivial to abuse. The player should always feel starved for options to restore these resources so that he will really need to think about what cards to put into his deck.

By making progress in the game the player may find opportunity to acquire better cards, level up to get more AP, reach skill levels to get higher energy or higher concentration and so on. Each piece of equipment the player puts on also gives him either a special card or a free action of a specific type. For example good clothing may give an additional social action per day. Or a tool like a hammer might give a special physical card that also adds +1 to labor skill checks.

All this resource management already provides interesting decision space, but I want to really integrate this system into all facets of my game. So I want to clearly define how my skill check system works. This whole system is only really interesting if the actions that the player can do each day are not totally static and predictable. The real magic happens if the player cannot be absolutely certain what the day will bring and this uncertainty informs his choices which cards and equipment to choose in the morning when starting the day. But because it is unrealistic to generate infinite content that changes each day of play, which would also be unrealistic, we can instead solve this problem through an intricate skill check system. The location spots the player can visit are mostly static and serve as a way for predictable player progression. The actions each location spot defines also have some unchanging properties like what kind of action type they represent (what card is needed), but they also have a variable component. Each day and time window, the location properties of each location spot change semi randomly. Some days, the tavern is crowded with customers while other days, it is mostly empty. This is not predictable by the player so he might add lots of physical cards into his deck in the hopes of making good coin by helping in the kitchen or serving customers, but might instead find himself in a near empty tavern and having to reconsider how to best use the deck of cards he is now stuck with. The way to implement this is through location properties affecting action yields. Bustling tavern makes the serve drinks action give +3 coins per action while a sparsely populated tavern only gives +1, not worth the card and energy spent.

In addition to this we also have the skill checks. Better cards give bonus +1 to specific skill checks. Each action the player does also gives XP in a skill. Each level in a skill gives an additional +1 to skill checks. So what are skill checks and how are they used? Actions are not simply executed by playing the card and that's it. Depending on the location properties, each action has different set of approaches the player can choose between which each have different skill requirement and different type of reward. To be able to pick an approach the player must pass the skill check which is entirely deterministic and includes all modifiers from location property, equipment and card and skill level bonus. If the modified value is higher than the skill requirement the player can choose the approach and gain the pre calculated reward. He still must decide for one of the eligible approaches. There is always one default approach that has no skill requirements but does not yield great rewards, usually a bad trade for the card. While the skill checks are deterministic, the required skill level is semi random and takes into account the difficulty of the location and the location properties. For example when the location spot has the crowded property, intellectual approaches have a heightened skill requirement of +3 because the environment is hard to concentrate in.

This system also allows the diversification of the play style for our 6 professions. Each profession gets a different initial card deck to play around with. While the warrior might get professions get 3 physical and 2 intellectual and 1 social card in the beginning, other professions have different initial decks.
