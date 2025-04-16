public class TutorialManager
{
    private readonly GameState gameState;
    private readonly TutorialState tutorialState;

    // Injected via DI
    public TutorialManager(GameState gameState, TutorialState tutorialState)
    {
        this.gameState = gameState;
        this.tutorialState = tutorialState;
    }

    public void Initialize()
    {
        tutorialState.SetFlag(TutorialState.TutorialFlags.TutorialStarted);
        gameState.WorldState.CurrentTimeInHours = 8;
        gameState.WorldState.DetermineCurrentTimeWindow(1); // Morning
        gameState.PlayerState.Food = 0;
        gameState.PlayerState.MedicinalHerbs = 0;
        gameState.PlayerState.Energy = (int)(gameState.PlayerState.MaxEnergy * 0.8);
        gameState.ActionStateTracker.SaveCurrentState(gameState);
    }

    public void CheckTutorialProgress()
    {
        if (gameState.GameMode != Modes.Tutorial) return;

        // Check conditions that might set flags
        CheckLocationConditions();
        CheckResourceConditions();
        CheckActionConditions();

        // Now update objectives based on flags
        UpdateObjectives();
    }

    private void CheckLocationConditions()
    {
        var currentSpot = gameState.WorldState.CurrentLocationSpot;
        if (currentSpot == null) return;

        switch (currentSpot.Name)
        {
            case "Forest Stream":
                tutorialState.SetFlag(TutorialState.TutorialFlags.VisitedStream);
                break;
            case "High Ground":
                tutorialState.SetFlag(TutorialState.TutorialFlags.VisitedHighGround);
                break;
        }
    }

    private void CheckResourceConditions()
    {
        // Check if player has resources
        if (gameState.PlayerState.MedicinalHerbs > 0)
        {
            tutorialState.SetFlag(TutorialState.TutorialFlags.GatheredHerbs);
        }

        if (gameState.PlayerState.Food > 0)
        {
            tutorialState.SetFlag(TutorialState.TutorialFlags.GatheredFood);
        }

        // Check resource depletion
        if (gameState.PlayerState.Food <= 0)
        {
            tutorialState.SetFlag(TutorialState.TutorialFlags.OutOfFood);
        }

        if (gameState.PlayerState.MedicinalHerbs <= 0)
        {
            tutorialState.SetFlag(TutorialState.TutorialFlags.OutOfHerbs);
        }
    }

    private void CheckActionConditions()
    {
        // Check for specific resource usage based on action name
        var currentAction = gameState.ActionStateTracker.CurrentAction;
        if (currentAction != null)
        {
            string actionId = currentAction.ActionImplementation.ActionId;
            switch (actionId)
            {
                case "ConsumeFood":
                    tutorialState.SetFlag(TutorialState.TutorialFlags.UsedFood);
                    break;

                case "ConsumeMedicinalHerbs":
                    tutorialState.SetFlag(TutorialState.TutorialFlags.UsedHerbs);
                    break;

                case "Rest":
                    tutorialState.SetFlag(TutorialState.TutorialFlags.Rested);
                    break;
            }
        }

        // Check for specific completed encounters
        if (gameState.WorldState.IsEncounterCompleted("FindPathOut"))
        {
            tutorialState.SetFlag(TutorialState.TutorialFlags.FoundPathOut);
        }

        if (gameState.WorldState.IsEncounterCompleted("SearchSurroundings"))
        {
            tutorialState.SetFlag(TutorialState.TutorialFlags.FoundStream);
        }

        if (gameState.WorldState.IsEncounterCompleted("GatherHerbs"))
        {
            tutorialState.SetFlag(TutorialState.TutorialFlags.GatheredHerbs);
        }
    }

    private void UpdateObjectives()
    {
        // This is where we check flags to determine objective progression
        TutorialState.TutorialObjective currentObjective = tutorialState.CurrentObjective;

        switch (currentObjective)
        {
            case TutorialState.TutorialObjective.ExploreClearing:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.FoundStream))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.FindStream);
                }
                break;

            case TutorialState.TutorialObjective.FindStream:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.VisitedStream))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.GatherHerbs);
                }
                break;

            case TutorialState.TutorialObjective.GatherHerbs:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.GatheredHerbs))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.UseHerbs);
                }
                break;

            case TutorialState.TutorialObjective.UseHerbs:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.UsedHerbs))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.FindFood);
                }
                break;

            case TutorialState.TutorialObjective.FindFood:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.GatheredFood))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.EatFood);
                }
                break;

            case TutorialState.TutorialObjective.EatFood:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.UsedFood))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.ReachHighGround);
                }
                break;

            case TutorialState.TutorialObjective.ReachHighGround:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.VisitedHighGround))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.FindPathOut);
                }
                break;

            case TutorialState.TutorialObjective.FindPathOut:
                if (tutorialState.CheckFlag(TutorialState.TutorialFlags.FoundPathOut))
                {
                    tutorialState.AdvanceToObjective(TutorialState.TutorialObjective.TutorialComplete);
                }
                break;
        }
    }
}