
public class MessageSystem
{
    private ActionResultMessages currentChanges = new();

    public void AddOutcome(Outcome outcome)
    {
        currentChanges.Outcomes.Add(outcome);
    }

    public void AddSystemMessage(string message, SystemMessageTypes type = SystemMessageTypes.Info)
    {
        currentChanges.SystemMessages.Add(new SystemMessage(message, type));
    }

    public ActionResultMessages GetAndClearChanges()
    {
        ActionResultMessages changes = currentChanges;
        currentChanges = new ActionResultMessages();
        return changes;
    }

    internal void GenerateChoiceResultMessages(ChoiceConsequences consequences)
    {
        throw new NotImplementedException();
    }
}