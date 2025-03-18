
public interface IQuestCondition
{
}
public class QuestCondidtion : IQuestCondition
{
    public string GetStatusMessage()
    {
        return "+1 Food per Day";
    }
}