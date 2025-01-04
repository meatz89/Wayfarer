public class QuestSystem
{
    private readonly GameState gameState;
    private readonly List<Quest> allQuests = new();
    private readonly ContextEngine contextEngine;
    private readonly ActionValidator actionValidator;
    private readonly List<Quest> activeQuests = new();
    private readonly int maxActiveQuests = 3;  // Fixed limit
    private int activeQuestCount;

    public QuestSystem(
        GameState gameState, 
        GameContentProvider contentProvider, 
        ContextEngine contextEngine,
        ActionValidator actionValidator)
    {
        this.gameState = gameState;
        this.contextEngine = contextEngine;
        this.actionValidator = actionValidator;
        this.allQuests = contentProvider.GetQuests();

        foreach (Quest quest in allQuests)
        {
            activeQuests.Add(quest);
        }
    }

    public void ProcessAction(BasicAction action)
    {
        // Check each active quest
        for (int i = 0; i < activeQuestCount; i++)
        {
            Quest quest = activeQuests[i];
            // If action matches quest step and requirements are met
            QuestStep questStep = quest.GetCurrentStep();

            BasicAction questAction = questStep.QuestAction;
            if (questAction.ActionType == action.ActionType &&
                actionValidator.CanExecuteAction(questStep.QuestAction))
            {
                quest.AdvanceQuest();
            }
        }
    }

    public List<Quest> GetAvailableQuests()
    {
        return activeQuests;
    }

    public GameRules GetModifiedRules(GameRules currentRules)
    {
        return currentRules;
    }

    public List<IGameStateModifier> GetActiveModifiers()
    {
        return new List<IGameStateModifier>();
    }

    public List<QuestCondidtion> GetActiveConditions()
    {
        return new List<QuestCondidtion>();
    }
}