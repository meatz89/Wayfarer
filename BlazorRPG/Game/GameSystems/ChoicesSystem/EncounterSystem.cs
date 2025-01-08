public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;

    public EncounterSystem(GameState gameState, ChoiceSystem choiceSystem)
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
    }

    public void ExecuteChoice(Encounter encounter, EncounterChoice choice, LocationProperties locationProperties)
    {
        // Create an instance of ConsequenceProcessor
        ConsequenceProcessor consequenceProcessor = new ConsequenceProcessor(gameState, encounter.Context, locationProperties);

        // Use ConsequenceProcessor to handle all choice consequences
        consequenceProcessor.ProcessConsequences(choice, encounter);


        // 1. Energy Costs
        ApplyEnergyCosts(choice, encounter.Context);

        // 2. Narrative Value Changes
        encounter.Context.CurrentValues.ApplyChanges(choice.EncounterValueChanges);

        // 3. Apply Encounter State Value Modifications
        ApplyEncounterStateValueModifications(encounter);

        // 4. Apply Choice Costs and Rewards
        foreach (Outcome cost in choice.PermanentCosts)
        {
            cost.Apply(gameState.Player);
        }

        foreach (Outcome reward in choice.PermanentRewards)
        {
            reward.Apply(gameState.Player);
        }

        // Check for game over conditions after applying choice effects
        if (IsGameOver())
        {
            // Handle game over
            Console.WriteLine("Game Over!");
            gameState.Actions.SetActiveEncounter(null);
        }
    }

    private void ApplyEncounterStateValueModifications(Encounter encounter)
    {
        EncounterActionContext context = encounter.Context;

        // **Resonance Modifiers**
        if (context.CurrentValues.Resonance >= 8)
        {
            context.CurrentValues.Outcome += 2;
        }
        else if (context.CurrentValues.Resonance >= 5)
        {
            context.CurrentValues.Outcome += 1;
        }

        // **Determine Base Outcome Based on Values**
        // This is where you'll implement logic to derive the outcome description
        // based on the combination of Outcome, Pressure, Insight, and Resonance values.
        // You can use a similar approach to what you had in `GetOutcomeType`, but
        // instead of returning a string, you might want to set properties on the
        // `Encounter` or `EncounterStage` that describe the outcome.

        // Clamp values after applying modifications
        context.CurrentValues.Outcome = Math.Clamp(context.CurrentValues.Outcome, 0, 10);
        context.CurrentValues.Insight = Math.Clamp(context.CurrentValues.Insight, 0, 10);
        context.CurrentValues.Resonance = Math.Clamp(context.CurrentValues.Resonance, 0, 10);
        context.CurrentValues.Pressure = Math.Clamp(context.CurrentValues.Pressure, 0, 10);
    }

    private void ApplyEnergyCosts(EncounterChoice choice, EncounterActionContext context)
    {
        foreach (Requirement req in choice.ChoiceRequirements)
        {
            if (req is EnergyRequirement energyReq)
            {
                int cost = energyReq.Amount;

                switch (energyReq.EnergyType)
                {
                    case EnergyTypes.Physical:
                        gameState.Player.PhysicalEnergy -= cost;
                        if (gameState.Player.PhysicalEnergy < 0)
                        {
                            gameState.Player.Health += gameState.Player.PhysicalEnergy; // Health penalty equals the amount of energy overspent
                            gameState.Player.PhysicalEnergy = 0; // Deplete energy
                            if (gameState.Player.Health <= 0)
                            {
                                Console.WriteLine("Game Over! Health depleted.");
                            }
                        }
                        break;

                    case EnergyTypes.Focus:
                        gameState.Player.FocusEnergy -= cost;
                        if (gameState.Player.FocusEnergy < 0)
                        {
                            // Apply a negative consequence related to low Focus Energy
                            context.CurrentValues.Pressure += 2; // Example: Increase Pressure due to lack of focus
                            gameState.Player.FocusEnergy = 0; // Deplete energy
                        }
                        break;

                    case EnergyTypes.Social:
                        gameState.Player.SocialEnergy -= cost;
                        if (gameState.Player.SocialEnergy < 0)
                        {
                            gameState.Player.Reputation += gameState.Player.SocialEnergy; // Reputation penalty equals the amount of energy overspent
                            gameState.Player.SocialEnergy = 0; // Deplete energy
                            if (gameState.Player.Reputation <= 0)
                            {
                                Console.WriteLine("Game Over! Reputation depleted.");
                            }
                        }
                        break;
                }
            }
        }
    }

    private void ApplyConsequences(EncounterActionContext context)
    {
        int outcome = context.CurrentValues.Outcome;
        int pressure = context.CurrentValues.Pressure;
        int insight = context.CurrentValues.Insight;
        int resonance = context.CurrentValues.Resonance;

        // **Examples of Consequences (Based on your descriptions):**

        // High Outcome + Low Pressure: Clean success
        if (outcome >= 7 && pressure <= 3)
        {
            // Full rewards based on Outcome magnitude and context
            // e.g., context.LocationArchetype might determine the type of reward
            // e.g., outcome magnitude determines the quantity
        }

        // High Outcome + High Pressure: Success with complications
        if (outcome >= 7 && pressure >= 7)
        {
            // Partial rewards, but also a penalty based on Pressure and context
            if (context.LocationType == LocationTypes.Industrial)
            {
                gameState.Player.Health -= (pressure / 2); // Injury based on Pressure
            }
            else if (context.LocationType == LocationTypes.Social)
            {
                gameState.Player.Reputation -= (pressure / 3); // Reputation damage
            }
        }

        // Low Outcome + High Pressure: Failure
        if (outcome <= 3 && pressure >= 7)
        {
            // Significant negative consequences based on context
            // e.g., loss of resources, major injury, major reputation damage
        }

        // High Insight: Knowledge gain
        if (insight >= 7)
        {
            gameState.Player.ModifyKnowledge(KnowledgeTypes.Clue, 1);
        }

        // High Resonance: Reputation gain
        if (resonance >= 7)
        {
            gameState.Player.ModifyReputation(ReputationTypes.Trusted, 1);
        }

        // Add more logic to determine specific rewards/penalties based on the combination of values and context
    }

    public Encounter GenerateEncounter(EncounterActionContext context)
    {
        // Generate initial stage
        EncounterStage initialStage = GenerateStage(context);
        if (initialStage == null)
        {
            return null;
        }

        // Create encounter with initial stage
        Encounter encounter = new Encounter(context, GenerateSituation(context));
        encounter.AddStage(initialStage);

        return encounter;
    }

    private EncounterStage GenerateStage(EncounterActionContext context)
    {
        // Generate relevant choices based on context
        List<EncounterChoice> choices = choiceSystem.GenerateChoices(context);
        if (choices == null || choices.Count == 0) return null;

        return new EncounterStage
        {
            Situation = GenerateStageSituation(context),
            Choices = choices
        };
    }

    public void SetActiveEncounter(Encounter encounter)
    {
        gameState.Actions.SetActiveEncounter(encounter);
        gameState.Player.CurrentEncounter = encounter;
    }

    public bool GetNextStage(Encounter encounter)
    {
        // Don't proceed if we've hit our success condition (Outcome ≥ 20) or if the player is in a game over state
        if (encounter.Context.CurrentValues.Outcome >= 20 || IsGameOver())
        {
            return false;
        }

        // Generate new stage and add it
        EncounterStage newStage = GenerateStage(encounter.Context);
        if (newStage == null) return false;

        encounter.AddStage(newStage);

        return true;
    }

    // New method to check for game over conditions
    private bool IsGameOver()
    {
        return gameState.Player.Health <= 0;
    }

    private string GenerateSituation(EncounterActionContext context)
    {
        // Improved situation generation
        List<string> situationElements = new List<string>();

        situationElements.Add($"You are trying to {context.ActionType} at the {context.LocationArchetype} ({context.LocationType}).");

        if (context.CurrentValues.Pressure >= 6)
        {
            situationElements.Add("The situation is tense.");
        }
        if (context.CurrentValues.Insight >= 7)
        {
            situationElements.Add("You have a good insight of what's going on.");
        }
        else if (context.CurrentValues.Insight <= 2)
        {
            situationElements.Add("You're not quite sure what to do.");
        }

        return string.Join(" ", situationElements);
    }

    private string GenerateStageSituation(EncounterActionContext context)
    {
        // Generate situation based on narrative values and context
        if (context.CurrentValues.Pressure >= 8)
            return "The situation is very tense...";
        if (context.CurrentValues.Insight >= 8)
            return "You have a clear grasp of the situation...";

        return "You consider your options...";
    }

    public EncounterStage GetCurrentStage(Encounter encounter)
    {
        return encounter.GetCurrentStage();
    }
}