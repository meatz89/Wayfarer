Wayfarer POC: Core Mechanics Specification for The Dusty Flagon
Section 1: Core Loop & Energy (AP) System Definition
1.1. Introduction to the Core Loop
The fundamental gameplay experience within the Dusty Flagon inn revolves around a cyclical process of resource management and decision-making under mounting pressure. Each significant time block, notionally considered a 'day', begins with the player character possessing a finite pool of Action Points (AP). This pool represents the character's capacity for deliberate effort and engagement within the inn's environment.

Players expend AP to perform various actions: exploring the inn's physical spaces (Explore Spot), interacting with its inhabitants (Talk), attempting to recover (Rest), or directly addressing their deteriorating physical and mental state (Manage Domain). As AP is spent and actions are taken, time implicitly passes. This passage of time is intrinsically linked to the passive increase of the four core 'Domain' stats, representing Hunger, Exhaustion, Mental Strain, and Isolation.

The primary objective for the player is twofold: manage these rising Domains to stave off debilitating penalties and potentially achieve specific narrative or character goals, all before succumbing to the cumulative effects of decay or exhausting their available options within the inn. This core loop inherently emphasizes the careful budgeting of AP, the strategic management of time, and the constant balancing act required to keep the Domains in check within the confines of a single, static location. The design intentionally creates an environment where every choice has a cost, and inaction leads to inevitable decline.

1.2. Action Point (AP) Economy
Action Points (AP) serve as the central currency governing player agency within a given time block (e.g., a 'day'). This system quantifies the character's energy, focus, and ability to undertake meaningful tasks.

Daily Budget: The character starts each day with a baseline AP budget. A suggested initial value for this pool is 10 AP. This figure is a critical tuning parameter, directly influencing the pace of gameplay and the number of significant actions a player can take before needing to Rest. A lower budget increases pressure and forces harder choices, while a higher budget allows for more exploration and interaction per cycle.

AP Costs: Core actions consume AP, reflecting the effort or time involved. Establishing clear base costs is essential for player planning and system balance. Initial baseline costs are proposed below, subject to playtesting and iteration:

Rest: Consumes AP (e.g., 2 AP). The cost might be fixed, but the outcome (AP restored, Domain increase) can vary. Making Rest cost AP reinforces the idea that even recovery requires effort and time.
Talk (NPC): Base cost (e.g., 1 AP). This cost could potentially increase for more involved, persuasive, or emotionally taxing conversations, reflecting greater investment.
Explore Spot: Base cost (e.g., 2 AP). Exploring requires more significant effort and time than a simple conversation. Costs could scale upwards for accessing particularly hidden, dangerous, or complex areas within the inn.
Use Domain Management Action: Costs vary depending on the specific action and its effectiveness (e.g., Eat Rations = 1 AP, Seek Reassurance = 1 AP, Meditate = 2 AP). These costs must be carefully balanced against the amount of Domain reduction they provide.
These costs form the backbone of the player's tactical decision-making process. The relative costs encourage players to weigh the value of information gathering (Talk), environmental interaction (Explore), immediate need satisfaction (Manage Domain), and long-term sustainability (Rest).

Table 1: Base Action Point Costs

Action Name	Base AP Cost	Potential Modifiers	Notes
Rest	2 AP	Skill effects, Domain penalties (e.g., high Exhaustion)	Sole source of AP restoration; triggers passive Domain increases.
Talk (Basic)	1 AP	Skill effects (e.g., negotiation), NPC disposition	Cost may increase for deeper/persuasive/difficult interactions.
Explore Spot (Basic)	2 AP	Skill effects (e.g., perception), Domain penalties	Cost may increase for complex/hidden/dangerous spots.
Manage Domain: Eat Rations	1 AP	Skill effects (e.g., resourcefulness)	Reduces Hunger; effectiveness may vary based on ration quality.
Manage Domain: Seek Reassurance	1 AP	Skill effects, NPC disposition	Reduces Isolation/Mental Strain; requires willing NPC.
Manage Domain: Meditate	2 AP	Skill effects, Archetype influence	Reduces Mental Strain; requires quiet space, higher AP cost for focus.
Other Domain Actions	Variable	Skill/Archetype/Spot/NPC specific	Costs defined based on action complexity and effectiveness.

In Google Sheets exportieren
This table provides a foundational reference for the AP economy. The inclusion of potential modifiers highlights key areas for introducing depth through character skills, Domain penalties, and situational factors, allowing for dynamic adjustments to the core costs based on player progression and current state.

1.3. Rest Mechanic
The Rest action holds a unique and critical position within the core loop. It is defined as the exclusive method for replenishing the character's daily AP budget. This design choice elevates Rest from a simple convenience to a mandatory, recurring strategic decision.

Primary Function & Restoration: Upon successfully executing the Rest action (at the cost of AP), the character's AP pool is typically restored to its maximum daily value (e.g., back to 10 AP). This signifies the start of a new cycle of action potential.

Limitations & Trade-offs: The strategic depth of Resting emerges from its inherent costs and trade-offs. Firstly, Resting consumes AP itself, meaning the player must budget for recovery. Secondly, and crucially, Resting signifies the passage of a significant block of time. During this time, the passive increase mechanics for the Domains (detailed in Section 2.2) continue unabated. Therefore, every act of Resting, while necessary for regaining AP, inevitably leads to a worsening of the character's underlying condition (Hunger increases, Exhaustion might paradoxically increase if rest is poor, Mental Strain and Isolation can worsen depending on circumstances).

This transforms the Rest mechanic into a core tension point. The player constantly weighs the immediate need for AP against the guaranteed increase in Domain pressure. Resting too often accelerates the decline; resting too infrequently risks running out of AP at a critical moment. Further complexity can be introduced through potential diminishing returns for frequent resting or specific penalties if attempting to Rest while certain Domains (like critical Hunger or severe Mental Strain) are dangerously high, potentially making the Rest less effective or even harmful. The act of recovery becomes a calculated risk, a vulnerable period where the character trades immediate capability for future pressure.

1.4. Skill Integration
Character skills, potentially derived from Archetype/Vigor choices or earned through progression, provide a mechanism for players to specialize and mitigate the pressures of the AP economy and Domain management. Skills interact directly with the AP system, offering avenues for optimization and efficiency.

Mechanisms & Examples: Skills can modify the AP system in several ways:

Cost Reduction (Efficiency): Certain skills can reduce the AP cost of specific actions. For example, a Skilled Negotiator trait might reduce the AP cost of Talk actions, particularly persuasive ones. A Cat Burglar background might reduce the AP cost of Explore Spot actions involving stealth or bypassing obstacles.
Increased AP Pool (Endurance): Skills related to stamina or resilience could grant a permanent increase to the character's base daily AP pool, allowing for more actions per cycle.
Improved Rest Efficiency (Meditation, Resilience): Skills could enhance the Rest action itself. This might manifest as restoring the full AP budget for a lower AP cost, or, critically, reducing the amount of passive Domain increase that occurs during the Rest period.
Domain Management Efficiency (Resourcefulness, Empathy): Skills could reduce the AP cost of specific Domain management actions (e.g., Resourcefulness making Eat Rations cost 0 AP) or increase their effectiveness (e.g., Empathy making Seek Reassurance reduce more Isolation).
The existence of skills that specifically target the efficiency of the Rest mechanic carries significant strategic weight. Because Rest is the sole source of AP restoration and inherently increases Domain pressure, any skill that makes Resting 'cheaper'—either in direct AP cost or, more potently, in the amount of associated Domain increase—directly addresses the core bottleneck of the gameplay loop. Such skills offer a powerful way to alleviate the fundamental pressure cycle. Consequently, these skills should likely be positioned as significant progression rewards, tied to difficult choices, or linked intrinsically to specific Archetype/Vigor paths, reflecting their substantial impact on the player's ability to cope with the escalating challenges of the Dusty Flagon. They represent a fundamental shift in the character's capacity for endurance.

Section 2: The Four Domains of Decay: Mechanics & Management
2.1. Introduction to Domains
The four Domains—Hunger, Exhaustion, Mental Strain, and Isolation—represent the core pillars of the character's deteriorating state within the oppressive environment of the Dusty Flagon. They function as status meters tracking the gradual decay of physical well-being, mental fortitude, and social connection. These Domains are not merely passive indicators; they are the primary drivers of pressure, actively influencing gameplay and forcing the player to engage in constant management to avoid escalating consequences.

Functionally, they operate as interconnected "needs" or "stressors." Left unmanaged, they increase passively and through specific triggers, eventually crossing critical thresholds that impose penalties on the player's capabilities. Active management requires spending valuable AP and utilizing resources or interactions discovered within the inn. Their interplay and the struggle to keep them balanced form the central challenge of survival.

2.2. Passive Increase Mechanics
The relentless pressure of the Domains stems primarily from their tendency to increase passively over time. This mechanic ensures that inaction is inherently detrimental and that the passage of time always carries a cost.

Core Driver & Rate: The fundamental driver of passive increase is the passage of time, which is intrinsically linked to the expenditure of AP and the execution of certain actions, particularly Rest. A specific rate of increase should be defined for each Domain per unit of time or AP spent. These rates should differ to create varied rhythms of pressure. For example:

Hunger: Increases steadily with time (e.g., +1 per 4 AP spent or per 'mealtime' block passed).
Exhaustion: Increases more rapidly with exertion (e.g., +1 per 2 AP spent on Explore or strenuous actions) and potentially during ineffective Rest.
Mental Strain: Increases significantly during Rest (reflecting nightmares or anxieties in downtime), or following stressful events/discoveries (e.g., +1 per Rest cycle, +5 after witnessing something disturbing).
Isolation: Increases based on longer cycles of time without meaningful positive social interaction (e.g., +1 per 'day' passed without a successful Talk action aimed at connection).
Establishing distinct rates and primary triggers for each Domain prevents them from feeling like a single, homogenous "decay" meter. Hunger might be a slow burn, while Exhaustion spikes with activity, and Mental Strain or Isolation are tied more closely to specific events or lack thereof. This forces the player into a dynamic prioritization puzzle – sometimes Hunger demands immediate attention, while at other times, managing looming Isolation or recovering from Exhaustion takes precedence. This prevents a static optimal strategy and keeps the management challenge fresh.

Triggers: Beyond passive accumulation, specific events, actions, or environmental factors can trigger additional, immediate increases in Domain levels. Examples include:

Finding disturbing evidence (+Mental Strain).
A failed social check leading to rejection (+Isolation, +Mental Strain).
Consuming spoiled food (+Hunger, potentially).
Experiencing a supernatural or unsettling event within the inn (+Mental Strain, +Exhaustion).
These triggers add unpredictability and reinforce the connection between the narrative/exploration layer and the core mechanics.

2.3. Thresholds & Penalties
To give the Domains teeth and create the sense of escalating pressure, each Domain possesses defined thresholds. Crossing these thresholds imposes increasingly severe, often persistent, penalties on the player character, directly impacting their ability to function and make choices.

Mechanism: Each Domain track (e.g., scaled 0-100) includes several critical thresholds (e.g., 25, 50, 75, 100). When a Domain's value crosses a threshold, the associated penalty is immediately applied and typically remains active until the Domain level is reduced below that threshold again.

Penalty Types: Penalties should be designed to meaningfully hinder the player and interact with other systems:

AP Penalties: Increased AP costs for actions (e.g., "Too hungry to focus: +1 AP cost for Explore"), reduced base daily AP ("Exhaustion headache: Daily AP reduced by 2"), slower AP regeneration during Rest.
Action Penalties: Certain actions might become unavailable (e.g., "Too exhausted to Explore the cellar"), success chances for actions might decrease, or new negative-outcome actions might appear.
Cognitive/Narrative Penalties: Higher levels of Mental Strain or Isolation could limit available dialogue options to reflect panic, despair, or paranoia. Information received might be distorted or misinterpreted. In a direct nod to the Sir Brante style, high Domain levels could force the player into choices reflecting their compromised state, potentially leading to negative consequences aligned with their current decay.
Domain Interaction Penalties: High levels in one Domain might exacerbate the increase rate or impact of another. For example, severe Exhaustion could make the character more susceptible to Mental Strain increases, or high Hunger could worsen Exhaustion gain.
Catastrophic Failure: Reaching the maximum level (e.g., 100) in a Domain could trigger a critical failure state: collapse, forced detrimental narrative outcomes, significant setbacks, or potentially a game over, depending on the desired lethality for the POC.
Table 2: Domain Thresholds & Penalties (Example)

Domain	Threshold	Penalty Description	Type
Hunger	50	+1 AP cost for Explore actions.	AP Penalty
Hunger	75	Cannot perform strenuous Explore actions; Vigor checks may be required for others.	Action Penalty
Hunger	100	Collapse; forced Rest with severe Exhaustion/Mental Strain gain.	Catastrophic Failure
Exhaustion	50	Increased chance of minor failures on physical tasks.	Action Penalty
Exhaustion	75	Daily AP budget reduced by 2. Passive Mental Strain gain increased.	AP Penalty, Domain
Exhaustion	100	Collapse; potential blackout/loss of time/items.	Catastrophic Failure
Mental Strain	50	Certain optimistic/rational dialogue options become unavailable.	Cognitive Penalty
Mental Strain	75	+1 AP cost for Rest action (difficulty sleeping). Passive Isolation gain increased.	AP Penalty, Domain
Mental Strain	100	Forced irrational action/choice; potential narrative spiral.	Cognitive/Catastrophe
Isolation	50	Reduced effectiveness of positive social Talk actions.	Action Penalty
Isolation	75	Cannot initiate Seek Reassurance action. Increased passive Mental Strain gain.	Action Penalty, Domain
Isolation	100	Forced negative interaction or withdrawal; potential narrative lock/ending.	Cognitive/Catastrophe

In Google Sheets exportieren
This table provides a clear structure for defining the consequences of neglecting Domains. The specific penalties and threshold values are key tuning levers for difficulty. The interaction between penalties (e.g., Exhaustion reducing AP needed to manage Hunger) creates the potential for downward spirals, reinforcing the escalating pressure.

2.4. Management Actions & Spot/NPC Gating
Players are not helpless against the rising tide of the Domains. Specific actions exist to actively reduce Domain levels, but access to these actions is deliberately constrained, tying survival directly to engagement with the inn's environment and inhabitants.

Mechanism: Players spend AP to perform Domain management actions, each targeting a specific Domain (e.g., Eat Rations for Hunger, Engage in Deep Conversation for Isolation).

Action Sources & Gating: Crucially, these management actions are generally not available by default. They must be discovered, unlocked, or enabled through interaction with the game world:

Exploring Spots: Interacting with specific locations within the Dusty Flagon is the primary way to find tangible resources or opportunities for Domain management. Examples include: finding discarded rations in the pantry (Manage Hunger), discovering a hidden, quiet alcove suitable for contemplation (Manage Mental Strain), finding a surprisingly comfortable chair near the hearth (Manage Exhaustion), or locating items that can be used to initiate positive interactions (Manage Isolation).
Interacting with NPCs: Engaging with the innkeeper, staff, or other patrons provides social avenues for Domain management. This could involve: purchasing or begging for food (Manage Hunger), finding a sympathetic ear for conversation (Manage Mental Strain, Manage Isolation), receiving tips for better resting techniques (Manage Exhaustion), or gaining companionship through shared activities (Manage Isolation).
Progression Link: The availability and effectiveness of management actions are intrinsically linked to player progression within the inn. Initial exploration might yield only basic, low-efficiency options (e.g., stale bread). Deeper exploration (potentially requiring more AP, specific skills, or overcoming Domain penalties) or building rapport with NPCs might unlock more potent solutions (e.g., a hearty meal, a truly calming conversation partner, access to a private, restful room).

This gating mechanism achieves a critical design goal: it forces players to actively engage with the Dusty Flagon's environment and social landscape not merely for narrative progress, but for fundamental survival. The need to reduce Hunger drives exploration of the kitchen; the need to combat Isolation encourages interaction with patrons. The inn itself becomes a repository of potential solutions, but accessing them requires AP investment, discovery, and sometimes social maneuvering. If Spots become depleted or NPCs become uncooperative or unavailable, the player's ability to manage their Domains diminishes, directly increasing the pressure in a way that feels organic to the confined setting, strongly evoking the Dotage-inspired sense of dwindling options.

Section 3: Archetype & Vigor: Influencing Choice and Consequence
3.1. Defining Archetype & Vigor
Archetype and Vigor are core character stats, likely established during character creation or through significant early choices, that define the Wayfarer's background, personality, and inner resilience.

Archetype: Represents the character's background, skillset, or predominant personality (e.g., Scholar, Rogue, Diplomat, Occultist, Guard). It influences their knowledge, innate abilities, and how they perceive and interact with the world.
Vigor: Represents the character's willpower, mental fortitude, physical constitution, or resolve. It reflects their ability to withstand hardship, push through limitations, and resist the decaying influences of the environment and their own internal state.
These stats serve as foundational elements that shape the player's experience beyond simple numerical bonuses, influencing the choices available to them and the outcomes of their actions.

3.2. Role in Unlocking Options (Sir Brante Style)
Inspired by "The Life and Suffering of Sir Brante," Archetype and Vigor play a crucial role in gating specific choices and modifying the consequences of actions, making the character's identity integral to navigating the narrative and mechanical challenges.

Choice Gating: Specific dialogue options, actions related to discovered Spots, or unique approaches to problems may only become available if the character possesses the relevant Archetype or meets a certain Vigor threshold.
Example: A character with the Scholar Archetype might find an old ledger in the innkeeper's office. Only they gain the unique action option Decipher Ledger, potentially revealing crucial information or a hidden resource management technique. A character attempting a physically demanding task while suffering from Exhaustion might require a high Vigor check to even make the attempt.
Consequence Modification: Beyond simply enabling options, these stats can alter the results of actions.
Example: A character with the Diplomat Archetype attempting the Seek Reassurance action might gain a significantly larger reduction in Isolation or build rapport more quickly with the NPC. A character with high Vigor facing a sudden, frightening event might automatically pass a check to resist a large spike in Mental Strain, whereas a low Vigor character might suffer the full effect or even additional penalties.
This approach ensures that the character's defined traits consistently influence their journey. A character facing high Mental Strain might normally only see dialogue options reflecting panic or despair. However, if that character possesses exceptionally high Vigor, they might retain access to calmer, more rational choices, allowing them to navigate the situation differently. This reflects the Sir Brante principle where inherent stats and past choices shape the character's present capabilities and available paths, making playthroughs with different Archetypes or Vigor levels feel distinct.

3.3. Impact on Domain Resistance & Management
To further integrate these core stats into the gameplay loop and reinforce their importance, Archetype and Vigor can directly influence the character's interaction with the Domain system.

Passive Resistance: The stats could offer subtle, passive benefits against Domain increases.
Example: High Vigor might confer a slight reduction in the rate of passive Exhaustion gain per AP spent. A character with a Stoic or Ascetic Archetype might naturally gain Isolation or Mental Strain at a slower rate than others. This makes the initial character build contribute directly to long-term resilience.
Management Efficiency: Archetype and Vigor can modify the effectiveness or AP cost of Domain management actions.
Example: A Scholar Archetype might gain double the benefit from the Meditate action for reducing Mental Strain due to their disciplined mind. High Vigor could make the basic Rest action more effective at reducing the Exhaustion Domain, or potentially slightly reduce the passive Domain increases incurred during Rest. An Herbalist Archetype might be more effective when using found ingredients to manage Hunger or Exhaustion.
Linking Archetype and Vigor to Domain resistance and management efficiency provides significant long-term mechanical weight to these character-defining stats. It moves them beyond being simple keys for unlocking occasional specific options and makes them a constant factor in the core survival loop. This reinforces the Sir Brante-esque feeling that the character's fundamental identity, established early on, profoundly shapes their ability to endure the escalating pressures of the Dusty Flagon. The choice of Archetype and the level of Vigor become integral components of the player's strategy for managing decay.

3.4. Optional Table: Archetype/Vigor Influence Examples
While the specific implementations will depend on the chosen Archetypes and the desired depth, the following table illustrates the potential types of influence:

Table 3: Archetype/Vigor Influence Examples

Stat / Level	Situation	Gated Option / Modified Outcome	System Interaction
Scholar Archetype	Finding cryptic note in a Spot	Unlock unique Decipher Note action (1 AP cost).	Spot Interaction, Info Gain
Rogue Archetype	Exploring locked cellar door	Reduced AP cost for Explore Spot attempt; higher chance of success.	AP Economy, Explore
Diplomat Archetype	Using Seek Reassurance with wary NPC	Increased Isolation reduction; potential for positive rapport shift.	Domain Management, NPC
High Vigor (>7)	Attempting Explore while Exhaustion > 50	Able to attempt the action (otherwise locked); may avoid additional Exhaustion gain on success.	Action Gating, Domain
Low Vigor (<4)	Witnessing a disturbing event	Increased Mental Strain gain; may unlock only panicked dialogue options afterwards.	Domain, Cognitive Penalty
Guard Archetype	Resting in the common room	Reduced chance of negative events during Rest; slightly less Mental Strain gain during Rest.	Rest Mechanic, Domain
High Vigor (>8)	General gameplay	-5% to passive Exhaustion gain rate per AP spent.	Passive Domain Increase

In Google Sheets exportieren
This table serves as a conceptual guide, demonstrating how Archetype and Vigor can be woven into various facets of the game—AP costs, action availability, Domain management, passive resistance, and narrative choices—to create a cohesive system where character identity matters mechanically.

Section 4: System Interlocks & Escalating Pressure within the Dusty Flagon
4.1. Mapping the Interactions
The core mechanics of Wayfarer are designed not as isolated systems, but as an interconnected web where each element influences the others. Understanding these feedback loops is crucial for balancing and for achieving the desired feeling of precarious survival.

AP <-> Domains: This is the central loop. Spending AP is necessary to perform actions, including those that manage Domains. However, spending AP inherently passes time, leading to passive Domain increases. Furthermore, high Domain levels inflict penalties that can increase AP costs or reduce the daily AP pool, making it harder to earn the AP needed to manage the Domains. This creates a potential downward spiral where worsening conditions make recovery progressively more difficult.
Domains <-> Domains: The Domains are not independent. High levels in one can directly accelerate the increase of another (e.g., severe Exhaustion making one more susceptible to Mental Strain; high Isolation potentially hindering efforts to find food, thus increasing Hunger). Managing one Domain effectively might require spending AP that could have been used to manage another, forcing constant prioritization and trade-offs.
AP <-> Spots/NPCs: AP is the currency required to Explore Spots and Talk to NPCs. These interactions are the primary gates for discovering Domain management actions and resources. Therefore, a lack of AP directly restricts the player's ability to find solutions to their rising Domain problems.
Domains <-> Spots/NPCs: High Domain levels can negatively impact interactions with Spots and NPCs. Severe Exhaustion might prevent thorough exploration of a physically demanding Spot. High Isolation or Mental Strain might lock dialogue options needed to gain an NPC's trust or assistance, cutting off potential avenues for Domain relief. The character's internal state affects their ability to engage with external solutions.
Archetype/Vigor <-> All Systems: As detailed in Section 3, these foundational stats permeate the entire system. They influence AP efficiency (costs, pool size), Domain resistance (passive gain rates), Domain management effectiveness, the ability to access specific options within Spots, and the outcomes of NPC interactions. They act as modifiers and gatekeepers across the board, tying character identity to every facet of the loop.
These interlocks ensure that player actions have cascading consequences. A decision to spend AP exploring might yield a resource but also increase Exhaustion, which in turn might make the next Rest less effective or increase Mental Strain gain, demonstrating the tightly coupled nature of the systems.

4.2. Mechanisms for Dotage-Inspired Escalating Pressure
The design aims to replicate the feeling of managing inevitable decline within a constrained system, drawing inspiration from games like "Dotage." Within the single location of the Dusty Flagon, this escalating pressure is achieved through several interconnected mechanisms that make survival progressively harder over time:

Increasing Passive Gain: As time passes (measured in days, cycles, or perhaps tied to narrative milestones), the base rates at which Domains increase passively can subtly rise. Hunger pangs become more frequent, exhaustion sets in faster, the weight of isolation grows heavier. This requires the player to become more efficient or find better management tools just to maintain equilibrium.
Decreasing Management Effectiveness: The potency of basic, easily accessible Domain management actions could diminish over time. Stale rations found early on might become less effective at sating Hunger later. Simple reassurances from NPCs might offer less comfort as Mental Strain or Isolation become more deeply ingrained. This forces the player to seek out rarer, more costly, or more complex solutions gated behind deeper exploration or NPC relationship progression.
Resource Scarcity & Environmental Exhaustion: The resources available within the Dusty Flagon are not infinite. Spots that provide food or items might be single-use or have limited supplies. NPCs who offer help might become unavailable (due to their own circumstances, narrative events, or the player's actions), demand increasingly higher costs (AP, favors), or simply run out of patience or resources. The inn itself, as the sole source of solutions, becomes a resource that is gradually depleted.
Environmental Decay: The atmosphere and state of the Dusty Flagon could subtly degrade over time, reflecting the passage of time and perhaps a growing sense of despair or otherworldly influence. Safe places to rest might become compromised, unsettling events might occur more frequently (increasing Mental Strain), or pathways might become blocked. The environment itself contributes to the rising pressure.
Compounding Penalties: As the player inevitably struggles to manage all four Domains perfectly, multiple Domains are likely to cross penalty thresholds simultaneously. The combined effects of reduced AP, increased action costs, locked options, and accelerated gain in other Domains can create a severe choke point, drastically limiting the player's options and forcing desperate, high-risk actions.
The confinement to the Dusty Flagon is instrumental in making this pressure effective. Unlike games where players can move to new areas with fresh resources, the Wayfarer is trapped with a finite and potentially degrading set of tools. The pressure escalates not just from the character's internal state (Domains) but from the exhaustion of the environment itself. Every action must be weighed against the long-term sustainability within this closed system, making careful planning, prioritization, and optimization critical from the outset. The inn is both sanctuary and prison, its resources dwindling as the character's needs grow.

Section 5: Spot/NPC Progression as Mechanical Gates
5.1. Spots as Resource/Information Nodes
The physical locations within the Dusty Flagon—the common room, the kitchen, the cellar, guest rooms, hidden crawlspaces, specific objects like a noticeboard or a discarded journal—serve as 'Spots'. Exploring these Spots by spending AP is the primary method for discovering tangible resources, crucial information, and opportunities for Domain management.

Function: Spots act as nodes containing potential solutions or clues. Interaction might yield:
Direct resources: Food items (Manage Hunger), clean water, makeshift bedding (Manage Exhaustion), calming herbs (Manage Mental Strain).
Information: Notes revealing NPC secrets or needs, hints about hidden locations, recipes for more effective resource use, lore that might contextualize Mental Strain.
Environmental opportunities: Discovering a quiet corner suitable for Meditate, finding a secure place to Rest more effectively.
Progression: The value derived from Spots should ideally scale with progression. Initial, easily accessible Spots (e.g., the main bar area) might offer only basic, low-impact relief (e.g., stale bread crusts). Reaching more remote, hidden, or initially inaccessible Spots (e.g., a locked chest in the cellar, the innkeeper's private quarters) should require greater AP investment, potentially specific Archetype skills (like Lockpicking for the Rogue), overcoming Domain penalties (e.g., needing high Vigor to brave a rat-infested pantry despite Hunger), or finding keys/tools via other Spots or NPCs. These deeper Spots should yield more significant rewards: higher quality food, unique items, critical information, or access to more potent Domain management techniques.
5.2. NPCs as Social/Service Nodes
The inhabitants of the Dusty Flagon—the innkeeper, bar staff, fellow travelers, perhaps more unusual residents—are not just set dressing. They serve as 'NPC Nodes', providing social and service-based avenues for Domain management and progression. Interacting with them via the Talk action (costing AP) is essential.

Function: NPCs can offer:
Direct aid: Selling or giving food (Manage Hunger), offering a listening ear (Manage Mental Strain, Manage Isolation), providing a service like mending clothes or offering shelter (Manage Exhaustion).
Information & Opportunities: Sharing rumors, revealing personal needs that could become side quests, teaching skills or management techniques, granting access to restricted Spots.
Companionship: Simply spending time in positive interaction can directly reduce Isolation.
Progression: Relationships with NPCs should evolve based on player interaction. A simple relationship metric (e.g., Rapport) could track disposition. Positive interactions, fulfilling requests, or using Archetype-specific dialogue (e.g., Diplomat) could increase Rapport, unlocking more helpful interactions: better prices, willingness to share sensitive information, access to unique services, or more effective Domain relief (e.g., a heartfelt conversation being more restorative than a perfunctory one). Conversely, negative interactions, failed checks, or betrayals could lower Rapport, closing off options, increasing costs, or making NPCs hostile. Furthermore, NPCs should have their own states and schedules; they might not always be available, their mood might change, or external events could alter their willingness or ability to help, adding another layer of dynamic scarcity.
5.3. Creating the Gameplay Loop
The gating of Domain management through Spots and NPCs solidifies the core gameplay loop:

Need Arises: Domains increase passively or due to events, approaching penalty thresholds.
Solution Required: Player recognizes the need for specific Domain management actions (e.g., need food, need conversation).
Action Gated: The required action is not immediately available; it must be found or enabled via the environment or its inhabitants.
Resource Investment: Player spends AP to Explore relevant Spots or Talk to potentially helpful NPCs.
Discovery/Unlock: Player finds the necessary resource (e.g., rations), information (e.g., learns NPC offers comfort), or unlocks the action (e.g., finds a quiet place to meditate).
Management Action: Player spends further AP to use the discovered resource or perform the unlocked action (e.g., Eat Rations, Seek Reassurance, Meditate).
Outcome & Cycle: Domain level is reduced. However, AP has been spent, time has passed, potentially increasing other Domains. The loop restarts, often under slightly increased pressure due to resource depletion or rising passive gain rates.
This loop fundamentally connects survival to engagement. A player cannot simply turtle and manage stats internally; they are forced outwards, into the fabric of the Dusty Flagon. Curiosity, exploration, and social maneuvering are not optional side activities but core requirements for managing the relentless decay represented by the Domains. The narrative discovery inherent in exploring the inn and learning about its inhabitants becomes inextricably linked to the mechanical challenge of staying alive and functional.

Section 6: Initial Balancing Considerations & POC Recommendations
6.1. Key Tuning Variables
Achieving the desired feel of escalating pressure without being unfairly punitive requires careful tuning. The following variables are critical points of adjustment during development and playtesting:

AP Economy:
Base Daily AP: Controls overall player agency per cycle.
AP Costs (Rest, Talk, Explore, Manage Domain): Determines action frequency and trade-offs.
Domain Mechanics:
Passive Increase Rates: Sets the baseline pressure level for each Domain.
Threshold Values: Defines when penalties trigger.
Penalty Severity & Type: Determines the impact of neglecting Domains.
Management Action Effectiveness: Controls how effectively players can push back against Domain increases.
Resource & Interaction Gates:
Availability/Depletion Rate of Spot Resources: Influences long-term sustainability.
NPC Interaction Costs (AP, Rapport): Balances the value of social solutions.
NPC Helpfulness & Availability: Affects the reliability of social solutions.
Character Stats:
Archetype/Vigor Impact: Magnitude of passive resistance, management efficiency boosts, and choice gating effectiveness.
Escalation Curve:
Rate of Passive Gain Increase: Controls how quickly baseline pressure rises over time.
Rate of Management Effectiveness Decrease: Controls how quickly basic solutions become insufficient.
Rate of Resource Depletion/Scarcity Increase: Controls how quickly the environment becomes exhausted.
These variables are interconnected; adjusting one will likely necessitate adjustments elsewhere to maintain the intended balance and pacing.

6.2. Suggestions for Initial POC Values
For an initial Proof of Concept build focused on testing the core loop, the following starting values are suggested as a baseline, explicitly intended for iteration based on playtesting:

Daily AP: 10 AP
AP Costs: Rest=2, Talk=1, Explore=2, Basic Eat=1, Basic Social=1, Meditate=2
Passive Gain (Example per 10 AP cycle / 'Day'): Hunger +4, Exhaustion +5, Mental Strain +3 (plus +2 per Rest), Isolation +2 (if no positive social interaction)
Domain Thresholds: 25 / 50 / 75 / 100
Basic Management Effectiveness: Eat Rations = -10 Hunger, Seek Reassurance = -10 Isolation / -5 Mental Strain
Initial Penalties (Examples):
@50: +1 AP cost to related actions OR minor effectiveness reduction.
@75: -2 Daily AP OR moderate effectiveness reduction / minor secondary Domain gain increase.
POC Scope: Focus on a limited timeframe (e.g., 3-5 'days' or cycles) to evaluate the initial pressure curve and the feel of the core AP/Domain/Rest loop without requiring extensive Spot/NPC content.
