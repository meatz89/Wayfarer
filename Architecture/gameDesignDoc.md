# Wayfarer: A Medieval Life Simulation with Elegant Encounters

## Introduction

*Wayfarer* is a medieval life simulation game that generates deeply personal stories through the elegant interaction of simple, interconnected systems.  We focus on the intimate moments of daily life in a medieval town, eschewing epic quests for the genuine struggles of survival, the gradual building of relationships, and the accumulation of knowledge that transforms a newcomer into an integral part of the community.

Our design philosophy centers on emergent narrative, where meaningful stories arise organically from player choices within a dynamic system, rather than following pre-scripted paths. We draw inspiration from games like *Roadwarden*, *Pentiment*, and the *Kingkiller Chronicle*, but where these games use masterful writing to evoke a sense of grounded realism, *Wayfarer* achieves this through systemic interaction. We've moved beyond fixed action menus and rigid time slots, embracing a more fluid and responsive system of **Encounters** that simulate the challenges and opportunities of medieval life.

## Core Experience: The Encounter System

The heart of *Wayfarer* lies in its **Encounter System**. Every action the player undertakes, whether it's working at the docks, haggling with a merchant, investigating a mystery, or socializing in the tavern, triggers an Encounter. These Encounters are not simply single choices or skill checks, but miniature, dynamic scenarios that play out in stages.


**Narrative Values: The Drivers of Encounters**

Each Encounter is governed by four **Narrative Values:**
You're right. Ranges, even small ones, can introduce ambiguity. We want each point in each encounter value to have a clear, consistent, and easily understandable effect on the next stage's choice generation.  Let's refine the system further to eliminate ranges and ensure each point has a distinct impact.

**Refined Framework:  Emphasis on Single-Point Impact and Strain as a Key Mechanic**

**Encounter Values:**

1.  **Outcome:**
    *   **Magnitude** (0-10): Represents the degree of success or failure.
        *   Each point represents a clearly definable increment of progress or setback.
    *   **Type** (Categorical): Represents the kind of outcome.
        *   Examples: `Neutral`, `Progress`, `Setback`, `Minor Injury`, `Major Injury`, `Minor Resource Gain`, `Major Resource Gain`, `Minor Relationship Gain`, `Major Relationship Gain`, `Minor Relationship Loss`, `Major Relationship Loss`, `Information Gain`, `Reputation Gain`, `Reputation Loss`
    *   **Initialization:**
        *   `Magnitude`: Starts at `3`.
        *   `Type`: Starts at `Neutral`.
    *   **Modification:**
        *   Actions add or subtract a flat value from `Magnitude` (e.g., +2, -1).
        *   Actions can change the `Type`.
    *   **Impact on Player State:**
        *   The `Type` determines the primary consequence for the player's permanent state, as previously defined.
        *   Each point of `Magnitude` has a consistent effect on the intensity of the `Type`'s impact. For example:
            *   `Minor Injury`: Each point of Magnitude could reduce Health by a set amount (e.g., 2 Health per point).
            *   `Minor Resource Gain`: Each point of Magnitude could grant a set amount of resources (e.g., 5 Coins per point).
            *   `Reputation Gain`: Each point of Magnitude might modify the player's Reputation by a set number of points (e.g., 1 Reputation point per point of Magnitude, either at a specific faction or generally).
    *   **Narrative Representation:**  Directly reflects success/failure, driving the narrative.
    *   **Challenge:** Achieving a high `Outcome.Magnitude` with a desirable `Type` is the core challenge.

2.  **Pressure:** (0-10) Represents the risk and strain of the encounter.
    *   **Initialization:**
        *   Base value determined by Location's Danger Level and Encounter Type.
        *   Modified by Player State: Low Health or Low Energy increases initial Pressure.
    *   **Modification:**
        *   Actions add or subtract a flat value from `Pressure` (e.g., +1, -2).
    *   **Impact on Player State:**
        *   Directly affects player state by adding to **Strain**, a temporary value accumulated within an encounter. Each point of Pressure adds one point to Strain
        *   **Strain Effects:**
            *   **Energy Cost Increase**: Every 3 points of Strain increase all energy costs for actions in the encounter by 1. This represents the increasing difficulty of performing tasks under pressure.
            *   **Point Penalties**:
                *   **Strain 3**:  -1 to all social actions (representing difficulty in social interactions when under stress)
                *   **Strain 5**: -1 to all physical actions (representing physical fatigue or difficulty moving quickly)
                *   **Strain 7**: -1 to all focus actions (representing difficulty concentrating or thinking clearly under pressure)
                *   **Strain 9**: -1 to all actions (representing overall exhaustion and severely impaired abilities)
                *   **Strain 10**: The player becomes completely unable to act, effectively ending the encounter and forcing a negative outcome of a type defined by the encounter and the location. This represents a character collapsing from exhaustion, being overwhelmed by stress, or making a critical error due to extreme pressure.
    *   **Narrative Representation:** `Pressure` reflects mounting tension and difficulty. Strain represents the character becoming exhausted and making mistakes under pressure.
    *   **Challenge:** Managing `Pressure` is crucial. Each point of `Pressure` directly contributes to `Strain`, which in turn makes actions more costly and imposes penalties.

3.  **Insight:** (0-10) Represents the player's understanding of the situation.
    *   **Initialization:**
        *   Base value determined by relevant Player Knowledge.
    *   **Modification:**
        *   Actions add or subtract a flat value from `Insight` (e.g., +1, -1).
    *   **Impact on Player State:**
        *   Primarily impacts the current encounter.
        *   Each point of `Insight` unlocks more information about the current encounter or situation. This could manifest as:
            *   Revealing hidden choices.
            *   Modifying the `Outcome.Type.Potential` of actions, making better outcomes possible.
            *   Reducing the `Pressure` increase of certain actions by providing a more efficient or less risky approach.
        *   At certain `Insight` thresholds (e.g., 5 and 8), grants a piece of permanent Knowledge.
    *   **Narrative Representation:**  Represents growing awareness and knowledge.
    *   **Challenge:** Balancing `Insight` with `Outcome` and `Pressure`.

4.  **Resonance:** (0-10) Represents the player's social standing within the encounter.
    *   **Initialization:**
        *   Base value determined by Player's Reputation within the specific context.
    *   **Modification:**
        *   Actions add or subtract a flat value from `Resonance` (e.g., +1, -2).
    *   **Impact on Player State:**
        *   Primarily impacts the current encounter.
        *   Each point of `Resonance` makes social interactions more effective:
            *   Modifies the `Outcome.Magnitude` of social actions. For example, each point of `Resonance` could add +1 to the `Outcome.Magnitude` of a social action.
            *   Unlocks new social choices or modifies existing ones, potentially allowing for more persuasive or influential actions.
        *   At certain `Resonance` thresholds (e.g., 4 and 7), significantly impacts the player's permanent Reputation with the relevant faction or individual (either positively or negatively, depending on the actions taken).
    *   **Narrative Representation:** Reflects social standing and influence.
    *   **Challenge:** Balancing `Resonance` with other objectives.


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
*   **Starting Outcome** is calculated as: `5 + (Player Level - Encounter Difficulty)`. This creates a natural sense of progression, where experienced players will have an easier time with low-difficulty Encounters.

**Choices that Matter:**

Each stage of an Encounter presents the player with 3-4 choices. These choices have deterministic costs and rewards:

*   **Costs:** Primarily Energy costs (Physical, Focus, or Social). High Pressure increases these costs. Choices might also involve trade-offs between Narrative Values (e.g., gaining Outcome but losing Resonance).
*   **Rewards:** Changes to Narrative Values (Outcome, Insight, Resonance, Pressure), resources, knowledge, relationship changes, reputation changes.

**Choice Generation:**

Choices are dynamically generated based on:

*   **Encounter Type:** (e.g., Labor, Trade, Investigate, Mingle, Romance)
*   **Location:** (e.g., Docks, Market, Tavern, Forest) and its Properties (e.g., Public, Private, Crowded, Dark)
*   **World State:** (e.g., Time of Day)
*   **Player State:** (Skills, Inventory, Relationships, Reputation, Health, Stress, Energy)
*   **Current Narrative Values:**

**Choice Types:**

*   **Basic Choices:** Offer straightforward progress in a specific Narrative Value (usually Outcome), typically with an Energy cost. The type of Energy cost is determined by the Encounter's context (e.g., Labor Encounters have mostly Physical Energy costs).
*   **Strategic Choices:** Focus on building up a specific Narrative Value (Insight or Resonance, or reducing Pressure if available) at a lower Energy cost but often with a trade-off in Outcome gain. These choices are crucial for long-term success and managing difficult situations.
*   **Special Choices:** Unlocked by meeting specific requirements related to high Narrative Values, player Skills, Items, Knowledge, or Reputation.

**Special Choice Categories:**

*   **Power Moves (High Pressure):**
    *   **Requirements:** Pressure ≥ 7, relevant Physical Skill at a specific threshold, context-appropriate item equipped (WEAPON for combat, VALUABLE for negotiation, TOOL for investigation, EQUIPMENT for labor, GIFT for social).
    *   **Effects:** Large Outcome gain (+3), Pressure reduction, possible permanent item effects (e.g., damage, consumption, enhancement). These choices represent risky but potentially decisive actions.
*   **Expert Moves (High Insight):**
    *   **Requirements:** Insight ≥ 7, relevant Focus Skill at a specific threshold, specific Knowledge flag (WEAKNESS for combat, LEVERAGE for negotiation, CLUE for investigation, TECHNIQUE for labor, SECRET for social).
    *   **Effects:** Significant Outcome gain (+2), Insight increase, may unlock new Knowledge flags. These choices represent the application of knowledge and expertise.
*   **Social Moves (High Resonance):**
    *   **Requirements:** Resonance ≥ 7, relevant Social Skill at a specific threshold, specific Reputation type (UNBREAKABLE for combat, HONEST for negotiation, SHARP for investigation, RELIABLE for labor, TRUSTED for social).
    *   **Effects:** Reliable Outcome gain (+2), Resonance increase, Reputation reinforcement in the relevant type. These choices leverage the player's social standing and relationships.

**The Elegance of Interconnected Mechanics:**

This system creates a compelling gameplay loop where:

*   **Every choice matters:** Each decision has immediate consequences on Energy and Narrative Values, as well as long-term implications for player stats and progression.
*   **Strategic resource management is crucial:** Players must balance their Energy levels, weighing the risks of depleting a resource against the potential rewards of success. They must also consider the long-term benefits of building Insight and Resonance.
*   **Character progression is deeply intertwined with gameplay:** Skills, Items, Knowledge, and Reputation are not just abstract stats but directly impact the choices available and their outcomes.
*   **Emergent narrative arises naturally:** The dynamic interplay of Narrative Values, context, and player choices creates unique and unpredictable stories.
*   **Deterministic Outcomes:** The system is designed to be completely deterministic. There is no randomness in the choices or their effects. Players can learn the mechanics and make informed decisions based on predictable outcomes.

**Encounter Flow:**

1.  **Initialization:**
    *   Determine Encounter Difficulty and calculate Starting Outcome.
    *   Set Narrative Values: Outcome (as calculated), Insight (0), Resonance (5), Pressure (0).
    *   Determine available Energy types and the distribution of Energy costs for this Encounter based on context (location, task, etc.).

2.  **Stage Generation (Repeat until Outcome reaches 10 or the player is forced to quit):**
    *   **Generate Situation:** A brief narrative description based on context.
    *   **Generate Choices (3-4):**
        *   **1-2 Basic Choices:** Offer Outcome gain with associated Energy costs and potential Narrative Value trade-offs.
        *   **0-1 Strategic Choice:** Focus on increasing Insight or Resonance (or reducing Pressure if the option is available), often at a lower Energy cost but with a trade-off in Outcome gain.
        *   **0-1 Special Choice:** If requirements are met (based on Narrative Values, Skills, Items, Knowledge, Reputation).

3.  **Player Selects Choice:** The player chooses an action, considering the costs and potential rewards.

4.  **Apply Choice Effects:**
    *   **Energy Costs:** Deduct Energy. Apply penalties if Energy is depleted (Health, Stress, or Reputation loss).
    *   **Narrative Value Changes:** Adjust Outcome, Insight, Resonance, and Pressure based on the deterministic outcome of the choice.
    *   **Resonance Bonus:** Apply the Outcome bonus based on the current Resonance value to choices that grant Outcome.
    *   **Pressure Modifier:** Increase Energy costs by +1 if Pressure is 6 or higher.
    *   **Item/Knowledge/Reputation Effects:** Apply any relevant effects from special choices, including unlocking new Knowledge flags or modifying items.

**No Fixed Number of Stages:** An encounter only ends when the player reaches 10 Outcome, or is forced to quit because of low energy, health, reputation or high stress (reaching 0 Health, 10 Stress, or 0 Reputation results in a game over).
