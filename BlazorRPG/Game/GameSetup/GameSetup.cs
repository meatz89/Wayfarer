public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.SetNewLocation(LocationNames.HarborStreets);
        gameState.SetNewTime(9);

        Inventory playerInventory = new Inventory();
        playerInventory.Food = 1;

        Player playerInfo = new Player();
        playerInfo.Coins = GameRules.StartingCoins;

        playerInfo.Health = GameRules.StartingHealth;
        playerInfo.MinHealth = GameRules.MinimumHealth;
        playerInfo.MaxHealth = 10;

        playerInfo.PhysicalEnergy = GameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.FocusEnergy = GameRules.StartingFocusEnergy;
        playerInfo.MaxFocusEnergy = 10;

        playerInfo.SocialEnergy = GameRules.StartingSocialEnergy;
        playerInfo.MaxSocialEnergy = 10;

        Dictionary<SkillTypes, int> skills = new Dictionary<SkillTypes, int>();
        skills[SkillTypes.Strength] = 1;
        skills[SkillTypes.Observance] = 1;
        skills[SkillTypes.Charisma] = 1;

        playerInfo.Skills = skills;
        playerInfo.Inventory = playerInventory;

        gameState.Player = playerInfo;


        return gameState;
    }
}
