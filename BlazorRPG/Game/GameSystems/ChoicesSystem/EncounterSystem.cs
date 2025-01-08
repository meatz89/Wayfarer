public class EncounterSystem
{
    private readonly GameState gameState;
    private readonly ChoiceSystem choiceSystem;

    public EncounterSystem(GameState gameState, ChoiceSystem choiceSystem)
    {
        this.gameState = gameState;
        this.choiceSystem = choiceSystem;
    }

    public Encounter GenerateEncounter(EncounterActionContext context)
    {
        // Generate initial stage
        EncounterStage initialStage = GenerateStage(context);

        // Create encounter with initial stage
        Encounter encounter = new Encounter(context, GenerateSituation(context));
        encounter.Stages.Add(initialStage);

        return encounter;
    }


    private EncounterStage GenerateStage(EncounterActionContext context)
    {
        // Generate relevant choices based on context
        List<EncounterChoice> choices = choiceSystem.GenerateChoices(context);

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

    public EncounterStage GetCurrentStage(Encounter encounter)
    {
        return encounter.Stages[encounter.CurrentStage];
    }

    public List<EncounterChoice> GetCurrentStageChoices(Encounter encounter)
    {
        return encounter.Stages[encounter.CurrentStage].Choices;
    }


    public bool GetNextStage(Encounter encounter)
    {
        // Don't proceed if we've hit our success condition (Advantage ≥ 10) or if the player is in a game over state
        if (encounter.Context.CurrentValues.Advantage >= 10 || IsGameOver())
        {
            return false;
        }

        // Generate new stage and add it
        EncounterStage newStage = GenerateStage(encounter.Context);
        encounter.Stages.Add(newStage);
        encounter.CurrentStage++;

        return true;
    }

    // New method to check for game over conditions
    private bool IsGameOver()
    {
        return gameState.Player.Health <= 0;
    }

    public void ExecuteChoice(Encounter encounter, EncounterChoice choice)
    {
        // 1. Energy Costs
        ApplyEnergyCosts(choice);

        // 2. Narrative Value Changes
        encounter.Context.CurrentValues.ApplyChanges(choice.EncounterValueChanges);

        // 3. Connection Bonus (Apply to Advantage gains only)
        if (encounter.Context.CurrentValues.Connection >= 8)
        {
            encounter.Context.CurrentValues.Advantage += 2;
        }
        else if (encounter.Context.CurrentValues.Connection >= 5)
        {
            encounter.Context.CurrentValues.Advantage += 1;
        }
        encounter.Context.CurrentValues.Advantage = Math.Clamp(encounter.Context.CurrentValues.Advantage, 0, 10); // Cap Advantage at 10
        encounter.Context.CurrentValues.Understanding = Math.Clamp(encounter.Context.CurrentValues.Understanding, 0, 10); // Cap Advantage at 10
        encounter.Context.CurrentValues.Connection = Math.Clamp(encounter.Context.CurrentValues.Connection, 0, 10); // Cap Advantage at 10
        encounter.Context.CurrentValues.Tension = Math.Clamp(encounter.Context.CurrentValues.Tension, 0, 10); // Cap Advantage at 10

        // 4. Tension Modifier
        if (encounter.Context.CurrentValues.Tension >= 6)
        {
            // Increase Energy costs (already handled in ApplyEnergyCosts)
        }

        // 5. Item/Knowledge/Reputation Effects (Handled in Special Choice generation)

        // Apply choice costs and rewards - These are now mainly for resources
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
            // Handle game over (e.g., display a message, end the encounter)
            Console.WriteLine("Game Over!");
            gameState.Actions.SetActiveEncounter(null); // Or other logic to end the encounter
        }
    }

    private void ApplyEnergyCosts(EncounterChoice choice)
    {
        foreach (Requirement req in choice.ChoiceRequirements)
        {
            if (req is EnergyRequirement energyReq)
            {
                int cost = energyReq.Amount;

                // Tension Modifier
                if (gameState.Actions.CurrentEncounter.Context.CurrentValues.Tension >= 6)
                {
                    cost += 1;
                }

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
                            gameState.Player.Stress -= gameState.Player.FocusEnergy; // Stress penalty equals the amount of energy overspent
                            gameState.Player.FocusEnergy = 0; // Deplete energy
                            if (gameState.Player.Stress >= 10)
                            {
                                Console.WriteLine("Game Over! Stress too high.");
                            }
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


    private string GenerateSituation(EncounterActionContext context)
    {
        // Improved situation generation
        List<string> situationElements = new List<string>();

        situationElements.Add($"You are trying to {context.ActionType} at the {context.LocationArchetype} ({context.LocationType}).");

        if (context.CurrentValues.Tension >= 6)
        {
            situationElements.Add("The situation is tense.");
        }
        if (context.CurrentValues.Understanding >= 7)
        {
            situationElements.Add("You have a good understanding of what's going on.");
        }
        else if (context.CurrentValues.Understanding <= 2)
        {
            situationElements.Add("You're not quite sure what to do.");
        }

        return string.Join(" ", situationElements);
    }

    private string GenerateStageSituation(EncounterActionContext context)
    {
        // Generate situation based on narrative values and context
        if (context.CurrentValues.Tension >= 8)
            return "The situation is very tense...";
        if (context.CurrentValues.Understanding >= 8)
            return "You have a clear grasp of the situation...";

        return "You consider your options...";
    }
}