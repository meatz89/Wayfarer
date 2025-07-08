
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
            case ChangeTypes.Knowledge:
                player.AddKnowledge(change.KnowledgeItem);
                break;

            case ChangeTypes.Currency:
                player.ModifyCurrency(change.Amount);
                break;

        }
    }
}