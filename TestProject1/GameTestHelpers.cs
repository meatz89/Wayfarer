
public class GameTestHelpers
{
    public static void ExecuteActionSequence(ActionManager manager, params BasicActionTypes[] sequence)
    {
        foreach (BasicActionTypes actionType in sequence)
        {
            BasicAction action = manager.GameState.ValidLocationActions
                .First(a => a.BasicAction.Id == actionType).BasicAction;

            manager.ExecuteBasicAction(action);
        }
    }

    public static void AdvanceToTimeWindow(ActionManager manager, TimeWindows target)
    {
        while (manager.GameState.CurrentTimeSlot == target)
        {
            manager.AdvanceTime();
        }

        while (manager.GameState.CurrentTimeSlot != target)
        {
            manager.AdvanceTime();
        }
    }
}