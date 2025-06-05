public class PlayerProgression
{
    private Player player;
    private MessageSystem messageSystem;

    public PlayerProgression(GameWorld gameWorld, MessageSystem messageSystem)
    {
        this.messageSystem = messageSystem;
        this.player = gameWorld.GetPlayer();
    }

    public void AddPlayerExp(int xpAmount)
    {
        player.CurrentXP += xpAmount;

        // Check for level up
        int xpRequiredForNextLevel = GetXpRequiredForNextLevel();
        player.XPToNextLevel = xpRequiredForNextLevel;

        if (player.CurrentXP >= xpRequiredForNextLevel)
        {
            LevelUp();
            player.XPToNextLevel = GetXpRequiredForNextLevel();
        }
    }

    public void AddSkillExp(SkillTypes skill, int xp)
    {
        SkillProgress prog = player.Skills.Skills[skill];
        prog.XP += xp;
        while (prog.XP >= prog.XPToNextLevel)
        {
            prog.XP -= prog.XPToNextLevel;
            prog.Level++;
        }
    }

    private void LevelUp()
    {
        player.Level++;

        // Heal on level up
        player.HealFully();

        // Reset XP for next level
        player.CurrentXP -= GetXpRequiredForNextLevel(player.Level - 1);

        messageSystem.AddSystemMessage("Level Up", SystemMessageTypes.Success);
    }

    public int GetXpRequiredForNextLevel(int level = 0)
    {
        if (level == 0) level = player.Level;

        // Simple formula: 100 * current level
        return 100 * level;
    }
}
