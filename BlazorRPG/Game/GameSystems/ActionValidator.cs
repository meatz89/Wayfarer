public class ActionValidator
{
    private readonly GameState gameState;
    private readonly KnowledgeSystem knowledgeSystem;
    private readonly CharacterRelationshipSystem relationshipSystem;
    private readonly LocationAccess locationAccess;

    public ActionValidator(
        GameState gameState,
        KnowledgeSystem knowledgeSystem,
        CharacterRelationshipSystem relationshipSystem,
        LocationAccess locationAccess
        )
    {
        this.gameState = gameState;
        this.knowledgeSystem = knowledgeSystem;
        this.relationshipSystem = relationshipSystem;
        this.locationAccess = locationAccess;
    }

    public bool CanExecuteAction(BasicAction action, CharacterNames? character = null)
    {
        foreach (IRequirement requirement in action.Requirements)
        {
            bool hasRequirement = CheckRequirement(requirement);
            if (!hasRequirement)
            {
                return false;
            }
        }
        return true;
    }

    public List<string> GetBlockingRequirements(BasicAction action)
    {
        return new List<string>();
    }


    private bool CheckRequirement(IRequirement requirement)
    {
        if (requirement is CoinsRequirement money)
        {
            return gameState.Player.Coins >= money.Amount;
        }
        if (requirement is FoodRequirement food)
        {
            return gameState.Player.Health >= food.Amount;
        }
        if (requirement is InventorySlotsRequirement inventorySlot)
        {
            return gameState.Player.Inventory.GetEmptySlots() >= inventorySlot.Count;
        }
        if (requirement is HealthRequirement health)
        {
            return gameState.Player.Health >= health.Amount;
        }
        if (requirement is PhysicalEnergyRequirement physicalEnergy)
        {
            return gameState.Player.PhysicalEnergy >= physicalEnergy.Amount;
        }
        if (requirement is FocusEnergyRequirement focusEnergy)
        {
            return gameState.Player.FocusEnergy >= focusEnergy.Amount;
        }
        if (requirement is SocialEnergyRequirement socialEnergy)
        {
            return gameState.Player.SocialEnergy >= socialEnergy.Amount;
        }
        if (requirement is SkillLevelRequirement skillLevel)
        {
            return gameState.Player.Skills[skillLevel.SkillType] >= skillLevel.Amount;
        }
        if (requirement is ItemRequirement item)
        {
            return gameState.Player.Inventory.GetItemCount(item.ResourceType) >= item.Count;
        }
        return false;
    }

}
