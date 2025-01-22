
public class PlayerStatusSystem
{
    private List<PlayerStatusTypes> activeStatusList = new() { PlayerStatusTypes.Wet, PlayerStatusTypes.Cold };

    public void ApplyStatus(PlayerStatusTypes status)
    {

    }

    public void RemoveStatus(PlayerStatusTypes status)
    {

    }

    public bool HasStatus(PlayerStatusTypes status)
    {
        return true;
    }

    public List<PlayerStatusTypes> GetActiveStatusList()
    {
        return activeStatusList;
    }
}
