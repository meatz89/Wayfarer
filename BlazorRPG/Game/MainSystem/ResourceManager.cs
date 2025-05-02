public class ResourceManager
{
    public void ApplyResourceChanges(PlayerState playerState, int healthChange, int focusChange, int spiritChange)
    {
        if (healthChange != 0)
            playerState.ModifyHealth(healthChange);

        if (focusChange != 0)
            playerState.ModifyConcentration(focusChange);

        if (spiritChange != 0)
            playerState.ModifyConfidence(spiritChange);
    }

}