using System.Diagnostics.Metrics;

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
        // Don't proceed if we've hit our success condition (Outcome ≥ 10) or if the player is in a game over state
        if (encounter.Context.CurrentValues.Outcome >= 10 || IsGameOver())
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

        // 3. Resonance Bonus (Apply to Outcome gains only)
        ApplyEncounterStateValueModifications(encounter);

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

    private static void ApplyEncounterStateValueModifications(Encounter encounter)
    {
        if (encounter.Context.CurrentValues.Resonance >= 8)
        {
            encounter.Context.CurrentValues.Outcome += 2;
        }
        else if (encounter.Context.CurrentValues.Resonance >= 5)
        {
            encounter.Context.CurrentValues.Outcome += 1;
        }

        encounter.Context.CurrentValues.Outcome =
            Math.Clamp(encounter.Context.CurrentValues.Outcome, 0, 10); // Cap Outcome at 10

        encounter.Context.CurrentValues.Insight =
            Math.Clamp(encounter.Context.CurrentValues.Insight, 0, 10); // Cap Outcome at 10

        encounter.Context.CurrentValues.Resonance =
            Math.Clamp(encounter.Context.CurrentValues.Resonance, 0, 10); // Cap Outcome at 10

        encounter.Context.CurrentValues.Pressure =
            Math.Clamp(encounter.Context.CurrentValues.Pressure, 0, 10); // Cap Outcome at 10
    }

    private void ApplyEnergyCosts(EncounterChoice choice)
    {
        foreach (Requirement req in choice.ChoiceRequirements)
        {
            if (req is EnergyRequirement energyReq)
            {
                int cost = energyReq.Amount;

                // Pressure Modifier
                if (gameState.Actions.CurrentEncounter.Context.CurrentValues.Pressure >= 6)
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
}