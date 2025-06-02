public record LocationAction
{
    public string ActionId { get; set; }
    public string Name { get; set; }
    public SkillCategories RequiredCardType { get; set; }
    public ActionExecutionTypes ActionExecutionType { get; set; } = ActionExecutionTypes.Encounter;

    public string ObjectiveDescription { get; set; }
    public string DestinationLocation { get; set; }
    public string DestinationLocationSpot { get; set; }

    public string LocationId { get; set; }
    public string LocationSpotId { get; set; }

    public List<IRequirement> Requirements { get; set; } = new();
    public int ActionPointCost { get; set; }

    public int Difficulty { get; set; } = 1;
    public List<ApproachDefinition> Approaches { get; set; } = new List<ApproachDefinition>();
    public OpportunityDefinition Opportunity { get; set; }
    public int Complexity { get; set; }


    // This method is your bridge into the encounterContext system
    public void Execute(Player player, LocationSpot location)
    {
        // Step 1: Check if player has appropriate card type available
        List<SkillCard> validCards = player.GetAvailableCardsByType(RequiredCardType);
        if (validCards.Count == 0)
        {
            UIManager UIManager = new();
            UIManager.ShowMessage("You don't have any " + RequiredCardType + " cards available.");
            return;
        }

        // Step 2: Create the encounterContext context
        EncounterContext context = new EncounterContext();
        context.LocationName = location.Name;
        context.LocationSpotName = location.SpotID;
        context.ActionName = this.Name;
        context.SkillCategory = this.RequiredCardType;
        context.PlayerSkillCards = validCards;
        context.PlayerAllCards = player.GetAllAvailableCards();

        // Step 3: Determine encounterContext parameters based on action type
        EncounterParameters parameters = DetermineEncounterParameters(this.RequiredCardType);

        // Step 4: Launch the encounter
        EncounterManager encounterManager = new EncounterManager();
        encounterManager.start(context, parameters, player);
    }

    private EncounterParameters DetermineEncounterParameters(SkillCategories actionType)
    {
        throw new NotImplementedException();
    }
}

public class UIManager
{
    public void ShowMessage(string message)
    {
        // Implementation for showing messages to the player
        Console.WriteLine(message);
    }
}