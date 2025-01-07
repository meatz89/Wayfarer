# Wayfarer: A Medieval Life Simulation with Elegant Encounters

## Introduction

*Wayfarer* is a medieval life simulation game that generates deeply personal stories through the elegant interaction of simple, interconnected systems.  We focus on the intimate moments of daily life in a medieval town, eschewing epic quests for the genuine struggles of survival, the gradual building of relationships, and the accumulation of knowledge that transforms a newcomer into an integral part of the community.

Our design philosophy centers on emergent narrative, where meaningful stories arise organically from player choices within a dynamic system, rather than following pre-scripted paths. We draw inspiration from games like *Roadwarden*, *Pentiment*, and the *Kingkiller Chronicle*, but where these games use masterful writing to evoke a sense of grounded realism, *Wayfarer* achieves this through systemic interaction. We've moved beyond fixed action menus and rigid time slots, embracing a more fluid and responsive system of **Encounters** that simulate the challenges and opportunities of medieval life.

## Core Experience: The Encounter System

The heart of *Wayfarer* lies in its **Encounter System**. Every action the player undertakes, whether it's working at the docks, haggling with a merchant, investigating a mystery, or socializing in the tavern, triggers an Encounter. These Encounters are not simply single choices or skill checks, but miniature, dynamic scenarios that play out in stages.


**Narrative Values: The Drivers of Encounters**

Each Encounter is governed by four **Narrative Values:**

  *   **Advantage (0-10):** Represents the player's success and progress within the Encounter.
    *   **Mechanic:**
        *   Starts at `5 + (Player Level - Encounter Difficulty)`.
        *   Increased through choices that offer Advantage gains.
        *   Decreased through choices that involve risks or setbacks (though this is rare in the current deterministic design).
        *   Reaching 10 Advantage signifies a successful outcome and ends the Encounter.
    *   **High Advantage (8+):** Unlocks powerful choices that can potentially end the encounter quickly or provide bonus rewards.
    *   **Low Advantage (0-2):** Significantly reduces the effectiveness of choices.

*   **Understanding (0-10):** Represents the player's grasp of the situation.
    *   **Mechanic:**
        *   Starts at 0.
        *   Increased through choices that involve investigation, observation, careful planning, or learning.
        *   Counteracts Tension (High Understanding unlocks Tension-reducing choices).
    *   **High Understanding (6+):** Unlocks choices that can reduce Tension or reveal hidden information.
    *   **Low Understanding (0-2):** Limits options and makes it difficult to manage Tension.

*   **Connection (0-10):** Represents the player's social capital and relationships within the current context.
    *   **Mechanic:**
        *   Starts at 5.
        *   Increased through choices that involve building rapport, helping others, or leveraging existing relationships.
        *   Decreased through choices that involve betrayal, selfishness, or damaging relationships.
        *   Directly modifies the Advantage gained from choices:
            *   **Connection 8-10:** +2 Advantage bonus to all choices that grant Advantage.
            *   **Connection 5-7:** +1 Advantage bonus to all choices that grant Advantage.
            *   **Connection 0-4:** +0 Advantage bonus.
    *   **High Connection (8+):**  Choices generate bonus trust or reputation. May unlock unique choices to call in favors.
    *   **Low Connection (0-2):** Choices may result in a loss of trust or reputation.

*   **Tension (0-10):** Represents the risk, pressure, and urgency of the situation.
    *   **Mechanic:**
        *   Starts at 0.
        *   Increased through choices that involve risk, conflict, or urgency.
        *   Can only be decreased through special choices unlocked by high Understanding.
    *   **High Tension (6+):** Increases Energy costs of all choices by +1. Also unlocks "Power Moves" under specific conditions.
    *   **Low Tension (0-2):** Choices are generally safer and less costly.

**Energy: The Fuel of Action and the Replacement for Momentum:**

Instead of a Momentum value, players manage three types of Energy:

*   **Physical Energy:** Spent on manual labor, combat, and other physically demanding actions.
*   **Focus Energy:** Spent on investigation, planning, crafting, and mental tasks.
*   **Social Energy:** Spent on social interactions, persuasion, and performance.

Energy is a persistent resource, carrying over between Encounters. Depleting an Energy type doesn't end the Encounter but introduces penalties:

*   **Physical Energy 0:** Choices costing Physical Energy now cost 1 Health.
*   **Focus Energy 0:** Choices costing Focus Energy now cost 1 Stress.
*   **Social Energy 0:** Choices costing Social Energy now cost 1 Reputation.

Reaching 0 Health, 10 Stress, or 0 Reputation results in a game over.

**Encounter Difficulty and Player Level:**

*   Each Encounter has a **Difficulty** level (a numerical value).
*   The **Player Level** represents the player's overall progress.
*   **Starting Advantage** is calculated as: `5 + (Player Level - Encounter Difficulty)`. This creates a natural sense of progression, where experienced players will have an easier time with low-difficulty Encounters.

**Choices that Matter:**

Each stage of an Encounter presents the player with 3-4 choices. These choices have deterministic costs and rewards:

*   **Costs:** Primarily Energy costs (Physical, Focus, or Social). High Tension increases these costs. Choices might also involve trade-offs between Narrative Values (e.g., gaining Advantage but losing Connection).
*   **Rewards:** Changes to Narrative Values (Advantage, Understanding, Connection, Tension), resources, knowledge, relationship changes, reputation changes.

**Choice Generation:**

Choices are dynamically generated based on:

*   **Encounter Type:** (e.g., Labor, Trade, Investigate, Mingle, Romance)
*   **Location:** (e.g., Docks, Market, Tavern, Forest) and its Properties (e.g., Public, Private, Crowded, Dark)
*   **World State:** (e.g., Time of Day)
*   **Player State:** (Skills, Inventory, Relationships, Reputation, Health, Stress, Energy)
*   **Current Narrative Values:**

**Choice Types:**

*   **Basic Choices:** Offer straightforward progress in a specific Narrative Value (usually Advantage), typically with an Energy cost. The type of Energy cost is determined by the Encounter's context (e.g., Labor Encounters have mostly Physical Energy costs).
*   **Strategic Choices:** Focus on building up a specific Narrative Value (Understanding or Connection, or reducing Tension if available) at a lower Energy cost but often with a trade-off in Advantage gain. These choices are crucial for long-term success and managing difficult situations.
*   **Special Choices:** Unlocked by meeting specific requirements related to high Narrative Values, player Skills, Items, Knowledge, or Reputation.

**Special Choice Categories:**

*   **Power Moves (High Tension):**
    *   **Requirements:** Tension ≥ 7, relevant Physical Skill at a specific threshold, context-appropriate item equipped (WEAPON for combat, VALUABLE for negotiation, TOOL for investigation, EQUIPMENT for labor, GIFT for social).
    *   **Effects:** Large Advantage gain (+3), Tension reduction, possible permanent item effects (e.g., damage, consumption, enhancement). These choices represent risky but potentially decisive actions.
*   **Expert Moves (High Understanding):**
    *   **Requirements:** Understanding ≥ 7, relevant Focus Skill at a specific threshold, specific Knowledge flag (WEAKNESS for combat, LEVERAGE for negotiation, CLUE for investigation, TECHNIQUE for labor, SECRET for social).
    *   **Effects:** Significant Advantage gain (+2), Understanding increase, may unlock new Knowledge flags. These choices represent the application of knowledge and expertise.
*   **Social Moves (High Connection):**
    *   **Requirements:** Connection ≥ 7, relevant Social Skill at a specific threshold, specific Reputation type (UNBREAKABLE for combat, HONEST for negotiation, SHARP for investigation, RELIABLE for labor, TRUSTED for social).
    *   **Effects:** Reliable Advantage gain (+2), Connection increase, Reputation reinforcement in the relevant type. These choices leverage the player's social standing and relationships.

**The Elegance of Interconnected Mechanics:**

This system creates a compelling gameplay loop where:

*   **Every choice matters:** Each decision has immediate consequences on Energy and Narrative Values, as well as long-term implications for player stats and progression.
*   **Strategic resource management is crucial:** Players must balance their Energy levels, weighing the risks of depleting a resource against the potential rewards of success. They must also consider the long-term benefits of building Understanding and Connection.
*   **Character progression is deeply intertwined with gameplay:** Skills, Items, Knowledge, and Reputation are not just abstract stats but directly impact the choices available and their outcomes.
*   **Emergent narrative arises naturally:** The dynamic interplay of Narrative Values, context, and player choices creates unique and unpredictable stories.
*   **Deterministic Outcomes:** The system is designed to be completely deterministic. There is no randomness in the choices or their effects. Players can learn the mechanics and make informed decisions based on predictable outcomes.

**Encounter Flow:**

1.  **Initialization:**
    *   Determine Encounter Difficulty and calculate Starting Advantage.
    *   Set Narrative Values: Advantage (as calculated), Understanding (0), Connection (5), Tension (0).
    *   Determine available Energy types and the distribution of Energy costs for this Encounter based on context (location, task, etc.).

2.  **Stage Generation (Repeat until Advantage reaches 10 or the player is forced to quit):**
    *   **Generate Situation:** A brief narrative description based on context.
    *   **Generate Choices (3-4):**
        *   **1-2 Basic Choices:** Offer Advantage gain with associated Energy costs and potential Narrative Value trade-offs.
        *   **0-1 Strategic Choice:** Focus on increasing Understanding or Connection (or reducing Tension if the option is available), often at a lower Energy cost but with a trade-off in Advantage gain.
        *   **0-1 Special Choice:** If requirements are met (based on Narrative Values, Skills, Items, Knowledge, Reputation).

3.  **Player Selects Choice:** The player chooses an action, considering the costs and potential rewards.

4.  **Apply Choice Effects:**
    *   **Energy Costs:** Deduct Energy. Apply penalties if Energy is depleted (Health, Stress, or Reputation loss).
    *   **Narrative Value Changes:** Adjust Advantage, Understanding, Connection, and Tension based on the deterministic outcome of the choice.
    *   **Connection Bonus:** Apply the Advantage bonus based on the current Connection value to choices that grant Advantage.
    *   **Tension Modifier:** Increase Energy costs by +1 if Tension is 6 or higher.
    *   **Item/Knowledge/Reputation Effects:** Apply any relevant effects from special choices, including unlocking new Knowledge flags or modifying items.

**No Fixed Number of Stages:** An encounter only ends when the player reaches 10 advantage, or is forced to quit because of low energy, health, reputation or high stress (reaching 0 Health, 10 Stress, or 0 Reputation results in a game over).
