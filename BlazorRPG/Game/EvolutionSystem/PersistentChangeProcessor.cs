
public class PersistentChangeProcessor
{
    public void ApplyChanges(EncounterResult conclusion, EncounterState state)
    {
        // Validate proposed changes against permission system
        foreach (ProposedChange change in conclusion.ProposedChanges)
        {
            if (IsChangePermitted(change))
            {
                ApplyChange(change);
            }
        }
    }

    private bool IsChangePermitted(ProposedChange change)
    {
        return true;
    }

    private void ApplyChange(ProposedChange change)
    {
        GameWorld gameWorld = new GameWorld();
        Player player = gameWorld.GetPlayer();

        switch (change.Type)
        {
            case ChangeTypes.Relationship:
                ApplyRelationshipChange(change, player);
                break;

            case ChangeTypes.Knowledge:
                player.AddKnowledge(change.KnowledgeItem);
                break;

            case ChangeTypes.Currency:
                player.ModifyCurrency(change.Amount);
                break;

            case ChangeTypes.NewLocation:
                gameWorld.RevealLocation(change.LocationID);
                break;
        }
    }

    private void ApplyRelationshipChange(ProposedChange change, Player player)
    {
        GameWorld GameWorld = new GameWorld();
        NPC targetNPC = GameWorld.GetCharacter(change.TargetID);
        if (targetNPC != null)
        {
            int currentRelationship = player.GetRelationship(targetNPC.ID);
            int newRelationship = currentRelationship + change.Magnitude;
            player.SetRelationship(targetNPC.ID, newRelationship);
        }
    }

    internal async Task<string> ConsolidateMemory(EncounterContext encounterContext, MemoryConsolidationInput memoryInput)
    {
        throw new NotImplementedException();
    }
}