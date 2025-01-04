public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetNewLocation(LocationContent.Tavern);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.MaxHealth = 10;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.FocusEnergy = gameRules.StartingFocusEnergy;
        playerInfo.MaxFocusEnergy = 10;

        playerInfo.SocialEnergy = gameRules.StartingSocialEnergy;
        playerInfo.MaxSocialEnergy = 10;

        Dictionary<SkillTypes, int> skills = new Dictionary<SkillTypes, int>();
        skills[SkillTypes.Strength] = 1;
        skills[SkillTypes.Observance] = 1;
        skills[SkillTypes.Charisma] = 1;

        playerInfo.Skills = skills;
        playerInfo.Inventory.AddItems(ResourceTypes.Food, 1);

        gameState.Player = playerInfo;


        return gameState;
    }
}
