
public class PlayerStatusSystem
{
    private List<PlayerStatus> activeStatusList = new() { 
        PlayerStatus.COLD,
        PlayerStatus.TRUSTED,
        PlayerStatus.EXHAUSTED
    };

    public void ApplyStatus(PlayerStatus status)
    {

    }

    public void RemoveStatus(PlayerStatus status)
    {

    }

    public bool HasStatus(PlayerStatus status)
    {
        return true;
    }

    public List<PlayerStatus> GetActiveStatusList()
    {
        return activeStatusList;
    }
}
