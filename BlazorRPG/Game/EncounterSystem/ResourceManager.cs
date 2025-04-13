public class ResourceManager
{
    public void ApplyResourceChanges(PlayerState playerState, int healthChange, int focusChange, int confidenceChange)
    {
        if (healthChange != 0)
            playerState.ModifyHealth(healthChange);

        if (focusChange != 0)
            playerState.ModifyConcentration(focusChange);

        if (confidenceChange != 0)
            playerState.ModifyConfidence(confidenceChange);
    }

}