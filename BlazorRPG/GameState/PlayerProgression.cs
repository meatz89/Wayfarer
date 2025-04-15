public class PlayerProgression
{
    private readonly PlayerState playerState;
    private readonly GameState gameState;
    private readonly MessageSystem messageSystem;

    public PlayerProgression(GameState gameState, MessageSystem messageSystem)
    {
        this.gameState = gameState;
        this.messageSystem = messageSystem;
        this.playerState = gameState.PlayerState;
    }

    // Add experience points and check for level up
    public void AddExperience(int xpAmount)
    {
        playerState.CurrentXP += xpAmount;

        // Check for level up
        int xpRequiredForNextLevel = GetXpRequiredForNextLevel();
        if (playerState.CurrentXP >= xpRequiredForNextLevel)
        {
            LevelUp();
        }
    }

    // Handle level up
    private void LevelUp()
    {
        playerState.Level++;

        // Award stat increases
        IncreaseStats();

        // Reset XP for next level
        playerState.CurrentXP -= GetXpRequiredForNextLevel(playerState.Level - 1);

        messageSystem.AddSystemMessage("Level Up", SystemMessageTypes.Success);
    }

    // Calculate XP required for next level
    private int GetXpRequiredForNextLevel(int level = 0)
    {
        if (level == 0) level = playerState.Level;

        // Simple formula: 100 * current level
        return 100 * level;
    }

    // Award stat increases on level up
    private void IncreaseStats()
    {
        // Increase max energy
        playerState.MaxEnergy += 5;

        // Increase max health/concentration/confidence
        playerState.MaxHealth += 2;
        playerState.MaxConcentration += 2;
        playerState.MaxConfidence += 2;

        // Heal on level up
        playerState.Health = playerState.MaxHealth;
        playerState.Concentration = playerState.MaxConcentration;
        playerState.Confidence = playerState.MaxConfidence;
        playerState.Energy = playerState.MaxEnergy;
    }
}
