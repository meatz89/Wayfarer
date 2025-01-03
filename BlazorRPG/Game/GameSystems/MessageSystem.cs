
public class MessageSystem
{
    public ActionResultMessages changes = new();

    public void AddSystemMessage(string message)
    {
        throw new NotImplementedException();
    }

    public ActionResultMessages GetAndClearChanges()
    {
        ActionResultMessages changesToReturn = changes;

        changes = new ActionResultMessages();
        return changesToReturn;
    }
}
