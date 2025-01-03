public class Quest
{
    public string Title { get; }
    public string Description { get; }
    public QuestStates State { get; private set; }
    public QuestStep[] Steps { get; }
    private int CurrentStep { get; set; }

    public Quest(string title, string description, QuestStep[] steps)
    {
        Title = title;
        Description = description;
        Steps = steps;
        State = QuestStates.NotStarted;
        CurrentStep = 0;
    }

    public void AdvanceQuest()
    {
        if (CurrentStep < Steps.Length - 1)
        {
            CurrentStep++;
            State = QuestStates.InProgress;
        }
        else
        {
            State = QuestStates.Completed;
        }
    }

    public QuestStep GetCurrentStep()
    {
        QuestStep questStep = Steps[CurrentStep];

        if(questStep == null) return null;
        return questStep;
    }
}
