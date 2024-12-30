
public class GameTestHelpers
{
    public static void ExecuteActionSequence(ActionManager manager, params BasicActionTypes[] sequence)
    {
        foreach (var actionType in sequence)
        {
            var action = manager.GameState.ValidUserActions
                .First(a => a.BasicAction.ActionType == actionType).BasicAction;
            manager.ExecuteBasicAction(action);
        }
    }

    public static void AdvanceToTimeWindow(ActionManager manager, TimeWindows target)
    {
        while (manager.GameState.CurrentTimeSlot != target)
        {
            manager.AdvanceTime();
        }
    }
}