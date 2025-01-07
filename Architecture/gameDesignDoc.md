# Wayfarer: A Medieval Life Simulation with Elegant Encounters

## Introduction

*Wayfarer* is a medieval life simulation game that generates deeply personal stories through the elegant interaction of simple, interconnected systems.  We focus on the intimate moments of daily life in a medieval town, eschewing epic quests for the genuine struggles of survival, the gradual building of relationships, and the accumulation of knowledge that transforms a newcomer into an integral part of the community.

Our design philosophy centers on emergent narrative, where meaningful stories arise organically from player choices within a dynamic system, rather than following pre-scripted paths. We draw inspiration from games like *Roadwarden*, *Pentiment*, and the *Kingkiller Chronicle*, but where these games use masterful writing to evoke a sense of grounded realism, *Wayfarer* achieves this through systemic interaction. We've moved beyond fixed action menus and rigid time slots, embracing a more fluid and responsive system of **Encounters** that simulate the challenges and opportunities of medieval life.

Okay, I understand. Let's integrate this missing information into the design document, focusing on the elegant encounter mechanics and the interplay of Narrative Values.

Here's the updated section:

## Core Experience: The Encounter System

The heart of *Wayfarer* lies in its **Encounter System**. Every action the player undertakes, whether it's working at the docks, haggling with a merchant, investigating a mystery, or socializing in the tavern, triggers an Encounter. These Encounters are not simply single choices or skill checks but miniature, dynamic scenarios that play out in stages.

**Narrative Values: The Drivers of Encounters**

Each Encounter is governed by four **Narrative Values:**

*   **Advantage (0-10):** Represents the player's success and progress within the Encounter. Reaching 10 Advantage signifies a successful outcome.
    *   **Mechanic:**  Starts at 5. Increased through choices that offer Advantage gains. Decreased through choices that involve risks or setbacks, or as a consequence of low Connection.
    *   **High Advantage (8+):** Unlocks powerful choices that can potentially end the encounter quickly or provide bonus rewards.
    *   **Low Advantage (0-2):** Significantly reduces the effectiveness of choices.

*   **Understanding (0-10):** Represents the player's grasp of the situation, unlocking options to reduce Tension and gain insights.
    *   **Mechanic:** Starts at 0. Increased through choices that involve investigation, observation, or careful planning. Counteracts Tension.
    *   **High Understanding (6+):** Unlocks choices that can reduce Tension or reveal hidden information, making the encounter easier.
    *   **Low Understanding (0-2):** Limits options and makes it difficult to manage Tension.

*   **Connection (0-10):** Represents the player's social capital and relationships, influencing choice effectiveness and reputation gains or losses.
    *   **Mechanic:** Starts at 5. Increased through choices that involve building rapport, helping others, or leveraging existing relationships. Decreased through choices that involve betrayal, selfishness, or damaging relationships. Modifies the effectiveness of all choices.
    *   **High Connection (8+):** Choices generate bonus trust or reputation. May unlock unique choices to call in favors.
    *   **Low Connection (0-2):** Choices may result in a loss of trust or reputation. Advantage gains from choices are reduced.

*   **Tension (0-10):** Represents the risk and pressure of the situation, increasing the cost of choices and the potential for negative consequences.
    *   **Mechanic:** Starts at 0. Increased through choices that involve risk, conflict, or urgency. Certain actions or events may automatically increase Tension. Can only be decreased through special choices unlocked by high Understanding.
    *   **High Tension (6+):** Choices that increase Advantage have an added cost, such as draining player Energy or risking Health/Reputation/Stress.
    *   **Low Tension (0-2):** Choices are generally safer and less costly.

**The Interplay of Narrative Values:**

These values are designed to interact with each other in an elegant and dynamic way:

*   **Advantage** is the primary success metric.
*   **Understanding** counteracts **Tension**.
*   **Connection** modifies the effectiveness of all other values and impacts long-term progression (reputation and trust).
*   **Tension** introduces risk and forces difficult choices, especially when Energy is low.

**Energy: The Fuel of Action and the Replacement for Momentum:**

Instead of a Momentum value, players manage three types of Energy:

*   **Physical Energy:** Spent on manual labor, combat, and other physically demanding actions.
*   **Focus Energy:** Spent on investigation, planning, crafting, and mental tasks.
*   **Social Energy:** Spent on social interactions, persuasion, and performance.

Energy is a persistent resource, carrying over between Encounters. Depleting an Energy type doesn't end the Encounter but introduces penalties:

*   **Physical Energy 0:** Choices costing Physical Energy now cost 1 Health.
*   **Focus Energy 0:** Choices costing Focus Energy now cost 1 Stress.
*   **Social Energy 0:** Choices costing Social Energy now cost 1 Reputation.

This creates a compelling risk-reward dynamic, forcing players to push their limits while managing the consequences. Reaching 0 Health, 10 Stress, or 0 Reputation results in a game over, emphasizing the importance of long-term resource management.

**Choices that Matter:**

Each stage of an Encounter presents the player with 3-4 choices. These choices are not simply dialogue options but actions with tangible costs (Energy, potential Narrative Value shifts) and rewards (Narrative Value changes, resources, knowledge, relationship changes).

Choice generation is a dynamic process influenced by:

*   **Encounter Type:** (e.g., Labor, Trade, Investigate, Mingle)
*   **Location:** (e.g., Docks, Market, Tavern, Forest) and its Properties (e.g., Public, Private, Crowded, Dark)
*   **World State:** (e.g., Time of Day)
*   **Player State:** (Skills, Inventory, Relationships, Reputation, Health, Stress, Energy)
*   **Current Narrative Values:**

**Choice Types:**

*   **Basic Choices:** Offer straightforward progress in a specific Narrative Value, typically with an Energy cost. The type of energy is determined by the encounter's context.
*   **Strategic Choices:** Focus on building up a specific Narrative Value (Understanding, Connection, or reducing Tension) at a lower Energy cost, often with a trade-off in another value.
*   **Special Choices:** Unlocked by high Narrative Values in combination with relevant player Skills, Items, Knowledge, or Reputation. These offer significant advantages but often come with higher costs or risks.

**Special Choice Categories:**

*   **Power Moves (High Tension):**
    *   **Requirements:** Tension ≥ 7, relevant Physical Skill at a specific threshold, context-appropriate item equipped (WEAPON for combat, VALUABLE for negotiation, TOOL for investigation, EQUIPMENT for labor, GIFT for social).
    *   **Effects:** Large Advantage gain (+3), Tension reduction, possible permanent item effects (e.g., damage, consumption, enhancement).
*   **Expert Moves (High Understanding):**
    *   **Requirements:** Understanding ≥ 7, relevant Focus Skill at a specific threshold, specific Knowledge flag (WEAKNESS for combat, LEVERAGE for negotiation, CLUE for investigation, TECHNIQUE for labor, SECRET for social).
    *   **Effects:** Significant Advantage gain (+2), Understanding increase, may unlock new Knowledge flags.
*   **Social Moves (High Connection):**
    *   **Requirements:** Connection ≥ 7, relevant Social Skill at a specific threshold, specific Reputation type (UNBREAKABLE for combat, HONEST for negotiation, SHARP for investigation, RELIABLE for labor, TRUSTED for social).
    *   **Effects:** Reliable Advantage gain (+2), Connection increase, Reputation reinforcement in the relevant type.

**Elegant Complexity Through Simple Mechanics:**

This system achieves complexity through the interaction of simple mechanics:

*   **Narrative Values:** Each value has a distinct role and interacts with the others in a clear, understandable way.
*   **Energy Costs:**  Create meaningful choices and force players to manage their resources carefully.
*   **Contextual Modifiers:** Ensure that choices are relevant to the situation and that the game world feels dynamic and responsive.
*   **Special Choices:**  Reward players for long-term progression and strategic decision-making.
*   **No Fixed Number of Stages:** An encounter only ends when the player reaches 10 advantage, or is forced to quit because of low energy, health, reputation or high stress.

**The Elegance of Interconnected Mechanics:**

This system creates a compelling gameplay loop where:

*   **Every choice matters:** Each decision has immediate consequences on Energy and Narrative Values, as well as long-term implications for player stats and progression.
*   **Strategic resource management is crucial:** Players must balance their Energy levels, weighing the risks of depleting a resource against the potential rewards of success.
*   **Character progression is deeply intertwined with gameplay:** Skills, items, knowledge, and reputation are not just abstract stats but directly impact the choices available and their outcomes.
*   **Emergent narrative arises naturally:** The dynamic interplay of Narrative Values, context, and player choices creates unique and unpredictable stories.

You're right, I need to clarify how Advantage, Understanding, Connection, Tension, and Energy work together in a more concrete and detailed way. Let's break down their interactions step-by-step:

**Core Relationship Summary:**

*   **Advantage:** The primary "success" meter. Get it to 10 to win the Encounter.
*   **Understanding:**  Helps manage and reduce Tension. Unlocks special options.
*   **Connection:** Makes all your choices more effective and improves social outcomes (reputation/trust).
*   **Tension:**  Increases the costs and risks of choices, especially when Energy is low.
*   **Energy:**  The fuel for actions. When depleted, penalties are applied to stats (Health, Stress, Reputation).

You are absolutely right, I apologize for the misunderstanding. I was still incorporating elements of randomness and risk that are not part of your design.  Let's lay out the mechanics clearly and deterministically, incorporating your latest points about difficulty, energy reduction, and the specific roles of each Narrative Value.

**Deterministic Encounter Mechanics**

**1. Encounter Difficulty and Starting Advantage:**

*   **Encounter Difficulty:** Each Encounter has a difficulty level (a numerical value). This could be influenced by the location, the specific task, or the characters involved.
*   **Player Level:** Represents the player's overall progress and capabilities. (This could be a simple level based on completed Encounters, or a more complex value derived from skills, reputation, and knowledge).
*   **Starting Advantage:**  `Starting Advantage = 5 + (Player Level - Encounter Difficulty)`
    *   If `Player Level >= Encounter Difficulty`, the player starts with an Advantage bonus.
    *   If `Player Level < Encounter Difficulty`, the player starts with an Advantage penalty (potentially even below 0, but likely capped at a minimum of 0).
*   **Maximum Advantage:** The encounter ends in success when Advantage reaches 10.

**2. Advantage Gain:**

*   **Optimal Choices:** Each stage of the Encounter offers choices that increase Advantage.
*   **Cost of Advantage:** Optimal choices (those that increase Advantage) often have associated costs:
    *   **Energy Cost:**  The primary cost of most choices.
    *   **Narrative Value Trade-offs:** Some choices might increase Advantage while decreasing Connection or increasing Tension.
    *   **Resource Consumption:** Some choices might require specific items or resources.
*   **No Randomness:** Advantage gain is always fixed and predetermined based on the choice and the current Connection value.

**3. Energy Reduction and Costs:**

*   **Energy Types:** Physical, Focus, Social.
*   **Energy Reduction:** Every choice made during an Encounter reduces a specific type of Energy. The type and amount of Energy reduced is determined by the choice itself.
*   **Location Influence:** Locations influence the distribution of Energy costs among the choices.
    *   **LABOR:** Primarily Physical Energy costs, some Focus, rare Social.
    *   **NEGOTIATION:** Primarily Social Energy costs, some Focus, rare Physical.
    *   **INVESTIGATION:** Primarily Focus Energy costs, some Social, rare Physical.
    *   **SOCIAL:** Primarily Social Energy costs, some Focus, rare Physical.
*   **Tension Modifier:** High Tension (6 or higher) increases Energy costs by +1.
*   **Depleted Energy Penalties:**
    *   **Physical Energy (0):**  Choices that would cost Physical Energy now cost 1 Health.
    *   **Focus Energy (0):** Choices that would cost Focus Energy now cost 1 Stress.
    *   **Social Energy (0):** Choices that would cost Social Energy now cost 1 Reputation.

**4. Understanding's Role:**

*   **Starts at 0.**
*   **Gaining Understanding:**  Increased through specific choices, often those that involve investigation, observation, or learning.
*   **Tension Reduction:**  High Understanding (6 or higher) unlocks special choices that reduce Tension. These choices typically have a Focus Energy cost.
*   **No Direct Advantage Gain:** Understanding does not directly increase Advantage. Its primary role is managing Tension and unlocking options.

**5. Connection's Role:**

*   **Starts at 5.**
*   **Gaining Connection:** Increased through choices that build rapport, strengthen relationships, or leverage existing social connections.
*   **Losing Connection:** Decreased through choices that damage relationships or involve betrayal or selfish actions.
*   **Advantage Bonus:**
    *   **Connection 8-10:**  +2 Advantage bonus to *all* choices that grant Advantage.
    *   **Connection 5-7:** +1 Advantage bonus to *all* choices that grant Advantage.
    *   **Connection 0-4:** +0 Advantage bonus.
*   **Reputation/Trust:** High Connection also adds bonus Reputation or increases trust with relevant characters or factions. Low connection might decrease them.

**6. Tension's Role:**

*   **Starts at 0.**
*   **Gaining Tension:** Increased by many choices, particularly those that involve:
    *   **Risk:** Choices with potentially negative consequences if they fail (even though in this system, failure is deterministic based on stats).
    *   **Conflict:** Choices that involve direct confrontation or opposition.
    *   **Urgency:** Choices that need to be made quickly.
*   **Exemptions:** Some choices, particularly those focused on building Understanding or Connection, might be exempt from increasing Tension.
*   **Effects of Tension:**
    *   **Increased Energy Costs:** At Tension 6 or higher, all Energy costs are increased by +1.
    *   **Unlocks Power Moves:** At Tension 7 or higher, and if other requirements are met, special "Power Moves" become available.

**7. Special Choices:**

*   **Power Moves (High Tension):**
    *   **Requirements:** Tension ≥ 7, relevant Physical Skill at a specific threshold, context-appropriate item equipped (WEAPON for combat, VALUABLE for negotiation, TOOL for investigation, etc.).
    *   **Effects:** Large Advantage gain (+3), Tension reduction, possible permanent item effects (e.g., damage, consumption, enhancement).
*   **Expert Moves (High Understanding):**
    *   **Requirements:** Understanding ≥ 7, relevant Focus Skill at a specific threshold, specific Knowledge flag (WEAKNESS for combat, LEVERAGE for negotiation, CLUE for investigation, etc.).
    *   **Effects:** Significant Advantage gain (+2), Understanding increase, may unlock new Knowledge flags.
*   **Social Moves (High Connection):**
    *   **Requirements:** Connection ≥ 7, relevant Social Skill at a specific threshold, specific Reputation type (UNBREAKABLE for combat, HONEST for negotiation, SHARP for investigation, etc.).
    *   **Effects:** Reliable Advantage gain (+2), Connection increase, Reputation reinforcement in the relevant type.

**Encounter Flow (Revised):**

1.  **Initialization:**
    *   Determine Encounter Difficulty and calculate Starting Advantage.
    *   Set Narrative Values: Advantage (as calculated), Understanding (0), Connection (5), Tension (0).
    *   Determine available Energy types and the distribution of Energy costs for this Encounter based on context (location, task, etc.).

2.  **Stage Generation (Repeat until Advantage reaches 10 or the player is forced to quit):**
    *   **Generate Situation:** A brief narrative description based on context.
    *   **Generate Choices (3-4):**
        *   **1-2 Basic Choices:**  Offer Advantage gain with associated Energy costs and potential Narrative Value trade-offs.
        *   **0-1 Strategic Choice:**  Focus on increasing Understanding or Connection (or reducing Tension if the option is available), often at a lower Energy cost but with a trade-off in Advantage gain.
        *   **0-1 Special Choice:**  If requirements are met (based on Narrative Values, Skills, Items, Knowledge, Reputation).

3.  **Player Selects Choice:**

4.  **Apply Choice Effects:**
    *   **Energy Costs:** Deduct Energy. Apply penalties if Energy is depleted (Health, Stress, or Reputation loss).
    *   **Narrative Value Changes:** Adjust Advantage, Understanding, Connection, and Tension based on the choice.
    *   **Connection Bonus:** Apply the Advantage bonus based on the current Connection value to choices that grant Advantage.
    *   **Tension Modifier:** Increase Energy costs by +1 if Tension is 6 or higher.
    *   **Item/Knowledge/Reputation Effects:** Apply any relevant effects from special choices.

**Example:**

**Encounter:** Negotiating a price for a rare item with a merchant (Difficulty: 5)

**Player:**

*   **Player Level:** 6
*   **Skills:** Social (High), Focus (Medium)
*   **Reputation:** Honest (High)
*   **Knowledge:** None relevant
*   **Inventory:** A valuable gem (could be used for a Power Move if Tension gets high enough)
*   **Energy:** Physical (5), Focus (3), Social (7)
*   **Stats:** Health (10), Stress (1), Reputation (8)

**Initial Narrative Values:**

*   **Advantage:** 6 (5 + (6 - 5))
*   **Understanding:** 0
*   **Connection:** 5
*   **Tension:** 0

**Stage 1:**

*   **Situation:** "The merchant eyes you shrewdly, quoting a high price for the rare item."
*   **Choices:**
    *   **"Haggle" (Basic, Social):** Cost: 2 Social Energy. Effect: +1 Advantage, +1 Tension.
    *   **"Inquire about the Item" (Strategic, Focus):** Cost: 1 Focus Energy. Effect: +2 Understanding.
    *   **"Build Rapport" (Strategic, Social):** Cost: 1 Social Energy. Effect: +1 Connection.

**(Player chooses "Haggle". Social Energy is now 5, Advantage is 7 (6+1 bonus from Connection), Tension is 1)**

**Stage 2:**

*   **Narrative Values:** Advantage: 7, Understanding: 0, Connection: 5, Tension: 1
*   **Situation:** "The merchant is unmoved by your initial offer, but seems open to further discussion."
*   **Choices:**
    *   **"Press for a Lower Price" (Basic, Social):** Cost: 2 Social Energy. Effect: +1 Advantage, +2 Tension.
    *   **"Observe the Merchant" (Strategic, Focus):** Cost: 1 Focus Energy. Effect: +2 Understanding.
    *   **(Special - Connection 7+, Social Skill High, Reputation: Honest):** "Use Your Reputation" (Social Move)
        *   **Cost:** 2 Social Energy
        *   **Effect:** +4 Advantage (2 base + 2 from high connection), +1 Connection, Reinforce Honest Reputation.

**(Player chooses "Use Your Reputation". Social Energy is now 3, Advantage is 10, Connection is 6, Tension is 1. Honest Reputation is reinforced.)**

**Encounter Ends:** The player successfully negotiated the price, reaching 10 Advantage. They receive the item and potentially other rewards based on the final Narrative Values.

**Key Points:**

*   **Deterministic Outcomes:**  All choices have clearly defined costs and effects.
*   **Meaningful Choices:** Players must balance Advantage gain with Energy management and Narrative Value considerations.
*   **Strategic Depth:**  Understanding and Connection are crucial for managing Tension and maximizing Advantage.
*   **Character Progression:** Skills, items, knowledge, and reputation provide tangible benefits within Encounters.

This detailed explanation should provide a solid foundation for your procedural Encounter system. By carefully balancing the interplay of these mechanics, you can create a game that is both strategically challenging and narratively rich. Remember to playtest and iterate on these rules to find the optimal balance for your game.
