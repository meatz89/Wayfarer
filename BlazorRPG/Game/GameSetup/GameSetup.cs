public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetNewLocation(LocationContent.LionsHeadTavern);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.MaxHealth = 20;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 20;

        playerInfo.FocusEnergy = gameRules.StartingFocusEnergy;
        playerInfo.MaxFocusEnergy = 20;

        playerInfo.SocialEnergy = gameRules.StartingSocialEnergy;
        playerInfo.MaxSocialEnergy = 20;

        Dictionary<SkillTypes, int> skills = new Dictionary<SkillTypes, int>();
        skills[SkillTypes.Strength] = 20;
        skills[SkillTypes.Perception] = 20;
        skills[SkillTypes.Charisma] = 20;

        playerInfo.Skills = skills;
        playerInfo.Inventory.AddResources(ResourceTypes.Food, 10);

        gameState.Player = playerInfo;


        return gameState;
    }
}
