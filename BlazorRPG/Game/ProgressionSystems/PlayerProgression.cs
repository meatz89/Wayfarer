
public class PlayerProgression
{
    private readonly PlayerState playerState;
    private readonly MessageSystem messageSystem;

    public PlayerProgression(GameState gameState, MessageSystem messageSystem)
    {
        this.messageSystem = messageSystem;
        this.playerState = gameState.PlayerState;
    }

    public void AddPlayerExp(int xpAmount)
    {
        playerState.CurrentXP += xpAmount;

        // Check for level up
        int xpRequiredForNextLevel = GetXpRequiredForNextLevel();
        playerState.XPToNextLevel = xpRequiredForNextLevel;

        if (playerState.CurrentXP >= xpRequiredForNextLevel)
        {
            LevelUp();
            playerState.XPToNextLevel = GetXpRequiredForNextLevel();
        }
    }

    public void AddSkillExp(SkillTypes skill, int xp)
    {
        SkillProgress prog = playerState.Skills.Skills[skill];
        prog.XP += xp;
        while (prog.XP >= prog.XPToNextLevel)
        {
            prog.XP -= prog.XPToNextLevel;
            prog.Level++;
            playerState.SetCharacterStats();
        }
    }

    private void LevelUp()
    {
        playerState.Level++;
        playerState.SetCharacterStats();

        // Heal on level up
        playerState.HealFully();

        // Reset XP for next level
        playerState.CurrentXP -= GetXpRequiredForNextLevel(playerState.Level - 1);

        messageSystem.AddSystemMessage("Level Up", SystemMessageTypes.Success);
    }

    public int GetXpRequiredForNextLevel(int level = 0)
    {
        if (level == 0) level = playerState.Level;

        // Simple formula: 100 * current level
        return 100 * level;
    }
}
